using PX.Data;

namespace PX.Objects.CR.Extensions.CRContactAccountDataSync
{
	/// <exclude/>
	[PXHidden]
	public class Document : PXMappedCacheExtension
	{
		#region OverrideRefContact
		public abstract class overrideRefContact : PX.Data.BQL.BqlBool.Field<overrideRefContact> { }
		public virtual bool? OverrideRefContact { get; set; }
		#endregion

		#region ContactID
		public abstract class refContactID : PX.Data.BQL.BqlInt.Field<refContactID> { }
		public virtual int? RefContactID { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public virtual int? BAccountID { get; set; }
		#endregion
	}
}