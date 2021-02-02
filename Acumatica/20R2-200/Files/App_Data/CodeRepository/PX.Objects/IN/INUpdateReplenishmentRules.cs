#define USE_OPEN_MODELS 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.AP;
using INItemSiteRS = PX.Objects.IN.INItemSite;


namespace PX.Objects.IN
{
		
	[PX.Objects.GL.TableAndChartDashboardType]  
    [Serializable]
	public class INUpdateReplenishmentRules : PXGraph<INUpdateReplenishmentRules>
	{
		[StockItem(IsKey = true, DisplayName = "Inventory ID", TabOrder = 1, CacheGlobal = true)]
		protected virtual void INItemSite_InventoryID_CacheAttached(PXCache sender) 
		{
		}


		#region Internal Type Definition
        [Serializable]
		public partial class Filter : IBqlTable
		{
			#region ForecastDate
			public abstract class forecastDate : PX.Data.BQL.BqlDateTime.Field<forecastDate> { }
			protected DateTime? _ForecastDate;
			[PXDBDate]
			[PXUIField(DisplayName = "Forecast Date")]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? ForecastDate
			{
				get
				{
					return this._ForecastDate;
				}
				set
				{
					this._ForecastDate = value;
				}
			}
			#endregion
			#region ItemClassCD
			public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
			protected string _ItemClassCD;

			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
			public virtual string ItemClassCD
			{
				get { return this._ItemClassCD; }
				set { this._ItemClassCD = value; }
			}
			#endregion
			#region ItemClassCDWildcard
			public abstract class itemClassCDWildcard : PX.Data.BQL.BqlString.Field<itemClassCDWildcard> { }
			[PXString(IsUnicode = true)]
			[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
			[PXDimension(INItemClass.Dimension, ParentSelect = typeof(Select<INItemClass>), ParentValueField = typeof(INItemClass.itemClassCD))]
			public virtual string ItemClassCDWildcard
				{
				get { return ItemClassTree.MakeWildcard(ItemClassCD); }
				set { }
			}
			#endregion
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			protected Int32? _SiteID;
			[IN.Site(DisplayName = "Warehouse")]
			public virtual Int32? SiteID
			{
				get
				{
					return this._SiteID;
				}
				set
				{
					this._SiteID = value;
				}
			}
			#endregion
			#region ReplenishmentPolicyID
			public abstract class replenishmentPolicyID : PX.Data.BQL.BqlString.Field<replenishmentPolicyID> { }
			protected String _ReplenishmentPolicyID;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Seasonality")]
			[PXSelector(typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>), DescriptionField = typeof(INReplenishmentPolicy.descr))]
			public virtual String ReplenishmentPolicyID
			{
				get
				{
					return this._ReplenishmentPolicyID;
				}
				set
				{
					this._ReplenishmentPolicyID = value;
				}
			}
			#endregion
			#region Action
			public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
			protected string _Action;
			[Actions.ActionsList]
			[PXDBString(10, IsFixed = false)]
			[PXUIField(DisplayName = "Action")]
			[PXDefault(Actions.Calculate)]
			public virtual string Action
			{
				get
				{
					return this._Action;
				}
				set
				{
					this._Action = value;
				}
			}
			#endregion

			public static class Actions
			{
				public const string Calculate = "Calc";
				public const string Clear = "Clear";

				public class ActionsListAttribute : PXStringListAttribute
				{
					public ActionsListAttribute() : base(
						new[]
						{
							Pair(Calculate, Messages.Calculate),
							Pair(Clear, Messages.Clear),
						}) {}
				}

			}
		}


		[PXProjection(typeof(Select<InventoryItem>), Persistent = false)]
        [Serializable]
        [PXHidden]
		public partial class InventoryItemRO : IBqlTable
		{
			#region Keys
			public class PK : PrimaryKeyOf<InventoryItemRO>.By<inventoryID>
			{
				public static InventoryItemRO Find(PXGraph graph, int? inventoryID) => FindBy(graph, inventoryID);
			}
			public static class FK
			{
				public class ItemClass : INItemClass.PK.ForeignKeyOf<InventoryItemRO>.By<itemClassID> { }
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[PXDBInt(BqlField = typeof(InventoryItem.inventoryID))]
			[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region InventoryCD
			public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }

			protected String _InventoryCD;

			[PXDBString(InputMask = "", IsUnicode = true, BqlField = typeof(InventoryItem.inventoryCD))]
			[PXUIField(DisplayName = "Inventory ID")]			
			public virtual String InventoryCD
			{
				get
				{
					return this._InventoryCD;
				}
				set
				{
					this._InventoryCD = value;
				}
			}
			#endregion
			#region ItemClassID
			public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
			protected int? _ItemClassID;
			[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
			[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]			
			public virtual int? ItemClassID
			{
				get
				{
					return this._ItemClassID;
				}
				set
				{
					this._ItemClassID = value;
				}
			}
			#endregion
			#region ItemStatus
			public abstract class itemStatus : PX.Data.BQL.BqlString.Field<itemStatus> { }
			protected String _ItemStatus;
			[PXDBString(2, IsFixed = true, BqlField = typeof(InventoryItem.itemStatus))]
			[PXUIField(DisplayName = "Item Status", Visibility = PXUIVisibility.SelectorVisible)]
			[InventoryItemStatus.List]
			public virtual String ItemStatus
			{
				get
				{
					return this._ItemStatus;
				}
				set
				{
					this._ItemStatus = value;
				}
			}
			#endregion
			#region ItemType
			public abstract class itemType : PX.Data.BQL.BqlString.Field<itemType> { }
			protected String _ItemType;
			[PXDBString(1, IsFixed = true, BqlField = typeof(InventoryItem.itemType))]
			[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
			[INItemTypes.List()]
			public virtual String ItemType
			{
				get
				{
					return this._ItemType;
				}
				set
				{
					this._ItemType = value;
				}
			}
			#endregion
		} 


		[PXProjection(typeof(Select<INTran>),Persistent = false)]
        [Serializable]
        [PXHidden]
		public partial class INTranSrc : IBqlTable 
		{
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			protected String _DocType;
			[PXUIField(DisplayName = INRegister.docType.DisplayName)]
			[PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(INTran.docType))]
			public virtual String DocType
			{
				get
				{
					return this._DocType;
				}
				set
				{
					this._DocType = value;
				}
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			protected String _RefNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTran.refNbr))]			
			public virtual String RefNbr
			{
				get
				{
					return this._RefNbr;
				}
				set
				{
					this._RefNbr = value;
				}
			}
			#endregion
			#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			protected Int32? _LineNbr;
			[PXDBInt(IsKey = true, BqlField = typeof(INTran.lineNbr))]			
			public virtual Int32? LineNbr
			{
				get
				{
					return this._LineNbr;
				}
				set
				{
					this._LineNbr = value;
				}
			}
			#endregion
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			protected Int32? _SiteID;
			[PXDBInt(BqlField = typeof(INTran.siteID))]			
			public virtual Int32? SiteID
			{
				get
				{
					return this._SiteID;
				}
				set
				{
					this._SiteID = value;
				}
			}
			#endregion
			#region TranDate
			public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
			protected DateTime? _TranDate;
			[PXDBDate(BqlField = typeof(INTran.tranDate))]			
			public virtual DateTime? TranDate
			{
				get
				{
					return this._TranDate;
				}
				set
				{
					this._TranDate = value;
				}
			}
			#endregion
			#region Released
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			protected Boolean? _Released;
			[PXDBBool(BqlField = typeof(INTran.released))]			
			public virtual Boolean? Released
			{
				get
				{
					return this._Released;
				}
				set
				{
					this._Released = value;
				}
			}
			#endregion
		} 

		#endregion
	

