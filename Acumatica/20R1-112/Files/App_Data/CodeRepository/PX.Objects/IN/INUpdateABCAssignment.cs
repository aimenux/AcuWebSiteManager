using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using PX.SM;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using System.Linq;

namespace PX.Objects.IN
{
	#region Update Settings

	[Serializable]
	public partial class UpdateABCAssignmentSettings : PX.Data.IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDefault()]
		[Site()]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		//[FinPeriodID]
		//[FinPeriodSelector(typeof(AccessInfo.businessDate))]
		[INClosedPeriod(typeof(AccessInfo.businessDate))]
		[PXDefault()]
		[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}

	#endregion

	#region UpdateResult
    [Serializable]
	public partial class UpdateABCAssignmentResult : PX.Data.IBqlTable
    {
		#region ID
		[PXGuid(IsKey = true)]
		public virtual Guid ID { get; set; } = Guid.NewGuid();
		public abstract class id : PX.Data.BQL.BqlGuid.Field<id> { } 
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
	    protected Int32? _InventoryID;
	    [Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Inventory ID")]
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
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion

		#region OldABCCode
		public abstract class oldABCCode : PX.Data.BQL.BqlString.Field<oldABCCode> { }
		protected String _OldABCCode;
		[PXString(1)]
		[PXUIField(DisplayName = "Current ABC Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OldABCCode
		{
			get
			{
				return this._OldABCCode;
			}
			set
			{
				this._OldABCCode = value;
			}
		}
		#endregion
		#region ABCCodeFixed
		public abstract class aBCCodeFixed : PX.Data.BQL.BqlBool.Field<aBCCodeFixed> { }
		protected bool? _ABCCodeFixed = false;
		[PXBool]
		[PXUIField(DisplayName = "Fixed", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? ABCCodeFixed
		{
			get
			{
				return this._ABCCodeFixed;
			}
			set
			{
				this._ABCCodeFixed = value;
			}
		}
		#endregion
		#region NewABCCode
		public abstract class newABCCode : PX.Data.BQL.BqlString.Field<newABCCode> { }
		protected String _NewABCCode;
		[PXString(1)]
		[PXUIField(DisplayName = "Projected ABC Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String NewABCCode
		{
			get
			{
				return this._NewABCCode;
			}
			set
			{
				this._NewABCCode = value;
			}
		}
		#endregion

		#region YtdCost
		public abstract class ytdCost : PX.Data.BQL.BqlDecimal.Field<ytdCost> { }
		protected decimal? _YtdCost;
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Criteria Value")]
		public virtual decimal? YtdCost
		{
			get
			{
				return _YtdCost;
			}
			set
			{
				_YtdCost = value;
			}
		}
		#endregion
		#region Ratio
		public abstract class ratio : PX.Data.BQL.BqlDecimal.Field<ratio> { }
		protected decimal? _Ratio;
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Ratio, %")]
		public virtual decimal? Ratio
		{
			get
			{
				return _Ratio;
			}
			set
			{
				_Ratio = value;
			}
		}
		#endregion
		#region CumulativeRatio
		public abstract class cumulativeRatio : PX.Data.BQL.BqlDecimal.Field<cumulativeRatio> { }
		protected decimal? _CumulativeRatio;
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Cumulative Ratio, %")]
		public virtual decimal? CumulativeRatio
		{
			get
			{
				return _CumulativeRatio;
			}
			set
			{
				_CumulativeRatio = value;
			}
		}
		#endregion


		#region //OldMovementClass
		/*
		public abstract class oldMovementClass : PX.Data.BQL.BqlString.Field<oldMovementClass> { }
		protected String _OldMovementClass;
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Old Movement Class", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OldMovementClass
		{
			get
			{
				return this._OldMovementClass;
			}
			set
			{
				this._OldMovementClass = value;
			}
		}
		*/
		#endregion
		#region //MovementClassFixed
		/*
		public abstract class movementClassFixed : PX.Data.BQL.BqlBool.Field<movementClassFixed> { }
		protected bool? _MovementClassFixed = false;
		[PXBool]
		[PXUIField(DisplayName = "Fixed", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? MovementClassFixed
		{
			get
			{
				return this._MovementClassFixed;
			}
			set
			{
				this._MovementClassFixed = value;
			}
		}
		*/
		#endregion
		#region //NewMovementClass
		/*
		public abstract class newMovementClass : PX.Data.BQL.BqlString.Field<newMovementClass> { }
		protected String _NewMovementClass;
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "New Movement Class", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String NewMovementClass
		{
			get
			{
				return this._NewMovementClass;
			}
			set
			{
				this._NewMovementClass = value;
			}
		}
		*/
		#endregion

	}
	#endregion


	[PX.Objects.GL.TableAndChartDashboardType]
	public class INUpdateABCAssignment : PXGraph<INUpdateABCAssignment>
	{
		public PXCancel<UpdateABCAssignmentSettings> Cancel;

		public PXFilter<UpdateABCAssignmentSettings> UpdateSettings;

		public PXSelectOrderBy<UpdateABCAssignmentResult, OrderBy<Desc<UpdateABCAssignmentResult.ytdCost, Asc<UpdateABCAssignmentResult.cumulativeRatio>>>> ResultPreview;

		public PXSelect<INItemSite> itemsite;

		public PXAction<UpdateABCAssignmentSettings> Process;

		[PXUIField(DisplayName = Messages.Process, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable process(PXAdapter adapter)
		{
			//  recalc and save to INItemSite
			CalcABCAssignments(true);

			return adapter.Get();
		}
		
		public INUpdateABCAssignment()
		{
			ResultPreview.Cache.AllowInsert = false;
			ResultPreview.Cache.AllowDelete = false;
			ResultPreview.Cache.AllowUpdate = false;
		}

		private List<UpdateABCAssignmentResult> CalcABCAssignments(bool updateDB)
		{

			UpdateABCAssignmentSettings us = UpdateSettings.Current;

			List<UpdateABCAssignmentResult> result_list = new List<UpdateABCAssignmentResult>();

			if (us == null) { return result_list; } //empty

			if ( (us.SiteID == null ) || (us.FinPeriodID == null) ) { return result_list; } //empty

			if (updateDB)
			{
				itemsite.Cache.Clear();
			}

			CostHistoryBySiteByPeriod costHistory = CreateCostHistory(us.SiteID.Value, us.FinPeriodID);

			PXSelectBase<INItemSite> cmd = new PXSelectJoin<INItemSite,
				InnerJoin<InventoryItem, 
					On2<INItemSite.FK.InventoryItem,
					And<InventoryItem.stkItem, NotEqual<boolFalse>,
					And<Match<InventoryItem, Current<AccessInfo.userName>>>>>>,
				Where<INItemSite.siteID, Equal<Current<UpdateABCAssignmentSettings.siteID>>>>(this);
			
			PXResultset<INItemSite> intermediateResult = (PXResultset<INItemSite>)cmd.Select();

			//	1. set next non-fixed item position to [0]
			//	2. for each ABC code X starting from 'A' to 'Z'
			//	{
			//		2.1 move items having X code to the result list, counting their cost until cumulative cost not greater than cumulative ABC cost
			//		 (fixed-ABC-code items do not change their code)
			//	}


			// 0.1 
			PXResultset<INABCCode> abc_codes = 
				PXSelectOrderBy<INABCCode,
					OrderBy<Asc<INABCCode.aBCCodeID>>>.Select(this);

			//0.2
			decimal total_cost_on_site = costHistory.GetTotalCostOnSite();
			
			// 0.3
			if ((abc_codes.Count == 0) || (total_cost_on_site == 0))
			{
				// nothing to change :
				foreach (PXResult<INItemSite, InventoryItem> it in intermediateResult)
				{
					INItemSite is_rec = (INItemSite)it;
					InventoryItem ii_rec = (InventoryItem)it;
					UpdateABCAssignmentResult r_rec = new UpdateABCAssignmentResult();

					r_rec.ABCCodeFixed = is_rec.ABCCodeIsFixed;
					r_rec.Descr = ii_rec.Descr;
					r_rec.InventoryID = is_rec.InventoryID;
					r_rec.OldABCCode = is_rec.ABCCodeID;
					r_rec.NewABCCode = is_rec.ABCCodeID; // null ?

					result_list.Add(r_rec);
				}

				return result_list;

			}

			intermediateResult.Sort((x, y) => 
			{
				INItemSite a = y;
				INItemSite b = x;

				decimal tranYtdCost_X = costHistory.GetTranYTDCost(a.InventoryID.Value);
				decimal tranYtdCost_Y = costHistory.GetTranYTDCost(b.InventoryID.Value);

				return tranYtdCost_X.CompareTo(tranYtdCost_Y);
			});

			//	1. set next item position to [0]
			int next_item_to_process = 0;
			decimal cumulative_cost = 0m;
			decimal cumulative_abc_pct = 0m;

			//	2. for each ABC code X starting from 'A' to 'Z'
			foreach (PXResult<INABCCode> abc_it in abc_codes)
			{
				INABCCode abc_rec = (INABCCode) abc_it; 
				cumulative_abc_pct += (abc_rec.ABCPct ?? 0m);

				// 2.1 move items having X code to the result list, counting their cost until cumulative cost not greater than cumulative ABC cost
				// (fixed-ABC-code items do not change their code)

				while (next_item_to_process < intermediateResult.Count)
				{
					PXResult<INItemSite, InventoryItem> it = (PXResult<INItemSite, InventoryItem>)intermediateResult[next_item_to_process];

					INItemSite is_rec = (INItemSite)it;
					InventoryItem ii_rec = (InventoryItem)it;

					decimal tranYtdCost = costHistory.GetTranYTDCost(ii_rec.InventoryID.Value);

					if ( ( (cumulative_cost + tranYtdCost) / total_cost_on_site ) <= ( cumulative_abc_pct / 100m ) )
					{
						cumulative_cost += tranYtdCost;
						UpdateABCAssignmentResult r_rec = new UpdateABCAssignmentResult();
						r_rec.ABCCodeFixed = is_rec.ABCCodeIsFixed;
						r_rec.Descr = ii_rec.Descr;
                        r_rec.InventoryID = is_rec.InventoryID;
						if (is_rec.ABCCodeIsFixed ?? false)
						{
							r_rec.NewABCCode = is_rec.ABCCodeID;
						}
						else
						{
							r_rec.NewABCCode = abc_rec.ABCCodeID;
						}
						r_rec.OldABCCode = is_rec.ABCCodeID;
						r_rec.YtdCost = tranYtdCost;
						r_rec.Ratio = r_rec.YtdCost/total_cost_on_site*100;
						r_rec.CumulativeRatio = cumulative_cost/total_cost_on_site*100;

						result_list.Add(r_rec);

						if (updateDB && (is_rec.ABCCodeID != r_rec.NewABCCode))
						{
							is_rec.ABCCodeID = r_rec.NewABCCode; 
							itemsite.Update(is_rec);
						}
						next_item_to_process++;
					}
					else
					{
						break;
					}
				}
			}

			if (updateDB)
			{
				this.Actions.PressSave();
			}
			return result_list;
		}
				
		public virtual CostHistoryBySiteByPeriod CreateCostHistory(int siteID, string finperiod)
		{
			return new CostHistoryBySiteByPeriod(this, siteID, finperiod);
		}

		protected virtual IEnumerable resultPreview()
		{
			return CalcABCAssignments(false);
		}
	}

	public class CostHistoryBySiteByPeriod
	{
		protected PXGraph context;
		protected int siteID;
		protected string finPeriod;
		protected Dictionary<int, decimal> tranYTDCostTable;

		public CostHistoryBySiteByPeriod(PXGraph context, int siteID, string finperiod)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (string.IsNullOrEmpty(finperiod))
				throw new ArgumentNullException(nameof(finperiod));

			this.context = context;
			this.siteID = siteID;
			this.finPeriod = finperiod;

		}

		protected virtual Dictionary<int, decimal> GetTranYTDCostTableByPeriod(string periodID)
		{
			if (string.IsNullOrEmpty(periodID))
				throw new ArgumentNullException();

			if (tranYTDCostTable != null)
			{
				return tranYTDCostTable;
			}

			tranYTDCostTable = new Dictionary<int, decimal>();

			var select = new PXSelectJoinGroupBy<INItemCostHist,
				InnerJoin<INItemCostHistByPeriod,
					On<INItemCostHist.inventoryID, Equal<INItemCostHistByPeriod.inventoryID>,
					And<INItemCostHist.costSiteID, Equal<INItemCostHistByPeriod.costSiteID>,
					And<INItemCostHist.costSubItemID, Equal<INItemCostHistByPeriod.costSubItemID>,
					And<INItemCostHist.accountID, Equal<INItemCostHistByPeriod.accountID>,
					And<INItemCostHist.subID, Equal<INItemCostHistByPeriod.subID>,
					And<INItemCostHist.finPeriodID, Equal<INItemCostHistByPeriod.lastActivityPeriod>>>>>>>>,
				Where<INItemCostHistByPeriod.finPeriodID, Equal<Required<INItemCostHistByPeriod.finPeriodID>>,
					And<INItemCostHistByPeriod.siteID, Equal<Required<INItemCostHistByPeriod.siteID>>>>,
				Aggregate<
					GroupBy<INItemCostHist.costSiteID,
					GroupBy<INItemCostHist.inventoryID,
					GroupBy<INItemCostHist.finPeriodID,
					Sum<INItemCostHist.tranYtdCost>>>>>
				>(context);

			foreach (PXResult<INItemCostHist, INItemCostHistByPeriod> res in select.Select(periodID, siteID))
			{
				INItemCostHist cost = (INItemCostHist)res;
				
					if (!tranYTDCostTable.ContainsKey(cost.InventoryID.Value))
						tranYTDCostTable.Add(cost.InventoryID.Value, cost.FinYtdCost.GetValueOrDefault());
					else
						tranYTDCostTable[cost.InventoryID.Value] += cost.FinYtdCost.GetValueOrDefault();
				}

			return tranYTDCostTable;
		}

		public virtual decimal GetTranYTDCost(int inventoryID)
		{
			Dictionary<int, decimal> table = GetTranYTDCostTableByPeriod(finPeriod);

			decimal result = 0;
			table.TryGetValue(inventoryID, out result);

			return result;
		}

		public virtual decimal GetTotalCostOnSite()
		{
			Dictionary<int, decimal> table = GetTranYTDCostTableByPeriod(finPeriod);

			return table.Sum(x => x.Value);
		}
	}
}
