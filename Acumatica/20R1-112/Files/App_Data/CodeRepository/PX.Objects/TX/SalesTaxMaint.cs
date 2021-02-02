using System;
using System.Linq;
using PX.Data;
using System.Collections.Generic;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.Common.Extensions;
using PX.Objects.Extensions.PerUnitTax;

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
		public PXSelect<Tax, Where<Tax.taxID, Equal<Current<Tax.taxID>>>> CurrentTax;

		public PXSelect<Tax,
					Where<Tax.taxID, Equal<Current<Tax.taxID>>>> TaxForPrintingParametersTab;

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

		public PXSetup<APSetup> APSetup;

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

			bool useVAT = PXAccess.FeatureInstalled<CS.FeaturesSet.vATReporting>();
			bool usePerUnitTax = PXAccess.FeatureInstalled<CS.FeaturesSet.perUnitTaxSupport>();
			var allowedTaxTypes = CSTaxType.GetTaxTypesWithLabels(useVAT, usePerUnitTax).ToArray();

			PXStringListAttribute.SetList<Tax.taxType>(Tax.Cache, null, allowedTaxTypes);
			PopulateBucketList(new Tax { TaxType = CSTaxType.Sales });
		}

		protected virtual void Tax_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Tax newTax = (Tax)e.Row;
			Tax oldTax = (Tax)e.OldRow;

			ClearOldWarningsAndErrors(sender, newTax);
			CheckAndFixTaxRates(newTax);
			SetWarningsOnRowUpdate(sender, newTax, oldTax);
			ProcessTaxRevOnTaxVendorChangeOnTaxUpdate(sender, newTax, oldTax);

			if (newTax.TaxType != oldTax.TaxType)
			{
				ProccessTaxTypeChangeOnTaxUpdate(newTax, oldTax);
			}

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

			switch (tax.TaxType)
			{
				case CSTaxType.VAT:
					FillAllowedValuesAndLabelsForVatTax(tax, allowedValues, allowedLabels);
					break;

				case CSTaxType.Use:
				case CSTaxType.Withholding:
					FillAllowedValuesAndLabelsForUseOrWitholdingTax(tax, allowedValues, allowedLabels);
					break;

				case CSTaxType.Sales:
					FillAllowedValuesAndLabelsForSalesTax(tax, allowedValues, allowedLabels);
					break;

				case CSTaxType.PerUnit:
					FillAllowedValuesAndLabelsForPerUnitTax(tax, allowedValues, allowedLabels);
					break;
			}

			if (allowedValues.Count > 0)
			{
				PXIntListAttribute.SetList<TaxRev.taxBucketID>(TaxRevisions.Cache, null, allowedValues.ToArray(), allowedLabels.ToArray());
			}
			else
			{
				const int defaultValue = 0;
				const string defaultLabel = "undefined";
				
				PXIntListAttribute.SetList<TaxRev.taxBucketID>(TaxRevisions.Cache, null, (defaultValue, defaultLabel));
			}
		}

		protected virtual void FillAllowedValuesAndLabelsForUseOrWitholdingTax(Tax tax, List<int> allowedValues, List<string> allowedLabels)
		{
			if (tax.TaxVendorID == null)
			{
				allowedValues.Add(CSTaxBucketType.DEFAULT_OUTPUT_GROUP);
				allowedLabels.Add(Messages.DefaultOutputGroup);
			}
			else
			{
				var taxBuckets =
					PXSelectReadonly<TaxBucket,
							Where<TaxBucket.vendorID, Equal<Required<TaxBucket.vendorID>>,
								And<TaxBucket.bucketType, Equal<CSTaxBucketType.sales>>>>
					.Select(this, tax.TaxVendorID);

				foreach (TaxBucket bucket in taxBuckets)
				{
					allowedValues.Add(bucket.BucketID.Value);
					allowedLabels.Add(bucket.Name);
				}
			}
		}

		protected virtual void FillAllowedValuesAndLabelsForPerUnitTax(Tax perUnitTax, List<int> allowedValues, List<string> allowedLabels)
		{
			//Groups for Per Unit taxes should be same as groups for Sales Taxes - both Input and Output
			FillAllowedValuesAndLabelsForSalesTax(perUnitTax, allowedValues, allowedLabels);
		}

		protected virtual void FillAllowedValuesAndLabelsForSalesTax(Tax salesTax, List<int> allowedValues, List<string> allowedLabels)
		{
			if (salesTax.TaxVendorID == null)
			{
				allowedValues.Add(CSTaxBucketType.DEFAULT_OUTPUT_GROUP);
				allowedLabels.Add(Messages.DefaultOutputGroup);
				allowedValues.Add(CSTaxBucketType.DEFAULT_INPUT_GROUP);
				allowedLabels.Add(Messages.DefaultInputGroup);
			}
			else
			{
				var taxBuckets = 
					PXSelectReadonly<TaxBucket,
					Where<TaxBucket.vendorID, Equal<Required<TaxBucket.vendorID>>,
						And<Where<TaxBucket.bucketType, Equal<CSTaxBucketType.sales>,
								Or<TaxBucket.bucketType, Equal<CSTaxBucketType.purchase>>>>>>
					.Select(this, salesTax.TaxVendorID);

				foreach (TaxBucket bucket in taxBuckets)
				{
					allowedValues.Add(bucket.BucketID.Value);
					allowedLabels.Add(bucket.Name);
				}
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
					allowedValues.Add(CSTaxBucketType.DEFAULT_OUTPUT_GROUP);
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

			if (newTax.TaxType == CSTaxType.Use && newTax.ReportExpenseToSingleAccount == false && newTax.TaxCalcType != CSTaxCalcType.Item)
			{
				cache.RaiseExceptionHandling<Tax.taxCalcRule>(newTax, newTax.TaxCalcRule, new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
				cache.RaiseExceptionHandling<Tax.reportExpenseToSingleAccount>(newTax, newTax.ReportExpenseToSingleAccount,
					new PXSetPropertyException(Messages.TheseTwoOptionsCantBeCombined));
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

			ResetTaxFieldsOnTaxUpdate(cache, newTax, oldTax);
		}

		/// <summary>
		/// Resets some of tax fields on tax row updated event.
		/// </summary>
		protected virtual void ResetTaxFieldsOnTaxUpdate(PXCache cache, Tax newTax, Tax oldTax)
		{
			switch (newTax.TaxType)
			{
				case CSTaxType.VAT:
				case CSTaxType.PerUnit when newTax.PerUnitTaxPostMode == PerUnitTaxPostOptions.TaxAccount:
					break;

				case CSTaxType.PerUnit when newTax.PerUnitTaxPostMode == PerUnitTaxPostOptions.LineAccount:
					cache.SetValue<Tax.salesTaxAcctID>(newTax, null);
					cache.SetValue<Tax.salesTaxSubID>(newTax, null);
					cache.SetValue<Tax.purchTaxAcctID>(newTax, null);
					cache.SetValue<Tax.purchTaxSubID>(newTax, null);
					break;

				default:
					cache.SetValue<Tax.purchTaxAcctID>(newTax, null);
					cache.SetValue<Tax.purchTaxSubID>(newTax, null);
					break;
			}

			if (newTax.TaxType != CSTaxType.VAT)
			{
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

			bool isDeductibleSet = newTax.DeductibleVAT == true && newTax.DeductibleVAT != oldTax.DeductibleVAT;
			bool isDeductibleRemoved = newTax.DeductibleVAT == false && newTax.DeductibleVAT != oldTax.DeductibleVAT;

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
		/// Process the tax revisions on tax vendor change on tax update. Called on <see cref="Tax_RowUpdated(PXCache, PXRowUpdatedEventArgs)"/>.
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
					rev.TaxBucketID = rev.TaxType == TaxType.Sales ? CSTaxBucketType.DEFAULT_OUTPUT_GROUP : CSTaxBucketType.DEFAULT_INPUT_GROUP;
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
		/// Proccess tax type change during tax row updated event. 
		/// Depending on the tax type defaults values in some of <see cref="TaxRev"/> fields.
		/// </summary>
		public virtual void ProccessTaxTypeChangeOnTaxUpdate(Tax newTax, Tax oldTax)
		{
			bool isPerUnitSet = newTax?.TaxType == CSTaxType.PerUnit;
			bool isPerUnitRemoved = oldTax?.TaxType == CSTaxType.PerUnit;

			if (isPerUnitSet)
			{
				string perUnitTaxCalcRule = CSTaxCalcType.Item + CSTaxCalcLevel.CalcOnItemQtyExclusively;
				Tax.Cache.SetValueExt<Tax.taxCalcRule>(newTax, perUnitTaxCalcRule);
				Tax.Cache.SetValueExt<Tax.taxApplyTermsDisc>(newTax, CSTaxTermsDiscount.NoAdjust);
				Tax.Cache.SetValueExt<Tax.taxCalcLevel2Exclude>(newTax, false);
			}
			else if (isPerUnitRemoved)
			{
				string defaultCalcRule = CSTaxCalcType.Item + CSTaxCalcLevel.CalcOnItemAmt;
				Tax.Cache.SetValueExt<Tax.taxCalcRule>(newTax, defaultCalcRule);
				Tax.Cache.SetDefaultExt<Tax.taxUOM>(newTax);
				Tax.Cache.SetDefaultExt<Tax.perUnitTaxPostMode>(newTax);
			}

			foreach (TaxRev taxRev in TaxRevisions.Select())
			{
				if (isPerUnitSet)
				{
					TaxRevisions.Cache.SetDefaultExt<TaxRev.taxableMax>(taxRev);
					TaxRevisions.Cache.SetDefaultExt<TaxRev.taxableMin>(taxRev);
				}
				else
				{
					TaxRevisions.Cache.SetDefaultExt<TaxRev.taxableMaxQty>(taxRev);
				}
			}
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
				var (isCompatible, errorMsg) = CheckTaxRevisionCompatibilityWithTax(taxRev, isDeductibleVATSet, isWithholdingSet);

				if (isCompatible)
					continue;

				var taxRevNotCompatibleWithTaxException = new PXSetPropertyException(errorMsg);
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
		private (bool IsCompatible, string ErrorMsg) CheckTaxRevisionCompatibilityWithTax(TaxRev taxRevision, bool taxIsDeductibleVAT,
																						  bool taxIsWithholding)
		{
			if (taxIsDeductibleVAT && taxRevision.TaxType == TaxType.Sales)
			{
				return (false, Messages.DeductibleVATWithOutputReportingGroupError);
			}
			else if (taxIsWithholding && taxRevision.TaxType == TaxType.Purchase)
			{
				return (false, Messages.WithholdingTaxWithInputReportingGroupError);		
			}

			return (true, null);
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
			if (!(e.Row is Tax tax))
				return;

			sender.SetDefaultExt<Tax.salesTaxAcctID>(tax);
			sender.SetDefaultExt<Tax.salesTaxSubID>(tax);

			if (tax.TaxType == CSTaxType.VAT)
			{
				sender.SetDefaultExt<Tax.purchTaxAcctID>(tax);
				sender.SetDefaultExt<Tax.purchTaxSubID>(tax);
			}

			if (tax.PendingTax == true)
			{
				sender.SetDefaultExt<Tax.pendingSalesTaxAcctID>(tax);
				sender.SetDefaultExt<Tax.pendingSalesTaxSubID>(tax);
				sender.SetDefaultExt<Tax.pendingPurchTaxAcctID>(tax);
				sender.SetDefaultExt<Tax.pendingPurchTaxSubID>(tax);
			}

			if (tax.TaxType == CSTaxType.Use || tax.TaxType == CSTaxType.Sales || ExpenseAccountRequired(tax))
			{
				sender.SetDefaultExt<Tax.expenseAccountID>(tax);
				sender.SetDefaultExt<Tax.expenseSubID>(tax);
			}

			foreach (TaxRev taxRev in TaxRevisions.View.SelectMultiBound(new[] { tax }))
			{
				TaxRevisions.Cache.MarkUpdated(taxRev);
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

			if (modifiedTaxRecord.TaxType == CSTaxType.Use)
			{
				sender.SetValueExt<Tax.taxCalcLevel2Exclude>(modifiedTaxRecord, true);
			}

			if (modifiedTaxRecord.TaxType == CSTaxType.Withholding)
			{
				sender.SetValueExt<Tax.taxCalcRule>(modifiedTaxRecord, CSTaxCalcType.Item + CSTaxCalcLevel.Inclusive);
			}
		}

		protected virtual void Tax_TaxUOM_FieldVerifying(Events.FieldVerifying<Tax, Tax.taxUOM> e)
		{
			if (e.Row?.TaxType != CSTaxType.PerUnit || e.NewValue != null)
				return;

			e.Cache.RaiseExceptionHandling<Tax.taxUOM>(e.Row, null,
				new PXSetPropertyException(ErrorMessages.FieldIsEmpty, 
										   PXUIFieldAttribute.GetDisplayName<Tax.taxUOM>(e.Cache)));
		}

		#region Mapping of GLAccount fields for per-unit taxes
		protected virtual void Tax_PurchTaxAcctIDOverride_FieldUpdated(Events.FieldUpdated<Tax, Tax.purchTaxAcctIDOverride> e) =>
			e.Cache.SetValueExt<Tax.purchTaxAcctID>(e.Row, e.NewValue);

		protected virtual void Tax_PurchTaxSubIDOverride_FieldUpdated(Events.FieldUpdated<Tax, Tax.purchTaxSubIDOverride> e) =>
			e.Cache.SetValueExt<Tax.purchTaxSubID>(e.Row, e.NewValue);

		protected virtual void Tax_SalesTaxAcctIDOverride_FieldUpdated(Events.FieldUpdated<Tax, Tax.salesTaxAcctIDOverride> e) =>
			e.Cache.SetValueExt<Tax.salesTaxAcctID>(e.Row, e.NewValue);

		protected virtual void Tax_SalesTaxSubIDOverride_FieldUpdated(Events.FieldUpdated<Tax, Tax.salesTaxSubIDOverride> e) =>
			e.Cache.SetValueExt<Tax.salesTaxSubID>(e.Row, e.NewValue);

		public virtual void Tax_SalesTaxAcctID_ExceptionHandling(Events.ExceptionHandling<Tax, Tax.salesTaxAcctID> e) =>
			MapErrorFromOriginalFieldToSubstitute<Tax.salesTaxAcctIDOverride>(e.Cache, e.Row, e.NewValue, e.Exception);

		public virtual void Tax_SalesTaxSubIDOverride_ExceptionHandling(Events.ExceptionHandling<Tax, Tax.salesTaxSubID> e) =>
			MapErrorFromOriginalFieldToSubstitute<Tax.salesTaxSubIDOverride>(e.Cache, e.Row, e.NewValue, e.Exception);

		public virtual void Tax_PurchTaxAcctIDOverride_ExceptionHandling(Events.ExceptionHandling<Tax, Tax.purchTaxAcctID> e) =>
			MapErrorFromOriginalFieldToSubstitute<Tax.purchTaxAcctIDOverride>(e.Cache, e.Row, e.NewValue, e.Exception);

		public virtual void Tax_PurchTaxSubIDOverride_ExceptionHandling(Events.ExceptionHandling<Tax, Tax.purchTaxSubID> e) =>
			MapErrorFromOriginalFieldToSubstitute<Tax.purchTaxSubIDOverride>(e.Cache, e.Row, e.NewValue, e.Exception);

		private void MapErrorFromOriginalFieldToSubstitute<TSubstituteAccountField>(PXCache cache, Tax tax, object newValue, Exception exception)
		where TSubstituteAccountField : PX.Data.BQL.BqlInt.Field<TSubstituteAccountField>
		{
			if (tax?.TaxType != CSTaxType.PerUnit)
				return;

			cache.RaiseExceptionHandling<TSubstituteAccountField>(tax, newValue, exception);
		}
		#endregion

		protected virtual void Tax_PurchTaxAcctID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) =>
			SetAccountIDOrSubIDWithValidation<Tax.purchTaxAcctID>(sender, e.Row as Tax, e.NewValue as int?);

		protected virtual void Tax_PurchTaxSubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) =>
			SetAccountIDOrSubIDWithValidation<Tax.purchTaxSubID>(sender, e.Row as Tax, e.NewValue as int?);

		protected virtual void Tax_SalesTaxAcctID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) =>
			SetAccountIDOrSubIDWithValidation<Tax.salesTaxAcctID>(sender, e.Row as Tax, e.NewValue as int?);
		
		protected virtual void Tax_SalesTaxSubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) =>
			SetAccountIDOrSubIDWithValidation<Tax.salesTaxSubID>(sender, e.Row as Tax, e.NewValue as int?);

		private void SetAccountIDOrSubIDWithValidation<Field>(PXCache sender, Tax tax, int? newAccountOrSubValue)
		where Field : IBqlField
		{
			if (PXAccess.FeatureInstalled<CS.FeaturesSet.taxEntryFromGL>())
			{
				Tax taxCopy = (Tax)sender.CreateCopy(tax);
				sender.SetValue<Field>(taxCopy, newAccountOrSubValue);

				if (taxCopy.PurchTaxAcctID == taxCopy.SalesTaxAcctID && taxCopy.PurchTaxSubID == taxCopy.SalesTaxSubID)
				{
					var exception = new PXSetPropertyException(Messages.ClaimableAndPayableAccountsAreTheSame, PXErrorLevel.Warning, taxCopy.TaxID);

					sender.RaiseExceptionHandling<Tax.purchTaxAcctID>(tax, null, exception);
					sender.RaiseExceptionHandling<Tax.purchTaxSubID>(tax, null, exception);
					sender.RaiseExceptionHandling<Tax.salesTaxAcctID>(tax, null, exception);
					sender.RaiseExceptionHandling<Tax.salesTaxSubID>(tax, null, exception);
				}
			}
		}

		protected virtual void Tax_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is Tax tax))
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

			bool isSales = tax.TaxType == CSTaxType.Sales;
			bool isVAT = tax.TaxType == CSTaxType.VAT;
			bool isPerUnitTax = tax.TaxType == CSTaxType.PerUnit;
			bool isUse = tax.TaxType == CSTaxType.Use;
			bool isDeductible = tax.DeductibleVAT == true;

			PXUIFieldAttribute.SetEnabled<Tax.reverseTax>(cache, tax, isVAT && !isExternal & !isUsedForManualVAT);
			PXUIFieldAttribute.SetEnabled<Tax.pendingTax>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.directTax>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.statisticalTax>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.exemptTax>(cache, tax, isVAT && !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.deductibleVAT>(cache, tax, isVAT && !isExternal & !isUsedForManualVAT);
			PXUIFieldAttribute.SetEnabled<Tax.includeInTaxable>(cache, tax, isVAT && !isExternal);

			SetGLAccounts(cache, tax);

			PXUIFieldAttribute.SetVisible<Tax.reportExpenseToSingleAccount>(cache, tax, isDeductible || isUse || isSales);
			PXUIFieldAttribute.SetEnabled<Tax.reportExpenseToSingleAccount>(cache, tax, isDeductible || isUse || isSales);

			SetTaxRevisionsUIPropertiesForTax(tax);

			PXUIFieldAttribute.SetEnabled<Tax.taxType>(cache, tax, !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.taxCalcType>(cache, tax, !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.taxCalcRule>(cache, tax, !isExternal);
			PXUIFieldAttribute.SetEnabled<Tax.taxCalcLevel2Exclude>(cache, tax, !isExternal && !isPerUnitTax);
			PXUIFieldAttribute.SetEnabled<Tax.taxApplyTermsDisc>(cache, tax, !isExternal && !isPerUnitTax);
			PXUIFieldAttribute.SetEnabled<Tax.outDate>(cache, tax, !isExternal);
			
			cache.Adjust<PXUIFieldAttribute>(tax)
				 .For<Tax.taxUOM>(a => a.Enabled = a.Required = a.Visible = isPerUnitTax)
				 .For<Tax.perUnitTaxPostMode>(a => a.Enabled = a.Visible = isPerUnitTax);
			
			PopulateBucketList(tax);
			SetAllowedTaxCalculationRulesList(tax);
			SetAllowedTaxTermDiscountsList(tax);
			SetPrintingSettingsTabUI(tax);

			if (isDeductible && tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment)
			{
				cache.RaiseExceptionHandling<Tax.deductibleVAT>(tax, tax.DeductibleVAT,
					new PXSetPropertyException(Messages.DeductiblePPDTaxProhibited, PXErrorLevel.Error));

				cache.RaiseExceptionHandling<Tax.taxApplyTermsDisc>(tax, tax.TaxApplyTermsDisc, 
					new PXSetPropertyException(Messages.DeductiblePPDTaxProhibited, PXErrorLevel.Error));			
			}
		}

		protected virtual void SetGLAccounts(PXCache cache, Tax tax)
		{
			bool requireExpenseAccount = ExpenseAccountRequired(tax);

			SetSalesTaxGAccountAndSubPersistingCheck(cache, tax);
			SetExpenseGLAccountAndSubPersistingCheck(cache, tax, requireExpenseAccount);
			
			SetSalesTaxGLAccountUI(cache, tax);
			SetPurchaiseTaxGLAccountUI(cache, tax);
			SetRetainageGLAccountUI(cache, tax);
			SetExpenseGLAccountUI(cache, tax, requireExpenseAccount);
			SetPendingGLAccountsUI(cache, tax);
		}

		private void SetSalesTaxGAccountAndSubPersistingCheck(PXCache cache, Tax tax)
		{
			bool isPerUnitTax = tax.TaxType == CSTaxType.PerUnit;
			bool postPerUnitTaxOnTaxAccount = tax.PerUnitTaxPostMode == PerUnitTaxPostOptions.TaxAccount;
			PXPersistingCheck actualPersistingCheck = isPerUnitTax && !postPerUnitTaxOnTaxAccount
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.Null;

			cache.Adjust<PXDefaultAttribute>(tax)
				 .For<Tax.salesTaxAcctID>(a => a.PersistingCheck = actualPersistingCheck)
				 .SameFor<Tax.salesTaxSubID>();
		}

		private void SetExpenseGLAccountAndSubPersistingCheck(PXCache cache, Tax tax, bool requireExpenseAccount)
		{
			PXPersistingCheck actualPersistingCheck = requireExpenseAccount
				? PXPersistingCheck.NullOrBlank
				: PXPersistingCheck.Nothing;

			cache.Adjust<PXDefaultAttribute>(tax)
				 .For<Tax.expenseAccountID>(a => a.PersistingCheck = actualPersistingCheck)
				 .SameFor<Tax.expenseSubID>();
		}

		private void SetSalesTaxGLAccountUI(PXCache cache, Tax tax)
		{
			bool isSalesTaxAccountUsed = tax.TaxType != CSTaxType.PerUnit;
			bool isOverrideAccountUsed = tax.TaxType == CSTaxType.PerUnit && tax.PerUnitTaxPostMode == PerUnitTaxPostOptions.TaxAccount;

			cache.Adjust<PXUIFieldAttribute>(tax)
				 .For<Tax.salesTaxAcctID>(a => a.Visible = a.Enabled = a.Required = isSalesTaxAccountUsed)
				 .SameFor<Tax.salesTaxSubID>()
				 .For<Tax.salesTaxAcctIDOverride>(a => a.Visible = a.Enabled = a.Required = isOverrideAccountUsed)
				 .SameFor<Tax.salesTaxSubIDOverride>();
		}

		private void SetPurchaiseTaxGLAccountUI(PXCache cache, Tax tax)
		{
			bool isTaxClaimableAccountUsed = tax.TaxType == CSTaxType.VAT;
			bool isOverrideAccountUsed = tax.TaxType == CSTaxType.PerUnit && tax.PerUnitTaxPostMode == PerUnitTaxPostOptions.TaxAccount;

			cache.Adjust<PXUIFieldAttribute>(tax)
				 .For<Tax.purchTaxAcctID>(a => a.Visible = a.Enabled = a.Required = isTaxClaimableAccountUsed)
				 .SameFor<Tax.purchTaxSubID>()
				 .For<Tax.purchTaxAcctIDOverride>(a => a.Visible = a.Enabled = a.Required = isOverrideAccountUsed)
				 .SameFor<Tax.purchTaxSubIDOverride>();
		}

		private void SetRetainageGLAccountUI(PXCache cache, Tax tax)
		{
			bool isPerUnitTax = tax.TaxType == CSTaxType.PerUnit;
			cache.Adjust<PXUIFieldAttribute>(tax)
				 .For<Tax.retainageTaxPayableAcctID>(a => a.Visible = a.Enabled = !isPerUnitTax)
				 .SameFor<Tax.retainageTaxPayableSubID>()
				 .SameFor<Tax.retainageTaxClaimableAcctID>()
				 .SameFor<Tax.retainageTaxClaimableSubID>();
		}

		private void SetExpenseGLAccountUI(PXCache cache, Tax tax, bool requireExpenseAccount)
		{
			bool isSales = tax.TaxType == CSTaxType.Sales;
			bool isUse = tax.TaxType == CSTaxType.Use;
			bool isPerUnitTax = tax.TaxType == CSTaxType.PerUnit;

			cache.Adjust<PXUIFieldAttribute>(tax)
				 .For<Tax.expenseAccountID>(a =>
					{
						a.Enabled = isUse || isSales || requireExpenseAccount;
						a.Required = requireExpenseAccount;
						a.Visible = !isPerUnitTax;
					})
				 .SameFor<Tax.expenseSubID>();
		}

		protected virtual void SetPendingGLAccountsUI(PXCache cache, Tax tax)
		{
			bool isPending = tax.PendingTax == true;

			cache.Adjust<PXUIFieldAttribute>(tax)
				 .For<Tax.pendingSalesTaxAcctID>(a => a.Visible = a.Enabled = isPending)
				 .SameFor<Tax.pendingSalesTaxSubID>()
				 .SameFor<Tax.pendingPurchTaxAcctID>()
				 .SameFor<Tax.pendingPurchTaxSubID>();
		}

		/// <summary>
		/// Sets "Printing Settings" tab user interface  for the given <paramref name="tax"/>. An extensibility point.
		/// Additional logic for the fields displayed on the tab could be added here. 
		/// </summary>
		/// <param name="tax">The tax.</param>
		protected virtual void SetPrintingSettingsTabUI(Tax tax)
		{
			TaxForPrintingParametersTab.AllowSelect = IsPrintingSettingsTabVisible(tax);
		}

		/// <summary>
		/// Check if Printing Settings tab should be visible for the given <paramref name="tax"/>. An extensibility point.
		/// </summary>
		/// <param name="tax">The tax.</param>
		/// <returns/>
		protected virtual bool IsPrintingSettingsTabVisible(Tax tax) => PXAccess.FeatureInstalled<CS.FeaturesSet.perUnitTaxSupport>();

		public virtual void SetAllowedTaxCalculationRulesList(Tax tax)
		{
			(string Value, string Label)[] allowedCalcRules;

			if (tax?.TaxType == CSTaxType.PerUnit)
			{
				allowedCalcRules = new[]
				{
					(Value: CSTaxCalcType.Item + CSTaxCalcLevel.CalcOnItemQtyExclusively, Label: Messages.CalcRuleItemAmt),
					(Value: CSTaxCalcType.Item + CSTaxCalcLevel.Inclusive, Label: Messages.CalcRuleExtract)
				};
			}
			else
			{
				allowedCalcRules = new[]
				{
					(Value: CSTaxCalcType.Item + CSTaxCalcLevel.Inclusive, Label: Messages.CalcRuleExtract),
					(Value: CSTaxCalcType.Item + CSTaxCalcLevel.CalcOnItemAmt, Label: Messages.CalcRuleItemAmt),
					(Value: CSTaxCalcType.Item + CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt, Label: Messages.CalcRuleItemAmtPlusTaxAmt),

					(Value: CSTaxCalcType.Doc + CSTaxCalcLevel.Inclusive, Label: Messages.CalcRuleDocInclusive),
					(Value: CSTaxCalcType.Doc + CSTaxCalcLevel.CalcOnItemAmt, Label: Messages.CalcRuleDocAmt),
					(Value: CSTaxCalcType.Doc + CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt, Label: Messages.CalcRuleDocAmtPlusTaxAmt),
				};
			}

			PXStringListAttribute.SetList<Tax.taxCalcRule>(Tax.Cache, null, allowedCalcRules);
		}

		public virtual void SetAllowedTaxTermDiscountsList(Tax tax)
		{
			bool isVAT = tax.TaxType == CSTaxType.VAT;
			bool isPerUnitTax = tax.TaxType == CSTaxType.PerUnit;

			if (isVAT)
			{
				PXStringListAttribute.SetList<Tax.taxApplyTermsDisc>(Tax.Cache, tax, new CSTaxTermsDiscount.ListAttribute());
			}
			else if (isPerUnitTax)
			{
				PXStringListAttribute.SetList<Tax.taxApplyTermsDisc>(Tax.Cache, tax,
					allowedValues: new[] { CSTaxTermsDiscount.NoAdjust },
					allowedLabels: new[] { Messages.DiscountToTotalAmount });
			}
			else
			{
				PXStringListAttribute.SetList<Tax.taxApplyTermsDisc>(Tax.Cache, tax,
					allowedValues: new[] { CSTaxTermsDiscount.ToTaxableAmount, CSTaxTermsDiscount.NoAdjust },
					allowedLabels: new[] { Messages.DiscountToTaxableAmount, Messages.DiscountToTotalAmount });
			}
		}

		protected virtual void SetTaxRevisionsUIPropertiesForTax(Tax tax)
		{
			bool isDeductible = tax.DeductibleVAT == true;
			bool isPerUnitTax = tax.TaxType == CSTaxType.PerUnit;
						
			PXUIFieldAttribute.SetEnabled<TaxRev.taxRate>(TaxRevisions.Cache, null, tax.ExemptTax == false);
			PXUIFieldAttribute.SetEnabled<TaxRev.nonDeductibleTaxRate>(TaxRevisions.Cache, null, isDeductible);
			PXUIFieldAttribute.SetVisible<TaxRev.nonDeductibleTaxRate>(TaxRevisions.Cache, null, isDeductible);

			TaxRevisions.Cache.Adjust<PXUIFieldAttribute>()
							  .For<TaxRev.taxableMaxQty>(a => a.Enabled = a.Visible = isPerUnitTax)

							  .For<TaxRev.taxableMin>(a =>
							  {
								  a.Enabled = !isPerUnitTax && (tax.TaxCalcLevel == "1" || tax.TaxCalcLevel == "2");
								  a.Visible = !isPerUnitTax;
							  })
							  .SameFor<TaxRev.taxableMax>();

			string displayName = isPerUnitTax
				? Messages.TaxRateLabelForPerUnitTax
				: Messages.TaxRateDefaultLabel;

			PXUIFieldAttribute.SetDisplayName<TaxRev.taxRate>(TaxRevisions.Cache, displayName);
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

			Tax tax = (Tax)e.Row;

			if (ExpenseAccountRequired(tax))
			{
				CheckFieldIsEmpty<Tax.expenseAccountID>(sender, e);
				CheckFieldIsEmpty<Tax.expenseSubID>(sender, e);
			}

			if (tax.TaxType == CSTaxType.VAT || 
			   (tax.TaxType == CSTaxType.PerUnit && tax.PerUnitTaxPostMode == PerUnitTaxPostOptions.TaxAccount))
			{
				CheckFieldIsEmpty<Tax.purchTaxAcctID>(sender, e);
				CheckFieldIsEmpty<Tax.purchTaxSubID>(sender, e);
			}
			
			if (tax.TaxType == CSTaxType.PerUnit)
			{
				CheckFieldIsEmpty<Tax.taxUOM>(sender, e);
			}

			if (tax.PendingTax == true)
			{
				CheckFieldIsEmpty<Tax.pendingSalesTaxAcctID>(sender, e);
				CheckFieldIsEmpty<Tax.pendingSalesTaxSubID>(sender, e);
				CheckFieldIsEmpty<Tax.pendingPurchTaxAcctID>(sender, e);
				CheckFieldIsEmpty<Tax.pendingPurchTaxSubID>(sender, e);
			}
		}

		protected bool ExpenseAccountRequired(Tax tax)
		{
			if (tax.ReportExpenseToSingleAccount != true) 
				return false;
			else if (tax.TaxType == CSTaxType.Use || tax.DeductibleVAT == true) 
				return true;
			else
			{
				bool inputGroupForSalesAccExists = tax.TaxType == CSTaxType.Sales && TaxRevisions.Select().ToList().Any(_ => ((TaxRev)_).TaxType == CSTaxType.Use);
				return inputGroupForSalesAccExists;
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
					case CSTaxBucketType.DEFAULT_INPUT_GROUP:
						taxRevision.TaxType = CSTaxType.Use;
						break;
					case CSTaxBucketType.DEFAULT_OUTPUT_GROUP:
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
			var (isCompatible, errorMsg) = CheckTaxRevisionCompatibilityWithTax(taxRevision, isDeductibleVAT, isWithholding);
			
			if (!isCompatible)
			{
				e.Cancel = true;
				throw new PXSetPropertyException<TaxRev.taxType>(errorMsg);
			}

			var summary = 
				PXSelect<TaxRev,
				Where<TaxRev.taxID, Equal<Current<Tax.taxID>>,
					And<TaxRev.taxType, Equal<Required<TaxRev.taxType>>,
					And<TaxRev.startDate, Equal<Required<TaxRev.startDate>>>>>>
				.Select(this, taxRevision.TaxType, taxRevision.StartDate);

			e.Cancel = UpdateRevisions(sender, taxRevision, summary, true);		
		}

		protected virtual void TaxRev_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is TaxRev taxRev))
				return;

			PXUIFieldAttribute.SetEnabled<TaxRev.nonDeductibleTaxRate>(sender, taxRev, taxRev.TaxType != TaxType.Sales);
			PXUIFieldAttribute.SetEnabled<TaxRev.nonDeductibleTaxRate>(sender, taxRev, taxRev.TaxType != TaxType.Sales);
		}

		protected virtual void TaxRev_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (!(e.NewRow is TaxRev newTaxRev))
				return;

			if (Tax.Current.TaxVendorID == null)
			{
				switch (newTaxRev.TaxBucketID)
				{
					case CSTaxBucketType.DEFAULT_INPUT_GROUP:
						newTaxRev.TaxType = "P";
						break;
					case CSTaxBucketType.DEFAULT_OUTPUT_GROUP:
						newTaxRev.TaxType = "S";
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

			if (taxrow.OutDate != null && !taxrow.Outdated.Value)
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
			foreach (TaxRev taxRev in this.TaxRevisions.Select())
			{
				var iRev = (TaxRev)this.TaxRevisions.Cache.Locate(taxRev) ?? taxRev;

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

		protected virtual void TaxZoneDet_TaxID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = (Tax.Cache.Current as Tax)?.TaxID;
		}

		protected virtual void TaxCategoryDet_TaxID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = (Tax.Cache.Current as Tax)?.TaxID;
		}
	}
}