﻿module dockyard.controllers {
    'use strict';

    export interface IReportIncidentListScope extends ng.IScope {
        filter: any;
        query: model.HistoryQueryDTO;
        promise: ng.IPromise<model.HistoryResultDTO<model.IncidentDTO>>;
        result: model.HistoryResultDTO<model.IncidentDTO>;
        getHistory: () => void;
        removeFilter: () => void;
        openModal: (historyItem: model.HistoryItemDTO) => void;
        orderBy: string;
        selected: any;
    }

    class ReportIncidentController {

        public static $inject = [
            '$scope',
            '$modal',
            'ReportService'
        ];

        constructor(private $scope: IReportIncidentListScope, private $modal: any, private ReportService: services.IReportService) {

            $scope.selected = [];

            $scope.query = new model.HistoryQueryDTO();
            $scope.query.itemPerPage = 10;
            $scope.query.page = 1;
            $scope.orderBy = "-createdDate";
            $scope.query.isCurrentUser = true;
            

            $scope.filter = {
                options: {
                    debounce: 500
                }
            };

            $scope.getHistory = <() => void>angular.bind(this, this.getHistory);
            $scope.removeFilter = <() => void>angular.bind(this, this.removeFilter);
            $scope.openModal = <(historyItem: model.HistoryItemDTO) => void>angular.bind(this, this.openModal);

            $scope.$watch('query.filter', (newValue, oldValue) => {
                var bookmark: number = 1;
                if (!oldValue) {
                    bookmark = $scope.query.page;
                }
                if (newValue !== oldValue) {
                    $scope.query.page = 1;
                }
                if (!newValue) {
                    $scope.query.page = bookmark;
                }

                this.getHistory();
            });
        }

        private openModal(historyItem: model.HistoryItemDTO) {
            var modalInstance = this.$modal.open({
                animation: true,
                templateUrl: '/AngularTemplate/ReportIncidentModal',
                controller: 'ReportIncidentModalController',
                size: 'lg',
                resolve: {
                    historyItem: () => historyItem
            }
            });
        }

        private removeFilter() {
            this.$scope.query.filter = null;
            this.$scope.filter.showFilter = false;
            this.getHistory();
        }

        private getHistory() {
            if (this.$scope.orderBy && this.$scope.orderBy.charAt(0) === '-') {
                this.$scope.query.isDescending = true;
            } else {
                this.$scope.query.isDescending = false;
            }
            this.$scope.promise = this.ReportService.getIncidentsByQuery(this.$scope.query).$promise;
            this.$scope.promise.then((data: model.HistoryResultDTO<model.IncidentDTO>) => {
                this.$scope.result = data;
            });
        }     
    }

    app.controller('ReportIncidentController', ReportIncidentController);

    app.controller('ReportIncidentModalController', ['$scope', '$modalInstance', 'historyItem', ($scope: any, $modalInstance: any, historyItem: interfaces.IHistoryItemDTO): void => {

        $scope.historyItem = historyItem;

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };
    }]);
}