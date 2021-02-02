using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.Common.Extensions;


namespace PX.Objects.CA
{
	public class CAReconEntry : PXGraph<CAReconEntry>
	{
		#region Internal Type Definitions

		#region CATrans
		[Serializable]
		public partial class CATranExt : CATran
		{
			#region BqlFields override
			public abstract new class tranID : PX.Data.BQL.BqlLong.Field<tranID>
			{
			}
			public abstract new class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
			{
			}
			public abstract new class origModule : PX.Data.BQL.BqlString.Field<origModule>
			{
			}
			public abstract new class origTranType : PX.Data.BQL.BqlString.Field<origTranType>
			{
			}
			public abstract new class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>
			{
			}
			public abstract new class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr>
			{
			}
			public abstract new class isPaymentChargeTran : PX.Data.BQL.BqlBool.Field<isPaymentChargeTran>
			{
			}
			public abstract new class reconNbr : PX.Data.BQL.BqlString.Field<reconNbr>
			{
			}
			public abstract new class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate>
			{
			}
			public new abstract class voidedTranID : PX.Data.BQL.BqlLong.Field<voidedTranID>
			{
			}
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released>
			{
			}
			public new abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled>
			{
			}
			public new abstract class reconDate : PX.Data.BQL.BqlDateTime.Field<reconDate>
			{
			}
			public new abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared>
			{
			}
			public new abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate>
			{
			}
			#endregion

			#region tstamp
			public new class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
			{
			}
			[PXDBTimestamp(RecordComesFirst = true)]
			public override byte[] tstamp
			{
				get;
				set;
			}
			#endregion

			#region CuryEffTranAmt
			public abstract class curyEffTranAmt : PX.Data.BQL.BqlDecimal.Field<curyEffTranAmt>
			{
			}
			[PXCury(typeof(curyID))]
			[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible)]
			public virtual decimal? CuryEffTranAmt
			{
				get;
				set;
			}
			#endregion
			#region EffTranAmt
			public abstract class effTranAmt : PX.Data.BQL.BqlDecimal.Field<effTranAmt>
			{
			}
			[PXDecimal(4)]
			[PXUIField(DisplayName = "Tran. Amount")]
			public virtual decimal? EffTranAmt
			{
				get;
				set;
			}
			#endregion
			#region CuryEffDebitAmt
			public abstract class curyEffDebitAmt : PX.Data.BQL.BqlDecimal.Field<curyEffDebitAmt>
			{
			}

