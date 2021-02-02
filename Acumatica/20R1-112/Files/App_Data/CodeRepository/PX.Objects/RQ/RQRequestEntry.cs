using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.PO;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.CR;
using System.Collections;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.TM;
using PX.Objects.Common;
using PX.Objects.Common.Bql;

namespace PX.Objects.RQ
{
	public class RQRequestEntry : PXGraph<RQRequestEntry, RQRequest>, IGraphWithInitialization
	{
		[PXViewName(Messages.RQRequest)]
		public PXSelectJoin<RQRequest,
			LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<RQRequest.employeeID>>>,
			Where<Customer.bAccountID, IsNull,
								Or<Match<Customer, Current<AccessInfo.userName>>>>> Document;

		public PXSelect<RQRequest, Where<RQRequest.orderNbr, Equal<Current<RQRequest.orderNbr>>>> CurrentDocument;
		public PXSelect<InventoryItem> invItems;
		public PXSelect<RQRequestLine, Where<RQRequestLine.orderNbr, Equal<Optional<RQRequest.orderNbr>>>> Lines;
		[PXViewName(PO.Messages.POShipAddress)]
		public PXSelect<POShipAddress, Where<POShipAddress.addressID, Equal<Current<RQRequest.shipAddressID>>>> Shipping_Address;
		[PXViewName(PO.Messages.POShipContact)]
		public PXSelect<POShipContact, Where<POShipContact.contactID, Equal<Current<RQRequest.shipContactID>>>> Shipping_Contact;
		[PXViewName(PO.Messages.PORemitAddress)]
		public PXSelect<PORemitAddress, Where<PORemitAddress.addressID, Equal<Current<RQRequest.remitAddressID>>>> Remit_Address;
		[PXViewName(PO.Messages.PORemitContact)]
		public PXSelect<PORemitContact, Where<PORemitContact.contactID, Equal<Current<RQRequest.remitContactID>>>> Remit_Contact;
		public PXSelectJoin<RQRequisitionContent,
							InnerJoin<RQRequisitionLineReceived,
											On<RQRequisitionLineReceived.reqNbr, Equal<RQRequisitionContent.reqNbr>,
											And<RQRequisitionLineReceived.lineNbr, Equal<RQRequisitionContent.reqLineNbr>>>,
							InnerJoin<RQRequisition, On<RQRequisition.reqNbr, Equal<RQRequisitionContent.reqNbr>>>>,
					Where<RQRequisitionContent.orderNbr, Equal<Optional<RQRequestLine.orderNbr>>,
						And<RQRequisitionContent.lineNbr, Equal<Optional<RQRequestLine.lineNbr>>>>> Contents;

		public PXSetup<RQSetup> Setup;
		public PXSetup<GLSetup> GLSetup;
		public PXSetup<Company> company;
		public PXSetup<MasterFinPeriod, Where<MasterFinPeriod.finPeriodID, Equal<Current<RQRequest.finPeriodID>>>> finperiod;

		public CRActivityList<RQRequest> Activity;
		//Document approve
		public PXSelect<RQSetupApproval, Where<RQSetupApproval.type, Equal<RQType.requestItem>>> SetupApproval;
		[PXViewName(Messages.Approval)]
		public EPApprovalAutomation<RQRequest, RQRequest.approved, RQRequest.rejected, RQRequest.hold, RQSetupApproval> Approval;
		/*
		//Line approve
		public PXSelect<RQRequestLine, Where<RQRequestLine.orderNbr, Equal<Current<RQRequest.orderNbr>>>> LinesToApprove;
		public EPApprovalList<EPApproval, Where<EPApproval.refNoteID, Equal<Current<RQRequestLine.noteID>>>, RQRequestLine, Search<RQRequestLine.noteID>> LineApproval;
		*/
		public PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Optional<RQRequest.vendorID>>>> bAccount;	
		public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Optional<RQRequest.vendorID>>>> vendor;
		[PXViewName(EP.Messages.Employee)]
		public PXSetup<EPEmployee, Where<EPEmployee.bAccountID, Equal<Optional<RQRequest.employeeID>>>> employee;
		[PXViewName(RQ.Messages.Department)]
		public PXSetup<EPDepartment, Where<EPDepartment.departmentID, Equal<Optional<RQRequest.departmentID>>>> depa;
		public PXSetup<RQRequestClass, Where<RQRequestClass.reqClassID, Equal<Optional<RQRequest.reqClassID>>>> reqclass;
		public PXSelect<RQBudget> Budget;	
		public CMSetupSelect CMSetup;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<RQRequest.curyInfoID>>>> currencyinfo;
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<RQRequest.employeeID>>>> customer;
		public ToggleCurrency<RQRequest> CurrencyView;

		#region SiteStatus Lookup
		public PXFilter<POSiteStatusFilter> sitestatusfilter;
		[PXFilterable]
        [PXCopyPasteHiddenView]
		public RQSiteStatusLookup<RQSiteStatusSelected, INSiteStatusFilter> sitestatus;

		public PXAction<RQRequest> addInvBySite;
		[PXUIField(DisplayName = "Add Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AddInvBySite(PXAdapter adapter)
		{
			sitestatusfilter.Cache.Clear();
			if (sitestatus.AskExt() == WebDialogResult.OK)
			{
				return AddInvSelBySite(adapter);
			}
			sitestatusfilter.Cache.Clear();
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<RQRequest> addInvSelBySite;
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
		{
			foreach (RQSiteStatusSelected line in sitestatus.Cache.Cached)
			{
				if (line.Selected == true && line.QtySelected > 0)
				{
					RQRequestLine newline = new RQRequestLine();					
					newline.InventoryID = line.InventoryID;
					newline.SubItemID = line.SubItemID;
					newline.UOM = line.PurchaseUnit;
					newline.OrderQty = line.QtySelected;
					Lines.Insert(newline);
				}
			}
			sitestatus.Cache.Clear();
			return adapter.Get();
		}
		protected virtual void POSiteStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			POSiteStatusFilter row = (POSiteStatusFilter)e.Row;
			if (row != null && Document.Current != null)
			{
				PXUIFieldAttribute.SetEnabled<POSiteStatusFilter.onlyAvailable>(sitestatusfilter.Cache, row, Document.Current.VendorID != null);
				row.OnlyAvailable = Document.Current.VendorID != null;
				row.VendorID = Document.Current.VendorID;
			}
		}

		public PXAction<RQRequest> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
        [PXButton]
        public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (RQRequest current in adapter.Get<RQRequest>())
			{
				if (current != null)
				{
					PORemitAddress address = this.Remit_Address.Select();
					if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
					{
						PXAddressValidator.Validate<PORemitAddress>(this, address, true);
					}

					POShipAddress shipAddress = this.Shipping_Address.Select();
					if (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
					{
						PXAddressValidator.Validate<POShipAddress>(this, shipAddress, true);
					}
				}
				yield return current;
			}
		}

		#endregion

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<RQRequest.branchID>>>))]
		protected virtual void POSiteStatusFilter_SiteID_CacheAttached(PXCache sender)
		{
		}

