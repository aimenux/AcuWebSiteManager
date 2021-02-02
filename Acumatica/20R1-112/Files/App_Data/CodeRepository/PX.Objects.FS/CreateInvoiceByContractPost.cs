using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class CreateInvoiceByContractPost : PXGraph<CreateInvoiceByContractPost>
    {
        #region Selects
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;
        public PXFilter<InvoiceContractPeriodFilter> Filter;
        public PXCancel<InvoiceContractPeriodFilter> Cancel;

        [PXFilterable]
        public PXFilteredProcessingJoin<ContractPeriodToPost, InvoiceContractPeriodFilter,
                    InnerJoinSingleTable<Customer,
                      On<Customer.bAccountID, Equal<ContractPeriodToPost.billCustomerID>,
                        And<Match<Customer, Current<AccessInfo.userName>>>>>,
               Where2<
                   Where<
                       CurrentValue<InvoiceContractPeriodFilter.customerID>, IsNull,
                       Or<ContractPeriodToPost.billCustomerID, Equal<CurrentValue<InvoiceContractPeriodFilter.customerID>>>>,
                   And<
                       Where<
                           CurrentValue<InvoiceContractPeriodFilter.serviceContractID>, IsNull,
                           Or<ContractPeriodToPost.serviceContractID, Equal<CurrentValue<InvoiceContractPeriodFilter.serviceContractID>>>>>>> Contracts;
        #endregion

        protected virtual IEnumerable contracts()
        {
            int? currentContractPeriodID = -1;

            var results = PXSelectJoin<ContractPeriodToPost,
                        LeftJoin<FSServiceOrder,
                            On<FSServiceOrder.billServiceContractID, Equal<ContractPeriodToPost.serviceContractID>,
                            And<FSServiceOrder.billContractPeriodID, Equal<ContractPeriodToPost.contractPeriodID>,
                            And<FSServiceOrder.allowInvoice, Equal<False>,
                            And<FSServiceOrder.status, Equal<FSServiceOrder.status.Open>>>>>,
                        LeftJoin<FSAppointment,
                            On<FSAppointment.billServiceContractID, Equal<ContractPeriodToPost.serviceContractID>,
                            And<FSAppointment.billContractPeriodID, Equal<ContractPeriodToPost.contractPeriodID>,
                            And<FSAppointment.status, NotEqual<FSAppointment.status.Closed>,
                        And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>>>>>,
                    InnerJoinSingleTable<Customer,
                        On<Customer.bAccountID, Equal<ContractPeriodToPost.billCustomerID>,
                        And<Match<Customer, Current<AccessInfo.userName>>>>>>>,
                        Where2<
                            Where<CurrentValue<InvoiceContractPeriodFilter.customerID>, IsNull,
                            Or<ContractPeriodToPost.billCustomerID, Equal<CurrentValue<InvoiceContractPeriodFilter.customerID>>>>,
                        And2<
                            Where<CurrentValue<InvoiceContractPeriodFilter.upToDate>, IsNull,
                            Or<ContractPeriodToPost.endPeriodDate, LessEqual<CurrentValue<InvoiceContractPeriodFilter.upToDate>>>>,
                        And<
                            Where<CurrentValue<InvoiceContractPeriodFilter.serviceContractID>, IsNull,
                            Or<ContractPeriodToPost.serviceContractID, Equal<CurrentValue<InvoiceContractPeriodFilter.serviceContractID>>>>>>>,
                        OrderBy<
                            Asc<ContractPeriodToPost.serviceContractID,
                            Asc<ContractPeriodToPost.contractPeriodID,
                            Asc<FSServiceOrder.sOID,
                            Asc<FSAppointment.appointmentID>>>>>>
                        .Select(this);

            foreach (PXResult<ContractPeriodToPost, FSServiceOrder, FSAppointment> it in results)
            {
                ContractPeriodToPost contractPeriodToPostRow = (ContractPeriodToPost)it;
                FSServiceOrder fsServiceOrderRow = (FSServiceOrder)it;
                FSAppointment fsAppointmentRow = (FSAppointment)it;

                if (fsServiceOrderRow?.SOID != null || fsAppointmentRow?.AppointmentID != null)
                {
                    continue;
                }
                else
                {
                    if (currentContractPeriodID != contractPeriodToPostRow.ContractPeriodID)
                    {
                        yield return new PXResult<ContractPeriodToPost>(it);
                    }

                    currentContractPeriodID = contractPeriodToPostRow.ContractPeriodID;
                }
            }

            Contracts.Cache.IsDirty = false;
            Contracts.View.RequestRefresh();
        }

        public CreateInvoiceByContractPost()
        {
            CreateInvoiceByContractPost graphCreateInvoiceByServiceOrderPost = null;

            Contracts.SetProcessDelegate(
                delegate (List<ContractPeriodToPost> contractPeriodToPostRows)
                {
                    graphCreateInvoiceByServiceOrderPost = PXGraph.CreateInstance<CreateInvoiceByContractPost>();

                    int rowIndex = 0;
                    InvoiceContractPeriodFilter filter = Filter.Current;
                    FSSetup fsSetupRow = ServiceManagementSetup.GetServiceManagementSetup(graphCreateInvoiceByServiceOrderPost);

                    ContractPostBatchEntry contractPostBatchGraph = PXGraph.CreateInstance<ContractPostBatchEntry>();
                    ContractPostPeriodEntry contractPostPeriodGraph = PXGraph.CreateInstance<ContractPostPeriodEntry>();

                    FSContractPostDoc fsContractPostDocRow = null;
                    FSContractPostBatch fsContractPostBatchRow = null;

                    List<ContractInvoiceLine> invoiceDet = new List<ContractInvoiceLine>();
                    List<ContractInvoiceLine> contractPeriodLines = null;
                    List<ContractInvoiceLine> contractInvoiceDetList = null;
                    List<ContractInvoiceLine> equipmentInvoiceDetList = null;

                    FSServiceContract fsServiceContractRow = null;
                    FSContractPeriod fsContractPeriodRow = null;

                    IInvoiceContractGraph invoiceGraph = GetInvoiceGraph(fsSetupRow);
                    
                    foreach (ContractPeriodToPost contractPeriodToPostRow in contractPeriodToPostRows)
                    {
                        fsContractPostDocRow = null;
                        GetContractAndPeriod(contractPeriodToPostRow.ServiceContractID, contractPeriodToPostRow.ContractPeriodID, out fsServiceContractRow, out fsContractPeriodRow);
                        contractPeriodLines = GetContractPeriodLines(contractPeriodToPostRow);

                        if (contractPeriodLines.Count > 0 && fsServiceContractRow != null && fsContractPeriodRow !=null)
                        {
                            contractInvoiceDetList = GetContractInvoiceLines(contractPeriodToPostRow);
                            invoiceDet.AddRange(contractInvoiceDetList);

                            if (fsSetupRow.ContractPostTo == ID.Contract_PostTo.SALES_ORDER_MODULE)
                            { 
                                equipmentInvoiceDetList = GetEquipmentInvoiceLines(contractPeriodToPostRow);
                            }

                            List<ContractInvoiceLine> contractInvoiceLines = GetInvoiceLines(invoiceDet, contractPeriodLines, contractInvoiceDetList, fsSetupRow.ContractPostTo);

                            if (equipmentInvoiceDetList != null && equipmentInvoiceDetList.Count > 0)
                            {
                                contractInvoiceLines.AddRange(equipmentInvoiceDetList);
                                invoiceDet.AddRange(equipmentInvoiceDetList);
                            }

                            try
                            {
                                using (var ts = new PXTransactionScope())
                                {
                                    if (fsContractPostBatchRow == null)
                                    {
                                        fsContractPostBatchRow = contractPostBatchGraph.CreatePostingBatch(filter.UpToDate, filter.InvoiceDate, filter.InvoiceFinPeriodID, SetupRecord.Current.ContractPostTo);
                                    }

                                    fsContractPostDocRow = invoiceGraph.CreateInvoiceByContract(graphCreateInvoiceByServiceOrderPost, filter.InvoiceDate, filter.InvoiceFinPeriodID, fsContractPostBatchRow, fsServiceContractRow, fsContractPeriodRow, contractInvoiceLines);

                                    fsContractPostDocRow = CreateContractPostDoc(contractPostPeriodGraph, fsContractPostDocRow);

                                    CreateContractPostDet(contractPostPeriodGraph, fsContractPostDocRow, invoiceDet);

                                    UpdateSourcePostDoc(fsSetupRow, fsContractPostDocRow, fsServiceContractRow);

                                    CreateContractPostRegister(this, fsContractPostDocRow);

                                    contractPeriodToPostRow.ContractPostBatchID = fsContractPostBatchRow.ContractPostBatchID;

                                    ts.Complete();
                                }

                                PXProcessing<ContractPeriodToPost>.SetInfo(rowIndex, TX.Messages.RECORD_PROCESSED_SUCCESSFULLY);
                            }
                            catch (Exception e)
                            {
                                PXProcessing<ContractPeriodToPost>.SetError(rowIndex, e.Message);
                            }
                        }
                        else
                        {
                            PXProcessing<ContractPeriodToPost>.SetError(rowIndex, TX.Error.PERIOD_WITHOUT_DETAILS);
                        }

                        rowIndex++;
                    }
                });
        }

        public virtual void InvoiceContractPeriodFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            InvoiceContractPeriodFilter createInvoiceFilterRow = (InvoiceContractPeriodFilter)e.Row;

            string errorMessage = PXUIFieldAttribute.GetErrorOnly<InvoiceContractPeriodFilter.invoiceFinPeriodID>(cache, createInvoiceFilterRow);
            bool enableProcessButtons = string.IsNullOrEmpty(errorMessage) == true;

            Contracts.SetProcessAllEnabled(enableProcessButtons);
            Contracts.SetProcessEnabled(enableProcessButtons);
        }

        public static IInvoiceContractGraph GetInvoiceGraph(FSSetup fsSetupRow)
        {
            if (fsSetupRow.ContractPostTo == ID.Batch_PostTo.SO)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
                {
                    return PXGraph.CreateInstance<SOOrderEntry>().GetExtension<SM_SOOrderEntry>();
                }
                else
                {
                    throw new PXException(TX.Error.DISTRIBUTION_MODULE_IS_DISABLED);
                }
            }
            else if (fsSetupRow.ContractPostTo == ID.Batch_PostTo.AR)
            {
                return PXGraph.CreateInstance<ARInvoiceEntry>().GetExtension<SM_ARInvoiceEntry>();
            }
            else if (fsSetupRow.ContractPostTo == ID.Batch_PostTo.SI)
            {
                throw new PXSetPropertyException(TX.Error.SOINVOICE_FROM_CONTRACT_NOT_IMPLEMENTED_CHANGE_IN_X_OR_Y, PXErrorLevel.Error,
                    DACHelper.GetDisplayName(typeof(FSEquipmentSetup)),
                    DACHelper.GetDisplayName(typeof(FSRouteSetup)));
            }

            return null;
        }

        public virtual void GetContractAndPeriod(int? serviceContractID, int? contractPeriodID, out FSServiceContract fsServiceContractRow, out FSContractPeriod fsContractPeriodRow)
        {
            fsContractPeriodRow = null;
            fsServiceContractRow = null;

            var result = (PXResult<FSServiceContract, FSContractPeriod>)
                                PXSelectJoin<FSServiceContract,
                                InnerJoin<FSContractPeriod,
                                    On<FSContractPeriod.contractPeriodID, Equal<FSContractPeriod.contractPeriodID>>>,
                                Where<
                                        FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>,
                                    And<
                                        FSContractPeriod.contractPeriodID, Equal<Required<FSContractPeriod.contractPeriodID>>>>>
                                .Select(this, serviceContractID, contractPeriodID);

            if (result != null)
            {
                fsServiceContractRow = (FSServiceContract)result;
                fsContractPeriodRow = (FSContractPeriod)result;
            }
        }

        public virtual List<ContractInvoiceLine> GetContractPeriodLines(ContractPeriodToPost contractPeriodToPostRow)
        {
            var resultSet = PXSelectJoin<FSContractPeriodDet,
                            InnerJoin<FSContractPeriod,
                            On<
                                FSContractPeriod.contractPeriodID, Equal<FSContractPeriodDet.contractPeriodID>>,
                            InnerJoin<FSServiceContract,
                            On<
                                FSServiceContract.serviceContractID, Equal<FSContractPeriod.serviceContractID>>,
                            InnerJoin<FSBranchLocation,
                            On<
                                FSBranchLocation.branchLocationID, Equal<FSServiceContract.branchLocationID>>>>>,
                            Where<
                                FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>,
                                And<FSContractPeriod.contractPeriodID, Equal<Required<FSContractPeriod.contractPeriodID>>>>>
                            .Select(this, contractPeriodToPostRow.ServiceContractID, contractPeriodToPostRow.ContractPeriodID);

            var docLines = new List<ContractInvoiceLine>();

            foreach (PXResult<FSContractPeriodDet, FSContractPeriod, FSServiceContract, FSBranchLocation> result in resultSet)
            {
                docLines.Add(new ContractInvoiceLine(result));
            }

            return docLines;
        }

        public virtual List<ContractInvoiceLine> GetContractInvoiceLines(ContractPeriodToPost contractPeriodToPostRow)
        {
            List<ContractInvoiceLine> contractInvoiceLines = new List<ContractInvoiceLine>();

            var appointmentDetResultSet = PXSelectJoin<FSAppointmentDet,
                                          InnerJoin<FSSODet,
                                          On<
                                              FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>,
                                          InnerJoin<FSAppointment,
                                          On<
                                              FSAppointment.srvOrdType, Equal<FSAppointmentDet.srvOrdType>,
                                              And<FSAppointment.refNbr, Equal<FSAppointmentDet.refNbr>>>>>,
                                          Where<
                                              FSAppointment.billServiceContractID, Equal<Required<FSAppointment.billServiceContractID>>,
                                              And<FSAppointment.billContractPeriodID, Equal<Required<FSAppointment.billContractPeriodID>>,
                                              And<FSAppointmentDet.equipmentAction, Equal<FSAppointmentDet.equipmentAction.None>,
                                              And<FSAppointmentDet.lineType, NotEqual<FSLineType.Comment>,
                                              And<FSAppointmentDet.lineType, NotEqual<FSLineType.Instruction>,
                                              And<FSAppointmentDet.isCanceledNotPerformed, NotEqual<True>,
                                              And<FSAppointmentDet.lineType, NotEqual<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                                              And<FSAppointmentDet.isPrepaid, Equal<False>>>>>>>>>>
                                          .Select(this, contractPeriodToPostRow.ServiceContractID, contractPeriodToPostRow.ContractPeriodID);

            foreach (PXResult<FSAppointmentDet, FSSODet, FSAppointment> result in appointmentDetResultSet)
            {
                contractInvoiceLines.Add(new ContractInvoiceLine(result));
            }

            var appointmentInventoryItemResultSet = PXSelectJoin<FSAppointmentDet,
                                                    InnerJoin<FSSODet,
                                                    On<
                                                        FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>,
                                                    InnerJoin<FSAppointment,
                                                    On<
                                                        FSAppointment.srvOrdType, Equal<FSAppointmentDet.srvOrdType>,
                                                        And<FSAppointment.refNbr, Equal<FSAppointmentDet.refNbr>>>>>,
                                                    Where<
                                                        FSAppointment.billServiceContractID, Equal<Required<FSAppointment.billServiceContractID>>,
                                                        And<FSAppointment.billContractPeriodID, Equal<Required<FSAppointment.billContractPeriodID>>,
                                                        And<FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                                                        And<FSAppointmentDet.equipmentAction, Equal<FSAppointmentDet.equipmentAction.None>>>>>>
                                                    .Select(this, contractPeriodToPostRow.ServiceContractID, contractPeriodToPostRow.ContractPeriodID);

            foreach (PXResult<FSAppointmentDet, FSSODet, FSAppointment> result in appointmentInventoryItemResultSet)
            {
                contractInvoiceLines.Add(new ContractInvoiceLine(result));
            }

            var soDetResultSet = PXSelectJoin<FSSODet,
                                InnerJoin<FSServiceOrder,
                                    On<FSServiceOrder.srvOrdType, Equal<FSSODet.srvOrdType>,
                                        And<FSServiceOrder.refNbr, Equal<FSSODet.refNbr>>>>,
                                Where<
                                    FSServiceOrder.billServiceContractID, Equal<Required<FSServiceOrder.billServiceContractID>>,
                                    And<FSServiceOrder.billContractPeriodID, Equal<Required<FSServiceOrder.billContractPeriodID>>,
                                    And<FSSODet.equipmentAction, Equal<FSAppointmentDet.equipmentAction.None>,
                                    And<FSSODet.lineType, NotEqual<FSLineType.Comment>,
                                    And<FSSODet.lineType, NotEqual<FSLineType.Instruction>,
                                    And<FSSODet.status, NotEqual<FSSODet.status.Canceled>,
                                    And<FSSODet.isPrepaid, Equal<False>>>>>>>>>
                                .Select(this, contractPeriodToPostRow.ServiceContractID, contractPeriodToPostRow.ContractPeriodID);

            foreach (PXResult<FSSODet, FSServiceOrder> result in soDetResultSet)
            {
                contractInvoiceLines.Add(new ContractInvoiceLine(result));
            }

            return contractInvoiceLines;
        }

        public virtual List<ContractInvoiceLine> GetEquipmentInvoiceLines(ContractPeriodToPost contractPeriodToPostRow)
        {
            List<ContractInvoiceLine> contractInvoiceLines = new List<ContractInvoiceLine>();

            var appointmentDetResultSet = PXSelectJoin<FSAppointmentDet,
                                            InnerJoin<FSSODet,
                                                On<FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>,
                                            InnerJoin<FSAppointment,
                                                On<FSAppointment.srvOrdType, Equal<FSAppointmentDet.srvOrdType>,
                                                    And<FSAppointment.refNbr, Equal<FSAppointmentDet.refNbr>>>>>,
                                            Where<
                                                FSAppointment.billServiceContractID, Equal<Required<FSAppointment.billServiceContractID>>,
                                                And<FSAppointment.billContractPeriodID, Equal<Required<FSAppointment.billContractPeriodID>>,
                                                And<FSAppointmentDet.equipmentAction, NotEqual<FSAppointmentDet.equipmentAction.None>,
                                                And<FSAppointmentDet.lineType, NotEqual<FSLineType.Comment>,
                                                And<FSAppointmentDet.lineType, NotEqual<FSLineType.Instruction>,
                                                And<FSAppointmentDet.isCanceledNotPerformed, NotEqual<True>,
                                                And<FSAppointmentDet.lineType, NotEqual<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                                                And<FSAppointmentDet.isPrepaid, Equal<False>>>>>>>>>>
                                            .Select(this, contractPeriodToPostRow.ServiceContractID, contractPeriodToPostRow.ContractPeriodID);

            foreach (PXResult<FSAppointmentDet, FSSODet, FSAppointment> result in appointmentDetResultSet)
            {
                contractInvoiceLines.Add(new ContractInvoiceLine(result));
            }

            var appointmentInventoryItemResultSet = PXSelectJoin<FSAppointmentDet,
                                                    InnerJoin<FSSODet,
                                                        On<FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>,
                                                    InnerJoin<FSAppointment,
                                                        On<FSAppointment.srvOrdType, Equal<FSAppointmentDet.srvOrdType>,
                                                            And<FSAppointment.refNbr, Equal<FSAppointmentDet.refNbr>>>>>,
                                                    Where<
                                                        FSAppointment.billServiceContractID, Equal<Required<FSAppointment.billServiceContractID>>,
                                                        And<FSAppointment.billContractPeriodID, Equal<Required<FSAppointment.billContractPeriodID>>,
                                                        And<FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                                                        And<FSAppointmentDet.equipmentAction, NotEqual<FSAppointmentDet.equipmentAction.None>>>>>>
                                                    .Select(this, contractPeriodToPostRow.ServiceContractID, contractPeriodToPostRow.ContractPeriodID);

            foreach (PXResult<FSAppointmentDet, FSSODet, FSAppointment> result in appointmentInventoryItemResultSet)
            {
                contractInvoiceLines.Add(new ContractInvoiceLine(result));
            }

            var soDetResultSet = PXSelectJoin<FSSODet,
                                InnerJoin<FSServiceOrder,
                                    On<FSServiceOrder.srvOrdType, Equal<FSSODet.srvOrdType>,
                                        And<FSServiceOrder.refNbr, Equal<FSSODet.refNbr>>>>,
                                Where<
                                    FSServiceOrder.billServiceContractID, Equal<Required<FSServiceOrder.billServiceContractID>>,
                                    And<FSServiceOrder.billContractPeriodID, Equal<Required<FSServiceOrder.billContractPeriodID>>,
                                    And<FSSODet.equipmentAction, NotEqual<FSAppointmentDet.equipmentAction.None>,
                                    And<FSSODet.lineType, NotEqual<FSLineType.Comment>,
                                    And<FSSODet.lineType, NotEqual<FSLineType.Instruction>,
                                    And<FSSODet.status, NotEqual<FSSODet.status.Canceled>,
                                    And<FSSODet.isPrepaid, Equal<False>>>>>>>>>
                                .Select(this, contractPeriodToPostRow.ServiceContractID, contractPeriodToPostRow.ContractPeriodID);

            foreach (PXResult<FSSODet, FSServiceOrder> result in soDetResultSet)
            {
                contractInvoiceLines.Add(new ContractInvoiceLine(result));
            }

            return contractInvoiceLines;
        }

        public virtual List<ContractInvoiceLine> GetInvoiceLines(List<ContractInvoiceLine> invoiceLine, List<ContractInvoiceLine> contractPeriodLines, List<ContractInvoiceLine> contractInvoiceDetLines, string PostTo)
        {
            decimal? coveredQty = 0;

            var periodInvoiceLines = contractPeriodLines.Select(cl => new ContractInvoiceLine(cl)).ToList();

            List<ContractInvoiceLine> invoiceLines = new List<ContractInvoiceLine>();

            if (PostTo == ID.Contract_PostTo.ACCOUNTS_RECEIVABLE_MODULE)
            {
                invoiceLines = contractInvoiceDetLines.GroupBy(l => new {
                                    l.BillingRule,
                                    l.InventoryID,
                                    l.UOM,
                                    l.SMEquipmentID,
                                    l.CuryUnitPrice,
                                    l.ContractRelated
                                })
                                .Select(cl => new ContractInvoiceLine(
                                    cl.First(),
                                    cl.Sum(p => p.Qty))
                                ).ToList();
            }
            else
            {
                invoiceLines = contractInvoiceDetLines.GroupBy(l => new {
                                                                l.BillingRule,
                                                                l.InventoryID,
                                                                l.UOM,
                                                                l.SMEquipmentID,
                                                                l.CuryUnitPrice,
                                                                l.ContractRelated,
                                                                l.SubItemID,
                                                                l.SiteID,
                                                                l.SiteLocationID,
                                                                l.IsBillable
                                                            })
                                                            .Select(cl => new ContractInvoiceLine(
                                                                cl.First(),
                                                                cl.Sum(p => p.Qty))
                                                            ).ToList();
            }

            for (int j = 0; j < invoiceLines.Count(); j++)
            {
                if (invoiceLines[j].ContractRelated == true)
                {
                    for (int i = 0; i < periodInvoiceLines.Count(); i++)
                    {
                        if (invoiceLines[j].InventoryID == periodInvoiceLines[i].InventoryID &&
                                (periodInvoiceLines[i].SMEquipmentID == null && invoiceLines[j].SMEquipmentID != null))
                        {
                            invoiceLines[j].SMEquipmentID = null;
                        }
                    }
                }
            }

            if (PostTo == ID.Contract_PostTo.ACCOUNTS_RECEIVABLE_MODULE)
            {
                invoiceLines = invoiceLines.GroupBy(l => new {
                                    l.BillingRule,
                                    l.InventoryID,
                                    l.UOM,
                                    l.SMEquipmentID,
                                    l.CuryUnitPrice,
                                    l.ContractRelated
                                })
                                .Select(cl => new ContractInvoiceLine(cl.First())).ToList();
            }
            else
            {
                invoiceLines = invoiceLines.GroupBy(l => new {
                                    l.BillingRule,
                                    l.InventoryID,
                                    l.UOM,
                                    l.SMEquipmentID,
                                    l.CuryUnitPrice,
                                    l.ContractRelated,
                                    l.SubItemID,
                                    l.SiteID,
                                    l.SiteLocationID,
                                    l.IsBillable
                                })
                                .Select(cl => new ContractInvoiceLine(cl.First())).ToList();
            }

            var result = new List<ContractInvoiceLine>();

            foreach (ContractInvoiceLine line in invoiceLines)
            {
                if (line.ContractRelated == true)
                {
                    for (int i = 0; i < periodInvoiceLines.Count(); i++)
                    {
                        if (line.InventoryID == periodInvoiceLines[i].InventoryID &&
                                (periodInvoiceLines[i].SMEquipmentID == null || periodInvoiceLines[i].SMEquipmentID == line.SMEquipmentID) &&
                                    line.BillingRule == periodInvoiceLines[i].BillingRule)
                        {
                            coveredQty = 0;
                            coveredQty = periodInvoiceLines[i].Qty;
                            periodInvoiceLines[i].Qty = periodInvoiceLines[i].Qty - line.Qty;
                            periodInvoiceLines[i].TranDescPrefix = TX.Contract_PostingPrefixPeriod.CONTRACT_COVERAGE;

                            if (periodInvoiceLines[i].Qty < 0)
                            {
                                periodInvoiceLines[i].Qty = periodInvoiceLines[i].Qty * -1;
                                periodInvoiceLines[i].CuryUnitPrice = line.OverageItemPrice;
                                periodInvoiceLines[i].TranDescPrefix = TX.Contract_PostingPrefixPeriod.CONTRACT_OVERAGE;
                                line.Qty = coveredQty;
                            }

                            if(periodInvoiceLines[i].Qty > 0)
                            { 
                                periodInvoiceLines[i].Processed = true;
                                invoiceLine.Add(periodInvoiceLines[i]);
                                result.Add(periodInvoiceLines[i]);
                            }

                            line.TranDescPrefix = TX.Contract_PostingPrefixPeriod.CONTRACT_USAGE;
                        }
                    }
                }

                line.Processed = true;
                result.Add(line);
            }

            foreach (ContractInvoiceLine periodDet in periodInvoiceLines)
            {
                if (periodDet.Processed != true)
                {
                    periodDet.TranDescPrefix = TX.Contract_PostingPrefixPeriod.CONTRACT_COVERAGE;

                    if (periodDet.Qty > 0)
                    {
                        periodDet.Processed = true;
                        invoiceLine.Add(periodDet);
                        result.Add(periodDet);
                    }
                }
            }

            return result;
        }

        public static void CreateContractPostRegister(PXGraph graph, FSContractPostDoc fsContractPostDocRow)
        {
            PXCache<FSContractPostRegister> cacheFSPostRegister = new PXCache<FSContractPostRegister>(graph);

            FSContractPostRegister fsPostRegisterRow = new FSContractPostRegister();

            fsPostRegisterRow.ServiceContractID = fsContractPostDocRow.ServiceContractID;
            fsPostRegisterRow.ContractPeriodID = fsContractPostDocRow.ContractPeriodID;
            fsPostRegisterRow.ContractPostBatchID = fsContractPostDocRow.ContractPostBatchID;
            fsPostRegisterRow.PostedTO = fsContractPostDocRow.PostedTO;
            fsPostRegisterRow.PostDocType = fsContractPostDocRow.PostDocType;
            fsPostRegisterRow.PostRefNbr = fsContractPostDocRow.PostRefNbr;

            cacheFSPostRegister.Insert(fsPostRegisterRow);

            cacheFSPostRegister.Persist(PXDBOperation.Insert);
        }

        public static FSContractPostDoc CreateContractPostDoc(ContractPostPeriodEntry graph, FSContractPostDoc fsContractPostDocRow)
        {
            graph.ContractPostDocRecord.Current = graph.ContractPostDocRecord.Insert(fsContractPostDocRow);

            graph.Save.Press();

            return graph.ContractPostDocRecord.Current;
        }

        private void CreateContractPostDet(ContractPostPeriodEntry graph, FSContractPostDoc fsContractPostDocRow, List<ContractInvoiceLine> contractInvoiceDetLines)
        {
            foreach (ContractInvoiceLine contractInvoiceLine in contractInvoiceDetLines)
            {
                graph.ContractPostDetRecords.Insert(CreatePostDet(fsContractPostDocRow, contractInvoiceLine));
            }

            graph.Save.Press();
        }

        public static FSContractPostDet CreatePostDet(FSContractPostDoc fsContractPostDocRow, ContractInvoiceLine contractInvoiceLine)
        {
            FSContractPostDet fsContractPostDetRow = new FSContractPostDet();

            fsContractPostDetRow.AppDetID = contractInvoiceLine.AppDetID;
            fsContractPostDetRow.AppointmentID = contractInvoiceLine.AppointmentID;
            fsContractPostDetRow.ContractPeriodDetID = contractInvoiceLine.ContractPeriodDetID;
            fsContractPostDetRow.ContractPeriodID = contractInvoiceLine.ContractPeriodID;
            fsContractPostDetRow.ContractPostBatchID = fsContractPostDocRow.ContractPostBatchID;
            fsContractPostDetRow.ContractPostDocID = fsContractPostDocRow.ContractPostDocID;
            fsContractPostDetRow.SODetID = contractInvoiceLine.SODetID;
            fsContractPostDetRow.SOID = contractInvoiceLine.SOID;

            return fsContractPostDetRow;
        }

        public virtual void UpdateSourcePostDoc(FSSetup fsSetupRow, FSContractPostDoc fsContractPostDocRow, FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
            {
                ServiceContractEntry ServiceContractGraph = PXGraph.CreateInstance<ServiceContractEntry>();
                ServiceContractGraph.ServiceContractRecords.Current = ServiceContractGraph.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsContractPostDocRow.ServiceContractID, fsServiceContractRow.CustomerID);
                ServiceContractGraph.InvoiceBillingPeriod(fsSetupRow, fsContractPostDocRow);
            }
            else
            {
                RouteServiceContractEntry RouteServiceContractGraph = PXGraph.CreateInstance<RouteServiceContractEntry>();
                RouteServiceContractGraph.ServiceContractRecords.Current = RouteServiceContractGraph.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsContractPostDocRow.ServiceContractID, fsServiceContractRow.CustomerID);
                RouteServiceContractGraph.InvoiceBillingPeriod(fsSetupRow, fsContractPostDocRow);
            }
        }
    }
}