using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.SO;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
    [PXCopyPasteHiddenFields(
        typeof(SOLineExt.aMOrderType),
        typeof(SOLineExt.aMProdOrdID),
        typeof(SOLineExt.aMEstimateID),
        typeof(SOLineExt.aMEstimateRevisionID),
        typeof(SOLineExt.aMProdQtyComplete),
        typeof(SOLineExt.aMProdBaseQtyComplete))]
    [Serializable]
    public sealed class SOLineExt : PXCacheExtension<SOLine>, IMfgConfigOrderLineExtension
    {
        // Developer note: new fields added here should also be added to SOLineMfgOnly
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMSelected
        public abstract class aMSelected : PX.Data.BQL.BqlBool.Field<aMSelected> { }
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public bool? AMSelected { get; set; }
        #endregion
        #region AMProdCreate
        public abstract class aMProdCreate : PX.Data.BQL.BqlBool.Field<aMProdCreate> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Mark for Production")]
        public Boolean? AMProdCreate { get; set; }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXUIField(DisplayName = "Prod. Order Type", Enabled = false)]
        [PXRestrictor(typeof(Where<AMOrderType.function, Equal<OrderTypeFunction.regular>>), PX.Objects.AM.Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public string AMOrderType { get; set; }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }

        [ProductionNbr]
        [ProductionOrderSelector(typeof(SOLineExt.aMOrderType), includeAll: true, ValidateValue = false)]
        public string AMProdOrdID { get; set; }
        #endregion
        #region AMProdQtyComplete
        public abstract class aMProdQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdQtyComplete> { }

        [PXUIField(DisplayName = "Production Qty Complete", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBQuantity(typeof(SOLine.uOM), typeof(SOLineExt.aMProdBaseQtyComplete), HandleEmptyKey = true)]
        public Decimal? AMProdQtyComplete { get; set; }
        #endregion
        #region AMProdBaseQtyComplete
        public abstract class aMProdBaseQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdBaseQtyComplete> { }

        [PXDBQuantity]
        [PXUIField(DisplayName = "Production Base Qty Complete", Enabled = false, Visible = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMProdBaseQtyComplete { get; set; }
        #endregion
        #region AMProdStatusID
        public abstract class aMProdStatusID : PX.Data.BQL.BqlString.Field<aMProdStatusID> { }

        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Production Status", Enabled = false)]
        [ProductionOrderStatus.List]
        public String AMProdStatusID { get; set; }
        #endregion

        #region AMEstimateID
        public abstract class aMEstimateID : PX.Data.BQL.BqlString.Field<aMEstimateID> { }

        [EstimateID(Enabled = false)]
        [EstimateIDSelectAll]
        public string AMEstimateID { get; set; }
        #endregion
        #region AMEstimateRevisionID
        public abstract class aMEstimateRevisionID : PX.Data.BQL.BqlString.Field<aMEstimateRevisionID> { }

        [RevisionIDField(DisplayName = "Est. Revision", Enabled = false, FieldClass = Features.ESTIMATINGFIELDCLASS)]
        [PXSelector(typeof(Search<AMEstimateItem.revisionID, Where<AMEstimateItem.estimateID, Equal<Current<SOLineExt.aMEstimateID>>>>))]
        public string AMEstimateRevisionID { get; set; }
        #endregion

        #region AMConfigurationID
        public abstract class aMConfigurationID : PX.Data.BQL.BqlString.Field<aMConfigurationID> { }
        [PXDBString]
        [PXUIField(DisplayName = "Configuration ID", Enabled = false, Visible = false, FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        public string AMConfigurationID { get; set; }

        #endregion
        #region AMParentLineNbr
        public abstract class aMParentLineNbr : PX.Data.BQL.BqlInt.Field<aMParentLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Parent Line Nbr.", Visible = false, Enabled = false, FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        public Int32? AMParentLineNbr { get; set; }
        #endregion
        #region AMIsSupplemental
        public abstract class aMIsSupplemental : PX.Data.BQL.BqlBool.Field<aMIsSupplemental> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Is Supplemental", Visible = false, Enabled = false, FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        public Boolean? AMIsSupplemental { get; set; }
        #endregion
        #region AMOrigParentLineNbr
        public abstract class aMOrigParentLineNbr : PX.Data.BQL.BqlInt.Field<aMOrigParentLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Orig Parent Line Nbr.", Visible = false, Enabled = false, FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        public Int32? AMOrigParentLineNbr { get; set; }
        #endregion
        #region AMConfigKeyID
        /// <summary>
        /// Configuration key ID which represents the key used/generated from the results of a finished configuration
        /// </summary>
        public abstract class aMConfigKeyID : PX.Data.BQL.BqlString.Field<aMConfigKeyID> { }

        /// <summary>
        /// Configuration key ID which represents the key used/generated from the results of a finished configuration
        /// </summary>
        [PXDBString(120, IsUnicode = true)]
        [PXUIField(DisplayName = "Config. Key", FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        [PXSelector(typeof(Search<AMConfigurationKeys.keyID,
                Where<AMConfigurationKeys.configurationID, Equal<Current<SOLineExt.aMConfigurationID>>>>),
            typeof(AMConfigurationKeys.keyID),
            typeof(AMConfigurationKeys.keyDescription),
            typeof(AMConfigurationKeys.createdDateTime),
            DescriptionField = typeof(AMConfigurationResults.keyDescription),
            ValidateValue = false)]
        public string AMConfigKeyID { get; set; }
        #endregion

        #region IsOrderTypeConfigurable
        public abstract class isOrderTypeConfigurable : PX.Data.BQL.BqlBool.Field<isOrderTypeConfigurable> { }
        [PXBool]
        [PXUIField(DisplayName = "Is Order Type Configurable", Enabled = false, FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        [PXFormula(typeof(Selector<SOLine.orderType, SOOrderTypeExt.aMConfigurationEntry>))]
        public bool? IsOrderTypeConfigurable
        {
            get;
            set;
        }
        #endregion

        #region IsConfigurable
        public abstract class isConfigurable : PX.Data.BQL.BqlBool.Field<isConfigurable> { }
        [PXBool]
        [PXUIField(DisplayName = "Configurable", Enabled = false, FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        [PXDependsOnFields(typeof(SOLineExt.aMConfigurationID), typeof(SOLineExt.isOrderTypeConfigurable))]
        public bool? IsConfigurable => !string.IsNullOrEmpty(AMConfigurationID) && IsOrderTypeConfigurable.GetValueOrDefault();

        #endregion

        #region READ ONLY FIELDS

        #region AMQtyReadOnly

        /// <summary>
        /// Read only sales order line quantity
        /// </summary>
        public abstract class aMQtyReadOnly : PX.Data.BQL.BqlDecimal.Field<aMQtyReadOnly> { }

        /// <summary>
        /// Read only sales order line quantity
        /// </summary>
        [PXUIField(DisplayName = "Quantity", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        [PXDecimal]
        public decimal? AMQtyReadOnly
        {
            get { return Base?.Qty; }
            set { }
        }

        #endregion
        #region AMUOMReadOnly
        /// <summary>
        /// Read only sales order line UOM
        /// </summary>
        public abstract class aMUOMReadOnly : PX.Data.BQL.BqlString.Field<aMUOMReadOnly> { }

        /// <summary>
        /// Read only sales order line UOM
        /// </summary>
        [PXString]
        [PXUIField(DisplayName = "UOM", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public String AMUOMReadOnly
        {
            get { return Base?.UOM; }
            set { }
        }
        #endregion

        #endregion
    }
}