		#region EPApproval Cahce Attached
		[PXDBDate()]
		[PXDefault(typeof(RQRequest.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(RQRequest.employeeID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid()]
		[PXDefault(typeof(RQRequest.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(RQRequest.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[CurrencyInfo(typeof(RQRequest.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(RQRequest.curyEstExtCostTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(RQRequest.estExtCostTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		#endregion

		public RQRequestEntry()
		{
			this.Contents.Cache.AllowInsert = false;
			this.Contents.Cache.AllowUpdate = false;
			this.Contents.Cache.AllowDelete = false;

			PXStringListAttribute.SetList<InventoryItem.itemType>(
					invItems.Cache, null,
					new string[] { INItemTypes.FinishedGood, INItemTypes.Component, INItemTypes.SubAssembly, INItemTypes.NonStockItem, INItemTypes.LaborItem, INItemTypes.ServiceItem, INItemTypes.ChargeItem, INItemTypes.ExpenseItem },
					new string[] { IN.Messages.FinishedGood, IN.Messages.Component, IN.Messages.SubAssembly, IN.Messages.NonStockItem, IN.Messages.LaborItem, IN.Messages.ServiceItem, IN.Messages.ChargeItem, IN.Messages.ExpenseItem }
					);
			this.Views.Caches.Remove(typeof (RQBudget));
			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null && InventoryHelper.CanCreateStockItem(sender.Graph) == false) e.NewValue = false; });
		}

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<RQRequest>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(RQRequestLine), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<RQRequestLine.orderNbr>(((RQRequestEntry)graph).Document.Current?.OrderNbr)
						};
					}));
			}
		}

		public PXAction<RQRequest> hold;
		[PXUIField(DisplayName = "Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Hold(PXAdapter adapter)
		{
			foreach (RQRequest order in adapter.Get<RQRequest>())
			{
				this.Document.Current = order;
				if (order.Hold == true)
				{
					yield return order;
				}
				else
				{
					if (order.Hold != true && order.Approved != true)
					{
						order.CheckBudget = false;
						if (order.BudgetValidation == true)
							foreach (RQBudget budget in this.Budget.Select())
							{
								if (budget.RequestAmt > budget.BudgetAmt)
								{
									order.CheckBudget = true; break;
								}
							}
						if(order.CheckBudget == true)
						{
							RQRequestClass cls = this.reqclass.SelectWindowed(0,1,order.ReqClassID);
							if (cls != null && cls.BudgetValidation == RQRequestClassBudget.Error)
								throw new PXRowPersistedException(typeof(RQRequest).Name, order, Messages.CheckBudgetWarning);
						}

						if (Setup.Current.RequestAssignmentMapID != null)
						{
							var processor = new EPAssignmentProcessor<RQRequest>();
							processor.Assign(order, Setup.Current.RequestAssignmentMapID);
                            order.WorkgroupID = order.ApprovalWorkgroupID;
                            order.OwnerID = order.ApprovalOwnerID;
                        }
					}
					yield return (RQRequest)Document.Search<RQRequest.orderNbr>(order.OrderNbr);
				}

			}
		}

        public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
        {
            if (viewName.ToLower() == "document" && values != null)
            {
                if (IsImport || IsExport || IsMobile || IsContractBasedAPI)
                {
                    Document.Cache.Locate(keys);
                    if (values.Contains("Hold") && values["Hold"] != PXCache.NotSetValue && values["Hold"] != null)
                    {
                        var hold = Document.Current.Hold ?? false;
                        if (Convert.ToBoolean(values["Hold"]) != hold)
                        {
                            ((PXAction<RQRequest>)this.Actions["Hold"]).PressImpl(false);
                        }
                    }
                }
            }
            return base.ExecuteUpdate(viewName, keys, values, parameters);
        }

        public PXAction<RQRequest> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter,
		[PXInt]
		[PXIntList(new int[] { 1, 2 }, new string[] { "Persist", "Update" })]
		int? actionID,
		[PXBool]
		bool refresh,
		[PXString]
		string actionName
		)
		{
			List<RQRequest> result = new List<RQRequest>();
			if (actionName != null)
			{
				PXAction a = this.Actions[actionName];
				if (a != null)
					result.AddRange(a.Press(adapter).Cast<RQRequest>());		
			}
			else
				result.AddRange(adapter.Get<RQRequest>());						

			if (refresh)
			{
				foreach (RQRequest order in result)
					Document.Search<RQRequest.orderNbr>(order.OrderNbr);
			}
			switch (actionID)
			{
				case 1:
					Save.Press();
					break;
				case 2:
					break;
			}
			return result;
		}
		public PXAction<RQRequest> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter,
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			string reportID)
		{
            List<RQRequest> list = adapter.Get<RQRequest>().ToList();
			if (!String.IsNullOrEmpty(reportID))
			{
                Save.Press();
				int i = 0;
				//reportID = Activity.SearchReport(APNotificationSource.Vendor, reportID);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				foreach (RQRequest order in list)
				{
					parameters["RQRequest.OrderNbr" + i.ToString()] = order.OrderNbr;										
				}				
				throw new PXReportRequiredException(parameters, reportID, "Report " + reportID);				
			}
			return list;
		}
		
		public PXAction<RQRequest> ViewDetails;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = "Requisition Details")]
		public virtual IEnumerable viewDetails(PXAdapter adapter)
		{
			this.Contents.AskExt();
			this.Contents.ClearDialog();
			yield break;
		}
		public PXAction<RQRequest> ViewRequisition;		
		[PXUIField(DisplayName = "View Requisition")]
		[PXLookupButton]
		public virtual IEnumerable viewRequisition(PXAdapter adapter)
		{
			if (this.Contents.Current != null)
			{
				EntityHelper helper = new EntityHelper(this);
				helper.NavigateToRow(typeof(RQRequisition).FullName, new object[]{this.Contents.Current.ReqNbr}, PXRedirectHelper.WindowMode.NewWindow);
			}
			yield break;
		}
		public PXAction<RQRequest> Assign;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = "Assign", Visible = false)]
		public virtual IEnumerable assign(PXAdapter adapter)
		{
			foreach (RQRequest req in adapter.Get<RQRequest>())
			{
				if (Setup.Current.RequestAssignmentMapID != null)
				{
					var processor = new EPAssignmentProcessor<RQRequest>();
					processor.Assign(req, Setup.Current.RequestAssignmentMapID);
				    req.WorkgroupID = req.ApprovalWorkgroupID;
				    req.OwnerID = req.ApprovalOwnerID;
				}
				yield return req;
			}
		}

		public virtual IEnumerable budget()
		{
			this.Budget.Cache.Clear();
			DateTime? startdate = null, enddate = null;

			string startPeriod  = string.Empty;
			string budgetPeriod = null;

			if (this.finperiod.Current == null) yield break;

			switch(Setup.Current.BudgetCalculation)
			{
				case RQBudgetCalculationType.YTD:
					enddate = this.finperiod.Current.EndDateUI;						
					startdate = new DateTime(enddate.Value.Year, 1, 1);
					budgetPeriod = this.finperiod.Current.FinPeriodID;										
					break;
				case RQBudgetCalculationType.PTD: 
					startdate = this.finperiod.Current.StartDate;
					enddate   = this.finperiod.Current.EndDateUI;
					startPeriod  = this.finperiod.Current.FinPeriodID;
					budgetPeriod = this.finperiod.Current.FinPeriodID;
					break;
				case RQBudgetCalculationType.Annual:
					int year = this.finperiod.Current.StartDate.Value.Year;
					startdate = new DateTime(year, 1,1);
					enddate = new DateTime(year, 12, 31);					
					MasterFinPeriod last = PXSelect<MasterFinPeriod,
					Where<MasterFinPeriod.finYear, Equal<Required<MasterFinPeriod.finYear>>>,
					OrderBy<Desc<MasterFinPeriod.periodNbr>>>.SelectWindowed(this, 0, 1, this.finperiod.Current.FinYear);

					MasterFinPeriod first = PXSelect<MasterFinPeriod,
					Where<MasterFinPeriod.finYear, Equal<Required<MasterFinPeriod.finYear>>>,
					OrderBy<Asc<MasterFinPeriod.periodNbr>>>.SelectWindowed(this, 0, 1, this.finperiod.Current.FinYear);

					if(first!= null)
						startPeriod = first.FinPeriodID;

					if (last != null)
						budgetPeriod = last.FinPeriodID;
					break;
			}
            bool isoverbudget = false;
            foreach (RQRequestLine line in this.Lines.Select())
			{
				if(line.ExpenseAcctID == null || line.ExpenseSubID == null) continue;
				
				RQBudget item = new RQBudget();				
				item.ExpenseAcctID = line.ExpenseAcctID;
				item.ExpenseSubID = line.ExpenseSubID;
				item = this.Budget.Locate(item);
				Account account = (Account)PXSelectorAttribute.Select<RQRequestLine.expenseAcctID>(this.Lines.Cache, line);
				

				if (item == null)
				{
					item = new RQBudget();
					item.ExpenseAcctID = line.ExpenseAcctID;
					item.ExpenseSubID = line.ExpenseSubID;
					item.CuryID = account.CuryID ?? this.company.Current.BaseCuryID;
					item.CuryInfoID = Document.Current.CuryInfoID;
					item.BudgetAmt = 0m;
					item.UsageAmt = 0m;
					item.DocRequestAmt = 0m;
					item.RequestAmt = 0m;
					item.AprovedAmt = 0m;
					item.UnaprovedAmt = 0m;

					RQBudget r =
					PXSelectJoinGroupBy<RQBudget,
						InnerJoin<MasterFinPeriod,
							On<MasterFinPeriod.finPeriodID, Equal<RQBudget.finPeriodID>>>,
					Where<RQBudget.expenseAcctID, Equal<Required<RQBudget.expenseAcctID>>,
							And<RQBudget.expenseSubID, Equal<Required<RQBudget.expenseSubID>>,
							And<MasterFinPeriod.finYear, Equal<Required<MasterFinPeriod.finYear>>,
							And<MasterFinPeriod.finPeriodID, Between<Required<MasterFinPeriod.finPeriodID>, Required<MasterFinPeriod.finPeriodID>>,
							And<RQBudget.orderNbr, NotEqual<Required<RQBudget.orderNbr>>>>>>>,
					Aggregate<
						GroupBy<RQBudget.expenseAcctID,
						GroupBy<RQBudget.expenseSubID,
						Sum<RQBudget.requestAmt,
						Sum<RQBudget.curyRequestAmt,
						Sum<RQBudget.aprovedAmt,
						Sum<RQBudget.curyAprovedAmt,						
						Sum<RQBudget.unaprovedAmt,
						Sum<RQBudget.curyUnaprovedAmt>>>>>>>>>>.SelectWindowed(this, 0, 1,
						line.ExpenseAcctID, line.ExpenseSubID, finperiod.Current.FinYear, startPeriod, budgetPeriod, this.Document.Current.OrderNbr);
					if (r != null)
					{
						item.RequestAmt += (account.CuryID == null ? r.RequestAmt : r.CuryRequestAmt )?? 0m;
                        item.AprovedAmt += (account.CuryID == null ? r.AprovedAmt : r.CuryAprovedAmt) ?? 0m;						
						item.UnaprovedAmt += (account.CuryID == null ? r.UnaprovedAmt : r.CuryUnaprovedAmt) ?? 0m;						
					}


					GLBudgetLineDetail bs =
					PXSelectGroupBy<GLBudgetLineDetail,
					Where<GLBudgetLineDetail.ledgerID, Equal<Required<GLBudgetLineDetail.ledgerID>>,
					And<GLBudgetLineDetail.finYear, Equal<Required<GLBudgetLineDetail.finYear>>,
					And<GLBudgetLineDetail.accountID, Equal<Required<GLBudgetLineDetail.accountID>>,
					And<GLBudgetLineDetail.subID, Equal<Required<GLBudgetLineDetail.subID>>,
					And<GLBudgetLineDetail.finPeriodID,
							Between<Required<GLBudgetLineDetail.finPeriodID>,
											Required<GLBudgetLineDetail.finPeriodID>>>>>>>,
					Aggregate<
						Sum<GLBudgetLineDetail.amount,
						Sum<GLBudgetLineDetail.releasedAmount>>>>
					.SelectWindowed(this, 0, 1,
						Setup.Current.BudgetLedgerId,
						finperiod.Current.FinYear,
						line.ExpenseAcctID,
						line.ExpenseSubID,
						startPeriod, budgetPeriod);

					if (bs != null && bs.ReleasedAmount != null)
						item.BudgetAmt = bs.ReleasedAmount;

					GLHistory gs = PXSelectJoin<GLHistory,
						InnerJoin<Branch, On<Branch.ledgerID, Equal<GLHistory.ledgerID>, And<Branch.branchID, Equal<GLHistory.branchID>>>>,
						Where<GLHistory.branchID, Equal<Required<GLHistory.branchID>>,
							And<GLHistory.accountID, Equal<Required<GLHistory.accountID>>,
							And<GLHistory.subID, Equal<Required<GLHistory.subID>>,
							And<GLHistory.finPeriodID, LessEqual<Required<GLHistory.finPeriodID>>>>>>,
						 OrderBy<Desc<GLHistory.finPeriodID>>>
					.SelectWindowed(this, 0, 1,
						line.BranchID,
						line.ExpenseAcctID,
						line.ExpenseSubID,
						budgetPeriod);
					if (gs != null)
						item.UsageAmt = account.CuryID == null ? gs.YtdBalance : gs.CuryYtdBalance;

					item = this.Budget.Insert(item);
					yield return item;
				}
				
				Decimal? estExtCost = account.CuryID == null ? line.EstExtCost : line.CuryEstExtCost;

				item.RequestAmt += estExtCost;
				item.DocRequestAmt += estExtCost;
				if (this.Document.Current.Approved == true)
					item.AprovedAmt += estExtCost;
				else
					item.UnaprovedAmt += estExtCost;

				if (item.RequestAmt > item.BudgetAmt)
				{
                    isoverbudget = true;
                    this.Budget.Cache.RaiseExceptionHandling<RQBudget.budgetAmt>(item, item.BudgetAmt,
							new PXSetPropertyException(Messages.OverbudgetWarning,
								this.reqclass.Current.BudgetValidation == RQRequestClassBudget.Warning ? PXErrorLevel.Warning : PXErrorLevel.Error));
				}
            }
            Document.Cache.SetValueExt<RQRequest.isOverbudget>(Document.Current, isoverbudget);
            this.Budget.Cache.IsDirty = false;
		}

		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{				
				if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryID))
				{
					e.NewValue = customer.Current.CuryID;
					e.Cancel = true;
				}
				else if(employee.Current != null && !string.IsNullOrEmpty(employee.Current.CuryID))
				{
					e.NewValue = employee.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{				
				if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryRateTypeID))
				{
					e.NewValue = customer.Current.CuryRateTypeID;
					e.Cancel = true;
					
				}
				else if(employee.Current != null && !string.IsNullOrEmpty(employee.Current.CuryRateTypeID))
				{
					e.NewValue = employee.Current.CuryRateTypeID;
					e.Cancel = true;
				}								
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((RQRequest)Document.Cache.Current).OrderDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Lines.Cache);
				
				if (customer.Current != null && !(bool)customer.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}
				if (employee.Current != null && !(bool)employee.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}
		#endregion

		protected virtual void RQRequest_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Shipping_Address.Cache, Shipping_Contact.Cache))
				{
					POShipAddressAttribute.DefaultRecord<RQRequest.shipAddressID>(sender, e.Row);
					POShipContactAttribute.DefaultRecord<RQRequest.shipContactID>(sender, e.Row);
				}
				if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
				{
					if (e.ExternalCall || sender.GetValuePending<RQRequest.curyID>(e.Row) == null)
					{
						CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<RQRequest.curyInfoID>(sender, e.Row);

						string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
						if (string.IsNullOrEmpty(message) == false)
						{
							sender.RaiseExceptionHandling<RQRequest.orderDate>(e.Row, ((RQRequest)e.Row).OrderDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
						}

						if (info != null)
						{
							((RQRequest)e.Row).CuryID = info.CuryID;
						}
					}
				}
				sender.IsDirty = false;
				this.Remit_Address.Cache.IsDirty = false;
				this.Remit_Contact.Cache.IsDirty = false;
			}
		}
		protected virtual void RQRequest_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;			
			if (row == null) return;
				
			RQRequestClass reqClassCur = reqclass.Current as RQRequestClass;
			bool invHidden = false;
			if (reqClassCur != null)
			{
				if (reqClassCur.CustomerRequest == true)
					employee.Current = null;

				row.VendorHidden		   = reqClassCur.VendorNotRequest == true;
				row.CustomerRequest		   = reqClassCur.CustomerRequest  == true;
				row.BudgetValidation	   = reqClassCur.BudgetValidation > 0;
				bool vendorMultiply		   = reqClassCur.VendorMultiply   == true;
				bool vendorHiddenMultiPlay = row.VendorHidden != true && vendorMultiply;
				invHidden				  = reqClassCur.HideInventoryID == true;
				PXUIFieldAttribute.SetVisible<RQRequestLine.vendorID>		  (this.Lines.Cache, null, vendorHiddenMultiPlay);
				PXUIFieldAttribute.SetVisible<RQRequestLine.vendorLocationID> (this.Lines.Cache, null, vendorHiddenMultiPlay);
				PXUIFieldAttribute.SetVisible<RQRequestLine.vendorRefNbr>	  (this.Lines.Cache, null, vendorHiddenMultiPlay);
				PXUIFieldAttribute.SetVisible<RQRequestLine.vendorName>		  (this.Lines.Cache, null, vendorHiddenMultiPlay);
				PXUIFieldAttribute.SetVisible<RQRequestLine.vendorDescription>(this.Lines.Cache, null, vendorHiddenMultiPlay);
				PXUIFieldAttribute.SetVisible<RQRequestLine.alternateID>	  (this.Lines.Cache, null, vendorHiddenMultiPlay);
			}
			bool notCustomerRequest = row.CustomerRequest != true;
			bool budgetValidation = reqClassCur != null ? reqClassCur.BudgetValidation > 0 : false;
			if (row.Status == RQRequestStatus.Open && this.Approval.ValidateAccess(row.WorkgroupID, row.OwnerID))
				invHidden = false;

			CMSetup	cmsetup = CMSetup.Current;
			PXUIFieldAttribute.SetVisible<RQRequest.departmentID>(sender, row, notCustomerRequest);
			PXUIFieldAttribute.SetVisible<RQRequest.curyID>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			bool curyEnable = true;

			if (customer.Current != null && this.customer.Current.AllowOverrideCury != true)
				curyEnable = false;
			if (employee.Current != null && this.employee.Current.AllowOverrideCury != true)
				curyEnable = false;

			if (curyEnable && reqClassCur != null && reqClassCur.BudgetValidation > 0 && this.Lines.Select().Count > 0)
				curyEnable = false;

			PXUIFieldAttribute.SetEnabled<RQRequest.curyID>(sender, row, curyEnable);			
			PXUIFieldAttribute.SetVisible<RQRequest.vendorID>(sender, null, row.VendorHidden != true);
			PXUIFieldAttribute.SetVisible<RQRequest.vendorLocationID>(sender, null, row.VendorHidden != true);
			PXUIFieldAttribute.SetVisible<RQRequestLine.expenseAcctID>(Lines.Cache, null, budgetValidation);
			PXUIFieldAttribute.SetVisible<RQRequestLine.expenseSubID>(Lines.Cache, null, budgetValidation);
			PXUIFieldAttribute.SetVisible<RQRequestLine.inventoryID>  (Lines.Cache, null, !invHidden);
			PXUIFieldAttribute.SetVisible<RQRequestLine.subItemID>    (Lines.Cache, null, !invHidden);

			PXPersistingCheck persistingCheck = budgetValidation ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
			OpenPeriodAttribute.SetValidatePeriod<RQRequest.finPeriodID>(Lines.Cache, null, budgetValidation ? PeriodValidation.DefaultUpdate : PeriodValidation.Nothing);
			PXDefaultAttribute.SetPersistingCheck<RQRequest.departmentID>  	  (sender, row, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RQRequest.finPeriodID>(Lines.Cache, null, persistingCheck);				
			PXDefaultAttribute.SetPersistingCheck<RQRequestLine.expenseAcctID>(Lines.Cache, null, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RQRequestLine.expenseSubID> (Lines.Cache, null, persistingCheck);
			
			
			this.addInvBySite.SetEnabled(Lines.Cache.AllowInsert);
			APOpenPeriodAttribute.SetValidatePeriod<RQRequest.finPeriodID>
				(sender, row, row.BudgetValidation == true ? PeriodValidation.DefaultUpdate : PeriodValidation.Nothing);

			POShipAddress shipAddress = this.Shipping_Address.Select();
			PORemitAddress remitAddress = this.Remit_Address.Select();
			bool enableAddressValidation = (row.Cancelled == false)
				&& ((shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
				|| (remitAddress != null && remitAddress.IsDefaultAddress == false && remitAddress.IsValidated == false));
			this.validateAddresses.SetEnabled(enableAddressValidation);

			PXUIFieldAttribute.SetEnabled<RQRequest.shipToBAccountID>(sender, row,
				(row.ShipDestType != POShippingDestination.CompanyLocation && row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetEnabled<RQRequest.shipToLocationID>(sender, row,
				(row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetEnabled<RQRequest.siteID>(sender, row, (row.ShipDestType == POShippingDestination.Site));

			PXUIFieldAttribute.SetRequired<RQRequest.siteID>(sender, true);
			PXUIFieldAttribute.SetRequired<RQRequest.shipToBAccountID>(sender, (row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetRequired<RQRequest.shipToLocationID>(sender, (row.ShipDestType != POShippingDestination.Site));

			PXUIFieldAttribute.SetVisible<RQRequest.shipToBAccountID>(sender, row, (row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetVisible<RQRequest.shipToLocationID>(sender, row, (row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetVisible<RQRequest.siteID>(sender, row, (row.ShipDestType == POShippingDestination.Site));

			if (row != null && row.ShipDestType == POShippingDestination.Site)
			{
				var siteIdErrorString = PXUIFieldAttribute.GetError<RQRequest.siteID>(sender, e.Row);
				if (siteIdErrorString == null)
				{
					var siteIdErrorMessage = row.SiteIdErrorMessage;
					if (!string.IsNullOrWhiteSpace(siteIdErrorMessage))
					{
						sender.RaiseExceptionHandling<RQRequest.siteID>(e.Row, sender.GetValueExt<RQRequest.siteID>(e.Row),
							new PXSetPropertyException(siteIdErrorMessage, PXErrorLevel.Error));
					}
				}
			}
		}
		protected virtual void RQRequest_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if (row.Hold != true)
			{
				if (this.reqclass.Current.BudgetValidation == RQRequestClassBudget.Error && Budget.Select().AsEnumerable().Any(_ => ((RQBudget)_).RequestAmt > ((RQBudget)_).BudgetAmt))
					sender.RaiseExceptionHandling<RQRequest.hold>(row, row.Hold, new PXSetPropertyException(Messages.OverbudgetWarning, PXErrorLevel.Error));
				else
					sender.RaiseExceptionHandling<RQRequest.hold>(row, row.Hold, null);
			}
		}

		protected virtual void RQRequest_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

			PXDefaultAttribute.SetPersistingCheck<RQRequest.siteID>(sender, row, (row.ShipDestType == POShippingDestination.Site) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<RQRequest.shipToLocationID>(sender, row, (row.ShipDestType != POShippingDestination.Site) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<RQRequest.shipToBAccountID>(sender, row, (row.ShipDestType != POShippingDestination.Site) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		protected virtual void RQRequest_ShipDestType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{			
			e.NewValue =
					this.employee.Current == null ?
					POShippingDestination.Customer :
					POShippingDestination.CompanyLocation;
			e.Cancel = true;
		}

		protected virtual void RQRequest_ShipDestType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if (row == null) return;

			if (row.ShipDestType == POShippingDestination.Site)
			{
				sender.SetDefaultExt<RQRequest.siteID>(e.Row);
				sender.SetValueExt<RQRequest.shipToBAccountID>(e.Row, null);
				sender.SetValueExt<RQRequest.shipToLocationID>(e.Row, null);
			}
			else
			{
				sender.SetValueExt<RQRequest.siteID>(e.Row, null);
				sender.SetDefaultExt<RQRequest.shipToBAccountID>(e.Row);
				sender.SetDefaultExt<RQRequest.shipToLocationID>(e.Row);
			}
		}

		protected virtual void RQRequest_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;

			if (row != null && row.ShipDestType == POShippingDestination.Site)
			{
				string siteIdErrorMessage = string.Empty;

				try
				{
					POShipAddressAttribute.DefaultRecord<RQRequest.shipAddressID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					sender.RaiseExceptionHandling<RQRequest.siteID>(e.Row, sender.GetValueExt<RQRequest.siteID>(e.Row),
						new PXSetPropertyException(PO.Messages.ShippingAddressMayNotBeEmpty, PXErrorLevel.Error));
					sender.SetValueExt<RQRequest.shipAddressID>(e.Row, null);
					siteIdErrorMessage = PO.Messages.ShippingAddressMayNotBeEmpty;
				}
				try
				{
					POShipContactAttribute.DefaultRecord<RQRequest.shipContactID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					sender.RaiseExceptionHandling<RQRequest.siteID>(e.Row, sender.GetValueExt<RQRequest.siteID>(e.Row),
						new PXSetPropertyException(PO.Messages.ShippingContactMayNotBeEmpty, PXErrorLevel.Error));
					sender.SetValueExt<RQRequest.shipContactID>(e.Row, null);
					siteIdErrorMessage = PO.Messages.ShippingContactMayNotBeEmpty;
				}

				sender.SetValueExt<RQRequest.siteIdErrorMessage>(e.Row, siteIdErrorMessage);

				if (string.IsNullOrWhiteSpace(siteIdErrorMessage))
					PXUIFieldAttribute.SetError<RQRequest.siteID>(sender, e.Row, null);
			}
		}


		protected virtual void RQRequest_ShipToBAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if (row != null && employee.Current == null && row.ShipDestType == POShippingDestination.Customer)
			{
				e.NewValue = row.EmployeeID;
				e.Cancel = true;
			}			
		}
		protected virtual void RQRequest_ShipToBAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<RQRequest.shipToLocationID>(e.Row);
			}
		}

		protected virtual void RQRequest_ShipToLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if (row != null)
			{
				try
				{
					POShipAddressAttribute.DefaultRecord<RQRequest.shipAddressID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					sender.RaiseExceptionHandling<RQRequest.siteID>(e.Row, sender.GetValueExt<RQRequest.siteID>(e.Row),
						new PXSetPropertyException(PO.Messages.ShippingAddressMayNotBeEmpty, PXErrorLevel.Error));
				}
				try
				{
					POShipContactAttribute.DefaultRecord<RQRequest.shipContactID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					sender.RaiseExceptionHandling<RQRequest.siteID>(e.Row, sender.GetValueExt<RQRequest.siteID>(e.Row),
						new PXSetPropertyException(PO.Messages.ShippingContactMayNotBeEmpty, PXErrorLevel.Error));
				}
			}
		}

		protected virtual void RQRequest_ShipToLocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if (row != null && row.ShipDestType == POShippingDestination.Site)
			{
				e.Cancel = true;
				e.NewValue = null;
			}
		}

		protected virtual void RQRequest_ShipToBAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if (row == null) return;

			if (row.ShipDestType == POShippingDestination.Site)
			{
				e.Cancel = true;
				e.NewValue = null;
			}
		}

		protected virtual void RQRequest_ReqClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;			
			RQRequestClass reqClassCur = this.reqclass.Select(e.NewValue);
			if (reqClassCur == null)
				return;

			if (reqClassCur.RestrictItemList == true)
			{
				foreach (RQRequestLine line in this.Lines.Select(row.OrderNbr))
				{
					this.Lines.Cache.MarkUpdated(line);

					RQRequestClassItem item = 
					PXSelect<RQRequestClassItem,
					Where<RQRequestClassItem.reqClassID, Equal<Required<RQRequestClassItem.reqClassID>>,
						And<RQRequestClassItem.inventoryID, Equal<Required<RQRequestClassItem.inventoryID>>>>>.SelectWindowed(this, 0, 1,
						e.NewValue,
						line.InventoryID);
					if(item == null && line.InventoryID != null)
					{					
						PXFieldState id = (PXFieldState)this.Lines.Cache.GetValueExt<RQRequestLine.inventoryID>(line);						
						this.Lines.Cache.RaiseExceptionHandling<RQRequestLine.inventoryID>(line, id.ToString(), 
							new PXSetPropertyException(Messages.ItemReqClassRestriction, id.ToString()));
						e.Cancel = true;
					}
				}
			}
			if (e.Cancel)
				throw new PXSetPropertyException(Messages.SelectedReqClassRestriction);
		}

		protected virtual void RQRequest_ReqClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequest row = (RQRequest)e.Row;
			if (row == null) return;

			RQRequestClass oldreqclass = (e.OldValue == null) ? null : this.reqclass.Select(e.OldValue);
			RQRequestClass reqClassCur = this.reqclass.Select(row.ReqClassID);
			if (reqClassCur == null) return;

			this.reqclass.Current = reqClassCur;

			if (oldreqclass?.BudgetValidation != reqClassCur.BudgetValidation)
			{
				if (reqClassCur.BudgetValidation == RQRequestClassBudget.None)
					sender.SetValuePending<RQRequest.finPeriodID>(row, null);
				else
					sender.SetDefaultExt<RQRequest.finPeriodID>(row);
			}

			if (oldreqclass?.CustomerRequest != reqClassCur.CustomerRequest)
			{
				if (reqClassCur.CustomerRequest == true)
				{
					sender.SetDefaultExt<RQRequest.locationID>(row);
					sender.SetValuePending<RQRequest.locationID>(row, null);
					row.DepartmentID = null;
					row.FinPeriodID = null;
					row.EmployeeID = null;
					row.LocationID = null;
				}
				else
				{
					sender.SetDefaultExt<RQRequest.employeeID>(row);
					sender.SetDefaultExt<RQRequest.locationID>(row);
					sender.SetDefaultExt<RQRequest.departmentID>(row);
					sender.SetDefaultExt<RQRequest.finPeriodID>(row);
				}
			}

			RQRequest doc = (RQRequest)e.Row;
			if(!DefaultExpenseAccount(doc, null, RQAcctSubDefault.MaskClass))
				foreach (RQRequestLine line in this.Lines.Select(row.OrderNbr))
				{
					this.Lines.Cache.MarkUpdated(line);
				}
		}
		protected virtual void RQRequest_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RQRequest.vendorLocationID>(e.Row);			
			UpdateLinesVendor((RQRequest)e.Row);
		}
		protected virtual void RQRequest_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PORemitAddressAttribute.DefaultRecord<RQRequest.remitAddressID>(sender, e.Row);
			PORemitContactAttribute.DefaultRecord<RQRequest.remitContactID>(sender, e.Row);
			UpdateLinesVendor((RQRequest)e.Row);
		}		
		protected virtual void RQRequest_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			RQRequest doc = (RQRequest)e.Row;
			if (doc.Hold == true)
			{			
				if (doc.Cancelled == true)
					cache.SetValueExt<RQRequest.cancelled>(doc, false);
			}
		}
		protected virtual void RQRequest_DepartmentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			RQRequest doc = (RQRequest)e.Row;
			if (doc != null)
			{
				foreach (RQRequestLine line in this.Lines.Select(doc.OrderNbr))
				{
					RQRequestLine upd = (RQRequestLine)this.Lines.Cache.CreateCopy(line);
					upd.DepartmentID = doc.DepartmentID;
					this.Lines.Update(upd);
				}
				DefaultExpenseAccount(doc, RQAccountSource.Department, RQAcctSubDefault.MaskDepartment);				
			}

		}
		protected virtual void RQRequest_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			this.employee.Current = null;
			this.customer.Current = null;
			RQRequest doc = (RQRequest)e.Row;
			doc.LocationID = null;
			cache.SetDefaultExt<RQRequest.locationID>(doc);
			RQRequestClass reqClassCur = reqclass.Current as RQRequestClass;
			if (doc != null && reqClassCur != null && !(reqClassCur.CustomerRequest == true))
				DefaultExpenseAccount(doc, RQAccountSource.Requester, RQAcctSubDefault.MaskRequester);
			cache.SetDefaultExt<RQRequest.shipDestType>(doc);
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.ExternalCall || cache.GetValuePending<RQRequest.curyID>(e.Row) == null)
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<RQRequest.curyInfoID>(cache, e.Row);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						cache.RaiseExceptionHandling<RQRequest.orderDate>(e.Row, ((RQRequest)e.Row).OrderDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						((RQRequest)e.Row).CuryID = info.CuryID;
					}
				}
			}
		}

		protected virtual void RQRequest_LocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			RQRequest doc = (RQRequest)e.Row;
			RQRequestClass reqClassCur = reqclass.Current as RQRequestClass;			
			if (doc != null && reqClassCur != null && reqClassCur.CustomerRequest == true)
				DefaultExpenseAccount(doc, RQAccountSource.Requester, RQAcctSubDefault.MaskRequester);
			cache.SetDefaultExt<RQRequest.shipToLocationID>(doc);
		}

		private bool DefaultExpenseAccount(RQRequest req, string accountSource, string subAccountMask  )
		{
			RQRequestClass reqClassCur = reqclass.Current as RQRequestClass;
			if (reqClassCur == null) return false;

			bool accDefault = 				
				reqClassCur.ExpenseAccountDefault != accountSource;
			bool subDefault =
				reqClassCur.ExpenseSubMask != null &&
				reqClassCur.ExpenseSubMask.Contains(subAccountMask);

			if (!accDefault && !subDefault) return false;

			foreach (RQRequestLine line in this.Lines.Select(req.OrderNbr))
			{
				RQRequestLine upd = (RQRequestLine)this.Lines.Cache.CreateCopy(line);
				if(accDefault)
					this.Lines.Cache.SetDefaultExt<RQRequestLine.expenseAcctID>(upd);
				if (accDefault || subDefault)
					try
					{
						this.Lines.Cache.SetDefaultExt<RQRequestLine.expenseSubID>(upd);
					}
					catch
					{
						upd.ExpenseAcctID = null;
						this.Lines.Cache.RaiseExceptionHandling<RQRequestLine.expenseSubID>(upd, null, null);
					}
				this.Lines.Update(upd);
			}
			return true;
		}


		protected virtual void RQRequest_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequest doc = (RQRequest)e.Row;
			if (doc != null && doc.EmployeeID == null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}			
				
		protected virtual void _(Events.FieldUpdated<RQRequest.orderDate> e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<RQRequest.orderDate>(e.Cache, e.Args);
		}
				
		protected virtual void RQRequestLine_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RQRequestLine.vendorLocationID>(e.Row);
			sender.SetDefaultExt<RQRequestLine.vendorName>(e.Row);
		}
		protected virtual void RQRequestLine_VendorName_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (((RQRequestLine)e.Row).VendorID == null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}
		protected virtual void RQRequestLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequestClass reqClassCur = reqclass.Current as RQRequestClass;
			if (this.Document.Current.Hold == true)
			{
				sender.SetDefaultExt<RQRequestLine.uOM>(e.Row);
				sender.SetDefaultExt<RQRequestLine.vendorID>(e.Row);				
				sender.SetDefaultExt<RQRequestLine.subItemID>(e.Row);
				sender.SetDefaultExt<RQRequestLine.description>(e.Row);				
				sender.SetDefaultExt<RQRequestLine.estUnitCost>(e.Row);
				sender.SetDefaultExt<RQRequestLine.curyEstUnitCost>(e.Row);
				sender.SetDefaultExt<RQRequestLine.promisedDate>(e.Row);
				

				if (reqClassCur != null) 
				{
					if (reqClassCur.ExpenseAccountDefault == RQAccountSource.PurchaseItem)
					{
						sender.SetDefaultExt<RQRequestLine.expenseAcctID>(e.Row);
						sender.SetDefaultExt<RQRequestLine.expenseSubID>(e.Row);
					}
					else if (
					reqClassCur.ExpenseSubMask != null &&
					reqClassCur.ExpenseSubMask.Contains(RQAcctSubDefault.MaskItem))
						sender.SetDefaultExt<RQRequestLine.expenseSubID>(e.Row);
				}
			}			
		}
		protected virtual void RQRequestLine_SubItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{			
			sender.SetDefaultExt<RQRequestLine.curyEstUnitCost>(e.Row);
		}
		protected virtual void RQRequestLine_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{			
			sender.SetDefaultExt<RQRequestLine.curyEstUnitCost>(e.Row);
		}
		
		protected virtual void RQRequestLine_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{			
			sender.SetDefaultExt<RQRequestLine.curyEstUnitCost>(e.Row);
		}
		protected virtual void RQRequestLine_VendorLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			if (row == null || row.VendorID == null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}
    protected virtual void RQRequestLine_ExpenseAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RQRequestLine.expenseSubID>(e.Row);
		}
		protected virtual void RQRequestLine_ExpenseAcctID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			if (row != null && e.NewValue != null && PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				Account account = (Account) PXSelectorAttribute.Select<RQRequestLine.expenseAcctID>(sender, row, e.NewValue);
				if (account != null && account.CuryID != null && account.CuryID != this.Document.Current.CuryID)
					throw new PXSetPropertyException(Messages.RequestBudgetCuryIDValidation);
			}
		}
		protected virtual void RQRequestLine_ExpenseAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			RQRequestClass reqClassCur = reqclass.Current as RQRequestClass;
			if (row == null) return;

			if (reqClassCur == null || reqClassCur.BudgetValidation == RQRequestClassBudget.None)
			{
				e.NewValue = null;				
				return;
			}
			switch (reqClassCur.ExpenseAccountDefault)
			{
				case RQAccountSource.Department:
					EPDepartment dep = PXSelect<EPDepartment,
					Where<EPDepartment.departmentID,
					Equal<Required<EPDepartment.departmentID>>>>.Select(this, this.Document.Current.DepartmentID);
					e.NewValue = dep != null ? dep.ExpenseAccountID : null;
					break;
				case RQAccountSource.Requester:
					if (reqClassCur.CustomerRequest == true)
					{
						Location loc =
							PXSelect<Location,
						Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
							And<Location.locationID, Equal<Required<Location.locationID>>>>>
							.Select(this, this.Document.Current.EmployeeID, this.Document.Current.LocationID);

						e.NewValue = loc != null ? loc.VExpenseAcctID : null;
					}
					else
					{
						EPEmployee emp = PXSelect<EPEmployee,
						Where<EPEmployee.bAccountID,
						Equal<Required<EPEmployee.bAccountID>>>>.Select(this, this.Document.Current.EmployeeID);
						e.NewValue = emp != null ? emp.ExpenseAcctID : null;
					}
					break;
				case RQAccountSource.PurchaseItem:
					InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
					e.NewValue = (item != null ? item.COGSAcctID : null) ?? reqclass.Current.ExpenseAcctID;
					break;
				case RQAccountSource.RequestClass:
					e.NewValue = reqClassCur.ExpenseAcctID;
					break;
			}
			if (e.NewValue != null && PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				Account account = (Account)PXSelectorAttribute.Select<RQRequestLine.expenseAcctID>(sender, row, e.NewValue);
				if (account != null && account.CuryID != null && account.CuryID != this.Document.Current.CuryID)
					e.NewValue = null;
			}
			e.Cancel = true;
		}
		protected virtual void RQRequestLine_ExpenseSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = (RQRequestLine)e.Row;
			RQRequestClass reqClassCur = reqclass.Current;
			if (row == null) return;
			if (reqClassCur == null || reqClassCur.BudgetValidation == RQRequestClassBudget.None)
			{
				e.NewValue = null;
				return;
			}

			if (Document.Current != null && row.ExpenseAcctID != null)
			{
				var item = InventoryItem.PK.Find(this, row.InventoryID);

				EPDepartment dep = PXSelect<EPDepartment,
					Where<EPDepartment.departmentID,
					Equal<Required<EPDepartment.departmentID>>>>.Select(this, row.DepartmentID);
				
				int? requester_SubID = null;
				if (reqClassCur != null && reqClassCur.CustomerRequest == true )
				{
					Location loc = 
						PXSelect<Location,
					Where<Location.bAccountID,	Equal<Required<Location.bAccountID>>,
						And<Location.locationID, Equal<Required<Location.locationID>>>>>
						.Select(this, Document.Current.EmployeeID, Document.Current.LocationID);

					requester_SubID = loc?.VExpenseSubID;
				}
				else
				{
					EPEmployee emp = PXSelect<EPEmployee, 
					Where<EPEmployee.bAccountID,
					Equal<Required<EPEmployee.bAccountID>>>>.Select(this, Document.Current.EmployeeID);
					requester_SubID = emp?.ExpenseSubID;
				}

				int? item_SubID = item?.COGSSubID;
				int? depatment_SubID = dep?.ExpenseSubID;
				if (reqClassCur != null)
				{
					int? class_SubID = reqClassCur.ExpenseSubID;
					object value = SubAccountMaskAttribute.MakeSub<RQRequestClass.expenseSubMask>(this, 
						reqClassCur.ExpenseSubMask,
						new object[] { class_SubID, depatment_SubID, item_SubID, requester_SubID },
						new Type[] 
						{
							typeof(RQRequestClass.expenseSubID),
							typeof(EPDepartment.expenseSubID),
							typeof(InventoryItem.cOGSSubID),
							reqClassCur.CustomerRequest == true
								? typeof(Location.vExpenseSubID)
								: typeof(EPEmployee.expenseSubID)
						});

					try
					{
						sender.RaiseFieldUpdating<RQRequestLine.expenseSubID>(e.Row, ref value);
						e.NewValue = (int?)value;
					}
					catch (PXSetPropertyException ex)
					{
						PXTrace.WriteWarning(ex);
						e.NewValue = null;
					}
				}
				
				e.Cancel = true;
			}
		}
		protected virtual void RQRequestLine_PromisedDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequestClass reqClassCur = reqclass.Current as RQRequestClass;
			DateTime? reqDate = this.Document.Current.OrderDate;
			if (reqClassCur == null || reqDate == null) return;
			e.NewValue = (reqClassCur.PromisedLeadTime > 0) ?
				((DateTime)reqDate).AddDays(reqClassCur.PromisedLeadTime.Value) :
				reqDate;
			e.Cancel = true;
		}

		protected virtual void RQRequestLine_Cancelled_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			if (row.Cancelled == true)
				sender.SetValueExt<RQRequestLine.orderQty>(row, row.IssuedQty);
			else
				sender.SetValueExt<RQRequestLine.orderQty>(row, row.OriginQty);				

		}
		protected virtual void RQRequestLine_OrderQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			if ((Decimal)e.NewValue < row.ReqQty)
			{				
				e.NewValue = row.ReqQty;
				sender.RaiseExceptionHandling<RQRequestLine.orderQty>(row, null,
					new PXSetPropertyException(Messages.InsuffQty_LineQtyUpdated, PXErrorLevel.Warning));
			}
		}

		protected virtual void RQRequestLine_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (RQRequestLine) e.Row;
			if (row != null && row.ManualPrice != true && !sender.Graph.IsCopyPasteContext)
			{
				sender.SetDefaultExt<RQRequestLine.curyEstUnitCost>(e.Row);
			}
		}

		protected virtual void RQRequestLine_UOM_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;			
			if (row != null && row.ReqQty != 0)
			{
				string UOM = (string)e.NewValue;
				if (row.InventoryID != null)
				{
					row.OrderQty = INUnitAttribute.ConvertFromBase(sender,
					                                               row.InventoryID, UOM, row.BaseOrderQty.GetValueOrDefault(),
					                                               INPrecision.QUANTITY);

					foreach (RQRequisitionContent content in PXSelect<RQRequisitionContent,
						Where<RQRequisitionContent.orderNbr, Equal<Required<RQRequestLine.orderNbr>>,
							And<RQRequisitionContent.lineNbr, Equal<Required<RQRequestLine.lineNbr>>>>>.Select(this, row.OrderNbr,
							                                                                                   row.LineNbr))
					{
						RQRequisitionContent upd = PXCache<RQRequisitionContent>.CreateCopy(content);
						upd.ItemQty = INUnitAttribute.ConvertFromBase(sender, row.InventoryID, UOM, upd.BaseItemQty.GetValueOrDefault(),
						                                              INPrecision.QUANTITY);
						this.Contents.Update(upd);
					}
				}
			}
		}

        protected virtual void RQRequestLine_CuryEstUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            RQRequestLine row = e.Row as RQRequestLine;

            if (row == null)
                return;

            if (row.ManualPrice == true && row.CuryEstUnitCost != null)
            {
                e.NewValue = row.CuryEstUnitCost;
                return;
            }

            e.Cancel = true;
            e.NewValue = row.CuryEstUnitCost ?? 0m;
            RQRequest order = Document.Current;

            if (row.InventoryID == null || string.IsNullOrEmpty(row.UOM) || order == null)
                return;

            decimal? newPrice = POItemCostManager.Fetch<RQRequestLine.inventoryID, RQRequestLine.curyInfoID>(sender.Graph, row,
                                                                                                             row.VendorID, row.VendorLocationID, null, order.CuryID,
                                                                                                             row.InventoryID, row.SubItemID, null, row.UOM);
            if (newPrice >= 0)
                e.NewValue = newPrice;

            APVendorPriceMaint.CheckNewUnitCost<RQRequestLine, RQRequestLine.curyEstUnitCost>(sender, row, e.NewValue);
        }

		protected virtual void RQRequestLine_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			RQRequestLine oldrow = (RQRequestLine)e.Row;
			RQRequestLine row = (RQRequestLine)e.NewRow;
			if (oldrow.InventoryID != row.InventoryID && this.Document.Current.Hold != true)
			{			
				object uom = row.UOM;
				try
				{
					sender.RaiseFieldVerifying<RQRequestLine.uOM>(row, ref uom);
				}
				catch(PXSetPropertyException ex)
				{					
					sender.RaiseExceptionHandling<RQRequestLine.uOM>(row, row.UOM,
						ex);
					row.UOM = null;
				}
			}
		}
		protected virtual void RQRequestLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			if (this.Document.Current != null && this.Document.Current.Hold == true && !(row.Cancelled == true))			
				row.OriginQty = row.OrderQty;	
			if(row.VendorLocationID == null)
				row.AlternateID = null;

			if ((e.ExternalCall || sender.Graph.IsImport)
				&& sender.ObjectsEqual<RQRequestLine.branchID, RQRequestLine.inventoryID, RQRequestLine.uOM, RQRequestLine.orderQty, RQRequestLine.manualPrice>(e.Row, e.OldRow)
				&& !sender.ObjectsEqual<RQRequestLine.curyEstUnitCost, RQRequestLine.curyEstExtCost>(e.Row, e.OldRow))
				row.ManualPrice = true;
		}
		protected virtual void RQRequestLine_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			if (row == null) return;
			RQRequisitionContent content = this.Contents.Select();
			if (content != null)
			{
				e.Cancel = true;
				throw new PXRowPersistingException( typeof(RQRequestLine.lineNbr).Name, row, Messages.UnableDeleteRequestLine);
			}
		}
		protected virtual void RQRequestLine_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			if (row != null && row.InventoryID == null) row.Updatable = true;							
		}
		protected virtual void RQRequestLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequestLine row = (RQRequestLine)e.Row;
			if (row == null)	return;			
			RQRequestClass reqClassCur = reqclass.Current as RQRequestClass;
			
			PXUIFieldAttribute.SetEnabled<RQRequestLine.orderQty>(sender, row, !(row.Cancelled == true));
			PXUIFieldAttribute.SetEnabled<RQRequestLine.curyEstExtCost>(sender, row, !(row.Cancelled == true));					
			if (row != null)
			{

				if (row.Cancelled == true)
					row.IssueStatus = RQRequestIssueType.Canceled;
				else if (row.IssuedQty >= row.OrderQty)
					row.IssueStatus = RQRequestIssueType.Closed;
				else if (row.OpenQty == 0)
				{
					bool ordered = true;
					bool received = true;
					foreach (PXResult<RQRequisitionContent, RQRequisitionLineReceived, RQRequisition> item in Contents.Select(row.OrderNbr, row.LineNbr))
					{
						RQRequisitionLineReceived line = item;
						if (line.Status == RQRequisitionReceivedStatus.Open)
						{
							ordered = false;
							received = false;
							break;
						}
						if (line.Status == RQRequisitionReceivedStatus.Partially ||
								line.Status == RQRequisitionReceivedStatus.Ordered)
						{
							received = false;
						}
					}
					row.IssueStatus =
						received ? RQRequestIssueType.Closed :
						ordered ? RQRequestIssueType.Ordered :
											 RQRequestIssueType.Requseted;
					if (reqClassCur != null && reqClassCur.IssueRequestor == true && row.IssueStatus == RQRequestIssueType.Closed)
						row.IssueStatus = RQRequestIssueType.Received;

				}
				else if (row.IssuedQty > 0)
					row.IssueStatus = RQRequestIssueType.PartiallyIssued;
				else
					row.IssueStatus = RQRequestIssueType.Open;
				
				bool vendorMultiply = reqClassCur != null && reqClassCur.VendorMultiply == true;
				PXUIFieldAttribute.SetEnabled<RQRequestLine.vendorID>		 (sender, row, vendorMultiply);
				PXUIFieldAttribute.SetEnabled<RQRequestLine.vendorLocationID>(sender, row, vendorMultiply);
				PXUIFieldAttribute.SetEnabled<RQRequestLine.alternateID>(sender, row, row.VendorLocationID != null);

				PXUIFieldAttribute.SetEnabled<RQRequestLine.inventoryID>(sender, row, row.ReqQty == 0);
				PXUIFieldAttribute.SetEnabled<RQRequestLine.subItemID>(sender, row, row.ReqQty == 0);
				//PXUIFieldAttribute.SetEnabled<RQRequestLine.uOM>(sender, row, row.ReqQty == 0);

				var inventory = InventoryItem.PK.Find(this, row.InventoryID);
				PXUIFieldAttribute.SetEnabled<RQRequestLine.subItemID>(sender, row, inventory == null || inventory.StkItem == true);

				if (this.Document.Current != null &&
						this.Document.Current.Status == RQRequestStatus.Open)
				{
					if (row.Updatable == true &&
						this.Approval.ValidateAccess(this.Document.Current.WorkgroupID, this.Document.Current.OwnerID))
					{
						PXUIFieldAttribute.SetEnabled<RQRequestLine.inventoryID>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.subItemID>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.description>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.uOM>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.orderQty>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.curyEstUnitCost>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.estUnitCost>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.cancelled>(sender, row, true);
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(sender, row, false);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.description>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.uOM>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.cancelled>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.requestedDate>(sender, row, true);
						PXUIFieldAttribute.SetEnabled<RQRequestLine.promisedDate>(sender, row, true);
					}
				}
			}		
		}
		protected virtual void RQBudget_ExpenseAcctID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		
		private void UpdateLinesVendor(RQRequest row)
		{
			foreach (RQRequestLine line in this.Lines.Select(row.OrderNbr))
			{
				RQRequestLine upd = (RQRequestLine)this.Lines.Cache.CreateCopy(line);
				upd.VendorID = row.VendorID;
				upd.VendorLocationID = row.VendorLocationID;
				this.Lines.Update(upd);
			}
		}		

		public class PriceRecalcExt : PriceRecalcExt<RQRequestEntry, RQRequest, RQRequestLine, RQRequestLine.curyEstUnitCost>
		{
			protected override PXSelectBase<RQRequestLine> DetailSelect => Base.Lines;
			protected override IPricedLine WrapLine(RQRequestLine line) => new PricedLine(line);

			private class PricedLine : IPricedLine
			{
				private readonly  RQRequestLine _line;
				public PricedLine(RQRequestLine line) { _line = line; }

				public bool? ManualPrice
				{
					get { return _line.ManualPrice; }
					set { _line.ManualPrice = value; }
				}

				public int? InventoryID
				{
					get { return _line.InventoryID; }
					set { _line.InventoryID = value; }
				}

				public decimal? CuryUnitPrice
				{
					get { return _line.CuryEstUnitCost; }
					set { _line.CuryEstUnitCost = value; }
				}

				public decimal? CuryExtPrice
				{
					get { return _line.CuryEstExtCost; }
					set { _line.CuryEstExtCost = value; }
				}
			}
		}
	}
	[System.SerializableAttribute()]
	[PXProjection(typeof(Select2<InventoryItem,
		LeftJoin<INSiteStatus,
						On2<INSiteStatus.FK.InventoryItem, And<INSiteStatus.siteID, NotEqual<SiteAttribute.transitSiteID>>>,
		LeftJoin<INSubItem,
						On<INSiteStatus.FK.SubItem>,
		LeftJoin<INSite,
						On<INSiteStatus.FK.Site>,
		LeftJoin<INItemXRef,
						On<INItemXRef.inventoryID, Equal<InventoryItem.inventoryID>,
						And2<Where<INItemXRef.subItemID, Equal<INSiteStatus.subItemID>,
								Or<INSiteStatus.subItemID, IsNull>>,
						And<Where<CurrentValue<POSiteStatusFilter.barCode>, IsNotNull,
						And<INItemXRef.alternateType, Equal<INAlternateType.barcode>>>>>>,
		LeftJoin<INItemClass,
						On<InventoryItem.FK.ItemClass>,
		LeftJoin<INPriceClass,
						On<INPriceClass.priceClassID, Equal<InventoryItem.priceClassID>>,
		LeftJoin<Vendor,
						On<Vendor.bAccountID, Equal<InventoryItem.preferredVendorID>>,
		LeftJoin<POVendorInventory,
						On<POVendorInventory.inventoryID, Equal<InventoryItem.inventoryID>,
					 And<POVendorInventory.vendorID, Equal<CurrentValue<POSiteStatusFilter.vendorID>>>>,
		LeftJoin<INUnit,
					On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>,
					And<INUnit.unitType, Equal<INUnitType.inventoryItem>,
					And<INUnit.fromUnit, Equal<InventoryItem.purchaseUnit>,
					And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>>
					 >>>>>>>>>,
		Where2<CurrentMatch<InventoryItem, AccessInfo.userName>,		  
			And2<Where<INSiteStatus.siteID, IsNull, Or<INSite.branchID, IsNotNull, And2<CurrentMatch<INSite, AccessInfo.userName>,
				And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
					Or<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>>>>>>,
			And2<Where<INSiteStatus.subItemID, IsNull, 
			        Or<CurrentMatch<INSubItem, AccessInfo.userName>>>,
			And2<Where<CurrentValue<INSiteStatusFilter.onlyAvailable>, Equal<boolFalse>,
						 Or<POVendorInventory.vendorID, IsNotNull>>,
			 And<InventoryItem.stkItem, Equal<boolTrue>,
			 And<InventoryItem.isTemplate, Equal<False>,
			 And<InventoryItem.itemStatus, NotIn3<
				 InventoryItemStatus.unknown,
				 InventoryItemStatus.inactive,
				 InventoryItemStatus.markedForDeletion,
				 InventoryItemStatus.noPurchases,
				 InventoryItemStatus.noRequest>>>>>>>>>), Persistent = false)]
	public partial class RQSiteStatusSelected : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(BqlField = typeof(InventoryItem.inventoryID), IsKey = true)]
		[PXDefault()]
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
		protected string _InventoryCD;
		[PXDefault()]
		[InventoryRaw(BqlField = typeof(InventoryItem.inventoryCD))]
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

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBLocalizableString(60, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
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

		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
		[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		[PXUIField(DisplayName = "Item Class ID", Visible = false)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), ValidComboRequired = true)]
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

		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected string _ItemClassCD;
		[PXDBString(30, IsUnicode = true, BqlField = typeof(INItemClass.itemClassCD))]
		public virtual string ItemClassCD
		{
			get
			{
				return this._ItemClassCD;
			}
			set
			{
				this._ItemClassCD = value;
			}
		}
		#endregion

		#region ItemClassDescription
		public abstract class itemClassDescription : PX.Data.BQL.BqlString.Field<itemClassDescription> { }
		protected String _ItemClassDescription;
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INItemClass.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Item Class Description", Visible = false)]
		public virtual String ItemClassDescription
		{
			get
			{
				return this._ItemClassDescription;
			}
			set
			{
				this._ItemClassDescription = value;
			}
		}
		#endregion

		#region PriceClassID
		public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }

		protected string _PriceClassID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.priceClassID))]
		[PXUIField(DisplayName = "Price Class ID", Visible = false)]
		public virtual String PriceClassID
		{
			get
			{
				return this._PriceClassID;
			}
			set
			{
				this._PriceClassID = value;
			}
		}
		#endregion

		#region PriceClassDescription
		public abstract class priceClassDescription : PX.Data.BQL.BqlString.Field<priceClassDescription> { }
		protected String _PriceClassDescription;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INPriceClass.description))]
		[PXUIField(DisplayName = "Price Class Description", Visible = false)]
		public virtual String PriceClassDescription
		{
			get
			{
				return this._PriceClassDescription;
			}
			set
			{
				this._PriceClassDescription = value;
			}
		}
		#endregion

		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }

		protected Int32? _PreferredVendorID;
		[AP.Vendor(DisplayName = "Preferred Vendor ID", Required = false, DescriptionField = typeof(AP.Vendor.acctName), BqlField = typeof(InventoryItem.preferredVendorID), Visible = false)]
		public virtual Int32? PreferredVendorID
		{
			get
			{
				return this._PreferredVendorID;
			}
			set
			{
				this._PreferredVendorID = value;
			}
		}
		#endregion

		#region PreferredVendorDescription
		public abstract class preferredVendorDescription : PX.Data.BQL.BqlString.Field<preferredVendorDescription> { }
		protected String _PreferredVendorDescription;
		[PXDBString(250, IsUnicode = true, BqlField = typeof(Vendor.acctName))]
		[PXUIField(DisplayName = "Preferred Vendor Name", Visible = false)]
		public virtual String PreferredVendorDescription
		{
			get
			{
				return this._PreferredVendorDescription;
			}
			set
			{
				this._PreferredVendorDescription = value;
			}
		}
		#endregion

		#region BarCode
		public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
		protected String _BarCode;
		[PXDBString(255, BqlField = typeof(INItemXRef.alternateID), IsUnicode = true)]
		//[PXUIField(DisplayName = "Barcode")]
		public virtual String BarCode
		{
			get
			{
				return this._BarCode;
			}
			set
			{
				this._BarCode = value;
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected int? _SiteID;
		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute(BqlField = typeof(INSiteStatus.siteID))]
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

        #region SiteCD
        public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
        protected String _SiteCD;
		[PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<RTrim<INSite.siteCD>, Empty>), typeof(string))]
		public virtual String SiteCD
        {
            get
            {
                return this._SiteCD;
            }
            set
            {
                this._SiteCD = value;
            }
        }
        #endregion

        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected int? _SubItemID;
		[SubItem(typeof(RQSiteStatusSelected.inventoryID),  BqlField = typeof(INSubItem.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<RTrim<INSubItem.subItemCD>, Empty>), typeof(string))]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion

		#region BaseUnit
		public  abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }

		protected string _BaseUnit;
		[PXDefault(typeof(Search<INItemClass.baseUnit, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible, BqlField = typeof(InventoryItem.baseUnit))]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}
		#endregion

		#region PurchaseUnit
		public abstract class purchaseUnit : PX.Data.BQL.BqlString.Field<purchaseUnit> { }
		protected string _PurchaseUnit;
		[INUnit(typeof(POSiteStatusSelected.inventoryID), DisplayName = "Purchase Unit", BqlField = typeof(InventoryItem.purchaseUnit))]
		public virtual String PurchaseUnit
		{
			get
			{
				return this._PurchaseUnit;
			}
			set
			{
				this._PurchaseUnit = value;
			}
		}
		#endregion

		#region QtySelected
		public abstract class qtySelected : PX.Data.BQL.BqlDecimal.Field<qtySelected> { }
		protected Decimal? _QtySelected;
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Selected")]
		public virtual Decimal? QtySelected
		{
			get
			{
				return this._QtySelected ?? 0m;
			}
			set
			{
				if (value != null && value != 0m)
					this._Selected = true;
				this._QtySelected = value;
			}
		}
		#endregion

		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion

		#region QtyOnHandExt
		public abstract class qtyOnHandExt : PX.Data.BQL.BqlDecimal.Field<qtyOnHandExt> { }
		protected Decimal? _QtyOnHandExt;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatus.qtyOnHand, INUnit.unitRate>>,
			Div<INSiteStatus.qtyOnHand, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHandExt
		{
			get
			{
				return this._QtyOnHandExt;
			}
			set
			{
				this._QtyOnHandExt = value;
			}
		}
		#endregion

		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion

		#region QtyAvailExt
		public abstract class qtyAvailExt : PX.Data.BQL.BqlDecimal.Field<qtyAvailExt> { }
		protected Decimal? _QtyAvailExt;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatus.qtyAvail, INUnit.unitRate>>,
			Div<INSiteStatus.qtyAvail, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvailExt
		{
			get
			{
				return this._QtyAvailExt;
			}
			set
			{
				this._QtyAvailExt = value;
			}
		}
		#endregion

		#region QtyPOPrepared
		public abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		protected Decimal? _QtyPOPrepared;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyPOPrepared))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Prepared")]
		public virtual Decimal? QtyPOPrepared
		{
			get
			{
				return this._QtyPOPrepared;
			}
			set
			{
				this._QtyPOPrepared = value;
			}
		}
		#endregion

		#region QtyPOPreparedExt
		public abstract class qtyPOPreparedExt : PX.Data.BQL.BqlDecimal.Field<qtyPOPreparedExt> { }
		protected Decimal? _QtyPOPreparedExt;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
				Mult<INSiteStatus.qtyPOPrepared, INUnit.unitRate>>,
				Div<INSiteStatus.qtyPOPrepared, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Prepared")]
		public virtual Decimal? QtyPOPreparedExt
		{
			get
			{
				return this._QtyPOPreparedExt;
			}
			set
			{
				this._QtyPOPreparedExt = value;
			}
		}
		#endregion

		#region QtyPOOrders
		public abstract class qtyPOOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		protected Decimal? _QtyPOOrders;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyPOOrders))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Orders")]
		public virtual Decimal? QtyPOOrders
		{
			get
			{
				return this._QtyPOOrders;
			}
			set
			{
				this._QtyPOOrders = value;
			}
		}
		#endregion

		#region QtyPOOrdersExt
		public abstract class qtyPOOrdersExt : PX.Data.BQL.BqlDecimal.Field<qtyPOOrdersExt> { }
		protected Decimal? _QtyPOOrdersExt;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
				Mult<INSiteStatus.qtyPOOrders, INUnit.unitRate>>,
				Div<INSiteStatus.qtyPOOrders, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Orders")]
		public virtual Decimal? QtyPOOrdersExt
		{
			get
			{
				return this._QtyPOOrdersExt;
			}
			set
			{
				this._QtyPOOrdersExt = value;
			}
		}
		#endregion

		#region QtyPOReceipts
		public abstract class qtyPOReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		protected Decimal? _QtyPOReceipts;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyPOReceipts))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Receipts")]
		public virtual Decimal? QtyPOReceipts
		{
			get
			{
				return this._QtyPOReceipts;
			}
			set
			{
				this._QtyPOReceipts = value;
			}
		}
		#endregion

		#region QtyPOReceiptsExt
		public abstract class qtyPOReceiptsExt : PX.Data.BQL.BqlDecimal.Field<qtyPOReceiptsExt> { }
		protected Decimal? _QtyPOReceiptsExt;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatus.qtyPOReceipts, INUnit.unitRate>>,
			Div<INSiteStatus.qtyPOReceipts, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Receipts")]
		public virtual Decimal? QtyPOReceiptsExt
		{
			get
			{
				return this._QtyPOReceiptsExt;
			}
			set
			{
				this._QtyPOReceiptsExt = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(BqlField = typeof(InventoryItem.noteID))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
	}

	// TODO: remove in 2018R2 and replace with refactored logic from DiscountEngine
	public abstract class PriceRecalcExt<TGraph, TMaster, TDetail, TPriceField> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TMaster : class, IBqlTable, new()
		where TDetail : class, IBqlTable, new()
		where TPriceField : IBqlField
	{
		public PXFilter<Common.Discount.RecalcDiscountsParamFilter> recalcPricesFilter;

		public PXAction<TMaster> recalculatePricesAction;

		[PXButton, PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		public virtual IEnumerable RecalculatePricesAction(PXAdapter adapter)
		{
			if (recalcPricesFilter.AskExt() == WebDialogResult.OK)
			{
				PXCache detailCache = DetailSelect.Cache;
				foreach (TDetail line in DetailSelect.Select())
				{
					TDetail oldLine = (TDetail)detailCache.CreateCopy(line);
					IPricedLine pl = WrapLine(line);
					if (recalcPricesFilter.Current.OverrideManualPrices == true)
						pl.ManualPrice = false;

					if (pl.InventoryID != null
						&& pl.ManualPrice != true
						&& (recalcPricesFilter.Current.RecalcUnitPrices == true
							|| (pl.CuryUnitPrice == 0m
								&& pl.CuryExtPrice == 0m
								&& recalcPricesFilter.Current.OverrideManualPrices == false)))
					{
						detailCache.RaiseFieldUpdated<TPriceField>(line, 0m);
						detailCache.SetDefaultExt<TPriceField>(line);

						detailCache.IsDirty = true;
						detailCache.RaiseRowUpdated(line, oldLine);
						detailCache.MarkUpdated(line);
					} 
				}
			}
			return adapter.Get();
		}

		public PXAction<TMaster> recalculatePricesActionOk;

		[PXLookupButton, PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable RecalculatePricesActionOk(PXAdapter adapter) => adapter.Get();


		protected abstract PXSelectBase<TDetail> DetailSelect { get; }

		protected abstract IPricedLine WrapLine(TDetail line);

		protected interface IPricedLine
		{
			bool? ManualPrice { get; set; }
			int? InventoryID { get; set; }
			decimal? CuryUnitPrice { get; set; }
			decimal? CuryExtPrice { get; set; }
		}
	}
}
