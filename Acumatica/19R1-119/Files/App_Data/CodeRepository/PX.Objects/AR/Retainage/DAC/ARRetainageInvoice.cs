using System;

using PX.Data;

namespace PX.Objects.AR
{
	[Serializable]
	public class ARRetainageInvoice : ARRegister
	{
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		public new abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }
	}
}