		#region Ctor + public Selects
		public PXFilter<Filter> filter;
		public PXCancel<Filter> Cancel;
		[PXFilterable]
		public PXFilteredProcessing<INItemSiteRS, Filter, Where<INItemSiteRS.siteID, Equal<Current<Filter.siteID>>,
															And<INItemSiteRS.replenishmentPolicyID, IsNotNull,
															And<INItemSiteRS.replenishmentPolicyID, Equal<Current<Filter.replenishmentPolicyID>>,
															And<INItemSiteRS.replenishmentClassID, IsNotNull,
                                                            And<Where<
                                                                INItemSiteRS.replenishmentSource, Equal<INReplenishmentSource.transfer>,
                                                                Or<INItemSiteRS.replenishmentSource, Equal<INReplenishmentSource.purchased>>>>>>>>> Records;
		public IEnumerable records() 
		{
			Filter filt = this.filter.Current;
			if(filt == null || filt.ForecastDate == null) 
				yield break;
			if (filt.Action == Filter.Actions.Calculate)
			{
				PXSelectBase<INItemSiteRS> select = new PXSelectJoin<INItemSiteRS, InnerJoin<InventoryItemRO, On<InventoryItemRO.inventoryID, Equal<INItemSiteRS.inventoryID>,
																And<InventoryItemRO.itemStatus, Equal<InventoryItemStatus.active>>>,
															InnerJoin<INItemRep, On<INItemRep.inventoryID, Equal<INItemSiteRS.inventoryID>,
																	And<INItemRep.replenishmentClassID, Equal<INItemSiteRS.replenishmentClassID>>>,
															LeftJoin<INItemClass, On<InventoryItemRO.FK.ItemClass>>>>,
															Where<INItemSiteRS.replenishmentPolicyID, IsNotNull,
															And<INItemSiteRS.replenishmentClassID, IsNotNull,
															And<INItemRep.forecastModelType, IsNotNull,
															And<INItemRep.forecastModelType, NotEqual<DemandForecastModelType.none>,
															And2<Where<INItemRep.launchDate, IsNull,
																Or<INItemRep.launchDate, LessEqual<Current<Filter.forecastDate>>>>,
															And2<Where<INItemRep.terminationDate, IsNull,
																Or<INItemRep.terminationDate, Greater<Current<Filter.forecastDate>>>>,
															And<Where<INItemSiteRS.lastForecastDate, IsNull,
																Or<INItemSite.lastForecastDate, LessEqual<Current<Filter.forecastDate>>>>>>>>>>>>(this);
				if (string.IsNullOrEmpty(filt.ItemClassCD) == false)
				{
					select.WhereAnd<Where<INItemClass.itemClassCD, Like<Current<Filter.itemClassCDWildcard>>>>();
				}
				if (String.IsNullOrEmpty(filt.ReplenishmentPolicyID) == false)
				{
					select.WhereAnd<Where<INItemSiteRS.replenishmentPolicyID, Equal<Current<Filter.replenishmentPolicyID>>>>();
				}
				if (filt.SiteID != null)
				{
					select.WhereAnd<Where<INItemSiteRS.siteID, Equal<Current<Filter.siteID>>>>();
				}
				int count = 0;
				int totalCount = 0;
				int startRow = PXView.StartRow;
				int totalRows = 0;


				foreach (PXResult<INItemSiteRS, InventoryItemRO, INItemRep> iRec in select.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					totalCount++;
					INItemSiteRS itemSite = iRec;
					InventoryItemRO item = iRec;
					INItemRep itemRep = iRec;
					if (itemSite.LastForecastDate.HasValue)
					{
						PeriodInfo current = new PeriodInfo(itemRep.ForecastPeriodType, filt.ForecastDate.Value);
						if (current.StartDate <= itemSite.LastForecastDate) continue;
					}
					count++;
					yield return itemSite;
				}
				PXView.StartRow = 0;
			}

			if (filt.Action == Filter.Actions.Clear)
			{
				PXSelectBase<INItemSiteRS> select = new PXSelectJoin<INItemSiteRS, InnerJoin<InventoryItemRO, On<InventoryItemRO.inventoryID, Equal<INItemSiteRS.inventoryID>,
																And<InventoryItemRO.itemStatus, Equal<InventoryItemStatus.active>>>,
															InnerJoin<INItemRep, On<INItemRep.inventoryID, Equal<INItemSiteRS.inventoryID>,
																	And<INItemRep.replenishmentClassID, Equal<INItemSiteRS.replenishmentClassID>>>,
															LeftJoin<INItemClass, On<InventoryItemRO.FK.ItemClass>>>>,
															Where<INItemSiteRS.replenishmentPolicyID, IsNotNull,
															And<INItemSiteRS.replenishmentClassID, IsNotNull,
															And<INItemRep.forecastModelType, IsNotNull,
															And<INItemRep.forecastModelType, NotEqual<DemandForecastModelType.none>,
															And2<Where<INItemRep.launchDate, IsNull,
																Or<INItemRep.launchDate, LessEqual<Current<Filter.forecastDate>>>>,
															And2<Where<INItemRep.terminationDate, IsNull,
																Or<INItemRep.terminationDate, Greater<Current<Filter.forecastDate>>>>,
															And<INItemSiteRS.lastForecastDate, GreaterEqual<Current<Filter.forecastDate>>,
																And<Where<INItemSiteRS.lastFCApplicationDate, IsNull,
																Or<INItemSiteRS.lastFCApplicationDate, Less<INItemSiteRS.lastForecastDate>>>>>>>>>>>>(this);
				if (string.IsNullOrEmpty(filt.ItemClassCD) == false)
				{
					select.WhereAnd<Where<INItemClass.itemClassCD, Like<Current<Filter.itemClassCDWildcard>>>>();
				}
				if (String.IsNullOrEmpty(filt.ReplenishmentPolicyID) == false)
				{
					select.WhereAnd<Where<INItemSiteRS.replenishmentPolicyID, Equal<Current<Filter.replenishmentPolicyID>>>>();
				}
				if (filt.SiteID != null)
				{
					select.WhereAnd<Where<INItemSiteRS.siteID, Equal<Current<Filter.siteID>>>>();
				}
				
				int startRow = PXView.StartRow;
				int totalRows = 0;


				foreach (PXResult<INItemSiteRS, InventoryItemRO, INItemRep> iRec in select.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{					
					INItemSiteRS itemSite = iRec;
					InventoryItemRO item = iRec;
					INItemRep itemRep = iRec;
					//if (itemSite.LastForecastDate.HasValue)
					//{
					//    PeriodInfo current = new PeriodInfo(itemRep.ForecastPeriodType, filt.ForecastDate.Value);
					//    if (current.StartDate <= itemSite.LastForecastDate) continue;
					//}				
					yield return itemSite;
				}
				PXView.StartRow = 0;
				
			}
		}

		public INUpdateReplenishmentRules()
		{
			Filter filter = this.filter.Current;
			Records.SetProcessDelegate<ReplenishmentStatsUpdateGraph>(delegate(ReplenishmentStatsUpdateGraph aGraph, INItemSiteRS item)
			{
				aGraph.Clear();
				UpdateReplenishmentProc(aGraph, item, filter);
			});
		} 
		#endregion

		protected virtual void Filter_RowSelected(PXCache sender, PXRowSelectedEventArgs e) 
		{
			if (e.Row != null && String.IsNullOrEmpty(((Filter)e.Row).Action) == false) 
			{
				Filter row = (Filter)e.Row;
				PXUIFieldAttribute.SetEnabled<Filter.siteID>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.warehouse>());
				if (row.Action == Filter.Actions.Calculate) 
				{
					Records.SetProcessDelegate<ReplenishmentStatsUpdateGraph>(delegate(ReplenishmentStatsUpdateGraph aGraph, INItemSiteRS item)
					{
						aGraph.Clear();
						UpdateReplenishmentProc(aGraph, item, row);
					});
				}
				if (row.Action == Filter.Actions.Clear) 
				{
					Records.SetProcessDelegate<ReplenishmentStatsUpdateGraph>(delegate(ReplenishmentStatsUpdateGraph aGraph, INItemSiteRS item)
					{
						aGraph.Clear();
						ClearReplenishmentProc(aGraph, item, row);
					});
				}

			}
		}

