using System;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PM.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.EP;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Services
{
    public class RequestForInformationDataProvider
    {
        private readonly PXGraph graph;

        public RequestForInformationDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public ProjectContact GetProjectContact(PXGraph entryGraph, RequestForInformation requestForInformation)
        {
            return new PXSelect<ProjectContact,
                    Where<ProjectContact.projectId, Equal<Required<ProjectContact.projectId>>,
                        And<ProjectContact.contactId, Equal<Required<ProjectContact.contactId>>>>>(entryGraph)
                .SelectSingle(requestForInformation.ProjectId, requestForInformation.ContactId);
        }

        public EPEmployeeContract GetEmployeeContact(PXGraph entryGraph, int? projectId, int? businessAccountId)
        {
            return new PXSelect<EPEmployeeContract,
                    Where<EPEmployeeContract.contractID, Equal<Required<EPEmployeeContract.contractID>>,
                        And<EPEmployeeContract.employeeID, Equal<Required<EPEmployeeContract.employeeID>>>>>(entryGraph)
                .SelectSingle(projectId, businessAccountId);
        }

        public int? GetBusinessAccountId(int? defaultAccountId)
        {
            return new PXSelect<BAccount,
                    Where<BAccount.defContactID, Equal<Required<BAccount.defContactID>>,
                        And<BAccount.type, Equal<ContactTypesAttribute.employee>>>>(graph)
                .SelectSingle(defaultAccountId)?.BAccountID;
        }

        public Contact GetContact(int? contactId)
        {
            return new PXSelect<Contact,
                Where<Contact.contactID, Equal<Required<Contact.contactID>>>>(graph).SelectSingle(contactId);
        }

        public PXView GetProjectContactsView()
        {
            return new PXView(graph, true, new Select2<BAccountR,
                InnerJoin<ProjectContact, On<ProjectContact.businessAccountId, Equal<BAccountR.bAccountID>>,
                LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>,
                    And<Contact.contactID, Equal<BAccountR.defContactID>>>,
                LeftJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>,
                    And<Address.addressID, Equal<BAccountR.defAddressID>>>>>>,
                Where<ProjectContact.projectId, Equal<Current<RequestForInformation.projectId>>,
                    And<Match<Current<AccessInfo.userName>>>>>());
        }

        public static RequestForInformation GetRequestForInformation(
            PXGraph graph, int? requestForInformationId)
        {
            return new PXSelect<RequestForInformation,
                    Where<RequestForInformation.requestForInformationId,
                        Equal<Required<RequestForInformation.requestForInformationId>>>>(graph)
                .SelectSingle(requestForInformationId);
        }

        public RequestForInformation GetRequestForInformation(Guid? noteId)
        {
            return SelectFrom<RequestForInformation>
                .Where<RequestForInformation.noteID.IsEqual<P.AsGuid>>.View.Select(graph, noteId);
        }

        public static Contract GetRelatedProject(PXGraph graph, int? requestForInformationId)
        {
            return new PXSelectJoin<Contract,
                    LeftJoin<RequestForInformation,
                        On<RequestForInformation.projectId, Equal<Contract.contractID>>>,
                    Where<RequestForInformation.requestForInformationId,
                        Equal<Required<RequestForInformation.requestForInformationId>>>>(graph)
                .SelectSingle(requestForInformationId);
        }
    }
}