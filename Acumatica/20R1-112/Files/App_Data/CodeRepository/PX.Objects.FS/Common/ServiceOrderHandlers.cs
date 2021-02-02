using PX.Data;

namespace PX.Objects.FS
{
    public static class ServiceOrderHandlers
    {
        public static void FSSODet_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (FSSODet)e.Row;

            if (row.LineRef == null)
            {
                row.LineRef = row.LineNbr.Value.ToString("0000");
            }
        }
    }
}
