using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Lists
{
    public class RequestForInformationRelationList : PXSelect<RequestForInformationRelation>
    {
        private const string TargetDetailsPostfix = "_TargetDetails";
        private const string EntityDetailsPostfix = "_EntityDetails";
        private const string ContactDetailsPostfix = "_ContactDetails";

        public RequestForInformationRelationList(PXGraph graph)
            : base(graph, GetSelectDelegate())
        {
            AttachEventHandlers(graph);
        }

        private void AttachEventHandlers(PXGraph graph)
        {
            graph.FieldDefaulting.AddHandler(
                typeof(RequestForInformationRelation),
                typeof(RequestForInformationRelation.requestForInformationNoteId).Name,
                RequestForInformationRelation_RefNoteId_FieldDefaulting);
            graph.FieldUpdated.AddHandler(
                typeof(RequestForInformationRelation),
                typeof(RequestForInformationRelation.isPrimary).Name,
                RequestForInformationRelation_IsPrimary_FieldUpdated);
            graph.RowUpdated.AddHandler(typeof(RequestForInformationRelation),
                RequestForInformationRelation_RowUpdated);
            graph.RowPersisting.AddHandler(
                typeof(RequestForInformationRelation),
                RequestForInformationRelation_RowPersisting);
            graph.Initialized += OnInitialized;
        }

        private void OnInitialized(PXGraph graph)
        {
            graph.Views.Caches.Remove(typeof(RequestForInformation.noteID));
            graph.EnsureCachePersistence(typeof(RequestForInformationRelation));
            AppendActions(graph);
        }

        private void AppendActions(PXGraph graph)
        {
            var viewName = graph.ViewNames[View];
            var primaryType = typeof(RequestForInformation);
            PXNamedAction.AddAction(graph, primaryType, viewName + TargetDetailsPostfix, null, TargetDetailsHandler);
            PXNamedAction.AddAction(graph, primaryType, viewName + EntityDetailsPostfix, null, EntityDetailsHandler);
            PXNamedAction.AddAction(graph, primaryType, viewName + ContactDetailsPostfix, null, ContactDetailsHandler);
        }

        private static void RequestForInformationRelation_RefNoteId_FieldDefaulting(PXCache cache,
            PXFieldDefaultingEventArgs args)
        {
            var requestForInformationCache = cache.Graph.Caches<RequestForInformation>();
            var requestForInformation = requestForInformationCache.Current as RequestForInformation;
            args.NewValue = requestForInformation?.NoteID;
        }

        private void RequestForInformationRelation_IsPrimary_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation
                && requestForInformationRelation.IsPrimary == true)
            {
                ClearIsPrimaryFlag(cache, requestForInformationRelation);
            }
        }

        private void RequestForInformationRelation_RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation)
            {
                ValidateRequestForInformationRelation(cache, requestForInformationRelation);
                if (requestForInformationRelation.Type == RequestForInformationRelationTypeAttribute.RequestForInformation)
                {
                    ProcessRelation(args, requestForInformationRelation);
                }
            }
        }

        private void RequestForInformationRelation_RowUpdated(PXCache cache, PXRowUpdatedEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation)
            {
                var businessAccount = GetBusinessAccount(requestForInformationRelation);
                var contact = GetContact(requestForInformationRelation);
                var users = GetUsers(contact);
                UpdateAdditionalFields(requestForInformationRelation, businessAccount, users, contact);
            }
        }

        private IEnumerable ContactDetailsHandler(PXAdapter adapter)
        {
            var requestForInformationRelation = Cache.Current as RequestForInformationRelation;
            NavigateToContactScreen(requestForInformationRelation);
            return adapter.Get();
        }

        private IEnumerable EntityDetailsHandler(PXAdapter adapter)
        {
            var requestForInformationRelation = Cache.Current as RequestForInformationRelation;
            NavigateToBusinessAccountScreen(requestForInformationRelation);
            return adapter.Get();
        }

        private IEnumerable TargetDetailsHandler(PXAdapter adapter)
        {
            var requestForInformationRelation = Cache.Current as RequestForInformationRelation;
            RequestForInformationRelationDocumentSelectorAttribute.NavigateToDocument(_Graph,
                requestForInformationRelation);
            return adapter.Get();
        }

        private void NavigateToBusinessAccountScreen(RequestForInformationRelation requestForInformationRelation)
        {
            if (requestForInformationRelation?.BusinessAccountId != null)
            {
                var businessAccount = GetBusinessAccount(requestForInformationRelation);
                if (businessAccount != null)
                {
                    NavigateToBusinessAccountScreen(businessAccount);
                }
            }
        }

        private void NavigateToBusinessAccountScreen(BAccount businessAccount)
        {
            var graph = businessAccount.Type == BAccountType.EmployeeType
                ? PXGraph.CreateInstance<EmployeeMaint>()
                : _Graph;
            PXRedirectHelper.TryRedirect(graph, businessAccount, PXRedirectHelper.WindowMode.NewWindow);
        }

        private void NavigateToContactScreen(RequestForInformationRelation requestForInformationRelation)
        {
            if (requestForInformationRelation?.ContactId != null)
            {
                var contact = GetContact(requestForInformationRelation);
                if (contact != null)
                {
                    PXRedirectHelper.TryRedirect(_Graph, contact, PXRedirectHelper.WindowMode.NewWindow);
                }
            }
        }

        private void ClearIsPrimaryFlag(PXCache cache, RequestForInformationRelation requestForInformationRelation)
        {
            foreach (var relation in GetRequestForInformationRelationsWithIsPrimaryFlag(requestForInformationRelation))
            {
                relation.IsPrimary = false;
                cache.Update(relation);
            }
            View.RequestRefresh();
        }

        private static void ValidateRequestForInformationRelation(PXCache cache, RequestForInformationRelation relation)
        {
            if (relation.Role == RequestForInformationRoleListAttribute.RelatedEntity)
            {
                ValidateRelatedEntityDocument(cache, relation);
            }
            else
            {
                ValidateContact(cache, relation);
            }
        }

        private IEnumerable<RequestForInformationRelation> GetRequestForInformationRelationsWithIsPrimaryFlag(
            RequestForInformationRelation requestForInformationRelation)
        {
            var query = new PXSelect<RequestForInformationRelation,
                Where<RequestForInformationRelation.role, Equal<Required<RequestForInformationRelation.role>>,
                    And<RequestForInformationRelation.type,
                        Equal<Required<RequestForInformationRelation.type>>,
                        And<RequestForInformationRelation.isPrimary, Equal<True>,
                            And<RequestForInformationRelation.requestForInformationNoteId,
                                Equal<Required<RequestForInformationRelation.requestForInformationNoteId>>,
                                And<RequestForInformationRelation.requestForInformationRelationId,
                                    NotEqual<Required<RequestForInformationRelation.requestForInformationRelationId>>>>>
                    >>>(_Graph);
            return query.Select(requestForInformationRelation.Role, requestForInformationRelation.Type,
                requestForInformationRelation.RequestForInformationNoteId,
                requestForInformationRelation.RequestForInformationRelationId).FirstTableItems;
        }

        private Contact GetContact(RequestForInformationRelation requestForInformationRelation)
        {
            var query = new PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>(_Graph);
            return query.SelectSingle(requestForInformationRelation.ContactId);
        }

        private BAccount GetBusinessAccount(RequestForInformationRelation requestForInformationRelation)
        {
            var query = new PXSelect<BAccount,
                Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>(_Graph);
            return query.SelectSingle(requestForInformationRelation.BusinessAccountId);
        }

        private Users GetUsers(Contact contact)
        {
            var query = new PXSelect<Users,
                Where<Users.pKID, Equal<Required<Users.pKID>>>>(_Graph);
            return query.SelectSingle(contact?.UserID);
        }

        private void ProcessRelation(PXRowPersistingEventArgs args, RequestForInformationRelation relation)
        {
            switch (args.Operation)
            {
                case PXDBOperation.Insert:
                    AddRelationForRequestForInformation(relation);
                    break;
                case PXDBOperation.Delete:
                    DeleteExistingRelations(relation);
                    break;
                case PXDBOperation.Update:
                    DeleteExistingRelationsIfNeeded(relation);
                    AddRelationForRequestForInformation(relation);
                    break;
            }
        }

        private void DeleteExistingRelationsIfNeeded(RequestForInformationRelation relation)
        {
            var originalRow = Cache.GetOriginal(relation);
            var originalRelation = (RequestForInformationRelation) originalRow;
            if (originalRelation.DocumentNoteId.HasValue &&
                relation.DocumentNoteId != originalRelation.DocumentNoteId)
            {
                DeleteExistingRelations(originalRelation);
            }
        }

        private void DeleteExistingRelations(RequestForInformationRelation relation)
        {
            SetRowPersistingAvailability(false);
            var relatedRelations = GetLinkedRequestForInformationRelation(relation.DocumentNoteId,
                relation.RequestForInformationNoteId);
            relatedRelations.ForEach((result, i) => Cache.PersistDeleted(result));
            SetRowPersistingAvailability(true);
        }

        private void AddRelationForRequestForInformation(RequestForInformationRelation requestForInformationRelation)
        {
            var copiedRelation = CreateLinkedRelation(requestForInformationRelation);
            SetRowPersistingAvailability(false);
            Cache.PersistInserted(copiedRelation);
            SetRowPersistingAvailability(true);
        }

        private RequestForInformationRelation CreateLinkedRelation(RequestForInformationRelation relation)
        {
            var copiedRelation = PXCache<RequestForInformationRelation>.CreateCopy(relation);
            SwapReferenceFields(copiedRelation);
            ResetField<RequestForInformationRelation.contactId>(copiedRelation);
            ResetField<RequestForInformationRelation.businessAccountId>(copiedRelation);
            copiedRelation.RequestForInformationRelationId = null;
            copiedRelation.AddToCc = default(bool);
            copiedRelation.IsPrimary = default(bool);
            return copiedRelation;
        }

        private void SetRowPersistingAvailability(bool shouldUse)
        {
            if (shouldUse)
            {
                _Graph.RowPersisting.AddHandler(
                    typeof(RequestForInformationRelation),
                    RequestForInformationRelation_RowPersisting);
            }
            else
            {
                _Graph.RowPersisting.RemoveHandler<RequestForInformationRelation>(
                    RequestForInformationRelation_RowPersisting);
            }
        }

        private void ResetField<T>(RequestForInformationRelation copiedRelation)
        {
            Cache.RaiseFieldDefaulting(typeof(T).Name, copiedRelation, out var newValue);
            Cache.SetValue(copiedRelation, typeof(T).Name, newValue);
        }

        private IEnumerable<RequestForInformationRelation> GetLinkedRequestForInformationRelation(
            Guid? documentNoteId, Guid? requestForInformationNoteId)
        {
            return new PXSelect<RequestForInformationRelation,
                    Where<RequestForInformationRelation.requestForInformationNoteId,
                        Equal<Required<RequestForInformationRelation.requestForInformationNoteId>>,
                    And<RequestForInformationRelation.documentNoteId,
                        Equal<Required<RequestForInformationRelation.documentNoteId>>>>>(_Graph)
                .Select(documentNoteId, requestForInformationNoteId).FirstTableItems;
        }

        private static PXSelectDelegate GetSelectDelegate()
        {
            return () => GetRequestForInformationRelations().Select(ProcessRequestForInformationRelation).ToList();
        }

        private static RequestForInformationRelation ProcessRequestForInformationRelation(PXResult result)
        {
            var requestForInformationRelation = result.GetItem<RequestForInformationRelation>();
            var businessAccount = result.GetItem<BAccount>();
            var users = result.GetItem<Users>();
            var originalContact = result.GetItem<Contact>();
            var contact = GetRelationContact(originalContact, requestForInformationRelation, businessAccount);
            UpdateAdditionalFields(requestForInformationRelation, businessAccount, users, contact);
            return requestForInformationRelation;
        }

        private static void UpdateAdditionalFields(RequestForInformationRelation requestForInformationRelation,
            BAccount businessAccount, Users users, Contact contact)
        {
            requestForInformationRelation.BusinessAccountName = businessAccount?.AcctName;
            requestForInformationRelation.BusinessAccountCd = businessAccount?.AcctCD;
            requestForInformationRelation.ContactEmail = contact?.EMail;
            if (businessAccount?.Type != BAccountType.EmployeeType)
            {
                requestForInformationRelation.ContactName = contact?.DisplayName;
            }
            else
            {
                if (string.IsNullOrEmpty(requestForInformationRelation.BusinessAccountName))
                {
                    requestForInformationRelation.BusinessAccountName = users?.FullName;
                }
                if (string.IsNullOrEmpty(requestForInformationRelation.ContactEmail))
                {
                    requestForInformationRelation.ContactEmail = users?.Email;
                }
            }
        }

        private static Contact GetRelationContact(Contact contact,
            RequestForInformationRelation requestForInformationRelation, BAccount businessAccount)
        {
            if (!contact.ContactID.HasValue
                && requestForInformationRelation.ContactId.HasValue
                && businessAccount.Type != BAccountType.EmployeeType)
            {
                var relationContact = SearchContact(requestForInformationRelation);
                if (relationContact != null)
                {
                    return relationContact;
                }
            }
            return contact;
        }

        private static Contact SearchContact(RequestForInformationRelation requestForInformationRelation)
        {
            return PXSelect<Contact>
                .Search<Contact.contactID>(PXView.CurrentGraph,
                    requestForInformationRelation.ContactId);
        }

        private static void SwapReferenceFields(RequestForInformationRelation copiedRelation)
        {
            (copiedRelation.DocumentNoteId, copiedRelation.RequestForInformationNoteId) =
                (copiedRelation.RequestForInformationNoteId, copiedRelation.DocumentNoteId);
        }

        private static void ValidateContact(PXCache cache, RequestForInformationRelation requestForInformationRelation)
        {
            if (requestForInformationRelation.ContactId == null)
            {
                cache.RaiseExceptionHandling<RequestForInformationRelation.contactId>(
                    requestForInformationRelation, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
                        typeof(RequestForInformationRelation.contactId).Name));
            }
        }

        private static void ValidateRelatedEntityDocument(PXCache cache, RequestForInformationRelation requestForInformationRelation)
        {
            if (requestForInformationRelation.DocumentNoteId == null)
            {
                cache.RaiseExceptionHandling<RequestForInformationRelation.documentNoteId>(
                    requestForInformationRelation, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
                        typeof(RequestForInformationRelation.documentNoteId).Name));
            }
        }

        private static PXResultset<RequestForInformationRelation> GetRequestForInformationRelations()
        {
            var query = new PXSelectJoin<RequestForInformationRelation, LeftJoin<BAccount,
                    On<BAccount.bAccountID, Equal<RequestForInformationRelation.businessAccountId>>,
                    LeftJoin<Contact, On<Contact.contactID, Equal<IIf<
                            Where<BAccount.type, Equal<BAccountType.employeeType>>,
                            BAccount.defContactID, RequestForInformationRelation.contactId>>>,
                        LeftJoin<Users, On<Users.pKID, Equal<Contact.userID>>>>>,
                Where<RequestForInformationRelation.requestForInformationNoteId,
                    Equal<Current<RequestForInformation.noteID>>>>(PXView.CurrentGraph);
            return query.Select();
        }
    }
}