		#region Data Collection and Calculation Functions
		public static void UpdateReplenishmentProc(ReplenishmentStatsUpdateGraph graph, INItemSiteRS aItem, Filter filter)
		{
			graph.Clear();
			PXCache itemCache = graph.Caches[typeof(INItemSite)];
			PXCache subItemCache = graph.Caches[typeof(INItemSiteReplenishment)];
			INItemRep itemRep = PXSelect<INItemRep, Where<INItemRep.inventoryID, Equal<Required<INItemRep.inventoryID>>,
													And<INItemRep.replenishmentClassID, Equal<Required<INItemRep.replenishmentClassID>>>>>.Select(graph, aItem.InventoryID, aItem.ReplenishmentClassID);

			if (String.IsNullOrEmpty(itemRep.ForecastModelType) || itemRep.ForecastModelType == DemandForecastModelType.None) return;

			DateTime maxDate = filter.ForecastDate.Value;
			DateTime? minDate = null;

			string periodType = itemRep.ForecastPeriodType;
			List<PeriodInfo> periods = null;
			PeriodInfo current = new PeriodInfo(periodType, maxDate);
			maxDate = current.StartDate;

			if (aItem.LastForecastDate.HasValue && aItem.LastForecastDate.Value >= maxDate) return; //Forecast is already made for a later date			
			if (itemRep.ForecastModelType == DemandForecastModelType.MovingAverage)
			{
				int? depth = itemRep.HistoryDepth.Value;
				if (depth.HasValue)
				{
					periods = CreatePeriods(periodType, maxDate, depth.Value);
					minDate = periods.Count > 0 ? periods[0].StartDate : maxDate;
				}
			}
			else
			{
				if (aItem.LastForecastDate.HasValue && aItem.LastForecastDate.Value < current.StartDate) //Just in case
				{
					minDate = aItem.LastForecastDate;
					periods = CreatePeriods(periodType, minDate.Value, maxDate, true);
				}
			}

			List<INReplenishmentSeason> seasonality = RetrivePredefinedSeasonality(graph, aItem, false);
			Dictionary<int, SalesStatInfo> subSalesStats;
			Dictionary<int, List<PeriodSalesStatInfo>> subSalesValues;
			RetrieveSalesHistory2(graph, aItem, itemRep, minDate, maxDate, out subSalesStats, out subSalesValues);
			//DateTime salesStartDate = DateTime.MaxValue;			
			//foreach (SalesStatInfo iStat in subSalesStats.Values)
			//{
			//    if (iStat.minDate < salesStartDate) salesStartDate = iStat.minDate;
			//}			
			List<PeriodSalesStatInfo> itemSales = new List<PeriodSalesStatInfo>();
			Dictionary<int, List<PeriodSalesStatInfo>> normalizedValues = new Dictionary<int, List<PeriodSalesStatInfo>>(subSalesValues.Count);
			foreach (KeyValuePair<int, List<PeriodSalesStatInfo>> it in subSalesValues)
			{
				normalizedValues.Add(it.Key, Normalize(it.Value, current));
				AppendToList(itemSales, it.Value);
			}
			itemSales = Normalize(itemSales, current);
			subSalesValues.Clear(); //Free
			subSalesValues = null;

			DemandForecastModel model = CreateModel(itemRep.ForecastModelType);
			if (model == null) return;
			foreach (KeyValuePair<int, List<PeriodSalesStatInfo>> iSubSales in normalizedValues)
			{
				//iSubSales.Value.ForEach((PeriodSalesStatInfo op) =>
				//{
				//    op.CalcSeasonalFactor(seasonality);
				//    op.Recalc();
				//});
				model.Init(iSubSales.Value.ConvertAll<DataPointSeasonalAdapter>(op => new DataPointSeasonalAdapter(op,seasonality)));
				PeriodSalesStatInfo forecast = new PeriodSalesStatInfo(current);
				model.GetForecast(new DataPointSeasonalAdapter(forecast,seasonality));
				SalesStatInfo iSubStats;
				if (!subSalesStats.TryGetValue(iSubSales.Key, out iSubStats))
				{
					iSubStats = new SalesStatInfo();
					subSalesStats.Add(iSubSales.Key, iSubStats);
				}
				iSubStats.Average = forecast.SalesPerDaySnAjusted;
				iSubStats.MSE = forecast.SalesPerDayMSE;
			}
			//itemSales.ForEach((PeriodSalesStatInfo op) => { op.CalcSeasonalFactor(seasonality); op.Recalc(); });
			model.Init(itemSales.ConvertAll<DataPointSeasonalAdapter>(op => new DataPointSeasonalAdapter(op,seasonality)));
			PeriodSalesStatInfo itemForecast = new PeriodSalesStatInfo(current);
			model.GetForecast(new DataPointSeasonalAdapter(itemForecast, seasonality));
			SalesStatInfo salesStats = new SalesStatInfo();
			salesStats.Average = itemForecast.SalesPerDaySnAjusted; 
			salesStats.MSE = itemForecast.SalesPerDayMSE;

			Decimal? leadTime, leadTimeMSE;
			RecalcLeadTime(graph, aItem, out leadTime, out leadTimeMSE);
			INItemSite copy = (INItemSite)itemCache.CreateCopy(aItem);
			copy.LastForecastDate = filter.ForecastDate;
			copy.ForecastModelType = itemRep.ForecastModelType;
			copy.ForecastPeriodType = itemRep.ForecastPeriodType;
			Copy(copy, salesStats);			
			if (leadTime.HasValue && leadTimeMSE.HasValue)
			{
				copy.LeadTimeAverage = leadTime;
				copy.LeadTimeMSE = leadTimeMSE;
			}
			Decimal serviceLevel = (aItem.ServiceLevel ?? 0.5m); 
			double serviceFactor = StatsUtilities.NormsInv((double)serviceLevel);			
			copy.SafetyStockSuggested = (decimal)(serviceFactor * Math.Sqrt(
								(double)(leadTimeMSE ?? Decimal.Zero) * Math.Pow((double)salesStats.Average, 2.0)
								+ Math.Pow((double)(leadTime ?? Decimal.Zero), 2.0) * (double)salesStats.MSE)); //MSE - is alreade a power(2)
			if (copy.LeadTimeAverage.HasValue)
			{
				copy.MinQtySuggested = (copy.LeadTimeAverage ?? Decimal.Zero) * salesStats.Average + (copy.SafetyStockSuggested ?? Decimal.Zero);
				copy.MaxQtySuggested = copy.MinQtySuggested;
			}
			
			//copy.ForecastLastDate = maxDate;
			copy = (INItemSite)itemCache.Update(copy);
			foreach (INItemSiteReplenishment iSubRepl in graph.inSubItemsSite.Select(aItem.SiteID, aItem.InventoryID))
			{
				INItemSiteReplenishment subCopy = (INItemSiteReplenishment)subItemCache.CreateCopy(iSubRepl);
				SalesStatInfo subStatInfo;
				if (subSalesStats.TryGetValue(subCopy.SubItemID.Value, out subStatInfo))
				{
					Copy(subCopy, subStatInfo);
					subCopy.SafetyStockSuggested = (decimal)(serviceFactor * Math.Sqrt(
                                (double)(leadTimeMSE ?? Decimal.Zero) * Math.Pow((double)subStatInfo.Average, 2.0)
								+ Math.Pow((double)((leadTime ?? Decimal.Zero)), 2.0) * (double)subStatInfo.MSE));
					subCopy.MinQtySuggested = (copy.LeadTimeAverage ?? Decimal.Zero) * subStatInfo.Average + (subCopy.SafetyStockSuggested ?? Decimal.Zero);
					subCopy.MaxQtySuggested = subCopy.MinQtySuggested;
				}
				subCopy = (INItemSiteReplenishment)subItemCache.Update(subCopy);
			}
			graph.Actions.PressSave();
		}

		public static void ClearReplenishmentProc(ReplenishmentStatsUpdateGraph graph, INItemSiteRS aItem, Filter filter) 
		{
			graph.Clear();
			PXCache itemCache = graph.Caches[typeof(INItemSite)];
			PXCache subItemCache = graph.Caches[typeof(INItemSiteReplenishment)];
			INItemSite copy = (INItemSite)itemCache.CreateCopy(aItem);
			ClearReplenishmentInfo(copy);
			copy = (INItemSite)itemCache.Update(copy);
			foreach (INItemSiteReplenishment iSubRepl in graph.inSubItemsSite.Select(aItem.SiteID, aItem.InventoryID))
			{
				INItemSiteReplenishment subCopy = (INItemSiteReplenishment)subItemCache.CreateCopy(iSubRepl);
				ClearReplenishmentInfo(subCopy);
				subCopy = (INItemSiteReplenishment)subItemCache.Update(subCopy);
			}
			graph.Actions.PressSave();
		}

		protected static void RecalcLeadTime(PXGraph aGraph, INItemSiteRS aItemInfo, out Decimal? aLeadTimeAve, out Decimal? aLeadTimeMSE) 
		{
			aLeadTimeAve = null;
			aLeadTimeMSE = null;
			if (aItemInfo.ReplenishmentSource == INReplenishmentSource.Purchased)
				RecalcVendorLeadTime(aGraph, aItemInfo, out aLeadTimeAve, out aLeadTimeMSE);
			if (aItemInfo.ReplenishmentSource == INReplenishmentSource.Transfer)
				RetrieveTransferLeadTime(aGraph, aItemInfo, out aLeadTimeAve, out aLeadTimeMSE);
		}

		protected static void RecalcVendorLeadTime(PXGraph aGraph, INItemSiteRS aItemInfo, out Decimal? aLeadTimeAve, out Decimal? aLeadTimeMSE)
		{
			aLeadTimeAve = null;
			aLeadTimeMSE = null;
			int? vendorID = aItemInfo.PreferredVendorID;
			int? vendorLocationID =aItemInfo.PreferredVendorLocationID;
			List<Decimal> leadTimes = new List<decimal>();
			decimal summary = Decimal.Zero;
			PXSelectBase<POLine> select = new PXSelectReadonly2<POLine, InnerJoin<POReceiptLine, On<POReceiptLine.pOType, Equal<POLine.orderType>, And<POReceiptLine.pONbr, Equal<POLine.orderNbr>,
									And<POLine.lineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
										InnerJoin<POReceipt, On<POReceipt.receiptNbr, Equal<POReceiptLine.receiptNbr>>>>,
											Where<POReceiptLine.receiptDate, IsNotNull,
												And<POLine.requestedDate, IsNotNull,
												And<POReceiptLine.inventoryID, Equal<Required<POReceiptLine.inventoryID>>,
												And<POReceiptLine.siteID, Equal<Required<POReceiptLine.siteID>>>>>>, 
												OrderBy<Asc<POLine.requestedDate>>>(aGraph);
			if (vendorID.HasValue)
			{
				select.WhereAnd<Where<POReceipt.vendorID, Equal<Required<POReceipt.vendorID>>>>();
				if(vendorLocationID.HasValue)
					select.WhereAnd<Where<POReceipt.vendorLocationID, Equal<Required<POReceipt.vendorLocationID>>>>();
			}
			foreach (PXResult<POLine, POReceiptLine, POReceipt> iRes in select.Select(aItemInfo.InventoryID, aItemInfo.SiteID, vendorID, vendorLocationID))
			{
				POLine poLine = iRes;
				POReceiptLine receiptLine = iRes;
				TimeSpan diff = receiptLine.ReceiptDate.Value - poLine.RequestedDate.Value;
				decimal leadTime = diff.Days;
				if (leadTime < 0) continue; //Invalid data
				leadTimes.Add(leadTime);
				summary += leadTime;
			}
			if (leadTimes.Count > 0)
			{
				decimal average = summary / (decimal)leadTimes.Count;
				decimal mse = decimal.Zero;
				decimal mad = decimal.Zero;
				leadTimes.ForEach(delegate(decimal value)
				{
					decimal dev = (value - average);
					mse += dev * dev;
					mad += Math.Abs(dev);
				});

				if (leadTimes.Count > 1)
					aLeadTimeMSE = (decimal)((double)mse / ((double)(leadTimes.Count)));
				else
					aLeadTimeMSE = Decimal.Zero;

				decimal stdev = (decimal)Math.Sqrt((double)aLeadTimeMSE);
				if (leadTimes.Count > 3 && stdev > average)
				{
					leadTimes.RemoveAll(delegate(decimal value)
					{
						return Math.Abs(value - average) > (2 * stdev);
					});

					average = Decimal.Zero;
					mse = decimal.Zero;
					mad = decimal.Zero;

					leadTimes.ForEach(delegate(decimal value)
					{
						average += value;
					});
					if (leadTimes.Count > 1)
					{
						average /= leadTimes.Count;
						leadTimes.ForEach(delegate(decimal value)
						{
							decimal dev = (value - average);
							mse += dev * dev;
							mad += Math.Abs(dev);
						});
					}
				}

				aLeadTimeAve = average;
				if (leadTimes.Count > 1)
					aLeadTimeMSE = (decimal)((double)mse / ((double)(leadTimes.Count)));
				else
					aLeadTimeMSE = Decimal.Zero;
			}
			else 
			{
				if (vendorID.HasValue && vendorLocationID.HasValue) 
				{
					CR.Location vendorLocation = PXSelectReadonly<CR.Location, Where<CR.Location.bAccountID, Equal<Required<CR.Location.bAccountID>>,
													And<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>>.Select(aGraph, vendorID, vendorLocationID);
					if (vendorLocation != null && vendorLocation.VLeadTime.HasValue) 
					{
						aLeadTimeAve = vendorLocation.VLeadTime.Value;
						aLeadTimeMSE = Decimal.Zero;
					}
				}
			}
		}

