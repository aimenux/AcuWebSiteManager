using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Discount.Mappers;

namespace PX.Objects.CT
{
	public class ContractMaint : PXGraph<ContractMaint, Contract>
	{
		#region DAC Overrides
		[PXString(1, IsFixed = true)]
		[RecurringOption.ListForDeposits]
		[PXDefault(RecurringOption.None)]
		[PXUIField(DisplayName = "Billing Type")]
		[PXFormula(typeof(Switch<Case<Where<ContractItem.deposit, Equal<True>>, RecurringOption.deposits>, ContractItem.recurringType>))]
		protected virtual void ContractItem_RecurringTypeForDeposits_CacheAttached(PXCache sender) { }       
		
		[PXUIField(DisplayName = "UOM")]
		[PXString(10, IsFixed = true)]
		[PXFormula(typeof(Switch<
			Case<Where<ContractItem.deposit, Equal<True>>, ContractItem.curyID>,
					 Selector<ContractItem.recurringItemID, InventoryItem.salesUnit>>))]
		protected virtual void ContractItem_UOMForDeposits_CacheAttached(PXCache sender) { } 
		
		[PXBool()]
		[PXFormula(typeof(Switch<Case<Where<ContractDetail.deposit, Equal<True>>,Switch<
						  Case<Where<Div<Mult<ContractDetail.recurringIncluded, Selector<ContractDetail.contractItemID, ContractItem.retainRate>>, decimal100>, 
						  Greater<Sub<ContractDetail.recurringIncluded, ContractDetail.recurringUsedTotal>>>,
						  True>,False>>,False>))]
		protected virtual void ContractDetail_WarningAmountForDeposit_CacheAttached(PXCache sender) { }
	
		#endregion

		#region Selects/Views
		public PXSelectJoin<
			Contract, 
			LeftJoin<Customer, 
				On<Contract.customerID, Equal<Customer.bAccountID>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>>> 
			Contracts;

		public PXSelect<Contract, Where<Contract.contractID, Equal<Optional<Contract.contractID>>>> CurrentContract;

		[PXCopyPasteHiddenFields(typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), typeof(ContractBillingSchedule.startBilling))]
		public PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<Contract.contractID>>>> Billing;
		
		public PXSelect<ContractSLAMapping, Where<ContractSLAMapping.contractID, Equal<Current<Contract.contractID>>>> SLAMapping;
		public PXSelectJoin<ContractDetail, LeftJoin<ContractItem, On<ContractDetail.contractItemID, Equal<ContractItem.contractItemID>>>, Where<ContractDetail.contractID, Equal<Current<Contract.contractID>>>> ContractDetails;
		public PXSelectJoin<ContractDetail,
				InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>, Or<InventoryItem.inventoryID, Equal<ContractItem.baseItemID>, And<ContractItem.deposit, Equal<True>>>>>>,
				Where<ContractDetail.contractID, Equal<Current<Contract.contractID>>>> RecurringDetails;

		public PXSelect<ContractItem> RecurringDetailsContractItem;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Optional<ContractBillingSchedule.accountID>>, And<Location.locationID, Equal<Optional<ContractBillingSchedule.locationID>>>>> BillingLocation;

		public PXSelect<SelContractWatcher, Where<SelContractWatcher.contractID, Equal<Current<Contract.contractID>>>> Watchers;
		[PXCopyPasteHiddenView]
		public PXSelect<ARInvoice, Where<ARInvoice.projectID, Equal<Current<Contract.contractID>>>> Invoices;
	   [PXCopyPasteHiddenView]
		public PXSelect<ContractRenewalHistory, Where<ContractRenewalHistory.contractID, Equal<Current<Contract.contractID>>>> RenewalHistory;
		public PXSetup<Company> Company;
		public PXSetup<ContractTemplate, Where<ContractTemplate.contractID, Equal<Current<Contract.templateID>>>> CurrentTemplate;
		[PXCopyPasteHiddenView]
		public PXFilter<ActivationSettingsFilter> ActivationSettings;
		[PXCopyPasteHiddenView]
		public PXFilter<SetupSettingsFilter> SetupSettings;
		[PXCopyPasteHiddenView]
		public PXFilter<TerminationSettingsFilter> TerminationSettings;
		[PXCopyPasteHiddenView]
		public PXFilter<BillingOnDemandSettingsFilter> OnDemandSettings;
		[PXCopyPasteHiddenView]
		public PXFilter<RenewalSettingsFilter> RenewalSettings;
		public CRAttributeList<Contract> Answers;
		public PXSelectJoin<EPContractRate
				, LeftJoin<EPEmployee, On<EPContractRate.employeeID, Equal<EPEmployee.bAccountID>>
					, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPContractRate.earningType>>>
					>
				, Where<EPContractRate.contractID, Equal<Current<Contract.contractID>>>
				, OrderBy<Asc<EPContractRate.earningType, Asc<EPContractRate.employeeID>>>
				> ContractRates;
		public PXFilter<RenewManualNumberingFilter> renewManualNumberingFilter;
		public PXSelect<CurrencyInfo> CuryInfo;
		#region ForPxaCTFormulaInvoiceEditor
		//These views need for objects in pxa:CTFormulaInvoice(Transaction)Editor control
		public PXSelect<ContractTemplate> contractTemplate;
		public PXSelect<Location> customerLocation;
		public PXSelect<PM.PMTran> pmTran;
		public PXSelect<InventoryItem> inventoryItem;
		public PXSelect<UsageData> usageData;
		public PXSelect<AccessInfo> accessInfo;
		#endregion
		#endregion

		public ContractMaint()
		{
			PXUIFieldAttribute.SetDisplayName(Caches[typeof(Contact)], typeof(Contact.salutation).Name, CR.Messages.Attention);
			PXUIFieldAttribute.SetDisplayName<ContractDetail.used>(RecurringDetails.Cache, Messages.Used);
			PXUIFieldAttribute.SetDisplayName<ContractDetail.qty>(RecurringDetails.Cache, Messages.Included);

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
			PXDefaultAttribute.SetPersistingCheck<ContractBillingSchedule.billTo>(Billing.Cache, null, PXPersistingCheck.Null);
			action.AddMenuAction(ChangeID);

			FieldUpdated.AddHandler<Contract.expireDate>(UpdateHistoryDate<Contract.expireDate, ContractRenewalHistory.expireDate>);
			FieldUpdated.AddHandler<Contract.startDate>(UpdateHistoryDate<Contract.startDate, ContractRenewalHistory.startDate>);
			FieldUpdated.AddHandler<Contract.activationDate>(UpdateHistoryDate<Contract.activationDate, ContractRenewalHistory.activationDate>);
		}
		public override bool CanClipboardCopyPaste() { return false; }

		public override void Clear(PXClearOption option)
		{
			if (this.Caches.ContainsKey(typeof(ContractDetail)))
			{
				this.Caches[typeof(ContractDetail)].ClearQueryCache();
			}

			base.Clear(option);
		}

		#region Repository methods

		public static Contract FindContract(PXGraph graph, int? contractID)
		{
			return PXSelect<Contract,
							Where<Contract.contractID, Equal<Required<Contract.contractID>>>>
							.Select(graph, contractID);
		}

		#endregion

		#region Actions

