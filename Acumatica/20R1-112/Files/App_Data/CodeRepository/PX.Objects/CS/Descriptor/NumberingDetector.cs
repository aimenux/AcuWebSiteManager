using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PX.Data;

namespace PX.Objects.CS
{
	[Flags]
	public enum ApplicationAreas
	{
		None = 0,
		StandardFinance = 1,
		AdvancedFinance = 2,
		Distribution = 4,
		Project = 8,
		TimeAndExpenses = 16
	}

	public class NumberingDetector
	{
		private readonly Dictionary<Type, PXSelectBase> setupNumberingSequences;

		private readonly PXGraph parentGraph;

		public NumberingDetector(PXGraph graph, ApplicationAreas areas)
		{
			parentGraph = graph;
			setupNumberingSequences = new Dictionary<Type, PXSelectBase>();
			if (areas.HasFlag(ApplicationAreas.StandardFinance))
			{
				PXSelect<AR.ARSetup> arsetup = new PXSelect<AR.ARSetup>(graph);
				PXSelect<AP.APSetup> apsetup = new PXSelect<AP.APSetup>(graph);
				PXSelect<CA.CASetup> casetup = new PXSelect<CA.CASetup>(graph);
				PXSelect<GL.GLSetup> glsetup = new PXSelect<GL.GLSetup>(graph);

				setupNumberingSequences.Add(typeof(AR.ARSetup.batchNumberingID), arsetup);
				setupNumberingSequences.Add(typeof(AR.ARSetup.creditAdjNumberingID), arsetup);
				setupNumberingSequences.Add(typeof(AR.ARSetup.debitAdjNumberingID), arsetup);
				setupNumberingSequences.Add(typeof(AR.ARSetup.finChargeNumberingID), arsetup);
				setupNumberingSequences.Add(typeof(AR.ARSetup.invoiceNumberingID), arsetup);
				setupNumberingSequences.Add(typeof(AR.ARSetup.paymentNumberingID), arsetup);
				setupNumberingSequences.Add(typeof(AR.ARSetup.priceWSNumberingID), arsetup);
				setupNumberingSequences.Add(typeof(AR.ARSetup.usageNumberingID), arsetup);
				setupNumberingSequences.Add(typeof(AR.ARSetup.writeOffNumberingID), arsetup);

				setupNumberingSequences.Add(typeof(AP.APSetup.batchNumberingID), apsetup);
				setupNumberingSequences.Add(typeof(AP.APSetup.creditAdjNumberingID), apsetup);
				setupNumberingSequences.Add(typeof(AP.APSetup.debitAdjNumberingID), apsetup);
				setupNumberingSequences.Add(typeof(AP.APSetup.checkNumberingID), apsetup);
				setupNumberingSequences.Add(typeof(AP.APSetup.invoiceNumberingID), apsetup);
				setupNumberingSequences.Add(typeof(AP.APSetup.priceWSNumberingID), apsetup);

				setupNumberingSequences.Add(typeof(CA.CASetup.batchNumberingID), casetup);
				setupNumberingSequences.Add(typeof(CA.CASetup.cABatchNumberingID), casetup);
				setupNumberingSequences.Add(typeof(CA.CASetup.cAImportPaymentsNumberingID), casetup);
				setupNumberingSequences.Add(typeof(CA.CASetup.cAStatementNumberingID), casetup);
				setupNumberingSequences.Add(typeof(CA.CASetup.registerNumberingID), casetup);
				setupNumberingSequences.Add(typeof(CA.CASetup.transferNumberingID), casetup);
				setupNumberingSequences.Add(typeof(CA.CASetup.corpCardNumberingID), casetup);

				setupNumberingSequences.Add(typeof(GL.GLSetup.batchNumberingID), glsetup);
				setupNumberingSequences.Add(typeof(GL.GLSetup.allocationNumberingID), glsetup);
				setupNumberingSequences.Add(typeof(GL.GLSetup.docBatchNumberingID), glsetup);
				setupNumberingSequences.Add(typeof(GL.GLSetup.scheduleNumberingID), glsetup);
				setupNumberingSequences.Add(typeof(GL.GLSetup.tBImportNumberingID), glsetup);

				var cashaccount = new PXSelect<CA.CashAccount, Where<CA.CashAccount.reconNumberingID, Equal<Required<Numbering.numberingID>>>>(graph);
				setupNumberingSequences.Add(typeof(CA.CashAccount.reconNumberingID), cashaccount);
			}
			if (areas.HasFlag(ApplicationAreas.AdvancedFinance))
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.fixedAsset>())
				{
					PXSelect<FA.FASetup> fasetup = new PXSelect<FA.FASetup>(graph);
					setupNumberingSequences.Add(typeof(FA.FASetup.registerNumberingID), fasetup);
					setupNumberingSequences.Add(typeof(FA.FASetup.assetNumberingID), fasetup);
					setupNumberingSequences.Add(typeof(FA.FASetup.batchNumberingID), fasetup);
					setupNumberingSequences.Add(typeof(FA.FASetup.tagNumberingID), fasetup);
				}
				if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
				{
					PXSelect<CM.CMSetup> cmsetup = new PXSelect<CM.CMSetup>(graph);
					setupNumberingSequences.Add(typeof(CM.CMSetup.batchNumberingID), cmsetup);
					setupNumberingSequences.Add(typeof(CM.CMSetup.translNumberingID), cmsetup);
					setupNumberingSequences.Add(typeof(CM.CMSetup.extRefNbrNumberingID), cmsetup);
				}
			}

