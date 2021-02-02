using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Workflows
{
	using static PX.Data.WorkflowAPI.BoundedTo<LeadMaint, CRLead>;
	using CreateContactExt = LeadMaint.CreateContactFromLeadGraphExt;
	using CreateAccountExt = LeadMaint.CreateBothAccountAndContactFromLeadGraphExt;
	using CreateOpportunityExt = LeadMaint.CreateOpportunityAllFromLeadGraphExt;
	using DuplicateExt = LeadMaint.CRDuplicateEntitiesForLeadGraphExt;
	/// <summary>
	/// Extensions that used to configure Workflow for <see cref="LeadMaint"/> and <see cref="CRLead"/>.
	/// Use Extensions Chaining for this extension if you want customize workflow with code for this graph of DAC.
	/// </summary>
	public class LeadWorkflow : PXGraphExtension<LeadMaint>
	{
		public static bool IsActive() => false;

		#region Consts
		/// <summary>
		/// Statuses for <see cref="CRLead.status"/> used by default in system workflow.
		/// Values could be changed and extended by workflow.
		/// Note, that <see cref="Converted"/> status used in Campaigns screen to count converted leads: <see cref="DAC.Standalone.CRCampaign.leadsConverted"/>.
		/// </summary>
		public static class States
		{
			
			public const string New = "H"; // "H" for historical reasons, to not to break customers data
			public const string Open = "O";
			public const string SalesReady = "Q";
			public const string SalesAccepted = "A";
			public const string Converted = "C";
			public const string Disqualified = "L"; // "L" for historical reasons, to not to break customers data. It was Closed.

			[Obsolete("This status used only for backward (data) compatibility.")]
			public const string Suspend = "S";

			internal class converted : BqlString.Constant<converted>
			{
				public converted() : base(Converted) { }
			}

			internal class List : PXStringListAttribute
			{
				public List()
					: base(
						new[]
						{
							New,
							Open,
							SalesReady,
							SalesAccepted,
							Converted,
							Disqualified,
						},
						new[]
						{
							"New",
							"Open",
							"Sales-Ready",
							"Sales-Accepted",
							"Converted",
							"Disqualified",
						}
					)
				{ }
			}
		}

		private const string
				_fieldReason = "Reason",

				_formOpen = "FormOpen",
				_formQualify = "FormQualify",
				_formAccept = "FormAccept",
				_formDisqualify = "FormDisqualify",
				_formConvert = "FormConvert",

				_actionOpen = "Open",
				_actionQualify = "Qualify",
				_actionAccept = "Accept",
				_actionDisqualify = "Disqualify",
				_actionMarkAsConverted = "MarkAsConverted",

				_reasonCreated = "CR",
				_reasonPotentialInterest = "PI",
				_reasonSubscribed = "SB",
				_reasonInquiry = "IQ",
				_reasonQualifiedByMarketing = "QM",
				_reasonAcceptedBySales = "AS",
				_reasonQualifiedBySales = "QS",
				_reasonNoInterest = "NI",
				_reasonDuplicate = "DL",
				_reasonUnableToContact = "CL",
				_reasonOther = "OT";
		#endregion

		public override void Configure(PXScreenConfiguration configuration)
		{
			var context = configuration.GetScreenConfigurationContext<LeadMaint, CRLead>();

			#region forms

			var reasons = new Dictionary<string, (string @default, string[] values)>(6)
			{
				[States.New] = (_reasonCreated,
					new[] { _reasonCreated }),

				[States.Open] = (_reasonPotentialInterest,
					new[] { _reasonPotentialInterest, _reasonSubscribed, _reasonInquiry }),

				[States.SalesReady] = (_reasonQualifiedByMarketing,
					new[] { _reasonQualifiedByMarketing }),

				[States.SalesAccepted] = (_reasonAcceptedBySales,
					new[] { _reasonAcceptedBySales, _reasonQualifiedByMarketing }),

				[States.Converted] = (_reasonQualifiedBySales,
					new[] { _reasonQualifiedBySales, _reasonAcceptedBySales, _reasonQualifiedByMarketing }),

				[States.Disqualified] = (null,
					new[] { _reasonNoInterest, _reasonUnableToContact, _reasonDuplicate, _reasonOther }),
			};

			var formOpen = context.Forms.Create(_formOpen, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, reasons[States.Open]);
					}));

			var formQualify = context.Forms.Create(_formQualify, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, reasons[States.SalesReady]);
					}));

			var formAccept = context.Forms.Create(_formAccept, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, reasons[States.SalesAccepted]);
					}));

			var formDisqualify = context.Forms.Create(_formDisqualify, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, reasons[States.Disqualified]);
					}));


			var formConvert = context.Forms.Create(_formConvert, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, reasons[States.Converted]);
					}));

			void AddResolutionFormField(FormField.IContainerFillerFields filler, (string defaultValue, string[] values) comboBox)
			{
				filler.Add(_fieldReason, field => field
					.WithSchemaOf<CRLead.resolution>()
					.IsRequired()
					.Prompt("Reason")
					.DefaultValue(comboBox.defaultValue)
					.OnlyComboBoxValues(comboBox.values));
			}

			#endregion

            var actionOpen = context.ActionDefinitions.CreateNew(_actionOpen, a => a
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CRLead.isActive>(f => f.SetFromValue(true));
                    fields.Add<CRLead.resolution>(f => f.SetFromFormField(formOpen, _fieldReason));
                })
                .DisplayName("Open")
                .WithForm(formOpen)
                .MassProcessingScreen<UpdateLeadMassProcess>());

            var actionQualify = context.ActionDefinitions.CreateNew(_actionQualify, a => a
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CRLead.isActive>(f => f.SetFromValue(true));
                    fields.Add<CRLead.resolution>(f => f.SetFromFormField(formQualify, _fieldReason));
                })
                .DisplayName("Qualify")
                .WithForm(formQualify)
                .MassProcessingScreen<UpdateLeadMassProcess>());

            var actionAccept = context.ActionDefinitions.CreateNew(_actionAccept, a => a
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CRLead.isActive>(f => f.SetFromValue(true));
                    fields.Add<CRLead.resolution>(f => f.SetFromFormField(formAccept, _fieldReason));
                })
                .DisplayName("Accept")
                .WithForm(formAccept)
                .MassProcessingScreen<UpdateLeadMassProcess>());

            var actionDisqualify = context.ActionDefinitions.CreateNew(_actionDisqualify, a => a
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CRLead.isActive>(f => f.SetFromValue(false));
                    fields.Add<CRLead.resolution>(f => f.SetFromFormField(formDisqualify, _fieldReason));
                    fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true)); // AC-152363: Disqualifying lead must set the Override checkbox
                })
                .DisplayName("Disqualify")
                .WithForm(formDisqualify)
                .MassProcessingScreen<UpdateLeadMassProcess>());

            var actionMarkAsConverted = context.ActionDefinitions.CreateNew(_actionMarkAsConverted, a => a
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CRLead.resolution>(f => f.SetFromFormField(formConvert, _fieldReason));
                    fields.Add<CRLead.isActive>(f => f.SetFromValue(false));
                })
                .DisplayName("Mark as Converted")
                .WithForm(formConvert)
                .MassProcessingScreen<UpdateLeadMassProcess>());

            var actionCloseAsDuplicate = context.ActionDefinitions.CreateExisting<DuplicateExt>(e => e.CloseAsDuplicate,
                a => a
                    .WithFieldAssignments(fields =>
                    {
                        fields.Add<CRLead.isActive>(f => f.SetFromValue(false));
                        fields.Add<CRLead.resolution>(f => f.SetFromValue(_reasonDuplicate));
                        fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true)); // AC-152363: Closing as duplicate lead must set the Override checkbox
                    }).InFolder(FolderType.ActionsFolder));

            var actionConvertToOpportunityAll = context.ActionDefinitions.CreateExisting<CreateOpportunityExt>(
                e => e.ConvertToOpportunityAll, a => a
                    // assignments only in transition, because of AC-156932
                    //.WithFieldAssignments(fields =>
                    //{
                    //    fields.Add<CRLead.resolution>(f => f.SetFromValue(reasons[States.Converted].@default));
                    //    fields.Add<CRLead.isActive>(f => f.SetFromValue(false));
                    //})
                    .InFolder(FolderType.ActionsFolder));

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.StateIdentifierIs<CRLead.status>()
					.AddDefaultFlow(DefaultLeadFlow)
					.WithActions(actions =>
					{
						actions.Add(actionOpen);
						actions.Add(actionQualify);
						actions.Add(actionAccept);
						actions.Add(actionDisqualify);
						actions.Add(actionMarkAsConverted);
                        actions.Add(actionCloseAsDuplicate);
                        actions.Add(actionConvertToOpportunityAll);

						actions.Add<CreateContactExt>(e => e.CreateContact, a => a.InFolder(FolderType.ActionsFolder));
						actions.Add<CreateAccountExt>(e => e.CreateBothContactAndAccount, a => a.InFolder(FolderType.ActionsFolder));
						actions.Add<DuplicateExt>(e => e.CheckForDuplicates, a => a.InFolder(FolderType.ActionsFolder));
						actions.Add<DuplicateExt>(e => e.MarkAsValidated, a => a.InFolder(FolderType.ActionsFolder));
                        actions.Add(AddressSelectBase._BUTTON_ACTION, a => a.InFolder(FolderType.ActionsFolder));
					})
					.WithForms(forms =>
					{
						forms.Add(formOpen);
						forms.Add(formQualify);
						forms.Add(formAccept);
						forms.Add(formDisqualify);
						forms.Add(formConvert);
					})
					.WithFieldStates(fields =>
					{
#pragma warning disable CS0618 // Type or member is obsolete
						fields.Add<CRLead.status>(field => field
								.SetComboValues(
									(States.Suspend, "Suspended")));

						fields.Add<CRLead.source>(field => field
								.SetComboValues(
									(CRMSourcesAttribute._WEB, "Web"),
									(CRMSourcesAttribute._PHONE_INQ, "Phone Inquiry"),
									(CRMSourcesAttribute._PURCHASED_LIST, "Purchased List")));

#pragma warning restore CS0618 // Type or member is obsolete

						fields.Add<CRLead.resolution>(field => field
								.SetComboValues(
									(_reasonCreated, "Created"),
									(_reasonPotentialInterest, "Potential Interest"),
									(_reasonSubscribed, "Subscribed"),
									(_reasonInquiry, "Inquiry"),
									(_reasonQualifiedByMarketing, "Qualified by Marketing"),
									(_reasonAcceptedBySales, "Accepted by Sales"),
									(_reasonQualifiedBySales, "Qualified by Sales"),
									(_reasonNoInterest, "No Interest"),
									(_reasonDuplicate, "Duplicate"),
									(_reasonUnableToContact, "Unable To Contact"),
									(_reasonOther, "Other")));
					});
			});

			Workflow.IConfigured DefaultLeadFlow(Workflow.INeedStatesFlow flow)
			{
				#region states

				var newState = context.FlowStates.Create(States.New, state => state
					.IsInitial()
					.WithFieldStates(fields =>
					{
						AddReasonFieldState(fields, States.New);
						AddSourceFieldState(fields);
					})
					.WithActions(actions =>
					{
						AddOpenAction(actions, addToToolbar: true);
						AddQualifyAction(actions);
						AddAcceptAction(actions);
						AddDisqualifyAction(actions);
						AddConvertToOpportunityAction(actions);
						AddMarkAsConvertedAction(actions);

						actions.Add<CreateContactExt>(e => e.CreateContact);
						actions.Add<CreateAccountExt>(e => e.CreateBothContactAndAccount);

						AddCloseAsDuplicateAction(actions);
						actions.Add<DuplicateExt>(e => e.CheckForDuplicates);
						actions.Add<DuplicateExt>(e => e.MarkAsValidated);

						actions.Add(AddressSelectBase._BUTTON_ACTION);
					}));

				var openState = context.FlowStates.Create(States.Open, state => state
					.WithFieldStates(fields =>
					{
						AddReasonFieldState(fields, States.Open);
						AddSourceFieldState(fields);
					})
					.WithActions(actions =>
					{
						AddQualifyAction(actions, addToToolbar: true);
						AddAcceptAction(actions);
						AddDisqualifyAction(actions);
						AddConvertToOpportunityAction(actions);
						AddMarkAsConvertedAction(actions);

						actions.Add<CreateContactExt>(e => e.CreateContact);
						actions.Add<CreateAccountExt>(e => e.CreateBothContactAndAccount);

						AddCloseAsDuplicateAction(actions);
						actions.Add<DuplicateExt>(e => e.CheckForDuplicates);
						actions.Add<DuplicateExt>(e => e.MarkAsValidated);

						actions.Add(AddressSelectBase._BUTTON_ACTION);
					}));

				var salesReadyState = context.FlowStates.Create(States.SalesReady, state => state
					.WithFieldStates(fields =>
					{
						AddReasonFieldState(fields, States.SalesReady, disabled: true);
						AddSourceFieldState(fields, disabled: true);
						fields.AddField<CRLead.campaignID>(f => f.IsDisabled());
					})
					.WithActions(actions =>
					{
						AddOpenAction(actions);
						AddAcceptAction(actions, addToToolbar: true);
						AddDisqualifyAction(actions);
						AddConvertToOpportunityAction(actions);
						AddMarkAsConvertedAction(actions);

						actions.Add<CreateContactExt>(e => e.CreateContact);
						actions.Add<CreateAccountExt>(e => e.CreateBothContactAndAccount);

						AddCloseAsDuplicateAction(actions);
						actions.Add<DuplicateExt>(e => e.CheckForDuplicates);
						actions.Add<DuplicateExt>(e => e.MarkAsValidated);

						actions.Add(AddressSelectBase._BUTTON_ACTION);
					}));

				var salesAcceptedState = context.FlowStates.Create(States.SalesAccepted, state => state
					.WithFieldStates(fields =>
					{
						AddReasonFieldState(fields, States.SalesAccepted, disabled: true);
						AddSourceFieldState(fields, disabled: true);
						fields.AddField<CRLead.campaignID>(f => f.IsDisabled());
					})
					.WithActions(actions =>
					{
						AddOpenAction(actions);
						AddDisqualifyAction(actions);
						AddConvertToOpportunityAction(actions, addToToolbar: true);
						AddMarkAsConvertedAction(actions);

						actions.Add<CreateContactExt>(e => e.CreateContact);
						actions.Add<CreateAccountExt>(e => e.CreateBothContactAndAccount);

						AddCloseAsDuplicateAction(actions);
						actions.Add<DuplicateExt>(e => e.CheckForDuplicates);
						actions.Add<DuplicateExt>(e => e.MarkAsValidated);

						actions.Add(AddressSelectBase._BUTTON_ACTION);
					}));

				var convertedState = context.FlowStates.Create(States.Converted, state => state
					.WithFieldStates(fields =>
					{
						AddReasonFieldState(fields, States.Converted, disabled: true);

						DisableFieldsForFinalStates(fields);
					})
					.WithActions(actions =>
					{
						AddOpenAction(actions);
					}));

				var disqualifiedState = context.FlowStates.Create(States.Disqualified, state => state
					.WithFieldStates(fields =>
					{
						AddReasonFieldState(fields, States.Disqualified, disabled: true);

						DisableFieldsForFinalStates(fields);
					})
					.WithActions(actions =>
					{
						AddOpenAction(actions);
					}));

#pragma warning disable CS0618 // Type or member is obsolete
				var suspendState = context.FlowStates.Create(States.Suspend, state => state
					.WithFieldStates(fields =>
					{
						DisableFieldsForFinalStates(fields);
					})
					.WithActions(actions =>
					{
						AddOpenAction(actions);
					}));
#pragma warning restore CS0618 // Type or member is obsolete

				#endregion

				return flow
					.WithFlowStates(states =>
					{
						states.Add(newState);
						states.Add(openState);
						states.Add(salesReadyState);
						states.Add(salesAcceptedState);
						states.Add(disqualifiedState);
						states.Add(convertedState);
						states.Add(suspendState);
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
							.To(salesReadyState)
							.IsTriggeredOn(actionQualify));

						transitions.Add(transition => transition
							.From(newState)
							.To(salesAcceptedState)
							.IsTriggeredOn(actionAccept));

						transitions.Add(transition => transition
							.From(newState)
							.To(disqualifiedState)
							.IsTriggeredOn(actionDisqualify));

						transitions.Add(transition => transition
							.From(newState)
							.To(disqualifiedState)
							.IsTriggeredOn<DuplicateExt>(e => e.CloseAsDuplicate));

						transitions.Add(transition => transition
							.From(newState)
							.To(convertedState)
							.IsTriggeredOn(actionMarkAsConverted)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true));
							}));

						transitions.Add(transition => transition
							.From(newState)
							.To(convertedState)
							.IsTriggeredOn<CreateOpportunityExt>(e => e.ConvertToOpportunityAll)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true));
								fields.Add<CRLead.isActive>(f => f.SetFromValue(false));
								fields.Add<CRLead.resolution>(f => f.SetFromValue(reasons[States.Converted].@default));
							}));

						#endregion
						#region open

						transitions.Add(transition => transition
							.From(openState)
							.To(salesReadyState)
							.IsTriggeredOn(actionQualify));

						transitions.Add(transition => transition
							.From(openState)
							.To(salesAcceptedState)
							.IsTriggeredOn(actionAccept));

						transitions.Add(transition => transition
							.From(openState)
							.To(disqualifiedState)
							.IsTriggeredOn(actionDisqualify));

						transitions.Add(transition => transition
							.From(openState)
							.To(disqualifiedState)
							.IsTriggeredOn<DuplicateExt>(e => e.CloseAsDuplicate));

						transitions.Add(transition => transition
							.From(openState)
							.To(convertedState)
							.IsTriggeredOn(actionMarkAsConverted)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true));
							}));

						transitions.Add(transition => transition
							.From(openState)
							.To(convertedState)
							.IsTriggeredOn<CreateOpportunityExt>(e => e.ConvertToOpportunityAll)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true));
								fields.Add<CRLead.isActive>(f => f.SetFromValue(false));
								fields.Add<CRLead.resolution>(f => f.SetFromValue(reasons[States.Converted].@default));
							}));


						#endregion
						#region salesReady

						transitions.Add(transition => transition
							.From(salesReadyState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						transitions.Add(transition => transition
							.From(salesReadyState)
							.To(salesAcceptedState)
							.IsTriggeredOn(actionAccept));

						transitions.Add(transition => transition
							.From(salesReadyState)
							.To(disqualifiedState)
							.IsTriggeredOn(actionDisqualify));

						transitions.Add(transition => transition
							.From(salesReadyState)
							.To(disqualifiedState)
							.IsTriggeredOn<DuplicateExt>(e => e.CloseAsDuplicate));

						transitions.Add(transition => transition
							.From(salesReadyState)
							.To(convertedState)
							.IsTriggeredOn(actionMarkAsConverted)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true));
							}));

						transitions.Add(transition => transition
							.From(salesReadyState)
							.To(convertedState)
							.IsTriggeredOn<CreateOpportunityExt>(e => e.ConvertToOpportunityAll)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true));
								fields.Add<CRLead.isActive>(f => f.SetFromValue(false));
								fields.Add<CRLead.resolution>(f => f.SetFromValue(reasons[States.Converted].@default));
							}));

						#endregion
						#region salesAccepted

						transitions.Add(transition => transition
							.From(salesAcceptedState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						transitions.Add(transition => transition
							.From(salesAcceptedState)
							.To(disqualifiedState)
							.IsTriggeredOn(actionDisqualify));

						transitions.Add(transition => transition
							.From(salesAcceptedState)
							.To(disqualifiedState)
							.IsTriggeredOn<DuplicateExt>(e => e.CloseAsDuplicate));

						transitions.Add(transition => transition
							.From(salesAcceptedState)
							.To(convertedState)
							.IsTriggeredOn(actionMarkAsConverted)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true));
							}));

						transitions.Add(transition => transition
							.From(salesAcceptedState)
							.To(convertedState)
							.IsTriggeredOn<CreateOpportunityExt>(e => e.ConvertToOpportunityAll)
							.WithFieldAssignments(fields =>
							{
								fields.Add<CRLead.overrideRefContact>(f => f.SetFromValue(true));
								fields.Add<CRLead.isActive>(f => f.SetFromValue(false));
								fields.Add<CRLead.resolution>(f => f.SetFromValue(reasons[States.Converted].@default));
							}));

						#endregion
						#region disqualified

						transitions.Add(transition => transition
							.From(disqualifiedState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						#endregion
						#region converted

						transitions.Add(transition => transition
							.From(convertedState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						#endregion
						#region suspend

						transitions.Add(transition => transition
							.From(suspendState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						#endregion
					});

				void AddReasonFieldState(FieldState.IContainerFillerFields filler, string state, bool disabled = false)
				{
					filler.AddField<CRLead.resolution>(field => field
						.DefaultValue(reasons[state].@default)
						.ComboBoxValues(reasons[state].values)
						.IsRequired()
						.IsDisabled(disabled));
				}

				void AddSourceFieldState(FieldState.IContainerFillerFields filler, bool disabled = false)
				{
					filler.AddField<CRLead.source>(field => field.ComboBoxValues(CRMSourcesAttribute.Values).IsDisabled(disabled));
				}

				void DisableFieldsForFinalStates(FieldState.IContainerFillerFields filler)
				{
					filler.AddTable<CRLead>(f => f.IsDisabled());
					filler.AddTable<Address>(f => f.IsDisabled());
					filler.AddTable<CRCampaignMembers>(f => f.IsDisabled());
					filler.AddTable<CROpportunity>(f => f.IsDisabled());
					filler.AddTable<CS.CSAnswers>(f => f.IsDisabled());
					filler.AddField<CRLead.contactID>();
					filler.AddField<CRLead.description>();
				}

				void AddCloseAsDuplicateAction(ActionState.IContainerFillerActions filler)
				{
					filler.Add(actionCloseAsDuplicate, a => a);
				}

				void AddOpenAction(ActionState.IContainerFillerActions filler, bool addToToolbar = false)
				{
					filler.Add(actionOpen, a => a.IsDuplicatedInToolbar(addToToolbar));
				}

				void AddQualifyAction(ActionState.IContainerFillerActions filler, bool addToToolbar = false)
				{
					filler.Add(actionQualify, a => a.IsDuplicatedInToolbar(addToToolbar));
				}

				void AddAcceptAction(ActionState.IContainerFillerActions filler, bool addToToolbar = false)
				{
					filler.Add(actionAccept, a => a.IsDuplicatedInToolbar(addToToolbar));
				}

				void AddDisqualifyAction(ActionState.IContainerFillerActions filler)
				{
					filler.Add(actionDisqualify, a => a);
				}

				void AddConvertToOpportunityAction(ActionState.IContainerFillerActions filler, bool addToToolbar = false)
				{
					filler.Add(actionConvertToOpportunityAll, a => a.IsDuplicatedInToolbar(addToToolbar));
				}

				void AddMarkAsConvertedAction(ActionState.IContainerFillerActions filler)
				{
					filler.Add(actionMarkAsConverted, a => a);
				}
			}
		}
	}
}
