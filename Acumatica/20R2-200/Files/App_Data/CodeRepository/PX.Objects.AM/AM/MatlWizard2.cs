using System;
using PX.Data;
using System.Collections.Generic;
using System.Collections;
using PX.Objects.Common.Extensions;

namespace PX.Objects.AM
{
    public class MatlWizard2 : PXGraph<MatlWizard2>
    {
        public PXProcessing<AMWrkMatl, Where<AMWrkMatl.userID, Equal<Current<AccessInfo.userID>>>> ProcessMatl;

        // Turn off the new UI processing window (19R1+)
        public override bool IsProcessing => false;

        public PXAction<AMWrkMatl> cancel;
        [PXUIField(DisplayName = "Cancel")]
        [PXButton]
        public virtual IEnumerable Cancel(PXAdapter adapter)
        {
            DeleteWrkTableRecs(this.Accessinfo.UserID);
            var rm = PXGraph.CreateInstance<MaterialEntry>();
            rm.Clear();
            throw new PXRedirectRequiredException(rm, string.Empty);
        }

        public MatlWizard2()
        {
            ProcessMatl.SetProcessDelegate(BuildMaterialTransaction);
            ProcessMatl.SetProcessCaption(Messages.Select);
            ProcessMatl.SetProcessAllCaption(Messages.SelectAll);
            PXUIFieldAttribute.SetEnabled<AMWrkMatl.uOM>(ProcessMatl.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMWrkMatl.matlQty>(ProcessMatl.Cache, null, true);
        }

        protected static void DeleteWrkTableRecs(Guid userID)
        {
            PXDatabase.Delete<AMWrkMatl>(new PXDataFieldRestrict<AMWrkMatl.userID>(userID));
        }

        public static void BuildMaterialTransaction(List<AMWrkMatl> list)
        {
            var rm = CreateInstance<MaterialEntry>();
            rm.Clear();
            var origIsImport = rm.IsImport;
            rm.IsImport = true;
            rm.IsInternalCall = true;

            if (rm.ampsetup.Current != null)
            {
                rm.ampsetup.Current.HoldEntry = true;
            }

            try
            {
                var matlbuilder = new MaterialTranBuilder(rm)
                {
                    StatusByLotSerial = true
                };
                var ammTrans = new List<AMMTran>();
                foreach (var wrkMatl in list)
                {
                    var trans = matlbuilder.BuildTransactions(
                        ToAMProdMatl(wrkMatl), 
                        wrkMatl.MatlQty.GetValueOrDefault(),
                        wrkMatl.UOM,
                        wrkMatl.SiteID,
                        wrkMatl.LocationID,
                        out var _);

                    if (trans != null && trans.Count > 0)
                    {
                        ammTrans.AddRange(trans);
                    }
                }

                MaterialTranBuilder.CreateMaterialTransaction(rm, ammTrans, null);
                
                rm.Persist();
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw new PXException(Messages.MaterialWizardBatchCreationError);
            }

            DeleteWrkTableRecs(rm.Accessinfo.UserID);

            rm.IsImport = origIsImport;
            rm.IsInternalCall = false;
            throw new PXRedirectRequiredException(rm, string.Empty);
        }

        protected static AMProdMatl ToAMProdMatl(AMWrkMatl wrkMatl)
        {
            var baseQty = Math.Abs(wrkMatl.BaseQtyReq.GetValueOrDefault()) * (wrkMatl.IsByproduct.GetValueOrDefault() ? -1 : 1);
            var qty = Math.Abs(wrkMatl.MatlQty.GetValueOrDefault()) * (wrkMatl.IsByproduct.GetValueOrDefault() ? -1 : 1);
            return new AMProdMatl
            {
                // there is no BaseQtyReq in AMProdMatl. BaseQty is linked to BaseQtyRemaining
                QtyReq = qty,
                BaseQty = baseQty,
                Qty = qty,
                // Need some values in the total fields for other calculations to work
                BaseTotalQtyRequired = baseQty,
                TotalQtyRequired = qty,
                OrderType = wrkMatl.OrderType,
                ProdOrdID = wrkMatl.ProdOrdID,
                OperationID = wrkMatl.OperationID,
                LineID = wrkMatl.LineID,
                SortOrder = wrkMatl.LineID,
                InventoryID = wrkMatl.InventoryID,
                SubItemID = wrkMatl.SubItemID,
                SiteID = wrkMatl.SiteID,
                LocationID = wrkMatl.LocationID,
                Descr = wrkMatl.Descr,
                UOM = wrkMatl.UOM,
                BFlush = wrkMatl.BFlush,
                SubcontractSource = wrkMatl.SubcontractSource
            };
        }

        protected virtual void AMWrkMatl_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            AMWrkMatl amWrkMatl = (AMWrkMatl) e.Row;
            if (amWrkMatl == null)
            {
                return;
            }

            decimal avilQtyInBaseUnits = InventoryHelper.GetQtyAvail(sender.Graph, amWrkMatl.InventoryID, amWrkMatl.SubItemID, amWrkMatl.SiteID, amWrkMatl.LocationID, null, null, true, true);
            
            // Now convert the required quantity to current UOM
            decimal? availQtyInProdUnits = 0;
            if (UomHelper.TryConvertFromBaseQty<AMWrkMatl.inventoryID>(sender, amWrkMatl,
                amWrkMatl.UOM,
                avilQtyInBaseUnits,
                out availQtyInProdUnits))
            {
                amWrkMatl.QtyAvail = availQtyInProdUnits;
            }
        }

        protected virtual void _(Events.FieldVerifying<AMWrkMatl, AMWrkMatl.matlQty> e)
        {
            if (IsMaterialQtyAllowed(e.Row, (decimal?)e.NewValue, out var ex))
            {
                e.Cache.ClearFieldErrors<AMWrkMatl.matlQty>(e.Row);
                return;
            }

            if(ex.ErrorLevel == PXErrorLevel.Error)
            {
                e.Cancel = true;
                e.NewValue = e.Row?.MatlQty;
            }

            e.Cache.RaiseExceptionHandling<AMWrkMatl.matlQty>(e.Row, e.Row?.MatlQty, ex);
        }

        protected virtual void _(Events.RowSelected<AMWrkMatl> e)
        {
            if (!IsMaterialQtyAllowed(e.Row, e.Row?.MatlQty, out var ex) && ex?.ErrorLevel == PXErrorLevel.Warning)
            {
                e.Cache.RaiseExceptionHandling<AMWrkMatl.matlQty>(e.Row, e.Row.MatlQty, ex);
            }
        }

        protected virtual bool IsMaterialQtyAllowed(AMWrkMatl row, decimal? qty, out PXSetPropertyException exceptionMsg)
        {
            exceptionMsg = null;
            if(row == null)
            {
                return true;
            }

            // UnreleasedBatchQty only set if settings require this field to lookup such qty
            var maxQtyToIssue = (row.QtyRemaining.GetValueOrDefault() - row.UnreleasedBatchQty.GetValueOrDefault()).NotLessZero();
            if (row == null || row.OverIssueMaterial == Attributes.SetupMessage.AllowMsg || qty.GetValueOrDefault() <= maxQtyToIssue)
            {
                return true;
            }

            exceptionMsg = new PXSetPropertyException(Messages.GetLocal(Messages.MaterialQuantityOverIssueShortMsg, 
                row.UOM, qty.GetValueOrDefault(), maxQtyToIssue), row.OverIssueMaterial == Attributes.SetupMessage.WarningMsg ? PXErrorLevel.Warning : PXErrorLevel.Error);

            return false;
        }
    }
}