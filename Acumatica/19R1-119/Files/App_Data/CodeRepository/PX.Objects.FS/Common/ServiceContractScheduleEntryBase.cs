using PX.Data;
using PX.Objects.CR;
using PX.Objects.FS.Scheduler;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class ServiceContractScheduleEntryBase<TGraph, TPrimary, Field, EntityField> : PXGraph<TGraph, TPrimary>
        where TGraph : PX.Data.PXGraph
        where TPrimary : class, PX.Data.IBqlTable, new()
        where Field : PX.Data.IBqlField
        where EntityField : PX.Data.IBqlField
    {
        public bool statusChanged = false;

        #region Selects
        [PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<Contact> Contact;
        [PXHidden]
        public PXSelect<FSSchedule> Schedule;

        public PXFilter<FromToFilter> FromToFilter;

        // Baccount workaround
        [PXHidden]
        public PXSetup<FSSetup> Setup;
        [PXHidden]
        public PXSelect<FSSalesPrice> salesPriceRecords;

        public PXSelect<TPrimary,
                    Where<
                        EntityField, Equal<Optional<EntityField>>>> ContractScheduleRecords;

        public PXSelect<TPrimary, 
                    Where<
                        Field, Equal<Current<Field>>>> ContractScheduleSelected;

        public PXSetup<FSServiceContract>.Where<
                    Where<FSServiceContract.serviceContractID, Equal<Current<EntityField>>>> CurrentServiceContract;

        public PXSelect<FSScheduleDetService,
                            Where<
                                FSScheduleDetService.scheduleID, Equal<Current<Field>>>> ScheduleDetServices;

        public PXSelect<FSScheduleDetPart,
                            Where<
                                FSScheduleDetPart.scheduleID, Equal<Current<Field>>>> ScheduleDetParts;

        [PXCopyPasteHiddenView]
        public PXSelect<FSSalesPrice,
                        Where<
                            FSSalesPrice.serviceContractID, Equal<Required<FSSalesPrice.serviceContractID>>,
                        And<
                            FSSalesPrice.inventoryID, Equal<Required<FSSalesPrice.inventoryID>>,
                        And<
                            FSSalesPrice.lineType, NotEqual<ListField_LineType_SalesPrices.Inventory_Item>>>>> SalesPriceServices;

        [PXCopyPasteHiddenView]
        public PXSelect<FSSalesPrice,
                        Where<
                            FSSalesPrice.serviceContractID, Equal<Required<FSSalesPrice.serviceContractID>>,
                        And<
                            FSSalesPrice.inventoryID, Equal<Required<FSSalesPrice.inventoryID>>,
                        And<
                            FSSalesPrice.lineType, Equal<ListField_LineType_SalesPrices.Inventory_Item>>>>> SalesPriceInventoryItems;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSScheduleDetService,
                            InnerJoin<FSSchedule,
                                On<FSSchedule.scheduleID, Equal<FSScheduleDetService.scheduleID>>>,
                            Where<
                                FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>,
                                And<FSScheduleDetService.inventoryID, Equal<Required<FSScheduleDetService.inventoryID>>,
                                And<FSScheduleDetService.scheduleDetID, NotEqual<Required<FSScheduleDetService.scheduleDetID>>>>>> ScheduleDetServicesByContract;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSServiceTemplateDet,
                            InnerJoin<FSScheduleDet,
                                On<FSScheduleDet.serviceTemplateID, Equal<FSServiceTemplateDet.serviceTemplateID>>,
                            InnerJoin<FSSchedule,
                                On<FSSchedule.scheduleID, Equal<FSScheduleDet.scheduleID>>>>,
                            Where<
                                  FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>,
                                  And<FSServiceTemplateDet.inventoryID, Equal<Required<FSServiceTemplateDet.inventoryID>>,
                                  And<FSScheduleDet.scheduleDetID, NotEqual<Required<FSScheduleDet.scheduleDetID>>>>>> ScheduleDetServicesInServiceTemplatesByContract;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSScheduleDetPart,
                            InnerJoin<FSSchedule,
                                On<FSSchedule.scheduleID, Equal<FSScheduleDetPart.scheduleID>>>,
                            Where<
                                FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>,
                                And<FSScheduleDetPart.inventoryID, Equal<Required<FSScheduleDetPart.inventoryID>>,
                                And<FSScheduleDetPart.scheduleDetID, NotEqual<Required<FSScheduleDetPart.scheduleDetID>>>>>> ScheduleDetPartsByContract;

        [PXCopyPasteHiddenView]
        [PXVirtualDAC]
        public PXSelectReadonly<ScheduleProjection> ScheduleProjectionRecords;

        [PXCopyPasteHiddenView]
        public PXSetup<FSServiceContract>.Where<
               Where<
                   FSServiceContract.serviceContractID, Equal<Current<EntityField>>>> ContractSelected;

        [PXCopyPasteHiddenView]
        public PXSelect<FSContractAction,
               Where<
                   FSContractAction.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>>> ContractHistory;

        [PXHidden]
        public PXSelectReadonly<FSSrvOrdTypeAttributeList> AttributeListRecords;

        protected virtual IEnumerable attributeListRecords()
        {
            FSSrvOrdTypeAttributeList fsSrvOrdTypeAttributeRow = AttributeListRecords.Current;

            if (fsSrvOrdTypeAttributeRow == null)
            {
                fsSrvOrdTypeAttributeRow = new FSSrvOrdTypeAttributeList();
            }

            FSSchedule fsContractScheduleRow = ContractScheduleRecords.Current as FSSchedule;

            if (fsContractScheduleRow != null)
            {
                fsSrvOrdTypeAttributeRow.ScheduleID = fsContractScheduleRow.ScheduleID;
                fsSrvOrdTypeAttributeRow.SrvOrdType = fsContractScheduleRow.SrvOrdType;
                fsSrvOrdTypeAttributeRow.NoteID = fsContractScheduleRow.NoteID;

                yield return fsSrvOrdTypeAttributeRow;
            }
            else
            {
                yield return null;
            }
        }

        [PXViewName(CR.Messages.Answers)]
        public FSAttributeList<FSSrvOrdTypeAttributeList, TPrimary> Answers;

        #endregion

        #region Private Members
        private PXGraph tempGraph;

        private PXGraph TempGraph
        {
            get
            {
                if (this.tempGraph == null)
                {
                    this.tempGraph = new PXGraph();
                }

                return this.tempGraph;
            }
        }
        #endregion

        #region Public Functions

        public void FSSchedule_Row_Deleted_PartialHandler(PXCache cache, PXRowDeletedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            //Detaching ServiceOrders and Appointments created by schedule generation linked to this Schedule.
            PXUpdate<
                Set<FSServiceOrder.scheduleID, Required<FSServiceOrder.scheduleID>>,
            FSServiceOrder,
            Where<FSServiceOrder.scheduleID, Equal<Required<FSServiceOrder.scheduleID>>>>
            .Update(this, null, fsScheduleRow.ScheduleID);

            PXUpdate<
                Set<FSAppointment.scheduleID, Required<FSAppointment.scheduleID>>,
            FSAppointment,
            Where<FSAppointment.scheduleID, Equal<Required<FSAppointment.scheduleID>>>>
            .Update(this, null, fsScheduleRow.ScheduleID);
        }

        public void FSSchedulePart_InventoryID_FieldUpdated_Handler(PXCache cache, FSScheduleDetPart fsSchedulePartRow)
        {
            object lineTypeValue;

            if (fsSchedulePartRow.LineType == null)
            {
                //We just run the field defaulting because this is the first field when you try to insert a new line.
                ScheduleDetParts.Cache.RaiseFieldDefaulting<FSScheduleDetPart.lineType>(ScheduleDetParts.Current, out lineTypeValue);
                fsSchedulePartRow.LineType = (string)lineTypeValue;
            }

            SharedFunctions.UpdateEquipmentFields(this, cache, fsSchedulePartRow, fsSchedulePartRow.InventoryID);
        }

        public void FSScheduleService_InventoryID_FieldUpdated_Handler(PXCache cache, FSScheduleDetService fsScheduleServiceRow)
        {
            object lineTypeValue;

            if (fsScheduleServiceRow.LineType == null)
            {
                //We just run the field defaulting because this is the first field when you try to insert a new line.
                ScheduleDetServices.Cache.RaiseFieldDefaulting<FSScheduleDetService.lineType>(ScheduleDetServices.Current, out lineTypeValue);
                fsScheduleServiceRow.LineType = (string)lineTypeValue;
            }
        }

        public List<ScheduleProjection> Delegate_ScheduleProjectionRecords(PXCache cache, FSSchedule fsScheduleRow, FromToFilter filter, string recordType)
        {
            FSSchedule fsRouteContractScheduleRow = fsScheduleRow;
            List<ScheduleProjection> returnList = new List<ScheduleProjection>();
            DateTime? dateBegin = filter.DateBegin;
            DateTime? dateEnd = null;

            if (fsScheduleRow == null)
            {
                return returnList;
            }

            if (filter.DateEnd.HasValue)
            {
                dateEnd = filter.DateEnd;
            }

            if (dateBegin.HasValue == true && dateEnd.HasValue == false)
            {
                dateEnd = filter.DateEnd.HasValue ? filter.DateEnd : dateBegin.Value.AddYears(1);
            }

            if (dateBegin != null && dateEnd != null)
            {
                var period = new Period((DateTime)dateBegin, (DateTime)dateEnd);

                List<Scheduler.Schedule> mapScheduleResults = new List<Scheduler.Schedule>();
                var generator = new TimeSlotGenerator();

                mapScheduleResults = MapFSScheduleToSchedule.convertFSScheduleToSchedule(cache, fsScheduleRow, dateEnd, recordType);

                if (recordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
                {
                    foreach (Scheduler.Schedule schedule in mapScheduleResults)
                    {
                        schedule.Priority = (int?)RouteScheduleProcess.SetSchedulePriority(schedule, this);
                        schedule.RouteInfoList = RouteScheduleProcess.getRouteListFromSchedule(schedule, this);
                    }
                }

                List<TimeSlot> timeSlots = generator.GenerateCalendar(period, mapScheduleResults);

                foreach (TimeSlot timeSlot in timeSlots)
                {
                    ScheduleProjection scheduleProjection = new ScheduleProjection();
                    scheduleProjection.Date = timeSlot.DateTimeBegin;
                    scheduleProjection.BeginDateOfWeek = SharedFunctions.StartOfWeek((DateTime)scheduleProjection.Date, DayOfWeek.Monday);
                    returnList.Add(scheduleProjection);
                }
            }

            return returnList;
        }

        [Obsolete("Remove in 2020R1")]
        /// <summary>
        /// Verifies if a <c>FSSalesPrice</c> record already exists in DB.
        /// </summary>
        /// <returns>Returns true if it exists, otherwise false.</returns>
        public bool IsThisSalesPricePersisted(PXCache cache, int? serviceContractID, int? inventoryID)
        {
            FSSalesPrice fsSalesPriceRow = PXSelect<FSSalesPrice,
                                           Where<
                                                FSSalesPrice.serviceContractID, Equal<Required<FSSalesPrice.serviceContractID>>,
                                           And<
                                                FSSalesPrice.inventoryID, Equal<Required<FSSalesPrice.inventoryID>>>>>
                                           .Select(cache.Graph, serviceContractID, inventoryID);

            return fsSalesPriceRow != null;
        }

        [Obsolete("Remove in 2020R1")]
        /// <summary>
        /// Gets an InventoryItem list containing the service(s) that belong(s) to the item specified in the <c>fsScheduleDetServiceRow</c> instance.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="fsScheduleDetRow"><c>FSScheduleDetService</c> instance.</param>
        /// <param name="previousScheduleDetValue">Flag indicating whether or not the desired value is the previous one.</param>
        /// <returns>Returns a <c>PXResultset</c> containing the inventory item(s).</returns>
        public PXResultset<InventoryItem> GetInventoryItemListFromScheduleDet(PXCache cache, FSScheduleDet fsScheduleDetRow, bool previousScheduleDetValue = false)
        {
            string lineType = fsScheduleDetRow.LineType;
            PXResultset<InventoryItem> bqlResultSet = new PXResultset<InventoryItem>();

            if (previousScheduleDetValue)
            {
                lineType = (string)cache.GetValueOriginal<FSScheduleDet.lineType>(fsScheduleDetRow);
            }

            if (lineType != ID.LineType_All.SERVICE_TEMPLATE)
            {
                int? inventoryItemID = fsScheduleDetRow.InventoryID;
                if (previousScheduleDetValue)
                {
                    inventoryItemID = (int?)cache.GetValueOriginal<FSScheduleDet.inventoryID>(fsScheduleDetRow);
                }

                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cache.Graph, inventoryItemID);
                if (inventoryItemRow != null)
                {
                    bqlResultSet.Add(new PXResult<InventoryItem>(inventoryItemRow));
                }
            }
            else
            {
                int? serviceTemplateID = fsScheduleDetRow.ServiceTemplateID;
                if (previousScheduleDetValue)
                {
                    serviceTemplateID = (int?)cache.GetValueOriginal<FSScheduleDet.serviceTemplateID>(fsScheduleDetRow);
                }

                bqlResultSet = GetInventoryItemListFromServiceTemplate(cache, serviceTemplateID);
            }

            return bqlResultSet;
        }

        /// <summary>
        /// Gets an InventoryItem list containing the service(s) that belong(s) to the serviceTemplateID.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="serviceTemplateID">Service Template ID.</param>
        /// <returns>Returns a <c>PXResultset</c> containing the inventory item(s).</returns>
        public PXResultset<InventoryItem> GetInventoryItemListFromServiceTemplate(PXCache cache, int? serviceTemplateID)
        {
            PXResultset<InventoryItem> bqlResultSet = new PXResultset<InventoryItem>();

            foreach (FSServiceTemplateDetService fsServiceTemplateDetRow in PXSelect<FSServiceTemplateDetService,
                                                                            Where<
                                                                                  FSServiceTemplateDetService.serviceTemplateID,
                                                                                        Equal<Required<FSServiceTemplateDetService.serviceTemplateID>>>>
                                                                            .Select(cache.Graph, serviceTemplateID))
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cache.Graph, fsServiceTemplateDetRow.InventoryID);
                if (inventoryItemRow != null)
                {
                    bqlResultSet.Add(new PXResult<InventoryItem>(inventoryItemRow));
                }
            }

            return bqlResultSet;
        }

        [Obsolete("Remove in 2020R1")]
        /// <summary>
        /// Verifies if the <c>FSSalesPrice</c> associated to the inventory ID needs to be Deleted.
        /// </summary>
        /// <param name="entityID">Current Service Contract ID.</param>
        /// <param name="inventoryID">Inventory ID to be analyzed.</param>
        /// <returns>Returns true if it is necessary to Delete the <c>FSSalesPrice</c> record, otherwise false.</returns>
        public bool isThisFSSalesPriceNeedToBeDeleted(int? entityID, int? inventoryID, int? currentScheduleDetID, string lineType)
        {
            int numRowsOld = 0;
            if (lineType == ID.LineType_All.SERVICE ||
                    lineType == ID.LineType_All.COMMENT_SERVICE ||
                    lineType == ID.LineType_All.INSTRUCTION_SERVICE ||
                    lineType == ID.LineType_All.NONSTOCKITEM)
            {
                //1. Search for the same InventoryID existing in other services in the same contract.
                numRowsOld = ScheduleDetServicesByContract.Select(entityID, inventoryID, currentScheduleDetID).Count;

                //2. Search for the same InventoryID existing in other serviceTemplates in the same contract.
                numRowsOld += ScheduleDetServicesInServiceTemplatesByContract.Select(entityID, inventoryID, currentScheduleDetID).Count;
            }
            else
            {
                //1. Search for the same InventoryID existing in other services in the same contract.
                numRowsOld = ScheduleDetPartsByContract.Select(entityID, inventoryID, currentScheduleDetID).Count;
            }

            //Delete record in case the service (InventoryID) is not included in other services and service templates (numRowsOld == 0)
            return numRowsOld == 0;
        }

        [Obsolete("Remove in 2020R1")]
        private bool? isLineService(FSScheduleDet fsScheduleDetRow)
        {
            if (fsScheduleDetRow.LineType == ID.LineType_All.SERVICE
                    || fsScheduleDetRow.LineType == ID.LineType_All.NONSTOCKITEM
                        || fsScheduleDetRow.LineType == ID.LineType_All.SERVICE_TEMPLATE)
            {
                return true;
            }
            else if (fsScheduleDetRow.LineType == ID.LineType_All.INVENTORY_ITEM)
            {
                return false;
            }

            return null;
        }
        
        [Obsolete("Remove in 2020")]
        public void CommitDBOperation(PXCache cache, FSSalesPrice fsSalesPriceRow, PXDBOperation operation, bool? isService)
        {
            if (isService == true)
            {
                CommitDBOperation(cache, fsSalesPriceRow, operation, SalesPriceServices.View);
            }
            else
            {
                CommitDBOperation(cache, fsSalesPriceRow, operation, SalesPriceInventoryItems.View);
            }
        }

        [Obsolete("Remove in 2020")]
        public void CommitDBOperation(PXCache cache, FSSalesPrice fsSalesPriceRow, PXDBOperation operation, PXView view)
        {
            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    switch (operation)
                    {
                        case PXDBOperation.Insert:
                            view.Cache.Insert(fsSalesPriceRow);
                            break;
                        case PXDBOperation.Update:
                            view.Cache.Update(fsSalesPriceRow);
                            break;
                        case PXDBOperation.Delete:
                            view.Cache.Delete(fsSalesPriceRow);
                            break;
                    }

                    view.Cache.Persist(operation);
                    ts.Complete();
                }

                view.Cache.Persisted(false);
            }
            catch
            {
                view.Cache.Persisted(true);
                throw;
            }
        }

        public string GetLineType(InventoryItem inventoryItemRow, string lineTypeFromSchedule)
        {
            if (lineTypeFromSchedule == ID.LineType_ServiceContract.SERVICE_TEMPLATE)
            {
                if (inventoryItemRow.StkItem == false)
                {
                    if (inventoryItemRow.ItemType == INItemTypes.ServiceItem)
                    {
                        return ID.LineType_ServiceContract.SERVICE;
                    }
                    else
                    {
                        return ID.LineType_ServiceContract.NONSTOCKITEM;
                    }
                }
                else
                {
                    return ID.LineType_ServiceContract.INVENTORY_ITEM;
                }
            }

            return lineTypeFromSchedule;
        }

        public void InsertDeleteFSSalesPrice(PXCache cache, int? serviceContractID, PXDBOperation operation)
        {
            TempGraph.Clear(PXClearOption.ClearQueriesOnly);

            if (operation == PXDBOperation.Insert || operation == PXDBOperation.Update)
            {
                FSServiceContract fsServiceContractRow = PXSelect<FSServiceContract,
                                                            Where<FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                         .Select(TempGraph, serviceContractID);

                if (fsServiceContractRow != null)
                {
                    //Check missing FSSalesPrice records
                    var list = PXSelectJoin<FSSchedule,
                                            LeftJoin<FSScheduleDet,
                                                On<FSScheduleDet.scheduleID, Equal<FSSchedule.scheduleID>>,
                                            LeftJoin<FSServiceTemplateDet,
                                                On<FSServiceTemplateDet.serviceTemplateID, Equal<FSScheduleDet.serviceTemplateID>>,
                                            LeftJoin<FSSalesPrice,
                                                On<FSSalesPrice.serviceContractID, Equal<FSSchedule.entityID>,
                                                        And<
                                                            Where2<
                                                                 Where<FSSalesPrice.inventoryID, Equal<FSScheduleDet.inventoryID>,
                                                                    And<FSScheduleDet.inventoryID, IsNotNull>>,
                                                                 Or<Where<FSSalesPrice.inventoryID, Equal<FSServiceTemplateDet.inventoryID>,
                                                                    And<FSScheduleDet.serviceTemplateID, IsNotNull>>>>>>>>>,
                                            Where<FSSalesPrice.salesPriceID, IsNull,
                                                And2<
                                                     Where<FSScheduleDet.inventoryID, IsNotNull,
                                                        Or<FSScheduleDet.serviceTemplateID, IsNotNull>>,
                                                And<FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>>>>>
                                            .Select(TempGraph, fsServiceContractRow.ServiceContractID);

                    bool runPersist = false;

                    foreach (PXResult<FSSchedule, FSScheduleDet, FSServiceTemplateDet, FSSalesPrice> result in list)
                    {
                        FSSchedule fsScheduleRow = (FSSchedule)result;
                        FSScheduleDet fsScheduleDetRow = (FSScheduleDet)result;
                        FSServiceTemplateDet fsServiceTemplateRow = (FSServiceTemplateDet)result;

                        int? inventoryID = fsScheduleDetRow.LineType == ID.LineType_All.SERVICE_TEMPLATE ? fsServiceTemplateRow.InventoryID : fsScheduleDetRow.InventoryID;
                        InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(TempGraph, inventoryID);

                        if (inventoryItemRow != null)
                        {
                            FSSalesPrice fsSalesPriceRow = new FSSalesPrice();
                            fsSalesPriceRow.ServiceContractID = fsServiceContractRow.ServiceContractID;
                            fsSalesPriceRow.InventoryID = inventoryItemRow.InventoryID;
                            fsSalesPriceRow.LineType = GetLineType(inventoryItemRow, fsScheduleDetRow.LineType);
                            fsSalesPriceRow.CuryID = Accessinfo.BaseCuryID;
                            fsSalesPriceRow.UOM = inventoryItemRow.BaseUnit;

                            if (fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                            {
                                SalesPriceSet salesPriceSet = FSPriceManagement.CalculateSalesPriceWithCustomerContract(
                                                                                    cache,
                                                                                    null,
                                                                                    null,
                                                                                    null,
                                                                                    fsServiceContractRow.CustomerID,
                                                                                    fsServiceContractRow.CustomerLocationID,
                                                                                    null,
                                                                                    fsSalesPriceRow.InventoryID,
                                                                                    null,
                                                                                    0m,
                                                                                    fsSalesPriceRow.UOM,
                                                                                    (DateTime)(fsServiceContractRow.StartDate ?? cache.Graph.Accessinfo.BusinessDate),
                                                                                    fsSalesPriceRow.UnitPrice,
                                                                                    alwaysFromBaseCurrency: true,
                                                                                    currencyInfo: null,
                                                                                    catchSalesPriceException: true);

                                fsSalesPriceRow.UnitPrice = salesPriceSet.Price;
                            }

                            runPersist = true;
                            salesPriceRecords.Cache.Insert(fsSalesPriceRow);
                        }
                    }

                    if (runPersist == true)
                    {
                        salesPriceRecords.Cache.Persist(PXDBOperation.Insert);
                    }
                }
            }

            if (operation == PXDBOperation.Delete || operation == PXDBOperation.Update)
            {
                //Check leftovers FSSalesPrice records
                var existingInventoryList = PXSelectJoin<FSServiceContract,
                                        LeftJoin<FSSchedule,
                                            On<FSSchedule.entityID, Equal<FSServiceContract.serviceContractID>>,
                                        LeftJoin<FSScheduleDet,
                                            On<FSScheduleDet.scheduleID, Equal<FSSchedule.scheduleID>>,
                                        LeftJoin<FSServiceTemplateDet,
                                            On<FSServiceTemplateDet.serviceTemplateID, Equal<FSScheduleDet.serviceTemplateID>>>>>,
                                        Where<FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                        .Select(TempGraph, serviceContractID);

                var existingSalesPrice = PXSelect<FSSalesPrice,
                                                Where<FSSalesPrice.serviceContractID, Equal<Required<FSSalesPrice.serviceContractID>>>>
                                        .Select(TempGraph, serviceContractID).RowCast<FSSalesPrice>().ToList();

                bool runPersist = false;

                foreach (PXResult<FSServiceContract, FSSchedule, FSScheduleDet, FSServiceTemplateDet> existingInventory in existingInventoryList)
                {
                    FSSchedule fsScheduleRow = (FSSchedule)existingInventory;
                    FSScheduleDet fsSCheduleDetRow = (FSScheduleDet)existingInventory;
                    FSServiceTemplateDet fsServiceTemplateDetRow = (FSServiceTemplateDet)existingInventory;
                    int? inventoryID = fsSCheduleDetRow.LineType == ID.LineType_All.SERVICE_TEMPLATE ? fsServiceTemplateDetRow.InventoryID : fsSCheduleDetRow.InventoryID;

                    if (inventoryID != null)
                    {
                        FSSalesPrice existingSalesPriceRow = existingSalesPrice.Where(_ => ((FSSalesPrice)_).InventoryID == inventoryID).FirstOrDefault();
                        existingSalesPrice.Remove(existingSalesPriceRow);
                    }
                }

                foreach (FSSalesPrice fsSalesPriceRow in existingSalesPrice)
                {
                    runPersist = true;
                    salesPriceRecords.Cache.Delete(fsSalesPriceRow);
                }

                if (runPersist == true)
                {
                    salesPriceRecords.Cache.Persist(PXDBOperation.Delete);
                }
            }
        }

        [Obsolete("Remove in 2020R1")]
        /// <summary>
        /// Manages the Insert/Update/Delete events for <c>FSSalesPrice</c> depending on same events for <c>FSScheduleServiceDet</c>.
        /// </summary>
        public void InsertUpdateDeleteFSSalesPrice(PXCache cache, FSScheduleDet fsScheduleDetRow, int? entityID, PXDBOperation operation, PXTranStatus transaction)
        {
            if (transaction != PXTranStatus.Open)
            {
                return;
            }

            bool? isService = isLineService(fsScheduleDetRow);

            if (isService != null)
            {
                Company companyRow = PXSelect<Company>.Select(cache.Graph);
                FSServiceContract serviceContractRow = PXSelect<FSServiceContract,
                                                       Where<
                                                            FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                       .Select(cache.Graph, entityID);

                switch (operation)
                {
                    case PXDBOperation.Insert:

                        PXResultset<InventoryItem> bqlResultSet_InventoryItemsFromSchedule = GetInventoryItemListFromScheduleDet(cache, fsScheduleDetRow);

                        foreach (InventoryItem inventoryItemRow in bqlResultSet_InventoryItemsFromSchedule)
                        {
                            if (IsThisSalesPricePersisted(cache, entityID, inventoryItemRow.InventoryID) == false)
                            {
                                FSSalesPrice fsSalesPriceRow = new FSSalesPrice();

                                fsSalesPriceRow.ServiceContractID = entityID;
                                fsSalesPriceRow.InventoryID = inventoryItemRow.InventoryID;
                                fsSalesPriceRow.LineType = GetLineType(inventoryItemRow, fsScheduleDetRow.LineType);
                                fsSalesPriceRow.CuryID = companyRow.BaseCuryID;
                                fsSalesPriceRow.UOM = inventoryItemRow.BaseUnit;

                                fsSalesPriceRow.UnitPrice = null;

                                if (serviceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                                {
                                    SalesPriceSet salesPriceSet = FSPriceManagement.CalculateSalesPriceWithCustomerContract(
                                                                                cache,
                                                                                null,
                                                                                null,
                                                                                null,
                                                                                serviceContractRow.CustomerID,
                                                                                serviceContractRow.CustomerLocationID,
                                                                                null,
                                                                                fsSalesPriceRow.InventoryID,
                                                                                null,
                                                                                0m,
                                                                                fsSalesPriceRow.UOM,
                                                                                (DateTime)(serviceContractRow.StartDate ?? cache.Graph.Accessinfo.BusinessDate),
                                                                                fsSalesPriceRow.UnitPrice,
                                                                                alwaysFromBaseCurrency: true,
                                                                                currencyInfo: null,
                                                                                catchSalesPriceException: true);

                                    fsSalesPriceRow.UnitPrice = salesPriceSet.Price;
                                }

                                CommitDBOperation(cache, fsSalesPriceRow, PXDBOperation.Insert, isService);
                            }
                        }
                        
                        break;

                    case PXDBOperation.Update:
                      
                        PXResultset<InventoryItem> bqlResultSet_PreviousInventoryItemList = GetInventoryItemListFromScheduleDet(cache, fsScheduleDetRow, true);
                        bool updateServiceItems = serviceContractRow?.SourcePrice == ID.SourcePrice.PRICE_LIST && bqlResultSet_PreviousInventoryItemList.Count > 0;

                        if (updateServiceItems)
                        {
                            PXResultset<InventoryItem> bqlResultSet_UpdateInventoryItemList = GetInventoryItemListFromScheduleDet(cache, fsScheduleDetRow);

                            //Managing existing Records in FSSalesPrice
                            foreach (InventoryItem inventoryItemRow in bqlResultSet_PreviousInventoryItemList)
                            {
                                if (isThisFSSalesPriceNeedToBeDeleted(entityID, inventoryItemRow.InventoryID, fsScheduleDetRow.ScheduleDetID, fsScheduleDetRow.LineType) == true)
                                {
                                    FSSalesPrice fsSalesPriceRow_Delete = isService == true ? SalesPriceServices.SelectSingle(entityID, inventoryItemRow.InventoryID) : SalesPriceInventoryItems.SelectSingle(entityID, inventoryItemRow.InventoryID);
                                    CommitDBOperation(cache, fsSalesPriceRow_Delete, PXDBOperation.Delete, isService);
                                }
                            }

                            //Inserting non-existing Records in FSSalesPrice
                            foreach (InventoryItem inventoryItemRow in bqlResultSet_UpdateInventoryItemList)
                            {
                                if (IsThisSalesPricePersisted(cache, entityID, inventoryItemRow.InventoryID) == false)
                                {
                                    FSSalesPrice fsSalesPriceRow = new FSSalesPrice();

                                    fsSalesPriceRow.ServiceContractID = entityID;
                                    fsSalesPriceRow.InventoryID = inventoryItemRow.InventoryID;
                                    fsSalesPriceRow.LineType = GetLineType(inventoryItemRow, fsScheduleDetRow.LineType);
                                    fsSalesPriceRow.CuryID = companyRow.BaseCuryID;
                                    fsSalesPriceRow.UOM = inventoryItemRow.BaseUnit;

                                    fsSalesPriceRow.UnitPrice = null;

                                    if (serviceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                                    {
                                        SalesPriceSet salesPriceSet = FSPriceManagement.CalculateSalesPriceWithCustomerContract(
                                                                                cache,
                                                                                null,
                                                                                null,
                                                                                null,
                                                                                serviceContractRow.CustomerID,
                                                                                serviceContractRow.CustomerLocationID,
                                                                                null,
                                                                                fsSalesPriceRow.InventoryID,
                                                                                null,
                                                                                0m,
                                                                                fsSalesPriceRow.UOM,
                                                                                (DateTime)(serviceContractRow.StartDate ?? cache.Graph.Accessinfo.BusinessDate),
                                                                                fsSalesPriceRow.UnitPrice,
                                                                                alwaysFromBaseCurrency: true,
                                                                                currencyInfo: null,
                                                                                catchSalesPriceException: true);

                                        fsSalesPriceRow.UnitPrice = salesPriceSet.Price;
                                    }

                                    CommitDBOperation(cache, fsSalesPriceRow, PXDBOperation.Insert, isService);
                                }
                            }
                        }

                        break;

                    case PXDBOperation.Delete:

                        PXResultset<InventoryItem> bqlResultSet_DeleteInventoryItemList = GetInventoryItemListFromScheduleDet(cache, fsScheduleDetRow);

                        foreach (InventoryItem inventoryItemRow in bqlResultSet_DeleteInventoryItemList)
                        {
                            if (isThisFSSalesPriceNeedToBeDeleted(entityID, inventoryItemRow.InventoryID, fsScheduleDetRow.ScheduleDetID, fsScheduleDetRow.LineType) == true)
                            {
                                FSSalesPrice fsSalesPriceRow_Old = isService == true ? SalesPriceServices.SelectSingle(entityID, inventoryItemRow.InventoryID) : SalesPriceInventoryItems.SelectSingle(entityID, inventoryItemRow.InventoryID);
                                CommitDBOperation(cache, fsSalesPriceRow_Old, PXDBOperation.Delete, isService);
                            }
                        }

                        break;

                    default: break;
                }
            }
        }

        /// <summary>
        /// Shows/Hides Season settings depending on the setup's flag EnableSeasonScheduleContract and on the frequencyType selected.
        /// </summary>
        public void ShowHideSeasonSetting(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool showSeasonSettings = ShowSeasonSettings(fsScheduleRow);

            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnJan>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnFeb>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnMar>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnApr>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnMay>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnJun>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnJul>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnAug>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnSep>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnOct>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnNov>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnDec>(cache, fsScheduleRow, showSeasonSettings);
        }

        private bool ShowSeasonSettings(FSSchedule fsScheduleRow)
        {
            bool showSeasonSettings = SharedFunctions.GetEnableSeasonSetting(this, fsScheduleRow, Setup.Current);
            
            showSeasonSettings = showSeasonSettings && fsScheduleRow.FrequencyType != ID.Schedule_FrequencyType.ANNUAL;

            return showSeasonSettings;
        }

        /// <summary>
        /// Manage common actions for FSRouteContractSchedule and FSContractSchedule in RowSelected event.
        /// </summary>
        public void ContractSchedule_RowSelected_PartialHandler(PXCache cache, FSSchedule fsScheduleRow)
        {
            SetControlsState(cache, fsScheduleRow);

            PXUIFieldAttribute.SetEnabled<FSSchedule.runLimit>(cache, fsScheduleRow, fsScheduleRow.NoRunLimit == false);
            PXDefaultAttribute.SetPersistingCheck<FSSchedule.runLimit>(cache, fsScheduleRow, (fsScheduleRow.NoRunLimit == true) ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

            PXUIFieldAttribute.SetEnabled<FSSchedule.restrictionMinTime>(cache, fsScheduleRow, fsScheduleRow.RestrictionMin == true);
            PXDefaultAttribute.SetPersistingCheck<FSSchedule.restrictionMinTime>(cache, fsScheduleRow, (fsScheduleRow.RestrictionMin == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSSchedule.restrictionMaxTime>(cache, fsScheduleRow, fsScheduleRow.RestrictionMax == true);
            PXDefaultAttribute.SetPersistingCheck<FSSchedule.restrictionMaxTime>(cache, fsScheduleRow, (fsScheduleRow.RestrictionMax == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthly3Selected>(cache, fsScheduleRow, fsScheduleRow.Monthly2Selected == true);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthly4Selected>(cache, fsScheduleRow, fsScheduleRow.Monthly3Selected == true);

            fsScheduleRow.SrvOrdTypeMessage = TX.Messages.SERVICE_ORDER_TYPE_USED_FOR_RECURRING_APPOINTMENTS;

            SharedFunctions.SetRecurrenceDescription(fsScheduleRow);

            EnableDisableScheduleDetPartsTab(fsScheduleRow.SrvOrdType, ScheduleDetParts.Cache);
            ShowHideSeasonSetting(cache, fsScheduleRow);
        }

        /// <summary>
        /// Manage common actions for FSRouteContractSchedule and FSContractSchedule in RowPersisting event.
        /// </summary>
        public void ContractSchedule_RowPersisting_PartialHandler(PXCache cache, FSServiceContract fsServiceContractRow, FSSchedule fsScheduleRow, PXDBOperation operation, string moduleName)
        {
            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.WEEKLY
                    && !HasSelectedAtLeastOneDay(fsScheduleRow))
            {
                cache.RaiseExceptionHandling<FSRouteContractSchedule.weeklyFrequency>(
                    fsScheduleRow,
                    fsScheduleRow.AnnualFrequency,
                    new PXSetPropertyException(TX.Error.SELECT_AT_LEAST_ONE_DAY_OF_WEEK));
                return;
            }

            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.ANNUAL
                    && !HasSelectedAtLeastOneMonth(fsScheduleRow))
            {
                cache.RaiseExceptionHandling<FSSchedule.annualFrequency>(
                    fsScheduleRow,
                    fsScheduleRow.AnnualFrequency,
                    new PXSetPropertyException(TX.Error.SELECT_AT_LEAST_ONE_MONTH));
                return;
            }

            if ((fsScheduleRow.NoRunLimit == null || !(bool)fsScheduleRow.NoRunLimit) && fsScheduleRow.RunLimit < 0)
            {
                cache.RaiseExceptionHandling<FSSchedule.runLimit>(
                    fsScheduleRow,
                    fsScheduleRow.RunLimit,
                    new PXSetPropertyException(TX.Error.CONTRACT_INCORRECT_EXECUTION_LIMIT));
                return;
            }

            if (fsScheduleRow.RestrictionMin == true && fsScheduleRow.RestrictionMinTime == null)
            {
                cache.RaiseExceptionHandling<FSSchedule.restrictionMinTime>(
                    fsScheduleRow,
                    fsScheduleRow.RestrictionMinTime,
                    new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(
                                                                        TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                                                        PXUIFieldAttribute.GetDisplayName<FSSchedule.restrictionMinTime>(cache))));
                return;
            }

            if (fsScheduleRow.RestrictionMax == true && fsScheduleRow.RestrictionMaxTime == null)
            {
                cache.RaiseExceptionHandling<FSSchedule.restrictionMaxTime>(
                    fsScheduleRow,
                    fsScheduleRow.RestrictionMaxTime,
                    new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(
                                                                        TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                                                        PXUIFieldAttribute.GetDisplayName<FSSchedule.restrictionMaxTime>(cache))));
                return;
            }

            if (fsScheduleRow.StartDate > fsScheduleRow.EndDate)
            {
                cache.RaiseExceptionHandling<FSSchedule.startDate>(
                    fsScheduleRow,
                    fsScheduleRow.StartDate,
                    new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE));

                cache.RaiseExceptionHandling<FSSchedule.endDate>(
                    fsScheduleRow,
                    fsScheduleRow.EndDate,
                    new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE));
                return;
            }

            if (fsServiceContractRow != null 
                    && fsServiceContractRow.StartDate > fsScheduleRow.StartDate)
            {
                cache.RaiseExceptionHandling<FSSchedule.startDate>(
                    fsScheduleRow,
                    fsScheduleRow.StartDate,
                    new PXSetPropertyException(TX.Error.SCHEDULE_DATE_LESSER_THAN_CONTRACT_DATE));
            }

            if (fsServiceContractRow != null
                    && fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING
                        && fsServiceContractRow.EndDate != null
                            && fsServiceContractRow.EndDate < fsScheduleRow.StartDate)
            {
                cache.RaiseExceptionHandling<FSSchedule.startDate>(
                    fsScheduleRow,
                    fsScheduleRow.StartDate,
                    new PXSetPropertyException(TX.Error.SCHEDULE_START_DATE_GREATER_THAN_CONTRACT_END_DATE));
            }

            if (fsServiceContractRow != null 
                    && fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING 
                        && fsServiceContractRow.EndDate != null  
                            && (fsServiceContractRow.EndDate == null 
                                ||fsServiceContractRow.EndDate < fsScheduleRow.EndDate))
            {
                cache.RaiseExceptionHandling<FSSchedule.endDate>(
                    fsScheduleRow,
                    fsScheduleRow.EndDate,
                    new PXSetPropertyException(TX.Error.SCHEDULE_END_DATE_GREATER_THAN_CONTRACT_END_DATE));
            }

            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.DAILY && fsScheduleRow.DailyFrequency <= 0)
            {
                cache.RaiseExceptionHandling<FSSchedule.dailyFrequency>(
                    fsScheduleRow,
                    fsScheduleRow.DailyFrequency,
                    new PXSetPropertyException(TX.Error.CONTRACT_INCORRECT_DAILY_FREQUENCY));
                return;
            }

            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.WEEKLY && fsScheduleRow.WeeklyFrequency <= 0)
            {
                cache.RaiseExceptionHandling<FSSchedule.weeklyFrequency>(
                    fsScheduleRow,
                    fsScheduleRow.WeeklyFrequency,
                    new PXSetPropertyException(TX.Error.CONTRACT_INCORRECT_WEEKLY_FREQUENCY));
                return;
            }

            if (operation == PXDBOperation.Insert || operation == PXDBOperation.Update)
            {
                fsScheduleRow.NextExecutionDate = SharedFunctions.GetNextExecution(cache, fsScheduleRow);
            }

            if (operation == PXDBOperation.Insert)
            {
                if (fsScheduleRow.ScheduleID > 0)
                {
                    ContractScheduleRecords.Ask(
                                                moduleName,
                                                TX.Warning.SCHEDULE_WILL_NOT_AFFECT_SYSTEM_UNTIL_GENERATION_OCCURS,
                                                MessageButtons.OK,
                                                MessageIcon.Warning);
                }
            }
        }

        public void InsertContractAction(FSSchedule fsScheduleRow, PXDBOperation operation)
        {
            if (operation == PXDBOperation.Update && this.statusChanged == false)
            {
                return;
            }

            FSContractAction fsContractActionRow = new FSContractAction();

            fsContractActionRow.Type = ID.RecordType_ContractAction.SCHEDULE;
            fsContractActionRow.ServiceContractID = fsScheduleRow.EntityID;
            fsContractActionRow.ScheduleRefNbr = fsScheduleRow.RefNbr;
            fsContractActionRow.ActionBusinessDate = Accessinfo.BusinessDate;
            fsContractActionRow.ScheduleRecurrenceDescr = fsScheduleRow.RecurrenceDescription;

            if (operation == PXDBOperation.Insert)
            {
                fsContractActionRow.Action = ID.Action_ContractAction.CREATE;
            }
            else if (operation == PXDBOperation.Update)
            {
                fsContractActionRow.Action = fsScheduleRow.Active == true ? ID.Action_ContractAction.ACTIVATE : ID.Action_ContractAction.INACTIVATE_SCHEDULE;
            }
            else if (operation == PXDBOperation.Delete)
            {
                fsContractActionRow.Action = ID.Action_ContractAction.DELETE_SCHEDULE;
            }

            ContractHistory.Cache.Insert(fsContractActionRow);
            ContractHistory.Cache.Persist(PXDBOperation.Insert);
        }
        #endregion

        #region Public Static Functions

        /// <summary>
        /// Makes visible the group that corresponds to the selected FrequencyType.
        /// </summary>
        public static void SetControlsState(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool isWeekly           = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.WEEKLY;
            bool isDaily            = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.DAILY;
            bool isMonthly          = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.MONTHLY;
            bool isAnnually         = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.ANNUAL;
            bool isMonthly2Selected = fsScheduleRow.Monthly2Selected  == true;
            bool isMonthly3Selected = fsScheduleRow.Monthly3Selected  == true;
            bool isMonthly4Selected = fsScheduleRow.Monthly4Selected  == true;

            //Daily Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.dailyFrequency>(cache, fsScheduleRow, isDaily);
            PXUIFieldAttribute.SetVisible<FSSchedule.days>(cache, fsScheduleRow, isDaily);

            //Weekly Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyFrequency>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnSun>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnMon>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnTue>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnWed>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnThu>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnFri>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnSat>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeks>(cache, fsScheduleRow, isWeekly);

            ////Monthly Frequency
            SetMonthlyControlsState(cache, fsScheduleRow);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyFrequency>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.months>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly2Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly3Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly4Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType1>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);

            ////Annual Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.annualFrequency>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.years>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualRecurrenceType>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnDay>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnDayOfWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnDay>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnDayOfWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);

            //?????
            //PXUIFieldAttribute.SetEnabled<FSSchedule.runLimit>(cache, fsScheduleRow, !(bool)fsScheduleRow.NoRunLimit);ffffff
        }

        /// <summary>
        /// Set exceptions in date fields.
        /// </summary>
        public static void SetDateFieldExceptions(PXCache cache, FSSchedule fsScheduleRow, Exception exception)
        {
            cache.RaiseExceptionHandling<FSRouteContractSchedule.startDate>(
                                                                            fsScheduleRow,
                                                                            fsScheduleRow.StartDate,
                                                                            exception);

            cache.RaiseExceptionHandling<FSRouteContractSchedule.endDate>(
                                                                        fsScheduleRow,
                                                                        fsScheduleRow.EndDate,
                                                                        exception);
        }

        public static void FSScheduleService_RowSelected_PartialHandler(PXCache cache, FSScheduleDet fsScheduleDetRow)
        {
            EnableDisable_LineType(cache, fsScheduleDetRow);
            SharedFunctions.UpdateEquipmentAction(cache, fsScheduleDetRow);
        }

        public static void FSScheduleService_RowPersisting_PartialHandler(PXCache cache, FSScheduleDet fsScheduleDetRow)
        {
            CheckLineByLineType(cache, fsScheduleDetRow);
        }

        /// <summary>
        /// Check if at least one month is selected.
        /// </summary>
        /// <param name="fsScheduleRow">Instance of the FSSchedule DAC.</param>
        /// <returns>True if at least one month is selected.</returns>
        public static bool HasSelectedAtLeastOneMonth(FSSchedule fsScheduleRow)
        {
            return fsScheduleRow.AnnualOnJan == true || fsScheduleRow.AnnualOnFeb == true || fsScheduleRow.AnnualOnMar == true
                        || fsScheduleRow.AnnualOnApr == true || fsScheduleRow.AnnualOnMay == true || fsScheduleRow.AnnualOnJun == true
                            || fsScheduleRow.AnnualOnJul == true || fsScheduleRow.AnnualOnAug == true || fsScheduleRow.AnnualOnSep == true
                                || fsScheduleRow.AnnualOnOct == true || fsScheduleRow.AnnualOnNov == true || fsScheduleRow.AnnualOnDec == true;
        }

        /// <summary>
        /// Check if at least one day of the week is selected.
        /// </summary>
        /// <param name="fsScheduleRow">Instance of the FSSchedule DAC.</param>
        /// <returns>True if at least one day is selected.</returns>
        public static bool HasSelectedAtLeastOneDay(FSSchedule fsScheduleRow)
        {
            return fsScheduleRow.WeeklyOnSun == true || fsScheduleRow.WeeklyOnMon == true || fsScheduleRow.WeeklyOnTue == true
                        || fsScheduleRow.WeeklyOnWed == true || fsScheduleRow.WeeklyOnThu == true || fsScheduleRow.WeeklyOnFri == true
                            || fsScheduleRow.WeeklyOnSat == true;
        }

        public static void SetMonthlyControlsState(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool isMonthly = fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.MONTHLY;

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
        }

        public static void CheckLineByLineType(
                                            PXCache cache,
                                            FSScheduleDet fsScheduleDetRow,
                                            PXErrorLevel errorLevel = PXErrorLevel.Error)
        {
            switch (fsScheduleDetRow.LineType)
            {
                case ID.LineType_ServiceContract.SERVICE_TEMPLATE:

                    if (fsScheduleDetRow.InventoryID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(
                                                                                fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.SMEquipmentID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.SMequipmentID>(cache, fsScheduleDetRow, true);

                        //@TODO SD-7061
                        /*cache.RaiseExceptionHandling<FSScheduleDet.equipmentID>(fsScheduleDetRow, null,
                                                    new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));*/
                    }

                    if (fsScheduleDetRow.ServiceTemplateID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.serviceTemplateID>(
                                                                                    fsScheduleDetRow,
                                                                                    null,
                                                                                    new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(
                                                                            fsScheduleDetRow,
                                                                            null,
                                                                            new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;
                    
                case ID.LineType_ServiceContract.SERVICE:
                case ID.LineType_ServiceContract.NONSTOCKITEM:

                    if (fsScheduleDetRow.InventoryID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(
                                                                                fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.Qty == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.qty>(
                                                                        fsScheduleDetRow,
                                                                        null,
                                                                        new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.SMEquipmentID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.SMequipmentID>(cache, fsScheduleDetRow, true);
                        
                        //@TODO SD-7061
                        /*cache.RaiseExceptionHandling<FSScheduleDet.equipmentID>(fsScheduleDetRow, null,
                                                    new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));*/
                    }

                    if (fsScheduleDetRow.ServiceTemplateID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.serviceTemplateID>(
                                                                                    fsScheduleDetRow,
                                                                                    null,
                                                                                    new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(
                                                                            fsScheduleDetRow,
                                                                            null,
                                                                            new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceContract.INVENTORY_ITEM:

                    if (fsScheduleDetRow.InventoryID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(
                                                                                fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.Qty == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.qty>(
                                                                        fsScheduleDetRow,
                                                                        null,
                                                                        new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(
                                                                            fsScheduleDetRow,
                                                                            null,
                                                                            new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceContract.COMMENT_SERVICE:
                case ID.LineType_ServiceContract.INSTRUCTION_SERVICE:

                    if (fsScheduleDetRow.InventoryID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(
                                                                                fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.ServiceTemplateID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.serviceTemplateID>(
                                                                                    fsScheduleDetRow,
                                                                                    null,
                                                                                    new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(
                                                                            fsScheduleDetRow,
                                                                            null,
                                                                            new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceContract.COMMENT_PART:
                case ID.LineType_ServiceContract.INSTRUCTION_PART:

                    if (fsScheduleDetRow.InventoryID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(
                                                                                fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(
                                                                            fsScheduleDetRow,
                                                                            null,
                                                                            new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;
            }
        }

        /// <summary>
        /// This method enables or disables the fields on the grid depending on the selected LineType.
        /// </summary>
        public static void EnableDisable_LineType(PXCache cache, FSScheduleDet fsScheduleDetRow)
        {
            switch (fsScheduleDetRow.LineType)
            {
                case ID.LineType_ServiceContract.SERVICE_TEMPLATE:
                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.qty>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);
                    break;

                case ID.LineType_ServiceContract.SERVICE:
                case ID.LineType_ServiceContract.NONSTOCKITEM:
                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.qty>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.billingRule>(cache, fsScheduleDetRow, fsScheduleDetRow.LineType == ID.LineType_All.SERVICE);

                    break;

                case ID.LineType_ServiceContract.INVENTORY_ITEM:
                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.qty>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);
                    break;

                case ID.LineType_ServiceContract.COMMENT_SERVICE:
                case ID.LineType_ServiceContract.INSTRUCTION_SERVICE:
                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.qty>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.billingRule>(cache, fsScheduleDetRow, false);
                    break;
                case ID.LineType_ServiceContract.COMMENT_PART:
                case ID.LineType_ServiceContract.INSTRUCTION_PART:
                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.qty>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, PXPersistingCheck.Nothing);

                    PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, PXPersistingCheck.NullOrBlank);
                    break;
            }
        }

        /// <summary>
        /// This method reset the fields of the <c>fsScheduleDetRow</c> depending on the selected LineType.
        /// </summary>
        public static void ResetLineByType(FSScheduleDet fsScheduleDetRow, PXCache cache)
        {
            cache.SetValueExt<FSScheduleDet.tranDesc>(fsScheduleDetRow, string.Empty);
            fsScheduleDetRow.BillingRule = ID.BillingRule.NONE;

            switch (fsScheduleDetRow.LineType)
            {
                case ID.LineType_ServiceContract.NONSTOCKITEM:
                    fsScheduleDetRow.BillingRule = ID.BillingRule.FLAT_RATE;
                    break;
                case ID.LineType_ServiceContract.SERVICE_TEMPLATE:
                    fsScheduleDetRow.SMEquipmentID = null;
                    break;
            }

            fsScheduleDetRow.ServiceTemplateID = null;
            fsScheduleDetRow.InventoryID = null;
        }

        /// <summary>
        /// Enable/Disable the Parts Tab if the selected Service Order Type post to AR.
        /// </summary>
        public static void EnableDisableScheduleDetPartsTab(string srvOrdType, PXCache cache)
        {
            SvrOrdTypeMaint svrOrdTypeMaintGraph = PXGraph.CreateInstance<SvrOrdTypeMaint>();
            FSSrvOrdType fsSrvOrdTypeRow = svrOrdTypeMaintGraph.SvrOrdTypeRecords.Search<FSSrvOrdType.srvOrdType>(srvOrdType);

            if (fsSrvOrdTypeRow == null)
            {
                return;
            }

            bool enable = fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE;

            cache.AllowInsert = enable;
            cache.AllowUpdate = enable;
            cache.AllowDelete = enable;
        }

        
        #endregion

        #region Events

        #region ContractSchedule Events

        protected virtual void FSSchedule_Active_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;
            this.statusChanged = fsScheduleRow.Active != (bool)e.OldValue;
        }

        protected virtual void FSSchedule_Active_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
                return;

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            e.NewValue = fsScheduleRow.ScheduleGenType != ID.ScheduleGenType_ServiceContract.NONE ? true : false;
            e.Cancel = true;
        }

        protected virtual void FSSchedule_StartDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.EndDate != null &&
                    !SharedFunctions.IsValidDateRange(fsScheduleRow.StartDate, fsScheduleRow.EndDate))
            {
                SetDateFieldExceptions(cache, fsScheduleRow, new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE, PXErrorLevel.Error));
            }
            else
            {
                SetDateFieldExceptions(cache, fsScheduleRow, null);
            }
        }

        protected virtual void FSSchedule_EndDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.EndDate != null 
                    &&  !SharedFunctions.IsValidDateRange(fsScheduleRow.StartDate, fsScheduleRow.EndDate))
            {
                SetDateFieldExceptions(cache, fsScheduleRow, new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE, PXErrorLevel.Error));
            }
            else
            {
                SetDateFieldExceptions(cache, fsScheduleRow, null);
            }
        }

        protected virtual void FSSchedule_NoRunLimit_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;
            fsScheduleRow.RunLimit = (bool)fsScheduleRow.NoRunLimit ? (short)0 : (short)1;
        }

        protected virtual void FSSchedule_Monthly2Selected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.Monthly2Selected == false)
            {
                fsScheduleRow.Monthly3Selected = false;
                fsScheduleRow.Monthly4Selected = false;
            }
        }

        protected virtual void FSSchedule_Monthly3Selected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.Monthly3Selected == false)
            {
                fsScheduleRow.Monthly4Selected = false;
            }
        }

        protected virtual void FSSchedule_EntityID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;
            cache.SetDefaultExt<FSSchedule.scheduleGenType>(fsScheduleRow);
        }

        protected virtual void FSSchedule_RestrictionMinTime_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = Accessinfo.BusinessDate.Value.AddHours(8);
        }

        protected virtual void FSSchedule_RestrictionMaxTime_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = Accessinfo.BusinessDate.Value.AddHours(12);
        }

        protected virtual void FSSchedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (AttributeListRecords.Current == null && ContractScheduleRecords.Current != null && fsScheduleRow.ScheduleID < 0)
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
            }
            else if (fsScheduleRow.ScheduleID > 0 && AttributeListRecords.Current == null)
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
            }
            else if (fsScheduleRow.ScheduleID > 0 && AttributeListRecords.Current.ScheduleID != fsScheduleRow.ScheduleID)
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
            }
            else if (AttributeListRecords.Current != null 
                        && (AttributeListRecords.Current.SrvOrdType != fsScheduleRow.SrvOrdType
                            || AttributeListRecords.Current.ScheduleID != fsScheduleRow.ScheduleID))
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
            }

            PXUIFieldAttribute.SetEnabled<FSSchedule.active>(cache, fsScheduleRow, fsScheduleRow.ScheduleGenType != ID.ScheduleGenType_ServiceContract.NONE);
            PXUIFieldAttribute.SetEnabled<FSSchedule.scheduleGenType>(cache, fsScheduleRow, false);
        }

        protected virtual void FSSchedule_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            FSSchedule_Row_Deleted_PartialHandler(cache, e);
        }

        protected virtual void FSSchedule_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                InsertContractAction((FSSchedule)e.Row, e.Operation);
                if (e.Operation == PXDBOperation.Delete)
                {
                    FSSchedule fsScheduleRow = (FSSchedule)e.Row;
                    InsertDeleteFSSalesPrice(cache, fsScheduleRow.EntityID, e.Operation);
                }
            }
        }

        protected virtual void FSSchedule_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.ScheduleGenType == ID.ScheduleGenType_ServiceContract.NONE)
            {
                throw new PXException(TX.Error.SCHEDULE_TYPE_NONE);
            }
        }

        #endregion

        #region FSScheduleDet Events

        #region FSScheduleDetService Events
        protected virtual void FSScheduleDetService_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetService fsScheduleDetServiceRow = (FSScheduleDetService)e.Row;

            FSScheduleService_RowSelected_PartialHandler(cache, fsScheduleDetServiceRow);
        }

        protected virtual void FSScheduleDetService_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetService fsScheduleDetServiceRow = (FSScheduleDetService)e.Row;
            FSScheduleService_RowPersisting_PartialHandler(cache, fsScheduleDetServiceRow);
        }

        protected virtual void FSScheduleDetService_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null || CurrentServiceContract.Current == null)
            {
                return;
            }

            if(e.TranStatus == PXTranStatus.Open)
            {
                InsertDeleteFSSalesPrice(cache, CurrentServiceContract.Current.ServiceContractID, e.Operation);
            }
        }

        protected virtual void FSScheduleDetService_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetService fsScheduleDetServiceRow = (FSScheduleDetService)e.Row;
            FSScheduleService_InventoryID_FieldUpdated_Handler(cache, fsScheduleDetServiceRow);
        }

        protected virtual void FSScheduleDetService_LineType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetService fsScheduleDetServiceRow = (FSScheduleDetService)e.Row;
            ResetLineByType(fsScheduleDetServiceRow, cache);
        }

        protected virtual void FSScheduleDetService_EquipmentLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetService fsScheduleDetServiceRow = (FSScheduleDetService)e.Row;

            if (fsScheduleDetServiceRow.ComponentID == null)
            {
                fsScheduleDetServiceRow.ComponentID = SharedFunctions.GetEquipmentComponentID(this, fsScheduleDetServiceRow.SMEquipmentID, fsScheduleDetServiceRow.EquipmentLineRef);
            }
        }

        [Obsolete("Remove in 2020R1")]
        protected virtual void FSScheduleDetService_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {

        }
        #endregion

        #region FSScheduleDetPart Events
        protected virtual void FSScheduleDetPart_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetPart fsScheduleDetPartRow = (FSScheduleDetPart)e.Row;
            FSScheduleService_RowSelected_PartialHandler(cache, fsScheduleDetPartRow);
        }
        protected virtual void FSScheduleDetPart_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetPart fsScheduleDetPartRow = (FSScheduleDetPart)e.Row;

            using (new PXConnectionScope())
            {
                SharedFunctions.SetInventoryItemExtensionInfo(this, fsScheduleDetPartRow.InventoryID, fsScheduleDetPartRow);
            }
        }
        protected virtual void FSScheduleDetPart_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetPart fsScheduleDetPartRow = (FSScheduleDetPart)e.Row;
            FSScheduleService_RowPersisting_PartialHandler(cache, fsScheduleDetPartRow);
        }

        protected virtual void FSScheduleDetPart_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetPart fsScheduleDetPartRow = (FSScheduleDetPart)e.Row;
            FSSchedulePart_InventoryID_FieldUpdated_Handler(cache, fsScheduleDetPartRow);
        }

        protected virtual void FSScheduleDetPart_LineType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetPart fsScheduleDetPartRow = (FSScheduleDetPart)e.Row;
            ResetLineByType(fsScheduleDetPartRow, cache);
        }

        protected virtual void FSScheduleDetPart_EquipmentAction_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetPart fsScheduleDetPartRow = (FSScheduleDetPart)e.Row;

            SharedFunctions.ResetEquipmentFields(cache, fsScheduleDetPartRow);
            SharedFunctions.UpdateEquipmentAction(cache, fsScheduleDetPartRow);
        }

        protected virtual void FSScheduleDetPart_EquipmentLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDetPart fsScheduleDetPartRow = (FSScheduleDetPart)e.Row;

            if (fsScheduleDetPartRow.ComponentID == null)
            {
                fsScheduleDetPartRow.ComponentID = SharedFunctions.GetEquipmentComponentID(this, fsScheduleDetPartRow.SMEquipmentID, fsScheduleDetPartRow.EquipmentLineRef);
            }
        }

        protected virtual void FSScheduleDetPart_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null || CurrentServiceContract.Current == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                InsertDeleteFSSalesPrice(cache, CurrentServiceContract.Current.ServiceContractID, e.Operation);
            }
        }

        [Obsolete("Remove in 2020R1")]
        protected virtual void FSScheduleDetPart_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {

        }
        #endregion

        #endregion

        #endregion
    }
}
