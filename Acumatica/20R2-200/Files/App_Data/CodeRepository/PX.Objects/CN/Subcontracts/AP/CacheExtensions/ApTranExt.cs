using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.AP.CacheExtensions
{
    public sealed class ApTranExt : PXCacheExtension<APTran>
    {
        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractNumber, Enabled = false, IsReadOnly = true)]
        public string SubcontractNbr =>
            Base.POOrderType == POOrderType.RegularSubcontract
                ? Base.PONbr
                : null;

        [PXInt]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractLine, Enabled = false, IsReadOnly = true, Visible = false)]
        public int? SubcontractLineNbr =>
            Base.POOrderType == POOrderType.RegularSubcontract
                ? Base.POLineNbr
                : null;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class subcontractNbr : IBqlField
        {
        }

        public abstract class subcontractLineNbr : IBqlField
        {
        }
    }
}