using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN.DAC
{
	[PXCacheName(Messages.RegisterCart, PXDacType.Details)]
	public class INRegisterCart : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INRegisterCart>.By<siteID, cartID, docType, refNbr>
		{
			public static INRegisterCart Find(PXGraph graph, int? siteID, int? cartID, string docType, string refNbr) 
				=> FindBy(graph, siteID, cartID, docType, refNbr);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INRegisterCart>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<INRegisterCart>.By<siteID, cartID> { }
			public class Register : INRegister.PK.ForeignKeyOf<INRegisterCart>.By<docType, refNbr> { }
		}
		#endregion

		#region SiteID
		[Site(IsKey = true, Visible = false)]
		[PXDefault(typeof(INCart.siteID))]
		[PXParent(typeof(FK.Site))]
		public int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region CartID
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(Search<INCart.cartID, Where<INCart.active, Equal<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		[PXDefault(typeof(INCart.cartID))]
		[PXParent(typeof(FK.Cart))]
		public int? CartID { get; set; }
		public abstract class cartID : BqlInt.Field<cartID> { }
		#endregion

		#region DocType
		[PXUIField(DisplayName = INRegister.docType.DisplayName)]
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(INRegister.docType))]
		public virtual string DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion

		#region RefNbr
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDBDefault(typeof(INRegister.refNbr))]
		[PXParent(typeof(FK.Register))]
		[PXUIField(DisplayName = INRegister.refNbr.DisplayName)]
		public virtual string RefNbr { get; set; }
		public abstract class refNbr : BqlString.Field<refNbr> { }
		#endregion

		#region tstamp
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}
