using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.Common;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.CA
{
	[Serializable]
	public class CADepositEntry : PXGraph<CADepositEntry, CADeposit>
	{
		#region Internal defintions

		[Serializable]
		public partial class PaymentFilter : IBqlTable
		{
			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID>
			{
			}

			[PXDBString(10)]
			[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.useForAR, Equal<True>>>))]
			[PXUIField(DisplayName = "Payment Method", Enabled = true)]
			public virtual string PaymentMethodID
			{
				get;
				set;
			}
			#endregion
			#region CashAccountID
			public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
			{
			}

			[CashAccount(null, typeof(Search5<CashAccount.cashAccountID,
										InnerJoin<CashAccountDeposit, On<CashAccountDeposit.depositAcctID, Equal<CashAccount.cashAccountID>>>,
										Where<CashAccountDeposit.accountID, Equal<Current<CADeposit.cashAccountID>>,
										And<Match<Current<AccessInfo.userName>>>>, Aggregate<GroupBy<CashAccount.cashAccountID>>>), DisplayName = "Clearing Account")]
			public virtual int? CashAccountID
			{
				get;
				set;
			}
			#endregion
			#region StartDate
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate>
			{
			}

			[PXDBDate]
			[PXDefault(typeof(CADeposit.tranDate))]
			[PXUIField(DisplayName = "Start Date")]
			public virtual DateTime? StartDate
			{
				get;
				set;
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate>
			{
			}

			[PXDBDate]
			[PXDefault(typeof(CADeposit.tranDate))]
			[PXUIField(DisplayName = "End Date")]
			public virtual DateTime? EndDate
			{
				get;
				set;
			}
			#endregion

		}

		[Serializable]
		public partial class ARPaymentUpdate : Light.ARPayment
		{
			#region DocType
			public abstract new class docType : PX.Data.BQL.BqlString.Field<docType>
			{
			}
			#endregion
			#region RefNbr
			public abstract new class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
			{
			}
			#endregion
			#region DepositDate
			public abstract new class depositDate : PX.Data.BQL.BqlDateTime.Field<depositDate>
			{
			}
			#endregion
			#region DepositType
			public abstract new class depositType : PX.Data.BQL.BqlString.Field<depositType>
			{
			}
			#endregion
			#region DepositNbr
			public abstract new class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr>
			{
			}
			#endregion
			#region Deposited
			public abstract new class deposited : PX.Data.BQL.BqlBool.Field<deposited>
			{
			}
			#endregion
			#region Methods
			public virtual void SetReferenceTo(CADeposit doc)
			{
				this.DepositType = doc.TranType;
				this.DepositNbr = doc.RefNbr;
				this.DepositDate = doc.TranDate;
			}
			public virtual void ClearDepositReference()
			{
				this.DepositType = null;
				this.DepositNbr = null;
				this.DepositDate = null;
			}
			#endregion
		}

		[Serializable]
		public partial class APPaymentUpdate : Light.APPayment
		{
			#region DocType
			public abstract new class docType : PX.Data.BQL.BqlString.Field<docType>
			{
			}
			#endregion
			#region RefNbr
			public abstract new class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
			{
			}
			#endregion
			#region DepositDate
			public abstract new class depositDate : PX.Data.BQL.BqlDateTime.Field<depositDate>
			{
			}
			#endregion
			#region DepositType
			public abstract new class depositType : PX.Data.BQL.BqlString.Field<depositType>
			{
			}
			#endregion
			#region DepositNbr
			public abstract new class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr>
			{
			}
			#endregion
			#region Deposited
			public abstract new class deposited : PX.Data.BQL.BqlBool.Field<deposited>
			{
			}
			#endregion
			#region Methods
			public virtual void SetReferenceTo(CADeposit doc)
			{
				this.DepositType = doc.TranType;
				this.DepositNbr = doc.RefNbr;
				this.DepositDate = doc.TranDate;
			}
			public virtual void ClearDepositReference()
			{
				this.DepositType = null;
				this.DepositNbr = null;
				this.DepositDate = null;
			}
			#endregion
		}
		#endregion

		#region Buttons
		public PXAction<CADeposit> Release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
			Save.Press();
			List<CADeposit> list = new List<CADeposit>();
			CADeposit doc = this.Document.Current;
			if (doc.Released == false && doc.Hold == false)
			{
				list.Add(doc);
				PXLongOperation.StartOperation(this, delegate () { CADepositEntry.ReleaseDoc(doc); });
				return list;
			}

			return adapter.Get();
		}

		public PXAction<CADeposit> VoidDocument;
		[PXUIField(DisplayName = Messages.Void, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable voidDocument(PXAdapter adapter)
		{
			CADeposit doc = this.Document.Current;
			if (doc != null && doc.Released == true && doc.TranType == CATranType.CADeposit)
			{
				try
				{
					this._IsVoidCheckInProgress = true;
					this.VoidDepositProc(doc);
					List<CADeposit> rs = new List<CADeposit>();
					rs.Add(Document.Current);
					return rs;
				}
				finally
				{
					this._IsVoidCheckInProgress = false;
				}
			}
			return adapter.Get();
		}

		public PXAction<CADeposit> addPayment;
		[PXUIField(DisplayName = Messages.AddARPayment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPayment(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				 this.Document.Current.TranType == CATranType.CADeposit &&
				  this.Document.Current.Released != true)
			{
				if (this.filter.Current != null)
				{
					this.filter.Cache.RaiseRowSelected(this.filter.Current);
				}
				bool needRefresh = false;
				if (this.AvailablePayments.AskExt() == WebDialogResult.OK)
				{
					this.AvailablePayments.Cache.AllowInsert = true;
					IEnumerable<PaymentInfo> toAdd = AvailablePayments.Cache.Inserted.Cast<PaymentInfo>().Where(payment => payment.Selected == true);
					AddPaymentInfoBatch(toAdd);
					foreach (PaymentInfo it in toAdd)
					{
						it.Selected = false;
						needRefresh = true;
					}
				}
				else
				{
					foreach (PaymentInfo it in this.AvailablePayments.Cache.Inserted)
						it.Selected = false;
				}
				this.AvailablePayments.Cache.AllowInsert = false;
				this.AvailablePayments.Cache.Clear();
				if (needRefresh)
					this.DepositPayments.View.RequestRefresh();
			}
			return adapter.Get();
		}

		public PXAction<CADeposit> viewBatch;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (this.Document.Current != null)
			{
				string BatchNbr = (string)this.Document.GetValueExt<CADeposit.tranID_CATran_batchNbr>(this.Document.Current);
				if (BatchNbr != null)
				{
					JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
					graph.Clear();
					Batch newBatch = new Batch();
					graph.BatchModule.Current = PXSelect<GL.Batch,
							Where<GL.Batch.module, Equal<GL.BatchModule.moduleCA>,
							And<GL.Batch.batchNbr, Equal<Required<GL.Batch.batchNbr>>>>>
							.Select(this, BatchNbr);
					throw new PXRedirectRequiredException(graph, "Batch Record");
				}
			}
			return adapter.Get();
		}

		public PXAction<CADeposit> report;
		[PXUIField(DisplayName = Messages.Reports, MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter,
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			string reportID,
			[PXBool]
			bool refresh
			)
		{
			if (!String.IsNullOrEmpty(reportID))
			{

				Dictionary<string, string> parameters = new Dictionary<string, string>();
				foreach (CADeposit order in adapter.Get<CADeposit>())
				{
					parameters["TranType"] = order.TranType;
					parameters["RefNbr"] = order.RefNbr;
					if (refresh)
						this.Document.Search<CADeposit.refNbr>(order.TranType, order.RefNbr);

				}
				throw new PXReportRequiredException(parameters, reportID, "Report " + reportID);
			}
			return adapter.Get();
		}


		public PXAction<CADeposit> viewDocument;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (this.DepositPayments.Current != null)
			{
				CADepositDetail row = (CADepositDetail)this.DepositPayments.Current;
				IDocGraphCreator creator = null;
				switch (row.OrigModule)
				{
					case PX.Objects.GL.BatchModule.AP:
						creator = new APDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.AR:
						creator = new ARDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.CA:
						creator = new CADocGraphCreator(); break;
				}
				if (creator != null)
				{
					PXGraph graph = creator.Create(row.OrigDocType, row.OrigRefNbr, null);
					if (graph != null)
					{
						throw new PXRedirectRequiredException(graph, true, "ViewDocument") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
				throw new PXException(Messages.CanNotRedirectToDocumentThisType);
			}
			return adapter.Get();
		}
		#endregion

		#region Ctor + Selects

		public CADepositEntry()
		{
			CASetup setup = casetup.Current;

			RowUpdated.AddHandler<CADeposit>(ParentFieldUpdated);
			this.AvailablePayments.Cache.AllowDelete = false;
			this.AvailablePayments.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(this.AvailablePayments.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARPayment.selected>(this.AvailablePayments.Cache, null, true);

			this.FieldSelecting.AddHandler<CADeposit.tranID_CATran_batchNbr>(CADeposit_TranID_CATran_BatchNbr_FieldSelecting);
		}

		public PXSelect<ARRegister> dummy_for_correct_navigation; //Must be first - otherwise CashSales are  not redirected correctly
		public PXSelect<CADeposit, Where<CADeposit.tranType, Equal<Optional<CADeposit.tranType>>>> Document;

		public PXSelect<CADeposit, Where<CADeposit.tranType, Equal<Current<CADeposit.tranType>>,
								And<CADeposit.refNbr, Equal<Current<CADeposit.refNbr>>>>> DocumentCurrent;
		public PXSelect<CADepositDetail, Where<CADepositDetail.refNbr, Equal<Current<CADeposit.refNbr>>,
									 And<CADepositDetail.tranType, Equal<Current<CADeposit.tranType>>>>> Details;

		public PXSelectJoin<CADepositDetail, LeftJoin<PaymentInfo, On<CADepositDetail.origDocType, Equal<PaymentInfo.docType>,
							And<CADepositDetail.origRefNbr, Equal<PaymentInfo.refNbr>>>>,
							Where<CADepositDetail.refNbr, Equal<Current<CADeposit.refNbr>>,
									 And<CADepositDetail.tranType, Equal<Current<CADeposit.tranType>>>>> DepositPayments;

		public PXFilter<PaymentFilter> filter;

		public PXSelectJoin<PaymentInfo,
							LeftJoin<CADepositDetail, On<CADepositDetail.origDocType, Equal<PaymentInfo.docType>,
											And<CADepositDetail.origRefNbr, Equal<PaymentInfo.refNbr>,
											And<CADepositDetail.origModule, Equal<PaymentInfo.module>>>>,
							InnerJoin<CashAccountDeposit, On<CashAccountDeposit.depositAcctID, Equal<PaymentInfo.cashAccountID>,
								And<Where<CashAccountDeposit.paymentMethodID, Equal<PaymentInfo.paymentMethodID>,
									Or<CashAccountDeposit.paymentMethodID, Equal<BQLConstants.EmptyString>>>>>>>,
								Where<CashAccountDeposit.accountID, Equal<Current<CADeposit.cashAccountID>>,
								And<PaymentInfo.depositNbr, IsNull,
								And<PaymentInfo.depositAfter, LessEqual<Current<CADeposit.tranDate>>,
								And<CADepositDetail.refNbr, IsNull,
								And<Where<PaymentInfo.paymentMethodID, Equal<Current<PaymentFilter.paymentMethodID>>,
									Or<Current<PaymentFilter.paymentMethodID>, IsNull>>>>>>>> AvailablePayments;

		public PXSelect<CATran> dummy_caTran;
		public PXSelect<ARPaymentUpdate, Where<ARPaymentUpdate.depositNbr, Equal<Current<CADeposit.refNbr>>,
										And<Current<CADeposit.tranType>, Equal<CATranType.cADeposit>>>> paymentUpdate;

		public PXSelect<APPaymentUpdate, Where<APPaymentUpdate.depositNbr, Equal<Current<CADeposit.refNbr>>,
										And<Current<CADeposit.tranType>, Equal<CATranType.cADeposit>>>> paymentUpdateAP;

		public PXSelect<CADepositCharge, Where<CADepositCharge.tranType, Equal<Current<CADeposit.tranType>>,
							And<CADepositCharge.refNbr, Equal<Current<CADeposit.refNbr>>>>> Charges;
		public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Optional<CADeposit.cashAccountID>>>> cashAccount;
		public PXSelect<CurrencyInfo> currencyinfo;
		public ToggleCurrency<CADeposit> CurrencyView;

		public PXSetup<CASetup> casetup;
		public PXSelect<CATran> catran;

		#endregion

		#region Select Delegate

		public virtual IEnumerable availablePayments()
		{
			PaymentFilter flt = this.filter.Current;
			CADeposit doc = this.Document.Current;
			if (doc == null || doc.CashAccountID == null || doc.Released == true)
				yield break;
			if (!filter.Current.StartDate.HasValue || !filter.Current.EndDate.HasValue)
				yield break;
			PXCache cache = this.Caches[typeof(PaymentInfo)];
			{
				PXSelectBase<Light.ARPayment> paymentSelect = new PXSelectJoin<Light.ARPayment,
									LeftJoin<CADepositDetail, On<CADepositDetail.origDocType, Equal<Light.ARPayment.docType>,
										And<CADepositDetail.origRefNbr, Equal<Light.ARPayment.refNbr>,
										And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAR>,
										And<CADepositDetail.tranType, Equal<CATranType.cADeposit>>>>>,
									LeftJoin<CADeposit, On<CADeposit.tranType, Equal<CADepositDetail.tranType>,
										And<CADeposit.refNbr, Equal<CADepositDetail.refNbr>>>,
									InnerJoin<CashAccountDeposit, On<CashAccountDeposit.depositAcctID, Equal<Light.ARPayment.cashAccountID>,
									And<Where<CashAccountDeposit.paymentMethodID, Equal<Light.ARPayment.paymentMethodID>,
										Or<CashAccountDeposit.paymentMethodID, Equal<BQLConstants.EmptyString>>>>>,
									InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<Light.ARPayment.paymentMethodID>>>>>>,
									Where<CashAccountDeposit.accountID, Equal<Current<CADeposit.cashAccountID>>,						
									And<Light.ARPayment.released, Equal<boolTrue>,
									And<Light.ARPayment.depositAsBatch, Equal<boolTrue>,
									And<Light.ARPayment.depositNbr, IsNull,
									And<Where<CADepositDetail.refNbr, IsNull, Or<CADeposit.voided, Equal<boolTrue>>>>>>>>,
									OrderBy<Asc<ARPayment.docType, Asc<Light.ARPayment.refNbr, Desc<CashAccountDeposit.paymentMethodID>>>>>(this);
				if (flt.CashAccountID.HasValue)
					paymentSelect.WhereAnd<Where<Light.ARPayment.cashAccountID, Equal<Current<PaymentFilter.cashAccountID>>>>();
				if (!String.IsNullOrEmpty(flt.PaymentMethodID))
					paymentSelect.WhereAnd<Where<Light.ARPayment.paymentMethodID, Equal<Current<PaymentFilter.paymentMethodID>>>>();

				if (flt.EndDate.HasValue)
					paymentSelect.WhereAnd<Where<Light.ARPayment.depositAfter, LessEqual<Current<PaymentFilter.endDate>>>>();
				if (flt.StartDate.HasValue)
					paymentSelect.WhereAnd<Where<Light.ARPayment.depositAfter, GreaterEqual<Current<PaymentFilter.startDate>>>>();


				Light.ARPayment last = null;
				foreach (PXResult<Light.ARPayment, CADepositDetail, CADeposit, CashAccountDeposit, PaymentMethod> it in paymentSelect.Select())
				{
					Light.ARPayment payment = it;
					CADepositDetail detail = it;
					PaymentMethod pmDef = it;
					if (pmDef.ARVoidOnDepositAccount == false && (payment.Voided == true || ARPaymentType.AllVoidingTypes.Contains(payment.DocType))) continue; //Skip voided and voiding Documents
					bool exist = false;
					if (last != null && last.DocType == payment.DocType && last.RefNbr == payment.RefNbr) continue; //Skip duplicates 
					last = payment;

					//Add filter for CashAccountDeposit.paymentMethodID according to priorities
					foreach (CADepositDetail iDet in this.Details.Select())
					{
						if (iDet.OrigDocType == payment.DocType && iDet.OrigRefNbr == payment.RefNbr
							&& iDet.OrigModule == GL.BatchModule.AR)
						{
							exist = true;
							break;
						}
					}
					if (exist) continue;
					PaymentInfo paymentInfo = Copy(payment, new PaymentInfo());
					try
					{
						cache.Insert(paymentInfo);
					}
					catch (PXException ex)
					{
						cache.SetStatus(paymentInfo, PXEntryStatus.Inserted);
						cache.RaiseExceptionHandling<PaymentInfo.refNbr>(paymentInfo, paymentInfo.Selected, new PXSetPropertyException(ex.Message, PXErrorLevel.RowWarning));
					}
					yield return paymentInfo;
				}
			}

			{
				PXSelectBase<Light.APPayment> apPaymentSelect = new PXSelectJoin<Light.APPayment,
									LeftJoin<CADepositDetail, On<CADepositDetail.origDocType, Equal<Light.APPayment.docType>,
										And<CADepositDetail.origRefNbr, Equal<Light.APPayment.refNbr>,
										And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAP>,
										And<CADepositDetail.tranType, Equal<CATranType.cADeposit>>>>>,
									LeftJoin<CADeposit, On<CADeposit.tranType, Equal<CADepositDetail.tranType>,
										And<CADeposit.refNbr, Equal<CADepositDetail.refNbr>>>,
									InnerJoin<CashAccountDeposit, On<CashAccountDeposit.depositAcctID, Equal<Light.APPayment.cashAccountID>,
									And<Where<CashAccountDeposit.paymentMethodID, Equal<Light.APPayment.paymentMethodID>,
										Or<CashAccountDeposit.paymentMethodID, Equal<BQLConstants.EmptyString>>>>>,
									InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<Light.APPayment.paymentMethodID>>>>>>,
									Where<CashAccountDeposit.accountID, Equal<Current<CADeposit.cashAccountID>>,
									And<Light.APPayment.docType, In3<APDocType.refund, APDocType.voidRefund>,
									And<Light.APPayment.released, Equal<boolTrue>,
									And<Light.APPayment.docType, In3<APDocType.refund, APDocType.voidRefund>,
									And<Light.APPayment.depositAsBatch, Equal<boolTrue>,
									And<Light.APPayment.depositNbr, IsNull,
									And<Where<CADepositDetail.refNbr, IsNull, Or<CADeposit.voided, Equal<boolTrue>>>>>>>>>>,
									OrderBy<Asc<Light.APPayment.docType, Asc<Light.APPayment.refNbr, Desc<CashAccountDeposit.paymentMethodID>>>>>(this);
				if (flt.CashAccountID.HasValue)
					apPaymentSelect.WhereAnd<Where<Light.APPayment.cashAccountID, Equal<Current<PaymentFilter.cashAccountID>>>>();
				if (!String.IsNullOrEmpty(flt.PaymentMethodID))
					apPaymentSelect.WhereAnd<Where<Light.APPayment.paymentMethodID, Equal<Current<PaymentFilter.paymentMethodID>>>>();
				if (flt.EndDate.HasValue)
					apPaymentSelect.WhereAnd<Where<Light.APPayment.depositAfter, LessEqual<Current<PaymentFilter.endDate>>>>();
				if (flt.StartDate.HasValue)
					apPaymentSelect.WhereAnd<Where<Light.APPayment.depositAfter, GreaterEqual<Current<PaymentFilter.startDate>>>>();

				Light.APPayment last = null;
				foreach (PXResult<Light.APPayment, CADepositDetail, CADeposit, CashAccountDeposit, PaymentMethod> it in apPaymentSelect.Select())
				{
					Light.APPayment payment = it;
					CADepositDetail detail = it;
					PaymentMethod pmDef = it;
					if (payment.DocType != APPaymentType.Refund && payment.DocType != APPaymentType.VoidRefund) continue;
				
					bool exist = false;
					if (last != null && last.DocType == payment.DocType && last.RefNbr == payment.RefNbr) continue; //Skip duplicates 
					last = payment;

					//Add filter for CashAccountDeposit.paymentMethodID according to priorities
					foreach (CADepositDetail iDet in this.Details.Select())
					{
						if (iDet.OrigDocType == payment.DocType && iDet.OrigRefNbr == payment.RefNbr
							&& iDet.OrigModule == GL.BatchModule.AP)
						{
							exist = true;
							break;
						}
					}
					if (exist) continue;
					PaymentInfo paymentInfo = Copy(payment, new PaymentInfo());
					try
					{
						cache.Insert(paymentInfo);
					}
					catch (PXException ex)
					{
						cache.SetStatus(paymentInfo, PXEntryStatus.Inserted);
						cache.RaiseExceptionHandling<PaymentInfo.refNbr>(paymentInfo, paymentInfo.Selected, new PXSetPropertyException(ex.Message, PXErrorLevel.RowWarning));
					}
					yield return paymentInfo;
				}
			}
		}

		public virtual IEnumerable depositPayments()
		{
			foreach (PXResult<CADepositDetail, Light.ARPayment> it in PXSelectJoin<CADepositDetail,
							LeftJoin<Light.ARPayment, On<CADepositDetail.origDocType, Equal<Light.ARPayment.docType>,
								And<CADepositDetail.origRefNbr, Equal<Light.ARPayment.refNbr>,
								And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAR>>>>>,
							Where<CADepositDetail.refNbr, Equal<Current<CADeposit.refNbr>>,
									And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAR>,
									And<CADepositDetail.tranType, Equal<Current<CADeposit.tranType>>>>>>.Select(this))
			{
				CADepositDetail detail = it;
				Light.ARPayment paymentAR = it;
				PaymentInfo pmInfo = new PaymentInfo();
				if (string.IsNullOrEmpty(paymentAR.RefNbr) == false)
				{
					pmInfo = Copy(paymentAR, pmInfo);
				}
				PXResult<CADepositDetail, PaymentInfo> result = new PXResult<CADepositDetail, PaymentInfo>(detail, pmInfo);
				yield return result;
			}

			foreach (PXResult<CADepositDetail, Light.APPayment> it in PXSelectJoin<CADepositDetail,
							LeftJoin<Light.APPayment, On<CADepositDetail.origDocType, Equal<Light.APPayment.docType>,
								And<CADepositDetail.origRefNbr, Equal<Light.APPayment.refNbr>,
								And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAP>>>>>,
							Where<CADepositDetail.refNbr, Equal<Current<CADeposit.refNbr>>,
								And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAP>,
								 And<CADepositDetail.tranType, Equal<Current<CADeposit.tranType>>>>>>.Select(this))
			{
				CADepositDetail detail = it;
				Light.APPayment paymentAP = it;
				PaymentInfo pmInfo = new PaymentInfo();
				if (string.IsNullOrEmpty(paymentAP.RefNbr) == false)
				{
					pmInfo = Copy(paymentAP, pmInfo);
				}
				PXResult<CADepositDetail, PaymentInfo> result = new PXResult<CADepositDetail, PaymentInfo>(detail, pmInfo);
				yield return result;
			}
		}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			//Disable Currency Rate edit form for VoidDeposits
			if (viewName == "_CADeposit_CurrencyInfo_")
			{
				if (this.Views[viewName].IsReadOnly == false)
				{
					CADeposit doc = this.Document.Current;
					if (doc != null && doc.TranType == CATranType.CAVoidDeposit)
					{
						this.Views[viewName].IsReadOnly = true;
					}
				}
			}
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}
		#endregion

		#region Events

		#region CATran Envents
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleCA>>>))]
		public virtual void CATran_BatchNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region CADeposit Events
		protected virtual void CADeposit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CADeposit row = e.Row as CADeposit;
			if (row == null) return;
			bool isVoiding = (row.DocType == CATranType.CAVoidDeposit);

			bool released = (row.Released == true);
			if (this._IsVoidCheckInProgress == false)
			{
				this.Details.Cache.AllowInsert = false;
				this.Details.Cache.AllowUpdate = false;
				this.Details.Cache.AllowDelete = !released && !isVoiding;

				this.Charges.Cache.AllowInsert = !released && !isVoiding;
				this.Charges.Cache.AllowUpdate = !released && !isVoiding;
				this.Charges.Cache.AllowDelete = !released && !isVoiding;

				this.currencyinfo.Cache.AllowDelete = !released && !isVoiding;
				this.currencyinfo.Cache.AllowInsert = !released && !isVoiding;
				this.currencyinfo.Cache.AllowUpdate = !(released || isVoiding);
			}

			PXUIFieldAttribute.SetEnabled(this.currencyinfo.Cache, null, !released && !isVoiding);
			this.CurrencyView.SetEnabled(!released && !isVoiding);
			PXUIFieldAttribute.SetEnabled(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CADeposit.refNbr>(sender, row, true);
			PXUIFieldAttribute.SetEnabled<CADeposit.tranType>(sender, row, true);

			PXUIFieldAttribute.SetRequired<CADeposit.extRefNbr>(sender, casetup.Current.RequireExtRefNbr == true);

			sender.AllowUpdate = !released;
			sender.AllowDelete = !released;

			CashAccount cashaccount = (CashAccount)PXSelectorAttribute.Select<CADeposit.cashAccountID>(sender, row);
			bool requireControlTotal = (bool)casetup.Current.RequireControlTotal;
			bool clearEnabled = (row.Released != true) && (cashaccount != null) && (cashaccount.Reconcile == true);
			bool allowExtraCash = (row.ExtraCashAccountID != null);
			if (!released)
			{
				if (!isVoiding)
				{
					PXUIFieldAttribute.SetEnabled<CADeposit.hold>(sender, row, !released);
					bool hasDetails = this.Details.Any();
					PXUIFieldAttribute.SetEnabled<CADeposit.cashAccountID>(sender, row, !(row.CashAccountID.HasValue && hasDetails));
					PXUIFieldAttribute.SetEnabled<CADeposit.extRefNbr>(sender, row);
					PXUIFieldAttribute.SetEnabled<CADeposit.tranDate>(sender, row, !hasDetails);
					PXUIFieldAttribute.SetEnabled<CADeposit.finPeriodID>(sender, row);
					PXUIFieldAttribute.SetEnabled<CADeposit.tranDesc>(sender, row);
					PXUIFieldAttribute.SetEnabled<CADeposit.curyControlAmt>(sender, row, requireControlTotal);
					PXUIFieldAttribute.SetEnabled<CADeposit.cleared>(sender, row, clearEnabled);
					PXUIFieldAttribute.SetEnabled<CADeposit.clearDate>(sender, row, clearEnabled && (row.Cleared == true));
					PXUIFieldAttribute.SetEnabled<CADeposit.chargesSeparate>(sender, row);
					PXUIFieldAttribute.SetEnabled<CADeposit.extraCashAccountID>(sender, row);
					PXUIFieldAttribute.SetEnabled<CADeposit.curyExtraCashTotal>(sender, row, allowExtraCash);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<CADeposit.hold>(sender, row, !released);
					PXUIFieldAttribute.SetEnabled<CADeposit.tranDesc>(sender, row, !released);
					PXUIFieldAttribute.SetEnabled<CADeposit.curyControlAmt>(sender, row, !released);
				}

			}
			this.Release.SetEnabled((row.TranType == CATranType.CADeposit || row.TranType == CATranType.CAVoidDeposit) && !released && (row.Hold == false));

			this.VoidDocument.SetEnabled((row.TranType == CATranType.CADeposit) && released && row.Voided == false);
			this.addPayment.SetEnabled((row.TranType == CATranType.CADeposit) && !released);

			PXUIFieldAttribute.SetVisible<CADeposit.curyControlAmt>(sender, null, requireControlTotal);
			PXUIFieldAttribute.SetRequired<CADeposit.curyControlAmt>(sender, requireControlTotal);
		}

		protected virtual void CADeposit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (CADeposit)e.Row;

			PXDefaultAttribute.SetPersistingCheck<CADeposit.extRefNbr>(sender, row, casetup.Current.RequireExtRefNbr == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		protected virtual void CADeposit_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			CADeposit row = e.Row as CADeposit;
			if (row != null && this.filter.Current != null)
			{
				this.filter.Cache.SetValue<PaymentFilter.endDate>(this.filter.Current.EndDate, row.TranDate);
			}
		}

		protected virtual void CADeposit_TranDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CADeposit row = e.Row as CADeposit;
			if (row == null) return;

			CurrencyInfoAttribute.SetEffectiveDate<CADeposit.tranDate>(sender, e);

			filter.Cache.SetDefaultExt<PaymentFilter.startDate>(filter.Current);
			filter.Cache.SetDefaultExt<PaymentFilter.endDate>(filter.Current);
		}

		protected virtual void CADeposit_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CADeposit row = (CADeposit)e.Row;
			row.Cleared = false;
			row.ClearDate = null;
			if (cashAccount.Current == null || cashAccount.Current.CashAccountID != row.CashAccountID)
			{
				cashAccount.Current = (CashAccount)PXSelectorAttribute.Select<CADeposit.cashAccountID>(sender, row);
				this.AvailablePayments.Cache.Clear();
			}

			if (cashAccount.Current != null && cashAccount.Current.Reconcile != true)
			{
				row.Cleared = true;
				row.ClearDate = row.TranDate;
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.ExternalCall || sender.GetValuePending<CADeposit.curyID>(row) == null)
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<CADeposit.curyInfoID>(sender, row);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<CADeposit.tranDate>(row, row.TranDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}
					if (info != null)
					{
						row.CuryID = info.CuryID;
					}
				}
			}
		}

		protected virtual void CADeposit_ExtraCashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CADeposit row = (CADeposit)e.Row;
			if (row.ExtraCashAccountID == null)
			{
				sender.SetDefaultExt<CADeposit.curyExtraCashTotal>(row);
			}
		}

		protected virtual void CADeposit_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CADeposit row = (CADeposit)e.Row;
			if (row.Released == false)
			{
				if (!this.casetup.Current.RequireControlTotal == true)
				{
					if (row.CuryControlAmt != row.CuryTranAmt)
					{
						if (row.CuryTranAmt != null && row.CuryTranAmt != 0)
							sender.SetValueExt<CADeposit.curyControlAmt>(e.Row, row.CuryTranAmt);
						else
							sender.SetValueExt<CADeposit.curyControlAmt>(e.Row, 0m);
					}
				}

				if ((bool)row.Hold == false)
				{
					if (row.CuryControlAmt != row.CuryTranAmt)
					{
						sender.RaiseExceptionHandling<CADeposit.curyControlAmt>(e.Row, row.CuryControlAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<CADeposit.curyControlAmt>(e.Row, row.CuryControlAmt, null);
					}
				}
			}
		}

		protected virtual void CADeposit_ChargesSeparate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CADeposit row = (CADeposit)e.Row;
			sender.RaiseFieldUpdated<CADeposit.chargeMult>(e.Row, Decimal.MinusOne);
		}

		protected virtual void CADeposit_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void CADeposit_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void CADeposit_TranID_CATran_BatchNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null || e.IsAltered)
			{
				string ViewName = null;
				PXCache cache = sender.Graph.Caches[typeof(CATran)];
				PXFieldState state = cache.GetStateExt<CATran.batchNbr>(null) as PXFieldState;
				if (state != null)
				{
					ViewName = state.ViewName;
				}

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, false, false, 0, 0, null, null, null, null, null, null, PXErrorLevel.Undefined, false, true, true, PXUIVisibility.Visible, ViewName, null, null);
			}
		}
		#endregion

		#region CADeposit Detail events

		private bool _isMassDelete = false;
		private bool _IsVoidCheckInProgress = false;

		protected virtual void CADepositDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			CADepositDetail row = (CADepositDetail)e.Row;
			if (row.DetailType == CADepositDetailType.CheckDeposit && row.OrigModule == GL.BatchModule.AR)
			{
				if (!String.IsNullOrEmpty(row.OrigDocType) && !String.IsNullOrEmpty(row.OrigRefNbr))
				{
					ARPaymentUpdate pmt = PXSelect<ARPaymentUpdate, Where<ARPaymentUpdate.docType, Equal<Required<ARPaymentUpdate.docType>>,
										And<ARPaymentUpdate.refNbr, Equal<Required<ARPaymentUpdate.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
					if (pmt != null)
					{
						pmt.ClearDepositReference();
						this.paymentUpdate.Update(pmt);
					}
				}
			}

			if (row.DetailType == CADepositDetailType.CheckDeposit && row.OrigModule == GL.BatchModule.AP)
			{
				if (!String.IsNullOrEmpty(row.OrigDocType) && !String.IsNullOrEmpty(row.OrigRefNbr))
				{
					APPaymentUpdate pmt = PXSelect<APPaymentUpdate, Where<APPaymentUpdate.docType, Equal<Required<APPaymentUpdate.docType>>,
										And<APPaymentUpdate.refNbr, Equal<Required<APPaymentUpdate.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
					if (pmt != null)
					{
						pmt.ClearDepositReference();
						this.paymentUpdateAP.Update(pmt);
					}
				}
			}
			if (this._IsVoidCheckInProgress == false)
			{
				if ((!this._isMassDelete) && (!String.IsNullOrEmpty(row.ChargeEntryTypeID)))
				{
					CADepositCharge charge = null;
					foreach (CADepositCharge it in this.Charges.Select())
					{
						if (row.ChargeEntryTypeID == it.EntryTypeID && row.AccountID == it.DepositAcctID
									&& row.PaymentMethodID == it.PaymentMethodID)
						{
							charge = it;
							break;
						}
					}

					if (charge != null)
					{
						CADepositCharge copy = (CADepositCharge)this.Charges.Cache.CreateCopy(charge);
						copy.CuryChargeableAmt -= row.CuryTranAmt;
						if (copy.CuryChargeableAmt == Decimal.Zero)
						{
							this.Charges.Delete(charge);
						}
						else
						{
							charge = this.Charges.Update(copy);
						}
					}
				}
			}
		}

		protected virtual void CADepositDetail_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			CADepositDetail row = (CADepositDetail)e.Row;
			if (row.DetailType == CADepositDetailType.CheckDeposit)
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
				{
					if (!String.IsNullOrEmpty(row.OrigDocType) && !String.IsNullOrEmpty(row.OrigRefNbr))
					{
						if (row.OrigModule == GL.BatchModule.AR)
						{
							ARPaymentUpdate pmt = PXSelect<ARPaymentUpdate, Where<ARPaymentUpdate.docType, Equal<Required<ARPaymentUpdate.docType>>,
														And<ARPaymentUpdate.refNbr, Equal<Required<ARPaymentUpdate.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);

							CADeposit deposit = this.Document.Current;
							if (pmt != null)
							{
								pmt.SetReferenceTo(deposit);
								pmt = this.paymentUpdate.Update(pmt);
							}

						}
						if (row.OrigModule == GL.BatchModule.AP)
						{
							APPaymentUpdate pmt = PXSelect<APPaymentUpdate, Where<APPaymentUpdate.docType, Equal<Required<APPaymentUpdate.docType>>,
															And<APPaymentUpdate.refNbr, Equal<Required<APPaymentUpdate.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
							CADeposit deposit = this.Document.Current;
							if (pmt != null)
							{
								pmt.SetReferenceTo(deposit);
								pmt = this.paymentUpdateAP.Update(pmt);
							}
						}
					}
				}
			}
		}

		protected virtual void CADepositDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			CADepositDetail row = (CADepositDetail)e.Row;
			if (this._IsVoidCheckInProgress == false)
			{
				if (!String.IsNullOrEmpty(row.ChargeEntryTypeID))
				{
					CADepositCharge charge = null;
					foreach (CADepositCharge it in this.Charges.Select())
					{
						if (row.ChargeEntryTypeID == it.EntryTypeID && row.AccountID == it.DepositAcctID
									&& row.PaymentMethodID == it.PaymentMethodID)
						{
							charge = it;
							break;
						}
					}
					if (charge == null)
					{
						charge = new CADepositCharge();

						charge.EntryTypeID = row.ChargeEntryTypeID;
						charge.DepositAcctID = row.AccountID;
						charge.PaymentMethodID = row.PaymentMethodID;
						charge.CuryChargeableAmt = row.CuryTranAmt;
						charge = this.Charges.Insert(charge);
					}
					else
					{
						CADepositCharge copy = (CADepositCharge)this.Charges.Cache.CreateCopy(charge);
						copy.CuryChargeableAmt += row.CuryTranAmt;
						charge = this.Charges.Update(copy);
					}
				}
			}
		}

		protected virtual void CADepositDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CADepositDetail row = (CADepositDetail)e.Row;
			CADepositDetail oldRow = (CADepositDetail)e.OldRow;
			if (this._IsVoidCheckInProgress == false)
			{
				if (row.ChargeEntryTypeID != oldRow.ChargeEntryTypeID)
				{
					CADepositCharge charge = null;
					CADepositCharge oldCharge = null;
					foreach (CADepositCharge it in this.Charges.Select())
					{
						if (row.ChargeEntryTypeID == it.EntryTypeID && row.AccountID == it.DepositAcctID
									&& row.PaymentMethodID == it.PaymentMethodID)
						{
							charge = it;
						}
						if (oldRow.ChargeEntryTypeID == it.EntryTypeID
							&& oldRow.AccountID == it.DepositAcctID
									&& oldRow.PaymentMethodID == it.PaymentMethodID)
						{
							oldCharge = it;
						}
					}
					if (charge == null)
					{
						charge = new CADepositCharge();
						charge.EntryTypeID = row.ChargeEntryTypeID;
						charge.DepositAcctID = row.AccountID;
						charge.PaymentMethodID = row.PaymentMethodID;
						charge.CuryChargeableAmt = row.CuryTranAmt;
						charge = this.Charges.Insert(charge);
					}
					else
					{
						charge.CuryChargeableAmt += row.CuryTranAmt;
						charge = this.Charges.Update(charge);
					}

					if (oldCharge != null)
					{
						oldCharge.CuryChargeableAmt -= row.CuryTranAmt;
						if (oldCharge.CuryChargeableAmt == Decimal.Zero)
						{
							this.Charges.Delete(oldCharge);
						}
						else
						{
							charge = this.Charges.Update(charge);
						}
					}
				}
			}
		}

		protected virtual void CADepositDetail_PaymentMethodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CADepositCharge_ChargeRate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CADepositCharge row = (CADepositCharge)e.Row;
			CADeposit doc = this.Document.Current;
			if (doc != null && doc.CashAccountID != null)
			{
				if (row.DepositAcctID != null)
				{
					CashAccountDeposit setting = PXSelect<CashAccountDeposit, Where<CashAccountDeposit.accountID, Equal<Required<CashAccountDeposit.accountID>>,
														   And<CashAccountDeposit.depositAcctID, Equal<Required<CashAccountDeposit.depositAcctID>>,
														   And<CashAccountDeposit.paymentMethodID, Equal<Required<CashAccountDeposit.paymentMethodID>>>>>>.Select(this, doc.CashAccountID, row.DepositAcctID, row.PaymentMethodID);
					if (setting != null)
					{
						e.NewValue = setting.ChargeRate;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void CADepositCharge_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CADepositCharge item = e.Row as CADepositCharge;

			if(item == null)
			{
				return;
			}

			string errorPaymentMethodID = PXUIFieldAttribute.GetError<CADepositCharge.paymentMethodID>(sender, item);
			string errorEntryTypeID = PXUIFieldAttribute.GetError<CADepositCharge.entryTypeID>(sender, item);

			if (!string.IsNullOrEmpty(errorPaymentMethodID))
			{
				sender.RaiseExceptionHandling<CADepositCharge.paymentMethodID>(item, item.PaymentMethodID, new PXSetPropertyException(errorPaymentMethodID, PXErrorLevel.Error));
			}

			if (!string.IsNullOrEmpty(errorEntryTypeID))
			{
				sender.RaiseExceptionHandling<CADepositCharge.entryTypeID>(item, item.EntryTypeID, new PXSetPropertyException(errorEntryTypeID, PXErrorLevel.Error));
			}
		}

		#endregion

		#region PaymentInfo events
		protected virtual void PaymentInfo_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PaymentInfo_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PaymentInfo_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PaymentInfo_PMInstanceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PaymentInfo_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PaymentInfo_CashSubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void PaymentInfo_PaymentMethodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PaymentInfo_CuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		#region CurrencyInfoEvents

		protected virtual void CurrencyInfo_ModuleCode_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BatchModule.GL;
			e.Cancel = true;
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CurrencyInfo currencyInfo = (CurrencyInfo)e.Row;
			if (currencyInfo == null || this.Document.Current == null || this.Document.Current.TranDate == null) return;
			e.NewValue = this.Document.Current.TranDate;
			e.Cancel = true;
		}

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (cashAccount.Current != null && !string.IsNullOrEmpty(cashAccount.Current.CuryID))
				{
					e.NewValue = cashAccount.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (cashAccount.Current != null && !string.IsNullOrEmpty(cashAccount.Current.CuryRateTypeID))
				{
					e.NewValue = cashAccount.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		#endregion
		#region PaymentFilterEvents
		protected virtual void PaymentFilter_StartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null && Document.Current.TranDate.HasValue)
				e.NewValue = Document.Current.TranDate.Value.AddDays(-7);
		}
		#endregion

		#endregion

		#region Methods

		public virtual PaymentInfo Copy(Light.ARPayment source, PaymentInfo destination)
		{
			destination.Module = GL.BatchModule.AR;
			destination.DocType = source.DocType;
			destination.RefNbr = source.RefNbr;
			destination.BAccountID = source.CustomerID;
			destination.LocationID = source.CustomerLocationID;
			destination.ExtRefNbr = source.ExtRefNbr;
			destination.Status = source.Status;
			destination.PaymentMethodID = source.PaymentMethodID;
			destination.CuryID = source.CuryID;
			destination.CuryInfoID = source.CuryInfoID;
			destination.CuryGrossPaymentAmount = source.CuryOrigDocAmt;
			destination.GrossPaymentAmount = source.OrigDocAmt;
			destination.CuryOrigDocAmt = source.CuryOrigDocAmt - source.CuryChargeAmt;
			destination.OrigDocAmt = source.OrigDocAmt - source.ChargeAmt;
			destination.CuryChargeTotal = source.CuryChargeAmt;
			destination.ChargeTotal = source.ChargeAmt;
			destination.DocDate = source.DocDate;
			destination.DepositAfter = source.DepositAfter;
			destination.CashAccountID = source.CashAccountID;
			destination.PMInstanceID = source.PMInstanceID;
			return destination;
		}
		public virtual PaymentInfo Copy(Light.APPayment source, PaymentInfo destination)
		{
			destination.Module = GL.BatchModule.AP;
			destination.DocType = source.DocType;
			destination.RefNbr = source.RefNbr;
			destination.BAccountID = source.VendorID;
			destination.LocationID = source.VendorLocationID;
			destination.ExtRefNbr = source.ExtRefNbr;
			destination.Status = source.Status;
			destination.PaymentMethodID = source.PaymentMethodID;
			destination.CuryID = source.CuryID;
			destination.CuryInfoID = source.CuryInfoID;
			destination.CuryOrigDocAmt = source.CuryOrigDocAmt;
			destination.OrigDocAmt = source.OrigDocAmt;
			destination.DocDate = source.DocDate;
			destination.DepositAfter = source.DepositAfter;
			destination.CashAccountID = source.CashAccountID;
			destination.PMInstanceID = null;
			return destination;
		}
		public virtual void AddPaymentInfoBatch(IEnumerable<PaymentInfo> payments)
		{
			var existingDetails = DepositPayments.Select();
			HashSet<string> existingDetailsHash = new HashSet<string>();
			foreach(CADepositDetail detail in existingDetails)
			{
				existingDetailsHash.Add(detail.OrigModule + detail.OrigDocType + detail.OrigRefNbr);
			}
			foreach (PaymentInfo payment in payments)
			{
				CashAccountDeposit settings = PXSelect<CashAccountDeposit, Where<CashAccountDeposit.accountID, Equal<Current<CADeposit.cashAccountID>>,
						And<CashAccountDeposit.depositAcctID, Equal<Required<CashAccountDeposit.depositAcctID>>,
							And<Where<CashAccountDeposit.paymentMethodID, Equal<BQLConstants.EmptyString>,
								Or<CashAccountDeposit.paymentMethodID, Equal<Required<CashAccountDeposit.paymentMethodID>>>>>>>,
								OrderBy<Desc<CashAccountDeposit.paymentMethodID>>>.Select(this, payment.CashAccountID, payment.PaymentMethodID);
				if (settings == null)
				{
					continue;
				}
				if (!existingDetailsHash.Contains(payment.Module + payment.DocType + payment.RefNbr))
				{
					AddPaymentInfo(payment, settings, true);
				}
			}
		}
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public virtual CADepositDetail AddPaymentInfo(PaymentInfo aPayment, bool skipCheck)
		{
			CashAccountDeposit settings = PXSelect<CashAccountDeposit, Where<CashAccountDeposit.accountID, Equal<Current<CADeposit.cashAccountID>>,
							And<CashAccountDeposit.depositAcctID, Equal<Required<CashAccountDeposit.depositAcctID>>,
								And<Where<CashAccountDeposit.paymentMethodID, Equal<BQLConstants.EmptyString>,
									Or<CashAccountDeposit.paymentMethodID, Equal<Required<CashAccountDeposit.paymentMethodID>>>>>>>, OrderBy<Desc<CashAccountDeposit.paymentMethodID>>>.Select(this, aPayment.CashAccountID, aPayment.PaymentMethodID);
			if (settings == null) return null;
			return AddPaymentInfo(aPayment, settings, skipCheck);
		}

		public virtual CADepositDetail AddPaymentInfo(PaymentInfo aPayment, CashAccountDeposit settings, bool skipCheck)
		{
			if (!skipCheck)
			{
				foreach (CADepositDetail iDel in this.DepositPayments.Select())
				{
					if (iDel.OrigDocType == aPayment.DocType && iDel.OrigRefNbr == aPayment.RefNbr)
					{
						return iDel;
					}
				}
			}

			CADepositDetail detail = new CADepositDetail();
			Copy(detail, aPayment);
			Copy(detail, settings);
			detail = this.Details.Insert(detail);
			return detail;
		}

		protected static void Copy(CADepositDetail aDest, ARPayment aPayment)
		{
			aDest.OrigModule = GL.BatchModule.AR;
			aDest.OrigDocType = aPayment.DocType;
			aDest.OrigRefNbr = aPayment.RefNbr;
			aDest.OrigCuryID = aPayment.CuryID;
			aDest.OrigCuryInfoID = aPayment.CuryInfoID;
			aDest.CuryOrigAmt = aPayment.CuryOrigDocAmt;
			aDest.OrigAmt = aPayment.OrigDocAmt;
			aDest.AccountID = aPayment.CashAccountID;
			aDest.CuryTranAmt = aDest.CuryOrigAmtSigned; //Check Later - for the case when currencies are different
			aDest.TranAmt = aDest.OrigAmtSigned;
		}
		protected void Copy(CADepositDetail aDest, APPayment aPayment)
		{
			aDest.OrigModule = GL.BatchModule.AP;
			aDest.OrigDocType = aPayment.DocType;
			aDest.OrigRefNbr = aPayment.RefNbr;
			aDest.OrigCuryID = aPayment.CuryID;
			aDest.OrigCuryInfoID = aPayment.CuryInfoID;
			aDest.CuryOrigAmt = aPayment.CuryOrigDocAmt;
			aDest.OrigAmt = aPayment.OrigDocAmt;
			aDest.AccountID = aPayment.CashAccountID;
			aDest.CuryTranAmt = aDest.CuryOrigAmtSigned; //Check Later - for the case when currencies are different
			aDest.TranAmt = aDest.OrigAmtSigned;
		}

		protected void Copy(CADepositDetail aDest, PaymentInfo aPayment)
		{
			aDest.OrigModule = aPayment.Module;
			aDest.OrigDocType = aPayment.DocType;
			aDest.OrigRefNbr = aPayment.RefNbr;
			aDest.OrigCuryID = aPayment.CuryID;
			aDest.OrigCuryInfoID = aPayment.CuryInfoID;
			aDest.CuryOrigAmt = aPayment.CuryOrigDocAmt;
			aDest.OrigAmt = aPayment.OrigDocAmt;
			aDest.AccountID = aPayment.CashAccountID;
			aDest.CuryTranAmt = aDest.CuryOrigAmtSigned; //Check Later - for the case when currencies are different
			aDest.TranAmt = aDest.OrigAmtSigned;
		}
		protected static void Copy(CADepositDetail aDest, CashAccountDeposit aSettings)
		{
			aDest.ChargeEntryTypeID = aSettings.ChargeEntryTypeID;
			aDest.PaymentMethodID = aSettings.PaymentMethodID;
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<CADeposit.tranDate, CADeposit.finPeriodID, CADeposit.curyID>(e.Row, e.OldRow))
			{
				foreach (CADepositDetail tran in this.Details.Select())
				{
					this.Details.Cache.MarkUpdated(tran);
				}
				foreach (CADepositCharge tran in this.Charges.Select())
				{
					this.Charges.Cache.MarkUpdated(tran);
				}
			}

		}

		public override void Persist()
		{
			base.Persist();
		}

		#endregion

		#region Release & Voiding
		/// <summary>
		/// Releases CADeposit and optionally posts it dependong on <see cref="CASetup.AutoPostOption" />.
		/// </summary>
		/// <param name="doc">Deposit to release.</param>
		/// <param name="externalPostList">List of Batches to post. 
		/// If this parameter is not null batches will not be posted. 
		/// These batches will be included to the list instead. </param>
		public static void ReleaseDoc(CADeposit doc, List<Batch> externalPostList = null)
		{
			bool skipPosting = externalPostList != null;

			CAReleaseProcess rg = PXGraph.CreateInstance<CAReleaseProcess>();
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			List<Batch> batches = new List<Batch>();
			rg.Clear();
			rg.ReleaseDeposit(je, ref batches, doc);

			if (rg.AutoPost)
			{
				if (!skipPosting)
				{
					PostGraph pg = PXGraph.CreateInstance<PostGraph>();
					List<int> batchbind = new List<int>();

					for (int i = batchbind.Count; i < batches.Count; i++)
					{
						batchbind.Add(i);
					}

					for (int i = 0; i < batches.Count; i++)
					{
						Batch batch = batches[i];
						try
						{
							pg.Clear();
							pg.TimeStamp = batch.tstamp;
							pg.PostBatchProc(batch);

						}
						catch (Exception e)
						{
							throw new PX.Objects.Common.PXMassProcessException(batchbind[i], e);
						}
					}
				}
				else
				{
					foreach (Batch batch in batches)
					{
						externalPostList.Add(batch);
					}
				}
			}

		}

		public virtual void VoidDepositProc(CADeposit doc)
		{
			if (doc.Released == false) throw new PXException(Messages.NotVoidedUnreleased);
			PXResult<CADeposit, CurrencyInfo> orig = (PXResult<CADeposit, CurrencyInfo>)PXSelectReadonly2<CADeposit, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<CADeposit.curyInfoID>>>,
									Where<CADeposit.tranType, Equal<Required<CADeposit.tranType>>,
									And<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr);

			this.Clear();
			this.Details.Cache.AllowUpdate = true;
			this.Details.Cache.AllowInsert = true;
			this.Details.Cache.AllowDelete = true;

			this.Charges.Cache.AllowInsert = true;
			this.Charges.Cache.AllowUpdate = true;
			this.Charges.Cache.AllowDelete = true;

			this.currencyinfo.Cache.AllowDelete = true;
			this.currencyinfo.Cache.AllowInsert = true;
			this.currencyinfo.Cache.AllowUpdate = true;

			CADeposit newdoc = (CADeposit)this.Document.Cache.CreateCopy((CADeposit)orig);
			newdoc.NoteID = null;
			newdoc.TranType = CATranType.CAVoidDeposit;
			newdoc.TranID = null;
			newdoc.CashTranID = null;
			newdoc.ChargeTranID = null;
			newdoc.CuryInfoID = null;
			newdoc.Released = false;
			newdoc.Hold = true;
			newdoc.ClearDate = null;
			newdoc.Cleared = false;
			newdoc.LineCntr = 0;
			newdoc.DrCr = doc.DrCr == CADrCr.CADebit ? CADrCr.CACredit : CADrCr.CADebit;
			CurrencyInfo newInfo = (CurrencyInfo)this.currencyinfo.Cache.CreateCopy((CurrencyInfo)orig);
			newInfo.CuryInfoID = null;
			newInfo = (CurrencyInfo)this.currencyinfo.Insert(newInfo);
			newdoc.CuryInfoID = newInfo.CuryInfoID;
			newdoc.CuryDetailTotal = Decimal.Zero;
			newdoc.CuryChargeTotal = Decimal.Zero;
			newdoc.CuryTranAmt = Decimal.Zero;
			newdoc = this.Document.Insert(newdoc);

			this.Document.Current = newdoc;
			foreach (PXResult<CADepositDetail, ARPayment, APPayment> iDet in PXSelectReadonly2<CADepositDetail, LeftJoin<ARPayment, On<ARPayment.docType, Equal<CADepositDetail.origDocType>,
																And<ARPayment.refNbr, Equal<CADepositDetail.origRefNbr>,
																And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAR>>>>,
															LeftJoin<APPayment, On<APPayment.docType, Equal<CADepositDetail.origDocType>,
																And<APPayment.refNbr, Equal<CADepositDetail.origRefNbr>,
																And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAP>>>>>>,
																Where<CADepositDetail.tranType, Equal<Required<CADepositDetail.tranType>>,
																	And<CADepositDetail.refNbr, Equal<Required<CADepositDetail.refNbr>>>>,
															OrderBy<
																Asc<Switch<Case<Where<ARPayment.docType, Equal<ARDocType.voidPayment>>, int0>, int1>,
																Asc<Switch<Case<Where<APPayment.docType, Equal<APDocType.voidCheck>>, int0>, int1>,
																Asc<CADepositDetail.tranType, Asc<CADepositDetail.refNbr, Asc<CADepositDetail.lineNbr>>>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				CADepositDetail detail = iDet;
				ARPayment pmt = iDet;
				APPayment pmtAP = iDet;
				if (!String.IsNullOrEmpty(pmt.RefNbr) && pmt.Voided == true)
				{
					if (this.Details.Search<CADepositDetail.origModule, CADepositDetail.origDocType, CADepositDetail.origRefNbr>(BatchModule.AR, ARPaymentType.GetVoidingARDocType(pmt.DocType), pmt.RefNbr).Count == 0)
						continue; //Skip paymnets, which are already voided
				}
				if (!String.IsNullOrEmpty(pmtAP.RefNbr) && pmtAP.Voided == true)
				{
					if (this.Details.Search<CADepositDetail.origModule, CADepositDetail.origDocType, CADepositDetail.origRefNbr>(BatchModule.AP, APPaymentType.GetVoidingAPDocType(pmtAP.DocType), pmtAP.RefNbr).Count == 0)
						continue; //Skip paymnets, which are already voided
				}

				CADepositDetail newDetail = (CADepositDetail)this.Details.Cache.CreateCopy(detail);
				newDetail.TranType = newdoc.TranType;
				newDetail.DetailType = detail.DetailType == CADepositDetailType.CheckDeposit ? CADepositDetailType.VoidCheckDeposit :
										(detail.DetailType == CADepositDetailType.CashDeposit ? CADepositDetailType.VoidCashDeposit : string.Empty);
				if (string.IsNullOrEmpty(newDetail.DetailType))
					throw new PXException(Messages.UnknownDetailType, detail.DetailType);
				newDetail.DrCr = (detail.DrCr == CADrCr.CADebit ? CADrCr.CACredit : CADrCr.CADebit);
				newDetail.CuryInfoID = newInfo.CuryInfoID;
				newDetail.TranID = null;
				newDetail = this.Details.Insert(newDetail);
			}

			foreach (CADepositCharge iCharge in PXSelectReadonly<CADepositCharge, Where<CADepositCharge.tranType, Equal<Required<CADepositCharge.tranType>>,
																And<CADepositCharge.refNbr, Equal<Required<CADepositCharge.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				CADepositCharge newCharge = (CADepositCharge)this.Charges.Cache.CreateCopy(iCharge);
				newCharge.TranType = newdoc.TranType;
				newCharge.DrCr = (iCharge.DrCr == CADrCr.CADebit ? CADrCr.CACredit : CADrCr.CADebit);
				newCharge.CuryInfoID = newInfo.CuryInfoID;
				newCharge = this.Charges.Insert(newCharge);
			}

			this.Document.View.RequestRefresh();

		}

		#endregion

		#region Static Methods
		public static bool IsKeyEqual(ARPayment op1, CADepositDetail op2)
		{
			return (op1.DocType == op2.OrigDocType && op1.RefNbr == op2.OrigRefNbr);
		}
		#endregion
	}
}
