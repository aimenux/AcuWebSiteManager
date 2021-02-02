using PX.Data;
using PX.Data.WorkflowAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Workflows
{
	using static PX.Data.WorkflowAPI.BoundedTo<CRCaseMaint, CRCase>;
	/// <summary>
	/// <summary>
	/// Extensions that used to configure Workflow for <see cref="CRCaseMaint"/> and <see cref="CRCase"/>.
	/// Use Extensions Chaining for this extension if you want customize workflow with code for this graph of DAC.
	/// </summary>
	public class CaseWorkflow : PX.Data.PXGraphExtension<CRCaseMaint>
	{

		public PXAction<CRCase> openCaseFromProcessing;
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Open")]
		[PXButton]
		protected virtual void OpenCaseFromProcessing() { }

		public PXAction<CRCase> openCaseFromPortal;
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Open")]
		[PXButton]
		protected virtual void OpenCaseFromPortal() { }

		public PXAction<CRCase> closeCaseFromPortal;
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Close")]
		[PXButton]
		protected virtual void CloseCaseFromPortal() { }

		#region Consts

		/// <summary>
		/// Statuses for <see cref="CROpportunity.status"/> used by default in system workflow.
		/// Values could be changed and extended by workflow.
		/// </summary>
		public static class States
		{
			public const string New = "N";
			public const string Open = "O";
			public const string Closed = "C";
			public const string Released = "R";
			public const string PendingCustomer = "P";

			internal class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new[]
						{
							New,
							Open,
							Closed,
							Released,
							PendingCustomer,
						},
						new[]
						{
							"New",
							"Open",
							"Closed",
							"Released",
							"Pending Customer",
						}
					)
				{ }
			}
		}

		private const string
				_fieldReason = "Reason",
				_fieldOwner = "Owner",

				_formOpen = "FormOpen",
				_formPending = "FormPendingCustomer",
				_formClose = "FormClose",

				_actionOpen = "Open",
				_actionPending = "PendingCustomer",
				_actionClose = "Close",

				_reasonRejected = "RJ",
				_reasonResolved = "RD",
				_reasonMoreInfoRequested = "MI",
				_reasonInProcess = "IP",
				_reasonInternal = "IN",
				_reasonInEscalation = "ES",
				_reasonDuplicate = "DP",
				_reasonWaitingConfirmation = "CR",
				_reasonCustomerPostpone = "CP",
				_reasonCanceled = "CL",
				_reasonPendingClosure = "CC",
				_reasonAbandoned = "CA",
				_reasonUnassigned = "AS",
				_reasonUpdated = "AD",
				_reasonClosedOnPortal = "PC",
				_reasonOpenedOnPortal = "PO";

		#endregion

		public override void Configure(PXScreenConfiguration config)
		{
			var context = config.GetScreenConfigurationContext<CRCaseMaint, CRCase>();

			#region forms

			var reasons = new Dictionary<string, string[]>(5)
			{
				[States.New] = new[] { _reasonUnassigned },
				[States.Open] = new[] { _reasonInProcess, _reasonUpdated, _reasonInEscalation, _reasonPendingClosure },
				[States.PendingCustomer] = new[] { _reasonMoreInfoRequested, _reasonWaitingConfirmation, _reasonPendingClosure },
				[States.Closed] = new[] { _reasonResolved, _reasonRejected, _reasonCanceled, _reasonAbandoned, _reasonDuplicate },
				[States.Released] = new[] { _reasonResolved, _reasonCanceled },
			};

			var formOpen = context.Forms.Create(_formOpen, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, _reasonInProcess, reasons[States.Open]);
						AddOwnerFormField(fields);
					}));

			var formPending = context.Forms.Create(_formPending, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, _reasonWaitingConfirmation, reasons[States.PendingCustomer]);
					}));

			var formClose = context.Forms.Create(_formClose, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, _reasonResolved, reasons[States.Closed]);
					}));

			void AddResolutionFormField(FormField.IContainerFillerFields filler, string defaultValue, string[] values)
			{
				filler.Add(_fieldReason, field => field
					.WithSchemaOf<CRCase.resolution>()
					.IsRequired()
					.Prompt("Reason")
					.DefaultValue(defaultValue)
					.OnlyComboBoxValues(values));
			}


			void AddOwnerFormField(FormField.IContainerFillerFields filler)
			{
				filler.Add(_fieldOwner, field => field
					.WithSchemaOf<CRCase.ownerID>()
					.Prompt("Owner")
					.DefaultValueFromSchemaField());
			}

			#endregion

            var actionOpen = context.ActionDefinitions.CreateNew(_actionOpen, a => a
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
                    fields.Add<CRCase.resolution>(f => f.SetFromFormField(formOpen, _fieldReason));
                    fields.Add<CRCase.ownerID>(f => f.SetFromFormField(formOpen, _fieldOwner));
                    fields.Add<CRCase.resolutionDate>(f => f.SetFromValue(null));
                })
                .DisplayName("Open")
                .WithForm(formOpen)
                .MassProcessingScreen<UpdateCaseMassProcess>());

            var actionPending = context.ActionDefinitions.CreateNew(_actionPending, a => a
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
                    fields.Add<CRCase.resolution>(f => f.SetFromFormField(formPending, _fieldReason));
                    fields.Add<CRCase.resolutionDate>(f => f.SetFromValue(null));
                })
                .DisplayName("Pending Customer")
                .WithForm(formPending)
                .MassProcessingScreen<UpdateCaseMassProcess>());

            var actionClose = context.ActionDefinitions.CreateNew(_actionClose, a => a
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CRCase.isActive>(f => f.SetFromValue(false));
                    fields.Add<CRCase.resolution>(f => f.SetFromFormField(formClose, _fieldReason));
                    fields.Add<CRCase.resolutionDate>(f => f.SetFromNow());
                })
                .DisplayName("Close")
                .WithForm(formClose)
                .MassProcessingScreen<UpdateCaseMassProcess>());

			var gactionRelease = context.ActionDefinitions.CreateExisting(g => g.release, a => a
					.InFolder(FolderType.ActionsFolder)
					.MassProcessingScreen<CRCaseReleaseProcess>());

			var gactionAssign = context.ActionDefinitions.CreateExisting(g => g.assign, a => a.InFolder(FolderType.ActionsFolder));
			var gactionViewInvoice = context.ActionDefinitions.CreateExisting(g => g.viewInvoice, a => a.InFolder(FolderType.InquiriesFolder));

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.StateIdentifierIs<CRCase.status>()
					.AddDefaultFlow(DefaultCaseFlow)
					.WithActions(actions =>
					{
						actions.Add(actionOpen);
						actions.Add(actionPending);
						actions.Add(actionClose);
						actions.Add(gactionRelease);
						actions.Add(gactionAssign);
						actions.Add(gactionViewInvoice);
					})
					.WithForms(forms =>
					{
						forms.Add(formOpen);
						forms.Add(formPending);
						forms.Add(formClose);
					})
					.WithFieldStates(fields =>
					{
						fields.Add<CRCase.resolution>(field => field
							.SetComboValues(
								(_reasonRejected, "Rejected"),
								(_reasonResolved, "Resolved"),
								(_reasonMoreInfoRequested, "More Info Requested"),
								(_reasonInProcess, "In Process"),
								(_reasonInternal, "Internal"),
								(_reasonInEscalation, "In Escalation"),
								(_reasonDuplicate, "Duplicate"),
								(_reasonWaitingConfirmation, "Waiting Confirmation"),
								(_reasonCustomerPostpone, "Customer Postpone"),
								(_reasonCanceled, "Canceled"),
								(_reasonPendingClosure, "Pending Closure"),
								(_reasonAbandoned, "Abandoned"),
								(_reasonUnassigned, "Unassigned"),
								(_reasonUpdated, "Updated"),
								(_reasonClosedOnPortal, "Closed on Portal"),
								(_reasonOpenedOnPortal, "Opened on Portal")));
					});
			});

			Workflow.IConfigured DefaultCaseFlow(Workflow.INeedStatesFlow flow)
			{
				#region states

				var newState = context.FlowStates.Create(States.New, state => state
					.IsInitial()
					.WithFieldStates(fields =>
					{
						fields.AddField<CRCase.resolution>(field => field
							.DefaultValue(_reasonUnassigned)
							.ComboBoxValues(reasons[States.New].Union(new[] { _reasonOpenedOnPortal }).ToArray()));
						fields.AddField<CRCase.isActive>(field => field.IsDisabled());
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.takeCase);
						actions.Add(g => g.assign);
						AddOpenAction(actions, isDuplicationInToolbar: true);
						AddPendingCustomerAction(actions);
						AddCloseAction(actions);
					}));

				var openState = context.FlowStates.Create(States.Open, state => state
					.WithFieldStates(fields =>
					{
						fields.AddField<CRCase.resolution>(field =>
							field.ComboBoxValues(reasons[States.Open]));
						fields.AddField<CRCase.isActive>(field => field.IsDisabled());
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.takeCase);
						actions.Add(g => g.assign);
						AddPendingCustomerAction(actions);
						AddCloseAction(actions, isDuplicationInToolbar: true);
					}));

				var pendingState = context.FlowStates.Create(States.PendingCustomer, state => state
					.WithFieldStates(fields =>
					{
						fields.AddField<CRCase.resolution>(field => field
							.ComboBoxValues(reasons[States.PendingCustomer])
							.IsDisabled());
						fields.AddField<CRCase.isActive>(field => field.IsDisabled());
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.takeCase);
						AddOpenAction(actions);
						AddCloseAction(actions, isDuplicationInToolbar: true);
					}));

				var closedState = context.FlowStates.Create(States.Closed, state => state
					.WithFieldStates(fields =>
					{
						fields.AddField<CRCase.resolution>(field => field
							.ComboBoxValues(reasons[States.Closed].Union(new[] { _reasonClosedOnPortal }).ToArray())
							.IsDisabled());

						DisableFieldsForFinalStates(fields);

						fields.AddField<CRCase.isBillable>();
						fields.AddField<CRCase.manualBillableTimes>();
						fields.AddField<CRCase.timeBillable>();
						fields.AddField<CRCase.overtimeBillable>();
					})
					.WithActions(actions =>
					{
						AddOpenAction(actions);
						AddPendingCustomerAction(actions);
						actions.Add(g => g.release, action => action.IsDuplicatedInToolbar());
					}));

				var releasedState = context.FlowStates.Create(States.Released, state => state
					.WithFieldStates(fields =>
					{
						fields.AddField<CRCase.resolution>(field => field
							.ComboBoxValues(reasons[States.Released])
							.IsDisabled());

						DisableFieldsForFinalStates(fields);
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.viewInvoice);
					}));


				#endregion

				return flow
					.WithFlowStates(states =>
					{
						states.Add(newState);
						states.Add(openState);
						states.Add(pendingState);
						states.Add(closedState);
						states.Add(releasedState);
					})
					.WithTransitions(transitions =>
					{
						#region new

						transitions.Add(transition => transition
							.From(newState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						transitions.Add(transition => transition
							.From(newState)
							.To(pendingState)
							.IsTriggeredOn(actionPending));

						transitions.Add(transition => transition
							.From(newState)
							.To(closedState)
							.IsTriggeredOn(actionClose));

						transitions.Add(transition => transition
							.From(newState)
							.To(openState)
							.IsTriggeredOn<CaseWorkflow>(e => e.openCaseFromPortal)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonOpenedOnPortal));
							}));

						transitions.Add(transition => transition
							.From(newState)
							.To(closedState)
							.IsTriggeredOn<CaseWorkflow>(e => e.closeCaseFromPortal)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(false));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonClosedOnPortal));
							}));

						#endregion
						#region open

						transitions.Add(transition => transition
							.From(openState)
							.To(pendingState)
							.IsTriggeredOn(actionPending));

						transitions.Add(transition => transition
							.From(openState)
							.To(closedState)
							.IsTriggeredOn(actionClose));

						transitions.Add(transition => transition
							.From(openState)
							.To(openState)
							.IsTriggeredOn<CaseWorkflow>(e => e.openCaseFromProcessing)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonUpdated));
							}));

						transitions.Add(transition => transition
							.From(openState)
							.To(openState)
							.IsTriggeredOn<CaseWorkflow>(e => e.openCaseFromPortal)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonOpenedOnPortal));
							}));

						transitions.Add(transition => transition
							.From(openState)
							.To(closedState)
							.IsTriggeredOn<CaseWorkflow>(e => e.closeCaseFromPortal)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(false));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonClosedOnPortal));
							}));

						#endregion
						#region pending

						transitions.Add(transition => transition
							.From(pendingState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						transitions.Add(transition => transition
							.From(pendingState)
							.To(closedState)
							.IsTriggeredOn(actionClose));

						transitions.Add(transition => transition
							.From(pendingState)
							.To(openState)
							.IsTriggeredOn<CaseWorkflow>(e => e.openCaseFromProcessing)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonUpdated));
							}));

						transitions.Add(transition => transition
							.From(pendingState)
							.To(openState)
							.IsTriggeredOn<CaseWorkflow>(e => e.openCaseFromPortal)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonOpenedOnPortal));
							}));

						transitions.Add(transition => transition
							.From(pendingState)
							.To(closedState)
							.IsTriggeredOn<CaseWorkflow>(e => e.closeCaseFromPortal)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(false));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonClosedOnPortal));
							}));

						#endregion
						#region closed

						transitions.Add(transition => transition
							.From(closedState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						transitions.Add(transition => transition
							.From(closedState)
							.To(pendingState)
							.IsTriggeredOn(actionPending));

						transitions.Add(transition => transition
							.From(closedState)
							.To(releasedState)
							.IsTriggeredOn(g => g.release)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(false));
							}));

						transitions.Add(transition => transition
							.From(closedState)
							.To(openState)
							.IsTriggeredOn<CaseWorkflow>(e => e.openCaseFromProcessing)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonUpdated));
							}));

						transitions.Add(transition => transition
							.From(closedState)
							.To(openState)
							.IsTriggeredOn<CaseWorkflow>(e => e.openCaseFromPortal)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRCase.isActive>(f => f.SetFromValue(true));
								fields.Add<CRCase.resolution>(f => f.SetFromValue(_reasonOpenedOnPortal));
							}));

						#endregion
					});

				void AddOpenAction(ActionState.IContainerFillerActions filler, bool isDuplicationInToolbar = false)
				{
					filler.Add(actionOpen, a => a.IsDuplicatedInToolbar(isDuplicationInToolbar));
				}

				void AddPendingCustomerAction(ActionState.IContainerFillerActions filler, bool isDuplicationInToolbar = false)
				{
					filler.Add(actionPending, a => a.IsDuplicatedInToolbar(isDuplicationInToolbar));
				}

				void AddCloseAction(ActionState.IContainerFillerActions filler, bool isDuplicationInToolbar = false)
				{
					filler.Add(actionClose, a => a.IsDuplicatedInToolbar(isDuplicationInToolbar));
				}

				void DisableFieldsForFinalStates(FieldState.IContainerFillerFields fields)
				{
					fields.AddTable<CRCase>(field => field.IsDisabled());
					fields.AddTable<CRPMTimeActivity>(field => field.IsDisabled());
					fields.AddTable<CRCaseArticle>(field => field.IsDisabled());
					fields.AddTable<CS.CSAnswers>(field => field.IsDisabled());
					fields.AddField<CRCase.caseCD>();
				}
			}
		}

	}
}
