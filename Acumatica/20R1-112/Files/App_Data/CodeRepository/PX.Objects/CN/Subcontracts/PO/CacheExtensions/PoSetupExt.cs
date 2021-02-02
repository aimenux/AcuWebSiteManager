using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PO;
using PoMessages = PX.Objects.CN.Subcontracts.PO.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PO.CacheExtensions
{
    public sealed class PoSetupExt : PXCacheExtension<POSetup>
    {
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PoMessages.PoSetup.SubcontractNumberingName)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = PoMessages.PoSetup.SubcontractNumberingId)]
        public string SubcontractNumberingID
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = PoMessages.PoSetup.RequireSubcontractControlTotal)]
        public bool? RequireSubcontractControlTotal
        {
            get;
            set;
        }

        [EPRequireApproval]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = PoMessages.PoSetup.SubcontractRequireApproval)]
        public bool? SubcontractRequestApproval
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        public bool? IsSubcontractSetupSaved
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class subcontractNumberingID : IBqlField
        {
        }

        public abstract class requireSubcontractControlTotal : IBqlField
        {
        }

        public abstract class subcontractRequestApproval : IBqlField
        {
        }

        public abstract class isSubcontractSetupSaved : IBqlField
        {
        }
    }
}