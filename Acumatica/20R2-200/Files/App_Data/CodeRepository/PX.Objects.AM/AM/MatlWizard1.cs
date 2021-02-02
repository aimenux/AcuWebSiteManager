using System;
using PX.Data;
using System.Collections.Generic;
using PX.Objects.CS;
using System.Collections;
using System.Linq;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class MatlWizard1 : PXGraph<MatlWizard1>
    {
        public PXFilter<WizFilter> filter;

        public PXProcessing<AMProdItem,
            Where<AMProdItem.hold, Equal<boolFalse>,
                And<AMProdItem.function, NotEqual<OrderTypeFunction.disassemble>,
                    And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                            Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>>>
                    >>>> OpenOrders;

        public PXSelect<AMWrkMatl, Where<AMWrkMatl.userID, Equal<Current<AccessInfo.userID>>>> MatlXref;

        public PXAction<AMProdItem> cancel;

        // Turn off the new UI processing window (19R1+)
        public override bool IsProcessing => false;

        [PXUIField(DisplayName = "Cancel")]
        [PXButton]
        public virtual IEnumerable Cancel(PXAdapter adapter)
        {
            DeleteWrkTableRecs();
            var rm = PXGraph.CreateInstance<MaterialEntry>();
            throw new PXRedirectRequiredException(rm, string.Empty);
        }

        public MatlWizard1()
        {
            var wizFilter = (WizFilter) filter.Cache.CreateCopy(filter.Current);
            OpenOrders.SetProcessDelegate(
                delegate(List<AMProdItem> list)
                {
#pragma warning disable PX1088 // Processing delegates cannot use the data views from processing graphs except for the data views of the PXFilter, PXProcessingBase, or PXSetup types
#if DEBUG
                    // OK to ignore PX1088 per Dmitry 11/19/2019 - Cause is use of MatlXref to set results for next wizard screen
#endif
                    FillMatlWrk(list, wizFilter);
#pragma warning restore PX1088
                }
            );
            OpenOrders.SetProcessCaption(Messages.Select);
            OpenOrders.SetProcessAllCaption(Messages.SelectAll);
            PXUIFieldAttribute.SetEnabled<AMProdItem.qtytoProd>(OpenOrders.Cache, null, true);
        }

        protected virtual void DeleteWrkTableRecs()
        {
            DeleteWrkTableRecs(this.Accessinfo.UserID);
        }

        protected static void DeleteWrkTableRecs(Guid userID)
        {
            PXDatabase.Delete<AMWrkMatl>(new PXDataFieldRestrict<AMWrkMatl.userID>(userID));
        }

        public static void FillMatlWrk(List<AMProdItem> list, WizFilter filter)
        {
            var mw = PXGraph.CreateInstance<MatlWizard1>();
            mw.DeleteWrkTableRecs();
            mw.Clear(PXClearOption.ClearAll);
            mw.filter.Current = filter;

            foreach (var amproditem in list.OrderBy(x => x.StartDate).ThenBy(x => x.SchPriority))
            {
                FillMatlWrk(mw, amproditem);
            }

            mw.Persist();

            //redirect to step 2
            var mw2 = PXGraph.CreateInstance<MatlWizard2>();
            throw new PXRedirectRequiredException(mw2, "Material Wizard 2");
        }

        public static void FillMatlWrk(AMProdItem amproditem)
        {
            FillMatlWrk(amproditem, PXRedirectHelper.WindowMode.Same);
        }

        public static void FillMatlWrk(AMProdItem amproditem, PXRedirectHelper.WindowMode windowMode)
        {
            var mw = PXGraph.CreateInstance<MatlWizard1>();
            mw.DeleteWrkTableRecs();
            mw.Clear(PXClearOption.ClearAll);

            FillMatlWrk(mw, amproditem);

            mw.Persist();

            //redirect to step 2
            var mw2 = PXGraph.CreateInstance<MatlWizard2>();
            PXRedirectHelper.TryRedirect(mw2, windowMode);
        }

        protected static decimal GetMaterialTotalQty(AMProdMatl prodMatl, decimal? qtyToProd)
        {
            if (prodMatl == null)
            {
                throw new ArgumentNullException(nameof(prodMatl));
            }

            if (qtyToProd.GetValueOrDefault() == 0 ||
                prodMatl.IsFixedMaterial.GetValueOrDefault() && prodMatl.QtyActual.GetValueOrDefault() != 0)
            {
                return 0m;
            }

            var multiplier = prodMatl.IsByproduct.GetValueOrDefault() ? -1 : 1;

            var totalQtyReq = prodMatl.GetTotalReqQty(qtyToProd.GetValueOrDefault());
            var remainingMatlQty = prodMatl.QtyRemaining.GetValueOrDefault() * multiplier;
            var actualQty = prodMatl.QtyActual.GetValueOrDefault();

            // Same as QtyRemaining PXFormula
            var calcRemainingQty = (prodMatl.IsByproduct.GetValueOrDefault()
                ? Math.Min(totalQtyReq - actualQty, 0)
                : totalQtyReq - actualQty) * multiplier;

            totalQtyReq *= multiplier;
            var remainingQty = Math.Max(remainingMatlQty, calcRemainingQty.NotLessZero());

            return remainingQty < totalQtyReq ? remainingQty : totalQtyReq;
        }

        public static void FillMatlWrk(MatlWizard1 mw, AMProdItem amproditem)
        {
            mw.ProcessMatlWrk(amproditem);
        }

        protected virtual void ProcessMatlWrk(AMProdItem amproditem)
        {
            if (amproditem == null || string.IsNullOrWhiteSpace(amproditem.ProdOrdID))
            {
                return;
            }
#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var counter = 0;
#endif
            foreach (PXResult<AMProdMatl, InventoryItem, AMOrderType> result in PXSelectJoin<
                            AMProdMatl,
                            InnerJoin<InventoryItem, 
                                On<AMProdMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
                            InnerJoin<AMOrderType, 
                                On<AMProdMatl.orderType, Equal<AMOrderType.orderType>>>>,
                            Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                                And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                                And<AMProdMatl.bFlush, NotEqual<True>,
                                And<AMProdMatl.qtyReq, NotEqual<decimal0>>>>>,
                            OrderBy<
                                Asc<AMProdMatl.sortOrder, 
                                Asc<AMProdMatl.lineID>>>
            >
                            .Select(this, amproditem.OrderType, amproditem.ProdOrdID))
            {
#if DEBUG
                if (counter == 0)
                {
                    AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, $"{amproditem.OrderType}:{amproditem.ProdOrdID} material query time"));
                    //skip query time from overall total
                    sw = System.Diagnostics.Stopwatch.StartNew();
                }
                counter++;
#endif
                // Cache for reuse in ProcessMatlWrk
                this.Caches[typeof(AMOrderType)].Current = (AMOrderType)result;

                ProcessMatlWrk(amproditem, result, result);
            }
