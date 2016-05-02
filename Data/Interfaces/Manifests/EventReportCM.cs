﻿using System;
using System.Collections.Generic;
using Data.Constants;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.Manifests
{
    [CrateManifestSerializer(typeof(EventReportSerializer))]
    public class EventReportCM : Manifest
    {
        public string EventNames { get; set; }
        public string ContainerDoId { get; set; }
        public string ExternalAccountId { get; set; }
        public string ExternalDomainId { get; set; }
        public ICrateStorage EventPayload { get; set; }
        public string Manufacturer { get; set; }
        public string Source { get; set; }

        public List<Guid> PlansAffected { get; set; }

        public EventReportCM()
            : base(MT.StandardEventReport)
         {
            EventPayload = new CrateStorage();
            //EventPayload = new List<CrateDTO>();
        }
    }


    public class EventReportSerializer : IManifestSerializer
    {
        public class EventReportCMSerializationProxy
        {
            public string EventNames { get; set; }
            public string ContainerDoId { get; set; }
            public string ExternalAccountId { get; set; }
            public string ExternalDomainId { get; set; }
            public CrateStorageDTO EventPayload { get; set; }
            public string Manufacturer { get; set; }

            public List<Guid> PlansAffected { get; set; }
        }

        private ICrateStorageSerializer _storageSerizlier;

        public void Initialize(ICrateStorageSerializer storageSerializer)
        {
            _storageSerizlier = storageSerializer;
        }

        public object Deserialize(JToken crateContent)
        {
            var proxy = crateContent.ToObject<EventReportCMSerializationProxy>();
            var storage = _storageSerizlier.ConvertFromDto(proxy.EventPayload);

            return new EventReportCM
            {
                EventNames = proxy.EventNames,
                ContainerDoId = proxy.ContainerDoId,
                ExternalAccountId = proxy.ExternalAccountId,
                ExternalDomainId = proxy.ExternalDomainId,
                EventPayload = storage,
                Manufacturer = proxy.Manufacturer,
                PlansAffected = proxy.PlansAffected
            };
        }

        public JToken Serialize(object content)
        {
            var e = (EventReportCM) content;
            
            var proxy = new EventReportCMSerializationProxy
            {
                EventNames = e.EventNames,
                ContainerDoId = e.ContainerDoId,
                ExternalAccountId = e.ExternalAccountId,
                ExternalDomainId = e.ExternalDomainId,
                Manufacturer = e.Manufacturer,
                EventPayload = _storageSerizlier.ConvertToDto(e.EventPayload),
                PlansAffected = e.PlansAffected
            };

            return JToken.FromObject(proxy);
        }
    }
}
