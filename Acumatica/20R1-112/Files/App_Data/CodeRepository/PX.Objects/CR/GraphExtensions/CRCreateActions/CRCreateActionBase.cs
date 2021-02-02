using PX.Data;
using PX.Data.MassProcess;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.CS.Contracts.Interfaces;
using PX.Objects.GDPR;
using PX.Common;
using PX.Common.Disposables;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[PXInternalUseOnly]
	public abstract class CRCreateActionBase<TGraph, TMain, TTargetGraph, TTarget, TFilter, TConversionOptions> : CRCreateActionBaseInit<TGraph, TMain>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
		where TTargetGraph : PXGraph, new()
		where TTarget : class, IBqlTable, INotable, new()
		where TFilter : class, IBqlTable, IClassIdFilter, new()
		where TConversionOptions : ConversionOptions<TTargetGraph, TTarget>
	{
		#region State

		protected virtual string TargetType => CRTargetEntityType.Contact;

		private CRPopupValidator.Generic<TFilter> _popupValidator;
		public virtual CRPopupValidator.Generic<TFilter> PopupValidator => _popupValidator
			?? (_popupValidator = CRPopupValidator.Create(FilterInfo, AdditionalFilters));

		protected virtual ICRValidationFilter[] AdditionalFilters { get; }

		#endregion

		#region Views

		protected abstract CRValidationFilter<TFilter> FilterInfo { get; }
		protected abstract PXSelectBase<CRPMTimeActivity> Activities { get; }

		#endregion

		#region Attributes

		public virtual void _(Events.FieldVerifying<PopupAttributes, PopupAttributes.displayName> e)
		{
			if (e.Row?.Value == null)
			{
				throw new PXSetPropertyException(Messages.FillReqiredAttributes, PXErrorLevel.Error);
			}
		}

		// specified attributes
		public virtual IEnumerable<PopupAttributes> GetFilledAttributes()
		{
			PXCache cache = Base.Caches[typeof(PopupAttributes)];

			foreach (var field in GetPreparedAttributes())
			{
				var item = (PopupAttributes)cache.Locate(field);

				if (item == null)
					cache.Hold(field);

				yield return item ?? field;
			}
		}

		// attributes from class (and prepared entity)
		protected virtual IEnumerable<PopupAttributes> GetPreparedAttributes()
		{
			var master = GetAttributesForMasterEntity().Where(a => a.Value != null).ToList();

			return CRAttribute
				.EntityAttributes(typeof(TTarget), FilterInfo.Current?.ClassID)
				.Where(a => a.Required)
				.Select(a => (entity: a, master: master.FirstOrDefault(_a => _a.AttributeID == a.ID)))
				.Select((a, i) => new PopupAttributes
				{
					Selected = false,
					CacheName = typeof(TTarget).FullName,
					Name = a.entity.ID + "_Attributes",
					DisplayName = a.entity.Description,
					AttributeID = a.entity.ID,
					Value = a.master?.Value ?? a.entity.DefaultValue,
					Order = i,
					Required = true
				});
		}

		protected virtual IEnumerable<CSAnswers> GetAttributesForMasterEntity()
		{
			// view from base lead
			return Base
				.Views[nameof(ContactMaint.Answers)]
				.SelectMulti()
				.RowCast<CSAnswers>();
		}

		public virtual void _(Events.FieldSelecting<PopupAttributes, PopupAttributes.value> e)
		{
			var row = e.Row as PopupAttributes;
			if (row == null || !typeof(TTarget).FullName.Equals(row.CacheName))
				return;

			PXDBAttributeAttribute.Activate(Base.Caches[typeof(TTarget)]);

			e.ReturnState = PXMassProcessHelper.InitValueFieldState(Base.Caches[typeof(TTarget)], e.Row as FieldValue);
			e.Cancel = true;
		}

		#endregion

		#region Conversion Options

		public virtual IDisposable HoldCurrents()
		{
			var current = FilterInfo.Current;
			return Disposable.Create(() =>
			{
				FilterInfo.Current = current;
			});
		}

		public Func<IDisposable> JoinHoldCurrents(params Func<IDisposable>[] holdCurrentCallbacks)
		{
			return () =>
			{
				var disposables = holdCurrentCallbacks.Select(c => c()).ToArray();
				return Disposable.Create(() =>
				{
					foreach (var d in disposables)
					{
						d.Dispose();
					}
				});
			};
		}

		#endregion

		#region Actions

		// you can use currents

		public virtual bool AskExtConvert(out bool redirect, params CRPopupValidator[] validators)
		{
			// no need to ask for cb, just return true if data is valid
			if (Base.IsContractBasedAPI || Base.IsImport)
			{
				AdjustFilterForContactBasedAPI(FilterInfo.Current);
				PopupValidator.Validate(validators);

				redirect = false;
				return true;
			}

			var result = PopupValidator.AskExtValidation(
				(graph, view) =>
				{
					if (Base.IsDirty)
						Base.Actions.PressSave();

					PopupValidator.Reset(validators);
				},
				reset: true,
				additionalValidators: validators);
			redirect = result == WebDialogResult.Yes;
			return result is WebDialogResult webResult && WebDialogResultExtension.IsPositive(webResult);
		}

		internal virtual void AdjustFilterForContactBasedAPI(TFilter filter)
		{
		}

		public TMain GetMain(Document doc) => (TMain)Documents.Cache.GetMain(doc);
		public TMain GetMainCurrent() => GetMain(Documents.Current);

		public virtual ConversionResult<TTarget> Convert(TConversionOptions options = null)
		{
			var graph = CreateTargetGraph();
			var entity = CreateMaster(graph, options);

			ReverseDocumentUpdate(graph, entity);

			graph.Actions.PressSave();

			IDisposable hold = options?.HoldCurrentsCallback?.Invoke();
			try
			{
				Base.Actions.PressCancel();
				Documents.View.Clear();
				Documents.Current = Documents.Search<Document.noteID>(Documents.Current.NoteID);
			}
			finally
			{
				hold?.Dispose();
			}

			return new ConversionResult<TTarget>
			{
				Entity = entity,
				Graph = graph,
			};
		}

		protected virtual TTargetGraph CreateTargetGraph()
		{
			var graph = PXGraph.CreateInstance<TTargetGraph>();
			graph.Caches<TMain>().Current = GetMainCurrent();
			return graph;
		}

		protected abstract TTarget CreateMaster(TTargetGraph graph, TConversionOptions options);

		public void Redirect(ConversionResult<TTarget> result)
		{
			if (result == null)
				throw new ArgumentNullException(nameof(result));

			var graph = result.Graph ?? CreateTargetGraph();
			graph.Caches<TTarget>().Current = result.Entity;

			throw new PXRedirectRequiredException(graph, typeof(TTarget).Name);
		}

		protected virtual void FillAttributes(CRAttributeList<TTarget> answers, TTarget entity)
		{
			answers.CopyAllAttributes(entity, GetMainCurrent());

			foreach (var answer in GetFilledAttributes()
				?.Where(a => a.Value != null)
				?? Enumerable.Empty<PopupAttributes>())
			{
				answers.Update(new CSAnswers
				{
					AttributeID = answer.AttributeID,
					Value = answer.Value,
					RefNoteID = entity.NoteID
				});
			}
		}

		protected virtual void FillRelations<TNoteField>(CRRelationsList<TNoteField> relations, TTarget target) where TNoteField : IBqlField
		{
			var entity = Documents.Current;
			var relation = (CRRelation)relations.Cache.CreateInstance();

			relation.RefNoteID = target.NoteID;
			relation.Role = CRRoleTypeList.Source;
			relation.TargetType = TargetType;
			relation.TargetNoteID = entity.NoteID;
			relation.ContactID = entity.RefContactID;

			// otherwise value would be rewriten from cache
			PXDBDefaultAttribute.SetDefaultForInsert<CRRelation.refNoteID>(relations.Cache, relation, false);

			relations.Insert(relation);
		}

		protected virtual void ReverseDocumentUpdate(TTargetGraph graph, TTarget entity)
		{

		}

		#endregion
	}

	/// <exclude/>
	[PXInternalUseOnly]
	public abstract class CRCreateActionBaseInit<TGraph, TMain> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Views

		#region Document Mapping
		protected class DocumentMapping : IBqlMapping
		{
			public Type Extension => typeof(Document);
			protected Type _table;
			public Type Table => _table;

			public DocumentMapping(Type table)
			{
				_table = table;
			}
			public Type ParentBAccountID = typeof(Document.parentBAccountID);
			public Type WorkgroupID = typeof(Document.workgroupID);
			public Type OwnerID = typeof(Document.ownerID);
			public Type BAccountID = typeof(Document.bAccountID);
			public Type ContactID = typeof(Document.contactID);
			public Type RefContactID = typeof(Document.refContactID);
			public Type ClassID = typeof(Document.classID);
			public Type NoteID = typeof(Document.noteID);
			public Type Source = typeof(Document.source);
			public Type CampaignID = typeof(Document.campaignID);
			public Type OverrideRefContact = typeof(Document.overrideRefContact);
			public Type Description = typeof(Document.description);
			public Type Location = typeof(Document.locationID);
			public Type TaxZoneID = typeof(Document.taxZoneID);
		}
		protected virtual DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(TMain));
		}
		#endregion

		#region Document Contact Mapping
		protected class DocumentContactMapping : IBqlMapping
		{
			public Type Extension => typeof(DocumentContact);
			protected Type _table;
			public Type Table => _table;

			public DocumentContactMapping(Type table)
			{
				_table = table;
			}
			public Type FullName = typeof(DocumentContact.fullName);
			public Type Title = typeof(DocumentContact.title);
			public Type FirstName = typeof(DocumentContact.firstName);
			public Type LastName = typeof(DocumentContact.lastName);
			public Type Salutation = typeof(DocumentContact.salutation);
			public Type Attention = typeof(DocumentContact.attention);
			public Type Email = typeof(DocumentContact.email);
			public Type Phone1 = typeof(DocumentContact.phone1);
			public Type Phone1Type = typeof(DocumentContact.phone1Type);
			public Type Phone2 = typeof(DocumentContact.phone2);
			public Type Phone2Type = typeof(DocumentContact.phone2Type);
			public Type Phone3 = typeof(DocumentContact.phone3);
			public Type Phone3Type = typeof(DocumentContact.phone3Type);
			public Type Fax = typeof(DocumentContact.fax);
			public Type FaxType = typeof(DocumentContact.faxType);
			public Type OverrideContact = typeof(DocumentContact.overrideContact);

			public Type ConsentAgreement = typeof(DocumentContact.consentAgreement);
			public Type ConsentDate = typeof(DocumentContact.consentDate);
			public Type ConsentExpirationDate = typeof(DocumentContact.consentExpirationDate);
		}
		protected abstract DocumentContactMapping GetDocumentContactMapping();
		#endregion

		#region Document Address Mapping
		protected class DocumentAddressMapping : IBqlMapping
		{
			public Type Extension => typeof(DocumentAddress);
			protected Type _table;
			public Type Table => _table;

			public DocumentAddressMapping(Type table)
			{
				_table = table;
			}
			public Type OverrideAddress = typeof(DocumentAddress.overrideAddress);
			public Type AddressLine1 = typeof(DocumentAddress.addressLine1);
			public Type AddressLine2 = typeof(DocumentAddress.addressLine2);
			public Type AddressLine3 = typeof(DocumentAddress.addressLine3);
			public Type City = typeof(DocumentAddress.city);
			public Type CountryID = typeof(DocumentAddress.countryID);
			public Type State = typeof(DocumentAddress.state);
			public Type PostalCode = typeof(DocumentAddress.postalCode);
			public Type IsValidated = typeof(DocumentAddress.isValidated);
		}
		protected abstract DocumentAddressMapping GetDocumentAddressMapping();
		#endregion

		public PXSelectExtension<Document> Documents;
		public PXSelectExtension<DocumentContact> Contacts;
		public PXSelectExtension<DocumentAddress> Addresses;

		#endregion

		#region Initialization

		protected virtual IPersonalContact MapContact(DocumentContact source, IPersonalContact target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.FullName = source.FullName;
			target.Title = source.Title;
			target.FirstName = source.FirstName;
			target.LastName = source.LastName;
			target.Salutation = source.Salutation;
			target.Attention = source.Attention;
			target.Email = source.Email;
			target.WebSite = source.WebSite;
			target.Phone1 = source.Phone1;
			target.Phone1Type = source.Phone1Type;
			target.Phone2 = source.Phone2;
			target.Phone2Type = source.Phone2Type;
			target.Phone3 = source.Phone3;
			target.Phone3Type = source.Phone3Type;
			target.Fax = source.Fax;
			target.FaxType = source.FaxType;

			return target;
		}

		protected virtual IConsentable MapConsentable(DocumentContact source, IConsentable target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.ConsentAgreement = source.ConsentAgreement;
			target.ConsentDate = source.ConsentDate;
			target.ConsentExpirationDate = source.ConsentExpirationDate;

			return target;
		}

		protected virtual IAddressBase MapAddress(DocumentAddress source, IAddressBase target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.AddressLine1 = source.AddressLine1;
			target.AddressLine2 = source.AddressLine2;
			target.City = source.City;
			target.CountryID = source.CountryID;
			target.State = source.State;
			target.PostalCode = source.PostalCode;

			return target;
		}

		#endregion
	}
}
