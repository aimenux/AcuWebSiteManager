using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.DAC.Unbound
{
	[PXBreakInheritance]
	[PXCacheName(Messages.SOPaymentProcessResult)]
	[PXVirtual]
	public class SOPaymentProcessResult : ARPayment
	{
		#region FundHoldExpDate
		public abstract class fundHoldExpDate : PX.Data.BQL.BqlDateTime.Field<fundHoldExpDate> { }
		[PXDateAndTime]
		[PXUIField(DisplayName = "Funds Hold Expiration Date", Enabled = false)]
		public virtual DateTime? FundHoldExpDate
		{
			get;
			set;
		}
		#endregion
		#region ProcessingStatus
		public abstract class relatedTranProcessingStatus : PX.Data.BQL.BqlString.Field<relatedTranProcessingStatus> { }
		[PXString]
		[ExtTransactionProcStatusCode.List]
		[PXUIField(DisplayName = "Proc. Status", Enabled = false)]
		public virtual string RelatedTranProcessingStatus
		{
			get;
			set;
		}
		#endregion
		#region RelatedDocument
		public abstract class relatedDocument : PX.Data.BQL.BqlString.Field<relatedDocument>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public const string SalesOrder = nameof(SalesOrder);
				public const string Invoice = nameof(Invoice);

				public ListAttribute() : base(new string[] { SalesOrder, Invoice }, new string[] { Messages.SOName, Messages.INName }) { }
			}

		}
		[PXString]
		[relatedDocument.List]
		[PXUIField(DisplayName = "Document Type")]
		public string RelatedDocument
		{
			get;
			set;
		}
		#endregion
		#region RelatedDocumentType
		public abstract class relatedDocumentType : PX.Data.BQL.BqlString.Field<relatedDocumentType> { }
		[PXString]
		[PXUIField(DisplayName = "Document Type")]
		public string RelatedDocumentType
		{
			get;
			set;
		}
		#endregion

		#region RelatedDocumentNumber
		public abstract class relatedDocumentNumber : PX.Data.BQL.BqlString.Field<relatedDocumentNumber> { }
		[PXString]
		[PXUIField(DisplayName = "Document Number")]
		public string RelatedDocumentNumber
		{
			get;
			set;
		}
		#endregion
		#region RelatedDocumentStatus
		public abstract class relatedDocumentStatus : PX.Data.BQL.BqlString.Field<relatedDocumentNumber> { }
		[PXString]
		[PXUIField(DisplayName = "Document Status")]
		public string RelatedDocumentStatus
		{
			get;
			set;
		}
		#endregion
		#region RelatedDocumentCuryInfoID
		public abstract class relatedDocumentCuryInfoID : PX.Data.BQL.BqlLong.Field<relatedDocumentCuryInfoID> { }
		[PXLong()]
		[CurrencyInfo()]
		public virtual Int64? RelatedDocumentCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region RelatedDocumentAppliedAmount
		public abstract class relatedDocumentAppliedAmount : PX.Data.BQL.BqlDecimal.Field<relatedDocumentAppliedAmount> { }
		[PXBaseCury()]
		public decimal? RelatedDocumentAppliedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryRelatedDocumentAppliedAmount
		public abstract class curyRelatedDocumentAppliedAmount : PX.Data.BQL.BqlDecimal.Field<curyRelatedDocumentAppliedAmount> { }
		[PXCurrency(typeof(relatedDocumentCuryInfoID), typeof(relatedDocumentAppliedAmount))]
		[PXUIField(DisplayName = "Applied Amount")]
		public decimal? CuryRelatedDocumentAppliedAmount
		{
			get;
			set;
		}
		#endregion
		#region RelatedDocumentCreditTerms
		public abstract class relatedDocumentCreditTerms : PX.Data.BQL.BqlString.Field<relatedDocumentCreditTerms> { }
		[PXString]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[PXUIField(DisplayName = "Credit Terms")]
		public string RelatedDocumentCreditTerms
		{
			get;
			set;
		}
		#endregion
		#region ErrorDescription
		public abstract class errorDescription : PX.Data.BQL.BqlString.Field<errorDescription> { }
		[PXString]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[PXUIField(DisplayName = "Error Description")]
		public string ErrorDescription
		{
			get;
			set;
		}
		#endregion
	}
}
