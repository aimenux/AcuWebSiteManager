using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Services;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public sealed class RequestForInformationRelationContactSelectorAttribute : PXCustomSelectorAttribute,
        IPXFieldDefaultingSubscriber
    {
        private const string FilterViewName = "_Contact_";
        private readonly ProjectContactFilterService projectContactFilterService;

        public RequestForInformationRelationContactSelectorAttribute()
            : base(typeof(Search<Contact.contactID>))
        {
            Filterable = true;
            DescriptionField = typeof(Contact.displayName);
            projectContactFilterService = new ProjectContactFilterService();
        }

        public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
        {
            base.FieldSelecting(cache, args);
            if (args.Row is RequestForInformationRelation requestForInformationRelation &&
                args.ReturnState is PXFieldState fieldState)
            {
                fieldState.Enabled = IsEnabled(requestForInformationRelation);
            }
        }

        public void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation &&
                requestForInformationRelation.Type == RequestForInformationRelationTypeAttribute.RequestForInformation)
            {
                args.NewValue = requestForInformationRelation.DocumentNoteId != null
                    ? GetRequestForInformationContactId(requestForInformationRelation.DocumentNoteId)
                    : null;
            }
        }

        public IEnumerable GetRecords()
        {
            return projectContactFilterService.UpdateAdditionalFields(_Graph, GetContacts());
        }

        protected override void CreateFilter(PXGraph graph)
        {
            projectContactFilterService.CreateFilterView(graph, _ViewName, FilterViewName);
        }

        private static bool IsEnabled(RequestForInformationRelation requestForInformationRelation)
        {
            switch (requestForInformationRelation.Type)
            {
                case RequestForInformationRelationTypeAttribute.ApInvoice:
                case RequestForInformationRelationTypeAttribute.ArInvoice:
                case RequestForInformationRelationTypeAttribute.RequestForInformation:
                case null:
                    return false;
                default:
                    return true;
            }
        }

        private static IEnumerable<Contact> GetContacts()
        {
            var requestForInformationRelation = GetCurrentRequestForInformationRelation();
            return requestForInformationRelation.BusinessAccountId != null
                ? GetContactsForBusinessAccount(requestForInformationRelation)
                : GetAllContacts();
        }

        private int? GetRequestForInformationContactId(Guid? noteId)
        {
            var query = new PXSelect<RequestForInformation,
                Where<RequestForInformation.noteID, Equal<Required<RequestForInformation.noteID>>>>(_Graph);
            return query.SelectSingle(noteId).ContactId;
        }

        private static IEnumerable<Contact> GetAllContacts()
        {
            return PXSelect<Contact,
                    Where<Contact.isActive, Equal<True>,
                    And<Contact.contactType, In3<ContactTypesAttribute.person, ContactTypesAttribute.employee>>>>
                .Select(PXView.CurrentGraph).FirstTableItems;
        }

        private static IEnumerable<Contact> GetContactsForBusinessAccount(
            RequestForInformationRelation requestForInformationRelation)
        {
            return PXSelect<Contact,
                    Where<Contact.bAccountID, Equal<Required<Contact.bAccountID>>,
                    And<Contact.isActive, Equal<True>,
                    And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>>
                .Select(PXView.CurrentGraph, requestForInformationRelation.BusinessAccountId)
                .FirstTableItems;
        }

        private static RequestForInformationRelation GetCurrentRequestForInformationRelation()
        {
            var currentInView = PXView.Currents.OfType<RequestForInformationRelation>().SingleOrDefault();
            var cache = PXView.CurrentGraph.Caches[typeof(RequestForInformationRelation)];
            var currentInCache = cache.Current as RequestForInformationRelation;
            return currentInView ?? currentInCache;
        }
    }
}