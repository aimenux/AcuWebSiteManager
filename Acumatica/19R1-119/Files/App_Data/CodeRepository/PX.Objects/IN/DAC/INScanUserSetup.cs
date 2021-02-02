using System;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INScanUserSetup, PXDacType.Config)]
	public class INScanUserSetup : IBqlTable
	{
		#region UserID
		[PXDBGuid(IsKey = true)]
		[PXDefault(typeof(Search<Users.pKID, Where<Users.pKID, Equal<Current<AccessInfo.userID>>>>))]
		[PXUIField(DisplayName = "User")]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		#endregion
		#region IsOverridden
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Overridden", Enabled = false)]
		public virtual bool? IsOverridden { get; set; }
		public abstract class isOverridden : PX.Data.BQL.BqlBool.Field<isOverridden> { }
		#endregion

		#region Mode
		[PXDBString(4, IsKey = true)]
		[PXUIField(DisplayName = "Mode", Enabled = false, Visible = false)]
		public virtual string Mode { get; set; }
		public abstract class mode : PX.Data.BQL.BqlString.Field<mode> { }
		#endregion

		#region DefaultWarehouse
		[PXDBBool]
		[PXDefault(true, typeof(Search<INScanSetup.defaultWarehouse, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Default Warehouse from User Profile")]
		public virtual bool? DefaultWarehouse { get; set; }
		public abstract class defaultWarehouse : PX.Data.BQL.BqlBool.Field<defaultWarehouse> { }
		#endregion
		#region DefaultLotSerialNumber
		[PXDBBool]
		[PXDefault(false, typeof(Search<INScanSetup.defaultLotSerialNumber, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Auto-Generated Lot/Serial Nbr.")]
		public virtual bool? DefaultLotSerialNumber { get; set; }
		public abstract class defaultLotSerialNumber : PX.Data.BQL.BqlBool.Field<defaultLotSerialNumber> { }
		#endregion
		#region DefaultExpireDate
		[PXDBBool]
		[PXDefault(true, typeof(Search<INScanSetup.defaultExpireDate, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Expiration Date")]
		public virtual bool? DefaultExpireDate { get; set; }
		public abstract class defaultExpireDate : PX.Data.BQL.BqlBool.Field<defaultExpireDate> { }
		#endregion

		#region PrintInventoryLabelsAutomatically
		[PXDBBool]
		[PXDefault(false, typeof(Search<INScanSetup.printInventoryLabelsAutomatically, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Print Inventory Labels Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintInventoryLabelsAutomatically { get; set; }
		public abstract class printInventoryLabelsAutomatically : PX.Data.BQL.BqlBool.Field<printInventoryLabelsAutomatically> { }
		#endregion
		#region InventoryLabelsReportID
		[PXDefault("IN619200", typeof(Search<INScanSetup.inventoryLabelsReportID, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
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

		#region UseScale
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Digital Scale", FieldClass = "DeviceHub", Visible = false)]
		public virtual bool? UseScale { get; set; }
		public abstract class useScale : PX.Data.BQL.BqlBool.Field<useScale> { }
		#endregion

		#region ScaleID
		[PXDBString(10)]
		[PXSelector(typeof(SMScale.scaleID))]
		[PXUIEnabled(typeof(useScale))]
		[PXUIField(DisplayName = "Scale", FieldClass = "DeviceHub", Visible = false)]
		public virtual string ScaleID { get; set; }
		public abstract class scaleID : PX.Data.BQL.BqlString.Field<scaleID> { }
		#endregion

		public virtual bool SameAs(INScanSetup setup)
		{
			return
				DefaultWarehouse == setup.DefaultWarehouse &&
				DefaultLotSerialNumber == setup.DefaultLotSerialNumber &&
				DefaultExpireDate == setup.DefaultExpireDate &&
				PrintInventoryLabelsAutomatically == setup.PrintInventoryLabelsAutomatically &&
				InventoryLabelsReportID == setup.InventoryLabelsReportID;
		}

		public virtual INScanUserSetup ApplyValuesFrom(INScanSetup setup)
		{
			DefaultWarehouse = setup.DefaultWarehouse;
			DefaultLotSerialNumber = setup.DefaultLotSerialNumber;
			DefaultExpireDate = setup.DefaultExpireDate;
			PrintInventoryLabelsAutomatically = setup.PrintInventoryLabelsAutomatically;
			InventoryLabelsReportID = setup.InventoryLabelsReportID;
			return this;
		}
	}
}
