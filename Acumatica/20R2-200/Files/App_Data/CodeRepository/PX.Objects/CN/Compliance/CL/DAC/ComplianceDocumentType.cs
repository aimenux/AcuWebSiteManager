using PX.Common;
using PX.Data.BQL;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [PXLocalizable]
    public class ComplianceDocumentType
    {
        public const string Certificate = "Certificate";
        public const string Insurance = "Insurance";
        public const string LienWaiver = "Lien Waiver";
        public const string Notice = "Notice";
        public const string Other = "Other";
        public const string Status = "Status";

        public class status : BqlString.Constant<status>
        {
            public status()
                : base(Status)
            {
            }
        }

        public class certificate : BqlString.Constant<certificate>
        {
            public certificate()
                : base(Certificate)
            {
            }
        }

        public class other : BqlString.Constant<other>
        {
            public other()
                : base(Other)
            {
            }
        }

        public class lienWaiver : BqlString.Constant<lienWaiver>
        {
            public lienWaiver()
                : base(LienWaiver)
            {
            }
        }

        public class insurance : BqlString.Constant<insurance>
        {
            public insurance()
                : base(Insurance)
            {
            }
        }

        public class notice : BqlString.Constant<notice>
        {
            public notice()
                : base(Notice)
            {
            }
        }
    }
}