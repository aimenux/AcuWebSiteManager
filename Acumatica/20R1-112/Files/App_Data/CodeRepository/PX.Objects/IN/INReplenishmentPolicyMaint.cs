using System;
using System.Collections;
using System.Linq;
using PX.Data;

namespace PX.Objects.IN
{
	public class INReplenishmentPolicyMaint : PXGraph<INReplenishmentPolicyMaint, INReplenishmentPolicy>
	{
		public PXSelect<INReplenishmentPolicy> Policies;

		[PXImport(typeof (INReplenishmentPolicy))]
        public PXSelect<INReplenishmentSeason, 
                    Where<INReplenishmentSeason.replenishmentPolicyID, Equal<Optional<INReplenishmentPolicy.replenishmentPolicyID>>>,
                    OrderBy<Asc<INReplenishmentSeason.startDate>>> Seasons;

        protected virtual void INReplenishmentSeason_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            INReplenishmentSeason row = (INReplenishmentSeason)e.Row;
            CheckSeasonOverlaps(sender, row);
        }

        protected virtual void INReplenishmentSeason_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
            var season = (INReplenishmentSeason)e.Row;
            if (CheckSeasonOverlaps(sender, season))
                throw new PXSetPropertyException(Messages.PeriodsOverlap);
        }

        protected virtual bool CheckSeasonOverlaps(PXCache cache, INReplenishmentSeason season)
        {
            foreach (INReplenishmentSeason otherSeason in this.Seasons.Select())
            {
                bool overlaps = false;

                if (otherSeason.SeasonID == season.SeasonID)
                    continue;

                if (IsDateInSeason(season.StartDate, otherSeason))
                {
                    overlaps = true;
                    cache.RaiseExceptionHandling<INReplenishmentSeason.startDate>(season, season.StartDate, new PXSetPropertyException(Messages.PeriodsOverlap));
                }

                if (IsDateInSeason(season.EndDate, otherSeason))
                {
                    overlaps = true;
                    cache.RaiseExceptionHandling<INReplenishmentSeason.endDate>(season, season.EndDate, new PXSetPropertyException(Messages.PeriodsOverlap));
                }

                if (IsDateInSeason(otherSeason.StartDate, season))
                {
                    overlaps = true;
                    cache.RaiseExceptionHandling<INReplenishmentSeason.startDate>(season, season.StartDate, new PXSetPropertyException(Messages.PeriodsOverlap));
                    cache.RaiseExceptionHandling<INReplenishmentSeason.endDate>(season, season.EndDate, new PXSetPropertyException(Messages.PeriodsOverlap));
                }

                if (overlaps)
                    return true;
            }

            return false;
        }

        public static bool IsDateInSeason(DateTime? date, INReplenishmentSeason season)
        {
            return season.StartDate <= date && date <= season.EndDate;
        }

		protected virtual void INReplenishmentSeason_Factor_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal) e.NewValue == 0)
			{
				throw new PXSetPropertyException(EP.Messages.ValueMustBeGreaterThanZero);
			}
		}
	}
}
