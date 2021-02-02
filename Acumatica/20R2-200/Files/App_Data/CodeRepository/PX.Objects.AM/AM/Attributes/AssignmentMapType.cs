namespace PX.Objects.AM.Attributes
{
    public static class AssignmentMapType
    {
        public class AssignmentMapTypeAMECR: PX.Data.BQL.BqlString.Constant<AssignmentMapTypeAMECR>
        {
            public AssignmentMapTypeAMECR() : base(typeof(AMECRItem).FullName) { }
        }

        public class AssignmentMapTypeAMECO : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeAMECO>
        {
            public AssignmentMapTypeAMECO() : base(typeof(AMECOItem).FullName) { }
        }
    }
}
