using PX.Data;
using System;

namespace PX.Objects.CR.Extensions.Relational
{
	[PXHidden]
	public abstract class Document<T> : PXMappedCacheExtension
		where T : PXGraphExtension
	{
		#region RelatedID
		public abstract class relatedID : PX.Data.BQL.BqlInt.Field<relatedID> { }
		public virtual int? RelatedID { get; set; }
		#endregion

		#region ChildID
		public abstract class childID : PX.Data.BQL.BqlInt.Field<childID> { }
		public virtual int? ChildID { get; set; }
		#endregion

		#region IsOverrideRelated
		public abstract class isOverrideRelated : PX.Data.BQL.BqlBool.Field<isOverrideRelated> { }
		public virtual bool? IsOverrideRelated { get; set; }
		#endregion
	}

	[PXHidden]
	public abstract class Related<T> : PXMappedCacheExtension
		where T : PXGraphExtension
	{
		#region RelatedID
		public abstract class relatedID : PX.Data.BQL.BqlInt.Field<relatedID> { }
		public virtual int? RelatedID { get; set; }
		#endregion

		#region ChildID
		public abstract class childID : PX.Data.BQL.BqlInt.Field<childID> { }
		public virtual int? ChildID { get; set; }
		#endregion
	}

	[PXHidden]
	public abstract class Child<T> : PXMappedCacheExtension
		where T : PXGraphExtension
	{
		#region ChildID
		public abstract class childID : PX.Data.BQL.BqlInt.Field<childID> { }
		public virtual int? ChildID { get; set; }
		#endregion

		#region RelatedID
		public abstract class relatedID : PX.Data.BQL.BqlInt.Field<relatedID> { }
		public virtual int? RelatedID { get; set; }
		#endregion
	}
}
