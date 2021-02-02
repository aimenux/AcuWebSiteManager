using PX.Objects.IN;
using PX.SM;

namespace PX.Objects.CR
{
	using System;
	using System.Linq;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CS;
	using PX.Objects.CT;

	[PXCacheName(Messages.CaseClass)]
	[PXPrimaryGraph(typeof(CRCaseClassMaint))]
	[System.SerializableAttribute()]
	public partial class CRCaseClass : PX.Data.IBqlTable
	{
		#region CaseClassID
		public abstract class caseClassID : PX.Data.BQL.BqlString.Field<caseClassID> { }
		protected String _CaseClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Case Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CRCaseClass.caseClassID), DescriptionField = typeof(CRCaseClass.description))]
		public virtual String CaseClassID
		{
			get
			{
				return this._CaseClassID;
			}
			set
			{
				this._CaseClassID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }
		protected Boolean? _IsBillable;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billable")]
		[PXUIEnabled(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, False>, isBillable>))]
		public virtual Boolean? IsBillable
		{
			get
			{
				return this._IsBillable;
			}
			set
			{
				this._IsBillable = value;
			}
		}
		#endregion
		#region AllowOverrideBillable
		public abstract class allowOverrideBillable : PX.Data.BQL.BqlBool.Field<allowOverrideBillable> { }
		protected Boolean? _AllowOverrideBillable;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Billable Option Override")]
		[PXUIEnabled(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, False>, allowOverrideBillable>))]
		public virtual Boolean? AllowOverrideBillable
		{
			get
			{
				return this._AllowOverrideBillable;
			}
			set
			{
				this._AllowOverrideBillable = value;
			}
		}
		#endregion
		#region RequireCustomer
		public abstract class requireCustomer : PX.Data.BQL.BqlBool.Field<requireCustomer> { }
		protected Boolean? _RequireCustomer;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Customer")]
		[PXUIEnabled(typeof(Where<isBillable, NotEqual<True>, And<allowOverrideBillable, NotEqual<True>>>))]
		[PXFormula(typeof(Switch<Case<Where<isBillable, Equal<True>, Or<allowOverrideBillable, Equal<True>>>, True>, Current<requireCustomer>>))]
		public virtual Boolean? RequireCustomer
		{
			get
			{
				return this._RequireCustomer;
			}
			set
			{
				this._RequireCustomer = value;
			}
		}
		#endregion
		#region RequireContact
		public abstract class requireContact : PX.Data.BQL.BqlBool.Field<requireContact> { }
		protected Boolean? _RequireContact;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Contact")]
		public virtual Boolean? RequireContact
		{
			get
			{
				return this._RequireContact;
			}
			set
			{
				this._RequireContact = value;
			}
		}
		#endregion
		#region RequireContract
		public abstract class requireContract : PX.Data.BQL.BqlBool.Field<requireContract> { }
		protected Boolean? _RequireContract;
		[PXDBBool()]
		[PXDefault(false)]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, True>, Current<requireContract>>))]
		[PXUIEnabled(typeof(Where<perItemBilling, NotEqual<BillingTypeListAttribute.perActivity>>))]
		[PXUIField(DisplayName = "Require Contract")]
		public virtual Boolean? RequireContract
		{
			get
			{
				return this._RequireContract;
			}
			set
			{
				this._RequireContract = value;
			}
		}
		#endregion
        #region PerItemBilling
        public abstract class perItemBilling : PX.Data.BQL.BqlInt.Field<perItemBilling> { }
		protected Int32? _PerItemBilling;
		[PXDBInt()]
		[BillingTypeList()]
		[PXDefault(BillingTypeListAttribute.PerCase)]
		[PXUIField(DisplayName = "Billing Mode")]
		public virtual Int32? PerItemBilling
        {
            get
            {
                return this._PerItemBilling;
            }
            set
            {
                this._PerItemBilling = value;
            }
        }
        #endregion
        #region LabourItemID
        public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
        protected Int32? _LabourItemID;
        [PXDBInt()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Item", Required = false)]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXUIRequired(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>, And<Where<isBillable, Equal<True>, Or<allowOverrideBillable, Equal<True>>>>>))]
		[PXUIEnabled(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, Null>, Current<labourItemID>>))]
		public virtual Int32? LabourItemID
        {
            get
            {
                return this._LabourItemID;
            }
            set
            {
                this._LabourItemID = value;
            }
        }
        #endregion
        #region OvertimeItemID
        public abstract class overtimeItemID : PX.Data.BQL.BqlInt.Field<overtimeItemID> { }
        protected Int32? _OvertimeItemID;
        [PXDBInt()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Overtime Labor Item", Required = false)]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<overtimeItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXUIRequired(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>, And<Where<isBillable, Equal<True>, Or<allowOverrideBillable, Equal<True>>>>>))]
		[PXUIEnabled(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, Null>, Current<overtimeItemID>>))]
		public virtual Int32? OvertimeItemID
        {
            get
            {
                return this._OvertimeItemID;
            }
            set
            {
                this._OvertimeItemID = value;
            }
        }
        #endregion



		#region DefaultEMailAccount
		public abstract class defaultEMailAccountID : PX.Data.BQL.BqlInt.Field<defaultEMailAccountID> { }

		[PXSelector(typeof(EMailAccount.emailAccountID), typeof(EMailAccount.description), DescriptionField = typeof(EMailAccount.description))]
		[PXUIField(DisplayName = "Default Email Account")]
		[PXDBInt]
		public virtual int? DefaultEMailAccountID { get; set; }
		#endregion
		#region RoundingInMinutes
		public abstract class roundingInMinutes : PX.Data.BQL.BqlInt.Field<roundingInMinutes> { }
		protected Int32? _RoundingInMinutes;
		[PXDBInt()]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Round Time by")]
		[PXTimeList(5, 6)]
		public virtual Int32? RoundingInMinutes
		{
			get
			{
				return this._RoundingInMinutes;
			}
			set
			{
				this._RoundingInMinutes = value;
			}
		}
		#endregion

		#region MinBillTimeInMinutes
		public abstract class minBillTimeInMinutes : PX.Data.BQL.BqlInt.Field<minBillTimeInMinutes> { }
		protected Int32? _MinBillTimeInMinutes;
		[PXDBInt()]
		[PXDefault(0)]
		[PXTimeList(5, 12)]
		[PXUIField(DisplayName = "Min Billable Time", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? MinBillTimeInMinutes
		{
			get
			{
				return this._MinBillTimeInMinutes;
			}
			set
			{
				this._MinBillTimeInMinutes = value;
			}
		}
		#endregion

		#region ReopenCaseTimeInDays
		public abstract class reopenCaseTimeInDays : PX.Data.BQL.BqlInt.Field<reopenCaseTimeInDays> { }
		protected Int32? _ReopenCaseTimeInDays;
		[PXDBInt]
		[PXUIField(DisplayName = "Allowed Period to Reopen Case (in Days)")]
		public virtual Int32? ReopenCaseTimeInDays
		{
			get
			{
				return this._ReopenCaseTimeInDays;
			}
			set
			{
				this._ReopenCaseTimeInDays = value;
			}
		}
		#endregion

		#region IsInternal

		public abstract class isInternal : PX.Data.BQL.BqlBool.Field<isInternal> { }
		protected Boolean? _IsInternal;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Internal", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsInternal
		{
			get
			{
				return this._IsInternal;
			}
			set
			{
				this._IsInternal = value;
			}
		}

		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion

		}

	public class BillingTypeListAttribute : PXIntListAttribute
	{
		public const int PerCase = 0;
		public const int PerActivity = 1;
        
		public class perCase : PX.Data.BQL.BqlInt.Constant<perCase>
		{
			public perCase() : base(PerCase) { }
		}

		public class perActivity : PX.Data.BQL.BqlInt.Constant<perActivity>
		{
			public perActivity() : base(PerActivity) { }
		}

        public override void CacheAttached(PXCache sender)
        {
            _AllowedValues = PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>()
                ? new[] { PerCase, PerActivity }
                : new[] { PerCase };

            _AllowedLabels = PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>()
                ? new[] { Messages.PerCase, Messages.PerActivity }
                : new[] { Messages.PerCase };

			_NeutralAllowedLabels = _AllowedLabels;

			base.CacheAttached(sender);
        }
	}
}
