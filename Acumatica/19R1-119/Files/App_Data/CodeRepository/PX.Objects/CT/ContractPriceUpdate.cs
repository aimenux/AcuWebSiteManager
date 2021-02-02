using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CR;
using PX.Objects.GL;

namespace PX.Objects.CT
{
	public class ContractPriceUpdate : PXGraph<ContractPriceUpdate>
	{
		public PXCancel<ContractFilter> Cancel;
		public PXAction<ContractFilter> ViewContract;

		public PXFilter<ContractFilter> Filter;
		public PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Current<ContractFilter.contractItemID>>>> SelectedContractItem; 
		public PXFilteredProcessingJoin<ContractDetail, ContractFilter, InnerJoin<Contract, On<Contract.contractID, Equal<ContractDetail.contractID>>>> Items;
		public PXSetup<Company> Company;

		protected class ContractMaintExt : ContractMaint
		{
			[PXDBInt(IsKey = true)]
			protected virtual void ContractDetail_ContractDetailID_CacheAttached() { }
		}

		public ContractPriceUpdate()
		{
			Items.SetSelected<ContractDetail.selected>();
			Items.SetProcessDelegate<ContractMaint>(UpdatePrices);
			Items.SetProcessCaption(Messages.Update);
			Items.SetProcessAllCaption(Messages.UpdateAll);

		    PXUIFieldAttribute.SetDisplayName<Contract.contractCD>(Caches[typeof(Contract)], Common.Messages.Identifier);
		}

		protected virtual IEnumerable items()
		{
			ContractItem ci = SelectedContractItem.Select();

			PXSelectBase<ContractDetail> select = new PXSelectJoin<ContractDetail, InnerJoin<Contract, On<Contract.contractID, Equal<ContractDetail.contractID>>>,
				Where<ContractDetail.contractItemID, Equal<Current<ContractFilter.contractItemID>>>>(this);


			List<PXResult<ContractDetail, Contract>> result = new List<PXResult<ContractDetail, Contract>>();
			foreach (PXResult<ContractDetail, Contract> res in select.Select())
			{
				ContractDetail item = res;
				bool isOutdated = false;

				ContractDetail copy = PXCache<ContractDetail>.CreateCopy(item);
				copy.ContractDetailID = -1; //Just to be safe that formulas on the following fields do not modify the original record.

				Items.Cache.SetDefaultExt<ContractDetail.basePriceOption>(copy);
				Items.Cache.SetDefaultExt<ContractDetail.basePrice>(copy);
				Items.Cache.SetDefaultExt<ContractDetail.renewalPriceOption>(copy);
				Items.Cache.SetDefaultExt<ContractDetail.renewalPrice>(copy);
				Items.Cache.SetDefaultExt<ContractDetail.fixedRecurringPriceOption>(copy);
				Items.Cache.SetDefaultExt<ContractDetail.fixedRecurringPrice>(copy);
				Items.Cache.SetDefaultExt<ContractDetail.usagePriceOption>(copy);
				Items.Cache.SetDefaultExt<ContractDetail.usagePrice>(copy);


				if (ci.BaseItemID != null)
				{
					if (ci.BasePriceOption != item.BasePriceOption ||
						ci.BasePrice != item.BasePrice ||
						copy.BasePriceVal != item.BasePriceVal)
					{
						isOutdated = true;
					}
				}

				if (ci.RenewalItemID != null)
				{
					if (ci.RenewalPriceOption != item.RenewalPriceOption ||
						ci.RenewalPrice != item.RenewalPrice ||
						copy.RenewalPriceVal != item.RenewalPriceVal)
					{
						isOutdated = true;
					}
				}


				if (ci.RecurringItemID != null)
				{
					if (ci.FixedRecurringPriceOption != item.FixedRecurringPriceOption ||
						ci.FixedRecurringPrice != item.FixedRecurringPrice ||
						copy.FixedRecurringPriceVal != item.FixedRecurringPriceVal)
					{
						isOutdated = true;
					}

					if (ci.UsagePriceOption != item.UsagePriceOption ||
						ci.UsagePrice != item.UsagePrice ||
						copy.UsagePriceVal != item.UsagePriceVal)
					{
						isOutdated = true;
					}
				}
				
				if (isOutdated)
				{
					result.Add(res);
				}
			}

			return result;
		}

		
		#region EventHandlers
		protected virtual void ContractFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}

		protected virtual void ContractItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ContractItem row = e.Row as ContractItem;
			if (row == null) return;

