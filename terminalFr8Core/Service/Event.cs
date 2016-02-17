﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Utilities.Configuration.Azure;
using System.Reflection;

namespace terminalFr8Core.Services
{
    public class Event : terminalFr8Core.Interfaces.IEvent
    {
        private readonly ICrateManager _crate;

        public Event()
        {
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        public async Task<Crate> Process(string eventPayload)
        {
            var eventLogging = JsonConvert.DeserializeObject<EventLoggingDTO>(eventPayload);
            var systemUser = CloudConfigurationManager.GetSetting("SystemAccount");
            string curAssemblyName = "terminalFr8Core.Managers.EventManager";
            string curMethodPath = eventLogging.EventName;

            try
            {
                Type calledType = Type.GetType(curAssemblyName);
                object curObject = Activator.CreateInstance(calledType);
                MethodInfo curMethodInfo = calledType.GetMethod(curMethodPath, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                StandardLoggingCM loggingManifest =  (StandardLoggingCM)curMethodInfo.Invoke(curObject, new Object[] { eventLogging });

                if (calledType == null)
                    throw new ArgumentException("Event Manager does not exist in terminal");

                await Task.Run(() => (StandardLoggingCM)curMethodInfo.Invoke(curObject, new Object[] { eventLogging }));

                // Create the eventReportContent from the posted JSON and the using the account.
                var eventReportContent = new EventReportCM
                {
                    ContainerDoId = "",
                    EventNames = eventLogging.EventName,
                    ExternalAccountId = systemUser,
                    Manufacturer = "Fr8Core",
                    EventPayload = EventPayload(eventLogging.EventName, loggingManifest),
                };

                var curEventReport = _crate.CreateStandardEventReportCrate("Fr8 Internal Event", eventReportContent);
                return curEventReport;
            }
            catch (Exception)
            {

                throw;
            }
        }

        // Create event payload from the JSON data.
        private ICrateStorage EventPayload(string eventName, StandardLoggingCM loggingManifest)
        {
            var storage = new CrateStorage();
            storage.Add(Data.Crates.Crate.FromContent(eventName, loggingManifest));
            return storage;
        }
    }
}