using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.GL.Reclassification.Common
{
	public class ReclassGraphState : IBqlTable
	{
		public virtual ReclassScreenMode ReclassScreenMode { get; set; }

		public virtual string EditingBatchModule { get; set; }
		public virtual string EditingBatchNbr { get; set; }
		public virtual string EditingBatchMasterPeriodID { get; set; }
		public virtual string EditingBatchCuryID { get; set; }

		public virtual string OrigBatchModuleToReverse { get; set; }
		public virtual string OrigBatchNbrToReverse { get; set; }

        private int currentSplitLineNbr;
        public virtual int CurrentSplitLineNbr { get { return currentSplitLineNbr; } }
        public virtual void IncSplitLineNbr()
        {
            currentSplitLineNbr += 1;
        }

        public virtual HashSet<GLTranForReclassification> GLTranForReclassToDelete { get; set; }

		public Dictionary<GLTranKey, List<GLTranKey>> SplittingGroups { get; set; }

        public ReclassGraphState()
		{
			GLTranForReclassToDelete = new HashSet<GLTranForReclassification>();
            SplittingGroups = new Dictionary<GLTranKey, List<GLTranKey>>();
            currentSplitLineNbr = int.MinValue;
        }

        public void ClearSplittingGroups()
        {
            SplittingGroups = new Dictionary<GLTranKey, List<GLTranKey>>();
            currentSplitLineNbr = int.MinValue;
        }
    }
}
