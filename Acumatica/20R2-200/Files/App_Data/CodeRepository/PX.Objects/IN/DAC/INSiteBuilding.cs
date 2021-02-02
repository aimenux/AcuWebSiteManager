using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.GL;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.WarehouseBuilding, PXDacType.Catalogue)]
	public class INSiteBuilding : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSiteBuilding>.By<buildingID>
		{
			public static INSiteBuilding Find(PXGraph graph, int? buildingID) => FindBy(graph, buildingID);
		}
		#endregion

		#region BuildingID
		[PXDBIdentity]
		[PXReferentialIntegrityCheck]
		public virtual int? BuildingID { get; set; }
		public abstract class buildingID : PX.Data.BQL.BqlInt.Field<buildingID> { }
		#endregion
		#region BuildingCD
		[PXDBString(30, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Building ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<buildingCD>), SubstituteKey = typeof(buildingCD), CacheGlobal = true)]
		[PXDefault]
		public virtual string BuildingCD { get; set; }
		public abstract class buildingCD : PX.Data.BQL.BqlString.Field<buildingCD> { }
		#endregion
		#region Descr
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual string Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region BranchID
		[Branch]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region AddressID
		[PXDBInt]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public virtual int? AddressID { get; set; }
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		#endregion
	}
}