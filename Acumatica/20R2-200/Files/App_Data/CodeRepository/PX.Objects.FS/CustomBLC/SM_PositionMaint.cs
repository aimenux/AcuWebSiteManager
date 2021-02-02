using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    public class SM_PositionMaint : PXGraphExtension<PositionMaint>
    {
        [PXHidden]
        public PXSelect<EPEmployee> Employee;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Event Handlers

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<EPPosition, FSxEPPosition.sDEnabled> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPPosition epPositionRow = (EPPosition)e.Row;
            FSxEPPosition fsxEPPositionRow = e.Cache.GetExtension<FSxEPPosition>(epPositionRow);

            fsxEPPositionRow.SDEnabledModified = true;
        }
        #endregion

        protected virtual void _(Events.RowSelecting<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowSelected<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowInserting<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowInserted<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowUpdating<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowUpdated<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowDeleting<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowDeleted<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowPersisting<EPPosition> e)
        {
        }

        protected virtual void _(Events.RowPersisted<EPPosition> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                EPPosition epPositionRow = (EPPosition)e.Row;
                FSxEPPosition fsxEPPositionRow = e.Cache.GetExtension<FSxEPPosition>(epPositionRow);

                if (fsxEPPositionRow.SDEnabledModified == true)
                {
                    WebDialogResult confirmResult = Base.EPPosition.Ask(TX.WebDialogTitles.POSITION_PROPAGATE_CONFIRM, TX.Messages.POSITION_PROPAGATE_CONFIRM, MessageButtons.YesNo);
                    if (confirmResult == WebDialogResult.Yes)
                    {
                        Employee.Cache.Clear();

                        var epEmployeeSet = PXSelectJoin<EPEmployee,
                                            InnerJoin<EPEmployeePosition,
                                            On<
                                                EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>>>,
                                            Where<
                                                EPEmployeePosition.positionID, Equal<Required<EPEmployeePosition.positionID>>,
                                                And<EPEmployeePosition.isActive, Equal<True>>>>
                                            .Select(Base, epPositionRow.PositionID);

                        foreach (EPEmployee epEmployeeRow in epEmployeeSet)
                        {
                            var fsxEPEmployeeRow = Employee.Cache.GetExtension<FSxEPEmployee>(epEmployeeRow);
                            if (fsxEPEmployeeRow != null)
                            {
                                fsxEPEmployeeRow.SDEnabled = fsxEPPositionRow.SDEnabled;
                                Employee.Cache.Update(epEmployeeRow);
                                Employee.Cache.SetStatus(epEmployeeRow, PXEntryStatus.Updated);
                            }
                        }


                        Employee.Cache.Persist(PXDBOperation.Insert | PXDBOperation.Update);
                    }
                }
            }
        }

        #endregion
    }
}
