namespace PX.Objects.GL
{
    public class SplitIcon
    {
		public class parent : PX.Data.BQL.BqlString.Constant<parent> { public parent() : base(Parent) { } }
		public class split : PX.Data.BQL.BqlString.Constant<split> { public split() : base(Split) { } }

        public const string Parent = "~/Icons/parent_cc.svg";
        public const string Split = "~/Icons/subdirectory_arrow_right_cc.svg";
    }
}
