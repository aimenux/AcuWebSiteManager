using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.MassProcess;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	[Serializable]
	public class CRLeadContactValidationProcess : PXGraph<CRLeadContactValidationProcess>
	{
		public PXCancel<ValidationFilter> Cancel;
		
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
		}
		public void ValidationFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ValidationFilter filter = (ValidationFilter)e.Row;
			Contacts.SetProcessDelegate(record=> ProcessValidation(filter, record));
		}

		private static void ProcessValidation(ValidationFilter Filter, Contact record)
		{
				PXPrimaryGraphCollection primaryGraph = new PXPrimaryGraphCollection(new PXGraph());
				PXGraph graph = primaryGraph[record];
				if (graph == null)
					throw new PXException(Messages.UnableToFindGraph);

				PXView view = graph.Views[graph.PrimaryView];
				int startRow = 0, totalRows = 0;
				List<object> list_contact = view.Select(null, null, new object[] { record.ContactID }, new string[] { typeof(Contact.contactID).Name }, null, null,
					ref startRow, 1, ref totalRows);
				if (list_contact == null || list_contact.Count < 1)
				{					
					throw new PXException(Messages.ContactNotFound);					
				}				
				Contact contact = PXResult.Unwrap<Contact>(list_contact[0]);
				contact.DuplicateFound = true;
				//Find duplicates view				
				PXView viewDuplicates = graph.Views.ToArray().First(v => v.Value.Cache.GetItemType() == typeof(CRDuplicateRecord)).Value;
				if (viewDuplicates == null)
					throw new PXException(Messages.DuplicateViewNotFound);
				viewDuplicates.Clear();
				List<object> duplicates = viewDuplicates.SelectMulti();
				contact = (Contact)view.Cache.CreateCopy(contact);
				string prevStatus = contact.DuplicateStatus;
				contact.DuplicateStatus = DuplicateStatusAttribute.Validated;
				Decimal? score = 0;
				contact.DuplicateFound = duplicates.Count > 0;
				foreach (PXResult<CRDuplicateRecord, Contact, Contact2> r in duplicates)
				{					
					Contact2 duplicate = r;
					CRDuplicateRecord contactScore = r;
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
				view.Cache.Update(contact);

				if (contact.DuplicateStatus == DuplicateStatusAttribute.PossibleDuplicated &&
						contact.ContactType == ContactTypesAttribute.Lead &&
						contact.Status == LeadStatusesAttribute.New &&
						Filter.CloseNoActivityLeads == true &&
						score > Filter.CloseThreshold)
				{
					CRActivity activity = PXSelect<CRActivity, 
						Where<CRActivity.contactID, Equal<Required<Contact.contactID>>>>.SelectWindowed(graph, 0, 1, contact.ContactID);

					if (activity == null)
					{
						PXAction action = graph.Actions["Action"];
						PXAdapter adapter = new PXAdapter(view);
						adapter.StartRow = 0;
						adapter.MaximumRows = 1;
						adapter.Searches = new object[] { contact.ContactID };
						adapter.Menu = Messages.CloseAsDuplicate;
						adapter.SortColumns = new[] { typeof(Contact.contactID).Name };
						foreach (Contact c in action.Press(adapter)) ;
						prevStatus = null;
					}
				}
				view.Cache.RestoreCopy(record, view.Cache.Current);
				if (prevStatus != contact.DuplicateStatus)
					graph.Actions.PressSave();			
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
		public PXFilteredProcessing<ContactAccount, ValidationFilter,					
				Where2<Where<Current<ValidationFilter.validationType>, Equal<True>,
									Or<ContactAccount.duplicateStatus, Equal<DuplicateStatusAttribute.notValidated>>>,				  									
					And<ContactAccount.isActive, Equal<True>,
					And<Where<ContactAccount.contactType, Equal<ContactTypesAttribute.lead>,
									Or<ContactAccount.contactType, Equal<ContactTypesAttribute.person>>>>>>> Contacts;	
	}

	[PXProjection(typeof(Select5<CRGrams, 
											InnerJoin<CRGrams2, 
												On<CRGrams.validationType, Equal<CRGrams2.validationType>,
													And<CRGrams.fieldName, Equal<CRGrams2.fieldName>,
													And<CRGrams.fieldValue, Equal<CRGrams2.fieldValue>>>>>,																																					
											Aggregate<GroupBy<CRGrams.entityID,
														GroupBy<CRGrams.validationType,
														GroupBy<CRGrams2.entityID,
														Sum<CRGrams.score>>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class CRDuplicateRecord : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		[PXUIEnabled(typeof(Where<Selector<CRDuplicateRecord.duplicateContactID, Contact.contactType>, NotEqual<ContactTypesAttribute.bAccountProperty>,
			And<Selector<CRDuplicateRecord.duplicateContactID, Contact.contactType>, NotEqual<ContactTypesAttribute.employee>>>))]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion		

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(CRGrams.entityID))]
		[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region EntityType
		public abstract class validationType : PX.Data.BQL.BqlString.Field<validationType> { }
		[PXDBString(2, BqlField = typeof(CRGrams.validationType), IsKey = true)]
		[PXUIField(DisplayName = "Entity Type")]		
		[ValidationTypes]
		public virtual String ValidationType { get; set; }
		#endregion

		#region DuplicateContactID
		public abstract class duplicateContactID : PX.Data.BQL.BqlInt.Field<duplicateContactID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(CRGrams2.entityID))]
		[PXUIField(DisplayName = "Duplicate Contact ID", Visibility = PXUIVisibility.Invisible)]
		[PXVirtualSelector(typeof(Contact.contactID))]
		public virtual Int32? DuplicateContactID { get; set; }
		#endregion		

		#region Score
		public abstract class score : PX.Data.BQL.BqlDecimal.Field<score> { }

		[PXDBDecimal(4, BqlField = typeof(CRGrams.score))]
		[PXDefault(TypeCode.Decimal, "1")]
		[PXUIField(DisplayName = "Score")]
		public virtual decimal? Score { get; set; }

		#endregion
	}

	[Serializable]
	[PXHidden]
	[PXVirtual]
	public partial class CRDuplicateRecordVirtual : IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		[PXUIEnabled(typeof(Where<Selector<CRDuplicateRecordVirtual.duplicateContactID, Contact.contactType>, NotEqual<ContactTypesAttribute.bAccountProperty>,
			And<Selector<CRDuplicateRecordVirtual.duplicateContactID, Contact.contactType>, NotEqual<ContactTypesAttribute.employee>>>))]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion		

		#region ContactID
		public abstract class contactID : IBqlField { }

		[PXDBInt(IsKey = true, BqlField = typeof(CRGrams.entityID))]
		[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region EntityType
		public abstract class validationType : IBqlField { }
		[PXDBString(2, BqlField = typeof(CRGrams.validationType), IsKey = true)]
		[PXUIField(DisplayName = "Entity Type")]		
		[ValidationTypes]
		public virtual String ValidationType { get; set; }
		#endregion

		#region DuplicateContactID
		public abstract class duplicateContactID : IBqlField { }

		[PXDBInt(IsKey = true, BqlField = typeof(CRGrams2.entityID))]
		[PXUIField(DisplayName = "Duplicate Contact ID", Visibility = PXUIVisibility.Invisible)]
		[PXVirtualSelector(typeof(Contact.contactID))]
		public virtual Int32? DuplicateContactID { get; set; }
		#endregion		

		#region Score
		public abstract class score : IBqlField { }

		[PXDBDecimal(4, BqlField = typeof(CRGrams.score))]
		[PXDefault(TypeCode.Decimal, "1")]
		[PXUIField(DisplayName = "Score")]
		public virtual decimal? Score { get; set; }

		#endregion
	}
}
