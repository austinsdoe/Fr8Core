﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using TerminalBase.BaseClasses;
using terminalSalesforce.Infrastructure;
using Utilities.Logging;

namespace terminalSalesforce.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curTerminal = "terminalSalesforce";
        
        private Authentication _authentication = new Authentication();


        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            return _authentication.GetExternalAuthUrl();
        }

        [HttpPost]
        [Route("token")]
        public Task<AuthorizationTokenDTO> GenerateOAuthToken(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                return Task.FromResult(_authentication.Authenticate(externalAuthDTO));
            }
            catch (Exception ex)
            {
                //The event reporting mechanism does not give the actual error message and it has been commented out in the BaseTerminal#ReportTerminalError
                //Logging explicitly to log4net to see the logs in the App Insights.
                //Logger.GetLogger().Error("Terminal SalesForce Authentication error happened. The error message is " + ex.Message);
                Logger.LogError($"Terminal SalesForce Authentication error happened. Fr8UserId = {externalAuthDTO.Fr8UserId} The error message is {ex.Message} ");

                //Report the terminal error in the standard Fr8 Event Reporting mechanism
                ReportTerminalError(curTerminal, ex,externalAuthDTO.Fr8UserId);

                return Task.FromResult(
                    new AuthorizationTokenDTO()
                    {
                        Error = string.Format("An error occured ({0}) while trying to authenticate, please try again later.", ex.Message)
                    }
                );
            }
        }
    }
}