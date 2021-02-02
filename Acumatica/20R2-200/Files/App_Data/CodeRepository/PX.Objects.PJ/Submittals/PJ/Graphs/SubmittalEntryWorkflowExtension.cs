using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.Submittals.PJ.Descriptor;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PJ.Submittals.PJ.Graphs;
using PX.Objects.CS;
using System;
using static PX.Data.WorkflowAPI.BoundedTo<PX.Objects.PJ.Submittals.PJ.Graphs.SubmittalEntry, PX.Objects.PJ.Submittals.PJ.DAC.PJSubmittal>;
using ReasonDefinition = PX.Objects.PJ.Submittals.PJ.DAC.PJSubmittal.reason;

namespace PX.Objects.PJ.Submittals.PJ.GraphExtensions
{
	public class SubmittalEntryWorkflowExtension : PXGraphExtension<SubmittalEntry>
	{
		public const string FormOpenID = "FormOpen";
		public const string FormCloseID = "FormClose";

		private const string DisableConditionID = "RevisionCondition";

		private static readonly string[] NewReasons =
		{
			ReasonDefinition.New,
			ReasonDefinition.Revision
		};

		private static readonly string[] OpenReasons =
		{
			ReasonDefinition.Issued,
			ReasonDefinition.Submitted,
			ReasonDefinition.PendingApproval
		};

		private static readonly string[] CloseReasons =
		{
			ReasonDefinition.Approved,
			ReasonDefinition.ApprovedAsNoted,
			ReasonDefinition.Rejected,
			ReasonDefinition.Canceled,
			ReasonDefinition.ReviseAndResubmit
		};

		private const string ReasonFieldName = nameof(PJSubmittal.Reason);
		private const string ClosedDateFieldName = nameof(PJSubmittal.DateClosed);

		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);

			var context = configuration.GetScreenConfigurationContext<SubmittalEntry, PJSubmittal>();

			var revisionCondition = context
				.Conditions
				.FromLambda((PJSubmittal s) => s?.Status == PJSubmittal.status.Closed && s?.IsLastRevision != true)
				.WithSharedName(DisableConditionID);

			var openForm = CreateReasonsForm(FormOpenID, OpenReasons, ReasonDefinition.Issued);
			var closeForm = CreateReasonsForm(FormCloseID, CloseReasons, ReasonDefinition.Approved, fields =>
			{
				fields.Add(ClosedDateFieldName, field =>
				{
					return field
						.WithSchemaOf<PJSubmittal.dateClosed>()
						.IsRequired()
						.Prompt(SubmittalMessage.DateClosed)
						.DefaultValue(RelativeDatesManager.TODAY);
				});
			});

			var openAction = CreateAction(openForm, SubmittalMessage.OpenAction, revisionCondition);
			var closeAction = CreateAction(closeForm, SubmittalMessage.CloseAction);
			var deleteAction = context.ActionDefinitions.CreateExisting(sub => sub.Delete,
				act => act.PlaceAfter(sub => sub.Insert));
			var createRevisionAction = context.ActionDefinitions.CreateExisting(sub => sub.CreateRevision, 
				act => act.IsDisabledWhen(revisionCondition).PlaceAfter(openAction));

			var newState = CreateState(PJSubmittal.status.New, 
				ReasonDefinition.New, 
				NewReasons, 
				true,
				openAction,
				deleteAction);

			var openState = CreateState(PJSubmittal.status.Open, 
				ReasonDefinition.Issued, 
				OpenReasons, 
				false,
				closeAction);

