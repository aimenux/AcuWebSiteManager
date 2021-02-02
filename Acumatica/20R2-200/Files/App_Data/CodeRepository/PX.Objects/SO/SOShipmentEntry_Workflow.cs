using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.Common.Extensions;
using PX.Objects.CR.Workflows;

namespace PX.Objects.SO
{
	using State = SOShipmentStatus;
	using static SOShipment;
	using static BoundedTo<SOShipmentEntry, SOShipment>;

	public class SOShipmentEntry_Workflow : PXGraphExtension<SOShipmentEntry>
	{
		public override void Configure(PXScreenConfiguration config) => Configure(config.GetScreenConfigurationContext<SOShipmentEntry, SOShipment>());

		protected virtual void Configure(WorkflowContext<SOShipmentEntry, SOShipment> context)
		{
			#region Conditions
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsOnHold
					= Bql<hold.IsEqual<True>>(),

				IsNotOnHold
					= Bql<hold.IsEqual<False>>(),

				IsNotBillable
					= Bql<unbilledOrderCntr.IsEqual<Zero>.And<billedOrderCntr.IsEqual<Zero>>.And<releasedOrderCntr.IsEqual<Zero>>>(),

				IsConfirmed
					= Bql<confirmed.IsEqual<True>.And<unbilledOrderCntr.IsGreater<Zero>>.And<billedOrderCntr.IsEqual<Zero>>.And<releasedOrderCntr.IsEqual<Zero>>>(),

				IsPartiallyInvoiced
					= Bql<confirmed.IsEqual<True>.And<unbilledOrderCntr.IsGreater<Zero>>.And<billedOrderCntr.IsGreater<Zero>.Or<releasedOrderCntr.IsGreater<Zero>>>>(),

				IsInvoiced
					= Bql<confirmed.IsEqual<True>.And<unbilledOrderCntr.IsEqual<Zero>>.And<billedOrderCntr.IsGreater<Zero>>>(),

				IsCompleted
					= Bql<confirmed.IsEqual<True>.And<unbilledOrderCntr.IsEqual<Zero>>.And<billedOrderCntr.IsEqual<Zero>>.And<releasedOrderCntr.IsGreater<Zero>>>(),
			}.AutoNameConditions();
			#endregion
			#region Macroses
			void DisableWholeScreen(FieldState.IContainerFillerFields states)
			{
				states.AddTable<SOShipment>(state => state.IsDisabled());
				states.AddTable<SOShipLine>(state => state.IsDisabled());
				states.AddTable<SOShipLineSplit>(state => state.IsDisabled());
				states.AddTable<SOShipmentAddress>(state => state.IsDisabled());
				states.AddTable<SOShipmentContact>(state => state.IsDisabled());
				states.AddTable<SOOrderShipment>(state => state.IsDisabled());
				//states.AddTable<*>(state => state.IsDisabled());
			}
			#endregion
			#region Event Handlers
			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnInvoiceLinked(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrderShipment>()
					.WithParametersOf<SOInvoice>()
					.OfEntityEvent<SOOrderShipment.Events>(e => e.InvoiceLinked)
					.Is(g => g.OnInvoiceLinked)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOShipment>.
						Where<
							SOShipment.shipmentType.IsEqual<SOOrderShipment.shipmentType.FromCurrent>.
							And<SOShipment.shipmentNbr.IsEqual<SOOrderShipment.shipmentNbr.FromCurrent>>>
						>()
					.DisplayName("Invoice Linked");
			}
            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnInvoiceUnlinked(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrderShipment>()
					.WithParametersOf<SOInvoice>()
					.OfEntityEvent<SOOrderShipment.Events>(e => e.InvoiceUnlinked)
					.Is(g => g.OnInvoiceUnlinked)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOShipment>.
						Where<
							SOShipment.shipmentType.IsEqual<SOOrderShipment.shipmentType.FromCurrent>.
							And<SOShipment.shipmentNbr.IsEqual<SOOrderShipment.shipmentNbr.FromCurrent>>>
						>()
					.DisplayName("Invoice Unlinked");
			}
            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnInvoiceReleased(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOInvoice>()
					.OfEntityEvent<SOInvoice.Events>(e => e.InvoiceReleased)
					.Is(g => g.OnInvoiceReleased)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOShipment>.
						InnerJoin<SOOrderShipment>.On<SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>>.
						Where<SOOrderShipment.FK.Invoice.SameAsCurrent>
					>(allowSelectMultipleRecords: true)
					.DisplayName("Invoice Released");
			}
            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnInvoiceCancelled(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOInvoice>()
					.OfEntityEvent<SOInvoice.Events>(e => e.InvoiceCancelled)
					.Is(g => g.OnInvoiceCancelled)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOShipment>.
						InnerJoin<SOOrderShipment>.On<SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>>.
						Where<SOOrderShipment.FK.Invoice.SameAsCurrent>
					>(allowSelectMultipleRecords: true)
					.DisplayName("Invoice Cancelled");
			}
			#endregion

