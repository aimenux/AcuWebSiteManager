using PX.Data;
using System;
using PX.Objects.IN;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOPickingWorksheet, PXDacType.Document)]
	[PXPrimaryGraph(typeof(SOPickingWorksheetReview))]
	public class SOPickingWorksheet : IBqlTable
	{
		public class PK : PrimaryKeyOf<SOPickingWorksheet>.By<worksheetNbr>
		{
			public static SOPickingWorksheet Find(PXGraph graph, string groupNbr) => FindBy(graph, groupNbr);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<SOPickingWorksheet>.By<siteID> { }
		}

		#region WorksheetNbr
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[CS.AutoNumber(typeof(SOSetup.pickingWorksheetNumberingID), typeof(pickDate))]
		[PX.Data.EP.PXFieldDescription]
		[PXReferentialIntegrityCheck]
		[PXSelector(typeof(
			SearchFor<worksheetNbr>.In<
				SelectFrom<SOPickingWorksheet>.
				InnerJoin<INSite>.On<FK.Site>.
				Where<Match<INSite, AccessInfo.userName.FromCurrent>>.
				OrderBy<SOPickingWorksheet.worksheetNbr.Desc>
			>))]
		public virtual String WorksheetNbr { get; set; }
		public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
		#endregion
		#region WorksheetType
		[PXDBString(2, IsFixed = true)]
		[PXExtraKey]
		[PXDefault]
		[worksheetType.List]
		[PXUIField(DisplayName = "Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String WorksheetType { get; set; }
		public abstract class worksheetType : PX.Data.BQL.BqlString.Field<worksheetType>
		{
			public const string Wave = "WV";
			public const string Batch = "BT";

			[PX.Common.PXLocalizable]
			public static class DisplayNames
			{
				public const string Wave = "Wave";
				public const string Batch = "Batch";
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base
				(
					Pair(Wave, DisplayNames.Wave),
					Pair(Batch, DisplayNames.Batch)
				) { }
			}

			public class wave : PX.Data.BQL.BqlString.Constant<wave>
			{
				public wave() : base(Wave) { }
			}

			public class batch : PX.Data.BQL.BqlString.Constant<batch>
			{
				public batch() : base(Batch) { }
			}
		}
		#endregion
		#region PickDate
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Picking Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual DateTime? PickDate { get; set; }
		public abstract class pickDate : PX.Data.BQL.BqlDateTime.Field<pickDate> { }
		#endregion
		#region Status
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[status.List]
		[PXFormula(typeof(status.hold.When<hold.IsEqual<True>>.Else<status.open>))]
		public virtual String Status { get; set; }
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public const string Open = SOShipmentStatus.Open;
			public const string Hold = SOShipmentStatus.Hold;
			public const string Picking = "I";
			public const string Picked = "P";
			public const string Completed = SOShipmentStatus.Completed;

			[PX.Common.PXLocalizable]
			public static class DisplayNames
			{
				public const string Open = Messages.Open;
				public const string Hold = Messages.Hold;
				public const string Picking = "Picking";
				public const string Picked = "Picked";
				public const string Completed = Messages.Completed;
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base
				(
					Pair(Open, DisplayNames.Open),
					Pair(Hold, DisplayNames.Hold),
					Pair(Picking, DisplayNames.Picking),
					Pair(Picked, DisplayNames.Picked),
					Pair(Completed, DisplayNames.Completed)
				) { }
			}

			public class open : PX.Data.BQL.BqlString.Constant<open> { public open() : base(Open) { } }
			public class hold : PX.Data.BQL.BqlString.Constant<hold> { public hold() : base(Hold) { } }
			public class picking : PX.Data.BQL.BqlString.Constant<picking> { public picking() : base(Picking) { } }
			public class picked : PX.Data.BQL.BqlString.Constant<picked> { public picked() : base(Picked) { } }
			public class completed : PX.Data.BQL.BqlString.Constant<completed> { public completed() : base(Completed) { } }
		}
		#endregion
		#region Hold
		[PXDBBool]
		[PXDefault(typeof(SOSetup.holdShipments))]
		[PXUIField(DisplayName = "Hold")]
		[PXUIEnabled(typeof(status.IsIn<status.open, status.hold>))]
		public virtual Boolean? Hold { get; set; }
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region SiteID
		[Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr), Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault]
		[PXForeignReference(typeof(FK.Site))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, AccessInfo.branchID.FromCurrent>>))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region ShipmentQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipped Quantity", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region ShipmentWeight
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipped Weight", Enabled = false)]
		public virtual Decimal? ShipmentWeight { get; set; }
		public abstract class shipmentWeight : PX.Data.BQL.BqlDecimal.Field<shipmentWeight> { }
		#endregion
		#region ShipmentVolume
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipped Volume", Enabled = false)]
		public virtual Decimal? ShipmentVolume { get; set; }
		public abstract class shipmentVolume : PX.Data.BQL.BqlDecimal.Field<shipmentVolume> { }
		#endregion
		#region PackageWeight
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Package Weight", Enabled = false)]
		public virtual Decimal? PackageWeight { get; set; }
		public abstract class packageWeight : PX.Data.BQL.BqlDecimal.Field<packageWeight> { }
		#endregion
		#region PackageCount
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Packages", Enabled = false)]
		public virtual int? PackageCount { get; set; }
		public abstract class packageCount : PX.Data.BQL.BqlInt.Field<packageCount> { }
		#endregion
		#region NoteID
		[PXSearchable(
			SM.SearchCategory.SO, "{0}: {1}", new Type[] { typeof(worksheetType), typeof(worksheetNbr) }, new Type[0],
			NumberFields = new Type[] { typeof(worksheetNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(pickDate), typeof(status), typeof(qty) }
		)]
		[PXNote(new Type[0], ShowInReferenceSelector = true, Selector = typeof(worksheetNbr))]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
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
