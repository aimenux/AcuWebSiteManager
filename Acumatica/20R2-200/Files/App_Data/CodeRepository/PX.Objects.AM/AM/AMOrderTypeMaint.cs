using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Order Type Maintenance graph
    /// </summary>
    public class AMOrderTypeMaint : PXGraph<AMOrderTypeMaint, AMOrderType>
    {
        public PXSelect<AMOrderType> OrderType;

        public PXSelect<AMOrderType, Where<AMOrderType.orderType, Equal<Current<AMOrderType.orderType>>>>
            CurrentOrderType;

        [PXImport(typeof(AMOrderType))]
        public PXSelect<AMOrderTypeAttribute, Where<AMOrderTypeAttribute.orderType, Equal<Current<AMOrderType.orderType>>
            >> OrderTypeAttributes;

        public PXSetup<AMPSetup> Setup;

        public AMOrderTypeMaint()
        {
            var setup = Setup.Current;
            AMPSetup.CheckSetup(setup);

            // Set Scrap Warehouse and Location Visibility
            PXUIFieldAttribute.SetVisible<AMOrderType.scrapSource>(CurrentOrderType.Cache, null,
                PXAccess.FeatureInstalled<FeaturesSet.warehouse>());
            PXUIFieldAttribute.SetVisible<AMOrderType.scrapSiteID>(CurrentOrderType.Cache, null,
                PXAccess.FeatureInstalled<FeaturesSet.warehouse>());
            PXUIFieldAttribute.SetVisible<AMOrderType.scrapLocationID>(CurrentOrderType.Cache, null,
                PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>());
        }

        protected virtual void AMOrderType_OverIssueMaterial_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMOrderType) e.Row;
            if (row == null)
            {
                return;
            }

            if (row.OverIssueMaterial == SetupMessage.AllowMsg &&
                row.IncludeUnreleasedOverIssueMaterial.GetValueOrDefault())
            {
                sender.SetValueExt<AMOrderType.includeUnreleasedOverIssueMaterial>(row, false);
            }
        }

        protected virtual void AMOrderType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var amOrderType = (AMOrderType) e.Row;

            if (amOrderType == null)
            {
                return;
            }

            AMProdItem amProdItem =
                PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>
                >>.SelectWindowed(this, 0, 1, amOrderType.OrderType);
            if (amProdItem != null)
            {
                throw new PXException(Messages.GetLocal(Messages.OrderTypeInUseCannotBeDeleted, amOrderType.OrderType));
            }

            AMPSetup ampSetup = PXSelect<AMPSetup>.Select(this);
            if (ampSetup != null && ampSetup.DefaultOrderType.EqualsWithTrim(amOrderType.OrderType))
            {
                throw new PXException(
                    GetOrderTypeInUseExceptionMessage(amOrderType, nameof(AMPSetup.DefaultOrderType), typeof(AMPSetup)));
            }

            AMEstimateSetup amEstimateSetup = PXSelect<AMEstimateSetup>.Select(this);
            if (amEstimateSetup != null && amEstimateSetup.DefaultOrderType.EqualsWithTrim(amOrderType.OrderType))
            {
                throw new PXException(GetOrderTypeInUseExceptionMessage(amOrderType, nameof(AMEstimateSetup.DefaultOrderType), typeof(AMEstimateSetup)));
            }

            AMRPSetup mrpSetup = PXSelect<AMRPSetup>.Select(this);
            if (mrpSetup != null && mrpSetup.PlanOrderType.EqualsWithTrim(amOrderType.OrderType))
            {
                throw new PXException(GetOrderTypeInUseExceptionMessage(amOrderType, nameof(AMRPSetup.PlanOrderType), typeof(AMRPSetup)));
            }
        }

        protected virtual string GetOrderTypeInUseExceptionMessage(AMOrderType amOrderType, string fieldName, System.Type dacType)
        {
            if (string.IsNullOrWhiteSpace(amOrderType?.OrderType))
            {
                throw new PXArgumentException(nameof(amOrderType));
            }

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new PXArgumentException(nameof(fieldName));
            }

            return Messages.GetLocal(Messages.OrderTypeIsReferenced,
                amOrderType.OrderType.TrimIfNotNullEmpty(),
                PXUIFieldAttribute.GetDisplayName(Caches[dacType], fieldName),
                Common.Cache.GetCacheName(dacType)
            );
        }

        protected virtual void AMOrderType_Active_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMOrderType) e.Row;

            if (row == null || e.NewValue == null)
            {
                return;
            }

            bool newValue = (bool) (e.NewValue ?? false);

            if (!newValue && row.Active.GetValueOrDefault())
            {
                AMProdItem amProdItem =
                    PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                            And<AMProdItem.statusID, NotEqual<Required<AMProdItem.statusID>>,
                                And<AMProdItem.statusID, NotEqual<Required<AMProdItem.statusID>>>>>
                    >.SelectWindowed(this, 0, 1, row.OrderType, ProductionOrderStatus.Cancel, ProductionOrderStatus.Closed);

                if (amProdItem != null)
                {
                    // Restrict a user from making an Order Type inactive when used in Open orders
                    e.NewValue = row.Active.GetValueOrDefault();
                    e.Cancel = true;
                    throw new PXSetPropertyException(Messages.GetLocal(Messages.CannotMakeOrderTypeInactiveOpenOrders),
                        PXErrorLevel.Error, row.OrderType);
                }

                AMPSetup ampSetup = PXSelect<AMPSetup>.Select(this);
                if (ampSetup != null && ampSetup.DefaultOrderType.EqualsWithTrim(row.OrderType))
                {
                    // Restrict a user from making an Order Type inactive when it is the default Production order Type
                    e.NewValue = row.Active.GetValueOrDefault();
                    e.Cancel = true;
                    throw new PXSetPropertyException(
                        GetOrderTypeInUseExceptionMessage(row, "DefaultOrderType", typeof(AMPSetup)), PXErrorLevel.Error);
                }

                AMEstimateSetup amEstimateSetup = PXSelect<AMEstimateSetup>.Select(this);
                if (amEstimateSetup != null && amEstimateSetup.DefaultOrderType.EqualsWithTrim(row.OrderType))
                {
                    // Restrict a user from making an Order Type inactive when it is the default Estimate order Type
                    e.NewValue = row.Active.GetValueOrDefault();
                    e.Cancel = true;
                    throw new PXSetPropertyException(
                        GetOrderTypeInUseExceptionMessage(row, "DefaultOrderType", typeof(AMEstimateSetup)), PXErrorLevel.Error);
                }

                AMRPSetup mrpSetup = PXSelect<AMRPSetup>.Select(this);
                if (mrpSetup != null && mrpSetup.PlanOrderType.EqualsWithTrim(row.OrderType))
                {
                    e.NewValue = row.Active.GetValueOrDefault();
                    e.Cancel = true;
                    throw new PXException(
                        GetOrderTypeInUseExceptionMessage(row, "PlanOrderType", typeof(AMRPSetup)));
                }
            }
        }

        protected virtual void AMOrderType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var aMOrderType = (AMOrderType) e.Row;
            if (aMOrderType == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMOrderType.defaultCostMethod>(sender, e.Row, aMOrderType.Function != OrderTypeFunction.Disassemble);
            PXUIFieldAttribute.SetEnabled<AMOrderType.includeUnreleasedOverIssueMaterial>(sender, e.Row, aMOrderType.OverIssueMaterial != SetupMessage.AllowMsg);

            // Set Visiblity for Data Entry Settings
            PXUIFieldAttribute.SetVisible<AMOrderType.underIssueMaterial>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Planning && aMOrderType.Function != OrderTypeFunction.Disassemble);
            PXUIFieldAttribute.SetVisible<AMOrderType.backflushUnderIssueMaterial>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Planning && aMOrderType.Function != OrderTypeFunction.Disassemble);
            PXUIFieldAttribute.SetVisible<AMOrderType.overIssueMaterial>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Planning);
            PXUIFieldAttribute.SetVisible<AMOrderType.includeUnreleasedOverIssueMaterial>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Planning);
            PXUIFieldAttribute.SetVisible<AMOrderType.issueMaterialOnTheFly>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Planning);
            PXUIFieldAttribute.SetVisible<AMOrderType.moveCompletedOrders>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Planning);
            PXUIFieldAttribute.SetVisible<AMOrderType.overCompleteOrders>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Planning);
            PXUIFieldAttribute.SetVisible<AMOrderType.defaultOperationMoveQty>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Planning);
            PXUIFieldAttribute.SetVisible<AMOrderType.scrapSource>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Disassemble);
            PXUIFieldAttribute.SetVisible<AMOrderType.scrapSiteID>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Disassemble);
            PXUIFieldAttribute.SetVisible<AMOrderType.scrapLocationID>(CurrentOrderType.Cache, null,
                aMOrderType.Function != OrderTypeFunction.Disassemble);
        }

        protected virtual void AMOrderType_Function_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var aMOrderType = (AMOrderType) e.Row;
            if (aMOrderType?.Function == null)
            {
                return;
            }

            if (aMOrderType.Function == OrderTypeFunction.Disassemble)
            {
                aMOrderType.DefaultCostMethod = CostMethod.Actual;
            }
        }

        protected virtual void AMOrderTypeAttribute_AttributeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMOrderTypeAttribute)e.Row;
            if (row == null)
            {
                return;
            }

            var item = (CSAttribute)PXSelectorAttribute.Select<AMOrderTypeAttribute.attributeID>(sender, row);
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.Label))
            {
                sender.SetValueExt<AMOrderTypeAttribute.label>(row, item.AttributeID);
            }
            if (string.IsNullOrWhiteSpace(row.Descr))
            {
                sender.SetValueExt<AMOrderTypeAttribute.descr>(row, item.Description);
            }

        }
    }
}
