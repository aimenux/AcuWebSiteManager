using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Common;
using PX.Objects.CM;
using PX.Objects.IN;

namespace PX.Objects.TX
{
	public abstract partial class TaxBaseAttribute : PXAggregateAttribute,
													 IPXRowInsertedSubscriber,
													 IPXRowUpdatedSubscriber,
													 IPXRowDeletedSubscriber,
													 IPXRowPersistedSubscriber,
													 IComparable
	{
		protected virtual string TaxUomFieldNameForTaxDetail => nameof(TaxDetail.TaxUOM);

		protected virtual string TaxableQtyFieldNameForTaxDetail => nameof(TaxDetail.TaxableQty);

		private Type _uom;

		/// <summary>
		/// The document line's UOM field for taxes per unit and specific taxes.
		/// </summary>
		public Type UOM
		{
			get => _uom;
			set
			{
				CheckDocLineFieldTypes(value, docLineFieldName: nameof(UOM));
				_uom = value;
			}
		}

		private Type _inventory;

		/// <summary>
		/// The document line's inventory for conversions to tax UOM.
		/// </summary>
		public Type Inventory
		{
			get => _inventory;
			set
			{
				CheckDocLineFieldTypes(value, docLineFieldName: nameof(Inventory));
				_inventory = value;
			}
		}

		private Type _lineQty;

		/// <summary>
		/// The document line quantity field in line UOM that will be used for taxes per unit and specific taxes calculation.
		/// </summary>
		public Type LineQty
		{
			get => _lineQty;
			set
			{
				CheckDocLineFieldTypes(value, docLineFieldName: nameof(LineQty));
				_lineQty = value;
			}
		}

		private void CheckDocLineFieldTypes(Type fieldTypeNewValue, string docLineFieldName)
		{
			fieldTypeNewValue.ThrowOnNull(nameof(fieldTypeNewValue));

			if (!typeof(IBqlField).IsAssignableFrom(fieldTypeNewValue))
			{
				throw new ArgumentException($"The {nameof(docLineFieldName)} should be a type implementing {nameof(IBqlField)}",
											nameof(fieldTypeNewValue));
			}
		}

		protected virtual bool IsPerUnitTax(Tax tax) => tax?.TaxType == CSTaxType.PerUnit;

		protected virtual void SetTaxUomForTaxDetail(PXCache taxDetailCache, TaxDetail taxDetail, string taxUOM) =>
			taxDetailCache.SetValue(taxDetail, TaxUomFieldNameForTaxDetail, taxUOM);

		protected virtual decimal? GetTaxableQuantityForTaxDetail(PXCache taxDetailCache, TaxDetail taxDetail) =>
			taxDetailCache.GetValue(taxDetail, TaxableQtyFieldNameForTaxDetail) as decimal?;

		protected virtual void SetTaxableQuantityForTaxDetail(PXCache taxDetailCache, TaxDetail taxDetail, decimal? taxableQty) =>
			taxDetailCache.SetValue(taxDetail, TaxableQtyFieldNameForTaxDetail, taxableQty);

		protected virtual decimal? GetLineQty(PXCache rowCache, object row) =>
			LineQty != null
				? rowCache.GetValue(row, LineQty.Name) as decimal?
				: null;

		protected virtual string GetUOM(PXCache rowCache, object row) =>
			UOM != null
				? rowCache.GetValue(row, UOM.Name) as string
				: null;

		protected virtual int? GetInventory(PXCache rowCache, object row) =>
			Inventory != null
				? rowCache.GetValue(row, Inventory.Name) as int?
				: null;

		#region Filling Taxes For Lines
		/// <summary>
		/// Fill tax details for line for per unit taxes.
		/// </summary>
		/// <exception cref="PXArgumentException">Thrown when a PX Argument error condition occurs.</exception>
		/// <param name="rowCache">The row cache.</param>
		/// <param name="row">The row.</param>
		/// <param name="tax">The tax.</param>
		/// <param name="taxRevision">The tax revision.</param>
		/// <param name="taxDetail">The tax detail.</param>
		protected virtual void TaxSetLineDefaultForPerUnitTaxes(PXCache rowCache, object row, Tax tax, TaxRev taxRevision, TaxDetail taxDetail)
		{
			PXCache taxDetailCache = rowCache.Graph.Caches[_TaxType];
			CurrencyInfo rowCuryInfo = GetDacCurrencyInfo(rowCache, row);
			bool isCalculatedInBaseCurrency = rowCuryInfo == null || rowCuryInfo.CuryID == rowCuryInfo.BaseCuryID;
			decimal taxableQty;
			decimal curyTaxAmount;

			switch (tax.TaxCalcLevel)
			{
				case CSTaxCalcLevel.CalcOnItemQtyExclusively:
				case CSTaxCalcLevel.CalcOnItemQtyInclusively:

					taxableQty = GetTaxableQuantityForPerUnitTaxes(rowCache, row, tax, taxRevision);

					if (isCalculatedInBaseCurrency)
						(_, curyTaxAmount) = GetTaxAmountForPerUnitTaxWithCorrectSign(rowCache, row, tax, taxRevision, taxDetailCache, taxDetail, taxableQty);
					else
						curyTaxAmount = 0m;

					break;

				default:
					PXTrace.WriteError(Messages.NotSupportedPerUnitTaxCalculationLevelErrorMsg);
					throw new PXArgumentException(Messages.NotSupportedPerUnitTaxCalculationLevelErrorMsg);
			}

			FillTaxDetailValuesForPerUnitTax(taxDetailCache, tax, taxRevision, taxDetail, rowCache, row, taxableQty, curyTaxAmount);
		}

		private decimal GetTaxableQuantityForPerUnitTaxes(PXCache rowCache, object row, Tax tax, TaxRev taxRevison)
		{
			decimal? lineQuantity = GetLineQty(rowCache, row);
			string lineUOM = GetUOM(rowCache, row);

			if (lineQuantity == null || lineQuantity == 0m || tax.TaxUOM == null)
				return 0m;
	
			// Even in case the TaxUOM and Line UOM are equal we still need to execute conversion 
			// because line quantity is specified in the base UOM, which could differ from the Tax UOM  
			lineQuantity = ConvertLineQtyToTaxUOM(rowCache, row, tax, lineUOM, lineQuantity.Value);

			if (lineQuantity == null)
				return 0m;
			
			return GetAdjustedTaxableQuantity(lineQuantity.Value, taxRevison);
		}

		protected virtual decimal? ConvertLineQtyToTaxUOM(PXCache rowCache, object row, Tax tax, string lineUOM, decimal lineQuantity)
		{
			if (tax.TaxUOM == lineUOM)	//Optimization to avoid conversion at all if document line UOM is equal to per-unit tax UOM
				return lineQuantity;

			int? lineInventoryID = GetInventory(rowCache, row);

			if (lineInventoryID == null && string.IsNullOrEmpty(lineUOM))
			{
				string taxZone = GetTaxZone(rowCache, row);
				string taxCategory = GetTaxCategory(rowCache, row);
				string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(Messages.MissingInventoryAndLineUomForPerUnitTaxErrorMsgFormat,
																		   tax.TaxID, taxZone, taxCategory);
				SetPerUnitTaxUomConversionError(rowCache, row, tax, errorMessage);
				return null;
			}

			decimal? lineQtyInTaxUom = null;

			if (lineInventoryID != null)
			{
				try
				{
					decimal lineQuantityInBaseUomNotRounded = GetNotRoundedLineQuantityInBaseUOM(rowCache, lineInventoryID, lineUOM, lineQuantity);
					lineQtyInTaxUom = INUnitAttribute.ConvertFromBase(rowCache, lineInventoryID, tax.TaxUOM,
																	  lineQuantityInBaseUomNotRounded, INPrecision.QUANTITY);
				}
				catch (PXUnitConversionException)
				{
					lineQtyInTaxUom = null;
				}
			}

			if (lineQtyInTaxUom == null)  //Try to use global conversions
			{
				lineQtyInTaxUom = ConvertLineQtyToTaxUOMWithGlobalConversions(rowCache.Graph, lineUOM, tax.TaxUOM, lineQuantity);

				if (lineQtyInTaxUom == null)
				{
					string taxZone = GetTaxZone(rowCache, row);
					string taxCategory = GetTaxCategory(rowCache, row);
					string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(Messages.MissingUomConversionForPerUnitTaxErrorMsgFormat,
																			   tax.TaxID, taxZone, taxCategory, tax.TaxUOM);
					SetPerUnitTaxUomConversionError(rowCache, row, tax, errorMessage);
				}
			}

			return lineQtyInTaxUom;
		}

		protected virtual decimal GetNotRoundedLineQuantityInBaseUOM(PXCache rowCache, int? lineInventoryID, string lineUOM, decimal lineQuantityInLineUOM)
		{
			return INUnitAttribute.ConvertToBase(rowCache, lineInventoryID, lineUOM, lineQuantityInLineUOM, INPrecision.NOROUND);
		}

		protected decimal? ConvertLineQtyToTaxUOMWithGlobalConversions(PXGraph graph, string lineUOM, string taxUOM, decimal lineQuantity)
		{
			if (INUnitAttribute.TryConvertGlobalUnits(graph, lineUOM, taxUOM, lineQuantity, INPrecision.QUANTITY, out decimal lineQtyInTaxUom))
				return lineQtyInTaxUom;
			else
				return null;
		}

		protected virtual void SetPerUnitTaxUomConversionError(PXCache rowCache, object row, Tax tax, string errorMessage)
		{
			PXException exception = new PXSetPropertyException(errorMessage, PXErrorLevel.Error);

			if (UOM != null)
			{
				string lineUom = GetUOM(rowCache, row);
				rowCache.RaiseExceptionHandling(UOM.Name, row, lineUom, exception);
			}
			else
				throw exception;
		}

		private decimal GetAdjustedTaxableQuantity(decimal lineQty, TaxRev taxRevison)
		{
			if (taxRevison.TaxableMaxQty == null)
				return lineQty;

			return lineQty <= taxRevison.TaxableMaxQty.Value
				? lineQty
				: taxRevison.TaxableMaxQty.Value;
		}

		protected virtual (decimal TaxAmount, decimal CuryTaxAmount) GetTaxAmountForPerUnitTaxWithCorrectSign(PXCache rowCache, object row, Tax tax,
																											  TaxRev taxRevison, PXCache taxDetailCache,
																											  TaxDetail taxDetail, decimal taxableQty)
		{
			var (taxAmount, curyTaxAmount) = GetTaxAmountForPerUnitTax(taxDetailCache, taxRevison, taxDetail, taxableQty);

			if (taxAmount == 0m && curyTaxAmount == 0m)
				return (taxAmount, curyTaxAmount);

			if (InvertPerUnitTaxAmountSign(rowCache, row, tax, taxRevison, taxDetailCache, taxDetail))
				return (-taxAmount, -curyTaxAmount);
			else
				return (taxAmount, curyTaxAmount);
		}

		protected virtual bool InvertPerUnitTaxAmountSign(PXCache rowCache, object row, Tax tax, TaxRev taxRevison,
														  PXCache taxDetailCache, TaxDetail taxDetail)
		{
			return false;
		}

		protected virtual (decimal TaxAmount, decimal CuryTaxAmount) GetTaxAmountForPerUnitTax(PXCache taxDetailCache, TaxRev taxRevison,
																							   TaxDetail taxDetail, decimal taxableQty)
		{
			decimal taxRateForPerUnitTaxes = GetTaxRateForPerUnitTaxes(taxRevison);
			decimal taxAmount = taxableQty * taxRateForPerUnitTaxes;

			PXDBCurrencyAttribute.CuryConvCury(taxDetailCache, taxDetail, taxAmount, out decimal curyTaxAmount);
			curyTaxAmount = PXDBCurrencyAttribute.RoundCury(taxDetailCache, taxDetail, curyTaxAmount, Precision);
			return (taxAmount, curyTaxAmount);
		}

		protected virtual decimal GetTaxRateForPerUnitTaxes(TaxRev taxRevison) =>
			taxRevison.TaxRate > 0m
				? taxRevison.TaxRate.Value
				: 0m;

		protected virtual void FillTaxDetailValuesForPerUnitTax(PXCache taxDetailCache, Tax tax, TaxRev taxRevision, TaxDetail taxDetail,
																PXCache rowCache, object row, decimal taxableQty, decimal curyTaxAmount)
		{
			SetTaxUomForTaxDetail(taxDetailCache, taxDetail, tax.TaxUOM);
			SetTaxableQuantityForTaxDetail(taxDetailCache, taxDetail, taxableQty);

			taxDetail.TaxRate = taxRevision.TaxRate;
			taxDetail.NonDeductibleTaxRate = taxRevision.NonDeductibleTaxRate;
			
			switch (tax.TaxCalcLevel)
			{
				case CSTaxCalcLevel.CalcOnItemQtyInclusively:
					FillLineTaxableAndTaxAmountsForInclusivePerUnitTax(taxDetailCache, taxDetail, rowCache, row, tax);
					break;

				case CSTaxCalcLevel.CalcOnItemQtyExclusively when tax.TaxCalcLevel2Exclude == false:
					CheckThatExclusivePerUnitTaxIsNotUsedWithInclusiveNonPerUnitTax(rowCache, row);
					break;
			}

			bool isExemptTaxCategory = IsExemptTaxCategory(rowCache, row);

			if (!isExemptTaxCategory)
			{
				SetTaxDetailTaxAmount(taxDetailCache, taxDetail, curyTaxAmount);
			}

			FillDiscountAmountsForPerUnitTax(taxDetailCache, taxDetail);

			if (taxRevision.TaxID != null && tax.DirectTax != true)
			{
				taxDetailCache.Update(taxDetail);
			}
			else
			{
				Delete(taxDetailCache, taxDetail);
			}
		}

		private void FillDiscountAmountsForPerUnitTax(PXCache taxDetailCache, TaxDetail taxDetail)
		{
			const decimal curyTaxableDiscountAmt = 0m;
			const decimal curyTaxDiscountAmt = 0m;

			SetValueOptional(taxDetailCache, taxDetail, curyTaxableDiscountAmt, _CuryTaxableDiscountAmt);
			SetValueOptional(taxDetailCache, taxDetail, curyTaxDiscountAmt, _CuryTaxDiscountAmt);
		}

		private void CheckThatExclusivePerUnitTaxIsNotUsedWithInclusiveNonPerUnitTax(PXCache rowCache, object row)
		{
			var hasInclusiveNonPerUnitTaxes = SelectInclusiveTaxes(rowCache.Graph, row)
															.Select(taxRow => PXResult.Unwrap<Tax>(taxRow))
															.Any(inclusiveTax => inclusiveTax != null && !IsPerUnitTax(inclusiveTax));
			if (hasInclusiveNonPerUnitTaxes)
			{
				PXTrace.WriteInformation(Messages.CombinationOfExclusivePerUnitTaxAndInclusveTaxIsForbiddenErrorMsg);
				throw new PXSetPropertyException(Messages.CombinationOfExclusivePerUnitTaxAndInclusveTaxIsForbiddenErrorMsg, PXErrorLevel.Error);
			}
		}

		private void FillLineTaxableAndTaxAmountsForInclusivePerUnitTax(PXCache taxDetailCache, TaxDetail taxDetail, PXCache rowCache, object row, Tax tax)
		{
			decimal curyLineAmount = GetCuryTranAmt(rowCache, row, tax.TaxCalcType) ?? 0m;
			decimal curyLineInclusivePerUnitTaxAmount = GetInclusivePerUnitTaxesTotalAmount(taxDetailCache, taxDetail, rowCache, row);
			decimal curyLineTaxableAmount = curyLineAmount - curyLineInclusivePerUnitTaxAmount;

			SetTaxableAmt(rowCache, row, curyLineTaxableAmount);
			SetTaxAmt(rowCache, row, curyLineInclusivePerUnitTaxAmount);
		}

		private decimal GetInclusivePerUnitTaxesTotalAmount(PXCache taxDetailCache, TaxDetail taxDetail, PXCache rowCache, object row)
		{
			Type taxDetailsCuryTaxAmountFieldType = taxDetailCache.GetBqlField(_CuryTaxAmt);

			if (taxDetailsCuryTaxAmountFieldType == null)
				return 0m;

			List<object> inclusivePerUnitTaxRows = GetInclusivePerUnitTaxRows(rowCache, row)?.ToList();

			if (inclusivePerUnitTaxRows == null || inclusivePerUnitTaxRows.Count == 0)
				return 0m;

			decimal curyTotalInclusivePerUnitTaxAmount = 0m;

			foreach (PXResult inclusiveNonPerUnitTaxRow in inclusivePerUnitTaxRows)
			{
				TaxRev inclusiveTaxRevision = inclusiveNonPerUnitTaxRow.GetItem<TaxRev>();
				Tax currentInclusiveTax = inclusiveNonPerUnitTaxRow.GetItem<Tax>();
				decimal taxableQty = GetTaxableQuantityForPerUnitTaxes(rowCache, row, currentInclusiveTax, inclusiveTaxRevision);
				decimal? taxRate = inclusiveTaxRevision.TaxRate;
				var (_, curyTaxAmount) = GetTaxAmountForPerUnitTaxWithCorrectSign(rowCache, row, currentInclusiveTax, inclusiveTaxRevision, 
																				  taxDetailCache, taxDetail, taxableQty);
				if (currentInclusiveTax.ReverseTax == true)
				{
					curyTaxAmount = -curyTaxAmount;
				}

				curyTotalInclusivePerUnitTaxAmount += curyTaxAmount;
			}

			return curyTotalInclusivePerUnitTaxAmount;
		}

		private IEnumerable<object> GetInclusivePerUnitTaxRows(PXCache rowCache, object row)
		{
			List<object> inclusiveTaxes = SelectInclusiveTaxes(rowCache.Graph, row);

			if (inclusiveTaxes == null || inclusiveTaxes.Count == 0)
				yield break;

			foreach (PXResult inclusiveTaxRow in inclusiveTaxes)
			{
				Tax inclusiveTax = inclusiveTaxRow.GetItem<Tax>();

				if (inclusiveTax != null && IsPerUnitTax(inclusiveTax))
				{
					yield return inclusiveTaxRow;
				}
			}
		}
		#endregion

		#region Calculation of Per Unit Tax correction to taxable amount for other taxes
		protected virtual decimal GetPerUnitTaxAmountForTaxableAdjustmentCalculation(Tax taxForTaxableAdustment, TaxDetail taxDetail, PXCache taxDetailCache,
																					 object row, PXCache rowCache)
		{
			if (taxForTaxableAdustment.TaxType == CSTaxType.PerUnit)
				return 0m;

			PerUnitTaxesAdjustmentToTaxableCalculator taxAdjustmentCalculator = GetPerUnitTaxAdjustmentCalculator();

			if (taxAdjustmentCalculator == null)
				return 0m;

			decimal? taxAdjustment =
				taxAdjustmentCalculator?.GetPerUnitTaxAmountForTaxableAdjustmentCalculation(taxForTaxableAdustment, taxDetail, taxDetailCache, row, rowCache,
															curyTaxAmtFieldName: _CuryTaxAmt,
															perUnitTaxSelector: () => SelectPerUnitTaxesForTaxableAdjustmentCalculation(taxDetailCache.Graph, row));
			return taxAdjustment ?? 0m;
		}

		protected virtual PerUnitTaxesAdjustmentToTaxableCalculator GetPerUnitTaxAdjustmentCalculator() =>
			PerUnitTaxesAdjustmentToTaxableCalculator.Instance;

		protected virtual List<object> SelectPerUnitTaxes(PXGraph graph, object row) =>
			IsExemptTaxCategory(graph, row)
				? new List<object>()
				: SelectTaxes<Where<Tax.taxType, Equal<CSTaxType.perUnit>>>(graph, row, PXTaxCheck.Line);

		protected virtual List<object> SelectDocumentPerUnitTaxes(PXGraph graph, object document) =>
			SelectTaxes<Where<Tax.taxType, Equal<CSTaxType.perUnit>>>(graph, document, PXTaxCheck.RecalcTotals);

		protected virtual List<object> SelectPerUnitTaxesForTaxableAdjustmentCalculation(PXGraph graph, object row) =>
			IsExemptTaxCategory(graph, row)
				? new List<object>()
				: SelectTaxes<Where<Tax.taxType, Equal<CSTaxType.perUnit>,
								And<Tax.taxCalcLevel2Exclude, Equal<False>>>>(graph, row, PXTaxCheck.Line);
		#endregion

		#region Filling Aggregated Taxes
		/// <summary>
		/// Fill aggregated tax detail for per unit tax.
		/// </summary>
		/// <param name="rowCache">The row cache.</param>
		/// <param name="row">The row.</param>
		/// <param name="tax">The tax.</param>
		/// <param name="taxRevision">The tax revision.</param>
		/// <param name="aggrTaxDetail">The aggregated tax detail.</param>
		/// <param name="taxItems">The tax items.</param>
		/// <returns/>
		protected virtual TaxDetail FillAggregatedTaxDetailForPerUnitTax(PXCache rowCache, object row, Tax tax, TaxRev taxRevision,
																		 TaxDetail aggrTaxDetail, List<object> taxItems)
		{
			PXCache taxDetailCache = rowCache.Graph.Caches[_TaxType];
			PXCache aggrTaxDetCache = rowCache.Graph.Caches[_TaxSumType];
			decimal totalTaxableQty = taxItems.OfType<PXResult>()
											  .Select(item => item[_TaxType])
											  .OfType<TaxDetail>()
											  .Sum(taxDetail => GetTaxableQuantityForTaxDetail(taxDetailCache, taxDetail) ?? 0m);

			SetTaxableQuantityForTaxDetail(aggrTaxDetCache, aggrTaxDetail, totalTaxableQty);
			SetTaxUomForTaxDetail(aggrTaxDetCache, aggrTaxDetail, tax.TaxUOM);
			return aggrTaxDetail;
		}
		#endregion

		#region Graph Events
		#region Row Selected Events
		protected virtual void CheckCurrencyAndRetainageOnDocumentRowSelected(PXCache documentCache, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is IBqlTable document) || !PXAccess.FeatureInstalled<CS.FeaturesSet.perUnitTaxSupport>())
				return;

