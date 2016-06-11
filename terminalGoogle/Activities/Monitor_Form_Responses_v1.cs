﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Errors;
using Newtonsoft.Json;
using terminalGoogle.Services;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Actions
{
    public class Monitor_Form_Responses_v1 : BaseGoogleTerminalActivity<Monitor_Form_Responses_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList FormsList { get; set; }

            public ActivityUi()
            {
                FormsList = new DropDownList()
                {
                    Label = "Select Google Form",
                    Name = "Selected_Google_Form",
                    Required = true,
                    Source = null,
                    Events = { ControlEvent.RequestConfig }
                };
                Controls.Add(FormsList);
            }
        }
        private readonly IGoogleDrive _googleDrive;
        private readonly IGoogleAppsScript _googleAppsScript;

        private const string ConfigurationCrateLabel = "Selected_Google_Form";
        private const string RunTimeCrateLabel = "Google Form Payload Data";
        private const string EventSubscriptionsCrateLabel = "Standard Event Subscriptions";
        private FieldDTO SelectedForm
        {
            get
            {
                var storedValues = Storage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationCrateLabel)?.Content;
                return storedValues?.Fields.First();
            }
            set
            {
                if (value == null)
                {
                    Storage.RemoveByLabel(ConfigurationCrateLabel);
                    return;
                }
                value.Availability = AvailabilityType.Configuration;
                var newValues = Crate.FromContent(ConfigurationCrateLabel, new FieldDescriptionsCM(value), AvailabilityType.Configuration);
                Storage.ReplaceByLabel(newValues);
            }
        }

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Monitor_Form_Responses",
            Label = "Monitor Form Responses",
            Version = "1",
            Category = ActivityCategory.Monitors,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            WebService = TerminalData.WebServiceDTO,
            MinPaneWidth = 300
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public Monitor_Form_Responses_v1(ICrateManager crateManager, IGoogleIntegration googleIntegration, IGoogleAppsScript googleAppsScript, IGoogleDrive googleDrive)
            : base(crateManager, googleIntegration)
        {
            _googleDrive = googleDrive;
            _googleAppsScript = googleAppsScript;
        }

        public override async Task Initialize()
        {
            var googleAuth = GetGoogleAuthToken();
            var forms = await _googleDrive.GetGoogleForms(googleAuth);
            ActivityUI.FormsList.ListItems = forms
                .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                .ToList();
            Storage.Add(CreateEventSubscriptionCrate());
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        public override async Task FollowUp()
        {
            var googleAuth = GetGoogleAuthToken();
            var forms = await _googleDrive.GetGoogleForms(googleAuth);
            ActivityUI.FormsList.ListItems = forms
                .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                .ToList();
            var selectedSpreadsheet = ActivityUI.FormsList.selectedKey;
            if (!string.IsNullOrEmpty(selectedSpreadsheet))
            {
                bool any = ActivityUI.FormsList.ListItems.Any(x => x.Key == selectedSpreadsheet);
                if (!any)
                {
                    ActivityUI.FormsList.selectedKey = null;
                    ActivityUI.FormsList.Value = null;
                }
            }
            if (string.IsNullOrEmpty(ActivityUI.FormsList.selectedKey))
                SelectedForm = null;

            //get form id
            var googleFormControl = ActivityUI.FormsList;
            var formId = googleFormControl.Value;
            if (string.IsNullOrEmpty(formId))
                throw new ArgumentNullException("Google Form selected is empty. Please select google form to receive.");

            //need to get all form fields and mark them available for runtime
            var formFields = await _googleAppsScript.GetGoogleFormFields(googleAuth, formId);

            CrateSignaller.ClearAvailableCrates();
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel).AddFields(formFields.Select(x => new FieldDTO() { Key = x.Title, Value = x.Title }).ToList());
        }

        public override async Task Activate()
        {
            var googleAuth = GetGoogleAuthToken();
            //get form id
            var googleFormControl = ActivityUI.FormsList;
            var formId = googleFormControl.Value;
            if (string.IsNullOrEmpty(formId))
                throw new ArgumentNullException("Google Form selected is empty. Please select google form to receive.");

            try
            {
                await _googleAppsScript.CreateFr8TriggerForDocument(googleAuth, formId, AuthorizationToken.ExternalAccountId);
            }
            catch
            {
                throw new ActivityExecutionException($"Failed to activate {ActivityPayload.Name} because of problem with activating trigger on google form.");
            }
        }

        public override Task Run()
        {
            var selectedForm = ActivityUI.FormsList.Value;
            if (string.IsNullOrEmpty(selectedForm))
                RaiseError("Form is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            var payloadFields = ExtractPayloadFields(Payload);
            // once we activate the plan we run it. When we run the plan manualy there is no payload with event data. 
            // Just return Success as a quick fix to avoid "Plan Failed" message.
            if (payloadFields == null)
            {
                RequestHubExecutionTermination();
                return Task.FromResult(0);
            }
            var formResponseFields = CreatePayloadFormResponseFields(payloadFields);

            // once we activate the plan we run it. When we run the plan manualy there is no payload with event data. 
            // Just return Success as a quick fix to avoid "Plan Failed" message.
            if (formResponseFields == null)
            {
                RequestHubExecutionTermination();
                return Task.FromResult(0); ;
            }
            Payload.Add(Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(formResponseFields)));
            return Task.FromResult(0);
        }

        private Crate CreateEventSubscriptionCrate()
        {
            var subscriptions = new string[] {
                "Google Form Response"
            };

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                EventSubscriptionsCrateLabel,
                "Google",
                subscriptions.ToArray()
                );
        }

        private List<FieldDTO> CreatePayloadFormResponseFields(List<FieldDTO> payloadfields)
        {
            var formFieldResponse = new List<FieldDTO>();
            string[] formresponses = payloadfields.FirstOrDefault(w => w.Key == "response").Value.Split(new char[] { '&' });

            if (formresponses.Length > 0)
            {
                formresponses[formresponses.Length - 1] = formresponses[formresponses.Length - 1].TrimEnd(new char[] { '&' });

                formFieldResponse.AddRange(from response in formresponses
                                           select response.Split(new char[] {'='}) into itemResponse
                                           where itemResponse.Length >= 2
                                           select new FieldDTO() {Key = itemResponse[0], Value = itemResponse[1]});
            }
            else
            {
                throw new ArgumentNullException("No payload fields extracted");
            }

            return formFieldResponse;
        }

        private List<FieldDTO> ExtractPayloadFields(ICrateStorage currentPayload)
        {
            var eventReportMS = currentPayload.CrateContentsOfType<EventReportCM>().SingleOrDefault();

            var eventFieldsCrate = eventReportMS?.EventPayload.SingleOrDefault();
            if (eventFieldsCrate == null)
                return null;

            return eventReportMS.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()).ToList();
        }
    }
}