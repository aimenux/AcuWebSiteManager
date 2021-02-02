using System;

using PX.Data;

namespace PX.Objects.AP
{
	[Serializable]
	public class APRetainageInvoice : APRegister
	{
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		public new abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		public new abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }
	}
}
