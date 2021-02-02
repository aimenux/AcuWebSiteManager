using System;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.DR.Descriptor;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using ComponentAmount = System.Tuple<PX.Objects.DR.Descriptor.InventoryItemComponentInfo, decimal>;

namespace PX.Objects.DR
{

	public class AccountSubaccountPair
	{
		public int? AccountID { get; }
		public int? SubID { get; }

		public AccountSubaccountPair(int? accountID, int? subID)
		{
			this.AccountID = accountID;
			this.SubID = subID;
		}
	}

	/// <summary>
	/// Entity responsible for creating and re-evaluating deferral schedules.
	/// </summary>
	public class ScheduleCreator
	{
		DRSchedule _schedule;

	    protected readonly IFinPeriodRepository FinPeriodRepository;

		private readonly IDREntityStorage _drEntityStorage;
		private readonly ISubaccountProvider _subaccountProvider;
		private readonly IBusinessAccountProvider _businessAccountProvider;
		private readonly IInventoryItemProvider _inventoryItemProvider;

		private readonly Func<decimal, decimal> _roundingFunction;

		private readonly bool _isDraft;
		private readonly int? _branchID;

		/// <param name="roundingFunction">
		/// The function that should be used for rounding transaction amounts.
		/// </param>
		/// <param name="isDraft">
		/// Indicates whether the schedule to be created or reevaluated is a draft schedule.
		/// In particular, it affects whether credit line / deferral transactions would be 
		/// generated for the schedule.
		/// </param>
		public ScheduleCreator(
			IDREntityStorage drEntityStorage, 
			ISubaccountProvider subaccountProvider, 
			IBusinessAccountProvider businessAccountProvider, 
			IInventoryItemProvider inventoryItemProvider,
			IFinPeriodRepository finPeriodRepository,
            Func<decimal, decimal> roundingFunction, 
			int? branchID, 
			bool isDraft)
		{
			if (drEntityStorage == null) throw new ArgumentNullException(nameof(drEntityStorage));
			if (subaccountProvider == null) throw new ArgumentNullException(nameof(subaccountProvider));
			if (businessAccountProvider == null) throw new ArgumentNullException(nameof(businessAccountProvider));
			if (inventoryItemProvider == null) throw new ArgumentNullException(nameof(inventoryItemProvider));
		    if (finPeriodRepository == null) throw new ArgumentNullException(nameof(finPeriodRepository));
            if (roundingFunction == null) throw new ArgumentNullException(nameof(roundingFunction));

			_drEntityStorage = drEntityStorage;
			_subaccountProvider = subaccountProvider;
			_businessAccountProvider = businessAccountProvider;
			_inventoryItemProvider = inventoryItemProvider;
		    FinPeriodRepository = finPeriodRepository;

            _roundingFunction = roundingFunction;

			_isDraft = isDraft;
			_branchID = branchID;
		}

		private void ValidateMDAConsistency(InventoryItem inventoryItem, DRDeferredCode deferralCode)
		{
			if (inventoryItem != null &&
				inventoryItem.IsSplitted == true && 
				deferralCode.MultiDeliverableArrangement == false)
			{
				throw new PXException(Messages.DeferralCodeNotMDA);
			}
			else if (
				inventoryItem == null &&
				deferralCode.MultiDeliverableArrangement == true)
			{
				throw new PXException(Messages.MDACodeButNoInventoryItem);
			}
		}

