using System;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	/// <exclude/>
	public class CRDuplicateLeadsSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly Type SourceEntityID;

		public CRDuplicateLeadsSelectorAttribute(Type sourceEntityID)
			: base(typeof(
				SelectFrom<
					CRLead>
				.InnerJoin<CRDuplicateGrams>
					.On<CRDuplicateGrams.entityID.IsEqual<CRLead.contactID>>
				.InnerJoin<CRGrams>
					.On<CRGrams.validationType.IsEqual<CRDuplicateGrams.validationType>
					.And<CRGrams.fieldName.IsEqual<CRDuplicateGrams.fieldName>
					.And<CRGrams.fieldValue.IsEqual<CRDuplicateGrams.fieldValue>>>>
				.Where<
					CRGrams.entityID.IsEqual<CRLead.contactID.AsOptional>
					.And<CRDuplicateGrams.validationType.IsEqual<ValidationTypesAttribute.leadContact>>>
				.AggregateTo<
					GroupBy<CRDuplicateGrams.entityID>,
					GroupBy<CRDuplicateGrams.validationType>,
					GroupBy<CRDuplicateGrams.entityID>,
					Sum<CRDuplicateGrams.score>>
				.Having<
					CRDuplicateGrams.score.Summarized.IsGreaterEqual<CRSetup.leadValidationThreshold.FromCurrent>>
				.SearchFor<
					CRLead.contactID>),

				fieldList: new[]
				{
					typeof(CRLead.displayName),
					typeof(CRLead.salutation),
					typeof(CRLead.eMail),
					typeof(CRLead.phone1)
				}
			)
		{
			DirtyRead = true;
			DescriptionField = typeof(CRLead.displayName);
			this.SourceEntityID = sourceEntityID;
			ValidateValue = false;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			View = new PXView(_Graph, true, _Select);
		}

		private PXView View;

		public IEnumerable GetRecords()
		{
			PXCache sourceCache = _Graph.Caches[SourceEntityID.DeclaringType];
			var currentEntity = PXView.Currents?.FirstOrDefault() ?? sourceCache.Current;
			if (currentEntity == null)
				yield break;

			int? currentID = (int?)sourceCache.GetValue(currentEntity, SourceEntityID.Name);

			foreach (PXResult rec in View.SelectMulti(currentID))
			{
				CRGrams gram = rec.GetItem<CRGrams>();
				CRDuplicateGrams duplicateGram = rec.GetItem<CRDuplicateGrams>();
				CRLead duplicateLead = rec.GetItem<CRLead>();

				var dupRecord = new CRDuplicateRecord()
				{
					ContactID = gram.EntityID,
					ValidationType = gram.ValidationType,
					DuplicateContactID = duplicateGram.EntityID,
					Score = gram.Score,
					DuplicateContactType = duplicateLead?.ContactType,
					DuplicateBAccountID = duplicateLead?.BAccountID,
					DuplicateRefContactID = duplicateLead?.RefContactID,
				};

				CRDuplicateRecord cached = (CRDuplicateRecord)_Graph.Caches[typeof(CRDuplicateRecord)].Locate(dupRecord);

				if (cached?.Selected == true || duplicateLead.ContactID == currentID)
					yield return duplicateLead;
			}
		}
	}

	/// <exclude/>
	public class CRDuplicateContactsSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly Type SourceEntityID;

		public CRDuplicateContactsSelectorAttribute(Type sourceEntityID)
			: base(typeof(
				SelectFrom<
					Contact>
				.InnerJoin<CRDuplicateGrams>
					.On<CRDuplicateGrams.entityID.IsEqual<Contact.contactID>>
				.InnerJoin<CRGrams>
					.On<CRGrams.validationType.IsEqual<CRDuplicateGrams.validationType>
					.And<CRGrams.fieldName.IsEqual<CRDuplicateGrams.fieldName>
					.And<CRGrams.fieldValue.IsEqual<CRDuplicateGrams.fieldValue>>>>
				.Where<
					CRGrams.entityID.IsEqual<Contact.contactID.AsOptional>
					.And<CRDuplicateGrams.validationType.IsEqual<ValidationTypesAttribute.leadContact>>>
				.AggregateTo<
					GroupBy<CRDuplicateGrams.entityID>,
					GroupBy<CRDuplicateGrams.validationType>,
					GroupBy<CRDuplicateGrams.entityID>,
					Sum<CRDuplicateGrams.score>>
				.Having<
					CRDuplicateGrams.score.Summarized.IsGreaterEqual<CRSetup.leadValidationThreshold.FromCurrent>>
				.SearchFor<
					Contact.contactID>),

				fieldList: new[]
				{
					typeof(Contact.isActive),
					typeof(Contact.displayName),
					typeof(Contact.contactType),
					typeof(Contact.salutation),
					typeof(Contact.eMail),
					typeof(Contact.phone1)
				}
			)
		{
			DirtyRead = true;
			DescriptionField = typeof(Contact.displayName);
			this.SourceEntityID = sourceEntityID;
			ValidateValue = false;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			View = new PXView(_Graph, true, _Select);
		}

		private PXView View;

		public IEnumerable GetRecords()
		{
			PXCache sourceCache = _Graph.Caches[SourceEntityID.DeclaringType];
			var currentEntity = PXView.Currents?.FirstOrDefault() ?? sourceCache.Current;
			if (currentEntity == null)
				yield break;

			int? currentID = (int?)sourceCache.GetValue(currentEntity, SourceEntityID.Name);

			foreach (PXResult rec in View.SelectMulti(currentID))
			{
				CRGrams gram = rec.GetItem<CRGrams>();
				CRDuplicateGrams duplicateGram = rec.GetItem<CRDuplicateGrams>();
				Contact duplicateContact = rec.GetItem<Contact>();
				CRLead duplicateLead = rec.GetItem<CRLead>();

				var dupRecord = new CRDuplicateRecord()
				{
					ContactID = gram.EntityID,
					ValidationType = gram.ValidationType,
					DuplicateContactID = duplicateGram.EntityID,
					Score = gram.Score,
					DuplicateContactType = duplicateContact?.ContactType,
					DuplicateBAccountID = duplicateContact?.BAccountID,
					DuplicateRefContactID = duplicateLead?.RefContactID,
				};

				CRDuplicateRecord cached = (CRDuplicateRecord)_Graph.Caches[typeof(CRDuplicateRecord)].Locate(dupRecord);

				if (cached?.Selected == true || duplicateContact.ContactID == currentID)
					yield return duplicateContact;
			}
		}
	}

	/// <exclude/>
	public class CRDuplicateBAccountSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly Type SourceEntityID;

		public CRDuplicateBAccountSelectorAttribute(Type sourceEntityID)
			: base(typeof(
				SelectFrom<
					BAccountR>
				.InnerJoin<CRDuplicateGrams>
					.On<CRDuplicateGrams.entityID.IsEqual<BAccountR.defContactID>>
				.InnerJoin<CRGrams>
					.On<CRGrams.validationType.IsEqual<CRDuplicateGrams.validationType>
					.And<CRGrams.fieldName.IsEqual<CRDuplicateGrams.fieldName>
					.And<CRGrams.fieldValue.IsEqual<CRDuplicateGrams.fieldValue>>>>
				.Where<
					CRGrams.entityID.IsEqual<BAccountR.defContactID.AsOptional>
					.And<CRDuplicateGrams.validationType.IsEqual<ValidationTypesAttribute.account>>>
				.AggregateTo<
					GroupBy<CRDuplicateGrams.entityID>,
					GroupBy<CRDuplicateGrams.validationType>,
					GroupBy<CRDuplicateGrams.entityID>,
					Sum<CRDuplicateGrams.score>>
				.Having<
					CRDuplicateGrams.score.Summarized.IsGreaterEqual<CRSetup.accountValidationThreshold.FromCurrent>>
				.SearchFor<
					BAccountR.bAccountID>),

				fieldList: new[]
				{
					typeof(BAccountR.acctCD),
					typeof(BAccountR.acctName),
					typeof(BAccountR.status),
					typeof(BAccountR.type),
					typeof(BAccountR.classID)
				}
			)
		{
			this.SourceEntityID = sourceEntityID;

			DirtyRead = true;
			DescriptionField = typeof(BAccountR.acctName);
			SubstituteKey = typeof(BAccountR.acctCD);
			ValidateValue = false;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			View = new PXView(_Graph, true, _Select);
		}

		private PXView View;

		public IEnumerable GetRecords()
		{
			PXCache sourceCache = _Graph.Caches[SourceEntityID.DeclaringType];
			var currentEntity = PXView.Currents?.FirstOrDefault() ?? sourceCache.Current;
			if (currentEntity == null)
				yield break;

			int? currentID = (int?)sourceCache.GetValue(currentEntity, SourceEntityID.Name);

			foreach (PXResult rec in View.SelectMulti(currentID))
			{
				CRGrams gram = rec.GetItem<CRGrams>();
				CRDuplicateGrams duplicateGram = rec.GetItem<CRDuplicateGrams>();
				BAccountR duplicateAccount = rec.GetItem<BAccountR>();
				CRLead duplicateLead = rec.GetItem<CRLead>();

				var dupRecord = new CRDuplicateRecord()
				{
					ContactID = gram.EntityID,
					ValidationType = gram.ValidationType,
					DuplicateContactID = duplicateGram.EntityID,
					Score = gram.Score,
					DuplicateContactType = ContactTypesAttribute.BAccountProperty,
					DuplicateBAccountID = duplicateAccount?.BAccountID,
					DuplicateRefContactID = duplicateLead?.RefContactID,
				};

				CRDuplicateRecord cached = (CRDuplicateRecord)_Graph.Caches[typeof(CRDuplicateRecord)].Locate(dupRecord);

				if (cached?.Selected == true || duplicateAccount.DefContactID == currentID)
					yield return duplicateAccount;
			}
		}
	}
}
