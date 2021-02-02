using System;
using System.Linq;
using PX.Data;
using System.Collections.Generic;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.Common.Extensions;

namespace PX.Objects.TX
{
	public class SalesTaxMaint : PXGraph<SalesTaxMaint>
	{		
		public PXSave<Tax> Save;
		public PXCancel<Tax> Cancel;
		public PXInsert<Tax> Insert;
		public PXDelete<Tax> Delete;
		public PXFirst<Tax> First;
		public PXPrevious<Tax> Previous;
		public PXNext<Tax> Next;
		public PXLast<Tax> Last;
		
		public PXSelect<Tax> Tax;

		public PXSelect<TaxRev, 
				Where<TaxRev.taxID,Equal<Current<Tax.taxID>>>,
				OrderBy<
					Asc<TaxRev.taxType, 
					Desc<TaxRev.startDate>>>> 
				TaxRevisions;

		public PXSelectJoin<TaxCategoryDet, 
				LeftJoin<TaxCategory, 
					On<TaxCategory.taxCategoryID, Equal<TaxCategoryDet.taxCategoryID>>>, 
				Where<TaxCategoryDet.taxID, Equal<Current<Tax.taxID>>>> 
				Categories;

		public PXSelectJoin<TaxZoneDet, 
				LeftJoin<TaxZone, 
					On<TaxZone.taxZoneID, Equal<TaxZoneDet.taxZoneID>>>, 
				Where<TaxZoneDet.taxID, Equal<Current<Tax.taxID>>>> 
				Zones;

		public PXSelectReadonly<TaxCategory> Category;

		public PXSelectReadonly<TaxZone> Zone;

		protected virtual void Tax_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Tax newTax = (Tax)e.Row;
			Tax oldTax = (Tax)e.OldRow;

			ClearOldWarningsAndErrors(sender, newTax);
			CheckAndFixTaxRates(newTax);
			SetWarningsOnRowUpdate(sender, newTax, oldTax);
			ProcessTaxRevOnTaxVendorChangeOnTaxUpdate(sender, newTax, oldTax);
			VerifyTaxRevReportingGroupTypesOnTaxUpdate(newTax, oldTax);

			if (!sender.ObjectsEqual<Tax.outDate>(e.Row, e.OldRow))
			{
				PXSelectBase<TaxRev> TaxRevFiltered = new 
					PXSelect<TaxRev, 
					Where<TaxRev.taxID, Equal<Current<Tax.taxID>>, 
						And<TaxRev.startDate, Greater<Required<Tax.outDate>>>>>(this);

				if (newTax.OutDate.HasValue && TaxRevFiltered.Select(newTax.OutDate).Count > 0)
				{
					Tax.Cache.RaiseExceptionHandling<Tax.outDate>(newTax, newTax.OutDate,
						new PXSetPropertyException(Messages.TaxOutDateTooEarly, PXErrorLevel.Warning));
				}
				else
				{
					sender.SetValue<Tax.outdated>(e.Row, newTax.OutDate.HasValue);
					DateTime newEndDate = newTax.OutDate ?? new DateTime(9999, 6, 6);

					foreach (TaxRev rev in TaxRevisions.Select())
					{
						TaxRev revcopy = (TaxRev)TaxRevisions.Cache.CreateCopy(rev);
						revcopy.EndDate = newEndDate;
						TaxRevisions.Cache.Update(revcopy);
					}
				}
			}

			if (!sender.ObjectsEqual<Tax.reportExpenseToSingleAccount>(e.Row, e.OldRow) && newTax.ReportExpenseToSingleAccount == false)
			{
				sender.SetValue<Tax.expenseAccountID>(newTax, null);
				sender.SetValue<Tax.expenseSubID>(newTax, null);
			}

			if (!sender.ObjectsEqual<Tax.reportExpenseToSingleAccount>(e.Row, e.OldRow) && newTax.ReportExpenseToSingleAccount == true)
			{
				sender.SetDefaultExt<Tax.expenseAccountID>(e.Row);
				sender.SetDefaultExt<Tax.expenseSubID>(e.Row);
			}
		}

		/// <summary>
		/// Populate values list for the combobox on <see cref="TaxRev.TaxBucketID"/> field.
		/// </summary>
		/// <param name="tax">The tax.</param>
		public virtual void PopulateBucketList(Tax tax)
		{
			List<int> allowedValues = new List<int>();
			List<string> allowedLabels = new List<string>();

			List<int> defaultAllowedValues = new List<int>(new int[] { 0 });
			List<string> defaultAllowedLabels = new List<string>(new string[] { "undefined" });

			switch (tax.TaxType)
			{
				case CSTaxType.VAT:
					FillAllowedValuesAndLabelsForVatTax(tax, allowedValues, allowedLabels);
					break;
				case CSTaxType.Sales:
				case CSTaxType.Use:
				case CSTaxType.Withholding:
					if (tax.TaxVendorID == null)
					{
						allowedValues.Add(-2);
						allowedLabels.Add(Messages.DefaultOutputGroup);
					}
					else
					{
						foreach (TaxBucket bucket in 
							PXSelectReadonly<TaxBucket, 
							Where<TaxBucket.vendorID, Equal<Required<TaxBucket.vendorID>>, 
								And<TaxBucket.bucketType, Equal<Required<TaxBucket.bucketType>>>>>
							.Select(this, tax.TaxVendorID, CSTaxBucketType.Sales))
						{
							allowedValues.Add((int)bucket.BucketID);
							allowedLabels.Add(bucket.Name);
						}
					}

					break;
			}

			if (allowedValues.Count > 0)
			{
				PXIntListAttribute.SetList<TaxRev.taxBucketID>(TaxRevisions.Cache, null, allowedValues.ToArray(), allowedLabels.ToArray());
			}
			else
			{
				PXIntListAttribute.SetList<TaxRev.taxBucketID>(TaxRevisions.Cache, null, defaultAllowedValues.ToArray(), defaultAllowedLabels.ToArray());
			}
		}

