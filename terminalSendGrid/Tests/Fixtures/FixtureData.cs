﻿using System;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace terminalSendGrid.Tests.Fixtures
{
    public class FixtureData
    {
        public static ActionDO ConfigureSendEmailViaSendGridAction()
        {
            var actionTemplate = SendEmailViaSendGridActionTemplateDTO();

            var actionDO = new ActionDO()
            {
                Name = "testaction",
                Id = 333,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = ""
            };

            return actionDO;
        }

        public static ActivityTemplateDO SendEmailViaSendGridActionTemplateDTO()
        {
            return new ActivityTemplateDO
            {
                Id = 1,
                Name = "Send Email Via SendGrid",
                Version = "1"
            };
        }

        public static CrateDTO CrateDTOForSendEmailViaSendGridConfiguration()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Configuration_Controls",
                Contents = "test contents",
                CreateTime = DateTime.Now,
                ManifestId = 1,
                ManifestType = "ManifestType",
                Manufacturer = new ManufacturerDTO(),
                ParentCrateId = "ParentCrateId"
            };
        }

        public static PayloadDTO CratePayloadDTOForSendEmailViaSendGridConfiguration
        {
            get
            {
                PayloadDTO payloadDTO = new PayloadDTO(1);
                return payloadDTO;
            }
        }
    }
}