#if DEBUG
            var elapsed = sw.Elapsed;
            var averageMs = counter == 0 ? 0 : elapsed.TotalMilliseconds / counter;
            var averageTicks = counter == 0 ? 0 : elapsed.Ticks / counter;
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(elapsed, $"{amproditem.OrderType}:{amproditem.ProdOrdID}; Material Counter = {counter}; Avg milliseconds = {averageMs}; Total Ticks = {elapsed.Ticks}; Avg Ticks = {averageTicks}"));
#endif
        }

        protected virtual void ProcessMatlWrk(AMProdItem amproditem, AMProdMatl amprodmatl, InventoryItem inventoryItem)
        {
            if (amprodmatl == null || amprodmatl.SubcontractSource == AMSubcontractSource.VendorSupplied || inventoryItem == null)
            {
                return;
            }

            var orderType = (AMOrderType)this.Caches[typeof(AMOrderType)].Current;

            var totalQtyReq = GetMaterialTotalQty(amprodmatl, amproditem.BaseQtytoProd);
            if (totalQtyReq == 0)
            {
                return;
            }

            var amWrkMatl = PXCache<AMWrkMatl>.CreateCopy(MatlXref.Insert(new AMWrkMatl()));
            amWrkMatl.BFlush = amprodmatl.BFlush;
            amWrkMatl.Descr = amprodmatl.Descr;
            amWrkMatl.InventoryID = amprodmatl.InventoryID;
            amWrkMatl.SubItemID = amprodmatl.SubItemID;
            amWrkMatl.LineID = amprodmatl.LineID;
            amWrkMatl.OperationID = amprodmatl.OperationID;
            amWrkMatl.OrderType = amprodmatl.OrderType;
            amWrkMatl.ProdOrdID = amprodmatl.ProdOrdID;
            amWrkMatl.IsByproduct = amprodmatl.IsByproduct;
            amWrkMatl.SiteID = amprodmatl.SiteID;
            amWrkMatl.LocationID = amprodmatl.LocationID;
            amWrkMatl.UOM = amprodmatl.UOM;
            amWrkMatl.UserID = Accessinfo.UserID;
            amWrkMatl.QtyReq = totalQtyReq;
            amWrkMatl.QtyRemaining = amprodmatl.QtyRemaining;
            amWrkMatl.BaseQtyRemaining = amprodmatl.BaseQtyRemaining;
            amWrkMatl.OverIssueMaterial = orderType?.OverIssueMaterial ?? SetupMessage.AllowMsg;
            amWrkMatl.SubcontractSource = amprodmatl.SubcontractSource;

            var avilQtyInBaseUnits = GetMaterialBaseQtyAvail(amprodmatl);
            amWrkMatl.QtyAvail = avilQtyInBaseUnits;
            avilQtyInBaseUnits -= GetBaseQtyUsed(amWrkMatl);

            if (UomHelper.TryConvertFromBaseQty<AMWrkMatl.inventoryID>(this.Caches<AMWrkMatl>(), amWrkMatl,
                amWrkMatl.UOM,
                avilQtyInBaseUnits,
                out var availQtyInProdUnits))
            {
                amWrkMatl.QtyAvail = availQtyInProdUnits.GetValueOrDefault();
            }

            amWrkMatl.MatlQty = amWrkMatl.QtyReq.GetValueOrDefault();
            if (amWrkMatl.QtyAvail.GetValueOrDefault() < amWrkMatl.QtyReq.GetValueOrDefault() &&
                inventoryItem.StkItem.GetValueOrDefault() && !inventoryItem.NegQty.GetValueOrDefault() &&
                !amWrkMatl.IsByproduct.GetValueOrDefault())
            {
                amWrkMatl.MatlQty = amWrkMatl.QtyAvail.GetValueOrDefault();
            }

            // Not allowed to over issue so adjust (as users can change production qty on wizard 1 which drive displayed qty shown on wizard 2)
            if (amWrkMatl.OverIssueMaterial == Attributes.SetupMessage.ErrorMsg && amWrkMatl.MatlQty.GetValueOrDefault() > amWrkMatl.QtyRemaining.GetValueOrDefault())
            {
                amWrkMatl.MatlQty = amWrkMatl.QtyRemaining.GetValueOrDefault();
            }

            if (amWrkMatl.QtyAvail.GetValueOrDefault() < 0)
            {
                amWrkMatl.QtyAvail = 0m;
            }

            var isOrderTypeCheckUnreleasedBatchQty = amWrkMatl.OverIssueMaterial != SetupMessage.AllowMsg && orderType?.IncludeUnreleasedOverIssueMaterial == true;
            var wizardCheckUnreleasedBatchQty = filter?.Current?.ExcludeUnreleasedBatchQty == true;
            if (wizardCheckUnreleasedBatchQty || isOrderTypeCheckUnreleasedBatchQty)
            {
                amWrkMatl.BaseUnreleasedBatchQty = ProductionTransactionHelper.GetUnreleasedMaterialBaseQty(this, amprodmatl);
                amWrkMatl.UnreleasedBatchQty = amWrkMatl.BaseUnreleasedBatchQty;
                if (amWrkMatl.BaseUnreleasedBatchQty != 0 && inventoryItem?.BaseUnit != null && 
                    !inventoryItem.BaseUnit.EqualsWithTrim(amprodmatl.UOM) && UomHelper.TryConvertFromBaseQty<AMProdMatl.inventoryID>(
                        this.Caches<AMProdMatl>(), 
                        amprodmatl,
                        amprodmatl.UOM,
                        amWrkMatl.BaseUnreleasedBatchQty.GetValueOrDefault(),
                        out var unreleasedQty))
                {
                    amWrkMatl.UnreleasedBatchQty = unreleasedQty.GetValueOrDefault();
                }

                if (amWrkMatl.UnreleasedBatchQty.GetValueOrDefault() > 0m && 
                        wizardCheckUnreleasedBatchQty || 
                        // If warn message then we will let the qty be what the qty is - wizard2 will show a warning on the qty
                        (isOrderTypeCheckUnreleasedBatchQty && amWrkMatl.OverIssueMaterial == SetupMessage.ErrorMsg))
                {
                    amWrkMatl.MatlQty = amWrkMatl.MatlQty.GetValueOrDefault() -
                                        amWrkMatl.UnreleasedBatchQty.GetValueOrDefault();
                }    
            }

#if DEBUG
            var shortageMsg = amWrkMatl.MatlQty.GetValueOrDefault() < amWrkMatl.QtyReq.GetValueOrDefault()
                ? "**Shortage**"
                : string.Empty;
            AMDebug.TraceWriteLine(
                $"{shortageMsg}[{amWrkMatl.OrderType.TrimIfNotNullEmpty()}-{amWrkMatl.ProdOrdID.TrimIfNotNullEmpty()}-({amWrkMatl.OperationID.GetValueOrDefault()})-{inventoryItem.InventoryCD.TrimIfNotNullEmpty()}] MatlQty = {amWrkMatl.MatlQty.GetValueOrDefault()} {amWrkMatl.UOM.TrimIfNotNullEmpty()}; QtyReq = {amWrkMatl.QtyReq.GetValueOrDefault()} {amWrkMatl.UOM.TrimIfNotNullEmpty()}; QtyAvail = {amWrkMatl.QtyAvail.GetValueOrDefault()} {amWrkMatl.UOM.TrimIfNotNullEmpty()}");
#endif
            if (amWrkMatl.MatlQty.GetValueOrDefault() > 0)
            {
                MatlXref.Update(amWrkMatl);
                return;
            }

            if (MatlXref.Cache.GetStatus(amWrkMatl) == PXEntryStatus.Inserted)
            {
                MatlXref.Cache.Remove(amWrkMatl);
                return;
            }

            MatlXref.Delete(amWrkMatl);
        }
        /// <summary>
        /// Find the base quantity available for the given material item
        /// </summary>
        protected virtual decimal GetMaterialBaseQtyAvail(AMProdMatl amprodmatl)
        {
            if (amprodmatl?.ProdOrdID == null)
            {
                return 0m;
            }

            var avilQtyInBaseUnits = InventoryHelper.GetQtyHardAvail(this, amprodmatl.InventoryID, amprodmatl.SubItemID, amprodmatl.SiteID, amprodmatl.LocationID, null, null, true, true);
            var allocatedBaseQty = AMProdMatl.GetSplits(this, amprodmatl)?.Where(x => x.IsAllocated == true).Sum(x => x.BaseQty) ?? 0m;

            return avilQtyInBaseUnits + allocatedBaseQty;
        }

        /// <summary>
        /// What base quantity has been used within the same wizard process
        /// </summary>
        protected virtual decimal GetBaseQtyUsed(AMWrkMatl amWrkMatl)
        {
            var usedBaseQty = 0m;
            //add up matlqty already used up in this same batch
            foreach (AMWrkMatl sumwrk in PXSelect<AMWrkMatl, Where<AMWrkMatl.userID, Equal<Required<AMWrkMatl.userID>>,
                And<AMWrkMatl.inventoryID, Equal<Required<AMWrkMatl.inventoryID>>,
                    And<AMWrkMatl.siteID, Equal<Required<AMWrkMatl.siteID>>,
                        And<IsNull<AMWrkMatl.subItemID, int0>, Equal<Required<AMWrkMatl.subItemID>>>>>
            >>.Select(this, Accessinfo.UserID, amWrkMatl.InventoryID, amWrkMatl.SiteID, amWrkMatl.SubItemID.GetValueOrDefault()))
            {
                if (sumwrk?.MatlQty == null)
                {
                    continue;
                }
                usedBaseQty += sumwrk.BaseMatlQty.GetValueOrDefault();
            }

            return usedBaseQty;
        }
    }

    [Serializable]
    [PXCacheName(Messages.MatlWizardFilter)]
    public class WizFilter : IBqlTable
    {
        #region ExcludeUnreleasedBatchQty
        /// <summary>
        /// Does the wizard process need to lookup and exclude any unreleased material qty from the suggested release qty.
        /// </summary>
        public abstract class excludeUnreleasedBatchQty : PX.Data.BQL.BqlBool.Field<excludeUnreleasedBatchQty> { }

        /// <summary>
        /// Does the wizard process need to lookup and exclude any unreleased material qty from the suggested release qty.
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Remove unreleased batch qty from calculations")]
        public virtual Boolean? ExcludeUnreleasedBatchQty { get; set; }
        #endregion
    }
}