			PXUIFieldAttribute.SetVisible<ContractItem.basePriceOption>(sender, row, row.BaseItemID != null);
			PXUIFieldAttribute.SetVisible<ContractItem.basePrice>(sender, row, row.BaseItemID != null);
			PXUIFieldAttribute.SetVisible<ContractItem.basePriceVal>(sender, row, row.BaseItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.basePriceOption>(Items.Cache, null, row.BaseItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.basePrice>(Items.Cache, null, row.BaseItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.basePriceVal>(Items.Cache, null, row.BaseItemID != null);

			PXUIFieldAttribute.SetVisible<ContractItem.renewalPriceOption>(sender, row, row.RenewalItemID != null);
			PXUIFieldAttribute.SetVisible<ContractItem.renewalPrice>(sender, row, row.RenewalItemID != null);
			PXUIFieldAttribute.SetVisible<ContractItem.renewalPriceVal>(sender, row, row.RenewalItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.renewalPriceOption>(Items.Cache, null, row.RenewalItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.renewalPrice>(Items.Cache, null, row.RenewalItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.renewalPriceVal>(Items.Cache, null, row.RenewalItemID != null);

			PXUIFieldAttribute.SetVisible<ContractItem.fixedRecurringPriceOption>(sender, row, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractItem.fixedRecurringPrice>(sender, row, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractItem.fixedRecurringPriceVal>(sender, row, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.fixedRecurringPriceOption>(Items.Cache, null, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.fixedRecurringPrice>(Items.Cache, null, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.fixedRecurringPriceVal>(Items.Cache, null, row.RecurringItemID != null);

			PXUIFieldAttribute.SetVisible<ContractItem.usagePriceOption>(sender, row, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractItem.usagePrice>(sender, row, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractItem.usagePriceVal>(sender, row, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.usagePriceOption>(Items.Cache, null, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.usagePrice>(Items.Cache, null, row.RecurringItemID != null);
			PXUIFieldAttribute.SetVisible<ContractDetail.usagePriceVal>(Items.Cache, null, row.RecurringItemID != null);
		}

		#endregion

		protected static void UpdatePrices(ContractMaint graph, ContractDetail item)
		{
			Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(graph, item.ContractID);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (CTPRType.IsTemplate(contract.BaseType) != true)
				{
					ContractMaintExt contractMaintExt = CreateInstance<ContractMaintExt>();
					contractMaintExt.Contracts.Current = contract;
					if (contract.IsActive == true && 
						contract.IsPendingUpdate != true &&
						contract.Status != Contract.status.PendingActivation)
					{
						CTBillEngine engine = CreateInstance<CTBillEngine>();
						engine.Upgrade(contract.ContractID);
						engine.Clear();
						contractMaintExt.Contracts.Current = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(engine, item.ContractID);
						item = PXSelect<ContractDetail,
							Where<ContractDetail.contractID, Equal<Required<ContractDetail.contractID>>,
								And<ContractDetail.lineNbr, Equal<Required<ContractDetail.lineNbr>>>>>.Select(engine, item.ContractID, item.LineNbr);
					}
					contractMaintExt.ContractDetails.Cache.SetDefaultExt<ContractDetail.basePriceOption>(item);
					contractMaintExt.ContractDetails.Cache.SetDefaultExt<ContractDetail.basePrice>(item);
					contractMaintExt.ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPriceOption>(item);
					contractMaintExt.ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPrice>(item);
					contractMaintExt.ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPriceOption>(item);
					contractMaintExt.ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPrice>(item);
					contractMaintExt.ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePriceOption>(item);
					contractMaintExt.ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePrice>(item);
					contractMaintExt.ContractDetails.Update(item);
				
					contractMaintExt.Actions.PressSave();
				}
				else
				{
					TemplateMaint templateMaint = CreateInstance<TemplateMaint>();
					templateMaint.Templates.Current = PXSelect<ContractTemplate, Where<ContractTemplate.contractID, Equal<Required<ContractTemplate.contractID>>>>.Select(graph, item.ContractID);
					templateMaint.ContractDetails.Cache.SetDefaultExt<ContractDetail.basePriceOption>(item);
					templateMaint.ContractDetails.Cache.SetDefaultExt<ContractDetail.basePrice>(item);
					templateMaint.ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPriceOption>(item);
					templateMaint.ContractDetails.Cache.SetDefaultExt<ContractDetail.renewalPrice>(item);
					templateMaint.ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPriceOption>(item);
					templateMaint.ContractDetails.Cache.SetDefaultExt<ContractDetail.fixedRecurringPrice>(item);
					templateMaint.ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePriceOption>(item);
					templateMaint.ContractDetails.Cache.SetDefaultExt<ContractDetail.usagePrice>(item);
					templateMaint.ContractDetails.Update(item);
					templateMaint.Actions.PressSave();
				}
				ts.Complete();
			}
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewContract(PXAdapter adapter)
		{
			ContractDetail contractDetail = Items.Current;

			if (contractDetail != null)
			{
				PXGraph graph = null;
				string message = null;

				Contract contract = ContractMaint.FindContract(this, contractDetail.ContractID);

				if (CTPRType.IsTemplate(contract.BaseType) == true)
				{
					TemplateMaint templateMaint = PXGraph.CreateInstance<TemplateMaint>();
					templateMaint.Templates.Current = templateMaint.Templates.Search<ContractTemplate.contractID>(contractDetail.ContractID);

					graph = templateMaint;
					message = CT.Messages.ViewContractTemplate;
				}
				else
				{
					ContractMaint contractMaint = PXGraph.CreateInstance<ContractMaint>();
					contractMaint.Contracts.Current = contract;

					graph = contractMaint;
					message = CT.Messages.ViewContract;
				}

				throw new PXRedirectRequiredException(graph, true, message)
				{
					Mode = PXBaseRedirectException.WindowMode.NewWindow
				};
			}

			return Filter.Select();
		}

		#region Local Types
		[Serializable]
		public partial class ContractFilter : IBqlTable
		{
			#region ContractItemID
			public abstract class contractItemID : PX.Data.BQL.BqlInt.Field<contractItemID> { }
			
			[PXDBInt]
			[PXDefault]
			[PXDimensionSelector(ContractItemAttribute.DimensionName,
				typeof(Search<ContractItem.contractItemID>),
				typeof(ContractItem.contractItemCD),
				typeof(ContractItem.contractItemCD), typeof(ContractItem.descr))]
			[PXUIField(DisplayName = "Item Code")]
			public virtual Int32? ContractItemID { get; set; }
			#endregion
		}
		#endregion
	}
}
