using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Common.Disposables;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public abstract partial class CRCreateAccountAction<TGraph, TMain>
		: CRCreateActionBase<
			TGraph,
			TMain,
			BusinessAccountMaint,
			BAccount,
			AccountsFilter,
			AccountConversionOptions>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Ctor

		protected override ICRValidationFilter[] AdditionalFilters => new[] { AccountInfoAttributes };

		public override IDisposable HoldCurrents()
		{
			var current = FilterInfo.Current;
			var attrs = AccountInfoAttributes.Cache.Updated.RowCast<PopupAttributes>().ToArray();
			return Disposable.Create(() =>
			{
				FilterInfo.Current = current;
				foreach (var at in attrs)
				{
					AccountInfoAttributes.Cache.SetStatus(at, PXEntryStatus.Updated);
				}
			});
		}

		#endregion

		#region Views

		public PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<Document.bAccountID>>>> ExistingAccount;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<AccountsFilter> AccountInfo;
		protected override CRValidationFilter<AccountsFilter> FilterInfo => AccountInfo;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupAttributes> AccountInfoAttributes;
		protected virtual IEnumerable accountInfoAttributes()
		{
			return GetFilledAttributes();
		}

		protected override IEnumerable<CSAnswers> GetAttributesForMasterEntity()
		{
			return ExistingAccount.SelectSingle() is BAccount account
				? PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<BAccount.noteID>>>>
						.Select(Base, account.NoteID).FirstTableItems
				: base.GetAttributesForMasterEntity();
		}

		#endregion

		#region Events

		protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.bAccountID> e)
		{
			var existing = ExistingAccount.SelectSingle();
			if (existing?.AcctCD != null)
			{
				e.NewValue = existing.AcctCD;
				return;
			}

			if (IsDimensionAutonumbered(Base, CustomerAttribute.DimensionName))
			{
				e.NewValue = GetDimensionAutonumberingNewValue(Base, CustomerAttribute.DimensionName);
			}
		}

		protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.accountName> e)
		{
			// HACK: for existing account it displayed empty for somereason
			if (ExistingAccount.SelectSingle()?.AcctName is string name)
			{
				e.NewValue = name;
				return;
			}

			var docContact = Contacts.Current ?? Contacts.SelectSingle();

			e.NewValue = docContact?.FullName;
		}

		protected virtual void _(Events.FieldVerifying<AccountsFilter, AccountsFilter.bAccountID> e)
		{
			if (ExistingAccount.SelectSingle() != null)
				return;

			BAccount existing = PXSelect<
					BAccount,
				Where<
					BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>
				.SelectSingleBound(Base, null, e.NewValue);

			if (existing != null)
			{
				AccountInfo.Cache.RaiseExceptionHandling<AccountsFilter.bAccountID>(e.Row, e.NewValue, new PXSetPropertyException(Messages.BAccountAlreadyExists, e.NewValue));
			}
			else
			{
				AccountInfo.Cache.RaiseExceptionHandling<AccountsFilter.bAccountID>(e.Row, e.NewValue, null);
			}
		}

		protected virtual void _(Events.FieldUpdated<AccountsFilter, AccountsFilter.accountClass> e)
		{
			Base.Caches<PopupAttributes>().Clear();
		}

		protected virtual void _(Events.RowSelected<AccountsFilter> e)
		{
			var existing = ExistingAccount.SelectSingle();
			PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, existing == null);
			PXUIFieldAttribute.SetEnabled<AccountsFilter.bAccountID>(e.Cache, e.Row,
				existing == null && !IsDimensionAutonumbered(Base, CustomerAttribute.DimensionName));
		}

		protected virtual void _(Events.RowSelected<Document> e)
		{
			var existing = ExistingAccount.SelectSingle();
			CreateBAccount.SetEnabled(existing == null);
			if(existing != null)
				AccountInfoAttributes.AllowUpdate = false;
		}

		#endregion

		protected virtual bool IsDimensionAutonumbered(PXGraph graph, string dimension)
		{
			return PXSelect<
					Segment, 
				Where<
					Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>
				.Select(graph, dimension)
				.RowCast<Segment>()
				.All(segment => segment.AutoNumber == true);
		}

		protected virtual string GetDimensionAutonumberingNewValue(PXGraph graph, string dimension)
		{
			Numbering numbering = (PXResult<Dimension, Numbering>) PXSelectJoin<
						Dimension,
					LeftJoin<Numbering, 
						On<Dimension.numberingID, Equal<Numbering.numberingID>>>,
					Where<
						Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>,
						And<Numbering.userNumbering, NotEqual<True>>>>
				.SelectSingleBound(graph, null, dimension);

			return numbering?.NewSymbol ?? Messages.New;
		}

		#region Actions

		public PXAction<TMain> CreateBAccount;
		[PXUIField(DisplayName = Messages.CreateAccount, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable createBAccount(PXAdapter adapter)
		{
			if (AskExtConvert(out bool redirect))
			{
				var processingGraph = Base.Clone();

				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateAccountAction<TGraph, TMain>>();

					var result = extension.Convert();

					if (redirect)
						Redirect(result);
				});
			}
			return adapter.Get();
		}

		public override ConversionResult<BAccount> Convert(AccountConversionOptions options = null)
		{
			// do nothing if account already exist
			if (ExistingAccount.SelectSingle() is BAccount account)
			{
				//PXTrace.WriteVerbose($"Using existing account: {account.AcctCD}.");
				return new ConversionResult<BAccount>
				{
					Converted = false,
					Entity = account,
				};
			}

			return base.Convert(options);
		}

		protected override BAccount CreateMaster(BusinessAccountMaint graph, AccountConversionOptions config)
		{
			var param = AccountInfo.Current;
			var document = Documents.Current;
			var docContact = Contacts.Current ?? Contacts.SelectSingle();
			var docAddress = Addresses.Current ?? Addresses.SelectSingle();

			object cd = param.BAccountID;
			graph.BAccount.Cache.RaiseFieldUpdating<BAccount.acctCD>(null, ref cd);

			BAccount account = graph.BAccount.Insert(new BAccount
			{
				AcctCD = (string)cd,
				AcctName = param.AccountName,
				Type = BAccountType.ProspectType,
				ParentBAccountID = document.ParentBAccountID,
				CampaignSourceID = document.CampaignID,
			});

			account.ClassID = param.AccountClass; // In case of (param.AccountClass == null) constructor fills ClassID with default value, so we have to set this directly.

			CRCustomerClass ocls = PXSelect<
						CRCustomerClass,
					Where<
						CRCustomerClass.cRCustomerClassID, Equal<Required<CRCustomerClass.cRCustomerClassID>>>>
				.SelectSingleBound(graph, null, account.ClassID);

			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				account.WorkgroupID = document.WorkgroupID;
				account.OwnerID = document.OwnerID;
			}

			account = graph.BAccount.Update(account);

			if (param.LinkContactToAccount == true)
			{
				// in case of opportunity
				Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<CROpportunity.contactID>>>>.Select(graph, document.RefContactID);
				if (contact != null)
				{
					graph.Answers.CopyAttributes(account, contact);
					contact.BAccountID = account.BAccountID;
					graph.Contacts.Update(contact);
				}
			}


			var defContact = graph.DefContact.SelectSingle()
				?? throw new InvalidOperationException("Cannot get Contact for Business Account."); // just to ensure
			MapContact(docContact, account, ref defContact);
			MapConsentable(docContact, defContact);
			defContact = graph.DefContact.Update(defContact);

			var defAddress = graph.AddressCurrent.SelectSingle()
				?? throw new InvalidOperationException("Cannot get Address for Business Account."); // just to ensure
			MapAddress(docAddress, account, ref defAddress);
			defAddress = graph.AddressCurrent.Update(defAddress);

			CR.Location location = graph.DefLocation.Select();
			location.DefAddressID = defAddress.AddressID;
			location.CTaxZoneID = document.TaxZoneID; // Saving tax zone before ReverseDocumentUpdate() removes it
			graph.DefLocation.Update(location);

			ReverseDocumentUpdate(graph, account);

			FillRelations(graph.Relations, account);

			FillAttributes(graph.Answers, account);

			TransferActivities(graph, account);

			// Copy Note text and Files references
			CRSetup setup = PXSetupOptional<CRSetup>.Select(graph);
			PXNoteAttribute.CopyNoteAndFiles(graph.Caches<TMain>(), GetMain(document), graph.CurrentBAccount.Cache, account, setup);

			return account;
		}

		protected override void ReverseDocumentUpdate(BusinessAccountMaint graph, BAccount entity)
		{
			var document = Documents.Current;
			Documents.Cache.SetValue<Document.bAccountID>(document, entity.BAccountID);
			Documents.Cache.SetValue<Document.locationID>(document, entity.DefLocationID);

			graph.Caches<TMain>().Update(GetMain(document));
		}


		protected virtual void MapContact(DocumentContact docContact, BAccount account, ref Contact contact)
		{
			base.MapContact(docContact, contact);
			contact.Title = null;
			contact.FirstName = null;
			contact.LastName = null;
			contact.ContactType = ContactTypesAttribute.BAccountProperty;
			contact.FullName = account.AcctName;
			contact.ContactID = account.DefContactID;
			contact.DefAddressID = account.DefAddressID;
			contact.BAccountID = account.BAccountID;
		}

		protected virtual void MapAddress(DocumentAddress docAddress, BAccount account, ref Address address)
		{
			base.MapAddress(docAddress, address);
		}

		protected virtual void TransferActivities(BusinessAccountMaint graph, BAccount account)
		{
			foreach (CRPMTimeActivity activity in Activities.Select())
			{
				activity.BAccountID = account.BAccountID;
				graph.Activities.Update(activity);
			}
		}

		#endregion
	}
}
