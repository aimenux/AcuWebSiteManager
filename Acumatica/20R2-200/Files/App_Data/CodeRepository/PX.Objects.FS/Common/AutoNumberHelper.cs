using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public static class AutoNumberHelper
    {
        public static void CheckAutoNumbering(PXGraph graph, string numberingID)
        {
            Numbering numbering = null;

            if (numberingID != null)
            {
                numbering = PXSelect<Numbering,
                            Where<
                                Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>
                            .Select(graph, numberingID);
            }

            if (numbering == null)
            {
                throw new PXSetPropertyException(Messages.NumberingIDNull);
            }

            if (numbering.UserNumbering == true)
            {
                throw new PXSetPropertyException(Messages.CantManualNumber, numbering.NumberingID);
            }
        }
    }
}
