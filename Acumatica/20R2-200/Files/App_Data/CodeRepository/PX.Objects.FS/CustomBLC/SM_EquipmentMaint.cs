using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    public class SM_EquipmentMaint : PXGraphExtension<EquipmentMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>();
        }

        #region Private Methods
        private FSEquipment GetRelatedFSEquipmentRow(PXGraph graph)
        {
            return PXSelect<FSEquipment,
                   Where<
                       FSEquipment.sourceID, Equal<Current<EPEquipment.equipmentID>>,
                   And<
                       Where<
                           FSEquipment.sourceType, Equal<FSEquipment.sourceType.EP_Equipment>,
                           Or<FSEquipment.sourceType, Equal<FSEquipment.sourceType.Vehicle>>>>>>
                   .Select(graph);
        }
        #endregion

        #region Actions
        #region ExtendToSMEquipment
        public PXAction<EPEquipment> extendToSMEquipment;
        [PXUIField(DisplayName = "Extend to SM Equipment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        public virtual void ExtendToSMEquipment()
        {
            SMEquipmentMaint graphSMEquipmentMaint = PXGraph.CreateInstance<SMEquipmentMaint>();
            FSEquipment fsEquipmentRow = new FSEquipment();

            fsEquipmentRow.SourceID = Base.Equipment.Current.EquipmentID;
            fsEquipmentRow.SourceRefNbr = Base.Equipment.Current.EquipmentCD;
            fsEquipmentRow.SourceType = ID.SourceType_Equipment.EP_EQUIPMENT;

            fsEquipmentRow.RequireMaintenance = false;
            fsEquipmentRow.ResourceEquipment = true;

            graphSMEquipmentMaint.EquipmentRecords.Current = graphSMEquipmentMaint.EquipmentRecords.Insert(fsEquipmentRow);
            EquipmentHelper.UpdateFSEquipmentWithEPEquipment(graphSMEquipmentMaint.EquipmentRecords.Cache, graphSMEquipmentMaint.EquipmentRecords.Current, Base.Equipment.Cache, Base.Equipment.Current);
            EquipmentHelper.SetDefaultValuesFromFixedAsset(graphSMEquipmentMaint.EquipmentRecords.Cache, graphSMEquipmentMaint.EquipmentRecords.Current, Base.Equipment.Current.FixedAssetID);

            throw new PXRedirectRequiredException(graphSMEquipmentMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion
        #region ViewInSMEquipment
        public PXAction<EPEquipment> viewInSMEquipment;
        [PXUIField(DisplayName = "View in SM Equipment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        public virtual void ViewInSMEquipment()
        {
            FSEquipment fsEquipmentRow = GetRelatedFSEquipmentRow(Base);

            if (fsEquipmentRow == null)
            {
                return;
            }

            switch (fsEquipmentRow.SourceType)
            {
                case ID.SourceType_Equipment.VEHICLE:
                    VehicleMaint graphVehicleMaint = PXGraph.CreateInstance<VehicleMaint>();

                    graphVehicleMaint.EPEquipmentRecords.Current = graphVehicleMaint.EPEquipmentRecords.Search<EPEquipment.equipmentCD>(fsEquipmentRow.SourceRefNbr);

                    throw new PXRedirectRequiredException(graphVehicleMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };

                default:
                    SMEquipmentMaint graphSMEquipmentMaint = PXGraph.CreateInstance<SMEquipmentMaint>();

                    graphSMEquipmentMaint.EquipmentRecords.Current = graphSMEquipmentMaint.EquipmentRecords.Search<FSEquipment.refNbr>(fsEquipmentRow.RefNbr);

                    throw new PXRedirectRequiredException(graphSMEquipmentMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #endregion

        #region Events

        #region EPEquipment
        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowSelected<EPEquipment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEquipment epEquipmentRow = (EPEquipment)e.Row;

            FSEquipment fsEquipmentRow = GetRelatedFSEquipmentRow(e.Cache.Graph);

            extendToSMEquipment.SetEnabled(e.Cache.GetStatus(epEquipmentRow) != PXEntryStatus.Inserted && fsEquipmentRow == null);
            viewInSMEquipment.SetEnabled(fsEquipmentRow != null);
        }

        protected virtual void _(Events.RowInserting<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowInserted<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowUpdating<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowUpdated<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowDeleting<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowDeleted<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowPersisting<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowPersisted<EPEquipment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                EPEquipment epEquipmentRow = (EPEquipment)e.Row;
                FSEquipment fsEquipmentRow = GetRelatedFSEquipmentRow(e.Cache.Graph);

                if (fsEquipmentRow != null)
                {
                    PXCache<FSEquipment> cacheFSEquipment = new PXCache<FSEquipment>(Base);

                    // This is to prevent an error on the FSEquipment cache trying to change a common field (status, description)
                    // after extending an equipment to FSEquipment.
                    cacheFSEquipment.Graph.SelectTimeStamp();

                    if (EquipmentHelper.UpdateFSEquipmentWithEPEquipment(cacheFSEquipment, fsEquipmentRow, e.Cache, epEquipmentRow))
                    {
                        cacheFSEquipment.Update(fsEquipmentRow);
                        cacheFSEquipment.Persist(PXDBOperation.Update);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
