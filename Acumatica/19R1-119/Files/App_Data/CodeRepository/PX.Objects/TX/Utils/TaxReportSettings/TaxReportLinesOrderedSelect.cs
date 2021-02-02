using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PX.Data;
using PX.Common;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.Objects.TX.Descriptor;

namespace PX.Objects.TX
{
	/// <summary>
	/// A <see cref="TaxReportMaint"/> graph's custom view for tax report lines which extends the <see cref="PXOrderedSelect{Primary, Table, Where, OrderBy}"/>. 
	/// The base view allows drag and drop functionality in the grid but auto reenumerate lines which is undesirable for tax report lines, so this derived view customize enumeration process.
	/// </summary>
	public class TaxReportLinesOrderedSelect :
		PXOrderedSelect<VendorMaster, TaxReportLine,
			Where<TaxReportLine.vendorID, Equal<Current<VendorMaster.bAccountID>>,
				And<
				Where2<
					Where<Current<VendorMaster.showNoTemp>, Equal<False>,
						And<TaxReportLine.tempLineNbr, IsNull>>,
					Or<
					   Where<Current<VendorMaster.showNoTemp>, Equal<True>,
					   And2<
							Where<TaxReportLine.tempLineNbr, IsNull,
								And<TaxReportLine.tempLine, Equal<False>>>,
							Or<TaxReportLine.tempLineNbr, IsNotNull>>>>>>>,
			OrderBy<
				Asc<TaxReportLine.sortOrder,
				Asc<TaxReportLine.taxZoneID>>>>
	{
		private readonly TaxReportMaint taxReportGraph;

		public bool AllowDragDrop { get; set; } = true;

		public bool AllowResetOrder { get; set; }

		public bool AllowAutoRenumbering { get; set; }
		
		public TaxReportLinesOrderedSelect(PXGraph graph) : base(graph)
		{
			RenumberTailOnDelete = false;
			taxReportGraph = graph as TaxReportMaint;
		}

		public TaxReportLinesOrderedSelect(PXGraph graph, Delegate handler) : base(graph, handler)
		{
			RenumberTailOnDelete = false;
			taxReportGraph = graph as TaxReportMaint;
		}

		public void ArrowUpForCurrentRow() => MoveRowByArrow(moveUp: true);

		public void ArrowDownForCurrentRow() => MoveRowByArrow(moveUp: false);

		private void MoveRowByArrow(bool moveUp)
		{
			TaxReportLine currentLine = Current;
			
			if (!AllowDragDrop || currentLine == null)
				return;

			var lines = Select().RowCast<TaxReportLine>()
								.Where(line => !Cache.ObjectsEqual(line, currentLine));

			TaxReportLine nextLine;

			if (moveUp)
			{
				nextLine = lines.Where(line => line.SortOrder < currentLine.SortOrder)
								.OrderByDescending(line => line.SortOrder)
								.FirstOrDefault();
			}
			else
			{
				nextLine = lines.Where(line => line.SortOrder > currentLine.SortOrder)
								.OrderBy(line => line.SortOrder)
								.FirstOrDefault();
			}

			if (nextLine == null)
				return;

			int? tempVar = nextLine.SortOrder;
			nextLine.SortOrder = currentLine.SortOrder;
			currentLine.SortOrder = tempVar;

			Cache.SmartSetStatus(currentLine, PXEntryStatus.Updated);
			Cache.SmartSetStatus(nextLine, PXEntryStatus.Updated);

			RenumberDetailTaxLines(new TaxReportLine[] { }, new[] { currentLine, nextLine });
			Cache.IsDirty = true;							
		}

		protected override IEnumerable ResetOrder(PXAdapter adapter)
		{
			if (!AllowResetOrder)
			{
				return adapter.Get();
			}

			return base.ResetOrder(adapter);
		}

		protected override IEnumerable PasteLine(PXAdapter adapter)
		{
			if (!AllowDragDrop)
			{
				return adapter.Get();
			}

			return base.PasteLine(adapter);
		}

		protected override void PasteLines(TaxReportLine focus, IList<TaxReportLine> moved)
		{
			if (!Cache.AllowUpdate || focus?.SortOrder == null || moved.Count == 0)
				return;

			int insertPos = focus.SortOrder.Value;
			int firstLinePos = moved.Min(line => line.SortOrder.Value);

			if (insertPos == firstLinePos)
				return;
			
			int firstTaxLinesSetDivisionPoint, secondTaxLinesSetDivisionPoint;

			if (insertPos > firstLinePos)	//Dragging down
			{
				firstTaxLinesSetDivisionPoint = firstLinePos;
				secondTaxLinesSetDivisionPoint = insertPos;
			}
			else                            //Dragging up
			{
				firstTaxLinesSetDivisionPoint = insertPos;

				//Sort order of the last of moved line. Is necessary to handle holes in selection during between lines list filling
				secondTaxLinesSetDivisionPoint = moved.Max(line => line.SortOrder.Value);  
			}

			HashSet<int> movedLineNumbers = moved.Select(line => line.LineNbr.Value).ToHashSet();

			List<TaxReportLine> betweenLines = new List<TaxReportLine>();
			List<TaxReportLine> movedLines = new List<TaxReportLine>(capacity: moved.Count);
			List<TaxReportLine> restLines = new List<TaxReportLine>();

			foreach (TaxReportLine line in Select())
			{
				bool lineIsMoved = movedLineNumbers.Contains(line.LineNbr.Value);

				if (lineIsMoved)
				{
					movedLines.Add(line);
				}
				else if (line.SortOrder >= firstTaxLinesSetDivisionPoint)
				{
					if (Cache.InsertPositionMode)
					{
						if (line.SortOrder <= secondTaxLinesSetDivisionPoint)
						{
							betweenLines.Add(line);
						}
						else
						{
							restLines.Add(line);
						}
					}
					else
					{
						if (line.SortOrder < secondTaxLinesSetDivisionPoint)
						{
							betweenLines.Add(line);
						}
						else
						{
							restLines.Add(line);
						}
					}
				}
			}

			ReEnumerateOnPasteLines(betweenLines, movedLines, restLines, firstLinePos, insertPos);

			View.Clear();   //clears stored cache so that grid lines are reordered.		
		}

		protected override void ReEnumerateOnPasteLines(List<TaxReportLine> betweenLines, List<TaxReportLine> movedLines, List<TaxReportLine> restLines, 
														int firstRowPos, int insertPos)
		{
			if (AllowAutoRenumbering)
			{
				base.ReEnumerateOnPasteLines(betweenLines, movedLines, restLines, firstRowPos, insertPos);
				return;
			}

			RenumberMainTaxLines(betweenLines, movedLines, moveUp: firstRowPos > insertPos);
			RenumberDetailTaxLines(betweenLines, movedLines);

			if (movedLines.Count > 0 && betweenLines.Count > 0)
			{
				Cache.IsDirty = true;
			}
		}

		private void RenumberMainTaxLines(List<TaxReportLine> betweenLines, List<TaxReportLine> movedLines, bool moveUp)
		{
			var orderedBetweenLines = moveUp 
				? betweenLines.OrderByDescending(line => line.SortOrder) 
				: betweenLines.OrderBy(line => line.SortOrder);

			IEnumerable<TaxReportLine> orderedMovedLines = movedLines;

			if (movedLines.Count > 1)
			{
				orderedMovedLines = moveUp
					? movedLines.OrderBy(line => line.SortOrder)
					: movedLines.OrderByDescending(line => line.SortOrder);
			}

			Func<TaxReportLine, TaxReportLine, bool> betweenLinesFilterBySortOrder;
			
			if (moveUp)
			{
				betweenLinesFilterBySortOrder = (betweenLine, movedLine) => betweenLine.SortOrder < movedLine.SortOrder;
			}
			else
			{
				betweenLinesFilterBySortOrder = (betweenLine, movedLine) => betweenLine.SortOrder > movedLine.SortOrder;
			}
			
			foreach (TaxReportLine movedLine in orderedMovedLines)
			{
				foreach (TaxReportLine lineBetween in orderedBetweenLines.Where(line => betweenLinesFilterBySortOrder(line, movedLine)))
				{					
					int? tempVar = lineBetween.SortOrder;
					lineBetween.SortOrder = movedLine.SortOrder;
					movedLine.SortOrder = tempVar;
				}
			}

			//Update statuses
			betweenLines.Concat(movedLines)
						.ForEach(line => Cache.SmartSetStatus(line, PXEntryStatus.Updated));	
		}

		private void RenumberDetailTaxLines(IList<TaxReportLine> betweenLines, IList<TaxReportLine> movedLines)
		{
			VendorMaster vendorMaster = taxReportGraph.TaxVendor.Current;

			if (vendorMaster == null)
				return;

			Dictionary<int, TaxReportLine> mainLines = betweenLines.Concat(movedLines)
																   .Where(line => line.TempLineNbr == null && line.TempLine == true)
																   .ToDictionary(line => line.LineNbr.Value);

			if (mainLines.Count == 0)
				return;

			var detailLinesGroupedByMainLines =
				PXSelect<TaxReportLine,
					Where<TaxReportLine.vendorID, Equal<Required<VendorMaster.bAccountID>>,
					  And<TaxReportLine.tempLineNbr, IsNotNull,
					  And<TaxReportLine.tempLineNbr, In<Required<TaxReportLine.tempLineNbr>>>>>>
				.Select(taxReportGraph, vendorMaster.BAccountID, mainLines.Keys.ToArray())
				.RowCast<TaxReportLine>()
				.GroupBy(line => line.TempLineNbr.Value);

			foreach (var detailLinesGroup in detailLinesGroupedByMainLines)
			{
				TaxReportLine mainLine;

				if (!mainLines.TryGetValue(detailLinesGroup.Key, out mainLine))
					continue;
				 
				foreach (TaxReportLine detailLine in detailLinesGroup)
				{
					detailLine.SortOrder = mainLine.SortOrder;
					Cache.SmartSetStatus(detailLine, PXEntryStatus.Updated);
				}
			}
		}

		protected override void OnRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			TaxReportLine insertedLine = e.Row as TaxReportLine;
			VendorMaster vendorMaster = taxReportGraph.TaxVendor.Current;

			if (vendorMaster == null || insertedLine == null || insertedLine.SortOrder != null)
				return;

			List<TaxReportLine> taxLines = Select().RowCast<TaxReportLine>()
												   .Where(line => line.SortOrder.HasValue)
												   .ToList();

			insertedLine.SortOrder = taxLines.Count > 0
				? taxLines.Max(line => line.SortOrder) + 1
				: insertedLine.LineNbr;						   
		}

		protected override void OnBeforeGraphPersist(PXGraph graph)
		{
			if (AllowAutoRenumbering)
			{
				base.OnBeforeGraphPersist(graph);
			}
		}

		public override void RenumberAll()
		{
			if (AllowAutoRenumbering)
			{
				base.RenumberAll();
			}		
		}

		public override void RenumberAll(IComparer<PXResult> comparer)
		{
			if (AllowAutoRenumbering)
			{
				base.RenumberAll(comparer);
			}
		}

		public override void RenumberTail()
		{
			if (AllowAutoRenumbering)
			{
				base.RenumberTail();
			}
		}
	}
}
