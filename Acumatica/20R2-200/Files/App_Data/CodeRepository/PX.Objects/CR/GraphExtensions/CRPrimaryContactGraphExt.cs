using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.SO;
using PX.SM;
using PX.Objects.CR.DAC;
using PX.Objects.CA;
using PX.Objects.CR.MassProcess;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRCreateActions;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.CR.Extensions.Relational;
using PX.CS.Contracts.Interfaces;
using PX.Objects.GDPR;

namespace PX.Objects.CR.Extensions
{
	#region DACs

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXHidden]
	[Serializable]
	public sealed class ContactExt : PXCacheExtension<Contact>
	{
		#region CanBeMadePrimary
		public abstract class canBeMadePrimary : PX.Data.BQL.BqlBool.Field<canBeMadePrimary> { }

		[PXBool]
		[PXUIField(DisplayName = "Can be made Primary", Visible = false, Visibility = PXUIVisibility.Invisible, Enabled = false)]
		//[PXFormula(typeof(True.When<Contact.isPrimary.IsEqual<False>>.Else<True>), Persistent = true, IsDirty = true)]
		[PXDependsOnFields(typeof(Contact.isPrimary), typeof(Contact.isActive))]
		public bool? CanBeMadePrimary =>
			Base.IsPrimary == false
			&& Base.IsActive == true;
		#endregion

		#region IsMeaningfull
		public abstract class isMeaningfull : PX.Data.BQL.BqlBool.Field<isMeaningfull> { }

		[PXBool]
		[PXDependsOnFields(
			typeof(Contact.firstName),
			typeof(Contact.lastName),
			typeof(Contact.salutation),
			typeof(Contact.eMail),
			typeof(Contact.phone1),
			typeof(Contact.phone2)
		)]
		[PXDBCalced(typeof(True), typeof(bool))]
		public bool? IsMeaningfull =>
			Base.FirstName != null
			|| Base.LastName != null
			|| Base.Salutation != null
			|| Base.EMail != null
			|| Base.Phone1 != null
			|| Base.Phone2 != null;
		#endregion
	}

	#endregion

	/// <exclude/>
	public abstract class CRPrimaryContactGraphExt<
			TGraph,
			TContactDetails,
			TMaster,
			FBAccountID,
			FPrimaryContactID> 
		: PXGraphExtension<TContactDetails, TGraph>
			where TGraph : PXGraph
			where TContactDetails : PXGraphExtension<TGraph>
			where TMaster : BAccount, IBqlTable, new()
			where FBAccountID : BqlInt.Field<FBAccountID>
			where FPrimaryContactID : BqlInt.Field<FPrimaryContactID>
	{
		#region Views

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.PrimaryContact)]
		public SelectFrom<Contact>
			.Where<
				Contact.bAccountID.IsEqual<BqlInt.Field<FBAccountID>.FromCurrent>
				.And<Contact.contactType.IsEqual<ContactTypesAttribute.person>>
				.And<Contact.contactID.IsEqual<BqlInt.Field<FPrimaryContactID>.FromCurrent>>>
			.View
			PrimaryContactCurrent;

		protected PXView NonDirtyContactsGrid = null;

		protected abstract PXView ContactsView { get; }

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected IEnumerable primaryContactCurrent()
		{
			Contact contact = null;

			var account = Base.Caches[typeof(TMaster)].Current as TMaster;

			if (account?.PrimaryContactID != null)
			{
				using (new PXReadDeletedScope())
				{
					contact = PrimaryContactCurrent.View.QuickSelect().FirstOrDefault_() as Contact;
				}
			}

			if (!Base.IsContractBasedAPI && !Base.IsImport)
			{
				if (contact == null
					&& account != null
					&& account.PrimaryContactID == null)
				{
					using (var r = new ReadOnlyScope(PrimaryContactCurrent.Cache, Base.Caches[typeof(TMaster)]))
					{
						contact = PrimaryContactCurrent.Insert();

						PrimaryContactCurrent.Cache.SetValue<Contact.contactType>(contact, ContactTypesAttribute.Person);
						PrimaryContactCurrent.Cache.SetValue<Contact.defAddressID>(contact, account.DefAddressID);
						PrimaryContactCurrent.Cache.SetValue<Contact.phone2Type>(contact, PhoneTypesAttribute.Cell);

						account.PrimaryContactID = contact.ContactID;

						var prevStatus = Base.Caches[typeof(TMaster)].GetStatus(account);

						Base.Caches[typeof(TMaster)].Update(account);

						Base.Caches[typeof(TMaster)].SetStatus(account, prevStatus == PXEntryStatus.Notchanged ? PXEntryStatus.Held : prevStatus);
					}
				}

				SetUI(account, contact);
			}

			yield return contact;
		}

		#endregion

		#region ctor

		public override void Initialize()
		{
			Base.Views[ContactsView.Name].WhereAnd<Where<ContactExt.isMeaningfull.IsEqual<True>>>();

			NonDirtyContactsGrid = new PXView(Base, true, Base.Views[ContactsView.Name].BqlSelect);
		}

		#endregion

		#region Actions

		public PXAction<TMaster> AddNewPrimaryContact;
		[PXUIField(DisplayName = Messages.AddNewContact, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable addNewPrimaryContact(PXAdapter adapter)
		{
			if (Base.Caches[typeof(TMaster)].Current is TMaster account && account != null)
			{
				ContactMaint target = PXGraph.CreateInstance<ContactMaint>();

				Contact maincontact = target.Contact.Insert();

				maincontact.BAccountID = account.BAccountID;

				CRContactClass ocls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>
					.SelectSingleBound(Base, new object[] { maincontact });
				if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
				{
					maincontact.WorkgroupID = account.WorkgroupID;
					maincontact.OwnerID = account.OwnerID;
				}

				maincontact = target.Contact.Update(maincontact);

				throw new PXRedirectRequiredException(target, true, "Contact") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		public PXAction<TMaster> MakeContactPrimary;
		[PXUIField(DisplayName = Messages.SetAsPrimary)]
		[PXButton]
		public virtual void makeContactPrimary()
		{
			var account = Base.Caches[typeof(TMaster)].Current as TMaster;
			if (account == null || account.BAccountID == null) return;

			var row = ContactsView.Cache.Current as Contact;
			if (row == null || row.ContactID == null) return;

			account.PrimaryContactID = row.ContactID;

			Base.Caches[typeof(TMaster)].Update(account);
		}

		#endregion

		#region Events

		protected virtual void _(Events.RowSelected<TMaster> e)
		{
			var row = e.Row as TMaster;
			if (row == null) return;

			var account = Base.Caches[typeof(TMaster)].Current;

			bool isContactsExists = NonDirtyContactsGrid?.SelectSingle() != null;

			PXUIFieldAttribute.SetVisible<FPrimaryContactID>(Base.Caches[typeof(TMaster)], account, isContactsExists);
		}

		protected virtual void _(Events.RowSelected<Contact> e)
		{
			var row = e.Row as Contact;
			if (row == null) return;

			// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Just for UI]
			row.IsPrimary = false;
			TMaster acct = Base.Caches[typeof(TMaster)].Current as TMaster;
			if (acct == null) return;

			if (row.ContactID == acct.PrimaryContactID)
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Just for UI]
				row.IsPrimary = true;
		}

		protected virtual void _(Events.RowDeleted<TMaster> e)
		{
			var row = e.Row as TMaster;
			if (row == null)
				return;

			if (e.Cache.GetStatus(row).IsIn(PXEntryStatus.Inserted, PXEntryStatus.InsertedDeleted))
				return;

			var primaryContact = PrimaryContactCurrent.SelectSingle();
			if (primaryContact == null || PrimaryContactCurrent.Cache.GetStatus(primaryContact) != PXEntryStatus.Inserted)
				return;

			PrimaryContactCurrent.Cache.Delete(primaryContact);
		}

		protected virtual void _(Events.FieldUpdated<FPrimaryContactID> e)
		{
			if (e.Row == null || e.OldValue == null) return;

			Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.SelectSingleBound(Base, null, e.OldValue);

			if (contact != null && ContactsView.Cache.GetStatus(contact) == PXEntryStatus.Inserted)
			{
				ContactsView.Cache.Delete(contact);
			}
		}

		protected virtual void _(Events.RowInserting<TMaster> e)
		{
			var row = e.Row;

			if (row == null)
				return;

			if (e.Cache.GetValue<FPrimaryContactID>(row) as int? < 0)
			{
				// It will be inserted properly in View delegate. No need to do it.
				// Plus, Cache.Extend will lead to PrimaryContactID < 0 here

				e.Cache.SetValue<FPrimaryContactID>(row, null);
			}
		}

		[PXOverride]
		public virtual void Persist(Action del)
		{
			var primaryContact = PrimaryContactCurrent.SelectSingle();

			if (primaryContact != null && PrimaryContactCurrent.Cache.GetStatus(primaryContact) == PXEntryStatus.Inserted)
			{
				var ext = PrimaryContactCurrent.Cache.GetExtension<ContactExt>(primaryContact);

				if (ext.IsMeaningfull != true)
				{
					PrimaryContactCurrent.Cache.Delete(primaryContact);

					Base.Caches[typeof(TMaster)].SetValue<FPrimaryContactID>(Base.Caches[typeof(TMaster)].Current, null);
				}
			}

			del();
		}

		#endregion

		#region Methods

		protected virtual void SetUI(TMaster account, Contact contact)
		{
			bool isRealContactExists = NonDirtyContactsGrid?.SelectSingle() != null;
			bool isRealContactSelected = (account?.PrimaryContactID > 0 && contact?.DeletedDatabaseRecord != true);
			bool isContactEditable = !isRealContactExists || isRealContactSelected;

			PXUIFieldAttribute.SetVisible<Contact.firstName>(PrimaryContactCurrent.Cache, contact, !isRealContactExists);
			PXUIFieldAttribute.SetVisible<Contact.lastName>(PrimaryContactCurrent.Cache, contact, !isRealContactExists);

			PXUIFieldAttribute.SetEnabled<Contact.salutation>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.eMail>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.phone1>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.phone1Type>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.phone2>(PrimaryContactCurrent.Cache, contact, isContactEditable);
			PXUIFieldAttribute.SetEnabled<Contact.phone2Type>(PrimaryContactCurrent.Cache, contact, isContactEditable);

			PXUIFieldAttribute.SetEnabled<Contact.consentAgreement>(PrimaryContactCurrent.Cache, contact, isRealContactSelected);
			PXUIFieldAttribute.SetEnabled<Contact.consentDate>(PrimaryContactCurrent.Cache, contact, isRealContactSelected);
			PXUIFieldAttribute.SetEnabled<Contact.consentExpirationDate>(PrimaryContactCurrent.Cache, contact, isRealContactSelected);
		}

		[PXOverride]
		public virtual void CopyPasteGetScript(bool isImportSimple, List<PX.Api.Models.Command> script, List<PX.Api.Models.Container> containers)
		{
			int primarycontactIDIndex = script.FindIndex(_ => _.FieldName == nameof(BAccount.PrimaryContactID));
			if (primarycontactIDIndex == -1)
				return;

			Api.Models.Command cmdPrimaryContactID = script[primarycontactIDIndex];
			Api.Models.Container cntPrimaryContactID = containers[primarycontactIDIndex];

			script.Remove(cmdPrimaryContactID);
			containers.Remove(cntPrimaryContactID);
		}

		#endregion
	}
}