		/// <param name="deferralCode">The item-level deferral code for <paramref name="inventoryItem"/>.</param>
		/// <param name="inventoryItem">The inventory item from the document line.</param>
		/// <param name="transactionAmount">Total transaction amount (with ALL discounts applied).</param>
		/// <param name="fairUnitPrice">The item price from the price list.</param>
		/// <param name="compoundDiscountRate"> Compound discount rate of all discounts 
		/// (including line, group and document discounts) that are applicable to deferred components.</param>
		public void CreateOriginalSchedule(
			DRProcess.DRScheduleParameters scheduleParameters, 
			DRDeferredCode deferralCode, 
			InventoryItem inventoryItem, 
			AccountSubaccountPair salesOrExpenseAccountSubaccount,
			decimal? transactionAmount, 
			decimal? fairUnitPrice, 
			decimal? compoundDiscountRate, 
			decimal? quantityInBaseUnit)
		{
			ValidateMDAConsistency(inventoryItem, deferralCode);

			_schedule = _drEntityStorage.CreateCopy(scheduleParameters);

			_schedule.IsDraft = _isDraft;
			_schedule.IsCustom = false;
			
			_schedule = _drEntityStorage.Insert(_schedule);

			CreateDetails(
				scheduleParameters, 
				deferralCode, 
				inventoryItem, 
				salesOrExpenseAccountSubaccount, 
				transactionAmount, 
				fairUnitPrice, 
				compoundDiscountRate, 
				quantityInBaseUnit);
		}

		/// <param name="attachedToOriginalSchedule">
		/// Flag added to handle <see cref="DRScheduleDetail"/>'s status 
		/// in the same way as <see cref="DRProcess"/> had done for documents 
		/// attached to original schedule.
		/// </param>
		public void ReevaluateSchedule(
			DRSchedule schedule, 
			DRProcess.DRScheduleParameters scheduleParameters, 
			DRDeferredCode deferralCode, 
			decimal? lineAmount,
			bool attachedToOriginalSchedule)
		{
			_schedule = schedule;

			_schedule.DocDate = scheduleParameters.DocDate;
			_schedule.BAccountID = scheduleParameters.BAccountID;
			_schedule.BAccountLocID = scheduleParameters.BAccountLocID;
			_schedule.FinPeriodID = scheduleParameters.FinPeriodID;
			_schedule.TranDesc = scheduleParameters.TranDesc;
			_schedule.IsCustom = false;
			_schedule.IsDraft = _isDraft;
			_schedule.BAccountType = _schedule.Module == BatchModule.AP ? BAccountType.VendorType : BAccountType.CustomerType;
			_schedule.TermStartDate = scheduleParameters.TermStartDate;
			_schedule.TermEndDate = scheduleParameters.TermEndDate;
			_schedule.ProjectID = scheduleParameters.ProjectID;
			_schedule.TaskID = scheduleParameters.TaskID;

			_schedule = _drEntityStorage.Update(_schedule);

			IList<DRScheduleDetail> scheduleDetails = _drEntityStorage.GetScheduleDetails(_schedule.ScheduleID);

			ReevaluateComponentAmounts(scheduleDetails, lineAmount);

			foreach (DRScheduleDetail scheduleDetail in scheduleDetails)
			{
				scheduleDetail.DocDate = _schedule.DocDate;
				scheduleDetail.BAccountID = _schedule.BAccountID;

			    FinPeriod detailFinPeriod = FinPeriodRepository
			        .GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(scheduleDetail.BranchID),
			            _schedule.FinPeriodID).GetValueOrRaiseError();

                scheduleDetail.FinPeriodID = detailFinPeriod.FinPeriodID;
			    scheduleDetail.TranPeriodID = detailFinPeriod.MasterFinPeriodID;

                if (!attachedToOriginalSchedule)
				{
					scheduleDetail.Status = _isDraft ? 
						DRScheduleStatus.Draft : 
						(scheduleDetail.IsResidual == null ? DRScheduleStatus.Closed : DRScheduleStatus.Open);
				}
				else
				{
					scheduleDetail.Status = _isDraft ?
						DRScheduleStatus.Draft :
						(scheduleDetail.IsOpen == true ? DRScheduleStatus.Open : DRScheduleStatus.Closed);
				}

				_drEntityStorage.Update(scheduleDetail);

				if (scheduleDetail.IsResidual != true)
				{
					DRDeferredCode detailDeferralCode = _drEntityStorage.GetDeferralCode(scheduleDetail.DefCode);

					IEnumerable<DRScheduleTran> componentTransactions =
						_drEntityStorage.GetDeferralTransactions(scheduleDetail.ScheduleID, scheduleDetail.ComponentID, scheduleDetail.DetailLineNbr);

					if (componentTransactions.Any())
					{
						ReevaluateTransactionAmounts(scheduleDetail, detailDeferralCode, componentTransactions);
					}

					if (!_isDraft)
					{
						if (!componentTransactions.Any())
						{
							componentTransactions = _drEntityStorage.CreateDeferralTransactions(_schedule, scheduleDetail, detailDeferralCode, _branchID);
						}

						_drEntityStorage.CreateCreditLineTransaction(scheduleDetail, deferralCode, _branchID);
						_drEntityStorage.NonDraftDeferralTransactionsPrepared(scheduleDetail, detailDeferralCode, componentTransactions);
					}
				}
			}
		}

