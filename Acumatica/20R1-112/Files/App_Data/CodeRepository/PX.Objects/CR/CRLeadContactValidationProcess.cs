using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	[Serializable]
	public class CRLeadContactValidationProcess : PXGraph<CRLeadContactValidationProcess>
	{
		#region DACs

		[Serializable]
		[PXHidden]
		public partial class ValidationFilter : IBqlTable
		{
			#region ValidationType
			public abstract class validationType : PX.Data.BQL.BqlShort.Field<validationType> { }

			[PXDBShort()]
			[PXDefault((short)0)]
			[PXUIField(DisplayName = "Validation Type")]
			public virtual Int16? ValidationType { get; set; }
			#endregion

			#region CloseNoActivityLeads
			public abstract class closeNoActivityLeads : PX.Data.BQL.BqlBool.Field<closeNoActivityLeads> { }

			[PXDBBool]
			[PXUIField(DisplayName = "Close Leads with No Activities")]
			public virtual Boolean? CloseNoActivityLeads { get; set; }
			#endregion

			#region CloseThreshold
			public abstract class closeThreshold : PX.Data.BQL.BqlDecimal.Field<closeThreshold> { }

			[PXDBDecimal(2)]
			[PXDefault(typeof(CRSetup.closeLeadsWithoutActivitiesScore))]
			[PXUIField(DisplayName = "Closing Threshold")]
			public virtual decimal? CloseThreshold { get; set; }
			#endregion
		}

		[Serializable]
		public partial class Contact2 : Contact
		{
			public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
			[PXDBInt]
			[PXUIField(DisplayName = "Business Account")]
			[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD))]			
			public override Int32? BAccountID { get; set; }

			public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

			public new abstract class duplicateStatus : PX.Data.BQL.BqlString.Field<duplicateStatus> { }
			public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			public new abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }
			public new abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		}

		#endregion

		public PXCancel<ValidationFilter> Cancel;

		public CRLeadContactValidationProcess()
		{
			Actions["Process"].SetVisible(false);
			Actions.Move("Process", "Cancel");
			var setup = Setup.Current;
			PXUIFieldAttribute.SetDisplayName<Contact.displayName>(this.Caches[typeof(Contact)], Messages.Contact);
			PXUIFieldAttribute.SetDisplayName<Contact2.displayName>(this.Caches[typeof(Contact2)], Messages.PossibleDuplicated);
	
			bool LeadValidationThresholdWrongValue = Setup.Current.LeadValidationThreshold == null || Setup.Current.LeadValidationThreshold <= 0;
			bool LeadToAccountValidationThresholdWrongValue = Setup.Current.LeadToAccountValidationThreshold == null || Setup.Current.LeadToAccountValidationThreshold <= 0;
			bool CloseLeadsWithoutActivitiesScoreWrongValue = Setup.Current.CloseLeadsWithoutActivitiesScore == null || Setup.Current.CloseLeadsWithoutActivitiesScore <= 0;

			if (LeadValidationThresholdWrongValue || LeadToAccountValidationThresholdWrongValue || CloseLeadsWithoutActivitiesScoreWrongValue)
			{
				string message = String.Empty;

				if (LeadValidationThresholdWrongValue)
					message += "'" + PXUIFieldAttribute.GetDisplayName<CRSetup.leadValidationThreshold>(Setup.Cache) + "' ";
				if (LeadToAccountValidationThresholdWrongValue)
					message += "'" + PXUIFieldAttribute.GetDisplayName<CRSetup.leadToAccountValidationThreshold>(Setup.Cache) + "' ";
				if (CloseLeadsWithoutActivitiesScoreWrongValue)
					message += "'" + PXUIFieldAttribute.GetDisplayName<CRSetup.closeLeadsWithoutActivitiesScore>(Setup.Cache) + "' ";

				throw new PXSetupNotEnteredException(Messages.CRSetupFieldsAreEmpty, typeof(CRSetup), typeof(CRSetup).Name, message);
			}

			var rules = PXSelect<CRValidationRules>.Select(this);
			if (rules == null || rules.Count == 0)
				throw new PXSetupNotEnteredException(Messages.DuplicateValidationRulesAreEmpty, typeof(CRSetup), typeof(CRSetup).Name);

			Contacts.ParallelProcessingOptions =
				settings =>
				{
					settings.IsEnabled = true;
				};
		}
		public void ValidationFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ValidationFilter filter = (ValidationFilter)e.Row;
			Contacts.SetProcessDelegate((PXGraph graph, ContactAccount record) => ProcessValidation(graph, record, filter));
		}

		private static void ProcessValidation(PXGraph graph, Contact record, ValidationFilter Filter)
		{
			Type itemType = record.GetType();
			PXCache cache = PXGraph.CreateInstance<PXGraph>().Caches[itemType];
			Type graphType;
			object copy = cache.CreateCopy(record);
			PXPrimaryGraphAttribute.FindPrimaryGraph(cache, ref copy, out graphType);

			if (graphType == null)
				throw new PXException(Messages.UnableToFindGraph);

			graph = PXGraph.CreateInstance(graphType);


			graph.Views[graph.PrimaryView].Cache.Current = copy;

			var entity = CRDuplicateEntities<PXGraph, Contact>.RunActionWithAppliedAutomation(
				graph,
				copy,
				nameof(CRDuplicateEntities<PXGraph, Contact>.CheckForDuplicates)) as Contact;

			record.DuplicateFound = entity?.DuplicateFound;
			record.DuplicateStatus = entity?.DuplicateStatus;

			DuplicateDocument document = graph.Caches[typeof(DuplicateDocument)].Current as DuplicateDocument;

			record.DuplicateStatus = document?.DuplicateStatus;
			record.DuplicateFound = document?.DuplicateFound;

			var maxScore = graph
				.Caches[typeof(CRDuplicateRecord)]
				.Cached
				.Cast<CRDuplicateRecord>()
				.Max(_ => _?.Score) ?? 0;

			if (record.DuplicateStatus == DuplicateStatusAttribute.PossibleDuplicated
				&& record.ContactType == ContactTypesAttribute.Lead
				//&& record.Status == LeadStatusesAttribute.New
				&& Filter.CloseNoActivityLeads == true
				&& maxScore > Filter.CloseThreshold)
			{
				CRActivity activity = PXSelect<CRActivity,
					Where<CRActivity.refNoteID, Equal<Required<Contact.noteID>>>>.SelectWindowed(graph, 0, 1, record.NoteID);

				if (activity == null)
				{
					CRDuplicateEntities<PXGraph, Contact>.RunActionWithAppliedAutomation(
						graph,
						copy,
						nameof(CRDuplicateEntities<PXGraph, Contact>.CloseAsDuplicate));
				}
			}
		}

		public PXSetupSelect<CRSetup> Setup;

		public PXFilter<ValidationFilter> Filter;

		[PXViewDetailsButton(typeof(ValidationFilter),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<Contact.contactID>>>>))]
		[PXViewDetailsButton(typeof(ValidationFilter),
			typeof(Select2<BAccount,
				InnerJoin<Contact, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
				Where<Contact.contactID, Equal<Current<Contact.contactID>>>>))]
		public PXFilteredProcessing<
				ContactAccount,
				ValidationFilter,
				Where2<
					Where<Current<ValidationFilter.validationType>, Equal<True>,
						Or<ContactAccount.duplicateStatus, Equal<DuplicateStatusAttribute.notValidated>>>,
					And<ContactAccount.isActive, Equal<True>,
					And<Where<ContactAccount.contactType, Equal<ContactTypesAttribute.lead>,
						Or<ContactAccount.contactType, Equal<ContactTypesAttribute.person>>>>>>>
			Contacts;
	}
}
