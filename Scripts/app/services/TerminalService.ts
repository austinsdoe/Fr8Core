﻿
module dockyard.services {
    export interface ITerminalService extends ng.resource.IResourceClass<interfaces.ITerminalVM> {
        getAll: () => Array<model.TerminalRegistrationDTO>;
        register: (terminal: model.TerminalRegistrationDTO) => ng.IPromise<any>;
    }

    app.factory("TerminalService", ["$resource", ($resource: ng.resource.IResourceService): ITerminalService =>
        <ITerminalService>$resource("/api/terminals?id=:id", { id: "@id" }, {
            getAll: {
                method: "GET",
                isArray: true,
                url: "/api/terminals/registrations"
            },
            register: {
                method: "POST",
                url: "/api/terminals"
            }
        })
    ]);
}