		/// <summary>
		/// Reevaluates the provided schedule details' amounts, prorating
		/// them in order for their sum to match the given line amount.
		/// </summary>
		private void ReevaluateComponentAmounts(
			IEnumerable<DRScheduleDetail> scheduleDetails,
			decimal? lineAmount)
		{
			decimal? detailsTotal = scheduleDetails.Sum(scheduleDetail => scheduleDetail.CuryTotalAmt);

			if (lineAmount != detailsTotal)
			{
				if (scheduleDetails.IsSingleElement())
				{
					scheduleDetails.Single().CuryTotalAmt = lineAmount;
					scheduleDetails.Single().CuryDefAmt = lineAmount;
				}
				else if (scheduleDetails.HasAtLeastTwoItems())
				{
					decimal correctedTotal = 0;

					scheduleDetails.SkipLast(1).ForEach(scheduleDetail =>
					{
						decimal correctedRaw = scheduleDetail.CuryTotalAmt.Value * lineAmount.Value / detailsTotal.Value;
						decimal corrected = _roundingFunction(correctedRaw);

						correctedTotal += corrected;

						scheduleDetail.CuryTotalAmt = corrected;
						scheduleDetail.CuryDefAmt = corrected;
					});

					scheduleDetails.Last().CuryTotalAmt = lineAmount - correctedTotal;
					scheduleDetails.Last().CuryDefAmt = lineAmount - correctedTotal;
				}
			}
		}

		/// <summary>
		/// For a given schedule detail, reevaluates deferral transactions if they exist.
		/// </summary>
		private void ReevaluateTransactionAmounts(
			DRScheduleDetail scheduleComponent, 
			DRDeferredCode componentDeferralCode, 
			IEnumerable<DRScheduleTran> componentTransactions)
		{
			decimal? totalTransactionAmount = componentTransactions.Sum(transaction => transaction.Amount);

			if (totalTransactionAmount == scheduleComponent.CuryTotalAmt)
			{
				return;
			}

			if (componentTransactions.IsSingleElement())
			{
				UpdateTransactionAmount(
					scheduleComponent,
					componentDeferralCode,
					componentTransactions.Single(),
					scheduleComponent.CuryTotalAmt);
			}
			else if (componentTransactions.HasAtLeastTwoItems())
			{
				decimal correctedTotal = 0;

				componentTransactions.SkipLast(1).ForEach(transaction =>
				{
					decimal correctedAmountRaw = transaction.Amount.Value * scheduleComponent.CuryTotalAmt.Value / totalTransactionAmount.Value;
					decimal correctedAmount = _roundingFunction(correctedAmountRaw);

					correctedTotal += correctedAmount;

					UpdateTransactionAmount(
						scheduleComponent,
						componentDeferralCode,
						transaction,
						correctedAmount);
				});

				UpdateTransactionAmount(
					scheduleComponent,
					componentDeferralCode,
					componentTransactions.Last(),
					scheduleComponent.CuryTotalAmt - correctedTotal);
			}
		}

