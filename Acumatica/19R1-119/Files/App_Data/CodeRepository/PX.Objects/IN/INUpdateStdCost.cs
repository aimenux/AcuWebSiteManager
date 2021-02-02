using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using System.Collections;
using System.Linq;

namespace PX.Objects.IN
{
    [Serializable]
	public partial class INSiteFilter : IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site()]
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
		#region PendingStdCostDate
		public abstract class pendingStdCostDate : PX.Data.BQL.BqlDateTime.Field<pendingStdCostDate> { }
		protected DateTime? _PendingStdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Max. Pending Cost Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? PendingStdCostDate
		{
			get
			{
				return this._PendingStdCostDate;
			}
			set
			{
				this._PendingStdCostDate = value;
			}
		}
		#endregion
		#region RebuildHistory
		public abstract class rebuildHistory : PX.Data.BQL.BqlBool.Field<rebuildHistory> { }
		protected Boolean? _RebuildHistory;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Rebuild Item History")]
		public virtual Boolean? RebuildHistory
		{
			get
			{
				return this._RebuildHistory;
			}
			set
			{
				this._RebuildHistory = value;
			}
		}
		#endregion
		#region ReplanBackorders
		public abstract class replanBackorders : PX.Data.BQL.BqlBool.Field<replanBackorders> { }
		protected Boolean? _ReplanBackorders;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Replan Back Orders")]
		public virtual Boolean? ReplanBackorders
		{
			get
			{
				return this._ReplanBackorders;
			}
			set
			{
				this._ReplanBackorders = value;
			}
		}
		#endregion
		#region RevalueInventory
		public abstract class revalueInventory : PX.Data.BQL.BqlBool.Field<revalueInventory> { }
		protected Boolean? _RevalueInventory;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Revalue Inventory")]
		public virtual Boolean? RevalueInventory
		{
			get
			{
				return this._RevalueInventory;
			}
			set
			{
				this._RevalueInventory = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodNonLockedSelector(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Fin. Period")]
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

    [Serializable]
	public partial class INUpdateStdCostRecord : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected Boolean? _Selected = false;
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual Boolean? Selected
		{
			get
			{
				return this._Selected;
			}
			set
			{
				this._Selected = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(IsKey = true, DirtyRead = true, DisplayName = "Inventory ID")]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
        [IN.Site()]
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
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
        [PXDBIdentity(IsKey = true)]
        public virtual int? RecordID
        {
            get;
            set;
        }
        #endregion
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.BQL.BqlInt.Field<invtAcctID> { }
		protected Int32? _InvtAcctID;
		[Account(DisplayName = "Inventory Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault()]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.BQL.BqlInt.Field<invtSubID> { }
		protected Int32? _InvtSubID;
		[SubAccount(typeof(INUpdateStdCostRecord.invtAcctID), DisplayName = "Inventory Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region PendingStdCost
		public abstract class pendingStdCost : PX.Data.BQL.BqlDecimal.Field<pendingStdCost> { }
		protected Decimal? _PendingStdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Cost")]
		public virtual Decimal? PendingStdCost
		{
			get
			{
				return this._PendingStdCost;
			}
			set
			{
				this._PendingStdCost = value;
			}
		}
		#endregion
		#region PendingStdCostDate
		public abstract class pendingStdCostDate : PX.Data.BQL.BqlDateTime.Field<pendingStdCostDate> { }
		protected DateTime? _PendingStdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Cost Date")]
		public virtual DateTime? PendingStdCostDate
		{
			get
			{
				return this._PendingStdCostDate;
			}
			set
			{
				this._PendingStdCostDate = value;
			}
		}
		#endregion
		#region PendingStdCostReset
		public abstract class pendingStdCostReset : PX.Data.IBqlField
		{
		}
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? PendingStdCostReset
		{
			get;
			set;
		}
		#endregion
		#region StdCost
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }
		protected Decimal? _StdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Current Cost", Enabled = false)]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region StdCostOverride
		public abstract class stdCostOverride : PX.Data.BQL.BqlBool.Field<stdCostOverride> { }
		protected Boolean? _StdCostOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Std. Cost Override")]
		public virtual Boolean? StdCostOverride
		{
			get
			{
				return this._StdCostOverride;
			}
			set
			{
				this._StdCostOverride = value;
			}
		}
		#endregion
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class INUpdateStdCost : PXGraph<INUpdateStdCost>
	{
		public PXCancel<INSiteFilter> Cancel;
		public PXFilter<INSiteFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<INUpdateStdCostRecord, INSiteFilter> INItemList;
		public PXSetup<INSetup> insetup;

        public override bool IsDirty
        {
            get
            {
                return false;
            }
        }

		public INUpdateStdCost()
		{
			INSetup record = insetup.Current;

			INItemList.SetProcessCaption(Messages.Process);
			INItemList.SetProcessAllCaption(Messages.ProcessAll);
		}

		protected virtual void INSiteFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				if (((INSiteFilter)e.Row).RevalueInventory == true && ((INSiteFilter)e.Row).SiteID != null)
				{
					INItemList.SetProcessDelegate<INUpdateStdCostProcess>(RevalueInventory, ReleaseAdjustment);
				}
				else
				{
					INItemList.SetProcessDelegate<INUpdateStdCostProcess>(UpdateStdCost, ReleaseAdjustment);
				}
			}
		}

		protected virtual IEnumerable initemlist()
		{
			INSiteFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}
			bool found = false;
			foreach (INUpdateStdCostRecord item in INItemList.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;


			if (Filter.Current.SiteID == null)
			{
				//Non-Stock:

                PXSelectBase<InventoryItem> inventoryItems = new PXSelectReadonly<InventoryItem, 
					Where<InventoryItem.stkItem, Equal<boolFalse>,
					And<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>, 
					And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
					And<InventoryItem.pendingStdCostDate, LessEqual<Current<INSiteFilter.pendingStdCostDate>>>>>>>(this);

                foreach (InventoryItem item in inventoryItems.Select())
				{
					INUpdateStdCostRecord record = new INUpdateStdCostRecord();
					record.InventoryID = item.InventoryID;
                    record.RecordID = 1;
					record.InvtAcctID = item.InvtAcctID;
					record.InvtSubID = item.InvtSubID;
					record.PendingStdCost = item.PendingStdCost;
					record.PendingStdCostDate = item.PendingStdCostDate;
					record.StdCost = item.StdCost;

					yield return INItemList.Insert(record);
				}

				//Stock:

                PXSelectBase<INItemSite> itemSites = new PXSelectJoin<INItemSite,
                    InnerJoin<InventoryItem, On2<INItemSite.FK.InventoryItem,
                        And<Match<InventoryItem, Current<AccessInfo.userName>>>>>,
                    Where<INItemSite.valMethod, Equal<INValMethod.standard>,
                        And<INItemSite.siteStatus, Equal<INItemStatus.active>,
						And<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>, And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
						And<Where<INItemSite.pendingStdCostDate, LessEqual<Current<INSiteFilter.pendingStdCostDate>>,
							Or<INItemSite.pendingStdCostReset, Equal<boolTrue>>>>>>>>>(this);

                foreach (INItemSite item in itemSites.Select())
                {
                    INUpdateStdCostRecord record = new INUpdateStdCostRecord();
                    record.InventoryID = item.InventoryID;
                    record.SiteID = item.SiteID;
                    record.RecordID = item.SiteID;
                    record.InvtAcctID = item.InvtAcctID;
                    record.InvtSubID = item.InvtSubID;
                    record.PendingStdCost = item.PendingStdCost;
                    record.PendingStdCostDate = item.PendingStdCostDate;
					record.PendingStdCostReset = item.PendingStdCostReset;
                    record.StdCost = item.StdCost;
                    record.StdCostOverride = item.StdCostOverride;

                    yield return INItemList.Insert(record);
                }
			}
			else
			{
				//Stock

				PXSelectBase<INItemSite> s = new PXSelectJoin<INItemSite,
					InnerJoin<InventoryItem, On2<INItemSite.FK.InventoryItem,
						And<Match<InventoryItem, Current<AccessInfo.userName>>>>>,
					Where<INItemSite.valMethod, Equal<INValMethod.standard>,
						And<INItemSite.siteStatus, Equal<INItemStatus.active>,
						And<INItemSite.siteID, Equal<Current<INSiteFilter.siteID>>,
						And<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
						And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
						And<Where<INItemSite.pendingStdCostDate, LessEqual<Current<INSiteFilter.pendingStdCostDate>>,
							Or<Current<INSiteFilter.revalueInventory>, Equal<boolTrue>,
							Or<INItemSite.pendingStdCostReset, Equal<boolTrue>>>>>>>>>>>(this);

				foreach (INItemSite item in s.Select())
				{
					INUpdateStdCostRecord record = new INUpdateStdCostRecord();
					record.InventoryID = item.InventoryID;
					record.SiteID = item.SiteID;
					record.RecordID = item.SiteID;
					record.InvtAcctID = item.InvtAcctID;
					record.InvtSubID = item.InvtSubID;
					record.PendingStdCost = item.PendingStdCost;
					record.PendingStdCostDate = item.PendingStdCostDate;
					record.PendingStdCostReset = item.PendingStdCostReset;
					record.StdCost = item.StdCost;
					record.StdCostOverride = item.StdCostOverride;

					yield return INItemList.Insert(record);
				}

			}
		}

        public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
        {
            try
            {
                return base.ExecuteUpdate(viewName, keys, values, parameters);
            }
            catch (Exception ex)
            {
                if (viewName.ToLower() == "initemlist" && ((ex is System.Data.SqlClient.SqlException && ((System.Data.SqlClient.SqlException)ex).Number == 208 /*invalid object name*/) || (ex is PXArgumentException && ((PXArgumentException)ex).ParamName =="table")))
                {
                    INItemList.Select();
                    try
                    {
                        return base.ExecuteUpdate(viewName, keys, values, parameters);
                    }
                    catch (System.Data.SqlClient.SqlException)
                    {
                        return 0;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

		protected virtual void INSiteFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			INItemList.Cache.Clear();
		}

		public static void UpdateStdCost(INUpdateStdCostProcess graph, INUpdateStdCostRecord itemsite)
		{
			graph.UpdateStdCost(itemsite);
		}

		public static void RevalueInventory(INUpdateStdCostProcess graph, INUpdateStdCostRecord itemsite)
		{
			graph.RevalueInventory(itemsite);
		}
		public static void ReleaseAdjustment(INUpdateStdCostProcess graph)
		{
			graph.ReleaseAdjustment();
		}
	}

	public class INUpdateStdCostProcess : PXGraph<INUpdateStdCostProcess>
	{
		public INAdjustmentEntry je = PXGraph.CreateInstance<INAdjustmentEntry>();

		public PXSelectJoin<INCostStatus,
				InnerJoin<InventoryItem, On<INCostStatus.FK.InventoryItem>,
				InnerJoin<INSite, On<INCostStatus.FK.CostSite>,
				InnerJoin<INPostClass, On<InventoryItem.FK.PostClass>,
				InnerJoin<INItemSite, On<INCostStatus.FK.CostItemSite>,
				LeftJoin<INTran, On<INTran.inventoryID, Equal<INCostStatus.inventoryID>, 
					And<INTran.siteID, Equal<INCostStatus.costSiteID>, 
					And<INTran.docType, Equal<Current<INRegister.docType>>, 
					And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>>>>>>>>,
			Where<INCostStatus.inventoryID, Equal<Current<INUpdateStdCostRecord.inventoryID>>, 
				And<INTran.refNbr, IsNull, 
				And<Where<INCostStatus.costSiteID, Equal<Current<INUpdateStdCostRecord.siteID>>, 
				Or<Where<Current<INUpdateStdCostRecord.stdCostOverride>, Equal<False>,
						And<Current<INUpdateStdCostRecord.pendingStdCostReset>, Equal<False>,
						And<INItemSite.stdCostOverride, Equal<False>,
						And<INItemSite.pendingStdCostReset, Equal<False>>>>>>>>>>> incoststatus;

		public INUpdateStdCostProcess()
			: base()
		{
			je.insetup.Current.RequireControlTotal = false;
			je.insetup.Current.HoldEntry = false;
		}

		public virtual void UpdateStdCost(INUpdateStdCostRecord itemsite)
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					//will be true for non-stock items as well
					if (itemsite.StdCostOverride == false && itemsite.PendingStdCostReset == false)
					{
						PXDatabase.Update<INItemSite>(
							new PXDataFieldAssign("StdCost", PXDbType.DirectExpression, "PendingStdCost"),
							new PXDataFieldAssign("StdCostDate", PXDbType.DateTime, itemsite.PendingStdCostDate),
							new PXDataFieldAssign("PendingStdCost", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("PendingStdCostDate", PXDbType.DateTime, null),
							new PXDataFieldAssign("LastStdCost", PXDbType.DirectExpression, "StdCost"),
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, itemsite.InventoryID),
							new PXDataFieldRestrict("StdCostOverride", PXDbType.Bit, false),
							new PXDataFieldRestrict("PendingStdCostDate", PXDbType.DateTime, 4, itemsite.PendingStdCostDate, PXComp.LE),
							new PXDataFieldRestrict("PendingStdCostReset", PXDbType.Bit, false));

						PXDatabase.Update<InventoryItem>(
							new PXDataFieldAssign("StdCost", PXDbType.DirectExpression, "PendingStdCost"),
							new PXDataFieldAssign("StdCostDate", PXDbType.DateTime, itemsite.PendingStdCostDate),
							new PXDataFieldAssign("PendingStdCost", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("PendingStdCostDate", PXDbType.DateTime, null),
							new PXDataFieldAssign("LastStdCost", PXDbType.DirectExpression, "StdCost"),
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, itemsite.InventoryID),
							new PXDataFieldRestrict("PendingStdCostDate", PXDbType.DateTime, 4, itemsite.PendingStdCostDate, PXComp.LE));
					}
					else
					{
						var updateParams = new List<PXDataFieldParam>
						{
							new PXDataFieldAssign("StdCost", PXDbType.DirectExpression, "PendingStdCost"),
							new PXDataFieldAssign("StdCostDate", PXDbType.DateTime, itemsite.PendingStdCostDate ?? Accessinfo.BusinessDate),
							new PXDataFieldAssign("PendingStdCost", PXDbType.Decimal, 0m),
							new PXDataFieldAssign("PendingStdCostDate", PXDbType.DateTime, null),
							new PXDataFieldAssign("PendingStdCostReset", PXDbType.Bit, false),
							new PXDataFieldAssign("LastStdCost", PXDbType.DirectExpression, "StdCost"),
							new PXDataFieldRestrict("InventoryID", PXDbType.Int, itemsite.InventoryID),
							new PXDataFieldRestrict("SiteID", PXDbType.Int, itemsite.SiteID)
						};
						
						// Restriction was added within AC-32883 and looks useless as in this 'else' branch double update is impossible.
						if (itemsite.PendingStdCostReset == false)
						{
							updateParams.Add(new PXDataFieldRestrict("PendingStdCostDate", PXDbType.DateTime, 4, itemsite.PendingStdCostDate, PXComp.LE));
						}

						PXDatabase.Update<INItemSite>(updateParams.ToArray());
					}

					if (je.adjustment.Current == null)
					{
						je.adjustment.Cache.Insert();
					}

					foreach (
						PXResult<INCostStatus, InventoryItem, INSite, INPostClass, INItemSite> res in
							incoststatus.View.SelectMultiBound(new object[] { je.adjustment.Current, itemsite }))
					{
						INCostStatus layer = res;
						INSite site = res;
						INTran tran = new INTran();

						try
						{
							ValidateProjectLocation(site, res);
						}
						catch (PXException ex)
						{
							PXProcessing<INUpdateStdCostRecord>.SetError(ex.Message);
							continue;
						}

						if (((INCostStatus)res).LayerType == INLayerType.Oversold)
						{
							tran.TranType = INTranType.NegativeCostAdjustment;
						}
						else
						{
							tran.TranType = INTranType.StandardCostAdjustment;
						}
						tran.BranchID = site.BranchID;
						tran.InvtAcctID = layer.AccountID;
						tran.InvtSubID = layer.SubID;
						tran.AcctID = INReleaseProcess.GetAccountDefaults<INPostClass.stdCstRevAcctID>
							(je, (InventoryItem)res, (INSite)res, (INPostClass)res);
						tran.SubID = INReleaseProcess.GetAccountDefaults<INPostClass.stdCstRevSubID>
							(je, (InventoryItem)res, (INSite)res, (INPostClass)res);

						tran.InventoryID = layer.InventoryID;
						tran.SubItemID = layer.CostSubItemID;
						tran.SiteID = layer.CostSiteID;
						tran.Qty = 0m;
						tran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(layer.QtyOnHand * itemsite.PendingStdCost)) -
														layer.TotalCost;

						if (tran.TranCost != 0m)
						je.transactions.Insert(tran);
					}

					if (je.transactions.Select().Count == 0)
					{
						je.Clear();
					}
					else
					{
						je.Save.Press();
						PXProcessing<INUpdateStdCostRecord>.SetInfo(PXMessages.LocalizeFormatNoPrefixNLA(Messages.AdjstmentCreated,je.adjustment.Current.RefNbr));
					}
					ts.Complete();
				}
			}
		}

		public virtual void RevalueInventory(INUpdateStdCostRecord itemsite)
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					if (je.adjustment.Current == null)
					{
						je.adjustment.Cache.Insert();
					}

					foreach (
						PXResult<INCostStatus, InventoryItem, INSite, INPostClass, INItemSite> res in
							incoststatus.View.SelectMultiBound(new object[] { itemsite }))
					{
						INCostStatus layer = res;
						INSite site = res;
						INTran tran = new INTran();

						try
						{
							ValidateProjectLocation(site, res);
						}
						catch (PXException ex)
						{
							PXProcessing<INUpdateStdCostRecord>.SetError(ex.Message);
							continue;
						}

						if (((INCostStatus)res).LayerType == INLayerType.Oversold)
						{
							tran.TranType = INTranType.NegativeCostAdjustment;
						}
						else
						{
							tran.TranType = INTranType.StandardCostAdjustment;
						}
						tran.BranchID = site.BranchID;
						tran.InvtAcctID = layer.AccountID;
						tran.InvtSubID = layer.SubID;
						tran.AcctID = INReleaseProcess.GetAccountDefaults<INPostClass.stdCstRevAcctID>
							(je, (InventoryItem)res, (INSite)res, (INPostClass)res);
						tran.SubID = INReleaseProcess.GetAccountDefaults<INPostClass.stdCstRevSubID>
							(je, (InventoryItem)res, (INSite)res, (INPostClass)res);

						tran.InventoryID = layer.InventoryID;
						tran.SubItemID = layer.CostSubItemID;
						tran.SiteID = layer.CostSiteID;
						tran.Qty = 0m;
						tran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(layer.QtyOnHand * itemsite.StdCost)) -
														layer.TotalCost;

						if (tran.TranCost != 0m)
						je.transactions.Insert(tran);
					}

					if (je.transactions.Select().Count == 0)
					{
						je.Clear();
					}
					else
					{
						je.Save.Press();
						PXProcessing<INUpdateStdCostRecord>.SetInfo(PXMessages.LocalizeFormatNoPrefixNLA(Messages.AdjstmentCreated,je.adjustment.Current.RefNbr));
					}

					ts.Complete();
				}
			}			
		}


		/// <summary>
		/// Validates that there are no Quantities on hand on any Project Locations for the given Warehouse.
		/// </summary>
		/// <param name="site">Warehouse</param>
		public virtual void ValidateProjectLocation(INSite site, InventoryItem item)
		{
			var select = new PXSelectJoin<INLocationStatus,
				InnerJoin<INLocation, On<INLocationStatus.FK.Location>>,
				Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
				And<INLocation.isCosted, Equal<True>,
				And<INLocation.taskID, IsNotNull,
				And<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>,
				And<INLocationStatus.qtyOnHand, NotEqual<decimal0>>>>>>>(this);

			var res = select.SelectWindowed(0, 1, site.SiteID, item.InventoryID).ToList();
			if (res.Count == 1)
			{
				INLocation loc = (PXResult<INLocationStatus, INLocation>)res.First();

				throw new PXException(Messages.StandardCostItemOnProjectLocation, loc.LocationCD);
			}

		}

		public virtual void ReleaseAdjustment()
		{
			if (je.adjustment.Current != null)
			{
				List<INRegister> adjList = new List<INRegister>();
				adjList.Add(je.adjustment.Current);
				INDocumentRelease.ReleaseDoc(adjList, false);
			}
		}
	}
}
