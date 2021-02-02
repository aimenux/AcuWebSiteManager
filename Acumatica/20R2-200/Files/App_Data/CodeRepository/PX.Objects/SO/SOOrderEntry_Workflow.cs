using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
	using State = SOOrderStatus;
	using CreatePaymentExt = GraphExtensions.SOOrderEntryExt.CreatePaymentExt;
	using static SOOrder;
	using static BoundedTo<SOOrderEntry, SOOrder>;

	public class SOOrderEntry_Workflow : PXGraphExtension<SOOrderEntry>
	{
		public override void Configure(PXScreenConfiguration config) => Configure(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());

		protected virtual void Configure(WorkflowContext<SOOrderEntry, SOOrder> context)
		{
			#region Shared conditions and Macroses
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsOnHold
					= Bql<hold.IsEqual<True>>(),

				IsCompleted
					= Bql<completed.IsEqual<True>>(),

				IsOnCreditHold
					= Bql<creditHold.IsEqual<True>>(),

				HasPaymentsInPendingProcessing
					= Bql<paymentsNeedValidationCntr.IsGreater<Zero>>(),

				HasNoPaymentsInPendingProcessing
					= Bql<paymentsNeedValidationCntr.IsEqual<Zero>>(),

				IsPaymentRequirementsViolated
					= Bql<prepaymentReqSatisfied.IsEqual<False>>(),

				HasAllBillsReleased
					= Bql<billedCntr.IsEqual<Zero>.And<releasedCntr.IsGreater<Zero>>>(),

				IsUnbilled
					= Bql<billedCntr.IsEqual<Zero>.And<releasedCntr.IsEqual<Zero>>>(),

				IsInvoicedCompleted
					= Bql<completed.IsEqual<True>.
						Or<billedCntr.IsEqual<Zero>.And<releasedCntr.IsEqual<Zero>>>.
						Or<openShipmentCntr.IsEqual<Zero>.And<openLineCntr.IsEqual<Zero>>>>(),

				IsInvoicedBackOrder
					= Bql<openLineCntr.IsGreater<Zero>>(),

				IsShippingCompleted
					= Bql<completed.IsEqual<True>.Or<openShipmentCntr.IsEqual<Zero>.And<openLineCntr.IsEqual<Zero>>>>(),

				CanNotBeCompleted
					= Bql<completed.IsEqual<True>.Or<shipmentCntr.IsEqual<Zero>>.Or<openShipmentCntr.IsGreater<Zero>.Or<openLineCntr.IsGreater<Zero>>>>(),

				IsShippingOpenRM
					= Bql<openLineCntr.IsGreater<Zero>>(),

				IsShippable
					= Bql<openShipmentCntr.IsEqual<Zero>.And<openLineCntr.IsGreater<Zero>>>(),
			}.AutoNameConditions();

			void DisableWholeScreen(FieldState.IContainerFillerFields states)
			{
				states.AddTable<SOOrder>(state => state.IsDisabled());
				states.AddTable<SOLine>(state => state.IsDisabled());
				//states.AddTable<*>(state => state.IsDisabled());
				states.AddTable<SOTaxTran>(state => state.IsDisabled());
				states.AddTable<SOBillingAddress>(state => state.IsDisabled());
				states.AddTable<SOBillingContact>(state => state.IsDisabled());
				states.AddTable<SOShippingAddress>(state => state.IsDisabled());
				states.AddTable<SOShippingContact>(state => state.IsDisabled());
				states.AddTable<SOLineSplit>(state => state.IsDisabled());
			}
			#endregion

			#region Event Handlers
            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnObtainedPaymentInPendingProcessing(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrder>()
					.OfEntityEvent<SOOrder.Events>(e => e.ObtainedPaymentInPendingProcessing)
					.Is(g => g.OnObtainedPaymentInPendingProcessing)
					.UsesTargetAsPrimaryEntity();
			}

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnLostLastPaymentInPendingProcessing(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrder>()
					.OfEntityEvent<SOOrder.Events>(e => e.LostLastPaymentInPendingProcessing)
					.Is(g => g.OnLostLastPaymentInPendingProcessing)
					.UsesTargetAsPrimaryEntity();
			}

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnPaymentRequirementsSatisfied(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrder>()
					.OfEntityEvent<SOOrder.Events>(e => e.PaymentRequirementsSatisfied)
					.Is(g => g.OnPaymentRequirementsSatisfied)
					.UsesTargetAsPrimaryEntity()
					.DisplayName("Payment Requirements Satisfied");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnPaymentRequirementsViolated(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrder>()
					.OfEntityEvent<SOOrder.Events>(e => e.PaymentRequirementsViolated)
					.Is(g => g.OnPaymentRequirementsViolated)
					.UsesTargetAsPrimaryEntity()
					.DisplayName("Payment Requirements Violated");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnCreditLimitSatisfied(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrder>()
					.OfEntityEvent<SOOrder.Events>(e => e.CreditLimitSatisfied)
					.Is(g => g.OnCreditLimitSatisfied)
					.UsesTargetAsPrimaryEntity()
					.DisplayName("Credit Limit Satisfied");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnCreditLimitViolated(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrder>()
					.OfEntityEvent<SOOrder.Events>(e => e.CreditLimitViolated)
					.Is(g => g.OnCreditLimitViolated)
					.UsesTargetAsPrimaryEntity()
					.DisplayName("Credit Limit Violated");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnShipmentCreationFailed(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrder>()
					.OfEntityEvent<SOOrder.Events>(e => e.ShipmentCreationFailed)
					.Is(g => g.OnShipmentCreationFailed)
					.UsesTargetAsPrimaryEntity()
                    .DisplayName("Shipment Creation Failed");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnShipmentLinked(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrderShipment>()
					.WithParametersOf<SOShipment>()
					.OfEntityEvent<SOOrderShipment.Events>(e => e.ShipmentLinked)
					.Is(g => g.OnShipmentLinked)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOOrder>.
						Where<SOOrder.orderType.IsEqual<SOOrderShipment.orderType.FromCurrent>.
							And<SOOrder.orderNbr.IsEqual<SOOrderShipment.orderNbr.FromCurrent>>>
					>()
					.DisplayName("Shipment Linked");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnShipmentUnlinked(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrderShipment>()
					.WithParametersOf<SOShipment>()
					.OfEntityEvent<SOOrderShipment.Events>(e => e.ShipmentUnlinked)
					.Is(g => g.OnShipmentUnlinked)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOOrder>.
						Where<SOOrder.orderType.IsEqual<SOOrderShipment.orderType.FromCurrent>.
							And<SOOrder.orderNbr.IsEqual<SOOrderShipment.orderNbr.FromCurrent>>>
					>()
					.DisplayName("Shipment Unlinked");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnShipmentConfirmed(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOShipment>()
					.OfEntityEvent<SOShipment.Events>(e => e.ShipmentConfirmed)
					.Is(g => g.OnShipmentConfirmed)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOOrder>.
						InnerJoin<SOOrderShipment>.On<SOOrderShipment.FK.Order>.
						Where<SOOrderShipment.FK.Shipment.SameAsCurrent>
					>(allowSelectMultipleRecords: true)
					.DisplayName("Shipment Confirmed");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnShipmentCorrected(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOShipment>()
					.OfEntityEvent<SOShipment.Events>(e => e.ShipmentCorrected)
					.Is(g => g.OnShipmentCorrected)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOOrder>.
						InnerJoin<SOOrderShipment>.On<SOOrderShipment.FK.Order>.
						Where<SOOrderShipment.FK.Shipment.SameAsCurrent>
					>(allowSelectMultipleRecords: true)
					.DisplayName("Shipment Corrected");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase InvoiceLinked(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrderShipment>()
					.WithParametersOf<SOInvoice>()
					.OfEntityEvent<SOOrderShipment.Events>(e => e.InvoiceLinked)
					.Is(g => g.OnInvoiceLinked)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOOrder>.
						Where<SOOrder.orderType.IsEqual<SOOrderShipment.orderType.FromCurrent>.
							And<SOOrder.orderNbr.IsEqual<SOOrderShipment.orderNbr.FromCurrent>>>
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
						SelectFrom<SOOrder>.
						Where<SOOrder.orderType.IsEqual<SOOrderShipment.orderType.FromCurrent>.
							And<SOOrder.orderNbr.IsEqual<SOOrderShipment.orderNbr.FromCurrent>>>
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
						SelectFrom<SOOrder>.
						InnerJoin<SOOrderShipment>.On<SOOrderShipment.FK.Order>.
						Where<SOOrderShipment.FK.Invoice.SameAsCurrent>
					>(allowSelectMultipleRecords: true)
					.DisplayName("Invoice Released");
			}

			WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnOrderDeleted_ReopenQuote(WorkflowEventHandlerDefinition.INeedEventTarget handler)
			{
				return handler
					.WithTargetOf<SOOrder>()
					.OfEntityEvent<SOOrder.Events>(e => e.OrderDeleted)
					.Is(g => g.OnOrderDeleted_ReopenQuote)
					.UsesPrimaryEntityGetter<
						SelectFrom<SOOrder>.
						Where<
							orderType.IsEqual<origOrderType.FromCurrent>.
							And<orderNbr.IsEqual<origOrderNbr.FromCurrent>>>
					>()
                    .DisplayName("Reopen Quote when Order Deleted");
			}
			#endregion

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.StateIdentifierIs<status>()
					.FlowTypeIdentifierIs<behavior>()
					.WithFlows(flows =>
					{
						flows.Add<SOBehavior.sO>(flow =>
						{
							return flow
								.WithFlowStates(flowStates =>
								{
									flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
									flowStates.Add<State.hold>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
												actions.Add<SOOrderEntryExternalTax>(e => e.recalcExternalTax);
											});
									});
									flowStates.Add<State.creditHold>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.releaseFromCreditHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.putOnHold);
												actions.Add(g => g.emailSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
												actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnPaymentRequirementsViolated);
												handlers.Add(g => g.OnObtainedPaymentInPendingProcessing);
                                                handlers.Add(g => g.OnCreditLimitSatisfied);
											});
									});
									flowStates.Add<State.pendingProcessing>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.emailSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
												actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnLostLastPaymentInPendingProcessing);
											});
									});
									flowStates.Add<State.awaitingPayment>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.emailSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
												actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnPaymentRequirementsSatisfied);
                                                handlers.Add(g => g.OnObtainedPaymentInPendingProcessing);
											});
									});
									flowStates.Add<State.open>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add<SOOrderEntry.SOQuickProcess>(g => g.quickProcess, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.createShipmentIssue, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.prepareInvoice);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.completeOrder);
												actions.Add(g => g.placeOnBackOrder);
												actions.Add(g => g.createPurchaseOrder);
												actions.Add(g => g.createTransferOrder);
												actions.Add(g => g.emailSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
												actions.Add<SOOrderEntryExternalTax>(e => e.recalcExternalTax);
												actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
												actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnShipmentLinked);
												handlers.Add(g => g.OnPaymentRequirementsViolated);
                                                handlers.Add(g => g.OnObtainedPaymentInPendingProcessing);
												handlers.Add(g => g.OnCreditLimitViolated);
												handlers.Add(g => g.OnShipmentCreationFailed);
											});
									});
									flowStates.Add<State.shipping>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.emailSalesOrder);
												actions.Add(g => g.createPurchaseOrder);
												actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
												actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnShipmentUnlinked);
												handlers.Add(g => g.OnShipmentConfirmed);
											})
											.WithFieldStates(DisableWholeScreen);
									});
									flowStates.Add<State.backOrder>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.openOrder, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.createShipmentIssue, a => a.IsDuplicatedInToolbar());
												actions.Add<SOOrderEntry.SOQuickProcess>(g => g.quickProcess);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.completeOrder);
												actions.Add(g => g.putOnHold);
												actions.Add(g => g.prepareInvoice);
												actions.Add(g => g.emailSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.createPurchaseOrder);
												actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
												actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnShipmentLinked);
												handlers.Add(g => g.OnShipmentCorrected);
												handlers.Add(g => g.OnPaymentRequirementsViolated);
                                                handlers.Add(g => g.OnObtainedPaymentInPendingProcessing);
												handlers.Add(g => g.OnCreditLimitViolated);
											});
									});
									flowStates.Add<State.completed>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.prepareInvoice, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.emailSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.reopenOrder);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnShipmentCorrected);
											})
											.WithFieldStates(DisableWholeScreen);
									});
									flowStates.Add<State.cancelled>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.copyOrder, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.reopenOrder, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.validateAddresses);
											})
											.WithFieldStates(DisableWholeScreen);
									});
								})
								.WithTransitions(transitions =>
								{
									transitions.AddGroupFrom(State.Initial, ts =>
									{
										ts.Add(t => t
											.To<State.hold>() // SO New Hold
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.HasPaymentsInPendingProcessing)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsPaymentRequirementsViolated)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.creditHold>() // SO New Credit Hold
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsOnCreditHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<hold>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.completed>() // SO New Completed
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsCompleted)
											.WithFieldAssignments(fas =>
											{
												fas.Add<completed>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t
											.To<State.open>() // SO New Open
											.IsTriggeredOn(g => g.initializeState)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
									});
									transitions.AddGroupFrom<State.hold>(ts =>
									{
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.releaseFromHold)
											.When(conditions.HasPaymentsInPendingProcessing));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.releaseFromHold)
											.When(conditions.IsPaymentRequirementsViolated));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.releaseFromHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.creditHold>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.OnObtainedPaymentInPendingProcessing)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.OnPaymentRequirementsViolated)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.releaseFromCreditHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.OnCreditLimitSatisfied)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.pendingProcessing>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.OnLostLastPaymentInPendingProcessing)
											.When(conditions.IsPaymentRequirementsViolated));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.OnLostLastPaymentInPendingProcessing)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.awaitingPayment>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold));
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.OnObtainedPaymentInPendingProcessing));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.OnPaymentRequirementsSatisfied)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.open>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
										ts.Add(t => t.To<State.shipping>().IsTriggeredOn(g => g.OnShipmentLinked));
										ts.Add(t => t
											.To<State.backOrder>()
											.IsTriggeredOn(g => g.placeOnBackOrder)
											.WithFieldAssignments(fas =>
											{
												fas.Add<backOrdered>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t
											.To<State.backOrder>()
											.IsTriggeredOn(g => g.OnShipmentCreationFailed)
											.WithFieldAssignments(fas =>
											{
												fas.Add<backOrdered>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.OnObtainedPaymentInPendingProcessing)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.OnPaymentRequirementsViolated)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.creditHold>()
											.IsTriggeredOn(g => g.OnCreditLimitViolated)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(true));
												fas.Add<hold>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
									});
									transitions.AddGroupFrom<State.shipping>(ts =>
									{
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.OnShipmentUnlinked)
											.When(conditions.HasPaymentsInPendingProcessing)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(false))));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.OnShipmentUnlinked)
											.When(conditions.IsPaymentRequirementsViolated)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(false))));
										ts.Add(t => t
											.To<State.open>() // SO Shipping Open
											.IsTriggeredOn(g => g.OnShipmentUnlinked)
											.When(conditions.IsShippable)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true))));
										ts.Add(t => t
											.To<State.completed>() // SO Shipping Completed / SO New Completed
											.IsTriggeredOn(g => g.OnShipmentConfirmed)
											.When(conditions.IsShippingCompleted)
											.WithFieldAssignments(fas => fas.Add<completed>(e => e.SetFromValue(true))));
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.OnShipmentConfirmed)
											.When(conditions.HasPaymentsInPendingProcessing)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(false))));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.OnShipmentConfirmed)
											.When(conditions.IsPaymentRequirementsViolated)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(false))));
										ts.Add(t => t
											.To<State.backOrder>() // SO Shipping BackOrder
											.IsTriggeredOn(g => g.OnShipmentConfirmed)
											.When(conditions.IsShippable)
											.WithFieldAssignments(fas => fas.Add<backOrdered>(e => e.SetFromValue(true))));
									});
									transitions.AddGroupFrom<State.backOrder>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<backOrdered>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.OnObtainedPaymentInPendingProcessing)
											.WithFieldAssignments(fas =>
											{
												fas.Add<backOrdered>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.OnPaymentRequirementsViolated)
											.WithFieldAssignments(fas =>
											{
												fas.Add<backOrdered>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.creditHold>()
											.IsTriggeredOn(g => g.OnCreditLimitViolated)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(true));
												fas.Add<backOrdered>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.openOrder)
											.WithFieldAssignments(fas =>
											{
												fas.Add<backOrdered>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.shipping>().IsTriggeredOn(g => g.OnShipmentLinked));
										ts.Add(t => t.To<State.shipping>().IsTriggeredOn(g => g.OnShipmentCorrected));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.completed>(ts =>
									{
										ts.Add(t => t.To<State.shipping>().IsTriggeredOn(g => g.OnShipmentCorrected));
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.reopenOrder)
											.When(conditions.HasPaymentsInPendingProcessing)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(false))));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.reopenOrder)
											.When(conditions.IsPaymentRequirementsViolated)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(false))));
										ts.Add(t => t
											.To<State.backOrder>()
											.IsTriggeredOn(g => g.reopenOrder)
											.WithFieldAssignments(fas => fas.Add<backOrdered>(v => v.SetFromValue(false))));
									});
									transitions.AddGroupFrom<State.cancelled>(ts =>
									{
										ts.Add(t => t
											.To<State.pendingProcessing>()
											.IsTriggeredOn(g => g.reopenOrder)
											.When(conditions.HasPaymentsInPendingProcessing)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(false))));
										ts.Add(t => t
											.To<State.awaitingPayment>()
											.IsTriggeredOn(g => g.reopenOrder)
											.When(conditions.IsPaymentRequirementsViolated)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(false))));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.reopenOrder)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(true))));
									});
								});
						});
						flows.Add<SOBehavior.qT>(flow =>
						{
							return flow
								.WithFlowStates(flowStates =>
								{
									flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
									flowStates.Add<State.hold>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.openOrder);
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
												actions.Add(g => g.emailSalesOrder);
											});
									});
									flowStates.Add<State.open>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.copyOrder, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
												actions.Add(g => g.emailSalesOrder);
											});
									});
									flowStates.Add<State.completed>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.openOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.emailSalesOrder);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnOrderDeleted_ReopenQuote);
											})
											.WithFieldStates(DisableWholeScreen);
									});
									flowStates.Add<State.cancelled>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.copyOrder, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.reopenOrder, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.validateAddresses);
                                            })
											.WithFieldStates(DisableWholeScreen);
									});
								})
								.WithTransitions(transitions =>
								{
									transitions.AddGroupFrom(State.Initial, ts =>
									{
										ts.Add(t => t // QT New Hold
											.To<State.hold>()
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsOnHold));
										ts.Add(t => t // QT New Open
											.To<State.open>()
											.IsTriggeredOn(g => g.initializeState));
									});
									transitions.AddGroupFrom<State.hold>(ts =>
									{
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.releaseFromHold)
											.WithFieldAssignments(fas => fas.Add<hold>(e => e.SetFromValue(false))));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.openOrder)
											.WithFieldAssignments(fas => fas.Add<hold>(e => e.SetFromValue(false))));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.open>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold)
											.WithFieldAssignments(fas => fas.Add<hold>(e => e.SetFromValue(true))));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.cancelled>(ts =>
									{
                                        ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.reopenOrder)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(true))));
									});
									transitions.AddGroupFrom<State.completed>(ts =>
									{
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.openOrder)
											.WithFieldAssignments(fas => fas.Add<completed>(e => e.SetFromValue(false))));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.OnOrderDeleted_ReopenQuote)
											.WithFieldAssignments(fas => fas.Add<completed>(e => e.SetFromValue(false))));
									});
								});
						});
						flows.Add<SOBehavior.rM>(flow =>
						{
							return flow
								.WithFlowStates(flowStates =>
								{
									flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
									flowStates.Add<State.hold>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
											});
									});
									flowStates.Add<State.open>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.createShipmentReceipt, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.createShipmentIssue, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.putOnHold);
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.completeOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
												actions.Add(g => g.prepareInvoice);
												actions.Add(g => g.createPurchaseOrder);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnShipmentConfirmed);
											});
									});
									flowStates.Add<State.completed>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.prepareInvoice, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.reopenOrder);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnShipmentCorrected);
											})
											.WithFieldStates(DisableWholeScreen);
									});
									flowStates.Add<State.cancelled>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.reopenOrder);
                                            })
											.WithFieldStates(DisableWholeScreen);
									});
								})
								.WithTransitions(transitions =>
								{
									transitions.AddGroupFrom(State.Initial, ts =>
									{
										ts.Add(t => t
											.To<State.hold>() // RM New Hold
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.open>() // RM New Open
											.IsTriggeredOn(g => g.initializeState)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
									});
									transitions.AddGroupFrom<State.hold>(ts =>
									{
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.releaseFromHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.open>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.completed>() // RM Shipping Completed / RM New Completed
											.IsTriggeredOn(g => g.OnShipmentConfirmed)
											.When(conditions.IsShippingCompleted)
											.WithFieldAssignments(fas =>
											{
												fas.Add<completed>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.cancelled>(ts =>
									{
                                        ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.reopenOrder)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(true))));
									});
									transitions.AddGroupFrom<State.completed>(ts =>
									{
										ts.Add(t => t.To<State.open>().IsTriggeredOn(g => g.OnShipmentCorrected));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.reopenOrder)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(true))));
									});
								});
						});
						flows.Add<SOBehavior.iN>(flow =>
						{
							return flow
								.WithFlowStates(flowStates =>
								{
									flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
									flowStates.Add<State.hold>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
											});
									});
									flowStates.Add<State.creditHold>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.releaseFromCreditHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.putOnHold);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnCreditLimitSatisfied);
											});
									});
									flowStates.Add<State.open>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
												actions.Add<SOOrderEntry.SOQuickProcess>(g => g.quickProcess, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.prepareInvoice, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
												actions.Add(g => g.emailSalesOrder);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnInvoiceLinked);
												handlers.Add(g => g.OnCreditLimitViolated);
											});
									});
									flowStates.Add<State.invoiced>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.emailSalesOrder);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnInvoiceReleased);
												handlers.Add(g => g.OnInvoiceUnlinked);
											})
											.WithFieldStates(DisableWholeScreen);
									});
									flowStates.Add<State.completed>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.emailSalesOrder);
											})
											.WithFieldStates(DisableWholeScreen);
									});
									flowStates.Add<State.cancelled>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.reopenOrder, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.copyOrder, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.validateAddresses);
                                            })
											.WithFieldStates(DisableWholeScreen);
									});
								})
								.WithTransitions(transitions =>
								{
									transitions.AddGroupFrom(State.Initial, ts =>
									{
										ts.Add(t => t
											.To<State.hold>() // IN New Hold
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t // IN New Credit Hold
											.To<State.creditHold>()
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsOnCreditHold));
										ts.Add(t => t
											.To<State.open>() // IN New Open
											.IsTriggeredOn(g => g.initializeState)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
									});
									transitions.AddGroupFrom<State.hold>(ts =>
									{
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.releaseFromHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.creditHold>(ts =>
									{
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.releaseFromCreditHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.OnCreditLimitSatisfied)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.open>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t.To<State.invoiced>().IsTriggeredOn(g => g.OnInvoiceLinked));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
										ts.Add(t => t
											.To<State.creditHold>()
											.IsTriggeredOn(g => g.OnCreditLimitViolated)
											.WithFieldAssignments(fas =>
											{
												fas.Add<creditHold>(e => e.SetFromValue(true));
												fas.Add<hold>(e => e.SetFromValue(false));
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
									});
									transitions.AddGroupFrom<State.invoiced>(ts =>
									{
										ts.Add(t => t
											.To<State.completed>() // IN New Completed
											.IsTriggeredOn(g => g.OnInvoiceReleased)
											.When(conditions.HasAllBillsReleased)
											.WithFieldAssignments(fas =>
											{
												fas.Add<completed>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.OnInvoiceUnlinked)
											.When(conditions.IsUnbilled)); // IN New Unbilled
									});
									transitions.AddGroupFrom<State.cancelled>(ts =>
									{
                                        ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.reopenOrder)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(true))));
									});
								});
						});
						flows.Add<SOBehavior.cM>(flow =>
						{
							return flow
								.WithFlowStates(flowStates =>
								{
									flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
									flowStates.Add<State.hold>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
											});
									});
									flowStates.Add<State.open>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.prepareInvoice, a => a.IsDuplicatedInToolbar());
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.cancelOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.recalculateDiscountsAction);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnInvoiceLinked);
											});
									});
									flowStates.Add<State.invoiced>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
											})
											.WithEventHandlers(handlers =>
											{
												handlers.Add(g => g.OnInvoiceReleased);
												handlers.Add(g => g.OnInvoiceUnlinked);
											})
											.WithFieldStates(DisableWholeScreen);
									});
									flowStates.Add<State.completed>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
											})
											.WithFieldStates(DisableWholeScreen);
									});
									flowStates.Add<State.cancelled>(flowState =>
									{
										return flowState
											.WithActions(actions =>
											{
												actions.Add(g => g.printSalesOrder);
												actions.Add(g => g.copyOrder);
												actions.Add(g => g.validateAddresses);
												actions.Add(g => g.reopenOrder);
                                            })
											.WithFieldStates(DisableWholeScreen);
									});
								})
								.WithTransitions(transitions =>
								{
									transitions.AddGroupFrom(State.Initial, ts =>
									{
										ts.Add(t => t
											.To<State.hold>() // CM New Hold
											.IsTriggeredOn(g => g.initializeState)
											.When(conditions.IsOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t
											.To<State.open>() // CM New Open
											.IsTriggeredOn(g => g.initializeState)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
									});
									transitions.AddGroupFrom<State.hold>(ts =>
									{
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.releaseFromHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.open>(ts =>
									{
										ts.Add(t => t
											.To<State.hold>()
											.IsTriggeredOn(g => g.putOnHold)
											.WithFieldAssignments(fas =>
											{
												fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
											}));
										ts.Add(t => t.To<State.invoiced>().IsTriggeredOn(g => g.OnInvoiceLinked));
										ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
									});
									transitions.AddGroupFrom<State.invoiced>(ts =>
									{
										ts.Add(t => t
											.To<State.completed>() // CM New Completed
											.IsTriggeredOn(g => g.OnInvoiceReleased)
											.When(conditions.HasAllBillsReleased)
											.WithFieldAssignments(fas =>
											{
												fas.Add<completed>(e => e.SetFromValue(true));
											}));
										ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.OnInvoiceUnlinked)
											.When(conditions.IsUnbilled)); // CM New Unbilled
									});
									transitions.AddGroupFrom<State.cancelled>(ts =>
									{
                                        ts.Add(t => t
											.To<State.open>()
											.IsTriggeredOn(g => g.reopenOrder)
											.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(v => v.SetFromValue(true))));
									});
								});
						});
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.initializeState);
						actions.Add<SOOrderEntry.SOQuickProcess>(g => g.quickProcess, c => c
							.InFolder(FolderType.ActionsFolder));

						actions.Add(g => g.createShipmentIssue, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOCreateShipment>() // +RM Open, SO (Back-Order, Open)
							.InBatchMode());
						actions.Add(g => g.createShipmentReceipt, c => c
							.InFolder(FolderType.ActionsFolder));

						actions.Add(g => g.openOrder, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOCreateShipment>()); // +SO Back-Order
						actions.Add(g => g.reopenOrder, c => c
							.InFolder(FolderType.ActionsFolder)
							.WithFieldAssignments(fas =>
							{
								fas.Add<cancelled>(e => e.SetFromValue(false));
								fas.Add<completed>(e => e.SetFromValue(false));
							}));

						actions.Add(g => g.copyOrder, c => c
							.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.emailSalesOrder, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOOrderProcess>()); // +IN (Completed, Invoiced, Open), +QT (Completed, Hold, Open), +SO (Back-Order, Completed, Credit-Hold, Open, Shipping)

						actions.Add(g => g.releaseFromCreditHold, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOCreateShipment>() // +IN Credit-Hold, +SO Credit-Hold
							.WithFieldAssignments(fas =>
							{
								fas.Add<approvedCredit>(e => e.SetFromValue(true));
								fas.Add<approvedCreditAmt>(e => e.SetFromField<orderTotal>());
							}));

						actions.Add(g => g.prepareInvoice, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOCreateShipment>() // +CM Open, +IN Open, +RM (Completed, Open), +SO (Back-Order, Completed, Open)
							.InBatchMode());

						actions.Add(g => g.createPurchaseOrder, c => c
							.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.createTransferOrder, c => c
							.InFolder(FolderType.ActionsFolder));

						actions.Add(g => g.completeOrder, c => c
							.InFolder(FolderType.ActionsFolder)
							.IsDisabledWhen(conditions.CanNotBeCompleted)
							.WithFieldAssignments(fas => fas.Add<forceCompleteOrder>(f => f.SetFromValue(true))));
						actions.Add(g => g.cancelOrder, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOCreateShipment>() // +QT (Hold, Open), +RM (Hold, Open), +SO (Hold, Open)
							.WithFieldAssignments(fas =>
							{
								fas.Add<cancelled>(e => e.SetFromValue(true));
								fas.Add<hold>(e => e.SetFromValue(false));
								fas.Add<creditHold>(e => e.SetFromValue(false));
								fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
							}));
						actions.Add(g => g.placeOnBackOrder, c => c
							.InFolder(FolderType.ActionsFolder));

						actions.Add(g => g.putOnHold, c => c
							.InFolder(FolderType.ActionsFolder)
							.DoesNotPersist()
							.WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.releaseFromHold, c => c
							.InFolder(FolderType.ActionsFolder)
							.DoesNotPersist()
							.WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(false))));

						actions.Add(g => g.validateAddresses, c => c
							.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.recalculateDiscountsAction, c => c
							.InFolder(FolderType.ActionsFolder));
						actions.Add<SOOrderEntryExternalTax>(e => e.recalcExternalTax, c => c
							.InFolder(FolderType.ActionsFolder));

						actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment, c => c
							.InFolder(FolderType.ActionsFolder)
							.IsHiddenAlways() // only for mass processing
							.MassProcessingScreen<SOCreateShipment>());
						actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment, c => c
							.InFolder(FolderType.ActionsFolder)
							.IsHiddenAlways() // only for mass processing
							.MassProcessingScreen<SOCreateShipment>());

						actions.Add(g => g.printSalesOrder, c => c
							.InFolder(FolderType.ReportsFolder)
							.MassProcessingScreen<SOOrderProcess>()
							// +SO (Cancelled, Completed, Hold, Open, Credit-Hold),
							// +QT (Hold, Open),
							// +RM (Cancelled, Completed, Hold, Open),
							// +IN (Cancelled, Completed, Hold, Open, Invoiced, Credit-Hold),
							// +CM (Cancelled, Completed, Hold, Open, Invoiced)
							.InBatchMode());
					})
					.WithHandlers(handlers =>
					{
						handlers.Add(OnOrderDeleted_ReopenQuote);
						handlers.Add(OnShipmentCreationFailed);

                        handlers.Add(OnObtainedPaymentInPendingProcessing);
                        handlers.Add(OnLostLastPaymentInPendingProcessing);
                        
                        handlers.Add(OnPaymentRequirementsSatisfied);
						handlers.Add(OnPaymentRequirementsViolated);

						handlers.Add(OnCreditLimitSatisfied);
						handlers.Add(OnCreditLimitViolated);

						handlers.Add(OnShipmentLinked);
						handlers.Add(OnShipmentUnlinked);

						handlers.Add(OnShipmentConfirmed);
						handlers.Add(OnShipmentCorrected);

						handlers.Add(InvoiceLinked);
						handlers.Add(OnInvoiceUnlinked);

						handlers.Add(OnInvoiceReleased);
						//handlers.Add(g => g.OnInvoiceCancelled);
					});
			});
		}
	}
}