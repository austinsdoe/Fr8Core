﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8Data.DataTransferObjects;
using StructureMap;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Services;

namespace terminalQuickBooks.Controllers
{    
    [RoutePrefix("activities")]
    public class ActivityController : BaseTerminalController
    {
        private const string curTerminal = "terminalQuickBooks";
        private readonly ActivityExecutor _activityExecutor;

        public ActivityController()
        {
            _activityExecutor = ObjectFactory.GetInstance<ActivityExecutor>();
        }

        [HttpPost]
        [fr8TerminalHMACAuthenticate(curTerminal)]
        [Authorize]
        public Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {
            return _activityExecutor.HandleFr8Request(curTerminal, actionType, curDataDTO);
        }
    }
}