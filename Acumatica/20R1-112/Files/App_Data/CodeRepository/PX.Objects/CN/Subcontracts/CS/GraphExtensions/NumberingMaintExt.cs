using PX.Data;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Subcontracts.PO.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.PO;
using ScMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.CS.GraphExtensions
{
    public class NumberingMaintExt : PXGraphExtension<NumberingMaint>
    {
        [InjectDependency]
        public INumberingSequenceUsage NumberingSequenceUsage
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected virtual void _(Events.RowDeleting<Numbering> args)
        {
            NumberingSequenceUsage.CheckForNumberingUsage<POSetup, PoSetupExt.subcontractNumberingID>(
                args.Row, Base, ScMessages.SubcontractsPreferencesScreenName);
        }
    }
}