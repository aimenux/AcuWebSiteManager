using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Configuration Roll-up Options
    /// </summary>
    public class RollupOptions
    {
        public const string Parent = "PA";
        public const string ChildrenAll = "CA";
        public const string ChildrenCFG = "CC";
        public const string ParentChildren = "PC";

        public static class Desc
        {
            public static string Parent => Messages.GetLocal(Messages.Parent);
            public static string ChildrenAll => Messages.GetLocal(Messages.ChildrenAll);
            public static string ChildrenCFG => Messages.GetLocal(Messages.ChildrenCFG);
            public static string ParentChildren => Messages.GetLocal(Messages.ParentChildren);
        }

        //BQL constants declaration
        public class parent : PX.Data.BQL.BqlString.Constant<parent>
        {
            public parent() : base(Parent) { }
        }
        public class childrenAll : PX.Data.BQL.BqlString.Constant<childrenAll>
        {
            public childrenAll() : base(ChildrenAll) { }
        }
        public class childrenCFG : PX.Data.BQL.BqlString.Constant<childrenCFG>
        {
            public childrenCFG() : base(ChildrenCFG) { }
        }
        public class parentChildren : PX.Data.BQL.BqlString.Constant<parentChildren>
        {
            public parentChildren() : base(ParentChildren) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { Parent, ChildrenAll, ChildrenCFG, ParentChildren },
                    new string[] { Messages.Parent, Messages.ChildrenAll, Messages.ChildrenCFG, Messages.ParentChildren })
            { }
        }
    }
}