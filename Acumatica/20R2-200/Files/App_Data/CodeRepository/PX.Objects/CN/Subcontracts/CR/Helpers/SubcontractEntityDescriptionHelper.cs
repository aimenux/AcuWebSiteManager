using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.CR.Helpers
{
    public static class SubcontractEntityDescriptionHelper
    {
        public const string StatusFieldName = "Status";

        public static string SubcontractDescription = string.Concat("<b>Subcontract</b><table width=\"100%\" ",
            "cellspacing=\"0\" style=\"margin-left: 7px;\" ><tr><td><font color=\"Gray\">Order Nbr.:</font></td><td>{0}</td></tr>",
            "<tr><td><font color=\"Gray\">Vendor:</font></td><td>{1}</td></tr><tr><td><font color=\"Gray\">Location:</font></td>",
            "<td>Primary Location</td></tr><tr><td><font color=\"Gray\">Date:</font></td><td>{2}</td></tr>",
            "<tr><td><font color=\"Gray\">Status:</font></td><td>{3}</td></tr></table>");

        public static string GetDescription(CRActivity activity, PXGraph graph)
        {
            var entityHelper = new EntityHelper(graph);
            var commitment = entityHelper.GetEntityRow(activity.RefNoteID) as POOrder;
            return commitment?.OrderType == POOrderType.RegularSubcontract
                ? GetEntityDescription(entityHelper, commitment, graph)
                : null;
        }

        private static string GetEntityDescription(EntityHelper entityHelper, POOrder commitment, PXGraph graph)
        {
            var status = entityHelper.GetFieldString(commitment, commitment.GetType(), StatusFieldName);
            var vendorName = GetVendorName(commitment.VendorID, graph);
            return string.Format(SubcontractDescription, commitment.OrderNbr, vendorName, commitment.OrderDate,
                status);
        }

        private static string GetVendorName(int? vendorId, PXGraph graph)
        {
            return new PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(graph)
                .SelectSingle(vendorId).AcctName;
        }
    }
}
