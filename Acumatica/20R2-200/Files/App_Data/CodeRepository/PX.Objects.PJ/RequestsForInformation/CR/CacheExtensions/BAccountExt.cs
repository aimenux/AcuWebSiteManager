using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions
{
    public sealed class BAccountExt : PXCacheExtension<BAccount>
    {
        [PXBool]
        [PXUIField(DisplayName = "Is Related To Project Contact",
            Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        public bool? IsRelatedToProjectContact
        {
            get;
            set;
        }

        // PXSelector attribute is used for querying inner entity in Notification Templates.
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<Address.addressID>))]
        public int? DefAddressID
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class defAddressID : IBqlField
        {
        }

        public abstract class isRelatedToProjectContact : IBqlField
        {
        }
    }
}