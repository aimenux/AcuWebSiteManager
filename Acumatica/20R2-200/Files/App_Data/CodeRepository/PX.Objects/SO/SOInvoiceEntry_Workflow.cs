using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt;

namespace PX.Objects.SO
{
	using State = ARDocStatus;
	using static ARInvoice;
	using static BoundedTo<SOInvoiceEntry, ARInvoice>;

	public class SOInvoiceEntry_Workflow : PXGraphExtension<SOInvoiceEntry>
	{
		public override void Configure(PXScreenConfiguration config) => Configure(config.GetScreenConfigurationContext<SOInvoiceEntry, ARInvoice>());

		protected virtual void Configure(WorkflowContext<SOInvoiceEntry, ARInvoice> context)
		{
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsOnHold
					= Bql<released.IsEqual<False>.And<hold.IsEqual<True>>>(),

				IsOnCreditHold
					= Bql<released.IsEqual<False>.And<creditHold.IsEqual<True>>>(),

				IsReleased
					= Bql<released.IsEqual<True>.And<openDoc.IsEqual<True>>>(),

				IsClosed
					= Bql<released.IsEqual<True>.And<openDoc.IsEqual<False>>>(),

				IsBalanced
					= Bql<hold.IsEqual<False>.And<creditHold.IsEqual<False>>.And<released.IsEqual<False>>.And<printInvoice.IsEqual<False>>>(),

				IsPendingPrint
					= Bql<hold.IsEqual<False>.And<creditHold.IsEqual<False>>.And<released.IsEqual<False>>.And<printInvoice.IsEqual<True>>>(),
			}.AutoNameConditions();

