using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.ProjectAccounting.Descriptor;

namespace PX.Objects.CN.ProjectAccounting.PM.Descriptor
{
    public class ProjectTaskType
    {
        public const string Cost = "Cost";
        public const string Revenue = "Rev";
        public const string CostRevenue = "CostRev";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] TaskTypesValues =
            {
	            Cost,
	            Revenue,
				CostRevenue
			};

            private static readonly string[] TaskTypesLabels =
            {
	            ProjectAccountingLabels.CostTask,
	            ProjectAccountingLabels.RevenueTask,
				ProjectAccountingLabels.CostAndRevenueTask
			};

			public ListAttribute()
                : base(TaskTypesValues, TaskTypesLabels)
            {
            }
        }

        public class cost : BqlString.Constant<cost>
        {
            public cost()
                : base(Cost)
            {
            }
        }

        public class revenue : BqlString.Constant<revenue>
        {
            public revenue()
                : base(Revenue)
            {
            }
        }

        public class costRevenue : BqlString.Constant<costRevenue>
        {
            public costRevenue()
                : base(CostRevenue)
            {
            }
        }
    }
}