﻿using System;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Moq;
using StructureMap;
using TerminalBase.Infrastructure;
using Fr8Data.Managers;

namespace UtilitiesTesting.Fixtures
{
    public static class DependecyConfigurationUtils
    {
        public static IContainer ConfigureHubToReturnEmptyPayload(this IContainer container)
        {
            var payload = new PayloadDTO(Guid.NewGuid());
            using (var storage = container.GetInstance<ICrateManager>().GetUpdatableStorage(payload))
            {
                storage.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
            }
            container.GetInstance<Mock<IHubCommunicator>>().Setup(x => x.GetPayload(It.IsAny<Guid>()))
                     .Returns(Task.FromResult(payload));
            return container;
        }
    }
}