using PX.Data;
using PX.Data.EP;
using PX.Objects.CN.Subcontracts.CR.Helpers;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.Subcontracts.CR.GraphExtensions
{
    public class CrEmailActivityMaintExt : PXGraphExtension<CREmailActivityMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected virtual void CrSmEmail_RowSelected(PXCache cache, PXRowSelectedEventArgs args,
            PXRowSelected baseHandler)
        {
            baseHandler(cache, args);
            if (args.Row is CRSMEmail email)
            {
                UpdateEntityDescriptionIfRequired(email);
            }
        }

        private void UpdateEntityDescriptionIfRequired(CRSMEmail email)
        {
            var entityDescription = SubcontractEntityDescriptionHelper.GetDescription(email, Base);
            if (entityDescription != null)
            {
                email.EntityDescription =
                    string.Concat(CacheUtility.GetErrorDescription(email.Exception), entityDescription);
            }
        }
    }
}
