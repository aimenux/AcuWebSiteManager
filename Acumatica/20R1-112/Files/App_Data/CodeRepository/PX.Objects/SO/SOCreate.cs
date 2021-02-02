using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.TM;
using PX.Objects.PO;
using System.Collections;
using PX.Objects.AR.MigrationMode;
using PX.Common;

namespace PX.Objects.SO
{
	[PX.Objects.GL.TableAndChartDashboardType]
    [Serializable]
	public class SOCreate : PXGraph<SOCreate>
	{
		public PXCancel<SOCreateFilter> Cancel;
		public PXAction<SOCreateFilter> viewDocument;
		public PXFilter<SOCreateFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessingJoin<SOFixedDemand, SOCreateFilter,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<SOFixedDemand.inventoryID>>,			
			LeftJoin<SOOrder, On<SOOrder.noteID, Equal<SOFixedDemand.refNoteID>>,
			LeftJoin<SOLineSplit, On<SOLineSplit.planID, Equal<SOFixedDemand.planID>>,
			LeftJoin<INItemClass, 
				On<InventoryItem.FK.ItemClass>>>>>,
			Where2<Where<SOFixedDemand.inventoryID, Equal<Current<SOCreateFilter.inventoryID>>, Or<Current<SOCreateFilter.inventoryID>, IsNull>>,
			And2<Where<SOFixedDemand.demandSiteID, Equal<Current<SOCreateFilter.siteID>>, Or<Current<SOCreateFilter.siteID>, IsNull>>,
			And2<Where<SOFixedDemand.sourceSiteID, Equal<Current<SOCreateFilter.sourceSiteID>>, Or<Current<SOCreateFilter.sourceSiteID>, IsNull>>,				
			And2<Where<SOOrder.customerID, Equal<Current<SOCreateFilter.customerID>>, Or<Current<SOCreateFilter.customerID>, IsNull>>,
			And2<Where<SOOrder.orderType, Equal<Current<SOCreateFilter.orderType>>, Or<Current<SOCreateFilter.orderType>, IsNull>>,
			And2<Where<SOOrder.orderNbr, Equal<Current<SOCreateFilter.orderNbr>>, Or<Current<SOCreateFilter.orderNbr>, IsNull>>,
			And<Where<INItemClass.itemClassCD, Like<Current<SOCreateFilter.itemClassCDWildcard>>, Or<Current<SOCreateFilter.itemClassCDWildcard>, IsNull>>>>>>>>>,
			OrderBy<Asc<SOFixedDemand.inventoryID>>> FixedDemand;

		public SOCreate()
        {
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

            PXUIFieldAttribute.SetEnabled<SOFixedDemand.sourceSiteID>(FixedDemand.Cache, null, true);

			PXUIFieldAttribute.SetDisplayName<InventoryItem.descr>(this.Caches[typeof(InventoryItem)], PO.Messages.InventoryItemDescr);
			PXUIFieldAttribute.SetDisplayName<INSite.descr>(this.Caches[typeof(INSite)], Messages.SiteDescr);
			PXUIFieldAttribute.SetDisplayName<INPlanType.descr>(this.Caches[typeof(INPlanType)], PO.Messages.PlanTypeDescr);

            /*commented out for future merge conflicts
             *  And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID> - condition must be left in the code after merge!
             *  
             *   if (PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
            {
                INSite toxicsite = PXSelect<INSite,
                    Where<INSite.active, Equal<True>,
                    And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
                    And<Where<INSite.addressID, IsNull, Or<INSite.contactID, IsNull>>>>>>.SelectWindowed(this, 0, 1);
                ...
		    } */
        }

        protected IEnumerable filter()
		{
			SOCreateFilter filter = this.Filter.Current;
			filter.OrderVolume = 0;
			filter.OrderWeight = 0;
			foreach (SOFixedDemand demand in this.FixedDemand.Cache.Updated)
				if (demand.Selected == true)
				{
					filter.OrderVolume += demand.ExtVolume ?? 0m;
					filter.OrderWeight += demand.ExtWeight ?? 0m;
				}
			yield return filter;
		}

