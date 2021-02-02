using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR
{
	public class PRCreateLiabilitiesAPBill : PXGraph<PRCreateLiabilitiesAPBill>
	{
		public override bool IsDirty => false;
		public PRCreateLiabilitiesAPBill()
		{
			var currentFilter = Filter.Current;
			Details.SetProcessDelegate(
				delegate (List<PayCheckDetail> detail)
				{
					Process(currentFilter, detail);
				}
			);
		}

		public PXCancel<PayCheckDetailFilter> Cancel;

		public PXFilter<PayCheckDetailFilter> Filter;
		public PXFilteredProcessing<PayCheckDetail, PayCheckDetailFilter> Details;

		public SelectFrom<PRDeductionDetail>
			.InnerJoin<PRDeductCode>.On<PRDeductionDetail.codeID.IsEqual<PRDeductCode.codeID>>
			.InnerJoin<PRPayment>.On<PRDeductionDetail.paymentDocType.IsEqual<PRPayment.docType>
				.And<PRDeductionDetail.paymentRefNbr.IsEqual<PRPayment.refNbr>>>
			.Where<PRDeductionDetail.released.IsEqual<True>
				.And<PRDeductionDetail.apInvoiceRefNbr.IsNull>
				.And<PRPayment.voided.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<PRPayment.hasUpdatedGL.IsEqual<True>>>.View DeductionDetails;

		public SelectFrom<PRBenefitDetail>
			.InnerJoin<PRDeductCode>.On<PRBenefitDetail.codeID.IsEqual<PRDeductCode.codeID>>
			.InnerJoin<PRPayment>.On<PRBenefitDetail.paymentDocType.IsEqual<PRPayment.docType>
				.And<PRBenefitDetail.paymentRefNbr.IsEqual<PRPayment.refNbr>>>
			.Where<PRBenefitDetail.released.IsEqual<True>
				.And<PRBenefitDetail.apInvoiceRefNbr.IsNull>
				.And<PRPayment.voided.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<PRPayment.hasUpdatedGL.IsEqual<True>>>.View BenefitDetails;

		public SelectFrom<PRTaxDetail>
			.InnerJoin<PRTaxCode>.On<PRTaxDetail.taxID.IsEqual<PRTaxCode.taxID>>
			.InnerJoin<PRPayment>.On<PRTaxDetail.paymentDocType.IsEqual<PRPayment.docType>
				.And<PRTaxDetail.paymentRefNbr.IsEqual<PRPayment.refNbr>>>
			.Where<PRTaxDetail.released.IsEqual<True>
				.And<PRTaxDetail.apInvoiceRefNbr.IsNull>
				.And<PRPayment.voided.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<PRPayment.hasUpdatedGL.IsEqual<True>>>.View TaxDetails;

		public class EmployeeDeduction : SelectFrom<PREmployeeDeduct>.
			Where<PREmployeeDeduct.bAccountID.IsEqual<P.AsInt>.
			And<PREmployeeDeduct.codeID.IsEqual<P.AsInt>>>
		{ }

		public static void Process(PayCheckDetailFilter filter, List<PayCheckDetail> list)
		{
			var apGraph = PXGraph.CreateInstance<APInvoiceEntry>();
			var payCheckGraph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();

			if (filter.SingleLineInvoices == true)
			{
				foreach (var detail in list)
				{
					try
					{
						if (AnyExistingVoidChecks(list.IndexOf(detail), payCheckGraph, detail.RefNbr))
						{
							continue;
						}
						if (detail.VendorID == null)
						{
							throw new PXException(Messages.VendorRequired);
						}

						using (var scope = new PXTransactionScope())
						{
							APInvoice apDoc = CreateAPInvoice(filter.DocDate, detail);
							apDoc = apGraph.Document.Insert(apDoc);

							APTran apDetail = CreateAPDetail(payCheckGraph, detail);
							if (filter.CreateZeroAmountLines == true || detail.Amount > 0)
							{
								apGraph.Transactions.Insert(apDetail);
							}
							var detailsInAPBill = new List<PayCheckDetail>() { detail };
							apDoc.InvoiceNbr = GetVendorRef(apGraph, apDoc, detailsInAPBill);
							apGraph.Actions.PressSave();

							AssociateDetailsToAPInvoice(payCheckGraph, apDoc, detailsInAPBill);
							scope.Complete();
						}
					}
					catch (Exception ex)
					{
						PXProcessing<PayCheckDetail>.SetError(list.IndexOf(detail), ex);
						continue;
					}
				}
			}
			else
			{
				var groupedDetails = list.GroupBy(x => (x.BranchID, x.VendorID));
				foreach (var group in groupedDetails)
				{
					using (var scope = new PXTransactionScope())
					{
						bool hasException = false;
						PayCheckDetail problematicRecord = null;
						var detailsInAPBill = new List<PayCheckDetail>();
						APInvoice apDoc = null;
						var isNewInvoice = true;
						foreach (var detail in group)
						{
							try
							{
								PXProcessing<PayCheckDetail>.SetCurrentItem(detail);
								if (AnyExistingVoidChecks(list.IndexOf(detail), payCheckGraph, detail.RefNbr))
								{
									continue;
								}
								if (detail.VendorID == null)
								{
									throw new PXException(Messages.VendorRequired);
								}

								if (isNewInvoice)
								{
									apGraph.Clear();
									apDoc = CreateAPInvoice(filter.DocDate, detail);
									apDoc = apGraph.Document.Insert(apDoc);
									isNewInvoice = false;
								}

								APTran apDetail = CreateAPDetail(payCheckGraph, detail);
								if (filter.CreateZeroAmountLines == true || detail.Amount > 0)
								{
									apGraph.Transactions.Insert(apDetail);
								}
								detailsInAPBill.Add(detail);
							}
							catch (Exception ex)
							{
								problematicRecord = detail;
								PXProcessing<PayCheckDetail>.SetError(list.IndexOf(detail), ex);
								hasException = true;
								break;
							}
						}

						if (hasException)
						{
							foreach (var detail in group)
							{
								if(detail != problematicRecord)
								{
									PXProcessing<PayCheckDetail>.SetWarning(list.IndexOf(detail), Messages.SameBillRecordError);
								}
							}
						}
						else
						{
							apDoc.InvoiceNbr = GetVendorRef(apGraph, apDoc, detailsInAPBill);
							apGraph.Actions.PressSave();
							AssociateDetailsToAPInvoice(payCheckGraph, apDoc, detailsInAPBill);
							scope.Complete();
						}
					}
				}
			}
		}

		public IEnumerable details()
		{
			if (!Details.Cache.Inserted.Any_())
			{
				foreach (PXResult<PRDeductionDetail, PRDeductCode, PRPayment> result in DeductionDetails.Select())
				{
					Details.Cache.Insert(CreateDeduction(result));
				}
				foreach (PXResult<PRBenefitDetail, PRDeductCode, PRPayment> result in BenefitDetails.Select())
				{
					Details.Cache.Insert(CreateBenefit(result));
				}
				foreach (PXResult<PRTaxDetail, PRTaxCode, PRPayment> result in TaxDetails.Select())
				{
					Details.Cache.Insert(CreateTax(result));
				}
			}

			foreach (PayCheckDetail row in Details.Cache.Inserted)
			{
				if ((row.DetailType == Filter.Current.DetailType || Filter.Current.DetailType == null)
					&& (row.VendorID == Filter.Current.VendorID || Filter.Current.VendorID == null)
					&& (row.BranchID == Filter.Current.BranchID || Filter.Current.BranchID == null))
				{
					yield return row;
				}
			}
		}

		#region Helpers
		private PayCheckDetail CreateDeduction(PXResult<PRDeductionDetail, PRDeductCode, PRPayment> result)
		{
			var detail = (PRDeductionDetail)result;
			var code = (PRDeductCode)result;
			var payment = (PRPayment)result;

			return new PayCheckDetail()
			{
				RecordID = detail.RecordID,
				DocType = detail.PaymentDocType,
				RefNbr = detail.PaymentRefNbr,
				Amount = detail.Amount,
				AccountID = detail.AccountID,
				SubID = detail.SubID,
				BranchID = detail.BranchID,
				CodeID = code.CodeID,
				CodeCD = code.CodeCD,
				Description = code.Description,
				DetailType = DetailTypeListAttribute.Type.Deduction,
				EmployeeID = detail.EmployeeID,
				TransactionDate = payment.TransactionDate,
				VendorID = code.BAccountID
			};
		}

		private PayCheckDetail CreateBenefit(PXResult<PRBenefitDetail, PRDeductCode, PRPayment> result)
		{
			var detail = (PRBenefitDetail)result;
			var code = (PRDeductCode)result;
			var payment = (PRPayment)result;

			return new PayCheckDetail()
			{
				RecordID = detail.RecordID,
				DocType = detail.PaymentDocType,
				RefNbr = detail.PaymentRefNbr,
				Amount = detail.Amount,
				AccountID = detail.LiabilityAccountID,
				SubID = detail.LiabilitySubID,
				BranchID = detail.BranchID,
				CodeID = code.CodeID,
				CodeCD = code.CodeCD,
				Description = code.Description,
				DetailType = DetailTypeListAttribute.Type.Benefit,
				EmployeeID = detail.EmployeeID,
				TransactionDate = payment.TransactionDate,
				VendorID = code.BAccountID
			};
		}

		private PayCheckDetail CreateTax(PXResult<PRTaxDetail, PRTaxCode, PRPayment> result)
		{
			var detail = (PRTaxDetail)result;
			var code = (PRTaxCode)result;
			var payment = (PRPayment)result;

			return new PayCheckDetail()
			{
				RecordID = detail.RecordID,
				DocType = detail.PaymentDocType,
				RefNbr = detail.PaymentRefNbr,
				Amount = detail.Amount,
				AccountID = detail.LiabilityAccountID,
				SubID = detail.LiabilitySubID,
				BranchID = detail.BranchID,
				CodeID = code.TaxID,
				CodeCD = code.TaxCD,
				Description = code.Description,
				DetailType = DetailTypeListAttribute.Type.Tax,
				EmployeeID = detail.EmployeeID,
				TransactionDate = payment.TransactionDate,
				VendorID = code.BAccountID
			};
		}

		private static string GetLineDescription(PXGraph graph, PayCheckDetail detail)
		{
			var map = new Dictionary<string, string>();
			var invDescrType = string.Empty;
			PXCache cache = null;
			PXCache garnishmentCache = null;
			object code = null;
			object garnishment = null;
			string codeField = string.Empty;
			string codeNameField = string.Empty;
			string freeFormatField = string.Empty;

			switch (detail.DetailType)
			{
				case DetailTypeListAttribute.Type.Deduction:
				case DetailTypeListAttribute.Type.Benefit:
					code = SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<P.AsInt>>.View.Select(graph, detail.CodeID).FirstTableItems.FirstOrDefault();
					cache = graph.Caches[typeof(PRDeductCode)];
					codeField = typeof(PRDeductCode.codeCD).Name;
					codeNameField = typeof(PRDeductCode.description).Name;
					freeFormatField = typeof(PRDeductCode.vndInvDescr).Name;
					invDescrType = typeof(PRDeductCode.dedInvDescrType).Name;
					break;
				case DetailTypeListAttribute.Type.Tax:
					code = SelectFrom<PRTaxCode>.Where<PRTaxCode.taxID.IsEqual<P.AsInt>>.View.Select(graph, detail.CodeID).FirstTableItems.FirstOrDefault();
					cache = graph.Caches[typeof(PRTaxCode)];
					codeField = typeof(PRTaxCode.taxCD).Name;
					codeNameField = typeof(PRTaxCode.description).Name;
					freeFormatField = typeof(PRTaxCode.vndInvDescr).Name;
					invDescrType = typeof(PRTaxCode.taxInvDescrType).Name;
					break;
			}

			switch (cache.GetValue(code, invDescrType))
			{
				case InvoiceDescriptionType.Code:
					return cache.GetValue(code, codeField).ToString();

				case InvoiceDescriptionType.CodeAndCodeName:
					return string.Format("{0} - {1}",
						cache.GetValue(code, codeField),
						cache.GetValue(code, codeNameField));

				case InvoiceDescriptionType.CodeName:
					return cache.GetValue(code, codeNameField).ToString();

				case InvoiceDescriptionType.EmployeeGarnishDescription:
					garnishmentCache = graph.Caches[typeof(PREmployeeDeduct)];
					garnishment = EmployeeDeduction.View.Select(graph, detail.EmployeeID, detail.CodeID).FirstTableItems.FirstOrDefault();
					return garnishmentCache.GetValue(garnishment, typeof(PREmployeeDeduct.vndInvDescr).Name).ToString();

				case InvoiceDescriptionType.EmployeeGarnishDescriptionPlusPaymentDate:
					garnishmentCache = graph.Caches[typeof(PREmployeeDeduct)];
					garnishment = EmployeeDeduction.View.Select(graph, detail.EmployeeID, detail.CodeID).FirstTableItems.FirstOrDefault();
					return string.Format("{0} - {1}",
						garnishmentCache.GetValue(garnishment, typeof(PREmployeeDeduct.vndInvDescr).Name),
						detail.TransactionDate.Value.ToString(LocaleInfo.GetCulture().DateTimeFormat.ShortDatePattern));

				case InvoiceDescriptionType.FreeFormatEntry:
					return cache.GetValue(code, freeFormatField).ToString();

				case InvoiceDescriptionType.PaymentDate:
					return detail.TransactionDate.Value.ToString(LocaleInfo.GetCulture().DateTimeFormat.ShortDatePattern);

				case InvoiceDescriptionType.PaymentDateAndCode:
					return string.Format("{0} - {1}",
						detail.TransactionDate.Value.ToString(LocaleInfo.GetCulture().DateTimeFormat.ShortDatePattern),
						cache.GetValue(code, codeField));

				case InvoiceDescriptionType.PaymentDateAndCodeName:
					return string.Format("{0} - {1}",
						detail.TransactionDate.Value.ToString(LocaleInfo.GetCulture().DateTimeFormat.ShortDatePattern),
						cache.GetValue(code, codeNameField));

				default:
					return string.Empty;
			}
		}

		private static APInvoice CreateAPInvoice(DateTime? invoiceDate, PayCheckDetail detail)
		{
			var apDoc = new APInvoice();
			apDoc.DocType = APDocType.Invoice;
			apDoc.DocDate = invoiceDate;
			apDoc.VendorID = detail.VendorID;
			apDoc.DocDesc = Messages.PayrollLiabilities;
			apDoc.BranchID = detail.BranchID;
			return apDoc;
		}

		private static APTran CreateAPDetail(PXGraph graph, PayCheckDetail detail)
		{
			var apDetail = new APTran();
			apDetail.AccountID = detail.AccountID;
			apDetail.SubID = detail.SubID;
			apDetail.CuryTranAmt = detail.Amount;
			apDetail.TranDesc = GetLineDescription(graph, detail);
			apDetail.BranchID = detail.BranchID;
			return apDetail;
		}

		private static void AssociateDetailsToAPInvoice(PRPayChecksAndAdjustments payCheckGraph, APInvoice invoice, List<PayCheckDetail> detailsToMark)
		{
			foreach (var detail in detailsToMark)
			{
				var payment = payCheckGraph.Document.Search<PRPayment.refNbr>(detail.RefNbr, detail.DocType).FirstTableItems.FirstOrDefault();
				payCheckGraph.Document.Current = payment;

				switch (detail.DetailType)
				{
					case DetailTypeListAttribute.Type.Deduction:
						PRDeductionDetail deductionDetail = payCheckGraph.DeductionDetails.Search<PRDeductionDetail.recordID>(detail.RecordID).FirstTableItems.FirstOrDefault();
						deductionDetail.APInvoiceDocType = invoice.DocType;
						deductionDetail.APInvoiceRefNbr = invoice.RefNbr;
						deductionDetail.LiabilityPaid = true;
						payCheckGraph.DeductionDetails.Update(deductionDetail);
						break;
					case DetailTypeListAttribute.Type.Benefit:
						PRBenefitDetail benefitDetail = payCheckGraph.BenefitDetails.Search<PRBenefitDetail.recordID>(detail.RecordID).FirstTableItems.FirstOrDefault();
						benefitDetail.APInvoiceDocType = invoice.DocType;
						benefitDetail.APInvoiceRefNbr = invoice.RefNbr;
						benefitDetail.LiabilityPaid = true;
						payCheckGraph.BenefitDetails.Update(benefitDetail);
						break;
					case DetailTypeListAttribute.Type.Tax:
						PRTaxDetail taxDetail = payCheckGraph.TaxDetails.Search<PRTaxDetail.recordID>(detail.RecordID).FirstTableItems.FirstOrDefault();
						taxDetail.APInvoiceDocType = invoice.DocType;
						taxDetail.APInvoiceRefNbr = invoice.RefNbr;
						taxDetail.LiabilityPaid = true;
						payCheckGraph.TaxDetails.Update(taxDetail);
						break;
				}

				if (!(payCheckGraph.DeductionDetails.Select().FirstTableItems.Any(x => x.LiabilityPaid == false)
					|| payCheckGraph.BenefitDetails.Select().FirstTableItems.Any(x => x.LiabilityPaid == false)
					|| payCheckGraph.TaxDetails.Select().FirstTableItems.Any(x => x.LiabilityPaid == false)))
				{
					payment.Closed = true;
				}
				else
				{
					payment.LiabilityPartiallyPaid = true;
				}
			}

			payCheckGraph.Persist();
		}

		/// <summary>
		/// Generate AP bill Vendor Ref from details' DocType and RefNbr
		/// </summary>
		private static string GetVendorRef(PXGraph graph, APInvoice apDoc, List<PayCheckDetail> detailsInAPBill)
		{
			var groups = detailsInAPBill.GroupBy(x => new { x.DocType, x.RefNbr });
			string fullValue = string.Join(", ", groups.Select(x => $"{x.Key.DocType} {x.Key.RefNbr}"));

			PXDBStringAttribute dbStringAttr = graph.Caches[typeof(APInvoice)].GetAttributesOfType<PXDBStringAttribute>(apDoc, nameof(APInvoice.invoiceNbr)).FirstOrDefault();
			if (dbStringAttr != null && fullValue.Length > dbStringAttr.Length)
			{
				StringBuilder sb = new StringBuilder();
				const string separator = ", ";
				foreach (var paymentRef in groups)
				{
					if (sb.Length > 0)
					{
						sb.Append(separator);
					}

					string refStr = $"{paymentRef.Key.DocType} {paymentRef.Key.RefNbr}";
					if (sb.Length + refStr.Length + separator.Length + Messages.Others.Length > dbStringAttr.Length)
					{
						sb.Append(Messages.Others);
						break;
					}

					sb.Append(refStr);
				}
				return sb.ToString();
			}
			else
			{
				return fullValue;
			}
		}

		private static bool AnyExistingVoidChecks(int index, PXGraph graph, string refNbr)
		{
			if (SelectFrom<PRPayment>
				.Where<PRPayment.docType.IsEqual<PayrollType.voidCheck>
				.And<PRPayment.refNbr.IsEqual<P.AsString>>>.View.Select(graph, refNbr).Any())
			{
				PXFilteredProcessing<PayCheckDetail, PayCheckDetailFilter>.SetError(index, Messages.VoidCheckExists);
				return true;
			}

			return false;
		}
		#endregion Helpers

		#region Events
		public void _(Events.FieldUpdated<PayCheckDetail.selected> e)
		{
			var row = (PayCheckDetail)e.Row;
			if (row == null)
			{
				return;
			}

			Filter.Current.Total += row.Selected.Value ? row.Amount : -row.Amount;
			Filter.Update(Filter.Current);
		}
		#endregion Events
	}

	[Serializable]
	[PXCacheName(Messages.PayCheckDetailFilter)]
	public class PayCheckDetailFilter : IBqlTable
	{
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDate]
		[PXUnboundDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Bill Date")]
		public virtual DateTime? DocDate { get; set; }
		#endregion
		#region SingleLineInvoices
		public abstract class singleLineInvoices : PX.Data.BQL.BqlBool.Field<singleLineInvoices> { }
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Single line per invoice")]
		public virtual bool? SingleLineInvoices { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[VendorActive]
		public int? VendorID { get; set; }
		#endregion
		#region DetailType
		public abstract class detailType : PX.Data.BQL.BqlString.Field<detailType> { }
		[PXString(3)]
		[PXUIField(DisplayName = "Type")]
		[DetailTypeList]
		public string DetailType { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[GL.Branch(addDefaultAttribute: false)]
		public int? BranchID { get; set; }
		#endregion
		#region Total
		public abstract class total : PX.Data.BQL.BqlDecimal.Field<total> { }
		[PXDecimal]
		[PXUIField(DisplayName = "Total", Enabled = false)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public decimal? Total { get; set; }
		#endregion
		#region CreateZeroAmountLines
		public abstract class createZeroAmountLines : PX.Data.BQL.BqlBool.Field<createZeroAmountLines> { }
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Create zero amount lines on bill")]
		public virtual bool? CreateZeroAmountLines { get; set; }
		#endregion
	}

	[Serializable]
	[PXCacheName(Messages.PayCheckDetail)]
	public class PayCheckDetail : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		[PXInt(IsKey = true)]
		public virtual Int32? RecordID { get; set; }
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXString(3, IsKey = true)]
		[PXUIField(DisplayName = "Document Type")]
		[PayrollType.List]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXString(15, IsKey = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXSelector(typeof(Search<PRPayment.refNbr>))]
		public string RefNbr { get; set; }
		#endregion
		#region DetailType
		public abstract class detailType : PX.Data.BQL.BqlString.Field<detailType> { }
		[PXString(3)]
		[PXUIField(DisplayName = "Type")]
		[DetailTypeList]
		public string DetailType { get; set; }
		#endregion
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		[PXInt]
		public int? CodeID { get; set; }
		#endregion
		#region CodeCD
		public abstract class codeCD : PX.Data.BQL.BqlString.Field<codeCD> { }
		[PXString]
		[PXUIField(DisplayName = "Code")]
		public string CodeCD { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXString(60)]
		[PXUIField(DisplayName = "Description")]
		public string Description { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[GL.Branch]
		public int? BranchID { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[VendorActive]
		public int? VendorID { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[Employee]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>))]
		public virtual int? EmployeeID { get; set; }
		#endregion
		#region Date
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Transaction Date")]
		public virtual DateTime? TransactionDate { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PXDecimal(2)]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? Amount { get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(typeof(PayCheckDetail.branchID), DisplayName = "Account")]
		public virtual Int32? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount(typeof(PayCheckDetail.accountID), typeof(PayCheckDetail.branchID), true,
			DisplayName = "Subaccount")]
		public virtual int? SubID { get; set; }
		#endregion
	}

	public class DetailTypeListAttribute : PXStringListAttribute
	{
		public class Type
		{
			public const string Benefit = "BEN";
			public const string Deduction = "DED";
			public const string Tax = "TAX";
		}

		private static string[] _values = { Type.Benefit, Type.Deduction, Type.Tax };
		private static string[] _labels = { Messages.Benefit, Messages.Deduction, Messages.Tax };

		public DetailTypeListAttribute() : base(_values, _labels)
		{
		}
	}
}