		//Reads Transfer Lead Time from settings
		protected static void RetrieveTransferLeadTime(PXGraph aGraph, INItemSiteRS aItemInfo, out Decimal? aLeadTimeAve, out Decimal? aLeadTimeMSE)
		{
			aLeadTimeAve = null;
			aLeadTimeMSE = null;
			INItemClassRep inItemClassSettings = PXSelectReadonly2<INItemClassRep, InnerJoin<InventoryItemRO, On<InventoryItemRO.itemClassID, Equal<INItemClassRep.itemClassID>>>,
													Where<INItemClassRep.replenishmentClassID, Equal<Required<INItemClassRep.replenishmentClassID>>,
														And<InventoryItemRO.inventoryID, Equal<Required<InventoryItemRO.inventoryID>>>>>.Select(aGraph, aItemInfo.ReplenishmentClassID, aItemInfo.InventoryID);
			if (inItemClassSettings != null) 
			{
				aLeadTimeAve = inItemClassSettings.TransferLeadTime;
				aLeadTimeMSE = Decimal.Zero;
			}
		}

		//Calcs Transfer lead time from the transfer orders
		protected static void RecalcTransferLeadTime(PXGraph aGraph, INItemSiteRS aItemInfo, out Decimal? aLeadTimeAve, out Decimal? aLeadTimeMSE) 
		{
			aLeadTimeAve = null;
			aLeadTimeMSE = null;			
			PXSelectBase<INTran> select = new PXSelectReadonly2<INTran, InnerJoin<INTranSrc, On<INTran.origDocType, Equal<INTranSrc.docType>,
																				And<INTran.origRefNbr, Equal<INTranSrc.refNbr>,
																				And<INTran.origLineNbr, Equal<INTranSrc.lineNbr>,
																					And<INTranSrc.released, Equal<True>>>>>>, 
																				Where<INTran.origTranType,Equal<INTranType.transfer>,
																					And<INTran.tranType, Equal<INTranType.transfer>,
																					And<INTran.docType,Equal<INDocType.receipt>,
																					And<INTran.released,Equal<True>,
																					And<INTran.siteID,NotEqual<INTranSrc.siteID>,
																					And<INTran.inventoryID, Equal<Required<INTran.inventoryID>>,
																					And<INTran.siteID,Equal<Required<INTran.siteID>>>>>>>>>>(aGraph);
			if (aItemInfo.ReplenishmentSourceSiteID.HasValue) 
			{
				select.WhereAnd<Where<INTranSrc.siteID, Equal<Required<INTran.siteID>>>>();
			}

			List<int> leadTimes = new List<int>();
			int total = 0; 
 			foreach(PXResult<INTran, INTranSrc> it in select.Select(aItemInfo.InventoryID, aItemInfo.SiteID,aItemInfo.ReplenishmentSourceSiteID))
			{
				INTran receiptTran =(INTran) it;
				INTranSrc transferTran = (INTranSrc)it;
				if (transferTran.TranDate.Value > receiptTran.TranDate.Value) continue;
				int leadTime = (receiptTran.TranDate.Value - transferTran.TranDate.Value).Days;
				leadTimes.Add(leadTime);
				total += leadTime;
			}
			if (leadTimes.Count > 0)
			{
				aLeadTimeAve = ((Decimal)total) / ((Decimal)leadTimes.Count);
				decimal mean = aLeadTimeAve.Value;
				Decimal se = Decimal.Zero;
				leadTimes.ForEach(i =>{Decimal dev = (Decimal)i - mean; 
									se += dev * dev; });
				aLeadTimeMSE = se /(Decimal) leadTimes.Count;
			}			
		}

		protected static void RetrieveSalesHistory(PXGraph aGraph, INItemSiteRS aItemInfo, INItemRep aItemRepSettings, DateTime? minDate, DateTime maxDate,
													out Dictionary<int, SalesStatInfo> subSalesStats,
													out	Dictionary<int, List<PeriodSalesStatInfo>> periodValues)
		{
			subSalesStats = new Dictionary<int, SalesStatInfo>();
			periodValues = new Dictionary<int, List<PeriodSalesStatInfo>>();
			bool hasSubStats = false;
			DateTime salesStartDate = DateTime.MaxValue;
			string periodType = aItemRepSettings.ForecastPeriodType;
			foreach (PXResult<INItemSalesHistD, INItemSiteReplenishment> iSalesStats in PXSelectJoin<INItemSalesHistD,
															LeftJoin<INItemSiteReplenishment,
																On<INItemSalesHistD.FK.ItemSiteReplenishment>>,
																	Where<INItemSalesHistD.siteID, Equal<Required<INItemSalesHistD.siteID>>,
																	And<INItemSalesHistD.inventoryID, Equal<Required<INItemSalesHistD.inventoryID>>,
																And<INItemSalesHistD.sDate, Less<Required<INItemSalesHistD.sDate>>>>>,
																	OrderBy<Asc<INItemSalesHistD.sDate, Asc<INItemSalesHistD.subItemID>>>>.Select(aGraph, aItemInfo.SiteID, aItemInfo.InventoryID, maxDate))
			{
				INItemSalesHistD iSales = iSalesStats;
				INItemSiteReplenishment iRepl = iSalesStats;

				if (!hasSubStats && iRepl.SubItemID.HasValue)
					hasSubStats = true;
				SalesStatInfo subSales;
				int subSalesKey = iSales.SubItemID.Value;
				if (!subSalesStats.TryGetValue(subSalesKey, out subSales))
				{
					subSales = new SalesStatInfo();
					subSales.minDate = iSales.SDate.Value;
					subSalesStats.Add(subSalesKey, subSales);
				}

				if (aItemInfo.LaunchDate.HasValue)
				{
					subSales.minDate = aItemInfo.LaunchDate.Value;
					salesStartDate = aItemInfo.LaunchDate.Value;
				}
				else
				{
					if (subSales.minDate > iSales.SDate.Value)
						subSales.minDate = iSales.SDate.Value;
					if (iSales.SDate < salesStartDate)
						salesStartDate = iSales.SDate.Value;
				}

				if (minDate.HasValue && iSales.SDate < minDate) continue;
				if (aItemInfo.LaunchDate.HasValue && iSales.SDate < aItemInfo.LaunchDate.Value) continue; 

				DateTime subItem_SalesStartDate = (aItemInfo.LaunchDate ?? subSales.minDate);
				PeriodSalesStatInfo info = new PeriodSalesStatInfo(periodType, iSales, subItem_SalesStartDate);
				List<PeriodSalesStatInfo> iSubSales;
				if (!periodValues.TryGetValue(subSalesKey, out iSubSales))
				{
					iSubSales = new List<PeriodSalesStatInfo>();
					periodValues.Add(subSalesKey, iSubSales);
				}
				AppendToList(iSubSales, info);
			}
		}

