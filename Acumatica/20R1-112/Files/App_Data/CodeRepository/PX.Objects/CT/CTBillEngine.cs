using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PX.Common.Parser;

using PX.Data;

using PX.Objects.AR;
using PX.Objects.AR.Repositories;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.CS;
using PX.Objects.Common.Discount;

namespace PX.Objects.CT
{
	public class CTBillEngine : PXGraph<CTBillEngine>, IContractInformation
	{
		public class ARContractInvoiceEntry : ARInvoiceEntry
		{
			[PXDBBool]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			protected virtual void ARInvoice_RetainageApply_CacheAttached(PXCache sender)
			{
			}
		}

		public PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>> Contracts;

		public PXSelect<
			ContractBillingSchedule, 
			Where<ContractBillingSchedule.contractID, Equal<Optional<Contract.contractID>>>>
			BillingSchedule;

		public PXSelect<
			ContractDetail, 
			Where<ContractDetail.contractID, Equal<Required<Contract.contractID>>>> 
			ContractDetails;

		public PXSelect<
			ContractDetailExt, 
			Where<ContractDetailExt.contractID, Equal<Required<Contract.contractID>>>> 
			ContractDetailsExt;

		public PXSelect<PMTran> Transactions;
		public PXSelect<ContractRenewalHistory> RenewalHistory;

		public PXSelect<
			ContractRenewalHistory, 
			Where<
				ContractRenewalHistory.contractID, Equal<Required<Contract.contractID>>,
				And<ContractRenewalHistory.revID, Equal<Required<Contract.revID>>>>> 
			CurrentRenewalHistory;

		public PXSelect<
			ContractBillingTrace, 
			Where<ContractBillingTrace.contractID, Equal<Required<ContractBillingTrace.contractID>>>, 
			OrderBy<Desc<ContractBillingTrace.recordID>>> 
			BillingTrace;

		public PXSetupOptional<INSetup> insetup;
		public PXSetupOptional<CommonSetup> commonsetup;
		public PXSetupOptional<CMSetup> cmsetup; 
		public PXSelect<ContractItem> contractItem;
		public PXSetup<GL.Company> company;

		public PXSetup<
			Location, 
			Where<
				Location.bAccountID, Equal<Current<ContractBillingSchedule.accountID>>, 
				And<Location.locationID, Equal<Current<ContractBillingSchedule.locationID>>>>> 
			BillingLocation;

		protected List<ARRegister> doclist = new List<ARRegister>();
		protected Dictionary<int?, decimal?> availableQty = new Dictionary<int?, decimal?>();
		protected Dictionary<int?, decimal?> availableDeposit = new Dictionary<int?, decimal?>();
		protected Dictionary<int?, UsageData> depositUsage = new Dictionary<int?, UsageData>();
		protected List<UsageData> nonRefundableDepositedUsage = new List<UsageData>();
		protected Dictionary<int?, ContractItem> nonRefundableDeposits = new Dictionary<int?, ContractItem>();
		protected Dictionary<int?, ContractItem> refundableDeposits = new Dictionary<int?, ContractItem>();
		
		protected CustomerRepository customerRepository;

		[PXBool]
		[PXDBScalar(typeof(Search<ContractTemplate.detailedBilling, Where<ContractTemplate.contractID, Equal<Contract.templateID>>>))]
		protected virtual void Contract_DetailedBilling_CacheAttached(PXCache sender) { }

		[PXDBInt(IsKey = true)]
		protected virtual void ContractDetail_ContractID_CacheAttached(PXCache sender) { }

		[PXDBInt(IsKey = true)]
		[PXDimensionSelector(
			ContractItemAttribute.DimensionName, 
			typeof(Search<ContractItem.contractItemID>),
																	typeof(ContractItem.contractItemCD),
																	typeof(ContractItem.contractItemCD), typeof(ContractItem.descr))]
		protected virtual void ContractDetail_ContractItemID_CacheAttached(PXCache sender) { }

