using V2 = PX.CCProcessingBase.Interfaces.V2;
using V1 = PX.CCProcessingBase;
using PX.Data;
using PX.Data.EP;
using System;
using PX.Objects.GL;
using PX.Objects.Common.Attributes;
namespace PX.Objects.CA
{
	[PXCacheName(Messages.CCProcessingCenter)]
	[Serializable]
	public partial class CCProcessingCenter : IBqlTable
	{
		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>))]
		[PXUIField(DisplayName = "Proc. Center ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual string Name
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		[PXDefault]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region ProcessingTypeName
		public abstract class processingTypeName : PX.Data.BQL.BqlString.Field<processingTypeName> { }

		[PXDBString(255)]
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[PXProviderTypeSelector(typeof(V2.ICCProcessingPlugin))]
		[DeprecatedProcessing(ChckVal = DeprecatedProcessingAttribute.CheckVal.ProcessingCenterType)]
		[PXUIField(DisplayName = "Payment Plug-In (Type)")]
		public virtual string ProcessingTypeName
		{
			get;
			set;
		}
		#endregion
		#region ProcessingAssemblyName
		public abstract class processingAssemblyName : PX.Data.BQL.BqlString.Field<processingAssemblyName> { }

		[PXDBString(255)]
		[PXUIField(DisplayName = "Assembly Name")]
		public virtual string ProcessingAssemblyName
		{
			get;
			set;
		}
		#endregion
		#region OpenTranTimeout
		public abstract class openTranTimeout : PX.Data.BQL.BqlInt.Field<openTranTimeout> { }

		[PXDBInt(MinValue = 0, MaxValue = 60)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Transaction Timeout (s)", Visibility = PXUIVisibility.Visible)]
		public virtual int? OpenTranTimeout
		{
			get;
			set;
		}
		#endregion
		#region AllowDirectInput
		public abstract class allowDirectInput : PX.Data.BQL.BqlBool.Field<allowDirectInput> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Direct Input", Visible = false)]
		public virtual bool? AllowDirectInput
		{
			get;
			set;
		}
		#endregion
		#region NeedsExpDateUpdate
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? NeedsExpDateUpdate
		{
			get;
			set;
		}
		#endregion
		#region SyncronizeDeletion
		public abstract class syncronizeDeletion : PX.Data.BQL.BqlBool.Field<syncronizeDeletion> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Synchronize Deletion", Visible = false)]
		public virtual bool? SyncronizeDeletion
		{
			get;
			set;
		}
		#endregion
		#region UseAcceptPaymentForm
		public abstract class useAcceptPaymentForm : PX.Data.BQL.BqlBool.Field<useAcceptPaymentForm> { }
	
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Accept Payments from New Cards")]
		public virtual bool? UseAcceptPaymentForm
		{
			get;
			set;
		}
		#endregion
		#region SyncRetryAttemptsNo
		public abstract class syncRetryAttemptsNo : PX.Data.BQL.BqlInt.Field<syncRetryAttemptsNo> { }

		[PXDBInt(MinValue = 0, MaxValue = 10)]
		[PXDefault(TypeCode.Int32, "3", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Number of Additional Synchronization Attempts", Visibility = PXUIVisibility.Visible)]
		public virtual int? SyncRetryAttemptsNo
		{
			get;
			set;
		}
		#endregion
		#region SyncRetryDelayMs
		public abstract class syncRetryDelayMs : PX.Data.BQL.BqlInt.Field<syncRetryDelayMs> { }

		[PXDBInt(MinValue = 0, MaxValue = 1000)]
		[PXDefault(TypeCode.Int32, "500", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Delay Between Synchronization Attempts (ms)", Visibility = PXUIVisibility.Visible)]
		public virtual int? SyncRetryDelayMs
		{
			get;
			set;
		}
		#endregion

		#region CreditCardLimit
		public abstract class creditCardLimit : PX.Data.BQL.BqlInt.Field<creditCardLimit> { }

		[PXDBInt(MinValue = 1)]
		[PXDefault(10, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Maximum Credit Cards per Profile", Visible = true)]
		public virtual int? CreditCardLimit
		{
			get;
			set;
		}
		#endregion

		#region CreateAdditionalCustomerProfile
		public abstract class createAdditionalCustomerProfiles : PX.Data.BQL.BqlBool.Field<createAdditionalCustomerProfiles> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Create Additional Customer Profiles", Visible = true)]
		public virtual Boolean? CreateAdditionalCustomerProfiles
		{
			get;
			set;
		}
		#endregion

		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(CCProcessingCenter.processingCenterID))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}

		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
