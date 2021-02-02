using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR
{
	[PXCacheName(Messages.ARPaymentTotals)]

	public class ARPaymentTotals : IBqlTable
	{
		#region Keys
		/// <exclude/>
		public class PK : PrimaryKeyOf<ARPaymentTotals>.By<docType, refNbr>
		{
			public static ARPaymentTotals Find(PXGraph graph, string docType, string refNbr) => FindBy(graph, docType, refNbr);
		}
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARPaymentType.ListEx()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDBDefault(typeof(ARRegister.refNbr), DefaultForUpdate = false)]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXParent(typeof(Select<ARRegister, Where<ARRegister.docType, Equal<Current<docType>>, And<ARRegister.refNbr, Equal<Current<refNbr>>>>>))]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion

		#region OrderCntr
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? OrderCntr
		{
			get;
			set;
		}
		public abstract class orderCntr : PX.Data.BQL.BqlInt.Field<orderCntr> { }
		#endregion
		#region AdjdOrderType
		public abstract class adjdOrderType : PX.Data.BQL.BqlString.Field<adjdOrderType> { }
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		[PXSelector(typeof(Search<SOOrderType.orderType>), CacheGlobal = true)]
		public virtual String AdjdOrderType
		{
			get;
			set;
		}
		#endregion
		#region adjdOrderNbr
		public abstract class adjdOrderNbr : PX.Data.BQL.BqlString.Field<adjdOrderNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<adjdOrderType>>>>), DirtyRead = true)]
		public virtual String AdjdOrderNbr
		{
			get;
			set;
		}
		#endregion

		#region InvoiceCntr
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? InvoiceCntr
		{
			get;
			set;
		}
		public abstract class invoiceCntr : PX.Data.BQL.BqlInt.Field<invoiceCntr> { }
		#endregion
		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		[PXDBString(3, IsFixed = true, InputMask = "")]
		[PXUIField(DisplayName = Messages.DocType, Visibility = PXUIVisibility.Visible)]
		[ARInvoiceType.AdjdList]
		public virtual string AdjdDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Current<adjdDocType>>>>), DirtyRead = true)]
		public virtual string AdjdRefNbr
		{
			get;
			set;
		}
		#endregion

		#region System Fields
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#endregion // System Fields
	}
}