			var closedState = CreateState(PJSubmittal.status.Closed,
				ReasonDefinition.Approved,
				CloseReasons,
				false,
				openAction,
				createRevisionAction);

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
				.StateIdentifierIs<PJSubmittal.status>()
				.AddDefaultFlow(flow =>
				{
					return flow
					.WithFlowStates(states =>
					{
						states.Add(newState);
						states.Add(openState);
						states.Add(closedState);
					})
					.WithTransitions(transitons =>
					{
						CreateConfig(newState, openState, openAction);
						CreateConfig(openState, closedState, closeAction, transition =>
						{
							return transition.WithFieldAssignments(fields =>
							{
								fields.Add<PJSubmittal.dateClosed>(f => f.SetFromFormField(closeForm, ClosedDateFieldName));
							});
						});
						CreateConfig(closedState, openState, openAction, transition =>
						{
							return transition.WithFieldAssignments(fields =>
							{
								fields.Add<PJSubmittal.dateClosed>(f => f.SetFromValue(null));
							});
						});

						void CreateConfig(FlowState.IConfigured fromState, 
							FlowState.IConfigured toState, 
							ActionDefinition.IConfigured trigger,
							Func<Transition.IAllowOptionalConfig, Transition.IAllowOptionalConfig> transitionExt = null)
						{
							transitons.Add(transition => 
							{
								var retTransition = transition
									.From(fromState)
									.To(toState)
									.IsTriggeredOn(trigger);
								
								if(transitionExt != null)
								{
									retTransition = transitionExt(retTransition);
								}

								return retTransition;
							});
						}
					});
				})
				.WithActions(actions =>
				{
					actions.Add(openAction);
					actions.Add(closeAction);
					actions.Add(deleteAction);
					actions.Add(createRevisionAction);
				})
				.WithForms(forms =>
				{
					forms.Add(openForm);
					forms.Add(closeForm);
				})
				.WithFieldStates(fields =>
				{
					fields.Add<ReasonDefinition>(field =>
					{
						return field
						.SetComboValues(
							(ReasonDefinition.Approved, SubmittalReason.Approved),
							(ReasonDefinition.ApprovedAsNoted, SubmittalReason.ApprovedAsNoted),
							(ReasonDefinition.Canceled, SubmittalReason.Canceled),
							(ReasonDefinition.Issued, SubmittalReason.Issued),
							(ReasonDefinition.New, SubmittalReason.New),
							(ReasonDefinition.PendingApproval, SubmittalReason.PendingApproval),
							(ReasonDefinition.Rejected, SubmittalReason.Rejected),
							(ReasonDefinition.ReviseAndResubmit, SubmittalReason.ReviseAndResubmit),
							(ReasonDefinition.Revision, SubmittalReason.Revision),
							(ReasonDefinition.Submitted, SubmittalReason.Submitted));
					});
				});
			});

			#region Setup Helpers
			Form.IConfigured CreateReasonsForm(string formID, string[] valueCollection, string defaultValue, Action<FormField.IContainerFillerFields> fieldsExt = null)
			{
				return context.Forms.Create(formID, form =>
				{
					return form
					.Prompt(SubmittalMessage.WorkflowPromptFormTitle)
					.WithFields(fields =>
					{
						fields.Add(ReasonFieldName, field =>
						{
							return field
								.WithSchemaOf<ReasonDefinition>()
								.IsRequired()
								.Prompt(ReasonFieldName)
								.DefaultValue(defaultValue)
								.OnlyComboBoxValues(valueCollection);
						});

						fieldsExt?.Invoke(fields);
					});
				});
			}

			FlowState.IConfigured CreateState(
				string stateName,
				string defaultValue,
				string[] reasons,
				bool isInitial,
				params ActionDefinition.IConfigured[] actionParams)
			{
				return context.FlowStates.Create(stateName, state =>
				{
					var returnState = state
					.WithFieldStates(fields =>
					{
						fields.AddField<ReasonDefinition>(field =>
						{
							return field
								.DefaultValue(defaultValue)
								.ComboBoxValues(reasons);
						});
					})
					.WithActions(actions =>
					{
						foreach (var action in actionParams)
						{
							actions.Add(action, a => a.IsDuplicatedInToolbar());
						}
					});

					return isInitial ? returnState.IsInitial() : returnState;
				});
			}

			ActionDefinition.IConfigured CreateAction(Form.IConfigured form, string name, Condition disableCondition = null)
			{
				return context.ActionDefinitions.CreateNew(name, action =>
				{
					var retAction = action
						.WithFieldAssignments(fields =>
						{
							fields.Add<ReasonDefinition>(field => field.SetFromFormField(form, ReasonFieldName));
						})
						.DisplayName(name)
						.WithForm(form)
						.PlaceAfter(sub => sub.SendEmail);

					return disableCondition != null ? retAction.IsDisabledWhen(disableCondition) : retAction;
				});
			} 
			#endregion
		}
	}
}
