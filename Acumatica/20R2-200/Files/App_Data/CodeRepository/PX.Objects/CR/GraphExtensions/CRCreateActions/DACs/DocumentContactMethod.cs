using System;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[PXHidden]
	public partial class DocumentContactMethod : PXMappedCacheExtension
	{
		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }
		public virtual String Method { get; set; }
		#endregion

		#region NoFax
		public abstract class noFax : PX.Data.BQL.BqlBool.Field<noFax> { }
		public virtual bool? NoFax { get; set; }
		#endregion

		#region NoMail
		public abstract class noMail : PX.Data.BQL.BqlBool.Field<noMail> { }
		public virtual bool? NoMail { get; set; }
		#endregion

		#region NoMarketing
		public abstract class noMarketing : PX.Data.BQL.BqlBool.Field<noMarketing> { }
		public virtual bool? NoMarketing { get; set; }
		#endregion

		#region NoCall
		public abstract class noCall : PX.Data.BQL.BqlBool.Field<noCall> { }
		public virtual bool? NoCall { get; set; }
		#endregion

		#region NoEMail
		public abstract class noEMail : PX.Data.BQL.BqlBool.Field<noEMail> { }
		public virtual bool? NoEMail { get; set; }
		#endregion

		#region NoMassMail
		public abstract class noMassMail : PX.Data.BQL.BqlBool.Field<noMassMail> { }
		public virtual bool? NoMassMail { get; set; }
		#endregion
	}
}
