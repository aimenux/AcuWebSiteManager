using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TaxProvider;
using PX.Objects.Common.Extensions;
using PX.Api.Helpers;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using System.Reflection;
using PX.Objects.CM;

namespace PX.Objects.SO
{
	public class SOOrderEntryExternalTaxImport : PXGraphExtension<SOOrderEntryExternalTax, SOOrderEntry>
	{
		[PXVirtualDAC]
		public PXFilter<SOTaxTranImported> ImportedTaxes;

		public override void Initialize()
		{
			base.Initialize();
			typeof(PX.Data.MassProcess.FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);
		}

		#region SOTaxTran Events
		protected virtual void _(Events.RowInserting<SOTaxTran> e)
		{
			if (e.Row == null) return;
			
			SOTaxTran taxTran = e.Row as SOTaxTran;
			var soorder = Base.Document.Current;

			if (e.ExternalCall == true && soorder != null && soorder.ExternalTaxesImportInProgress == true && e.Cache.Graph.IsContractBasedAPI)
			{
				SOTaxTranImported importedTaxTran = (SOTaxTranImported)ImportedTaxes.Cache.CreateInstance();
				Base.Taxes.Cache.RestoreCopy(importedTaxTran, taxTran);

				ImportedTaxes.Insert(importedTaxTran);

				//Do not insert tax if it has already been inserted automatically
				//TaxAmount and TaxableAmount will be updated later
				foreach (SOTaxTran tax in Base.Taxes.Cache.Inserted)
				{
					if (tax.TaxID == taxTran.TaxID)
					{
						e.Cancel = true;
						break;
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<SOTaxTran, SOTaxTran.taxID> e)
		{
			if (e.Row == null)
				return;

			var soorder = Base.Document.Current;
			var taxZone = Base.taxzone.Current;

			if (soorder != null && taxZone != null && soorder.ExternalTaxesImportInProgress == true && taxZone.IsExternal == true)
			{
				e.Cancel = true;
			}
		}
		#endregion

		[PXOverride]
		public virtual void InsertImportedTaxes()
		{
			SOOrder order = Base.Document.Current;

			if (order != null && order.ExternalTaxesImportInProgress == true && ImportedTaxes.Cache.Inserted.Any_() == true &&
				Base.IsContractBasedAPI && !Base1.skipExternalTaxCalcOnSave && Base.Document.Current != null &&
				!Base.IsTransferOrder && !Base.RecalculateExternalTaxesSync)
			{
				TaxZone taxZone = Base.taxzone.Current;
				bool isExternalTaxZone = Base.taxzone.Current != null && Base.taxzone.Current.IsExternal == true;

				GetTaxResult result = new GetTaxResult();
				List<PX.TaxProvider.TaxLine> taxLines = new List<PX.TaxProvider.TaxLine>();
				List<PX.TaxProvider.TaxDetail> taxDetails = new List<PX.TaxProvider.TaxDetail>();
				decimal totalTaxAmount = 0m;

				try
				{
					//All exisitng taxes will be deleted. Imported taxes will be inserted as-is, ignoring all internal tax calculation rules
					if (isExternalTaxZone)
					{
						//Mimicking Avalara behavior - it produces negative tax amounts for receipt operations. Sign will be changed back to positive later.
						var sign = order.DefaultOperation == SOOperation.Receipt ? Sign.Minus : Sign.Plus;

						foreach (SOTaxTranImported taxTran in ImportedTaxes.Cache.Inserted)
						{
							decimal taxableAmount = sign * taxTran.CuryTaxableAmt ?? 0m;
							decimal taxAmount = sign * taxTran.CuryTaxAmt ?? 0m;
							decimal rate = !taxTran.TaxRate.IsNullOrZero() ? (taxTran.TaxRate ?? 0m) :
									(taxTran.CuryTaxableAmt.IsNullOrZero() ? 0m :
									Decimal.Round((taxTran.CuryTaxAmt ?? 0m) / (taxTran.CuryTaxableAmt ?? 1m), 6));

							PX.TaxProvider.TaxDetail taxDetail = new TaxProvider.TaxDetail
							{
								TaxName = taxTran.TaxID,
								TaxableAmount = taxableAmount,
								TaxAmount = taxAmount,
								Rate = rate
							};

							if (taxTran.LineNbr == 32000)
							{
								PX.TaxProvider.TaxLine taxLine = new TaxProvider.TaxLine
								{
									Index = short.MinValue,
									TaxableAmount = taxableAmount,
									TaxAmount = taxAmount,
									Rate = rate
								};
								taxLines.Add(taxLine);
							}

							totalTaxAmount += taxTran.CuryTaxAmt ?? 0m;

							taxDetails.Add(taxDetail);
						}
						result.TaxSummary = taxDetails.ToArray();
						result.TotalTaxAmount = sign * totalTaxAmount;

						ImportedTaxes.Cache.Clear();

						Base1.ApplyExternalTaxes(order, result, result, result);
					}
					//Exisitng taxes will be preserved. Taxable and tax amounts on exisiting taxes will be updated using imported values. 
					//Exception will be thrown in case any of imported taxes was not inserted properly.
					else
					{
						List<KeyValuePair<string, Dictionary<string, string>>> errors = new List<KeyValuePair<string, Dictionary<string, string>>>();
						foreach (SOTaxTran tax in Base.Taxes.Cache.Cached)
						{
							PXEntryStatus status = Base.Taxes.Cache.GetStatus(tax);
							Dictionary<string, string> lineErrors = PXUIFieldAttribute.GetErrors(Base.Taxes.Cache, tax);
							if (lineErrors.Count != 0)
								errors.Add(new KeyValuePair<string, Dictionary<string, string>>(tax.TaxID, lineErrors));
						}
						if (errors.Any())
						{
							string errorMessage = string.Empty;
							foreach (KeyValuePair<string, Dictionary<string, string>> error in errors)
							{
								errorMessage += string.Format(Messages.TaxWasNotImported, error.Key, error.Value.Select(x => x.Value).Aggregate((e1, e2) => e1 + "; " + e2)) + " ";
							}
							throw new PXException(errorMessage);
						}

						TaxBaseAttribute.SetTaxCalc<SOLine.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

						foreach (SOTaxTran taxTran in Base.Taxes.Select())
						{
							SOTaxTranImported matchingTax = null;
							foreach (SOTaxTranImported importedTax in ImportedTaxes.Cache.Inserted)
							{
								if (importedTax.TaxID == taxTran.TaxID)
								{
									matchingTax = importedTax;
								}
							}

							if (matchingTax != null)
							{
								if (matchingTax.CuryTaxableAmt != null)
									taxTran.CuryTaxableAmt = matchingTax.CuryTaxableAmt;
								if (matchingTax.CuryTaxAmt != null)
									taxTran.CuryTaxAmt = matchingTax.CuryTaxAmt;
								Base.Taxes.Update(taxTran);
							}
							else
							{
								Base.Taxes.Delete(taxTran);
							}
						}
					}
				}
				finally
				{
					ImportedTaxes.Cache.Clear();
				}
			}
		}
	}

	[System.SerializableAttribute()]
	[PXVirtual]
	[PXBreakInheritance]
	public partial class SOTaxTranImported : SOTaxTran
	{
		#region TaxID
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), DirtyRead = true, ValidateValue = false)]
		public override String TaxID { get; set; }
		#endregion

		#region CuryTaxableAmt
		public new abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public new Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
	}
}
