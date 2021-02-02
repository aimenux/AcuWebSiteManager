namespace PX.Objects.AM.Attributes
{
    public class SchdPlanFlg
    {
        public const string DefaultFlag = "";
        public const string Plan = "P";
        public const string Move = "M";
        public const string Queue = "Q";

        //BQL constants declaration
        public class defaultFlag : PX.Data.BQL.BqlString.Constant<defaultFlag>
        {
            public defaultFlag() : base(DefaultFlag) { ;}
        }
        public class plan : PX.Data.BQL.BqlString.Constant<plan>
        {
            public plan() : base(Plan) { ;}
        }
        public class move : PX.Data.BQL.BqlString.Constant<move>
        {
            public move() : base(Move) { ;}
        }
        public class queue : PX.Data.BQL.BqlString.Constant<queue>
        {
            public queue() : base(Queue) { ;}
        }
    }
}
