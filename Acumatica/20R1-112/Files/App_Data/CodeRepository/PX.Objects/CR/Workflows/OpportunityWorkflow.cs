using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.WorkflowAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Workflows
{
	using static PX.Data.WorkflowAPI.BoundedTo<OpportunityMaint, CROpportunity>;
	using CreateContactExt = OpportunityMaint.CreateContactFromOpportunityGraphExt;
	using CreateAccountExt = OpportunityMaint.CreateBothAccountAndContactFromOpportunityGraphExt;

	/// <summary>
	/// Extensions that used to configure Workflow for <see cref="OpportunityMaint"/> and <see cref="CROpportunity"/>.
	/// Use Extensions Chaining for this extension if you want customize workflow with code for this graph of DAC.
	/// </summary>
	public class OpportunityWorkflow : PX.Data.PXGraphExtension<OpportunityMaint>
	{
		// workflow works without checking active
		public static bool IsActive() => false;

		#region Consts

		/// <summary>
		/// Statuses for <see cref="CROpportunity.status"/> used by default in system workflow.
		/// Values could be changed and extended by workflow.
		/// Note, that <see cref="Won"/> status used in Campaigns screen to count won opportunities: <see cref="DAC.Standalone.CRCampaign.closedOpportunities"/>.
		/// </summary>
		public static class States
		{
			public const string New = "N";
			public const string Open = "O";
			public const string Won = "W";
			public const string Lost = "L";

			internal class won : BqlString.Constant<won>
			{
				public won() : base(Won) { }
			}

			internal class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new[]
						{
							New,
							Open,
							Won,
							Lost,
						},
						new[]
						{
							"New",
							"Open",
							"Won",
							"Lost",
						}
					)
				{ }
			}
		}

		private const string
				_fieldReason = "Reason",
				_fieldStage = "Stage",

				_formOpen = "FormOpen",
				_formWon = "FormWon",
				_formLost = "FormLost",

				_actionOpen = "Open",
				_actionWon = "Won",
				_actionLost = "Lost",

				_reasonTechnology = "TH",
				_reasonRelationship = "RL",
				_reasonPrice = "PR",
				_reasonOther = "OT",
				_reasonAssign = "NA",
				_reasonInProcess = "IP",
				_reasonFunctionality = "FC",
				_reasonCompanyMaturity = "CM",
				_reasonCanceled = "CL";

		#endregion

		public override void Configure(PXScreenConfiguration config)
		{
			var context = config.GetScreenConfigurationContext<OpportunityMaint, CROpportunity>();
			#region forms

			var formOpen = context.Forms.Create(_formOpen, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, _reasonInProcess, _reasonInProcess);
						AddStageIDFormField(fields);
					}));

			var formWon = context.Forms.Create(_formWon, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, _reasonOther,
							_reasonOther, _reasonPrice, _reasonRelationship, _reasonTechnology);
						AddStageIDFormField(fields);
					}));

			var formLost = context.Forms.Create(_formLost, form => form
					.Prompt("Details")
					.WithFields(fields =>
					{
						AddResolutionFormField(fields, _reasonCanceled,
							_reasonCanceled, _reasonOther, _reasonPrice, _reasonCompanyMaturity, _reasonFunctionality);
						AddStageIDFormField(fields);
					}));

			void AddResolutionFormField(FormField.IContainerFillerFields filler, string defaultValue, params string[] values)
			{
				filler.Add(_fieldReason, field => field
					.WithSchemaOf<CROpportunity.resolution>()
					.IsRequired()
					.Prompt("Reason")
					.DefaultValue(defaultValue)
					.OnlyComboBoxValues(values));
			}

			void AddStageIDFormField(FormField.IContainerFillerFields filler)
			{
				filler.Add(_fieldStage, field => field
					.WithSchemaOf<CROpportunity.stageID>()
					.DefaultValueFromSchemaField()
					.Prompt("Stage"));
			}

			#endregion

            var actionOpen = context.ActionDefinitions.CreateNew(_actionOpen, action => action
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CROpportunity.resolution>(f => f.SetFromFormField(formOpen, _fieldReason));
                    fields.Add<CROpportunity.stageID>(f => f.SetFromFormField(formOpen, _fieldStage));
                    fields.Add<CROpportunity.isActive>(f => f.SetFromValue(true));
                    fields.Add<CROpportunity.closingDate>(f => f.SetFromValue(null));
                })
                .DisplayName("Open")
                .WithForm(formOpen)
                .MassProcessingScreen<UpdateOpportunityMassProcess>());

            var actionWon = context.ActionDefinitions.CreateNew(_actionWon, action => action
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CROpportunity.resolution>(f => f.SetFromFormField(formWon, _fieldReason));
                    fields.Add<CROpportunity.stageID>(f => f.SetFromFormField(formWon, _fieldStage));
                    fields.Add<CROpportunity.isActive>(f => f.SetFromValue(false));
                    fields.Add<CROpportunity.closingDate>(f => f.SetFromToday());
                })
                .DisplayName("Close as Won")
                .WithForm(formWon)
                .MassProcessingScreen<UpdateOpportunityMassProcess>());

            var actionLost = context.ActionDefinitions.CreateNew(_actionLost, action => action
                .WithFieldAssignments(fields =>
                {
                    fields.Add<CROpportunity.resolution>(f => f.SetFromFormField(formLost, _fieldReason));
                    fields.Add<CROpportunity.stageID>(f => f.SetFromFormField(formLost, _fieldStage));
                    fields.Add<CROpportunity.isActive>(f => f.SetFromValue(false));
                    fields.Add<CROpportunity.closingDate>(f => f.SetFromToday());
                })
                .DisplayName("Close as Lost")
                .WithForm(formLost)
                .MassProcessingScreen<UpdateOpportunityMassProcess>());

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.StateIdentifierIs<CROpportunity.status>()
					.AddDefaultFlow(DefaultOpportunityFlow)
					.WithActions(actions =>
					{
						actions.Add(actionOpen);
						actions.Add(actionWon);
						actions.Add(actionLost);

						actions.Add(g => g.submitQuote, c => c.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.editQuote, c => c.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.createSalesOrder, c => c.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.createInvoice, c => c.InFolder(FolderType.ActionsFolder));
						actions.Add(g => g.validateAddresses, c => c.InFolder(FolderType.ActionsFolder));
						actions.Add<CreateContactExt>(e => e.CreateContact, c => c.InFolder(FolderType.ActionsFolder));
						actions.Add<CreateAccountExt>(e => e.CreateBothContactAndAccount, c => c.InFolder(FolderType.ActionsFolder));
						actions.Add<OpportunityMaint.Discount>(e => e.recalculatePrices, c => c.InFolder(FolderType.ActionsFolder));
					})
					.WithForms(forms =>
					{
						forms.Add(formOpen);
						forms.Add(formWon);
						forms.Add(formLost);
					})
					.WithFieldStates(fields =>
					{
#pragma warning disable CS0618 // Type or member is obsolete
						fields.Add<CROpportunity.source>(field => field
								.SetComboValues(
									(CRMSourcesAttribute._WEB, "Web"),
									(CRMSourcesAttribute._PHONE_INQ, "Phone Inquiry"),
									(CRMSourcesAttribute._PURCHASED_LIST, "Purchased List")));
#pragma warning restore CS0618 // Type or member is obsolete

						fields.Add<CROpportunity.resolution>(field => field
								.SetComboValues(
									(_reasonTechnology, "Technology"),
									(_reasonRelationship, "Relationship"),
									(_reasonPrice, "Price"),
									(_reasonOther, "Other"),
									(_reasonAssign, "Assign"),
									(_reasonInProcess, "In Process"),
									(_reasonFunctionality, "Functionality"),
									(_reasonCompanyMaturity, "Company Maturity"),
									(_reasonCanceled, "Canceled")));
					});
			});

			Workflow.IConfigured DefaultOpportunityFlow(Workflow.INeedStatesFlow flow)
			{
				#region states

				var newState = context.FlowStates.Create(States.New, state => state
					.IsInitial()
					.WithFieldStates(fields =>
					{
						fields.AddField<CROpportunity.resolution>(field =>
							field.DefaultValue(_reasonAssign).ComboBoxValues(_reasonAssign));

						AddSourceFieldState(fields);
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.createSalesOrder);
						actions.Add(g => g.createInvoice);
						actions.Add<CreateContactExt>(e => e.CreateContact);
						actions.Add<CreateAccountExt>(e => e.CreateBothContactAndAccount);
						actions.Add(g => g.validateAddresses);
						actions.Add<OpportunityMaint.Discount>(e => e.recalculatePrices);
						AddOpenAction(actions, isDuplicationInToolbar: true);
						AddWonAction(actions);
						AddLostAction(actions);
					}));

				var openState = context.FlowStates.Create(States.Open, state => state
					.WithFieldStates(fields =>
					{
						fields.AddField<CROpportunity.resolution>(field =>
							field.ComboBoxValues(_reasonInProcess));

						AddSourceFieldState(fields);
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.createSalesOrder);
						actions.Add(g => g.createInvoice);
						actions.Add<CreateContactExt>(e => e.CreateContact);
						actions.Add<CreateAccountExt>(e => e.CreateBothContactAndAccount);
						actions.Add(g => g.validateAddresses);
						actions.Add<OpportunityMaint.Discount>(e => e.recalculatePrices);
						AddWonAction(actions, isDuplicationInToolbar: true);
						AddLostAction(actions);
					}));

				var wonState = context.FlowStates.Create(States.Won, state => state
					.WithFieldStates(fields =>
					{
						fields.AddField<CROpportunity.resolution>(field => field
							.ComboBoxValues(_reasonOther, _reasonPrice, _reasonRelationship, _reasonTechnology)
							.IsDisabled());

						DisableFieldsForFinalStates(fields);
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.createSalesOrder);
						actions.Add(g => g.createInvoice);
						AddOpenAction(actions);
					}));

				var lostState = context.FlowStates.Create(States.Lost, state => state
					.WithFieldStates(fields =>
					{
						fields.AddField<CROpportunity.resolution>(field => field
							.ComboBoxValues(_reasonCanceled, _reasonOther, _reasonPrice, _reasonCompanyMaturity, _reasonFunctionality)
							.IsDisabled());

						DisableFieldsForFinalStates(fields);
					})
					.WithActions(actions =>
					{
						AddOpenAction(actions);
					}));

				#endregion

				return flow
					.WithFlowStates(states =>
					{
						states.Add(newState);
						states.Add(openState);
						states.Add(wonState);
						states.Add(lostState);
					})
					.WithTransitions(transitions =>
					{
						transitions.Add(transition => transition
							.From(newState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						transitions.Add(transition => transition
							.From(newState)
							.To(wonState)
							.IsTriggeredOn(actionWon));

						transitions.Add(transition => transition
							.From(newState)
							.To(lostState)
							.IsTriggeredOn(actionLost));
						transitions.Add(transition => transition
							.From(openState)
							.To(wonState)
							.IsTriggeredOn(actionWon));

						transitions.Add(transition => transition
							.From(openState)
							.To(lostState)
							.IsTriggeredOn(actionLost));

						transitions.Add(transition => transition
							.From(wonState)
							.To(openState)
							.IsTriggeredOn(actionOpen));

						transitions.Add(transition => transition
							.From(lostState)
							.To(openState)
							.IsTriggeredOn(actionOpen));
					});

				void AddOpenAction(ActionState.IContainerFillerActions filler, bool isDuplicationInToolbar = false)
				{
					filler.Add(actionOpen, a => a
						
						.IsDuplicatedInToolbar(isDuplicationInToolbar));
				}

				void AddWonAction(ActionState.IContainerFillerActions filler, bool isDuplicationInToolbar = false)
				{
					filler.Add(actionWon, a => a
					
						.IsDuplicatedInToolbar(isDuplicationInToolbar));
				}

				void AddLostAction(ActionState.IContainerFillerActions filler, bool isDuplicationInToolbar = false)
				{
					filler.Add(actionLost, a => a
						
						.IsDuplicatedInToolbar(isDuplicationInToolbar));
				}


				void AddSourceFieldState(FieldState.IContainerFillerFields filler)
				{
					filler.AddField<CROpportunity.source>(field => field.ComboBoxValues(CRMSourcesAttribute.Values));
				}

				void DisableFieldsForFinalStates(FieldState.IContainerFillerFields fields)
				{
					fields.AddTable<CROpportunity>(field => field.IsDisabled());
					fields.AddTable<CROpportunityProducts>(field => field.IsDisabled());
					fields.AddTable<CRTaxTran>(field => field.IsDisabled());
					fields.AddTable<CROpportunityDiscountDetail>(field => field.IsDisabled());
					fields.AddTable<CRContact>(field => field.IsDisabled());
					fields.AddTable<CRAddress>(field => field.IsDisabled());
					fields.AddTable<CS.CSAnswers>(field => field.IsDisabled());
					fields.AddTable<CROpportunityTax>(field => field.IsDisabled());
					fields.AddField<CROpportunity.opportunityID>();
				}
			}
		}
	}
}
