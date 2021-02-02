using PX.Data;
using PX.Objects.CS;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
    public static class SOAutomationHelper
    {

        private class stringBehavior : PX.Data.BQL.BqlString.Constant<stringBehavior>
		{
            public stringBehavior()
                : base(typeof(SOOrder.behavior).Name)
            {
            }
        }
        private class stringRejected : PX.Data.BQL.BqlString.Constant<stringRejected>
		{
            public stringRejected()
                : base(typeof(SOOrder.rejected).Name)
            {
            }
        }

        private class stringTrue : PX.Data.BQL.BqlString.Constant<stringTrue>
		{
            public stringTrue()
                : base(typeof(True).Name)
            {
            }
        }

        private static BqlCommand SupportsApprovalSelect =
             new Select2<AUStep,
                        InnerJoin<AUStepFilter, On<AUStepFilter.stepID, Equal<AUStep.stepID>>,
                        InnerJoin<AUStepFill, On<AUStepFill.stepID, Equal<AUStep.stepID>>>>,
                            Where<
                                AUStepFill.isActive, Equal<True>,
                                And<AUStepFill.fieldName, Equal<stringRejected>,
                                And<AUStepFill.value, Equal<stringTrue>,
                                And<AUStepFilter.fieldName, Equal<stringBehavior>,
                                And<AUStepFilter.isActive, Equal<True>,
                                And<AUStepFilter.condition, Equal<int1>,
                                And<AUStepFilter.value, Equal<Required<SOOrderType.behavior>>>>>>>>>>();

        public static void SetSupportsApproval(PXGraph sender, SOOrderType row)
        {
            if (row == null)
                return;

			using (new PXConnectionScope())
			{
				PXView view = new PXView(sender, true, SupportsApprovalSelect);
				var r = view.SelectSingle(row.Behavior);
				row.SupportsApproval = r != null;
			}
        }

    }
}