			if (areas.HasFlag(ApplicationAreas.Distribution) && PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
			{
				PXSelect<PO.POSetup> posetup = new PXSelect<PO.POSetup>(graph);
				setupNumberingSequences.Add(typeof(PO.POSetup.standardPONumberingID), posetup);
				setupNumberingSequences.Add(typeof(PO.POSetup.regularPONumberingID), posetup);

				if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				{
					setupNumberingSequences.Add(typeof(PO.POSetup.receiptNumberingID), posetup);

					PXSelect<IN.INSetup> insetup = new PXSelect<IN.INSetup>(graph);
					setupNumberingSequences.Add(typeof(IN.INSetup.batchNumberingID), insetup);
					setupNumberingSequences.Add(typeof(IN.INSetup.receiptNumberingID), insetup);
					setupNumberingSequences.Add(typeof(IN.INSetup.issueNumberingID), insetup);
					setupNumberingSequences.Add(typeof(IN.INSetup.adjustmentNumberingID), insetup);
					setupNumberingSequences.Add(typeof(IN.INSetup.kitAssemblyNumberingID), insetup);
					setupNumberingSequences.Add(typeof(IN.INSetup.pINumberingID), insetup);
					setupNumberingSequences.Add(typeof(IN.INSetup.replenishmentNumberingID), insetup);

					PXSelect<SO.SOSetup> sosetup = new PXSelect<SO.SOSetup>(graph);
					setupNumberingSequences.Add(typeof(SO.SOSetup.shipmentNumberingID), sosetup);
				}

				var orderTypeInvoice = new PXSelect<SO.SOOrderType,
								Where<SO.SOOrderType.invoiceNumberingID, Equal<Required<Numbering.numberingID>>>>(graph);
				var orderTypeOrder = new PXSelect<SO.SOOrderType,
								Where<SO.SOOrderType.orderNumberingID, Equal<Required<Numbering.numberingID>>>>(graph);
				setupNumberingSequences.Add(typeof(SO.SOOrderType.invoiceNumberingID), orderTypeInvoice);
				setupNumberingSequences.Add(typeof(SO.SOOrderType.orderNumberingID), orderTypeOrder);

				PXSelect<RQ.RQSetup> rqsetup = new PXSelect<RQ.RQSetup>(graph);
				setupNumberingSequences.Add(typeof(RQ.RQSetup.requestNumberingID), rqsetup);
				setupNumberingSequences.Add(typeof(RQ.RQSetup.requisitionNumberingID), rqsetup);
			}

			if (areas.HasFlag(ApplicationAreas.Project) && PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				PXSelect<PM.PMSetup> pmsetup = new PXSelect<PM.PMSetup>(graph);
				setupNumberingSequences.Add(typeof(PM.PMSetup.tranNumbering), pmsetup);
				setupNumberingSequences.Add(typeof(PM.PMSetup.batchNumberingID), pmsetup);
			}

			if(areas.HasFlag(ApplicationAreas.TimeAndExpenses))
			{
				PXSelect<EP.EPSetup> epsetup = new PXSelect<EP.EPSetup>(graph);
				setupNumberingSequences.Add(typeof(EP.EPSetup.claimNumberingID), epsetup);
				setupNumberingSequences.Add(typeof(EP.EPSetup.receiptNumberingID), epsetup);
				setupNumberingSequences.Add(typeof(EP.EPSetup.timeCardNumberingID), epsetup);
				setupNumberingSequences.Add(typeof(EP.EPSetup.equipmentTimeCardNumberingID), epsetup);
			}
		}
		#region Methods
		/// <summary>
		/// Checks whether subsequences of two numbering sequences can intersect.
		/// </summary>
		/// <returns>If at least one subsequence of the first numbering sequence can intersect 
		/// with a subsequence of the second numbering sequence, the method returns <tt>True</tt>; 
		/// otherwise, the method returns <tt>False</tt>.</returns>
		public static bool CanNumberingIntersect(string firstNumberingID, string secondNumberingID, PXGraph graph)
		{
			foreach (CS.NumberingSequence firstSequence in PXSelect<CS.NumberingSequence, Where<CS.NumberingSequence.numberingID, Equal<Required<CS.Numbering.numberingID>>>>.Select(graph, firstNumberingID))
			{
				foreach (CS.NumberingSequence secondSequence in PXSelect<CS.NumberingSequence, Where<CS.NumberingSequence.numberingID, Equal<Required<CS.Numbering.numberingID>>>>.Select(graph, secondNumberingID))
				{
					if (CanSequencesIntersect(firstSequence, secondSequence))
					{
						return true;
					}
				}
			}
			return false;
		}
	
