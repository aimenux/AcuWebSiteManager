using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.TX
{
	/// <summary>
	/// A tax report lines by tax zones reloader - helper which implements "Reload Tax Zones" action of <see cref="TaxReportMaint"/> graph.
	/// </summary>
	internal class TaxReportLinesByTaxZonesReloader
	{
		public const int DefaultMaxCountOfDisplayedLineNumbers = 10;

		private readonly TaxReportMaint taxReportGraph;
		private readonly int maxCountOfDisplayedLineNumbers;		
		private string messageDisplayed;

		/// <summary>
		/// Initializes a new instance of the <see cref="TaxReportLinesByTaxZonesReloader"/> class.
		/// </summary>
		/// <param name="aTaxReportGraph">The tax report graph.</param>
		/// <param name="aMaxCountOfDisplayedLineNumbers">(Optional) The maximum count of displayed line numbers. 
		/// Default is <see cref="DefaultMaxCountOfDisplayedLineNumbers"/></param>
		public TaxReportLinesByTaxZonesReloader(TaxReportMaint aTaxReportGraph, int? aMaxCountOfDisplayedLineNumbers = null)
		{
			if (aTaxReportGraph == null)
			{
				throw new ArgumentNullException(nameof(aTaxReportGraph));
			}

			if (aMaxCountOfDisplayedLineNumbers <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(aMaxCountOfDisplayedLineNumbers), PM.Messages.ValueMustBeGreaterThanZero);
			}

			taxReportGraph = aTaxReportGraph;
			maxCountOfDisplayedLineNumbers = aMaxCountOfDisplayedLineNumbers ?? DefaultMaxCountOfDisplayedLineNumbers;
		}

		public void ReloadTaxReportLinesForTaxZones()
		{
			messageDisplayed = messageDisplayed ?? ReloadTaxReportLinesForTaxZonesInternalAndPrepareChangesDescription();

			if (messageDisplayed != null)
			{
				WebDialogResult result = taxReportGraph.TaxVendor.Ask(Messages.CreateReportLinesForNewTaxZones, messageDisplayed,
																	  MessageButtons.OK, refreshRequired: false);

				if (result.IsPositive())
				{
					//Cleanup during the second pass of the method after the user clicked on OK button
					messageDisplayed = null;
				}
			}
		}

		private string ReloadTaxReportLinesForTaxZonesInternalAndPrepareChangesDescription()
		{
			VendorMaster vendor = taxReportGraph?.TaxVendor.Current;

			if (vendor == null)
				return null;

			Dictionary<int, TaxReportLine> templateLinesByLineNumber =
				PXSelect<TaxReportLine,
				Where<TaxReportLine.vendorID, Equal<Required<VendorMaster.bAccountID>>,
					And<TaxReportLine.tempLineNbr, IsNull,
					And<TaxReportLine.tempLine, Equal<True>>>>>
				.Select(taxReportGraph, vendor.BAccountID)
				.RowCast<TaxReportLine>()
				.ToDictionary(line => line.LineNbr.Value);

			if (templateLinesByLineNumber.Count == 0)
				return PXLocalizer.Localize(Messages.NoLinesByTaxZone, typeof(TX.Messages).FullName);

			Dictionary<string, TaxZone> zonesInSystem = PXSelect<TaxZone>.Select(taxReportGraph)
																		 .RowCast<TaxZone>()
																		 .ToDictionary(zone => zone.TaxZoneID);

			ILookup<int, TaxReportLine> detailLinesByTemplateLineNumber =
				PXSelect<TaxReportLine,
				Where<TaxReportLine.vendorID, Equal<Required<VendorMaster.bAccountID>>,
					And<TaxReportLine.tempLineNbr, IsNotNull>>>
				.Select(taxReportGraph, vendor.BAccountID)
				.RowCast<TaxReportLine>()
				.ToLookup(line => line.TempLineNbr.Value);


			List<TaxReportLine> deletedLines = DeleteChildTaxLinesForOldZones(zonesInSystem, detailLinesByTemplateLineNumber);
			List<TaxReportLine> addedLines = GenerateChildTaxLinesForMissingZones(templateLinesByLineNumber, zonesInSystem,
																				  detailLinesByTemplateLineNumber);

			return GetTextFromReloadTaxZonesResult(addedLines, deletedLines);
		}

		/// <summary>
		/// Generates a child tax lines for missing zones.
		/// </summary>
		/// <param name="templateLinesByLineNumber">The template lines by line number.</param>
		/// <param name="zonesInSystem">The zones in system.</param>
		/// <param name="detailLinesByTemplateLineNumber">The detail lines by template line number.</param>
		/// <returns>
		/// The generated child tax lines for missing zones.
		/// </returns>
		private List<TaxReportLine> GenerateChildTaxLinesForMissingZones(Dictionary<int, TaxReportLine> templateLinesByLineNumber,
																		 Dictionary<string, TaxZone> zonesInSystem, 
																		 ILookup<int, TaxReportLine> detailLinesByTemplateLineNumber)
		{
			Dictionary<int, List<string>> missingZonesByTemplateLineNumber =
				templateLinesByLineNumber.ToDictionary(templatePair => templatePair.Key,
													   templatePair => zonesInSystem.Select(zone => zone.Value.TaxZoneID)
																					.Except(detailLinesByTemplateLineNumber[templatePair.Key].Select(line => line.TaxZoneID))
																					.ToList());

			List<TaxReportLine> addedLines = new List<TaxReportLine>(capacity: 8);

			foreach (KeyValuePair<int, List<string>> templateNumberWithMissingZones in missingZonesByTemplateLineNumber)
			{
				TaxReportLine template = templateLinesByLineNumber[templateNumberWithMissingZones.Key];

				foreach (string missingZoneId in templateNumberWithMissingZones.Value)
				{
					TaxReportLine child = taxReportGraph.CreateChildLine(template, zonesInSystem[missingZoneId]);
					TaxReportLine inserted = taxReportGraph.ReportLine.Cache.Insert(child) as TaxReportLine;

					addedLines.Add(inserted);
				}
			}

			return addedLines;
		}

		/// <summary>
		/// Deletes the child tax lines for old zones.
		/// </summary>
		/// <param name="zonesInSystem">The zones in system.</param>
		/// <param name="detailLinesByTemplateLineNumber">The detail lines by template line number.</param>
		/// <returns>
		/// The deleted child lines.
		/// </returns>
		private List<TaxReportLine> DeleteChildTaxLinesForOldZones(Dictionary<string, TaxZone> zonesInSystem,
																   ILookup<int, TaxReportLine> detailLinesByTemplateLineNumber)
		{
			var detailLinesToDelete = detailLinesByTemplateLineNumber.SelectMany(group => group)
																	 .Where(line => !zonesInSystem.ContainsKey(line.TaxZoneID));

			List<TaxReportLine> deletedLines = detailLinesToDelete.Select(line => taxReportGraph.ReportLine.Cache.Delete(line))
																  .OfType<TaxReportLine>()
																  .ToList();
			return deletedLines;
		}

		private string GetTextFromReloadTaxZonesResult(List<TaxReportLine> addedLines, List<TaxReportLine> deletedLines)
		{
			if (addedLines.Count == 0 && deletedLines.Count == 0)
			{
				return PXLocalizer.Localize(Messages.TheTaxZonesReloadSummaryNoChangesOnReload, typeof(TX.Messages).FullName);								
			}

			string resultsText = PXLocalizer.Localize(Messages.TheTaxZonesReloadSummaryBeginning, typeof(TX.Messages).FullName) + Environment.NewLine;

			if (deletedLines.Count > 0)
			{
				string delLineNumbersString = CreateTextToDisplayFromLineNumbers(deletedLines);
				string localizedDeletedInfo = PXMessages.LocalizeFormatNoPrefixNLA(Messages.TheTaxZonesReloadSummaryDeletedLinesFormat, 
																				   delLineNumbersString);

				resultsText += Environment.NewLine + localizedDeletedInfo;
			}

			if (addedLines.Count > 0)
			{
				string addedLineNumbersString = CreateTextToDisplayFromLineNumbers(addedLines);
				string localizedAddedInfo = PXMessages.LocalizeFormatNoPrefixNLA(Messages.TheTaxZonesReloadSummaryCreatedLinesFormat,
																				 addedLineNumbersString);

				resultsText += Environment.NewLine + localizedAddedInfo;
			}

			return resultsText;
		}

		private string CreateTextToDisplayFromLineNumbers(List<TaxReportLine> lines)
		{
			List<int> changedTemplateLineNumbers = lines.Select(line => line.SortOrder.Value)
														.Distinct()
														.OrderBy(orderNumber => orderNumber)
														.ToList();

			string lineNumbersString = string.Join(", ", changedTemplateLineNumbers.Take(maxCountOfDisplayedLineNumbers));

			if (changedTemplateLineNumbers.Count > maxCountOfDisplayedLineNumbers)
			{
				int countOfNotDisplayedLines = changedTemplateLineNumbers.Count - maxCountOfDisplayedLineNumbers;
				string localizedSuffix = PXMessages.LocalizeFormatNoPrefixNLA(Messages.TheTaxZonesReloadSummaryManyLinesSuffixFormat, 
																			  countOfNotDisplayedLines);

				lineNumbersString += " " + localizedSuffix;
			}

			return lineNumbersString;
		}		
	}
}