			[PXDecimal]
			[PXUIField(DisplayName = "Receipt")]
			public virtual decimal? CuryEffDebitAmt
			{
				[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
				get
				{
					if (DrCr == null)
					{
						return null;
					}
					else
					{
						return (DrCr == CADrCr.CADebit) ? CuryEffTranAmt : 0m;
					}
				}
				set
				{

				}
			}
			#endregion
			#region CuryEffCreditAmt
			public abstract class curyEffCreditAmt : PX.Data.BQL.BqlDecimal.Field<curyEffCreditAmt>
			{
			}
			[PXDecimal]
			[PXUIField(DisplayName = "Disbursement")]
			public virtual decimal? CuryEffCreditAmt
			{
				[PXDependsOnFields(typeof(drCr), typeof(curyEffTranAmt))]
				get
				{
					if (this.DrCr == null)
					{
						return null;
					}
					else
					{
						return (DrCr == CADrCr.CACredit) ? -CuryEffTranAmt : 0m;
					}
				}
				set
				{

				}
			}
			#endregion
			#region CuryEffClearedDebitAmt
			public abstract class curyEffClearedDebitAmt : PX.Data.BQL.BqlDecimal.Field<curyEffClearedDebitAmt>
			{
			}
			[PXDecimal]
			[PXUIField(DisplayName = "Receipt")]
			public virtual decimal? CuryEffClearedDebitAmt
			{
				[PXDependsOnFields(typeof(cleared), typeof(drCr), typeof(curyEffDebitAmt))]
				get
				{
					return (Cleared == true ? (CuryEffDebitAmt ?? decimal.Zero) : decimal.Zero);
				}
				set
				{
				}
			}
			#endregion
			#region CuryEffClearedCreditAmt
			public abstract class curyEffClearedCreditAmt : PX.Data.BQL.BqlDecimal.Field<curyEffClearedCreditAmt>
			{
			}
			[PXDecimal]
			[PXUIField(DisplayName = "Disbursement")]
			public virtual decimal? CuryEffClearedCreditAmt
			{
				[PXDependsOnFields(typeof(cleared), typeof(drCr), typeof(curyTranAmt))]
				get
				{
					return (Cleared == true && DrCr == CADrCr.CACredit) ? -CuryEffTranAmt : 0m;
				}
				set
				{
				}
			}
			#endregion
			#region Reconcilation Members
			#region ReconciledDebit
			public abstract class reconciledDebit : PX.Data.BQL.BqlDecimal.Field<reconciledDebit>
			{
			}
			[PXDecimal]
			[PXFormula(null, typeof(SumCalc<CARecon.reconciledDebits>))]
			public virtual decimal? ReconciledDebit
			{
				[PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(effTranAmt), typeof(skipCount))]
				get
				{
					if (this.SkipCount == true)
					{
						return (Reconciled == true && DrCr == CADrCr.CACredit) ? EffTranAmt : 0m;
					}
					else
					{
						return (Reconciled == true && DrCr == CADrCr.CADebit) ? EffTranAmt : 0m;
					}
				}
				set
				{
				}
			}
			#endregion
			#region CuryReconciledDebit
			public abstract class curyReconciledDebit : PX.Data.BQL.BqlDecimal.Field<curyReconciledDebit>
			{
			}
			[PXDecimal]
			[PXFormula(null, typeof(SumCalc<CARecon.curyReconciledDebits>))]
			public virtual decimal? CuryReconciledDebit
			{
				[PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(curyEffTranAmt), typeof(skipCount))]
				get
				{
					if (this.SkipCount == true)
					{
						return (this.Reconciled == true && this.DrCr == CADrCr.CACredit) ? this.CuryEffTranAmt : 0m;
					}
					else
					{
						return (this.Reconciled == true && this.DrCr == CADrCr.CADebit) ? this.CuryEffTranAmt : 0m;
					}
				}
				set
				{
				}
			}
			#endregion
			#region ReconciledCredit
			public abstract class reconciledCredit : PX.Data.BQL.BqlDecimal.Field<reconciledCredit>
			{
			}
			[PXDecimal]
			[PXFormula(null, typeof(SumCalc<CARecon.reconciledCredits>))]
			public virtual decimal? ReconciledCredit
			{
				[PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(effTranAmt), typeof(skipCount))]
				get
				{
					if (this.SkipCount == true)
					{
						return (Reconciled == true && DrCr == CADrCr.CADebit) ? -EffTranAmt : 0m;
					}
					else
					{
						return (Reconciled == true && DrCr == CADrCr.CACredit) ? -EffTranAmt : 0m;
					}
				}
				set
				{
				}
			}
			#endregion
			#region CuryReconciledCredit
			public abstract class curyReconciledCredit : PX.Data.BQL.BqlDecimal.Field<curyReconciledCredit>
			{
			}
			[PXDecimal]
			[PXFormula(null, typeof(SumCalc<CARecon.curyReconciledCredits>))]
			public virtual decimal? CuryReconciledCredit
			{
				[PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(curyEffTranAmt), typeof(skipCount))]
				get
				{
					if (SkipCount == true)
					{
						return (Reconciled == true && DrCr == CADrCr.CADebit) ? -CuryEffTranAmt : 0m;
					}
					else
					{
						return (Reconciled == true && DrCr == CADrCr.CACredit) ? -CuryEffTranAmt : 0m;
					}
				}
				set
				{
				}
			}
			#endregion
			#region SkipCount
			public abstract class skipCount : PX.Data.BQL.BqlBool.Field<skipCount>
			{
			}
			/// <summary>
			/// This flag is required for the specific situation when SkipVoids is true and 
			/// transaction was included to Reconcillation Statement before it was voided and then voiding transaction was included.
			/// </summary>
			/// <value>
			/// If this flag is set to true Transaction will not be counted in Reconcillation Statement header and 
			/// Transaction's credit amount will be subtracted from Reconcillation Statement's debit amount 
			/// instead of adding to Reconcillation Statement's credit amount and 
			/// Transaction's debit amount will be subtracted from Reconcillation Statement's credit amount 
			/// instead of adding to Reconcillation Statement's debit amount.  
			/// </value>
			[PXBool]
			public virtual bool? SkipCount
			{
				get;
				set;
			}
			#endregion
			#region CountDebit
			public abstract class countDebit : PX.Data.BQL.BqlInt.Field<countDebit>
			{
			}
			[PXInt]
			[PXFormula(null, typeof(SumCalc<CARecon.countDebit>))]
			public virtual int? CountDebit
			{
				[PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(skipCount))]
				get
				{
					return (Reconciled == true && DrCr == CADrCr.CADebit && SkipCount != true) ? 1 : 0;
				}
				set
				{
				}
			}
			#endregion
			#region CountCredit
			public abstract class countCredit : PX.Data.BQL.BqlInt.Field<countCredit>
			{
			}
			[PXInt]
			[PXFormula(null, typeof(SumCalc<CARecon.countCredit>))]
			public virtual int? CountCredit
			{
				[PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(skipCount))]
				get
				{
					return (Reconciled == true && DrCr == CADrCr.CACredit && SkipCount != true) ? 1 : 0;
				}
				set
				{
				}
			}
			#endregion
			#endregion
			#region Voided
			public abstract class voided : PX.Data.BQL.BqlBool.Field<voided>
			{
			}
			[PXBool]
			[PXUIField(DisplayName = "Voided")]
			public virtual bool? Voided
			{
				get;
				set;
			}
			#endregion
			#region VoidingTranID
			public abstract class voidingTranID : PX.Data.BQL.BqlLong.Field<voidingTranID>
			{
			}
			[PXLong]
			public virtual long? VoidingTranID
			{
				get;
				set;
			}
			#endregion
			#region VoidingNotReleased
			public abstract class voidingNotReleased : PX.Data.BQL.BqlBool.Field<voidingNotReleased>
			{
			}
			[PXBool]
			public virtual bool? VoidingNotReleased
			{
				get;
				set;
			}
			#endregion
			#region Status
			public abstract new class status : PX.Data.BQL.BqlString.Field<status>
			{
			}
			[PXString(11, IsFixed = true)]
			[PXUIField(DisplayName = "Status", Enabled = false)]
			[ExtTranStatus.List]
			public override string Status
			{
				[PXDependsOnFields(typeof(posted), typeof(released), typeof(hold), typeof(voided), typeof(voidingNotReleased))]
				get
				{
					if (VoidingNotReleased == true)
					{
						return ExtTranStatus.PartiallyVoided;
					}
					else if (Voided == true)
					{
						return BatchStatus.Voided;
					}
					else
						if (Posted == true)
					{
						if (Released == true)
							return BatchStatus.Posted;
						else
							return BatchStatus.Unposted;
					}
					else if (Released == true && Posted != true)
					{
						return BatchStatus.Released;
					}
					else if (Hold == true)
					{
						return BatchStatus.Hold;
					}
					else
					{
						return BatchStatus.Balanced;
					}
				}
				set
				{
				}
			}
			#endregion
			#region ProcessedFromFeed
			public abstract class processedFromFeed : PX.Data.BQL.BqlBool.Field<processedFromFeed> { }

			[PXBool]
			public bool? ProcessedFromFeed
			{
				get;
				set;
			}
			#endregion

			public class ExtTranStatus : BatchStatus
			{
				public new class ListAttribute : PXStringListAttribute
				{
					public ListAttribute()
						: base(
					new string[] { Hold, Balanced, Unposted, Posted, Completed, Voided, Released, PartiallyReleased, Scheduled, PartiallyVoided },
							new string[] { Messages.Hold, Messages.Balanced, GL.Messages.Unposted, GL.Messages.Posted, GL.Messages.Completed, Messages.Voided, Messages.Released, GL.Messages.PartiallyReleased, GL.Messages.Scheduled, Messages.VoidPendingStatus })
					{ }
				}
				public const string PartiallyVoided = "PV";
			}
		}
		[Serializable]
		[UseIndexWhenJoined("CATran_VoidedTranID", On = typeof(On<CATranVoiding.voidedTranID, Equal<CATranExt.tranID>>))]
		[PXCacheName(Messages.CATran2)]
		public partial class CATranVoiding : CATran
		{
			#region BqlFields override
			public abstract new class tranID : PX.Data.BQL.BqlLong.Field<tranID>
			{
			}
			public abstract new class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
			{
			}
			public abstract new class origModule : PX.Data.BQL.BqlString.Field<origModule>
			{
			}
			public abstract new class origTranType : PX.Data.BQL.BqlString.Field<origTranType>
			{
			}
			public abstract new class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>
			{
			}
			public abstract new class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr>
			{
			}
			public abstract new class isPaymentChargeTran : PX.Data.BQL.BqlBool.Field<isPaymentChargeTran>
			{
			}
			public abstract new class reconNbr : PX.Data.BQL.BqlString.Field<reconNbr>
			{
			}
			public abstract new class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate>
			{
			}
			public new abstract class voidedTranID : PX.Data.BQL.BqlLong.Field<voidedTranID>
			{
			}
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released>
			{
			}
			public new abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled>
			{
			}
			public new abstract class reconDate : PX.Data.BQL.BqlDateTime.Field<reconDate>
			{
			}
			public new abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared>
			{
			}
			public new abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate>
			{
			}
			#endregion
		}
		[Serializable]
		public partial class CATranInBatch : CATran
		{
			#region BqlFields override
			public abstract new class tranID : PX.Data.BQL.BqlLong.Field<tranID>
			{
			}
			public abstract new class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
			{
			}
			public abstract new class origModule : PX.Data.BQL.BqlString.Field<origModule>
			{
			}
			public abstract new class origTranType : PX.Data.BQL.BqlString.Field<origTranType>
			{
			}
			public abstract new class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>
			{
			}
			public abstract new class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr>
			{
			}
			public abstract new class isPaymentChargeTran : PX.Data.BQL.BqlBool.Field<isPaymentChargeTran>
			{
			}
			public abstract new class reconNbr : PX.Data.BQL.BqlString.Field<reconNbr>
			{
			}
			public abstract new class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate>
			{
			}
			public new abstract class voidedTranID : PX.Data.BQL.BqlLong.Field<voidedTranID>
			{
			}
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released>
			{
			}
			public new abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled>
			{
			}
			public new abstract class reconDate : PX.Data.BQL.BqlDateTime.Field<reconDate>
			{
			}
			public new abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared>
			{
			}
			public new abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate>
			{
			}
			#endregion


			public void CopyFrom(CATran source)
			{
				this.ClearDate = source.ClearDate;
				this.Cleared = source.Cleared;
				this.Reconciled = source.Reconciled;
				this.ReconDate = source.ReconDate;
				this.ReconNbr = source.ReconNbr;
			}
		}
		#endregion

		[Serializable]
		public partial class CABatchDetail2 : CABatchDetail
		{
			#region BatchNbr
			public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr>
			{
			}
			#endregion
			#region OrigModule
			public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule>
			{
			}
			#endregion
			#region OrigDocType
			public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType>
			{
			}
			#endregion
			#region OrigRefNbr
			public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>
			{
			}
			#endregion
		}
		[Serializable]
		public partial class CABankTranMatch2 : CABankTranMatch
		{
			#region CATranID
			public new abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID>
			{
			}
			#endregion
			#region TranID
			public new abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID>
			{
			}
			#endregion
		}
		#endregion

		#region Buttons Definition
		public PXSave<CARecon> Save;
		#region Button Cancel
		public PXCancel<CARecon> Cancel;
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable cancel(PXAdapter a)
		{
			CARecon current = null;
			CARecon cachesRecon = null;
			PXSelectBase<CARecon> currentSelectStatement;
			object cashAccountCD = a.Searches != null && a.Searches.Length > 0
				? a.Searches[0]
				: null;

			object reconNbr = a.Searches != null && a.Searches.Length > 1
				? a.Searches[1]
				: null;

			foreach (PXResult<CARecon> recon in (new PXCancel<CARecon>(this, "Cancel")).Press(a))
			{
				cachesRecon = recon;
			}

			CashAccount acct = PXSelect<CashAccount,
				Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(this, cashAccountCD);

			if (acct != null)
			{
				bool inserted = (cachesRecon != null && CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted);
				bool manualRefFilled = false;
				if (inserted)
				{
					Numbering numbering = PXSelect<Numbering, Where<Numbering.numberingID, Equal<Required<CashAccount.reconNumberingID>>>>.Select(this, acct.ReconNumberingID);
					manualRefFilled = (numbering != null && numbering.UserNumbering == true);
				}

				if (reconNbr != null && !manualRefFilled)
				{
					currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																			And<CARecon.reconNbr, Equal<Required<CARecon.reconNbr>>>>>(this);
					current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID, reconNbr);
				}

				if (current == null && !manualRefFilled)
				{
					currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																			And<CARecon.reconciled, Equal<boolFalse>, And<CARecon.voided, Equal<False>>>>,
																		OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
					current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID);
				}

				if (current == null)
				{
					if (inserted)
					{
						current = cachesRecon;
					}
					else
					{
						current = new CARecon();
						current.CashAccountID = acct.CashAccountID;
						current = CAReconRecords.Insert(current);
					}
				}
				if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
				{
					if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
					{
						CAReconRecords.Delete(cachesRecon);
						CAReconRecords.Cache.IsDirty = false;
					}
					CAReconRecords.Current = current;
				}
			}
			yield return current;

			// return new PXCancel<CARecon>(this, "Cancel").Press(a);
		}
		#endregion
		public PXInsert<CARecon> Insert;
		#region Button Delete
		public PXDelete<CARecon> Delete;
		[PXDeleteButton]
		[PXUIField]
		protected virtual IEnumerable delete(PXAdapter a)
		{
			CARecon current = null;
			object cashAccountCD = a.Searches != null && a.Searches.Length > 0
				? a.Searches[0]
				: null;
			foreach (PXResult<CARecon> headerDeleted in (new PXDelete<CARecon>(this, "Delete")).Press(a))
			{
				current = headerDeleted;
			}

			CashAccount acct = PXSelect<CashAccount,
				Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(this, cashAccountCD);

			if (acct != null)
			{
				PXSelectBase<CARecon> activitySelect = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																					   And<CARecon.reconciled, Equal<boolFalse>>>,
																					 OrderBy<Asc<CARecon.cashAccountID, Desc<CARecon.reconNbr>>>>(this);
				current = (CARecon)activitySelect.View.SelectSingle(acct.CashAccountID);
			}

			if (current == null)
			{
				current = new CARecon();
				current.CashAccountID = acct.CashAccountID;
				current = CAReconRecords.Insert(current);
				CAReconRecords.Cache.SetStatus(current, PXEntryStatus.Inserted);
			}

			yield return current;
		}
		#endregion
		#region Button First
		public PXFirstOrderBy<CARecon, OrderBy<Asc<CARecon.cashAccountID, Asc<CARecon.reconNbr>>>> First;
		[PXFirstButton]
		[PXUIField]
		protected virtual IEnumerable first(PXAdapter a)
		{
			PXLongOperation.ClearStatus(this.UID);
			CARecon current = null;
			CARecon cachesRecon = null;
			PXSelectBase<CARecon> currentSelectStatement;
			object cashAccountCD = a.Searches != null && a.Searches.Length > 0
				? a.Searches[0]
				: null;

			object reconNbr = a.Searches != null && a.Searches.Length > 1
				? a.Searches[1]
				: null;

			foreach (PXResult<CARecon> recon in (new PXFirst<CARecon>(this, "First")).Press(a))
			{
				cachesRecon = recon;
			}

			CashAccount acct = PXSelect<CashAccount,
				Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(this, cashAccountCD);

			if (acct != null)
			{
				if (reconNbr != null)
				{
					currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																		   OrderBy<Asc<CARecon.reconDate, Asc<CARecon.reconNbr>>>>(this);
					current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID);
				}
				if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
				{
					if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
					{
						CAReconRecords.Delete(cachesRecon);
						CAReconRecords.Cache.IsDirty = false;
					}
					CAReconRecords.Current = current;
				}
			}
			yield return current;
		}
		#endregion
		#region Button Prev
		public PXPreviousOrderBy<CARecon, OrderBy<Asc<CARecon.cashAccountID, Asc<CARecon.reconNbr>>>> Previous;
		[PXPreviousButton]
		[PXUIField]
		protected virtual IEnumerable previous(PXAdapter a)
		{
			CARecon current = null;
			CARecon currentPre = CAReconRecords.Current;
			CARecon cachesRecon = null;
			PXSelectBase<CARecon> currentSelectStatement;
			object cashAccountCD = a.Searches != null && a.Searches.Length > 0
				? a.Searches[0]
				: null;

			object reconNbr = a.Searches != null && a.Searches.Length > 1
				? a.Searches[1]
				: null;

			foreach (PXResult<CARecon> recon in (new PXPrevious<CARecon>(this, "Prev")).Press(a))
			{
				cachesRecon = recon;
			}

			CashAccount acct = PXSelect<CashAccount,
				Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(this, cashAccountCD);

			if (acct != null)
			{
				if (reconNbr != null)
				{
					currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																			And<CARecon.reconNbr, Less<Required<CARecon.reconNbr>>>>,
																		   OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
					current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID, reconNbr);
				}
				if (current == null)
				{
					currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																		   OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
					current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID);
				}
				if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
				{
					if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
					{
						CAReconRecords.Delete(cachesRecon);
						CAReconRecords.Cache.IsDirty = false;
					}
					CAReconRecords.Current = current;
				}
			}
			yield return current;
		}
		#endregion
		#region Button Next
		public PXNextOrderBy<CARecon, OrderBy<Asc<CARecon.cashAccountID, Asc<CARecon.reconNbr>>>> Next;
		[PXNextButton]
		[PXUIField]
		protected virtual IEnumerable next(PXAdapter a)
		{
			PXLongOperation.ClearStatus(this.UID);
			CARecon current = null;
			CARecon cachesRecon = null;
			PXSelectBase<CARecon> currentSelectStatement;
			object cashAccountCD = a.Searches != null && a.Searches.Length > 0
				? a.Searches[0]
				: null;

			object reconNbr = a.Searches != null && a.Searches.Length > 1
				? a.Searches[1]
				: null;

			foreach (PXResult<CARecon> recon in (new PXNext<CARecon>(this, "Next")).Press(a))
			{
				cachesRecon = recon;
			}

			CashAccount acct = PXSelect<CashAccount,
				Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(this, cashAccountCD);

			if (acct != null)
			{
				if (reconNbr != null)
				{
					currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																			And<CARecon.reconNbr, Greater<Required<CARecon.reconNbr>>>>,
																		   OrderBy<Asc<CARecon.reconDate, Asc<CARecon.reconNbr>>>>(this);
					current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID, reconNbr);
				}
				if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
				{
					if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
					{
						CAReconRecords.Delete(cachesRecon);
						CAReconRecords.Cache.IsDirty = false;
					}
					CAReconRecords.Current = current;
				}
			}
			yield return current;
		}
		#endregion
		#region Button Last
		public PXLastOrderBy<CARecon, OrderBy<Asc<CARecon.cashAccountID, Asc<CARecon.reconNbr>>>> Last;
		[PXLastButton]
		[PXUIField]
		protected virtual IEnumerable last(PXAdapter a)
		{
			PXLongOperation.ClearStatus(this.UID);
			CARecon current = null;
			CARecon cachesRecon = null;
			PXSelectBase<CARecon> currentSelectStatement;
			object cashAccountCD = a.Searches != null && a.Searches.Length > 0
				? a.Searches[0]
				: null;

			object reconNbr = a.Searches != null && a.Searches.Length > 1
				? a.Searches[1]
				: null;

			foreach (PXResult<CARecon> recon in (new PXLast<CARecon>(this, "Last")).Press(a))
			{
				cachesRecon = recon;
			}

			CashAccount acct = PXSelect<CashAccount,
				Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(this, cashAccountCD);

			if (acct != null)
			{
				if (reconNbr != null)
				{
					currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																		   OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
					current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID);
				}
				if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
				{
					if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
					{
						CAReconRecords.Delete(cachesRecon);
						CAReconRecords.Cache.IsDirty = false;
					}
					CAReconRecords.Current = current;
				}
			}
			yield return current;
		}
		#endregion

		#region Button Toggle Reconciled
		public PXAction<CARecon> ToggleReconciled;

		[PXUIField(DisplayName = "Toggle Reconciled", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable toggleReconciled(PXAdapter adapter)
		{
			bool? reconciledValue = null;
			PXSetPropertyException lastException = null;

			foreach (CATranExt tran in GetReconTrans())
			{
				if (tran.Released != true)
				{
					continue;
				}

				reconciledValue = reconciledValue ?? !tran.Reconciled;

				CATranExt copy = PXCache<CATranExt>.CreateCopy(tran);
				copy.Reconciled = reconciledValue;

				try
				{
					CAReconTranRecords.Update(copy);
				}
				catch (PXSetPropertyException e)
				{
					lastException = e;
				}
			}

			if (lastException != null)
			{
				throw lastException;
			}

			return adapter.Get();
		}
		#endregion

		#region Button Toggle Cleared
		public PXAction<CARecon> ToggleCleared;

		[PXUIField(DisplayName = "Toggle Cleared", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable toggleCleared(PXAdapter adapter)
		{
			bool? clearedValue = null;

			foreach (CATranExt tran in GetReconTrans())
			{
				if (tran.Released != true)
				{
					continue;
				}

				if (tran.Cleared == true && (tran.Reconciled == true || tran.ProcessedFromFeed == true))
				{
					continue;
				}

				if (IsDisabledBecauseOfVoidingNotReleased(tran))
				{
					continue;
				}

				clearedValue = clearedValue ?? !tran.Cleared;
				CATranExt copy = PXCache<CATranExt>.CreateCopy(tran);
				copy.Cleared = clearedValue;
				CAReconTranRecords.Update(copy);
			}
			return adapter.Get();
		}
		#endregion

		#region ToggleReconciled and ToggleCleared helper
		private IEnumerable<CATranExt> GetReconTrans()
		{
			int start = 0;
			int total = 0;
			bool skipVoids = CAReconRecords.Current.SkipVoided ?? false;

			foreach (PXResult<CATranExt> record in CAReconTranRecords.View.Select(
																		   PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings,
																		   CAReconTranRecords.View.GetExternalFilters(), ref start, PXView.MaximumRows, ref total))

			{
				CATranExt tran = record;

				yield return tran;
			}
		}
		#endregion

		#region Button ReconcileProcessed
		public PXAction<CARecon> ReconcileProcessed;

		[PXUIField(DisplayName = "Reconcile Processed", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable reconcileProcessed(PXAdapter adapter)
		{
			foreach (CATranExt record in CAReconTranRecords.Select())
			{
				if (record.ProcessedFromFeed == true && record.Reconciled == false)
				{
					CAReconTranRecords.Cache.SetValueExt<CATranExt.reconciled>(record, true);
					CAReconTranRecords.Update(record);
				}
			}

			return adapter.Get();
		}
		#endregion

		#region Button Release
		public PXAction<CARecon> Release;
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			PXCache cache = this.CAReconRecords.Cache;
			List<CARecon> list = new List<CARecon>();
			foreach (PXResult<CARecon> r in adapter.Get())
			{
				list.Add((CARecon)r);
			}

			Save.Press();

			foreach (CARecon recon in list)
			{
				PXLongOperation.StartOperation(this, () => ReleaseCARecon(recon));
			}
			return list;
		}
		#endregion

		#region Button Voided
		public PXAction<CARecon> Voided;
		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable voided(PXAdapter adapter)
		{
			CARecon recon = CAReconRecords.Current;
			if (CAReconRecords.Ask(Messages.Void, Messages.ConfirmVoid, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
				PXLongOperation.ClearStatus(this.UID);
				PXLongOperation.StartOperation(this, () => VoidCARecon(recon));
				yield return recon;
			}
			else
			{
				yield return recon;
			}
		}
		#endregion

		#region Button ViewDoc
		public PXAction<CARecon> viewDoc;
		[PXUIField(
			DisplayName = "View Document",
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			CATran tran = CAReconTranRecords.Current;
			CAReconTranRecords.Current = null;
			PXSelect<CATran, Where<CATran.cashAccountID, Equal<Current<CARecon.cashAccountID>>,
				And<Where<CATran.reconNbr, Equal<Current<CARecon.reconNbr>>,
				Or<Current<CARecon.reconciled>, Equal<boolFalse>,
				And<CATran.reconNbr, IsNull,
				And<Where<CATran.tranDate, LessEqual<Current<CARecon.loadDocumentsTill>>,
				Or<Current<CARecon.loadDocumentsTill>, IsNull>>>>>>>>>.Clear(this);
			CATran.Redirect(CAReconRecords.Cache, tran);
			return adapter.Get();
		}
		#endregion

		#region Button CreateAdjustment
		public PXAction<CARecon> CreateAdjustment;
		[PXUIField(DisplayName = "Create Adjustment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable createAdjustment(PXAdapter adapter)
		{
			CARecon header = CAReconRecords.Current;
			if (header != null && IsHeaderUpdateEnabled(header))
			{
				WebDialogResult result = AddFilter.AskExt(true);
				if (result == WebDialogResult.OK)
				{
					using (new PXTimeStampScope(this.TimeStamp))
					{
						CATran rectran = AddTrxFilter.VerifyAndCreateTransaction(this, AddFilter, currencyinfo);
						Save.Press();
					}
				}
				AddFilter.Cache.Clear();
			}

			yield return header;
		}
		#endregion
		#endregion

		#region Variables
		public ToggleCurrency<CARecon> CurrencyView;

		public PXSelectJoin<CARecon,
				LeftJoin<CashAccount,
					On<CashAccount.cashAccountID, Equal<CARecon.cashAccountID>>,
				LeftJoin<Account,
					On<Account.accountID, Equal<CashAccount.accountID>>,
				LeftJoin<Sub,
					On<Sub.subID, Equal<CashAccount.subID>>>>>,
				Where<CARecon.cashAccountID, IsNull,
					Or2<Match<Account, Current<AccessInfo.userName>>, And<Match<Sub, Current<AccessInfo.userName>>>>>,
				OrderBy<Asc<CARecon.cashAccountID, Desc<CARecon.reconDate>>>> CAReconRecords;

		[PXFilterable]
		public PXSelect<CATranExt,
				Where<True, Equal<True>>,
				OrderBy<Asc<CATran.tranDate>>> CAReconTranRecords;

		public PXSelect<CATranVoiding,
				Where<CATranVoiding.cashAccountID, Equal<Required<CATranVoiding.cashAccountID>>,
				And<CATranVoiding.tranID, Equal<Required<CATranVoiding.tranID>>>>> VoidingTrans;

		public PXSelect<CABatch,
				Where<CABatch.batchNbr, Equal<Required<CABatch.batchNbr>>>> CABatches;

		public PXSelectJoin<CATranInBatch,
				InnerJoinSingleTable<Light.APPayment,
					On<Light.APPayment.cATranID, Equal<CATranInBatch.tranID>>,
				InnerJoin<CABatchDetail,
					On<BatchModule.moduleAP, Equal<CABatchDetail.origModule>,
					And<Light.APPayment.docType, Equal<CABatchDetail.origDocType>,
					And<Light.APPayment.refNbr, Equal<CABatchDetail.origRefNbr>>>>>>,
				Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>>> TransactionsInBatch;


		public PXFilter<AddTrxFilter> AddFilter;
		public PXSelect<CurrencyInfo> currencyinfo;

		public PXSetup<CashAccount, Where<CashAccount.reconcile, Equal<boolTrue>>> cashaccount;
		public PXSetup<CASetup> casetup;
		public PXSetup<APSetup> apsetup;
		public PXSetup<ARSetup> arsetup;
		public CMSetupSelect CMSetup;

		public PXSelect<CR.BAccountR> BaccountCache;
		#endregion

		#region Execute Select
		protected virtual IEnumerable carecontranrecords()
		{
			if (CAReconRecords.Current == null || CAReconRecords.Current.Voided == true)
			{
				yield break;
			}

			CARecon doc = CAReconRecords.Current;
			CashAccount cashAcct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CARecon.cashAccountID>>>>.Select(this, doc.CashAccountID);
			if (cashAcct == null)
				yield break;

			HashSet<long> catrans = new HashSet<long>();
			bool skipVoids = doc.SkipVoided ?? false;

			if (doc.ShowBatchPayments == true)
			{
				foreach (CATranExt result in GetBatchTransactions(catrans))
				{
					PXCache cache = this.Caches[typeof(CATranExt)];
					CATranExt tran = null;
					foreach (CATranExt cached in cache.Cached)
					{
						if (cached.OrigModule == BatchModule.AP && cached.OrigTranType == CATranType.CABatch && cached.OrigRefNbr == result.OrigRefNbr)
						{
							cache.Hold(cached);
							tran = cached;
							break;
						}
					}
					if (tran == null)
					{
						tran = result;

						object newVal = null;
						cache.RaiseFieldDefaulting<CATran.tranID>(tran, out newVal);
						tran.TranID = (long?)newVal;

						cache.SetStatus(tran, PXEntryStatus.Held);
					}

					if (!catrans.Contains(tran.TranID.Value))
					{
						catrans.Add(tran.TranID.Value);
						yield return new PXResult<CATranExt>(tran);
					}
					else
					{
						continue;
					}

				}
			}
			IEnumerable<CATranExt> transToReconcile = PXSelect<CATranExt,
												Where<CATranExt.cashAccountID, Equal<Current<CARecon.cashAccountID>>,
												And<Where<CATranExt.reconNbr, Equal<Current<CARecon.reconNbr>>,
													Or<Current<CARecon.reconciled>, Equal<boolFalse>,
													And<CATranExt.reconNbr, IsNull,
													And<Where<CATranExt.tranDate, LessEqual<Current2<CARecon.loadDocumentsTill>>,
														Or<Current2<CARecon.loadDocumentsTill>, IsNull>>>>>>>>>.Select(this).AsEnumerable().Select(result => (CATranExt)result);
			if (transToReconcile.Any())
			{
				Dictionary<long?, CABankTran> bankTrans;
				Dictionary<long?, CATranVoiding> voidingTrans;
				SelectAdditionalTables(doc, transToReconcile, out bankTrans, out voidingTrans);
				foreach (CATranExt tran in transToReconcile)
				{
					tran.CuryEffTranAmt = tran.CuryTranAmt;
					tran.EffTranAmt = tran.TranAmt;
					tran.Voided = false;
					tran.VoidingNotReleased = false;
					tran.VoidingTranID = null;
					if (skipVoids)
					{
						if (Skip(voidingTran: tran, voidedTran: null, searchInCache: false))
						{
							continue; //Skip voiding transaction
						}

						CATranVoiding voidingTran = null;
						voidingTrans.TryGetValue(tran.TranID, out voidingTran);

						if (voidingTran?.TranID != null)
						{
							if (voidingTran.Released == true)
							{
								if (Skip(voidingTran, tran))
								{
									tran.CuryEffTranAmt += (voidingTran.CuryTranAmt ?? Decimal.Zero);
									tran.EffTranAmt += (voidingTran.TranAmt ?? Decimal.Zero);
								}

								tran.Voided = true;
							}
							else
							{
								tran.VoidingNotReleased = true;
							}
							tran.VoidingTranID = voidingTran.TranID;
						}
					}

					CABankTran bankTran = null;
					bankTrans.TryGetValue(tran.TranID, out bankTran);
					if (bankTran != null && bankTran.Processed == true)
					{
						tran.ProcessedFromFeed = true;
					}

				this.Caches[typeof(CATranExt)].Hold(tran);

					if (!catrans.Contains(tran.TranID.Value))
					{
						catrans.Add(tran.TranID.Value);
						yield return new PXResult<CATranExt>(tran);
					}
					else
					{
						continue;
					}
				}
			}
		}

		//We have to select additional info in a separate query to avoid AppendTail on merging cache (it will generate select for each CATran merged with updated record)
		//We have to select additional info by batches to avoid generation of extra big queries that may fail on MySQL
		private void SelectAdditionalTables(CARecon doc, IEnumerable<CATranExt> transToReconcile, out Dictionary<long?, CABankTran> bankTrans, out Dictionary<long?, CATranVoiding> voidingTrans)
		{
			bankTrans = new Dictionary<long?, CABankTran>();
			voidingTrans = new Dictionary<long?, CATranVoiding>();
			const int batchSize = 500;
			var idsCollection = transToReconcile.Select(tran => tran.TranID);
			foreach (var batch in idsCollection.BreakInto(batchSize))
			{
				long?[] tranIDs = batch.ToArray();

				foreach (PXResult<CABankTran, CABankTranMatch> item in PXSelectJoin<CABankTran,
											InnerJoin<CABankTranMatch,
												On<CABankTran.tranID, Equal<CABankTranMatch.tranID>>>,
											Where<CABankTranMatch.cATranID, In<Required<CATranExt.tranID>>>>.Select(this, tranIDs))
				{
					CABankTranMatch match = item;
					if (!bankTrans.ContainsKey(match.CATranID))

					{
						bankTrans.Add(match.CATranID, item);
					}
					else
					{
						var errorMsg = $"Error message: {Messages.MultipleCABankTransMatchedToSingleCATran}; Date: {DateTime.Now}; Screen: {Accessinfo.ScreenID}; CATranID: {match.CATranID};";
						PXTrace.WriteError(errorMsg);
					}
				}
				if (doc.SkipVoided == true)
				{
					voidingTrans.AddRange(PXSelectReadonly<CATranVoiding,
												  Where<CATranVoiding.voidedTranID, In<Required<CATranExt.tranID>>,
												  And<CATranVoiding.cashAccountID, Equal<Required<CATranExt.cashAccountID>>>>>.Select(this, tranIDs, doc.CashAccountID)
												  .ToDictionary(result => ((CATranVoiding)result).VoidedTranID, result => (CATranVoiding)result));
				}
			}
		}

		private List<CATranExt> GetBatchTransactions(HashSet<long> catrans)
		{
			HashSet<string> partiallyMatchedBatches = new HashSet<string>();
			HashSet<string> partiallyReconciledBatches = new HashSet<string>();
			var lines = PXSelectJoin<CABatch,
							InnerJoin<CABatchDetail,
								On<CABatchDetail.batchNbr, Equal<CABatch.batchNbr>>,
							InnerJoin<CATran,
								On<CATran.origModule, Equal<CABatchDetail.origModule>,
								And<CATran.origTranType, Equal<CABatchDetail.origDocType>,
								And<CATran.origRefNbr, Equal<CABatchDetail.origRefNbr>,
								And<CATran.isPaymentChargeTran, Equal<False>>>>>,
							LeftJoin<CABankTranMatch,
								On<CABankTranMatch.docRefNbr, Equal<CABatch.batchNbr>,
								And<CABankTranMatch.docModule, Equal<BatchModule.moduleAP>,
								And<CABankTranMatch.docType, Equal<CATranType.cABatch>>>>,
							LeftJoin<CABankTran,
								On<CABankTran.tranID, Equal<CABankTranMatch.tranID>>,
							LeftJoin<CABankTranMatch2,
								On<CABankTranMatch2.cATranID, Equal<CATran.tranID>>>>>>>,
							Where<CABatch.cashAccountID, Equal<Current<CARecon.cashAccountID>>,
								And<Where<CABatch.reconNbr, Equal<Current<CARecon.reconNbr>>,
								Or<Current<CARecon.reconciled>, Equal<boolFalse>,
									And<CABatch.reconNbr, IsNull>>>>>,
							OrderBy<Asc<CABatch.batchNbr, Desc<CABankTranMatch2.tranID>>>>.Select(this);

			foreach (PXResult<CABatch, CABatchDetail, CATran, CABankTranMatch, CABankTran, CABankTranMatch2> result in lines)
			{
				CATran catran = result;
				CABatch batch = result;
				CABankTranMatch2 match = result;

				if (catran.ReconNbr != this.CAReconRecords.Current.ReconNbr &&
					(this.CAReconRecords.Current.Reconciled == true || catran.ReconNbr != null))
				{
					partiallyReconciledBatches.Add(batch.BatchNbr);
				}

				if (match != null && match.TranID.HasValue)
				{
					partiallyMatchedBatches.Add(batch.BatchNbr);
				}
			}

			List<CATranExt> trans = new List<CATranExt>();
			CATranExt tran = null;
			string prevBatchNbr = null;
			foreach (PXResult<CABatch, CABatchDetail, CATran, CABankTranMatch, CABankTran, CABankTranMatch2> result in lines)
			{
				CATran catran = result;
				CABatch batch = result;

				if (partiallyReconciledBatches.Contains(batch.BatchNbr))
				{
					continue;
				}
				if (partiallyMatchedBatches.Contains(batch.BatchNbr))
				{
					continue;
				}

				catrans.Add(catran.TranID.Value);

				if (batch.Reconciled == false && batch.TranDate > CAReconRecords.Current.LoadDocumentsTill)
				{
					continue;
				}

				if (prevBatchNbr != batch.BatchNbr)
				{
					tran = new CATranExt();
					batch.CopyTo(tran);
					tran.Voided = false;
					tran.VoidingNotReleased = false;
					tran.VoidingTranID = null;
					tran.TranPeriodID = catran.TranPeriodID;
					tran.FinPeriodID = catran.FinPeriodID;
					tran.CuryEffTranAmt = tran.CuryTranAmt;
					tran.EffTranAmt = tran.TranAmt;
					tran.ReferenceID = catran.ReferenceID;
					CABankTran bankTran = result;
					if (bankTran != null && bankTran.Processed == true)
					{
						tran.ProcessedFromFeed = true;
					}

					trans.Add(tran);
					prevBatchNbr = batch.BatchNbr;
				}
				else
				{
					if (tran != null && tran.ReferenceID != ((CATran)result).ReferenceID)
					{
						tran.ReferenceID = null;
					}
				}
			}
			return trans;
		}
		#endregion

		#region Implement
		public override void Persist()
		{
			List<CATran> list = new List<CATran>((IEnumerable<CATran>)this.Caches[typeof(CATran)].Updated);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				base.Persist();

				foreach (CATran tran in list)
				{
					if (tran.OrigModule == BatchModule.AP && tran.OrigTranType == CATranType.CABatch)
					{
						foreach (PXResult<CATranInBatch> res in TransactionsInBatch.Select(tran.OrigRefNbr))
						{
							UpdateClearedOnSourceDoc(res);
						}
					}
					else
					{
						UpdateClearedOnSourceDoc(tran);
					}
				}

				ts.Complete();
			}

			foreach (CM.CurrencyInfo iInfo in Caches[typeof(CM.CurrencyInfo)].Cached)
			{
				long? key = this.AddFilter.Current.CuryInfoID;
				if (iInfo.CuryInfoID == key)
				{
					Caches[typeof(CM.CurrencyInfo)].SetStatus(iInfo, PXEntryStatus.Inserted);
				}
			}
		}

		public override void Clear()
		{
			if (AddFilter.Current != null)
			{
				AddFilter.Current.TranDate = null;
				AddFilter.Current.FinPeriodID = null;
				AddFilter.Current.CuryInfoID = null;
			}
			base.Clear();
		}

		public CAReconEntry()
		{
			CASetup setup = casetup.Current;
			PXCache tranCache = CAReconTranRecords.Cache;

			PXParentAttribute.SetLeaveChildren<CATran.reconNbr>(tranCache, null, true);

			PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyClearedCreditAmt>(tranCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyClearedDebitAmt>(tranCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyCreditAmt>(tranCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyDebitAmt>(tranCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyReconciledCredit>(tranCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyReconciledDebit>(tranCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyTranAmt>(tranCache, null, false);

			PXCache reconCache = CAReconRecords.Cache;
			PXDBCurrencyAttribute.SetBaseCalc<CARecon.curyReconciledBalance>(reconCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CARecon.curyReconciledCredits>(reconCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CARecon.curyReconciledDebits>(reconCache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CARecon.curyReconciledTurnover>(reconCache, null, false);
			PXUIFieldAttribute.SetVisible<CARecon.curyID>(reconCache, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
		}
		#endregion

		#region Functions

		public static void UpdateClearedOnSourceDoc(CATran tran)
		{
			if (tran.IsPaymentChargeTran == false)
			{
				Type table = null;

				var updateStatement = new List<PXDataFieldParam>
				{
					new PXDataFieldAssign(nameof(tran.Cleared), tran.Cleared),
					new PXDataFieldAssign(nameof(tran.ClearDate), tran.ClearDate)
				};

				switch (tran.OrigModule)
				{
					case GL.BatchModule.AP:
						if (tran.OrigTranType != CATranType.CABatch)
						{
							table = typeof(APPayment);
							updateStatement.Add(new PXDataFieldRestrict<APPayment.docType>(PXDbType.Char, tran.OrigTranType));
							updateStatement.Add(new PXDataFieldRestrict<APPayment.refNbr>(tran.OrigRefNbr));
						}
						else
						{
							table = typeof(CABatch);
							updateStatement.Add(new PXDataFieldRestrict<CABatch.batchNbr>(tran.OrigRefNbr));
						}
						break;
					case GL.BatchModule.AR:
						table = typeof(ARPayment);
						updateStatement.Add(new PXDataFieldRestrict<ARPayment.docType>(PXDbType.Char, tran.OrigTranType));
						updateStatement.Add(new PXDataFieldRestrict<ARPayment.refNbr>(tran.OrigRefNbr));
						break;
					case GL.BatchModule.CA:
						switch (tran.OrigTranType)
						{
							case CATranType.CADeposit:
							case CATranType.CAVoidDeposit:
								table = typeof(CADeposit);
								updateStatement.Add(new PXDataFieldRestrict<CADeposit.tranType>(PXDbType.Char, tran.OrigTranType));
								updateStatement.Add(new PXDataFieldRestrict<CADeposit.refNbr>(tran.OrigRefNbr));
								break;
							case CATranType.CAAdjustment:
								table = typeof(CAAdj);
								updateStatement.Add(new PXDataFieldRestrict<CAAdj.adjTranType>(PXDbType.Char, tran.OrigTranType));
								updateStatement.Add(new PXDataFieldRestrict<CAAdj.adjRefNbr>(tran.OrigRefNbr));
								break;
							case CATranType.CATransferExp:
								table = typeof(CAExpense);
								updateStatement.Add(new PXDataFieldRestrict<CAExpense.cashTranID>(tran.TranID));
								break;
							case CATranType.CATransferIn:
								table = typeof(CATransfer);
								updateStatement = new List<PXDataFieldParam>
								{
									new PXDataFieldAssign<CATransfer.clearedIn>(tran.Cleared),
									new PXDataFieldAssign<CATransfer.clearDateIn>(tran.ClearDate),
									new PXDataFieldRestrict<CATransfer.transferNbr>(tran.OrigRefNbr)
								};
								break;
							case CATranType.CATransferOut:
								table = typeof(CATransfer);
								updateStatement = new List<PXDataFieldParam>
								{
									new PXDataFieldAssign<CATransfer.clearedOut>(tran.Cleared),
									new PXDataFieldAssign<CATransfer.clearDateOut>(tran.ClearDate),
									new PXDataFieldRestrict<CATransfer.transferNbr>(tran.OrigRefNbr)
								};
								break;
						}
						break;
				}

				if (table != null && updateStatement.Any())
				{
					PXDatabase.Update(table, updateStatement.ToArray());
				}
			}
		}

		public static void ReconCreate(CashAccount acct)
		{
			if (acct == null) return;
			if (acct.Reconcile != true)
			{
				throw new Exception(Messages.CashAccounNotReconcile);
			}
			CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();
			PXSelectBase<CARecon> selectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																					  OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(graph);
			CARecon lastRecon = (CARecon)selectStatement.View.SelectSingle(acct.CashAccountID);
			if (lastRecon != null && lastRecon.Reconciled != true)
				throw new Exception(Messages.CanNotCreateStatement);

			CARecon current = new CARecon();
			current.CashAccountID = acct.CashAccountID;
			graph.CAReconRecords.Insert(current);
			throw new PXRedirectRequiredException(graph, "Document");
		}

		public static void VoidCARecon(CARecon recon)
		{
			CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();

			if (!IsVoidAllowed(recon, graph))
				throw new Exception(Messages.CanNotVoidStatement);

			recon = graph.CAReconRecords.Update(recon);
			decimal? reconciledDebits = recon.ReconciledDebits;
			decimal? reconciledCredits = recon.ReconciledCredits;
			decimal? curyReconciledDebits = recon.CuryReconciledDebits;
			decimal? curyReconciledCredits = recon.CuryReconciledCredits;
			int? countDebit = recon.CountDebit;
			int? countCredit = recon.CountCredit;

			foreach (CATranExt res in PXSelect<CATranExt, Where<CATranExt.reconNbr, Equal<Required<CARecon.reconNbr>>, And<CATranExt.cashAccountID, Equal<Required<CARecon.cashAccountID>>>>>
												  .Select(graph, recon.ReconNbr, recon.CashAccountID))
			{
				if (res.Reconciled == true)
				{
					CATranExt newrow = PXCache<CATranExt>.CreateCopy(res);
					newrow.Reconciled = false;
					graph.CAReconTranRecords.Update(newrow);
				}
			}
			CABatch newbatch;
			foreach (CABatch res in PXSelect<CABatch, Where<CABatch.reconNbr, Equal<Required<CARecon.reconNbr>>, And<CABatch.cashAccountID, Equal<Required<CARecon.cashAccountID>>>>>
												  .Select(graph, recon.ReconNbr, recon.CashAccountID))
			{
				if (res.Reconciled == true)
				{
					newbatch = PXCache<CABatch>.CreateCopy(res);
					newbatch.Reconciled = false;
					newbatch.ReconNbr = null;
					newbatch.ReconDate = null;
					graph.CABatches.Update(newbatch);
				}
			}

			CARecon newheader = (CARecon)graph.CAReconRecords.Cache.CreateCopy(recon);
			newheader.Reconciled = false;
			newheader.Voided = true;
			newheader.ReconciledDebits = reconciledDebits;
			newheader.ReconciledCredits = reconciledCredits;
			newheader.CuryReconciledDebits = curyReconciledDebits;
			newheader.CuryReconciledCredits = curyReconciledCredits;
			newheader.CountDebit = countDebit;
			newheader.CountCredit = countCredit;
			graph.CAReconRecords.Update(newheader);

			graph.Save.Press();
		}

		private static bool IsVoidAllowed(CARecon recon, CAReconEntry graph)
		{
			PXSelectBase<CARecon> statements = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																				And<CARecon.reconDate, GreaterEqual<Required<CARecon.reconDate>>>>,
																					  OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(graph);

			foreach (CARecon statement in statements.Select(recon.CashAccountID, recon.ReconDate))
			{
				if (statement != null)
				{
					if (statement.ReconNbr == recon.ReconNbr)
						return statement.Reconciled ?? false;
					else if (statement.Voided == false)
						return false;
				}
			}

			return false;
		}

		public static void ReleaseCARecon(CARecon recon)
		{
			if (recon.Hold == true)
			{
				throw new PXException(AP.Messages.Document_OnHold_CannotRelease);
			}

			CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();

			bool skipVoids = recon.SkipVoided ?? false;
			if (skipVoids)
			{
				CATran partiallyVoidedTran = PXSelectJoin<CATran,
						InnerJoin<CATranVoiding,
							On<CATranVoiding.cashAccountID, Equal<CATran.cashAccountID>,
							And<CATranVoiding.voidedTranID, Equal<CATran.tranID>,
							And<CATranVoiding.released, Equal<False>>>>>,
						Where<CATran.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
						And<Where<CATran.reconNbr, Equal<Required<CARecon.reconNbr>>>>>>.SelectWindowed(graph, 0, 1, recon.CashAccountID, recon.ReconNbr);

				if (partiallyVoidedTran?.TranID != null)
				{
					throw new PXException(Messages.TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation);
				}
			}

			CATran notReleasedTran = PXSelect<CATran,
											Where<CATran.reconNbr, Equal<Required<CARecon.reconNbr>>,
											And<CATran.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
											And<CATran.reconciled, Equal<True>,
											And<CATran.released, Equal<False>>>>>>
												  .SelectWindowed(graph, 0, 1, recon.ReconNbr, recon.CashAccountID);
			if (notReleasedTran?.TranID != null)
			{
				throw new Exception(Messages.OneOrMoreItemsAreNotReleasedAndStatementCannotBeCompleted);
			}

			recon.Reconciled = true;
			graph.CAReconRecords.Update(recon);
			graph.Save.Press();
		}

		public virtual CARecon GetLastRecon(CARecon row)
		{
			if (row?.CashAccountID == null)
			{
				return null;
			}

			CAReconRecords.Cache.ClearQueryCacheObsolete();
			PXSelectBase<CARecon> selectQuery =
				new PXSelectReadonly<CARecon,
							  Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
								And<CARecon.voided, NotEqual<True>>>,
							OrderBy<
									Desc<CARecon.reconDate,
								Desc<CARecon.reconNbr>>>>(this);

			return selectQuery.View.SelectSingle(row.CashAccountID) as CARecon;
		}

		public virtual bool IsHeaderUpdateEnabled(CARecon header)
		{
			return (header.Reconciled != true) && (header.Voided != true) && (header.CashAccountID != null);
		}

		private bool IsDisabledBecauseOfVoidingNotReleased(CATranExt tran)
		{
			return CAReconRecords.Current.SkipVoided == true && tran.VoidingNotReleased == true && tran.VoidingTranID.HasValue;
		}

		protected virtual bool Skip(CATran voidingTran, CATran voidedTran = null, bool searchInCache = true)
		{
			if (!voidingTran.VoidedTranID.HasValue || voidingTran.Released != true)
			{
				return false;
			}

			if (voidedTran == null)
			{
				voidedTran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATran.voidedTranID>>,
					And<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>>>>.Select(this, voidingTran.VoidedTranID, voidingTran.CashAccountID);
			}

			if (voidedTran == null)
			{
				return false;
			}

			if (CAReconRecords.Current.ShowBatchPayments == true)
			{
				bool batchExistsAndNotPartiallyMatched = false;

				foreach (PXResult<CABatchDetail, CABatchDetail2, AP.Standalone.APPayment, CABankTranMatch> res in PXSelectJoin<CABatchDetail,
						LeftJoin<CABatchDetail2, On<CABatchDetail2.batchNbr, Equal<CABatchDetail.batchNbr>,
							And<CABatchDetail2.origModule, Equal<BatchModule.moduleAP>>>,
						LeftJoin<AP.Standalone.APPayment, On<AP.Standalone.APPayment.docType, Equal<CABatchDetail2.origDocType>,
							And<AP.Standalone.APPayment.refNbr, Equal<CABatchDetail2.origRefNbr>>>,
						LeftJoin<CABankTranMatch, On<CABankTranMatch.cATranID, Equal<AP.Standalone.APPayment.cATranID>>>>>,
						Where<CABatchDetail.origDocType, Equal<Required<CABatchDetail.origDocType>>,
							And<CABatchDetail.origModule, Equal<Required<CABatchDetail.origModule>>,
							And<CABatchDetail.origRefNbr, Equal<Required<CABatchDetail.origRefNbr>>>>>>.Select(this, voidedTran.OrigTranType, voidedTran.OrigModule, voidedTran.OrigRefNbr))
				{
					batchExistsAndNotPartiallyMatched = true;
					CABankTranMatch match = res;

					if (match != null && match.TranID.HasValue)
					{
						batchExistsAndNotPartiallyMatched = false;
						break;
					}
				}

				if (batchExistsAndNotPartiallyMatched)
					return false;
			}

			//The updated voidingTran may be stored in the Cache
			//TODO: replace by search or locates
			if (searchInCache)
			{
				voidingTran = PXSelect<CATranVoiding, Where<CATranVoiding.voidedTranID, Equal<Required<CATran.tranID>>>>.Select(this, voidedTran.TranID);
			}

			return (voidedTran.Reconciled == false && voidingTran.Reconciled == false) ||
				(voidedTran.ReconNbr == voidingTran.ReconNbr && voidedTran.Reconciled == voidingTran.Reconciled);
		}

		private void UpdateVoidingTran(CATranExt row, CATranExt oldRow)
		{
			CARecon recon = CAReconRecords.Current;

			if (row?.VoidingTranID == null || recon == null)
			{
				return;
			}

			bool skipVoids = recon.SkipVoided ?? false;

			CATranVoiding voidingTran = VoidingTrans.Select(row.CashAccountID, row.VoidingTranID);

			if (!skipVoids || voidingTran?.TranID == null || !Skip(voidingTran, oldRow))
			{
				return;
			}

			bool isModified = false;

			if (row.Cleared != oldRow.Cleared || row.ClearDate != oldRow.ClearDate)
			{
				voidingTran.ClearDate = row.ClearDate;
				voidingTran.Cleared = row.Cleared;
				isModified = true;
			}

			if (row.Reconciled != oldRow.Reconciled || row.ReconDate != oldRow.ReconDate || row.ReconNbr != oldRow.ReconNbr)
			{
				isModified = true;
				voidingTran.Reconciled = row.Reconciled;
				voidingTran.ReconDate = row.ReconDate;
				voidingTran.ReconNbr = row.ReconNbr;
				if (voidingTran.Reconciled == true && voidingTran.Cleared != true)
				{
					voidingTran.ClearDate = row.ClearDate;
					voidingTran.Cleared = row.Cleared;
				}
			}

			if (isModified)
			{
				voidingTran = VoidingTrans.Update(voidingTran);
			}
		}

		private void UpdateBatchTransactions(CATranExt row)
		{
			if (row.OrigModule == BatchModule.AP && row.OrigTranType == CATranType.CABatch)
			{
				foreach (CATranInBatch innerTran in TransactionsInBatch.Select(row.OrigRefNbr))
				{
					innerTran.CopyFrom(row);
					TransactionsInBatch.Update(innerTran);
				}
				CABatch batch = CABatches.Select(row.OrigRefNbr);
				batch.Cleared = row.Cleared;
				batch.ClearDate = row.ClearDate;
				batch.Reconciled = row.Reconciled;
				batch.ReconDate = row.ReconDate;
				batch.ReconNbr = row.ReconNbr;
				CABatches.Update(batch);
			}
		}
		#endregion

		#region CurrencyInfo
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (!string.IsNullOrEmpty(cashaccount.Current?.CuryID))
				{
					e.NewValue = cashaccount.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (!string.IsNullOrEmpty(cashaccount.Current?.CuryRateTypeID))
				{
					e.NewValue = cashaccount.Current.CuryRateTypeID;
					e.Cancel = true;
				}
				else
				{
					CMSetup cmsetup = PXSelect<CMSetup>.Select(this);
					if (!string.IsNullOrEmpty(cmsetup?.CARateTypeDflt))
					{
						e.NewValue = cmsetup.CARateTypeDflt;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CAReconRecords.Current != null)
			{
				e.NewValue = CAReconRecords.Current.ReconDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			if (AddFilter.Current != null && AddFilter.Current.CuryInfoID == ((CurrencyInfo)e.Row).CuryInfoID)
			{
				e.Cancel = true;
			}
		}

		#endregion

		#region CATran Events
		#region CATranExt
		protected virtual void CATranExt_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CATranExt row = (CATranExt)e.Row;
			bool skipVoids = CAReconRecords.Current?.SkipVoided ?? false;
			if (row.OrigModule == BatchModule.AP && row.OrigTranType == CATranType.CABatch)
			{
				e.Cancel = true;
			}
			if (row.Reconciled == true)
			{
				if (row.Cleared != true)
				{
					sender.RaiseExceptionHandling<CATran.cleared>(row, row.Cleared, new PXSetPropertyException(Messages.ReconciledDocCanNotBeNotCleared));
				}

				if (row.ClearDate == null)
				{
					sender.RaiseExceptionHandling<CATran.clearDate>(row, row.ClearDate, new PXSetPropertyException(Messages.ReconciledDocCanNotBeNotCleared));
				}

				if (row.Released == false)
				{
					sender.RaiseExceptionHandling<CATran.reconciled>(row, row.Reconciled, new PXSetPropertyException(Messages.NotReleasedDocCanNotAddToReconciliation));
				}

				if (skipVoids && row.VoidingTranID.HasValue && row.VoidingNotReleased == true)
				{
					sender.RaiseExceptionHandling<CATran.reconciled>(row, row.ClearDate, new PXSetPropertyException(Messages.VoidedTransactionHavingNotReleasedVoidCannotBeAddedToReconciliation));
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				CARecon header = CAReconRecords.Current;
				if (row.Reconciled == true)
				{
					row.ReconNbr = header.ReconNbr;
					row.ReconDate = header.ReconDate;
					UpdateBatchTransactions(row);
				}
				else
				{
					row.ReconNbr = null;
					row.ReconDate = null;
				}
			}
		}
		protected virtual void CATranExt_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			CATran row = (CATran)e.Row;

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
				e.TranStatus == PXTranStatus.Aborted && row.Reconciled == true)
			{
				CARecon header = CAReconRecords.Current;
				row.ReconNbr = header.ReconNbr;
				row.ReconDate = header.ReconDate;
			}
		}
		protected virtual void CATranExt_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CATranExt row = (CATranExt)e.Row;
			CARecon recon = CAReconRecords.Current;
			if (row == null || recon == null) return;
			bool isReconcilable = (recon.Reconciled != true);
			PXUIFieldAttribute.SetEnabled(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CATran.tranID>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CATran.hold>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CATran.reconciled>(cache, row, isReconcilable);

			bool allowClear = isReconcilable && row.ProcessedFromFeed != true;

			PXUIFieldAttribute.SetEnabled<CATran.cleared>(CAReconTranRecords.Cache, row, allowClear);
			PXUIFieldAttribute.SetEnabled<CATran.clearDate>(CAReconTranRecords.Cache, row, allowClear && row.Cleared == true);
			if (isReconcilable)
			{
				//TODO: Refactoring
				if (row.Released != true || IsDisabledBecauseOfVoidingNotReleased(row))
				{
					PXUIFieldAttribute.SetEnabled(cache, row, false);
					if (IsDisabledBecauseOfVoidingNotReleased(row) && row.Reconciled == true)
					{
						PXUIFieldAttribute.SetEnabled<CATran.reconciled>(cache, row, true);
					}
				}
			}
		}
		protected virtual void CATranExt_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			CATranExt newRow = (CATranExt)e.NewRow;
			CATranExt row = (CATranExt)e.Row;
			CARecon recon = CAReconRecords.Current;
			if (newRow == null || row == null || recon == null) return;
			bool skipVoids = recon.SkipVoided ?? false;
			if (skipVoids && newRow.Reconciled == true && Skip(newRow, null, false) && !Skip(row, null, false))
			{
				CATranExt voidedTran = CAReconTranRecords.Locate(new CATranExt() { TranID = newRow.VoidedTranID });
				if (voidedTran.Reconciled == true)
				{
					newRow.SkipCount = true;
					CAReconTranRecords.View.RequestRefresh();
				}
			}
			else if (skipVoids && newRow.Reconciled == false && newRow.VoidingTranID.HasValue)
			{
				CAReconTranRecords.View.RequestRefresh();
			}
		}
		protected virtual void CATranExt_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			CATranExt row = (CATranExt)e.Row;
			CATranExt oldRow = (CATranExt)e.OldRow;

			UpdateVoidingTran(row, oldRow);
			UpdateBatchTransactions(row);

			row.SkipCount = false;
		}

		protected virtual void CATranExt_Reconciled_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = (CATranExt)e.Row;
			bool skipVoids = this.CAReconRecords.Current.SkipVoided ?? false;

			if ((bool)e.NewValue != true)
			{
				return;
			}

			var message = string.Empty;

			if (row.Hold == true)
			{
				e.NewValue = false;
				e.Cancel = true;
				message = Messages.HoldDocCanNotAddToReconciliation;
			}

			if (row.Released != true && string.IsNullOrEmpty(message))
			{
				e.NewValue = false;
				e.Cancel = true;
				message = Messages.NotReleasedDocCanNotAddToReconciliation;
			}

			if (skipVoids == true && row.VoidingTranID.HasValue && row.VoidingNotReleased == true && string.IsNullOrEmpty(message))
			{
				message = Messages.TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation;
			}

			if (skipVoids == true && Skip(row) && string.IsNullOrEmpty(message))
			{
				message = Messages.TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation;
			}

			if (string.IsNullOrEmpty(message) == false)
			{
				e.NewValue = PXFieldState.CreateInstance(e.NewValue, typeof(string), false, null, null, null, null, null, typeof(CATranExt.reconciled).Name,
					null, null, message, PXErrorLevel.RowWarning, null, null, null, PXUIVisibility.Undefined, null, null, null);
				e.Cancel = true;
				e.NewValue = false;
				throw new PXSetPropertyException(message);
			}
		}
		protected virtual void CATranExt_Cleared_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CATranExt row = (CATranExt)e.Row;
			if (row.Cleared == true)
			{
				row.ClearDate = CAReconRecords.Current.ReconDate;
			}
			else
			{
				row.ClearDate = null;
			}
		}
		protected virtual void CATranExt_Reconciled_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CATranExt row = (CATranExt)e.Row;

			if (row.Reconciled == true)
			{
				row.Cleared = true;
				row.ReconNbr = CAReconRecords.Current.ReconNbr;
				row.ReconDate = CAReconRecords.Current.ReconDate;
				if (row.ClearDate == null)
				{
					row.ClearDate = CAReconRecords.Current.ReconDate;
				}
			}
			else
			{
				row.ReconNbr = null;
				row.ReconDate = null;
			}
		}
		protected virtual void CATranExt_Status_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CATranExt row = (CATranExt)e.Row;
			if (row != null)
			{
				bool skipVoids = (CAReconRecords.Current.SkipVoided ?? false);
				Dictionary<long, CAMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as Dictionary<long, CAMessage>;
				TimeSpan timespan;
				Exception ex;
				PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);
				string fieldName = typeof(CATranExt.status).Name;
				if ((status == PXLongRunStatus.Aborted || status == PXLongRunStatus.Completed) && listMessages != null)
				{
					CAMessage message = null;
					if (listMessages.ContainsKey(row.TranID.Value))
						message = listMessages[row.TranID.Value];
					if (message != null)
					{

						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
									null, null, PXMessages.LocalizeNoPrefix(message.Message), message.ErrorLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
						e.IsAltered = true;
					}
				}
				else
				{
					string message = String.Empty;
					PXErrorLevel msgLevel = PXErrorLevel.RowWarning;
					if (skipVoids && row.VoidingNotReleased == true && row.VoidingTranID.HasValue)
					{
						message = CA.Messages.VoidedTransactionHavingNotReleasedVoidCannotBeAddedToReconciliation;
						if (row.Reconciled == true)
						{
							msgLevel = PXErrorLevel.RowError;
						}
					}
					else if (row.Released != true)
					{
						message = Messages.NotReleasedDocCanNotAddToReconciliation;
					}
					else if (row.Hold == true)
					{
						message = Messages.NotReleasedDocCanNotAddToReconciliation;
					}

					if (string.IsNullOrEmpty(message) == false)
					{
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
						 null, null, PXMessages.LocalizeNoPrefix(message), msgLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
						e.IsAltered = true;
					}

				}
			}
		}
		#endregion
		#region CATranVoiding
		protected virtual void CATranVoiding_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CATranVoiding row = (CATranVoiding)e.Row;
			if (row.Reconciled == true)
			{
				if (row.Cleared != true)
				{
					sender.RaiseExceptionHandling<CATran.cleared>(row, row.Cleared, new PXSetPropertyException(Messages.ReconciledDocCanNotBeNotCleared));
				}
				if (row.ClearDate == null)
				{
					sender.RaiseExceptionHandling<CATran.clearDate>(row, row.ClearDate, new PXSetPropertyException(Messages.ReconciledDocCanNotBeNotCleared));
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				CARecon header = CAReconRecords.Current;
				if (row.Reconciled == true)
				{
					row.ReconNbr = header.ReconNbr;
					row.ReconDate = header.ReconDate;
				}
				else
				{
					row.ReconNbr = null;
					row.ReconDate = null;
				}
			}
		}
		protected virtual void CATranVoiding_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			CATranVoiding row = (CATranVoiding)e.Row;

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
				e.TranStatus == PXTranStatus.Aborted && row.Reconciled == true)
			{
				CARecon header = CAReconRecords.Current;
				row.ReconNbr = header.ReconNbr;
				row.ReconDate = header.ReconDate;
			}
		}
		#endregion

		#endregion

		#region CARecon Events
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDefault]
		[CashAccount(suppressActiveVerification: true, filterBranch: true, branchID: null, search: typeof(Search<CashAccount.cashAccountID,
				Where<CashAccount.active, Equal<True>,
					And<CashAccount.reconcile, Equal<True>,
					And<Match<Current<AccessInfo.userName>>>>>>), IsKey = true, DisplayName = "Cash Account",
				Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr), Required = true)]
		protected virtual void CARecon_CashAccountID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void CARecon_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			CARecon row = (CARecon)e.Row;
			if (row.Reconciled == true || row.Voided == true)
			{
				throw new PXException(Messages.CannotDeleteRecon);
			}
		}
		protected virtual void CARecon_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			CARecon header = (CARecon)e.Row;
			foreach (CATranExt res in CAReconTranRecords.Select())
			{
				if (res.Reconciled == true)
				{
					CATranExt newTran = (CATranExt)CAReconTranRecords.Cache.CreateCopy(res);
					newTran.Reconciled = false;
					CAReconTranRecords.Update(newTran);
				}
			}
			if (CAReconRecords.Cache.GetStatus(header) != PXEntryStatus.Inserted)
			{
				Save.Press();
			}
		}
		protected virtual void CARecon_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CARecon row = (CARecon)e.Row;

			PXEntryStatus status = cache.GetStatus(row);

			if (status == PXEntryStatus.Inserted)
			{
				CARecon openRecon = PXSelectReadonly<CARecon,
											   Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
												 And<CARecon.voided, Equal<False>,
												 And<CARecon.reconciled, Equal<False>>>>>.
									Select(this, row.CashAccountID);

				if (openRecon != null)
				{
					cache.RaiseExceptionHandling<CARecon.reconNbr>(row, row.ReconNbr, new PXSetPropertyException(Messages.PrevStatementNotReconciled));
				}
				CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
					.Select(this, row.CashAccountID)?.First();
				if (cashAccount?.Active == false)
				{
					cache.RaiseExceptionHandling<CABankTranHeader.cashAccountID>(row, row.CashAccountID,
						new PXSetPropertyException(Messages.CashAccountInactive, PXErrorLevel.RowError, cashAccount.CashAccountCD));

				}

				if (cashAccount?.Reconcile == false)
				{
					throw new PXSetPropertyException(Messages.CashAccountNotReconcile, PXErrorLevel.RowError, cashAccount.CashAccountCD);
				}
			}

			if (row.Voided == false)
			{
				CARecon laterRecon = PXSelectReadonly<CARecon,
												Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
												  And<CARecon.voided, Equal<False>,
								And<CARecon.reconNbr, NotEqual<Required<CARecon.reconNbr>>,
												  And<CARecon.reconDate, GreaterEqual<Required<CARecon.reconDate>>>>>>>.
									 Select(this, row.CashAccountID, row.ReconNbr, row.ReconDate);

				if (laterRecon != null)
				{
					cache.RaiseExceptionHandling<CARecon.reconDate>(row, row.ReconDate, new PXSetPropertyException(Messages.ReconDateNotAvailable));
				}
			}


			if (row.Hold != true && row.Voided != true)
			{
				if (row.CuryDiffBalance != 0)
				{
					cache.RaiseExceptionHandling<CARecon.curyBalance>(row, row.CuryBalance, new PXSetPropertyException(Messages.DocumentOutOfBalance));
				}
			}
		}
		protected virtual void CARecon_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			CARecon row = (CARecon)e.Row;
			CARecon oldRow = (CARecon)e.OldRow;
			if (oldRow.ReconDate != row.ReconDate
					|| oldRow.LoadDocumentsTill != row.LoadDocumentsTill)
			{
				this.CAReconTranRecords.View.RequestRefresh();
			}
		}
		protected virtual void CARecon_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			CARecon header = (CARecon)e.Row;
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<CARecon.curyInfoID>(cache, header);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) != true)
				{
					throw new PXSetPropertyException(message, PXErrorLevel.Error);
				}

				message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) != true)
				{
					throw new PXSetPropertyException(message, PXErrorLevel.Error);
				}

				if (info != null)
				{
					header.CuryID = info.CuryID;
				}
			}

			CARecon lastDoc = (CARecon)PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																					And<CARecon.reconNbr, NotEqual<Required<CARecon.reconNbr>>, And<CARecon.voided, NotEqual<True>>>>,
																				OrderBy<Asc<CARecon.cashAccountID, Desc<CARecon.reconDate>>>>.Select(this, header.CashAccountID, header.ReconNbr);
			if ((lastDoc != null) && (lastDoc.ReconDate != null))
			{
				if (lastDoc.Reconciled == true)
				{
					header.LastReconDate = lastDoc.ReconDate;
					cache.SetValueExt<CARecon.curyBegBalance>(header, lastDoc.CuryBalance);
					cache.SetValueExt<CARecon.curyReconciledBalance>(header,
						header.CuryBegBalance + header.CuryReconciledDebits - header.CuryReconciledCredits);
					cache.SetValueExt<CARecon.curyDiffBalance>(header, header.CuryBalance - header.CuryReconciledBalance);
				}
				else
				{
					cache.RaiseExceptionHandling<CARecon.reconNbr>(header, header.ReconNbr, new PXSetPropertyException(Messages.PrevStatementNotReconciled));
				}
			}
		}
		protected virtual void CARecon_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CARecon header = (CARecon)e.Row;

			if (header == null)
			{
				CAReconTranRecords.Cache.SetAllEditPermissions(false);
				CAReconRecords.Cache.SetAllEditPermissions(allowEdit: false);
				Next.SetEnabled(false);
				Previous.SetEnabled(false);
				First.SetEnabled(false);
				Last.SetEnabled(false);
				return;
			}

			if (header.CashAccountID != null && cashaccount.Current?.CashAccountID != header.CashAccountID)
			{
				cashaccount.Current = PXSelectReadonly<CashAccount,
												 Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.
									  Select(this, header.CashAccountID);
			}

			if (cashaccount.Current != null && AddFilter.Current != null && AddFilter.Current.CashAccountID != header.CashAccountID)
			{
				AddFilter.Cache.SetValueExt<AddTrxFilter.cashAccountID>(AddFilter.Current, cashaccount.Current.CashAccountCD);
			}

			PXUIFieldAttribute.SetEnabled<CARecon.reconNbr>(cache, header, true);
			PXUIFieldAttribute.SetEnabled<CARecon.cashAccountID>(cache, header, true);

			bool headerReconciled = header.Reconciled == true;
			bool headerOnHold = header.Hold == true;
			bool headerVoided = header.Voided == true;

			CARecon lastHeader = GetLastRecon(header);

			bool isCashAccountInactive = cashaccount.Current != null && cashaccount.Current.Active != true;
			bool voidEnabled = header.Reconciled == true &&
							   header.Voided == false &&
							   !isCashAccountInactive &&
							   lastHeader != null && lastHeader.ReconNbr == header.ReconNbr;

			bool insertEnabled = header.CashAccountID != null && (lastHeader == null || lastHeader.Reconciled == true || lastHeader.Voided == true);
			bool updateEnabled = IsHeaderUpdateEnabled(header);
			bool deleteEnabled = !headerReconciled && !headerVoided && (cache.GetStatus(header) == PXEntryStatus.Inserted || lastHeader != null && lastHeader.ReconNbr == header.ReconNbr);
			bool releaseEnabled = !headerReconciled && !headerOnHold && !headerVoided;

			Voided.SetEnabled(voidEnabled);

			CAReconRecords.Cache.AllowUpdate = updateEnabled;
			CAReconRecords.Cache.AllowDelete = deleteEnabled;
			CAReconRecords.Cache.AllowInsert = insertEnabled;

			CAReconTranRecords.Cache.AllowDelete = false; // updateEnabled;
			CAReconTranRecords.Cache.AllowInsert = false; // updateEnabled;
			CAReconTranRecords.Cache.AllowUpdate = updateEnabled;

			PXUIFieldAttribute.SetEnabled<CARecon.curyBalance>(cache, header, !headerReconciled && !headerVoided);
			PXUIFieldAttribute.SetEnabled<CARecon.hold>(cache, header, !headerReconciled && !headerVoided);
			PXUIFieldAttribute.SetEnabled<CARecon.reconDate>(cache, header, !headerReconciled && !headerVoided);
			PXUIFieldAttribute.SetEnabled<CARecon.loadDocumentsTill>(cache, header, !headerReconciled && !headerVoided);

			PXUIFieldAttribute.SetVisible<CARecon.skipVoided>(cache, header, ((CASetup)PXSelect<CASetup>.Select(this)).SkipVoided != header.SkipVoided);

			bool showBatchPaymentsVisible = cashaccount.Current?.MatchToBatch != null &&
											header.ShowBatchPayments != null &&
											cashaccount.Current.MatchToBatch != header.ShowBatchPayments;

			PXUIFieldAttribute.SetVisible<CARecon.showBatchPayments>(cache, header, showBatchPaymentsVisible);


			AddFilter.Cache.RaiseRowSelected(AddFilter.Current);
			Release.SetEnabled(releaseEnabled);

			bool nextEnabled = false;
			bool prevEnabled = false;
			bool lastEnabled = false;
			bool firstEnabled = false;

			if (header.CashAccountID.HasValue)
			{
				if (CAReconRecords.Cache.GetStatus(header) == PXEntryStatus.Inserted)
				{
					CARecon cachesReconPrev = PXSelectReadonly<CARecon,
														 Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
													   OrderBy<
															   Desc<CARecon.reconDate,
															   Desc<CARecon.reconNbr>>>>.
														Select(this, header.CashAccountID);

					prevEnabled = cachesReconPrev != null;
				}
				else
				{
					CARecon cachesReconNext = PXSelectReadonly<CARecon,
														 Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																			  And<CARecon.reconNbr, Greater<Required<CARecon.reconNbr>>>>,
													   OrderBy<
															   Asc<CARecon.reconDate, Asc<CARecon.reconNbr>>>>.
														Select(this, header.CashAccountID, header.ReconNbr);

					CARecon cachesReconPrev = PXSelectReadonly<CARecon,
														 Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																			  And<CARecon.reconNbr, Less<Required<CARecon.reconNbr>>>>,
													   OrderBy<
															   Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>.
														Select(this, header.CashAccountID, header.ReconNbr);

					nextEnabled = cachesReconNext != null;
					prevEnabled = cachesReconPrev != null;
				}

				firstEnabled = prevEnabled;
				lastEnabled = nextEnabled;
			}

			Next.SetEnabled(nextEnabled);
			Previous.SetEnabled(prevEnabled);
			First.SetEnabled(firstEnabled);
			Last.SetEnabled(lastEnabled);

			bool CuryViewStateNotSet = (this.Accessinfo.CuryViewState != true);
			PXUIFieldAttribute.SetVisible<CARecon.curyBegBalance>(cache, header, CuryViewStateNotSet);
			PXUIFieldAttribute.SetVisible<CARecon.curyBalance>(cache, header, CuryViewStateNotSet);
			PXUIFieldAttribute.SetVisible<CARecon.curyDiffBalance>(cache, header, CuryViewStateNotSet);
			PXUIFieldAttribute.SetVisible<CARecon.curyReconciledBalance>(cache, header, CuryViewStateNotSet);
		}

		protected virtual void CARecon_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CARecon row = e.Row as CARecon;
			if (row == null) return;

			CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
				.Select(this, e.NewValue)?.First();
			if (cashAccount?.Active == false)
			{
				sender.RaiseExceptionHandling<CARecon.cashAccountID>(row, e.NewValue,
					new PXSetPropertyException(Messages.CashAccountInactive, PXErrorLevel.RowError, cashAccount.CashAccountCD));
				e.Cancel = true;
			}
		}
		protected virtual void CARecon_Hold_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CARecon header = (CARecon)e.Row;
			if ((bool)e.NewValue != true)
			{
				if (header.CuryDiffBalance != null && header.CuryDiffBalance != 0)
				{
					cache.RaiseExceptionHandling<CARecon.curyBalance>(header, header.CuryBalance, new PXSetPropertyException(Messages.DocumentOutOfBalance));
				}
			}
		}
		protected virtual void CARecon_ReconDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CARecon row = (CARecon)e.Row;
			if (row.CashAccountID != null && e.NewValue != null)
			{
				if (row.LastReconDate != null && ((DateTime)e.NewValue).Date <= row.LastReconDate.Value.Date)
				{
					throw new PXSetPropertyException(Messages.ReconDateNotAvailable);
				}
			}
		}
		protected virtual void CARecon_LoadDocumentsTill_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CARecon row = (CARecon)e.Row;

			DateTime? newValue = (DateTime?)e.NewValue;
			if (row?.ReconDate != null && newValue != null && newValue < row.ReconDate)
			{
				cache.RaiseExceptionHandling<CARecon.loadDocumentsTill>(row, newValue, new PXSetPropertyException(Messages.LastDateToLoadNotAvailable, PXErrorLevel.Warning));
			}
		}
		protected virtual void CARecon_ReconDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CARecon header = (CARecon)e.Row;
			if (header == null)
			{
				return;
			}

			header.LoadDocumentsTill = header.ReconDate;

			foreach (CATranExt res in PXSelect<CATranExt,
				Where<CATranExt.reconNbr, Equal<Required<CARecon.reconNbr>>,
				And<CATranExt.cashAccountID, Equal<Required<CARecon.cashAccountID>>>>>
												  .Select(this, header.ReconNbr, header.CashAccountID))
			{
				if (res.Reconciled == true)
				{
					if (header.SkipVoided == true && Skip(res))
					{
						continue; //Skip voiding transaction
					}

					CATranExt newrow = PXCache<CATranExt>.CreateCopy(res);
					newrow.ReconDate = header.ReconDate;
					CAReconTranRecords.Update(newrow);
				}
			}
		}
		#endregion

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", CR.Messages.BAccountName)]
		protected virtual void BAccountR_AcctName_CacheAttached(PXCache sender) { }

		#region Obsolete
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public partial class CATran2 : CATranVoiding
		{
			#region BqlFields override
			public abstract new class tranID : PX.Data.BQL.BqlLong.Field<tranID>
			{
			}
			public abstract new class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
			{
			}
			public abstract new class origModule : PX.Data.BQL.BqlString.Field<origModule>
			{
			}
			public abstract new class origTranType : PX.Data.BQL.BqlString.Field<origTranType>
			{
			}
			public abstract new class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>
			{
			}
			public abstract new class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr>
			{
			}
			public abstract new class isPaymentChargeTran : PX.Data.BQL.BqlBool.Field<isPaymentChargeTran>
			{
			}
			public abstract new class reconNbr : PX.Data.BQL.BqlString.Field<reconNbr>
			{
			}
			public abstract new class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate>
			{
			}
			public new abstract class voidedTranID : PX.Data.BQL.BqlLong.Field<voidedTranID>
			{
			}
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released>
			{
			}
			public new abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled>
			{
			}
			public new abstract class reconDate : PX.Data.BQL.BqlDateTime.Field<reconDate>
			{
			}
			public new abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared>
			{
			}
			public new abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate>
			{
			}
			#endregion
		}
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public partial class CATranR : CATranInBatch
		{
			#region BqlFields override
			public abstract new class tranID : PX.Data.BQL.BqlLong.Field<tranID>
			{
			}
			public abstract new class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
			{
			}
			public abstract new class origModule : PX.Data.BQL.BqlString.Field<origModule>
			{
			}
			public abstract new class origTranType : PX.Data.BQL.BqlString.Field<origTranType>
			{
			}
			public abstract new class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>
			{
			}
			public abstract new class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr>
			{
			}
			public abstract new class isPaymentChargeTran : PX.Data.BQL.BqlBool.Field<isPaymentChargeTran>
			{
			}
			public abstract new class reconNbr : PX.Data.BQL.BqlString.Field<reconNbr>
			{
			}
			public abstract new class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate>
			{
			}
			public new abstract class voidedTranID : PX.Data.BQL.BqlLong.Field<voidedTranID>
			{
			}
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released>
			{
			}
			public new abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled>
			{
			}
			public new abstract class reconDate : PX.Data.BQL.BqlDateTime.Field<reconDate>
			{
			}
			public new abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared>
			{
			}
			public new abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate>
			{
			}
			#endregion

		}
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public static void ReconVoided(CARecon recon)
		{
			VoidCARecon(recon);
		}
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public static void ReconRelease(CARecon recon, bool updateCATranInfo)
		{
			ReleaseCARecon(recon);
		}
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public virtual bool headerUpdateEnabled(CARecon header)
		{
			return IsHeaderUpdateEnabled(header);
		}
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public PXSelect<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>, And<CARecon.reconNbr, Equal<Required<CARecon.reconNbr>>>>> CAReconRecordByCashAccountAndNumbr;

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public PXSelectJoin<CATranExt, InnerJoin<CATran2, On<CATran2.cashAccountID, Equal<CATranExt.cashAccountID>,
																And<CATran2.voidedTranID, Equal<CATranExt.tranID>,
																And<CATran2.released, Equal<False>>>>>,
											Where<CATranExt.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
											And<Where<CATranExt.reconNbr, Equal<Required<CARecon.reconNbr>>>>>> PartiallyVoidedTrans;
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public PXSelect<CABatch, Where<CABatch.batchNbr, Equal<Required<CABatch.batchNbr>>>> cabatches;
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public PXSelectJoin<CATranR,
				InnerJoinSingleTable<Light.APPayment,
					On<Light.APPayment.cATranID, Equal<CATranR.tranID>>,
				InnerJoin<CABatchDetail,
					On<BatchModule.moduleAP, Equal<CABatchDetail.origModule>,
					And<Light.APPayment.docType, Equal<CABatchDetail.origDocType>,
					And<Light.APPayment.refNbr, Equal<CABatchDetail.origRefNbr>>>>>>,
				Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>>> transactionsInBatch;
		#endregion
	}
}