		protected static void RetrieveSalesHistory2(PXGraph aGraph, INItemSiteRS aItemInfo, INItemRep aItemRepSettings, DateTime? minDate, DateTime maxDate,
													out Dictionary<int, SalesStatInfo> subSalesStats,
													out	Dictionary<int, List<PeriodSalesStatInfo>> periodValues)
		{
			subSalesStats = new Dictionary<int, SalesStatInfo>();
			periodValues = new Dictionary<int, List<PeriodSalesStatInfo>>();
			string periodType = aItemRepSettings.ForecastPeriodType;

			var histRecords = Lazy.By(() => PXSelectJoin<INItemSiteHistD,
					LeftJoin<INItemSiteReplenishment, On<INItemSiteHistD.FK.ItemSiteReplenishment>>,
					Where<INItemSiteHistD.siteID, Equal<Required<INItemSiteHistD.siteID>>,
						And<INItemSiteHistD.inventoryID, Equal<Required<INItemSiteHistD.inventoryID>>,
						And<INItemSiteHistD.sDate, Less<Required<INItemSiteHistD.sDate>>>>>,
					OrderBy<Asc<INItemSiteHistD.sDate, Asc<INItemSiteHistD.subItemID>>>>
				.Select(aGraph, aItemInfo.SiteID, aItemInfo.InventoryID, maxDate)
				.Cast<PXResult<INItemSiteHistD, INItemSiteReplenishment>>()
				.ToArray());

			DateTime? salesStartDate = null;
			if (aItemInfo.LaunchDate.HasValue) 
			{
				salesStartDate = aItemInfo.LaunchDate.Value;
			}
			else
			{
				INItemSiteHistD firstSale = histRecords.Value
					.Select(h => (INItemSiteHistD)h)
					.FirstOrDefault(h => (h.QtySales ?? 0m) > 0m || (h.QtyTransferOut ?? 0m) > 0m);
				salesStartDate = firstSale?.SDate;
			}

			if (!salesStartDate.HasValue || salesStartDate >= maxDate) return;
				//Init stats - needed for the case when Launch date belongs to the period in which there was no sales at all
			DateTime initDate = (minDate.HasValue && salesStartDate.Value < minDate) ? minDate.Value : salesStartDate.Value;
				InitSalesStats(aGraph, aItemInfo, aItemRepSettings, initDate, subSalesStats, periodValues);				

			foreach (PXResult<INItemSiteHistD, INItemSiteReplenishment> iSalesStats in histRecords.Value)
			{
				INItemSiteHistD iSales = iSalesStats;
				INItemSiteReplenishment iRepl = iSalesStats;

				SalesStatInfo subSales;
				int subSalesKey = iSales.SubItemID.Value;
				if (!subSalesStats.TryGetValue(subSalesKey, out subSales))
				{
					subSales = new SalesStatInfo();
					subSales.minDate = iSales.SDate.Value;
					subSalesStats.Add(subSalesKey, subSales);
				}

				if (aItemInfo.LaunchDate.HasValue)
				{
					subSales.minDate = aItemInfo.LaunchDate.Value;
				}
				else
				{
					if (subSales.minDate > iSales.SDate.Value && ((iSales.QtySales ?? Decimal.Zero) > 0 || (iSales.QtyTransferOut ?? Decimal.Zero) > 0)) //Skip receipts for start date estimation
						subSales.minDate = iSales.SDate.Value;
				}
				if (minDate.HasValue && iSales.SDate < minDate) continue;
				if (aItemInfo.LaunchDate.HasValue && iSales.SDate < aItemInfo.LaunchDate.Value) continue; 
				DateTime subItem_SalesStartDate = (aItemInfo.LaunchDate ?? subSales.minDate); // History is ordered
				PeriodSalesStatInfo info = new PeriodSalesStatInfo(periodType, iSales, subItem_SalesStartDate);
				List<PeriodSalesStatInfo> iSubSales;
				if (!periodValues.TryGetValue(subSalesKey, out iSubSales))
				{
					iSubSales = new List<PeriodSalesStatInfo>();
					periodValues.Add(subSalesKey, iSubSales);
				}
				AppendToList(iSubSales, info);
			}
		}

		protected static List<INReplenishmentSeason> RetrivePredefinedSeasonality(PXGraph aGraph, INItemSiteRS aItemInfo, bool normalize)
		{
			List<INReplenishmentSeason> result = new List<INReplenishmentSeason>();
			decimal totalWeight = 0;
			foreach (INReplenishmentSeason it in PXSelectReadonly<INReplenishmentSeason, Where<INReplenishmentSeason.active, Equal<True>,
						And<INReplenishmentSeason.replenishmentPolicyID, Equal<Required<INReplenishmentSeason.replenishmentPolicyID>>>>>.Select(aGraph, aItemInfo.ReplenishmentPolicyID))
			{
				result.Add(it);
				totalWeight += it.Factor ?? Decimal.One;
			}
			if (normalize && totalWeight > Decimal.Zero)
			{
				result.ForEach((INReplenishmentSeason op) => { op.Factor /= totalWeight; });
			}
			return result;
		} 

		protected static void InitSalesStats(PXGraph aGraph, INItemSiteRS aItemInfo,
													INItemRep aItemRepSettings,					
													DateTime initDate,
													Dictionary<int, SalesStatInfo> subSalesStats,
													Dictionary<int, List<PeriodSalesStatInfo>> periodValues)
		{
			string periodType = aItemRepSettings.ForecastPeriodType;
			int subCount = 0;
			foreach (INItemSiteReplenishment iSubRepl in PXSelectReadonly<INItemSiteReplenishment, 
																Where<INItemSiteReplenishment.siteID, Equal<Required<INItemSiteReplenishment.siteID>>,
																And<INItemSiteReplenishment.inventoryID, Equal<Required<INItemSiteReplenishment.inventoryID>>>>>.Select(aGraph, aItemInfo.SiteID, aItemInfo.InventoryID))
			{
				PeriodSalesStatInfo info = new PeriodSalesStatInfo(periodType, initDate);
				List<PeriodSalesStatInfo> iSubSales;
				int subSalesKey = iSubRepl.SubItemID.Value;
				if (!periodValues.TryGetValue(subSalesKey, out iSubSales))
				{
					iSubSales = new List<PeriodSalesStatInfo>();
					periodValues.Add(subSalesKey, iSubSales);
				}
				AppendToList(iSubSales, info);
				SalesStatInfo subSales;
				if (!subSalesStats.TryGetValue(subSalesKey, out subSales))
				{
					subSales = new SalesStatInfo();
					subSales.minDate = initDate;
					subSalesStats.Add(subSalesKey, subSales);
				}
				subCount++;
			}
			if (subCount == 0)
			{
				INSubItem emptySub = PXSelectReadonly2<INSubItem, CrossJoin<FeaturesSet>, 
										Where<INSubItem.subItemCD, Equal<INSubItem.Zero>,
												 And<FeaturesSet.subItem,Equal<False>>>>.Select(aGraph);
				if (emptySub != null && emptySub.SubItemID.HasValue)
				{
					PeriodSalesStatInfo info = new PeriodSalesStatInfo(periodType, initDate);
					List<PeriodSalesStatInfo> iSubSales;
					int subSalesKey = emptySub.SubItemID.Value;

					if (!periodValues.TryGetValue(subSalesKey, out iSubSales))
					{
						iSubSales = new List<PeriodSalesStatInfo>();
						periodValues.Add(subSalesKey, iSubSales);
					}
					AppendToList(iSubSales, info);
					SalesStatInfo subSales;
					if (!subSalesStats.TryGetValue(subSalesKey, out subSales))
					{
						subSales = new SalesStatInfo();
						subSales.minDate = initDate;
						subSalesStats.Add(subSalesKey, subSales);
					}
				}
			}
		} 
		#endregion

		#region Public Utility Functions
		public static void RecalcReplenishmentParam(decimal aDemandPerDay, decimal aDemandMSE, decimal aLeadTime, decimal aLeadTimeMSE, decimal aServiceLevel, ref decimal? reorderPoint, ref decimal? safetyStockLevel)
		{
			double serviceFactor = StatsUtilities.NormsInv((double)aServiceLevel);
			safetyStockLevel = (decimal)(serviceFactor * Math.Sqrt(Math.Pow((double)(aLeadTimeMSE * aDemandPerDay), 2.0)
								+ Math.Pow((double)(aLeadTime * aDemandMSE), 2.0)));
			reorderPoint = (aLeadTime * aDemandPerDay) + safetyStockLevel;
		}
		public static List<PeriodInfo> CreatePeriods(string periodType, DateTime maxDate, int count)
		{
			List<PeriodInfo> periods = new List<PeriodInfo>(count);
			PeriodInfo last = new PeriodInfo(periodType, maxDate);
			PeriodInfo current = new PeriodInfo(periodType, maxDate);
			maxDate = last.StartDate;
			for (int i = 0; i < count; i++)
			{
				PeriodInfo period = new PeriodInfo(last.PeriodType, last.StartDate.AddDays(-1));
				periods.Add(period);
				last = period;
			}
			periods.Reverse();
			return periods;
		}
		public static List<PeriodInfo> CreatePeriods(string periodType, DateTime minDate, DateTime maxDate, bool skipLast)
		{
			List<PeriodInfo> periods = new List<PeriodInfo>();
			PeriodInfo last = new PeriodInfo(periodType, maxDate);
			if (!skipLast) periods.Add(last);
			while (last.StartDate >= minDate)
			{
				PeriodInfo period = new PeriodInfo(last.PeriodType, last.StartDate.AddDays(-1));
				periods.Add(period);
				last = period;
			}
			periods.Reverse();
			return periods;
		}
		public static DemandForecastModel CreateModel(string aModelName)
		{
			switch (aModelName)
			{
				case DemandForecastModelType.MovingAverage:
					return new MovingAverageModel();
				case DemandForecastModelType.None:
					return null;
				default:
					throw new PXException(Messages.ThisTypeOfForecastModelIsNotImplemetedYet, aModelName);
			}
		} 
		#endregion

