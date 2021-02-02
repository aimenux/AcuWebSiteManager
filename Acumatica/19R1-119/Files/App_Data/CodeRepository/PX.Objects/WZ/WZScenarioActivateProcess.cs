using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.WZ
{
    [Serializable]
    public class WZScenarioActivateProcess : PXGraph<WZScenarioActivateProcess>
    {
        public PXCancel<PendingWZScenario> Cancel;

        [PXFilterable]
        public PXProcessing<Schedule, Where<Schedule.active, Equal<boolTrue>,
                    And<Schedule.module, Equal<BatchModule.moduleWZ>>>> ScheduleList;

        public WZScenarioActivateProcess()
        {
            ScheduleList.SetProcessDelegate(Activate);
        }

        public static void Activate(List<Schedule> list)
        {
            WZScheduleProcess graph = PXGraph.CreateInstance<WZScheduleProcess>();
            foreach (Schedule schedule in list)
            {
                graph.GenerateProc(schedule);
            }
            PXSiteMap.Provider.Clear();
        }
    }

    [Serializable]
    public class PendingWZScenario : WZScenario
    {
        #region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
    }
}
