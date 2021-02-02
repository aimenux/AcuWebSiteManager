using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.PJ.Common.CacheExtensions
{
	public sealed class CRActivityExt : PXCacheExtension<CRActivity>
    {
        [PXDBBool]
        [PXUIField(DisplayName = "Final Answer")]
        public bool? IsFinalAnswer
        {
            get;
            set;
        }

        [PXDBString]
        public string RecipientNotes
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class isFinalAnswer : IBqlField
        {
        }

        public abstract class recipientNotes : IBqlField
        {
        }
    }
}