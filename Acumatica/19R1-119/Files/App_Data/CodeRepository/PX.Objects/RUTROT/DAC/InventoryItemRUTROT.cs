using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.RUTROT
{
	[Serializable]

	public partial class InventoryItemRUTROT : PXCacheExtension<InventoryItem>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region IsRUTROTDeductible
		public abstract class isRUTROTDeductible : PX.Data.BQL.BqlBool.Field<isRUTROTDeductible> { }

		/// <summary>
		/// Indicates whether the item is subjected to RUT or ROT deductions.
		/// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">RUT and ROT Deduction</see> feature is enabled.
		/// </summary>
		/// <value>
		/// The value of this field is used to set default value of the <see cref="ARTranRUTROT.IsRUTROTDeductible">ARTran.IsRUTROTDeductible</see> field
		/// for the lines of a <see cref="ARInvoiceRUTROT.IsRUTROTDeductible">RUT or ROT deductible</see> Accounts Receivable Invoice.
		/// </value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "ROT or RUT Deductible Item", FieldClass = RUTROTMessages.FieldClass)]
		public virtual bool? IsRUTROTDeductible
		{
			get;
			set;
		}
		#endregion
		#region RUTROTType
		public abstract class rUTROTType : PX.Data.BQL.BqlString.Field<rUTROTType> { }
		/// <summary>
		/// The type of deduction (RUT or ROT).
		/// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">RUT and ROT Deduction</see> feature is enabled.
		/// </summary>
		/// <value>
		/// The value of the field shows whether the inventory item is suitable for a particular deduction type.
		/// </value>
		[PXDBString(1, IsFixed = true)]
        [PXDefault(RUTROTTypes.RUT, typeof(Search<BranchRUTROT.defaultRUTROTType, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Type", FieldClass = RUTROTMessages.FieldClass, Enabled = true)]
		[RUTROTTypes.List]
		public virtual string RUTROTType
		{
			get;
			set;
		}
		#endregion
		#region RUTROTItemType
		public abstract class rUTROTItemType : PX.Data.BQL.BqlString.Field<rUTROTItemType> { }
		/// <summary>
		/// The type of the item.
		/// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">RUT and ROT Deduction</see> feature is enabled.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="RUTROTItemTypes.ListAttribute"/>.
		/// The value of the field cannot be <see cref="RUTROTItemTypes.Service"/> for stock items. 
		/// (For details, see <see cref="InventoryItemMaintRUTROT.HiddenInventoryItem_RUTROTItemType_CacheAttached"/>.)
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Type", FieldClass = RUTROTMessages.FieldClass, Enabled = true)]
		[RUTROTItemTypes.List]
		public virtual string RUTROTItemType
		{
			get;
			set;
		}
        #endregion
        #region RUTROTWorkTypeID
        public abstract class rUTROTWorkTypeID : PX.Data.BQL.BqlInt.Field<rUTROTWorkTypeID> { }
		/// <summary>
		/// The identifier of the <see cref="RUTROTWorkType">work type</see> selected for the item.
		/// List of available <see cref="RUTROTWorkType">Work Types</see> depends on
		/// <see cref="RUTROT.RUTROTType"/> of the item.
		/// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">RUT and ROT Deduction</see> feature is enabled.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="RUTROTWorkType.WorkTypeID"/>
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Type of Work", FieldClass = RUTROTMessages.FieldClass, Enabled = false)]
		[PXSelector(typeof(Search<RUTROTWorkType.workTypeID, Where<RUTROTWorkType.rUTROTType, Equal<Current<rUTROTType>>>>),
			SubstituteKey = typeof(RUTROTWorkType.description), DescriptionField = typeof(RUTROTWorkType.xmlTag))]
		public virtual int? RUTROTWorkTypeID
		{
			get;
			set;
		}
		#endregion
	}

    /// <summary>
    /// This class is a workaround for Kensium tests.
    /// This class will be removed in Acumatica 7.0.
    /// </summary>
    public class HiddenInventoryItem : InventoryItem
    {
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
    }

    /// <summary>
    /// This class is a workaround for Kensium tests.
    /// This class will be removed in Acumatica 7.0.
    /// </summary>
    [Serializable]
    public partial class HiddenInventoryItemRUTROT : PXCacheExtension<HiddenInventoryItem>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
        }
        #region IsRUTROTDeductible
        public abstract class isRUTROTDeductible : PX.Data.BQL.BqlBool.Field<isRUTROTDeductible> { }

        /// <summary>
        /// Indicates whether the item is subjected to RUT or ROT deductions.
        /// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">RUT and ROT Deduction</see> feature is enabled.
        /// </summary>
        /// <value>
        /// The value of this field is used to set default value of the <see cref="ARTranRUTROT.IsRUTROTDeductible">ARTran.IsRUTROTDeductible</see> field
        /// for the lines of a <see cref="ARInvoiceRUTROT.IsRUTROTDeductible">RUT or ROT deductible</see> Accounts Receivable Invoice.
        /// </value>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "ROT or RUT Deductible Item", FieldClass = RUTROTMessages.FieldClass)]
        public virtual bool? IsRUTROTDeductible
        {
            get;
            set;
        }
        #endregion
        #region RUTROTType
        public abstract class rUTROTType : PX.Data.BQL.BqlString.Field<rUTROTType> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(RUTROTTypes.RUT, typeof(Search<BranchRUTROT.defaultRUTROTType, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Type", FieldClass = RUTROTMessages.FieldClass, Enabled = true)]
        [RUTROTTypes.List]
        public virtual string RUTROTType
        {
            get;
            set;
        }
        #endregion
        #region RUTROTItemType
        public abstract class rUTROTItemType : PX.Data.BQL.BqlString.Field<rUTROTItemType> { }
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Type", FieldClass = RUTROTMessages.FieldClass, Enabled = true)]
        [RUTROTItemTypes.List]
        public virtual string RUTROTItemType
        {
            get;
            set;
        }
        #endregion
        #region RUTROTWorkTypeID
        public abstract class rUTROTWorkTypeID : PX.Data.BQL.BqlInt.Field<rUTROTWorkTypeID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Type of Work", FieldClass = RUTROTMessages.FieldClass, Enabled = false)]
        [PXSelector(typeof(Search<RUTROTWorkType.workTypeID, Where<RUTROTWorkType.rUTROTType, Equal<Current<rUTROTType>>>>),
            SubstituteKey = typeof(RUTROTWorkType.description), DescriptionField = typeof(RUTROTWorkType.xmlTag))]
        public virtual int? RUTROTWorkTypeID
        {
            get;
            set;
        }
        #endregion
    }
}