		/// <summary>
		/// Fill allowed values and labels for VAT tax on the population of values for combobox on <see cref="TaxRev.TaxBucketID"/> field. 
		/// </summary>
		/// <param name="vatTax">The VAT tax.</param>
		/// <param name="allowedValues">The allowed values.</param>
		/// <param name="allowedLabels">The allowed labels.</param>
		public virtual void FillAllowedValuesAndLabelsForVatTax(Tax vatTax, List<int> allowedValues, List<string> allowedLabels)
		{
			if (vatTax.TaxVendorID == null)
			{
				allowedValues.Add(-1);
				allowedLabels.Add(Messages.DefaultInputGroup);

				if (vatTax.DeductibleVAT == false)
				{
					allowedValues.Add(-2);
					allowedLabels.Add(Messages.DefaultOutputGroup);
				}	
			}
			else
			{
				PXResultset<TaxBucket> taxBuckets;

				if (vatTax.DeductibleVAT == true)
				{
					taxBuckets = PXSelectReadonly<TaxBucket,
									Where<TaxBucket.vendorID, Equal<Required<TaxBucket.vendorID>>,
										And<TaxBucket.bucketType, Equal<Required<TaxBucket.bucketType>>>>>
								.Select(this, vatTax.TaxVendorID, CSTaxBucketType.Purchase);
				}
				else
				{
					taxBuckets = PXSelectReadonly<TaxBucket,
									Where<TaxBucket.vendorID, Equal<Required<TaxBucket.vendorID>>>>
								 .Select(this, vatTax.TaxVendorID);
				}

				foreach (TaxBucket bucket in taxBuckets)
				{
					allowedValues.Add((int)bucket.BucketID);
					allowedLabels.Add(bucket.Name);
				}
			}
		}

		/// <summary>
		/// Clears the old warnings and errors. Called on <see cref="Tax_RowUpdated(PXCache, PXRowUpdatedEventArgs)"/>.
		/// </summary>
		/// <param name="cache">The cache.</param>
		/// <param name="newTax">The new tax.</param>
		public virtual void ClearOldWarningsAndErrors(PXCache cache, Tax newTax) 
		{
			cache.RaiseExceptionHandling<Tax.taxType>(newTax, newTax.TaxType, null);
			cache.RaiseExceptionHandling<Tax.taxCalcRule>(newTax, newTax.TaxCalcRule, null);
			cache.RaiseExceptionHandling<Tax.taxCalcLevel2Exclude>(newTax, newTax.TaxCalcLevel2Exclude, null);
			cache.RaiseExceptionHandling<Tax.reverseTax>(newTax, newTax.ReverseTax, null);
			cache.RaiseExceptionHandling<Tax.pendingTax>(newTax, newTax.PendingTax, null);
			cache.RaiseExceptionHandling<Tax.exemptTax>(newTax, newTax.ExemptTax, null);
			cache.RaiseExceptionHandling<Tax.statisticalTax>(newTax, newTax.StatisticalTax, null);
			cache.RaiseExceptionHandling<Tax.directTax>(newTax, newTax.DirectTax, null);
			cache.RaiseExceptionHandling<Tax.includeInTaxable>(newTax, newTax.IncludeInTaxable, null);
		}

		/// <summary>
		/// Check and fix tax rates. Called on <see cref="Tax_RowUpdated(PXCache, PXRowUpdatedEventArgs)"/>.
		/// </summary>
		/// <param name="newTax">The new tax.</param>
		public virtual void CheckAndFixTaxRates(Tax newTax)
		{
			if (newTax.ExemptTax != true)
				return;

			foreach (TaxRev iRev in this.TaxRevisions.Select())
			{
				if (iRev.TaxRate == null || iRev.TaxRate != 0)
				{
					iRev.TaxRate = 0;
					TaxRevisions.Cache.Update(iRev);
				}
			}
		}