		/// <summary>
		/// Updates the transaction amount and performs the callback to
		/// <see cref="IDREntityStorage"/>, notifying about the update.
		/// </summary>
		private void UpdateTransactionAmount(
			DRScheduleDetail scheduleDetail, 
			DRDeferredCode detailDeferralCode, 
			DRScheduleTran transaction, 
			decimal? newAmount)
		{
			DRScheduleTran oldTransaction = _drEntityStorage.CreateCopy(transaction);

			transaction.Amount = newAmount;

			_drEntityStorage.ScheduleTransactionModified(
				scheduleDetail,
				detailDeferralCode,
				oldTransaction,
				transaction);
		}

		private void CreateDetails(
			DRProcess.DRScheduleParameters scheduleParameters, 
			DRDeferredCode deferralCode, 
			InventoryItem inventoryItem, 
			AccountSubaccountPair salesOrExpenseAccountSubaccount,
			decimal? transactionAmount, 
			decimal? fairUnitPrice, 
			decimal? compoundDiscountRate, 
			decimal? quantityInBaseUnit)
		{
			if (deferralCode.MultiDeliverableArrangement == true && inventoryItem != null && inventoryItem.IsSplitted == true)
			{
				CreateDetailsForSplitted(
					scheduleParameters, 
					inventoryItem, 
					salesOrExpenseAccountSubaccount.SubID, 
					transactionAmount, 
					fairUnitPrice, 
					compoundDiscountRate ?? 1.0m, 
					quantityInBaseUnit ?? 0.0m);
			}
			else
			{
				var deferralAccountSubaccount = GetDeferralAccountSubaccount(deferralCode, inventoryItem, scheduleParameters);
				
				int? componentID = inventoryItem == null ? DRScheduleDetail.EmptyComponentID : inventoryItem.InventoryID;

				DRScheduleDetail scheduleDetail = InsertScheduleDetail(
					componentID, 
					deferralCode, 
					transactionAmount.Value, 
					deferralAccountSubaccount,
					salesOrExpenseAccountSubaccount);

				if (!_isDraft)
				{
					IEnumerable<DRScheduleTran> deferralTransactions = 
						_drEntityStorage.CreateDeferralTransactions(_schedule, scheduleDetail, deferralCode, _branchID);

					_drEntityStorage.NonDraftDeferralTransactionsPrepared(scheduleDetail, deferralCode, deferralTransactions);
				}
			}
		}

