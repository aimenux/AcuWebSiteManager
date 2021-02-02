using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.RQ
{
	[System.SerializableAttribute()]	
	public partial class RQRequisitionOrder : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<RQRequisitionOrder>.By<reqNbr, orderCategory, orderType, orderNbr>
		{
			public static RQRequisitionOrder Find(PXGraph graph, string reqNbr, string orderCategory, string orderType, string orderNbr)
				=> FindBy(graph, reqNbr, orderCategory, orderType, orderNbr);
		}
		public static class FK
		{
			public class Requisition : RQRequisition.PK.ForeignKeyOf<RQRequisitionOrder>.By<reqNbr> { }
			public class SOOrder : SO.SOOrder.PK.ForeignKeyOf<RQRequisitionOrder>.By<orderType, orderNbr> { }
			public class POOrder : PO.POOrder.PK.ForeignKeyOf<RQRequisitionOrder>.By<orderType, orderNbr> { }
		}
		#endregion
		
		#region ReqNbr
		public abstract class reqNbr : PX.Data.BQL.BqlString.Field<reqNbr> { }
		protected String _ReqNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault(typeof(RQRequisition.reqNbr))]
		[PXParent(typeof(FK.Requisition))]
		public virtual String ReqNbr
		{
			get
			{
				return this._ReqNbr;
			}
			set
			{
				this._ReqNbr = value;
			}
		}
		#endregion
		#region OrderCategory
		public abstract class orderCategory : PX.Data.BQL.BqlString.Field<orderCategory> { }
		protected String _OrderCategory;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">aa")]
		[PXDefault()]
		public virtual String OrderCategory
		{
			get
			{
				return this._OrderCategory;
			}
			set
			{
				this._OrderCategory = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">aa")]
		[PXDefault()]		
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]				
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
	}

	public class RQOrderCategory
	{
		public const string PO = "PO";
		public const string SO = "SO";

		public class po : PX.Data.BQL.BqlString.Constant<po> { public po() : base(PO) { } }
		public class so : PX.Data.BQL.BqlString.Constant<so> { public so() : base(SO) { } }
	}
}
