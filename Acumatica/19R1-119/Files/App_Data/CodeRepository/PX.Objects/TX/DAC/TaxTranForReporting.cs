using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.TX.DAC
{
	/// <summary>
	/// The extention of the <see cref="TaxTran"/> record for reporting with selectors and some additional fields. 
	/// </summary>
	public class TaxTranForReporting : TaxTran
	{
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[Vendor(typeof(Search<Vendor.bAccountID, Where<Vendor.taxAgency, Equal<boolTrue>>>),
						DisplayName = "Tax Agency",
						Visibility = PXUIVisibility.SelectorVisible,
						DescriptionField = typeof(Vendor.acctName))]
		public override Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region TaxPeriodID
		public new abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }

		[GL.FinPeriodID()]
		[PXSelector(typeof(Search<TaxPeriod.taxPeriodID,
								Where<TaxPeriod.vendorID, Equal<Current<TaxTranReport.vendorID>>,
										Or<Current<TaxTranReport.vendorID>, IsNull>>>))]
		public override String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
		#region TaxID
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }

		[PXDBString(Tax.taxID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Tax.taxID,
							Where<Tax.taxVendorID, Equal<Current<TaxTranReport.vendorID>>,
									Or<Current<TaxTranReport.vendorID>, IsNull>>>))]
		public override String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion

		#region Additional

		#region TranTypeInvoiceDiscriminated
		public abstract class tranTypeInvoiceDiscriminated : PX.Data.BQL.BqlString.Field<tranTypeInvoiceDiscriminated> { }

		protected String _TranTypeInvoiceDiscriminated;

		[PXString]
		[LabelList(typeof(TaxTranReport.tranTypeInvoiceDiscriminated))]
		[PXDBCalced(typeof(Switch<
			Case<
				Where<TaxTran.module, Equal<BatchModule.moduleAP>,
					And<TaxTran.tranType, Equal<APDocType.invoice>>>,
				TaxTranReport.tranTypeInvoiceDiscriminated.apInvoice,
			Case<
				Where<TaxTran.module, Equal<BatchModule.moduleAR>,
					And<TaxTran.tranType, Equal<ARDocType.invoice>>>,
				TaxTranReport.tranTypeInvoiceDiscriminated.arInvoice>>,
			TaxTran.tranType>),
			typeof(string))]
		[PXUIField(DisplayName = "Tran. Type")]
		public virtual String TranTypeInvoiceDiscriminated
		{
			get
			{
				return this._TranTypeInvoiceDiscriminated;
			}
			set
			{
				this._TranTypeInvoiceDiscriminated = value;
			}
		}

		#endregion
		#region Sign
		public abstract class sign : PX.Data.BQL.BqlDecimal.Field<sign> { }
		protected decimal? _Sign;

		/// <summary>
		/// Sign of TaxTran with which it will adjust net tax amount.
		/// Consists of following multipliers:
		/// - Tax type of TaxTran:
		///		- Sales (Output): 1
		///		- Purchase (Input): -1
		/// - Document type and module:
		///		- AP
		///			- Debit Adjustment, Voided Quick Check, Refund: -1  
		///			- Invoice, Credit Adjustment, Quik Check, Voided Check, any other not listed: 1  
		///		- AR
		///			- Credit Memo, Cash Return: -1  
		///			- Invoice, Debit Memo, Fin Charge, Cash Sale, any other not listed: 1 
		///		- GL
		///			- Reversing GL Entry: -1  
		///			- GL Entry, any other not listed: 1   
		///		- CA: 1 
		///		- Any other not listed combinations: -1
		/// </summary>
		[PXDecimal]
		public virtual decimal? Sign
		{
			[PXDependsOnFields(typeof(module), typeof(tranType), typeof(taxType))]
			get
			{
				return ReportTaxProcess.GetMult(this._Module, this._TranType, this._TaxType, 1);
			}
			set
			{
				this._Sign = value;
			}
		}
		#endregion

		#endregion
	}
}
