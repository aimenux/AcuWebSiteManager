using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectManagement.PM.GraphExtensions
{
    public class ChangeOrderEntryExt : PXGraphExtension<ChangeOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public virtual void _(Events.RowSelected<PMChangeOrder> args)
        {
            if (Base.IsMobile && Base.Setup.Current.CostCommitmentTracking == false)
            {
                Base.Details.Cache.Enable(false);
            }
        }

        public virtual void _(Events.FieldSelecting<PMChangeOrder.classID> args)
        {
            if (Base.IsMobile && args.Row is PMChangeOrder changeOrder &&
                IsTwoTierLevelManagementEnabled(changeOrder.ClassID))
            {
                args.Cache.RaiseException<PMChangeOrder.classID>(
                    changeOrder, ProjectManagementMessages.ChangeOrderClassIsTwoTier, null, PXErrorLevel.RowWarning);
            }
        }

        private PMChangeOrderClass GetChangeOrderClass(string classId)
        {
            return SelectFrom<PMChangeOrderClass>
                .Where<PMChangeOrderClass.classID.IsEqual<P.AsString>>.View.Select(Base, classId);
        }

        private bool IsTwoTierLevelManagementEnabled(string classId)
        {
            return GetChangeOrderClass(classId)?.IsAdvance == true;
        }
    }
}
