using PX.Common.Disposables;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public abstract partial class CRCreateContactAction<TGraph, TMain>
		: CRCreateActionBase<
			TGraph,
			TMain,
			ContactMaint,
			Contact,
			ContactFilter,
			ContactConversionOptions>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Ctor
		protected override string TargetType => CRTargetEntityType.Contact;

		protected override ICRValidationFilter[] AdditionalFilters => new[] { ContactInfoAttributes };

		public override IDisposable HoldCurrents()
		{
			var current = FilterInfo.Current;
			var attrs = ContactInfoAttributes.Cache.Updated.RowCast<PopupAttributes>().ToArray();
			return Disposable.Create(() =>
			{
				FilterInfo.Current = current;
				foreach (var at in attrs)
				{
					ContactInfoAttributes.Cache.SetStatus(at, PXEntryStatus.Updated);
				}
			});
		}

		#endregion

		#region Views

		#region Document Contact Method Mapping
		protected class DocumentContactMethodMapping : IBqlMapping
		{
			public Type Extension => typeof(DocumentContactMethod);
			protected Type _table;
			public Type Table => _table;

			public DocumentContactMethodMapping(Type table)
			{
				_table = table;
			}
			public Type Method = typeof(DocumentContactMethod.method);
			public Type NoFax = typeof(DocumentContactMethod.noFax);
			public Type NoMail = typeof(DocumentContactMethod.noMail);
			public Type NoMarketing = typeof(DocumentContactMethod.noMarketing);
			public Type NoCall = typeof(DocumentContactMethod.noCall);
			public Type NoEMail = typeof(DocumentContactMethod.noEMail);
			public Type NoMassMail = typeof(DocumentContactMethod.noMassMail);
		}
		protected abstract DocumentContactMethodMapping GetDocumentContactMethodMapping();
		#endregion

		public PXSelectExtension<DocumentContactMethod> ContactMethod;
		public PXSelect<Contact, Where<Contact.contactID, Equal<Current<Document.refContactID>>>> ExistingContact;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<ContactFilter> ContactInfo;
		protected override CRValidationFilter<ContactFilter> FilterInfo => ContactInfo;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupAttributes> ContactInfoAttributes;
		protected virtual IEnumerable contactInfoAttributes()
		{
			return GetFilledAttributes();
		}

		protected override IEnumerable<CSAnswers> GetAttributesForMasterEntity()
		{
			return ExistingContact.SelectSingle() is Contact contact
				? PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<Contact.noteID>>>>
						.Select(Base, contact.NoteID).FirstTableItems
				: base.GetAttributesForMasterEntity();
		}

		#endregion

		#region Events

		protected object GetDefaultFieldValueFromCache<TExistingField, TField>()
		{
			return ExistingContact.SelectSingle() is Contact existing
						&& existing.ContactID > 0
					? ExistingContact.Cache.GetValue(existing, typeof(TExistingField).Name)
					: Contacts.Cache.GetValue(Contacts.Current ?? Contacts.SelectSingle(), typeof(TField).Name);
		}

		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.firstName> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.firstName, DocumentContact.firstName>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.lastName> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.lastName, DocumentContact.lastName>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.salutation> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.salutation, DocumentContact.salutation>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.phone1> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.phone1, DocumentContact.phone1>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.phone1Type> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.phone1Type, DocumentContact.phone1Type>();
		}
		public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.email> e)
		{
			e.NewValue = GetDefaultFieldValueFromCache<Contact.eMail, DocumentContact.email>();
		}

		public virtual void _(Events.FieldUpdated<ContactFilter, ContactFilter.contactClass> e)
		{
			Base.Caches<PopupAttributes>().Clear();
		}

		public virtual void _(Events.RowSelected<ContactFilter> e)
		{
			var existing = ExistingContact.SelectSingle();
			PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, existing == null);
		}

		public virtual void _(Events.RowSelected<Document> e)
		{
			var existing = ExistingContact.SelectSingle();
			CreateContact.SetEnabled(existing == null);
			if (existing != null)
				ContactInfoAttributes.AllowUpdate = false;
		}

		#endregion

		#region Actions

		public PXAction<TMain> CreateContact;
		[PXUIField(DisplayName = Messages.CreateContact, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable createContact(PXAdapter adapter)
		{
			if (AskExtConvert(out bool redirect))
			{
				var processingGraph = Base.Clone();

				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateContactAction<TGraph, TMain>>();

					var result = extension.Convert();

					if (redirect)
						Redirect(result);
				});
			}
			return adapter.Get();
		}

		public override ConversionResult<Contact> Convert(ContactConversionOptions options = null)
		{
			// do nothing if account already exist
			if (ExistingContact.SelectSingle() is Contact contact)
			{
				//PXTrace.WriteVerbose($"Using existing contact: {contact.ContactID}.");
				return new ConversionResult<Contact>
				{
					Entity = contact,
					Converted = false,
				};
			}
			var result = base.Convert(options);

			FillDependedRelation(result.Entity, options);

			return result;
		}

		protected override Contact CreateMaster(ContactMaint graph, ContactConversionOptions _)
		{
			var entity = Documents.Current;
			var param = ContactInfo.Current;
			var docContact = Contacts.Current ?? Contacts.SelectSingle();
			var docContactMethod = ContactMethod.Current ?? ContactMethod.SelectSingle();
			var docAddress = Addresses.Current ?? Addresses.SelectSingle();

			Contact contact = new Contact
			{
				ContactType = ContactTypesAttribute.Person,
				ParentBAccountID = entity.ParentBAccountID
			};

			MapContact(docContact, contact);
			MapConsentable(docContact, contact);

			contact.FirstName = param.FirstName;
			contact.LastName = param.LastName;
			contact.Salutation = param.Salutation;
			contact.Phone1 = param.Phone1;
			contact.Phone1Type = param.Phone1Type;
			contact.EMail = param.Email;
			contact.ContactType = ContactTypesAttribute.Person;
			contact.ParentBAccountID = entity.ParentBAccountID;
			contact.BAccountID = entity.BAccountID;
			contact.Source = entity.Source;

			MapContactMethod(docContactMethod, contact);

			var address = (Address)graph.AddressCurrent.Cache.CreateInstance();
			address = graph.AddressCurrent.Insert(address);

			contact = graph.Contact.Insert(contact);

			contact.ClassID = param.ContactClass;

			CRContactClass cls = PXSelect<
						CRContactClass,
					Where<
						CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>
				.SelectSingleBound(graph, null, contact.ClassID);

			if (cls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				contact.WorkgroupID = entity.WorkgroupID;
				contact.OwnerID = entity.OwnerID;
			}

			MapAddress(docAddress, address);

			address = (Address)graph.AddressCurrent.Cache.Update(address);

			contact.DefAddressID = address.AddressID;

			contact = graph.Contact.Update(contact);

			ReverseDocumentUpdate(graph, contact);

			FillRelations(graph.Relations, contact);

			FillAttributes(graph.Answers, contact);

			TransferActivities(graph, contact);

			// Copy Note text and Files references
			CRSetup setup = PXSetupOptional<CRSetup>.Select(graph);
			PXNoteAttribute.CopyNoteAndFiles(graph.Caches<TMain>(), Documents.Cache.GetMain(entity), graph.Contact.Cache, contact, setup);

			return graph.Contact.Update(contact);
		}

		/// <summary>
		/// Updates CRRelation view of related Graph with new ContactID if needed
		/// </summary>
		/// <param name="contact"></param>
		/// <param name="options"></param>
		protected virtual void FillDependedRelation(Contact contact, ContactConversionOptions options)
		{
			if (options?.GraphWithRelation != null)
			{
				var relation = (CRRelation)options.GraphWithRelation.Caches<CRRelation>()?.Current;
				var entity = Documents.Current;
				if (relation != null &&
					relation.ContactID == null &&
					relation.TargetNoteID == entity.NoteID)
				{
					relation.ContactID = contact.ContactID;
					options.GraphWithRelation.Caches<CRRelation>().RaiseFieldUpdated<CRRelation.contactID>(relation, null);
					options.GraphWithRelation.Caches<CRRelation>().MarkUpdated(relation);
					options.GraphWithRelation.Actions.PressSave();
				}
			}
		}
		protected override void ReverseDocumentUpdate(ContactMaint graph, Contact entity)
		{
			// need for right update Documents
			//Base.Caches<Contact>().SetStatus(entity, PXEntryStatus.Inserted);

			var doc = Documents.Current;
			Documents.Cache.SetValue<Document.refContactID>(doc, entity.ContactID);
			graph.Caches<TMain>().Update(GetMain(doc));

			var contact = Contacts.Current ?? Contacts.SelectSingle();
			Contacts.Cache.SetValue<DocumentContact.firstName>(contact, entity.FirstName);
			Contacts.Cache.SetValue<DocumentContact.lastName>(contact, entity.LastName);
			Contacts.Cache.SetValue<DocumentContact.salutation>(contact, entity.Salutation);
			Contacts.Cache.SetValue<DocumentContact.phone1>(contact, entity.Phone1);
			Contacts.Cache.SetValue<DocumentContact.phone1Type>(contact, entity.Phone1Type);
			Contacts.Cache.SetValue<DocumentContact.email>(contact, entity.EMail);
			var contactMain = Contacts.Cache.GetMain(contact);
			graph.Caches[contactMain.GetType()].Update(contactMain);
		}

		protected virtual void TransferActivities(ContactMaint graph, Contact contact)
		{
			foreach (CRPMTimeActivity activity in Activities.Select())
			{
				activity.ContactID = contact.ContactID;
				graph.Activities.Update(activity);
			}
		}

		protected override IPersonalContact MapContact(DocumentContact docContact, IPersonalContact target)
		{
			base.MapContact(docContact, target);

			target.Title = null;

			return target;
		}

		protected virtual void MapContactMethod(DocumentContactMethod source, Contact target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.Method = source.Method;
			target.NoFax = source.NoFax;
			target.NoMail = source.NoMail;
			target.NoMarketing = source.NoMarketing;
			target.NoCall = source.NoCall;
			target.NoEMail = source.NoEMail;
			target.NoMassMail = source.NoMassMail;
		}

		#endregion
	}
}
