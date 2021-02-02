using System;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.PO;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    [PXDBInt]
    [PXDefault]
    [PXUIField(DisplayName = "Vendor ID")]
    public sealed class SubcontractorAttribute : PXDimensionSelectorAttribute
    {
        private static readonly Type[] Fields =
        {
            typeof(Vendor.acctCD),
            typeof(Vendor.acctName),
            typeof(Address.addressLine1),
            typeof(Address.addressLine2),
            typeof(Address.postalCode),
            typeof(Contact.phone1),
            typeof(Address.city),
            typeof(Address.countryID),
            typeof(Location.taxRegistrationID),
            typeof(Vendor.curyID),
            typeof(Contact.attention),
            typeof(Vendor.vendorClassID),
            typeof(Vendor.status)
        };

        private static readonly Type SearchType = typeof(SelectFrom<Vendor>
            .InnerJoin<POLine>.On<POLine.vendorID.IsEqual<Vendor.bAccountID>
                .And<POLine.projectID.IsEqual<DailyFieldReport.projectId.FromCurrent>>>
            .LeftJoin<Address>.On<Address.bAccountID.IsEqual<Vendor.bAccountID>>
            .LeftJoin<Contact>.On<Contact.bAccountID.IsEqual<Vendor.bAccountID>>
            .LeftJoin<Location>.On<Location.bAccountID.IsEqual<Vendor.bAccountID>>
            .AggregateTo<GroupBy<Vendor.bAccountID>>.SearchFor<Vendor.bAccountID>);

        public SubcontractorAttribute()
            : base("VENDOR", SearchType, typeof(Vendor.acctCD), Fields)
        {
            Filterable = true;
            FilterEntity = typeof(Vendor);
            CacheGlobal = true;
        }
    }
}