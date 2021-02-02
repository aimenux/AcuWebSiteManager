using System.Collections.Generic;
using PX.Objects.GL.FinPeriods.TableDefinition;


namespace PX.Objects.Common.Abstractions.Periods
{
    public class OrganizationDependedPeriodKey
    {
        public string PeriodID { get; set; }

        public int? OrganizationID { get; set; }

        public virtual bool Defined => PeriodID != null && OrganizationID != null;

        public virtual List<object> ToListOfObjects(bool skipPeriodID = false)
        {
            var values = new List<object>();

            if (!skipPeriodID)
            {
                values.Add(PeriodID);
            }

            values.Add(OrganizationID);

            return values;
        }

        public virtual bool IsNotPeriodPartsEqual(object otherKey)
        {
            return ((OrganizationDependedPeriodKey)otherKey).OrganizationID == OrganizationID;
        }

		public virtual bool IsMasterCalendar => OrganizationID == FinPeriod.organizationID.MasterValue;
    }
}
