﻿using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;
using JsonSerializer = Utilities.Serializers.Json.JsonSerializer;

namespace Core.Services
{
    public class Crate : ICrate
    {
        public Crate()
        {
        }

        public CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0)
        {
            var crateDTO = new CrateDTO() 
            { 
                Id = Guid.NewGuid().ToString(), 
                Label = label, 
                Contents = contents, 
                ManifestType = manifestType, 
                ManifestId = manifestId 
            };
            return crateDTO;
        }

        public CrateDTO CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields)
        {    
            return Create(label, 
                JsonConvert.SerializeObject(new StandardDesignTimeFieldsMS() {Fields = fields.ToList()}),
                manifestType: CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, 
                manifestId: CrateManifests.DESIGNTIME_FIELDS_MANIFEST_ID);
        }

        public CrateDTO CreateStandardConfigurationControlsCrate(string label, params FieldDefinitionDTO[] controls)
        {
            return Create(label, 
                JsonConvert.SerializeObject(new StandardConfigurationControlsMS() { Controls = controls.ToList() }),
                manifestType: CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                manifestId: CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_ID);
        }

        public CrateDTO CreateStandardEventSubscriptionsCrate(string label, params string[] subscriptions)
        {
            return Create(label,
                JsonConvert.SerializeObject(new EventSubscriptionMS() { Subscriptions = subscriptions.ToList() }),
                manifestType: CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME,
                manifestId: CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_ID);
        }

        public T GetContents<T>(CrateDTO crate)
        {
            return JsonConvert.DeserializeObject<T>(crate.Contents);
        }

        public IEnumerable<JObject> GetElementByKey<TKey>(IEnumerable<CrateDTO> searchCrates, TKey key, string keyFieldName)
        {

            List<JObject> resultsObjects = new List<JObject>();
            foreach (var curCrate in searchCrates.Where(c => !string.IsNullOrEmpty(c.Contents)))
            {
                JContainer curCrateJSON = JsonConvert.DeserializeObject<JContainer>(curCrate.Contents);
                var results = curCrateJSON.Descendants()
                    .OfType<JObject>()
                    .Where(x => x[keyFieldName] != null && x[keyFieldName].Value<TKey>().Equals(key));
                resultsObjects.AddRange(results); ;
            }
            return resultsObjects;
        }

        public void RemoveCrateByManifestId(IList<CrateDTO> crates, int manifestId)
        {
            var curCrates = crates.Where(c => c.ManifestId == manifestId).ToList();
            if (curCrates.Count() > 0)
            {
                foreach (CrateDTO crate in curCrates)
                {
                    crates.Remove(crate);
                }
            }
        }

        public void RemoveCrateByLabel(IList<CrateDTO> crates, string label)
        {
            var curCrates = crates.Where(c => c.Label == label).ToList();
            if (curCrates.Count() > 0)
            {
                foreach (CrateDTO crate in curCrates)
                {
                    crates.Remove(crate);
                }
            }
        }

        public void RemoveCrateByManifestType(IList<CrateDTO> crates, string manifestType)
        {
            var curCrates = crates.Where(c => c.ManifestType == manifestType).ToList();
            if (curCrates.Count() > 0)
            {
                foreach (CrateDTO crate in curCrates)
                {
                    crates.Remove(crate);
                }
            }
        }
    }
}
