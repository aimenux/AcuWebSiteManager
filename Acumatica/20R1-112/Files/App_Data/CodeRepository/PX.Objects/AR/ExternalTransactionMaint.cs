using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.SO;
using PX.Objects.CA;
using PX.Data;

namespace PX.Objects.AR
{
	[PXHidden]
	public class ExternalTransactionMaint : PXGraph<ExternalTransactionMaint,ExternalTransaction>
	{
		#region Select

		public PXSelect<ExternalTransaction> CurrentTransaction;

		public PXSelectJoin<ExternalTransaction,
						LeftJoin<CustomerPaymentMethod,
							On<ExternalTransaction.pMInstanceID, Equal<CustomerPaymentMethod.pMInstanceID>>,
						LeftJoin<CCProcessingCenter,
							On<CustomerPaymentMethod.cCProcessingCenterID, Equal<CCProcessingCenter.processingCenterID>>,
						LeftJoin<CashAccount,
							On<CCProcessingCenter.cashAccountID, Equal<CashAccount.cashAccountID>>>>>,
						Where<ExternalTransaction.transactionID, Equal<Current<ExternalTransaction.transactionID>>>>
			Transaction;

		public PXSelect<CustomerPaymentMethod,
			Where<CustomerPaymentMethod.pMInstanceID, Equal<Current<ExternalTransaction.pMInstanceID>>>> customerPaymentMethod;

		public PXSelect<CCProcTran, 
					Where<CCProcTran.transactionID, Equal<Current<ExternalTransaction.transactionID>>>, 
					OrderBy<
						Desc<CCProcTran.tranNbr>>> 
			ccProcTran;

		#endregion

		#region Action

		public PXAction<ExternalTransaction> validateCCPayment;
		[PXUIField(DisplayName = "Validate CC Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable ValidateCCPayment(PXAdapter adapter)
		{
			List<IExternalTransaction> list = new List<IExternalTransaction>();
			foreach (ExternalTransaction doc in adapter.Get<ExternalTransaction>())
			{
				list.Add(doc);
			}

			PXLongOperation.StartOperation(this, delegate ()
			{
				ExternalTransactionValidation.ValidateCCPayment(this, list, isMassProcess: false);
			});

			return list;
		}
		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Transaction Date")]
		protected virtual void ExternalTransaction_LastActivityDate_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		protected virtual void CustomerPaymentMethod_CCProcessingCenterID_CacheAttached(PXCache sender) { }
		#endregion

		#region Row events

		public virtual void _(Events.RowSelected<ExternalTransaction> e)
		{
			ExternalTransaction tran = (ExternalTransaction)e.Row;
			PXCache cache = e.Cache;

			if (tran == null)
				return;

			PXUIFieldAttribute.SetVisible<ExternalTransaction.origDocType>(cache, tran, tran.OrigDocType != null);
			PXUIFieldAttribute.SetVisible<ExternalTransaction.origRefNbr>(cache, tran, tran.OrigRefNbr != null);
		}

		#endregion

		public ExternalTransactionMaint()
		{
			ccProcTran.Cache.AllowDelete = false;
			ccProcTran.Cache.AllowUpdate = false;
			ccProcTran.Cache.AllowInsert = false;
			CurrentTransaction.Cache.AllowInsert = false;
			CurrentTransaction.Cache.AllowDelete = false;
		}
	}
}