using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.Common;
using System.Linq;

namespace PX.Objects.AP
{
	[PXProjection(typeof(
				Select2<APPrintCheckDetail,
					
					LeftJoin<APInvoice, On<APPrintCheckDetail.source, Equal<AdjustmentGroupKey.AdjustmentType.aPAdjustment>,
						And<APInvoice.docType, Equal<APPrintCheckDetail.adjdDocType>,
							And<APInvoice.refNbr, Equal<APPrintCheckDetail.adjdRefNbr>>>>,
					
					LeftJoin<POOrder, On<APPrintCheckDetail.source, Equal<AdjustmentGroupKey.AdjustmentType.pOAdjustment>,
						And<POOrder.orderType, Equal<APPrintCheckDetail.adjdDocType>,
							And<POOrder.orderNbr, Equal<APPrintCheckDetail.adjdRefNbr>>>>>>>

				), Persistent = false)]
	[Serializable]
	[PXCacheName(Messages.APPrintCheckDetailWithAdjdDoc)]
	public class APPrintCheckDetailWithAdjdDoc : IBqlTable
	{
		#region AdjgFields

		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(APPrintCheckDetail.adjgDocType))]
		[PXDefault()]
		[APDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PXFieldDescription]
		public virtual String AdjgDocType
		{
			get;
			set;
		}
		#endregion
		
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, BqlField = typeof(APPrintCheckDetail.adjgRefNbr))]
		[PXDefault()]
		public virtual String AdjgRefNbr
		{
			get;
			set;
		}
		#endregion

		#region CuryAdjgAmt
		public abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }
		[PXDecimal(4)]
		[PXDBCalced(typeof(IIf<Where<APPrintCheckDetail.source, Equal<AdjustmentGroupKey.AdjustmentType.outstandingBalance>>,
			APPrintCheckDetail.outstandingBalance, APPrintCheckDetail.curyAdjgAmt>), typeof(decimal))]
		public virtual decimal? CuryAdjgAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryAdjgDiscAmt
		public abstract class curyAdjgDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgDiscAmt> { }
		[PXDBDecimal(4, BqlField = typeof(APPrintCheckDetail.curyAdjgDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryAdjgDiscAmt
		{
			get;
			set;
		}
		#endregion

		#endregion

		#region StubNbr
		public abstract class stubNbr : PX.Data.BQL.BqlString.Field<stubNbr> { }
		[PXDBString(40, IsUnicode = true, BqlField = typeof(APPrintCheckDetail.stubNbr))]
		public string StubNbr { get; set; }
		#endregion

		#region Adjd fields

		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		[PXString(15, IsKey = true, IsUnicode = true)]
		[PXDBCalced(typeof(IsNull<APInvoice.refNbr, POOrder.orderNbr>), typeof(string))]
		public virtual string AdjdRefNbr
		{
			get;
			set;
		}
		#endregion

		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType>
		{
			public const int Length = 3;

			public class ListAttribute : LabelListAttribute
			{
				public ListAttribute()
					: base(new APDocType.ListAttribute().ValueLabelDic
						  .Select(k => new ValueLabelPair(k.Key, k.Value)).Union(
							new ValueLabelPair[]
							{
								new ValueLabelPair(POOrderType.RegularOrder, PO.Messages.POOrderDocTypeOnPrepaymentCheck),
								new ValueLabelPair(POOrderType.DropShip, PO.Messages.POOrderDocTypeOnPrepaymentCheck),
							}))
				{
				}
			}
		}
		[PXString(adjdDocType.Length, IsKey = true)]
		[PXDBCalced(typeof(IsNull<APInvoice.docType, POOrder.orderType>), typeof(string))]
		[adjdDocType.List()]
		public virtual string AdjdDocType
		{
			get;
			set;
		}
		#endregion

		#region AdjdPrintDocType
		public abstract class adjdPrintDocType : PX.Data.BQL.BqlString.Field<adjdPrintDocType>
		{
			public class PrintListAttribute : LabelListAttribute
			{
				public PrintListAttribute()
					: base(new APDocType.PrintListAttribute().ValueLabelDic
						  .Select(k => new ValueLabelPair(k.Key, k.Value)).Union(
							new ValueLabelPair[]
							{
								new ValueLabelPair(POOrderType.RegularOrder, PO.Messages.POOrderDocTypeOnPrepaymentCheck),
								new ValueLabelPair(POOrderType.DropShip, PO.Messages.POOrderDocTypeOnPrepaymentCheck),
							}))
				{
				}
			}
		}
		[PXString(20)]
		[adjdPrintDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual string AdjdPrintDocType
		{
			get => AdjdDocType;
			set { }
		}
		#endregion

		#region AdjdDocNbr
		public abstract class adjdDocNbr : PX.Data.BQL.BqlString.Field<adjdDocNbr> { }
		[PXString(40, IsUnicode = true)]
		[PXDBCalced(typeof(IsNull<APInvoice.invoiceNbr, POOrder.vendorRefNbr>), typeof(string))]
		public virtual string AdjdDocNbr
		{
			get;
			set;
		}
		#endregion

		#region AdjdDocDate
		public abstract class adjdDocDate : PX.Data.BQL.BqlDateTime.Field<adjdDocDate> { }
		[PXDate()]
		[PXDBCalced(typeof(IIf<Where<APPrintCheckDetail.source, Equal<AdjustmentGroupKey.AdjustmentType.outstandingBalance>>, APPrintCheckDetail.outstandingBalanceDate,
			IsNull<APInvoice.docDate, POOrder.orderDate>>), typeof(DateTime))]
		public virtual DateTime? AdjdDocDate
		{
			get;
			set;
		}
		#endregion

		#region AdjdInvtMult
		public abstract class adjdInvtMult : PX.Data.BQL.BqlShort.Field<adjdInvtMult> { }
		[PXShort]
		[PXDBCalced(typeof(IIf<
			Where<APInvoice.docType, Equal<APInvoiceType.debitAdj>>,
			shortMinus1,
			short1>), typeof(short))]
		public virtual short? AdjdInvtMult
		{
			get;
			set;
		}
		#endregion

		#region AdjdCuryOrigDocAmt
		public abstract class adjdCuryOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<adjdCuryOrigDocAmt> { }
		[PXDecimal(4)]
		[PXDBCalced(typeof(IIf<Where<APPrintCheckDetail.source, Equal<AdjustmentGroupKey.AdjustmentType.outstandingBalance>>, APPrintCheckDetail.outstandingBalance,
			IsNull<APInvoice.curyOrigDocAmt, POOrder.orderTotal>>), typeof(decimal))]
        public virtual decimal? AdjdCuryOrigDocAmt
		{
			get;
			set;
		}
		#endregion

		#region AdjdCuryDocBal
		public abstract class adjdCuryDocBal : PX.Data.BQL.BqlDecimal.Field<adjdCuryDocBal> { }
		[PXDecimal(4)]
		[PXDBCalced(typeof(Add<IsNull<APInvoice.curyDocBal, POOrder.curyUnprepaidTotal>, APPrintCheckDetail.curyExtraDocBal>), typeof(decimal))]
		public virtual decimal? AdjdCuryDocBal
		{
			get;
			set;
		}
		#endregion

		#region AdjdSuppliedByVendorID
		public abstract class adjdSuppliedByVendorID : PX.Data.BQL.BqlInt.Field<adjdSuppliedByVendorID> { }
		[PXInt]
		[PXDBCalced(typeof(IsNull<APInvoice.suppliedByVendorID, POOrder.vendorID>), typeof(int))]
		public virtual int? AdjdSuppliedByVendorID
		{
			get;
			set;
		}
		#endregion

		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }
		[PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(APPrintCheckDetail.source))]
		public string Source { get; set; }
		#endregion

		#region OrderBy
		public abstract class orderBy : PX.Data.BQL.BqlShort.Field<orderBy> { }
		[PXShort()]
		[PXDBCalced(typeof(Switch<
			Case<Where<APPrintCheckDetail.source, Equal<AdjustmentGroupKey.AdjustmentType.aPAdjustment>>, short0,
			Case<Where<APPrintCheckDetail.source, Equal<AdjustmentGroupKey.AdjustmentType.pOAdjustment>>, short1>>,
			short2>), typeof(short))]
		public short? OrderBy { get; set; }
		#endregion
	}
}
