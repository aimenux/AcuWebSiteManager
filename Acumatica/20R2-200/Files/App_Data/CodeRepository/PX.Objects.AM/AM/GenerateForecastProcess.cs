using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Generate forecast/wizard process graph
    /// </summary>
    public class GenerateForecastProcess : PXGraph<GenerateForecastProcess>
    {
        public PXSave<AMForecastStaging> Save;
        public PXFilter<ForecastSettings> Settings;
        [PXImport(typeof(AMForecastStaging))]
        public PXSelect<AMForecastStaging, Where<AMForecastStaging.userID, 
                Equal<Current<AccessInfo.userID>>>> SelectedRecs;

        [PXHidden]
        public PXSelect<AMForecast> ForecastRecs;

        public PXSetup<AMRPSetup> setup;
        [PXHidden]
        public PXSetup<Numbering>.Where<Numbering.numberingID.IsEqual<AMRPSetup.forecastNumberingID.FromCurrent>> Numbering;

        public bool IsForecastAutoNumbered
        {
            get
            {
                if (Numbering.Current == null)
                {
                    Numbering.Current = Numbering.Select();
                }

                return Numbering.Current != null && Numbering.Current.UserNumbering == false;
            }
        }

        public DateTime BusinessDate => Common.Current.BusinessDate(this);

        /// <summary>
        /// Indicates there are records in the processing grid
        /// </summary>
        public bool ContainsForecastStaging => SelectedRecs != null && SelectedRecs.Current != null && SelectedRecs.Cache.Cached.Any_();

        public bool IsStagingCalculation;

        public GenerateForecastProcess()
        {
            var mrpSetup = setup.Current;
            var processEnabled = mrpSetup != null && !string.IsNullOrWhiteSpace(mrpSetup.ForecastNumberingID) &&
                           Numbering.Current != null && !Numbering.Current.UserNumbering.GetValueOrDefault();

            processRecords.SetEnabled(processEnabled);
            processAllRecords.SetEnabled(processEnabled);

            PXUIFieldAttribute.SetVisible<ForecastSettings.type>(Settings.Cache, null, AM.InventoryHelper.FullReplenishmentsEnabled);
            PXUIFieldAttribute.SetVisible<ForecastSettings.seasonality>(Settings.Cache, null, AM.InventoryHelper.FullReplenishmentsEnabled);
        }

        public virtual void Process(AMForecastStaging forecastRec)
        {
            var forecastRecord = new AMForecast
            {
                ActiveFlg = true,
                BeginDate = forecastRec.BeginDate,
                EndDate = forecastRec.EndDate,
                InventoryID = forecastRec.InventoryID,
                SubItemID = forecastRec.SubItemID,
                SiteID = forecastRec.SiteID,
                Interval = ForecastInterval.OneTime,
                UOM = forecastRec.UOM,
                Qty = forecastRec.ForecastQty,
                NoteID = null,
                Dependent = forecastRec.Dependent,
                CustomerID = forecastRec.CustomerID
            };

            AMForecast existingRec;

            if (forecastRec.CustomerID == null)
            {
                // Check the AMForecast Table for existing record
                existingRec = PXSelect<AMForecast, Where<AMForecast.inventoryID, Equal<Required<AMForecast.inventoryID>>,
                    And<AMForecast.subItemID, Equal<Required<AMForecast.subItemID>>,
                        And<AMForecast.siteID, Equal<Required<AMForecast.siteID>>,
                            And<AMForecast.beginDate, Equal<Required<AMForecast.beginDate>>,
                                And<AMForecast.endDate, Equal<Required<AMForecast.endDate>>,
                                    And<AMForecast.uOM, Equal<Required<AMForecast.uOM>>,
                                        And<AMForecast.subItemID, IsNotNull
                                            >>>>>>>>.Select(this, forecastRec.InventoryID, forecastRec.SubItemID, forecastRec.SiteID,
                                                forecastRec.BeginDate, forecastRec.EndDate, forecastRec.UOM);
            }
            else
            {
                // Check the AMForecast Table for existing record
                existingRec = PXSelect<AMForecast, Where<AMForecast.inventoryID, Equal<Required<AMForecast.inventoryID>>,
                    And<AMForecast.subItemID, Equal<Required<AMForecast.subItemID>>,
                        And<AMForecast.siteID, Equal<Required<AMForecast.siteID>>,
                            And<AMForecast.beginDate, Equal<Required<AMForecast.beginDate>>,
                                And<AMForecast.endDate, Equal<Required<AMForecast.endDate>>,
                                    And<AMForecast.uOM, Equal<Required<AMForecast.uOM>>,
                                        And<AMForecast.customerID, Equal<Required<AMForecast.customerID>>,
                                            And<AMForecast.subItemID, IsNotNull
                                                >>>>>>>>>.Select(this, forecastRec.InventoryID, forecastRec.SubItemID, forecastRec.SiteID, 
                                                    forecastRec.BeginDate, forecastRec.EndDate, forecastRec.UOM, forecastRec.CustomerID);
            }

            if (existingRec != null)
            {
                existingRec.Qty = forecastRec.ForecastQty.GetValueOrDefault();
                ForecastRecs.Update(existingRec);
            }
            else
            {
                ForecastRecs.Insert(forecastRecord);
            }
        }

        protected virtual void AMForecast_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            // Temp key to allow multiple inserts before persisting. When persisting and auto number it will swap the value for us.
            var insertedCounter = cache.Inserted.Count() + 1;
            ((AMForecast)e.Row).ForecastID = $"-{insertedCounter}";
        }

        #region Process Button
        public PXAction<AMForecastStaging> processRecords;
        [PXUIField(DisplayName = PX.Objects.IN.Messages.Process)]
        [PXProcessButton]
        protected IEnumerable ProcessRecords(PXAdapter adapter)
        {
            Actions.PressSave();
            PXLongOperation.StartOperation(this, () =>
            {
                foreach (AMForecastStaging forecastRec in SelectedRecs.Cache.Cached)
                {
                    if (!forecastRec.Selected.GetValueOrDefault())
                    {
                        continue;
                    }

                    try
                    {
                        Process(forecastRec);
                    }
                    catch (Exception e)
                    {
                        if (e is PXOuterException)
                        {
                            PXTraceHelper.PxTraceOuterException((PXOuterException)e, PXTraceHelper.ErrorLevel.Error);
                        }

                        InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                .Select(this, forecastRec.InventoryID);

                        if (item == null)
                        {
                            throw;
                        }

                        throw new PXException(AM.Messages.GetLocal(AM.Messages.UnableToSaveRecordForInventoryID), AM.Messages.GetLocal(AM.Messages.Forecast), item.InventoryCD.Trim(), e.Message);
                    }
                    SelectedRecs.Delete(forecastRec);

                }
                if (SelectedRecs.Cache.Deleted.Any_())
                {
                    Actions.PressSave();
                }
            });
            return adapter.Get();
        }
        #endregion

        #region Process All Button
        public PXAction<AMForecastStaging> processAllRecords;
        [PXUIField(DisplayName = PX.Objects.IN.Messages.ProcessAll)]
        [PXProcessButton]
        protected IEnumerable ProcessAllRecords(PXAdapter adapter)
        {
            Actions.PressSave();
            PXLongOperation.StartOperation(this, () =>
            {
                foreach (AMForecastStaging forecastRec in SelectedRecs.Cache.Cached)
                {
                    try
                    {
                        Process(forecastRec);
                    }
                    catch (Exception e)
                    {
                        if (e is PXOuterException)
                        {
                            PXTraceHelper.PxTraceOuterException((PXOuterException)e, PXTraceHelper.ErrorLevel.Error);
                        }

                        InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                .Select(this, forecastRec.InventoryID);

                        if (item == null)
                        {
                            throw;
                        }

                        throw new PXException(AM.Messages.GetLocal(AM.Messages.UnableToSaveRecordForInventoryID), AM.Messages.GetLocal(AM.Messages.Forecast), item.InventoryCD.Trim(), e.Message);
                    }

                    SelectedRecs.Delete(forecastRec);
                }
                if (SelectedRecs.Cache.Deleted.Any_())
                {
                    Actions.PressSave();
                }
            });
            return adapter.Get();
        }
        #endregion

        #region Calculate Button
        public PXAction<AMForecastStaging> calculate;

        [PXUIField(DisplayName = PX.Objects.AM.Messages.Calculate)]
        [PXProcessButton]
        protected IEnumerable Calculate(PXAdapter adapter)
        {
            var confirmCalculateForecast = WebDialogResult.Yes;

            if (SelectedRecs.Current != null)
            {
                //ASK USER TO CONFIRM Generate Forecasts...
                confirmCalculateForecast = this.SelectedRecs.Ask(AM.Messages.GetLocal(AM.Messages.ConfirmProcess),
                    AM.Messages.GetLocal(AM.Messages.ConfirmCalculateMessage), MessageButtons.YesNo);
                if (confirmCalculateForecast == WebDialogResult.No)
                {
                    return adapter.Get();
                }
            }

            if (confirmCalculateForecast == WebDialogResult.Yes)
            {
                PXLongOperation.StartOperation(this, ProcessHistory);
            }
            return adapter.Get();
        }
    
        #endregion

        #region Process History
        public void ProcessHistory()
        {
            IsStagingCalculation = true;
            try
            {
                DeleteSessionData();

                var nbrOfYears = Settings.Current.Years.GetValueOrDefault();
                if (nbrOfYears <= 0)
                {
                    throw new PXArgumentException(nameof(nbrOfYears));
                }

                var inventoryId = Settings?.Current?.InventoryID;
                var subItemId = AM.InventoryHelper.SubItemFeatureEnabled ? Settings?.Current?.SubItemID : null;
                var siteId = AM.InventoryHelper.MultiWarehousesFeatureEnabled ? Settings?.Current?.SiteID : null;
                var itemClassId = Settings?.Current?.ItemClassID;
                var customerId = Settings?.Current?.BAccountID;
                var queryRestriction = new QueryRestriction(siteId != null, itemClassId != null, customerId != null, inventoryId != null, subItemId != null);

                // Get the Command to select relevant sales History by Customer as needed
                PXSelectBase<INTranAllSalesHistory> baseCmdTran;

                if (customerId != null || Settings.Current.ProcessByCustomer == true)
                {
                    baseCmdTran = GetAllSalesHistoryByCustomerCommand(queryRestriction);
                }
                else
                {
                    baseCmdTran = GetAllSalesHistoryCommand(queryRestriction);
                }

                PXResultset<INTranAllSalesHistory> resultset = GetAllSalesHistory(baseCmdTran, queryRestriction, inventoryId, subItemId, siteId, itemClassId, customerId);

                var maxExceptions = 10;
                var nbrOfExceptions = 0;

                foreach (var aggregate in resultset)
                {
                    var row = (INTranAllSalesHistory)aggregate;
                    if (row?.InventoryID == null)
                    {
                        continue;
                    }

                    var buckets = CreateBuckets(row);
                    if(buckets == null)
                    {
                        continue;
                    }

                    var qr = new QueryRestriction(AM.InventoryHelper.MultiWarehousesFeatureEnabled,
                        AM.InventoryHelper.SubItemFeatureEnabled, row.BAccountID != null);

                    var baseCmd = GetSalesHistoryCommand(qr);

                    try
                    {
                        ProcessBuckets(baseCmd, qr, buckets);
                    }
                    catch (Exception e)
                    {
                        PXTraceHelper.PxTraceException(e);

                        nbrOfExceptions++;
                        
                        var item = InventoryItem.PK.Find(this, row.InventoryID);
                        if (item?.InventoryCD != null)
                        {
                            PXTrace.WriteInformation(AM.Messages.GetLocal(AM.Messages.UnableToProcessForecastBucketsForItem, item.InventoryCD, e.Message));
                        }
                        
                        if (nbrOfExceptions >= maxExceptions)
                        {
                            break;
                        }
                    }
                }

                Actions.PressSave();

                if (nbrOfExceptions >= maxExceptions)
                {
                    throw new PXException(ErrorMessages.SeveralItemsFailed);
                }

                if (nbrOfExceptions > 0)
                {
                    throw new PXOperationCompletedException(ErrorMessages.SeveralItemsFailed);
                }
            }
            finally
            {
                IsStagingCalculation = false;
            }
        }
        #endregion

        protected virtual decimal GetSalesHistoryTotal(PXSelectBase<INTranSalesHistory> cmd, QueryRestriction queryRestriction, DateTime beginDate, DateTime endDate, Bucket bucket, int? bAccountId)
        {
            var qty = 0m;
            foreach (INTranSalesHistory row in GetSalesHistory(cmd, queryRestriction, beginDate, endDate, bucket, bAccountId))
            {
                if (row?.InventoryID == null)
                {
                    continue;
                }

                qty += Math.Abs(row.BaseQty.GetValueOrDefault()) * -1 * row.InvtMult.GetValueOrDefault();
            }
            return qty;
        }

        protected virtual PXResultset<INTranSalesHistory> GetSalesHistory(PXSelectBase<INTranSalesHistory> cmd, QueryRestriction queryRestriction, DateTime beginDate, DateTime endDate, Bucket bucket, int? bAccountId)
        {
            return GetSalesHistory(cmd, queryRestriction, beginDate, endDate, bucket?.InventoryID, bucket?.SiteID, bucket?.SubItemID, bAccountId);
        }

        protected virtual PXResultset<INTranSalesHistory> GetSalesHistory(PXSelectBase<INTranSalesHistory> cmd, QueryRestriction queryRestriction, DateTime beginDate, DateTime endDate, int? inventoryId, int? siteId, int? subItemId, int? bAccountId)
        {
            if (inventoryId == null)
            {
                throw new ArgumentNullException(nameof(inventoryId));
            }

            var parms = new List<object>
            {
                inventoryId,
                beginDate,
                endDate
            };

            if(queryRestriction.Warehouse)
            {
                if (siteId == null)
                {
                    throw new ArgumentNullException(nameof(siteId));
                }

                parms.Add(siteId);
            }

            if (queryRestriction.SubItem)
            {
                if (subItemId == null)
                {
                    throw new ArgumentNullException(nameof(subItemId));
                }

                parms.Add(subItemId);
            }

            if (queryRestriction.Customer)
            {
                if (bAccountId == null)
                {
                    throw new ArgumentNullException(nameof(bAccountId));
                }

                parms.Add(bAccountId);
            }

            return GetSalesHistory(cmd, parms);
        }

        protected virtual PXResultset<INTranSalesHistory> GetSalesHistory(PXSelectBase<INTranSalesHistory> cmd, List<object> parms)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            if ((parms?.Count ?? 0) == 0)
            {
                throw new ArgumentNullException(nameof(parms));
            }

            return cmd.Select(parms.ToArray());
        }

        protected virtual PXResultset<INTranAllSalesHistory> GetAllSalesHistory(PXSelectBase<INTranAllSalesHistory> cmdTran, QueryRestriction queryRestriction, 
            int? inventoryId, int? subItemId, int? siteId, int? itemClassId, int? bAccountId)
        {
            var forecastDate = Settings.Current.ForecastDate.GetValueOrDefault();
            var startDate = forecastDate.AddYears((int)Settings.Current.Years * -1);
            var endDate = forecastDate.AddDays(-1);

            var parms = new List<object>
            {
                startDate,
                endDate
            };

            if (queryRestriction.InventoryID)
            {
                if (inventoryId == null)
                {
                    throw new ArgumentNullException(nameof(inventoryId));
                }
                parms.Add(inventoryId);
            }

            if (queryRestriction.SubItem)
            {
                if (subItemId == null)
                {
                    throw new ArgumentNullException(nameof(subItemId));
                }

                parms.Add(subItemId);
            }

            if (queryRestriction.Warehouse)
            {
                if (siteId == null)
                {
                    throw new ArgumentNullException(nameof(siteId));
                }

                parms.Add(siteId);
            }

            if (queryRestriction.ItemClass)
            {
                if (itemClassId == null)
                {
                    throw new ArgumentNullException(nameof(itemClassId));
                }

                parms.Add(siteId);
            }

            if (queryRestriction.Customer)
            {
                if (bAccountId == null)
                {
                    throw new ArgumentNullException(nameof(bAccountId));
                }

                parms.Add(bAccountId);
            }

            return GetAllSalesHistory(cmdTran, parms);
        }

        protected virtual PXResultset<INTranAllSalesHistory> GetAllSalesHistory(PXSelectBase<INTranAllSalesHistory> cmdTran, List<object> parms)
        {
            if (cmdTran == null)
            {
                throw new ArgumentNullException(nameof(cmdTran));
            }

            if ((parms?.Count ?? 0) == 0)
            {
                throw new ArgumentNullException(nameof(parms));
            }

            return cmdTran.Select(parms.ToArray());
        }

        protected struct QueryRestriction
        {
            public bool InventoryID { get; private set; }
            public bool SubItem { get; private set; }
            public bool Warehouse { get; private set; }
            public bool ItemClass { get; private set; }
            public bool Customer { get; private set; }

            public QueryRestriction(bool byWarehouse, bool bySubItem, bool byCustomer)
                : this(byWarehouse, bySubItem, byCustomer, false, false)
            {
            }

            public QueryRestriction(bool byWarehouse, bool byItemClass, bool byCustomer, bool byInventoryID, bool bySubItem)
            {
                InventoryID = byInventoryID;
                SubItem = bySubItem;
                Warehouse = byWarehouse;
                ItemClass = byItemClass;
                Customer = byCustomer;
            }
        }

        protected virtual PXSelectBase<INTranSalesHistory> GetSalesHistoryCommand(QueryRestriction queryRestriction)
        {
            PXSelectBase<INTranSalesHistory> cmd = new PXSelect<INTranSalesHistory,
                Where<INTranSalesHistory.inventoryID, Equal<Required<INTranSalesHistory.inventoryID>>,
                    And<INTranSalesHistory.tranDate, Between<Required<INTran.tranDate>, Required<INTran.tranDate>>>>>(this);

            if (queryRestriction.Warehouse)
            {
                cmd.WhereAnd<Where<INTranSalesHistory.siteID, Equal<Required<INTranSalesHistory.siteID>>>>();
            }

            if (queryRestriction.SubItem)
            {
                cmd.WhereAnd<Where<INTranSalesHistory.subItemID, Equal<Required<INTranSalesHistory.subItemID>>>>();
            }

            if (queryRestriction.Customer)
            {
                cmd.WhereAnd<Where<INTranSalesHistory.bAccountID, Equal<Required<INTranSalesHistory.bAccountID>>>>();
            }

            return cmd;
        }

        protected virtual PXSelectBase<INTranAllSalesHistory> GetAllSalesHistoryCommand(QueryRestriction queryRestriction)
        {
            // Create Base select command for selecting sales history records
            PXSelectBase<INTranAllSalesHistory> cmdTran = new PXSelectGroupBy<INTranAllSalesHistory,
                Where<INTranAllSalesHistory.tranDate, Between<Required<INTran.tranDate>, Required<INTran.tranDate>>>,
                Aggregate<
                    GroupBy<INTranAllSalesHistory.inventoryID,
                    GroupBy<INTranAllSalesHistory.subItemID,
                    GroupBy<INTranAllSalesHistory.siteID,
                    Sum<INTranAllSalesHistory.tranAmt>>>
                >>>(this);

            #region INVENTORY ITEM filter
            if (queryRestriction.InventoryID)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.inventoryID, Equal<Current<ForecastSettings.inventoryID>>>>();
            }
            #endregion

            #region SubItem filter
            if (queryRestriction.SubItem)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.subItemID, Equal<Current<ForecastSettings.subItemID>>>>();
            }
            #endregion

            #region WAREHOUSE (site id) filter

            if (queryRestriction.Warehouse)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.siteID, Equal<Current<ForecastSettings.siteID>>>>();
            }

            #endregion

            #region ITEM CLASS filter

            if (queryRestriction.ItemClass)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.itemClassID, Equal<Current<ForecastSettings.itemClassID>>>>();
            }
            #endregion

            return cmdTran;
        }

        protected virtual PXSelectBase<INTranAllSalesHistory> GetAllSalesHistoryByCustomerCommand(QueryRestriction queryRestriction)
        {
            // Create Base select command for selecting sales history records Customer
            PXSelectBase<INTranAllSalesHistory> cmdTran = new PXSelectJoinGroupBy<INTranAllSalesHistory,
                InnerJoin<Customer, On<Customer.bAccountID, Equal<INTranAllSalesHistory.bAccountID>>>,
                Where<INTranAllSalesHistory.tranDate, Between<Required<INTran.tranDate>, Required<INTran.tranDate>>>,
                Aggregate<
                    GroupBy<INTranAllSalesHistory.bAccountID,
                    GroupBy<INTranAllSalesHistory.inventoryID,
                    GroupBy<INTranAllSalesHistory.subItemID,
                    GroupBy<INTranAllSalesHistory.siteID,
                    Sum<INTranAllSalesHistory.tranAmt>>>
                >>>>(this);

            #region INVENTORY ITEM filter
            if (queryRestriction.InventoryID)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.inventoryID, Equal<Current<ForecastSettings.inventoryID>>>>();
            }
            #endregion

            #region SubItem filter
            if (queryRestriction.SubItem)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.subItemID, Equal<Current<ForecastSettings.subItemID>>>>();
            }
            #endregion

            #region WAREHOUSE (site id) filter

            if (queryRestriction.Warehouse)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.siteID, Equal<Current<ForecastSettings.siteID>>>>();
            }

            #endregion

            #region ITEM CLASS filter

            if (queryRestriction.ItemClass)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.itemClassID, Equal<Current<ForecastSettings.itemClassID>>>>();
            }
            #endregion

            #region Customer filter
            if (queryRestriction.Customer)
            {
                cmdTran.WhereAnd<Where<INTranAllSalesHistory.bAccountID, Equal<Required<ForecastSettings.bAccountID>>>>();
            }
            #endregion

            return cmdTran;
        }

        #region Sales Projections
        [PXProjection(typeof(Select<
            INTran, 
            Where<INTran.tranType, Equal<INTranType.invoice>,
                Or<INTran.tranType, Equal<INTranType.creditMemo>,
                Or<INTran.tranType, Equal<INTranType.debitMemo>>>>>), Persistent = false)]
        [PXHidden]
        [Serializable]
        public class INTranSalesHistory : IBqlTable
        {
            #region BranchID
            public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
            protected Int32? _BranchID;
            [PXDBInt(BqlField = typeof(INTran.branchID))]
            [PXUIField(DisplayName = "Branch", FieldClass = "BRANCH")]
            public virtual Int32? BranchID
            {
                get
                {
                    return this._BranchID;
                }
                set
                {
                    this._BranchID = value;
                }
            }
            #endregion
            #region DocType
            public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
            protected String _DocType;
            [PXUIField(DisplayName = INRegister.docType.DisplayName)]
            [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(INTran.docType))]
            public virtual String DocType
            {
                get
                {
                    return this._DocType;
                }
                set
                {
                    this._DocType = value;
                }
            }
            #endregion
            #region TranType
            public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
            protected String _TranType;
            [PXDBString(3, IsFixed = true, BqlField = typeof(INTran.tranType))]
            [PXUIField(DisplayName = "Tran. Type")]
            public virtual String TranType
            {
                get
                {
                    return this._TranType;
                }
                set
                {
                    this._TranType = value;
                }
            }
            #endregion
            #region RefNbr
            public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
            protected String _RefNbr;
            [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTran.refNbr))]
            [PXUIField(DisplayName = INRegister.refNbr.DisplayName)]
            public virtual String RefNbr
            {
                get
                {
                    return this._RefNbr;
                }
                set
                {
                    this._RefNbr = value;
                }
            }
            #endregion
            #region LineNbr
            public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
            protected Int32? _LineNbr;
            [PXDBInt(IsKey = true, BqlField = typeof(INTran.lineNbr))]
            [PXUIField(DisplayName = "Line Number")]
            public virtual Int32? LineNbr
            {
                get
                {
                    return this._LineNbr;
                }
                set
                {
                    this._LineNbr = value;
                }
            }
            #endregion
            #region TranDate
            public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
            protected DateTime? _TranDate;
            [PXDBDate(BqlField = typeof(INTran.tranDate))]
            public virtual DateTime? TranDate
            {
                get
                {
                    return this._TranDate;
                }
                set
                {
                    this._TranDate = value;
                }
            }
            #endregion
            #region InvtMult
            public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
            protected Int16? _InvtMult;
            [PXDBShort(BqlField = typeof(INTran.invtMult))]
            [PXUIField(DisplayName = "Multiplier")]
            public virtual Int16? InvtMult
            {
                get
                {
                    return this._InvtMult;
                }
                set
                {
                    this._InvtMult = value;
                }
            }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
            protected Int32? _InventoryID;
            [PXDBInt(BqlField = typeof(INTran.inventoryID))]
            [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
            public virtual Int32? InventoryID
            {
                get
                {
                    return this._InventoryID;
                }
                set
                {
                    this._InventoryID = value;
                }
            }
            #endregion
            #region SubItemID
            public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
            protected Int32? _SubItemID;

            [PXDBInt(BqlField = typeof(INTran.subItemID))]
            [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
            public virtual Int32? SubItemID
            {
                get
                {
                    return this._SubItemID;
                }
                set
                {
                    this._SubItemID = value;
                }
            }
            #endregion
            #region SiteID
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
            protected Int32? _SiteID;
            [PXDBInt(BqlField = typeof(INTran.siteID))]
            [PXUIField(DisplayName = "Warehouse", FieldClass = "INSITE")]
            public virtual Int32? SiteID
            {
                get
                {
                    return this._SiteID;
                }
                set
                {
                    this._SiteID = value;
                }
            }
            #endregion
            #region BAccountID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            protected Int32? _BAccountID;
            [PXDBInt(BqlField = typeof(INTran.bAccountID))]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._BAccountID;
                }
                set
                {
                    this._BAccountID = value;
                }
            }
            #endregion
            #region BaseQty
            public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
            protected Decimal? _BaseQty;
            [PXDBQuantity(BqlField = typeof(INTran.baseQty))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? BaseQty
            {
                get
                {
                    return this._BaseQty;
                }
                set
                {
                    this._BaseQty = value;
                }
            }
            #endregion
        }

        [PXProjection(typeof(Select2<
            INTran,
            InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INTran.inventoryID>>,
            InnerJoin<INItemSite, On<INItemSite.inventoryID, Equal<INTran.inventoryID>,
                And<INItemSite.siteID, Equal<INTran.siteID>>>>>,
            Where2<Where<InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
                    Or<InventoryItem.itemStatus, Equal<InventoryItemStatus.noPurchases>>>,
                And<Where<INTran.tranType, Equal<INTranType.invoice>,
                    Or<INTran.tranType, Equal<INTranType.creditMemo>,
                    Or<INTran.tranType, Equal<INTranType.debitMemo>>>>>>>), Persistent = false)]
        [PXHidden]
        [Serializable]
        public class INTranAllSalesHistory : IBqlTable
        {
            #region BranchID
            public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
            protected Int32? _BranchID;
            [PXDBInt(BqlField = typeof(INTran.branchID))]
            [PXUIField(DisplayName = "Branch", FieldClass = "BRANCH")]
            public virtual Int32? BranchID
            {
                get
                {
                    return this._BranchID;
                }
                set
                {
                    this._BranchID = value;
                }
            }
            #endregion
            #region DocType
            public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
            protected String _DocType;
            [PXUIField(DisplayName = INRegister.docType.DisplayName)]
            [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(INTran.docType))]
            public virtual String DocType
            {
                get
                {
                    return this._DocType;
                }
                set
                {
                    this._DocType = value;
                }
            }
            #endregion
            #region TranType
            public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
            protected String _TranType;
            [PXDBString(3, IsFixed = true, BqlField = typeof(INTran.tranType))]
            [PXUIField(DisplayName = "Tran. Type")]
            public virtual String TranType
            {
                get
                {
                    return this._TranType;
                }
                set
                {
                    this._TranType = value;
                }
            }
            #endregion
            #region RefNbr
            public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
            protected String _RefNbr;
            [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTran.refNbr))]
            [PXUIField(DisplayName = INRegister.refNbr.DisplayName)]
            public virtual String RefNbr
            {
                get
                {
                    return this._RefNbr;
                }
                set
                {
                    this._RefNbr = value;
                }
            }
            #endregion
            #region LineNbr
            public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
            protected Int32? _LineNbr;
            [PXDBInt(IsKey = true, BqlField = typeof(INTran.lineNbr))]
            [PXUIField(DisplayName = "Line Number")]
            public virtual Int32? LineNbr
            {
                get
                {
                    return this._LineNbr;
                }
                set
                {
                    this._LineNbr = value;
                }
            }
            #endregion
            #region TranDate
            public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
            protected DateTime? _TranDate;
            [PXDBDate(BqlField = typeof(INTran.tranDate))]
            public virtual DateTime? TranDate
            {
                get
                {
                    return this._TranDate;
                }
                set
                {
                    this._TranDate = value;
                }
            }
            #endregion
            #region InvtMult
            public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
            protected Int16? _InvtMult;
            [PXDBShort(BqlField = typeof(INTran.invtMult))]
            [PXUIField(DisplayName = "Multiplier")]
            public virtual Int16? InvtMult
            {
                get
                {
                    return this._InvtMult;
                }
                set
                {
                    this._InvtMult = value;
                }
            }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
            protected Int32? _InventoryID;
            [PXDBInt(BqlField = typeof(INTran.inventoryID))]
            [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
            public virtual Int32? InventoryID
            {
                get
                {
                    return this._InventoryID;
                }
                set
                {
                    this._InventoryID = value;
                }
            }
            #endregion
            #region SubItemID
            public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
            protected Int32? _SubItemID;

            [PXDBInt(BqlField = typeof(INTran.subItemID))]
            [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
            public virtual Int32? SubItemID
            {
                get
                {
                    return this._SubItemID;
                }
                set
                {
                    this._SubItemID = value;
                }
            }
            #endregion
            #region SiteID
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
            protected Int32? _SiteID;
            [PXDBInt(BqlField = typeof(INTran.siteID))]
            [PXUIField(DisplayName = "Warehouse", FieldClass = "INSITE")]
            public virtual Int32? SiteID
            {
                get
                {
                    return this._SiteID;
                }
                set
                {
                    this._SiteID = value;
                }
            }
            #endregion
            #region BAccountID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            protected Int32? _BAccountID;
            [PXDBInt(BqlField = typeof(INTran.bAccountID))]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._BAccountID;
                }
                set
                {
                    this._BAccountID = value;
                }
            }
            #endregion
            #region BaseQty
            public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
            protected Decimal? _BaseQty;
            [PXDBQuantity(BqlField = typeof(INTran.baseQty))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? BaseQty
            {
                get
                {
                    return this._BaseQty;
                }
                set
                {
                    this._BaseQty = value;
                }
            }
            #endregion
            #region TranAmt SUM
            public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
            protected Decimal? _TranAmt;
            [PXDBBaseCury(BqlField = typeof(INTran.tranAmt))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Tran Amount")]
            public virtual Decimal? TranAmt
            {
                get
                {
                    return this._TranAmt;
                }
                set
                {
                    this._TranAmt = value;
                }
            }
            #endregion
            #region ItemStatus
            public abstract class itemStatus : PX.Data.BQL.BqlString.Field<itemStatus> { }
            protected String _ItemStatus;
            [PXDBString(2, IsFixed = true, BqlField = typeof(InventoryItem.itemStatus))]
            [PXUIField(DisplayName = "Item Status", Visibility = PXUIVisibility.SelectorVisible)]
            [InventoryItemStatus.List]
            public virtual String ItemStatus
            {
                get
                {
                    return this._ItemStatus;
                }
                set
                {
                    this._ItemStatus = value;
                }
            }
            #endregion
            #region ItemClassID
            public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
            protected int? _ItemClassID;
            [PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
            [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual int? ItemClassID
            {
                get
                {
                    return this._ItemClassID;
                }
                set
                {
                    this._ItemClassID = value;
                }
            }
            #endregion
            #region BaseUnit
            public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
            protected String _BaseUnit;
            [INUnit(DisplayName = "Base Unit", BqlField = typeof(InventoryItem.baseUnit))]
            public virtual String BaseUnit
            {
                get
                {
                    return this._BaseUnit;
                }
                set
                {
                    this._BaseUnit = value;
                }
            }
            #endregion
            #region SalesUnit
            public abstract class salesUnit : PX.Data.BQL.BqlString.Field<salesUnit> { }
            protected String _SalesUnit;
            [INUnit(typeof(InventoryItem.inventoryID), DisplayName = "Sales Unit", BqlField = typeof(InventoryItem.salesUnit))]
            public virtual String SalesUnit
            {
                get
                {
                    return this._SalesUnit;
                }
                set
                {
                    this._SalesUnit = value;
                }
            }
            #endregion
            #region QtyRoundUp
            public abstract class qtyRoundUp : PX.Data.BQL.BqlBool.Field<qtyRoundUp> { }
            protected Boolean? _QtyRoundUp;
            [PXDBBool(BqlField = typeof(InventoryItemExt.aMQtyRoundUp))]
            [PXUIField(DisplayName = "Quantity Round Up")]
            public Boolean? QtyRoundUp
            {
                get
                {
                    return this._QtyRoundUp;
                }
                set
                {
                    this._QtyRoundUp = value;
                }
            }
            #endregion
            #region ReplenishmentPolicyID
            public abstract class replenishmentPolicyID : PX.Data.BQL.BqlString.Field<replenishmentPolicyID> { }
            protected String _ReplenishmentPolicyID;
            [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa", BqlField = typeof(INItemSite.replenishmentPolicyID))]
            [PXUIField(DisplayName = "Seasonality")]
            public virtual String ReplenishmentPolicyID
            {
                get
                {
                    return this._ReplenishmentPolicyID;
                }
                set
                {
                    this._ReplenishmentPolicyID = value;
                }
            }
            #endregion
        }

        #endregion

        #region Create Item Sites

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)] // Use CreateForecastItems(PXResultset<INTranAllSalesHistory> resultset)
        protected virtual ItemSiteCollection CreateForecastItems(PXResultset<INItemSite> resultset)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GetReplenishmentPolicyID
        private string GetReplenishmentPolicyID(int? invID)
        {
            INItemRep itemRepRecM = PXSelect<INItemRep, Where<INItemRep.inventoryID, Equal<Required<INItemRep.inventoryID>>,
                    And<INItemRep.replenishmentSource, Equal<Required<INItemRep.replenishmentSource>>
                        >>>.SelectWindowed(this, 0, 1, invID, INReplenishmentSource.Manufactured);
            if (itemRepRecM?.ReplenishmentPolicyID != null)
            {
                return itemRepRecM.ReplenishmentPolicyID;
            }

            INItemRep itemRepRecP = PXSelect<INItemRep, Where<INItemRep.inventoryID, Equal<Required<INItemRep.inventoryID>>,
                And<INItemRep.replenishmentSource, NotEqual<Required<INItemRep.replenishmentSource>>
                    >>>.SelectWindowed(this, 0, 1, invID, INReplenishmentSource.Manufactured);

            if (itemRepRecP?.ReplenishmentPolicyID != null)
            {
                return itemRepRecP.ReplenishmentPolicyID;
            }

            return null;
        }
        #endregion

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)] // Use CreateBuckets(INTranAllSalesHistory, DateTime, BucketPeriod)
        protected virtual void CreateMonthlyBuckets(ItemSiteCollection items, ref BucketCollection buckets)
        {
            throw new NotImplementedException();
        }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)] // Use CreateBuckets(INTranAllSalesHistory, DateTime, BucketPeriod)
        protected virtual void CreateYearlyBuckets(ItemSiteCollection items, ref BucketCollection buckets)
        {
            throw new NotImplementedException();
        }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)] // Use CreateSeasonalityBuckets(INTranAllSalesHistory)
        protected virtual void CreateSeasonalityBuckets(ItemSiteCollection items, ref BucketCollection buckets)
        {
            throw new NotImplementedException();
        }

        protected virtual BucketCollection CreateSeasonalityBuckets(INTranAllSalesHistory row)
        {
            if (row?.InventoryID == null)
            {
                return new BucketCollection();
            }

            if (row.ReplenishmentPolicyID != null)
            {
                INReplenishmentSeason itemSiteRepSeason = PXSelect<INReplenishmentSeason,
                    Where<INReplenishmentSeason.replenishmentPolicyID,
                        Equal<Required<INReplenishmentSeason.replenishmentPolicyID>>
                            >>.SelectWindowed(this, 0, 1, row.ReplenishmentPolicyID);
                if (itemSiteRepSeason != null)
                {
                    return CreateTheSeasonBuckets(row, row.ReplenishmentPolicyID);
                }
            }

            return CreateTheSeasonBuckets(row, Settings?.Current?.Seasonality);
        }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)] // Use CreateTheSeasonBuckets(INTranAllSalesHistory, string)
        protected virtual void CreateTheSeasonBuckets(ForecastItemSite forecastItemSite, string replenishmentPolicyID, ref BucketCollection buckets)
        {
            throw new NotImplementedException();
        }

        protected virtual BucketCollection CreateTheSeasonBuckets(INTranAllSalesHistory row, string replenishmentPolicyID)
        {
            var seasonBuckets = new BucketCollection();

            var seasonalityStartDateLimit = Settings.Current.ForecastDate.GetValueOrDefault(BusinessDate).AddYears(1);
            foreach (INReplenishmentSeason itemSiteRepSeason in PXSelect<INReplenishmentSeason,
                Where<INReplenishmentSeason.replenishmentPolicyID,
                        Equal<Required<INReplenishmentSeason.replenishmentPolicyID>>, And<INReplenishmentSeason.active, Equal<True>,
                            And<INReplenishmentSeason.startDate, GreaterEqual<Current<ForecastSettings.forecastDate>>,
                                    And<INReplenishmentSeason.startDate, LessEqual<Required<INReplenishmentSeason.startDate>>>>>>,
                                        OrderBy<Asc<INReplenishmentSeason.startDate>>
                                            >.Select(this, replenishmentPolicyID, seasonalityStartDateLimit))
            {
                if (itemSiteRepSeason?.StartDate == null || itemSiteRepSeason.EndDate == null)
                {
                    continue;
                }

                var bucket = CreateBucket(row,
                    itemSiteRepSeason.StartDate.GetValueOrDefault(),
                    itemSiteRepSeason.EndDate.GetValueOrDefault(),
                    itemSiteRepSeason.Factor,
                    itemSiteRepSeason.ReplenishmentPolicyID);

                if (!seasonBuckets.Contains(bucket))
                {
                    seasonBuckets.Add(bucket);
                }
            }

            return seasonBuckets;
        }

        protected virtual BucketCollection CreateBuckets(INTranAllSalesHistory row)
        {
            if (Settings.Current.ForecastDate == null)
            {
                throw new PXSetPropertyException(AM.Messages.GetLocal(AM.Messages.MustChooseForecastDate), PXErrorLevel.Error);
            }

            var beginDate = Settings.Current.ForecastDate.GetValueOrDefault();

            if (Settings.Current.Type == GenerateForecastType.Regular)
            {
                return CreateBuckets(row, beginDate,
                    Settings.Current.CalculateByMonth.GetValueOrDefault() ? BucketPeriod.Monthly : BucketPeriod.Yearly);
            }

            return CreateSeasonalityBuckets(row);
        }

        protected virtual BucketCollection CreateBuckets(INTranAllSalesHistory row, DateTime beginDate, BucketPeriod period)
        {
            var buckets = new BucketCollection();

            var bucket = CreateBucket(row, beginDate, period);
            if (bucket != null)
            {
                buckets.Add(bucket);
            }

            if (period == BucketPeriod.Yearly || bucket == null)
            {
                return buckets;
            }

            for (var i = 1; i <= 11; i++)
            {
                bucket = CreateBucket(row, beginDate.AddMonths(i), period);

                if (!buckets.Contains(bucket))
                {
                    buckets.Add(bucket);
                }
            }

            return buckets;
        }

        protected virtual DateTime GetEndDate(DateTime date, BucketPeriod period)
        {
            if (period == BucketPeriod.Monthly)
            {
                return date.AddMonths(1).AddDays(-1);
            }

            return date.AddYears(1).AddDays(-1);
        }

        protected virtual Bucket CreateBucket(INTranAllSalesHistory row, DateTime beginDate, BucketPeriod period)
        {
            return CreateBucket(row, beginDate, GetEndDate(beginDate, period));
        }

        protected virtual Bucket CreateBucket(INTranAllSalesHistory row, DateTime beginDate, DateTime endDate)
        {
            return CreateBucket(row, beginDate, endDate, Settings.Current.GrowthFactor, null);
        }

        protected virtual Bucket CreateBucket(INTranAllSalesHistory row, DateTime beginDate, DateTime endDate, decimal? growthFactor, string seasonality)
        {
            if (row?.InventoryID == null)
            {
                return null;
            }

            return new Bucket(beginDate, endDate, row.InventoryID, row.SubItemID,
                row.SiteID, growthFactor, seasonality)
            {
                BaseUOM = row.BaseUnit,
                SalesUOM = row.SalesUnit,
                QtyRoundUp = row.QtyRoundUp,
                BAccountID = row.BAccountID
            };
        }

        protected virtual void DeleteSessionData()
        {
            SelectedRecs.Cache.Clear();
            SelectedRecs.Cache.ClearQueryCache();

            foreach (AMForecastStaging historyRecord in SelectedRecs.Select())
            {
                SelectedRecs.Delete(historyRecord);
            }

            if (SelectedRecs.Cache.Deleted.Any_())
            {
                Persist();
            }
        }

        protected virtual void WriteForecastStagingRecord(Bucket bucket, decimal forecastQty, decimal lastYearQty)
        {
            var forecastStagingRecord = new AMForecastStaging
            {
                UserID = Accessinfo.UserID,
                InventoryID = bucket.InventoryID,
                SubItemID = bucket.SubItemID,
                SiteID = bucket.SiteID,
                BeginDate = bucket.BeginDate,
                EndDate = bucket.EndDate,
                Seasonality = bucket.ReplenishmentPolicyID,
                UOM = bucket.BaseUOM,
                ForecastQty = forecastQty,
                LastYearBaseQty = lastYearQty,
                LastYearSalesQty = lastYearQty,
                Dependent = Settings?.Current?.Dependent,
                CustomerID = bucket.BAccountID
            };
            
            if (TryConvertToSalesUnit(forecastStagingRecord, bucket.SalesUOM, out var uomQty, out var lastYearUomQty))
            {
                forecastStagingRecord.ForecastQty = uomQty;
                forecastStagingRecord.LastYearSalesQty = lastYearUomQty;
                forecastStagingRecord.UOM = bucket.SalesUOM;
            }

            if (bucket.QtyRoundUp.GetValueOrDefault())
            {
                forecastStagingRecord.ForecastQty = Math.Ceiling(forecastStagingRecord.ForecastQty.GetValueOrDefault());
            }

            SelectedRecs.Insert(forecastStagingRecord);
        }

        protected virtual bool TryConvertToSalesUnit(AMForecastStaging forecastStaging, string salesUOM, out decimal? forecastQty, out decimal? lastYearQty)
        {
            forecastQty = null;
            lastYearQty = null;

            if(forecastStaging?.UOM == null || string.IsNullOrWhiteSpace(salesUOM) || forecastStaging?.UOM == salesUOM)
            {
                return false;
            }

            if (!UomHelper.TryConvertFromBaseQty<AMForecastStaging.inventoryID>(this.SelectedRecs.Cache, forecastStaging,
                    salesUOM,
                    forecastStaging.ForecastQty.GetValueOrDefault(),
                    out var forecastQtyInSalesUnits))
            {
                return false;
            }

            if (!UomHelper.TryConvertFromBaseQty<AMForecastStaging.inventoryID>(this.SelectedRecs.Cache, forecastStaging,
                    salesUOM,
                    forecastStaging.LastYearSalesQty.GetValueOrDefault(),
                    out var lastYearQtyInSalesUnits))
            {
                return false;
            }

            if(forecastQtyInSalesUnits == null || lastYearQtyInSalesUnits == null)
            {
                return false;
            }

            forecastQty = forecastQtyInSalesUnits;
            lastYearQty = lastYearQtyInSalesUnits;
            return true;
        }

        protected virtual void ForecastSettings_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var fss = (ForecastSettings)e.Row;
            if (fss == null)
            {
                return;
            }

            if (!IsForecastAutoNumbered)
            {
                PXUIFieldAttribute.SetWarning<ForecastSettings.forecastDate>(cache, e.Row, AM.Messages.GetLocal(AM.Messages.ForecastNumberSequenceMustBeAutoNumbered));
            }

            var isRegular = fss.Type == GenerateForecastType.Regular;
            PXUIFieldAttribute.SetEnabled<ForecastSettings.seasonality>(cache, e.Row, !isRegular);
            PXUIFieldAttribute.SetEnabled<ForecastSettings.calculateByMonth>(cache, e.Row, isRegular);
            PXUIFieldAttribute.SetEnabled<ForecastSettings.growthFactor>(cache, e.Row, isRegular);

            processRecords.SetEnabled(ContainsForecastStaging);
            processAllRecords.SetEnabled(ContainsForecastStaging);
        }

        protected virtual void AMForecastStaging_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var fs = (AMForecastStaging)e.Row;
            if(fs?.InventoryID == null || IsStagingCalculation)
            {
                return;
            }

            cache.SetDefaultExt<AMForecastStaging.subItemID>(e.Row);
            cache.SetDefaultExt<AMForecastStaging.uOM>(e.Row);
            cache.SetDefaultExt<AMForecastStaging.siteID>(e.Row);
        }

        protected virtual void ProcessBuckets(PXSelectBase<INTranSalesHistory> cmd, QueryRestriction queryRestriction, BucketCollection buckets)
        {
            if (buckets == null)
            {
                return;
            }

            var nbrOfYears = Settings.Current.Years.GetValueOrDefault();
            if (nbrOfYears <= 0)
            {
                throw new PXArgumentException(nameof(nbrOfYears));
            }

            foreach (Bucket bucket in buckets)
            {
                if (bucket?.InventoryID == null)
                {
                    continue;
                }

                var adjustedNbrOfYears = nbrOfYears - 1;
                var totalForecastQty = 0m;
                for (var i = nbrOfYears; i >= 2; i--)
                {
                    totalForecastQty += GetSalesHistoryTotal(cmd, queryRestriction, bucket.BeginDate.AddYears(-i), bucket.EndDate.AddYears(-i), bucket, bucket.BAccountID);
                }

                //Get previous year sales
                var previousYearQty = GetSalesHistoryTotal(cmd, queryRestriction, bucket.BeginDate.AddYears(-1), bucket.EndDate.AddYears(-1), bucket, bucket.BAccountID);

                var forecastQty = 0m;
                if (totalForecastQty != 0 && adjustedNbrOfYears != 0)
                {
                    forecastQty = (previousYearQty * bucket.GrowthFactor.GetValueOrDefault() + (totalForecastQty / adjustedNbrOfYears) * (1 - bucket.GrowthFactor.GetValueOrDefault()))
                        * (1 + Settings.Current.GrowthRate.GetValueOrDefault());
                }
                else
                {
                    forecastQty = previousYearQty * (1 + Settings.Current.GrowthRate.GetValueOrDefault());
                }

                if (forecastQty > 0)
                {
                    WriteForecastStagingRecord(bucket, forecastQty, previousYearQty);
                }
            }
        }

        public enum BucketPeriod
        {
            Monthly,
            Yearly
        }
    }
    
    #region ForecastSettings
    [Serializable]
    [PXCacheName(AM.Messages.ForecastSettings)]
    public class ForecastSettings : IBqlTable
    {
        #region ForecastDate
        public abstract class forecastDate : PX.Data.BQL.BqlDateTime.Field<forecastDate> { }

        protected DateTime? _ForecastDate;
        [PXDBDate(IsKey = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Forecast Date")]
        public virtual DateTime? ForecastDate
        {
            get
            {
                return this._ForecastDate;
            }
            set
            {
                this._ForecastDate = value;
            }
        }
        #endregion

        #region Type

        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

        protected String _Type;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(GenerateForecastType.Regular)]
        [PXUIField(DisplayName = "Type")]
        [GenerateForecastType.List]
        public virtual String Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
            }
        }

        #endregion

        #region Seasonality
        public abstract class seasonality : PX.Data.BQL.BqlString.Field<seasonality> { }
        protected String _Seasonality;
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Seasonality")]
        [PXSelector(typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>), DescriptionField = typeof(INReplenishmentPolicy.descr))]
        public virtual String Seasonality
        {
            get
            {
                return this._Seasonality;
            }
            set
            {
                this._Seasonality = value;
            }
        }
        #endregion

        #region GrowthRate
        public abstract class growthRate : PX.Data.BQL.BqlDecimal.Field<growthRate> { }
        protected Decimal? _GrowthRate;
        [PXUIField(DisplayName = "Growth Rate", Enabled = true)]
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        public virtual Decimal? GrowthRate
        {
            get
            {
                return _GrowthRate;
            }
            set
            {
                _GrowthRate = value;
            }
        }
        #endregion

        #region GrowthFactor
        public abstract class growthFactor : PX.Data.BQL.BqlDecimal.Field<growthFactor> { }
        protected Decimal? _GrowthFactor;
        [PXUIField(DisplayName = "Factor", Enabled = true)]
        [PXDBDecimal(4, MaxValue = 1.0000)]
        [PXDefault(TypeCode.Decimal, "1.0000")]
        public virtual Decimal? GrowthFactor
        {
            get
            {
                return _GrowthFactor;
            }
            set
            {
                _GrowthFactor = value;
            }
        }
        #endregion

        #region CalculateByMonth
        public abstract class calculateByMonth : PX.Data.BQL.BqlBool.Field<calculateByMonth> { }
        protected Boolean? _CalculateByMonth;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Calculate By Month")]
        public virtual Boolean? CalculateByMonth
        {
            get
            {
                return this._CalculateByMonth;
            }
            set
            {
                this._CalculateByMonth = value;
            }
        }
        #endregion

        #region Dependent
        public abstract class dependent : PX.Data.BQL.BqlBool.Field<dependent> { }
        protected Boolean? _Dependent;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Dependent")]
        public virtual Boolean? Dependent
        {
            get
            {
                return this._Dependent;
            }
            set
            {
                this._Dependent = value;
            }
        }
        #endregion

        #region Years
        public abstract class years : PX.Data.BQL.BqlInt.Field<years> { }
        protected Int32? _Years;
        [PXDefault(TypeCode.Int32, "1")]
        [PXDBInt(MinValue = 1)]
        [PXUIField(DisplayName = "Years of History")]
        public virtual Int32? Years
        {
            get
            {
                return _Years;
            }
            set
            {
                _Years = value;
            }
        }
        #endregion

        #region BAccountID "Customer"
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        protected Int32? _BAccountID;
        [Customer(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Customer", DescriptionField = typeof(Customer.bAccountID))]
        public virtual Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }
		#endregion
        
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [Site]
        public virtual Int32? SiteID
        {
            get { return this._SiteID; }
            set { this._SiteID = value; }
        }
        #endregion

        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem]
        public virtual Int32? InventoryID
        {
            get { return this._InventoryID; }
            set { this._InventoryID = value; }
        }
        #endregion

        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [SubItem(typeof(ForecastSettings.inventoryID))]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion

        #region ProcessByCustomer
        public abstract class processByCustomer : PX.Data.BQL.BqlBool.Field<processByCustomer> { }
        protected Boolean? _ProcessByCustomer;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Process By Customer")]
        public virtual Boolean? ProcessByCustomer
        {
            get
            {
                return this._ProcessByCustomer;
            }
            set
            {
                this._ProcessByCustomer = value;
            }
        }
        #endregion
    }
    #endregion

    #region Forecast Type

    public class GenerateForecastType
    {
        /// <summary>
        /// Regular Forecast
        /// </summary>
        public const string Regular = "R";
        /// <summary>
        /// Seasonality based forecast
        /// </summary>
        public const string Seasonality = "S";

        /// <summary>
        /// Descriptions for Forecast type string identifiers
        /// </summary>
        public static class Desc
        {
            public static string Regular { get { return AM.Messages.GetLocal(AM.Messages.Regular); } }
            public static string Seasonality { get { return AM.Messages.GetLocal(AM.Messages.Seasonality); } }
        }

        public class regular : PX.Data.BQL.BqlString.Constant<regular>
        {
            public regular() : base(Regular) {; }
        }
        public class seasonality : PX.Data.BQL.BqlString.Constant<seasonality>
        {
            public seasonality() : base(Seasonality) {; }
        }

        #region List attribute
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[]
                {
                    GenerateForecastType.Regular, 
                    GenerateForecastType.Seasonality},
                new string[]{
                   GenerateForecastType.Desc.Regular,
                   GenerateForecastType.Desc.Seasonality}) 
            { ; }
        }
        #endregion
    }
    #endregion

    #region BucketCollection
    [Serializable]
    public class BucketCollection : CollectionBase
    {
        #region Indexer
        // Indexer for our collection
        public Bucket this[int index]
        {
            get
            {
                //returns the RollCostItem at the specified index
                return (Bucket)List[index];
            }
            set
            {
                List[index] = value;
            }

        }
        #endregion

        #region Add Method
        public void Add(Bucket bucket)
        {
            //add this item to the list
            List.Add(bucket);
        }
        #endregion

        #region Contains
        // Check the collection for RollCostItem
        public bool Contains(Bucket bucket)
        {
            //loop through all the items in the list
            foreach (Bucket item in this.List)
            {
                if (item.IsEqual(bucket))
                {
                    return true;
                }

            }
            return false;
        }
        #endregion

        #region Exists
        // Check the collection for RollCostItem
        public bool Exists(Bucket bucket)
        {
            //loop through all the items in the list
            foreach (Bucket item in this.List)
            {
                if (item.IsInList(bucket))
                {
                    return true;
                }

            }
            return false;
        }
        #endregion

        #region Get index of item
        public int IndexOf(Bucket bucket)
        {
            int index = 0;

            //loop through all the items in the list
            foreach (Bucket item in this.List)
            {
                if (item.IsEqual(bucket))
                {
                    return index;
                }
                index += 1;
            }
            return -1;

        }
        #endregion
    }
    #endregion

    #region Bucket class
    [Serializable]
    public class Bucket
    {
        #region Constructors
        public Bucket(DateTime bDate, DateTime eDate, int? inventoryid, int? subitemid, int? siteid, decimal? growthFactor, string seasonality)
        {
            BeginDate = bDate;
            EndDate = eDate;
            InventoryID = inventoryid;
            SubItemID = subitemid;
            SiteID = siteid;
            GrowthFactor = growthFactor;
            ReplenishmentPolicyID = seasonality;
        }
        #endregion
        #region BeginDate
        public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }

        protected DateTime _BeginDate;
        [PXDBDate]
        [PXUIField(DisplayName = "Begin Date")]
        public virtual DateTime BeginDate
        {
            get
            {
                return this._BeginDate;
            }
            set
            {
                this._BeginDate = value;
            }
        }
        #endregion
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

        protected DateTime _EndDate;
        [PXDBDate]
        [PXUIField(DisplayName = "End Date")]
        public virtual DateTime EndDate
        {
            get
            {
                return this._EndDate;
            }
            set
            {
                this._EndDate = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXUIField(DisplayName = "Inventory ID")]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region BaseUOM
        public abstract class baseUOM : PX.Data.BQL.BqlString.Field<baseUOM> { }
        [PXDBString]
        public virtual String BaseUOM { get; set; }
        #endregion
        #region SalesUOM
        public abstract class salesUOM : PX.Data.BQL.BqlString.Field<salesUOM> { }
        [PXDBString]
        public virtual String SalesUOM { get; set; }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXUIField(DisplayName = "SubItem ID")]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXUIField(DisplayName = "Site ID")]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region GrowthFactor
        public abstract class growthFactor : PX.Data.BQL.BqlDecimal.Field<growthFactor> { }

        protected Decimal? _GrowthFactor;
        [PXUIField(DisplayName = "Factor", Enabled = true)]
        [PXDBDecimal(4, MaxValue = 1.0000)]
        [PXDefault(TypeCode.Decimal, "1.0000")]
        public virtual Decimal? GrowthFactor
        {
            get
            {
                return _GrowthFactor;
            }
            set
            {
                _GrowthFactor = value;
            }
        }
        #endregion
        #region ReplenishmentPolicyID
        public abstract class replenishmentPolicyID : PX.Data.BQL.BqlString.Field<replenishmentPolicyID> { }

        protected String _ReplenishmentPolicyID;
        [PXDefault]
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "ReplenishmentPolicyID")]
        public virtual String ReplenishmentPolicyID
        {
            get
            {
                return this._ReplenishmentPolicyID;
            }
            set
            {
                this._ReplenishmentPolicyID = value;
            }
        }
        #endregion
        #region QtyRoundUp
        [PXDBBool]
        [PXUIField(DisplayName = "Quantity Round Up")]
        public Boolean? QtyRoundUp { get; set; }
        #endregion
        #region IS Equal
        public bool IsEqual(Bucket compareItem)
        {
            if (compareItem == null)
            {
                return false;
            }

            if (BeginDate != compareItem.BeginDate)
            {
                return false;
            }

            if (EndDate != compareItem.EndDate)
            {
                return false;
            }

            if (InventoryID != compareItem.InventoryID)
            {
                return false;
            }

            if (SubItemID != compareItem.SubItemID)
            {
                return false;
            }

            if (SiteID != compareItem.SiteID)
            {
                return false;
            }

            return true;
        }
        #endregion
        #region Is In List
        public bool IsInList(Bucket compareItem)
        {
            if (compareItem == null)
            {
                return false;
            }

            if (BeginDate != compareItem.BeginDate)
            {
                return false;
            }

            if (EndDate != compareItem.EndDate)
            {
                return false;
            }

            if (InventoryID != compareItem.InventoryID)
            {
                return false;
            }

            if (SubItemID != compareItem.SubItemID)
            {
                return false;
            }

            if (SiteID != compareItem.SiteID)
            {
                return false;
            }

            return true;
        }
        #endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        protected Int32? _BAccountID;
        [PXDBInt]
        public virtual Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }
        #endregion
    }
    #endregion

    #region ItemSiteCollection
    // Class to hold the collection of Item Sites
    [Obsolete(InternalMessages.ClassIsObsoleteAndWillBeRemoved2020R2)]
    [Serializable]
    public class ItemSiteCollection : CollectionBase
    {
        #region Indexer
        // Indexer for our collection
        public ForecastItemSite this[int index]
        {
            get
            {
                //returns the RollCostItem at the specified index
                return (ForecastItemSite)List[index];
            }
            set
            {
                List[index] = value;
            }

        }
        #endregion

        #region Add Method
        public void Add(ForecastItemSite itemSite)
        {
            //add this item to the list
            List.Add(itemSite);
        }
        #endregion

        #region Contains
        // Check the collection for ItemSite
        public bool Contains(ForecastItemSite itemSite)
        {
            //loop through all the items in the list
            foreach (ForecastItemSite item in this.List)
            {
                if (item.IsEqual(itemSite))
                {
                    return true;
                }

            }
            return false;
        }
        #endregion

        #region Exists
        // Check the collection for RollCostItem
        public bool Exists(ForecastItemSite itemSite)
        {
            //loop through all the items in the list
            foreach (ForecastItemSite item in this.List)
            {
                if (item.IsInList(itemSite))
                {
                    return true;
                }

            }
            return false;
        }
        #endregion

        #region Get index of item
        public int IndexOf(ForecastItemSite itemSite)
        {
            int index = 0;

            //loop through all the items in the list
            foreach (ForecastItemSite item in this.List)
            {
                if (item.IsEqual(itemSite))
                {
                    return index;
                }
                index += 1;
            }
            return -1;

        }
        #endregion
    }
    #endregion

    #region ForecastItemSite
    [Obsolete(InternalMessages.ClassIsObsoleteAndWillBeRemoved2020R2)]
    [Serializable]
    public class ForecastItemSite
    {
        #region Constructors
        public ForecastItemSite(int? inventoryid, int? subitemid, int? siteid, string repPolicyID)
        {
            InventoryID = inventoryid;
            SubItemID = subitemid;
            SiteID = siteid;
            ReplenishmentPolicyID = repPolicyID;
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXUIField(DisplayName = "Inventory ID")]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXUIField(DisplayName = "Subitem")]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXUIField(DisplayName = "Site ID")]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region ReplenishmentPolicyID
        public abstract class replenishmentPolicyID : PX.Data.BQL.BqlString.Field<replenishmentPolicyID> { }

        protected String _ReplenishmentPolicyID;
        [PXDefault]
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "ReplenishmentPolicyID")]
        public virtual String ReplenishmentPolicyID
        {
            get
            {
                return this._ReplenishmentPolicyID;
            }
            set
            {
                this._ReplenishmentPolicyID = value;
            }
        }
        #endregion
        #region BaseUOM
        public abstract class baseUOM : PX.Data.BQL.BqlString.Field<baseUOM> { }

        [PXDBString]
        public virtual String BaseUOM { get; set; }
        #endregion
        #region SalesUOM
        public abstract class salesUOM : PX.Data.BQL.BqlString.Field<salesUOM> { }

        [PXDBString]
        public virtual String SalesUOM { get; set; }
        #endregion
        #region QtyRoundUp
        [PXDBBool]
        [PXUIField(DisplayName = "Quantity Round Up")]
        public Boolean? QtyRoundUp { get; set; }
        #endregion
        #region IS Equal
        public bool IsEqual(ForecastItemSite compareItem)
        {
            if (compareItem == null)
            {
                return false;
            }

            if (InventoryID != compareItem.InventoryID)
            {
                return false;
            }

            if (SubItemID != compareItem.SubItemID)
            {
                return false;
            }

            if (SiteID != compareItem.SiteID)
            {
                return false;
            }

            if (ReplenishmentPolicyID != compareItem.ReplenishmentPolicyID)
            {
                return false;
            }

            return true;
        }
        #endregion
        #region Is In List
        public bool IsInList(ForecastItemSite compareItem)
        {
            if (compareItem == null)
            {
                return false;
            }

            if (InventoryID != compareItem.InventoryID)
            {
                return false;
            }

            if (SubItemID != compareItem.SubItemID)
            {
                return false;
            }

            if (SiteID != compareItem.SiteID)
            {
                return false;
            }

            if (ReplenishmentPolicyID != compareItem.ReplenishmentPolicyID)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
    #endregion
}
