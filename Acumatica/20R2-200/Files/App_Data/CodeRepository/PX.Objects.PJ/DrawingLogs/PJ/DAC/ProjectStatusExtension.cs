using PX.Data.BQL;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    public class ProjectStatusExtension
    {
        public class planning : BqlString.Constant<planning>
        {
            public planning()
                : base(ProjectStatus.Planned)
            {
            }
        }
    }
}
