using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CA
{
    public class CADrCr : DrCr
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { CADebit, CACredit },
                new string[] { Messages.CADebit, Messages.CACredit })
            { }
        }

        public const string CADebit = Debit;
        public const string CACredit = Credit;

        public class cADebit : debit { }

        public class cACredit : credit { }

        public static decimal DebitAmt(string drCr, decimal curyTranAmt)
        {
            switch (drCr)
            {
                case Credit:
                    return (decimal)0.0;
                case Debit:
                    return curyTranAmt;
                default:
                    return (decimal)0.0;
            }
        }

        public static decimal CreditAmt(string drCr, decimal curyTranAmt)
        {
            switch (drCr)
            {
                case Credit:
                    return curyTranAmt;
                case Debit:
                    return (decimal)0.0;
                default:
                    return (decimal)0.0;
            }
        }
    }
}
