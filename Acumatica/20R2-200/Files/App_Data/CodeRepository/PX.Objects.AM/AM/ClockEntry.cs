using System;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    public class ClockEntry : PXGraph<ClockEntry, AMClockItem>
    {
        public PXSelect<AMClockItem, Where<AMClockItem.employeeID, Equal<Optional<AMClockItem.employeeID>>>> header;
        public PXSelect<AMClockTran, Where<AMClockTran.employeeID, Equal<Current<AMClockItem.employeeID>>>> transactions;
        public PXSelect<AMClockItemSplit, Where<AMClockItemSplit.employeeID, Equal<Current<AMClockItem.employeeID>>,
            And<AMClockItemSplit.lineNbr, Equal<int0>>>> splits;
        public PXSelect<AMClockTranSplit, Where<AMClockTran.employeeID, Equal<Current<AMClockTran.employeeID>>,
            And<AMClockTranSplit.lineNbr, Equal<Current<AMClockTran.lineNbr>>>>> transplits;

        public PXAction<AMClockItem> clockInOut;
        
        public LSAMClockItem lsselect;

        public PXSetup<AMPSetup> prodsetup;

        #region Events

        protected virtual void AMClockItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
             var row = (AMClockItem)e.Row;

            if (row == null)
                return;

            var prodEmployee = CheckProdEmployee(sender, row);
            EnableFields(row.IsClockedIn == true, prodEmployee);
        }

        protected virtual void AMClockItem_OrderType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = prodsetup.Current.DefaultOrderType;
        }
        
        protected virtual void AMClockItem_OperationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var tran = (AMClockItem)e.Row;
            SetDefaultShift(sender, tran);
            if (!string.IsNullOrWhiteSpace(tran?.ProdOrdID) && tran.OperationID != null)
            {
                AMProdItem prodItem = PXSelect<AMProdItem,
                    Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                        And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>
                    >>>.Select(this, tran.OrderType, tran.ProdOrdID);

                sender.SetValueExt<AMClockItem.lastOper>(tran, prodItem?.LastOperationID == tran.OperationID);
                sender.SetValueExt<AMClockItem.siteID>(tran, prodItem?.SiteID);
                sender.SetValueExt<AMClockItem.locationID>(tran, prodItem?.LocationID);
            }
        }

        protected virtual void AMClockItem_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            var row = (AMClockItem)e.Row;
            if (this.IsMobile)
            {
                if (row == null)
                    return;

                var existing = (AMClockItem)PXSelect<AMClockItem, Where<AMClockItem.employeeID, Equal<Required<AMClockItem.employeeID>>>>.Select(this, row.EmployeeID);
                if (existing == null)
                    return;

                header.Current = existing;
                e.Cancel = true;
            }
        }

        protected virtual void AMClockItem_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (prodsetup.Current.RestrictClockCurrentUser != true)
            {
                return;
            }
            var row = (AMClockItem)e.Row;
            EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, row.EmployeeID);
            if (emp == null)
                return;

            if (emp.UserID != Accessinfo.UserID)
            {
                sender.RaiseExceptionHandling("employeeID", row, null, new PXSetPropertyException(Messages.EmployeeNotCurrentUser, PXErrorLevel.Error));
            }
        }

        #endregion

        #region Buttons 
        [PXUIField(DisplayName = "Clock")]
        [PXButton()]
        public virtual IEnumerable ClockInOut(PXAdapter adapter)
        {
            try
            {
                if (header.Current.IsClockedIn == true)
                {
                    //set the status to pending approval
                    AMClockItem item = header.Current;
                    item.EndTime = Common.Current.BusinessTimeOfDay(this);
                    int laborTime = ClockTimeAttribute.GetTimeBetween(item.StartTime, item.EndTime);
                    if (laborTime > 0)
                    {
                        AMClockTran tran = (AMClockTran)transactions.Insert();
                        CopyTran(item, tran);
                        tran.Released = true;
                        tran.LaborTime = laborTime;
                        foreach (AMClockItemSplit split in PXParentAttribute.SelectChildren(splits.Cache, item, typeof(AMClockItem)))
                        {
                            AMClockTranSplit newSplit = (AMClockTranSplit)transplits.Insert();
                            CopySplit(split, newSplit);
                            newSplit.Released = true;                          
                        }
                    }
                    ClearHeader(item);
                }
                else
                {
                    AMClockItem item = (AMClockItem)header.Cache.CreateCopy(header.Current);
                    item.StartTime = Common.Current.BusinessTimeOfDay(this);
                    item.TranDate = Common.Current.BusinessDate(this);
                    header.Cache.Update(item);
                }
                Actions.PressSave();
                return Actions["Cancel"].Press(adapter);
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw;
            }
        }

        public PXAction<AMClockItem> fillCurrentUser;
        [PXUIField(DisplayName = "Current User")]
        [PXButton()]        
        public virtual IEnumerable FillCurrentUser(PXAdapter adapter)
        {
            var emp = (EPEmployee)PXSelect <EPEmployee,
                    Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
            if (emp == null)
                return adapter.Get();

            header.Cache.Clear();

            return header.Select(emp.BAccountID);
        }

        #endregion

        #region Methods

        protected virtual void ClearHeader(AMClockItem item)
        {
            item.Qty = 0;
            item.StartTime = null;
            item.EndTime = null;
            header.Update(item);
            foreach (AMClockItemSplit split in PXParentAttribute.SelectChildren(splits.Cache, item, typeof(AMClockItem)))
            {
                var status = splits.Cache.GetStatus(split);
                if (status == PXEntryStatus.Inserted)
                    splits.Cache.Remove(split);
                else
                    splits.Cache.Delete(split);
            }
        }

        protected virtual void CopyTran(AMClockItem item, AMClockTran tran)
        {
            tran.EmployeeID = item.EmployeeID;
            tran.OrderType = item.OrderType;
            tran.ProdOrdID = item.ProdOrdID;
            tran.OperationID = item.OperationID;
            tran.ShiftID = item.ShiftID;
            tran.Qty = item.Qty;
            tran.BaseQty = item.BaseQty;
            tran.UOM = item.UOM;
            tran.InventoryID = item.InventoryID;
            tran.SiteID = item.SiteID;
            tran.StartTime = item.StartTime;
            tran.EndTime = item.EndTime;
            tran.LocationID = item.LocationID;
            tran.InvtMult = item.InvtMult;
            tran.LastOper = item.LastOper;
            tran.TranDate = item.TranDate;
        }

        protected virtual void CopySplit(AMClockItemSplit split, AMClockTranSplit newSplit)
        {
            newSplit.Qty = split.Qty;
            newSplit.LotSerialNbr = split.LotSerialNbr;
            newSplit.LocationID = split.LocationID;
            newSplit.InventoryID = split.InventoryID;
            newSplit.TranType = split.TranType;
            newSplit.UOM = split.UOM;
            newSplit.SiteID = split.SiteID;
            newSplit.SubItemID = split.SubItemID;
            newSplit.InvtMult = split.InvtMult;
            newSplit.EmployeeID = split.EmployeeID;
            newSplit.TranDate = split.TranDate;
        }


        protected virtual void EnableFields(bool clockedIn, bool prodEmployee)
        {
            this.Actions["LSAMClockItem_binLotSerial"].SetEnabled(clockedIn && header.Current.Qty > 0 && header.Current.LastOper == true && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.employeeID>(header.Cache, null, prodsetup.Current.RestrictClockCurrentUser == false || IsMobile);
            PXUIFieldAttribute.SetEnabled<AMClockItem.orderType>(header.Cache, null, !clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.prodOrdID>(header.Cache, null, !clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.operationID>(header.Cache, null, !clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.shiftID>(header.Cache, null, !clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.qty>(header.Cache, null, clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.uOM>(header.Cache, null, clockedIn && prodEmployee);
            this.clockInOut.SetCaption(clockedIn ? Messages.ClockOut : Messages.ClockIn);
            clockInOut.SetEnabled(prodEmployee);
            Save.SetEnabled(clockedIn && prodEmployee);
            transactions.AllowInsert = false;
            transactions.AllowUpdate = false;
            transactions.AllowDelete = false;
        }

        protected virtual void SetDefaultShift(PXCache sender, AMClockItem row)
        {
            if (row?.OperationID == null || row.ProdOrdID == null)
            {
                return;
            }

            PXResultset<AMShift> result = PXSelectJoin<AMShift,
                    InnerJoin<AMProdOper, On<AMShift.wcID, Equal<AMProdOper.wcID>>>,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
                .Select(this, row.OrderType, row.ProdOrdID, row.OperationID);

            if (result == null || result.Count != 1)
            {
                return;
            }

            sender.SetValueExt<AMClockItem.shiftID>(row, ((AMShift)result[0])?.ShiftID);
        }


        protected virtual bool CheckProdEmployee(PXCache sender, AMClockItem row)
        {
            EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, row.EmployeeID);
            if (emp == null)
                return false;
            var ext = emp.GetExtension<EPEmployeeExt>();
            if (ext == null)
                return false;

            if(ext.AMProductionEmployee == false)
            {
                sender.RaiseExceptionHandling("employeeID", row, null, new PXSetPropertyException(Messages.EmployeeNotProduction, PXErrorLevel.Error));
            }
            return (ext.AMProductionEmployee == true);
        }

        #endregion
    }
}