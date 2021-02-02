using System;
using System.Collections.Generic;
using System.Linq;

using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.DR.Descriptor;

using PX.Data;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.Common.Aging
{
	public static class AgingEngine
	{
		/// <param name="shortFormat">
		/// If set to <c>true</c>, then no "Past Due" or "Outstanding" postfix will
		/// be appended to the bucket description, making <paramref name="agingDirection"/>
		/// parameter irrelevant.
		/// </param>
		public static string GetDayAgingBucketDescription(
			int? lowerExclusiveBucketBoundary,
			int? upperInclusiveBucketBoundary,
			AgingDirection agingDirection,
			bool shortFormat)
		{
			if (lowerExclusiveBucketBoundary == null)
			{
				string description = agingDirection == AgingDirection.Backwards
					? Messages.Current
					: Messages.PastDue;

				return PXMessages.LocalizeNoPrefix(description);
			}
			else if (lowerExclusiveBucketBoundary != null && upperInclusiveBucketBoundary == null)
			{
				string descriptionFormat = shortFormat 
					? Messages.OverDays
					: agingDirection == AgingDirection.Backwards
						? Messages.OverDaysPastDue
						: Messages.OverDaysOutstanding;

				return PXMessages.LocalizeFormatNoPrefix(descriptionFormat, lowerExclusiveBucketBoundary.Value);
			}
			else if (lowerExclusiveBucketBoundary != null && upperInclusiveBucketBoundary != null)
			{
				string descriptionFormat = shortFormat
					? Messages.IntervalDays
					: agingDirection == AgingDirection.Backwards
						? Messages.IntervalDaysPastDue
						: Messages.IntervalDaysOutstanding;

				return PXMessages.LocalizeFormatNoPrefix(
					descriptionFormat,
					lowerExclusiveBucketBoundary.Value + 1,
					upperInclusiveBucketBoundary.Value);
			}
	
			return null;
		}

		/// <param name="shortFormat">
		/// If set to <c>true</c>, then no "Past Due" or "Outstanding" postfix will
		/// be appended to bucket descriptions, making <paramref name="agingDirection"/>
		/// parameter irrelevant.
		/// </param>
		public static IEnumerable<string> GetDayAgingBucketDescriptions(
			AgingDirection agingDirection,
			IEnumerable<int> bucketBoundaries,
			bool shortFormat)
		{
			if (bucketBoundaries == null) throw new ArgumentNullException(nameof(bucketBoundaries));

			if (!bucketBoundaries.Any())
			{
				yield return GetDayAgingBucketDescription(null, null, AgingDirection.Backwards, shortFormat);
				yield break;
			}

			int? currentLowerBoundary = null;
			int? currentUpperBoundary = null;

			// Generate descriptions for the "current" and 
			// fully bounded buckets.
			// -
			foreach (int boundary in bucketBoundaries)
			{
				currentLowerBoundary = currentUpperBoundary;
				currentUpperBoundary = boundary;

				yield return GetDayAgingBucketDescription(
					currentLowerBoundary, 
					currentUpperBoundary,
					agingDirection,
					shortFormat);
			}

			// Generate description for the "over" bucket.
			// -
			currentLowerBoundary = currentUpperBoundary;
			currentUpperBoundary = null;

			yield return GetDayAgingBucketDescription(
				currentLowerBoundary,
				currentUpperBoundary,
				agingDirection,
				shortFormat);
		}

	    [Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2019R2)]
	    public static IEnumerable<string> GetPeriodAgingBucketDescriptions(
	        IFinPeriodRepository finPeriodRepository,
	        DateTime currentDate,
	        AgingDirection agingDirection,
	        int numberOfBuckets) =>
	        GetPeriodAgingBucketDescriptions(finPeriodRepository, currentDate, agingDirection, numberOfBuckets, FinPeriod.organizationID.MasterValue, true);

	    public static IEnumerable<string> GetPeriodAgingBucketDescriptions(
	        IFinPeriodRepository finPeriodRepository,
	        DateTime currentDate,
	        AgingDirection agingDirection,
	        int numberOfBuckets,
            int calendarOrganizationID,
	        bool usePeriodDescription)
        {
			if (finPeriodRepository == null) throw new ArgumentNullException(nameof(finPeriodRepository));
			if (numberOfBuckets <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfBuckets));
						
			short periodStep = (short)(agingDirection == AgingDirection.Backwards ? -1 : 1);

			FinPeriod currentPeriod = finPeriodRepository.GetByID(
				finPeriodRepository.GetPeriodIDFromDate(currentDate, calendarOrganizationID),
			    calendarOrganizationID);

            yield return usePeriodDescription
                ? currentPeriod.Descr
                : FinPeriodUtils.FormatForError(currentPeriod.FinPeriodID);

			--numberOfBuckets;
			
			while (numberOfBuckets > 1)
			{
				currentPeriod = finPeriodRepository.GetByID(
					finPeriodRepository.GetOffsetPeriodId(currentPeriod.FinPeriodID, periodStep, calendarOrganizationID),
				    calendarOrganizationID);

			    yield return usePeriodDescription
			        ? currentPeriod.Descr
			        : FinPeriodUtils.FormatForError(currentPeriod.FinPeriodID);

                --numberOfBuckets;
			}

			if (numberOfBuckets > 0)
			{
				yield return PXMessages.LocalizeFormatNoPrefix(
					agingDirection == AgingDirection.Backwards
						? Messages.BeforeMonth
						: Messages.AfterMonth,
				    usePeriodDescription
				        ? currentPeriod.Descr
				        : FinPeriodUtils.FormatForError(currentPeriod.FinPeriodID));
			}
		}

		/// <summary>
		/// Given the current date and the aging bucket boundaries (in maximum 
		/// inclusive days from the current date), calculates the days difference
		/// between the current date and the test date, returning a zero-based number 
		/// of aging bucket that the test date falls into.
		/// </summary>
		/// <param name="bucketBoundaries">
		/// Upper inclusive boundaries, in days, of the aging buckets.
		/// The first element of this sequence defines the upper inclusive
		/// boundary of the current bucket (it is usually zero).
		/// The total number of buckets would be equal to the number of elements
		/// in the sequence, plus one. The values in the sequence should
		/// be strictly non-decreasing.
		/// </param>
		/// <returns>
		/// The number of the aging bucket that the <paramref name="dateToAge"/>
		/// falls into, in the [0; N] interval, where N is the the number of elements 
		/// in <paramref name="bucketBoundaries"/>. The value of N corresponds
		/// to the last aging bucket, which encompasses all dates that exceed
		/// the maximum bucket boundary.
		/// </returns>
		public static int AgeByDays(
			DateTime currentDate, 
			DateTime dateToAge,
			AgingDirection agingDirection,
			IEnumerable<int> bucketBoundaries)
		{
			if (bucketBoundaries == null) throw new ArgumentNullException(nameof(bucketBoundaries));
			if (!bucketBoundaries.Any()) return 0;

			if (agingDirection == AgingDirection.Forward)
			{
				agingDirection = AgingDirection.Backwards;
				Utilities.Swap(ref currentDate, ref dateToAge);
			}

			int days = currentDate.Subtract(dateToAge).Days;

			int bucketIndex = bucketBoundaries.FindIndex(boundary => boundary >= days);

			if (bucketIndex < 0)
			{
				bucketIndex = -bucketIndex - 1;
			}

			return bucketIndex;
		}

		public static int AgeByDays(
			DateTime currentAge,
			DateTime dateToAge,
			AgingDirection agingDirection,
			params int[] bucketBoundaries)
			=> AgeByDays(currentAge, dateToAge, agingDirection, bucketBoundaries as IEnumerable<int>);

	    [Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2019R2)]
        public static int AgeByPeriods(
	        DateTime currentDate,
	        DateTime dateToAge,
	        IFinPeriodRepository finPeriodRepository,
	        AgingDirection agingDirection,
	        int numberOfBuckets) => AgeByPeriods(
	        currentDate,
	        dateToAge,
	        finPeriodRepository,
	        agingDirection,
	        numberOfBuckets,
	        FinPeriod.organizationID.MasterValue);
        /// <summary>
        /// Given the current date and the number of period-based aging buckets, 
        /// returns the zero-based number of bucket that the specified test date 
        /// falls into.
        /// </summary>
        /// <param name="numberOfBuckets">
        /// The total number of period-based buckets, including the "Current" 
        /// and "Over" bucket. For backwards aging, the "Current" bucket encompasses 
        /// dates in the same (or later) financial period as the current date, and 
        /// the "Over" bucket corresponds to dates that are at least (numberOfBuckets - 1) 
        /// periods back in time from the current date.
        /// </param>
        public static int AgeByPeriods(
			DateTime currentDate, 
			DateTime dateToAge,
			IFinPeriodRepository finPeriodRepository,
			AgingDirection agingDirection,
			int numberOfBuckets,
			int organizationID)
		{
			if (finPeriodRepository == null) throw new ArgumentNullException(nameof(finPeriodRepository));
			if (numberOfBuckets <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfBuckets));

			if (agingDirection == AgingDirection.Forward)
			{
				agingDirection = AgingDirection.Backwards;
				Utilities.Swap(ref currentDate, ref dateToAge);
			}

			if (dateToAge > currentDate) return 0;

			int bucketNumber = finPeriodRepository
				.PeriodsBetweenInclusive(dateToAge, currentDate, organizationID)
				.Count();

			--bucketNumber;

			if (bucketNumber < 0)
			{
				// No financial periods found between the dates,
				// cannot proceed with aging.
				// -
				throw new PXException(GL.Messages.NoPeriodsDefined);
			}

			if (bucketNumber > numberOfBuckets - 1)
			{
				// Force into the last ("over") aging bucket.
				// -
				bucketNumber = numberOfBuckets - 1;
			}

			return bucketNumber;
		}
	}
}