		private void CreateDetailsForSplitted(
			DRProcess.DRScheduleParameters scheduleParameters, 
			InventoryItem inventoryItem, 
			int? subaccountID, 
			decimal? transactionAmount, 
			decimal? fairUnitPrice, 
			decimal compoundDiscountRate, 
			decimal qtyInBaseUnit)
		{
			int? salesExpenseSubIDOverride = inventoryItem.UseParentSubID == true ? null : subaccountID;

			IEnumerable<InventoryItemComponentInfo> fixedAllocationComponents = 
				_inventoryItemProvider.GetInventoryItemComponents(inventoryItem.InventoryID, INAmountOption.FixedAmt);

			IEnumerable<InventoryItemComponentInfo> percentageAllocationComponents =
				_inventoryItemProvider.GetInventoryItemComponents(inventoryItem.InventoryID, INAmountOption.Percentage);

			IEnumerable<InventoryItemComponentInfo> residualAllocationComponents =
				_inventoryItemProvider.GetInventoryItemComponents(inventoryItem.InventoryID, INAmountOption.Residual);

			if (residualAllocationComponents.Any() && !residualAllocationComponents.IsSingleElement())
			{
				throw new PXException(Messages.TooManyResiduals);
			}

			bool canUseResidual = residualAllocationComponents.Any();

			decimal fixedTotalAmount = 0;

			foreach (InventoryItemComponentInfo componentInfo in fixedAllocationComponents)
			{
				INComponent component = componentInfo.Component;

				decimal amountRaw = (component.FixedAmt ?? 0) * qtyInBaseUnit * compoundDiscountRate;
				decimal amount = _roundingFunction(amountRaw);

				fixedTotalAmount += amount;

				AddComponentScheduleDetail(
					componentInfo.Component, 
					componentInfo.DeferralCode, 
					componentInfo.Item, 
					Math.Sign((decimal)transactionAmount) * amount, 
					scheduleParameters,
					salesExpenseSubIDOverride);
			}

			decimal fixedPerUnit = 
				fixedAllocationComponents.Sum(componentInfo => componentInfo.Component.FixedAmt ?? 0);

			decimal amountToDistribute = transactionAmount.Value - fixedTotalAmount;
			
			if (transactionAmount >= 0 && amountToDistribute < 0 || 
				transactionAmount < 0 && amountToDistribute > 0)
			{
				throw new PXException(Messages.FixedAmtSumOverload);
			}

			if (!percentageAllocationComponents.Any() && amountToDistribute != 0 && !canUseResidual)
			{
				throw new PXException(Messages.NoResidual);
			}

			if (percentageAllocationComponents.Any())
			{
				bool canUseResidualInPercentageDistribution = 
					canUseResidual && fairUnitPrice != null && fairUnitPrice != 0.0m;

				PercentageDistribution distribution;

				if (canUseResidualInPercentageDistribution)
				{
					distribution = new PercentageWithResidualDistribution(
						_inventoryItemProvider,
						percentageAllocationComponents,
						fairUnitPrice.Value,
						fixedPerUnit,
						compoundDiscountRate,
						qtyInBaseUnit,
						_roundingFunction);
				}
				else
				{
					distribution = new PercentageDistribution(
						_inventoryItemProvider,
						percentageAllocationComponents,
						_roundingFunction);
				}

				IEnumerable<ComponentAmount> percentageAmounts = 
					distribution.Distribute(transactionAmount.Value, amountToDistribute);

				foreach (var componentAmount in percentageAmounts)
				{
					AddComponentScheduleDetail(
						componentAmount.Item1.Component, 
						componentAmount.Item1.DeferralCode,
						componentAmount.Item1.Item, 
						componentAmount.Item2, 
						scheduleParameters,
						salesExpenseSubIDOverride);
				}

				amountToDistribute -= percentageAmounts.Sum(componentAmount => componentAmount.Item2);
			}

			if (canUseResidual && amountToDistribute > 0m)
			{
				INComponent residualComponent = residualAllocationComponents.Single().Component;
				InventoryItem residualComponentItem = residualAllocationComponents.Single().Item;

				AccountSubaccountPair salesOrExpenseAccountSubaccount = 
					GetSalesOrExpenseAccountSubaccount(residualComponent, residualComponentItem);

				InsertResidualScheduleDetail(
					residualComponent.ComponentID, 
					amountToDistribute, 
					salesOrExpenseAccountSubaccount.AccountID, 
					salesExpenseSubIDOverride ?? salesOrExpenseAccountSubaccount.SubID);
			}
		}

		private void AddComponentScheduleDetail(
			INComponent inventoryItemComponent,
			DRDeferredCode componentDeferralCode,
			InventoryItem inventoryItem, 
			decimal amount, 
			DRProcess.DRScheduleParameters tranInfo,
			int? overridenSubID)
		{
			if (amount == 0m) return;

			DRScheduleDetail scheduleDetail = InsertScheduleDetail(
				inventoryItem.InventoryID,
				componentDeferralCode,
				amount,
				GetDeferralAccountSubaccount(componentDeferralCode, inventoryItem, tranInfo),
				GetSalesOrExpenseAccountSubaccount(inventoryItemComponent, inventoryItem),
				overridenSubID);

			if (!_isDraft)
			{
				IEnumerable<DRScheduleTran> deferralTransactions 
					= _drEntityStorage.CreateDeferralTransactions(_schedule, scheduleDetail, componentDeferralCode, _branchID);

				_drEntityStorage.NonDraftDeferralTransactionsPrepared(scheduleDetail, componentDeferralCode, deferralTransactions);
			}
		}

