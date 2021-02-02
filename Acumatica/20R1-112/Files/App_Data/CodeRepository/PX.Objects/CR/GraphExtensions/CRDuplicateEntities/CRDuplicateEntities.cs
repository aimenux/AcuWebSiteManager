using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.MassProcess;
using PX.Objects.Common;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Api;
using FieldValue = PX.Data.MassProcess.FieldValue;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Common.Mail;
using System.Reflection;

namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	/// <summary>
	/// Extension that is used for deduplication purposes. Extension uses CRGrams mechanizm. Works with BAccount and Contact entities. 
	/// </summary>
	public abstract class CRDuplicateEntities<TGraph, TMain> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Views

		#region DocumentMapping Mapping
		protected class DocumentMapping : IBqlMapping
		{
			public Type Extension => typeof(Document);
			protected Type _table;
			public Type Table => _table;

			public DocumentMapping(Type table)
			{
				_table = table;
			}
			public Type Key = typeof(Document.key);
		}
		protected abstract DocumentMapping GetDocumentMapping();
		#endregion

		#region DuplicateDocumentMapping Mapping
		protected class DuplicateDocumentMapping : IBqlMapping
		{
			public Type Extension => typeof(DuplicateDocument);
			protected Type _table;
			public Type Table => _table;

			public DuplicateDocumentMapping(Type table)
			{
				_table = table;
			}
			public Type ContactID = typeof(DuplicateDocument.contactID);
			public Type RefContactID = typeof(DuplicateDocument.refContactID);
			public Type BAccountID = typeof(DuplicateDocument.bAccountID);
			public Type ContactType = typeof(DuplicateDocument.contactType);
			public Type DuplicateStatus = typeof(DuplicateDocument.duplicateStatus);
			public Type DuplicateFound = typeof(DuplicateDocument.duplicateFound);
			public Type Email = typeof(DuplicateDocument.email);
		}
		protected abstract DuplicateDocumentMapping GetDuplicateDocumentMapping();
		#endregion

		[PXHidden]
		public PXSelectExtension<Document> Documents;

		[PXHidden]
		public PXSelectExtension<DuplicateDocument> DuplicateDocuments;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<MergeParams> MergeParam;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSetup<CRSetup> Setup;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<FieldValue, Where<FieldValue.attributeID, IsNull>, OrderBy<Asc<FieldValue.order>>> PopupConflicts;
		public IEnumerable popupConflicts()
		{
			return Base.Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().Where(fld => fld.Hidden != true);
		}

		protected PXView dbView;

		//dummy, see delegate
		[PXOverride]
		public SelectFrom<
				CRDuplicateRecord>
			.LeftJoin<Contact>
				.On<Contact.contactID.IsEqual<CRDuplicateRecord.contactID>>
			.LeftJoin<DuplicateContact>
				.On<DuplicateContact.contactID.IsEqual<CRDuplicateRecord.duplicateContactID>>
			.LeftJoin<BAccountR>
				.On<BAccountR.bAccountID.IsEqual<DuplicateContact.bAccountID>>
			.LeftJoin<Standalone.CRLead>
				.On<Standalone.CRLead.contactID.IsEqual<CRDuplicateRecord.contactID>>
			.OrderBy<
				Asc<DuplicateContact.contactPriority>,
				Asc<DuplicateContact.contactID>>
			.View Duplicates;
		protected virtual IEnumerable duplicates()
		{
			var currentSetup = Base.Caches[typeof(CRSetup)].Current as CRSetup;
			if (currentSetup == null)
			{
				yield break;
			}

			var possibleDuplicates = dbView.SelectMulti();

			foreach (PXResult rec in possibleDuplicates)
			{
				CRGrams gram = rec.GetItem<CRGrams>();
				CRDuplicateGrams duplicateGram = rec.GetItem<CRDuplicateGrams>();
				DuplicateContact duplicateContact = rec.GetItem<DuplicateContact>();
				Standalone.CRLead duplicateLead = rec.GetItem<Standalone.CRLead>();

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

				CRDuplicateRecord cached = (CRDuplicateRecord)Base.Caches[typeof(CRDuplicateRecord)].Locate(dupRecord);
				if (cached == null)
				{
					Base.Caches[typeof(CRDuplicateRecord)].Hold(dupRecord);

					dupRecord.Selected = false;
				}
				else
				{
					dupRecord.Selected = cached.Selected;
				}

				yield return new PXResult<CRDuplicateRecord, Contact, DuplicateContact, BAccountR>(
					dupRecord,
					rec.GetItem<Contact>(),
					duplicateContact,
					rec.GetItem<BAccountR>());
			}
		}

		#endregion

		#region ctor

		protected virtual Type AdditionalConditions => typeof(True.IsEqual<True>);

		protected virtual string WarningMessage => "";

		protected static bool IsExtensionActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>();
		}

		public override void Initialize()
		{
			base.Initialize();

			PXDBAttributeAttribute.Activate(Base.Caches[typeof(TMain)]);

			Base.EnsureCachePersistence(typeof(CRActivityStatistics));

			BqlCommand bqlCommand = BqlTemplate.OfCommand<
					SelectFrom<
						CRGrams>
					.InnerJoin<CRDuplicateGrams>
						.On<CRDuplicateGrams.validationType.IsEqual<CRGrams.validationType>
						.And<CRDuplicateGrams.fieldName.IsEqual<CRGrams.fieldName>>
						.And<CRDuplicateGrams.fieldValue.IsEqual<CRGrams.fieldValue>>
						.And<CRDuplicateGrams.entityID.IsNotEqual<CRGrams.entityID>>>
					.InnerJoin<Contact>
						.On<Contact.contactID.IsEqual<CRGrams.entityID>>
					.InnerJoin<DuplicateContact>
						.On<DuplicateContact.contactID.IsEqual<CRDuplicateGrams.entityID>>
					.LeftJoin<BAccountR>
						.On<BAccountR.bAccountID.IsEqual<DuplicateContact.bAccountID>>
					.LeftJoin<Standalone.CRLead>
						.On<Standalone.CRLead.contactID.IsEqual<CRDuplicateGrams.entityID>>
					//.PlaceholderJoin<BqlPlaceholder.J>
					.Where<
						CRGrams.entityID.IsEqual<DuplicateDocument.contactID.FromCurrent>
						.And<DuplicateDocument.duplicateFound.FromCurrent.IsEqual<True>>
						.And<CRGrams.validationType.IsEqual<
							ValidationTypesAttribute.account
							.When<Contact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
								.And<DuplicateContact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>>>
							.Else<ValidationTypesAttribute.leadAccount>
							.When<Contact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
								.Or<DuplicateContact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>>>
							.Else<ValidationTypesAttribute.leadContact>>>
						.And<DuplicateContact.isActive.IsEqual<True>>
						.And<
							Brackets<DuplicateContact.contactType.IsNotEqual<ContactTypesAttribute.bAccountProperty>
							.Or<DuplicateContact.contactID.IsEqual<BAccountR.defContactID>>>>
						.And<
							Brackets<BqlPlaceholder.W>>
					>
					.AggregateTo<
						GroupBy<CRGrams.entityID>,
						GroupBy<CRGrams.validationType>,
						GroupBy<CRDuplicateGrams.entityID>,
						GroupBy<DuplicateContact.contactType>,
						Sum<CRGrams.score>>
					.Having<
						CRGrams.score.Summarized.IsGreaterEqual<CRSetup.leadToAccountValidationThreshold.FromCurrent>
							.And<Contact.contactType.Maximized.IsEqual<ContactTypesAttribute.bAccountProperty>>
						
						.Or<CRGrams.score.Summarized.IsGreaterEqual<CRSetup.leadToAccountValidationThreshold.FromCurrent>
							.And<DuplicateContact.contactType.Maximized.IsEqual<ContactTypesAttribute.bAccountProperty>>>

						.Or<CRGrams.score.Summarized.IsGreaterEqual<CRSetup.leadValidationThreshold.FromCurrent>>
							.And<Contact.contactType.Maximized.IsNotEqual<ContactTypesAttribute.bAccountProperty>>
							.And<DuplicateContact.contactType.Maximized.IsNotEqual<ContactTypesAttribute.bAccountProperty>>

						.Or<CRGrams.score.Summarized.IsGreaterEqual<CRSetup.accountValidationThreshold.FromCurrent>>
							.And<Contact.contactType.Maximized.IsEqual<ContactTypesAttribute.bAccountProperty>>
							.And<DuplicateContact.contactType.Maximized.IsEqual<ContactTypesAttribute.bAccountProperty>>
					>
					>
					//.Replace<BqlPlaceholder.J>(this.AdditionalConditions)
					.Replace<BqlPlaceholder.W>(this.AdditionalConditions)
					.ToCommand();

			dbView = new PXView(Base, false, bqlCommand);
		}

		#endregion

		#region Actions

		public PXAction<TMain> CheckForDuplicates;

		[PXUIField(DisplayName = Messages.CheckForDuplicates, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable checkForDuplicates(PXAdapter adapter)
		{
			DuplicateDocument duplicateDocument = DuplicateDocuments.Current ?? DuplicateDocuments.Select();

			if (duplicateDocument == null)
				return adapter.Get();

			var prevStatus = duplicateDocument.DuplicateStatus;
			var prevIsDirty = Base.Caches[DuplicateDocuments.Cache.GetMain(duplicateDocument).GetType()].IsDirty;

			if (CheckIsActive())
			{
				duplicateDocument.DuplicateFound = true;
				duplicateDocument = DuplicateDocuments.Update(duplicateDocument);

				Duplicates.View.Clear();
				var result = Duplicates.Select().ToList();

				bool anyFound = result != null && result.Count > 0;
				duplicateDocument.DuplicateFound = anyFound;
				duplicateDocument.DuplicateStatus = anyFound 
					? DuplicateStatusAttribute.PossibleDuplicated 
					: DuplicateStatusAttribute.Validated;

				duplicateDocument = DuplicateDocuments.Update(duplicateDocument);

				if (duplicateDocument.ContactType?.IsIn(ContactTypesAttribute.Lead, ContactTypesAttribute.Person) == true
					&& Setup.Current?.ContactEmailUnique == true
					&& !String.IsNullOrWhiteSpace(duplicateDocument.Email))
				{
					try
					{
						var parsedEmail = EmailParser.ParseAddresses(duplicateDocument.Email).FirstOrDefault();

						if (result
							.Select(item => item.GetItem<DuplicateContact>())
							.Where(item => !String.IsNullOrWhiteSpace(item.EMail) && item.ContactType.IsIn(ContactTypesAttribute.Lead, ContactTypesAttribute.Person))
							.Any(item => item.EMail.Contains(parsedEmail.Address)))
						{
							DuplicateDocuments.Cache.RaiseExceptionHandling<DuplicateDocument.email>(
								duplicateDocument,
								duplicateDocument.Email,
								new PXSetPropertyException(Messages.ContactEmailNotUnique, PXErrorLevel.Warning));
						}
					}
					catch (Exception) { }
				}
			}

			if (duplicateDocument.DuplicateStatus != prevStatus)
				Base.Actions.PressSave();
			else
			{
				Base.Caches[DuplicateDocuments.Cache.GetMain(duplicateDocument).GetType()].IsDirty = prevIsDirty;
			}

			if (duplicateDocument.DuplicateStatus == DuplicateStatusAttribute.PossibleDuplicated || duplicateDocument.DuplicateFound == true)
			{
				DuplicateDocuments.Cache.RaiseExceptionHandling<DuplicateDocument.duplicateStatus>(duplicateDocument,
					duplicateDocument.DuplicateStatus,
					new PXSetPropertyException(WarningMessage, PXErrorLevel.Warning));
			}
			else
			{
				DuplicateDocuments.Cache.RaiseExceptionHandling<DuplicateDocument.duplicateStatus>(duplicateDocument,
					duplicateDocument.DuplicateStatus,
					null);
			}

			return adapter.Get();
		}

		public PXAction<TMain> DuplicateMerge;
		[PXUIField(DisplayName = Messages.Merge, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void duplicateMerge()
		{
			List<TMain> duplicateEntities = null;

			bool res = MergeParam.AskExtFullyValid((graph, name) =>
			{
				Base.Caches<MergeParams>().Clear();
				Base.Caches<MergeParams>().Insert();

				duplicateEntities = new List<TMain>(PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(Base.Caches[typeof(MergeParams)], Base.Caches[typeof(MergeParams)].Current)
					.RowCast<TMain>()
					.Select(c => Base.Caches[typeof(TMain)].CreateCopy(c))
					.Cast<TMain>());

				if (duplicateEntities.Count < 2)
					throw new PXException(Messages.DuplicatesNotSelected);

				ValidateEntitiesBeforeMerge(duplicateEntities);

				FillPropertyValue();
			}, DialogAnswerType.Positive);

			if (res == false)
				return;

			if (duplicateEntities == null)
			{
				duplicateEntities = new List<TMain>(PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(Base.Caches[typeof(MergeParams)], Base.Caches[typeof(MergeParams)].Current)
					.RowCast<TMain>()
					.Select(c => Base.Caches[typeof(TMain)].CreateCopy(c))
					.Cast<TMain>());

			}

			int? targetID = ((MergeParams)Base.Caches[typeof(MergeParams)].Current).TargetEntityID;
			List<FieldValue> values = new List<FieldValue>(Base.Caches[typeof(FieldValue)].Cached.Cast<FieldValue>()
				.Select(v => Base.Caches[typeof(FieldValue)].CreateCopy(v))
				.Cast<FieldValue>());

			Base.Actions.PressSave();

			var processingGraph = Base.Clone();

			PXLongOperation.StartOperation(Base, () =>
			{
				var extension = processingGraph.GetProcessingExtension<CRDuplicateEntities<TGraph, TMain>>();

				extension.MergeDuplicates(
					(int)targetID,
					duplicateEntities,
					values,
					Base.IsContractBasedAPI);
			});
		}

		public PXAction<TMain> DuplicateAttach;
		[PXUIField(DisplayName = Messages.LinkToEntity, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void duplicateAttach()
		{
			DuplicateDocument duplicateDocument = DuplicateDocuments.Current;
			if (duplicateDocument == null)
				return;

			DoDuplicateAttach(duplicateDocument);

			if (Base.IsContractBasedAPI)
				Base.Actions.PressSave();
		}

		public PXAction<TMain> ViewDuplicate;
		[PXUIField(DisplayName = Messages.View, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void viewDuplicate()
		{
			Contact contact = PXSelect<Contact,
				Where<Contact.contactID, Equal<Current<CRDuplicateRecord.duplicateContactID>>>>.Select(Base);

			OpenEntityScreen(contact, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<TMain> ViewDuplicateRefContact;
		[PXUIField(DisplayName = Messages.View, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void viewDuplicateRefContact()
		{
			Contact contact = PXSelect<Contact,
				Where<Contact.contactID, Equal<Current<CRDuplicateRecord.duplicateRefContactID>>>>.Select(Base);

			OpenEntityScreen(contact, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<TMain> MarkAsValidated;
		[PXUIField(DisplayName = Messages.MarkAsValidated, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable markAsValidated(PXAdapter adapter)
		{
			DuplicateDocument duplicateDocument = DuplicateDocuments.Current;
			if (duplicateDocument == null)
				return adapter.Get();

			duplicateDocument.DuplicateStatus = DuplicateStatusAttribute.Validated;
			duplicateDocument.DuplicateFound = false;

			DuplicateDocuments.Update(duplicateDocument);

			Base.Actions.PressSave();

			return adapter.Get();
		}

		public PXAction<TMain> CloseAsDuplicate;
		[PXUIField(DisplayName = Messages.CloseAsDuplicate, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable closeAsDuplicate(PXAdapter adapter)
		{
			DuplicateDocument duplicateDocument = DuplicateDocuments.Current;
			if (duplicateDocument == null)
				return adapter.Get();

			duplicateDocument.DuplicateStatus = DuplicateStatusAttribute.Duplicated;

			DuplicateDocuments.Update(duplicateDocument);

			return adapter.Get();
		}

		#endregion

		#region Events

		[PXUIField(DisplayName = Messages.BAccountType, Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void _(Events.CacheAttached<BAccountR.type> e) { }

		public virtual void _(Events.FieldSelecting<FieldValue, FieldValue.value> e)
		{
			if (e.Row == null) return;

			e.ReturnState = InitValueFieldState(e.Row as FieldValue);
		}

		public virtual void _(Events.RowSelected<Document> e)
		{
			if (e.Row == null) return;

			DuplicateMerge.SetEnabled(e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted && Base.Caches[typeof(CRDuplicateRecord)].Cached.Cast<CRDuplicateRecord>().Any(_ => _.Selected == true));
		}

		public virtual void _(Events.RowSelected<DuplicateDocument> e)
		{
			if (e.Row == null) return;

			MarkAsValidated.SetEnabled(e.Row.IsActive == true && e.Row.DuplicateStatus != DuplicateStatusAttribute.Validated);
			CloseAsDuplicate.SetEnabled(e.Row.IsActive == true && e.Row.DuplicateStatus != DuplicateStatusAttribute.Duplicated);
		}

		public virtual void _(Events.RowSelected<CRDuplicateRecord> e)
		{
			e.Cache.IsDirty = false;
			if (e.Row == null) return;
			PXUIFieldAttribute.SetReadOnly<CRDuplicateRecord.duplicateRefContactID>(e.Cache, e.Row);
		}

		public virtual void _(Events.RowPersisting<CRDuplicateRecord> e)
		{
			e.Cancel = true;
		}

		public virtual void _(Events.RowPersisting<FieldValue> e)
		{
			e.Cancel = true;
		}

		public virtual void _(Events.FieldUpdated<MergeParams.targetEntityID> e)
		{
			if (e.NewValue == null || (e.OldValue as int?).Equals(e.NewValue as int?))
				return;

			FillPropertyValue();
		}

		protected virtual void _(Events.RowPersisted<DuplicateDocument> e)
		{
			DuplicateDocument row = e.Row as DuplicateDocument;
			if (row == null || e.TranStatus != PXTranStatus.Open)
				return;

			if (PersistGrams(row))
			{
				DuplicateDocuments.Cache.SetValue<DuplicateDocument.duplicateStatus>(row, row.DuplicateStatus);
				DuplicateDocuments.Cache.SetValue<DuplicateDocument.grammValidationDateTime>(row, row.GrammValidationDateTime);
			}
		}

		protected virtual bool PersistGrams(DuplicateDocument document)
		{
			if (document != null && document.ContactID > 1)
			{
				return new CRGramProcessor(Base).PersistGrams(document);
			}

			return false;
		}

		#endregion

		#region Fields logic

		public static Tuple<IEnumerable<string>, IEnumerable<string>> GetPossibleValues<T>(PXGraph graph, IEnumerable<T> entities, string fieldName)
			where T : IBqlTable
		{
			PXCache cache = graph.Caches[typeof(T)];

			return entities.Cast<object>()
				.Select(entity => cache.GetStateExt(entity, fieldName))
				.Cast<PXFieldState>()
				.Select(st =>
				{
					string value = null;
					string label = string.Empty;

					if (st != null)
					{
						var stringState = st as PXStringState;
						value = st.Value != null ? st.Value.ToString() : null;

						if (stringState != null && stringState.AllowedValues != null)
						{
							int i = Array.IndexOf(stringState.AllowedValues, value);
							label = (i == -1) ? value : stringState.AllowedLabels[i];
						}
					}

					return new[] { value != null ? new Tuple<string, string>(value, label) : new Tuple<string, string>(null, string.Empty) };
				})
				.SelectMany(z => z.Select(entry => entry))
				.GroupBy(z => z.Item1)
				.Select(g => new Tuple<string, string>(g.Key, g.First(z => z.Item1 == g.Key).Item2))
				.OrderBy(pair => pair.Item1)
				.UnZip();
		}

		protected PXFieldState InitValueFieldState(FieldValue field)
		{
			Tuple<IEnumerable<string>, IEnumerable<string>> possibleValues = new Tuple<IEnumerable<string>, IEnumerable<string>>(new string[0], new string[0]);

			List<TMain> entities = new List<TMain>(PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(Base.Caches[typeof(MergeParams)], Base.Caches[typeof(MergeParams)].Current).RowCast<TMain>());

			if (field.CacheName == typeof(TMain).FullName)
			{
				possibleValues = GetPossibleValues(Base, entities, field.Name);
			}
			else if (field.CacheName == typeof(Contact).FullName && field.CacheName != typeof(TMain).FullName)
			{
				PXSelectBase<Contact> cmd = new PXSelect<Contact>(Base);

				List<int?> defContactIDs = new List<int?>(entities.Select(entity => Base.Caches[typeof(TMain)].GetValue(entity, nameof(Document.DefContactID)) as int?).Where(c => c != null));
				foreach (int? c in defContactIDs)
				{
					cmd.WhereOr<Where<Contact.contactID, Equal<Required<BAccount.defContactID>>>>();
				}

				possibleValues = GetPossibleValues(Base, cmd.Select(defContactIDs.Cast<object>().ToArray()).RowCast<Contact>(), field.Name);
			}
			else if (field.CacheName == typeof(Address).FullName)
			{
				PXSelectBase<Address> cmd = new PXSelect<Address>(Base);

				List<int?> addressIDs = new List<int?>(entities.Select(entity => Base.Caches[typeof(TMain)].GetValue(entity, nameof(Document.DefAddressID)) as int?).Where(c => c != null));
				foreach (int? a in addressIDs)
				{
					cmd.WhereOr<Where<Address.addressID, Equal<Required<Address.addressID>>>>();
				}

				possibleValues = GetPossibleValues(Base, cmd.Select(addressIDs.Cast<object>().ToArray()).RowCast<Address>(), field.Name);
			}

			string[] values = possibleValues.Item1.ToArray();
			string[] labels = possibleValues.Item2.ToArray();

			return PXStringState.CreateInstance(field.Value, null, null, typeof(FieldValue.value).Name,
				false, 0, null, values, labels, null, null);
		}

		protected void InsertPropertyValue(FieldValue field, Dictionary<Type, object> targets)
		{
			Type t = Type.GetType(field.CacheName);
			PXCache cache = Base.Caches[t];
			object target = targets[t];

			PXStringState state = InitValueFieldState(field) as PXStringState;

			if (state != null)
			{
				if (state.AllowedValues == null || !state.AllowedValues.Any() || state.AllowedValues.Count() == 1 && field.AttributeID == null)
					return;

				if (state.AllowedValues.Count() == 1)
				{
					field.Hidden = true;
					field.Value = state.AllowedValues[0];
				}
				else if (target != null)
				{
					state.Required = true;

					object value = cache.GetValueExt(target, field.Name);

					if (value is PXFieldState)
						value = ((PXFieldState)value).Value;

					field.Value = value != null ? value.ToString() : null;
				}
			}

			Base.Caches[typeof(FieldValue)].Insert(field);
		}

		protected void FillPropertyValue()
		{
			PXCache cache = Base.Caches[typeof(FieldValue)];
			cache.Clear();

			PXCache<MergeParams> pcache = Base.Caches<MergeParams>();

			List<FieldValue> values = new List<FieldValue>();
			HashSet<string> fieldNames = new HashSet<string>();

			GetAllProperties(values, fieldNames);

			TMain targetEntity = GetTargetEntity(((MergeParams)pcache.Current).TargetEntityID ?? 0);

			Contact targetContact = GetTargetContact(targetEntity);
			Address targetAddress = GetTargetAddress(targetEntity);

			Dictionary<Type, object> targets = new Dictionary<Type, object>();

			targets[typeof(TMain)] = targetEntity;
			targets[typeof(Contact)] = targetContact;
			targets[typeof(Address)] = targetAddress;

			foreach (FieldValue fieldValue in values)
			{
				InsertPropertyValue(fieldValue, targets);
			}
			cache.IsDirty = false;
		}
		#endregion

		#region Implementation

		protected void MergeDuplicates(int targetID, List<TMain> duplicateEntities, List<FieldValue> values, bool IsContractBasedAPI)
		{
			TMain targetEntity = GetTargetEntity(targetID);

			object realTargetEntity = targetEntity;
			var graphType = new EntityHelper(Base).GetPrimaryGraphType(ref realTargetEntity, false);

			if (graphType == null)
				return;

			PXGraph targetGraph = PXGraph.CreateInstance(graphType);

			PXCache cache = targetGraph.Caches[typeof(TMain)];

			var refNoteIdField = EntityHelper.GetNoteField(cache.GetItemType());

			realTargetEntity = cache.CreateCopy(realTargetEntity);

			Contact targetContact = GetTargetContact(targetEntity);
			Address targetAddress = GetTargetAddress(targetEntity);

			Dictionary<Type, object> targets = new Dictionary<Type, object>
			{
				[typeof(TMain)] = realTargetEntity,
				[typeof(Contact)] = targetContact,
				[typeof(Address)] = targetAddress
			};

			foreach (FieldValue fieldValue in values)
			{
				if (fieldValue.AttributeID == null)
				{
					Type type = Type.GetType(fieldValue.CacheName);
					PXFieldState state = (PXFieldState)targetGraph.Caches[type].GetStateExt(targets[type], fieldValue.Name);
					if (state == null || !Equals(state.Value, fieldValue.Value))
					{
						targetGraph.Caches[type].SetValueExt(targets[type], fieldValue.Name, fieldValue.Value);
						targets[type] = targetGraph.Caches[type].CreateCopy(targetGraph.Caches[type].Update(targets[type]));
					}
				}
				else
				{
					PXCache attrCache = cache.Graph.Caches[typeof(CSAnswers)];
					CSAnswers attr = new CSAnswers
					{
						AttributeID = fieldValue.AttributeID,
						RefNoteID = cache.GetValue(targetEntity, refNoteIdField) as Guid?,
						Value = fieldValue.Value,
					};

					Dictionary<string, object> keys = new Dictionary<string, object>();
					foreach (string key in attrCache.Keys.ToArray())
					{
						keys[key] = attrCache.GetValue(attr, key);
					}


					if (attrCache.Locate(keys) == 0)
						attrCache.Insert(attr);
					else
					{
						var located = attrCache.Locate(attr) as CSAnswers;
						located.Value = attr.Value;
						attrCache.Update(located);
					}
				}
			}

			PXPrimaryGraphCollection primaryGraph = new PXPrimaryGraphCollection(targetGraph);

			using (PXTransactionScope scope = new PXTransactionScope())
			{
				foreach (TMain duplicateEntity in duplicateEntities)
				{
					// only int, only single field
					if (cache.GetValue(duplicateEntity, cache.Keys[0]) as int? == targetID)
						continue;

					targetGraph.Caches[realTargetEntity.GetType()].Current = realTargetEntity;

					MergeEntities(targetGraph, realTargetEntity as TMain, duplicateEntity);

					targetGraph.Actions.PressSave();

					PXGraph operGraph = primaryGraph[duplicateEntity];

					RunActionWithAppliedAutomation(operGraph, duplicateEntity, nameof(CloseAsDuplicate));

					operGraph.Actions.PressSave();
				}

				scope.Complete();
			}

			// should become validated if no possible duplicates
			Base.Views[Base.PrimaryView].Cache.Current = targetEntity;

			Base.Actions.PressCancel();

			RunActionWithAppliedAutomation(Base, targetEntity, nameof(CheckForDuplicates));

			if (!IsContractBasedAPI)
			{
				RunActionWithAppliedAutomation(targetGraph, realTargetEntity, "Cancel");

				throw new PXRedirectRequiredException(targetGraph, "");
			}
		}

		protected abstract void MergeEntities(PXGraph targetGraph, TMain targetEntity, TMain duplicateEntity);

		protected abstract TMain GetTargetEntity(int targetID);
		protected abstract Contact GetTargetContact(TMain targetEntity);
		protected abstract Address GetTargetAddress(TMain targetEntity);

		protected abstract void DoDuplicateAttach(DuplicateDocument duplicateDocument);

		protected virtual void ValidateEntitiesBeforeMerge(List<TMain> duplicateEntities) { }

		protected virtual bool CheckIsActive()
		{
			DuplicateDocument duplicateDocument = DuplicateDocuments.Current ?? DuplicateDocuments.Select();

			if (duplicateDocument == null)
				return false;

			return duplicateDocument.IsActive == true;
		}

		public static IEnumerable<FieldValue> GetMarkedPropertiesOf<TPrimary>(PXGraph graph, ref int firstSortOrder)
			where TPrimary : class, IBqlTable, IPXSelectable, new()
		{
			PXCache cache = graph.Caches[typeof(TPrimary)];
			int order = firstSortOrder;
			List<FieldValue> res = (cache.Fields.Where(
					fieldname => cache.GetAttributesReadonly(fieldname).OfType<PXMassMergableFieldAttribute>().Any())
				.Select(fieldname => new { fieldname, state = cache.GetStateExt(null, fieldname) as PXFieldState })
				.Where(@t => @t.state != null)
				.Select(@t => new FieldValue()
				{
					Selected = false,
					CacheName = typeof(TPrimary).FullName,
					Name = @t.fieldname,
					DisplayName = @t.state.DisplayName,
					AttributeID = null,
					Order = order++
				})).ToList();

			firstSortOrder = order;
			return res;
		}

		public static IEnumerable<FieldValue> GetAttributeProperties(PXGraph graph, ref int firstSortOrder, List<string> suffixes)
		{
			int order = firstSortOrder;

			GetAttributeSuffixes(graph, ref suffixes);

			List<FieldValue> res = new List<FieldValue>();
			PXCache cache = graph.Caches[typeof(TMain)];

			foreach (string field in cache.Fields)
			{
				if (!suffixes.Any(suffix => field.EndsWith(string.Format("_{0}", suffix))))
					continue;

				PXFieldState state = cache.GetStateExt(null, field) as PXFieldState;

				if (state == null)
					continue;

				string displayName = state.DisplayName;
				string attrID = field;
				string local = field;

				foreach (string suffix in suffixes.Where(suffix => local.EndsWith(string.Format("_{0}", suffix))))
				{
					attrID = field.Replace(string.Format("_{0}", suffix), string.Empty);
					displayName = state.DisplayName.Replace(string.Format("${0}$-", suffix), string.Empty);
					break;
				}

				res.Add(new FieldValue
				{
					Selected = false,
					CacheName = typeof(TMain).FullName,
					Name = field,
					DisplayName = displayName,
					AttributeID = attrID,
					Order = order++ + 1000
				});
			}

			firstSortOrder = order;
			return res;
		}

		protected static void GetAttributeSuffixes(PXGraph graph, ref List<string> suffixes)
		{
			suffixes = suffixes 
				?? new List<string>(graph.Caches[typeof(TMain)].BqlTable
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.SelectMany(p => p.GetCustomAttributes(true).Where(atr => atr is PXDBAttributeAttribute), (p, atr) => p.Name));
		}

		public virtual void GetAllProperties(List<FieldValue> values, HashSet<string> fieldNames)
		{
			int order = 0;

			values.AddRange(
				GetMarkedPropertiesOf<Address>(Base, ref order)
					.Union(GetAttributeProperties(Base, ref order, null))
					.Where(fld => fieldNames.Add(fld.Name)));
		}

		// TODO: rework on Workflow implementation
		internal static object RunActionWithAppliedAutomation(PXGraph graph, object entity, string menu)
		{
			graph.Views[graph.PrimaryView].Cache.Current = entity;

			List<object> searches = new List<object>();
			List<string> sorts = new List<string>();

			foreach (string key in graph.Views[graph.PrimaryView].Cache.Keys)
			{
				searches.Add(graph.Views[graph.PrimaryView].Cache.GetValue(entity, key));
				sorts.Add(key);
			}

			PXAdapter a = new PXAdapter(graph.Views[graph.PrimaryView])
			{
				StartRow = 0,
				MaximumRows = 1,
				Searches = searches.ToArray(),
				Menu = menu,
				SortColumns = sorts.ToArray()
			};

			if (graph.Actions.Contains("Action"))
			{
				// set automation step before running automation action. Workflow should work the same way
				PXAutomation.GetStep(graph, new List<object> { entity }, a.View.BqlSelect);

				foreach (var c in graph.Actions["Action"].Press(a))
				{
					return c;
				}
			}

			return null;
		}

		private void OpenEntityScreen(IBqlTable entity, PXRedirectHelper.WindowMode windowMode)
		{
			if (entity == null)
				return;

			PXPrimaryGraphCollection primaryGraph = new PXPrimaryGraphCollection(Base);

			PXRedirectHelper.TryRedirect(primaryGraph[entity], entity, windowMode);
		}

		#endregion
	}
}
