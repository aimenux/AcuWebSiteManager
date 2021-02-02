using PX.Common;
using PX.Common.Disposables;
using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Linq;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public abstract partial class CRCreateOpportunityAction<TGraph, TMain>
		: CRCreateActionBase<
			TGraph,
			TMain,
			OpportunityMaint,
			CROpportunity,
			OpportunityFilter,
			OpportunityConversionOptions>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Ctor

		protected override ICRValidationFilter[] AdditionalFilters => new[] { OpportunityInfoAttributes };

		public override IDisposable HoldCurrents()
		{
			var current = FilterInfo.Current;
			var attrs = OpportunityInfoAttributes.Cache.Updated.RowCast<PopupAttributes>().ToArray();
			return Disposable.Create(() =>
			{
				FilterInfo.Current = current;
				foreach (var at in attrs)
				{
					OpportunityInfoAttributes.Cache.SetStatus(at, PXEntryStatus.Updated);
				}
			});
		}



		#endregion

		#region Views

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<OpportunityFilter> OpportunityInfo;
		protected override CRValidationFilter<OpportunityFilter> FilterInfo => OpportunityInfo;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupAttributes> OpportunityInfoAttributes;
		protected virtual IEnumerable opportunityInfoAttributes()
		{
			return GetFilledAttributes();
		}

		#endregion

		#region Events

		public virtual void _(Events.FieldDefaulting<OpportunityFilter, OpportunityFilter.subject> e)
		{
			e.NewValue = Documents.Current?.Description?.Replace("\r\n", " ")?.Replace("\n", " ");
		}

		public virtual void _(Events.FieldUpdated<OpportunityFilter, OpportunityFilter.opportunityClass> e)
		{
			Base.Caches<PopupAttributes>().Clear();
		}

		#endregion

		#region Actions
		public PXAction<TMain> ConvertToOpportunity;
		[PXUIField(DisplayName = Messages.CreateOpportunity, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable convertToOpportunity(PXAdapter adapter)
		{
			// todo: should create new instance of extension before call
			if (AskExtConvert(out bool redirect))
			{
				var processingGraph = Base.Clone();

				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateOpportunityAction<TGraph, TMain>>();

					bool? overrideContact = extension.Documents.Cache.GetValueOriginal<Document.overrideRefContact>(Documents.Current) as bool?;

					var result = extension.Convert(new OpportunityConversionOptions
					{
						ForceOverrideContact = overrideContact
					});

					if(redirect)
						Redirect(result);
				});
			}

			return adapter.Get();
		}

		internal override void AdjustFilterForContactBasedAPI(OpportunityFilter filter)
		{
			base.AdjustFilterForContactBasedAPI(filter);
			if (filter.Subject == null)
				filter.Subject = (Contacts.Current ?? Contacts.SelectSingle())?.FullName;
		}

		protected override CROpportunity CreateMaster(OpportunityMaint graph, OpportunityConversionOptions options)
		{
			var filter = OpportunityInfo.Current;
			var document = Documents.Current;
			var docContact = Contacts.Current ?? Contacts.SelectSingle();
			var docAddress = Addresses.Current ?? Addresses.SelectSingle();

			var opp = graph.Opportunity.Insert(new CROpportunity
			{
				Subject = filter.Subject,
				CloseDate = filter.CloseDate,
				ClassID = filter.OpportunityClass,
				LeadID = document.NoteID,
				Source = document.Source,
				CampaignSourceID = document.CampaignID,
			});

			opp.ContactID = document.RefContactID;
			opp.BAccountID = document.BAccountID;
			
			if (graph.OpportunityClass.SelectSingle()?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				opp.OwnerID = document.OwnerID;
				opp.WorkgroupID = document.WorkgroupID;
			}

			opp = graph.Opportunity.Update(opp);

			// should be after contact and baccount changes
			if (options?.ForceOverrideContact is bool foc && foc)
				graph.Opportunity.Cache.SetValueExt<CROpportunity.allowOverrideContactAddress>(opp, true);

			if (opp.AllowOverrideContactAddress == true)
			{
				var address = graph.Opportunity_Address.SelectSingle();
				MapAddress(docAddress, address);
				graph.Opportunity_Address.Update(address);

				var contact = graph.Opportunity_Contact.SelectSingle();
				MapContact(docContact, contact);
				MapConsentable(docContact, contact);
				graph.Opportunity_Contact.Update(contact);

				var shipAddress = graph.Shipping_Address.SelectSingle();
				MapAddress(docAddress, shipAddress);
				graph.Shipping_Address.Update(shipAddress);

				var shipContact = graph.Shipping_Contact.SelectSingle();
				MapContact(docContact, shipContact);
				MapConsentable(docContact, shipContact);
				graph.Shipping_Contact.Update(shipContact);
			}

			FillAttributes(graph.Answers, opp);

			FillRelations(graph.Relations, opp);

			CRSetup setup = PXSetupOptional<CRSetup>.Select(graph);
			PXNoteAttribute.CopyNoteAndFiles(graph.Caches<TMain>(), Documents.Cache.GetMain(document), graph.Opportunity.Cache, opp, setup);

			return opp;
		}

		protected override void ReverseDocumentUpdate(OpportunityMaint graph, CROpportunity entity)
		{
			var doc = Documents.Current;
			Documents.Cache.SetValue<Document.description>(doc, entity.Subject);
			Documents.Cache.SetValue<Document.qualificationDate>(doc, PXTimeZoneInfo.Now);
			Documents.Cache.SetValue<Document.convertedBy>(doc, PXAccess.GetUserID());
			graph.Caches<TMain>().Update(GetMain(doc));
		}

		#endregion
	}
}
