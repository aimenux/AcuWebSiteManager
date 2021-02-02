using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.CL.Services.DataProviders
{
    public class RecipientEmailDataProvider : IRecipientEmailDataProvider
    {
        private readonly PXGraph graph;

        public RecipientEmailDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public string GetRecipientEmail(NotificationRecipient notificationRecipient, int? vendorId)
        {
            switch (notificationRecipient.ContactType)
            {
                case NotificationContactType.Primary:
                    return GetEmailForPrimaryVendor(vendorId);
                case NotificationContactType.Employee:
                    return GetContactEmail(notificationRecipient.ContactID);
                case NotificationContactType.Remittance:
                    return GetEmailForRemittanceContact(vendorId);
                case NotificationContactType.Shipping:
                    return GetEmailForShippingContact(vendorId);
                default:
                    return GetContactEmail(notificationRecipient.ContactID);
            }
        }

        private string GetEmailForRemittanceContact(int? vendorId)
        {
            var locationExtensionAddress = GetLocationExtensionAddress(vendorId);
            var contactId = locationExtensionAddress.VRemitContactID;
            return GetContactEmail(contactId);
        }

        private string GetEmailForShippingContact(int? vendorId)
        {
            var locationExtensionAddress = GetLocationExtensionAddress(vendorId);
            var contactId = locationExtensionAddress.DefContactID;
            return GetContactEmail(contactId);
        }

        private string GetContactEmail(int? contactId)
        {
            return graph.Select<Contact>().SingleOrDefault(c => c.ContactID == contactId)?.EMail;
        }

        private string GetEmailForPrimaryVendor(int? vendorId)
        {
            var locationExtensionAddress = GetLocationExtensionAddress(vendorId);
            return GetContactEmail(locationExtensionAddress.VDefContactID);
        }

        private Location GetLocationExtensionAddress(int? vendorId)
        {
            return SelectFrom<Location>
                .Where<Location.bAccountID.IsEqual<P.AsInt>>.View
                .Select(graph, vendorId);
        }
    }
}