			const string initialState = "_";

			context.AddScreenConfigurationFor(screen =>
				screen
				.StateIdentifierIs<status>()
				.AddDefaultFlow(flow =>
					flow
					.WithFlowStates(fss =>
					{
						fss.Add(initialState, flowState => flowState.IsInitial(g => g.initializeState));
						fss.Add<State.hold>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.printPickListAction);
								});
						});
						fss.Add<State.open>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.confirmShipmentAction, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.printShipmentConfirmation);
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.getReturnLabelsAction);
									actions.Add(g => g.printPickListAction);
								});
						});
						fss.Add<State.confirmed>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.createInvoice, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.UpdateIN, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.printShipmentConfirmation);
									actions.Add(g => g.correctShipmentAction);
									actions.Add(g => g.printLabels);
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.emailShipment);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnInvoiceLinked);
								})
								.WithFieldStates(DisableWholeScreen);
						});
						fss.Add<State.partiallyInvoiced>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.createInvoice, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.printShipmentConfirmation);
									actions.Add(g => g.printLabels);
									actions.Add(g => g.validateAddresses);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnInvoiceLinked);
									handlers.Add(g => g.OnInvoiceUnlinked);
								})
								.WithFieldStates(DisableWholeScreen);
						});
						fss.Add<State.invoiced>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.validateAddresses);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnInvoiceUnlinked);
									handlers.Add(g => g.OnInvoiceReleased);
									handlers.Add(g => g.OnInvoiceCancelled);
								})
								.WithFieldStates(DisableWholeScreen);
						});
						fss.Add<State.completed>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.printShipmentConfirmation);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnInvoiceUnlinked);
									handlers.Add(g => g.OnInvoiceCancelled);
								})
								.WithFieldStates(DisableWholeScreen);
						});
						fss.Add<State.receipted>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.createDropshipInvoice, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.cancelReturn);
								});
						});
					})
					.WithTransitions(transitions =>
					{
						transitions.AddGroupFrom(initialState, ts =>
						{
							ts.Add(t => t.To<State.hold>().IsTriggeredOn(g => g.initializeState).When(conditions.IsOnHold)); // New Hold
							ts.Add(t => t.To<State.open>().IsTriggeredOn(g => g.initializeState).When(conditions.IsNotOnHold)); // New Open
							ts.Add(t => t.To<State.confirmed>().IsTriggeredOn(g => g.initializeState).When(conditions.IsConfirmed)); // New Confirmed
							ts.Add(t => t.To<State.partiallyInvoiced>().IsTriggeredOn(g => g.initializeState).When(conditions.IsPartiallyInvoiced)); // New Partially Invoiced
							ts.Add(t => t.To<State.invoiced>().IsTriggeredOn(g => g.initializeState).When(conditions.IsInvoiced)); // New Invoiced
							ts.Add(t => t.To<State.completed>().IsTriggeredOn(g => g.initializeState).When(conditions.IsCompleted)); // New Completed
						});
						transitions.AddGroupFrom<State.hold>(ts =>
						{
							ts.Add(t => t.To<State.open>().IsTriggeredOn(g => g.releaseFromHold).WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(false))));
							ts.Add(t => t.To<State.open>().IsTriggeredOn(g => g.printPickListAction).WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(false))));
						});
						transitions.AddGroupFrom<State.open>(ts =>
						{
							ts.Add(t => t.To<State.hold>().IsTriggeredOn(g => g.putOnHold).WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(true))));
							ts.Add(t => t.To<State.confirmed>().IsTriggeredOn(g => g.confirmShipmentAction));
						});
						transitions.AddGroupFrom<State.confirmed>(ts =>
						{
							ts.Add(t => t.To<State.open>().IsTriggeredOn(g => g.correctShipmentAction));
							ts.Add(t => t.To<State.completed>().IsTriggeredOn(g => g.UpdateIN).When(conditions.IsNotBillable));
							ts.Add(t => t.To<State.invoiced>().IsTriggeredOn(g => g.OnInvoiceLinked).When(conditions.IsInvoiced));
							ts.Add(t => t.To<State.partiallyInvoiced>().IsTriggeredOn(g => g.OnInvoiceLinked).When(conditions.IsPartiallyInvoiced));
						});
						transitions.AddGroupFrom<State.partiallyInvoiced>(ts =>
						{
							ts.Add(t => t.To<State.confirmed>().IsTriggeredOn(g => g.OnInvoiceUnlinked).When(conditions.IsConfirmed));
							ts.Add(t => t.To<State.invoiced>().IsTriggeredOn(g => g.OnInvoiceLinked).When(conditions.IsInvoiced));
						});
						transitions.AddGroupFrom<State.invoiced>(ts =>
						{
							ts.Add(t => t.To<State.confirmed>().IsTriggeredOn(g => g.OnInvoiceUnlinked).When(conditions.IsConfirmed));
							ts.Add(t => t.To<State.partiallyInvoiced>().IsTriggeredOn(g => g.OnInvoiceUnlinked).When(conditions.IsPartiallyInvoiced));
							ts.Add(t => t.To<State.completed>().IsTriggeredOn(g => g.OnInvoiceReleased).When(conditions.IsCompleted));
							ts.Add(t => t.To<State.partiallyInvoiced>().IsTriggeredOn(g => g.OnInvoiceCancelled).When(conditions.IsPartiallyInvoiced));
						});
						transitions.AddGroupFrom<State.completed>(ts =>
						{
							ts.Add(t => t.To<State.confirmed>().IsTriggeredOn(g => g.OnInvoiceUnlinked).When(conditions.IsConfirmed));
							ts.Add(t => t.To<State.partiallyInvoiced>().IsTriggeredOn(g => g.OnInvoiceUnlinked).When(conditions.IsPartiallyInvoiced));
							ts.Add(t => t.To<State.invoiced>().IsTriggeredOn(g => g.OnInvoiceCancelled).When(conditions.IsInvoiced));
						});
						transitions.AddGroupFrom<State.receipted>(ts =>
						{
						});
					}))
				.WithActions(actions =>
				{
					actions.Add(g => g.initializeState, a => a.IsHiddenAlways());
					actions.Add(g => g.confirmShipmentAction, a => a
						.InFolder(FolderType.ActionsFolder)
						.MassProcessingScreen<SOInvoiceShipment>()
						.InBatchMode());
					actions.Add(g => g.createInvoice, a => a
						.InFolder(FolderType.ActionsFolder)
						.IsDisabledWhen(conditions.IsNotBillable)
						.MassProcessingScreen<SOInvoiceShipment>()
						.InBatchMode());
					actions.Add(g => g.createDropshipInvoice, a => a
						.InFolder(FolderType.ActionsFolder)
						.MassProcessingScreen<SOInvoiceShipment>()
						.InBatchMode());
					actions.Add(g => g.UpdateIN, a => a
						.InFolder(FolderType.ActionsFolder)
						.MassProcessingScreen<SOInvoiceShipment>()
						.InBatchMode());
					actions.Add(g => g.correctShipmentAction, a => a
						.InFolder(FolderType.ActionsFolder));
					actions.Add(g => g.printLabels, a => a
						.InFolder(FolderType.ActionsFolder)
						.MassProcessingScreen<SOInvoiceShipment>(/* +Confirmed, +PartInvoice */)
						.InBatchMode());
					actions.Add(g => g.getReturnLabelsAction, a => a
						.InFolder(FolderType.ActionsFolder));
					actions.Add(g => g.cancelReturn, a => a
						.InFolder(FolderType.ActionsFolder));
					actions.Add(g => g.validateAddresses, a => a
						.InFolder(FolderType.ActionsFolder));
					actions.Add(g => g.emailShipment, a => a
						.InFolder(FolderType.ActionsFolder)
						.MassProcessingScreen<SOInvoiceShipment>());
					actions.Add(g => g.printPickListAction, a => a
						.InFolder(FolderType.ActionsFolder)
						.MassProcessingScreen<SOInvoiceShipment>()
						.InBatchMode());
					actions.Add(g => g.putOnHold, a => a
						.InFolder(FolderType.ActionsFolder));
					actions.Add(g => g.releaseFromHold, a => a
						.InFolder(FolderType.ActionsFolder));
					actions.Add(g => g.printShipmentConfirmation, a => a
						.InFolder(FolderType.ReportsFolder)
						.MassProcessingScreen<SOInvoiceShipment>(/* +Open */)
						.InBatchMode());
				})
				.WithHandlers(handlers =>
				{
					handlers.Add(OnInvoiceLinked);
					handlers.Add(OnInvoiceUnlinked);
					handlers.Add(OnInvoiceReleased);
					handlers.Add(OnInvoiceCancelled);
				}));
		}
	}
}