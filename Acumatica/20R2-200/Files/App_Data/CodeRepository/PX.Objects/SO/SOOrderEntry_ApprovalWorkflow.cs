using PX.Common;
using PX.Data;
using PX.Data.WorkflowAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
	using State = SOOrderStatus;
	using static SOOrder;
	using static BoundedTo<SOOrderEntry, SOOrder>;

	public class SOOrderEntry_ApprovalWorkflow : PXGraphExtension<SOOrderEntry_Workflow, SOOrderEntry>
	{
		private class SOSetupApproval : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<SOSetupApproval>(nameof(SOSetupApproval), typeof(SOSetup)).OrderRequestApproval;

			private bool OrderRequestApproval;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord soSetup = PXDatabase.SelectSingle<SOSetup>(new PXDataField<SOSetup.orderRequestApproval>()))
				{
					if (soSetup != null)
						OrderRequestApproval = (bool)soSetup.GetBoolean(0);
				}
			}
		}

		private static bool ApprovalIsActive => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && SOSetupApproval.IsActive;

		protected virtual bool IsAnyApprovalActive() => IsSOApprovalActive() || IsQTApprovalActive() || IsRMApprovalActive() || IsINApprovalActive() || IsCMApprovalActive();
		protected virtual bool IsSOApprovalActive() => ApprovalIsActive && true;
		protected virtual bool IsQTApprovalActive() => ApprovalIsActive && true;
		protected virtual bool IsRMApprovalActive() => ApprovalIsActive && false;
		protected virtual bool IsINApprovalActive() => ApprovalIsActive && true;
		protected virtual bool IsCMApprovalActive() => ApprovalIsActive && true;

		[PXWorkflowDependsOnType(typeof(SOSetup))]
		public override void Configure(PXScreenConfiguration config)
		{
			if (IsAnyApprovalActive())
				Configure(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
			else
				HideApproveAndRejectActions(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
		}

		protected virtual void Configure(WorkflowContext<SOOrderEntry, SOOrder> context)
		{
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsApprovedAndHasPaymentsInPendingProcessing
					= Bql<approved.IsEqual<True>.And<paymentsNeedValidationCntr.IsGreater<Zero>>>(),

				IsApprovedAndPrepaymentRequirementsViolated
					= Bql<approved.IsEqual<True>.And<prepaymentReqSatisfied.IsEqual<False>>>(),

				IsNotApproved
					= Bql<approved.IsEqual<False>>(),

				IsApproved
					= Bql<approved.IsEqual<True>>(),

				IsRejected
					= Bql<rejected.IsEqual<True>>(),
			}.AutoNameConditions();

			var approve = context.ActionDefinitions
				.CreateExisting<SOOrderEntry_ApprovalWorkflow>(g => g.approve, a => a
				.InFolder(FolderType.ActionsFolder)
				.PlaceAfter(g => g.putOnHold)
				.WithFieldAssignments(fa => fa.Add<approved>(e => e.SetFromValue(true))));
			var reject = context.ActionDefinitions
				.CreateExisting<SOOrderEntry_ApprovalWorkflow>(g => g.reject, a => a
				.InFolder(FolderType.ActionsFolder)
				.PlaceAfter(approve)
				.WithFieldAssignments(fa => fa.Add<rejected>(e => e.SetFromValue(true))));

			Workflow.ConfiguratorFlow InjectApprovalWorkflow(Workflow.ConfiguratorFlow flow, string behavior)
			{
				bool includeCreditHold = behavior.IsIn(SOBehavior.SO, SOBehavior.IN);
				bool inclCustOpenOrders = behavior.IsIn(SOBehavior.SO, SOBehavior.IN, SOBehavior.CM);

				const string initialState = "_";

				return flow
					.WithFlowStates(states =>
					{
						states.Add<State.pendingApproval>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add(approve, a => a.IsDuplicatedInToolbar());
									actions.Add(reject, a => a.IsDuplicatedInToolbar());
								});
						});
						states.Add<State.voided>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.copyOrder);
								});
						});
					})
					.WithTransitions(transitions =>
					{
						transitions.UpdateGroupFrom(initialState, ts =>
						{
							if (behavior == SOBehavior.SO)
							{
								ts.Update(
									t => t.To<State.pendingProcessing>().IsTriggeredOn(g => g.initializeState),
									t => t.When(conditions.IsApprovedAndHasPaymentsInPendingProcessing)); // HasPaymentsInPendingProcessing -> [IsApproved]AndHasPaymentsInPendingProcessing
								ts.Update(
									t => t.To<State.awaitingPayment>().IsTriggeredOn(g => g.initializeState),
									t => t.When(conditions.IsApprovedAndPrepaymentRequirementsViolated)); // IsPaymentRequirementsViolated -> [IsApproved]AndPrepaymentRequirementsViolated
							}

							ts.Update(
								t => t.To<State.open>().IsTriggeredOn(g => g.initializeState),
								t => t.When(conditions.IsApproved)); // null -> IsApproved

							ts.Add(t => t // New Pending Approval
								.To<State.pendingApproval>()
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsNotApproved)
								.WithFieldAssignments(fas =>
								{
									if (inclCustOpenOrders)
										fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
								}));
						});

						transitions.UpdateGroupFrom<State.hold>(ts =>
						{
							if (behavior == SOBehavior.SO)
							{
								ts.Update(
									t => t.To<State.pendingProcessing>().IsTriggeredOn(g => g.releaseFromHold),
									t => t.When(conditions.IsApprovedAndHasPaymentsInPendingProcessing)); // HasPaymentsInPendingProcessing -> [IsApproved]AndHasPaymentsInPendingProcessing

								ts.Update(
									t => t.To<State.awaitingPayment>().IsTriggeredOn(g => g.releaseFromHold),
									t => t.When(conditions.IsApprovedAndPrepaymentRequirementsViolated)); // IsPaymentRequirementsViolated -> [IsApproved]AndPrepaymentRequirementsViolated
							}

							ts.Update(
								t => t.To<State.open>().IsTriggeredOn(g => g.releaseFromHold),
								t => t.When(conditions.IsApproved)); // null -> IsApproved

							ts.Add(t => t
								.To<State.pendingApproval>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.When(conditions.IsNotApproved));

							if (behavior == SOBehavior.QT)
							{
								ts.Update(
									t => t.To<State.open>().IsTriggeredOn(g => g.openOrder),
									t => t.When(conditions.IsApproved));
								ts.Add( t => t
									.To<State.pendingApproval>()
									.IsTriggeredOn(g => g.openOrder)
									.When(conditions.IsNotApproved));
							}
						});

						if (includeCreditHold)
						{
							transitions.UpdateGroupFrom<State.creditHold>(ts =>
							{
								ts.Update(
									t => t.To<State.open>().IsTriggeredOn(g => g.releaseFromCreditHold),
									t => t.When(conditions.IsApproved));
								ts.Update(
									t => t.To<State.open>().IsTriggeredOn(g => g.OnCreditLimitSatisfied),
									t => t.When(conditions.IsApproved));

								ts.Add(t => t
									.To<State.pendingApproval>()
									.IsTriggeredOn(g => g.releaseFromCreditHold)
									.When(conditions.IsNotApproved)
									.WithFieldAssignments(fas =>
									{
										fas.Add<creditHold>(e => e.SetFromValue(false));
									}));
								ts.Add(t => t
									.To<State.pendingApproval>()
									.IsTriggeredOn(g => g.OnCreditLimitSatisfied)
									.When(conditions.IsNotApproved)
									.WithFieldAssignments(fas =>
									{
										fas.Add<creditHold>(e => e.SetFromValue(false));
									}));
							});
						}

						transitions.AddGroupFrom<State.pendingApproval>(ts =>
						{
							if (behavior == SOBehavior.SO)
							{
								ts.Add(t => t
									.To<State.pendingProcessing>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApprovedAndHasPaymentsInPendingProcessing));
								ts.Add(t => t
									.To<State.awaitingPayment>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApprovedAndPrepaymentRequirementsViolated));
							}

							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(approve)
								.When(conditions.IsApproved)
								.WithFieldAssignments(fas =>
								{
									if (inclCustOpenOrders)
										fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
								}));
							ts.Add(t => t
								.To<State.voided>()
								.IsTriggeredOn(reject)
								.When(conditions.IsRejected));
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold));
						});

						transitions.AddGroupFrom<State.voided>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold));
						});
					});
			}

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithFlows(flows =>
					{
						if (IsSOApprovalActive()) flows.Update<SOBehavior.sO>(f => InjectApprovalWorkflow(f, SOBehavior.SO));
						if (IsQTApprovalActive()) flows.Update<SOBehavior.qT>(f => InjectApprovalWorkflow(f, SOBehavior.QT));
						if (IsRMApprovalActive()) flows.Update<SOBehavior.rM>(f => InjectApprovalWorkflow(f, SOBehavior.RM));
						if (IsINApprovalActive()) flows.Update<SOBehavior.iN>(f => InjectApprovalWorkflow(f, SOBehavior.IN));
						if (IsCMApprovalActive()) flows.Update<SOBehavior.cM>(f => InjectApprovalWorkflow(f, SOBehavior.CM));
					})
					.WithActions(actions =>
					{
						actions.Add(approve);
						actions.Add(reject);
						actions.Update(
							g => g.putOnHold,
							a => a.WithFieldAssignments(fas =>
							{
								fas.Add<SOOrder.approved>(f => f.SetFromValue(false));
								fas.Add<SOOrder.rejected>(f => f.SetFromValue(false));
							}));
						actions.Update(
							g => g.openOrder,
							a => a.WithFieldAssignments(fas =>
							{
								fas.Add<SOOrder.hold>(f => f.SetFromValue(false));
							}));
					});
			});
		}

		protected virtual void HideApproveAndRejectActions(WorkflowContext<SOOrderEntry, SOOrder> context)
		{
			var approveHidden = context.ActionDefinitions
				.CreateExisting<SOOrderEntry_ApprovalWorkflow>(g => g.approve, a => a
				.InFolder(FolderType.ActionsFolder)
				.IsHiddenAlways());
			var rejectHidden = context.ActionDefinitions
				.CreateExisting<SOOrderEntry_ApprovalWorkflow>(g => g.reject, a => a
				.InFolder(FolderType.ActionsFolder)
				.IsHiddenAlways());

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add(approveHidden);
						actions.Add(rejectHidden);
					});
			});
		}

		public PXAction<SOOrder> approve;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Approve", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<SOOrder> reject;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Reject", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}