﻿using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDropbox.Services;

namespace terminalDropbox.Actions
{
    public class Get_File_List_v1 : BaseTerminalAction
    {
        private readonly DropboxService _dropboxService;
        protected ICrateManager _crateManager;

        public Get_File_List_v1()
        {
            _dropboxService = ObjectFactory.GetInstance<DropboxService>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            base.CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            base.CheckAuthentication(authTokenDO);

            var processPayload = await GetProcessPayload(curActionDO, containerId);

            var fileNames = await _dropboxService.GetFileList(authTokenDO);

            using (var updater = _crateManager.UpdateStorage(processPayload))
            {
                updater.CrateStorage.Add(PackCrate_DropboxFileList(fileNames));
            }

            return processPayload;
        }

        private Crate PackCrate_DropboxFileList(List<string> fileNames)
        {
            return Data.Crates.Crate.FromJson("Dropbox File List", JsonConvert.SerializeObject(fileNames));
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return ConfigurationRequestType.Initial;
        }
    }
}