		#region Protected Utility Functions
		protected static List<PeriodSalesStatInfo> Normalize(List<PeriodSalesStatInfo> aSequence, PeriodInfo maxPeriod)
		{
			List<PeriodSalesStatInfo> result = new List<PeriodSalesStatInfo>(aSequence.Count);
			PeriodSalesStatInfo last = null;
			foreach (PeriodSalesStatInfo it in aSequence)
			{
				if (last != null)
				{
					if (last.Period.CompareTo(it.Period) >= 0)
						throw new PXException(Messages.InternalErrorSequenceIsNotSortedCorrectly);
					while (last.Period.IsAdjacent(it.Period) == false)
					{
						last = new PeriodSalesStatInfo(last.Period.PeriodType, last.Period.EndDate);
						result.Add(last);
					}
				}
				result.Add(it);
				last = it;
			}
			if (last != null)
			{
				if (last.Period.CompareTo(maxPeriod) >= 0)
					throw new PXException(Messages.InternalErrorSequenceIsNotSortedCorrectly);
				while (last.Period.IsAdjacent(maxPeriod) == false)
				{
					last = new PeriodSalesStatInfo(last.Period.PeriodType, last.Period.EndDate);
					result.Add(last);
				}
			}
			return result;
		}
		protected static void AppendToList(List<PeriodSalesStatInfo> aList, PeriodSalesStatInfo aInfo)
		{
			int periodIndex = aList.BinarySearch(aInfo);
			if (periodIndex < 0)
			{
				periodIndex = ~periodIndex;
				aList.Insert(periodIndex, aInfo);
			}
			else
			{
				aList[periodIndex].Append(aInfo);
			}
		}
		protected static void AppendToList(List<PeriodSalesStatInfo> aDest, List<PeriodSalesStatInfo> aSrc)
		{
			foreach (PeriodSalesStatInfo it in aSrc)
			{
				AppendToList(aDest, it);
			}
		}
		protected static void Copy(INItemSite dest, SalesStatInfo src)
		{
			dest.DemandPerDayAverage = src.Average;
			dest.DemandPerDayMSE = src.MSE;
			dest.DemandPerDayMAD = src.MAD;
		}

		protected static void Copy(INItemSiteReplenishment dest, SalesStatInfo src)
		{
			dest.DemandPerDayAverage = src.Average;
			dest.DemandPerDayMSE = src.MSE;
			dest.DemandPerDayMAD = src.MAD;
		}

		protected static void ClearReplenishmentInfo(INItemSite dest) 
		{
			dest.DemandPerDayAverage = null;
			dest.DemandPerDayMSE = null;
			dest.DemandPerDayMAD = null;
			dest.SafetyStockSuggested = null;
			dest.MinQtySuggested = null;
			dest.MaxQtySuggested = null;
			dest.ForecastModelType = null;
			dest.ForecastPeriodType = null;
			dest.LastForecastDate = null;
		}

		protected static void ClearReplenishmentInfo(INItemSiteReplenishment dest)
		{
			dest.DemandPerDayAverage = null;
			dest.DemandPerDayMSE = null;
			dest.DemandPerDayMAD = null;
			dest.SafetyStockSuggested = null;
			dest.MinQtySuggested = null;
			dest.MaxQtySuggested = null;
		}
		#endregion

		#region Internal Types Definition
		public class PeriodInfo : IComparable<PeriodInfo>, IComparable<DateTime>
		{
			public PeriodInfo(string aPeriodType, DateTime aFromDate)
			{
				this._StartDate = CalcStartDate(aPeriodType, aFromDate);
				this._EndDate = CalcEndDate(aPeriodType, aFromDate);
				this._PeriodType = aPeriodType;
			}

			public PeriodInfo(PeriodInfo aPeriod)
			{
				this._StartDate = aPeriod.StartDate;
				this._EndDate = aPeriod.EndDate;
				this._PeriodType = aPeriod.PeriodType;
			}
			private string _PeriodType;
			private DateTime _StartDate;
			private DateTime _EndDate;

			public string PeriodType
			{
				get { return this._PeriodType; }
			}
			public DateTime StartDate
			{
				get { return _StartDate; }
			}
			public DateTime EndDate
			{
				get { return _EndDate; }
			}

			public bool IsAdjacent(PeriodInfo op)
			{
				return op.StartDate == this.EndDate || op.EndDate == this.StartDate;
			}
			public int CompareTo(DateTime date)
			{
				int startComp = DateTime.Compare(date, this._StartDate);
				if (startComp < 0) return -1;
				if (DateTime.Compare(date, this._EndDate) >= 0) return 1;
				return 0;
			}
			#region IComparable<PeriodInfo> Members
			public int CompareTo(PeriodInfo op)
			{
				if (op.PeriodType != this.PeriodType) throw new Exception(Messages.AttemptToComparePeriodsOfDifferentType);
				return DateTime.Compare(this.StartDate, op.StartDate);
			}
			#endregion

			public static DateTime CalcStartDate(string aPeriodType, DateTime aDate)
			{
				DateTime startDate = aDate;
				switch (aPeriodType)
				{
					case DemandPeriodType.Month:
						startDate = new DateTime(aDate.Year, aDate.Month, 1);
						break;
					case DemandPeriodType.Quarter:
						int quarter = (aDate.Month - 1) / 3;
						startDate = new DateTime(aDate.Year, 1 + (quarter * 3), 1);
						break;
					case DemandPeriodType.Week:
						int offset = (int) DayOfWeek.Monday - (int)aDate.DayOfWeek;
						if (offset > 0) offset -= 7;
						startDate = aDate.AddDays(offset);
						break;
					case DemandPeriodType.Day:
						startDate = aDate;
						break;
				}
				return startDate;
			}
			public static DateTime CalcEndDate(string aPeriodType, DateTime aDate)
			{
				DateTime endDate = aDate;
				switch (aPeriodType)
				{
					case DemandPeriodType.Month:
						endDate = new DateTime(aDate.Year, aDate.Month, 1).AddMonths(1);
						break;
					case DemandPeriodType.Quarter:
						int quarter = (aDate.Month - 1) / 3;
						endDate = new DateTime(aDate.Year, 1 + (quarter * 3), 1).AddMonths(3);
						break;
					case DemandPeriodType.Week:
						int offset = (int) DayOfWeek.Monday - (int)aDate.DayOfWeek;
						if (offset > 0) offset -= 7;						
						//endDate = aDate.AddDays((int)DayOfWeek.Monday - (int)aDate.DayOfWeek + 7);
						endDate = aDate.AddDays(offset + 7);
						break;
					case DemandPeriodType.Day:
						endDate = aDate.AddDays(1);
						break;
				}
				return endDate;
			}

			#region IComparable<PeriodInfo> Members

			int IComparable<PeriodInfo>.CompareTo(PeriodInfo other)
			{
				throw new NotImplementedException();
			}

			#endregion
		}
		public class PeriodSalesStatInfo : IComparable<PeriodSalesStatInfo>//, DataPoint, DemandForecastModel.IDataPoint
		{
			#region Ctors
			public PeriodSalesStatInfo(string aPeriodType, DateTime aFromDate)
			{
				this.Period = new PeriodInfo(aPeriodType, aFromDate);
				this.SalesStartDate = this.Period.StartDate;
				this.SalesTotal = Decimal.Zero;
			}
			public PeriodSalesStatInfo(PeriodInfo aPeriod)
			{
				this.Period = new PeriodInfo(aPeriod);
				this.SalesStartDate = this.Period.StartDate;
				this.SalesTotal = Decimal.Zero;
			}
			public PeriodSalesStatInfo(string aPeriodType, INItemSalesHistD aSalesInfo)
			{
				this.Period = new PeriodInfo(aPeriodType, aSalesInfo.SDate.Value);
				this.SalesStartDate = this.Period.StartDate;
				this.SalesTotal = Decimal.Zero;
				this.SalesPerDayMSE = Decimal.Zero;
				this.Append(aSalesInfo);
			}

			public PeriodSalesStatInfo(string aPeriodType, INItemSiteHistD aSalesInfo)
			{
				this.Period = new PeriodInfo(aPeriodType, aSalesInfo.SDate.Value);
				this.SalesStartDate = this.Period.StartDate;
				this.SalesTotal = Decimal.Zero;
				this.SalesPerDayMSE = Decimal.Zero;
				this.Append(aSalesInfo);
			}
			public PeriodSalesStatInfo(string aPeriodType, INItemSalesHistD aSalesInfo, DateTime aSalesStartingDate)
			{
				this.Period = new PeriodInfo(aPeriodType, aSalesInfo.SDate.Value);
				this.SalesStartDate = aSalesStartingDate > this.Period.StartDate ? aSalesStartingDate : this.Period.StartDate;
				this.SalesTotal = Decimal.Zero;
				this.Append(aSalesInfo);
			}

			public PeriodSalesStatInfo(string aPeriodType, INItemSiteHistD aSalesInfo, DateTime aSalesStartingDate)
			{
				this.Period = new PeriodInfo(aPeriodType, aSalesInfo.SDate.Value);
				this.SalesStartDate = aSalesStartingDate > this.Period.StartDate ? aSalesStartingDate : this.Period.StartDate;
				this.SalesTotal = Decimal.Zero;
				this.Append(aSalesInfo);
			}
			#endregion
			protected DateTime SalesStartDate;

