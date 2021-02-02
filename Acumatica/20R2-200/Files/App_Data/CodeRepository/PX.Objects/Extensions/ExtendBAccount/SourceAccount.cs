using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.GraphExtensions.ExtendBAccount
{
	public class SourceAccount<Extension> : PXMappedCacheExtension
		where Extension : PXGraphExtension
	{
		#region AcctCD
		public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

		public virtual String AcctCD { get; set; }
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		public virtual String Type { get; set; }
		#endregion

		#region LocaleName
		public abstract class localeName : PX.Data.BQL.BqlString.Field<localeName> { }
		public virtual string LocaleName { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }		
		public virtual Guid? NoteID { get; set; }
		#endregion
	}
}
