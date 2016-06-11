﻿using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Infrastructure;
using terminalGoogle.Infrastructure;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("terminals/terminalGoogle")]
    public class EventController : ApiController
    {
        private IEvent _event;
        private BaseTerminalEvent _baseTerminalEvent;

        public EventController(IRestfulServiceClient restfulServiceClient)
        {
            _event = new Event();
            _baseTerminalEvent = new BaseTerminalEvent(restfulServiceClient);
        }

        [HttpPost]
        [Route("events")]
        public async Task ProcessIncomingNotification()
        {
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            await _baseTerminalEvent.Process(eventPayLoadContent, _event.Process);
        }
    }
}