		protected virtual void SOCreateFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOCreateFilter filter = Filter.Current;

			FixedDemand.SetProcessDelegate(list => SOCreateProc(list, filter.PurchDate));

			TimeSpan span;
			Exception message;
			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out span, out message);

			PXUIFieldAttribute.SetVisible<SOLine.orderNbr>(Caches[typeof(SOLine)], null, (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted));

            if (PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
            {
                INSite toxicsite = PXSelectReadonly<INSite,
                Where<INSite.siteID, Equal<Current<SOCreateFilter.siteID>>, 
                And<INSite.active, Equal<True>, 
                And<Where<INSite.addressID, IsNull, Or<INSite.contactID, IsNull>>>>>>.SelectSingleBound(this, new object[] { e.Row });

                if (toxicsite != null)
                    throw new PXSetupNotEnteredException<INSite, INSite.siteCD>(Messages.WarehouseWithoutAddressAndContact, toxicsite.SiteCD, toxicsite.SiteCD);
		}
        }

		//TODO:refactor
		public static void SOCreateProc(List<SOFixedDemand> list, DateTime? PurchDate)
		{
			SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
			SOSetup sosetup = docgraph.sosetup.Current;
			DocumentList<SOOrder> created = new DocumentList<SOOrder>(docgraph);

            docgraph.ExceptionHandling.AddHandler<SOLineSplit.qty>((cache, e) => { e.Cancel = true; });
            docgraph.ExceptionHandling.AddHandler<SOLineSplit.isAllocated>((cache, e) => { ((SOLineSplit)e.Row).IsAllocated = true; e.Cancel = true; });

			foreach (SOFixedDemand demand in list)
			{
				string OrderType = 
					sosetup.TransferOrderType ?? SOOrderTypeConstants.TransferOrder;
				
				string demandPlanType = demand.PlanType;

				try
				{
					if (demand.SourceSiteID == null)
					{
						PXProcessing<SOFixedDemand>.SetWarning(list.IndexOf(demand), Messages.MissingSourceSite);
						continue;
					}

					SOOrder order;					
					SOLineSplit2 sosplit = PXSelect<SOLineSplit2, Where<SOLineSplit2.planID, Equal<Required<SOLineSplit2.planID>>>>.Select(docgraph, demand.PlanID);

					if (sosplit != null)
					{
						order = created.Find<SOOrder.orderType, SOOrder.destinationSiteID, SOOrder.defaultSiteID>(OrderType, sosplit.ToSiteID, sosplit.SiteID);
					}
					else
					{
                        if (demand.SourceSiteID == demand.SiteID)
                        {
                            PXProcessing<SOFixedDemand>.SetWarning(list.IndexOf(demand), Messages.EqualSourceDestinationSite);
                            continue;
                        }

						order = created.Find<SOOrder.orderType, SOOrder.destinationSiteID, SOOrder.defaultSiteID>(OrderType, demand.SiteID, demand.SourceSiteID);
					}

					if(order == null) order = new SOOrder();
					
					if (order.OrderNbr == null)
					{
						docgraph.Clear();

						if (sosplit != null)
						{
							INSite sourceSite = INSite.PK.Find(docgraph, sosplit.SiteID);
							order.BranchID = sourceSite.BranchID;
							order.OrderType = OrderType;
							order = PXCache<SOOrder>.CreateCopy(docgraph.Document.Insert(order));
							order.DefaultSiteID = sosplit.SiteID;
							order.DestinationSiteID = sosplit.ToSiteID;
							order.OrderDate = PurchDate;

							docgraph.Document.Update(order);
						}
						else
						{
							INSite sourceSite = INSite.PK.Find(docgraph, demand.SourceSiteID);
							order.BranchID = sourceSite.BranchID;
							order.OrderType = OrderType;
							order = PXCache<SOOrder>.CreateCopy(docgraph.Document.Insert(order));
							order.DefaultSiteID = demand.SourceSiteID;
							order.DestinationSiteID = demand.SiteID;
							order.OrderDate = PurchDate;

							docgraph.Document.Update(order);
						}
					}
					else if (docgraph.Document.Cache.ObjectsEqual(docgraph.Document.Current, order) == false)
					{
						docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
					}

					SOLine newline;
					SOLineSplit newsplit;
					PXCache cache = docgraph.Caches[typeof(INItemPlan)];
					INItemPlan rp;

					if (sosplit != null)
					{
						docgraph.Transactions.Current = newline = docgraph.Transactions.Search<SOLine.inventoryID, SOLine.subItemID, SOLine.siteID, SOLine.pOCreate>(demand.InventoryID, demand.SubItemID, demand.SiteID, false);
						if (newline == null)
						{
							newline = PXCache<SOLine>.CreateCopy(docgraph.Transactions.Insert());
							newline.InventoryID = demand.InventoryID;
							newline.SubItemID = demand.SubItemID;
							newline.SiteID = demand.SiteID;
							newline.UOM = demand.UOM;
							newline.OrderQty = 0m;

							newline = docgraph.Transactions.Update(newline);
						}

						newsplit = new SOLineSplit();
                        newsplit.LotSerialNbr = sosplit.LotSerialNbr;
						newsplit.IsAllocated = true;
                        newsplit.IsMergeable = false;
                        newsplit.SiteID = demand.SiteID; //SiteID should be explicitly set because of PXFormula
						newsplit.Qty = demand.OrderQty;
                        newsplit.RefNoteID = demand.RefNoteID;

						//we have to delete previous allocation and reinsert it after the newsplit for transfer-order allocation to work properly
						rp = PXCache<INItemPlan>.CreateCopy(demand);
						cache.RaiseRowDeleted(demand);

						newsplit = docgraph.splits.Insert(newsplit);

                        sosplit.SOOrderType = newsplit.OrderType;
                        sosplit.SOOrderNbr = newsplit.OrderNbr;
                        sosplit.SOLineNbr = newsplit.LineNbr;
                        sosplit.SOSplitLineNbr = newsplit.SplitLineNbr;
                        docgraph.sodemand.Update(sosplit);

						rp.SiteID = sosplit.ToSiteID;
						rp.PlanType =INPlanConstants.Plan93;
						rp.FixedSource = null;
						rp.SupplyPlanID = newsplit.PlanID;
						cache.RaiseRowInserted(rp);
						cache.SetStatus(rp, PXEntryStatus.Updated);
					}
					else
					{
						docgraph.Transactions.Current = newline = docgraph.Transactions.Search<SOLine.inventoryID, SOLine.subItemID, SOLine.siteID, SOLine.pOCreate>(demand.InventoryID, demand.SubItemID, demand.SourceSiteID, demand.VendorID != null);
						if (newline == null)
						{
							newline = PXCache<SOLine>.CreateCopy(docgraph.Transactions.Insert());
							newline.InventoryID = demand.InventoryID;
							newline.SubItemID = demand.SubItemID;
							newline.SiteID = demand.SourceSiteID;
							newline.UOM = demand.UOM;
							newline.OrderQty = 0m;
                            //newline.POCreate = (demand.VendorID != null);
                            newline.POCreate = (demand.FixedSource == INReplenishmentSource.TransferToPurchase);
                            newline.VendorID = demand.VendorID;
                            //newline.VendorLocationID = demand.VendorLocationID;

							newline = docgraph.Transactions.Update(newline);
						}

						newsplit = new SOLineSplit();
						newsplit.IsAllocated = false;
						newsplit.Qty = demand.OrderQty;
                        //newsplit.POCreate = (demand.VendorID != null);
                        newsplit.POCreate = (demand.FixedSource == INReplenishmentSource.TransferToPurchase);
                        newsplit.VendorID = demand.VendorID;
                        //newsplit.VendorLocationID = demand.VendorLocationID;
						newsplit = docgraph.splits.Insert(newsplit) ?? docgraph.splits.Current;

						rp = PXCache<INItemPlan>.CreateCopy(demand);
						cache.RaiseRowDeleted(demand);
						rp.SiteID = demand.SiteID;
						rp.PlanType = INPlanConstants.Plan94;
						rp.FixedSource = null;
						rp.SupplyPlanID = newsplit.PlanID;
						cache.RaiseRowInserted(rp);
						cache.SetStatus(rp, PXEntryStatus.Updated);

					}

					if (newsplit.PlanID == null)
						throw new PXRowPersistedException(typeof(SOLine).Name, newline, RQ.Messages.UnableToCreateSOOrders);			

					if (docgraph.Transactions.Cache.IsInsertedUpdatedDeleted)
					{
						using (PXTransactionScope scope = new PXTransactionScope())
						{
							docgraph.Save.Press();
							if (demandPlanType == INPlanConstants.Plan90)
							{
								docgraph.Replenihment.Current = docgraph.Replenihment.Search<INReplenishmentOrder.noteID>(demand.RefNoteID);
								if (docgraph.Replenihment.Current != null)
								{
									INReplenishmentLine rLine =
										PXCache<INReplenishmentLine>.CreateCopy(docgraph.ReplenishmentLines.Insert(new INReplenishmentLine()));
									rLine.InventoryID = newsplit.InventoryID;
									rLine.SubItemID = newsplit.SubItemID;
									rLine.UOM = newsplit.UOM;
									rLine.Qty = newsplit.Qty;
									rLine.SOType = newsplit.OrderType;
									rLine.SONbr = docgraph.Document.Current.OrderNbr;
									rLine.SOLineNbr = newsplit.LineNbr;
                                    rLine.SOSplitLineNbr = newsplit.SplitLineNbr;
									rLine.SiteID = demand.SourceSiteID;
									rLine.DestinationSiteID = demand.SiteID;
									rLine.PlanID = demand.PlanID;
									docgraph.ReplenishmentLines.Update(rLine);
									INItemPlan plan = PXSelect<INItemPlan,
										Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.SelectWindowed(docgraph, 0, 1,
										                                                                             demand.SupplyPlanID);
									if (plan != null)
									{									
										//plan.SupplyPlanID = rp.PlanID;
										rp.SupplyPlanID = plan.PlanID;
										cache.SetStatus(rp, PXEntryStatus.Updated);
									}

									docgraph.Save.Press();
								}
							}
							scope.Complete();
						}
						
						PXProcessing<SOFixedDemand>.SetInfo(list.IndexOf(demand), PXMessages.LocalizeFormatNoPrefixNLA(Messages.TransferOrderCreated, docgraph.Document.Current.OrderNbr));						
						
						if (created.Find(docgraph.Document.Current) == null)
						{
							created.Add(docgraph.Document.Current);
						}
					}
				}
				catch (Exception e)
				{
					PXProcessing<SOFixedDemand>.SetError(list.IndexOf(demand), e);
				}
			}
			if (created.Count == 1)
			{
				using (new PXTimeStampScope(null))
				{
					docgraph.Clear();
					docgraph.Document.Current = docgraph.Document.Search<POOrder.orderNbr>(created[0].OrderNbr, created[0].OrderType);
					throw new PXRedirectRequiredException(docgraph, Messages.SOOrder);
				}
			}
		}
		
