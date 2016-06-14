﻿using System.Web.Http;
using System.Web.Http.Description;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Services;

namespace Fr8.TerminalBase.BaseClasses
{
    public abstract class DefaultTerminalController : ApiController
    {
        private readonly IActivityStore _activityStore;

        protected DefaultTerminalController(IActivityStore activityStore)
        {
            _activityStore = activityStore;
        }

        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult Get()
        {
            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM
            {
                Definition = _activityStore.Terminal,
                Activities = _activityStore.GetAllTemplates()
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}