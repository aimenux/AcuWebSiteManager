using PX.Data;
using System;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[PXHidden]
	public class Document : PXMappedCacheExtension, INotable
	{
		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		public virtual int? ParentBAccountID { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		public virtual int? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		public virtual Guid? OwnerID { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public virtual int? BAccountID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public virtual int? ContactID { get; set; }
		#endregion

		#region RefContactID
		public abstract class refContactID : PX.Data.BQL.BqlInt.Field<refContactID> { }
		public virtual int? RefContactID { get; set; }
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		public virtual string ClassID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }
		public virtual string Source { get; set; }
		#endregion

		#region CampaignID
		public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
		public virtual string CampaignID { get; set; }
		#endregion

		#region OverrideRefContact
		public abstract class overrideRefContact : PX.Data.BQL.BqlBool.Field<overrideRefContact> { }
		public virtual bool? OverrideRefContact { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		public virtual String Description { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		public virtual string TaxZoneID { get; set; }
		#endregion

		#region QualificationDate
		public abstract class qualificationDate : PX.Data.BQL.BqlDateTime.Field<qualificationDate> { }
		public virtual DateTime? QualificationDate { get; set; }
		#endregion

		#region ConvertedBy
		public abstract class convertedBy : PX.Data.BQL.BqlGuid.Field<convertedBy> { }
		public virtual Guid? ConvertedBy { get; set; }
		#endregion
	}
}
