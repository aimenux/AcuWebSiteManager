using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using PX.Data;
using PX.Data.EP;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CT
{
	public class UsageMaint : PXGraph<UsageMaint, UsageMaint.UsageFilter>
	{
		#region DAC Attributes Override

		[NonStockItem]
		[PXRestrictor(typeof(Where<InventoryItem.kitItem, NotEqual<True>>), Messages.CannotUseKit)]
		protected virtual void PMTran_InventoryID_CacheAttached(PXCache sender) { }

		[PXFormula(typeof(Selector<PMTran.inventoryID, InventoryItem.baseUnit>))]
		[PMUnit(typeof(PMTran.inventoryID))]
		protected virtual void PMTran_UOM_CacheAttached(PXCache sender) { }

		[CustomerAndProspect]
		[PXDefault(typeof(Search<Contract.customerID, Where<Contract.contractID, Equal<Current<PMTran.projectID>>>>))]
		protected virtual void PMTran_BAccountID_CacheAttached(PXCache sender) { }

		[PXDBQuantity]
		[PXUIField(DisplayName = "Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void PMTran_BillableQty_CacheAttached(PXCache sender) { }

		[PXDBInt]
		[PXDefault(typeof(UsageFilter.contractID))]
		protected virtual void PMTran_ProjectID_CacheAttached(PXCache sender) { }
		
		[Branch(typeof(Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), IsDetail = false)]
		protected virtual void PMTran_BranchID_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(true)]
		protected virtual void PMTran_IsQtyOnly_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Type")]
		protected virtual void PMTran_ARTranType_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(
			typeof(PXUIFieldAttribute),
			nameof(PXUIFieldAttribute.DisplayName),
			"Reference Nbr.")]
		protected virtual void PMTran_ARRefNbr_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(
			typeof(PXUIFieldAttribute), 
			nameof(PXUIFieldAttribute.DisplayName), 
			"Billing Date")]
		protected virtual void PMTran_BilledDate_CacheAttached(PXCache sender) { }

		#endregion
		
		#region Selects/Views

		public PXSelect<PMRegister> Document;
		public PXFilter<UsageFilter> Filter;
		public PXSelect<Contract, Where<Contract.contractID, Equal<Current<UsageFilter.contractID>>>> CurrentContract;

		[PXImport(typeof(UsageFilter))]
		public PXSelectJoin<
			PMTran, 
			LeftJoin<CRCase, 
				On<CRCase.noteID, Equal<PMTran.origRefID>, Or<CRCase.caseCD, Equal<PMTran.caseCD>>>>, 
			Where<PMTran.billed, Equal<False>, 
				And<PMTran.projectID, Equal<Current<UsageFilter.contractID>>>>> 
			UnBilled;

		public PXSelectJoin<
			PMTran, 
			LeftJoin<CRCase, 
				On<CRCase.noteID, Equal<PMTran.origRefID>, Or<CRCase.caseCD, Equal<PMTran.caseCD>>>>, 
			Where<PMTran.billed, Equal<True>, 
				And<PMTran.projectID, Equal<Current<UsageFilter.contractID>>>>> 
			Billed;

		public PXSelect<ContractDetailAcum> ContractDetails;
		public PXSetup<ARSetup> arsetup;

		protected virtual IEnumerable billed()
		{
			UsageFilter filter = Filter.Current;

			if (filter == null)
			{
				return new List<PMTran>();
			}

			PXSelectBase<PMTran> select = new PXSelectJoin<
				PMTran,
				LeftJoin<CRCase,
					On<CRCase.noteID, Equal<PMTran.origRefID>, 
						Or<CRCase.caseCD, Equal<PMTran.caseCD>>>>, 
				Where<
					PMTran.billed, Equal<True>, 
					And<PMTran.projectID, Equal<Current<UsageFilter.contractID>>>>>
				(this);

			if (!string.IsNullOrEmpty(filter.InvFinPeriodID))
			{
				MasterFinPeriod finPeriod = PXSelect<MasterFinPeriod, Where<MasterFinPeriod.finPeriodID, Equal<Current<UsageFilter.invFinPeriodID>>>>.Select(this);

				if (finPeriod != null)
					select.WhereAnd<Where<PMTran.billedDate, Between<Required<MasterFinPeriod.startDate>, Required<MasterFinPeriod.endDate>>>>();
			}

			return select.Select();
		}

		#endregion

		public UsageMaint()
		{
			PXImportAttribute importAttribute = UnBilled.GetAttribute<PXImportAttribute>();
			importAttribute.MappingPropertiesInit += ImportAttributeMappingPropertiesHandler;

			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				AutoNumberAttribute.SetNumberingId<PMRegister.refNbr>(Document.Cache, arsetup.Current.UsageNumberingID);
			}

			EnsurePMDocumentCreated();
		}

		private void EnsurePMDocumentCreated()
		{
			if (Document.Cache.Inserted.Cast<PMRegister>().Any() == false)
			{
				Document.Cache.Insert();
				Document.Cache.IsDirty = false;
			}
		}

		#region Event Handlers

		/// <summary>
		/// Restricts the set of columns in Property Name combo box during Excel Import,
		/// so as to display only fields that are visible in the UI grid.
		/// </summary>
		protected virtual void ImportAttributeMappingPropertiesHandler(object sender, PXImportAttribute.MappingPropertiesInitEventArgs e)
		{
			e.Names.Clear();
			e.DisplayNames.Clear();

			IEnumerable<Type> fields = new Type[]
			{
				typeof(PMTran.branchID),
				typeof(PMTran.inventoryID),
				typeof(PMTran.description),
				typeof(PMTran.bAccountID),
				typeof(PMTran.uOM),
				typeof(PMTran.billableQty),
				typeof(PMTran.date),
				typeof(PMTran.caseCD)
			};

			IEnumerable<string> fieldNames = fields.Select(field => field.Name.Capitalize());
			IEnumerable<string> displayNames = fields.Select(field => PXUIFieldAttribute.GetDisplayName(UnBilled.Cache, field.Name));

			e.Names.Add(fieldNames);
			e.DisplayNames.Add(displayNames);
		}

		protected virtual void UsageFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			UsageFilter row = e.Row as UsageFilter;
			if (row != null)
			{
				UnBilled.Cache.AllowInsert = row.ContractID != null && (row.ContractStatus==Contract.status.Active || row.ContractStatus==Contract.status.InUpgrade);
			}
		}
		
		protected virtual void UsageFilter_ContractID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
				Contract contract = PXSelectReadonly<Contract,
					Where<Contract.contractID, Equal<Required<CRCase.contractID>>>>.Select(this, e.NewValue);

				if (contract != null)
				{
					#region Set Warnings for expired and 'In Grace Period' contracts

					int daysLeft;
					if (ContractMaint.IsExpired(contract, Accessinfo.BusinessDate.Value))
					{
						sender.RaiseExceptionHandling<UsageFilter.contractID>(e.Row, contract.ContractCD, new PXSetPropertyException(CR.Messages.ContractExpired, PXErrorLevel.Warning));

					}
					else if (ContractMaint.IsInGracePeriod(contract, Accessinfo.BusinessDate.Value, out daysLeft))
					{
						sender.RaiseExceptionHandling<UsageFilter.contractID>(e.Row, contract.ContractCD, new PXSetPropertyException(CR.Messages.ContractInGracePeriod, PXErrorLevel.Warning, daysLeft));
					}
					#endregion
				}

			}
		}

		public override void Clear()
		{
			base.Clear();

			// We need to insert the document after Cancel, because C-B API calls Cancel during the graph's lifetime
			// so we lose the PMRegister inserted in the graph constructor.
			//
			EnsurePMDocumentCreated();
		}

		protected virtual void PMTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Contract contract = CurrentContract.Select();
			if (contract == null) return;

			PXUIFieldAttribute.SetEnabled<PMTran.bAccountID>(sender, e.Row, contract.CustomerID == null);
		}
		
		protected virtual void PMTran_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<PMTran.locationID>(e.Row);
		}

		protected virtual void PMTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row=(PMTran)e.Row;
			if (row?.InventoryID == null)
			{
				return;
			}
			InventoryItem inventory = PXSelect<InventoryItem, 
				Where<InventoryItem.inventoryID, Equal<Required<PMTran.inventoryID>>>>.Select(this, row.InventoryID);
			if (inventory != null)
			{
				row.Description = inventory.Descr;

				PMProject project = PXSelect<PMProject, 
					Where<PMProject.contractID, Equal<Required<PMTran.projectID>>>>.Select(this, row.ProjectID);

				if (project != null && project.CustomerID != null)
				{
					Customer customer = PXSelect<Customer, 
						Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, project.CustomerID);
					if (customer != null && !string.IsNullOrEmpty(customer.LocaleName))
					{
						row.Description = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], inventory, nameof(InventoryItem.Descr), customer.LocaleName);
					}
				}
			}

		}

		protected virtual void PMTran_BillableQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null)
			{
				AddUsage(sender, row.ProjectID, row.InventoryID, (row.BillableQty ?? 0m) - ((decimal?)e.OldValue ?? 0m), row.UOM);
			}
		}

		protected virtual void PMTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null && row.BillableQty != 0)
			{
				SubtractUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty ?? 0m, (string)e.OldValue);
				AddUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty ?? 0m, row.UOM);
			}
		}

		protected virtual void PMTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null && row.BillableQty != 0)
			{
				SubtractUsage(sender, row.ProjectID, row.InventoryID, row.BillableQty ?? 0m, row.UOM);
			}
		}

		#endregion

		public static void AddUsage(PXCache cache, int? contractID, int? inventoryID, decimal used, string UOM)
		{
			if (contractID != null && inventoryID != null &&  used != 0)
			{
				decimal inTargetUnit = INUnitAttribute.ConvertToBase(cache, inventoryID, UOM, used, INPrecision.QUANTITY);

				//update all revisions starting from last active
				foreach (ContractDetailAcum item in PXSelectJoin<ContractDetailExt,
					InnerJoin<Contract, On<ContractDetailExt.contractID, Equal<Contract.contractID>>,
					InnerJoin<ContractItem, On<ContractItem.contractItemID, Equal<ContractDetailExt.contractItemID>>,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ContractItem.recurringItemID>>>>>,
					Where<ContractDetailExt.contractID, Equal<Required<ContractDetailExt.contractID>>,
						And<ContractDetailExt.revID, GreaterEqual<Contract.lastActiveRevID>,
						And<ContractItem.recurringItemID, Equal<Required<ContractItem.recurringItemID>>>>>>.Select(cache.Graph, contractID, inventoryID)
					.RowCast<ContractDetailExt>()
					.Select(detail => PXCache<ContractDetailAcum>.Insert(cache.Graph, new ContractDetailAcum {ContractDetailID = detail.ContractDetailID})))
				{
					item.Used += inTargetUnit;
					item.UsedTotal += inTargetUnit;
				}
			}
		}

		public static void SubtractUsage(PXCache sender, int? contractID, int? inventoryID, decimal used, string UOM)
		{
			AddUsage(sender, contractID, inventoryID, -used, UOM);
		}

		public override void Persist()
		{
			if (!UnBilled.Cache.Inserted.Cast<PMTran>().Any())
			{
				Document.Cache.Clear();
			}

			base.Persist();
		}
				
		#region Local Types

		[Serializable]
		public partial class UsageFilter : IBqlTable
		{
			#region ContractID
			public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
			protected Int32? _ContractID;
			[PXDBInt]
			[PXSelector(typeof(Search<Contract.contractID, 
				Where<Contract.baseType, Equal<CTPRType.contract>, 
					And<Contract.status, NotEqual<Contract.status.draft>, 
					And<Contract.status, NotEqual<Contract.status.inApproval>>>>>), 
			SubstituteKey = typeof(Contract.contractCD), DescriptionField = typeof(Contract.description))]
			[PXUIField(DisplayName = "Contract ID")]
			public virtual Int32? ContractID
			{
				get
				{
					return this._ContractID;
				}
				set
				{
					this._ContractID = value;
				}
			}
			#endregion

			#region InvFinPeriodID
			public abstract class invFinPeriodID : PX.Data.BQL.BqlString.Field<invFinPeriodID> { }
			protected string _InvFinPeriodID;
			[FinPeriodSelector]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
			public virtual String InvFinPeriodID
			{
				get
				{
					return this._InvFinPeriodID;
				}
				set
				{
					this._InvFinPeriodID = value;
				}
			}
			#endregion

			#region ContractStatus
			public abstract class contractStatus : PX.Data.BQL.BqlString.Field<contractStatus> { }
			protected String _ContractStatus;
			[PXDBString]
			[PXFormula(typeof(Selector<UsageFilter.contractID, Contract.status>))]
			public virtual String ContractStatus
			{
				get
				{
					return this._ContractStatus;
				}
				set
				{
					this._ContractStatus = value;
				}
			}
			#endregion

		}

		#endregion
	}

	


}