		/// <summary>
		/// Sets warnings on row update. Called on <see cref="Tax_RowUpdated(PXCache, PXRowUpdatedEventArgs)"/>.
		/// </summary>
		/// <param name="cache">The cache.</param>
		/// <param name="newTax">The new tax.</param>
		/// <param name="oldTax">The old tax.</param>
		public virtual void SetWarningsOnRowUpdate(PXCache cache, Tax newTax, Tax oldTax)
		{
			bool isDeductibleSet = newTax.DeductibleVAT == true && newTax.DeductibleVAT != oldTax.DeductibleVAT;
			bool isDeductibleRemoved = newTax.DeductibleVAT == false && newTax.DeductibleVAT != oldTax.DeductibleVAT;

			if (newTax.TaxType != CSTaxType.VAT && newTax.DirectTax == true)
			{
				cache.RaiseExceptionHandling<Tax.taxType>(newTax, newTax.TaxType, new PXSetPropertyException(Messages.ThisOptionCanOnlyBeUsedWithTaxTypeVAT));
				cache.RaiseExceptionHandling<Tax.directTax>(newTax, newTax.DirectTax, new PXSetPropertyException(Messages.ThisOptionCanOnlyBeUsedWithTaxTypeVAT));
				return;
			}

			if (newTax.ExemptTax == true && newTax.DirectTax == true)
			{
				cache.RaiseExceptionHandling<Tax.exemptTax>(newTax, newTax.ExemptTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.directTax>(newTax, newTax.DirectTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.StatisticalTax == true && newTax.DirectTax == true)
			{
				cache.RaiseExceptionHandling<Tax.statisticalTax>(newTax, newTax.StatisticalTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.directTax>(newTax, newTax.DirectTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}


			if (newTax.PendingTax == true && newTax.DirectTax == true)
			{
				cache.RaiseExceptionHandling<Tax.pendingTax>(newTax, newTax.PendingTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.directTax>(newTax, newTax.DirectTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.TaxType != CSTaxType.VAT && newTax.PendingTax == true)
			{
				cache.RaiseExceptionHandling<Tax.taxType>(newTax, newTax.TaxType, new PXSetPropertyException(Messages.ThisOptionCanOnlyBeUsedWithTaxTypeVAT));
				cache.RaiseExceptionHandling<Tax.pendingTax>(newTax, newTax.PendingTax, new PXSetPropertyException(Messages.ThisOptionCanOnlyBeUsedWithTaxTypeVAT));
				return;
			}

			if (newTax.ReverseTax == true && newTax.ExemptTax == true)
			{
				cache.RaiseExceptionHandling<Tax.reverseTax>(newTax, newTax.ReverseTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.exemptTax>(newTax, newTax.ExemptTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.ReverseTax == true && newTax.StatisticalTax == true)
			{
				cache.RaiseExceptionHandling<Tax.reverseTax>(newTax, newTax.ReverseTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.statisticalTax>(newTax, newTax.StatisticalTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.PendingTax == true && newTax.StatisticalTax == true)
			{
				cache.RaiseExceptionHandling<Tax.pendingTax>(newTax, newTax.PendingTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.statisticalTax>(newTax, newTax.StatisticalTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.PendingTax == true && newTax.ExemptTax == true)
			{
				cache.RaiseExceptionHandling<Tax.pendingTax>(newTax, newTax.PendingTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.exemptTax>(newTax, newTax.ExemptTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.StatisticalTax == true && newTax.ExemptTax == true)
			{
				cache.RaiseExceptionHandling<Tax.statisticalTax>(newTax, newTax.StatisticalTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.exemptTax>(newTax, newTax.ExemptTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.TaxCalcLevel == "0" && newTax.TaxCalcLevel2Exclude == true)
			{
				cache.RaiseExceptionHandling<Tax.taxCalcRule>(newTax, newTax.TaxCalcRule, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.taxCalcLevel2Exclude>(newTax, newTax.TaxCalcLevel2Exclude, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.TaxType == CSTaxType.Use && newTax.TaxCalcLevel2Exclude == false)
			{
				cache.RaiseExceptionHandling<Tax.taxType>(newTax, newTax.TaxType, new PXSetPropertyException(Messages.TheseTwoOptionsShouldBeCombined));
				cache.RaiseExceptionHandling<Tax.taxCalcLevel2Exclude>(newTax, newTax.TaxCalcLevel2Exclude, new PXSetPropertyException(Messages.TheseTwoOptionsShouldBeCombined));
				return;
			}

			if (newTax.TaxType == CSTaxType.Use && newTax.TaxCalcLevel == "0")
			{
				cache.RaiseExceptionHandling<Tax.taxType>(newTax, newTax.TaxType, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.taxCalcRule>(newTax, newTax.TaxCalcRule, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.TaxType == CSTaxType.Withholding && newTax.TaxCalcLevel != "0")
			{
				cache.RaiseExceptionHandling<Tax.taxType>(newTax, newTax.TaxType, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.taxCalcRule>(newTax, newTax.TaxCalcRule, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.ReverseTax == true && newTax.TaxCalcLevel == "0")
			{
				cache.RaiseExceptionHandling<Tax.reverseTax>(newTax, newTax.ReverseTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.taxCalcRule>(newTax, newTax.TaxCalcRule, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.ExemptTax == true && newTax.IncludeInTaxable == true)
			{
				cache.RaiseExceptionHandling<Tax.exemptTax>(newTax, newTax.ExemptTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.includeInTaxable>(newTax, newTax.IncludeInTaxable, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.StatisticalTax == true && newTax.IncludeInTaxable == true)
			{
				cache.RaiseExceptionHandling<Tax.statisticalTax>(newTax, newTax.StatisticalTax, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.includeInTaxable>(newTax, newTax.IncludeInTaxable, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}

			if (newTax.DeductibleVAT == true && newTax.ReportExpenseToSingleAccount == false && newTax.TaxCalcType != CSTaxCalcType.Item)
			{
				cache.RaiseExceptionHandling<Tax.taxCalcRule>(newTax, newTax.TaxCalcRule, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.reportExpenseToSingleAccount>(newTax, newTax.ReportExpenseToSingleAccount,
					new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				return;
			}


			if (newTax.TaxType != CSTaxType.VAT)
			{
				cache.SetValue<Tax.purchTaxAcctID>(newTax, null);
				cache.SetValue<Tax.purchTaxSubID>(newTax, null);
				cache.SetValue<Tax.reverseTax>(newTax, false);
				cache.SetValue<Tax.pendingTax>(newTax, false);
				cache.SetValue<Tax.exemptTax>(newTax, false);
				cache.SetValue<Tax.statisticalTax>(newTax, false);
				cache.SetValue<Tax.deductibleVAT>(newTax, false);
			}

			if (newTax.PendingTax != true)
			{
				cache.SetValue<Tax.pendingSalesTaxAcctID>(newTax, null);
				cache.SetValue<Tax.pendingSalesTaxSubID>(newTax, null);
				cache.SetValue<Tax.pendingPurchTaxAcctID>(newTax, null);
				cache.SetValue<Tax.pendingPurchTaxSubID>(newTax, null);
			}

			if (newTax.TaxType != CSTaxType.Use && isDeductibleRemoved)
			{
				cache.SetValue<Tax.expenseAccountID>(newTax, null);
				cache.SetValue<Tax.expenseSubID>(newTax, null);
			}

			if (isDeductibleRemoved)
			{
				cache.SetValue<Tax.reportExpenseToSingleAccount>(newTax, true);
			}

			if (isDeductibleSet)
			{
				cache.SetDefaultExt<Tax.expenseAccountID>(newTax);
				cache.SetDefaultExt<Tax.expenseSubID>(newTax);
			}
		}

		/// <summary>
		/// Process the tax reverse on tax vendor change on tax update. Called on <see cref="Tax_RowUpdated(PXCache, PXRowUpdatedEventArgs)"/>.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="newTax">The new tax.</param>
		/// <param name="oldTax">The old tax.</param>
		public virtual void ProcessTaxRevOnTaxVendorChangeOnTaxUpdate(PXCache sender, Tax newTax, Tax oldTax)
		{
			bool taxVendorChanged = !sender.ObjectsEqual<Tax.taxVendorID>(newTax, oldTax);

			if (!taxVendorChanged)
				return;

			foreach (TaxRev rev in TaxRevisions.Select())
			{
				if (newTax.TaxVendorID == null)
				{
					rev.TaxBucketID = rev.TaxType == TaxType.Sales ? -2 : -1;
				}
				else
				{
					rev.TaxBucketID = null;
					TaxRevisions.Cache.RaiseExceptionHandling<TaxRev.taxBucketID>(rev, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
				}

					TaxRevisions.Cache.MarkUpdated(rev);
			}

			PopulateBucketList(newTax);
		}

		/// <summary>
		/// Verify tax reverse reporting group types on tax update. Called on <see cref="Tax_RowUpdated(PXCache, PXRowUpdatedEventArgs)"/>.
		/// </summary>
		/// <param name="newTax">The new tax.</param>
		/// <param name="oldTax">The old tax.</param>
		public virtual void VerifyTaxRevReportingGroupTypesOnTaxUpdate(Tax newTax, Tax oldTax)
		{
			bool isDeductibleVATSet = newTax.TaxType == CSTaxType.VAT && newTax.DeductibleVAT == true && newTax.DeductibleVAT != oldTax.DeductibleVAT;
			bool isWithholdingSet = newTax.TaxType == CSTaxType.Withholding && newTax.TaxType != oldTax.TaxType;

			if (!isDeductibleVATSet && !isWithholdingSet)
				return;

			foreach (TaxRev taxRev in TaxRevisions.Select())
			{
				var checkInfo = CheckTaxRevisionCompatibilityWithTax(taxRev, isDeductibleVATSet, isWithholdingSet);
				bool isSuccess = checkInfo.Key;

				if (isSuccess)
					continue;

				var taxRevNotCompatibleWithTaxException = new PXSetPropertyException(checkInfo.Value);
				TaxRevisions.Cache.RaiseExceptionHandling<TaxRev.taxType>(taxRev, taxRev.TaxType, taxRevNotCompatibleWithTaxException);
				TaxRevisions.Cache.RaiseExceptionHandling<TaxRev.taxBucketID>(taxRev, null, taxRevNotCompatibleWithTaxException);
			}
		}

		/// <summary>
		/// Validates the tax revision compatibility with tax. Returns a pair of values - result of the check (true is success) and error message.
		/// </summary>
		/// <param name="taxRevision">The tax revision.</param>
		/// <param name="taxIsDeductibleVAT">True if tax is deductible VAT.</param>
		/// <param name="taxIsWithholding">True if tax is withholding.</param>
		/// <returns/>
		private KeyValuePair<bool, string> CheckTaxRevisionCompatibilityWithTax(TaxRev taxRevision, bool taxIsDeductibleVAT, bool taxIsWithholding)
		{
			if (taxIsDeductibleVAT && taxRevision.TaxType == TaxType.Sales)
			{
				return new KeyValuePair<bool, string>(false, Messages.DeductibleVATWithOutputReportingGroupError);
			}
			else if (taxIsWithholding && taxRevision.TaxType == TaxType.Purchase)
			{
				return new KeyValuePair<bool, string>(false, Messages.WithholdingTaxWithInputReportingGroupError);		
			}

			return new KeyValuePair<bool, string>(true, null);
		}

		/// <summary>
		/// In a given Tax record, sets all VAT related fields to their default values.
		/// </summary>
		protected void DefaultAllVatRelatedFields(PXCache cache, Tax taxRecord)
		{
			cache.SetDefaultExt<Tax.deductibleVAT>(taxRecord);
			cache.SetDefaultExt<Tax.reverseTax>(taxRecord);
			cache.SetDefaultExt<Tax.statisticalTax>(taxRecord);
			cache.SetDefaultExt<Tax.exemptTax>(taxRecord);
			cache.SetDefaultExt<Tax.includeInTaxable>(taxRecord);
			cache.SetDefaultExt<Tax.pendingTax>(taxRecord);
			cache.SetDefaultExt<Tax.directTax>(taxRecord);
		}

		protected virtual void Tax_TaxVendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<Tax.salesTaxAcctID>(e.Row);
			sender.SetDefaultExt<Tax.salesTaxSubID>(e.Row);

			if (((Tax)e.Row).TaxType == CSTaxType.VAT)
			{
				sender.SetDefaultExt<Tax.purchTaxAcctID>(e.Row);
				sender.SetDefaultExt<Tax.purchTaxSubID>(e.Row);
			}

			if (((Tax)e.Row).PendingTax == true)
			{
				sender.SetDefaultExt<Tax.pendingSalesTaxAcctID>(e.Row);
				sender.SetDefaultExt<Tax.pendingSalesTaxSubID>(e.Row);
				sender.SetDefaultExt<Tax.pendingPurchTaxAcctID>(e.Row);
				sender.SetDefaultExt<Tax.pendingPurchTaxSubID>(e.Row);
			}

			if (((Tax)e.Row).TaxType == CSTaxType.Use)
			{
				sender.SetDefaultExt<Tax.expenseAccountID>(e.Row);
				sender.SetDefaultExt<Tax.expenseSubID>(e.Row);
			}

			if (((Tax)e.Row).DeductibleVAT == true && ((Tax)e.Row).ReportExpenseToSingleAccount == true)
			{
				sender.SetDefaultExt<Tax.expenseAccountID>(e.Row);
				sender.SetDefaultExt<Tax.expenseSubID>(e.Row);
			}

			foreach (TaxRev taxrev in TaxRevisions.View.SelectMultiBound(new object[]{e.Row}))
			{
				TaxRevisions.Cache.MarkUpdated(taxrev);
			}
		}

		protected virtual void Tax_TaxType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			Tax modifiedTaxRecord = e.Row as Tax;

			if (modifiedTaxRecord.TaxType != CSTaxType.VAT)
			{
				this.DefaultAllVatRelatedFields(sender, modifiedTaxRecord);
			}
		}

		protected virtual void Tax_PurchTaxAcctID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;
			CheckAcctIDSubID<Tax.purchTaxAcctID>(sender, e.Row, e.NewValue);
		}

		protected virtual void Tax_SalesTaxAcctID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;
			CheckAcctIDSubID<Tax.salesTaxAcctID>(sender, e.Row, e.NewValue);

		}

		protected virtual void Tax_SalesTaxSubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;
			CheckAcctIDSubID<Tax.salesTaxSubID>(sender, e.Row, e.NewValue);
			
		}

		protected virtual void Tax_PurchTaxSubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;
			CheckAcctIDSubID<Tax.purchTaxSubID>(sender, e.Row, e.NewValue);
		}

		private void CheckAcctIDSubID<Field>(PXCache sender, object row, object NewValue)
			where Field : IBqlField
		{
			if (PXAccess.FeatureInstalled<CS.FeaturesSet.taxEntryFromGL>())
			{
				Tax taxrow = (Tax)sender.CreateCopy(row as Tax);
				sender.SetValue<Field>(taxrow, NewValue);
				if (taxrow.PurchTaxAcctID == taxrow.SalesTaxAcctID && taxrow.PurchTaxSubID == taxrow.SalesTaxSubID)
				{
					sender.RaiseExceptionHandling<Tax.purchTaxAcctID>(row, null, new PXSetPropertyException(Messages.ClaimableAndPayableAccountsAreTheSame, PXErrorLevel.Warning, taxrow.TaxID));
					sender.RaiseExceptionHandling<Tax.purchTaxSubID>(row, null, new PXSetPropertyException(Messages.ClaimableAndPayableAccountsAreTheSame, PXErrorLevel.Warning, taxrow.TaxID));
					sender.RaiseExceptionHandling<Tax.salesTaxAcctID>(row, null, new PXSetPropertyException(Messages.ClaimableAndPayableAccountsAreTheSame, PXErrorLevel.Warning, taxrow.TaxID));
					sender.RaiseExceptionHandling<Tax.salesTaxSubID>(row, null, new PXSetPropertyException(Messages.ClaimableAndPayableAccountsAreTheSame, PXErrorLevel.Warning, taxrow.TaxID));
				}
			}
		}

		protected virtual void Tax_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Tax tax = e.Row as Tax;

			if (tax == null)
				return;
						
			bool isUsedForManualVAT = 
				PXSelect<TaxZone, 
					Where<TaxZone.taxID, Equal<Required<TaxZone.taxID>>>>
				.Select(this, tax.TaxID)
				.Count != 0;

			bool isOutdated = tax.OutDate.HasValue && (tax.OutDate.Value.CompareTo(Accessinfo.BusinessDate) < 0);

			if (Tax.Cache.GetStatus(tax) != PXEntryStatus.Updated)
			{
				TaxRevisions.Cache.SetAllEditPermissions(!isOutdated);
			}

			bool isExternal = tax.IsExternal ?? false;
			TaxRevisions.Cache.SetAllEditPermissions(!isExternal);
			Categories.Cache.SetAllEditPermissions(!isExternal);
			Zones.Cache.SetAllEditPermissions(!isExternal);		

			bool isVAT = tax.TaxType == CSTaxType.VAT;
			bool isPending = tax.PendingTax == true;
			bool isUse = tax.TaxType == CSTaxType.Use;
			bool isDeductible = tax.DeductibleVAT == true;
			bool isReportingExpenseToSingleAcc = isDeductible && tax.ReportExpenseToSingleAccount == true;

			PXUIFieldAttribute.SetEnabled<Tax.reverseTax>(cache, tax, isVAT && !isExternal & !isUsedForManualVAT);
			PXUIFieldAttribute.SetEnabled<Tax.pendingTax>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.directTax>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.statisticalTax>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.exemptTax>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.deductibleVAT>(cache, tax, isVAT && !isExternal & !isUsedForManualVAT);
			PXUIFieldAttribute.SetEnabled<Tax.includeInTaxable>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.purchTaxAcctID>(cache, tax, isVAT);
			PXUIFieldAttribute.SetEnabled<Tax.purchTaxSubID>(cache, tax, isVAT);
			PXUIFieldAttribute.SetRequired<Tax.purchTaxAcctID>(cache, isVAT);
			PXUIFieldAttribute.SetRequired<Tax.purchTaxSubID>(cache, isVAT);

			PXUIFieldAttribute.SetEnabled<Tax.pendingSalesTaxAcctID>(cache, tax, isPending);
			PXUIFieldAttribute.SetEnabled<Tax.pendingSalesTaxSubID>(cache, tax, isPending);
			PXUIFieldAttribute.SetEnabled<Tax.pendingPurchTaxAcctID>(cache, tax, isPending);
			PXUIFieldAttribute.SetEnabled<Tax.pendingPurchTaxSubID>(cache, tax, isPending);

			PXUIFieldAttribute.SetVisible<Tax.reportExpenseToSingleAccount>(cache, tax, isDeductible);

			PXUIFieldAttribute.SetEnabled<Tax.expenseAccountID>(cache, tax, isUse || isReportingExpenseToSingleAcc);
			PXUIFieldAttribute.SetEnabled<Tax.expenseSubID>(cache, tax, isUse || isReportingExpenseToSingleAcc);
			PXDefaultAttribute.SetPersistingCheck<Tax.expenseAccountID>(cache, tax, isReportingExpenseToSingleAcc ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<Tax.expenseSubID>(cache, tax, isReportingExpenseToSingleAcc ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<Tax.expenseAccountID>(cache, isUse || isReportingExpenseToSingleAcc);
			PXUIFieldAttribute.SetRequired<Tax.expenseSubID>(cache, isUse || isReportingExpenseToSingleAcc);

			PXUIFieldAttribute.SetEnabled<TaxRev.taxableMin>(TaxRevisions.Cache, null, tax.TaxCalcLevel == "1" || tax.TaxCalcLevel == "2");
			PXUIFieldAttribute.SetEnabled<TaxRev.taxableMax>(TaxRevisions.Cache, null, tax.TaxCalcLevel == "1" || tax.TaxCalcLevel == "2");

			PXUIFieldAttribute.SetEnabled<TaxRev.taxRate>(TaxRevisions.Cache, null, tax.ExemptTax == false);
			PXUIFieldAttribute.SetEnabled<TaxRev.nonDeductibleTaxRate>(TaxRevisions.Cache, null, isDeductible);
			PXUIFieldAttribute.SetVisible<TaxRev.nonDeductibleTaxRate>(TaxRevisions.Cache, null, isDeductible);

			PXUIFieldAttribute.SetEnabled<Tax.taxType>(cache, tax, !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.taxCalcType>(cache, tax, !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.taxCalcRule>(cache, tax, !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.taxCalcLevel2Exclude>(cache, tax, !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.taxApplyTermsDisc>(cache, tax, !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.outDate>(cache, tax, !isExternal);

			PopulateBucketList(tax);

			if (isVAT)
			{
				PXStringListAttribute.SetList<Tax.taxApplyTermsDisc>(cache, tax, new CSTaxTermsDiscount.ListAttribute());
			}
			else
			{
				PXStringListAttribute.SetList<Tax.taxApplyTermsDisc>(cache, tax,
					new [] { CSTaxTermsDiscount.ToTaxableAmount, CSTaxTermsDiscount.NoAdjust },
					new [] { Messages.DiscountToTaxableAmount, Messages.DiscountToTotalAmount });
			}

			if (isDeductible && tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment)
			{
				cache.RaiseExceptionHandling<Tax.deductibleVAT>(tax, tax.DeductibleVAT,
					new PXSetPropertyException(Messages.DeductiblePPDTaxProhibited, PXErrorLevel.Error));

				cache.RaiseExceptionHandling<Tax.taxApplyTermsDisc>(tax, tax.TaxApplyTermsDisc, 
					new PXSetPropertyException(Messages.DeductiblePPDTaxProhibited, PXErrorLevel.Error));
					
			}
		}

		protected void ThrowFieldIsEmpty<Field>(PXCache sender, PXRowPersistingEventArgs e)
		where Field : IBqlField
		{
			sender.RaiseExceptionHandling<Field>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<Field>(sender)));
		}

		protected void CheckFieldIsEmpty<Field>(PXCache sender, PXRowPersistingEventArgs e)
		where Field : IBqlField
		{
			if (sender.GetValue<Field>(e.Row) == null)
			{
				ThrowFieldIsEmpty<Field>(sender, e);
			}
		}

		protected virtual void Tax_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
				return;


			if (((Tax)e.Row).TaxType == CSTaxType.Use)
			{
				CheckFieldIsEmpty<Tax.expenseAccountID>(sender, e);
				CheckFieldIsEmpty<Tax.expenseSubID>(sender, e);
			}

			if (((Tax)e.Row).TaxType == CSTaxType.VAT)
			{
				CheckFieldIsEmpty<Tax.purchTaxAcctID>(sender, e);
				CheckFieldIsEmpty<Tax.purchTaxSubID>(sender, e);
			}

			if (((Tax)e.Row).PendingTax == true)
			{
				CheckFieldIsEmpty<Tax.pendingSalesTaxAcctID>(sender, e);
				CheckFieldIsEmpty<Tax.pendingSalesTaxSubID>(sender, e);
				CheckFieldIsEmpty<Tax.pendingPurchTaxAcctID>(sender, e);
				CheckFieldIsEmpty<Tax.pendingPurchTaxSubID>(sender, e);
			}
		}

		public override void Persist()
		{
			this.PrepareRevisions();
			base.Persist();
		}

		protected virtual void TaxRev_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			ForcePersisting();
		}

		private bool UpdateRevisions(PXCache sender, TaxRev item, PXResultset<TaxRev> summary, bool PerformUpdate)
		{
			foreach (TaxRev summ_item in summary)
			{
				if (!sender.ObjectsEqual(summ_item, item))
				{
					if (PerformUpdate)
					{
						summ_item.TaxBucketID = item.TaxBucketID;
						summ_item.TaxableMax = item.TaxableMax;
						summ_item.TaxableMin = item.TaxableMin;
						summ_item.TaxRate = item.TaxRate;
						summ_item.Outdated = item.Outdated;
						TaxRevisions.Cache.Update(summ_item);
					}
					return true;
				}
			}
			return false;
		}

		protected virtual void TaxRev_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			TaxRev taxRevision = e.Row as TaxRev;

			if (taxRevision == null)
				return;

			if (Tax.Current.TaxVendorID == null)
			{
				switch (taxRevision.TaxBucketID)
				{
					case -1:
						taxRevision.TaxType = CSTaxType.Use;
						break;
					case -2:
						taxRevision.TaxType = CSTaxType.Sales;
						break;
				}
			}
			else if (taxRevision.TaxBucketID != null)
			{
				TaxBucket bucket = (TaxBucket)
					PXSelect<TaxBucket, 
					Where<TaxBucket.vendorID, Equal<Current<Tax.taxVendorID>>, 
						And<TaxBucket.bucketID, Equal<Required<TaxBucket.bucketID>>>>>
					.Select(this,taxRevision.TaxBucketID);

				if (bucket != null)
					taxRevision.TaxType = bucket.BucketType;
			}

			Tax tax = Tax.Current;
			bool isDeductibleVAT = tax.TaxType == CSTaxType.VAT && tax.DeductibleVAT == true;
			bool isWithholding = tax.TaxType == CSTaxType.Withholding;
			var checkInfo = CheckTaxRevisionCompatibilityWithTax(taxRevision, isDeductibleVAT, isWithholding);
			bool isSuccess = checkInfo.Key;
			
			if (!isSuccess)
			{
				e.Cancel = true;
				throw new PXSetPropertyException<TaxRev.taxType>(checkInfo.Value);
			}

			object StartDate = taxRevision.StartDate;
			object TaxType = taxRevision.TaxType;
			var summary = 
				PXSelect<TaxRev,
				Where<TaxRev.taxID, Equal<Current<Tax.taxID>>,
					And<TaxRev.taxType, Equal<Required<TaxRev.taxType>>,
					And<TaxRev.startDate, Equal<Required<TaxRev.startDate>>>>>>
				.Select(this, TaxType, StartDate);

			e.Cancel = UpdateRevisions(sender, taxRevision, summary, true);		
		}

		protected virtual void TaxRev_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			TaxRev taxrev = e.Row as TaxRev;

			if (taxrev == null)
				return;

			PXUIFieldAttribute.SetEnabled<TaxRev.nonDeductibleTaxRate>(sender, taxrev, taxrev.TaxType != TaxType.Sales);
		}

		protected virtual void TaxRev_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			TaxRev oldTaxRev = e.Row as TaxRev;
			TaxRev newTaxRev = e.NewRow as TaxRev;

			if (newTaxRev == null)
				return;

			if (Tax.Current.TaxVendorID == null)
			{
				if (oldTaxRev == null)
					return;

				switch (oldTaxRev.TaxBucketID)
				{
					case -1:
						oldTaxRev.TaxType = "P";
						break;
					case -2:
						oldTaxRev.TaxType = "S";
						break;
				}
			} 
			else if (newTaxRev.TaxBucketID != null)
			{
				TaxBucket bucket = (TaxBucket)
					PXSelect<TaxBucket, 
						Where<TaxBucket.vendorID, Equal<Current<Tax.taxVendorID>>, 
							And<TaxBucket.bucketID, Equal<Required<TaxBucket.bucketID>>>>>
					.Select(this, newTaxRev.TaxBucketID);

				if (bucket != null)
					newTaxRev.TaxType = bucket.BucketType;
			}

			var summary =
				PXSelect<TaxRev,
				Where<TaxRev.taxID, Equal<Current<Tax.taxID>>,
					And<TaxRev.taxType, Equal<Required<TaxRev.taxType>>,
					And<TaxRev.startDate, Equal<Required<TaxRev.startDate>>>>>>
				.Select(this, newTaxRev.TaxType, newTaxRev.StartDate);

			e.Cancel = UpdateRevisions(sender, newTaxRev, summary, false);		
		}

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Enabled), false)]
		protected virtual void TaxRev_TaxType_CacheAttached(PXCache sender) {}

		protected virtual void TaxRev_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if(((TaxRev)e.OldRow).StartDate != ((TaxRev)e.Row).StartDate)
			{
				ForcePersisting();
			}
		}

		protected virtual void TaxRev_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			ForcePersisting();
		}

		private void ForcePersisting() 
		{
			this.Tax.Cache.MarkUpdated(this.Tax.Current);
		}

		private void PrepareRevisions() 
		{
			Tax taxrow = (Tax)Tax.Current;

			if (Tax.Current == null)
			{
				return;
			}

			if (taxrow.OutDate != null && !(bool) taxrow.Outdated)
			{
				{
					TaxRev rev = new TaxRev();
					rev.StartDate = taxrow.OutDate;
					rev.TaxType = "S";
					TaxRevisions.Insert(rev);
				}

				if (taxrow.TaxType == "V")
				{
					TaxRev rev = new TaxRev();
					rev.StartDate = taxrow.OutDate;
					rev.TaxType = "P";
					TaxRevisions.Insert(rev);
				}

				taxrow.Outdated = true;
				Tax.Update(taxrow);
			}

			DateTime? lastSalesDate = null;
			DateTime? lastPurchDate = null;
			
			//Assumes that taxRevisions are sorted in the descending order (latest first);
			foreach (TaxRev iRev in this.TaxRevisions.Select())
			{
				if (iRev.TaxType == "S" && lastSalesDate == null ||
					  iRev.TaxType == "P" && lastPurchDate == null)
				{
					if (taxrow.OutDate.HasValue)
					{
						iRev.EndDate = taxrow.OutDate.Value;
					}
					else 
					{
						if (iRev.EndDate != iRev.GetDefaultEndDate())
							TaxRevisions.Cache.SetDefaultExt<TaxRev.endDate>(iRev);
					}
				}

				if ((taxrow.OutDate != null) && (((DateTime)taxrow.OutDate).CompareTo(iRev.StartDate) <= 0))
				{
					iRev.Outdated = true;
				}
				else
				{
					iRev.Outdated = false;
				}

				if (iRev.TaxType == "S")
				{
					if (lastSalesDate != null && iRev.EndDate != lastSalesDate)
					{
						iRev.EndDate = lastSalesDate;
					}
					lastSalesDate = ((DateTime)iRev.StartDate).AddDays(-1);
				}
				else if (iRev.TaxType == "P")
				{
					if (lastPurchDate != null && iRev.EndDate != lastPurchDate)
					{
						iRev.EndDate = lastPurchDate;
					}
					lastPurchDate = ((DateTime)iRev.StartDate).AddDays(-1);
				}

				TaxRevisions.Update(iRev);
			}
		}

		public SalesTaxMaint()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetVisible<TaxCategoryDet.taxID>(Categories.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TaxCategoryDet.taxID>(Categories.Cache, null, false);
			PXUIFieldAttribute.SetVisible<TaxCategoryDet.taxCategoryID>(Categories.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<TaxCategoryDet.taxCategoryID>(Categories.Cache, null, true);
			//SWUIFieldAttribute.SetVisible<TaxCategory.taxCategoryID>(Category.Cache, null, false);

			PXUIFieldAttribute.SetVisible<TaxZoneDet.taxID>(Zones.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TaxZoneDet.taxID>(Zones.Cache, null, false);
			PXUIFieldAttribute.SetVisible<TaxZoneDet.taxZoneID>(Zones.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<TaxZoneDet.taxZoneID>(Zones.Cache, null, true);
			//SWUIFieldAttribute.SetVisible<TaxZone.taxZoneID>(Zone.Cache, null, false);
			
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => 
			{
				if (e.Row != null)
					e.NewValue = BAccountType.VendorType;
			});

			if(!PXAccess.FeatureInstalled<CS.FeaturesSet.vATReporting>())
				PXStringListAttribute.SetList<TX.Tax.taxType>(Tax.Cache, null, new CSTaxType.ListSimpleAttribute());
		}

		public PXSetup<APSetup> APSetup;

		protected virtual void TaxZoneDet_TaxID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = (Tax.Cache.Current == null) ? null : ((Tax)Tax.Cache.Current).TaxID;
		}

		protected virtual void TaxCategoryDet_TaxID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = (Tax.Cache.Current == null) ? null : ((Tax)Tax.Cache.Current).TaxID;
		}
	}
}
