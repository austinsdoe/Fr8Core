﻿using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
	public interface ICriteria
	{
		bool Evaluate(string criteria, int processId, IEnumerable<EnvelopeDataDTO> envelopeData);
		bool Evaluate(List<EnvelopeDataDTO> envelopeData, ProcessNodeDO curProcessNode);
		IQueryable<EnvelopeDataDTO> Filter(string criteria, int processId, IQueryable<EnvelopeDataDTO> envelopeData);
	}
}