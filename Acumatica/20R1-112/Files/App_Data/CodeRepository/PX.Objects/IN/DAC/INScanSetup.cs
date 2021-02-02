using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.SM;
using System;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INScanSetup, PXDacType.Config)]
	public class INScanSetup : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INScanSetup>.By<branchID>
		{
			public static INScanSetup Find(PXGraph graph, int? branchID) => FindBy(graph, branchID);
		}
		#endregion

		#region BranchID
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(Search<GL.Branch.branchID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Branch")]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion

		#region UseCartsForTransfers
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Carts for Transferring", FieldClass = "Carts", Visible = false)]
		public virtual bool? UseCartsForTransfers { get; set; }
		public abstract class useCartsForTransfers : PX.Data.BQL.BqlBool.Field<useCartsForTransfers> { }
		#endregion
		#region ExplicitLineConfirmation
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Explicit Line Confirmation")]
		public virtual bool? ExplicitLineConfirmation { get; set; }
		public abstract class explicitLineConfirmation : PX.Data.BQL.BqlBool.Field<explicitLineConfirmation> { }
		#endregion

		#region UseDefaultQtyInReceipt
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Quantity in Receipts")]
		public virtual bool? UseDefaultQtyInReceipt { get; set; }
		public abstract class useDefaultQtyInReceipt : PX.Data.BQL.BqlBool.Field<useDefaultQtyInReceipt> { }
		#endregion
		#region UseDefaultQtyInIssue
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Quantity in Issues")]
		public virtual bool? UseDefaultQtyInIssue { get; set; }
		public abstract class useDefaultQtyInIssue : PX.Data.BQL.BqlBool.Field<useDefaultQtyInIssue> { }
		#endregion
		#region UseDefaultQtyInTransfer
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Quantity in Transfers")]
		public virtual bool? UseDefaultQtyInTransfer { get; set; }
		public abstract class useDefaultQtyInTransfer : PX.Data.BQL.BqlBool.Field<useDefaultQtyInTransfer> { }
		#endregion
		#region UseDefaultQtyInCount
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Quantity in PI Counts")]
		public virtual bool? UseDefaultQtyInCount { get; set; }
		public abstract class useDefaultQtyInCount : PX.Data.BQL.BqlBool.Field<useDefaultQtyInCount> { }
		#endregion

		#region UseDefaultReasonCodeInReceipt
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Default Reason Code in Receipts")]
		public virtual bool? UseDefaultReasonCodeInReceipt { get; set; }
		public abstract class useDefaultReasonCodeInReceipt : PX.Data.BQL.BqlBool.Field<useDefaultReasonCodeInReceipt> { }
		#endregion
		#region UseDefaultReasonCodeInIssue
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Default Reason Code in Issues")]
		public virtual bool? UseDefaultReasonCodeInIssue { get; set; }
		public abstract class useDefaultReasonCodeInIssue : PX.Data.BQL.BqlBool.Field<useDefaultReasonCodeInIssue> { }
		#endregion
		#region UseDefaultReasonCodeInTransfer
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Default Reason Code in Transfers")]
		public virtual bool? UseDefaultReasonCodeInTransfer { get; set; }
		public abstract class useDefaultReasonCodeInTransfer : PX.Data.BQL.BqlBool.Field<useDefaultReasonCodeInTransfer> { }
		#endregion

		#region RequestLocationForEachItemInReceipt
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request Location for Each Item in Receipts")]
		public virtual bool? RequestLocationForEachItemInReceipt { get; set; }
		public abstract class requestLocationForEachItemInReceipt : PX.Data.BQL.BqlBool.Field<requestLocationForEachItemInReceipt> { }
		#endregion
		#region RequestLocationForEachItemInIssue
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request Location for Each Item in Issues")]
		public virtual bool? RequestLocationForEachItemInIssue { get; set; }
		public abstract class requestLocationForEachItemInIssue : PX.Data.BQL.BqlBool.Field<requestLocationForEachItemInIssue> { }
		#endregion
		#region RequestLocationForEachItemInTransfer
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request Location for Each Item in Transfers")]
		public virtual bool? RequestLocationForEachItemInTransfer { get; set; }
		public abstract class requestLocationForEachItemInTransfer : PX.Data.BQL.BqlBool.Field<requestLocationForEachItemInTransfer> { }
		#endregion

		#region DefaultWarehouse
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Warehouse from User Profile")]
		public virtual bool? DefaultWarehouse { get; set; }
		public abstract class defaultWarehouse : PX.Data.BQL.BqlBool.Field<defaultWarehouse> { }
		#endregion

		#region PrintInventoryLabelsAutomatically
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Print Inventory Labels Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintInventoryLabelsAutomatically { get; set; }
		public abstract class printInventoryLabelsAutomatically : PX.Data.BQL.BqlBool.Field<printInventoryLabelsAutomatically> { }
		#endregion
		#region InventoryLabelsReportID
		[PXDefault("IN619200", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Inventory Labels Report ID", FieldClass = "DeviceHub")]
		[PXSelector(typeof(Search<SiteMap.screenID,
			Where<SiteMap.screenID, Like<CA.PXModule.in_>, And<SiteMap.url, Like<Common.urlReports>>>,
			OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
			Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
			DescriptionField = typeof(SiteMap.title))]
		[PXUIEnabled(typeof(printInventoryLabelsAutomatically))]
		[PXUIRequired(typeof(Where<printInventoryLabelsAutomatically, Equal<True>, And<FeatureInstalled<FeaturesSet.deviceHub>>>))]
		public virtual String InventoryLabelsReportID { get; set; }
		public abstract class inventoryLabelsReportID : PX.Data.BQL.BqlString.Field<inventoryLabelsReportID> { }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
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
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		{
		}
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
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		{
		}
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
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		{
		}
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
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		{
		}
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
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		{
		}
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
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		{
		}
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
}