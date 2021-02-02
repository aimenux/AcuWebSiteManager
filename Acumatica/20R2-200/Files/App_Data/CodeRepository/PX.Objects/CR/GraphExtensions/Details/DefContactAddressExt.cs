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
	/// <summary>
	/// Extension that is used for selecting and defaulting the Default Address and Default Contact of the Business Account and it's inheritors.
	/// No Inserting of Contact and Address is implemented, as the Inserting is handled inside the <see cref="SharedChildOverrideGraphExt"/> graph extension.
	/// </summary>
	public abstract class DefContactAddressExt<TGraph, TMaster, FAcctName> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TMaster : BAccount, IBqlTable, new()
		where FAcctName : class, IBqlField
	{
		#region State

		protected virtual bool PersistentAddressValidation => false;

		#endregion

		#region Views

		[PXViewName(Messages.AccountAddress)]
		public PXSelect<
				Address,
			Where<
				Address.bAccountID, Equal<Current<BAccount.bAccountID>>,
				And<Address.addressID, Equal<Current<BAccount.defAddressID>>>>>
			DefAddress;

		[PXViewName(Messages.AccountContact)]
		[PXCopyPasteHiddenFields(typeof(Contact.duplicateStatus), typeof(Contact.duplicateFound))]
		public PXSelect<
				Contact,
			Where<
				Contact.bAccountID, Equal<Current<BAccount.bAccountID>>,
				And<Contact.contactID, Equal<Current<BAccount.defContactID>>>>>
			DefContact;

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(this.DefContact.Cache, null);

			PXUIFieldAttribute.SetVisible<Contact.languageID>(this.DefContact.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}

		#endregion

		#region Actions

		public PXAction<TMaster> ViewMainOnMap;
		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewMainOnMap(PXAdapter adapter)
		{
			if (this.DefAddress.SelectSingle() is Address addr)
			{
				BAccountUtility.ViewOnMap(addr);
			}

			return adapter.Get();
		}


		public PXAction<TMaster> ValidateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable validateAddresses(PXAdapter adapter)
		{
			var account = Base.Caches[typeof(TMaster)].Current as TMaster;

			if (account == null)
				return adapter.Get();

			bool needSave = false;

			if (PersistentAddressValidation)
				Base.Actions.PressSave();

			needSave = DoValidateAddresses();

			if (PersistentAddressValidation && needSave)
				Base.Actions.PressSave();

			return adapter.Get();
		}

		public virtual bool DoValidateAddresses()
		{
			Address address = this.DefAddress.SelectSingle();
			if (address != null && address.IsValidated == false)
			{
				if (PXAddressValidator.Validate<Address>(Base, address, true))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Events

		#region CacheAttached

		[PXDefault(ContactTypesAttribute.BAccountProperty)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.contactType> e) { }

		[PXDefault(PhoneTypesAttribute.Business2, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.phone2Type> e) { }

		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		#endregion

		#region Field-level

		protected virtual void _(Events.FieldUpdated<TMaster, FAcctName> e)
		{
			BAccount row = e.Row;

			if (this.DefContact.SelectSingle() is Contact defContact)
			{
				defContact.FullName = row.AcctName;

				this.DefContact.Update(defContact);
			}
		}

		protected virtual void _(Events.FieldUpdated<Address, Address.countryID> e)
		{
			Address addr = e.Row;

			if (!Base.IsContractBasedAPI && ((string)e.OldValue != addr.CountryID))
			{
				addr.State = null;
			}
		}

		#endregion

		#region Row-level

		protected virtual void _(Events.RowSelected<TMaster> e)
		{
			BAccount row = e.Row;
			if (row == null)
				return;

			ValidateAddresses.SetEnabled(e.Cache.GetStatus(row) != PXEntryStatus.Inserted);
		}

		protected virtual void _(Events.RowPersisting<Contact> e)
		{
			var row = e.Row;
			if (row == null || e.Operation != PXDBOperation.Update) return;

			var oldLang = (string)this.DefContact.Cache.GetValueOriginal<Contact.languageID>(row);
			if (oldLang == row.LanguageID)
				return;

			var account = Base.Caches[typeof(TMaster)].Current as TMaster;

			switch (account?.Type)
			{
				case BAccountType.CustomerType:

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
					PXDatabase.Update<AR.Customer>(
						new PXDataFieldAssign<AR.Customer.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AR.Customer.bAccountID>(account?.BAccountID));

					break;

				case BAccountType.VendorType:

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
					PXDatabase.Update<AP.Vendor>(
						new PXDataFieldAssign<AP.Vendor.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AP.Vendor.bAccountID>(account?.BAccountID));

					break;

				case BAccountType.CombinedType:

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
					PXDatabase.Update<AR.Customer>(
						new PXDataFieldAssign<AR.Customer.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AR.Customer.bAccountID>(account?.BAccountID));

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
					PXDatabase.Update<AP.Vendor>(
						new PXDataFieldAssign<AP.Vendor.localeName>(row.LanguageID),
						new PXDataFieldRestrict<AP.Vendor.bAccountID>(account?.BAccountID));

					break;

			}
		}

		#endregion

		#endregion

		#region Inner Classes

		public abstract class WithPersistentAddressValidation : DefContactAddressExt<TGraph, TMaster, FAcctName>
		{
			protected override bool PersistentAddressValidation => true;
		}

		public abstract class WithCombinedTypeValidation : DefContactAddressExt<TGraph, TMaster, FAcctName>
		{
			#region Events

			protected virtual void _(Events.RowDeleting<TMaster> e)
			{
				BAccount row = e.Row;
				if (row == null)
					return;

				if (row != null && (row.Type == BAccountType.CombinedType || row.IsBranch == true))
				{
					PXParentAttribute.SetLeaveChildren<Contact.bAccountID>(this.DefContact.Cache, null, true);
					PXParentAttribute.SetLeaveChildren<Address.bAccountID>(this.DefAddress.Cache, null, true);
				}
			}

			#endregion
		}

		#endregion
	}
}
