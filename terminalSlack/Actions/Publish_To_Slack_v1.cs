﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using Hub.Managers;
using TerminalBase.Infrastructure;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using TerminalBase.BaseClasses;
using Data.Entities;

namespace terminalSlack.Actions
{

    public class Publish_To_Slack_v1 : BaseTerminalAction
    {
        private readonly ISlackIntegration _slackIntegration;


        public Publish_To_Slack_v1()
        {
            _slackIntegration = new SlackIntegration();
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO=null)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var processPayload = await GetProcessPayload(containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var actionChannelId = ExtractControlFieldValue(actionDO, "Selected_Slack_Channel");
            if (string.IsNullOrEmpty(actionChannelId))
            {
                throw new ApplicationException("No selected channelId found in action.");
            }

            var actionFieldName = ExtractControlFieldValue(actionDO, "Select_Message_Field");
            if (string.IsNullOrEmpty(actionFieldName))
            {
                throw new ApplicationException("No selected field found in action.");
            }

            var payloadFields = ExtractPayloadFields(processPayload);

            var payloadMessageField = payloadFields.FirstOrDefault(x => x.Key == actionFieldName);
            if (payloadMessageField == null)
            {
                throw new ApplicationException("No specified field found in action.");
            }

            await _slackIntegration.PostMessageToChat(authTokenDO.Token,
                actionChannelId, payloadMessageField.Value);

            return processPayload;
        }

        private List<FieldDTO> ExtractPayloadFields(PayloadDTO processPayload)
        {
            var payloadDataCrates = Crate.FromDto(processPayload.CrateStorage).CratesOfType<StandardPayloadDataCM>();

            var result = new List<FieldDTO>();
            foreach (var payloadDataCrate in payloadDataCrates)
            {
                result.AddRange(payloadDataCrate.Content.AllValues());
            }

            return result;
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, x => ConfigurationEvaluator(x), authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var oauthToken = authTokenDO.Token;
            var channels = await _slackIntegration.GetChannelList(oauthToken);

            var crateControls = PackCrate_ConfigurationControls();
            var crateAvailableChannels = CreateAvailableChannelsCrate(channels);
            var crateAvailableFields = await CreateAvailableFieldsCrate(curActionDO);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(crateControls);
                updater.CrateStorage.Add(crateAvailableChannels);
                updater.CrateStorage.Add(crateAvailableFields);
            }

            return curActionDO;
        }

        private Crate PackCrate_ConfigurationControls()
        {
            var fieldSelectChannel = new DropDownListControlDefinitionDTO()
            {
                Label = "Select Slack Channel",
                Name = "Selected_Slack_Channel",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Channels",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            var fieldSelectMessageField = new DropDownListControlDefinitionDTO()
            {
                Label = "Select Message Field",
                Name = "Select_Message_Field",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Fields",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            return PackControlsCrate(fieldSelectChannel, fieldSelectMessageField);
        }

        private Crate CreateAvailableChannelsCrate(IEnumerable<FieldDTO> channels)
        {
            var crate =
                Crate.CreateDesignTimeFieldsCrate(
                    "Available Channels",
                    channels.ToArray()
                );

            return crate;
        }

        private async Task<Crate> CreateAvailableFieldsCrate(ActionDO actionDO)
        {
            var curUpstreamFields =
                (await GetCratesByDirection<StandardDesignTimeFieldsCM>(actionDO.Id, GetCrateDirection.Upstream))

                .Where(x => x.Label != "Available Channels")
                .SelectMany(x => x.Content.Fields)
                .ToArray();

            var availableFieldsCrate =
                Crate.CreateDesignTimeFieldsCrate(
                    "Available Fields",
                    curUpstreamFields
                );

            return availableFieldsCrate;
        }

        // TODO: finish that later.
        /*
        public object Execute(SlackPayloadDTO curSlackPayload)
        {
            string responseText = string.Empty;
            Encoding encoding = new UTF8Encoding();

            const string webhookUrl = "WebhookUrl";
            Uri uri = new Uri(ConfigurationManager.AppSettings[webhookUrl]);

            string payloadJson = JsonConvert.SerializeObject(curSlackPayload);

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = payloadJson;

                var response = client.UploadValues(uri, "POST", data);

                responseText = encoding.GetString(response);
            }
            return responseText;
        }
        */
    }
}