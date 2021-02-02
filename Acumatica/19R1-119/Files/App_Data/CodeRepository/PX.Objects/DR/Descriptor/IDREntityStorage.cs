using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.DR.Descriptor
{
	public interface IDREntityStorage
	{
		/// <summary>
		/// Retrieve all schedule details for the specified 
		/// deferral schedule ID.
		/// </summary>
		IList<DRScheduleDetail> GetScheduleDetails(int? scheduleID);

		/// <summary>
		/// Retrieve all deferral transactions that match the given
		/// <see cref="DRScheduleDetail.ScheduleID"/> and <see cref="DRScheduleDetail.ComponentID"/>.
		/// </summary>
		IList<DRScheduleTran> GetDeferralTransactions(int? scheduleID, int? componentID, int? detailLineNbr);

		/// <summary>
		/// Retrieve the deferral code by its ID.
		/// </summary>
		DRDeferredCode GetDeferralCode(string deferralCodeID);

		/// <summary>
		/// Create an independent copy of the original deferral schedule.
		/// </summary>
		DRSchedule CreateCopy(DRSchedule originalSchedule);

		/// <summary>
		/// Create an independent copy of the original deferral transaction.
		/// </summary>
		DRScheduleTran CreateCopy(DRScheduleTran originalTransaction);

		/// <summary>
		/// Add a new schedule to the internal storage (e.g. <see cref="PXCache"/>)
		/// </summary>
		/// <returns>
		/// The added record where some record fields may have been further 
		/// changed according to the internal storage rules.
		/// </returns>
		DRSchedule Insert(DRSchedule schedule);

		/// <summary>
		/// Update a deferral schedule in the internal storage (e.g. <see cref="PXCache"/>)
		/// </summary>
		/// <returns>
		/// The updated record where some record fields may have been further 
		/// changed according to the internal storage rules.
		/// </returns>
		DRSchedule Update(DRSchedule schedule);

		/// <summary>
		/// Insert a deferral schedule detail into the internal storage (e.g. <see cref="PXCache"/>)
		/// </summary>
		/// <returns>
		/// The updated record where some record fields may have been further 
		/// changed according to the internal storage rules.
		/// </returns>
		DRScheduleDetail Insert(DRScheduleDetail scheduleDetail);

		/// <summary>
		/// Update a deferral schedule in the internal storage (e.g. <see cref="PXCache"/>)
		/// </summary>
		/// <returns>
		/// The updated record where some record fields may have been further 
		/// changed according to the internal storage rules.
		/// </returns>
		DRScheduleDetail Update(DRScheduleDetail scheduleDetail);

		/// <summary>
		/// Handle the event whereby a deferral transaction has been modified.
		/// </summary>
		/// <param name="oldTransaction">The deferral transaction state prior to the update.</param>
		/// <param name="newTransaction">The deferral transaction state after the update.</param>
		/// <param name="scheduleDetail">The schedule detail that this deferral transaction corresponds to.</param>
		/// <param name="deferralCode">The deferral code of the schedule detail.</param>
		void ScheduleTransactionModified(
			DRScheduleDetail scheduleDetail,
			DRDeferredCode deferralCode,
			DRScheduleTran oldTransaction,
			DRScheduleTran newTransaction);

		/// <summary>
		/// Create a credit line transaction for the specified schedule detail.
		/// </summary>
		void CreateCreditLineTransaction(
			DRScheduleDetail scheduleDetail,
			DRDeferredCode deferralCode,
			int? branchID);

		/// <summary>
		/// Create deferral transactions for the specified schedule detail.
		/// </summary>
		IEnumerable<DRScheduleTran> CreateDeferralTransactions(
			DRSchedule deferralSchedule,
			DRScheduleDetail scheduleDetail,
			DRDeferredCode deferralCode,
			int? branchID);

		/// <summary>
		/// Handle the event when the schedule creator has prepared (generated or re-evaluated)
		/// the deferral transactions in non-draft mode, which means e.g. that projection balance
		/// tables must be updated in the system.
		/// </summary>
		void NonDraftDeferralTransactionsPrepared(
			DRScheduleDetail scheduleDetail,
			DRDeferredCode deferralCode,
			IEnumerable<DRScheduleTran> deferralTransactions);
	}
}