using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.IN;
using System;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOPickerToShipmentLink, PXDacType.Details)]
	public class SOPickerToShipmentLink : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOPickerToShipmentLink>.By<siteID, toteID, shipmentNbr>
		{
			public static SOPickerToShipmentLink Find(PXGraph graph, int? siteID, int? toteID, string shipmentNbr) => FindBy(graph, siteID, toteID, shipmentNbr);
		}

		public static class FK
		{
			public class Worksheet : SOPickingWorksheet.PK.ForeignKeyOf<SOPickerToShipmentLink>.By<worksheetNbr> { }
			public class Picker : SOPicker.PK.ForeignKeyOf<SOPickerToShipmentLink>.By<worksheetNbr, pickerNbr> { }
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOPickerToShipmentLink>.By<shipmentNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<SOPickerToShipmentLink>.By<siteID> { }
			public class Tote : INTote.PK.ForeignKeyOf<SOPickerToShipmentLink>.By<siteID, toteID> { }
		}
		#endregion

		#region WorksheetNbr
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBDefault(typeof(SOPickingWorksheet.worksheetNbr))]
		[PXParent(typeof(FK.Worksheet))]
		public virtual String WorksheetNbr { get; set; }
		public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
		#endregion
		#region PickerNbr
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(SOPicker.pickerNbr))]
		[PXUIField(DisplayName = "Picker Nbr.")]
		public virtual Int32? PickerNbr { get; set; }
		public abstract class pickerNbr : PX.Data.BQL.BqlInt.Field<pickerNbr> { }
		#endregion
		#region ShipmentNbr
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXDefault]
		[PXForeignReference(typeof(FK.Shipment))]
		[PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region SiteID
		[Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXForeignReference(typeof(FK.Site))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region ToteID
		[PXDBInt]
		[PXForeignReference(typeof(FK.Tote))]
		[PXUIField(DisplayName = "Tote ID", Enabled = false)]
		[PXSelector(typeof(INTote.toteID), SubstituteKey = typeof(INTote.toteCD), DescriptionField = typeof(INTote.descr))]
		public virtual int? ToteID { get; set; }
		public abstract class toteID : PX.Data.BQL.BqlInt.Field<toteID> { }
		#endregion

		#region Audit Fields
		#region tstamp
		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		[PXUIField(DisplayName = "Created At", Enabled = false, IsReadOnly = true)]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedByID, Enabled = false, IsReadOnly = true)]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		[PXUIField(DisplayName = "Last Modified At", Enabled = false, IsReadOnly = true)]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion
	}
}