		public PXAction<Contract> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					List<object> result = new List<object>();
					foreach (object data in action.Press(adapter))
					{
						result.Add(data);
					}
					return result;
				}
			}
			return adapter.Get();
		}

		public PXAction<Contract> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.InquiriesFolder)]
		protected virtual IEnumerable Inquiry(PXAdapter adapter,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					foreach (object data in action.Press(adapter)) ;
				}
			}
			return adapter.Get();
		}

		public PXAction<Contract> viewInvoice;
		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			if (Invoices.Current == null)
			{
				return adapter.Get();
			}

			PXRedirectHelper.TryRedirect(
				Invoices.Cache, 
				Invoices.Current, 
				Messages.ViewInvoice, 
				PXRedirectHelper.WindowMode.NewWindow);

			return adapter.Get();
		}

		public PXAction<Contract> viewUsage;
		[PXUIField(DisplayName = Messages.ViewUsage, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewUsage(PXAdapter adapter)
		{
			if (Contracts.Current != null && Contracts.Cache.GetStatus(Contracts.Current) != PXEntryStatus.Inserted)
			{
				UsageMaint target = PXGraph.CreateInstance<UsageMaint>();
				target.Clear();
				target.Filter.Current.ContractID = Contracts.Current.ContractID;
				target.Filter.Current.ContractStatus = Contracts.Current.Status;

				throw new PXRedirectRequiredException(target, Messages.ViewUsage);
			}

			return adapter.Get();
		}

		public PXAction<Contract> showContact;
		[PXUIField(DisplayName = "Show Contact", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ShowContact(PXAdapter adapter)
		{
			SelContractWatcher watcher = this.Watchers.Current;
			if (watcher?.ContactID != null)
			{
				ContactMaint graph = PXGraph.CreateInstance<ContactMaint>();
				graph.Clear();
				Contact contact = graph.Contact.Search<Contact.contactID>(watcher.ContactID);
				if (contact != null)
				{
					graph.Contact.Current = contact;
					throw new PXRedirectRequiredException(graph, PX.Objects.CR.Messages.ContactMaint);
				}
			}
			return adapter.Get();
		}

		public PXAction<Contract> renew;
		[PXUIField(DisplayName = "Renew Contract", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable Renew(PXAdapter adapter)
		{
			if (RenewalSettings.AskExt().IsPositive() && RenewalSettings.VerifyRequired() && RenewalSettings.Current.RenewalDate != null)
			{
				RenewContract((DateTime)RenewalSettings.Current.RenewalDate, true);
			}
			return adapter.Get();
		}

		public virtual void RenewContract(DateTime renewalDate, bool redirect = false)
		{
			Contract contract = PXCache<Contract>.CreateCopy(CurrentContract.Current);
			contract.IsLastActionUndoable = true;
			CurrentContract.Update(contract);
			Save.Press();
			if (contract.Type == Contract.type.Expiring || (contract.Type == Contract.type.Renewable && IsExpired(contract, renewalDate)))
		{
				ContractMaint target = CreateInstance<ContractMaint>();
				target.Clear();

				bool isAutonumbered = IsDimensionAutonumbered(ContractAttribute.DimensionName);
				if (!redirect && !isAutonumbered)
					throw new PXException(Messages.CannotRenewContract);

				string contractCD = null;
				if (!isAutonumbered && renewManualNumberingFilter.AskExt().IsPositive())
		{
					if (!renewManualNumberingFilter.VerifyRequired())
						return;
					if (PXSelectReadonly<Contract,
						Where<Contract.baseType, Equal<CTPRType.contract>,
								And<Contract.contractCD, Equal<Required<Contract.contractCD>>>>>.Select(this, renewManualNumberingFilter.Current.ContractCD.Trim()).Count != 0)
					{
						renewManualNumberingFilter.Cache.RaiseExceptionHandling<RenewManualNumberingFilter.contractCD>(renewManualNumberingFilter.Current, renewManualNumberingFilter.Current.ContractCD,
							new PXSetPropertyException(Messages.DublicateContractCD, renewManualNumberingFilter.Current.ContractCD));
						return;
					}
					contractCD = renewManualNumberingFilter.Current.ContractCD;
		}

				PXLongOperation.StartOperation(this, delegate
			{
					using (PXTransactionScope ts = new PXTransactionScope())
				{
						RenewExpiring(target, renewalDate, contractCD);
						target.Actions.PressSave();

					CreateExpiringRenewalHistory(target.Contracts.Current);

						target.Actions.PressSave();
						Save.Press();
						CTBillEngine.ClearBillingTrace(Contracts.Current.ContractID);
						ts.Complete();
					}
					if (redirect)
						throw new PXRedirectRequiredException(target, "Navigate to New Contract");
				});
				}
			else if (contract.Type == Contract.type.Renewable)
				{
				PXLongOperation.StartOperation(this, delegate
				{
					CTBillEngine engine = CreateInstance<CTBillEngine>();
					engine.Renew(contract.ContractID, renewalDate);
				});
			}
		}

		private bool IsDimensionAutonumbered(string dimension)
		{
			return PXSelect<Segment, Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>.Select(this, dimension)
				.RowCast<Segment>()
				.All(segment => segment.AutoNumber == true);
		}

		private void CreateExpiringRenewalHistory(Contract child)
		{
			if (child.OriginalContractID != null)
			{
				Contract parent = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.originalContractID>>>>.Select(this, child.OriginalContractID);

				parent.LastActiveRevID = parent.RevID;

				CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
				ContractBillingSchedule schedule = engine.BillingSchedule.SelectSingle(parent.ContractID);
				ContractRenewalHistory history = engine.CurrentRenewalHistory.SelectSingle(parent.ContractID, parent.RevID);

				CTBillEngine.UpdateContractHistoryEntry(history, parent, schedule);

				history.ContractID = parent.ContractID;
				history.ChildContractID = child.ContractID;
				history.Status = parent.Status;
				history.Action = ContractAction.Renew;
				history.RenewalDate = child.CreatedDateTime;
				history.RevID = ++parent.RevID;

				RenewalHistory.Insert(history);

				Contracts.Update(parent);
			}
		}

		public PXAction<Contract> viewContract;
		[PXUIField(DisplayName = Messages.ViewContract, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			if (RenewalHistory.Current != null && RenewalHistory.Current.ChildContractID != null)
			{
				ContractMaint target = PXGraph.CreateInstance<ContractMaint>();
				target.Clear();
				target.Contracts.Current = PXSelect<Contract, Where<Contract.contractID, Equal<Current<ContractRenewalHistory.childContractID>>>>.Select(this);
				throw new PXRedirectRequiredException(target, "ViewContract"){Mode = PXBaseRedirectException.WindowMode.NewWindow};              
			}

			return adapter.Get();
		}          

		public PXAction<Contract> bill;
		[PXUIField(DisplayName = Messages.Bill, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual void Bill()
		{
			Billing.Current = Billing.Select();
			DateTime? billingDate = Billing.Current.Type == BillingType.OnDemand && OnDemandSettings.AskExt().IsPositive() ? OnDemandSettings.Current.BillingDate : null;
			PXLongOperation.StartOperation(this, delegate
			{
				CTBillEngine engine = CreateInstance<CTBillEngine>();
				engine.Bill(Contracts.Current.ContractID, billingDate);
			});
		}

		protected virtual void SetupSettingsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SetupSettingsFilter filter = (SetupSettingsFilter) e.Row;
			if(filter == null) return;

			sender.RaiseExceptionHandling<SetupSettingsFilter.startDate>(filter, filter.StartDate, 
				filter.StartDate != Contracts.Current.StartDate
				? new PXSetPropertyException(Messages.ExpiraionDateWillBeRecalculated, PXErrorLevel.Warning, typeof(SetupSettingsFilter.startDate).Name)
				: null);
		}

		protected virtual void ActivationSettingsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ActivationSettingsFilter filter = (ActivationSettingsFilter)e.Row;
			if (filter == null) return;

			sender.RaiseExceptionHandling<ActivationSettingsFilter.activationDate>(filter, filter.ActivationDate,
				(filter.ActionName == nameof(activate) || filter.ActionName == nameof(setupAndActivate)) &&
				filter.ActivationDate != Contracts.Current.ActivationDate
				? new PXSetPropertyException(Messages.ExpiraionDateWillBeRecalculated, PXErrorLevel.Warning, typeof(ActivationSettingsFilter.activationDate).Name)
				: null);
		}

		public PXAction<Contract> setup;
		[PXUIField(DisplayName = Messages.SetupContract, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable Setup(PXAdapter adapter)
		{
			if (Contracts.Current == null) return adapter.Get();

			if (SetupSettings.AskExt((graph, viewName) => SetupSettings.Cache.SetDefaultExt<SetupSettingsFilter.startDate>(SetupSettings.Current)).IsPositive())
			{
				Contracts.Current.StartDate = SetupSettings.Current.StartDate;

				// to prevent contract history changes
				FieldUpdated.RemoveHandler<Contract.startDate>(UpdateHistoryDate<Contract.startDate, ContractRenewalHistory.startDate>);
				FieldUpdated.RemoveHandler<Contract.activationDate>(UpdateHistoryDate<Contract.activationDate, ContractRenewalHistory.activationDate>);
				FieldUpdated.RemoveHandler<Contract.expireDate>(UpdateHistoryDate<Contract.expireDate, ContractRenewalHistory.expireDate>);

				Contracts.Update(Contracts.Current); // for recalculate formula of expiration date

				// for valid import/contract based API
				FieldUpdated.AddHandler<Contract.startDate>(UpdateHistoryDate<Contract.startDate, ContractRenewalHistory.startDate>);
				FieldUpdated.AddHandler<Contract.activationDate>(UpdateHistoryDate<Contract.activationDate, ContractRenewalHistory.activationDate>);
				FieldUpdated.AddHandler<Contract.expireDate>(UpdateHistoryDate<Contract.expireDate, ContractRenewalHistory.expireDate>);

				Save.Press();

				PXLongOperation.StartOperation(this, () =>
				{
					CTBillEngine engine = CreateInstance<CTBillEngine>();
					engine.Setup(Contracts.Current.ContractID, Contracts.Current.StartDate);
				});
			}
			return new object[] { Contracts.Current };
		}

		public PXAction<Contract> activate;
		[PXUIField(DisplayName = Messages.ActivateContract, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable Activate(PXAdapter adapter)
		{
			if (Contracts.Current == null) return adapter.Get();

			PXDefaultAttribute.SetPersistingCheck<Contract.activationDate>(CurrentContract.Cache, CurrentContract.Current, PXPersistingCheck.NullOrBlank);
			if (ActivationSettings.AskExt((graph, viewName) =>
			{
				ActivationSettings.Cache.SetDefaultExt<ActivationSettingsFilter.activationDate>(ActivationSettings.Current);
				ActivationSettings.Current.ActionName = nameof(activate);
			}).IsPositive())
			{
				Contracts.Current.ActivationDate = ActivationSettings.Current.ActivationDate;

				// to prevent contract history changes
				FieldUpdated.RemoveHandler<Contract.activationDate>(UpdateHistoryDate<Contract.activationDate, ContractRenewalHistory.activationDate>);
				FieldUpdated.RemoveHandler<Contract.expireDate>(UpdateHistoryDate<Contract.expireDate, ContractRenewalHistory.expireDate>);

				Contracts.Update(Contracts.Current); // for recalculate formula of expiration date

				// for valid import/contract based API
				FieldUpdated.AddHandler<Contract.activationDate>(UpdateHistoryDate<Contract.activationDate, ContractRenewalHistory.activationDate>);
				FieldUpdated.AddHandler<Contract.expireDate>(UpdateHistoryDate<Contract.expireDate, ContractRenewalHistory.expireDate>);

				Save.Press();

				PXLongOperation.StartOperation(this, () =>
				{
					CTBillEngine engine = CreateInstance<CTBillEngine>();
					engine.Activate(Contracts.Current.ContractID, Contracts.Current.ActivationDate);
				});
			}
			return new object[] { Contracts.Current };
		}

		public PXAction<Contract> setupAndActivate;
		[PXUIField(DisplayName = Messages.SetupAndActivateContract, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable SetupAndActivate(PXAdapter adapter)
		{
			if (Contracts.Current == null) return adapter.Get();

			PXDefaultAttribute.SetPersistingCheck<Contract.activationDate>(CurrentContract.Cache, CurrentContract.Current, PXPersistingCheck.NullOrBlank);
			if (ActivationSettings.AskExt((graph, viewName) =>
			{
				ActivationSettings.Cache.SetDefaultExt<ActivationSettingsFilter.activationDate>(ActivationSettings.Current);
				ActivationSettings.Current.ActionName = nameof(setupAndActivate);
			}).IsPositive())
			{
				Contracts.Current.ActivationDate = ActivationSettings.Current.ActivationDate;
				Contracts.Current.StartDate = ActivationSettings.Current.ActivationDate;

				// to prevent contract history changes
				FieldUpdated.RemoveHandler<Contract.startDate>(UpdateHistoryDate<Contract.startDate, ContractRenewalHistory.startDate>);
				FieldUpdated.RemoveHandler<Contract.activationDate>(UpdateHistoryDate<Contract.activationDate, ContractRenewalHistory.activationDate>);
				FieldUpdated.RemoveHandler<Contract.expireDate>(UpdateHistoryDate<Contract.expireDate, ContractRenewalHistory.expireDate>);

				Contracts.Update(Contracts.Current); // for recalculate formula of expiration date

				// for valid import/contract based API
				FieldUpdated.AddHandler<Contract.startDate>(UpdateHistoryDate<Contract.startDate, ContractRenewalHistory.startDate>);
				FieldUpdated.AddHandler<Contract.activationDate>(UpdateHistoryDate<Contract.activationDate, ContractRenewalHistory.activationDate>);
				FieldUpdated.AddHandler<Contract.expireDate>(UpdateHistoryDate<Contract.expireDate, ContractRenewalHistory.expireDate>);
				Save.Press();

				PXLongOperation.StartOperation(this, () =>
				{
					CTBillEngine engine = CreateInstance<CTBillEngine>();
					engine.SetupAndActivate(Contracts.Current.ContractID, Contracts.Current.ActivationDate);
				});
			}
			return new object[] { Contracts.Current };
		}

		public PXAction<Contract> terminate;
		[PXUIField(DisplayName = Messages.Terminate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual void Terminate()
		{
			if (Contracts.Current != null)
			{
				if (Contracts.Current.CustomerID != null)
				{
					if (TerminationSettings.AskExt().IsPositive())
					{
						PXLongOperation.StartOperation(this, delegate()
						{
							CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
							engine.Terminate(Contracts.Current.ContractID, TerminationSettings.Current.TerminationDate);

						});
						if (this.IsImport) 
						{
							this.Actions.PressCancel();
						}
					}
				}
				else
				{
					throw new PXException(Messages.VirtualContractCannotBeTerminated);
					//virtual contract
				}
				
			}
		}

		public PXAction<Contract> upgrade;
		[PXUIField(DisplayName = Messages.Upgrade, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual void Upgrade()
		{
			this.Save.Press();
			if (Contracts.Current != null)
			{
				PXLongOperation.StartOperation(this, delegate()
				{
					CTBillEngine engine = PXGraph.CreateInstance<CTBillEngine>();
					engine.Upgrade(Contracts.Current.ContractID);
				});
			}
		}

		public PXAction<Contract> activateUpgrade;
		[PXUIField(DisplayName = Messages.ActivateUpgrade, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ActivateUpgrade(PXAdapter adapter)
		{
			if (Contracts.Current != null)
			{
				if (ActivationSettings.AskExt((graph, viewName) =>
				{
					ActivationSettings.Cache.SetDefaultExt<ActivationSettingsFilter.activationDate>(ActivationSettings.Current);
					ActivationSettings.Current.ActionName = nameof(activateUpgrade);
				}).IsPositive())
				{
					Save.Press();
					PXLongOperation.StartOperation(this, delegate()
					{
						CTBillEngine engine = CreateInstance<CTBillEngine>();
						engine.ActivateUpgrade(Contracts.Current.ContractID, ActivationSettings.Current.ActivationDate);
					});
				}

				return new object[] { Contracts.Current };
			}

			return adapter.Get();
		}

		public PXAction<Contract> undoBilling;
		[PXUIField(DisplayName = Messages.UndoBilling, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXLookupButton(ImageKey = PX.Web.UI.Sprite.Main.Process, Tooltip = Messages.UndoBillingTooltip)]
		public virtual void UndoBilling()
		{
			if (Contracts.Current == null) return;

			if(Contracts.Current.IsLastActionUndoable != true)
			{
				throw new PXException(Messages.CannotUndoAction);
			}

			PXLongOperation.StartOperation(this, delegate
			{
				CTBillEngine engine = CreateInstance<CTBillEngine>();
				engine.UndoBilling(Contracts.Current.ContractID);
			});
		}

		public PXChangeID<Contract, Contract.contractCD> ChangeID;

		#endregion

		public ContractBillingSchedule contractBillingSchedule => Billing.Select();

		[PXDBInt]
		[PXDimensionSelector(InventoryAttribute.DimensionName, 
			typeof(Search2<InventoryItem.inventoryID, 
				LeftJoin<ARSalesPrice, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>, 
					And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
				LeftJoin<ARSalesPrice2, On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, 
					And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>, 
				Where<InventoryItem.stkItem, Equal<False>>>), 
			typeof(InventoryItem.inventoryCD))]
		[PXUIField(DisplayName = "Setup Item")]
		public void ContractItem_BaseItemID_CacheAttached(PXCache sender) {}

		[PXDBInt]
		[PXDimensionSelector(InventoryAttribute.DimensionName, 
			typeof(Search2<InventoryItem.inventoryID,
				LeftJoin<ARSalesPrice, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>,
					And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>,
					And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
				LeftJoin<ARSalesPrice2, On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, 
					And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>,
				Where<InventoryItem.stkItem, Equal<False>>>), 
			typeof(InventoryItem.inventoryCD))]
		[PXUIField(DisplayName = "Renewal Item")]
		public void ContractItem_RenewalItemID_CacheAttached(PXCache sender) {}

		[PXDBInt]
		[PXDimensionSelector(InventoryAttribute.DimensionName, 
			typeof(Search2<InventoryItem.inventoryID,
				LeftJoin<ARSalesPrice, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>,
					And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>,
					And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
				LeftJoin<ARSalesPrice2, On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, 
					And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>,
				Where<InventoryItem.stkItem, Equal<False>>>), 
			typeof(InventoryItem.inventoryCD))]
		[PXUIField(DisplayName = "Recurring Item")]
		public void ContractItem_RecurringItemID_CacheAttached(PXCache sender) {}
		
		[PXDefault]
		[Customer(DescriptionField = typeof(Customer.acctName), Visibility=PXUIVisibility.SelectorVisible)]
		public void Contract_CustomerID_CacheAttached(PXCache sender) {}

		// to supress defaulting Expiration Date before set template
		[PXDBString(1, IsFixed = true)]
		[Contract.durationType.List]
		[PXDefault]
		[PXUIField(DisplayName = "Duration Unit")]
		public void Contract_DurationType_CacheAttached(PXCache sender) {}
		#region Event Handlers

		#region Contract Event Handlers

		protected virtual void Contract_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null)
			{
				ContractBillingSchedule schedule = new ContractBillingSchedule();
				schedule.ContractID = row.ContractID;
				Billing.Insert(schedule);

				PXUIFieldAttribute.SetRequired<ContractBillingSchedule.nextDate>(sender, true);

				PXStringState state = SLAMapping.Cache.GetStateExt<ContractSLAMapping.severity>(null) as PXStringState;
				if (state != null && state.AllowedValues != null && state.AllowedValues.Length > 0)
				{
					foreach (string severity in state.AllowedValues)
					{
						SLAMapping.Insert(new ContractSLAMapping
						{
							ContractID = row.ContractID,
							Severity = severity
						});
					}
				}

				ContractRenewalHistory history = new ContractRenewalHistory
				{
					RenewalDate = Accessinfo.BusinessDate,
					Status = row.Status,
					Action = ContractAction.Create,
					ChildContractID = row.OriginalContractID
				};

				CTBillEngine.UpdateContractHistoryEntry(history, row, schedule);

				RenewalHistory.Insert(history);

				Billing.Cache.IsDirty = false;
				SLAMapping.Cache.IsDirty = false;
				RenewalHistory.Cache.IsDirty = false;
			}
		}

		protected virtual void Contract_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row == null) return;

			SetControlsState(row, sender);
			CalcDetail(row);

			if (row.TotalsCalculated != 1)
			{
				CalcSummary(sender, row);
				if (row.TotalUsage != null)
				{
					row.TotalsCalculated = 1;
				}
			}
		}

		protected virtual void Contract_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(PM.PMTran.projectID));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(CRCase.contractID));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(ARTran.projectID));
		}


		protected virtual void Contract_TemplateID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract contract = e.Row as Contract;

			if (contract != null && contract.TemplateID == null)
			{
				foreach (ContractDetail item in ContractDetails.Select())
				{
					ContractDetails.Delete(item);
				}
				return;
			}

			if (contract != null && contract.TemplateID != null && Contracts.Cache.GetStatus(contract) == PXEntryStatus.Inserted)
			{
				DefaultFromTemplate(contract);
			}
		}

		protected virtual void Contract_ContractCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row == null) return;

			var selectContractTemplate = PXSelect<
				ContractTemplate, 
				Where<ContractTemplate.contractCD, Equal<Required<ContractTemplate.contractCD>>, 
				And<ContractTemplate.baseType, Equal<CTPRType.contractTemplate>>>>
					.Select(this, e.NewValue);
			var currentTemplate = selectContractTemplate.Select(c=>c.GetItem<ContractTemplate>().ContractCD).FirstOrDefault();
			if (currentTemplate != null )
			{
				throw new PXSetPropertyException(Messages.ContractIDAlreadyUsed, PXErrorLevel.Error);
			}
		}

		protected virtual void Contract_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null)
			{
				SetControlsState(row, sender);
			}
		}
		
		protected virtual void Contract_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null)
			{
				if (Contracts.Cache.GetStatus(row) == PXEntryStatus.Inserted)
				{
					#region Default CuryID and Rate type from Customer and CustomerClass

					PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>> cs = new PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
					Customer customer = cs.Select(row.CustomerID);

					string defaultCuryID = null;
					string defaultCuryRateType = null;
					if (customer != null)
					{
						if (!string.IsNullOrEmpty(customer.CuryID))
						{
							defaultCuryID = customer.CuryID;
						}

						if (!string.IsNullOrEmpty(customer.CuryRateTypeID))
						{
							defaultCuryRateType = customer.CuryRateTypeID;
						}

						if (string.IsNullOrEmpty(defaultCuryID) || string.IsNullOrEmpty(defaultCuryRateType))
						{
							PXSelect<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>> ccs = new PXSelect<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>(this);
							CustomerClass customerClass = ccs.Select(customer.CustomerClassID);
							if (customerClass != null)
							{
								if (!string.IsNullOrEmpty(defaultCuryID))
								{
									defaultCuryID = customerClass.CuryID;
								}

								if (!string.IsNullOrEmpty(defaultCuryRateType))
								{
									defaultCuryRateType = customerClass.CuryRateTypeID;
								}
							}

						}
					}

					if (!string.IsNullOrEmpty(defaultCuryRateType))
					{
						row.RateTypeID = defaultCuryRateType;
					}
					
					#endregion
				}

				Billing.Cache.SetDefaultExt<ContractBillingSchedule.accountID>(Billing.Current);
				sender.SetDefaultExt<Contract.locationID>(row);

				CheckBillingAccount(Billing.Current);
			}
		}

		protected virtual void Contract_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row != null && Company.Current != null)
			{
				row.CuryID = Company.Current.BaseCuryID;
			}
		}

		protected virtual void ResetDiscounts(PXCache sender, ContractDetail line)
		{
			line.BaseDiscountID = null;
			line.BaseDiscountSeq = null;
			line.BaseDiscountPct = 0;
			line.BaseDiscountAmt = 0;
			line.RecurringDiscountID = null;
			line.RecurringDiscountSeq = null;
			line.RecurringDiscountPct = 0;
			line.RecurringDiscountAmt = 0;
			line.RenewalDiscountID = null;
			line.RenewalDiscountSeq = null;
			line.RenewalDiscountPct = 0;
			line.RenewalDiscountAmt = 0;
		}

		public static void CalculateDiscounts(PXCache sender, Contract contract, ContractDetail det)
		{
			if (contract?.DiscountID != null)
			{
				ContractItem item = (new PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractItem.contractItemID>>>>(sender.Graph)).Select(det.ContractItemID);

				if (item != null)
				{
					if (item.IsBaseValid == true)
					{
						InventoryItem inventory = (new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(sender.Graph)).Select(item.BaseItemID);
						if (inventory != null)
						{

							DiscountEngine.SetLineDiscountOnly<ContractDetail>(sender, det,
								new DiscountLineFields<DiscountLineFields.skipDisc, ContractDetail.baseDiscountAmt, ContractDetail.baseDiscountPct, ContractDetail.baseDiscountID,
								ContractDetail.baseDiscountSeq, DiscountLineFields.discountsAppliedToLine, DiscountLineFields.manualDisc, DiscountLineFields.manualPrice, DiscountLineFields.lineType, DiscountLineFields.isFree, DiscountLineFields.calculateDiscountsOnImport>(sender, det), contract.DiscountID,
								det.BasePriceVal, det.Qty * det.BasePriceVal, det.Qty, contract.LocationID, contract.CustomerID, contract.CuryID, contract.EffectiveFrom ?? contract.StartDate.Value, null, item.BaseItemID);
						}
					}

					if (item.IsFixedRecurringValid == true || item.IsUsageValid == true)
					{
						InventoryItem inventory = (new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(sender.Graph)).Select(item.RecurringItemID);
						if (inventory != null)
						{
							DiscountEngine.SetLineDiscountOnly<ContractDetail>(sender, det,
								new DiscountLineFields<DiscountLineFields.skipDisc, ContractDetail.recurringDiscountAmt, ContractDetail.recurringDiscountPct, ContractDetail.recurringDiscountID,
								ContractDetail.recurringDiscountSeq, DiscountLineFields.discountsAppliedToLine, DiscountLineFields.manualDisc, DiscountLineFields.manualPrice, DiscountLineFields.lineType, DiscountLineFields.isFree, DiscountLineFields.calculateDiscountsOnImport>(sender, det), contract.DiscountID,
								det.FixedRecurringPriceVal, det.Qty * det.FixedRecurringPriceVal, det.Qty, contract.LocationID, contract.CustomerID, contract.CuryID, contract.EffectiveFrom ?? contract.StartDate.Value, null, item.RecurringItemID);
						}
					}

					if (item.IsRenewalValid == true)
					{
						InventoryItem inventory = (new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(sender.Graph)).Select(item.RenewalItemID);
						if (inventory != null)
						{
							DiscountEngine.SetLineDiscountOnly<ContractDetail>(sender, det,
								new DiscountLineFields<DiscountLineFields.skipDisc, ContractDetail.renewalDiscountAmt, ContractDetail.renewalDiscountPct, ContractDetail.renewalDiscountID,
								ContractDetail.renewalDiscountSeq, DiscountLineFields.discountsAppliedToLine, DiscountLineFields.manualDisc, DiscountLineFields.manualPrice, DiscountLineFields.lineType, DiscountLineFields.isFree, DiscountLineFields.calculateDiscountsOnImport>(sender, det), contract.DiscountID,
								det.RenewalPriceVal, det.Qty * det.RenewalPriceVal, det.Qty, contract.LocationID, contract.CustomerID, contract.CuryID, contract.EffectiveFrom ?? contract.StartDate.Value, null, item.RenewalItemID);
						}
					}
				}
			}
		}

		protected virtual void Contract_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract contract = (Contract)e.Row;

			foreach (ContractDetail det in ContractDetails.Select())
			{
				ResetDiscounts(ContractDetails.Cache, det);
				ContractDetails.Update(det);
			}
		}

		protected virtual void Contract_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contract contract = (Contract)e.Row;
			if (contract != null)
			{
				if (Billing.Current != null)
				{
					if (Billing.Current.BillTo == ContractBillingSchedule.billTo.CustomerAccount)
					{
						Billing.Current.LocationID = contract.LocationID;
						Billing.Cache.Update(Billing.Current);
					}
				}
			}
		}

		protected virtual void Contract_EffectiveFrom_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Contract row = e.Row as Contract;
			if (row == null) return;

			ARInvoice doc = PXSelect<ARInvoice, Where<ARInvoice.projectID, Equal<Current<Contract.contractID>>, And<ARInvoice.docDate, Greater<Required<ARInvoice.docDate>>>>>.SelectWindowed(this, 0, 1, (DateTime?) e.NewValue);

			if (doc != null)
			{
				sender.RaiseExceptionHandling<Contract.effectiveFrom>(row, e.NewValue, new PXSetPropertyException(Messages.InvoiceExistPostGivendate, PXErrorLevel.Warning));
			}
		}

		protected virtual void Contract_ExpireDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Contract contract = e.Row as Contract;
			if (contract == null) return;

			ContractBillingSchedule billing = Billing.Select();

			if (billing != null && e.NewValue != null && (DateTime)e.NewValue < billing.NextDate)
			{
				throw new PXSetPropertyException(Messages.ContractExpirationPrevNextBilling, billing.NextDate);
			}

			DateTime? max = contract.StartDate > contract.ActivationDate ? contract.StartDate : contract.ActivationDate;
			if ((DateTime?)e.NewValue < max)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, max);
			}
		}

		protected static void UpdateHistoryDate<ContractDate, HistoryDate>(PXCache sender, PXFieldUpdatedEventArgs e)
			where ContractDate : IBqlField
			where HistoryDate : IBqlField
		{
			Contract contract = e.Row as Contract;
			if (contract == null) return;

			ContractRenewalHistory history = PXSelect<ContractRenewalHistory,
				Where<ContractRenewalHistory.contractID, Equal<Required<Contract.contractID>>,
					And<ContractRenewalHistory.revID, Equal<Required<Contract.revID>>>>>.Select(sender.Graph, contract.ContractID, contract.RevID);
			sender.Graph.Caches<ContractRenewalHistory>().SetValue<HistoryDate>(history, sender.Graph.Caches<Contract>().GetValue<ContractDate>(contract));
			sender.Graph.Caches<ContractRenewalHistory>().Update(history);
		}
		#endregion

		#region ContractDetail Event Handlers

		protected virtual void ContractDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row != null)
			{
				if (!IsValidDetailPrice(this, row))
				{
					PXUIFieldAttribute.SetWarning<ContractDetail.contractItemID>(sender, row, Messages.ItemNotPrice);
				}
				if (row.WarningAmountForDeposit==true) 
				{
					this.RecurringDetails.Cache.RaiseExceptionHandling<ContractDetail.recurringIncluded>(row, row.RecurringIncluded, new PXSetPropertyException(Messages.DepositBalanceIsBelowTheRetainingAmountThreshold, PXErrorLevel.RowWarning));
				}

				bool stateAllowsPriceEdit = row.LastQty == null;

				PXUIFieldAttribute.SetEnabled<ContractDetail.qty>(sender, row, CurrentContract.Current.Status == Contract.status.Draft || CurrentContract.Current.Status == Contract.status.InUpgrade);
				PXUIFieldAttribute.SetEnabled<ContractDetail.basePriceVal>(sender, row, row.BasePriceEditable == true && stateAllowsPriceEdit);
				PXUIFieldAttribute.SetEnabled<ContractDetail.renewalPriceVal>(sender, row, row.RenewalPriceEditable == true && (stateAllowsPriceEdit || (CurrentContract.Current.Status == Contract.status.Draft || CurrentContract.Current.Status == Contract.status.InUpgrade)));
				PXUIFieldAttribute.SetEnabled<ContractDetail.fixedRecurringPriceVal>(sender, row, row.FixedRecurringPriceEditable == true && stateAllowsPriceEdit);
				PXUIFieldAttribute.SetEnabled<ContractDetail.usagePriceVal>(sender, row, row.UsagePriceEditable == true && stateAllowsPriceEdit);
				PXUIFieldAttribute.SetEnabled<ContractDetail.contractItemID>(sender, row, (CurrentContract.Current.Status == Contract.status.Draft || CurrentContract.Current.Status == Contract.status.InUpgrade) && CurrentTemplate.Current.AllowOverride==true);
				PXUIFieldAttribute.SetEnabled<ContractDetail.description>(sender, row, (CurrentContract.Current.Status == Contract.status.Draft || CurrentContract.Current.Status == Contract.status.InUpgrade) && CurrentTemplate.Current.AllowOverride == true);
				PXUIFieldAttribute.SetVisible<ContractDetail.change>(sender, null, CurrentContract.Current.Status != Contract.status.Active);
			}
		}

		protected virtual void ContractDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row != null)
			{
				try
				{
					ValidateUniqueness(this, row);
				}
				catch (PXException ex)
				{
					ContractItem cItem = (ContractItem)PXSelectorAttribute.Select<ContractDetail.contractItemID>(this.Caches<ContractDetail>(), row);
					sender.RaiseExceptionHandling<ContractDetail.contractItemID>(row, cItem.ContractItemCD, ex);
					e.Cancel = true;
				}

				int? revID = row.RevID;
				int? contractDetailID = row.ContractDetailID;
				var noteId = row.NoteID;
				ContractDetail detailExt = PXSelectReadonly<ContractDetailExt,
												Where<ContractDetailExt.contractID, Equal<Required<ContractDetail.contractID>>,
														And<ContractDetailExt.contractItemID, Equal<Required<ContractDetail.contractItemID>>,
														And<ContractDetailExt.revID, Equal<Required<ContractDetail.revID>>>>>>.Select(this, row.ContractID, row.ContractItemID, row.RevID - 1);
				if (detailExt != null)
				{
					sender.RestoreCopy(row, detailExt);
					row.RevID = revID;
					row.NoteID = noteId;
					ContractDetail det = sender.Locate(row) as ContractDetail;
					row.ContractDetailID = det != null ? det.ContractDetailID : contractDetailID;
					row.LastQty = row.Qty;
					row.Change = 0;
					row.LastBaseDiscountPct = row.BaseDiscountPct;
					row.LastRecurringDiscountPct = row.RecurringDiscountPct;
					row.LastRenewalDiscountPct = row.RenewalDiscountPct;
				}
			}
		}

		protected virtual void ContractDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			ContractDetail oldRow = e.Row as ContractDetail;
			ContractDetail row = e.NewRow as ContractDetail;
			if (!sender.ObjectsEqual<ContractDetail.contractItemID>(oldRow,row))
			{
				try
				{
					ValidateUniqueness(this, row);
				}
				catch (PXException ex)
				{
					sender.RaiseExceptionHandling<ContractDetail.contractItemID>(row, row.ContractItemID, ex);
					e.Cancel = true;
				}
			}
		}

		protected virtual void ContractDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row != null)
			{
				CalculateDiscounts(sender, Contracts.Current, row);
				Contracts.Current.TotalsCalculated = null;

				if (!IsImport)
				{
					ContractItem contractItem = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, row.ContractItemID);
					if (contractItem != null && contractItem.Deposit == false && contractItem.DepositItemID != null)
					{
						ContractItem depositItem = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, contractItem.DepositItemID);
						ContractDetail newDetail = new ContractDetail();
						sender.SetValueExt<ContractDetail.contractItemID>(newDetail, depositItem.ContractItemID);
						ContractDetails.Insert(newDetail);
						ContractDetails.View.RequestRefresh();
					}
				}
			}
		}

		protected virtual void ContractDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CalculateDiscounts(sender, Contracts.Current, (ContractDetail)e.Row);
			Contracts.Current.TotalsCalculated = null;
		}

		protected virtual void ContractDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Contracts.Current.TotalsCalculated = null;
		}

		protected virtual void ContractDetail_ContractItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row == null) return;

			ContractDetail templateDetail = PXSelect<ContractDetail,
				Where<ContractDetail.contractID, Equal<Current<Contract.templateID>>,
				And<ContractDetail.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>>.Select(this, row.ContractItemID);

			if (templateDetail != null)
			{
				row.Qty = templateDetail.Qty;
				row.Description = templateDetail.Description;
				PXDBLocalizableStringAttribute.CopyTranslations<ContractDetail.description, ContractDetail.description>
					(Caches[typeof(ContractDetail)], templateDetail, Caches[typeof(ContractDetail)], row);
			}
			else
			{
				ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractItem.contractItemID>>>>.Select(this, row.ContractItemID);
				if (item != null)
				{
					row.Qty = item.DefaultQty;
					row.Description = item.Descr;
					PXDBLocalizableStringAttribute.CopyTranslations<ContractItem.descr, ContractDetail.description>
						(Caches[typeof(ContractItem)], item, Caches[typeof(ContractDetail)], row);
				}
			}

			ResetDiscounts(sender, row);
		}

		protected virtual void ContractDetail_ContractItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ContractDetail detail = (ContractDetail)e.Row;
			ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, e.NewValue);
			Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<ContractDetail.contractID>>>>.Select(this, detail.ContractID);
			if (item != null && contract != null && item.CuryID != contract.CuryID)
			{
				e.NewValue = item.ContractItemCD;
				throw new PXSetPropertyException(Messages.ItemHasAnotherCuryID, item.ContractItemCD, item.CuryID, contract.CuryID, PXUIFieldAttribute.GetItemName(CurrentContract.Cache));
			}
		}

		protected virtual void ContractDetail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ContractDetail row = (ContractDetail)e.Row;
			ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>.Select(this, row.ContractItemID);
			if (item != null && (item.MaxQty < (decimal?)e.NewValue || item.MinQty > (decimal?)e.NewValue))
			{
				throw new PXSetPropertyException(Messages.QtyErrorWithParameters, PXDBQuantityAttribute.Round(item.MinQty ?? 0m), PXDBQuantityAttribute.Round(item.MaxQty ?? 0m));
			}
		}

		protected virtual void ContractDetail_BasePriceVal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row.BasePriceOption == PriceOption.Manually)
			{
				sender.SetValue<ContractDetail.basePrice>(e.Row, sender.GetValue<ContractDetail.basePriceVal>(e.Row));
			}
		}

		protected virtual void ContractDetail_RenewalPriceVal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row.RenewalPriceOption == PriceOption.Manually)
			{
				sender.SetValue<ContractDetail.renewalPrice>(e.Row, sender.GetValue<ContractDetail.renewalPriceVal>(e.Row));
			}
		}

		protected virtual void ContractDetail_FixedRecurringPriceVal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row.FixedRecurringPriceOption == PriceOption.Manually)
			{
				sender.SetValue<ContractDetail.fixedRecurringPrice>(e.Row, sender.GetValue<ContractDetail.fixedRecurringPriceVal>(e.Row));
			}
		}

		protected virtual void ContractDetail_UsagePriceVal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractDetail row = e.Row as ContractDetail;
			if (row.UsagePriceOption == PriceOption.Manually)
			{
				sender.SetValue<ContractDetail.usagePrice>(e.Row, sender.GetValue<ContractDetail.usagePriceVal>(e.Row));
			}
		}

		#endregion
		
		#region ContractBillingSchedule Event Handlers
		
		protected virtual void ContractBillingSchedule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ContractBillingSchedule row = e.Row as ContractBillingSchedule;

			if (row == null) return;

				SetControlsState(row, sender);

			PXDefaultAttribute.SetPersistingCheck<ContractBillingSchedule.accountID>(
				sender,
				row,
				row.BillTo == ContractBillingSchedule.billTo.SpecificAccount
					? PXPersistingCheck.NullOrBlank
					: PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<ContractBillingSchedule.locationID>(
				sender,
				row,
				row.BillTo == ContractBillingSchedule.billTo.CustomerAccount
					? PXPersistingCheck.Nothing
					: PXPersistingCheck.NullOrBlank);
			PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.invoiceFormula>(sender, row, CurrentTemplate.Current?.AllowOverrideFormulaDescription == true);
			PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.tranFormula>(sender, row, CurrentTemplate.Current?.AllowOverrideFormulaDescription == true);
		}

		protected virtual void ContractBillingSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ContractBillingSchedule row = (ContractBillingSchedule)e.Row;

			if (row == null) return;

				CheckBillingAccount(row);
			}

		protected virtual void ContractBillingSchedule_BillTo_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractBillingSchedule schedule = (ContractBillingSchedule)e.Row;
			if (schedule != null)
			{
				if (CurrentContract.Current.CustomerID != null)
				{
					sender.SetDefaultExt<ContractBillingSchedule.accountID>(schedule);
					switch (schedule.BillTo)
					{
						case ContractBillingSchedule.billTo.ParentAccount:
							sender.SetDefaultExt<ContractBillingSchedule.locationID>(schedule);
							break;
						case ContractBillingSchedule.billTo.CustomerAccount:
							schedule.LocationID = CurrentContract.Current.LocationID;
							break;
						case ContractBillingSchedule.billTo.SpecificAccount:
						default:
							schedule.LocationID = null;
							break;
					}
					CheckBillingAccount(schedule);
				}
			}
		}

		protected virtual void ContractBillingSchedule_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ContractBillingSchedule schedule = (ContractBillingSchedule)e.Row;
			if (schedule != null)
		{
				switch (schedule.BillTo)
			{
					case ContractBillingSchedule.billTo.ParentAccount:
						BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, CurrentContract.Current.CustomerID);
						Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<BAccount.parentBAccountID>>>>.Select(this, bAccount.ParentBAccountID);
						if (customer != null)
				{
							e.NewValue = customer.BAccountID;
				}
						break;
					case ContractBillingSchedule.billTo.CustomerAccount:
						e.NewValue = CurrentContract.Current.CustomerID;
						break;
					default:
						e.NewValue = null;
						break;
				}
			}
		}

		protected virtual void ContractBillingSchedule_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ContractBillingSchedule.locationID>(e.Row);
		}

		protected virtual void ContractBillingSchedule_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			BillingLocation.RaiseFieldUpdated(sender, e.Row);

			foreach (ContractDetail det in ContractDetails.Select())
			{
				ContractDetails.Cache.SetDefaultExt<ContractDetail.basePriceVal>(det);
				ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPriceVal>(det);
				ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPriceVal>(det);
				ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePriceVal>(det);

				ContractDetails.Cache.MarkUpdated(det);
			}
		}

		#endregion

		#region SelContractWatcher Event Handlers

		protected virtual void SelContractWatcher_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SelContractWatcher member = (SelContractWatcher)e.Row;
			if (member.ContactID != null)
			{
				Contact cont = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>
					.Select(this, member.ContactID);
				member.FirstName = cont.FirstName;
				member.MidName = cont.MidName;
				member.LastName = cont.LastName;
				member.Title = cont.Title;
			}
		}

		protected virtual void SelContractWatcher_ContactID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SelContractWatcher row = e.Row as SelContractWatcher;
			if (row != null)
			{
				Contact contact = PXSelect<Contact>.Search<Contact.contactID>(this, row.ContactID);

				if (contact != null && string.IsNullOrEmpty(contact.EMail) == false && string.IsNullOrEmpty(row.EMail))
				{
					row.EMail = contact.EMail;
				}
				if (contact != null && string.IsNullOrEmpty(contact.Salutation) == false && string.IsNullOrEmpty(row.Salutation))
				{
					row.Salutation = contact.Salutation;
				}
			}
		}

		protected virtual void SelContractWatcher_WatchTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SelContractWatcher caseContact = e.Row as SelContractWatcher;
			if (caseContact != null)
			{
				e.NewValue = "A";
			}
		}

		#endregion

		#region EPContractRate
		[PXDBInt()]
		[PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<EPContractRate.contractID>>>>))]
		[PXDBDefault(typeof(Contract.contractID))]
		protected virtual void EPContractRate_ContractID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXEPEmployeeSelector()]
		[PXCheckUnique(IgnoreNulls = false, Where = typeof(Where<EPContractRate.earningType, Equal<Current<EPContractRate.earningType>>, And<EPContractRate.contractID, Equal<Current<EPContractRate.contractID>>>>))]
		[PXUIField(DisplayName = "Employee")]
		protected virtual void EPContractRate_EmployeeID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL")]
		[PXDefault()]
		[PXRestrictor(typeof(Where<EP.EPEarningType.isActive, Equal<True>>), EP.Messages.EarningTypeInactive, typeof(EP.EPEarningType.typeCD))]
		[PXSelector(typeof(EP.EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type")]
		protected virtual void EPContractRate_EarningType_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#endregion

		public override void Persist()
		{
			Billing.Current = Billing.Select();
			List<ContractDetail> list = ContractDetails.Select().RowCast<ContractDetail>().ToList();
			foreach(ContractDetail detail in list
				.Where(detail => Billing.Current != null && Billing.Current.Type == BillingType.OnDemand))
			{
				string itemCD;
				if (!TemplateMaint.ValidItemForOnDemand(this, detail, out itemCD))
				{
					ContractDetails.Cache.RaiseExceptionHandling<ContractDetail.contractItemID>(detail, itemCD, new PXException(Messages.ItemOnDemandRecurringItem));
					ContractDetails.Cache.SetStatus(detail, PXEntryStatus.Updated);
				}
			}
			TemplateMaint.CheckContractOnDepositItems(list, Contracts.Current);
			base.Persist();
		}

		//TODO: Nightmare! Get rid ASAP
		private bool customerChanged;
		private bool templateChanged;
		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == "Contracts" && values != null)
			{
				customerChanged = values.Contains("CustomerID") && values["CustomerID"] != PXCache.NotSetValue;
				templateChanged = values.Contains("TemplateID") && values["TemplateID"] != PXCache.NotSetValue;
			}
			if (viewName.ToLower() == "billing" && values != null)
			{
				if (!IsImport && (customerChanged || templateChanged))
				{
					values["BillTo"] = PXCache.NotSetValue;
				}
				if (Billing.Current != null && Billing.Current.BillTo != ContractBillingSchedule.billTo.SpecificAccount)
				{
					values["AccountID"] = PXCache.NotSetValue;
				}
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public static void ValidateUniqueness(PXGraph graph, ContractDetail row, bool validateRecurring = false)
		{
			//assuming that row is not in cache yet
			if (row.ContractItemID.HasValue)
			{
				PXSelectBase<ContractDetail> s = new PXSelect<ContractDetail,
					Where<ContractDetail.contractItemID, Equal<Required<ContractDetail.contractItemID>>,
					And<ContractDetail.contractID, Equal<Required<Contract.contractID>>,
					And<ContractDetail.revID, Equal<Required<ContractDetail.revID>>,
					And<ContractDetail.lineNbr, NotEqual<Required<ContractDetail.lineNbr>>>>>>>(graph);

				ContractDetail item = s.SelectWindowed(0, 1, row.ContractItemID, row.ContractID, row.RevID, row.LineNbr);
				ContractItem cItem = (ContractItem)PXSelectorAttribute.Select<ContractDetail.contractItemID>(graph.Caches<ContractDetail>(), row);

				if (item != null)
				{
					throw new PXException(Messages.DuplicateItem, cItem.ContractItemCD);
				}

				if (validateRecurring && cItem.RecurringItemID != null)
				{
					InventoryItem invItem = (InventoryItem)PXSelectorAttribute.Select<ContractItem.recurringItemID>(graph.Caches<ContractItem>(), cItem);
					
					item = (new PXSelectJoin<ContractDetail, InnerJoin<ContractItem, On<ContractDetail.contractItemID, Equal<ContractItem.contractItemID>>>,
						Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>, 
						And<ContractDetail.revID, Equal<Required<ContractDetail.revID>>,
						And<ContractItem.recurringItemID, Equal<Required<ContractItem.recurringItemID>>>>>>(graph)).SelectWindowed(0, 1, row.ContractID, row.RevID, cItem.RecurringItemID);

					if (item != null)
					{
						throw new PXException(Messages.DuplicateRecurringItem, invItem.InventoryCD);
					}
				}
			}
		}

		protected virtual void SetControlsState(Contract row, PXCache cache)
		{
			if (row == null) return;

			ContractTemplate template = CurrentTemplate.Current;
			bool persisted = cache.GetStatus(row) != PXEntryStatus.Inserted;
			bool cancelled = row.Status == Contract.status.Canceled;
			bool enabled = !cancelled && (template == null || template.AllowOverride == true);

			RecurringDetails.Cache.AllowUpdate = false;
			RecurringDetails.Cache.AllowInsert = false;
			RecurringDetails.Cache.AllowDelete = false;
			ContractDetails.Cache.AllowInsert = (row.TemplateID != null && (row.Status == Contract.status.Draft || row.Status == Contract.status.InUpgrade) && enabled);
			ContractDetails.Cache.AllowUpdate = true;
			ContractDetails.Cache.AllowDelete = (row.TemplateID != null && (row.Status == Contract.status.Draft || row.Status == Contract.status.InUpgrade) && enabled);

			terminate.SetEnabled(persisted && row.CustomerID != null);

			PXUIFieldAttribute.SetEnabled<Contract.startDate>(cache, row, !persisted || !cancelled);
			PXUIFieldAttribute.SetEnabled<Contract.activationDate>(cache, row, !persisted || !cancelled);
			PXUIFieldAttribute.SetEnabled<Contract.autoRenew>(cache, row, (!persisted || !cancelled) && row.Type != Contract.type.Unlimited);
			PXUIFieldAttribute.SetEnabled<Contract.autoRenewDays>(cache, row, !persisted || !cancelled);
			PXUIFieldAttribute.SetEnabled<Contract.calendarID>(cache, row, !persisted || !cancelled);
			PXUIFieldAttribute.SetEnabled<Contract.rateTypeID>(cache, row, (!persisted || !cancelled) && IsMultyCurrency );
			PXUIFieldAttribute.SetEnabled<Contract.detailedBilling>(cache, row, !persisted || !cancelled);
			PXUIFieldAttribute.SetEnabled<Contract.effectiveFrom>(cache, row, row.IsPendingUpdate == true);
			PXUIFieldAttribute.SetEnabled<Contract.type>(cache, row, !row.TemplateID.HasValue);
			PXUIFieldAttribute.SetEnabled<Contract.templateID>(cache, row, !persisted);
			PXUIFieldAttribute.SetEnabled<Contract.customerID>(cache, row, !persisted);
			PXUIFieldAttribute.SetVisible<Contract.curyID>(cache, row, IsMultyCurrency || (!cancelled && template != null && template.AllowOverrideCury == true));
			PXUIFieldAttribute.SetEnabled<Contract.caseItemID>(cache, row,
				(template == null || template.AllowOverride == true)
				&& (row.Status == Contract.status.Draft || row.Status == Contract.status.PendingActivation || row.Status == Contract.status.InUpgrade));

			SLAMapping.Cache.AllowUpdate = (!persisted || !cancelled) && (persisted || row.TemplateID.HasValue);
			SLAMapping.Cache.AllowInsert = (!persisted || !cancelled) && (persisted || row.TemplateID.HasValue);
			SLAMapping.Cache.AllowDelete = (!persisted || !cancelled) && (persisted || row.TemplateID.HasValue);

			Delete.SetEnabled(row.Status == Contract.status.Draft);
			Contracts.Cache.AllowDelete = row.Status == Contract.status.Draft;
			undoBilling.SetEnabled(row.IsLastActionUndoable == true);
		}

		protected virtual void SetControlsState(ContractBillingSchedule row, PXCache cache)
		{
			if (row == null) return;

			if (Contracts.Current != null && Contracts.Current.TemplateID.HasValue)
			{
				PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.type>(cache, row, false);
			}

			if (Contracts.Current != null && Contracts.Current.Status != Contract.status.Active)
			{
				PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.nextDate>(cache, row, false);
			}

			PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.accountID>(cache, row, row.BillTo == ContractBillingSchedule.billTo.SpecificAccount);
			PXUIFieldAttribute.SetEnabled<ContractBillingSchedule.locationID>(cache, row, row.BillTo == ContractBillingSchedule.billTo.SpecificAccount || row.BillTo == ContractBillingSchedule.billTo.ParentAccount);

			Contract nextContract = PXSelectReadonly<Contract, Where<Contract.originalContractID, Equal<Current<Contract.contractID>>>>.SelectSingleBound(this, new object[]{Contracts.Current});
			renew.SetEnabled(Contracts.Current != null && Contracts.Current.Type != Contract.type.Unlimited && (row.NextDate >= Contracts.Current.ExpireDate || (row.NextDate == null && Contracts.Current.IsCompleted != true)) && nextContract == null);
		}
		
		protected virtual void DefaultFromTemplate(Contract contract)
		{
			ContractTemplate template = PXSelectReadonly<ContractTemplate>.Search<ContractTemplate.contractID>(this, contract.TemplateID);
			PXCache cache = this.Caches<Contract>();
			if (template != null)
			{
				contract.AutoRenew = template.AutoRenew;
				contract.AutoRenewDays = template.AutoRenewDays;
				contract.ClassType = template.ClassType;
				contract.CuryID = template.CuryID;
				contract.GracePeriod = template.GracePeriod;
				contract.RateTypeID = template.RateTypeID;
				contract.Type = template.Type;
				contract.Description = template.Description;
				PXDBLocalizableStringAttribute.CopyTranslations<ContractTemplate.description, Contract.description>
					(Caches[(typeof(ContractTemplate))], template, cache, contract);
				contract.Duration = template.Duration;
				contract.CalendarID = template.CalendarID;
				contract.DetailedBilling = template.DetailedBilling;
				contract.CaseItemID = template.CaseItemID;
				contract.AutomaticReleaseAR = template.AutomaticReleaseAR;
				contract.ScheduleStartsOn = template.ScheduleStartsOn;
				// SetValueExt for invoke Expiration Date calculation by formula
				cache.SetValueExt<Contract.durationType>(contract, template.DurationType);
				
				ContractBillingSchedule templateBilling = PXSelectReadonly<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<Contract.templateID>>>>.Select(this);
				if (templateBilling != null && Billing.Current != null)
				{
					Billing.Current.Type = templateBilling.Type;
					Billing.Cache.SetValueExt<ContractBillingSchedule.billTo>(Billing.Current, templateBilling.BillTo);
					Billing.Cache.SetValueExt<ContractBillingSchedule.invoiceFormula>(Billing.Current, templateBilling.InvoiceFormula);
					Billing.Cache.SetValueExt<ContractBillingSchedule.tranFormula>(Billing.Current, templateBilling.TranFormula);
				}

				SLAMapping.Cache.Clear();
				PXResultset<ContractSLAMapping> list = PXSelectReadonly<ContractSLAMapping, Where<ContractSLAMapping.contractID, Equal<Current<Contract.templateID>>>>.Select(this);
				foreach (ContractSLAMapping item in list)
				{
					ContractSLAMapping sla = new ContractSLAMapping();
					sla.ContractID = contract.ContractID;
					sla.Severity = item.Severity;
					sla.Period = item.Period;
					SLAMapping.Insert(sla);
				}
				
				foreach (ContractDetail item in ContractDetails.Select())
				{
					ContractDetails.Delete(item);
				}

				PXResultset<ContractDetail> items = PXSelectReadonly<ContractDetail,
						Where<ContractDetail.contractID, Equal<Current<Contract.templateID>>, And<ContractDetail.inventoryID, IsNull>>>.Select(this);

				foreach (ContractDetail item in items)
				{
					ContractDetail newitem = new ContractDetail{ContractItemID = item.ContractItemID};
					newitem = (ContractDetail)ContractDetails.Cache.CreateCopy(ContractDetails.Insert(newitem));
					if (item.Deposit != true || newitem != null)
					{
					CopyTemplateDetail(item, newitem);
					ContractDetails.Update(newitem);
				}
				}


				var history = RenewalHistory.Current;
				CTBillEngine.UpdateContractHistoryEntry(history, contract, Billing.Current);
				RenewalHistory.Update(history);
			}

		}

		protected virtual void CopyTemplateDetail(ContractDetail source, ContractDetail target)
		{
			target.Description = source.Description;
			PXDBLocalizableStringAttribute.CopyTranslations<ContractDetail.description, ContractDetail.description>(Caches[typeof(ContractDetail)], source, Caches[typeof(ContractDetail)], target);
			target.Included = source.Included;
			target.InventoryID = source.InventoryID;
			target.ResetUsage = source.ResetUsage;
			target.UOM = source.UOM;
			target.ContractItemID = source.ContractItemID;
			target.Qty = source.Qty;
		}


		protected virtual void CopyContractDetail(ContractDetail source, ContractDetail target)
		{
			CopyTemplateDetail(source, target);

			target.BasePrice = source.BasePrice;
			target.BasePriceOption = source.BasePriceOption;

			target.RenewalPrice = source.RenewalPrice;
			target.RenewalPriceOption = source.RenewalPriceOption;

			target.FixedRecurringPrice = source.FixedRecurringPrice;
			target.FixedRecurringPriceOption = source.FixedRecurringPriceOption;

			target.FixedRecurringPrice = source.FixedRecurringPrice;
			target.FixedRecurringPriceOption = source.FixedRecurringPriceOption;

			target.UsagePrice = source.UsagePrice;
			target.UsagePriceOption = source.UsagePriceOption;
		}       

		private void CalcDetail(Contract row)
		{
			decimal pendingSetup = 0;
			decimal pendingRecurring = 0;
			decimal pendingRenewal = 0;
			decimal currentSetup = 0;
			decimal currentRecurring = 0;
			decimal currentRenewal = 0;
			foreach (PXResult<ContractDetail, ContractItem> res in ContractDetails.View.SelectMultiBound(new object[] { row }))
			{
				ContractDetail detail = (ContractDetail)res;
				ContractItem item = (ContractItem)res;
				pendingSetup += detail.Change.GetValueOrDefault() * detail.BasePriceVal.GetValueOrDefault() * (100 - detail.BaseDiscountPct.GetValueOrDefault()) / 100;
				pendingRecurring += detail.Qty.GetValueOrDefault() * detail.FixedRecurringPriceVal.GetValueOrDefault() * (100 - detail.RecurringDiscountPct.GetValueOrDefault()) / 100;
				pendingRenewal += detail.Qty.GetValueOrDefault() * detail.RenewalPriceVal.GetValueOrDefault() * (100 - detail.RenewalDiscountPct.GetValueOrDefault()) / 100;
				currentSetup += detail.LastQty.GetValueOrDefault() * detail.BasePriceVal.GetValueOrDefault() * (100 - detail.LastBaseDiscountPct.GetValueOrDefault()) / 100;
				currentRecurring += detail.LastQty.GetValueOrDefault() * detail.FixedRecurringPriceVal.GetValueOrDefault() * (100 - detail.LastRecurringDiscountPct.GetValueOrDefault()) / 100;
				currentRenewal += detail.LastQty.GetValueOrDefault() * detail.RenewalPriceVal.GetValueOrDefault() * (100 - detail.LastRenewalDiscountPct.GetValueOrDefault()) / 100;
			}
			row.PendingSetup = pendingSetup;
			row.PendingRecurring = pendingRecurring;
			row.PendingRenewal = pendingRenewal;
			row.CurrentSetup = row.Status == Contract.status.Active ? pendingSetup : currentSetup;
			row.CurrentRecurring = row.Status ==  Contract.status.Active ? pendingRecurring : currentRecurring;
			row.CurrentRenewal = row.Status ==  Contract.status.Active ? pendingRenewal : currentRenewal;
			row.TotalPending = row.PendingRecurring + row.PendingRenewal + row.PendingSetup;
		}

		private void CalcSummary(PXCache sender, Contract row)
		{
			//balance = Invoice + DebitMemo - CreditMemo
			PXSelectBase<ARInvoice> selectInvoices = new PXSelectGroupBy<ARInvoice, Where<ARInvoice.projectID, Equal<Required<Contract.contractID>>, And<ARInvoice.docType, NotEqual<ARDocType.creditMemo>, And<ARInvoice.released, Equal<True>>>>, Aggregate<Sum<ARInvoice.curyDocBal>>>(this);
			PXSelectBase<ARInvoice> selectCreditMemo = new PXSelectGroupBy<ARInvoice, Where<ARInvoice.projectID, Equal<Required<Contract.contractID>>, And<ARInvoice.docType, Equal<ARDocType.creditMemo>, And<ARInvoice.released, Equal<True>>>>, Aggregate<Sum<ARInvoice.curyDocBal>>>(this);
			ARInvoice aggregate = selectInvoices.SelectSingle(row.ContractID) ?? new ARInvoice();
			ARInvoice aggregateCM = selectCreditMemo.SelectSingle(row.ContractID) ?? new ARInvoice();
			row.Balance = (aggregate.CuryDocBal ?? 0m) - (aggregateCM.CuryDocBal ?? 0m);

			decimal? totalRecurring = 0m;
			decimal? totalOveruse = 0m;
			foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in RecurringDetails.View.SelectMultiBound(new object[] { row }))
			{
				ContractDetail detail = res;
				ContractItem item = res;

				if (item.DepositItemID == null)
				{
					totalRecurring += detail.Qty.GetValueOrDefault() * detail.FixedRecurringPriceVal.GetValueOrDefault() * (100 - detail.RecurringDiscountPct.GetValueOrDefault()) * 0.01m;
					decimal overuse;
					if (item.ResetUsageOnBilling == true)
					{
						overuse = detail.Used.GetValueOrDefault() - detail.Qty.GetValueOrDefault();
					}
					else
					{
						decimal billedQty = detail.UsedTotal.GetValueOrDefault() - detail.Used.GetValueOrDefault();
						decimal available = detail.Qty.GetValueOrDefault() - billedQty;
						overuse = detail.Used.GetValueOrDefault() - Math.Max(available, 0);
					}
					if (overuse > 0)
						totalOveruse += overuse * detail.UsagePriceVal.GetValueOrDefault();
				}
			}
			CTBillEngine biller = CreateInstance<CTBillEngine>();
			try
			{
				totalOveruse += biller.RecalcDollarUsage(row);
			}
			catch (Exception)
			{
				totalOveruse = null;
				sender.RaiseExceptionHandling<Contract.totalUsage>(row, null, new PXSetPropertyException(Messages.CannotCalculateValue, PXErrorLevel.Warning));
			}
			row.TotalRecurring = totalRecurring;
			row.TotalUsage = totalOveruse;
			row.TotalDue = totalRecurring + totalOveruse;
		}
								
		protected virtual void RenewExpiring(ContractMaint graph, DateTime renewalDate, string contractCD)
		{
			Contract renewed = PXSelect<Contract, Where<Contract.originalContractID, Equal<Current<Contract.contractID>>>>.Select(this);
			if (renewed != null)
				throw new PXException(Messages.ContractAlreadyRenewed, renewed.ContractCD);

			Contract newContract = PXCache<Contract>.CreateCopy(Contracts.Current);

			PXDBLocalizableStringAttribute.CopyTranslations<Contract.description, Contract.description>
					(CurrentContract.Cache, CurrentContract.Current, CurrentContract.Cache, newContract);

			newContract.ContractCD = contractCD;
			newContract.ContractID = null;
			newContract.NoteID = Guid.NewGuid();

			newContract.IsCompleted = false;
			newContract.IsActive = false;
			newContract.IsPendingUpdate = false;
			newContract.RevID = 1;
			newContract.LastActiveRevID = null;
			newContract.Status = Contract.status.Draft;

			ContractBillingSchedule schedule = Billing.Select();
			if (IsExpired(Contracts.Current, renewalDate) && schedule != null && schedule.Type == BillingType.OnDemand)
			{
				Contracts.Current.IsCompleted = true;
				Contracts.Current.Status = Contract.status.Expired;
				Contracts.Update(Contracts.Current);
			}

			newContract.OriginalContractID = Contracts.Current.ContractID;
			newContract.StartDate = GetNextStartDate();
			newContract.ActivationDate = newContract.StartDate;
			newContract.RenewalBillingStartDate = null;
			newContract.ExpireDate = null;
			newContract.LineCtr = 0;

			//We have to drop these fields so that RowInserted does its work before any call to CheckBillingAccount
			newContract.CustomerID = null;
			newContract.LocationID = null;
			newContract = graph.Contracts.Insert(newContract);
			
			#region Setup Billing
			ContractBillingSchedule billingCurrent = Billing.Select();
			graph.Billing.Current.Type = billingCurrent.Type;
			graph.Billing.Current.BillTo = billingCurrent.BillTo;
			graph.Billing.Update(graph.Billing.Current);
			#endregion
			
			//Now, with Billing.Current having been set up in the graph we can update Customer
			newContract.CustomerID = Contracts.Current.CustomerID;
			newContract.LocationID = Contracts.Current.LocationID;
			newContract = graph.Contracts.Update(newContract);

			#region Copy SLA
			graph.SLAMapping.Cache.Clear();

			PXSelectReadonly<ContractSLAMapping, Where<ContractSLAMapping.contractID, Equal<Current<Contract.contractID>>>>.Select(this)
			.RowCast<ContractSLAMapping>()
			.ForEach(item =>
			{
				graph.SLAMapping.Insert(new ContractSLAMapping
			{
					ContractID = newContract.ContractID,
					Severity = item.Severity,
					Period = item.Period
				});
			});
			#endregion
			
			#region Copy Watchers
			var listWatchers = PXSelectReadonly<SelContractWatcher, Where<SelContractWatcher.contractID, Equal<Current<Contract.contractID>>>>.Select(this).AsEnumerable();
			foreach (SelContractWatcher watcher in listWatchers.Cast<SelContractWatcher>().Select(item => (SelContractWatcher) Watchers.Cache.CreateCopy(item)))
			{
				watcher.ContractID = newContract.ContractID;
				graph.Watchers.Insert(watcher);
			}  
			#endregion

			#region CSAnswers
			PXResultset<CSAnswers> cSAnswerList = PXSelectReadonly<CSAnswers, 
				Where<CSAnswers.refNoteID, Equal<Current<Contract.noteID>>>>.Select(this);
			foreach (CSAnswers newAnswer in cSAnswerList.RowCast<CSAnswers>().Select(cSAnswer => (CSAnswers) Answers.Cache.CreateCopy(cSAnswer)))
			{
			    newAnswer.RefNoteID = newContract.NoteID;
                graph.Answers.Insert(newAnswer);
			}
			#endregion

			PXResultset<ContractDetail> items = PXSelect<ContractDetail,
						Where<ContractDetail.contractID, Equal<Current<Contract.contractID>>>>.Select(this);

			foreach (ContractDetail item in items)
			{
				CopyContractDetail(item, graph.ContractDetails.Insert(new ContractDetail()));
				ContractDetail newItem = PXCache<ContractDetail>.CreateCopy(item) as ContractDetail;
				ContractDetails.Cache.Remove(newItem);
				newItem.RevID += 1;

				newItem.NoteID = Guid.NewGuid();

				PXDBLocalizableStringAttribute.CopyTranslations<ContractDetail.description, ContractDetail.description>
					(ContractDetails.Cache, item, ContractDetails.Cache, newItem);

				ContractDetails.Insert(newItem);
			}

			if (CurrentTemplate.Current.RefreshOnRenewal == true)
			{
				foreach (ContractDetail item in graph.ContractDetails.Select())
				{
					ContractDetail templateItem = PXSelect<ContractDetail,
						Where<ContractDetail.contractID, Equal<Current<Contract.templateID>>,
						And<ContractDetail.contractItemID, Equal<Required<ContractDetail.contractItemID>>>>>.Select(this, item.ContractItemID);

					if (templateItem != null)
					{
						graph.CopyTemplateDetail(templateItem, item);

						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.basePrice>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPrice>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPrice>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePrice>(item);

						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.basePriceOption>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPriceOption>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPriceOption>(item);
						graph.ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePriceOption>(item);

						graph.ContractDetails.Update(item);
					}
				}
			}
		}
		
		protected virtual DateTime GetNextStartDate()
		{
			if (CurrentTemplate.Current != null)
			{
				if (CurrentTemplate.Current.IsContinuous == true || !IsExpired(Contracts.Current, Accessinfo.BusinessDate.Value))
				{
					return Contracts.Current.ExpireDate.Value.AddDays(1);
				}
				else
				{
					return Accessinfo.BusinessDate.Value;
				}
			}
			else
			{
				return Contracts.Current.ExpireDate.Value.AddDays(1);
			}
		}

		public static bool IsExpired(Contract row, DateTime businessDate)
		{
			return row.ExpireDate != null && (businessDate.Date.Subtract(row.ExpireDate.Value)).Days > (row.GracePeriod ?? 0);
		}

		public static bool IsInGracePeriod(Contract row, DateTime businessDate, out int daysLeft)
		{
			daysLeft = 0;
			
			if (row.ExpireDate != null)
			{
				int daysAfterExpire = ((TimeSpan)businessDate.Subtract(row.ExpireDate.Value.Date)).Days;

				if (daysAfterExpire > 0 && daysAfterExpire < (row.GracePeriod ?? 0))
				{
					daysLeft = (row.GracePeriod ?? 0) - daysAfterExpire;
					return true;
				}
				else
					return false;

			}
			else
				return false;
		}

		protected bool IsMultyCurrency
		{
			get { return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>(); }
		}

		private void CheckBillingAccount(ContractBillingSchedule schedule)
		{
			if (schedule.AccountID == null && schedule.BillTo == ContractBillingSchedule.billTo.ParentAccount)
			{
				Billing.Cache.RaiseExceptionHandling<ContractBillingSchedule.billTo>(schedule, ContractBillingSchedule.billTo.ParentAccount, new PXSetPropertyException(Messages.CustomerDoesNotHaveParentAccount));
			}
			if (schedule.AccountID != null)
			{
				if (CurrentContract.Current != null)
				{
					Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, schedule.AccountID);
					CustomerClass customerClass = PXSelect<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<Customer.customerClassID>>>>.Select(this, customer.CustomerClassID);
					string customerCury = customer.CuryID ?? customerClass.CuryID;
					if (CurrentContract.Current.CuryID != null && CurrentContract.Current.CuryID != customerCury && customer.AllowOverrideCury != true)
					{
						Billing.Cache.RaiseExceptionHandling<ContractBillingSchedule.accountID>(schedule, customer.AcctCD, new PXSetPropertyException(Messages.CustomerCuryNotMatchWithContractCury));
					}
				}
			}
		}

		public static bool IsValidDetailPrice(PXGraph graph, ContractDetail detail, out string message)
		{
			PXCache cache = graph.Caches<ContractDetail>();
			return (IsValidPrice<ContractDetail.basePriceVal>(cache, detail, out message)
				&& IsValidPrice<ContractDetail.fixedRecurringPriceVal>(cache, detail, out message)
				&& IsValidPrice<ContractDetail.renewalPriceVal>(cache, detail, out message));
		}

		public static bool IsValidDetailPrice(PXGraph graph, ContractDetail detail)
		{
			string message;
			return IsValidDetailPrice(graph, detail, out message);
		}

		private static bool IsValidPrice<PriceField>(PXCache cache, ContractDetail detail, out string message) 
			where PriceField : IBqlField
		{
			message = detail != null && cache.GetValue<PriceField>(detail) == null ? PXUIFieldAttribute.GetDisplayName<PriceField>(cache) : null;
			return message == null;
		}

		#region Local Types
		[Serializable]
		public partial class SetupSettingsFilter : IBqlTable
			{
			#region StartDate
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
			protected DateTime? _StartDate;
			[PXDefault(typeof(Contract.startDate))]
			[PXDBDate]
			[PXUIField(DisplayName = "Setup Date", Required = true)]
			public virtual DateTime? StartDate
			{
				get
				{
					return this._StartDate;
				}
				set
				{
					this._StartDate = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class ActivationSettingsFilter : IBqlTable
		{
			#region ActivationDate
			public abstract class activationDate : PX.Data.BQL.BqlDateTime.Field<activationDate> { }
			protected DateTime? _ActivationDate;
			[PXFormula(typeof(Switch<
				Case<Where<Current<Contract.status>, Equal<Contract.status.inUpgrade>, And<Current<Contract.effectiveFrom>, IsNotNull>>, Current<Contract.effectiveFrom>>,
				Current<Contract.activationDate>>))]
			[PXDBDate]
			[PXUIField(DisplayName = "Activation Date", Required = true)]
			public virtual DateTime? ActivationDate
			{
				get
				{
					return this._ActivationDate;
				}
				set
				{
					this._ActivationDate = value;
				}
			}
			#endregion
			#region ActionName
			public abstract class actionName : PX.Data.BQL.BqlString.Field<actionName> { }
			[PXString]
			[PXUIField(DisplayName = "Action name", Visibility = PXUIVisibility.Invisible)]
			public virtual string ActionName
			{
				get;
				set;
			}
			#endregion
		}

		[Serializable]
		public partial class TerminationSettingsFilter : IBqlTable
		{
			#region TerminationDate
			public abstract class terminationDate : PX.Data.BQL.BqlDateTime.Field<terminationDate> { }
			protected DateTime? _TerminationDate;
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXDate()]
			[PXUIField(DisplayName = "Termination Date", Required = true)]
			public virtual DateTime? TerminationDate
			{
				get
				{
					return this._TerminationDate;
				}
				set
				{
					this._TerminationDate = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class BillingOnDemandSettingsFilter : IBqlTable
		{
			#region BillingDate
			public abstract class billingDate : PX.Data.BQL.BqlDateTime.Field<billingDate> { }
			protected DateTime? _BillingDate;
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXDate()]
			[PXUIField(DisplayName = "Billing Date", Required = true)]
			public virtual DateTime? BillingDate
			{
				get
				{
					return this._BillingDate;
				}
				set
				{
					this._BillingDate = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class RenewalSettingsFilter : IBqlTable
		{
			#region RenewalDate
			public abstract class renewalDate : PX.Data.BQL.BqlDateTime.Field<renewalDate> { }
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXDBDate]
			[PXUIField(DisplayName = "Renewal Date", Required = true)]
			public virtual DateTime? RenewalDate { get; set; }
			#endregion
		}
		#endregion
	}

	[PXProjection(typeof(Select2<ContractWatcher, RightJoin<Contact, On<Contact.contactID, Equal<ContractWatcher.contactID>>>>), Persistent = true)]
	[Serializable]
	public partial class SelContractWatcher : ContractWatcher
	{
		#region DisplayName
		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
		protected String _displayName;
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ContactDisplayName(typeof(SelContractWatcher.lastName), typeof(SelContractWatcher.firstName),
			typeof(SelContractWatcher.midName), typeof(SelContractWatcher.title), true, 
			BqlField = typeof(Contact.displayName))]
		public virtual String DisplayName
		{
			get { return _displayName; }
			set { _displayName = value; }
		}
		#endregion
		#region FirstName
		public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }
		protected String _FirstName;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.firstName))]
		[PXUIField(DisplayName = "First Name")]
		public virtual String FirstName
		{
			get
			{
				return this._FirstName;
			}
			set
			{
				this._FirstName = value;
			}
		}
		#endregion
		#region MidName
		public abstract class midName : PX.Data.BQL.BqlString.Field<midName> { }
		protected String _MidName;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.midName))]
		[PXUIField(DisplayName = "Middle Name")]
		public virtual String MidName
		{
			get
			{
				return this._MidName;
			}
			set
			{
				this._MidName = value;
			}
		}
		#endregion
		#region LastName
		public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }
		protected String _LastName;
		[PXDBString(100, IsUnicode = true, BqlField = typeof(Contact.lastName))]
		[PXUIField(DisplayName = "Last Name")]
		public virtual String LastName
		{
			get
			{
				return this._LastName;
			}
			set
			{
				this._LastName = value;
			}
		}
		#endregion
		#region Title
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
		protected String _Title;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.title))]
		[Titles]
		[PXUIField(DisplayName = "Title")]
		public virtual String Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				this._Title = value;
			}
		}
		#endregion
		#region Salutation
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		protected String _Salutation;
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.salutation))]
		[PXUIField(DisplayName = "Attention", Visibility = PXUIVisibility.SelectorVisible)]
		//[PXParentSearch()]
		public virtual String Salutation
		{
			get
			{
				return this._Salutation;
			}
			set
			{
				this._Salutation = value;
			}
		}
		#endregion
		#region Phone1
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
		protected String _Phone1;
		[PXDBString(50, BqlField = typeof(Contact.phone1))]
		[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PhoneValidation()]
		public virtual String Phone1
		{
			get
			{
				return this._Phone1;
			}
			set
			{
				this._Phone1 = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBInt(IsKey = false, BqlField = typeof(Contact.bAccountID))]
		[PXDimensionSelector("BIZACCT", typeof(Search<BAccount.bAccountID>), typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName), DirtyRead = true)]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region ContactContactID
		public abstract class contactContactID : PX.Data.BQL.BqlInt.Field<contactContactID> { }
		protected Int32? _ContactContactID;
		[PXDBInt(BqlField = typeof(Contact.contactID))]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		[PXExtraKey]
		public virtual Int32? ContactContactID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		public new abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
	}

	[Serializable]
	public partial class RenewManualNumberingFilter : IBqlTable
	{
		#region ContractCD
		public abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
		[PXDBString(IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Contract ID")]
		[PXDefault]
		public virtual string ContractCD { get; set; }
		#endregion
	}
}
