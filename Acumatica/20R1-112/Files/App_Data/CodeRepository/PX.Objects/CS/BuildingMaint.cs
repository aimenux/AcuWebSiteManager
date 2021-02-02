using PX.Data;
using PX.Objects.CR;
using PX.Objects.GL;
using System;

namespace PX.Objects.CS
{
    public class BuildingMaint : PXGraph<BuildingMaint, BuildingMaint.BuildingFilter>
    {
        [Serializable]
        public partial class BuildingFilter : IBqlTable
        {
            public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

            [Branch(typeof(AccessInfo.branchID), IsDBField = false)]
            public int? BranchID { get; set; }
        }

        public PXFilter<BuildingFilter> filter;
        public PXSelect<Building, Where<Building.branchID, Equal<Current<BuildingFilter.branchID>>>> building;
    }
}