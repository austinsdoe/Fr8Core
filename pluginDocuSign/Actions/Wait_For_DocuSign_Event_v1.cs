﻿using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using Core.Interfaces;
using StructureMap;
using Newtonsoft.Json;
using Data.Wrappers;
using Data.Interfaces;

namespace pluginDocuSign.Actions
{
    public class Wait_For_DocuSign_Event_v1 : BasePluginAction
    {
        IAction _action = ObjectFactory.GetInstance<IAction>();
        ICrate _crate = ObjectFactory.GetInstance<ICrate>();
        IDocuSignTemplate _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
        IEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IEnvelope>();


        public object Configure(ActionDataPackageDTO curDataPackageDTO, bool forceFollowupConfiguration = false)
        {
            //TODO: The coniguration feature for Docu Sign is not yet defined. The configuration evaluation needs to be implemented.
            return ProcessConfigurationRequest(curDataPackageDTO,
                actionDo => (forceFollowupConfiguration) ?
                    ConfigurationRequestType.Followup :
                    ConfigurationRequestType.Initial); // will be changed to complete the config feature for docu sign
        }

        public object Activate(ActionDataPackageDTO curDataPackage)
        {
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        public object Execute(ActionDataPackageDTO curDataPackage)
        {
            string envelopeId = "11f41f43-57bd-4568-86f5-9ceabdaafc43"; //TODO: how to extract envelope it?

            //Create a field
            var fields = new List<FieldDTO>()
            {
                new FieldDTO()
                {
                    Key = "EnvelopeId",
                    Value = envelopeId
                }
            };

            var cratePayload = _crate.Create("DocuSign Envelope Payload Data", JsonConvert.SerializeObject(fields), STANDARD_PAYLOAD_MANIFEST_NAME, STANDARD_PAYLOAD_MANIFEST_ID);
            curDataPackage.ActionDTO.CrateStorage.CratesDTO.Add(cratePayload);

            return null;
        }

        protected override CrateStorageDTO InitialConfigurationResponse(ActionDataPackageDTO curDataPackage)
        {
            var fieldSelectDocusignTemplate = new FieldDefinitionDTO()
            {
                FieldLabel = "Select DocuSign Template",
                Type = "dropdownlistField",
                Name = "Selected_DocuSign_Template",
                Required = true,
                Events = new List<FieldEvent>() {
                     new FieldEvent("onSelect", "requestConfiguration")
                }
            };

            var fieldEnvelopeSent = new FieldDefinitionDTO()
            {
                FieldLabel = "Envelope Sent",
                Type = "checkboxField",
                Name = "Event_Envelope_Sent"
            };

            var fieldEnvelopeReceived = new FieldDefinitionDTO()
            {
                FieldLabel = "Envelope Received",
                Type = "checkboxField",
                Name = "Event_Envelope_Received"
            };

            var fieldRecipientSigned = new FieldDefinitionDTO()
            {
                FieldLabel = "Recipient Signed",
                Type = "checkboxField",
                Name = "Event_Recipient_Signed"
            };

            var fieldEventRecipientSent = new FieldDefinitionDTO()
            {
                FieldLabel = "Recipient Sent",
                Type = "checkboxField",
                Name = "Event_Recipient_Sent"
            };

            var fields = new List<FieldDefinitionDTO>()
            {
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent
            };

            var crateControls = _crate.Create("Configuration_Controls", JsonConvert.SerializeObject(fields));

            curDataPackage.ActionDTO.CrateStorage.CratesDTO.Add(crateControls);

            return curDataPackage.ActionDTO.CrateStorage;
        }

        protected override CrateStorageDTO FollowupConfigurationResponse(ActionDataPackageDTO curDataPackage)
        {
            var curCrates = curDataPackage.ActionDTO.CrateStorage.CratesDTO;

            if (curCrates == null || curCrates.Count == 0)
            {
                return curDataPackage.ActionDTO.CrateStorage;
            }

            // Extract DocuSign Template Id
            var configurationFieldsCrate = curCrates.SingleOrDefault(c => c.Label == "Configuration_Controls");

            if (configurationFieldsCrate == null || String.IsNullOrEmpty(configurationFieldsCrate.Contents))
            {
                return curDataPackage.ActionDTO.CrateStorage;
            }

            var configurationFields = JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(configurationFieldsCrate.Contents);

            if (configurationFields == null || !configurationFields.Any(c => c.Name == "Selected_DocuSign_Template"))
            {
                return curDataPackage.ActionDTO.CrateStorage;
            }

            var docusignTemplateId = configurationFields.SingleOrDefault(c => c.Name == "Selected_DocuSign_Template").Value;
            var userDefinedFields = _docusignEnvelope.GetEnvelopeDataByTemplate(docusignTemplateId);
            var crateConfiguration = new List<CrateDTO>();
            var fieldCollection = userDefinedFields.Select(f => new FieldDefinitionDTO()
            {
                FieldLabel = f.Name,
                Type = f.Type,
                Name = f.Name,
                Value = f.Value
            });

            crateConfiguration.Add(_crate.Create(
                "DocuSignTemplateUserDefinedFields",
                JsonConvert.SerializeObject(fieldCollection),
                "DocuSignTemplateUserDefinedFields"));

            //crateConfiguration.Add(_crate.Create(
            //    "DocuSignEnvelopeStandardFields", 
            //    JsonConvert.SerializeObject(fieldCollection), 
            //    "DocuSignEnvelopeStandardFields"));

            curDataPackage.ActionDTO.CrateStorage.CratesDTO.AddRange(crateConfiguration);
            return curDataPackage.ActionDTO.CrateStorage;
        }
    }
}