		public PXAction<SOCreateFilter> inventorySummary;
		[PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(VisibleOnProcessingResults = true)]
		public virtual System.Collections.IEnumerable InventorySummary(PXAdapter adapter)
		{
			PXCache tCache = FixedDemand.Cache;
			SOFixedDemand line = FixedDemand.Current;
			if (line == null) return adapter.Get();

			InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
			if (item != null && item.StkItem == true)
			{
				INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<SOFixedDemand.subItemID>(tCache, line);
				InventorySummaryEnq.Redirect(item.InventoryID,
											 ((sbitem != null) ? sbitem.SubItemCD : null),
											 line.SiteID,
											 line.LocationID);
			}
			return adapter.Get();
		}

		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual System.Collections.IEnumerable ViewDocument(PXAdapter adapter)
		{
			PXCache tCache = FixedDemand.Cache;
			SOFixedDemand line = FixedDemand.Current;
			if (line == null) return adapter.Get();

			SOOrderEntry graph = PXGraph.CreateInstance<SOOrderEntry>();
			graph.Document.Current = graph.Document.Search<SOOrder.orderNbr>(line.OrderNbr, line.OrderType);
			throw new PXRedirectRequiredException(graph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		[Serializable]
		public partial class SOCreateFilter : IBqlTable
		{
			#region CurrentOwnerID
			public abstract class currentOwnerID : PX.Data.BQL.BqlGuid.Field<currentOwnerID> { }

			[PXDBGuid]
			[CR.CRCurrentOwnerID]
			public virtual Guid? CurrentOwnerID { get; set; }
			#endregion
			#region MyOwner
			public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
			protected Boolean? _MyOwner;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Me")]
			public virtual Boolean? MyOwner
			{
				get
				{
					return _MyOwner;
				}
				set
				{
					_MyOwner = value;
				}
			}
			#endregion
			#region OwnerID
			public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
			protected Guid? _OwnerID;
			[PXDBGuid]
			[PXUIField(DisplayName = "Product Manager")]
			[PX.TM.PXSubordinateOwnerSelector]
			public virtual Guid? OwnerID
			{
				get
				{
					return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
				}
				set
				{
					_OwnerID = value;
				}
			}
			#endregion
			#region WorkGroupID
			public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
			protected Int32? _WorkGroupID;
			[PXDBInt]
			[PXUIField(DisplayName = "Product  Workgroup")]
			[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
				Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
			 SubstituteKey = typeof(EPCompanyTree.description))]
			public virtual Int32? WorkGroupID
			{
				get
				{
					return (_MyWorkGroup == true) ? null : _WorkGroupID;
				}
				set
				{
					_WorkGroupID = value;
				}
			}
			#endregion
			#region MyWorkGroup
			public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
			protected Boolean? _MyWorkGroup;
			[PXDefault(false)]
			[PXDBBool]
			[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? MyWorkGroup
			{
				get
				{
					return _MyWorkGroup;
				}
				set
				{
					_MyWorkGroup = value;
				}
			}
			#endregion
			#region FilterSet
			public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
			[PXDefault(false)]
			[PXDBBool]
            public virtual Boolean? FilterSet
			{
				get
				{
					return
						this.OwnerID != null ||
						this.WorkGroupID != null ||
						this.MyWorkGroup == true;
				}
			}
			#endregion
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[VendorNonEmployeeActive]
			public virtual Int32? VendorID
			{
				get
				{
					return this._VendorID;
				}
				set
				{
					this._VendorID = value;
				}
			}
			#endregion
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			protected Int32? _SiteID;
			[IN.Site(DisplayName = " To Warehouse")]
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
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			protected DateTime? _EndDate;
			[PXDBDate]
			[PXUIField(DisplayName = "Date Promised")]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
			#region PurchDate
			public abstract class purchDate : PX.Data.BQL.BqlDateTime.Field<purchDate> { }
			protected DateTime? _PurchDate;
			[PXDBDate]
			[PXUIField(DisplayName = "Creation Date")]
			[PXDefault(typeof(AccessInfo.businessDate))]
			public virtual DateTime? PurchDate
			{
				get
				{
					return this._PurchDate;
				}
				set
				{
					this._PurchDate = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[Customer]
			public virtual Int32? CustomerID
			{
				get
				{
					return this._CustomerID;
				}
				set
				{
					this._CustomerID = value;
				}
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[StockItem]
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
			#region ItemClassCD
			public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
			protected string _ItemClassCD;

			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassCD, Where<INItemClass.stkItem, Equal<boolTrue>>>), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
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
            #region SourceSiteID
            public abstract class sourceSiteID : PX.Data.BQL.BqlInt.Field<sourceSiteID> { }
            protected Int32? _SourceSiteID;
            [IN.Site(DisplayName = "From Warehouse", DescriptionField = typeof(INSite.descr))]
            public virtual Int32? SourceSiteID
            {
                get
                {
                    return this._SourceSiteID;
                }
                set
                {
                    this._SourceSiteID = value;
                }
            }
            #endregion
            #region OrderType
            public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
            protected String _OrderType;
            [PXDBString(2, IsFixed = true, InputMask = ">aa")]
            [PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<boolTrue>>>))]
            [PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String OrderType
            {
                get
                {
                    return this._OrderType;
                }
                set
                {
                    this._OrderType = value;
                }
            }
            #endregion
            #region OrderNbr
            public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
            protected String _OrderNbr;
            [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
            [SO.RefNbr(typeof(Search2<SOOrder.orderNbr,
				LeftJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>,
                        And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
                Where<SOOrder.orderType, Equal<Optional<SOCreateFilter.orderType>>,
                And<Where<SOOrder.orderType, Equal<SOOrderTypeConstants.transferOrder>,
                 Or<Customer.bAccountID, IsNotNull>>>>,
                 OrderBy<Desc<SOOrder.orderNbr>>>))]
            [PXFormula(typeof(Default<SOCreateFilter.orderType>))]
            public virtual String OrderNbr
            {
                get
                {
                    return this._OrderNbr;
                }
                set
                {
                    this._OrderNbr = value;
                }
            }
            #endregion
			#region OrderWeight
			public abstract class orderWeight : PX.Data.BQL.BqlDecimal.Field<orderWeight> { }
			protected Decimal? _OrderWeight;
			[PXDBDecimal(6)]
			[PXUIField(DisplayName = "Weight", Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXFormula(null, typeof(SumCalc<SOFixedDemand.extWeight>))]
			public virtual Decimal? OrderWeight
			{
				get
				{
					return this._OrderWeight;
				}
				set
				{
					this._OrderWeight = value;
				}
			}
			#endregion
			#region OrderVolume
			public abstract class orderVolume : PX.Data.BQL.BqlDecimal.Field<orderVolume> { }
			protected Decimal? _OrderVolume;
			[PXDBDecimal(6)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Volume", Enabled = false)]
			[PXFormula(null, typeof(SumCalc<SOFixedDemand.extVolume>))]
			public virtual Decimal? OrderVolume
			{
				get
				{
					return this._OrderVolume;
				}
				set
				{
					this._OrderVolume = value;
				}
			}
			#endregion
		}

		/// <summary>
		/// Returns records that are displayed in SOCreate screen. 
		/// Please refer to SOCreate screen documentation for details. 
		/// </summary>
		public class SOCreateProjectionAttribute : TM.OwnedFilter.ProjectionAttribute
		{
			public SOCreateProjectionAttribute()
				: base(typeof(SOCreateFilter),
				BqlCommand.Compose(
			typeof(Select2<,,>),
				typeof(INItemPlan),
				typeof(InnerJoin<INPlanType,
											On<INItemPlan.FK.PlanType>,
				InnerJoin<InventoryItem, On<INItemPlan.FK.InventoryItem>,
				InnerJoin<INUnit, 
				       On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>, 
				      And<INUnit.fromUnit, Equal<InventoryItem.purchaseUnit>, 
				      And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>,
                LeftJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>,
                LeftJoin<IN.S.INItemSite, On<IN.S.INItemSite.inventoryID, Equal<INItemPlan.inventoryID>, And<IN.S.INItemSite.siteID, Equal<INItemPlan.siteID>>>>>>>>),
			typeof(Where2<,>),
			typeof(Where<INItemPlan.hold, Equal<boolFalse>,
							And2<Where<INItemPlan.fixedSource, Equal<INReplenishmentSource.transfer>, Or<INItemPlan.fixedSource, Equal<INReplenishmentSource.transferToPurchase>>>,	
							And<INItemPlan.supplyPlanID, IsNull>>>),
			typeof(And<>),
			TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
			typeof(SOCreateFilter),
			typeof(INItemSite.productWorkgroupID),
			typeof(INItemSite.productManagerID))))
			{
			}
		}

		/*[Serializable]
		[PXProjection(typeof(Select2<INItemPlan,
			InnerJoin<INPlanType, On<INPlanType.planType, Equal<INItemPlan.planType>, And<INPlanType.isFixed, Equal<boolTrue>, And<INPlanType.isDemand, Equal<boolTrue>>>>,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemPlan.inventoryID>>,
			InnerJoin<INUnit, On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>, And<INUnit.fromUnit, Equal<InventoryItem.purchaseUnit>, And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>>>>,
			Where<INItemPlan.supplyPlanID, IsNull, And<INItemPlan.hold, Equal<boolFalse>>>>))]*/
		[SOCreateProjectionAttribute]
        [Serializable]
		public partial class SOFixedDemand : INItemPlan
		{
			#region Selected
			public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region SiteID
			public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region PlanDate
			public new abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }
			[PXDBDate]
			[PXDefault]
			[PXUIField(DisplayName = "Requested On")]
			public override DateTime? PlanDate
			{
				get
				{
					return this._PlanDate;
				}
				set
				{
					this._PlanDate = value;
				}
			}
			#endregion
            #region FixedSource
            public new abstract class fixedSource : PX.Data.BQL.BqlString.Field<fixedSource> { }
            [PXDBString(1, IsFixed = true)]
            [PXUIField(DisplayName = "Fixed Source", Enabled = false)]
            [PXDefault(INReplenishmentSource.Purchased, PersistingCheck = PXPersistingCheck.Nothing)]
            [INReplenishmentSource.INPlanList]
            public override String FixedSource
            {
                get
                {
                    return this._FixedSource;
                }
                set
                {
                    this._FixedSource = value;
                }
            }
            #endregion
			#region PlanID
			public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
			#endregion
			#region PlanType
			public new abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
			[PXDBString(2, IsFixed = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Plan Type")]
			[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true, DescriptionField = typeof(INPlanType.descr))]
			public override String PlanType
			{
				get
				{
					return this._PlanType;
				}
				set
				{
					this._PlanType = value;
				}
			}
			#endregion
            #region DemandSiteID
            public abstract class demandSiteID : PX.Data.BQL.BqlInt.Field<demandSiteID> { }
            protected Int32? _DemandSiteID;
            [PXDBCalced(typeof(IsNull<SOLineSplit.toSiteID, INItemPlan.siteID>),typeof(int))]
            [PXUIField(DisplayName = "To Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
            [PXDimensionSelector(SiteAttribute.DimensionName, typeof(Search<INSite.siteID>), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr), CacheGlobal=true)]
            public virtual Int32? DemandSiteID
            {
                get
                {
                    return this._DemandSiteID;
                }
                set
                {
                    this._DemandSiteID = value;
                }
            }
            #endregion
			#region SourceSiteID
			public new abstract class sourceSiteID : PX.Data.BQL.BqlInt.Field<sourceSiteID> { }
			[IN.SiteAvail(typeof(SOFixedDemand.inventoryID), typeof(SOFixedDemand.subItemID),new Type[] { typeof(INSite.siteCD), typeof(INSiteStatus.qtyOnHand), typeof(INSite.descr), typeof(INSite.replenishmentClassID) }, DisplayName = "From Warehouse", DescriptionField = typeof(INSite.descr), BqlField = typeof(INItemPlan.sourceSiteID))]
			public override Int32? SourceSiteID
			{
				get
				{
					return this._SourceSiteID;
				}
				set
				{
					this._SourceSiteID = value;
				}
			}
			#endregion
			#region SubItemID
			public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			#endregion
			#region LocationID
			public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			#endregion
			#region LotSerialNbr
			public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
			#endregion
			#region SupplyPlanID
			public new abstract class supplyPlanID : PX.Data.BQL.BqlLong.Field<supplyPlanID> { }
			#endregion
			#region PlanQty
			public new abstract class planQty : PX.Data.BQL.BqlDecimal.Field<planQty> { }
			[PXDBQuantity]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Requested Qty.")]
			public override Decimal? PlanQty
			{
				get
				{
					return this._PlanQty;
				}
				set
				{
					this._PlanQty = value;
				}
			}
			#endregion
			#region UOM
			public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
			protected String _UOM;
			[PXDBString(BqlField = typeof(INUnit.fromUnit))]
			[PXUIField(DisplayName = "UOM")]
			public virtual String UOM
			{
				get
				{
					return this._UOM;
				}
				set
				{
					this._UOM = value;
				}
			}
			#endregion
			#region UnitMultDiv
			public abstract class unitMultDiv : PX.Data.BQL.BqlString.Field<unitMultDiv> { }
			protected String _UnitMultDiv;
			[PXDBString(1, IsFixed = true, BqlField = typeof(INUnit.unitMultDiv))]
			public virtual String UnitMultDiv
			{
				get
				{
					return this._UnitMultDiv;
				}
				set
				{
					this._UnitMultDiv = value;
				}
			}
			#endregion
			#region UnitRate
			public abstract class unitRate : PX.Data.BQL.BqlDecimal.Field<unitRate> { }
			protected Decimal? _UnitRate;
			[PXDBDecimal(6, BqlField = typeof(INUnit.unitRate))]
			public virtual Decimal? UnitRate
			{
				get
				{
					return this._UnitRate;
				}
				set
				{
					this._UnitRate = value;
				}
			}
			#endregion
			#region OrderQty
			public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
			protected Decimal? _OrderQty;
			[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>, Mult<INItemPlan.planQty, INUnit.unitRate>>, Div<INItemPlan.planQty, INUnit.unitRate>>), typeof(decimal))]
			[PXQuantity]
			[PXUIField(DisplayName = "Quantity")]
			public virtual Decimal? OrderQty
			{
				get
				{
					return this._OrderQty;
				}
				set
				{
					this._OrderQty = value;
				}
			}
			#endregion
			#region RefNoteID
			public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
			[PXRefNote]
			[PXUIField(DisplayName = "Reference Nbr.")]
			public override Guid? RefNoteID
			{
				get
				{
					return this._RefNoteID;
				}
				set
				{
					this._RefNoteID = value;
				}
			}
			#endregion
			#region Hold
			public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
			#endregion
			#region VendorID_Vendor_acctName
			public abstract class vendorID_Vendor_acctName : PX.Data.BQL.BqlString.Field<vendorID_Vendor_acctName> { }
			#endregion
			#region InventoryID_InventoryItem_descr
			public abstract class inventoryID_InventoryItem_descr : PX.Data.BQL.BqlString.Field<inventoryID_InventoryItem_descr> { }
			#endregion
			#region SiteID_INSite_descr
			public abstract class siteID_INSite_descr : PX.Data.BQL.BqlString.Field<siteID_INSite_descr> { }
			#endregion		
			#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			protected String _OrderType;
			[PXDBString(2, IsFixed = true, BqlField = typeof(SOLineSplit.orderType))]
			[PXUIField(DisplayName = "Order Type", Enabled = false)]
			public virtual String OrderType
			{
				get
				{
					return this._OrderType;
				}
				set
				{
					this._OrderType = value;
				}
			}
			#endregion
			#region OrderNbr
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			protected String _OrderNbr;
			[PXDBString(15, BqlField = typeof(SOLineSplit.orderNbr), IsUnicode = true)]
			[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
			public virtual String OrderNbr
			{
				get
				{
					return this._OrderNbr;
				}
				set
				{
					this._OrderNbr = value;
				}
			}
			#endregion
			#region ExtWeight
			public abstract class extWeight : PX.Data.BQL.BqlDecimal.Field<extWeight> { }
			protected Decimal? _ExtWeight;
			[PXDecimal(6)]
			[PXUIField(DisplayName = "Weight")]
			[PXFormula(typeof(Mult<SOFixedDemand.orderQty, Selector<SOFixedDemand.inventoryID, InventoryItem.baseWeight>>))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? ExtWeight
			{
				get
				{
					return this._ExtWeight;
				}
				set
				{
					this._ExtWeight = value;
				}
			}
			#endregion
			#region ExtVolume
			public abstract class extVolume : PX.Data.BQL.BqlDecimal.Field<extVolume> { }
			protected Decimal? _ExtVolume;
			[PXDecimal(6)]
			[PXUIField(DisplayName = "Volume")]
			[PXFormula(typeof(Mult<SOFixedDemand.orderQty, Selector<SOFixedDemand.inventoryID, InventoryItem.baseVolume>>))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? ExtVolume
			{
				get
				{
					return this._ExtVolume;
				}
				set
				{
					this._ExtVolume = value;
				}
			}
			#endregion	
		}
	}

}
