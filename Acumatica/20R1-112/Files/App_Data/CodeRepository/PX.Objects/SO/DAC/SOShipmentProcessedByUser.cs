using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.SO
{
	[PXCacheName("SO Shipment Processed by User", PXDacType.History)]
	public class SOShipmentProcessedByUser : IBqlTable
	{
		public static class FK
		{
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOShipmentProcessedByUser>.By<shipmentNbr> { }
			public class User : Users.PK.ForeignKeyOf<SOShipmentProcessedByUser>.By<userID> { }
		}

		#region RecordID
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID { get; set; }
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		#endregion
		#region ShipmentNbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault(typeof(SOShipment.shipmentNbr))]
		[PXParent(typeof(FK.Shipment))]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region UserID
		[PXDBGuid]
		[PXDefault(typeof(AccessInfo.userID))]
		[PXParent(typeof(FK.User))]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		#endregion
		#region Confirmed
		[PXBool]
		public bool? Confirmed { get; set; }
		public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
		#endregion

		#region StartDateTime
		[PXDBDateAndTime]
		public virtual DateTime? StartDateTime { get; set; }
		public abstract class startDateTime : PX.Data.BQL.BqlDateTime.Field<startDateTime> { }
		#endregion
		#region LastModifiedDate
		[PXDBDateAndTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#region EndDateTime
		[PXDBDateAndTime]
		public virtual DateTime? EndDateTime { get; set; }
		public abstract class endDateTime : PX.Data.BQL.BqlDateTime.Field<endDateTime> { }
		#endregion
	}
}