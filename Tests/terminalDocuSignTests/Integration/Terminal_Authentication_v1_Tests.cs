﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using HealthMonitor.Utility;
using NUnit.Framework;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class Terminal_Authentication_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        /// <summary>
        /// Make sure http call fails with invalid authentication
        /// </summary>
        [Test, Category("Integration.Authentication.terminalDocuSign")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"Authorization has been denied for this request.",
            MatchType = MessageMatch.Contains
        )]
        public async Task Should_Fail_WithAuthorizationError()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Receive_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            var uri = new Uri(configureUrl);
            var hmacHeader = new Dictionary<string, string>()
            {
                { System.Net.HttpRequestHeader.Authorization.ToString(), "hmac test:2:3:4" }
            };
            //lets modify hmacHeader
            
            await RestfulServiceClient.PostAsync<Fr8DataDTO, ActivityDTO>(uri, dataDTO, null, hmacHeader);
        }
    }
}