		private AccountSubaccountPair GetSalesOrExpenseAccountSubaccount(INComponent component, InventoryItem componentItem)
		{
			return _schedule.Module == BatchModule.AP
				? new AccountSubaccountPair(componentItem.COGSAcctID, componentItem.COGSSubID)
				: new AccountSubaccountPair(component.SalesAcctID, component.SalesSubID);
		}

		private DRScheduleDetail InsertScheduleDetail(
			int? componentID, 
			DRDeferredCode defCode, 
			decimal amount, 
			AccountSubaccountPair deferralAccountSubaccount, 
			AccountSubaccountPair salesOrExpenseAccountSubaccount,
			int? overridenSubID = null)
		{
		    FinPeriod detailFinPeriod = FinPeriodRepository
		        .GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(_branchID),
		            _schedule.FinPeriodID).GetValueOrRaiseError();

            DRScheduleDetail scheduleDetail = new DRScheduleDetail
			{
				ScheduleID = _schedule.ScheduleID,
				BranchID = _branchID,
				ComponentID = componentID,
				CuryTotalAmt = amount,
				CuryDefAmt = amount,
				DefCode = defCode.DeferredCodeID,
				Status = _isDraft ? DRScheduleStatus.Draft : DRScheduleStatus.Open,
				IsCustom = false,
				IsOpen = true,
				Module = _schedule.Module,
				DocType = _schedule.DocType,
				RefNbr = _schedule.RefNbr,
				LineNbr = _schedule.LineNbr,
				FinPeriodID = detailFinPeriod.FinPeriodID,
                TranPeriodID = detailFinPeriod.MasterFinPeriodID,
                BAccountID = _schedule.BAccountID,
				AccountID = salesOrExpenseAccountSubaccount.AccountID,
				SubID = overridenSubID ?? salesOrExpenseAccountSubaccount.SubID,
				DefAcctID = deferralAccountSubaccount.AccountID,
				DefSubID = deferralAccountSubaccount.SubID,
				CreditLineNbr = 0,
				DocDate = _schedule.DocDate,
				BAccountType =
					_schedule.Module == BatchModule.AP
						? BAccountType.VendorType
						: BAccountType.CustomerType,
			};

			scheduleDetail = _drEntityStorage.Insert(scheduleDetail);

			if (!_isDraft)
			{
				_drEntityStorage.CreateCreditLineTransaction(scheduleDetail, defCode, _branchID);
			}

