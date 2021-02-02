using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	#region TransferCashTranIDAttribute
	/// <summary>
	/// Specialized for the Transfer version of the CashTranID attribute<br/>
	/// Defines methods to create new CATran from (and for) CATransfer document<br/>
	/// Should be used on the CATransfer - derived types only.
	/// <example>
	/// [TransferCashTranID()]
	/// </example>
	/// </summary>
	public class TransferCashTranIDAttribute : CashTranIDAttribute
	{
		protected bool _IsIntegrityCheck = false;

		public static CATran DefaultValues<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
			{
				if (attr is TransferCashTranIDAttribute)
				{
					((TransferCashTranIDAttribute)attr)._IsIntegrityCheck = true;
					return ((TransferCashTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
				}
			}
			return null;
		}

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			CATransfer parentDoc = (CATransfer)orig_Row;
			if ((parentDoc.Released == true) && (catran_Row.TranID != null))
			{
				return null;
			}
			if (catran_Row.TranID == null || catran_Row.TranID < 0)
			{
				catran_Row.OrigModule = BatchModule.CA;
				catran_Row.OrigRefNbr = parentDoc.TransferNbr;
			}

			if (object.Equals(_FieldOrdinal, sender.GetFieldOrdinal<CATransfer.tranIDOut>()))
			{
				catran_Row.CashAccountID = parentDoc.OutAccountID;
				catran_Row.OrigTranType = CATranType.CATransferOut;
				catran_Row.ExtRefNbr = parentDoc.OutExtRefNbr;
				catran_Row.CuryID = parentDoc.OutCuryID;
				catran_Row.CuryInfoID = parentDoc.OutCuryInfoID;
				catran_Row.CuryTranAmt = -parentDoc.CuryTranOut;
				catran_Row.TranAmt = -parentDoc.TranOut;
				catran_Row.DrCr = DrCr.Credit;
				catran_Row.Cleared = parentDoc.ClearedOut;
				catran_Row.ClearDate = parentDoc.ClearDateOut;
				catran_Row.TranDate = parentDoc.OutDate;
			    SetPeriodsByMaster(sender, catran_Row, parentDoc.OutTranPeriodID);
            }
			else if (object.Equals(_FieldOrdinal, sender.GetFieldOrdinal<CATransfer.tranIDIn>()))
			{
				catran_Row.CashAccountID = parentDoc.InAccountID;
				catran_Row.OrigTranType = CATranType.CATransferIn;
				catran_Row.ExtRefNbr = parentDoc.InExtRefNbr;
				catran_Row.CuryID = parentDoc.InCuryID;
				catran_Row.CuryInfoID = parentDoc.InCuryInfoID;
				catran_Row.CuryTranAmt = parentDoc.CuryTranIn;
				catran_Row.TranAmt = parentDoc.TranIn;
				catran_Row.DrCr = DrCr.Debit;
				catran_Row.Cleared = parentDoc.ClearedIn;
				catran_Row.ClearDate = parentDoc.ClearDateIn;
				catran_Row.TranDate = parentDoc.InDate;
			    SetPeriodsByMaster(sender, catran_Row, parentDoc.InTranPeriodID);
            }
			else
			{
				throw new PXException(AP.Messages.UnknownDocumentType);
			}

			SetCleared(catran_Row, GetCashAccount(catran_Row, sender.Graph));

			catran_Row.TranDesc = parentDoc.Descr;
			catran_Row.ReferenceID = null;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;

			return catran_Row;
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisting(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisted(sender, e);
			}
		}
	}
	#endregion

	#region AdjCashTranIDAttribute
	public class AdjCashTranIDAttribute : CashTranIDAttribute
	{
		protected bool _IsIntegrityCheck = false;

		public static CATran DefaultValues<Field>(PXCache sender, object data)
		where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
			{
				if (attr is AdjCashTranIDAttribute)
				{
					((AdjCashTranIDAttribute)attr)._IsIntegrityCheck = true;
					return ((AdjCashTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
				}
			}
			return null;
		}

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			CAAdj parentDoc = (CAAdj)orig_Row;
			if ((parentDoc.Released == true) && (catran_Row.TranID != null))
			{
				return null;
            }
            if (catran_Row.TranID == null || catran_Row.TranID < 0)
            {
                catran_Row.OrigModule = BatchModule.CA;
                catran_Row.OrigTranType = parentDoc.AdjTranType;

                if (parentDoc.TransferNbr == null)
                {
                    catran_Row.OrigRefNbr = parentDoc.AdjRefNbr;
                }
                else
                {
                    catran_Row.OrigRefNbr = parentDoc.TransferNbr;
                }
            }

			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CuryID = parentDoc.CuryID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryTranAmt = parentDoc.CuryTranAmt * (parentDoc.DrCr == DrCr.Debit ? 1 : -1);
			catran_Row.TranAmt = parentDoc.TranAmt * (parentDoc.DrCr == DrCr.Debit ? 1 : -1);
			catran_Row.DrCr = parentDoc.DrCr;
			catran_Row.TranDate = parentDoc.TranDate;
			catran_Row.TranDesc = parentDoc.TranDesc;
			catran_Row.ReferenceID = null;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
		    SetPeriodsByMaster(sender, catran_Row, parentDoc.TranPeriodID);
            catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;

			SetCleared(catran_Row, GetCashAccount(catran_Row, sender.Graph));

			return catran_Row;
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisting(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisted(sender, e);
			}
		}
	}
	#endregion

	#region DepositTranIDAttribute
	public class DepositTranIDAttribute : CashTranIDAttribute
	{
		protected bool _IsIntegrityCheck = false;

		public static CATran DefaultValues(PXCache sender, CATran catran_Row, CADeposit parentDoc, string fieldName)
		{
			if (((parentDoc.Released == true) && (catran_Row.TranID != null)))
			{
				return null;
			}
			
			catran_Row.OrigModule = BatchModule.CA;
			catran_Row.OrigTranType = parentDoc.TranType;
			catran_Row.OrigRefNbr = parentDoc.RefNbr;
			
			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CuryID = parentDoc.CuryID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryTranAmt = parentDoc.CuryTranAmt * (parentDoc.DrCr == DrCr.Debit ? 1 : -1);
			catran_Row.TranAmt = parentDoc.TranAmt * (parentDoc.DrCr == DrCr.Debit ? 1 : -1);
			catran_Row.DrCr = parentDoc.DrCr;
			catran_Row.TranDate = parentDoc.TranDate;
			catran_Row.TranDesc = parentDoc.TranDesc;
			catran_Row.ReferenceID = null;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
		    SetPeriodsByMaster(sender, catran_Row, parentDoc.TranPeriodID);
            catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;
			if (parentDoc.DocType == CATranType.CAVoidDeposit)
			{
				CADeposit voidedDoc = PXSelectReadonly<CADeposit, Where<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>,
												And<CADeposit.tranType, Equal<Required<CADeposit.tranType>>>>>.Select(sender.Graph, parentDoc.RefNbr, CATranType.CADeposit);
				if (voidedDoc != null)
				{
					catran_Row.VoidedTranID = (long?)sender.GetValue(voidedDoc, fieldName);
				}
			}

			SetCleared(catran_Row, GetCashAccount(catran_Row, sender.Graph));

			return catran_Row;
		}

		public static CATran DefaultValues<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
			{
				if (attr is DepositTranIDAttribute)
				{
					((DepositTranIDAttribute)attr)._IsIntegrityCheck = true;
					return ((DepositTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
				}
			}
			return null;
		}

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return DefaultValues(sender, catran_Row, (CADeposit)orig_Row, this._FieldName);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisting(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisted(sender, e);
			}
		}
	}
	#endregion

	#region DepositCashTranIDAttribute
	/// <summary>
	/// Creates the cash transaction on the <see cref="CADeposit.ExtraCashAccountID"/> Cash Drop Account.
	/// </summary>
	public class DepositCashTranIDAttribute : CashTranIDAttribute
	{
		protected bool _IsIntegrityCheck = false;

		public static CATran DefaultValues(PXCache sender, CATran catran_Row, CADeposit parentDoc, string fieldName)
		{
			if ((parentDoc.Released == true) && (catran_Row.TranID != null) ||
				IsCreationNeeded(parentDoc) == false)
			{
				return null;
			}
			
				catran_Row.OrigModule = BatchModule.CA;
				catran_Row.OrigTranType = parentDoc.TranType;
				catran_Row.OrigRefNbr = parentDoc.RefNbr;
			
			catran_Row.CashAccountID = parentDoc.ExtraCashAccountID;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CuryID = parentDoc.CuryID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.DrCr = parentDoc.DrCr == CADrCr.CADebit ? CADrCr.CACredit : CADrCr.CADebit;
			catran_Row.CuryTranAmt = parentDoc.CuryExtraCashTotal * (catran_Row.DrCr == CADrCr.CADebit ? 1 : -1);
			catran_Row.TranAmt = parentDoc.ExtraCashTotal * (catran_Row.DrCr == CADrCr.CADebit ? 1 : -1);

			catran_Row.TranDate = parentDoc.TranDate;
			catran_Row.TranDesc = parentDoc.TranDesc;
			catran_Row.ReferenceID = null;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
		    SetPeriodsByMaster(sender, catran_Row, parentDoc.TranPeriodID);
            catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;
			if (parentDoc.DocType == CATranType.CAVoidDeposit)
			{
				CADeposit voidedDoc = PXSelectReadonly<CADeposit, Where<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>,
												And<CADeposit.tranType, Equal<Required<CADeposit.tranType>>>>>.Select(sender.Graph, parentDoc.RefNbr, CATranType.CADeposit);
				if (voidedDoc != null)
				{
					catran_Row.VoidedTranID = (long?)sender.GetValue(voidedDoc, fieldName);
				}
			}

			SetCleared(catran_Row, GetCashAccount(catran_Row, sender.Graph));

			return catran_Row;
		}

		public static CATran DefaultValues<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
			{
				if (attr is DepositCashTranIDAttribute)
				{
					((DepositCashTranIDAttribute)attr)._IsIntegrityCheck = true;
					return ((DepositCashTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
				}
			}
			return null;
		}

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return DefaultValues(sender, catran_Row, (CADeposit)orig_Row, this._FieldName);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{

				base.RowPersisting(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisted(sender, e);
			}
		}

		protected static bool IsCreationNeeded(CADeposit parentDoc)
		{
			return (parentDoc.ExtraCashAccountID != null
					&& parentDoc.ExtraCashTotal != null
					&& parentDoc.ExtraCashTotal != decimal.Zero);
		}
	}
	#endregion

	#region DepositChargeTranIDAttribute
	public class DepositChargeTranIDAttribute : CashTranIDAttribute
	{
		protected bool _IsIntegrityCheck = false;

		public static CATran DefaultValues(PXCache sender, CATran catran_Row, CADeposit parentDoc, string fieldName)
		{
			if ((parentDoc.Released == true) && (catran_Row.TranID != null)
				|| IsCreationNeeded(parentDoc) == false)
			{
				return null;
			}
			
				catran_Row.OrigModule = BatchModule.CA;
				catran_Row.OrigTranType = parentDoc.TranType;
				catran_Row.OrigRefNbr = parentDoc.RefNbr;
			
			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CuryID = parentDoc.CuryID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.DrCr = parentDoc.DrCr == CADrCr.CADebit ? CADrCr.CACredit : CADrCr.CADebit;
			catran_Row.CuryTranAmt = parentDoc.CuryChargeTotal * (catran_Row.DrCr == CADrCr.CADebit ? 1 : -1);
			catran_Row.TranAmt = parentDoc.ChargeTotal * (catran_Row.DrCr == CADrCr.CADebit ? 1 : -1);
			catran_Row.TranDate = parentDoc.TranDate;
			catran_Row.TranDesc = parentDoc.TranDesc;
			catran_Row.ReferenceID = null;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
		    SetPeriodsByMaster(sender, catran_Row, parentDoc.TranPeriodID);
            catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;
			if (parentDoc.DocType == CATranType.CAVoidDeposit)
			{
				CADeposit voidedDoc = PXSelectReadonly<CADeposit, Where<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>,
												And<CADeposit.tranType, Equal<Required<CADeposit.tranType>>>>>.Select(sender.Graph, parentDoc.RefNbr, CATranType.CADeposit);
				if (voidedDoc != null)
				{
					catran_Row.VoidedTranID = (long?)sender.GetValue(voidedDoc, fieldName);
				}
			}

			SetCleared(catran_Row, GetCashAccount(catran_Row, sender.Graph));

			return catran_Row;
		}

		public static CATran DefaultValues<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
			{
				if (attr is DepositCashTranIDAttribute)
				{
					((DepositChargeTranIDAttribute)attr)._IsIntegrityCheck = true;
					return ((DepositChargeTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
				}
			}
			return null;
		}

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return DefaultValues(sender, catran_Row, (CADeposit)orig_Row, this._FieldName);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					PXCache cache = sender.Graph.Caches[typeof(CATran)];
					CATran info = null;
					if (key != null)
					{
						info = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATran.tranID>>>>.Select(sender.Graph, key);
						if (info == null)
						{
							key = null;
							sender.SetValue(e.Row, _FieldOrdinal, null);
						}
					}
					if (info != null && !IsCreationNeeded((CADeposit)e.Row))
					{
						cache.Delete(info);
						cache.PersistDeleted(info);
					}
				}
				base.RowPersisting(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisted(sender, e);
			}
		}

		protected static bool IsCreationNeeded(CADeposit parentDoc)
		{
			return !(parentDoc.ChargesSeparate != true || parentDoc.CuryChargeTotal == null ||
				 parentDoc.CuryChargeTotal == 0);

		}
	}
	#endregion

	#region DepositDetailTranIDAttribute
	public class DepositDetailTranIDAttribute : CashTranIDAttribute
	{
		protected bool _IsIntegrityCheck = false;

		public static CATran DefaultValues(PXCache sender, CATran catran_Row, CADepositDetail orig_Row)
		{
			CADepositDetail parentDoc = orig_Row;
			CADeposit deposit = PXSelect<CADeposit, Where<CADeposit.tranType, Equal<Required<CADeposit.tranType>>,
															And<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>>>>.Select(sender.Graph, parentDoc.TranType, parentDoc.RefNbr);
			if ((parentDoc.Released == true) && (catran_Row.TranID != null) ||
				 parentDoc.CuryTranAmt == null ||
				 parentDoc.CuryTranAmt == 0)
			{
				return null;
			}
			
				catran_Row.OrigModule = BatchModule.CA;
				catran_Row.OrigTranType = parentDoc.TranType;
				catran_Row.OrigRefNbr = parentDoc.RefNbr;
			
			catran_Row.CashAccountID = parentDoc.AccountID;

			catran_Row.CuryID = parentDoc.OrigCuryID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryTranAmt = parentDoc.CuryOrigAmtSigned * (parentDoc.DrCr == CADrCr.CADebit ? 1 : -1);
			catran_Row.TranAmt = parentDoc.OrigAmtSigned * (parentDoc.DrCr == CADrCr.CADebit ? 1 : -1);
			catran_Row.DrCr = parentDoc.DrCr;
			catran_Row.TranDesc = parentDoc.TranDesc;
			catran_Row.ReferenceID = null;
			catran_Row.Released = parentDoc.Released;
			if (parentDoc.DetailType == CADepositDetailType.CheckDeposit || parentDoc.DetailType == CADepositDetailType.VoidCheckDeposit)
			{
				catran_Row.TranDesc = string.Format("{0}-{1}", parentDoc.OrigDocType, parentDoc.OrigRefNbr);
			}
			if (deposit != null)
			{
				catran_Row.Hold = deposit.Hold;
			    SetPeriodsByMaster(sender, catran_Row, deposit.TranPeriodID);
                catran_Row.ExtRefNbr = deposit.ExtRefNbr;
				catran_Row.TranDate = deposit.TranDate;
				catran_Row.Cleared = deposit.Cleared;
				catran_Row.ClearDate = deposit.ClearDate;             
			}
			if (parentDoc.DetailType == CADepositDetailType.VoidCheckDeposit)
			{
				CADepositDetail voidedDoc = PXSelectReadonly<CADepositDetail, Where<CADepositDetail.refNbr, Equal<Required<CADepositDetail.refNbr>>,
												And<CADepositDetail.tranType, Equal<Required<CADepositDetail.tranType>>,
													And<CADepositDetail.lineNbr, Equal<Required<CADepositDetail.lineNbr>>>>>>.Select(sender.Graph, parentDoc.RefNbr, CATranType.CADeposit, parentDoc.LineNbr);
				if (voidedDoc != null)
				{
					catran_Row.VoidedTranID = voidedDoc.TranID;
				}
			}

			SetCleared(catran_Row, GetCashAccount(catran_Row, sender.Graph));

			return catran_Row;
		}

		public static CATran DefaultValues<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
			{
				if (attr is DepositDetailTranIDAttribute)
				{
					((DepositDetailTranIDAttribute)attr)._IsIntegrityCheck = true;
					return ((DepositDetailTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
				}
			}
			return null;
		}

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return DepositDetailTranIDAttribute.DefaultValues(sender, catran_Row, (CADepositDetail)orig_Row);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisting(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisted(sender, e);
			}
		}
	}
	#endregion

	#region CashTranIDAttribute

	public abstract class CashTranIDAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXRowDeletedSubscriber
	{
		#region State
		protected object _KeyToAbort;
		protected Type _ChildType;
		private object _SelfKeyToAbort;
		private Dictionary<long?, object> _persisted;

		#endregion

		public CashTranIDAttribute()
			: base()
		{
		}

		public abstract CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row);

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_ChildType = sender.GetItemType();
			_persisted = new Dictionary<long?, object>();

			sender.Graph.RowPersisting.AddHandler<CATran>(CATran_RowPersisting);
			sender.Graph.RowPersisted.AddHandler<CATran>(CATran_RowPersisted);
		}

		#region Events
		public virtual void CATran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_SelfKeyToAbort = sender.GetValue<CATran.tranID>(e.Row);
			}
		}

		public virtual void CATran_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[_ChildType];
			long? newKey;

			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _SelfKeyToAbort != null)
			{
				newKey = (long?)sender.GetValue<CATran.tranID>(e.Row);

				if (!_persisted.ContainsKey(newKey))
				{
					_persisted.Add(newKey, _SelfKeyToAbort);
				}

				foreach (object item in cache.Inserted)
				{
					if ((long?)cache.GetValue(item, _FieldOrdinal) == (long?)_SelfKeyToAbort)
					{
						cache.SetValue(item, _FieldOrdinal, newKey);
					}
				}

				foreach (object item in cache.Updated)
				{
					if ((long?)cache.GetValue(item, _FieldOrdinal) == (long?)_SelfKeyToAbort)
					{
						cache.SetValue(item, _FieldOrdinal, newKey);
					}
				}

				_SelfKeyToAbort = null;
			}

			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				foreach (object item in cache.Inserted)
				{
					if ((newKey = (long?)cache.GetValue(item, _FieldOrdinal)) != null && _persisted.TryGetValue(newKey, out _SelfKeyToAbort))
					{
						cache.SetValue(item, _FieldOrdinal, _SelfKeyToAbort);
					}
				}

				foreach (object item in cache.Updated)
				{
					if ((newKey = (long?)cache.GetValue(item, _FieldOrdinal)) != null && _persisted.TryGetValue(newKey, out _SelfKeyToAbort))
					{
						cache.SetValue(item, _FieldOrdinal, _SelfKeyToAbort);
					}
				}
			}

            if (e.TranStatus != PXTranStatus.Open)
            {
                _KeyToAbort = null;
                _SelfKeyToAbort = null;
            }
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (sender.Graph.Views.Caches.Contains(typeof(CATran)))
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				PXCache cache = sender.Graph.Caches[typeof(CATran)];
				if (key != null)
				{
					CATran info = new CATran { TranID = (long?)key };
					cache.Delete(info);

					sender.SetValue(e.Row, _FieldOrdinal, null);
				}
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object key = sender.GetValue(e.Row, _FieldOrdinal);
			PXCache cache = sender.Graph.Caches[typeof(CATran)];
			CATran info = null;

			if (key != null)
			{
				//do not read cached record in release processes
				if ((info = PXSelectReadonly<CATran, Where<CATran.tranID, Equal<Required<CATran.tranID>>>>.Select(sender.Graph, key)) != null)
				{
					CATran cached = (CATran)cache.Locate(info);
					if (cached != null)
					{
						if ((cached.OrigModule != null && info.OrigModule != null && cached.OrigModule != info.OrigModule) ||
							(cached.OrigRefNbr != null && info.OrigRefNbr != null && cached.OrigRefNbr != info.OrigRefNbr) ||
							(cached.OrigTranType != null && info.OrigTranType != null && cached.OrigTranType != info.OrigTranType))
						{
							// TO Be removed after solving CATran issue (check JIRA item AC-57875 for details)
							throw new PXException(Messages.CouldNotInsertCATran); 
						}
						if (cache.GetStatus(cached) == PXEntryStatus.Notchanged)
						{
							PXCache<CATran>.RestoreCopy(cached, info);
						}
						info = cached;
					}
					else
					{
						cache.SetStatus(info, PXEntryStatus.Notchanged);
					}
				}

				if ((long)key < 0L && info == null)
				{
					info = new CATran();
					info.TranID = (long)key;
					info = (CATran)cache.Locate(info);
				}

				if (info == null)
				{
					key = null;
					sender.SetValue(e.Row, _FieldOrdinal, null);
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				if (info != null)
				{
					cache.Delete(info);
					cache.PersistDeleted(info);
				}
			}
			else if (info == null)
			{
                if (!this.NeedPreventCashTransactionCreation(sender, e.Row))
                {
                    info = DefaultValues(sender, new CATran(), e.Row);

                    if (info != null)
                    {
                        info = (CATran)cache.Insert(info);
                        if (!(info.TranID < 0L))
						{
                            throw new PXException(Messages.CouldNotInsertCATran);
						}

						// TO Be removed after solving CATran issue (check JIRA item AC-57875 for details)
                        sender.SetValue(e.Row, _FieldOrdinal, info.TranID);
                        _KeyToAbort = info.TranID;
                        cache.PersistInserted(info);
                        long id = info.TranID ?? 0;
                        if (id == 0 || id < 0)
                        {
                            throw new PXException(Messages.CATranNotSaved, sender.GetItemType().Name);
                        }
                        sender.SetValue(e.Row, _FieldOrdinal, id);
                        info.TranID = id;
                        cache.Normalize();
                    }
                }
			}
			else if (info.TranID < 0L)
			{
				info = DefaultValues(sender, PXCache<CATran>.CreateCopy(info), e.Row);
				if (info != null)
				{
				info = (CATran)cache.Update(info);

				sender.SetValue(e.Row, _FieldOrdinal, info.TranID);
				_KeyToAbort = info.TranID;
				cache.PersistInserted(info);
				long id = info.TranID ?? 0;
				if (id == 0 || id < 0)
				{
					throw new PXException(Messages.CATranNotSaved, sender.GetItemType().Name);
				}
				sender.SetValue(e.Row, _FieldOrdinal, id);
				info.TranID = id;
				cache.Normalize();
			}
			}
			else
			{
				CATran copy = PXCache<CATran>.CreateCopy(info);
				copy = DefaultValues(sender, copy, e.Row);
				if (copy != null)
				{
					if ((copy.OrigModule != null && info.OrigModule != null && copy.OrigModule != info.OrigModule) ||
						(copy.OrigRefNbr != null && info.OrigRefNbr != null && copy.OrigRefNbr != info.OrigRefNbr) ||
						(copy.OrigTranType != null && info.OrigTranType != null && copy.OrigTranType != info.OrigTranType))
					{
						// TO Be removed after solving CATran issue (check JIRA item AC-57875 for details)
						throw new PXException(Messages.CouldNotInsertCATran);
					}
					info = (CATran)cache.Update(copy);
					//to avoid another process updated DefaultValues will return null for Released docs, except for GLTran
					cache.PersistUpdated(info);
				}
				//JournalEntry is usually persisted prior to ReleaseGraph to obtain BatchNbr reference, read info should contain set Released flag
				else if (info.Released == false)
				{
					key = null;
					sender.SetValue(e.Row, _FieldOrdinal, null);

					cache.Delete(info);
				}
			}
			foreach (CATran toDelete in cache.Deleted)
			{
				cache.PersistDeleted(toDelete);
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(CATran)];

			if (e.TranStatus == PXTranStatus.Open)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
			}
			else if (e.TranStatus == PXTranStatus.Aborted)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (_KeyToAbort != null && (long)_KeyToAbort < 0L)
				{
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
					foreach (CATran data in cache.Inserted)
					{
						if (Equals(key, data.TranID))
						{
							data.TranID = (long)_KeyToAbort;
							cache.ResetPersisted(data);
							break;
						}
					}
				}
				else
				{
					foreach (CATran data in cache.Updated)
					{
						if (object.Equals(key, data.TranID))
						{
							cache.ResetPersisted(data);
						}
					}
				}

				cache.Normalize();
			}
			else
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				foreach (CATran data in cache.Inserted)
				{
					if (object.Equals(key, data.TranID))
					{
						cache.RaiseRowPersisted(data, PXDBOperation.Insert, e.TranStatus, e.Exception);
						cache.SetStatus(data, PXEntryStatus.Notchanged);
						PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
						cache.ResetPersisted(data);
					}
				}
				foreach (CATran data in cache.Updated)
				{
					if (object.Equals(key, data.TranID))
					{
						cache.RaiseRowPersisted(data, PXDBOperation.Update, e.TranStatus, e.Exception);
						cache.SetStatus(data, PXEntryStatus.Notchanged);
						PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
						cache.ResetPersisted(data);
					}
				}
				foreach (CATran data in cache.Deleted)
				{
					cache.RaiseRowPersisted(data, PXDBOperation.Delete, e.TranStatus, e.Exception);
					cache.SetStatus(data, PXEntryStatus.Notchanged);
					PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
					cache.ResetPersisted(data);
				}
				cache.IsDirty = false;
				cache.Normalize();
			}
		}
		#endregion

		protected static void SetCleared(CATran catran, CashAccount cashAccount)
		{
			if (cashAccount != null && cashAccount.Reconcile == false && (catran.Cleared != true || catran.TranDate == null))
			{
				catran.Cleared = true;
				catran.ClearDate = catran.TranDate;
			}
		}

		protected static CashAccount GetCashAccount(CATran catran, PXGraph graph)
		{
			PXSelectBase<CashAccount> selectStatement = new PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(graph);
			CashAccount cashacc = (CashAccount)selectStatement.View.SelectSingle(catran.CashAccountID);
			return cashacc;
		}

		/// <summary>
		/// Returns <c>true</c> if, during parent row persist, the
		/// corresponding cash transaction row should not be created, e.g.
		/// in case of a document that is part of a recurring schedule.
		/// </summary>
		protected virtual bool NeedPreventCashTransactionCreation(PXCache sender, object row)
		{
			return false;
		}

	    public static void SetPeriodsByMaster(PXCache docCache, CATran caTran, string masterPeriodID)
	    {
            FinPeriodIDAttribute.SetPeriodsByMaster<CATran.finPeriodID>(docCache.Graph.Caches[typeof(CATran)], caTran, masterPeriodID);
	    }
	}
	#endregion
}