			#region Public Members
			public PeriodInfo Period;
			public Decimal SalesPerDay = Decimal.Zero;
			public Decimal SalesPerDaySnAjusted = Decimal.Zero;
			public Decimal SalesTotal = Decimal.Zero;
			public Decimal SalesPerDayMSE = Decimal.Zero;
			public Decimal SeasonalFactor = Decimal.One;

			#endregion
			#region Public Properties
			public Decimal StartSalesWeight
			{
				get { return DaysSinceStart / Days; }
			}
			public virtual int Days
			{
				get { return (this.Period.EndDate - this.Period.StartDate).Days; }
			}
			public virtual int DaysSinceStart
			{
				get { return (this.Period.EndDate - SalesStartDate).Days; }
			}

			#endregion
			#region Public Methods
			public Decimal Append(INItemSalesHistD aSales)
			{
				return this.SalesTotal += (aSales.QtyIssues ?? Decimal.Zero);
			}

			public Decimal Append(INItemSiteHistD aSales)
			{
				return this.SalesTotal += (aSales.QtySales?? Decimal.Zero) - (aSales.QtyCreditMemos??Decimal.Zero) + (aSales.QtyTransferOut?? Decimal.Zero) + (aSales.QtyAssemblyOut ?? Decimal.Zero);
			}
			public PeriodSalesStatInfo Append(PeriodSalesStatInfo op)
			{
				this.SalesTotal += op.SalesTotal;
				if (this.SalesStartDate > op.SalesStartDate)
					this.SalesStartDate = op.SalesStartDate;
				return this;
			}
			public void Recalc()
			{
				this.SalesPerDay = this.SalesTotal / this.DaysSinceStart;
				this.SalesPerDaySnAjusted = this.SalesTotal / (this.DaysSinceStart * this.SeasonalFactor);
			}
			public void CalcSeasonalFactor(IEnumerable<INReplenishmentSeason> seasonality)
			{
				bool crossYear = this.Period.StartDate.Year != this.Period.EndDate.Year;
				int startYear = this.Period.StartDate.Year;
				int endYear = this.Period.StartDate.Year;
				Dictionary<decimal, int> factors = new Dictionary<decimal, int>();
				foreach (INReplenishmentSeason iSeason in seasonality)
				{
					int overlap = 0;
					KeyValuePair<DateTime, DateTime> range1 = new KeyValuePair<DateTime, DateTime>(
								new DateTime(startYear, iSeason.StartDate.Value.Month, iSeason.StartDate.Value.Day),
								new DateTime(startYear, iSeason.EndDate.Value.Month, iSeason.EndDate.Value.Day));

					bool outOfRange1 = (this.Period.EndDate < range1.Key || this.Period.StartDate > range1.Value);
					if (!outOfRange1)
					{
						DateTime from = this.Period.StartDate > range1.Key ? this.Period.StartDate : range1.Key;
						DateTime to = this.Period.EndDate < range1.Value ? this.Period.EndDate : range1.Value;
						overlap += Math.Abs((to - from).Days);
					}
					if (startYear != endYear)
					{
						bool outOfRange2 = false;
						KeyValuePair<DateTime, DateTime> range2 = new KeyValuePair<DateTime, DateTime>(
								new DateTime(startYear, iSeason.StartDate.Value.Month, iSeason.StartDate.Value.Day),
								new DateTime(startYear, iSeason.EndDate.Value.Month, iSeason.EndDate.Value.Day));
						if (!outOfRange2)
						{
							DateTime from = this.Period.StartDate > range2.Key ? this.Period.StartDate : range2.Key;
							DateTime to = this.Period.EndDate < range2.Value ? this.Period.EndDate : range2.Value;
							overlap += Math.Abs((to - from).Days);
						}
					}
					if (overlap > 0)
					{
						Decimal factor = iSeason.Factor ?? Decimal.One;
						int current;
						if (!factors.TryGetValue(factor, out current))
						{
							factors.Add(factor, overlap);
						}
						else
						{
							factors[factor] += overlap;
						}
					}
				}
				int daysLeft = this.Days;
				decimal averageFactor = Decimal.Zero;
				foreach (KeyValuePair<Decimal, int> iFact in factors)
				{
					averageFactor += iFact.Key * iFact.Value / this.Days;
					daysLeft -= iFact.Value;
				}
				if (daysLeft < 0)
					throw new PXException(Messages.SeasonalSettingsAreOverlaped);
				averageFactor += Decimal.One * daysLeft / this.Days;
				this.SeasonalFactor = averageFactor;
			}

			#endregion

			#region IComparable<PeriodSalesStatInfo> Members

			public int CompareTo(PeriodSalesStatInfo other)
			{
				return this.Period.CompareTo(other.Period);
			}

			#endregion

		}
		public class DataPointAdapter : DemandForecastModel.IDataPoint
		{
			protected PeriodSalesStatInfo _info;
			protected readonly DateTime basisDate;

			public DataPointAdapter(PeriodSalesStatInfo op)
			{
				this._info = op;
				this._info.Recalc();
				//The exact date does not actually matters - it needs to reduce values for x variable 
				this.basisDate = new DateTime(2004, 1, 1);
			}
			#region IDataPoint Members

			public virtual double X
			{
				get
				{
					int days = (_info.Period.StartDate - basisDate).Days;
					return (double)days;
				}
			}

			public virtual double Y
			{
				get
				{
					return (double)(this._info.SalesPerDay);
				}
				set
				{
					this._info.SalesPerDay = (decimal)value;
				}
			}

			public virtual double YError
			{
				get
				{
					return (double)this._info.SalesPerDayMSE;
				}
				set
				{
					this._info.SalesPerDayMSE = (decimal)value;
				}
			}

			#endregion
		}
		public class DataPointSeasonalAdapter : DataPointAdapter
		{			
			public DataPointSeasonalAdapter(PeriodSalesStatInfo op, IEnumerable<INReplenishmentSeason> seansons)
				: base(op)
			{
				this._info.CalcSeasonalFactor(seansons);
				this._info.Recalc();
			}

			public override double Y
			{
				get
				{
					return (double)(this._info.SalesPerDaySnAjusted);
				}
				set
				{
					this._info.SalesPerDaySnAjusted = (decimal)value;
				}
			}

		}
		public class SalesStatInfo
		{
			public Decimal Total;
			public Decimal Average;
			public Decimal MSE;
			public Decimal MAD;
			private Decimal SETotal;
			private Decimal ADTotal;
			public DateTime minDate;
			public int Count;
			public SalesStatInfo()
			{
				this.Total = Decimal.Zero;
				this.Average = Decimal.Zero;
				this.MSE = Decimal.Zero;
				this.SETotal = Decimal.Zero;
				this.MAD = Decimal.Zero;
				this.minDate = DateTime.MaxValue;
				this.Count = 0;
			}

			public void Clear()
			{
				this.Total = Decimal.Zero;
				this.Average = Decimal.Zero;
				this.MSE = Decimal.Zero;
				this.SETotal = Decimal.Zero;
				this.MAD = Decimal.Zero;
				this.minDate = DateTime.MaxValue;
				this.Count = 0;
			}
			public void CalcDevs(Decimal iValue)
			{
				Decimal dev = iValue - this.Average;
				this.ADTotal += Math.Abs(dev);
				this.SETotal += (dev * dev);
			}
			public void Recalc()
			{
				CalcAverage();
				CalcMSE();
				CalcMAD();
			}
			public void CalcAverage() 
			{
				if (this.Count > 0)
					this.Average = this.Total / this.Count;
			}
			private Decimal CalcMSE()
			{
				if (this.Count > 1)
				{
					this.MSE = (Decimal)(((Double)this.SETotal) / ((Double)(this.Count)));
				}
				else
				{
					this.MSE = Decimal.Zero;
				}
				return this.MSE;
			}

			private Decimal CalcMAD()
			{
				if (this.Count > 1)
				{
					this.MAD = this.ADTotal / ((Decimal)(this.Count));
				}
				else
				{
					this.MAD = this.ADTotal;
				}
				return this.MAD;
			}
		}

		public static class StatsUtilities
		{
			
			/// <summary>
			/// Implement a Excel NormsInv function
			/// </summary>
			/// <returns></returns>			
			public static double NormsInv(double aProbability)
			{
				const double a1 = -39.6968302866538;
				const double a2 = 220.946098424521;
				const double a3 = -275.928510446969;
				const double a4 = 138.357751867269;
				const double a5 = -30.6647980661472;
				const double a6 = 2.50662827745924;

				const double b1 = -54.4760987982241;
				const double b2 = 161.585836858041;
				const double b3 = -155.698979859887;
				const double b4 = 66.8013118877197;
				const double b5 = -13.2806815528857;

				const double c1 = -7.78489400243029E-03;
				const double c2 = -0.322396458041136;
				const double c3 = -2.40075827716184;
				const double c4 = -2.54973253934373;
				const double c5 = 4.37466414146497;
				const double c6 = 2.93816398269878;

				const double d1 = 7.78469570904146E-03;
				const double d2 = 0.32246712907004;
				const double d3 = 2.445134137143;
				const double d4 = 3.75440866190742;

				
				const double pLow = double.Epsilon;
				const double pHigh = 1 - pLow;
								
				double q;
				double result = 0;

				if (aProbability <= 0)
					aProbability = pLow;

				if (aProbability >= 1)
					aProbability = pHigh;

				if (aProbability < pLow)
				{					
					q = Math.Sqrt(-2 * Math.Log(aProbability));
					result=(((((c1*q+c2)*q+c3)*q+c4)*q+c5)*q+c6)/((((d1*q+d2)*q+d3)*q+d4)*q+1);
				}
				else if (aProbability <= pHigh)
				{				
					q = aProbability - 0.5;
					double r = q * q;
					result=(((((a1*r+a2)*r+a3)*r+a4)*r+a5)*r+a6)*q/(((((b1*r+b2)*r+b3)*r+b4)*r+b5)*r+1);
				}
				else if (aProbability < 1)
				{				
					q = Math.Sqrt(-2 * Math.Log(1 - aProbability));
					result=-(((((c1*q+c2)*q+c3)*q+c4)*q+c5)*q+c6)/((((d1*q+d2)*q+d3)*q+d4)*q+1);
				}
				return result;
			}
		}

