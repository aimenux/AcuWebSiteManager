using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// MRP Supply/Demand
    /// </summary>
    public class MRPSDFlag
    {
        public const string Unknown = "";
        public const string Supply = "S";
        public const string Demand = "D";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Supply => Messages.GetLocal(Messages.Supply);
            public static string Demand => Messages.GetLocal(Messages.Demand);
        }

        public static string GetDescription(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Messages.GetLocal(Messages.Unknown);
            }

            try
            {
                var x = new ListAttribute();
                return x.ValueLabelDic[id];
            }
            catch
            {
                return Messages.GetLocal(Messages.Unknown);
            }
        }

        //BQL constants declaration
        public class supply : PX.Data.BQL.BqlString.Constant<supply>
        {
            public supply() : base(Supply) { ;}
        }
        public class demand : PX.Data.BQL.BqlString.Constant<demand>
        {
            public demand() : base(Demand) { ;}
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[] { Supply, Demand },
                    new string[] { Messages.Supply, Messages.Demand })
            { ; }
        }
    }
}