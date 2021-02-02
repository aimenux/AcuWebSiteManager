using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
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
		public PXFilteredProcessing<ContactAccount, ValidationFilter,
		Where<ContactAccount.contactType, Equal<ContactTypesAttribute.bAccountProperty>,
		 And<ContactAccount.type, NotEqual<BAccountType.branchType>,
		 And<ContactAccount.type, NotEqual<BAccountType.organizationType>,
		 And<ContactAccount.type, NotEqual<BAccountType.organizationBranchCombinedType>,
		 And<ContactAccount.defContactID, Equal<Contact.contactID>, 
		 And<Where<Current<ValidationFilter.validationType>, Equal<True>,
							Or<ContactAccount.duplicateStatus, Equal<DuplicateStatusAttribute.notValidated>>>>>>>>>> Contacts;

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
		}	

		public void ValidationFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ValidationFilter filter = (ValidationFilter) e.Row;
			Contacts.SetProcessDelegate(record => ProcessValidation(filter, record));
		}

		private static void ProcessValidation(ValidationFilter filter, Contact record)
		{
			BusinessAccountMaint graph = PXGraph.CreateInstance<BusinessAccountMaint>();
			PXView view = graph.BAccount.View;

			int startRow = 0, totalRows = 0;
			
			BAccount baccount = null;
			Contact contact = null;

			if (record.ContactType == ContactTypesAttribute.BAccountProperty && record.BAccountID != null)
			{
				List<object> list_baccount = view.Select(null, null, new object[] { record.BAccountID }, new string[] { typeof(BAccount.bAccountID).Name }, null, null,
					ref startRow, 1, ref totalRows);
				if (list_baccount != null && list_baccount.Count >= 1)
					baccount = PXResult.Unwrap<BAccount>(list_baccount[0]);
			}
			if (baccount == null || baccount.DefContactID != record.ContactID)
				throw new PXException(Messages.ContactNotFound);

			contact = graph.DefContact.Current = graph.DefContact.SelectWindowed(0, 1);
			contact.DuplicateFound = true;

			PXView viewDuplicates = graph.Duplicates.View;
			if (viewDuplicates == null)
				throw new PXException(Messages.DuplicateViewNotFound);
			viewDuplicates.Clear();
			List<object> duplicates = viewDuplicates.SelectMulti();
			contact = (Contact)graph.DefContact.Cache.CreateCopy(contact);
			contact.DuplicateStatus = DuplicateStatusAttribute.Validated;
			Decimal? score = 0;
			foreach (PXResult<CRDuplicateRecordVirtual, Contact, CRLeadContactValidationProcess.Contact2> r in duplicates)
			{
				CRLeadContactValidationProcess.Contact2 duplicate = r;
				CRDuplicateRecordVirtual contactScore = r;

				int duplicateWeight = ContactMaint.GetContactWeight(duplicate);
				int currentWeight = ContactMaint.GetContactWeight(contact);
				if (duplicateWeight > currentWeight ||
						(duplicateWeight == currentWeight &&
						 duplicate.ContactID < contact.ContactID))
				{
					contact.DuplicateStatus = DuplicateStatusAttribute.PossibleDuplicated;
					if (contactScore.Score > score)
						score = contactScore.Score;
				}
			}
			graph.DefContact.Cache.Update(contact);
			graph.DefContact.Cache.RestoreCopy(record,contact);
			graph.Actions.PressSave();
			
		}
	}
}
