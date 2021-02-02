using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages.Subcontract;

namespace PX.Objects.CN.Subcontracts.AP.CacheExtensions
{
    public sealed class PoLineRsExt : PXCacheExtension<POLineRS>
    {
        [PXString]
        [PXUIField(DisplayName = Messages.SubcontractNumber)]
        public string SubcontractNbr => Base.OrderNbr;

        [PXDate]
        [PXUIField(DisplayName = Messages.SubcontractDate)]
        public DateTime? SubcontractDate => Base.OrderDate;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class subcontractNbr : BqlString.Field<subcontractNbr>
        {
        }

        public abstract class subcontractDate : BqlDateTime.Field<subcontractDate>
        {
        }
    }
}