			const string initialState = "_";

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.StateIdentifierIs<status>()
					.AddDefaultFlow(flow =>
					{
						return flow
							.WithFlowStates(flowStates =>
							{
								flowStates.Add(initialState, flowState => flowState.IsInitial(g => g.initializeState));
								flowStates.Add<State.hold>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.releaseFromHold, act => act.IsDuplicatedInToolbar());
											actions.Add(g => g.putOnCreditHold);
											actions.Add(g => g.printInvoice);
											actions.Add(g => g.validateAddresses);
											actions.Add(g => g.recalculateDiscountsAction);
										});
								});
								flowStates.Add<State.cCHold>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.putOnCreditHold);
											actions.Add(g => g.printInvoice);
											actions.Add(g => g.validateAddresses);
											actions.Add(g => g.recalculateDiscountsAction);
											actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
										});
								});
								flowStates.Add<State.creditHold>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.releaseFromCreditHold, act => act.IsDuplicatedInToolbar());
											actions.Add(g => g.putOnHold, act => act.IsDuplicatedInToolbar());
											actions.Add(g => g.validateAddresses);
											actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
										});
								});
								flowStates.Add<State.pendingPrint>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.printInvoice, act => act.IsDuplicatedInToolbar());
											actions.Add(g => g.emailInvoice);
											actions.Add(g => g.putOnHold);
											actions.Add(g => g.putOnCreditHold);
											actions.Add(g => g.validateAddresses);
											actions.Add(g => g.recalculateDiscountsAction);
										});
								});
								flowStates.Add<State.pendingEmail>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.emailInvoice, act => act.IsDuplicatedInToolbar());
											actions.Add(g => g.printInvoice);
											actions.Add(g => g.putOnHold);
											actions.Add(g => g.putOnCreditHold);
											actions.Add(g => g.validateAddresses);
											actions.Add(g => g.recalculateDiscountsAction);
										});
								});
								flowStates.Add<State.balanced>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.release, act => act.IsDuplicatedInToolbar());
											actions.Add(g => g.putOnHold, act => act.IsDuplicatedInToolbar());
											actions.Add(g => g.putOnCreditHold);
											actions.Add(g => g.printInvoice);
											actions.Add(g => g.emailInvoice);
											actions.Add(g => g.arEdit);
											actions.Add(g => g.validateAddresses);
											actions.Add(g => g.recalculateDiscountsAction);
											actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
										});
								});
								flowStates.Add<State.open>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.post);
											actions.Add(g => g.writeOff);
											actions.Add(g => g.payInvoice);
											actions.Add(g => g.printInvoice);
											actions.Add(g => g.validateAddresses);
											actions.Add(g => g.reclassifyBatch);
											actions.Add<Correction>(g => g.cancelInvoice);
											actions.Add<Correction>(g => g.correctInvoice);
										});
								});
								flowStates.Add<State.closed>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.post);
											actions.Add(g => g.printInvoice);
											actions.Add(g => g.validateAddresses);
											actions.Add(g => g.reclassifyBatch);
											actions.Add<Correction>(g => g.cancelInvoice);
											actions.Add<Correction>(g => g.correctInvoice);
										})
										.WithFieldStates(states =>
										{
											states.AddTable<ARInvoice>(state => state.IsDisabled());
										});
								});
								flowStates.Add<State.canceled>(flowState =>
								{
									return flowState
										.WithActions(actions =>
										{
											actions.Add(g => g.printInvoice);
										})
										.WithFieldStates(states =>
										{
											states.AddTable<ARInvoice>(state => state.IsDisabled());
										});
								});
							})
							.WithTransitions(transitions =>
							{
								transitions.AddGroupFrom(initialState, ts =>
								{
									ts.Add(t => t.To<State.hold>().IsTriggeredOn(g => g.initializeState).When(conditions.IsOnHold));
									ts.Add(t => t.To<State.creditHold>().IsTriggeredOn(g => g.initializeState).When(conditions.IsOnCreditHold));
									ts.Add(t => t.To<State.pendingPrint>().IsTriggeredOn(g => g.initializeState).When(conditions.IsPendingPrint));
									ts.Add(t => t.To<State.balanced>().IsTriggeredOn(g => g.initializeState).When(conditions.IsBalanced));
									ts.Add(t => t.To<State.open>().IsTriggeredOn(g => g.initializeState).When(conditions.IsReleased));
									ts.Add(t => t.To<State.closed>().IsTriggeredOn(g => g.initializeState).When(conditions.IsClosed));
								});
								transitions.AddGroupFrom<State.hold>(ts =>
								{
									ts.Add(t => t.To<State.creditHold>().IsTriggeredOn(g => g.putOnCreditHold));
									ts.Add(t => t.To<State.pendingPrint>().IsTriggeredOn(g => g.releaseFromHold).When(conditions.IsPendingPrint));
									ts.Add(t => t.To<State.balanced>().IsTriggeredOn(g => g.releaseFromHold).When(conditions.IsBalanced));
								});
								transitions.AddGroupFrom<State.creditHold>(ts =>
								{
									ts.Add(t => t.To<State.hold>().IsTriggeredOn(g => g.putOnHold));
									ts.Add(t => t.To<State.pendingPrint>().IsTriggeredOn(g => g.releaseFromCreditHold).When(conditions.IsPendingPrint));
									ts.Add(t => t.To<State.balanced>().IsTriggeredOn(g => g.releaseFromCreditHold).When(conditions.IsBalanced));
								});
								transitions.AddGroupFrom<State.pendingPrint>(ts =>
								{
									ts.Add(t => t.To<State.hold>().IsTriggeredOn(g => g.putOnHold));
									ts.Add(t => t.To<State.creditHold>().IsTriggeredOn(g => g.putOnCreditHold));
									ts.Add(t => t.To<State.balanced>().IsTriggeredOn(g => g.printInvoice));
								});
								transitions.AddGroupFrom<State.balanced>(ts =>
								{
									ts.Add(t => t.To<State.hold>().IsTriggeredOn(g => g.putOnHold));
									ts.Add(t => t.To<State.creditHold>().IsTriggeredOn(g => g.putOnCreditHold));
									ts.Add(t => t.To<State.open>().IsTriggeredOn(g => g.release).When(conditions.IsReleased));
									ts.Add(t => t.To<State.closed>().IsTriggeredOn(g => g.release).When(conditions.IsClosed));
								});
								transitions.AddGroupFrom<State.open>(ts =>
								{
									//ts.Add(t => t.To<State.closed>().IsTriggeredOn(g => g.OnApplicationReleased).When(conditions.IsClosed));
								});
								transitions.AddGroupFrom<State.closed>(ts =>
								{
									// terminal status
								});
							});
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.initializeState, a => a.IsHiddenAlways());
						actions.Add(g => g.release, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOReleaseInvoice>()
							.InBatchMode());
						actions.Add(g => g.post, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOReleaseInvoice>()
							.InBatchMode());

						actions.Add(g => g.putOnHold, c => c
							.InFolder(FolderType.ActionsFolder)
							.WithFieldAssignments(fass => fass.Add<hold>(v => v.SetFromValue(true))));
						actions.Add(g => g.releaseFromHold, c => c
							.InFolder(FolderType.ActionsFolder)
							.WithFieldAssignments(fass => fass.Add<hold>(v => v.SetFromValue(false))));

						actions.Add(g => g.putOnCreditHold, c => c
							.InFolder(FolderType.ActionsFolder)
							.WithFieldAssignments(fass =>
							{
								fass.Add<creditHold>(v => v.SetFromValue(true));
								fass.Add<approvedCredit>(v => v.SetFromValue(false));
								fass.Add<approvedCreditAmt>(v => v.SetFromValue(0));
								fass.Add<approvedCaptureFailed>(v => v.SetFromValue(false));
								fass.Add<approvedPrepaymentRequired>(v => v.SetFromValue(false));
							}));
						actions.Add(g => g.releaseFromCreditHold, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOReleaseInvoice>()
							.WithFieldAssignments(fass =>
							{
								fass.Add<creditHold>(v => v.SetFromValue(false));
							}));

						actions.Add(g => g.emailInvoice, c => c
							.InFolder(FolderType.ActionsFolder)
							.MassProcessingScreen<SOReleaseInvoice>()
							.WithFieldAssignments(fass => fass.Add<emailed>(v => v.SetFromValue(true))));

						actions.Add(g => g.recalculateDiscountsAction, c => c
							.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.writeOff, c => c
							.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.reclassifyBatch, c => c
							.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.payInvoice, c => c
							.InFolder(FolderType.ActionsFolder));

						actions.Add<Correction>(g => g.cancelInvoice, c => c
							.InFolder(FolderType.ActionsFolder));
						actions.Add<Correction>(g => g.correctInvoice, c => c
							.InFolder(FolderType.ActionsFolder));

						actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment, c => c
							.InFolder(FolderType.ActionsFolder)
							.IsHiddenAlways() // only for mass processing
							.MassProcessingScreen<SOReleaseInvoice>());

						actions.Add(g => g.validateAddresses, c => c
							.InFolder(FolderType.ActionsFolder));

						actions.Add(g => g.arEdit, c => c
							.InFolder(FolderType.ReportsFolder));
						actions.Add(g => g.printInvoice, c => c
							.InFolder(FolderType.ReportsFolder)
							.MassProcessingScreen<SOReleaseInvoice>()
							.InBatchMode()
							.WithFieldAssignments(fass => fass.Add<printed>(v => v.SetFromValue(true))));
					})
					.WithHandlers(handlers =>
					{
					});
			});
		}
	}
}