		[PXDBInt]
		[PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<ContractDetail.contractID>>>>))]
		[PXParent(typeof(Select<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<ContractDetail.contractID>>>>))]
		protected virtual void ContractDetail_RevID_CacheAttached(PXCache sender) { }

		[PXDBInt]
		protected virtual void ContractDetail_LineNbr_CacheAttached(PXCache sender) { }

		public ContractBillingSchedule contractBillingSchedule => BillingSchedule.Select();

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDimensionSelector(
			InventoryAttribute.DimensionName, 
			typeof(Search2<
				InventoryItem.inventoryID,
					LeftJoin<ARSalesPrice, 
						On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>,
					And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>, 
					And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
					LeftJoin<ARSalesPrice2, 
						On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, 
					And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>,
				Where<InventoryItem.stkItem, Equal<False>>>), 
			typeof(InventoryItem.inventoryCD))]
		public void ContractItem_BaseItemID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDimensionSelector(
			InventoryAttribute.DimensionName, 
			typeof(Search2<
				InventoryItem.inventoryID,
					LeftJoin<ARSalesPrice, 
						On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>, 
					And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
					LeftJoin<ARSalesPrice2, 
						On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, 
					And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>,
				Where<InventoryItem.stkItem, Equal<False>>>), 
			typeof(InventoryItem.inventoryCD))]
		public void ContractItem_RenewalItemID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDimensionSelector(InventoryAttribute.DimensionName, 
			typeof(Search2<
				InventoryItem.inventoryID,
					LeftJoin<ARSalesPrice, 
						On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>, 
					And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>>>>>,
					LeftJoin<ARSalesPrice2, 
						On<ARSalesPrice2.inventoryID, Equal<InventoryItem.inventoryID>, 
					And<ARSalesPrice2.uOM, Equal<InventoryItem.baseUnit>, 
					And<ARSalesPrice2.custPriceClassID, Equal<Current<Location.cPriceClassID>>, 
					And<ARSalesPrice2.curyID, Equal<Current<ContractItem.curyID>>>>>>>>,
				Where<InventoryItem.stkItem, Equal<False>>>), 
			typeof(InventoryItem.inventoryCD))]
		public void ContractItem_RecurringItemID_CacheAttached(PXCache sender) { }

		#region UsagePriceVal
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFormula(typeof(GetItemPriceValue<
			ContractDetail.contractID, 
			ContractDetail.contractItemID, 
			ContractDetailType.ContractDetailUsagePrice, 
			ContractDetail.usagePriceOption, 
			Selector<ContractDetail.contractItemID, ContractItem.recurringItemID>, 
			ContractDetail.usagePrice, 
			ContractDetail.basePriceVal,
			ContractDetail.qty,
			Parent<ContractBillingSchedule.nextDate>>))]
		public void ContractDetail_UsagePriceVal_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFormula(typeof(GetItemPriceValue<
			ContractDetailExt.contractID, 
			ContractDetailExt.contractItemID, 
			ContractDetailType.ContractDetailUsagePrice, 
			ContractDetail.usagePriceOption, 
			Selector<ContractDetailExt.contractItemID, ContractItem.recurringItemID>, 
			ContractDetail.usagePrice, 
			ContractDetail.usagePrice,
			ContractDetail.qty,
			Parent<ContractBillingSchedule.nextDate>>))]
		public void ContractDetailExt_UsagePriceVal_CacheAttached(PXCache sender) { }
		#endregion
		#region DescriptionGenerator
		
		protected virtual object ConvertFromExtValue(object extValue)
		{
			PXFieldState fs = extValue as PXFieldState;
			if (fs != null)
				return fs.Value;
			else
			{
				return extValue;
			}
		}
		protected virtual object EvaluateAttribute(string attribute, Guid? refNoteID)
		{
			PXResultset<CSAnswers> res = PXSelectJoin<CSAnswers,
				InnerJoin<CSAttribute, On<CSAttribute.attributeID, Equal<CSAnswers.attributeID>>>,
				Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>,
					And<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>>.Select(this, refNoteID, attribute);

			CSAnswers ans = null;
			CSAttribute attr = null;
			if (res.Count > 0)
			{
				ans = (CSAnswers)res[0][0];
				attr = (CSAttribute)res[0][1];
			}

			if (ans == null || ans.AttributeID == null)
			{
				//answer not found. if attribute exists return the default value.
				attr = PXSelect<CSAttribute, Where<CSAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>>.Select(this, attribute);

				if (attr != null && attr.ControlType == CSAttribute.CheckBox)
				{
					return false;
				}
			}

			if (ans != null)
			{
				if (ans.Value != null)
					return ans.Value;
				else
				{
					if (attr != null && attr.ControlType == CSAttribute.CheckBox)
					{
						return false;
					}
				}
			}

			return string.Empty;
		}
		object IContractInformation.Evaluate(CTObjectType objectName, string fieldName, string attribute, CTFormulaDescriptionContainer row)
		{
			Contract contract;
			Customer customer;
			switch (objectName)
			{
				case CTObjectType.Contract:
					contract = (Contract)PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, row.ContractID);
					if (contract != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, contract.NoteID);
						}
						else
						{
							return ConvertFromExtValue(this.Caches[typeof(Contract)].GetValueExt(contract, fieldName));
						}
					}
					break;
				case CTObjectType.ContractTemplate:
					contract = (Contract)PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, row.ContractID);
					ContractTemplate contractTemplate = (ContractTemplate)PXSelect<ContractTemplate, Where<ContractTemplate.contractID, Equal<Required<ContractTemplate.contractID>>>>.Select(this, contract.TemplateID);
					if (contractTemplate != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, contractTemplate.NoteID);
						}
						else
						{
							return ConvertFromExtValue(this.Caches[typeof(ContractTemplate)].GetValueExt(contractTemplate, fieldName));
						}
					}
					break;
				case CTObjectType.Customer:
					customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.CustomerID);
					if (customer != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, customer.NoteID);
						}
						else
						{
							return ConvertFromExtValue(this.Caches[typeof(Customer)].GetValueExt(customer, fieldName));
						}
					}
					break;
				case CTObjectType.Location:
					Location customerLocation = (Location)PXSelect<Location, Where<Location.locationID, Equal<Required<Location.locationID>>>>.Select(this, row.CustomerLocationID);
					if (customerLocation != null)
					{
						return ConvertFromExtValue(this.Caches[typeof(Location)].GetValueExt(customerLocation, fieldName));
					}
					break;
				case CTObjectType.ContractItem:
					ContractItem contractItem = (ContractItem)PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractItem.contractItemID>>>>.Select(this, row.ContractItemID);
					if (contractItem != null)
					{
						return ConvertFromExtValue(this.Caches[typeof(ContractItem)].GetValueExt(contractItem, fieldName));
					}
					break;
				case CTObjectType.ContractDetail:
					ContractDetail contractDetail = (ContractDetail)PXSelect<ContractDetail, Where<ContractDetail.contractDetailID, Equal<Required<ContractDetail.contractDetailID>>>>.Select(this, row.ContractDetailID);
					if (contractDetail != null)
					{
						return ConvertFromExtValue(this.Caches[typeof(ContractDetail)].GetValueExt(contractDetail, fieldName));
					}
					break;
				case CTObjectType.InventoryItem:
					InventoryItem inventoryItem = (InventoryItem)PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
					if (inventoryItem != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, inventoryItem.NoteID);
						}
						else
						{
							return ConvertFromExtValue(this.Caches[typeof(InventoryItem)].GetValueExt(inventoryItem, fieldName));
						}
					}
					break;
				case CTObjectType.ContractBillingSchedule:
					ContractBillingSchedule contractBillingSchedule = (ContractBillingSchedule)PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Required<ContractBillingSchedule.contractID>>>>.Select(this, row.ContractID);
					if (contractBillingSchedule != null)
					{
						return ConvertFromExtValue(this.Caches[typeof(ContractBillingSchedule)].GetValueExt(contractBillingSchedule, fieldName));
					}
					break;
				case CTObjectType.PMTran:
					if (row.pmTranIDs.Count == 1)
					{
						PMTran pmTran = (PMTran)PXSelect<PMTran, Where<PMTran.tranID, Equal<Required<PMTran.tranID>>>>.Select(this, row.pmTranIDs[0]);
						if (pmTran != null)
						{
							return ConvertFromExtValue(this.Caches[typeof(PMTran)].GetValueExt(pmTran, fieldName));
						}
					
					}
					break;
				case CTObjectType.UsageData:
					if (row.usageData != null)
					{
						return ConvertFromExtValue(this.Caches[typeof(UsageData)].GetValueExt(row.usageData, fieldName));
					}
					break;
				case CTObjectType.AccessInfo:
						return ConvertFromExtValue(this.Caches[typeof(AccessInfo)].GetValueExt(Accessinfo, fieldName));
				default:
					break;
			}

					return null;
		}

		string IContractInformation.GetParametrActionInvoice(CTFormulaDescriptionContainer tran)
		{
			return tran.ActionInvoice;
		}
		string IContractInformation.GetParametrActionItem(CTFormulaDescriptionContainer tran)
		{
			return tran.ActionItem;
		}
		string IContractInformation.GetParametrInventoryPrefix(CTFormulaDescriptionContainer tran)
		{
			return tran.InventoryPrefix;
		}
		/// <summary>
		/// Parameter which defines a variant of description for inventory in accordance with contract action
		/// </summary>
		private enum InventoryAction
		{
			Setup,
			ActivateRenew,
			SetupUpgrade,
			UpgradeActivation
		}
		/// <summary>
		/// Returns an inventory item description in accordance with an action
		/// </summary>
		/// <param name="action">Inventory action from list <see = InventoryAction/></param>
		/// <param name="inventory">Inventory item for which a description generated</param>
		/// <returns>Item description</returns>
		private string GetDescriptionMessageForInventory(InventoryAction action, InventoryItem inventory)
		{
			return PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory);
		}
		/// <summary>
		/// Returns an invoice description in accordance with an contract action
		/// </summary>
		/// <param name="action">Contract action from list <see = ContractAction/></param>
		/// <param name="inventory">Invoice for which a description generated</param>
		/// <returns>Invoice description</returns>
		private string GetInvoiceDescription(string action, Contract contract, Customer customer, ARInvoice invoice)
		{
			string contractDescriptions = null;
			CTFormulaDescriptionContainer container = new CTFormulaDescriptionContainer();
			container.ContractID = contract.ContractID;
			container.CustomerID = customer.BAccountID;
			container.CustomerLocationID = invoice.CustomerLocationID;

			using (new PXLocaleScope(customer.LocaleName))
			{
				switch (action)
				{
					case ContractAction.Setup:
						container.ActionInvoice = PXMessages.LocalizeNoPrefix(Messages.ActionInvoiceSettingUpContract);
						break;
					case ContractAction.Activate:
						container.ActionInvoice = PXMessages.LocalizeNoPrefix(contract.IsPendingUpdate == true ? Messages.ActionInvoiceUpgradingContract : Messages.ActionInvoiceActivatingContract);
						break;
					case ContractAction.Terminate:
						container.ActionInvoice = PXMessages.LocalizeNoPrefix(Messages.ActionInvoiceTerminatingContract);
						break;
					case ContractAction.Bill:
						container.ActionInvoice = PXMessages.LocalizeNoPrefix(Messages.ActionInvoiceBillingContract);
						break;
					case ContractAction.Renew:
						container.ActionInvoice = PXMessages.LocalizeNoPrefix(Messages.ActionInvoiceRenewingContract);
						break;
					default:
						container.ActionInvoice = PXMessages.LocalizeNoPrefix(Messages.ActionInvoiceActivatingContract);
						break;
				}
			}
			var billing = (ContractBillingSchedule)PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Required<Contract.contractID>>>>.Select(this, contract.ContractID).FirstOrDefault();

			CTDataNavigator navigator = new CTDataNavigator(this, new List<CTFormulaDescriptionContainer>(new CTFormulaDescriptionContainer[1] { container }));
			ExpressionNode descNode = CTExpressionParser.Parse(this, billing.InvoiceFormula);
			descNode.Bind(navigator);
			object val = descNode.Eval(container);
			if (val != null)
			{
				contractDescriptions += val.ToString();
			}
			return contractDescriptions;
		}
		/// <summary>
		/// Returns an transaction description with a prefix if it is or without otherwise
		/// </summary>
		/// <param name="prefix">Description prefix or empty string</param>
		/// <param name="description">Description</param>
		/// <returns>Description with prefix or without</returns>
		private string GetTransactionDescriptionWithPrefix(ARTran row, UsageData item)
		{
			string transactionDescription = null;
			var customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.CustomerID);
			using (new PXLocaleScope(customer.LocaleName))
			{
				CTFormulaDescriptionContainer container = new CTFormulaDescriptionContainer();
				container.InventoryPrefix = item.Prefix;
				container.CustomerID = row.CustomerID;
				container.ContractID = row.ProjectID;
				container.InventoryID = row.InventoryID;
				container.ActionItem = item.ActionItem;
				container.ContractItemID = item.ContractItemID;
				container.ContractDetailID = item.ContractDetailID;
				container.pmTranIDs = item.TranIDs;
				container.usageData = item;
				var billing = (ContractBillingSchedule)PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Required<Contract.contractID>>>>.Select(this, row.ProjectID).FirstOrDefault();

				CTDataNavigator navigator = new CTDataNavigator(this, new List<CTFormulaDescriptionContainer>(new CTFormulaDescriptionContainer[1] { container }));
				ExpressionNode descNode = CTExpressionParser.Parse(this, billing.TranFormula);
				descNode.Bind(navigator);
				object val = descNode.Eval(container);
				if (val != null)
				{
					transactionDescription += val.ToString();
				}
			}
			return transactionDescription;
		}
		private string GetInvoiceDescriptionRenewing(Contract contract, Customer customer)
		{
			string docDesc;
			using (new PXLocaleScope(customer.LocaleName))
				docDesc = string.Format(PXMessages.LocalizeNoPrefix(Messages.RenewingContract), contract.ContractCD, contract.Description);
			return docDesc;
		}
		private string GetInvoiceDescriptionBilling(Contract contract, Customer customer)
		{
			string DocDesc;
			using (new PXLocaleScope(customer.LocaleName))
			{
				DocDesc = string.Format(PXMessages.LocalizeNoPrefix(Messages.BillingContract),
					contract.ContractCD, PXDBLocalizableStringAttribute.GetTranslation<Contract.description>(Caches[typeof(Contract)], contract));
			}
			return DocDesc;
		}
		#endregion DescriptionGenerator
		public CTBillEngine()
		{
			AutomationView = nameof(Contracts);
			customerRepository = new CustomerRepository(this);
		}

		protected int DecPlPrcCst
		{
			get
			{
				CommonSetup setup = PXSelect<CommonSetup>.Select(this);
				return  setup?.DecPlPrcCst != null ? Convert.ToInt32(setup.DecPlPrcCst) : 2;
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Contract.contractID), SubstituteKey = typeof(Contract.contractCD), ValidateValue = false)]
		public void _(Events.CacheAttached<ContractRenewalHistory.childContractID> e) { }

		private void CreateNewRevision(int? contractID, string action, string newStatus)
		{
			Contract contract = (Contract)Contracts.Cache.CreateCopy(Contracts.SelectSingle(contractID));

			foreach (ContractDetail det in ContractDetails.Select(contract.ContractID)
				.RowCast<ContractDetail>())
			{
				ContractDetail newdet = ContractDetails.Cache.CreateCopy(det) as ContractDetail;

				ContractDetails.Cache.Normalize();
				ContractDetails.Cache.Remove(newdet);
				newdet.RevID += 1;
				newdet.NoteID = null;
				if (action == ContractAction.Setup || action == ContractAction.SetupAndActivate || action == ContractAction.Activate)
				{
					ContractMaint.ValidateUniqueness(this, newdet, true);
				}

				var newdetWithDefaults = ContractDetails.Insert(newdet);
				PXNoteAttribute.CopyNoteAndFiles(ContractDetails.Cache, det, ContractDetails.Cache, newdetWithDefaults);

				if (action == ContractAction.Upgrade)
				{
					ContractMaint.CalculateDiscounts(ContractDetails.Cache, contract, newdetWithDefaults);
					ContractDetails.Update(newdetWithDefaults);
				}
			}
			ContractRenewalHistory history = CurrentRenewalHistory.SelectSingle(contract.ContractID, contract.RevID);
			ContractRenewalHistory newHistory = CurrentRenewalHistory.Cache.CreateCopy(history) as ContractRenewalHistory;
			newHistory.ActionBusinessDate = null; // for correct defaulting by insert
			newHistory.Status = newStatus;
			newHistory.Action = action;
			newHistory.RevID += 1;

			CurrentRenewalHistory.Insert(newHistory);

			if (history.Status == Contract.status.Active)
			{
				contract.LastActiveRevID = history.RevID;
			}
			if (newHistory.Status == Contract.status.Active)
			{
				contract.LastActiveRevID = newHistory.RevID;
			}

			contract.RevID = newHistory.RevID;

			contract.IsLastActionUndoable = true;
			Contracts.Update(contract);
		}

		public static void UpdateContractHistoryEntry(ContractRenewalHistory history, Contract contract, ContractBillingSchedule schedule)
		{
			history.IsActive = contract.IsActive;
			history.IsCancelled = contract.IsCancelled;
			history.IsCompleted = contract.IsCompleted;
			history.IsPendingUpdate = contract.IsPendingUpdate;

			history.ExpireDate = contract.ExpireDate;
			history.EffectiveFrom = contract.EffectiveFrom;
			history.ActivationDate = contract.ActivationDate;
			history.StartDate = contract.StartDate;
			history.TerminationDate = contract.TerminationDate;

			history.DiscountID = contract.DiscountID;

			history.LastDate = schedule.LastDate;
			history.NextDate = schedule.NextDate;
			history.StartBilling = schedule.StartBilling;
			if (contract.Status != Contract.status.Draft)
				history.ChildContractID = null;
		}

		private void UpdateHistory(Contract contract)
		{
			Contract updated = Contracts.SelectSingle(contract.ContractID);
			ContractBillingSchedule schedule = BillingSchedule.SelectSingle(contract.ContractID);
			ContractRenewalHistory history = CurrentRenewalHistory.SelectSingle(updated.ContractID, updated.RevID);

			UpdateContractHistoryEntry(history, updated, schedule);

			CurrentRenewalHistory.Update(history);
		}

		public static void ClearBillingTrace(int? contractID)
		{
			PXDatabase.Delete<ContractBillingTrace>(new PXDataFieldRestrict(nameof(Contract.ContractID), PXDbType.Int, contractID));
		}

		private void ClearFuture(Contract contract)
		{
			if (contract == null || contract.ContractID == null || contract.RevID == null)
				return;

			ClearFutureDetails(contract);
			ClearFutureHistory(contract);
		}

		private void ClearFutureHistory(Contract contract)
		{
			PXDatabase.Delete<ContractRenewalHistory>(
				new PXDataFieldRestrict(nameof(Contract.ContractID), PXDbType.Int, 4, contract.ContractID, PXComp.EQ),
				new PXDataFieldRestrict(nameof(Contract.RevID), PXDbType.Int, 4, contract.RevID, PXComp.GT));
		}

		private void ClearFutureDetails(Contract contract)
		{
			PXDatabase.Delete<ContractDetail>(
				new PXDataFieldRestrict(nameof(Contract.ContractID), PXDbType.Int, 4, contract.ContractID, PXComp.EQ),
				new PXDataFieldRestrict(nameof(Contract.RevID), PXDbType.Int, 4, contract.RevID, PXComp.GT));
		}

		private void ClearState()
		{
			availableQty.Clear();
			availableDeposit.Clear();
			depositUsage.Clear();
			doclist.Clear();
			nonRefundableDepositedUsage.Clear();
			nonRefundableDeposits.Clear();
			refundableDeposits.Clear();
		}

		public bool IsFullyBilledContract(Contract contract)
		{
			if (contract.Status == Contract.status.Expired) return true;

			List<UsageData> data; 
			List<UsageData> tranData;
			Dictionary<int, List<TranNotePair>> sourceTran;
			RecalcUsage(contract, out data, out sourceTran, out tranData);
			return !data.Any(d => d.ExtPrice != 0m || d.Qty != 0m);
		}

		public virtual void Setup(int? contractID, DateTime? date)
		{
			#region Parameter and State Verification

			if (contractID == null) throw new ArgumentNullException(nameof(contractID));
			if (date == null) throw new ArgumentNullException(nameof(date));

			Contract contract = Contracts.Select(contractID);
			ContractBillingSchedule schedule = BillingSchedule.Select(contractID);

			if (contract.IsCompleted == true)
				throw new PXException(Messages.ContractCompletedExpiredCantSetUp);

			if (contract.Status != Contract.status.Draft)
				throw new PXException(Messages.ContractAlreadySetup);

			if (contract.IsCancelled == true)
				throw new PXException(Messages.ContractTerminatedCantSetUp);

			if (contract.ExpireDate != null && date.Value > contract.ExpireDate.Value)
			{
				throw new PXException(Messages.ActivationDateTooLate);
			}


			foreach (PXResult<ContractItem, ContractDetail> res in GetContractDetails(contract))
			{
				ContractItem item = res;
				ContractDetail detail = res;
				string message;
				if (!ContractMaint.IsValidDetailPrice(this, detail, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}
			#endregion

			ClearState();
			ClearBillingTrace(contractID);

			CreateNewRevision(contractID, ContractAction.Setup, Contract.status.PendingActivation);

			contract.StartDate = date;
			Contracts.Update(contract);

			Contract template = Contracts.SelectSingle(contract.TemplateID);
			contract.ScheduleStartsOn = template.ScheduleStartsOn;

			Customer customer;
			Location location;
			SetBillingTarget(contract, out customer, out location);

			List<InvoiceData> invoices = new List<InvoiceData>();
			InvoiceData data = new InvoiceData(date.Value);
			using (new PXLocaleScope(customer.LocaleName))
				data.UsageData.AddRange(GetSetupFee(contract));
			if (contract.ScheduleStartsOn == Contract.scheduleStartsOn.SetupDate)
			{
				using (new PXLocaleScope(customer.LocaleName))
					data.UsageData.AddRange(GetActivationFee(contract, date));
			}
			if (data.UsageData.Count > 0)
				invoices.Add(data);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (invoices.Count > 0)
				{
					using (new PXLocaleScope(customer.LocaleName))
						CreateInvoice(contract, template, invoices, customer, location, ContractAction.Setup);
				}

				if (contract.ScheduleStartsOn == Contract.scheduleStartsOn.SetupDate)
				{
					schedule.LastDate = date.Value;
				}

				foreach (ARRegister doc in doclist)
				{
					BillingTrace.Insert(new ContractBillingTrace
					{
						ContractID = contractID, 
						DocType = doc.DocType, 
						RefNbr = doc.RefNbr, 
						LastDate = schedule.LastDate, 
						NextDate = schedule.NextDate
					});
				}

				if (contract.ScheduleStartsOn == Contract.scheduleStartsOn.SetupDate)
				{
					schedule.StartBilling = date.Value;
					if (schedule.Type != BillingType.OnDemand)
						schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, date, date);
				}
				contract.EffectiveFrom = contract.StartDate;
				BillingSchedule.Update(schedule);
				contract.Status = Contract.status.PendingActivation;
				Contracts.Update(contract);

				UpdateHistory(contract);

				Actions.PressSave();

				ts.Complete();
			}

			EnsureContractDetailTranslations();

			AutoReleaseInvoice(contract);
		}

		public virtual void Activate(int? contractID, DateTime? date, bool setActivationDate=true)
		{
			#region Parameter and State Verification

			if (contractID == null) throw new ArgumentNullException(nameof(contractID));
			if (date == null) throw new ArgumentNullException(nameof(date));

			Contract contract = Contracts.Select(contractID);
			ContractBillingSchedule schedule = BillingSchedule.Select(contractID);

			if (schedule != null)
			{
				object customerID = schedule.AccountID;
				BillingSchedule.Cache.RaiseFieldVerifying<ContractBillingSchedule.accountID>(schedule, ref customerID);
			}

			if (contract.IsCompleted == true)
				throw new PXException(Messages.ContractCompletedExpiredCantActivate);

			if (contract.IsActive == true && contract.IsPendingUpdate != true)
				throw new PXException(Messages.ActiveContractCannotBeActivated);

			if (contract.IsCancelled == true)
				throw new PXException(Messages.ContractTerminatedCantActivate);

			if (date.Value < contract.StartDate.Value)
			{
				throw new PXException(Messages.ActivationDateError);
			}

			if (contract.ExpireDate != null && date.Value > contract.ExpireDate.Value)
			{
				throw new PXException(Messages.ActivationDateTooLate);
			}

			if (contract.IsPendingUpdate == true)
			{
				DateTime minDate = schedule.LastDate ?? contract.StartDate.Value;

				if (date < minDate || (schedule.Type != BillingType.OnDemand && date > schedule.NextDate.Value))
				{
					throw new PXException(Messages.UpdateActivationDateTooEarlyOrTooLate);
				}
			}

			foreach (PXResult<ContractItem, ContractDetail> res in GetContractDetails(contract))
			{
				ContractItem item = res;
				ContractDetail detail = res;
				string message;
				if (!ContractMaint.IsValidDetailPrice(this, detail, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

			if (contract.OriginalContractID != null)
			{
				Contract origContract = Contracts.Select(contract.OriginalContractID);
				if (!IsFullyBilledContract(origContract))
				{
					throw new PXException(Messages.OriginalContractIsNotFullyBilledInActivate, origContract.ContractCD);
				}

				origContract.IsCompleted = true;
				origContract.Status = Contract.status.Expired;
				Contracts.Update(origContract);
			}
			#endregion

			ClearState();
			ClearBillingTrace(contractID);

			CreateNewRevision(contractID, ContractAction.Activate, Contract.status.Active);

			if (setActivationDate)
			{
				contract.ActivationDate = date;
				Contracts.Update(contract);
			}

			Contract template = Contracts.SelectSingle(contract.TemplateID);

			Customer customer;
			Location location;
			SetBillingTarget(contract, out customer, out location);

			List<InvoiceData> invoices = new List<InvoiceData>();

			InvoiceData data = new InvoiceData(date.Value);
			if (contract.IsPendingUpdate != true)
			{
				if (contract.ScheduleStartsOn == Contract.scheduleStartsOn.SetupDate)
				{
					if (schedule.Type != BillingType.OnDemand)
					{
						while (schedule.NextDate < date)
						{
							schedule.LastDate = schedule.NextDate;
							schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, schedule.LastDate, schedule.StartBilling);
						}
					}
					else
					{
						schedule.LastDate = date;
						schedule.NextDate = null;
					}
				}

				if (contract.ScheduleStartsOn == Contract.scheduleStartsOn.ActivationDate)
				{
					using (new PXLocaleScope(customer.LocaleName))
						data.UsageData.AddRange(GetActivationFee(contract, date));
				}
				using (new PXLocaleScope(customer.LocaleName))
					data.UsageData.AddRange(GetPrepayment(contract, date, schedule.LastDate, schedule.NextDate));
			}
			else
			{
				using (new PXLocaleScope(customer.LocaleName))
					data.UsageData.AddRange(GetUpgradeFee(contract, schedule.LastDate, schedule.NextDate, date));
				contract.EffectiveFrom = date;
			}

			if (data.UsageData.Count > 0)
				invoices.Add(data);


			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (invoices.Count > 0)
				{
					using (new PXLocaleScope(customer.LocaleName))
						CreateInvoice(contract, template, invoices, customer, location, ContractAction.Activate);
				}

				foreach (ARRegister doc in doclist)
				{
					BillingTrace.Insert(new ContractBillingTrace
					{
						ContractID = contractID, 
						DocType = doc.DocType, 
						RefNbr = doc.RefNbr, 
						LastDate = schedule.LastDate, 
						NextDate = schedule.NextDate
					});
				}

				if (contract.IsPendingUpdate != true && contract.ScheduleStartsOn == Contract.scheduleStartsOn.ActivationDate)
				{
					if (schedule.Type != BillingType.OnDemand)
						schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, date, date);
					schedule.LastDate = date.Value;
				}

				if (contract.IsPendingUpdate != true)
				{
					contract.EffectiveFrom = contract.StartDate;
					if (contract.ScheduleStartsOn == Contract.scheduleStartsOn.ActivationDate)
						schedule.StartBilling = date;
				}

				BillingSchedule.Update(schedule);

				contract.Status = Contract.status.Active;
				UpdateStatusFlags(contract);
				contract.ServiceActivate = true;
				Contracts.Update(contract);

				UpdateHistory(contract);

				Actions.PressSave();

				ts.Complete();
			}//ts

			EnsureContractDetailTranslations();

			AutoReleaseInvoice(contract);
		}

		public virtual void SetupAndActivate(int? contractID, DateTime? date)
		{
			#region Parameter and State Verification

			if (contractID == null) throw new ArgumentNullException(nameof(contractID));
			if (date == null) throw new ArgumentNullException(nameof(date));

			Contract contract = Contracts.Select(contractID);
			ContractBillingSchedule schedule = (ContractBillingSchedule)BillingSchedule.Select(contractID);

			if (contract.IsCompleted == true)
				throw new PXException(Messages.CancelledContarctCannotBeActivated);

			if (contract.IsActive == true && contract.IsPendingUpdate != true)
				throw new PXException(Messages.ActiveContractCannotBeActivated);

			if (contract.IsCancelled == true)
				throw new PXException(Messages.ContractTerminatedCantActivate);

			if (contract.ExpireDate != null && date.Value > contract.ExpireDate.Value)
			{
				throw new PXException(Messages.ActivationDateTooLate);
			}

			if (contract.IsPendingUpdate == true)
			{
				DateTime minDate = schedule.LastDate ?? contract.StartDate.Value;

				if (date < minDate || date > schedule.NextDate.Value)
				{
					throw new PXException(Messages.UpdateActivationDateTooEarlyOrTooLate);
				}
			}

			foreach (PXResult<ContractItem, ContractDetail> res in GetContractDetails(contract))
			{
				ContractItem item = res;
				ContractDetail detail = res;
				string message;
				if (!ContractMaint.IsValidDetailPrice(this, detail, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

			if (contract.OriginalContractID != null)
			{
				Contract origContract = Contracts.Select(contract.OriginalContractID);
				if (!IsFullyBilledContract(origContract))
				{
					throw new PXException(Messages.OriginalContractIsNotFullyBilledInSetupAndActivate, origContract.ContractCD);
				}

				origContract.IsCompleted = true;
				origContract.Status = Contract.status.Expired;
				Contracts.Update(origContract);
			}
			#endregion

			ClearState();
			ClearBillingTrace(contractID);

			CreateNewRevision(contractID, ContractAction.SetupAndActivate, Contract.status.Active);

			contract.ActivationDate = date;
			contract.StartDate = date;
			Contracts.Update(contract);

			Contract template = Contracts.SelectSingle(contract.TemplateID);
			contract.ScheduleStartsOn = template.ScheduleStartsOn;

			Customer customer;
			Location location;
			SetBillingTarget(contract, out customer, out location);

			List<InvoiceData> invoices = new List<InvoiceData>();

			InvoiceData data = new InvoiceData(date.Value);
			using (new PXLocaleScope(customer.LocaleName))
			{
				data.UsageData.AddRange(GetSetupFee(contract));
				data.UsageData.AddRange(GetActivationFee(contract, date));
				data.UsageData.AddRange(GetPrepayment(contract, date, null, null));
			}

			if (data.UsageData.Count > 0)
			{
				invoices.Add(data);
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (invoices.Count > 0)
				{
					using (new PXLocaleScope(customer.LocaleName))
						CreateInvoice(contract, template, invoices, customer, location, ContractAction.SetupAndActivate);
				}

				schedule.LastDate = date;

				foreach (ARRegister doc in doclist)
				{
					BillingTrace.Insert(new ContractBillingTrace
				{
						ContractID = contractID, 
						DocType = doc.DocType, 
						RefNbr = doc.RefNbr, 
						LastDate = schedule.LastDate, 
						NextDate = schedule.NextDate
					});
				}
				if (schedule.Type != BillingType.OnDemand)
					schedule.NextDate = GetNextBillingDate(schedule.Type, contract.CustomerID, date.Value, date);
				contract.EffectiveFrom = contract.StartDate;
				schedule.StartBilling = date;

				BillingSchedule.Update(schedule);
				contract.IsActive = true;
				contract.Status = Contract.status.Active;
				contract.ServiceActivate = true;
				Contracts.Update(contract);

				UpdateHistory(contract);

				this.Actions.PressSave();
				
				ts.Complete();
			}//ts

			EnsureContractDetailTranslations();

			AutoReleaseInvoice(contract);
		}

		public virtual void ActivateUpgrade(int? contractID, DateTime? date)
		{
			Activate(contractID, date, false);
		}

		private static bool IsBillable(UsageData item)
		{
			return item.IsTranData == false 
				? (item.Proportion ?? 1m)*(item.ExtPrice ?? 0m) != 0m 
				: (item.Qty ?? 0m) != 0m || (item.Proportion ?? 1m)*(item.PriceOverride ?? 0m) != 0m;
		}

		private void CreateInvoice(Contract contract, Contract template, List<InvoiceData> invoices, Customer customer, Location location, string action, Dictionary<int, List<TranNotePair>> sourceTran = null, List<UsageData> tranData = null)
		{
			ARContractInvoiceEntry invoiceEntry = CreateInstance<ARContractInvoiceEntry>();
			AROpenPeriodAttribute.DefaultFirstOpenPeriod<ARRegister.finPeriodID>(invoiceEntry.Document.Cache);
			invoiceEntry.ARSetup.Current.RequireControlTotal = false;
			invoiceEntry.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
			invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			foreach (InvoiceData invData in invoices)
			{
				invoiceEntry.Clear(PXClearOption.ClearAll);

				ARInvoice invoice = (ARInvoice)invoiceEntry.Document.Cache.CreateInstance();
				invoice.DocType = invData.GetDocType();
				int mult = invoice.DocType == ARDocType.CreditMemo ? -1 : 1;
				invoice = invoiceEntry.Document.Insert(invoice);

				invoice.CustomerID = customer.BAccountID;
				invoice.CustomerLocationID = location.LocationID;
				invoice.DocDesc = GetInvoiceDescription(action, contract, customer, invoice);
				invoice.DocDate = invData.InvoiceDate;
				invoice = invoiceEntry.Document.Update(invoice);
				invoiceEntry.customer.Current.CreditRule = customer.CreditRule;

				invoice.ProjectID = contract.ContractID;
				invoice.CuryID = contract.CuryID;

				invoice = invoiceEntry.Document.Update(invoice);

				foreach (UsageData item in invData.UsageData.Where(IsBillable).OrderBy(item=>item.ContractDetailsLineNbr))
				{
					//Note: The transactions is first inserted and then updated - this pattern is required so that Discounts are not reseted at the ARInvoice level.
					ARTran tran = invoiceEntry.Transactions.Insert();
					if (item.TranDate != null)
						tran.TranDate = item.TranDate;
					tran.InventoryID = item.InventoryID;
					tran.TranDesc = GetTransactionDescriptionWithPrefix(tran, item);
					tran.UOM = item.UOM;
					tran.SalesPersonID = contract.SalesPersonID;
					tran.CaseCD = item.CaseCD;

					tran.Qty = item.Qty * mult;
					if (item.IsTranData != false)
					{
						if (item.IsFree == true)
						{
							tran.CuryUnitPrice = 0;
							tran.CuryExtPrice = 0;
						}
						else if (item.PriceOverride != null)
						{
							tran = invoiceEntry.Transactions.Update(tran); // TODO: Need to rework by #AC-53064
							tran.CuryUnitPrice = item.PriceOverride.Value;
							tran = invoiceEntry.Transactions.Update(tran);

							decimal extPriceRaw = tran.CuryUnitPrice.GetValueOrDefault() * item.PreciseQty.GetValueOrDefault()*mult * item.Proportion.GetValueOrDefault(1);
							tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury(invoiceEntry.Transactions.Cache, tran, extPriceRaw);
						}
					}
					else // item.IsTranData == false
					{
						tran.Qty = 0m;
						tran.UOM = null;
						tran.CuryUnitPrice = 0m;
						tran.CuryExtPrice = item.ExtPrice * mult;
					}

					tran = invoiceEntry.Transactions.Update(tran); //Discounts are set;

					// TODO: Need to rework by #AC-53064
					if (item.IsTranData != false && item.IsFree == true)
					{
						tran.CuryUnitPrice = 0;
						tran.CuryExtPrice = 0;
					}
					// END TODO

					SetDiscountsForTran(invoiceEntry, invoice, tran, item.DiscountID);
					tran = invoiceEntry.Transactions.Update(tran);// price default is handled by ARInvoiceEntry

					// TODO: Need to rework by #AC-53064
					if (item.IsTranData == false)
					{
						tran.Qty = 0m;
						tran.UOM = null;
						tran.CuryUnitPrice = 0m;
				}
					// END TODO

					item.RefLineNbr = tran.LineNbr;
				}

				UpdateReferencePMTran2ARTran(invoiceEntry, sourceTran, tranData);

				if (template.AutomaticReleaseAR == true)
				{
					invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);
					invoice = invoiceEntry.Document.Update(invoice);
				}

				doclist.Add((ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Current);
				invoiceEntry.Save.Press();
			}
		}

		private void SetDiscountsForTran(ARContractInvoiceEntry invoiceEntry, ARInvoice invoice, ARTran tran, string discountID)
		{
			tran.ManualDisc = true;
			tran.ManualPrice = true;

			if (discountID != null && invoiceEntry.Transactions != null)
			{
				tran.DiscountID = discountID;
				DiscountEngineProvider.GetEngineFor<ARTran, ARInvoiceDiscountDetail>().UpdateManualLineDiscount(
					invoiceEntry.Transactions.Cache, invoiceEntry.Transactions, tran, invoiceEntry.ARDiscountDetails, invoice.BranchID, invoice.CustomerLocationID, invoice.DocDate, DiscountEngine.DefaultARDiscountCalculationParameters);
			}
		}

		private void UndoInvoices(Contract contract)
		{
			var toRemove = new List<ARInvoice>();
			var toUnlink = new List<ARInvoice>();


			foreach (var d in GetDocuments(contract))
			{
				if (d.Released != true)
				{
					toRemove.Add(d);
				}
				else if (CanIgnoreInvoice(d) == false)
				{
					throw new PXException(Messages.CannotUndoActionDueToReleasedDocument);
				}
				else
				{
					toUnlink.Add(d);
				}
			}

			if (toRemove.Any() || toUnlink.Any())
			{
				Remove(toRemove);
				Unlink(toUnlink);
			}
		}

		private IEnumerable<ARInvoice> GetDocuments(Contract contract)
		{
			return PXSelectJoin<
				ARInvoice, 
					InnerJoin<ContractBillingTrace, 
						On<ContractBillingTrace.docType, Equal<ARInvoice.docType>,
						 And<ContractBillingTrace.refNbr, Equal<ARInvoice.refNbr>>>>,
				Where<
					ContractBillingTrace.contractID, Equal<Required<Contract.contractID>>>>
				.Select(this, contract.ContractID)
				.RowCast<ARInvoice>()
				.ToList();
		}

		private static void Remove(IEnumerable<ARInvoice> documents)
		{
			var invoiceEntry = CreateInvoiceGraph();
			foreach (var d in documents)
			{
				invoiceEntry.Document.Current = d;
				invoiceEntry.Document.Delete(d);
				invoiceEntry.Save.Press();
			}
		}

		private void Unlink(IEnumerable<ARInvoice> documents)
		{
			foreach (PMTran pMRef in documents
				.SelectMany(d => PXSelect<
					PMTran, 
					Where<
						PMTran.projectID, Equal<Current<Contract.contractID>>,
					And<PMTran.aRTranType, Equal<Current<ARInvoice.docType>>,
						And<PMTran.aRRefNbr, Equal<Current<ARInvoice.refNbr>>>>>>
					.SelectMultiBound(this, new object[] {d})
					.RowCast<PMTran>()
					.Select(PXCache<PMTran>.CreateCopy)))
				{
					pMRef.ARRefNbr = null;
					pMRef.ARTranType = null;
					pMRef.RefLineNbr = null;
					pMRef.Billed = false;

					Transactions.Update(pMRef);
				}
			}

		private bool CanIgnoreInvoice(ARInvoice invoice)
		{
			if (invoice.Released != true || invoice.OpenDoc == true)
				return false;

			if (invoice.DocType != ARDocType.Invoice && invoice.DocType != ARDocType.CreditMemo)
				return false;

			string reversingDocType = invoice.DocType == ARDocType.Invoice ? ARDocType.CreditMemo : ARDocType.DebitMemo;
			List<PXResult<ARAdjust>> items = null;

			if (invoice.DocType == ARDocType.Invoice)
			{
				var applications = new PXSelectJoin<
					ARAdjust,
						InnerJoin<ARRegister, 
							On<ARRegister.docType, Equal<ARAdjust.adjgDocType>, 
							And<ARRegister.refNbr, Equal<ARAdjust.adjgRefNbr>>>>,
					Where<
						ARAdjust.adjdDocType, Equal<Required<ARInvoice.docType>>,
										  And<ARAdjust.adjdRefNbr, Equal<Required<ARInvoice.refNbr>>,
						And<ARAdjust.released, Equal<True>,
						And<ARAdjust.voided, Equal<False>>>>>>(this);

				items = applications.Select(invoice.DocType, invoice.RefNbr).ToList();
			}
			else if (invoice.DocType == ARDocType.CreditMemo)
			{
				var applications = new PXSelectJoin<
					ARAdjust,
						InnerJoin<ARRegister, 
							On<ARRegister.docType, Equal<ARAdjust.adjdDocType>, 
							And<ARRegister.refNbr, Equal<ARAdjust.adjdRefNbr>>>>,
					Where<
						ARAdjust.adjgDocType, Equal<Required<ARInvoice.docType>>,
										  And<ARAdjust.adjgRefNbr, Equal<Required<ARInvoice.refNbr>>,
						And<ARAdjust.released, Equal<True>,
						And<ARAdjust.voided, Equal<False>>>>>>(this);

				items = applications.Select(invoice.DocType, invoice.RefNbr).ToList();
			}

			if (items.Count != 1) return false;

			ARRegister reversal = items[0].GetItem<ARRegister>();

			return 
				reversal.DocType == reversingDocType
				&& reversal.OrigDocType == invoice.DocType
				&& reversal.OrigRefNbr == invoice.RefNbr;
		}

		private static ARContractInvoiceEntry CreateInvoiceGraph()
		{
			ARContractInvoiceEntry invoiceEntry = CreateInstance<ARContractInvoiceEntry>();
			AROpenPeriodAttribute.DefaultFirstOpenPeriod<ARInvoice.finPeriodID>(invoiceEntry.Document.Cache);
			invoiceEntry.ARSetup.Current.RequireControlTotal = false;
			invoiceEntry.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
			invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			return invoiceEntry;
		}

		public virtual void Bill(int? contractID, DateTime? date = null)
		{
			#region Parameter and State Verification

			if (contractID == null) throw new ArgumentNullException(nameof(contractID));

			Contract contract = Contracts.Select(contractID);
			ContractBillingSchedule schedule = BillingSchedule.Select(contractID);

			if (contract.IsActive != true)
				throw new PXException(Messages.ContractMustBeActive);

			if (contract.IsCancelled == true)
				throw new PXException(Messages.ContractTerminatedCantBill);

			if (contract.IsCompleted == true)
				throw new PXException(Messages.ContractCompletedExpiredCantBill);

			if (schedule.Type == BillingType.OnDemand && date == null)
				throw new PXException(Messages.BillingDateMustBeSet);

			if (date > contract.ExpireDate)
				throw new PXException(Messages.BillingDateGreaterThanExpiration);

			if (date < schedule.StartBilling)
				throw new PXException(Messages.BillingDateLessThanScheduleStartDate);

			foreach (PXResult<ContractItem, ContractDetail> res in GetContractDetails(contract))
			{
				ContractItem item = res;
				ContractDetail detail = res;

				string message;

				if (!ContractMaint.IsValidDetailPrice(this, detail, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}
			#endregion

			ContractBillingTrace trace = new ContractBillingTrace
			{
				ContractID = contractID, 
				LastDate = schedule.LastDate, 
				NextDate = schedule.NextDate
			};

			ClearState();
			ClearBillingTrace(contractID);

			if (schedule.Type == BillingType.OnDemand)
			{
				schedule.NextDate = date;
				BillingSchedule.Update(schedule);
			}

			Contract template = Contracts.SelectSingle(contract.TemplateID);

			availableQty = new Dictionary<int?, decimal?>();
			List<UsageData> data;
			Dictionary<int, List<TranNotePair>> sourceTran;
			List<UsageData> tranData;

			if (IsLastBillBeforeExpiration(contract, schedule))
			{
				RaiseErrorIfUnreleasedUsageExist(contract);

				contract.IsCompleted = true;
				contract.Status = Contract.status.Expired;
				contract.ServiceActivate = false;
				Contracts.Update(contract);
			}

			CreateNewRevision(contract.ContractID, ContractAction.Bill, contract.Status);

			RecalcUsage(contract, out data, out sourceTran, out tranData);

			DateTime? billingDate;
			if (schedule.Type == BillingType.OnDemand)
			{
				billingDate = date;
			}
			else
			{
				billingDate = schedule.NextDate;
				DateTime? nextBillingDate = GetNextBillingDate(schedule.Type, contract.CustomerID, schedule.NextDate, schedule.StartBilling);
				if (contract.ExpireDate != null && contract.ExpireDate < nextBillingDate)
				{
					nextBillingDate = contract.ExpireDate;
				}
				schedule.NextDate = nextBillingDate;
			}
			BillingSchedule.Update(schedule);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (data.Count > 0)
				{
					Customer customer;
					Location location;
					SetBillingTarget(contract, out customer, out location);

					ARContractInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARContractInvoiceEntry>();
					AROpenPeriodAttribute.DefaultFirstOpenPeriod<ARInvoice.finPeriodID>(invoiceEntry.Document.Cache);
					invoiceEntry.ARSetup.Current.RequireControlTotal = false;
					invoiceEntry.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
					invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

					ARInvoice invoice = (ARInvoice)invoiceEntry.Document.Cache.CreateInstance();
					invoice.DocType = ARDocType.Invoice;
					invoice.DocDate = billingDate;
					invoice = invoiceEntry.Document.Insert(invoice);

					invoice.CustomerID = customer.BAccountID;
					invoice.CustomerLocationID = location.LocationID;
					invoice.DocDesc = GetInvoiceDescription(ContractAction.Bill, contract, customer, invoice);

					invoice = invoiceEntry.Document.Update(invoice);

					invoiceEntry.customer.Current.CreditRule = customer.CreditRule;

					invoice.ProjectID = contract.ContractID;
					invoice.CuryID = contract.CuryID;

					invoice = invoiceEntry.Document.Update(invoice);

					foreach (UsageData item in data.Where(IsBillable).OrderBy(item => item.ContractDetailsLineNbr))
					{
						if (item.Qty == 0 && item.ExtPrice == 0)
							continue;

						//Note: The transactions is first inserted and then updated - this pattern is required so that Discounts are not reseted at the ARInvoice level.
						ARTran tran = invoiceEntry.Transactions.Insert();
						tran.InventoryID = item.InventoryID;
						tran.TranDesc = GetTransactionDescriptionWithPrefix(tran, item);
						tran.UOM = item.UOM;
						//tran.DeferredCode = item.DefCode;
						if (item.BranchID != null)
							tran.BranchID = item.BranchID;
						tran.EmployeeID = item.EmployeeID;
						tran.SalesPersonID = contract.SalesPersonID;
						tran.CaseCD = item.CaseCD;

						tran.Qty = item.Qty;
						if (item.IsTranData != false)
						{
							if (item.IsFree == true)
							{
								tran.CuryUnitPrice = 0;
								tran.CuryExtPrice = 0;
							}
							else if (item.PriceOverride != null)
							{
								tran = invoiceEntry.Transactions.Update(tran); // TODO: Need to rework by #AC-53064
								tran.CuryUnitPrice = item.PriceOverride.Value;
								tran = invoiceEntry.Transactions.Update(tran);

								decimal extPriceRaw = tran.CuryUnitPrice.GetValueOrDefault() * item.PreciseQty.GetValueOrDefault() * item.Proportion.GetValueOrDefault(1);
								tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury(invoiceEntry.Transactions.Cache, tran, extPriceRaw);
							}
						}
						else // item.IsTranData == false
						{
							tran.Qty = 0m;
							tran.UOM = null;
							tran.CuryUnitPrice = 0m;
							tran.CuryExtPrice = item.ExtPrice;
						}

						tran = invoiceEntry.Transactions.Update(tran); //Discounts are set;

						// TODO: Need to rework by #AC-53064
						if (item.IsTranData != false && item.IsFree == true)
						{
							tran.CuryUnitPrice = 0;
							tran.CuryExtPrice = 0;
						}
						// END TODO

						SetDiscountsForTran(invoiceEntry, invoice, tran, item.DiscountID);
						tran = invoiceEntry.Transactions.Update(tran);// price default is handled by ARInvoiceEntry

						// TODO: Need to rework by #AC-53064
						if (item.IsTranData == false)
						{
							tran.Qty = 0m;
							tran.UOM = null;
							tran.CuryUnitPrice = 0m;
						}
						// END TODO

						item.RefLineNbr = tran.LineNbr;
					}

					UpdateReferencePMTran2ARTran(invoiceEntry, sourceTran, tranData);

					if (template.AutomaticReleaseAR == true)
					{
						invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);
						invoice = invoiceEntry.Document.Update(invoice);
					}

					doclist.Add((ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Current);
					invoiceEntry.Actions.PressSave();


					trace.DocType = doclist[0].DocType;
					trace.RefNbr = doclist[0].RefNbr;
					BillingTrace.Insert(trace);
				}
				if (schedule.Type == BillingType.OnDemand)
					schedule.NextDate = null;
				schedule.LastDate = billingDate;
				BillingSchedule.Update(schedule);

				UpdateHistory(contract);

				Actions.PressSave();
				ts.Complete();
			}

			EnsureContractDetailTranslations();

			AutoReleaseInvoice(contract);
		}

		public virtual void Renew(int? contractID, DateTime renewalDate)
		{
			#region Parameter and State Verification

			if(contractID == null)
			{
				throw new ArgumentNullException(nameof(contractID));
			}

			Contract contract = Contracts.Select(contractID);
			ContractBillingSchedule schedule = BillingSchedule.Select(contractID);

			if (schedule != null)
			{
				object customerID = schedule.AccountID;
				BillingSchedule.Cache.RaiseFieldVerifying<ContractBillingSchedule.accountID>(schedule, ref customerID);
			}

			if (contract.IsCancelled == true)
				throw new PXException(Messages.ContractTerminatedCantActivate);

			if (contract.ExpireDate == null)
				throw new PXException(Messages.ExpireDateMissingCantRenew);

			foreach (PXResult<ContractItem, ContractDetail> res in GetContractDetails(contract))
			{
				ContractItem item = res;
				ContractDetail detail = res;
				string message;
				if (!ContractMaint.IsValidDetailPrice(this, detail, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

			#endregion

			ClearState();
			ClearBillingTrace(contractID);

			DateTime newStartDate = contract.ExpireDate.Value.Date.AddDays(1);
			contract.RenewalBillingStartDate = newStartDate;
			contract.IsActive = true;
			contract.IsCompleted = false;
			contract.Status = Contract.status.Active;
			contract.ServiceActivate = true;
			contract = Contracts.Update(contract);
			if (contract.ExpireDate == null)
			{
				throw new PXException(Messages.ContractExpirationDateCantBeCalculated, contract.ContractCD);
			}

			CreateNewRevision(contractID, ContractAction.Renew, Contract.status.Active);

			if (schedule.Type != BillingType.OnDemand)
			{
				schedule.NextDate = newStartDate;
				BillingSchedule.Update(schedule);
			}

			contract = Contracts.Select(contractID);

			ContractRenewalHistory crh = CurrentRenewalHistory.SelectSingle(contract.ContractID, contract.RevID);
			crh.RenewalDate = renewalDate;
			crh = CurrentRenewalHistory.Update(crh);

			Contract template = Contracts.SelectSingle(contract.TemplateID);

			availableQty = new Dictionary<int?, decimal?>();

			List<UsageData> data = GetRenewalUsage(contract);
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (data.Count > 0)
				{
					Customer customer;
					Location location;
					SetBillingTarget(contract, out customer, out location);

					ARContractInvoiceEntry invoiceEntry = CreateInstance<ARContractInvoiceEntry>();
					AROpenPeriodAttribute.DefaultFirstOpenPeriod<ARRegister.finPeriodID>(invoiceEntry.Document.Cache);
					invoiceEntry.ARSetup.Current.RequireControlTotal = false;
					invoiceEntry.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
					invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((sender, e) => { e.Cancel = true; });
					invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((sender, e) => { e.Cancel = true; });

					ARInvoice invoice = invoiceEntry.Document.Insert(new ARInvoice
					{
						DocType = ARDocType.Invoice,
						DocDate = renewalDate,
						CustomerID = customer.BAccountID,
						CustomerLocationID = location.LocationID,
						ProjectID = contract.ContractID,
						CuryID = contract.CuryID
					});

					invoice.DocDesc = GetInvoiceDescription(ContractAction.Renew, contract, customer, invoice);

					invoiceEntry.customer.Current.CreditRule = customer.CreditRule;

					foreach (UsageData item in data.Where(IsBillable).OrderBy(item => item.ContractDetailsLineNbr))
					{
						if (item.Qty == 0 && item.ExtPrice == 0) continue;

						//Note: The transactions is first inserted and then updated - this pattern is required so that Discounts are not reseted at the ARInvoice level.
						ARTran tran = invoiceEntry.Transactions.Insert();
						tran.InventoryID = item.InventoryID;
						tran.TranDesc = GetTransactionDescriptionWithPrefix(tran, item); 
						tran.Qty = item.Qty;
						tran.UOM = item.UOM;
						tran.ProjectID = contract.ContractID;
						if(item.BranchID != null)
						{
							tran.BranchID = item.BranchID;
						}
						tran.EmployeeID = item.EmployeeID;
						tran.SalesPersonID = contract.SalesPersonID;
						tran.CaseCD = item.CaseCD;

						tran.Qty = item.Qty;

						if (item.IsTranData != false)
						{
							if (item.IsFree == true)
							{
								tran.CuryUnitPrice = 0;
								tran.CuryExtPrice = 0;
							}
							else if (item.PriceOverride != null)
							{
								tran = invoiceEntry.Transactions.Update(tran); // TODO: Need to rework by #AC-53064
								tran.CuryUnitPrice = item.PriceOverride;
								tran = invoiceEntry.Transactions.Update(tran);

								decimal extPriceRaw = tran.CuryUnitPrice.GetValueOrDefault() * item.PreciseQty.GetValueOrDefault() * item.Proportion.GetValueOrDefault(1);
								tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury(invoiceEntry.Transactions.Cache, tran, extPriceRaw);
							}
						}
						else // item.IsTranData == false
						{
							tran.Qty = 0m;
							tran.UOM = null;
							tran.CuryUnitPrice = 0m;
							decimal extPrice = (item.Proportion ?? 1) * (item.ExtPrice ?? 0m);
							tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury(invoiceEntry.Transactions.Cache, tran, extPrice);
						}

						tran = invoiceEntry.Transactions.Update(tran); //Discounts are set;

						// TODO: Need to rework by #AC-53064
						if (item.IsTranData != false && item.IsFree == true)
						{
							tran.CuryUnitPrice = 0;
							tran.CuryExtPrice = 0;
						}
						// END TODO

						SetDiscountsForTran(invoiceEntry, invoice, tran, item.DiscountID);
						tran = invoiceEntry.Transactions.Update(tran);

						// TODO: Need to rework by #AC-53064
						if (item.IsTranData == false)
						{
							tran.Qty = 0m;
							tran.UOM = null;
							tran.CuryUnitPrice = 0m;
						}
						// END TODO

						item.RefLineNbr = tran.LineNbr;
					}

					if (template.AutomaticReleaseAR == true)
					{
						invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);
						invoice = invoiceEntry.Document.Update(invoice);
					}

					doclist.Add((ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Current);
					invoiceEntry.Actions.PressSave();

					BillingTrace.Insert(new ContractBillingTrace
					{
						ContractID = contractID, 
						LastDate = schedule.LastDate,
						NextDate = schedule.NextDate, 
						DocType = doclist[0].DocType, 
						RefNbr = doclist[0].RefNbr
					});
				}
				UpdateHistory(contract);

				Actions.PressSave();
				ts.Complete();
			}

			EnsureContractDetailTranslations();

			AutoReleaseInvoice(contract);
		}

		public virtual void Terminate(int? contractID, DateTime? date)
		{
			#region Parameter and State Verification

			if (contractID == null) throw new ArgumentNullException(nameof(contractID));
			if (date == null) throw new ArgumentNullException(nameof(date));

			Contract contract = Contracts.Select(contractID);
			ContractBillingSchedule schedule = BillingSchedule.Select(contractID);

			if (contract.IsCancelled == true)
				throw new PXException(Messages.TerminatedContractCannotBeTerminated);

			DateTime minDate = schedule.LastDate ?? contract.StartDate.Value;
			if (date.Value < minDate)
			{
				throw new PXException(Messages.TerminationDateTooEarly);
			}

			if (schedule.Type != BillingType.OnDemand && schedule.NextDate.HasValue && date.Value > schedule.NextDate.Value)
			{
				throw new PXException(Messages.TerminationDateTooLate);
			}

			foreach (PXResult<ContractItem, ContractDetail> res in GetContractDetails(contract))
			{
				ContractItem item = (ContractItem)res;
				ContractDetail detail = (ContractDetail)res;

				string message;

				if (!ContractMaint.IsValidDetailPrice(this, detail, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}

			RaiseErrorIfUnreleasedUsageExist(contract);

			#endregion

			ContractBillingTrace baseTrace = new ContractBillingTrace
			{
				ContractID = contractID, 
				LastDate = schedule.LastDate, 
				NextDate = schedule.NextDate
			};

			ClearState();
			ClearBillingTrace(contractID);

			Contract template = Contracts.SelectSingle(contract.TemplateID);

			contract.Status = Contract.status.Canceled;
			contract.IsCancelled = true;
			contract.IsActive = false;
			contract.TerminationDate = date;
			contract.ServiceActivate = false;
			Contracts.Update(contract);

			CreateNewRevision(contractID, ContractAction.Terminate, Contract.status.Canceled);

			Customer customer;
			Location location;
			SetBillingTarget(contract, out customer, out location);

			List<InvoiceData> invoices = new List<InvoiceData>();

			InvoiceData data = new InvoiceData(date.Value);
			Dictionary<int, List<TranNotePair>> sourceTran;
			List<UsageData> tranData;
			using (new PXLocaleScope(customer.LocaleName))
			{
				List<UsageData> fee = GetTerminationFee(contract, schedule.LastDate, schedule.NextDate, date.Value, out sourceTran, out tranData);
				data.UsageData.AddRange(fee);
			}

			data.UsageData.RemoveAll(ud => ud.Proportion == 0.0m && ud.IsTranData != false);

			if (data.UsageData.Count > 0)
			{
				invoices.Add(data);
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (invoices.Count > 0)
				{
					using (new PXLocaleScope(customer.LocaleName))
						CreateInvoice(contract, template, invoices, customer, location, ContractAction.Terminate, sourceTran, tranData);
				}

				foreach (ARRegister register in doclist)
				{
					ContractBillingTrace trace = new ContractBillingTrace();
					trace.ContractID = contractID;
					trace.DocType = register.DocType;
					trace.RefNbr = register.RefNbr;
					trace.LastDate = baseTrace.LastDate;
					trace.NextDate = baseTrace.NextDate;

					BillingTrace.Insert(trace);
				}

				schedule.LastDate = date;
				schedule.NextDate = null;
				BillingSchedule.Update(schedule);

				UpdateHistory(contract);

				this.Actions.PressSave();
				ts.Complete();

			}//ts

			EnsureContractDetailTranslations();

			AutoReleaseInvoice(contract);
		}

		public virtual void UndoBilling(int? contractID)
		{
			if(contractID == null)
			{
				throw new ArgumentNullException(nameof(contractID));
			}

			Contract contract = Contracts.Select(contractID);

			if (contract.IsLastActionUndoable != true)
				throw new PXException(Messages.CannotUndoAction);

			var previousHistory = new PXSelect<
				ContractRenewalHistory,
				Where<
					ContractRenewalHistory.contractID, Equal<Required<Contract.contractID>>,
					And<ContractRenewalHistory.revID, Equal<Required<Contract.revID>>>>>(this)
				.SelectSingle(contract.ContractID, (contract.RevID ?? 0) - 1);

			if (previousHistory == null)
				throw new PXException(Messages.CannotUndoAction);

			ContractBillingSchedule schedule = BillingSchedule.Select(contractID);
			ContractRenewalHistory toUndo = CurrentRenewalHistory.SelectSingle(contract.ContractID, contract.RevID);

			contract.LastActiveRevID = GetLastActiveRevisionID(contract);
			contract.RevID = previousHistory.RevID;
			contract.Status = previousHistory.Status;
			contract.IsLastActionUndoable = false;

			Contracts.Update(contract);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				UndoInvoices(contract);
				RestoreScheduleFromHistory(schedule, previousHistory);

				if (toUndo.Action == ContractAction.Terminate)
				{
					contract.TerminationDate = null;
				}

				if (toUndo.Action == ContractAction.Renew && toUndo.ChildContractID != null)
				{
					ContractMaint graph = PXGraph.CreateInstance<ContractMaint>();
					graph.Clear();

					graph.Contracts.Current = PXSelect<
						Contract, 
						Where<Contract.contractID, Equal<Required<Contract.contractID>>>>
						.Select(graph, toUndo.ChildContractID);

					graph.Billing.Current = graph.Billing.Select();

					try
					{
						graph.Delete.Press();
					}
					catch (Exception e)
					{
						throw new PXException(e, Messages.CantDeleteRenewingContract);
					}
				}

				RestoreFieldsFromHistory(contract, previousHistory);

				ClearBillingTrace(contract.ContractID);
				ClearFuture(contract);

				Actions.PressSave();
				ts.Complete();
			}
		}

		private void RestoreFieldsFromHistory(Contract contract, ContractRenewalHistory history)
		{
			Contract contractCopy = PXCache<Contract>.CreateCopy(contract);

			contractCopy.EffectiveFrom = history.EffectiveFrom;
			contractCopy.ActivationDate = history.ActivationDate;
			contractCopy.StartDate = history.StartDate;
			contractCopy.ExpireDate = history.ExpireDate;

			contractCopy.IsActive = history.IsActive;
			contractCopy.IsCancelled = history.IsCancelled;
			contractCopy.IsCompleted = history.IsCompleted;
			contractCopy.IsPendingUpdate = history.IsPendingUpdate;

			contractCopy.DiscountID = history.DiscountID;

			Contracts.Update(contractCopy);
		}

		private void RestoreScheduleFromHistory(ContractBillingSchedule schedule, ContractRenewalHistory history)
		{
			schedule.LastDate = history.LastDate;
			schedule.NextDate = history.NextDate;
			schedule.StartBilling = history.StartBilling;
			BillingSchedule.Update(schedule);
		}

		private int? GetLastActiveRevisionID(Contract contract)
		{
			if (contract == null)
				return null;

			ContractRenewalHistory lastActiveRevision = new PXSelect<
				ContractRenewalHistory,
				Where<
					ContractRenewalHistory.contractID, Equal<Required<Contract.contractID>>,
				And<ContractRenewalHistory.status, Equal<Contract.status.active>,
				And<ContractRenewalHistory.revID, Less<Required<Contract.revID>>>>>,
				OrderBy<
					Desc<ContractRenewalHistory.revID>>>(this)
				.SelectSingle(contract.ContractID, contract.RevID);

			return lastActiveRevision == null ? null : lastActiveRevision.RevID;
		}

		private void UpdateStatusFlags(Contract contract)
		{
			contract.IsActive = contract.Status == Contract.status.Active || contract.Status == Contract.status.InUpgrade;
			contract.IsCancelled = contract.Status == Contract.status.Canceled;
			contract.IsCompleted = contract.Status == Contract.status.Completed;
			contract.IsPendingUpdate = contract.Status == Contract.status.InUpgrade;
		}

		public virtual void Upgrade(int? contractID)
		{
			#region Parameter and State Verification

			Contract contract = Contracts.Select(contractID);

			contract = Contracts.Cache.CreateCopy(contract) as Contract;

			if (contract == null) throw new ArgumentNullException(nameof(contractID));

			if (contract.IsActive != true)
				throw new PXException(Messages.ContractMustBeActive);

			if (contract.IsCancelled == true)
				throw new PXException(Messages.ContractTerminatedCantUpgrade);

			if (contract.IsCompleted == true)
				throw new PXException(Messages.ContractCompletedExpiredCantUpgrade);

			foreach (PXResult<ContractItem, ContractDetail> res in GetContractDetails(contract))
			{
				ContractItem item = res;
				ContractDetail detail = res;

				string message;
				if (!ContractMaint.IsValidDetailPrice(this, detail, out message))
				{
					throw new PXException(Messages.SpecificItemNotSpecificPrice, item.ContractItemCD, message);
				}
			}
			#endregion

			ContractBillingSchedule schedule = BillingSchedule.Select(contractID);

			contract.Status = Contract.status.InUpgrade;
			contract.IsPendingUpdate = true;
			contract.EffectiveFrom = schedule.NextDate ?? Accessinfo.BusinessDate;

			Contracts.Update(contract);

			CreateNewRevision(contractID, ContractAction.Upgrade, Contract.status.InUpgrade);
			UpdateHistory(contract);
			
			Actions.PressSave();
			
			EnsureContractDetailTranslations();
		}

		protected virtual List<UsageData> GetSetupFee(Contract contract)
		{
			List<UsageData> list = new List<UsageData>();

			foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in PXSelectJoin<
				ContractDetail,
			InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.baseItemID>>>>,
				Where<
					ContractDetail.contractID, Equal<Required<Contract.contractID>>,
					And<ContractDetail.isBaseValid, Equal<True>>>>
				.Select(this, contract.ContractID))
			{
				ContractDetail det = res;
				ContractItem item = res;
				InventoryItem inventory = res;

				UsageData data = new UsageData();
				data.ContractItemID = item.ContractItemID;
				data.ContractDetailID = det.ContractDetailID;
				data.ActionItem = PXMessages.LocalizeNoPrefix(Messages.ActionItemSetupDescription);
				data.InventoryID = item.BaseItemID;
				data.ContractDetailsLineNbr = det.LineNbr;

				//it's ok to use current culture here as this method is always executing in the locale scope
				data.Description = GetDescriptionMessageForInventory(InventoryAction.Setup, inventory);
				data.UOM = inventory.BaseUnit;
				data.Qty = det.Qty;
				data.PriceOverride = det.BasePriceVal;
				data.ExtPrice = data.Qty * det.BasePriceVal;
				data.DiscountID = det.BaseDiscountID;
				data.DiscountSeq = det.BaseDiscountSeq;

				list.Add(data);

				if (item.Deposit == true)
				{
					det.DepositAmt = data.ExtPrice;
					ContractDetails.Update(det);
				}
			}
			return list;
		}

		protected virtual PXResultset<ContractItem> GetContractDetails(Contract contract)
			=> GetContractDetails(contract.ContractID, contract.RevID);

		protected virtual PXResultset<ContractItem> GetContractDetails(int? contractID, int? revisionID)
			=> PXSelectJoin<
				ContractItem,
					InnerJoin<ContractDetail,
						On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>,
				Where<
					ContractDetail.contractID, Equal<Required<Contract.contractID>>,
					And<ContractDetail.revID, Equal<Required<Contract.revID>>>>>
				.Select(this, contractID, revisionID);

		protected virtual List<UsageData> GetActivationFee(Contract contract, DateTime? date)
		{
			List<UsageData> list = new List<UsageData>();

			PXSelectBase<ContractDetail> renewalItemsSelect = new PXSelectJoin<
				ContractDetail,
					InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.renewalItemID>>>>,
				Where<
					ContractDetail.contractID, Equal<Required<Contract.contractID>>,
					And<ContractDetail.isRenewalValid, Equal<True>>>>(this);

			foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in renewalItemsSelect.Select(contract.ContractID))
			{
				ContractDetail det = res;
				ContractItem item = res;
				InventoryItem inventory = res;

				if (item.CollectRenewFeeOnActivation == true)
				{
					UsageData data = new UsageData();
					data.ContractItemID = item.ContractItemID;
					data.ContractDetailID = det.ContractDetailID;
					data.ActionItem = PXMessages.LocalizeNoPrefix(Messages.ActionItemActivateRenew);
					data.InventoryID = inventory.InventoryID;
					data.ContractDetailsLineNbr = det.LineNbr;
					//it's ok to use current culture here as this method is always executing in the locale scope
					data.Description = GetDescriptionMessageForInventory(InventoryAction.ActivateRenew, inventory);
					data.UOM = inventory.BaseUnit;
					data.Qty = det.Qty;
					data.PriceOverride = det.RenewalPriceVal;
					data.ExtPrice = det.Qty * det.RenewalPriceVal;
					data.DiscountID = det.RenewalDiscountID;
					data.DiscountSeq = det.RenewalDiscountSeq;
					list.Add(data);
				}
			}

			return list;
		}

		protected virtual void GetUpgradeSetup(IEnumerable<PXResult<ContractDetail, ContractItem>> details, List<UsageData> usagedata, decimal? prorate)
		{
			foreach (PXResult<ContractDetail, ContractItem> res in details)
			{
				ContractDetail det = res;
				ContractItem item = res;

				decimal change = det.Change ?? 0m;

				//only refundable items can be decreased.
				if (item.BaseItemID != null && (change != 0 && (change > 0 || item.Refundable == true)))
				{
					InventoryItem inventory = InventoryItem.PK.Find(this, item.BaseItemID);
					InventoryItem inventoryRenew = InventoryItem.PK.Find(this, item.RenewalItemID);

					UsageData data = new UsageData();
					data.ContractItemID = item.ContractItemID;
					data.ContractDetailID = det.ContractDetailID;
					data.ActionItem = PXMessages.LocalizeNoPrefix(Messages.ActionItemSetupUpgrade);
					data.InventoryID = item.BaseItemID;
					data.ContractDetailsLineNbr = det.LineNbr;
					//it's ok to use current culture here as this method is always executing in the locale scope
					data.Description = GetDescriptionMessageForInventory(InventoryAction.SetupUpgrade, inventory);
					data.UOM = inventory.BaseUnit;
					data.Qty = change;
					data.PriceOverride = det.BasePriceVal;
					data.ExtPrice = data.Qty * det.BasePriceVal;
					data.DiscountID = det.BaseDiscountID;
					data.DiscountSeq = det.BaseDiscountSeq;
					if (item.ProrateSetup == true)
						data.Proportion = prorate;
					else
						data.Proportion = 1;

					if (usagedata != null)
					{
						usagedata.Add(data);
					}

					if (item.RenewalItemID != null && item.CollectRenewFeeOnActivation == true)
					{
						UsageData renew = new UsageData();
						renew.ContractItemID = item.ContractItemID;
						renew.ContractDetailID = det.ContractDetailID;
						renew.ActionItem = PXMessages.LocalizeNoPrefix(Messages.ActionItemUpgradeActivation);
						renew.InventoryID = item.RenewalItemID;
						renew.ContractDetailsLineNbr = det.LineNbr;
						//it's ok to use current culture here as this method is always executing in the locale scope
						renew.Description = GetDescriptionMessageForInventory(InventoryAction.UpgradeActivation, inventory);
						renew.UOM = inventoryRenew.BaseUnit;
						renew.Qty = change;
						renew.PriceOverride = det.RenewalPriceVal;
						renew.ExtPrice = renew.Qty * det.RenewalPriceVal;
						renew.DiscountID = det.RenewalDiscountID;
						renew.DiscountSeq = det.RenewalDiscountSeq;
						renew.Proportion = prorate;

						if (usagedata != null)
						{
							usagedata.Add(renew);
						}
					}
				}
			}
		}

		protected virtual void GetUpgradeRecurring(IEnumerable<PXResult<ContractDetail, ContractItem>> details, List<UsageData> usagedata, decimal? prorate)
		{
			foreach (PXResult<ContractDetail, ContractItem> res in details)
			{
				ContractDetail det = res;
				ContractItem item = res;

				decimal change = det.Change ?? 0;

				if (item.RecurringItemID != null && item.DepositItemID == null && (item.RecurringType == RecurringOption.Prepay || change < 0m))
				{
					InventoryItem inventory = InventoryItem.PK.Find(this, item.RecurringItemID);

					string prefix = PXMessages.LocalizeNoPrefix(item.RecurringType == RecurringOption.Prepay ? Messages.PrefixPrepaid : Messages.PrefixIncluded);

					UsageData data = new UsageData();
					data.InventoryID = item.RecurringItemID;
					data.ContractDetailsLineNbr = det.LineNbr;
					data.ContractDetailID = det.ContractDetailID;
					data.ContractItemID = item.ContractItemID;
					//it's ok to use current culture here as this method is always executing in the locale scope
					data.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory);
					data.UOM = inventory.BaseUnit;
					data.Qty = (item.RecurringType == RecurringOption.Prepay ? change : -change);
					data.PriceOverride = det.FixedRecurringPriceVal;
					data.ExtPrice = (item.RecurringType == RecurringOption.Prepay ? change : -change) * det.FixedRecurringPriceVal;
					data.Prefix = prefix;
					data.Proportion = (item.RecurringType == RecurringOption.Prepay ? prorate : 1 - prorate);
					data.DiscountID = det.RecurringDiscountID;
					data.DiscountSeq = det.RecurringDiscountSeq;

					usagedata.Add(data);
				}

				if (item.RecurringItemID != null && item.DepositItemID == null && item.RecurringType == RecurringOption.Usage && change > 0)
				{
					InventoryItem inventory = InventoryItem.PK.Find(this, item.RecurringItemID);

					string prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixIncluded);

					UsageData data = new UsageData();
					data.InventoryID = item.RecurringItemID;
					data.ContractDetailsLineNbr = det.LineNbr;
					data.ContractDetailID = det.ContractDetailID;
					data.ContractItemID = item.ContractItemID;
					//it's ok to use current culture here as this method is always executing in the locale scope
					data.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory);
					data.UOM = inventory.BaseUnit;
					data.Qty = -change;
					data.PriceOverride = det.FixedRecurringPriceVal;
					data.ExtPrice = -change * det.FixedRecurringPriceVal;
					data.Prefix = prefix;
					data.Proportion = 1 - prorate;
					data.DiscountID = det.RecurringDiscountID;
					data.DiscountSeq = det.RecurringDiscountSeq;

					usagedata.Add(data);
				}

				UpdateAvailableQty(det, item);
				AddDepositUsage(det, item, usagedata);
			}
		}

		protected virtual void GetTerminateRecurring(IEnumerable<PXResult<ContractDetail, ContractItem>> details, List<UsageData> usagedata, decimal? prorate)
		{
			foreach (PXResult<ContractDetail, ContractItem> res in details)
			{
				ContractDetail det = res;
				ContractItem item = res;

				if (item.RecurringItemID != null && item.DepositItemID == null)
				{
					InventoryItem inventory = InventoryItem.PK.Find(this, item.RecurringItemID);

					string prefix = PXMessages.LocalizeNoPrefix(item.RecurringType == RecurringOption.Prepay ? Messages.PrefixPrepaid : Messages.PrefixIncluded);

					decimal unusedQty = Math.Max(det.Qty.GetValueOrDefault(0) - det.Used.GetValueOrDefault(0), 0.0m);

					if (unusedQty > 0)
					{
						UsageData unusedData = new UsageData();
						unusedData.InventoryID = item.RecurringItemID;
						unusedData.ContractDetailsLineNbr = det.LineNbr;
						unusedData.ContractDetailID = det.ContractDetailID;
						unusedData.ContractItemID = item.ContractItemID;
						//it's ok to use current culture here as this method is always executing in the locale scope
						unusedData.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory);
						unusedData.UOM = inventory.BaseUnit;
						unusedData.Qty = (item.RecurringType == RecurringOption.Prepay ? -unusedQty : unusedQty);
						unusedData.PriceOverride = det.FixedRecurringPriceVal;
						unusedData.ExtPrice = unusedData.Qty * det.FixedRecurringPriceVal;
						unusedData.Prefix = prefix;
						unusedData.Proportion = (item.RecurringType == RecurringOption.Prepay ? prorate : 1 - prorate);
						unusedData.DiscountID = det.RecurringDiscountID;
						unusedData.DiscountSeq = det.RecurringDiscountSeq;

						usagedata.Add(unusedData);
					}

					var usedQty = det.Qty - unusedQty;

					if (usedQty > 0 && item.RecurringType != RecurringOption.Prepay)
					{
						UsageData usedData = new UsageData();
						usedData.InventoryID = item.RecurringItemID;
						usedData.ContractDetailsLineNbr = det.LineNbr;
						usedData.ContractDetailID = det.ContractDetailID;
						usedData.ContractItemID = item.ContractItemID;
						//it's ok to use current culture here as this method is always executing in the locale scope
						usedData.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory);
						usedData.UOM = inventory.BaseUnit;
						usedData.Qty = usedQty;
						usedData.PriceOverride = det.FixedRecurringPriceVal;
						usedData.ExtPrice = usedData.Qty * det.FixedRecurringPriceVal;
						usedData.Prefix = prefix;
						usedData.Proportion = 1.0m;
						usedData.DiscountID = det.RecurringDiscountID;
						usedData.DiscountSeq = det.RecurringDiscountSeq;

						usagedata.Add(usedData);
					}
				}

				UpdateAvailableQty(det, item);
				AddDepositUsage(det, item, usagedata);
			}
		}

		protected virtual void UpdateAvailableQty(ContractDetail det, ContractItem item)
		{
			decimal change = det.Change ?? 0;

			if (item.RecurringItemID != null && item.DepositItemID == null && (item.RecurringType == RecurringOption.Prepay || change <= 0m))
			{
				decimal? avail;
				if (item.ResetUsageOnBilling == true)
				{
					//either last for active details or qty for deleted
					avail = det.LastQty ?? det.Qty;
				}
				else
				{
					decimal billedQty = (det.UsedTotal ?? 0m) - (det.Used ?? 0m);
					avail = (det.LastQty ?? det.Qty) - billedQty;
				}
				availableQty.Add(item.RecurringItemID, Math.Max(0, (decimal)avail));
			}
		}

		protected virtual void AddDepositUsage(ContractDetail det, ContractItem item, List<UsageData> usagedata)
		{
			if (item.BaseItemID != null && item.Deposit == true)
			{
				// TODO: add LastDepositAmt
				// -
				ContractDetail billing = det;

				InventoryItem inventory = InventoryItem.PK.Find(this, item.BaseItemID);

				availableDeposit[billing.ContractItemID] = billing.DepositAmt - billing.DepositUsedTotal;

				UsageData tranData = new UsageData();
				tranData.InventoryID = inventory.InventoryID;
				tranData.ContractDetailsLineNbr = det.LineNbr;
				tranData.ContractDetailID = det.ContractDetailID;
				tranData.ContractItemID = item.ContractItemID;
				tranData.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>
					(Caches[typeof(InventoryItem)], inventory);
				tranData.Qty = 0m;
				tranData.UOM = inventory.BaseUnit;
				tranData.IsTranData = false;
				tranData.PriceOverride = 0m;
				tranData.ExtPrice = 0m;
				tranData.IsFree = false;
				tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage);
				tranData.IsDollarUsage = true;

				usagedata.Add(tranData);
				depositUsage[billing.ContractItemID] = tranData;
			}
		}

		protected virtual void GetUpgradeUsage(Contract contract, IEnumerable<PXResult<ContractDetail, ContractItem>> details, List<UsageData> list, out Dictionary<int, List<TranNotePair>> sourceTran, out List<UsageData> tranData)
		{
			sourceTran = GetTransactions(contract);
			tranData = new List<UsageData>();

			foreach (KeyValuePair<int, List<TranNotePair>> kv in sourceTran)
				{
				PXResult<ContractDetail, ContractItem, InventoryItem> billing = details
					.Select(res => new { res, item = res })
					.Where(@t => ((ContractItem) @t.item).RecurringItemID == kv.Key)
					.Select(@t => new { @t, inventory = InventoryItem.PK.Find(this, ((ContractItem) @t.item).RecurringItemID) })
					.Select(@t => new PXResult<ContractDetail, ContractItem, InventoryItem>(@t.@t.res, @t.@t.res, @t.inventory))
					.FirstOrDefault();

				if (billing != null)
				{
					tranData.AddRange(ProcessTransactions(contract, billing, kv.Value));

					foreach (TranNotePair tran in kv.Value)
					{
						tran.Tran.Billed = true;
						tran.Tran.BilledDate = Accessinfo.BusinessDate;
						Transactions.Update(tran.Tran);//only Billed field is updated.
					}
				}
			}

			list.AddRange(tranData);
		}

		protected virtual List<UsageData> GetUpgradeFee(Contract contract, DateTime? lastBillingDate, DateTime? nextBillingDate, DateTime? activationDate)
		{
			List<UsageData> list = new List<UsageData>();

			decimal prorate = 1;

			ContractBillingSchedule contractBillingSchedule = BillingSchedule.Select(contract.ContractID);

			if (contract.ExpireDate != null)
			{
				DateTime? startDate = contract.ScheduleStartsOn == Contract.scheduleStartsOn.SetupDate 
					? contract.StartDate 
					: contract.ActivationDate;

				DateTime? expirationDate = contract.ExpireDate.Value.Date.AddDays(1);

				prorate = Prorate(activationDate, startDate, expirationDate);
			}

			Dictionary<int?, PXResult<ContractDetail, ContractItem>> details = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();
			Dictionary<int?, PXResult<ContractDetail, ContractItem>> deleted = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();

			foreach (PXResult<ContractDetailExt, ContractItem> res in PXSelectReadonly2<
				ContractDetailExt,
					InnerJoin<ContractItem, 
						On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
					InnerJoin<Contract, 
						On<Contract.contractID, Equal<ContractDetailExt.contractID>, 
						And<Contract.lastActiveRevID, Equal<ContractDetailExt.revID>>>>>,
				Where<
					ContractDetailExt.contractID, Equal<Required<ContractDetailExt.contractID>>>>
				.Select(this, contract.ContractID))
			{
				ContractDetailExt det = res;
				ContractDetail del = PXCache<ContractDetail>.CreateCopy(res);
				ContractItem item = res;

				det.Change = -det.Qty;

				details[det.LineNbr] = new PXResult<ContractDetail, ContractItem>(det, item);
				deleted[del.LineNbr] = new PXResult<ContractDetail, ContractItem>(del, item);
			}

			foreach (PXResult<ContractDetail, ContractItem> res in PXSelectReadonly2<
				ContractDetail,
					InnerJoin<ContractItem, 
						On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>,
				Where<
					ContractDetail.contractID, Equal<Required<Contract.contractID>>>>
				.Select(this, contract.ContractID))
			{
				ContractDetail det = res;
				ContractDetail del = PXCache<ContractDetail>.CreateCopy(res);
				ContractItem item = res;

				PXDBLocalizableStringAttribute.CopyTranslations<ContractDetail.description, ContractDetail.description>
					(ContractDetails.Cache, det, ContractDetails.Cache, del);

				if (details.ContainsKey(det.LineNbr))
				{
					deleted.Remove(det.LineNbr);
				}

				ContractDetail detail = (ContractDetail)Caches[typeof(ContractDetail)].Locate(det);
				if (detail != null && item.Deposit == true)
				{
					detail.DepositAmt = detail.Qty * detail.BasePriceVal;
					Caches[typeof(ContractDetail)].Update(detail);
				}

				details[det.LineNbr] = res;

				if (det.Change < 0m)
				{
					del.Qty = del.LastQty;
					deleted[del.LineNbr] = new PXResult<ContractDetail, ContractItem>(del, item);
				}
			}

			GetUpgradeSetup(details.Values, list, prorate);

			prorate = 1;

			if (nextBillingDate != null && lastBillingDate != null)
			{
				DateTime? startDate = lastBillingDate;

				if (contractBillingSchedule?.Type == BillingType.Statement)
				{
					startDate = GetStatementBillDates(contract.CustomerID, startDate).Start;
				}

				prorate = Prorate(activationDate, startDate, nextBillingDate);
			}

			GetUpgradeRecurring(details.Values, list, prorate);

			Dictionary<int, List<TranNotePair>> sourceTran;
			List<UsageData> tranData;
			GetUpgradeUsage(contract, deleted.Values, list, out sourceTran, out tranData);

			return list;
		}

		protected virtual List<UsageData> GetTerminationFee(Contract contract, DateTime? lastBillingDate, DateTime? nextBillingDate, DateTime terminationDate, out Dictionary<int, List<TranNotePair>> sourceTran, out List<UsageData> tranData)
		{
			List<UsageData> list = new List<UsageData>();

			Dictionary<int?, PXResult<ContractDetail, ContractItem>> details = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();
			Dictionary<int?, PXResult<ContractDetail, ContractItem>> deleted = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();

			foreach (PXResult<ContractDetail, ContractItem> res in PXSelectReadonly2<
				ContractDetail,
					InnerJoin<ContractItem, 
						On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>>,
				Where<
					ContractDetail.contractID, Equal<Required<Contract.contractID>>>>
				.Select(this, contract.ContractID))
			{
				ContractDetail det = res;
				ContractDetail del = PXCache<ContractDetail>.CreateCopy(res);
				ContractItem item = res;
				det.Change = -det.Qty;

				PXDBLocalizableStringAttribute.CopyTranslations<ContractDetail.description, ContractDetail.description>
					(ContractDetails.Cache, det, ContractDetails.Cache, del);

				details[det.LineNbr] = new PXResult<ContractDetail, ContractItem>(det, item);
				deleted[del.LineNbr] = new PXResult<ContractDetail, ContractItem>(del, item);

				ContractDetail detail = (ContractDetail)Caches[typeof(ContractDetail)].Locate(det);
				if (detail != null && item.Deposit == true)
				{
					detail.DepositAmt = detail.Qty * detail.BasePriceVal;
					Caches[typeof(ContractDetail)].Update(detail);
				}

				details[det.LineNbr] = res;

				if (det.Change < 0m)
				{
					del.Qty = del.LastQty;
					deleted[del.LineNbr] = new PXResult<ContractDetail, ContractItem>(del, item);
				}
			}

			MemorizeDeposits(details.Values);

			decimal setupProrate = 1;
			if (contract.ExpireDate != null)
			{
				DateTime? startDate = contract.ScheduleStartsOn == Contract.scheduleStartsOn.SetupDate ? contract.StartDate : contract.ActivationDate;
				decimal totalDays = contract.ExpireDate.Value.Date.Subtract(startDate.Value.Date).Days;
				decimal daysLeft = contract.ExpireDate.Value.Date.Subtract(terminationDate.Date).Days;
				setupProrate = totalDays == 0m ? 1m : daysLeft / totalDays;
			}

			Contract template = Contracts.SelectSingle(contract.TemplateID);
			bool isRefundAllowed = template.Refundable == true && contract.StartDate.Value.Date.AddDays(template.RefundPeriod.GetValueOrDefault()) >= terminationDate;
			GetUpgradeSetup(details.Values, isRefundAllowed ? list : null, setupProrate);

			decimal? recurringProrate = 1;
			if (nextBillingDate != null && lastBillingDate != null)
			{
				decimal totalDays = nextBillingDate.Value.Date.Subtract(lastBillingDate.Value.Date).Days;
				decimal daysLeft = nextBillingDate.Value.Date.Subtract(terminationDate.Date).Days;
				recurringProrate = totalDays == 0m ? (decimal?)null : daysLeft / totalDays;
			}

			if (recurringProrate.HasValue)
			{
				GetTerminateRecurring(details.Values, list, recurringProrate);
			}

			GetUpgradeUsage(contract, deleted.Values, list, out sourceTran, out tranData);

			foreach (var usage in depositUsage.Values)
				list.Remove(usage);

			foreach (var usage in nonRefundableDepositedUsage)
				list.Remove(usage);

			return list;
		}

		protected virtual void MemorizeDeposits(IEnumerable<PXResult<ContractDetail, ContractItem>> details)
		{
			foreach (var detail in details)
			{
				ContractItem item = detail;
				if (item.BaseItemID != null && item.Deposit == true)
				{
					if (item.Refundable == true)
					{
						refundableDeposits.Add(item.ContractItemID, item);
					}
					else
					{
						nonRefundableDeposits.Add(item.ContractItemID, item);
					}
				}
			}
		}

		protected virtual List<UsageData> GetRenewalUsage(Contract contract)
		{

			List<UsageData> list = new List<UsageData>();

			PXSelectBase<ContractDetail> renewalItemsSelect = new PXSelectJoin<
				ContractDetail,
					InnerJoin<ContractItem, 
						On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
					InnerJoin<InventoryItem, 
						On<InventoryItem.inventoryID, Equal<ContractItem.renewalItemID>>>>,
				Where<
					ContractDetail.contractID, Equal<Required<Contract.contractID>>,
					 And<ContractDetail.isRenewalValid, Equal<True>>>>(this);

			Customer customer = customerRepository.FindByID(contract.CustomerID);

			foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in renewalItemsSelect.Select(contract.ContractID))
			{
				ContractDetail det = res;
				ContractItem item = res;
				InventoryItem inventory = res;

				UsageData data = new UsageData();
				data.InventoryID = inventory.InventoryID;
				data.ContractDetailsLineNbr = det.LineNbr;
				data.ContractDetailID = det.ContractDetailID;
				data.ContractItemID = item.ContractItemID;
				data.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory, customer?.LocaleName);
				data.UOM = inventory.BaseUnit;
				data.Qty = det.Qty;
				data.PriceOverride = det.RenewalPriceVal;
				data.ExtPrice = det.Qty * det.RenewalPriceVal;
				data.DiscountID = det.RenewalDiscountID;
				data.DiscountSeq = det.RenewalDiscountSeq;
				list.Add(data);
			}


			return list;
		}

		protected static decimal Prorate(DateTime? date, DateTime? startDate, DateTime? endDate)
		{
			var actual = date.Value.Date;
			var start = startDate.Value.Date;
			var end = endDate.Value.Date;

			decimal totalDays = end.Subtract(start).Days;
			decimal daysLeft = end.Subtract(actual).Days;
			return totalDays == 0.0m ? 1.0m : daysLeft / totalDays;
		}

		protected static decimal Prorate(DateTime? date, DatePair period)
		{
			return Prorate(date, period.Start, period.End);
		}

		protected virtual List<UsageData> GetPrepayment(Contract contract, DateTime? activationDate, DateTime? lastBillingDate, DateTime? nextBillingDate)
		{
			List<UsageData> list = new List<UsageData>();

			decimal prorate = 1;

			ContractBillingSchedule schedule = BillingSchedule.SelectSingle(contract.ContractID);
			if (schedule != null && schedule.Type == BillingType.Statement)
			{
				var statementDates = GetStatementBillDates(contract.CustomerID, activationDate);
				prorate = Prorate(activationDate, statementDates);
			}
			else if (nextBillingDate != null && contract.ScheduleStartsOn == Contract.scheduleStartsOn.SetupDate)
			{
				prorate = Prorate(activationDate, lastBillingDate, nextBillingDate);
			}

			if (contract.IsPendingUpdate == true)
			{
				return list;//do not collect prepayment. it will be collected on next billing.
			}

			foreach (PXResult<ContractDetail, ContractItem, InventoryItem> res in PXSelectJoin<
				ContractDetail,
					InnerJoin<ContractItem, 
						On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
					InnerJoin<InventoryItem, 
						On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>>>>,
				Where<
					ContractDetail.contractID, Equal<Required<ContractDetail.contractID>>,
					 And<ContractItem.recurringType, Equal<RecurringOption.prepay>,
					And<ContractItem.depositItemID, IsNull>>>>
				.Select(this, contract.ContractID))
			{
				ContractDetail det = res;
				ContractItem item = res;
				InventoryItem inventory = res;

				// TODO: delete?
				// -
				System.Diagnostics.Debug.WriteLine((item.RecurringType == RecurringOption.Prepay) + "\t" + (item.DepositItemID == null));

				decimal included = det.Qty ?? 0;
				string prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaid);

				UsageData data = new UsageData();
				data.InventoryID = item.RecurringItemID;
				data.ContractDetailsLineNbr = det.LineNbr;
				data.ContractDetailID = det.ContractDetailID;
				data.ContractItemID = item.ContractItemID;
				//it's ok to use current culture here as this method is always executing in the locale scope
				data.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory); 
				data.UOM = inventory.BaseUnit;
				data.Qty = included;
				data.PriceOverride = det.FixedRecurringPriceVal;
				data.ExtPrice = included * det.FixedRecurringPriceVal;
				data.Prefix = prefix;
				data.Proportion = prorate;
				data.DiscountID = det.RecurringDiscountID;
				data.DiscountSeq = det.RecurringDiscountSeq;
				list.Add(data);
			}

			return list;
		}

		protected virtual List<UsageData> GetRecurringBilling(IEnumerable<PXResult<ContractDetail, ContractItem>> details, Contract contract)
		{
			ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

			List<UsageData> list = new List<UsageData>();

			var proportions = new Proportions();

			if (schedule != null && schedule.Type == BillingType.Statement && contract.CustomerID != null)
			{
				proportions = CalculateStatementBasedRecurringProportions(contract, schedule);
			}
			else if(schedule != null && schedule.Type != BillingType.Statement && schedule.Type != BillingType.OnDemand)
			{
				proportions = CalculateRecurringProportions(contract, schedule);
			}

			Customer customer = customerRepository.FindByID(contract.CustomerID);

			using (new PXLocaleScope(customer?.LocaleName))
			{
				foreach (PXResult<ContractDetail, ContractItem> res in details)
				{
					ContractDetail det = res;
					ContractItem item = res;
					if (item.RecurringItemID != null && item.DepositItemID == null)
					{
						InventoryItem inventory = InventoryItem.PK.Find(this, item.RecurringItemID);

						decimal included = 0;
						UsageData data = new UsageData
						{
							InventoryID = inventory.InventoryID,
							ContractDetailsLineNbr = det.LineNbr,
							ContractDetailID = det.ContractDetailID,
							ContractItemID = item.ContractItemID,
							Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory, customer?.LocaleName),
							UOM = inventory.BaseUnit,
							Qty = det.Qty,
							PriceOverride = det.FixedRecurringPriceVal,
							Proportion = item.RecurringType == RecurringOption.Usage ? 1.0m - proportions.Postpaid : proportions.Prepaid,
							ExtPrice = det.Qty * det.FixedRecurringPriceVal,
							Prefix = PXMessages.LocalizeNoPrefix(item.RecurringType == RecurringOption.Usage ? Messages.PrefixIncluded : Messages.PrefixPrepaid),
							DiscountID = det.RecurringDiscountID,
							DiscountSeq = det.RecurringDiscountSeq
						};

						// Add Post-Payment Usage
						// -
						if (item.RecurringType == RecurringOption.Usage && det.Qty > 0m)
						{
							if (!IsFirstBillAfterRenewalExpiredContractInGracePeriod(contract, schedule))
							{
								list.Add(data);
							}
							included = det.Qty ?? 0;
						}

						// Add Pre-Payment Usage
						// -
						if (item.RecurringType == RecurringOption.Prepay && det.Qty > 0m)
						{
							if (contract.IsCancelled != true && !IsLastBillBeforeExpiration(contract, schedule))
							{
								list.Add(data);
							}
							included = det.Qty ?? 0;
						}

						decimal avail;
						if (item.ResetUsageOnBilling == true)
						{
							avail = included;
						}
						else
						{
							decimal billedQty = det.UsedTotal.GetValueOrDefault() - det.Used.GetValueOrDefault();
							avail = included - billedQty;
						}
						availableQty.Add(item.RecurringItemID, Math.Max(0, avail));
					}

					if (item.BaseItemID != null && item.Deposit == true)
					{
						ContractDetail billing = res;

						InventoryItem inventory = InventoryItem.PK.Find(this, item.BaseItemID);

						availableDeposit[billing.ContractItemID] = billing.DepositAmt - billing.DepositUsedTotal;

						UsageData tranData = new UsageData
						{
							InventoryID = inventory.InventoryID,
							ContractDetailsLineNbr = det.LineNbr,
							ContractDetailID = det.ContractDetailID,
							ContractItemID = item.ContractItemID,
							Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>
								(Caches[typeof(InventoryItem)], inventory),
							Qty = 0m,
							UOM = inventory.BaseUnit,
							IsTranData = false,
							PriceOverride = 0m,
							ExtPrice = 0m,
							IsFree = false,
							Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage),
							IsDollarUsage = true
						};

						list.Add(tranData);
						depositUsage[billing.ContractItemID] = tranData;
					}
				}
			}

			return list;
		}

		public class Proportions
		{
			public Proportions(decimal prepaid, decimal postpaid)
			{
				Prepaid = prepaid;
				Postpaid = postpaid;
			}

			public Proportions() : this(1m, 0m) { }

			public decimal Prepaid { get; set; }
			public decimal Postpaid { get; set; }
		}

		protected virtual Proportions CalculateStatementBasedRecurringProportions(Contract contract, ContractBillingSchedule schedule)
		{
			var proportions = new Proportions();

			bool isFirstRegularBill = IsFirstRegularBill(contract, schedule);
			bool isPrevToLastBill = IsPrevToLastBillBeforeExpiration(contract, schedule);
			bool isLastBill = IsLastBillBeforeExpiration(contract, schedule);

			if (isFirstRegularBill && contract.ActivationDate.HasValue)
			{
				var statementDates = GetStatementBillDates(contract.CustomerID, contract.ActivationDate);
				proportions.Postpaid = 1.0m - Prorate(contract.ActivationDate, statementDates);
			}

			if (isPrevToLastBill)
			{
				var statementDates = GetStatementBillDates(contract.CustomerID, schedule.NextDate);
				proportions.Prepaid = 1.0m - Prorate(contract.ExpireDate.Value.AddDays(1), statementDates);
			}

			if (isLastBill)
			{
				var statementDates = GetStatementBillDates(contract.CustomerID, schedule.NextDate);
				proportions.Postpaid = Prorate(contract.ExpireDate.Value.AddDays(1), statementDates);
			}

			return proportions;
		}

		protected virtual Proportions CalculateRecurringProportions(Contract contract, ContractBillingSchedule schedule)
		{
			var proportions = new Proportions();

			bool isPrevToLastBill = IsPrevToLastBillBeforeExpiration(contract, schedule);
			bool isLastBill = IsLastBillBeforeExpiration(contract, schedule);

			if (isPrevToLastBill)
			{
				var nextDateByBillingPeriod = GetNextBillingDate(schedule.Type, contract.CustomerID, schedule.NextDate, schedule.StartBilling);
				proportions.Prepaid = 1m - Prorate(contract.ExpireDate.Value.AddDays(1), schedule.NextDate, nextDateByBillingPeriod);
			}

			if (isLastBill)
			{
				var nextDateByBillingPeriod = GetNextBillingDate(schedule.Type, contract.CustomerID, schedule.LastDate, schedule.StartBilling);
				proportions.Postpaid = Prorate(contract.ExpireDate.Value.AddDays(1), schedule.LastDate, nextDateByBillingPeriod);
			}

			return proportions;
		}

		protected virtual DateTime DateSetDay(DateTime date, int day)
		{
			int days = DateTime.DaysInMonth(date.Year, date.Month);
			if (date.Day < day && days > date.Day)
			{
				return (days >= day) ? new DateTime(date.Year, date.Month, day) : new DateTime(date.Year, date.Month, days);
			}
			return date;
		}

		protected virtual DateTime? GetNextBillingDate(string scheduleType, int? customerID, DateTime? date, DateTime? startDate)
		{
			if (date == null) return null;

			switch (scheduleType)
			{
				case BillingType.Annual:
					return DateSetDay(date.Value.AddYears(1), startDate.Value.Day);
				case BillingType.SemiAnnual:
					return DateSetDay(date.Value.AddMonths(6), startDate.Value.Day);
				case BillingType.Monthly:
					return DateSetDay(date.Value.AddMonths(1), startDate.Value.Day);
				case BillingType.Weekly:
					return date.Value.AddDays(7);
				case BillingType.Quarterly:
					return DateSetDay(date.Value.AddMonths(3), startDate.Value.Day);
				case BillingType.OnDemand:
					return null;
				case BillingType.Statement:
					return GetStatementBillDates(customerID, date).End;
				default:
					throw new ArgumentException(
						PXMessages.LocalizeFormatNoPrefixNLA(Messages.InvalidScheduleType, scheduleType ?? "null"), 
						nameof(scheduleType));
			}
		}

		public class DatePair : Tuple<DateTime?, DateTime?>
		{
			public DatePair(DateTime? last, DateTime? next) : base(last, next) { }

			public DateTime? Start { get { return Item1; } }
			public DateTime? End { get { return Item2; } }
		}

		protected virtual DatePair GetStatementBillDates(int? customerID, DateTime? dateInside)
		{
			if (dateInside == null) return new DatePair(null, null);

			DateTime date = dateInside.Value.AddDays(1);

			ARStatementCycle cycle = PXSelectJoin<
				ARStatementCycle,
					LeftJoin<Customer, 
						On<ARStatementCycle.statementCycleId, Equal<Customer.statementCycleId>>>,
				Where<
					Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.SelectWindowed(this, 0, 1, customerID);

			if (cycle == null)
			{
				throw new PXSetPropertyException(Messages.StatementCycleIsNull, PXErrorLevel.Error);
			}
			else
			{
				return new DatePair(
					ARStatementProcess.CalcStatementDateBefore(
						this,
						date, 
						cycle.PrepareOn, 
						cycle.Day00, 
						cycle.Day01,
						cycle.DayOfWeek),
					ARStatementProcess.FindNextStatementDateAfter(this, date, cycle));
			}
		}

		protected virtual void SetBillingTarget(Contract contract, out Customer customer, out Location location)
		{
			customer = null;
			location = null;
			if (contract.CustomerID != null)
			{
				int? locationID;
				ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

				if (schedule != null && schedule.AccountID != null)
				{
					customer = customerRepository.FindByID(schedule.AccountID);
					locationID = schedule.LocationID;
				}
				else
				{
					customer = customerRepository.FindByID(contract.CustomerID);
					locationID = contract.LocationID;
				}

				location = locationID != null
					? PXSelect<
						Location, 
						Where<
							Location.bAccountID, Equal<Required<ContractBillingSchedule.accountID>>, 
							And<Location.locationID, Equal<Required<ContractBillingSchedule.locationID>>>>>
						.Select(this, customer.BAccountID, locationID)
					: PXSelect<
						Location, 
						Where<
							Location.bAccountID, Equal<Required<Customer.bAccountID>>, 
							And<Location.locationID, Equal<Required<Customer.defLocationID>>>>>
						.Select(this, customer.BAccountID, customer.DefLocationID);
			}
		}

		public virtual void RecalcUsage(Contract contract, out List<UsageData> data, out Dictionary<int, List<TranNotePair>> sourceTran, out List<UsageData> tranData)
		{
			Dictionary<int?, PXResult<ContractDetail, ContractItem>> details = new Dictionary<int?, PXResult<ContractDetail, ContractItem>>();

			foreach (PXResult<ContractDetailExt, ContractItem> res in PXSelectReadonly2<
				ContractDetailExt,
					InnerJoin<ContractItem, 
						On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
					InnerJoin<Contract, 
						On<Contract.contractID, Equal<ContractDetailExt.contractID>, 
						And<Contract.lastActiveRevID, Equal<ContractDetailExt.revID>>>>>,
				Where<
					ContractDetailExt.contractID, Equal<Required<ContractDetailExt.contractID>>>>
				.Select(this, contract.ContractID))
			{
				ContractDetailExt det = res;
				ContractItem item = res;

				details[det.LineNbr] = new PXResult<ContractDetail, ContractItem>(det, item);
			}

			data = GetRecurringBilling(details.Values, contract);

			sourceTran = GetTransactions(contract);

			Customer customer = customerRepository.FindByID(contract.CustomerID);

			using (new PXLocaleScope(customer?.LocaleName))
			{
				tranData = ProcessTransactions(contract, sourceTran);
			}

			data.AddRange(tranData);
		}

		public virtual decimal? RecalcDollarUsage(Contract contract)
		{
			List<UsageData> data;
			List<UsageData> tranData;
			Dictionary<int, List<TranNotePair>> sourceTran;

			RecalcUsage(contract, out data, out sourceTran, out tranData);

			return data
				.Where(item => item.IsDollarUsage == true)
				.Aggregate<UsageData, decimal?>(0m, (current, item) => current + (item.IsTranData == true && item.IsFree != true && item.PriceOverride != null ? item.Qty*item.PriceOverride : item.ExtPrice));
		}

		protected virtual Dictionary<int, List<TranNotePair>> GetTransactions(Contract contract)
		{
			//Returns Transactions grouped by InvetoryID for the billing period (all transactions up to billing date) for customer base contract.
			ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);

			Dictionary<int, List<TranNotePair>> dict = new Dictionary<int, List<TranNotePair>>();
			PXResultset<PMTran> resultset = null;
			if (contract.IsCancelled == true || schedule == null || schedule.NextDate == null || IsLastBillBeforeExpiration(contract, schedule))
			{
				resultset = PXSelectJoin<
					PMTran,
					   LeftJoin<Note, On<PMTran.origRefID, Equal<Note.noteID>>>,
					Where<
						PMTran.projectID, Equal<Required<PMTran.projectID>>,
						And<PMTran.billed, Equal<False>>>>
					.Select(this, contract.ContractID);
			}
			else
			{

				//To select all usage that fall under billing date we must select all usage that is less then billingdate + 1!
				DateTime includeDate = schedule.NextDate.Value.Date.AddDays(1);

				resultset = PXSelectJoin<
					PMTran,
					   LeftJoin<Note, On<PMTran.origRefID, Equal<Note.noteID>>>,
					Where<
						PMTran.projectID, Equal<Required<PMTran.projectID>>,
					   And<PMTran.billed, Equal<False>,
						And<PMTran.date, Less<Required<PMTran.date>>>>>>
					.Select(this, contract.ContractID, includeDate);
			}
			foreach (PXResult<PMTran, Note> res in resultset)
			{
				PMTran tran = res;
				Note note = res;

				if (dict.ContainsKey(tran.InventoryID.Value))
				{
					dict[tran.InventoryID.Value].Add(new TranNotePair(tran, note));
				}
				else
				{
					List<TranNotePair> list = new List<TranNotePair> { new TranNotePair(tran, note) };
					dict.Add(tran.InventoryID.Value, list);
				}
			}

			return dict;
		}

		protected virtual List<UsageData> ProcessTransactions(Contract contract, Dictionary<int, List<TranNotePair>> transactions)
		{
			List<UsageData> list = new List<UsageData>();

			foreach (KeyValuePair<int, List<TranNotePair>> kv in transactions)
			{
				ContractBillingSchedule schedule = BillingSchedule.Select(contract.ContractID);
				PXResult<ContractDetail, ContractItem, InventoryItem> billing = null;
				if (contract.CustomerID != null)//for Virtual Contract Billing items are not applicable.
				{
					billing = (PXResult<ContractDetail, ContractItem, InventoryItem>) PXSelectJoin<
						ContractDetail,
							InnerJoin<ContractItem, 
								On<ContractItem.contractItemID, Equal<ContractDetail.contractItemID>>,
							InnerJoin<InventoryItem, 
								On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>>>>,
						Where<
							ContractDetail.contractID, Equal<Required<Contract.contractID>>,
							And<ContractItem.recurringItemID, Equal<Required<ContractDetail.inventoryID>>>>>
						.Select(this, contract.ContractID, kv.Key);
				}

				if (billing == null)
				{
					InventoryItem inventoryItem = InventoryItem.PK.Find(this, kv.Key);
					billing = new PXResult<ContractDetail, ContractItem, InventoryItem>(null, null, inventoryItem);
				}

				ContractItem item = billing;

				if (!(IsFirstBillAfterRenewalExpiredContractInGracePeriod(contract, schedule)
					&& item != null
					&& item.RecurringType != RecurringOption.Prepay))
				{
					list.AddRange(ProcessTransactions(contract, billing, kv.Value));

				foreach (TranNotePair tran in kv.Value)
				{
					tran.Tran.Billed = true;
					tran.Tran.BilledDate = schedule.NextDate;
						Transactions.Update(tran.Tran); //only Billed field is updated.
				}
				}
			}

			return list;
		}

		protected virtual List<UsageData> ProcessTransactions(Contract contract, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<TranNotePair> list)
		{
			ContractDetail billing = res;
			ContractItem item = res;
			InventoryItem inventory = res;

			List<UsageData> data = new List<UsageData>();

			if (list.Count > 1)
			{
				if (contract.DetailedBilling == Contract.detailedBilling.Detail)
				{
					foreach (TranNotePair tran in list)
					{
						ProcessSingleRecord(contract, tran, res, data);
					}

				}
				else
				{
					// Note: When expense claims are linked to Contract. The cost/price is specified in the transaction, and can 
					// vary from one tran to another within the same contract. if billing or usagePrice is supplied we can take those; 
					// otherwise individual unit price must be used.
					// -
					List<TranNotePair> commonPrice = new List<TranNotePair>();
					foreach (TranNotePair tran in list)
					{
						if (billing != null || tran.Tran.IsQtyOnly == true)
							commonPrice.Add(tran);
						else
						{
							ProcessSingleRecord(contract, tran, res, data);
						}
					}

					ProcessSummaryUsageItems(contract, commonPrice, res, data);
				}
			}
			else if (list.Count == 1)
			{
				ProcessSingleRecord(contract, list[0], res, data);
			}

			return data;
		}

		protected virtual bool ProcessDollarUsage(Contract contract, TranNotePair tran, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<UsageData> addedData)
		{
			return ProcessDollarUsage(contract, tran, tran.Tran.UOM, tran.Tran.BillableQty, tran.Tran.ResourceID, res, addedData);
		}

		protected virtual bool ProcessDollarUsage(Contract contract, TranNotePair tran, string UOM, decimal? BillableQty, int? EmployeeID, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<UsageData> addedData)
		{
			ContractDetail billing = res;
			ContractItem item = res;
			InventoryItem inventory = res;

			UsageData tranData = addedData[addedData.Count - 1];

			if (billing != null && item != null && item.DepositItemID != null)
			{
				if (nonRefundableDeposits.ContainsKey(item.DepositItemID))
				{
					nonRefundableDepositedUsage.Add(tranData);
				}

				decimal? available;
				if (availableDeposit.TryGetValue(item.DepositItemID, out available))
				{
					decimal baseQty = ConvertUsage(Transactions.Cache, tran.Tran.InventoryID, UOM, inventory.BaseUnit, BillableQty);
					decimal amount = PXDBCurrencyAttribute.BaseRound(this, (decimal)(baseQty * billing.FixedRecurringPriceVal));
					if (available > 0m && amount <= available)
					{
						tranData.PriceOverride = billing.FixedRecurringPriceVal;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage);
						tranData.IsDollarUsage = true;

						available -= amount;
					}
					else if (available > 0m && baseQty > 0m)
					{
						decimal fixedBaseQty = (decimal)(available / billing.FixedRecurringPriceVal);
						decimal roundedBaseQty = PXDBQuantityAttribute.Round(fixedBaseQty);
						tranData.UOM = inventory.BaseUnit;
						tranData.Qty = roundedBaseQty;
						tranData.PreciseQty = fixedBaseQty;
						tranData.PriceOverride = billing.FixedRecurringPriceVal;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage);
						tranData.IsDollarUsage = true;

						addedData.Add(new UsageData
						{
							InventoryID = tran.Tran.InventoryID,
							ContractDetailsLineNbr = billing.LineNbr,
							ContractDetailID = billing.ContractDetailID,
							ContractItemID = item.ContractItemID,
							Description = tran.Tran.Description,
							Qty = baseQty - roundedBaseQty,
							PreciseQty = baseQty - fixedBaseQty,
							UOM = inventory.BaseUnit,
							EmployeeID = tran.Tran.ResourceID,
							IsTranData = true,
							PriceOverride = billing.UsagePriceVal,
							IsFree = false,
							Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixOverused),
							BranchID = tran.Tran.BranchID,
							IsDollarUsage = true,
							DiscountID = billing.RecurringDiscountID,
							DiscountSeq = billing.RecurringDiscountSeq
						});

						amount = PXDBCurrencyAttribute.BaseRound(this, (decimal)((baseQty - fixedBaseQty) * billing.UsagePriceVal));
						available = -amount;
					}
					else
					{
						tranData.PriceOverride = billing.UsagePriceVal;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixOverused);
						tranData.IsDollarUsage = true;
						tranData.DiscountID = billing.RecurringDiscountID;
						tranData.DiscountSeq = billing.RecurringDiscountSeq;

						amount = PXDBCurrencyAttribute.BaseRound(this, (decimal)(baseQty * billing.UsagePriceVal));
						available -= amount;
					}

					ContractDetail deposit = (ContractDetail)ContractDetails.Cache.Locate(new ContractDetail { ContractID = contract.ContractID, ContractItemID = item.DepositItemID, RevID = contract.RevID });
					if (deposit != null)
					{
						deposit.DepositUsedTotal += PXDBCurrencyAttribute.BaseRound(this, (decimal)(availableDeposit[item.DepositItemID] - available));
						ContractDetails.Update(deposit);

						if (availableDeposit[item.DepositItemID] > 0m)
						{
							if (depositUsage.TryGetValue(item.DepositItemID, out tranData))
							{
								tranData.ExtPrice -= (availableDeposit[item.DepositItemID] - (available > 0m ? available : 0m));
							}
						}

						availableDeposit[item.DepositItemID] = available;
					}
					
					billing.Used -= tran.Tran.BillableQty;
					ContractDetails.Update(billing);
					return true;
				}
			}

			return false;
		}

		protected virtual void ProcessSingleRecord(Contract contract, TranNotePair tran, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<UsageData> addedData)
		{
			ContractDetail billing = res;
			ContractItem item = res;
			InventoryItem inventory = res;

			//Prepayment and Postpayment items already added.
			//if used is greater then available then add 1 more line.

			UsageData tranData = new UsageData();
			tranData.TranIDs.Add(tran.Tran.TranID);
			tranData.InventoryID = tran.Tran.InventoryID;
			//if there is no ContractDetail for the line, we should put it after all other lines
			tranData.ContractDetailsLineNbr = billing?.LineNbr ?? int.MaxValue; 
			tranData.ContractDetailID = billing?.ContractDetailID;
			tranData.ContractItemID = item?.ContractItemID;
			tranData.Description = tran.Tran.Description;
			tranData.Qty = tran.Tran.BillableQty;
			tranData.UOM = tran.Tran.UOM;
			tranData.EmployeeID = tran.Tran.ResourceID;
			tranData.IsTranData = true;
			tranData.BranchID = tran.Tran.BranchID;
			tranData.TranDate = tran.Tran.Date;
			tranData.CaseCD = tran.Tran.CaseCD;

			/*
			 CRM - records only quantities and no prices. UnitPrice=0.0
			 EP (ExpenseClaims) - record unitprice also.
			 */

			addedData.Add(tranData);

			if (ProcessDollarUsage(contract, tran, res, addedData))
			{
				return;
			}

			if (billing != null && item != null)
				tranData.PriceOverride = billing.UsagePriceVal;
			else
			{
				if (tran.Tran.IsQtyOnly != true)
					tranData.PriceOverride = tran.Tran.UnitRate;
				else
				{
					tranData.PriceOverride = CalculatePrice(contract, tranData);
					if (tranData.PriceOverride == null)
					{
						throw new PXException(Messages.SpecificItemNotPrice, inventory.InventoryCD);
					}
				}
			}

			if (billing != null)
			{
				//billing can be from previous active revision, relookup in cache for correct revision
				ContractDetail cached;
				if ((cached = ContractDetails.Locate(billing)) != null && !object.ReferenceEquals(cached, billing))
				{
					billing = cached;
				}

				decimal? available = availableQty[tran.Tran.InventoryID.Value];

				if (tran.Tran.BillableQty <= available)
				{
					//Transaction is already payed for either ar prepayed incuded or a a post payment included. Thus it should be free.
					tranData.IsFree = true;
					tranData.Prefix = item.RecurringType == RecurringOption.Prepay ? PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage) : PXMessages.LocalizeNoPrefix(Messages.PrefixIncludedUsage);

					availableQty[tran.Tran.InventoryID.Value] -= tran.Tran.BillableQty.Value;//decrease available qty

				}
				else
				{
					if (available > 0m)
					{
						//Transaction is already payed for either ar prepayed incuded or a a post payment included. Thus it should be free.
						tranData.Qty = available;
						tranData.IsFree = true;
						tranData.Prefix = item.RecurringType == RecurringOption.Prepay ? PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage) : PXMessages.LocalizeNoPrefix(Messages.PrefixIncludedUsage);

						tranData = new UsageData();
						tranData.InventoryID = tran.Tran.InventoryID;
						tranData.ContractDetailsLineNbr = billing.LineNbr;
						tranData.ContractDetailID = billing.ContractDetailID;
						tranData.ContractItemID = item?.ContractItemID;
						tranData.Description =  tran.Tran.Description;
						tranData.UOM = inventory.BaseUnit;
						tranData.EmployeeID = tran.Tran.ResourceID;
						tranData.IsTranData = true;
						tranData.PriceOverride = billing.UsagePriceVal;
						tranData.BranchID = tran.Tran.BranchID;

						addedData.Add(tranData);
					}

					tranData.Qty = tran.Tran.BillableQty - available;
					tranData.IsFree = false;
					tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixOverused);

					tranData.DiscountID = billing.RecurringDiscountID;
					tranData.DiscountSeq = billing.RecurringDiscountSeq;

					availableQty[tran.Tran.InventoryID.Value] = 0;//all available qty was used.
				}

				billing.Used -= tran.Tran.BillableQty;

				ContractDetails.Update(billing);
			}
		}


		public virtual decimal? CalculatePrice(Contract contract, UsageData tranData)
		{
			Customer customer;
			Location location;
			SetBillingTarget(contract, out customer, out location);

			DateTime effectiveDate = (DateTime) (/*tranData.TranDate ?? */Accessinfo.BusinessDate);
			PXCache<CurrencyInfo> ciCache = this.Caches<CurrencyInfo>();
			CurrencyInfo ci = (CurrencyInfo)ciCache.Insert(new CurrencyInfo
			{
				CuryRateTypeID = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() ? customer.CuryRateTypeID : cmsetup.Current.ARRateTypeDflt,
				CuryID = contract.CuryID
			});
			ci.SetCuryEffDate(ciCache, effectiveDate);
			ciCache.Update(ci);

			decimal? price = ARSalesPriceMaint.CalculateSalesPrice(ContractDetails.Cache, location.CPriceClassID, contract.CustomerID, tranData.InventoryID, ci, tranData.UOM, tranData.PreciseQty, effectiveDate, null);
			ciCache.Delete(ci);
			return price;
		}

		protected virtual void ProcessSummaryUsageItems(Contract contract, List<TranNotePair> trans, PXResult<ContractDetail, ContractItem, InventoryItem> res, List<UsageData> addedData)
		{
			ContractDetail billing = res;
			ContractItem item = res;
			InventoryItem inventory = res;

			string targetUOM = inventory.BaseUnit;
			decimal used = SumUsage(trans, targetUOM);

			if (used > 0)
			{
				UsageData tranData = new UsageData();

				int? lastEmployeeID;

				tranData.TranIDs = trans.Select(tnp => tnp.Tran.TranID).ToList();
				HashSet<int> employees = new HashSet<int>(trans.Select(tnp => tnp.Tran.ResourceID).Where(employeeID => employeeID != null).Cast<int>());
				HashSet<DateTime> dates = new HashSet<DateTime>(trans.Select(tnp => tnp.Tran.Date).Where(date => date != null).Cast<DateTime>());

				tranData.EmployeeID = lastEmployeeID = employees.Count == 1 ? (int?)employees.FirstOrDefault() : null;
				tranData.TranDate = dates.Count == 1 ? (DateTime?)dates.FirstOrDefault() : null;

				tranData.InventoryID = trans[0].Tran.InventoryID.Value;
				//if there is no ContractDetail for the line, we should put it after all other lines
				tranData.ContractDetailsLineNbr = billing?.LineNbr ?? int.MaxValue;
				tranData.ContractDetailID = billing?.ContractDetailID;
				tranData.ContractItemID = item?.ContractItemID;
				tranData.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory);
				tranData.Qty = used;
				tranData.UOM = targetUOM;
				tranData.IsTranData = true;
				tranData.CaseCD = trans[0].Tran.CaseCD;

				addedData.Add(tranData);

				if (ProcessDollarUsage(contract, trans[0], targetUOM, used, tranData.EmployeeID, res, addedData))
				{
					return;
				}

				if (item != null)
					tranData.PriceOverride = billing.UsagePriceVal;
				else
				{
					tranData.PriceOverride = CalculatePrice(contract, tranData);
					if (tranData.PriceOverride == null)
					{
						throw new PXException(Messages.SpecificItemNotPrice, inventory.InventoryCD);
					}
				}
				if (billing != null)
				{
					tranData.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory);
					decimal? available = availableQty[tranData.InventoryID];

					if (used <= available)
					{
						//Transaction is already payed for either ar prepayed incuded or a a post payment included. Thus it should be free.
						tranData.IsFree = true;
						tranData.Prefix = item.RecurringType == RecurringOption.Prepay ? PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage) : PXMessages.LocalizeNoPrefix(Messages.PrefixIncludedUsage);

						availableQty[tranData.InventoryID] -= used;//decrease available qty
					}
					else
					{
						if (available > 0m)
						{
							//Transaction is already payed for either ar prepayed incuded or a a post payment included. Thus it should be free.
							tranData.Qty = available;
							tranData.IsFree = true;
							tranData.Prefix = item.RecurringType == RecurringOption.Prepay ? PXMessages.LocalizeNoPrefix(Messages.PrefixPrepaidUsage) : PXMessages.LocalizeNoPrefix(Messages.PrefixIncludedUsage);

							tranData = new UsageData();
							tranData.InventoryID = trans[0].Tran.InventoryID;
							tranData.ContractDetailsLineNbr = billing.LineNbr;
							tranData.ContractDetailID = billing.ContractDetailID;
							tranData.ContractItemID = item?.ContractItemID;
							tranData.Description = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>(Caches[typeof(InventoryItem)], inventory);
							tranData.UOM = targetUOM;
							tranData.EmployeeID = lastEmployeeID;
							tranData.IsTranData = true;
							tranData.PriceOverride = billing.UsagePriceVal;

							addedData.Add(tranData);
						}
						tranData.Qty = used - available;
						tranData.IsFree = false;
						tranData.Prefix = PXMessages.LocalizeNoPrefix(Messages.PrefixOverused);

						tranData.DiscountID = billing.RecurringDiscountID;
						tranData.DiscountSeq = billing.RecurringDiscountSeq;

						availableQty[tranData.InventoryID] = 0;//all available qty was used.
					}

					billing.Used -= used;
					ContractDetails.Update(billing);
				}
			}
		}

		protected virtual decimal SumUsage(List<TranNotePair> trans, string targetUOM)
		{
			return trans.Sum(item => ConvertUsage(Transactions.Cache, item.Tran.InventoryID, item.Tran.UOM, targetUOM, item.Tran.BillableQty));
		}

		protected virtual decimal SumUnbilledAmt(Contract contract, List<UsageData> data)
		{
			Customer customer;
			Location location;
			SetBillingTarget(contract, out customer, out location);

			ARContractInvoiceEntry emulator = null;

			decimal total = 0;
			foreach (UsageData item in data)
			{
				if (item.IsTranData == null || item.IsTranData == true)
				{
					decimal unitPrice = 0;
					if (item.IsFree != true)
					{
						if (item.PriceOverride != null)
						{
							unitPrice = item.PriceOverride.Value;
						}
						else
						{
							//we don't know exact price untill we create the invoice so we'll try to emulate this fuctionality.
							//ARInvoice will figure out sales price based on several settings.
							if (emulator == null)
							{
								emulator = PXGraph.CreateInstance<ARContractInvoiceEntry>();
								emulator.ARSetup.Current.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;

								emulator.Clear();
								ARInvoice doc = emulator.Document.Insert();
								doc.CustomerID = customer.BAccountID;
								doc.CustomerLocationID = contract.LocationID ?? customer.DefLocationID;
								doc.ProjectID = contract.ContractID;
								emulator.Document.Update(doc);
							}

							ARTran tran = (ARTran)emulator.Transactions.Cache.CreateInstance();
							tran.InventoryID = item.InventoryID;
							tran.Qty = item.Qty;
							tran.UOM = item.UOM;
							tran.FreezeManualDisc = true;
							tran.ManualDisc = true;
							tran.ManualPrice = true;
							tran.DiscountID = item.DiscountID;
							tran.DiscountSequenceID = item.DiscountSeq;

							tran = (ARTran)emulator.Caches[typeof(ARTran)].Update(tran);

							unitPrice = tran.UnitPrice ?? 0;
						}
					}

					total += item.Qty.Value * unitPrice;
				}
				else
				{
					//TODO: Add precision handling
					decimal extPrice = Math.Round((item.Proportion ?? 1) * item.ExtPrice.Value, 4, MidpointRounding.AwayFromZero); //used for proportional prepayment in statement-based contracts.
					total += extPrice;
				}
			}

			return total;
		}

		protected virtual decimal ConvertUsage(PXCache sender, int? inventoryID, string fromUOM, string toUOM, decimal? value)
		{
			if (value == null)
				return 0;

			if (fromUOM == toUOM)
				return value.Value;

			decimal inBase = INUnitAttribute.ConvertToBase(sender, inventoryID, fromUOM, value.Value, INPrecision.QUANTITY);
			return INUnitAttribute.ConvertFromBase(sender, inventoryID, toUOM, inBase, INPrecision.QUANTITY);
		}

		protected virtual bool IsLastBillBeforeExpiration(Contract contract, ContractBillingSchedule schedule)
		{
			return contract.ExpireDate != null && (schedule.NextDate.HasValue && schedule.NextDate.Value.Date >= contract.ExpireDate.Value.Date);
		}

		protected virtual bool IsFirstBillAfterRenewalExpiredContractInGracePeriod(Contract contract, ContractBillingSchedule schedule)
		{
			return schedule.NextDate != null && schedule.LastDate != null && (int) (schedule.NextDate.Value.Date - schedule.LastDate.Value.Date).TotalDays == 1;
		}

		protected virtual bool IsPrevToLastBillBeforeExpiration(Contract contract, ContractBillingSchedule schedule)
		{
			if (contract.ExpireDate == null)
				return false;

			if (schedule.NextDate.HasValue && schedule.NextDate.Value.Date < contract.ExpireDate.Value.Date)
			{
				DateTime? nextBilling = GetNextBillingDate(schedule.Type, contract.CustomerID, schedule.NextDate, schedule.StartBilling);
				return nextBilling.HasValue && nextBilling.Value.Date >= contract.ExpireDate.Value.Date;
			}
			else
				return false;
		}

		protected virtual bool IsFirstRegularBill(Contract contract, ContractBillingSchedule schedule)
		{
			return contract.ActivationDate != null && (schedule.LastDate == null || schedule.LastDate == contract.ActivationDate);
		}

		protected virtual void UpdateReferencePMTran2ARTran(ARContractInvoiceEntry graph, Dictionary<int, List<TranNotePair>> sourceTran, List<UsageData> tranData)
		{
			if (sourceTran == null || tranData == null) return;

			Dictionary<long, PMTran> sourceTranDict = sourceTran.Values.SelectMany(list => list).ToDictionary(pair => pair.Tran.TranID.Value, pair => pair.Tran);

			foreach (UsageData ud in tranData.Where(IsBillable))
			{
				foreach (PMTran source in ud.TranIDs.Cast<long>().Select(tranID => sourceTranDict[tranID]))
				{
					source.ARTranType = graph.Document.Current.DocType;
					source.ARRefNbr = graph.Document.Current.RefNbr;
					source.RefLineNbr = ud.RefLineNbr;
					graph.RefContractUsageTran.Update(source); //Note: PMTran should be saved via InvoiceEntry so that reference is updated correctly.
				}
			}
		}

		protected virtual void AutoReleaseInvoice(Contract contract)
		{
			if (doclist.Count > 0)
			{
				Contract template = Contracts.SelectSingle(contract.TemplateID);
				if (template.AutomaticReleaseAR == true)
				{
					try
					{
						ARDocumentRelease.ReleaseDoc(doclist, false);
					}
					catch (Exception ex)
					{
						throw new PXException(PM.Messages.AutoReleaseARFailed, ex);
					}
				}
			}
		}

		protected virtual void RaiseErrorIfUnreleasedUsageExist(Contract contract)
		{
			bool failed = false;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(PXLocalizer.Localize(Messages.ListOfUnreleasedBillableCases));

			PXSelectBase<CRCase> selectCases = new PXSelectJoin<
				CRCase,
					InnerJoin<CRCaseClass, 
						On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>>,
				Where<
					CRCase.contractID, Equal<Required<CRCase.contractID>>,
				And<CRCase.released, Equal<False>,
				And<CRCase.isBillable, Equal<True>,
				And<CRCaseClass.perItemBilling, Equal<BillingTypeListAttribute.perCase>>>>>>(this);

			foreach (CRCase c in selectCases.Select(contract.ContractID))
			{
				failed = true;
				sb.AppendLine(c.CaseCD);
			}

			sb.AppendLine(PXLocalizer.Localize(Messages.ListOfUnreleasedBillableActivities));

			PXSelectBase<CRPMTimeActivity> selectActivities = new PXSelectJoin<
					CRPMTimeActivity,
				InnerJoin<CRCase, 
					On<CRPMTimeActivity.refNoteID, Equal<CRCase.noteID>>,
				InnerJoin<CRCaseClass, 
					On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>>>,
				Where<
					CRCase.contractID, Equal<Required<CRCase.contractID>>,
					And<CRCaseClass.perItemBilling, Equal<BillingTypeListAttribute.perActivity>,
					And<CRPMTimeActivity.isBillable, Equal<True>,
					And<CRPMTimeActivity.billed, Equal<False>,
					And<CRPMTimeActivity.uistatus, NotEqual<ActivityStatusListAttribute.canceled>>>>>>>(this);

			foreach (PXResult<CRPMTimeActivity, CRCase, CRCaseClass> res in selectActivities.Select(contract.ContractID))
			{
				failed = true;

				sb.AppendFormat(PXLocalizer.LocalizeFormat(
					Messages.CaseActivityFormat, 
					((CRCase)res).CaseCD, 
					((CRPMTimeActivity)res).Subject, 
					Environment.NewLine));
			}

			if (failed)
			{
				PXTrace.WriteInformation(sb.ToString());
				throw new PXException(PXLocalizer.LocalizeFormat(Messages.UnreasedActivityExists, contract.ContractCD));
			}
		}

		protected virtual void EnsureContractDetailTranslations()
		{
			// create records in ContractDetailKvExt for new ContractDetail records (with increased RevId)
			PXDBLocalizableStringAttribute.EnsureTranslations(tableName => String.Equals(tableName, nameof(ContractDetail), StringComparison.OrdinalIgnoreCase));
		}

		protected class InvoiceData
		{
			public DateTime InvoiceDate { get; private set; }
			public List<UsageData> UsageData { get; private set; }

			public InvoiceData(DateTime date)
			{
				this.InvoiceDate = date;
				this.UsageData = new List<UsageData>();

			}

			public string GetDocType()
			{
				decimal sum = UsageData.Sum(data => data.IsTranData != false && data.IsFree != true && data.PriceOverride != null 
					? Math.Round((data.Qty * data.PriceOverride * data.Proportion.GetValueOrDefault(1.0m)) ?? 0m, 4, MidpointRounding.AwayFromZero) //TODO: add precision handling
					: (data.ExtPrice ?? 0m));

				return sum < 0m ? ARDocType.CreditMemo : ARDocType.Invoice;
			}
		}

		protected class Refund
		{
			public readonly decimal? Amount;
			public readonly PXResult<ContractDetail, ContractItem, InventoryItem> Item;
			public readonly bool IsRenew;

			public Refund(decimal? amount, PXResult<ContractDetail, ContractItem, InventoryItem> item, bool isRenew)
			{
				this.Amount = amount;
				this.Item = item;
				this.IsRenew = isRenew;
			}

		}

		[PXHidden]
		public partial class InventoryItemEx : InventoryItem
		{
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		}
	}
}