			return scheduleDetail;
		}

		private void InsertResidualScheduleDetail(int? componentID, decimal amount, int? acctID, int? subID)
		{
		    FinPeriod detailFinPeriod = FinPeriodRepository
		        .GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(_branchID),
		            _schedule.FinPeriodID).GetValueOrRaiseError();

            DRScheduleDetail residualScheduleDetail = new DRScheduleDetail
			{
				ScheduleID = _schedule.ScheduleID,
				BranchID = _branchID,
				ComponentID = componentID,
				CuryTotalAmt = amount,
				CuryDefAmt = 0.0m,
				DefCode = null,
				IsCustom = false,
				IsResidual = true,
				IsOpen = false,
				Module = _schedule.Module,
				DocType = _schedule.DocType,
				RefNbr = _schedule.RefNbr,
				LineNbr = _schedule.LineNbr,
				FinPeriodID = detailFinPeriod.FinPeriodID,
                TranPeriodID = detailFinPeriod.MasterFinPeriodID,
                BAccountID = _schedule.BAccountID,
				AccountID = acctID,
				SubID = subID,
				DefAcctID = acctID,
				DefSubID = subID,
				CreditLineNbr = 0,
				DocDate = _schedule.DocDate,
				BAccountType = _schedule.Module == BatchModule.AP ? 
					BAccountType.VendorType :
					BAccountType.CustomerType,
				Status = _isDraft ? 
					DRScheduleStatus.Draft : 
					DRScheduleStatus.Closed,
			};

			residualScheduleDetail.CloseFinPeriodID = residualScheduleDetail.FinPeriodID;

			_drEntityStorage.Insert(residualScheduleDetail);
		}

		private AccountSubaccountPair GetDeferralAccountSubaccount(
			DRDeferredCode deferralCode, 
			InventoryItem item, 
			DRProcess.DRScheduleParameters scheduleParameters)
		{
			int? accountID = deferralCode.AccountID;

			string subaccountCD = null;
			int? subaccountID = null;

			if (deferralCode.AccountSource == DeferralAccountSource.Item)
			{
				accountID = item != null ? item.DeferralAcctID : subaccountID; 
				// this is fishy. subID is always null at this point.
			}

			if (deferralCode.CopySubFromSourceTran == true)
			{
				subaccountID = scheduleParameters.SubID;
			}
			else if (scheduleParameters.Module == BatchModule.AP)
			{
				int? itemSubID = item?.DeferralSubID;

				Location location = _businessAccountProvider
					.GetLocation(scheduleParameters.BAccountID, scheduleParameters.BAccountLocID);

				int? locationSubID = location?.VExpenseSubID;

				EPEmployee employee = _businessAccountProvider
					.GetEmployee(scheduleParameters.EmployeeID);
					
				int? employeeSubaccountID = employee?.ExpenseSubID;

				subaccountCD = _subaccountProvider.MakeSubaccount<DRScheduleDetail.subID>(
					deferralCode.DeferralSubMaskAP,
					new object[] { locationSubID, itemSubID, employeeSubaccountID, deferralCode.SubID },
					new []
					{
						typeof(Location.vExpenseSubID),
						typeof(InventoryItem.deferralSubID),
						typeof(EPEmployee.expenseSubID),
						typeof(DRDeferredCode.subID)
					});
			}
			else if (scheduleParameters.Module == BatchModule.AR)
			{
				int? itemSubID = item?.DeferralSubID;

				Location location = _businessAccountProvider
					.GetLocation(scheduleParameters.BAccountID, scheduleParameters.BAccountLocID);

				int? locationSubaccountID = location?.CSalesSubID;

				EPEmployee employee = _businessAccountProvider
					.GetEmployee(scheduleParameters.EmployeeID);

				int? employeeSubaccountID = employee?.SalesSubID;

				SalesPerson salesPerson = _businessAccountProvider
					.GetSalesPerson(scheduleParameters.SalesPersonID);

				int? salesPersonSubaccountID = salesPerson?.SalesSubID;

				subaccountCD = _subaccountProvider.MakeSubaccount<DRScheduleDetail.subID>(
					deferralCode.DeferralSubMaskAR,
					new object[] { locationSubaccountID, itemSubID, employeeSubaccountID, deferralCode.SubID, salesPersonSubaccountID },
					new []
					{
						typeof(Location.cSalesSubID),
						typeof(InventoryItem.deferralSubID),
						typeof(EPEmployee.salesSubID),
						typeof(DRDeferredCode.subID),
						typeof(SalesPerson.salesSubID)
					});
			}

			if (subaccountCD != null)
			{
				subaccountID = _subaccountProvider.GetSubaccountID(subaccountCD);
			}

			return new AccountSubaccountPair(accountID, subaccountID);
		}
	}

	#region Distributions

	class PercentageDistribution
	{
		protected IInventoryItemProvider _inventoryItemProvider;
		protected IEnumerable<InventoryItemComponentInfo> _percentageComponents;
		protected List<ComponentAmount> _amounts;
		protected Func<decimal, decimal> _roundingFunction;

		public PercentageDistribution(
			IInventoryItemProvider inventoryItemProvider,
			IEnumerable<InventoryItemComponentInfo> percentageComponents,
			Func<decimal, decimal> roundingFunction)
		{
			_inventoryItemProvider = inventoryItemProvider;
			_percentageComponents = percentageComponents;
			_roundingFunction = roundingFunction;
		}

		public virtual IEnumerable<ComponentAmount> Distribute(decimal transactionAmount, decimal amountToDistribute)
		{
			_amounts = new List<ComponentAmount>();

			foreach (InventoryItemComponentInfo componentInfo in _percentageComponents)
			{
				INComponent component = componentInfo.Component;

				decimal componentAmount = GetAmountForComponent(componentInfo.Component, amountToDistribute);

				if ((componentAmount < 0 && transactionAmount >= 0) || (componentAmount > 0 && transactionAmount <= 0))
				{
					throw new PXException(
						Messages.NegativeAmountForComponent,
						_inventoryItemProvider.GetComponentName(component));
				}

				_amounts.Add(new ComponentAmount(componentInfo, componentAmount));
			}

			return _amounts;
		}

		protected virtual decimal GetAmountForComponent(INComponent inventoryItemComponent, decimal amountToDistribute)
		{
			if (inventoryItemComponent != _percentageComponents.Last().Component)
			{
				decimal componentAmountRaw = 
					amountToDistribute * 
					inventoryItemComponent.Percentage.Value / 
					_percentageComponents.Sum(componentInfo => componentInfo.Component.Percentage.Value);

				return _roundingFunction(componentAmountRaw);
			}
			else
			{
				return amountToDistribute - _amounts.Sum(ca => ca.Item2);
			}
		}
	}

	class PercentageWithResidualDistribution : PercentageDistribution
	{
		private readonly decimal _fairUnitPrice;
		private readonly decimal _fixedPerUnit;
		private readonly decimal _compoundDiscountRate;
		private readonly decimal _qtyInBaseUnit;

		public PercentageWithResidualDistribution(
			IInventoryItemProvider inventoryItemProvider, 
			IEnumerable<InventoryItemComponentInfo> percentageComponents, 
			decimal fairUnitPrice, 
			decimal fixedPerUnit, 
			decimal compoundDiscountRate, 
			decimal qtyInBaseUnit,
			Func<decimal, decimal> roundingFunction)
				: base(inventoryItemProvider, percentageComponents, roundingFunction)
		{
			_fairUnitPrice = fairUnitPrice;
			_fixedPerUnit = fixedPerUnit;
			_compoundDiscountRate = compoundDiscountRate;
			_qtyInBaseUnit = qtyInBaseUnit;
		}

		public override IEnumerable<ComponentAmount> Distribute(decimal tranAmt, decimal amountToDistribute)
		{
			base.Distribute(tranAmt, amountToDistribute);

			if (amountToDistribute - _amounts.Sum(componentAmount => componentAmount.Item2) < 0)
			{
				_amounts = new PercentageDistribution(_inventoryItemProvider, _percentageComponents, _roundingFunction)
					.Distribute(tranAmt, amountToDistribute)
					.ToList();
			}

			return _amounts;
		}

		protected override decimal GetAmountForComponent(INComponent inventoryItemComponent, decimal amountToDistribute)
		{
			if (amountToDistribute == 0m)
				return 0m;

			decimal componentAmountRaw = 
				(_fairUnitPrice - _fixedPerUnit) * 
				_compoundDiscountRate * 
				_qtyInBaseUnit * 
				inventoryItemComponent.Percentage.Value * 0.01m;

			return _roundingFunction(componentAmountRaw);
		}
	}

	#endregion
}
