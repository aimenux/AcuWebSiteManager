using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    public class ProjectManagementImpact : BaseCache, IProjectManagementImpact
    {
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Schedule Impact", Visibility = PXUIVisibility.Visible)]
        public virtual bool? IsScheduleImpact
        {
            get;
            set;
        }

        [PXDBInt]
        [PXUIVisible(typeof(isScheduleImpact.IsEqual<True>))]
        [PXUIField(DisplayName = "Schedule Impact (days)")]
        public virtual int? ScheduleImpact
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Cost Impact", Visibility = PXUIVisibility.Visible)]
        public virtual bool? IsCostImpact
        {
            get;
            set;
        }

        [PXDBDecimal]
        [PXUIVisible(typeof(isCostImpact.IsEqual<True>))]
        [PXUIField(DisplayName = "Cost Impact")]
        public virtual decimal? CostImpact
        {
            get;
            set;
        }

        public abstract class isScheduleImpact : BqlBool.Field<isScheduleImpact>
        {
        }

        public abstract class scheduleImpact : BqlInt.Field<scheduleImpact>
        {
        }

        public abstract class isCostImpact : BqlBool.Field<isCostImpact>
        {
        }

        public abstract class costImpact : BqlDecimal.Field<costImpact>
        {
        }
    }
}