		/// <summary>
		/// Checks whether the specified numbering subsequences can intersect.
		/// </summary>
		/// <returns>If the specified numbering subsequences can intersect, the method returns <tt>True</tt>; 
		/// otherwise, the method returns <tt>False</tt>.</returns>
		protected static bool CanSequencesIntersect(NumberingSequence firstSequence, NumberingSequence secondSequence)
		{
			string firstStart = null;
			string firstMask = MakeMask(firstSequence.StartNbr, ref firstStart);
			string secondStart = null;
			string secondMask = MakeMask(secondSequence.StartNbr, ref secondStart);
			if (firstMask != secondMask)
			{
				return false;
			}

			string firstEnd = null;
			MakeMask(firstSequence.EndNbr, ref firstEnd);
			string secondEnd = null;
			MakeMask(secondSequence.EndNbr, ref secondEnd);
			long firstStartNbr = long.Parse(firstStart);
			long firstEndNbr = long.Parse(firstEnd);
			long secondStartNbr = long.Parse(secondStart);
			long secondEndNbr = long.Parse(secondEnd);
			if ((firstStartNbr >= secondStartNbr && firstStartNbr <= secondEndNbr) || (secondStartNbr >= firstStartNbr && secondStartNbr <= firstEndNbr))
			{
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Extracts a mask from the specified string.
		/// </summary>
		/// <param name="str">An input string.</param>
		/// <param name="nbr">A number to be extracted from the input string.</param>
		/// <returns>The method returns a string with the extracted mask, such as <c>abc999</c>.</returns>
		public static string MakeMask(string str, ref string nbr)
		{
			int i;
			bool j = true;

			StringBuilder bld = new StringBuilder();
			StringBuilder bldNbr = new StringBuilder();
			for (i = str.Length; i > 0; i--)
			{
				if (Regex.IsMatch(str.Substring(i - 1, 1), "[^0-9]"))
				{
					j = false;
				}

				if (j)
				{
					bld.Append(Regex.Replace(str.Substring(i - 1, 1), "[0-9]", "9"));
					bldNbr.Append(str.Substring(i - 1, 1));
				}
				else
				{
					bld.Append(str[i - 1]);
				}
			}

			char[] dig = bldNbr.ToString().ToCharArray();
			Array.Reverse(dig);
			nbr = new string(dig);

			char[] c = bld.ToString().ToCharArray();
			Array.Reverse(c);
			return new string(c);
		}
		
		/// <summary>
		/// Searches for a reference to <paramref name="numberingID"/> in Setup tables.
		/// </summary>
		/// <param name="numberingID">The identifier of the numbering sequence to search for.</param>
		/// <param name="cacheName">The name of the Setup table in which the reference has been found.</param>
		/// <param name="fieldName">The display name of the field that contains the reference.</param>
		/// <returns>If a Setup table with the reference to the specified <paramref name="numberingID"/> has 
		/// been found, the method returns <tt>True</tt>; otherwise, the method returns <tt>False</tt>.</returns>
		public bool IsInUseSetups(string numberingID, out string cacheName, out string fieldName)
		{
			return IsInUseCustom(numberingID, out cacheName, out fieldName, setupNumberingSequences);
		}
		
		/// <summary>
		/// Searches for a reference to <paramref name="numberingID"/> in workbooks.
		/// </summary>
		/// <param name="numberingID">The identifier of the numbering sequence to search for.</param>
		/// <param name="wrokBookID">The ID of the workbook in which the reference has been found.</param>
		/// <param name="ignoreWorkBookID">The ID of the workbook that should be ignored during search.</param>
		/// <returns>If a workbook with the reference to the specified <paramref name="numberingID"/> has 
		/// been found, the method returns <tt>True</tt>; otherwise, the method returns <tt>False</tt>.</returns>
		public bool IsInUseWorkbooks(string numberingID, out string wrokBookID, string ignoreWorkBookID = null)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>())
			{
				GL.GLWorkBook wb = PXSelect<GL.GLWorkBook,
							Where2<Where<GL.GLWorkBook.voucherNumberingID, Equal<Required<Numbering.numberingID>>,
								Or<GL.GLWorkBook.voucherBatchNumberingID, Equal<Required<Numbering.numberingID>>>>,
							And<GL.GLWorkBook.workBookID, NotEqual<Required<GL.GLWorkBook.workBookID>>>>>.Select(parentGraph, numberingID, numberingID, ignoreWorkBookID);
				if (wb != null && wb.WorkBookID != null)
				{
					wrokBookID = wb.WorkBookID;
					return true;
				}
			}
			wrokBookID = null;
			return false;
		}
		
