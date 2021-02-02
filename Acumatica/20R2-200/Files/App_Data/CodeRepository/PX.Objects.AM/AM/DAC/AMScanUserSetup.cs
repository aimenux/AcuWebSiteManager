using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.AM
{
	[PXCacheName(Messages.AMScanUserSetup, PXDacType.Config)]
	public class AMScanUserSetup : IBqlTable
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
		[PXDefault(true, typeof(Search<AMScanSetup.defaultWarehouse, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Default Warehouse from User Profile")]
		public virtual bool? DefaultWarehouse { get; set; }
		public abstract class defaultWarehouse : PX.Data.BQL.BqlBool.Field<defaultWarehouse> { }
		#endregion
		#region DefaultLotSerialNumber
		[PXDBBool]
		[PXDefault(true, typeof(Search<AMScanSetup.defaultLotSerialNumber, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Auto-Generated Lot/Serial Nbr.")]
		public virtual bool? DefaultLotSerialNumber { get; set; }
		public abstract class defaultLotSerialNumber : PX.Data.BQL.BqlBool.Field<defaultLotSerialNumber> { }
		#endregion
		#region DefaultExpireDate
		[PXDBBool]
		[PXDefault(true, typeof(Search<AMScanSetup.defaultExpireDate, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Expiration Date")]
		public virtual bool? DefaultExpireDate { get; set; }
		public abstract class defaultExpireDate : PX.Data.BQL.BqlBool.Field<defaultExpireDate> { }
        #endregion

        public virtual bool SameAs(AMScanSetup setup)
		{
            return
                DefaultWarehouse == setup.DefaultWarehouse &&
                DefaultLotSerialNumber == setup.DefaultLotSerialNumber &&
                DefaultExpireDate == setup.DefaultExpireDate;
		}
    }
}
