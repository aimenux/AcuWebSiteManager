using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRLocation)]
	[PXPrimaryGraph(typeof(WorkLocationsMaint))]
	[Serializable]
	public class PRLocation : IBqlTable
	{
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[PXDBIdentity]
		[PXReferentialIntegrityCheck]
		public int? LocationID { get; set; }
		#endregion
		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(PRLocation.locationCD))]
		public string LocationCD { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Location Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public string Description { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Is Active")]
		public bool? IsActive { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(typeof(Branch.branchID), DisplayName = "Use Address from Branch ID", PersistingCheck = PXPersistingCheck.Nothing)]
		public int? BranchID { get; set; }
		#endregion
		#region AddressID
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Address ID", Visible = false)]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public int? AddressID { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
