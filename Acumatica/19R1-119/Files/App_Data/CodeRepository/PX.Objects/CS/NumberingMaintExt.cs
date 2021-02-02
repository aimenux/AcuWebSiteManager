using System.Linq;
using PX.Data;
using PX.Objects.GL;


namespace PX.Objects.CS
{
    public class NumberingMaintExt : PXGraphExtension<NumberingMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
        }

        protected virtual void Numbering_UserNumbering_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if ((bool)e.NewValue == true && PXSelect<GLWorkBook, Where<GLWorkBook.voucherNumberingID, Equal<Current<Numbering.numberingID>>>>.Select(Base).Any())
            {
                e.NewValue = false;
                var row = (Numbering) e.Row;
                cache.RaiseExceptionHandling<Numbering.userNumbering>(row, row.UserNumbering, new PXSetPropertyException(Messages.NubmeringCannotBeSetManual, PXErrorLevel.Warning));
            }
        }
    }
}
