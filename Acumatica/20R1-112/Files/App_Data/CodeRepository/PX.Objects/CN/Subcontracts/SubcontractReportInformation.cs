using PX.Data;
using PX.Objects.PO;
using ScMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts
{
    [PXCacheName("Subcontract Report Information")]
    public class SubcontractReportInformation : IBqlTable
    {
        [PXSelector(typeof(Search<POOrder.orderNbr,
                Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>>,
                OrderBy<Desc<POOrder.orderNbr>>>),
            typeof(POOrder.orderNbr),
            typeof(POOrder.vendorID),
            typeof(POOrder.vendorLocationID),
            typeof(POOrder.orderDate),
            typeof(POOrder.status),
            typeof(POOrder.curyID),
            typeof(POOrder.vendorRefNbr),
            typeof(POOrder.curyOrderTotal),
            typeof(POOrder.lineTotal),
            typeof(POOrder.sOOrderType),
            typeof(POOrder.sOOrderNbr),
            typeof(POOrder.orderDesc),
            typeof(POOrder.ownerID),
            Headers = new[]
            {
                ScMessages.Subcontract.SubcontractNumber,
                ScMessages.Subcontract.Vendor,
                ScMessages.Subcontract.Location,
                ScMessages.Subcontract.Date,
                ScMessages.Subcontract.Status,
                ScMessages.Subcontract.Currency,
                ScMessages.Subcontract.VendorReference,
                ScMessages.Subcontract.SubcontractTotal,
                ScMessages.Subcontract.LineTotal,
                ScMessages.Subcontract.SalesOrderType,
                ScMessages.Subcontract.SalesOrderNumber,
                ScMessages.Subcontract.Description,
                ScMessages.Subcontract.Owner,
            })]
        public virtual int? SubcontractNumber
        {
            get;
            set;
        }
    }
}