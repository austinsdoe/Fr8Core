﻿using System;
using System.Collections.Generic;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;

namespace terminalTwilio.Tests.Fixtures
{
    public class FixtureData
    {
        public static Guid TestGuid_Id_57()
        {
            return new Guid("A1C11E86-9B54-42D4-AA91-605BF46E68E9");
        }

        public static ActionDO ConfigureTwilioAction()
        {
            var actionTemplate = TwilioActionTemplateDTO();

            var actionDO = new ActionDO
            {
                Name = "testaction",
                Id = TestGuid_Id_57(),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = ""
            };

            return actionDO;
        }

        public static ActivityTemplateDO TwilioActionTemplateDTO()
        {
            return new ActivityTemplateDO
            {
                Id = 1,
                Name = "Send_Via_Twilio",
                Version = "1"
            };
        }

        public static Crate CrateDTOForTwilioConfiguration()
        {
            var confControls =
                JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(
                    "{\"Controls\": [{\"initialLabel\": \"For the SMS Number Use:\",\"upstreamSourceLabel\": null,\"valueSource\": \"specific\",\"listItems\": [],\"name\": \"Recipient\",\"required\": false,\"value\": \"+15005550006\",\"label\": null,\"type\": \"TextSource\",\"selected\": false,\"events\": null,\"source\": {\"manifestType\": \"Standard Design-Time Fields\",\"label\": \"Upstream Terminal-Provided Fields\"}},{\"name\": \"SMS_Body\",\"required\": true,\"value\": \"DO-1437 test\",\"label\": \"SMS Body\",\"type\": \"TextBox\",\"selected\": false,\"events\": null,\"source\": null}]}",
                    new ControlDefinitionDTOConverter());

            return Crate.FromContent("Configuration_Controls", confControls);
        }

        public static AuthorizationTokenDO AuthTokenDOTest1()
        {
            return new AuthorizationTokenDO
            {
                Token =
                    @"{""Email"":""docusign_developer@dockyard.company"",""ApiPassword"":""VIXdYMrnnyfmtMaktD+qnD4sBdU=""}",
                ExternalAccountId = "docusign_developer@dockyard.company",
                UserID = "0addea2e-9f27-4902-a308-b9f57d811c0a"

            };
        }

        public static StandardDesignTimeFieldsCM TestFields()
        {
            return new StandardDesignTimeFieldsCM
            {
                Fields = new List<FieldDTO>
                {
                    new FieldDTO("key", "value")
                }
            };
        }
    }
}