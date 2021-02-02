using System;
using PX.Data;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CS
{
	public class NumberingMaint : PXGraph<NumberingMaint, Numbering>
	{
		public PXSelectReadonly<Numbering> Numbering;
		public PXSelect<Numbering> Header;
		public PXSelect<NumberingSequence, Where<NumberingSequence.numberingID, Equal<Optional<Numbering.numberingID>>>> Sequence;
		NumberingDetector detector;
		public NumberingMaint()
		{
			PXUIFieldAttribute.SetRequired<NumberingSequence.nbrStep>(Sequence.Cache, true);
			detector = new NumberingDetector(this, ApplicationAreas.StandardFinance | ApplicationAreas.AdvancedFinance |
				ApplicationAreas.Distribution | ApplicationAreas.Project | ApplicationAreas.TimeAndExpenses);
		}

		private void CheckNumbers(PXCache cache, object row)
		{
			NumberingSequence currow = row as NumberingSequence;
			if (currow == null) return;

			currow.StartNbr = currow.StartNbr.SafeTrimStart();
			currow.EndNbr = currow.EndNbr.SafeTrimStart();
			currow.LastNbr = currow.LastNbr.SafeTrimStart();
			currow.WarnNbr = currow.WarnNbr.SafeTrimStart();

			string nbr = null;
			string s_mask = (currow.StartNbr == null) ? null : NumberingDetector.MakeMask(currow.StartNbr, ref nbr);
			string s_nbr = nbr;
			string e_mask = (currow.EndNbr == null) ? null : NumberingDetector.MakeMask(currow.EndNbr, ref nbr);
			string e_nbr = nbr;
			string l_mask = (currow.LastNbr == null) ? null : NumberingDetector.MakeMask(currow.LastNbr, ref nbr);
			string l_nbr = nbr;
			string w_mask = (currow.WarnNbr == null) ? null : NumberingDetector.MakeMask(currow.WarnNbr, ref nbr);
			string w_nbr = nbr;

			if (s_mask != e_mask || l_mask != null && s_mask != l_mask || w_mask != null && s_mask != w_mask)
			{
				cache.RaiseExceptionHandling<NumberingSequence.numberingID>(currow, currow.NumberingID, new PXSetPropertyException(Messages.SameNumberingMask, PXErrorLevel.RowError));
				return;
			}
			if (currow.StartNbr?.CompareTo(currow.EndNbr) >= 0)
			{
				cache.RaiseExceptionHandling<NumberingSequence.numberingID>(currow, currow.NumberingID, new PXSetPropertyException(Messages.StartNumMustBeGreaterEndNum, PXErrorLevel.RowError));
				return;
			}
			if (currow.WarnNbr?.CompareTo(currow.EndNbr) >= 0)
			{
				cache.RaiseExceptionHandling<NumberingSequence.numberingID>(currow, currow.NumberingID, new PXSetPropertyException(Messages.WarnNumMustBeLessEndNum, PXErrorLevel.RowError));
				return;
			}
			if (currow.WarnNbr?.CompareTo(currow.StartNbr) <= 0)
			{
				cache.RaiseExceptionHandling<NumberingSequence.numberingID>(currow, currow.NumberingID, new PXSetPropertyException(Messages.WarnNumMustBeGreaterStartNum, PXErrorLevel.RowError));
				return;
			}
			if (currow.LastNbr?.CompareTo(currow.EndNbr) >= 0)
			{
				cache.RaiseExceptionHandling<NumberingSequence.numberingID>(currow, currow.NumberingID,  new PXSetPropertyException(Messages.LastNumMustBeLessEndNum, PXErrorLevel.RowError));
				return;
			}
			if (currow.LastNbr?.CompareTo(currow.StartNbr) < 0 && !EqualLastAndStartMinusOne(s_nbr, l_nbr))
			{
				cache.RaiseExceptionHandling<NumberingSequence.numberingID>(currow, currow.NumberingID, new PXSetPropertyException(Messages.LastNumMustBeGreaterOrEqualStartNum, PXErrorLevel.RowError));
				return;
			}
		}

		private bool EqualLastAndStartMinusOne(string start, string last)
		{
			char[] charStart = start.ToCharArray();
			for (int i = charStart.Length - 1; i >= 0; i--)
			{
				if (charStart[i] == '0')
					charStart[i] = '9';
				else
				{
					charStart[i] = Convert.ToChar((Convert.ToInt16(charStart[i]) - 1));
					break;
				}
			}
			return String.Equals(new string(charStart), last);
		}

		protected virtual void NumberingSequence_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			CheckNumbers(cache, e.Row);
		}

		private struct NumPair
		{
			public int? BranchID;
			public DateTime? StartDate;

			public NumPair(int? BranchID, DateTime? StartDate)
			{
				this.BranchID = BranchID;
				this.StartDate = StartDate;
			}
		}

		#region Events
		#region Numbering
		protected virtual void Numbering_NewSymbol_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
                e.NewValue = e.NewValue.ToString().TrimStart();
                foreach (NumberingSequence seq in Sequence.Select(((Numbering)e.Row).NumberingID))
				{
					if (seq.StartNbr.Length < ((string)e.NewValue).Length)
					{
						throw new PXSetPropertyException(Messages.NewSymbolLength);
					}
				}
			}
		}

		protected virtual void Numbering_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			Numbering row = (Numbering)e.Row;
			if (row.UserNumbering != true && string.IsNullOrEmpty(row.NewSymbol))
			{
				cache.RaiseExceptionHandling<Numbering.newSymbol>(e.Row, row.NewSymbol, new PXException(Messages.OneFieldMustBeFilled));
			}

			if (row.NewSymbol != null)
			{
				foreach (NumberingSequence num in Sequence.Select(row.NumberingID))
				{
					if (num.StartNbr.Length < row.NewSymbol.Length)
					{
						cache.RaiseExceptionHandling<Numbering.newSymbol>(e.Row, row.NewSymbol, new PXException(Messages.NewSymbolLength));
						break;
					}
				}
			}
		}
		protected virtual void Numbering_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			Numbering num = (Numbering)e.Row;
			string dimensionID;
			string segmentID;
			if (detector.IsInUseSegments(num.NumberingID, out dimensionID, out segmentID))
			{
				cache.RaiseExceptionHandling<Numbering.numberingID>(num, num.NumberingID, new PXSetPropertyException(Messages.NumberingIsUsedFailedDelete, dimensionID, segmentID));
				e.Cancel = true;
				return;
			}
			string workBookID;
			if (detector.IsInUseWorkbooks(num.NumberingID, out workBookID))
			{
				cache.RaiseExceptionHandling<Numbering.numberingID>(num, num.NumberingID, new PXSetPropertyException(Messages.NumberingIsUsedFailedDeleteWorkBook, workBookID));
				e.Cancel = true;
				return;
			}
			string cacheName;
			string fieldName;
			if (detector.IsInUseSetups(num.NumberingID, out cacheName, out fieldName))
			{
				cache.RaiseExceptionHandling<Numbering.numberingID>(num, num.NumberingID, new PXSetPropertyException(Messages.NumberingIsUsedFailedDeleteSetup, cacheName, fieldName));
				e.Cancel = true;
				return;
			}
		}
		protected virtual void Numbering_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Numbering row = (Numbering)e.Row;
			PXUIFieldAttribute.SetRequired<Numbering.newSymbol>(cache, row.UserNumbering == false);
		}
		#endregion
		#region NumberingSequence
		protected virtual void NumberingSequence_EndNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			NumberingSequence row = e.Row as NumberingSequence;
			if (row != null)
			{

				if (!string.IsNullOrEmpty(row.StartNbr) && string.IsNullOrEmpty(row.EndNbr))

				{
					char[] result = row.StartNbr.ToCharArray();

					for (int i = result.Length - 1; i >= 0; i--)
					{
						if (char.IsDigit(result[i]))
						{
							result[i] = '9';
						}
						else
							break;
					}

					row.EndNbr = new string(result);
				}
			}
		}
		protected virtual void NumberingSequence_StartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			NumberingSequence row = e.Row as NumberingSequence;
			if (row != null)
			{
				if (row.StartDate == null)
				{
					PXResultset<NumberingSequence> resultset = PXSelect<NumberingSequence, Where<NumberingSequence.numberingID, Equal<Current<Numbering.numberingID>>>>.SelectWindowed(this, 0, 2);
					if (resultset.Count < 1)
					{
						row.StartDate = new DateTime(1900, 1, 1);
					}
				}
			}
		}
		protected virtual void NumberingSequence_LastNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			NumberingSequence row = e.Row as NumberingSequence;
			if (row != null)
			{
				if (string.IsNullOrEmpty(row.LastNbr) && !string.IsNullOrEmpty(row.StartNbr))
				{
					char[] startNumber = row.StartNbr.ToCharArray();
					char lastChar = startNumber[startNumber.GetUpperBound(0)];

					if (char.IsDigit(lastChar))
					{
						int digit = int.Parse(new string(new char[1] { lastChar }));

						if (digit > 0)
						{
							startNumber[startNumber.GetUpperBound(0)] = (digit - 1).ToString().ToCharArray()[0];
						}

					}

					row.LastNbr = new string(startNumber);
				}
			}
		}
		protected virtual void NumberingSequence_NbrStep_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null && (int)e.NewValue == 0)
			{
				throw new PXSetPropertyException(Messages.ZeroIncrementIsNotAllowed);
			}
		}
		protected virtual void NumberingSequence_WarnNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			NumberingSequence row = e.Row as NumberingSequence;
			if (row != null)
			{
				if (!string.IsNullOrEmpty(row.EndNbr) && string.IsNullOrEmpty(row.WarnNbr))
				{
					if (row.EndNbr.Length >= 3)
					{
						if (char.IsDigit(row.EndNbr[row.EndNbr.Length - 1]) &&
							char.IsDigit(row.EndNbr[row.EndNbr.Length - 2]) &&
							char.IsDigit(row.EndNbr[row.EndNbr.Length - 3])
							)
						{
							int number = int.Parse(row.EndNbr.Substring(row.EndNbr.Length - 3, 3));

							if (number > 100)
							{
								int warningNumber = number - 100;

								string str = string.Format("{0:000}", warningNumber);
								row.WarnNbr = row.EndNbr.Substring(0, row.EndNbr.Length - 3) + str;
							}

						}
					}
				}
			}
		}

		protected virtual void NumberingSequence_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			NumberingSequence num = (NumberingSequence)e.Row;
			CheckNumbers(cache, num);
			foreach (PXResult<Dimension, Segment> r in PXSelectJoin<Dimension, InnerJoin<Segment, On<Segment.dimensionID, Equal<Dimension.dimensionID>>>, Where<Dimension.numberingID, Equal<Optional<Numbering.numberingID>>, And<Segment.autoNumber, Equal<Optional<Segment.autoNumber>>>>>.Select(this, ((NumberingSequence)e.Row).NumberingID, (bool)true))
			{
				Dimension dim = r;
				Segment segrow = r;

				if (num.StartNbr.Length != segrow.Length)
				{
					cache.RaiseExceptionHandling<NumberingSequence.startNbr>(num, num.StartNbr, new PXSetPropertyException(Messages.NumberingViolatesSegmentDef, segrow.DimensionID, segrow.SegmentID.ToString()));
				}

				string mask = Regex.Replace(Regex.Replace(num.StartNbr, "[0-9]", "9"), "[^0-9]", "?");
				if (segrow.EditMask == "?" && mask.Contains("9") || segrow.EditMask == "9" && mask.Contains("?"))
				{
					cache.RaiseExceptionHandling<NumberingSequence.startNbr>(num, num.StartNbr, new PXSetPropertyException(Messages.NumberingViolatesSegmentDef, segrow.DimensionID, segrow.SegmentID.ToString()));
				}


			}
		}
		#endregion
		#endregion

		public override void Persist()
		{
			HashSet<NumPair> uniqueStartDates = new HashSet<NumPair>();
			foreach (NumberingSequence ns in Sequence.Select())
			{
				if (Sequence.Cache.GetStatus(ns) == PXEntryStatus.Notchanged && ns.StartDate != null)
				{
					uniqueStartDates.Add(new NumPair(ns.NBranchID, ns.StartDate));
				}
			}
			foreach (NumberingSequence ns in Sequence.Cache.Inserted)
			{
				if (ns.StartDate != null && !uniqueStartDates.Add(new NumPair(ns.NBranchID, ns.StartDate)))
				{
					Sequence.Cache.RaiseExceptionHandling<NumberingSequence.startDate>(ns, ns.StartDate, new PXSetPropertyException(Messages.StartDateNotUnique));
				}
			}
			foreach (NumberingSequence ns in Sequence.Cache.Updated)
			{
				if (ns.StartDate != null && !uniqueStartDates.Add(new NumPair(ns.NBranchID, ns.StartDate)))
				{
					Sequence.Cache.RaiseExceptionHandling<NumberingSequence.startDate>(ns, ns.StartDate, new PXSetPropertyException(Messages.StartDateNotUnique));
				}
			}

			base.Persist();
		}
	}
}