		#endregion
	}

	[PXHidden()]
	public class ReplenishmentStatsUpdateGraph : PXGraph<ReplenishmentStatsUpdateGraph,INItemSite>
	{
		public PXSelect<INItemSite, Where<INItemSite.siteID, Equal<Required<INItemSite.siteID>>,
				And<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>>>> inItemsSite;
		public PXSelect<INItemSiteReplenishment, Where<INItemSiteReplenishment.siteID, Equal<Required<INItemSiteReplenishment.siteID>>,
				And<INItemSiteReplenishment.inventoryID, Equal<Required<INItemSiteReplenishment.inventoryID>>>>> inSubItemsSite;
	}

    [Serializable]
	public class INRepForecastApplicationGraph : PXGraph<INRepForecastApplicationGraph>
	{
        [Serializable]
		public partial class Filter : IBqlTable
		{
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			protected Int32? _SiteID;
			[IN.Site(DisplayName = "Warehouse")]
			[PXDefault()]
			public virtual Int32? SiteID
			{
				get
				{
					return this._SiteID;
				}
				set
				{
					this._SiteID = value;
				}
			}
			#endregion
		
			#region ItemClassCD
			public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
			protected string _ItemClassCD;

			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
			public virtual string ItemClassCD
				{
				get { return this._ItemClassCD; }
				set { this._ItemClassCD = value; }
				}
			#endregion
			#region ItemClassCDWildcard
			public abstract class itemClassCDWildcard : PX.Data.BQL.BqlString.Field<itemClassCDWildcard> { }
			[PXString(IsUnicode = true)]
			[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
			[PXDimension(INItemClass.Dimension, ParentSelect = typeof(Select<INItemClass>), ParentValueField = typeof(INItemClass.itemClassCD))]
			public virtual string ItemClassCDWildcard
				{
				get { return ItemClassTree.MakeWildcard(ItemClassCD); }
				set { }
			}
			#endregion
			#region ReplenishmentPolicyID
			public abstract class replenishmentPolicyID : PX.Data.BQL.BqlString.Field<replenishmentPolicyID> { }
			protected String _ReplenishmentPolicyID;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Seasonality")]
			[PXSelector(typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>), DescriptionField = typeof(INReplenishmentPolicy.descr))]
			public virtual String ReplenishmentPolicyID
			{
				get
				{
					return this._ReplenishmentPolicyID;
				}
				set
				{
					this._ReplenishmentPolicyID = value;
				}
			}
			#endregion
		}

		public PXFilter<Filter> filter;
		public PXCancel<Filter> Cancel;
		[PXFilterable]
		public PXFilteredProcessingJoin<INItemSite, Filter, 
				LeftJoin<InventoryItem, On<INItemSite.FK.InventoryItem>,
				LeftJoin<INItemClass, On<InventoryItem.FK.ItemClass>>>,
				Where<INItemSite.siteID, Equal<Current<INItemSite.siteID>>,
				And2<Where<
					Current<Filter.itemClassCDWildcard>, IsNull, 
					Or<INItemClass.itemClassCD, Like<Current<Filter.itemClassCDWildcard>>>>,
                And2<Where<
                    INItemSite.replenishmentSource, Equal<INReplenishmentSource.transfer>,
                    Or<INItemSite.replenishmentSource, Equal<INReplenishmentSource.purchased>>>,
                And<INItemSite.lastForecastDate,IsNotNull,
				And<Where<
					INItemSite.lastFCApplicationDate, IsNull, 
					Or<INItemSite.lastFCApplicationDate, Less<INItemSite.lastForecastDate>>>>>>>>> Records;
		public IEnumerable records() 
		{
			foreach (INItemSite it in PXSelectJoin<INItemSite,
				LeftJoin<InventoryItem, On<INItemSite.FK.InventoryItem>,
				LeftJoin<INItemClass, On<InventoryItem.FK.ItemClass>>>,
				Where <INItemSite.siteID, Equal<Current<Filter.siteID>>,
				And<INItemSite.lastForecastDate, IsNotNull,
				And2<Where<
					Current<Filter.itemClassCDWildcard>, IsNull,
					Or<INItemClass.itemClassCD, Like<Current<Filter.itemClassCDWildcard>>>>,
				And<Where<
					INItemSite.lastFCApplicationDate, IsNull,
					Or<INItemSite.lastFCApplicationDate, Less<INItemSite.lastForecastDate>>>>>>>>.Select(this)) 
			{
				yield return it;
			}
		}

		public PXSelect<INItemSiteReplenishment, Where<INItemSiteReplenishment.siteID, Equal<Current<INItemSite.siteID>>,
				And<INItemSiteReplenishment.inventoryID, Equal<Current<INItemSiteReplenishment.inventoryID>>>>> subItemSettings;

		public INRepForecastApplicationGraph()
		{
			Filter filter = this.filter.Current;
			Records.SetProcessDelegate<ReplenishmentStatsUpdateGraph>(delegate(ReplenishmentStatsUpdateGraph aGraph, INItemSite item)
			{
				aGraph.Clear();
				UpdateReplenishmentProc(aGraph, item, filter);
			});
		}

		protected virtual void Filter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				Filter row = (Filter)e.Row;
				PXUIFieldAttribute.SetEnabled<Filter.siteID>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.warehouse>());
			}
		}

		public static void UpdateReplenishmentProc(ReplenishmentStatsUpdateGraph graph, INItemSite aItem, Filter filter)
		{
			PXCache itemCache = graph.Caches[typeof(INItemSite)];
			PXCache subItemCache = graph.Caches[typeof(INItemSiteReplenishment)];
			INItemSite item = graph.inItemsSite.Select(aItem.SiteID, aItem.InventoryID);
			INItemSite copy = (INItemSite) itemCache.CreateCopy(item);
			copy.MinQty = copy.MinQtySuggested;
			copy.MinQtyOverride = true;
			copy.MaxQty = copy.MaxQtySuggested;
			copy.MaxQtyOverride = true;

			copy.SafetyStock = copy.SafetyStockSuggested;
			copy.SafetyStockOverride = true;
			copy.LastFCApplicationDate = aItem.LastForecastDate;
			itemCache.Update(copy);
			foreach(INItemSiteReplenishment iSubSettings in graph.inSubItemsSite.Select(aItem.SiteID, aItem.InventoryID))
			{
				INItemSiteReplenishment subCopy = (INItemSiteReplenishment) subItemCache.CreateCopy(iSubSettings);
				subCopy.MinQty = subCopy.MinQtySuggested ?? Decimal.Zero;
				subCopy.SafetyStock = subCopy.SafetyStockSuggested ?? Decimal.Zero;
				subCopy.MaxQty = subCopy.MaxQtySuggested ?? Decimal.Zero; 
				subItemCache.Update(subCopy);
			}
			graph.Actions.PressSave();
		}
	}
	public abstract class DemandForecastModel 
	{
		
		public DemandForecastModel()
		{
			
		}
		
		public interface IDataPoint 
		{
			double X
			{
				get;
			}
			double Y
			{
				get;
				set;
			}
			double YError
			{
				get;
				set;
			}
		}
		public abstract void Init(IEnumerable<IDataPoint> dataSequence);
 		public abstract IDataPoint GetForecast(IDataPoint point);
	}

	public class MovingAverageModel : DemandForecastModel 
	{
		protected static readonly string _modelName = DemandForecastModelType.MovingAverage;

		public static string ModelName
		{
			get { return _modelName; }
		}

		public MovingAverageModel() : base() { }


		public override void Init(IEnumerable<IDataPoint> dataSequence)
		{
			this.statsInfo = new INUpdateReplenishmentRules.SalesStatInfo();
			foreach (IDataPoint iPt in dataSequence) 
			{
				statsInfo.Total += (decimal) iPt.Y;
				statsInfo.Count++;
			}
			statsInfo.CalcAverage();
			foreach (IDataPoint iPt in dataSequence)
			{
				statsInfo.CalcDevs((decimal)iPt.Y);				
			}
			statsInfo.Recalc();
		}

		public override IDataPoint GetForecast(IDataPoint point) 
		{
			point.Y = (double) this.statsInfo.Average;
			point.YError = Math.Abs((double)this.statsInfo.MSE);
			return point;
		} 

		protected INUpdateReplenishmentRules.SalesStatInfo statsInfo; 
	}

}