		/// <summary>
		/// Searches for a reference to <paramref name="numberingID"/> in segmented keys.
		/// </summary>
		/// <param name="numberingID">The identifier of the numbering sequence to search for.</param>
		/// <param name="demensionID">The ID of the segmented key in which the reference has been found.</param>
		/// <param name="segmentID">The ID of the segment in which the reference has been found.</param>
		/// <returns>If a segmented key with the reference to the specified <paramref name="numberingID"/> has 
		/// been found, the method returns <tt>True</tt>; otherwise, the method returns <tt>False</tt>.</returns>
		public bool IsInUseSegments(string numberingID, out string demensionID, out string segmentID)
		{
			foreach (PXResult<Dimension, Segment> r in PXSelectJoin<Dimension,
							InnerJoin<Segment, On<Segment.dimensionID, Equal<Dimension.dimensionID>>>,
							Where<Dimension.numberingID, Equal<Optional<Numbering.numberingID>>,
							And<Segment.autoNumber, Equal<Optional<Segment.autoNumber>>>>>.Select(parentGraph, numberingID, true))
			{
				Dimension dim = r;
				Segment segrow = r;
				demensionID = segrow.DimensionID;
				segmentID = segrow.SegmentID.ToString();
				return true;
			}
			demensionID = null;
			segmentID = null;
			return false;
		}
		
		/// <summary>
		/// Searches for reference to <paramref name="numberingID"/> in the fields, which are specified in <paramref name="sequences"/>.
		/// </summary>
		/// <param name="numberingID">The identifier of the numbering sequence to search for.</param>
		/// <param name="cacheName">The name of the cache in which the reference has been found.</param>
		/// <param name="fieldName">The display name of the field that contains the reference.</param>
		/// <param name="sequences">The dictionary that contains the field type and PXSelectBase, 
		/// which is used to select a field of the specified type.</param>
		/// <returns>If a field with the reference to the specified <paramref name="numberingID"/> has been found, the method 
		/// returns <tt>True</tt>; otherwise, the method returns <tt>False</tt>.</returns>
		private bool IsInUseCustom(string numberingID, out string cacheName, out string fieldName, Dictionary<Type, PXSelectBase> sequences)
		{
			foreach (KeyValuePair<Type, PXSelectBase> pair in sequences)
			{
				PXSelectBase select = pair.Value;
				Type fieldType = pair.Key;
				object result = select.View.SelectSingle(numberingID);
				if (result != null)
				{
					if ((string)select.Cache.GetValue(result, fieldType.Name) == numberingID)
					{
						cacheName = select.Cache.DisplayName;
						fieldName = PXUIFieldAttribute.GetDisplayName(select.Cache, fieldType.Name);
						return true;
					}
				}
			}
			cacheName = null;
			fieldName = null;
			return false;
		}
		#endregion
	}
}
