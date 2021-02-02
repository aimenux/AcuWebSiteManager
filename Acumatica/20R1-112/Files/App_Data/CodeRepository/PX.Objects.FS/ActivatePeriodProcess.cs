using PX.Data;

namespace PX.Objects.FS
{
    public class ActivateContractPeriodProcess : PXGraph<ActivateContractPeriodProcess>
    {
        public ServiceContractEntry graphServiceContractEntry;
        public RouteServiceContractEntry graphRouteServiceContractEntry;

        public ActivateContractPeriodProcess()
        {
            ActivateContractPeriodProcess graphActivateContractPeriodProcess = null;

            ServiceContracts.SetProcessDelegate(
                delegate(FSServiceContract fsServiceContractRow)
                {
                    if (graphActivateContractPeriodProcess == null)
                    {
                        graphActivateContractPeriodProcess = PXGraph.CreateInstance<ActivateContractPeriodProcess>();
                        graphActivateContractPeriodProcess.graphServiceContractEntry = PXGraph.CreateInstance<ServiceContractEntry>();
                        graphActivateContractPeriodProcess.graphServiceContractEntry.skipStatusSmartPanels = true;
                        graphActivateContractPeriodProcess.graphRouteServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();
                        graphActivateContractPeriodProcess.graphRouteServiceContractEntry.skipStatusSmartPanels = true;
                    }

                    graphActivateContractPeriodProcess.ProcessContractPeriod(graphActivateContractPeriodProcess,fsServiceContractRow);
                });
        }

        public virtual void ProcessContractPeriod(ActivateContractPeriodProcess graphActivateContractPeriodProcess, FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
            {
                graphActivateContractPeriodProcess.graphServiceContractEntry.ServiceContractRecords.Current =
                    graphActivateContractPeriodProcess.graphServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

                graphActivateContractPeriodProcess.graphServiceContractEntry.ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.actions>(graphActivateContractPeriodProcess.graphServiceContractEntry.ContractPeriodFilter.Current, ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD);
                graphActivateContractPeriodProcess.graphServiceContractEntry.ActivatePeriod();
            }
            else if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
            {
                graphActivateContractPeriodProcess.graphRouteServiceContractEntry.ServiceContractRecords.Current =
                    graphActivateContractPeriodProcess.graphRouteServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

                graphActivateContractPeriodProcess.graphRouteServiceContractEntry.ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.actions>(graphActivateContractPeriodProcess.graphServiceContractEntry.ContractPeriodFilter.Current, ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD);
                graphActivateContractPeriodProcess.graphRouteServiceContractEntry.ActivatePeriod();
            }
        }

        #region Select
        public PXFilter<ServiceContractFilter> Filter;
        public PXCancel<ServiceContractFilter> Cancel;

        [PXFilterable]
        public
            PXFilteredProcessingJoin<FSServiceContract, ServiceContractFilter,
            LeftJoin<FSContractPeriod,
            On<
                FSContractPeriod.serviceContractID, Equal<FSServiceContract.serviceContractID>,
                And<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>,
            Where2<
                Where<CurrentValue<ServiceContractFilter.customerID>, IsNull,
                Or<FSServiceContract.customerID, Equal<CurrentValue<ServiceContractFilter.customerID>>>>,
            And2<
                Where<CurrentValue<ServiceContractFilter.refNbr>, IsNull,
                Or<FSServiceContract.refNbr, Equal<CurrentValue<ServiceContractFilter.refNbr>>>>,
            And2<
                Where<CurrentValue<ServiceContractFilter.customerLocationID>, IsNull,
                Or<FSServiceContract.customerLocationID, Equal<CurrentValue<ServiceContractFilter.customerLocationID>>>>,
            And2<
                Where<CurrentValue<ServiceContractFilter.branchID>, IsNull,
                Or<FSServiceContract.branchID, Equal<CurrentValue<ServiceContractFilter.branchID>>>>,
            And2<
                Where<CurrentValue<ServiceContractFilter.branchLocationID>, IsNull,
                Or<FSServiceContract.branchLocationID, Equal<CurrentValue<ServiceContractFilter.branchLocationID>>>>,
            And<
                FSContractPeriod.contractPeriodID, IsNull,
            And<
                FSServiceContract.billingType, Equal<FSServiceContract.billingType.StandardizedBillings>,
            And<
                FSServiceContract.status, Equal<FSServiceContract.status.Active>>>>>>>>>,
            OrderBy<
                Asc<FSServiceContract.customerID,
                Asc<FSServiceContract.refNbr>>>> ServiceContracts;
        #endregion
    }
}
