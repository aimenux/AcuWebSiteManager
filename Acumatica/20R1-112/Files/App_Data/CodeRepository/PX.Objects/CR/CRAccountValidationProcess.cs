using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	[Serializable]
	public class CRAccountValidationProcess : PXGraph<CRAccountValidationProcess>
	{
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
		}

		public PXCancel<ValidationFilter> Cancel;

		public PXSetupSelect<CRSetup> Setup;

		public PXFilter<ValidationFilter> Filter;

		[PXViewDetailsButton(typeof(ValidationFilter),
			typeof(Select2<BAccount,
				InnerJoin<Contact, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
				Where<Contact.contactID, Equal<Current<Contact.contactID>>>>))]
		public PXFilteredProcessing<
			ContactAccount,
			ValidationFilter,
			Where<ContactAccount.contactType, Equal<ContactTypesAttribute.bAccountProperty>,
				And<ContactAccount.type, NotEqual<BAccountType.branchType>,
				And<ContactAccount.type, NotEqual<BAccountType.organizationType>,
				And<ContactAccount.type, NotEqual<BAccountType.organizationBranchCombinedType>,
				And<ContactAccount.defContactID, Equal<Contact.contactID>,
				And<ContactAccount.accountStatus, NotEqual<BAccount.status.inactive>,
				And<Where<Current<ValidationFilter.validationType>, Equal<True>,
					Or<ContactAccount.duplicateStatus, Equal<DuplicateStatusAttribute.notValidated>>>>>>>>>>>
			Contacts;

		public CRAccountValidationProcess()
		{
			Actions["Process"].SetVisible(false);			
			Actions.Move("Process", "Cancel");

			var setup = Setup.Current;

			if (Setup.Current.AccountValidationThreshold == null || Setup.Current.AccountValidationThreshold <= 0 ||
				Setup.Current.LeadToAccountValidationThreshold == null || Setup.Current.LeadToAccountValidationThreshold <= 0)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(CRSetup), typeof(CRSetup).Name);

			var rules = PXSelect<CRValidationRules>.Select(this);
			if (rules == null || rules.Count == 0)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(CRSetup), typeof(CRSetup).Name);

			Contacts.ParallelProcessingOptions =
				settings =>
				{
					settings.IsEnabled = true;
				};
		}	

		public void ValidationFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ValidationFilter filter = (ValidationFilter) e.Row;
			Contacts.SetProcessDelegate((BusinessAccountMaint graph, ContactAccount record) => ProcessValidation(graph, record, filter));
		}

		private static void ProcessValidation(BusinessAccountMaint graph, Contact record, ValidationFilter Filter)
		{
			Type itemType = record.GetType();
			PXCache cache = PXGraph.CreateInstance<PXGraph>().Caches[itemType];
			Type graphType;
			object copy = cache.CreateCopy(record);
			PXPrimaryGraphAttribute.FindPrimaryGraph(cache, ref copy, out graphType);

			graph.Views[graph.PrimaryView].Cache.Current = copy as BAccount;

			var entity = CRDuplicateEntities<PXGraph, BAccount>.RunActionWithAppliedAutomation(
				graph,
				copy,
				nameof(CRDuplicateEntities<PXGraph, BAccount>.CheckForDuplicates)) as PXResult<BAccount, Contact>;

			record.DuplicateFound = entity?.GetItem<Contact>()?.DuplicateFound;
			record.DuplicateStatus = entity?.GetItem<Contact>()?.DuplicateStatus;

			graph.Clear();
		}
	}
}
