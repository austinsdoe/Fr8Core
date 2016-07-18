﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Unit;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalAsana.Asana;
using terminalAsana.Interfaces;
using terminalAsana.Asana.Services;
using terminalAsanaTests.Fixtures;

namespace terminalAsanaTests.Unit
{
    class AsanaOAuthServiceTests
    {
        private IHubCommunicator _hubCommunicator;
        private IRestfulServiceClient _restClient;
        private IAsanaParameters _parameters;
        private Mock<IRestfulServiceClient> _restClientMock;
        private Mock<IHubCommunicator> _hubCommunicatorMock;
        private Mock<IAsanaParameters> _parametersMock;

        [SetUp]
        public void Startup()
        {
            _restClientMock = new Mock<IRestfulServiceClient>();
            _restClient = _restClientMock.Object;

            _hubCommunicatorMock = new Mock<IHubCommunicator>();
            _hubCommunicator = _hubCommunicatorMock.Object;

            _parametersMock = new Mock<IAsanaParameters>();
            _parameters = _parametersMock.Object;
        }

        [Test]
        public async Task Should_Initialize_Itself()
        {
            var tokenData = FixtureData.SampleAuthorizationToken();

            var asanaOAuth = await new AsanaOAuthService(_restClient, _parameters).InitializeAsync(new OAuthToken());

            Assert.IsTrue(asanaOAuth.OAuthToken.ExpirationDate > DateTime.UtcNow && asanaOAuth.OAuthToken.ExpirationDate < DateTime.UtcNow.AddSeconds(3601));
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.AccessToken);
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.RefreshToken);
        }

        [Test]
        public async Task Should_Refresh_Token_If_It_Expired()
        {
            _restClientMock.Setup(
                x => x.PostAsync<JObject>(  It.IsAny<Uri>(), 
                                            It.IsAny<HttpContent>(),
                                            It.IsAny<string>(),
                                            It.IsAny<Dictionary<string, string>>()))

                       .ReturnsAsync(JObject.Parse(FixtureData.SampleAsanaOauthTokenResponse()));

            var tokenData = FixtureData.SampleAuthorizationToken();
            tokenData.AdditionalAttributes = DateTime.Parse(tokenData.AdditionalAttributes).AddHours(-12).ToString("O");

            var asanaOAuth = await new AsanaOAuthService(_restClient, _parameters).InitializeAsync(new OAuthToken());

            _restClientMock.Verify(x => x.PostAsync(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()));

            Assert.IsTrue(asanaOAuth.OAuthToken.ExpirationDate > DateTime.UtcNow && asanaOAuth.OAuthToken.ExpirationDate < DateTime.UtcNow.AddSeconds(3601));
            Assert.IsNotEmpty(asanaOAuth.OAuthToken.AccessToken);
        }
    }
}