using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class RequestForInformationRoleListAttribute : PXStringListAttribute
    {
        public const string Owner = "OW";
        public const string Approver = "AP";
        public const string GeneralContractor = "GC";
        public const string Subcontractor = "SC";
        public const string Vendor = "VE";
        public const string Customer = "CU";
        public const string TechnicalExpert = "TE";
        public const string RelatedEntity = "RE";

        public RequestForInformationRoleListAttribute()
            : base(new[]
            {
                Owner,
                Approver,
                GeneralContractor,
                Subcontractor,
                Vendor,
                Customer,
                TechnicalExpert,
                RelatedEntity
            }, new[]
            {
                "Owner",
                "Approver",
                "General Contractor",
                "Subcontractor",
                "Vendor",
                "Customer",
                "Technical Expert",
                "Related Entity"
            })
        {
        }

        public static List<string> GetContactRoles()
        {
            return new List<string>
            {
                Owner,
                Approver,
                GeneralContractor,
                Subcontractor,
                Vendor,
                Customer,
                TechnicalExpert
            };
        }

        public sealed class owner : BqlString.Constant<owner>
        {
            public owner()
                : base(Owner)
            {
            }
        }

        public sealed class approver : BqlString.Constant<approver>
        {
            public approver()
                : base(Approver)
            {
            }
        }

        public sealed class generalContractor : BqlString.Constant<generalContractor>
        {
            public generalContractor()
                : base(GeneralContractor)
            {
            }
        }

        public sealed class subcontractor : BqlString.Constant<subcontractor>
        {
            public subcontractor()
                : base(Subcontractor)
            {
            }
        }

        public sealed class vendor : BqlString.Constant<vendor>
        {
            public vendor()
                : base(Vendor)
            {
            }
        }

        public sealed class customer : BqlString.Constant<customer>
        {
            public customer()
                : base(Customer)
            {
            }
        }

        public sealed class technicalExpert : BqlString.Constant<technicalExpert>
        {
            public technicalExpert()
                : base(TechnicalExpert)
            {
            }
        }

        public sealed class relatedEntity : BqlString.Constant<relatedEntity>
        {
            public relatedEntity()
                : base(RelatedEntity)
            {
            }
        }
    }
}