			var perUnitTaxes = SelectDocumentPerUnitTaxes(documentCache.Graph, document);

			if (perUnitTaxes == null || perUnitTaxes.Count == 0)
				return;

			CheckIfDocumentTaxesAreRetained();
			CheckDocumentCurrency();

			//--------------------------------------Local Function-------------------------------------------
			void CheckIfDocumentTaxesAreRetained()
			{
				var retainedTaxesException = IsRetainedTaxes(documentCache.Graph)
					? new PXSetPropertyException(Messages.PerUnitTaxCannotBeCalculatedForRetainedDocumentsErrorMsg, PXErrorLevel.Error)
					: null;

				string retainedFieldName = RetainageApplyFieldName;

				if (string.IsNullOrWhiteSpace(retainedFieldName))
				{
					if (retainedTaxesException != null)
						throw retainedTaxesException;
					else
						return;
				}

				object retainageApplyCalue = documentCache.GetValue(document, retainedFieldName);
				documentCache.RaiseExceptionHandling(retainedFieldName, document, retainageApplyCalue, retainedTaxesException);
			}

			//-----------------------------------------------------------------
			void CheckDocumentCurrency()
			{
				CurrencyInfoAttribute documentCuryAttribute = documentCache.GetAttributesReadonly(document, name: null)
																		  ?.OfType<CurrencyInfoAttribute>()
																		   .FirstOrDefault();
				if (documentCuryAttribute?.CuryIDField == null)
					return;

				CurrencyInfo docCuryInfo = GetDacCurrencyInfo(documentCache, document);
				var exception = docCuryInfo != null && docCuryInfo.BaseCuryID != docCuryInfo.CuryID
					? new PXSetPropertyException(Messages.PerUnitTaxCannotBeCalculatedInNonBaseCurrencyErrorMsg, PXErrorLevel.Error)
					: null;

				documentCache.RaiseExceptionHandling(documentCuryAttribute.CuryIDField, document, docCuryInfo.CuryID, exception);
			}
		}

		protected virtual void DisablePerUnitTaxesOnAggregatedTaxDetailRowSelected(PXCache aggrTaxDetCache, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is TaxDetail aggrTaxDetail) || !PXAccess.FeatureInstalled<CS.FeaturesSet.perUnitTaxSupport>() ||
				!CheckIfTaxDetailHasPerUnitTaxType(aggrTaxDetCache.Graph, aggrTaxDetail.TaxID))
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled(aggrTaxDetCache, aggrTaxDetail, false);		
		}
		#endregion

		#region Row Deleting Event
		protected virtual void CheckForPerUnitTaxesOnAggregatedTaxRowDeleting(PXCache aggrTaxDetCache, PXRowDeletingEventArgs e)
		{
			if (!(e.Row is TaxDetail aggrTaxDetail) || !PXAccess.FeatureInstalled<CS.FeaturesSet.perUnitTaxSupport>())
			{
				return;
			}

			if (e.ExternalCall && CheckIfTaxDetailHasPerUnitTaxType(aggrTaxDetCache.Graph, aggrTaxDetail.TaxID)) //Forbid to delete per-unit tax from UI
			{
				e.Cancel = true;
				throw new PXException(Messages.PerUnitTaxCannotBeDeletedManuallyErrorMsg);
			}
		}
		#endregion

		#region Check Document\Document Line Per Unit Taxes on persist
		public void DocumentLineCheckPerUnitTaxesOnRowPersisting(PXCache rowCache, PXRowPersistingEventArgs e)
		{
			object row = e.Row;
			if (row == null)
				return;

			List<object> taxes = SelectTaxes(rowCache, row, PXTaxCheck.Line);

			if (taxes == null || taxes.Count == 0)
				return;

			var (hasInclusiveNonPerUnitTax, hasLevel1NonExcludedPerUnitTax, perUnitTaxes) = GetDocumentTaxesInfo(taxes);

			if (hasInclusiveNonPerUnitTax && hasLevel1NonExcludedPerUnitTax)
			{
				PXTrace.WriteInformation(Messages.CombinationOfExclusivePerUnitTaxAndInclusveTaxIsForbiddenErrorMsg);
				throw new PXSetPropertyException(Messages.CombinationOfExclusivePerUnitTaxAndInclusveTaxIsForbiddenErrorMsg, PXErrorLevel.Error);
			}

			CheckPerUnitTaxesForConversionToTaxUomPossibility();

			//------------------------------------------------------Local Function-----------------------------------------------------
			(bool HasInclusiveNonPerUnitTax, bool HasLevel1NonExcludedPerUnitTax, List<Tax> PerUnitTaxes) GetDocumentTaxesInfo(List<object> documentTaxRows)
			{
				bool inclusiveNonPerUnitTax = false;
				bool level1NonExcludedPerUnitTax = false;
				List<Tax> documentPerUnitTaxes = new List<Tax>(documentTaxRows.Count);

				foreach (PXResult taxRow in documentTaxRows)
				{
					Tax tax = PXResult.Unwrap<Tax>(taxRow);

					if (tax.TaxType != CSTaxType.PerUnit)
					{
						inclusiveNonPerUnitTax = inclusiveNonPerUnitTax || tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive;
					}
					else
					{
						documentPerUnitTaxes.Add(tax); 

						if (tax.TaxCalcLevel2Exclude == false)
						{
							level1NonExcludedPerUnitTax = level1NonExcludedPerUnitTax || tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemQtyExclusively;
						}
					}
				}

				return (inclusiveNonPerUnitTax, level1NonExcludedPerUnitTax, documentPerUnitTaxes);
			}

			//---------------------------------------------------------------------------------------
			void CheckPerUnitTaxesForConversionToTaxUomPossibility()
			{
				decimal lineQuantity = GetLineQty(rowCache, row) ?? 0m;
				if (lineQuantity == 0m)
					return;

				string lineUOM = GetUOM(rowCache, row);

				foreach (Tax perUnitTax in perUnitTaxes)
				{
					decimal? lineQtyInTaxUom = ConvertLineQtyToTaxUOM(rowCache, row, perUnitTax, lineUOM, lineQuantity);

					if (lineQtyInTaxUom == null)    //Failed to convert line quantity to tax uom
					{
						e.Cancel = true;
						string error = PXUIFieldAttribute.GetError(rowCache, row, UOM.Name);
						throw new PXException(error);
					}
				}
			}
		}

		protected virtual void DocumentCheckPerUnitTaxesOnRowPersisting(PXCache documentCache, PXRowPersistingEventArgs e)
		{
			if (!(e.Row is IBqlTable document))
				return;

			var perUnitTaxes = SelectDocumentPerUnitTaxes(documentCache.Graph, document);
			bool hasPerUnitTax = perUnitTaxes?.Count > 0;
		
			if (!hasPerUnitTax)
				return;

			if (IsRetainedTaxes(documentCache.Graph))
			{
				PXTrace.WriteInformation(Messages.PerUnitTaxCannotBeCalculatedForRetainedDocumentsErrorMsg);
				throw new PXSetPropertyException(Messages.PerUnitTaxCannotBeCalculatedForRetainedDocumentsErrorMsg, PXErrorLevel.Error);
			}

			CurrencyInfo documentCurrencyInfo = GetDacCurrencyInfo(documentCache, document);

			if (documentCurrencyInfo != null && documentCurrencyInfo.CuryID != documentCurrencyInfo.BaseCuryID)
			{
				PXTrace.WriteInformation(Messages.PerUnitTaxCannotBeCalculatedInNonBaseCurrencyErrorMsg);
				throw new PXSetPropertyException(Messages.PerUnitTaxCannotBeCalculatedInNonBaseCurrencyErrorMsg, PXErrorLevel.Error);
			}		
		}
		#endregion
		#endregion

		private CurrencyInfo GetDacCurrencyInfo(PXCache dacCache, object dac)
		{
			long? curyInfoID = GetCuryInfoIDFromDac(dacCache, dac);

			if (curyInfoID == null)
				return null;

			CurrencyInfo currencyInfo =
				PXSelect<CurrencyInfo,
					Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
				.SelectSingleBound(dacCache.Graph, currents: null, curyInfoID);

			return currencyInfo;
		}

		private long? GetCuryInfoIDFromDac(PXCache dacCache, object dac)
		{
			if (dacCache == null || dac == null)
				return null;

			var dacCuryAttribute = dacCache.GetAttributesReadonly(dac, name: null)
										  ?.OfType<CurrencyInfoAttribute>()
										   .FirstOrDefault();

			if (dacCuryAttribute?.FieldName == null)
				return null;

			int ordinal = dacCache.GetFieldOrdinal(dacCuryAttribute.FieldName);
			return ordinal >= 0
				? dacCache.GetValue(dac, ordinal) as long?
				: null;
		}

		private bool CheckIfTaxDetailHasPerUnitTaxType(PXGraph graph, string taxID)
		{
			if (string.IsNullOrWhiteSpace(taxID))
				return false;

			Tax tax = GetTax(graph, taxID);
			return tax?.TaxType == CSTaxType.PerUnit;
		}

		private Tax GetTax(PXGraph graph, string taxID) =>
			 PXSelect<Tax,
				Where<Tax.taxID, Equal<Required<Tax.taxID>>>>
			.SelectSingleBound(graph, currents: null, pars: taxID);

		protected virtual bool IsRetainedTaxes(PXGraph graph) => false; 
	}
}
