using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.Objects.TX.Descriptor;
using PX.Objects.Common;

namespace PX.Objects.TX
{
	[Serializable]
	[PXCacheName(Messages.VendorMaster)]
	public partial class VendorMaster : Vendor
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[TaxAgencyActive(IsKey = true)]
		public override Int32? BAccountID { get; set; }
		#endregion
		
		#region TaxAgency
		public new abstract class taxAgency : PX.Data.BQL.BqlBool.Field<taxAgency>
		{
		}
		#endregion
		
		#region ShowNoTemp
		public abstract class showNoTemp : PX.Data.BQL.BqlBool.Field<showNoTemp> { }

		protected bool? _ShowNoTemp = false;

		[PXBool]
		[PXUIField(DisplayName = "Show Tax Zones")]
		public virtual bool? ShowNoTemp
		{
			get
			{
				return this._ShowNoTemp;
			}
			set
			{
				this._ShowNoTemp = value;
			}
		}
		#endregion

		#region LineCntr
		public abstract class lineCntr : IBqlField { }

		[PXInt]
		[PXDefault(0)]
		[PXDBScalar(typeof(Search4<TaxReportLine.lineNbr,
			Where<TaxReportLine.vendorID, Equal<BAccount.bAccountID>>,
			Aggregate<Max<TaxReportLine.lineNbr>>>))]
		public virtual int? LineCntr
		{
			get;
			set;
		}
		#endregion
	}

	public class TaxReportMaint : PXGraph<TaxReportMaint>
	{	
		public const string TAG_TAXZONE = "<TAXZONE>";
		private readonly TaxReportLinesByTaxZonesReloader taxDetailsReloader;

		#region Cache Attached Events
		#region TaxBucket
		#region VendorID

		[PXDBInt(IsKey = true)]
        [PXDefault(typeof(VendorMaster.bAccountID))]        
        protected virtual void TaxBucket_VendorID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region BucketID

        [PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(VendorMaster))]
		[PXParent(typeof(Select<VendorMaster, Where<VendorMaster.bAccountID, Equal<Current<TaxBucket.vendorID>>>>), LeaveChildren = true)]
        [PXUIField(DisplayName = "Reporting Group", Visibility = PXUIVisibility.Visible)]
        protected virtual void TaxBucket_BucketID_CacheAttached(PXCache sender)
        {
        }
        #endregion       
        
        #endregion

		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(VendorMaster.bAccountID))]
		protected virtual void TaxReportLine_VendorID_CacheAttached(PXCache sender)
		{
		}

        #endregion

        public PXSave<VendorMaster> Save;
		public PXCancel<VendorMaster> Cancel;

		public PXAction<VendorMaster> Up;

		[PXUIField(DisplayName = ActionsMessages.RowUp, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp, Tooltip = ActionsMessages.ttipRowUp)]
		public virtual IEnumerable up(PXAdapter adapter)
		{
			ReportLine.ArrowUpForCurrentRow();
			return adapter.Get();
		}

		public PXAction<VendorMaster> Down;


		[PXUIField(DisplayName = ActionsMessages.RowDown, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown, Tooltip = ActionsMessages.ttipRowDown)]		
		public virtual IEnumerable down(PXAdapter adapter)
		{
			ReportLine.ArrowDownForCurrentRow();
			return adapter.Get();
		}

		public PXSelect<VendorMaster, Where<VendorMaster.taxAgency, Equal<boolTrue>>> TaxVendor;

		public TaxReportLinesOrderedSelect ReportLine;

		public PXSelect<TaxBucket, Where<TaxBucket.vendorID, Equal<Current<VendorMaster.bAccountID>>>> Bucket;

		public PXSelect<TaxBucketLine, 
						Where<TaxBucketLine.vendorID, Equal<Required<TaxReportLine.vendorID>>, 
								And<TaxBucketLine.lineNbr, Equal<Required<TaxBucketLine.lineNbr>>>>> 
						TaxBucketLine_Vendor_LineNbr;

        protected IEnumerable reportLine()
        {
			if (TaxVendor.Current.BAccountID == null)
				yield break;

            bool showTaxZones = TaxVendor.Current.ShowNoTemp == true;
			TaxBucketAnalizer analyzerTax = new TaxBucketAnalizer(this, TaxVendor.Current.BAccountID.Value, TaxReportLineType.TaxAmount);
			Dictionary<int, List<int>> taxBucketsDict = analyzerTax.AnalyzeBuckets(showTaxZones);
			TaxBucketAnalizer testAnalyzerTaxable = new TaxBucketAnalizer(this, (int)this.TaxVendor.Current.BAccountID, TaxReportLineType.TaxableAmount);
			Dictionary<int, List<int>> taxableBucketsDict = testAnalyzerTaxable.AnalyzeBuckets(showTaxZones);
            Dictionary<int, List<int>>[] bucketsArr = { taxBucketsDict, taxableBucketsDict };     

			Dictionary<int, TaxReportLine> taxReporLinesByLineNumber =
				PXSelect<TaxReportLine,
					Where<TaxReportLine.vendorID, Equal<Current<VendorMaster.bAccountID>>,
						And<
							Where2<
								Where<Current<VendorMaster.showNoTemp>, Equal<False>,
								  And<TaxReportLine.tempLineNbr, IsNull>>,
								Or<
									Where<Current<VendorMaster.showNoTemp>, Equal<True>, 
									And<
										Where<TaxReportLine.tempLineNbr, IsNull,
										And<TaxReportLine.tempLine, Equal<False>,
										Or<TaxReportLine.tempLineNbr, IsNotNull>>>>>>>>>,
					OrderBy<
						Asc<TaxReportLine.sortOrder,
						Asc<TaxReportLine.taxZoneID>>>>
					.Select(this)
					.RowCast<TaxReportLine>()
					.ToDictionary(taxLine => taxLine.LineNbr.Value);

			foreach (TaxReportLine taxline in taxReporLinesByLineNumber.Values)
            {
				if (!showTaxZones)
				{
					foreach (Dictionary<int, List<int>> bucketsDict in bucketsArr.Where(dict => dict?.ContainsKey(taxline.LineNbr.Value) == true))
					{
						var calcRuleWithLineNumberReplacedBySortOrder =
							bucketsDict[taxline.LineNbr.Value].Where(lineNbr => taxReporLinesByLineNumber.ContainsKey(lineNbr))
															  .Select(lineNbr => taxReporLinesByLineNumber[lineNbr].SortOrder.Value)
															  .OrderBy(lineNbr => lineNbr);

						taxline.BucketSum = string.Join("+", calcRuleWithLineNumberReplacedBySortOrder);
					}
				}

                yield return taxline;
            }
        }
		
		protected virtual void VendorMaster_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		public PXAction<VendorMaster> viewGroupDetails;

		[PXUIField(DisplayName = Messages.ViewGroupDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewGroupDetails(PXAdapter adapter)
		{
			if (TaxVendor.Current != null && Bucket.Current != null)
			{
				TaxBucketMaint graph = CreateInstance<TaxBucketMaint>();
				graph.Bucket.Current.VendorID = TaxVendor.Current.BAccountID;
				graph.Bucket.Current.BucketID = Bucket.Current.BucketID;
				graph.Bucket.Current.BucketType = Bucket.Current.BucketType;

				throw new PXRedirectRequiredException(graph, Messages.ViewGroupDetails);
			}

			return adapter.Get();
		}

		public PXAction<VendorMaster> updateTaxZoneLines;

		[PXUIField(DisplayName = Messages.CreateReportLinesForNewTaxZones, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable UpdateTaxZoneLines(PXAdapter adapter)
		{
			taxDetailsReloader.ReloadTaxReportLinesForTaxZones();
			return adapter.Get();
		}

		public class TaxBucketAnalizer
		{
			private Dictionary<int, List<int>> _bucketsLinesAggregates;
			private Dictionary<int, List<int>> _bucketsLinesAggregatesSorted;
			private Dictionary<int, List<int>> _bucketsDict;
			private Dictionary<int, int> _bucketLinesOccurence;
			private Dictionary<int, Dictionary<int, int>> _bucketsLinesPairs;
			private int _bAccountID;
			private string _taxLineType;
			private PXGraph _graph;

			private PXSelectJoin<TaxBucketLine, 
				LeftJoin<TaxReportLine,
					On<TaxBucketLine.lineNbr, Equal<TaxReportLine.lineNbr>, 
					And<TaxBucketLine.vendorID, Equal<TaxReportLine.vendorID>>>>,
				Where<TaxBucketLine.vendorID, Equal<Required<TaxBucketLine.vendorID>>, 
					And<TaxReportLine.lineType, Equal<Required<TaxReportLine.lineType>>>>> _vendorBucketLines;

			public Func<TaxReportLine, bool> showTaxReportLine
			{
				get;
                set;
			}

			private IEnumerable<PXResult<TaxBucketLine>> selectBucketLines(int VendorId, string LineType)
			{
				return _vendorBucketLines.Select(VendorId, LineType).AsEnumerable()
										 .Where(set => showTaxReportLine(set.GetItem<TaxReportLine>()));
			}

			public TaxBucketAnalizer(PXGraph graph, int BAccountID, string TaxLineType)
			{
				_bAccountID = BAccountID;
				_taxLineType = TaxLineType;
				_graph = graph;
				_vendorBucketLines = 
                    new PXSelectJoin<TaxBucketLine, 
                            LeftJoin<TaxReportLine,
                                On<TaxBucketLine.lineNbr, Equal<TaxReportLine.lineNbr>, 
                                And<TaxBucketLine.vendorID, Equal<TaxReportLine.vendorID>>>>,
					    Where<TaxBucketLine.vendorID, Equal<Required<TaxBucketLine.vendorID>>, 
                          And<TaxReportLine.lineType, Equal<Required<TaxReportLine.lineType>>>>>(_graph);

				showTaxReportLine = line => true;
			}

			public Dictionary<int, List<int>> AnalyzeBuckets(bool CalcWithZones)
			{
				calcOccurances(CalcWithZones);
				fillAgregates();
				return _bucketsLinesAggregatesSorted;
			}

			public void DoChecks(int BucketID)
			{
				if (_bucketsDict == null)
				{
					calcOccurances(true);
					fillAgregates();
				}

				doChecks(BucketID);
			}

			#region Public Static functions

			public static void CheckTaxAgencySettings(PXGraph graph, int BAccountID)
			{
				PXResultset<TaxBucket> buckets = 
                    PXSelect<TaxBucket, 
                        Where<TaxBucket.vendorID, Equal<Required<TaxBucket.vendorID>>>>
                    .Select(graph, BAccountID);

				if (buckets == null)
                    return;

				TaxBucketAnalizer taxAnalizer = new TaxBucketAnalizer(graph, BAccountID, TaxReportLineType.TaxAmount);
				TaxBucketAnalizer taxableAnalizer = new TaxBucketAnalizer(graph, BAccountID, TaxReportLineType.TaxableAmount);

				foreach (TaxBucket bucket in buckets)
				{
                    int bucketID = bucket.BucketID.Value;
                    taxAnalizer.DoChecks(bucketID);
					taxableAnalizer.DoChecks(bucketID);
				}
			}

			[Obsolete("Will be removed in future versions of Acumatica")]
			public static Dictionary<int, int> TransposeDictionary(Dictionary<int, List<int>> oldDict)
			{
				if (oldDict == null)
				{
					return null;
				}

				Dictionary<int, int> newDict = new Dictionary<int, int>(capacity: oldDict.Count);

				foreach (KeyValuePair<int, List<int>> kvp in oldDict)
				{
					foreach (int val in kvp.Value)
					{
						newDict[val] = kvp.Key;
					}
				}

				return newDict;
			}

			public static bool IsSubList(List<int> searchList, List<int> subList)
			{
				if (subList.Count > searchList.Count)
				{
					return false;
				}

				for (int i = 0; i < subList.Count; i++)
				{
					if (!searchList.Contains(subList[i]))
					{
						return false;
					}
				}

				return true;
			}

			public static List<int> SubstList(List<int> searchList, List<int> substList, int substVal)
			{
				if (!IsSubList(searchList, substList))
				{
					return searchList;
				}

				List<int> resList = searchList.ToList();
                substList.ForEach(val => resList.Remove(val));
				resList.Add(substVal);
				return resList;
			}

			#endregion

			#region Private Methods
			private void calcOccurances(bool CalcWithZones)
			{
				if (!CalcWithZones)
				{
					_vendorBucketLines.WhereAnd<Where<TaxReportLine.tempLineNbr, IsNull>>();
				}

				IEnumerable<PXResult<TaxBucketLine>> BucketLineTaxAmt = selectBucketLines(_bAccountID, _taxLineType);

				if (BucketLineTaxAmt == null)
				{
					_bucketsDict = null;
					return;
				}

				_bucketsDict = new Dictionary<int, List<int>>();

				foreach (PXResult<TaxBucketLine> bucketLineSet in BucketLineTaxAmt)
				{
					TaxBucketLine bucketLine = (TaxBucketLine) bucketLineSet[typeof (TaxBucketLine)];
					TaxReportLine reportLine = (TaxReportLine) bucketLineSet[typeof (TaxReportLine)];

					if (bucketLine.BucketID != null && reportLine.LineNbr != null)
					{
						if (!_bucketsDict.ContainsKey((int) bucketLine.BucketID))
						{
							_bucketsDict[(int) bucketLine.BucketID] = new List<int>();
						}

						_bucketsDict[(int) bucketLine.BucketID].Add((int) bucketLine.LineNbr);
					}
				}

				List<int> bucketsList = _bucketsDict.Keys.ToList();

				for (int i = 0; i < bucketsList.Count; i++)
				{
					for (int j = i + 1; j < bucketsList.Count; j++)
					{
						if (_bucketsDict[bucketsList[i]].Count == _bucketsDict[bucketsList[j]].Count
						    && IsSubList(_bucketsDict[bucketsList[i]], _bucketsDict[bucketsList[j]]))
						{
							_bucketsDict.Remove(bucketsList[i]);
							break;
						}
					}
				}

				_bucketLinesOccurence = new Dictionary<int, int>();
				_bucketsLinesPairs = new Dictionary<int, Dictionary<int, int>>();

				foreach (KeyValuePair<int, List<int>> kvp in _bucketsDict)
				{
					foreach (int lineNbr in kvp.Value)
					{
						if (!_bucketLinesOccurence.ContainsKey(lineNbr))
						{
							_bucketLinesOccurence[lineNbr] = 0;
						}
						_bucketLinesOccurence[lineNbr]++;
					}

					for (int i = 0; i < kvp.Value.Count - 1; i++)
					{
						for (int j = i + 1; j < kvp.Value.Count; j++)
						{
							int key;
							int value;

							if (kvp.Value[i] < kvp.Value[j])
							{
								key = kvp.Value[i];
								value = kvp.Value[j];
							}
							else
							{
								key = kvp.Value[j];
								value = kvp.Value[i];
							}

							if (!_bucketsLinesPairs.ContainsKey(key))
							{
								_bucketsLinesPairs[key] = new Dictionary<int, int>();
							}

							if (!_bucketsLinesPairs[key].ContainsKey(value))
							{
								_bucketsLinesPairs[key][value] = 0;
							}

							_bucketsLinesPairs[key][value]++;
						}
					}
				}
			}

			private void fillAgregates()
			{
				if (_bucketsDict == null || _bucketLinesOccurence == null || _bucketsLinesPairs == null)
					return;

				_bucketsLinesAggregates = new Dictionary<int, List<int>>();

				foreach (KeyValuePair<int, Dictionary<int, int>> kvp in _bucketsLinesPairs)
				{
					foreach (KeyValuePair<int, int> innerkvp in kvp.Value)
					{
						if (innerkvp.Value == 1)
						{
							int keyOccurence = _bucketLinesOccurence[kvp.Key];
							int valOccurence = _bucketLinesOccurence[innerkvp.Key];
							int aggregate = 0;
							int standAloneVal = 0;

							if (keyOccurence != valOccurence)
							{
								if (keyOccurence > valOccurence)
								{
									aggregate = kvp.Key;
									standAloneVal = innerkvp.Key;
								}
								else
								{
									aggregate = innerkvp.Key;
									standAloneVal = kvp.Key;
								}
							}

							if (aggregate != 0)
							{
								if (!_bucketsLinesAggregates.ContainsKey(aggregate))
								{
									_bucketsLinesAggregates[aggregate] = new List<int>();
								}

								_bucketsLinesAggregates[aggregate].Add(standAloneVal);
							}
						}
					}
				}

				List<KeyValuePair<int, List<int>>> sortedAggregatesList = _bucketsLinesAggregates.ToList();
				sortedAggregatesList.Sort((firstPair, nextPair) => firstPair.Value.Count - nextPair.Value.Count == 0 ?
					                                                   firstPair.Key - nextPair.Key : firstPair.Value.Count - nextPair.Value.Count);
				for (int i = 0; i < sortedAggregatesList.Count; i++)
				{
					for (int j = i + 1; j < sortedAggregatesList.Count; j++)
					{
						List<int> newList = SubstList(sortedAggregatesList[j].Value, sortedAggregatesList[i].Value, sortedAggregatesList[i].Key);

						if (newList != sortedAggregatesList[j].Value)
						{
							sortedAggregatesList[j].Value.Clear();
							sortedAggregatesList[j].Value.AddRange(newList);
						}
					}
				}

				_bucketsLinesAggregatesSorted = new Dictionary<int, List<int>>();

				foreach (KeyValuePair<int, List<int>> kvp in sortedAggregatesList)
				{
					kvp.Value.Sort();
					_bucketsLinesAggregatesSorted[kvp.Key] = kvp.Value;
				}
			}

			private void doChecks(int BucketID)
			{
				if (_bucketsLinesAggregatesSorted == null)
				{
					throw new PXException(Messages.UnexpectedCall);
				}

				int standaloneLinesCount = 0;
				int aggrergateLinesCount = 0;

				if (_bucketsDict.ContainsKey(BucketID))
				{
					foreach (int line in _bucketsDict[BucketID])
					{
						//can only be 1 or more
						if (_bucketsLinesAggregatesSorted.ContainsKey(line))
						{
							aggrergateLinesCount++;
						}
						else
						{
							standaloneLinesCount++;
						}
					}

					if (aggrergateLinesCount > 0 && standaloneLinesCount == 0)
					{
						throw new PXSetPropertyException(Messages.BucketContainsOnlyAggregateLines, PXErrorLevel.Error, BucketID.ToString());
					}
				}
			}

			#endregion
		}

		public static Dictionary<int, List<int>> AnalyseBuckets(PXGraph graph, int BAccountID, string TaxLineType, bool CalcWithZones, Func<TaxReportLine, bool> ShowTaxReportLine = null)
        {
			TaxBucketAnalizer analizer = new TaxBucketAnalizer(graph, BAccountID, TaxLineType);

			if (ShowTaxReportLine != null)
			{
				analizer.showTaxReportLine = ShowTaxReportLine;
			}

			return analizer.AnalyzeBuckets(CalcWithZones);
        }
  
        private void UpdateNet(object row)
		{
			bool refreshNeeded = false;
			TaxReportLine currow = row as TaxReportLine;

			if (currow.NetTax.Value && currow.TempLineNbr == null)
			{
				foreach (TaxReportLine reportrow in PXSelect<TaxReportLine, Where<TaxReportLine.vendorID, Equal<Required<VendorMaster.bAccountID>>>>.Select(this,currow.VendorID))
				{
					if (reportrow.NetTax.Value && reportrow.LineNbr != currow.LineNbr && reportrow.TempLineNbr != currow.LineNbr)
					{
						reportrow.NetTax = false;
						ReportLine.Cache.Update(reportrow);
						refreshNeeded = true;
					}
				}
			}

			if (refreshNeeded)
			{
				ReportLine.View.RequestRefresh();
			}
		}

		public TaxReportLine CreateChildLine(TaxReportLine template, TaxZone zone)
		{
			TaxReportLine child = PXCache<TaxReportLine>.CreateCopy(template);

			child.TempLineNbr = child.LineNbr;
			child.TaxZoneID = zone.TaxZoneID;
			child.LineNbr = null;
			child.TempLine = false;
			child.ReportLineNbr = null;
			child.SortOrder = template.SortOrder;

			if (!string.IsNullOrEmpty(child.Descr))
			{
				int fid = child.Descr.IndexOf(TAG_TAXZONE, StringComparison.OrdinalIgnoreCase);

				if (fid >= 0)
				{
					child.Descr = child.Descr.Remove(fid, TAG_TAXZONE.Length).Insert(fid, child.TaxZoneID);
				}
			}

			return child;
		}

		private void UpdateZones(PXCache sender, TaxReportLine oldRow, TaxReportLine newRow)
		{			
			if (oldRow != null && (newRow == null || newRow.TempLine == false))
			{
				if (!string.IsNullOrEmpty(newRow?.Descr))
				{
					int fid = newRow.Descr.IndexOf(TAG_TAXZONE, StringComparison.OrdinalIgnoreCase);

					if (fid >= 0)
					{
						newRow.Descr = newRow.Descr.Remove(fid, TAG_TAXZONE.Length).TrimEnd(' ');
					}
				}

				DeleteChildTaxLinesForMainTaxLine(oldRow);
			}

			if (newRow?.TempLine == true && newRow.TempLine != oldRow?.TempLine)
			{
				newRow.TaxZoneID = null;

				if (string.IsNullOrEmpty(newRow.Descr) || newRow.Descr.IndexOf(TAG_TAXZONE, StringComparison.OrdinalIgnoreCase) < 0)
				{
					newRow.Descr += ' ' + TAG_TAXZONE;
				}

				foreach (TaxZone zone in PXSelect<TaxZone>.Select(this))
				{
					TaxReportLine child = CreateChildLine(newRow, zone);
					sender.Insert(child);
				}
			}

			if (newRow?.TempLine == true && oldRow?.TempLine == true)
			{
				UpdateTaxLineOnFieldUpdatedWhenDetailByTaxZoneNotChanged(sender, oldRow, newRow);
			}
		}

		private void DeleteChildTaxLinesForMainTaxLine(TaxReportLine mainLine)
		{
			var childTaxLines =
				PXSelect<TaxReportLine,
					Where<TaxReportLine.vendorID, Equal<Required<TaxReportLine.vendorID>>,
						And<TaxReportLine.tempLineNbr, Equal<Required<TaxReportLine.tempLineNbr>>>>>
					.Select(this, mainLine.VendorID, mainLine.LineNbr);

			foreach (TaxReportLine child in childTaxLines)
			{
				ReportLine.Cache.Delete(child);
			}
		}

		private void UpdateTaxLineOnFieldUpdatedWhenDetailByTaxZoneNotChanged(PXCache sender, TaxReportLine oldRow, TaxReportLine newRow)
		{
			var childTaxLines =
					PXSelect<TaxReportLine,
						Where<TaxReportLine.vendorID, Equal<Required<TaxReportLine.vendorID>>,
							And<TaxReportLine.tempLineNbr, Equal<Required<TaxReportLine.tempLineNbr>>>>>
					.Select(this, oldRow.VendorID, oldRow.LineNbr);

			foreach (TaxReportLine child in childTaxLines)
			{
				child.Descr = newRow.Descr;

				if (!string.IsNullOrEmpty(child.Descr))
				{
					int fid = child.Descr.IndexOf(TAG_TAXZONE, StringComparison.OrdinalIgnoreCase);

					if (fid >= 0)
					{
						child.Descr = child.Descr.Remove(fid, TAG_TAXZONE.Length)
												 .Insert(fid, child.TaxZoneID);
					}
				}

				child.NetTax = newRow.NetTax;
				child.LineType = newRow.LineType;
				child.LineMult = newRow.LineMult;
				child.SortOrder = newRow.SortOrder;
				sender.Update(child);
			}
		}

		protected virtual void VendorMaster_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			var vendor = (VendorMaster)e.Row;
			bool showWithoutTaxZones = vendor.ShowNoTemp.Value == false;

			PXUIFieldAttribute.SetVisible<TaxReportLine.tempLine>(ReportLine.Cache, null, showWithoutTaxZones);
			PXUIFieldAttribute.SetVisible<TaxReportLine.bucketSum>(ReportLine.Cache, null, showWithoutTaxZones);

			PXUIFieldAttribute.SetEnabled<TaxReportLine.tempLine>(ReportLine.Cache, null, showWithoutTaxZones);
			PXUIFieldAttribute.SetEnabled<TaxReportLine.netTax>(ReportLine.Cache, null, showWithoutTaxZones);
			PXUIFieldAttribute.SetEnabled<TaxReportLine.taxZoneID>(ReportLine.Cache, null, showWithoutTaxZones);
			PXUIFieldAttribute.SetEnabled<TaxReportLine.lineType>(ReportLine.Cache, null, showWithoutTaxZones);
			PXUIFieldAttribute.SetEnabled<TaxReportLine.lineMult>(ReportLine.Cache, null, showWithoutTaxZones);
			PXUIFieldAttribute.SetEnabled<TaxReportLine.sortOrder>(ReportLine.Cache, null, showWithoutTaxZones);

			ReportLine.AllowDragDrop = showWithoutTaxZones;
			ReportLine.AllowInsert = showWithoutTaxZones;			

			Up.SetEnabled(showWithoutTaxZones);
			Down.SetEnabled(showWithoutTaxZones);

			CheckReportSettingsEditableAndSetWarningTo<VendorMaster.bAccountID>(this, sender, vendor, vendor.BAccountID);
		}

		protected virtual void VendorMaster_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			VendorMaster vendorMaster = e.Row as VendorMaster;
			if(vendorMaster == null)
			{
				return;
			}

			if (vendorMaster.ShowNoTemp == null)
			{
				vendorMaster.ShowNoTemp = false;
			}

			if (vendorMaster.LineCntr == null)
			{
				vendorMaster.LineCntr = 0;
			}
		}

		protected virtual void TaxReportLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			TaxReportLine line = e.Row as TaxReportLine;

			if (line == null)
				return;

			PXUIFieldAttribute.SetEnabled<TaxReportLine.reportLineNbr>(sender, line, line.HideReportLine != true);
		}
		
		protected virtual void TaxReportLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			UpdateNet(e.Row);
			UpdateZones(sender, null, e.Row as TaxReportLine);
		}

		protected virtual void TaxReportLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			UpdateNet(e.Row);
			UpdateZones(sender, e.OldRow as TaxReportLine, e.Row as TaxReportLine);
		}

		protected virtual void TaxReportLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			UpdateZones(sender, e.Row as TaxReportLine, null);
		}

		protected virtual void TaxReportLine_HideReportLine_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxReportLine line = e.Row as TaxReportLine;

			if (line == null)
				return;

			if (line.HideReportLine == true)
			{
				sender.SetValueExt<TaxReportLine.reportLineNbr>(line, null);
			}
		}

		public override void Persist()
		{
			CheckAndWarnTaxBoxNumbers();
			CheckReportSettingsEditable(this, TaxVendor.Current.BAccountID);

			SyncReportLinesAndBucketLines();

			base.Persist();
		}

		private void SyncReportLinesAndBucketLines()
		{
			TaxBucketLine_Vendor_LineNbr.Cache.Clear();

			foreach (var reportLine in ReportLine.Cache.Inserted.RowCast<TaxReportLine>())
			{
				var parentBuckets = TaxBucketLine_Vendor_LineNbr.Select(reportLine.VendorID, reportLine.TempLineNbr).RowCast<TaxBucketLine>();

				foreach (var bucketLine in parentBuckets)
				{
					TaxBucketLine newBucketLine = PXCache<TaxBucketLine>.CreateCopy(bucketLine);
					newBucketLine.LineNbr = reportLine.LineNbr;
					TaxBucketLine_Vendor_LineNbr.Cache.Insert(newBucketLine);
				}
			}

			foreach (var reportLine in ReportLine.Cache.Deleted.RowCast<TaxReportLine>())
			{
				var bucketLinesToDelete = TaxBucketLine_Vendor_LineNbr.Select(reportLine.VendorID, reportLine.LineNbr);
				foreach (var bucketLine in bucketLinesToDelete)
				{
					TaxBucketLine_Vendor_LineNbr.Cache.Delete(bucketLine);
				}
			}
		}

		public static void CheckReportSettingsEditableAndSetWarningTo<TVendorIDField>(PXGraph graph, PXCache cache, object row, int? vendorID)
			where TVendorIDField : IBqlField
		{
			if (TaxYearMaint.PrepearedTaxPeriodForVendorExists(graph, vendorID))
			{
				var bAccIDfieldState = (PXFieldState)cache.GetStateExt<TVendorIDField>(row);

				cache.RaiseExceptionHandling<TVendorIDField>(row, bAccIDfieldState.Value,
					new PXSetPropertyException(Messages.TheTaxReportSettingsCannotBeModified, PXErrorLevel.Warning));
			}
		}

		public static void CheckReportSettingsEditable(PXGraph graph, int? vendorID)
		{
			if (TaxYearMaint.PrepearedTaxPeriodForVendorExists(graph, vendorID))
				throw new PXException(Messages.TheTaxReportSettingsCannotBeModified);
		}

        private void CheckAndWarnTaxBoxNumbers()
        {
            HashSet<String> taxboxNumbers = new HashSet<String>();
			var taxReportLines = ReportLine.Select().RowCast<TaxReportLine>()
													.Where(line => line.ReportLineNbr != null);

			foreach (TaxReportLine line in taxReportLines)
            {
                if (ReportLine.Cache.GetStatus(line) == PXEntryStatus.Notchanged)
                {
                    taxboxNumbers.Add(line.ReportLineNbr);
                }
            }

			CheckTaxBoxNumberUniqueness(ReportLine.Cache.Inserted, taxboxNumbers);
			CheckTaxBoxNumberUniqueness(ReportLine.Cache.Updated, taxboxNumbers);
        }

        public virtual void CheckTaxBoxNumberUniqueness(IEnumerable toBeChecked, HashSet<String> taxboxNumbers)
        {
			var lineWithErrors = toBeChecked.OfType<TaxReportLine>()
											.Where(line => line.ReportLineNbr != null && !taxboxNumbers.Add(line.ReportLineNbr));

			foreach (TaxReportLine line in lineWithErrors)
			{
				ReportLine.Cache.RaiseExceptionHandling<TaxReportLine.reportLineNbr>(line, line.ReportLineNbr, 
					new PXSetPropertyException(Messages.TaxBoxNumbersMustBeUnique));
			}
        }

		protected virtual void TaxReportLine_SortOrder_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			VendorMaster taxVendor = TaxVendor.Current;
			TaxReportLine reportLine = (TaxReportLine)e.Row;
			int? newSortOrder = e.NewValue as int?;

			if (reportLine == null || reportLine.TempLineNbr != null || taxVendor == null || taxVendor.ShowNoTemp == true ||
				reportLine.SortOrder == newSortOrder)
			{
				return;
			}

			if (newSortOrder == null || newSortOrder <= 0)
			{
				string errorMsg = newSortOrder == null 
					? Common.Messages.MustHaveValue 
					: Common.Messages.ShouldBePositive;

				throw new PXSetPropertyException(errorMsg, nameof(TaxReportLine.SortOrder));
			}

			bool alreadyExists = ReportLine.Select()
										   .RowCast<TaxReportLine>()
										   .Any(line => line.SortOrder.Value == newSortOrder);

			if (alreadyExists)
			{
				throw new PXSetPropertyException(Messages.SortOrderNumbersMustBeUnique);
			}
		}

		protected virtual void TaxReportLine_SortOrder_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxReportLine reportLine = (TaxReportLine)e.Row;

			if (reportLine == null || reportLine.TempLineNbr != null || Equals(reportLine.SortOrder, e.OldValue))
				return;

			if (reportLine.TempLine == true)
			{
				var childTaxLines =
				   PXSelect<TaxReportLine,
					   Where<TaxReportLine.vendorID, Equal<Required<TaxReportLine.vendorID>>,
						   And<TaxReportLine.tempLineNbr, Equal<Required<TaxReportLine.tempLineNbr>>>>>
				   .Select(this, reportLine.VendorID, reportLine.LineNbr);

				foreach (TaxReportLine childLine in childTaxLines)
				{
					childLine.SortOrder = reportLine.SortOrder;
					ReportLine.Cache.SmartSetStatus(childLine, PXEntryStatus.Updated);
				}
			}

			ReportLine.View.RequestRefresh();
		}

		protected virtual void TaxReportLine_LineType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			TaxReportLine line = (TaxReportLine)e.Row;

			if (e.NewValue != null && line.NetTax != null && (bool)line.NetTax && (string)e.NewValue == "A")
			{
				throw new PXSetPropertyException(Messages.NetTaxMustBeTax, PXErrorLevel.RowError);
			}
		}

		protected virtual void TaxReportLine_NetTax_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			TaxReportLine line = (TaxReportLine)e.Row;

			if (e.NewValue != null && (bool)e.NewValue && line.LineType == "A")
			{
				throw new PXSetPropertyException(Messages.NetTaxMustBeTax, PXErrorLevel.RowError);
			}
		}

		public TaxReportMaint()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetVisible<TaxReportLine.lineNbr>(ReportLine.Cache, null, false);
			taxDetailsReloader = new TaxReportLinesByTaxZonesReloader(this);

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => 
			{
				if (e.Row != null)
					e.NewValue = BAccountType.VendorType;
			});
		}

		public PXSetup<APSetup> APSetup;
	}
}
