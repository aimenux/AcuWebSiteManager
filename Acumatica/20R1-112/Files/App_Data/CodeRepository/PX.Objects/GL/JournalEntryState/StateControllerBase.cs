using System;
using System.Linq;
using System.Web;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.GL.JournalEntryState
{
	public abstract class StateControllerBase
	{
		protected readonly JournalEntry JournalEntry;

		protected PXCache<Batch> BatchCache => (PXCache<Batch>) JournalEntry.BatchModule.Cache;

		protected Ledger Ledger => (Ledger)JournalEntry.ledger.Current;

		protected PXCache<GLTran> GLTranCache => (PXCache<GLTran>)JournalEntry.GLTranModuleBatNbr.Cache;

		protected StateControllerBase(JournalEntry journalEntry)
		{
			JournalEntry = journalEntry;
		}


		#region Handlers

		public virtual void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var batch = e.Row as Batch;

			bool batchNotReleased = (batch.Released != true);
			bool batchPosted = (batch.Posted == true);
			bool batchVoided = (batch.Voided == true);
			bool batchModuleGL = (batch.Module == GL.BatchModule.GL);
			bool batchStatusInserted = (cache.GetStatus(batch) == PXEntryStatus.Inserted);

			bool isReclass = batch.BatchType == BatchTypeCode.Reclassification;
            bool isTrialBalance = batch.BatchType == BatchTypeCode.TrialBalance;
			bool isAllocation = batch.BatchType == BatchTypeCode.Allocation;

			bool allowCreateTaxTrans = PXAccess.FeatureInstalled<FeaturesSet.taxEntryFromGL>();
			allowCreateTaxTrans = allowCreateTaxTrans && batch != null && batch.Module == GL.BatchModule.GL;
			bool isViewSourceSupported = true;
			if (batch.Module == GL.BatchModule.GL && batch.BatchType != BatchTypeCode.TrialBalance
			    || batch.Module == GL.BatchModule.CM
			    || batch.Module == GL.BatchModule.FA)
			{
				isViewSourceSupported = false;
			}


			JournalEntry.viewDocument.SetEnabled(isViewSourceSupported);

			PXUIFieldAttribute.SetVisible<Batch.curyID>(cache, batch, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetVisible<Batch.createTaxTrans>(cache, batch, allowCreateTaxTrans);
			PXUIFieldAttribute.SetEnabled<Batch.createTaxTrans>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.autoReverseCopy>(cache, batch, false);

			if (!JournalEntry.IsImport || HttpContext.Current != null)
			{
				PXUIFieldAttribute.SetVisible<GLTran.tranDate>(GLTranCache, null, batch.Module != BatchModule.GL);

				PXUIFieldAttribute.SetVisible<GLTran.taxID>(GLTranCache, null, ShouldCreateTaxTrans(batch));
				PXUIFieldAttribute.SetVisible<GLTran.taxCategoryID>(GLTranCache, null, ShouldCreateTaxTrans(batch));
			}

			JournalEntry.release.SetEnabled(batchModuleGL && batchNotReleased && batch.Scheduled != true && batch.Status != BatchStatus.Hold && !batchVoided);
			JournalEntry.createSchedule.SetEnabled(batchModuleGL && !batchVoided && !batchStatusInserted && (batchNotReleased || batch.Scheduled == true) &&
				!isReclass && !isTrialBalance && !isAllocation);

			PXUIFieldAttribute.SetEnabled<Batch.module>(cache, batch);
			PXUIFieldAttribute.SetEnabled<Batch.batchNbr>(cache, batch);
			PXUIFieldAttribute.SetVisible<Batch.scheduleID>(cache, batch, false);
			PXUIFieldAttribute.SetVisible<Batch.curyControlTotal>(cache, batch, (bool)JournalEntry.glsetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetVisible<Batch.skipTaxValidation>(cache, batch, batch.CreateTaxTrans == true);

			JournalEntry.batchRegisterDetails.SetEnabled(!batchNotReleased);
			JournalEntry.glEditDetails.SetEnabled(batchNotReleased && !batchPosted && !batchStatusInserted);

			bool canReverse = CanReverseBatch(batch);
			JournalEntry.reverseBatch.SetEnabled(canReverse);
			JournalEntry.glReversingBatches.SetEnabled(batch.ReverseCount > 0);
			PXUIFieldAttribute.SetVisible<Batch.reverseCount>(cache, batch, batch.ReverseCount > 0);

			SetReclassifyButtonState(batch, out bool alreadyReclassified);

			PXUIFieldAttribute.SetVisible<GLTran.curyReclassRemainingAmt>(GLTranCache, null, batch.HasRamainingAmount == true);

			PXUIFieldAttribute.SetVisible<GLTran.origBatchNbr>(GLTranCache, null, isReclass);
            JournalEntry.editReclassBatch.SetVisible(isReclass);

			PXUIFieldAttribute.SetVisible<GLTran.reclassBatchNbr>(JournalEntry.GLTranModuleBatNbr.Cache, null, alreadyReclassified);
		}

		public virtual void GLTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e, Batch batch)
		{
			var tran = e.Row as GLTran;

			JournalEntry.SetReclassTranWarningsIfNeed(sender, tran);

			SetGLTranRefNbrRequired(tran, batch);

            if (ShouldCreateTaxTrans(batch))
			{
				PXFieldState state = (PXFieldState)sender.GetStateExt<GLTran.taxID>(tran); //Needed to prevent error from being overriden by warning
				if (String.IsNullOrEmpty(state.Error) || state.IsWarning == true)
				{
					WarnIfMissingTaxID(tran);
				}
			}

			tran.IncludedInReclassHistory = JournalEntry.CanShowReclassHistory(tran, batch.BatchType);
		}

		public virtual void Batch_CreateTaxTrans_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetGLTranRefNbrRequired(null, (Batch)e.Row);
		}

		public virtual void GLTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, Batch batch)
		{
			if (ShouldCreateTaxTrans(batch) == false)
			{
				e.Cancel = true;
			}
		}

		#endregion


		#region Service

		protected bool IsTaxTranCreationAllowed(Batch batch)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.taxEntryFromGL>()
					   && batch != null
					   && batch.Module == BatchModule.GL;
		}

		public bool ShouldCreateTaxTrans(Batch batch)
		{
			return IsTaxTranCreationAllowed(batch)
						&& batch.CreateTaxTrans == true;
		}

		private void SetGLTranRefNbrRequired(GLTran tran, Batch batch)
		{
			bool refNbrRequired = JournalEntry.glsetup.Current.RequireRefNbrForTaxEntry == true
									&& batch.CreateTaxTrans == true
									&& PXAccess.FeatureInstalled<FeaturesSet.taxEntryFromGL>();

			var persistingCheck = refNbrRequired
										? PXPersistingCheck.NullOrBlank
										: PXPersistingCheck.Nothing;

			PXDefaultAttribute.SetPersistingCheck<GLTran.refNbr>(GLTranCache, tran, persistingCheck);
		}

        private void SetReclassifyButtonState(Batch batch, out bool alreadyReclassified)
		{
			alreadyReclassified = false;

			if (JournalEntry.UnattendedMode)
			{
				// AC-89516 - the flag is used as a proxy for graph not being worked with
				// from the UI. The reasoning behind this is that on databases with many GL 
				// transactions the below select of GLTrans takes up a considerable amount of 
				// time during mass AR documents release. 
				// TODO: perhaps all UI state controlling should be disabled in unattended mode, 
				// but we were afraid of regression at the moment.
				// -
				return;
			}

			var enabled = Ledger != null && JournalEntry.IsBatchReclassifiable(batch, Ledger);

			if (enabled)
			{
				var trans = JournalEntry.GLTranModuleBatNbr.Select().RowCast<GLTran>();
				alreadyReclassified = trans.Any(tran => tran.ReclassBatchNbr != null);
				batch.HasRamainingAmount = trans.Any(tran => (tran.ReclassRemainingAmt ?? 0m) != 0m);
				enabled = trans.Any(tran => JournalEntry.IsTransactionReclassifiable(tran, batch.BatchType, Ledger.BalanceType, ProjectDefaultAttribute.NonProject()));
			}

			JournalEntry.reclassify.SetEnabled(enabled);
		}

		private void WarnIfMissingTaxID(GLTran tran)
		{
			bool needWarning = false;
			if (tran != null && tran.AccountID != null && tran.SubID != null && tran.TaxID == null)
			{
				PXResultset<TX.Tax> taxset = PXSelect<TX.Tax, Where2<Where<TX.Tax.purchTaxAcctID, Equal<Required<GLTran.accountID>>,
							And<TX.Tax.purchTaxSubID, Equal<Required<GLTran.subID>>>>,
							Or<Where<TX.Tax.salesTaxAcctID, Equal<Required<GLTran.accountID>>,
							And<TX.Tax.salesTaxSubID, Equal<Required<GLTran.subID>>>>>>>.Select(JournalEntry, tran.AccountID, tran.SubID, tran.AccountID, tran.SubID);

				if (taxset.Count > 0 && tran.TaxID == null)
				{
					needWarning = true;
				}
			}
			if (tran != null)
			{
				if (needWarning)
				{
					GLTranCache.RaiseExceptionHandling<GLTran.taxID>(tran, null, new PXSetPropertyException(Messages.TaxIDMissingForAccountAssociatedWithTaxes, PXErrorLevel.Warning));
				}
				else
				{
					GLTranCache.RaiseExceptionHandling<GLTran.taxID>(tran, null, null);
				}
			}
		}

		private bool CanReverseBatch(Batch batch)
		{
			bool canReverse = (batch.Released == true)
				&& (batch.Module != GL.BatchModule.CM)
				&& batch.BatchType != BatchTypeCode.TrialBalance;

			if (batch.BatchType == BatchTypeCode.Reclassification)
			{
				GLTran tran = (GLTran)PXSelect<GLTran,
												Where<GLTran.module, Equal<Required<Batch.module>>,
													And<GLTran.batchNbr, Equal<Required<Batch.batchNbr>>,
													And<GLTran.reclassBatchNbr, IsNotNull>>>>
												.Select(JournalEntry, batch.Module, batch.BatchNbr);

				canReverse = canReverse && tran == null;
			}

			return canReverse;
		}

		#endregion
	}
}
