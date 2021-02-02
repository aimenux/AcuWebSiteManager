using PX.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.SM;
using System;

namespace PX.Objects.PO
{
	[PXCacheName(Messages.POReceivePutAwaySetup, PXDacType.Config)]
	public class POReceivePutAwaySetup : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceivePutAwaySetup>.By<branchID>
		{
			public static POReceivePutAwaySetup Find(PXGraph graph, int? branchID) => FindBy(graph, branchID);
		}
		#endregion

		#region BranchID
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(Search<GL.Branch.branchID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Branch")]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion

		#region ShowReceivingTab
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Display the Receive Tab", Enabled = true)]
		public virtual bool? ShowReceivingTab { get; set; }
		public abstract class showReceivingTab : PX.Data.BQL.BqlBool.Field<showReceivingTab> { }
		#endregion
		#region ShowPutAwayTab
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Display the Put Away Tab", Enabled = true)]
		public virtual bool? ShowPutAwayTab { get; set; }
		public abstract class showPutAwayTab : PX.Data.BQL.BqlBool.Field<showPutAwayTab> { }
		#endregion
		#region ShowScanLogTab
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Display the Scan Log Tab", Enabled = true)]
		public virtual bool? ShowScanLogTab { get; set; }
		public abstract class showScanLogTab : PX.Data.BQL.BqlBool.Field<showScanLogTab> { }
		#endregion
		#region UseCartsForPutAway
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Carts for Putting Away", FieldClass = "Carts")]
		[PXUIEnabled(typeof(showPutAwayTab))]
		[PXFormula(typeof(Switch<Case<Where<showPutAwayTab, Equal<False>>, False>, useCartsForPutAway>))]
		public virtual bool? UseCartsForPutAway { get; set; }
		public abstract class useCartsForPutAway : PX.Data.BQL.BqlBool.Field<useCartsForPutAway> { }
		#endregion
		#region ExplicitLineConfirmation
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Explicit Line Confirmation")]
		public virtual bool? ExplicitLineConfirmation { get; set; }
		public abstract class explicitLineConfirmation : PX.Data.BQL.BqlBool.Field<explicitLineConfirmation> { }
		#endregion
		#region UseDefaultQty
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Quantity")]
		public virtual bool? UseDefaultQty { get; set; }
		public abstract class useDefaultQty : PX.Data.BQL.BqlBool.Field<useDefaultQty> { }
		#endregion

		#region RequestLocationForEachItemInReceive
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request Location for Each Item on Receiving")]
		public virtual bool? RequestLocationForEachItemInReceive { get; set; }
		public abstract class requestLocationForEachItemInReceive : PX.Data.BQL.BqlBool.Field<requestLocationForEachItemInReceive> { }
		#endregion
		#region RequestLocationForEachItemInPutAway
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request Location for Each Item on Putting Away")]
		public virtual bool? RequestLocationForEachItemInPutAway { get; set; }
		public abstract class requestLocationForEachItemInPutAway : PX.Data.BQL.BqlBool.Field<requestLocationForEachItemInPutAway> { }
		#endregion

		#region DefaultReceivingLocation
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Receiving Location")]
		public virtual bool? DefaultReceivingLocation { get; set; }
		public abstract class defaultReceivingLocation : PX.Data.BQL.BqlBool.Field<defaultReceivingLocation> { }
		#endregion
		#region DefaultLotSerialNumber
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Auto-Generated Lot/Serial Nbr.")]
		public virtual bool? DefaultLotSerialNumber { get; set; }
		public abstract class defaultLotSerialNumber : PX.Data.BQL.BqlBool.Field<defaultLotSerialNumber> { }
		#endregion
		#region DefaultExpireDate
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Default Expiration Date")]
		public virtual bool? DefaultExpireDate { get; set; }
		public abstract class defaultExpireDate : PX.Data.BQL.BqlBool.Field<defaultExpireDate> { }
		#endregion
		#region SingleLocation
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Single Receiving Location", FieldClass = IN.LocationAttribute.DimensionName)]
		public virtual bool? SingleLocation { get; set; }
		public abstract class singleLocation : PX.Data.BQL.BqlBool.Field<singleLocation> { }
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

		#region PrintPurchaseReceiptAutomatically
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Print Purchase Receipts Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintPurchaseReceiptAutomatically { get; set; }
		public abstract class printPurchaseReceiptAutomatically : PX.Data.BQL.BqlBool.Field<printPurchaseReceiptAutomatically> { }
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

	public sealed class DefaultReceivePutAwayModeByUser : PXCacheExtension<UserPreferences>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.wMSReceiving>();

		[PXDBString(4, IsFixed = true)]
		[rPAMode.List]
		[PXUIField(DisplayName = "Receive and Put Away")]
		[PXDefault(rPAMode.None, PersistingCheck = PXPersistingCheck.Nothing)]
		public string RPAMode { get; set; }
		public abstract class rPAMode : PX.Data.BQL.BqlString.Field<rPAMode>
		{
			[PXLocalizable]
			public static class DisplayNames
			{
				public const string None = "None";
				public const string Receive = "Receive";
				public const string PutAway = "Put Away";
			}

			public const string None = "NONE";
			public const string Receive = "RCPT";
			public const string PutAway = "PTAW";

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					Pair(None, DisplayNames.None),
					Pair(Receive, DisplayNames.Receive),
					Pair(PutAway, DisplayNames.PutAway)
				) { }
			}
		}
	}
}