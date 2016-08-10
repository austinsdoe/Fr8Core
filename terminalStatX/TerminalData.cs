﻿using Newtonsoft.Json;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalStatX
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "StatX",
            IconPath = "/Content/icons/web_services/statx-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalStatX.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalStatX",
            Label = "StatX",
            Version = "1",
            AuthenticationType = AuthenticationType.PhoneNumberWithCode,
            AuthenticationAdditionalInfo = JsonConvert.SerializeObject(
                new PhoneNumberAuthenticationAdditionalInfoDTO()
                {
                    Title = "Enter the verification code from your StatX App",
                    Note = "Go to Settings, and then Additional Authorizations, and then tap \"Get code\""
                }
            )
        };
    }
}