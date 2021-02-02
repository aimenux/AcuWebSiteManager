using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions
{
    public sealed class ContactExt : PXCacheExtension<Contact>
    {
        [PXBool]
        [PXUIField(DisplayName = "Is Related To Project Contact",
            Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        public bool? IsRelatedToProjectContact
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class isRelatedToProjectContact : IBqlField
        {
        }
    }
}