﻿using System.Xml.Serialization;

namespace Data.Entities.DocuSignParserModels
{
    [XmlRoot(ElementName = "EnvelopeStatus")]
    public class EnvelopeStatus
    {
        [XmlElement("EnvelopeID")]
        public string EnvelopeId { get; set; }

        [XmlElement("Status")]
        public string Status { get; set; }

        [XmlElement("RecipientStatuses")]
        public RecipientStatuses RecipientStatuses { get; set; }
    }
}