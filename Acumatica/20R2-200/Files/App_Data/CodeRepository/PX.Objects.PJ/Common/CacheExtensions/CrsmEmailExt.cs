using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.PJ.Common.CacheExtensions
{
    public sealed class CrSmEmailExt : PXCacheExtension<CRSMEmail>
    {
	    [PXDBString(BqlField = typeof(CRActivityExt.recipientNotes))]
        [PXUIField(DisplayName = "Recipient Notes")]
        public string RecipientNotes
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class recipientNotes : IBqlField
        {
        }
    }
}