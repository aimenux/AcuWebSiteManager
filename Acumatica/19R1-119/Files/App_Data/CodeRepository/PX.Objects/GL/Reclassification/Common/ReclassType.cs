using System;
using PX.Data;

namespace PX.Objects.GL
{
    public class ReclassType
    {
        public class List : PXStringListAttribute
        {
            public List()
                : base(
                new string[] { Common, Split },
                new string[] { Messages.CommonReclassType, Messages.Split })
            { }
        }

        public const string Common = "C";
        public const string Split = "S";
        
        
        public class common : PX.Data.BQL.BqlString.Constant<common>
		{
            public common() : base(Common) { }
        }

        public class split : PX.Data.BQL.BqlString.Constant<split>
		{
            public split() : base(Split) { }
        }
    }
}
