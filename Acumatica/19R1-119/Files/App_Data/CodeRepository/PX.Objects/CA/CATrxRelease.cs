using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	#region Additional classes and interfaces
	[System.SerializableAttribute()]
	public partial class CARegister : PX.Data.IBqlTable
	{
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }
		protected Int64? _TranID;
		[PXLong(IsKey = true)]
		[PXUIField(DisplayName = "Transaction Num.")]
		public virtual Int64? TranID
		{
			get
			{
				return this._TranID;
			}
			set
			{
				this._TranID = value;
			}
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected bool? _Hold = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hold")]
		public bool? Hold
		{
			get
			{
				return _Hold;
			}
			set
			{
				_Hold = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected bool? _Released = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public bool? Released
		{
			get
			{
				return _Released;
			}
			set
			{
				_Released = value;
			}
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;
		[PXString(3)]
		[GL.BatchModule.List()]
		[PXUIField(DisplayName = "Module")]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        protected String _TranType;
        [PXString(3)]
        [CAAPARTranType.ListByModule(typeof(module))]
        [PXUIField(DisplayName = "Transaction Type")]
        public virtual String TranType
        {
            get
            {
                return this._TranType;
            }
            set
            {
                this._TranType = value;
            }
        }
        #endregion
		#region ReferenceNbr
		public abstract class referenceNbr : PX.Data.BQL.BqlString.Field<referenceNbr> { }
		protected String _ReferenceNbr;
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ReferenceNbr
		{
			get
			{
				return this._ReferenceNbr;
			}
			set
			{
				this._ReferenceNbr = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		protected DateTime? _DocDate;
		[PXDate()]
		[PXUIField(DisplayName = "Doc. Date")]
		public virtual DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodID()]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDefault]
		[CashAccount(suppressActiveVerification: true, DisplayName = "Cash Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		protected Decimal? _TranAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount", Enabled = false)]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		protected Decimal? _CuryTranAmt;
		[PXDBCury(typeof(CARecon.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Enabled = false)]
		public virtual Decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
	}

	public interface ICADocument
	{
		string DocType
		{
			get;
		}
		string RefNbr
		{
			get;
		}
		Boolean? Released
		{
			get;
			set;
		}
		Boolean? Hold
		{
			get;
			set;
		}
	}

	public abstract class InfoMessage
	{
        public InfoMessage(PXErrorLevel aLevel, string aMessage)
        {
            this._ErrorLevel = aLevel;
            this._Message = aMessage;
        }

		#region PXErrorLevel
		protected PXErrorLevel _ErrorLevel;
	    public virtual PXErrorLevel ErrorLevel
		{
			get
			{
				return this._ErrorLevel;
			}
			set
			{
				this._ErrorLevel = value;
			}
		}
		#endregion
		#region Message

		protected String _Message;
		public virtual String Message
		{
			get
			{
				return this._Message;
			}
			set
			{
				this._Message = value;
			}
		}
		#endregion
	}

	public class CAMessage : InfoMessage
	{
        public CAMessage(long aKey, PXErrorLevel aLevel, string aMessage):base(aLevel, aMessage)
        {
            this._Key = aKey;
        }
		#region Key
		protected Int64 _Key;
		public virtual Int64 Key
		{
			get
			{
				return this._Key;
			}
			set
			{
				this._Key = value;
			}
		}
		#endregion
	}

	public class CAReconMessage : InfoMessage
	{
        public CAReconMessage(int aCashAccountID, string aReconNbr, PXErrorLevel aLevel, string aMessage):
                base(aLevel,aMessage)
        {
            this._KeyCashAccount = aCashAccountID;
            this._KeyReconNbr = aReconNbr;
        }

		#region KeyCashAccount
		public abstract class keyCashAccount : PX.Data.BQL.BqlInt.Field<keyCashAccount> { }
		protected Int32 _KeyCashAccount;
		public virtual Int32 KeyCashAccount
		{
			get
			{
				return this._KeyCashAccount;
			}
			set
			{
				this._KeyCashAccount = value;
			}
		}
		#endregion
		#region KeyReconNbr
		protected String _KeyReconNbr;
		public virtual String KeyReconNbr
		{
			get
			{
				return this._KeyReconNbr;
			}
			set
			{
				this._KeyReconNbr = value;
			}
		}
		#endregion
	}
	#endregion

	[Serializable]
	[PXPrimaryGraph(new Type[] {
					typeof(CATranEntry),
					typeof(CashTransferEntry)},
					new Type[] {
					typeof(Select<CAAdj, Where<CAAdj.tranID, Equal<Current<CATran.tranID>>>>),
					typeof(Select<CATransfer, Where<CATransfer.tranIDIn, Equal<Current<CATran.tranID>>,
							Or<CATransfer.tranIDOut, Equal<Current<CATran.tranID>>>>>)
				})]
	[TableAndChartDashboardType]
	public class CATrxRelease : PXGraph<CATrxRelease>
	{
		/// <summary>
		/// CashAccount override - SQL Alias
		/// </summary>
		[Serializable]
		[PXHidden]
		public class CashAccount1 : CashAccount
		{
			public new abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		}
		public CATrxRelease()
		{
			CASetup setup = cASetup.Current;
			CARegisterList.SetProcessDelegate(delegate (List<CARegister> list)
			{
				GroupRelease(list, true);
			});
			CARegisterList.SetProcessCaption(Messages.Release);
			CARegisterList.SetProcessAllCaption(Messages.ReleaseAll);
		}
		#region Buttons
		#region Cancel
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			CARegisterList.Cache.Clear();
			TimeStamp = null;
			PXLongOperation.ClearStatus(this.UID);
			return adapter.Get();
		}
		#endregion

		#region viewCATrax
		public PXAction<CARegister> viewCATrx;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewCATrx(PXAdapter adapter)
		{
			CARegister register = CARegisterList.Current;
			if (register != null)
			{
				switch (register.TranType)
				{
					case (CATranType.CAAdjustment):
						CATranEntry graphTranEntry = PXGraph.CreateInstance<CATranEntry>();
						graphTranEntry.Clear();
						CARegisterList.Cache.IsDirty = false;
						graphTranEntry.CAAdjRecords.Current = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CATran.origRefNbr>>>>
							.Select(this, register.ReferenceNbr); // !!!
						throw new PXRedirectRequiredException(graphTranEntry, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					//break;
					case (CATranType.CAVoidDeposit):
					case (CATranType.CADeposit):
						CADepositEntry graphDepositEntry = PXGraph.CreateInstance<CADepositEntry>();
						graphDepositEntry.Clear();
						CARegisterList.Cache.IsDirty = false;
						graphDepositEntry.Document.Current = PXSelect<CADeposit, Where<CADeposit.refNbr, Equal<Required<CATran.origRefNbr>>, And<CADeposit.tranType, Equal<Required<CATran.origTranType>>>>>
							.Select(this, register.ReferenceNbr, register.TranType);
						throw new PXRedirectRequiredException(graphDepositEntry, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					//break;
					case (CATranType.CATransfer):
						CashTransferEntry graphTransferEntry = PXGraph.CreateInstance<CashTransferEntry>();
						graphTransferEntry.Clear();
						CARegisterList.Cache.IsDirty = false;
						graphTransferEntry.Transfer.Current = PXSelect<CATransfer, Where<CATransfer.transferNbr, Equal<Required<CATransfer.transferNbr>>>>
							.Select(this, register.ReferenceNbr);
						throw new PXRedirectRequiredException(graphTransferEntry, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
						//break;
				}
			}
			return CARegisterList.Select();
		}
		#endregion

		#endregion

		#region selectStatements
		public PXAction<CARegister> cancel;

		[PXFilterable]
		public PXProcessing<CARegister> CARegisterList;
		public PXSetup<CASetup> cASetup;
		#endregion

		#region Function
		protected virtual IEnumerable caregisterlist()
		{
			bool anyFound = false;
			foreach (CARegister tlist in CARegisterList.Cache.Inserted)
			{
				anyFound = true;
				yield return tlist;
			}

			if (anyFound)
			{
				yield break;
			}
			foreach (CADeposit deposit in PXSelectJoin<CADeposit, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CADeposit.cashAccountID>, And<Match<CashAccount, Current<AccessInfo.userName>>>>>,
				Where<CADeposit.released, Equal<boolFalse>, And<CADeposit.hold, Equal<boolFalse>,
				And<Where<CADeposit.tranType, Equal<CATranType.cADeposit>, Or<CADeposit.tranType, Equal<CATranType.cAVoidDeposit>>>>>>>.Select(this))
			{
				if (deposit.TranID != null)
					yield return CARegisterList.Cache.Insert(CARegister(deposit));
			}
			foreach (CAAdj adj in PXSelectJoin<CAAdj, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CAAdj.cashAccountID>, And<Match<CashAccount, Current<AccessInfo.userName>>>>>, Where<CAAdj.released, Equal<boolFalse>, And<CAAdj.status, Equal<CATransferStatus.balanced>, And<CAAdj.adjTranType, Equal<CATranType.cAAdjustment>>>>>.Select(this))
			{
				if (adj.TranID != null)
					yield return CARegisterList.Cache.Insert(CARegister(adj));
			}

			foreach (CATransfer trf in PXSelectJoin<CATransfer,
											InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CATransfer.inAccountID>, And<Match<CashAccount, Current<AccessInfo.userName>>>>,
											InnerJoin<CashAccount1, On<CashAccount1.cashAccountID, Equal<CATransfer.outAccountID>, And<Match<CashAccount1, Current<AccessInfo.userName>>>>>>,
											Where<CATransfer.released, Equal<boolFalse>, And<CATransfer.hold, Equal<boolFalse>>>>.Select(this))
			{
				foreach (CATran tran in PXSelect<CATran, Where<CATran.released, Equal<boolFalse>,
																													 And<CATran.hold, Equal<boolFalse>,
																													 And<Where<CATran.tranID, Equal<Required<CATransfer.tranIDIn>>,
																																	Or<CATran.tranID, Equal<Required<CATransfer.tranIDOut>>>>>>>>.Select(this, trf.TranIDIn, trf.TranIDOut))
				{
					yield return CARegisterList.Cache.Insert(CARegister(trf, tran));
				}
			}

			CARegisterList.Cache.IsDirty = false;
		}

		protected virtual void CARegister_TranID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CARegister row = (CARegister)e.Row;
			if (row != null)
			{
				Dictionary<long, CAMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as Dictionary<long, CAMessage>;
				TimeSpan timespan;
				Exception ex;
				PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);
				if ((status == PXLongRunStatus.Aborted || status == PXLongRunStatus.Completed)
							&& listMessages != null)
				{
					CAMessage message = null;

					if (listMessages.ContainsKey(row.TranID.Value))
						message = listMessages[row.TranID.Value];
					if (message != null)
					{
						string fieldName = nameof(CABankTran.extTranID);
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
									null, null, message.Message, message.ErrorLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
						e.IsAltered = true;
					}
				}
			}
		}

		public static void GroupReleaseTransaction(List<CATran> tranList, bool allowAP, bool allowAR, bool updateInfo)
		{
			Dictionary<long, CAMessage> listMessages = new Dictionary<long, CAMessage>();
			if (updateInfo == true)
				PXLongOperation.SetCustomInfo(listMessages);
			List<CARegister> caRegisterList = new List<CARegister>();
			bool allPassed = true;
			PXGraph searchGraph = null;
			for (int i = 0; i < tranList.Count; i++)
			{
				CATran tran = tranList[i];
				try
				{
					if (tran.Released == true)
					{
						continue;
					}
					if (tran.Hold == true)
					{
						throw new PXException(Messages.DocumentStatusInvalid);
					}
					switch (tran.OrigModule)
					{
						case GL.BatchModule.GL:
							throw new PXException(Messages.ThisDocTypeNotAvailableForRelease);
						case GL.BatchModule.AP:
							if (allowAP != true)
							{
								throw new PXException(Messages.ThisDocTypeNotAvailableForRelease);
							}
							else
							{
								CATrxRelease.ReleaseCATran(tran, ref searchGraph);
							}
							break;
						case GL.BatchModule.AR:
							if (allowAR != true)
							{
								throw new PXException(Messages.ThisDocTypeNotAvailableForRelease);
							}
							else
							{
								CATrxRelease.ReleaseCATran(tran, ref searchGraph);
							}
							break;
						case GL.BatchModule.CA:
							CATrxRelease.ReleaseCATran(tran, ref searchGraph);
							break;
						default:
							throw new PXException(Messages.ThisDocTypeNotAvailableForRelease);
					}
					if (updateInfo == true)
						listMessages.Add(tran.TranID.Value, new CAMessage(tran.TranID.Value, PXErrorLevel.RowInfo, ActionsMessages.RecordProcessed));
				}
				catch (Exception ex)
				{
					allPassed = false;
					if (updateInfo == true)
						listMessages.Add(tran.TranID.Value, new CAMessage(tran.TranID.Value, PXErrorLevel.RowError, ex.Message));
				}
			}
			if (!allPassed)
			{
				throw new PXException(Messages.OneOrMoreItemsAreNotReleased);
			}
		}

		public static void ReleaseCATran(CATran aTran, ref PXGraph aGraph)
		{
			ReleaseCATran(aTran, ref aGraph, null);
		}
		public static void ReleaseCATran(CATran aTran, ref PXGraph aGraph, List<Batch> externalPostList)
		{
			int i = 0;
			if (aTran != null)
			{
				if (aGraph == null)
					aGraph = PXGraph.CreateInstance<CATranEntry>();
				PXGraph caGraph = aGraph;
				switch (aTran.OrigModule)
				{
					case GL.BatchModule.AP:
						List<APRegister> apList = new List<APRegister>();
						APRegister apReg = PXSelect<APRegister,
															Where<APRegister.docType, Equal<Required<APRegister.docType>>,
																And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.
																Select(caGraph, aTran.OrigTranType, aTran.OrigRefNbr);
						if (apReg != null)
						{
							if (apReg.Hold == false)
							{
								if (apReg.Approved == false)
								{
									throw new PXException(Messages.CannotReleasePendingApprovalDocument);
								}
								if (apReg.Released == false)
								{
									apList.Add(apReg);
									APDocumentRelease.ReleaseDoc(apList, false, externalPostList);
								}
							}
							else
							{
								throw new PXException(Messages.DocumentStatusInvalid);
							}
						}
						else
						{
							throw new Exception(Messages.DocNotFound);
						}
						break;

					case GL.BatchModule.AR:
						List<ARRegister> arList = new List<ARRegister>();
						ARRegister arReg = PXSelect<ARRegister,
														Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
															And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.
															Select(caGraph, aTran.OrigTranType, aTran.OrigRefNbr);
						if (arReg != null)
						{
							if (arReg.Hold == false)
							{

								if (arReg.Approved == false)
								{
									throw new PXException(Messages.CannotReleasePendingApprovalDocument);
								}
								if (arReg.Released == false)
								{
									arList.Add(arReg);
									ARDocumentRelease.ReleaseDoc(arList, false, externalPostList);
								}
							}
							else
							{
								throw new PXException(Messages.DocumentStatusInvalid);
							}
						}
						else
						{
							throw new Exception(Messages.DocNotFound);
						}
						break;

					case GL.BatchModule.CA:
						switch (aTran.OrigTranType)
						{
							case CATranType.CAAdjustment:
								CAAdj docAdj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>, And<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>>>>.Select(caGraph, aTran.OrigRefNbr, aTran.OrigTranType);
								if (docAdj != null)
								{
									if (docAdj.Hold == false)
									{
										if (docAdj.Approved == false && docAdj.RequestApproval == true)
										{
											throw new PXException(Messages.CannotReleasePendingApprovalDocument);
										}
										if (docAdj.Released == false)
										{
											ReleaseDoc<CAAdj>(docAdj, i, externalPostList);
										}
									}
									else
									{
										throw new PXException(Messages.DocumentStatusInvalid);
									}
								}
								else
								{
									throw new Exception(Messages.DocNotFound);
								}
								break;
							case CATranType.CATransferIn:
							case CATranType.CATransferOut:
							case CATranType.CATransferExp:

								CATransfer docTransfer = PXSelect<CATransfer, Where<CATransfer.transferNbr, Equal<Required<CATransfer.transferNbr>>>>.Select(caGraph, aTran.OrigRefNbr);
								if (docTransfer != null)
								{
									if (docTransfer.Hold == false)
									{
										if (docTransfer.Released == false)
										{
											ReleaseDoc<CATransfer>(docTransfer, i, externalPostList);
										}
									}
									else
									{
										throw new PXException(Messages.DocumentStatusInvalid);
									}
								}
								else
								{
									throw new Exception(Messages.DocNotFound);
								}
								break;
							case CATranType.CADeposit:
							case CATranType.CAVoidDeposit:
								CADeposit docDeposit = PXSelect<CADeposit, Where<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>>>.Select(caGraph, aTran.OrigRefNbr);
								if (docDeposit != null)
								{
									if (docDeposit.Hold == false)
									{
										if (docDeposit.Released == false)
										{
											CADepositEntry.ReleaseDoc(docDeposit, externalPostList);
										}
									}
									else
									{
										throw new PXException(Messages.DocumentStatusInvalid);
									}
								}
								else
								{
									throw new Exception(Messages.DocNotFound);
								}
								break;
							default:
								throw new Exception(Messages.DocNotFound);
						}
						break;
					default:
						throw new Exception(Messages.ThisDocTypeNotAvailableForRelease);
				}
			}
		}

		public static void GroupRelease(List<CARegister> list, bool updateInfo)
		{
			Dictionary<long, CAMessage> listMessages = new Dictionary<long, CAMessage>();
			if (updateInfo == true)
				PXLongOperation.SetCustomInfo(listMessages);


			CAReleaseProcess rgForCA = PXGraph.CreateInstance<CAReleaseProcess>();
			JournalEntry jeForCA = CreateJournalEntry();

			HashSet<int> batchbind = new HashSet<int>();

			Exception exception = null;
			for (int i = 0; i < list.Count; i++)
			{
				CARegister caRegisterItem = list[i];
				if (caRegisterItem.TranID == null)
				{
					throw new PXException(Messages.ErrorsProcessingEmptyLines);
				}
				if (caRegisterItem != null)
				{
					try
					{
						if (caRegisterItem.Released == false)
						{
							if ((bool)caRegisterItem.Hold)
							{
								throw new Exception(Messages.HoldDocCanNotBeRelease);
							}
							else
							{
								switch (caRegisterItem.Module)
								{
									case GL.BatchModule.AP:
										List<APRegister> apList = new List<APRegister>();
										APRegister apReg = PXSelect<APRegister,
																		Where<APRegister.docType, Equal<Required<APRegister.docType>>,
																			And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.
																			Select(jeForCA, caRegisterItem.TranType, caRegisterItem.ReferenceNbr);
										if (apReg != null)
										{
											apList.Add(apReg);
											APDocumentRelease.ReleaseDoc(apList, false);
										}
										else
											throw new Exception(Messages.TransactionNotComplete);
										break;
									case GL.BatchModule.AR:
										List<ARRegister> arList = new List<ARRegister>();
										ARRegister arReg = PXSelect<ARRegister,
																		Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
																			And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.
																			Select(jeForCA, caRegisterItem.TranType, caRegisterItem.ReferenceNbr);
										if (arReg != null)
										{
											arList.Add(arReg);
											ARDocumentRelease.ReleaseDoc(arList, false);
										}
										else
											throw new Exception(Messages.TransactionNotComplete);
										break;
									case GL.BatchModule.CA:
										switch (caRegisterItem.TranType)
										{
											case CATranType.CAAdjustment:
												CAAdj docAdj = PXSelect<CAAdj,
													Where<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>,
													And<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>>>>.Select(jeForCA, caRegisterItem.ReferenceNbr, caRegisterItem.TranType);
												if (docAdj != null)
												{
													ReleaseAndRecordCADoc(jeForCA, rgForCA, docAdj, caRegisterItem.TranID.Value);
												}
												else
													throw new Exception(Messages.DocNotFound);
												break;
											case CATranType.CATransfer:
												CATransfer docTransfer = PXSelect<CATransfer,
													Where<CATransfer.transferNbr, Equal<Required<CATransfer.transferNbr>>>>.Select(jeForCA, caRegisterItem.ReferenceNbr);
												if (docTransfer != null)
												{
													ReleaseAndRecordCADoc(jeForCA, rgForCA, docTransfer, caRegisterItem.TranID.Value);
												}
												else
													throw new Exception(Messages.DocNotFound);
												break;

											case CATranType.CADeposit:
											case CATranType.CAVoidDeposit:
												CADeposit docDeposit = PXSelect<CADeposit,
													Where<CADeposit.tranType, Equal<Required<CADeposit.tranType>>,
													And<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>>>>.Select(jeForCA, caRegisterItem.TranType, caRegisterItem.ReferenceNbr);
												if (docDeposit != null)
												{
													CADepositEntry.ReleaseDoc(docDeposit);
												}
												else
													throw new Exception(Messages.DocNotFound);
												break;
											default:
												throw new Exception(Messages.DocNotFound);
										}
										break;
									default:
										throw new Exception(Messages.DocNotFound);
								}
								int k;
								if ((k = jeForCA.created.IndexOf(jeForCA.BatchModule.Current)) >= 0)
								{
									batchbind.Add(k);
								}
								if (updateInfo == true && (caRegisterItem.Module != GL.BatchModule.CA || rgForCA.AutoPost == false))
								{
									listMessages.Add(caRegisterItem.TranID.Value, new CAMessage(caRegisterItem.TranID.Value, PXErrorLevel.RowInfo, ActionsMessages.RecordProcessed));
								}
							}
						}
						else
						{
							throw new Exception(Messages.OriginalDocAlreadyReleased);
						}
					}
					catch (Exception e)
					{
						if (updateInfo == true)
						{
							string message = e is PXOuterException ? (e.Message + " " + String.Join(" ", ((PXOuterException)e).InnerMessages)) : e.Message;
							listMessages.Add(caRegisterItem.TranID.Value, new CAMessage(caRegisterItem.TranID.Value, PXErrorLevel.RowError, message));
						}
						jeForCA.Clear();
                        jeForCA.CleanupCreated(batchbind);
						exception = e;
					}
				}
			}

			Exception caPostingException = null;
			if (rgForCA.AutoPost)
			{
				var errors = PostGraph.Post(jeForCA.created.Where(b => b.Released == true).ToList());
				if (errors.Count > 0)
				{
					caPostingException = errors.Values.FirstOrDefault();
					if (updateInfo)
					{
						foreach (KeyValuePair<Batch, Exception> error in errors)
						{
							foreach (GLTran tran in PXSelect<GLTran,
										Where<GLTran.module, Equal<Required<Batch.module>>,
										And<GLTran.batchNbr, Equal<Required<Batch.batchNbr>>,
										And<GLTran.cATranID, IsNotNull>>>>.Select(rgForCA, error.Key.Module, error.Key.BatchNbr))
							{
								long catranID = tran.CATranID ?? 0;
								if (!listMessages.ContainsKey(catranID))
				{
									listMessages.Add(catranID, new CAMessage(catranID, PXErrorLevel.RowError, error.Value.Message));
				}
							}
						}
					}
				}
				if (updateInfo)
				{
					foreach (CARegister doc in list)
				{
						long catranID = doc.TranID.Value;
						if (!listMessages.ContainsKey(catranID))
						{
							listMessages.Add(catranID, new CAMessage(catranID, PXErrorLevel.RowInfo, ActionsMessages.RecordProcessed));
						}
				}
			}
			}


			if (exception != null)
				if (list.Count == 1)
					throw exception;
				else
					throw new Exception(Messages.OneOrMoreItemsAreNotReleased);

			if (caPostingException != null)
				if (list.Count == 1)
					throw caPostingException;
				else
					throw new Exception(Messages.OneOrMoreItemsAreNotPosted);
		}

		public static JournalEntry CreateJournalEntry()
		{
			JournalEntry jeForCA = PXGraph.CreateInstance<JournalEntry>();
			jeForCA.PrepareForDocumentRelease();
			jeForCA.RowInserting.AddHandler<GLTran>((sender, e) =>
			{
				((GLTran)e.Row).ZeroPost = ((GLTran)e.Row).ZeroPost ?? true;
			});
			return jeForCA;
		}

		private static void ReleaseAndRecordCADoc<TCADoc>(JournalEntry je, CAReleaseProcess rg, TCADoc doc, long tranID)
			where TCADoc : class, ICADocument, new()
		{
			var ignoredBatchList = new List<Batch>();
			rg.Clear();
			var batches = rg.ReleaseDocProc(je, ref ignoredBatchList, doc);

		}

		public static CARegister CARegister(CATran item)
		{
			CATranEntry caGraph = PXGraph.CreateInstance<CATranEntry>();

			switch (item.OrigModule)
			{
				case GL.BatchModule.AP:
					APPayment apPay = (APPayment)PXSelect<APPayment, Where<APPayment.cATranID, Equal<Required<APPayment.cATranID>>>>.
																							Select(caGraph, item.TranID);
					if (apPay != null)
					{
						return CARegister(apPay);
					}
					else
						throw new Exception(Messages.OrigDocCanNotBeFound);

				case GL.BatchModule.AR:
					ARPayment arPay = (ARPayment)PXSelect<ARPayment, Where<ARPayment.cATranID, Equal<Required<ARPayment.cATranID>>>>.
																							Select(caGraph, item.TranID);
					if (arPay != null)
					{
						return CARegister(arPay);
					}
					else
						throw new Exception(Messages.OrigDocCanNotBeFound);

				case GL.BatchModule.GL:
					GLTran gLTran = PXSelect<GLTran,
														 Where<GLTran.module, Equal<Required<GLTran.module>>,
															 And<GLTran.cATranID, Equal<Required<GLTran.cATranID>>>>>.
													Select(caGraph, item.OrigModule, item.TranID);
					if (gLTran != null)
					{
						CARegister reg = CARegister(gLTran);
						int? cashAccountID;
						if (GL.GLCashTranIDAttribute.CheckGLTranCashAcc(caGraph, gLTran, out cashAccountID) == true)
						{
							reg.CashAccountID = cashAccountID;
							return reg;
						}
						else
						{
							Branch branch = (Branch)PXSelectorAttribute.Select<GLTran.branchID>(caGraph.Caches[typeof(GLTran)], gLTran);
							Account account = (Account)PXSelectorAttribute.Select<GLTran.accountID>(caGraph.Caches[typeof(GLTran)], gLTran);
							Sub sub = (Sub)PXSelectorAttribute.Select<GLTran.subID>(caGraph.Caches[typeof(GLTran)], gLTran);
							throw new PXException(GL.Messages.CashAccountDoesNotExist, branch.BranchCD, account.AccountCD, sub.SubCD);
						}
					}
					else
						throw new Exception(Messages.OrigDocCanNotBeFound);

				case GL.BatchModule.CA:
					switch (item.OrigTranType)
					{
						case CATranType.CAAdjustment:
							CAAdj docAdj = PXSelect<CAAdj, Where<CAAdj.tranID, Equal<Required<CAAdj.tranID>>>>.Select(caGraph, item.TranID);
							if (docAdj != null)
							{
								return CARegister(docAdj);
							}
							else
								throw new Exception(Messages.OrigDocCanNotBeFound);
						case CATranType.CATransferIn:
							CATransfer docTransferIn = PXSelect<CATransfer, Where<CATransfer.tranIDIn, Equal<Required<CATransfer.tranIDIn>>>>
																			.Select(caGraph, item.TranID);
							if (docTransferIn != null)
							{
								return CARegister(docTransferIn, item);
							}
							else
								throw new Exception(Messages.OrigDocCanNotBeFound);
						case CATranType.CATransferOut:
							CATransfer docTransferOut = PXSelect<CATransfer, Where<CATransfer.tranIDOut, Equal<Required<CATransfer.tranIDOut>>>>
																			.Select(caGraph, item.TranID);
							if (docTransferOut != null)
							{
								return CARegister(docTransferOut, item);
							}
							else
								throw new Exception(Messages.OrigDocCanNotBeFound);
						default:
							throw new Exception(Messages.ThisCATranOrigDocTypeNotDefined);
					}
				default:
					throw new Exception(Messages.ThisCATranOrigDocTypeNotDefined);
			}
		}
		public static CARegister CARegister(CADeposit item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.TranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.CA;
			ret.TranType = item.TranType;
			ret.Description = item.TranDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.TranDate;
			ret.ReferenceNbr = item.RefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.CashAccountID;
			ret.CuryID = item.CuryID;
			ret.TranAmt = item.TranAmt;
			ret.CuryTranAmt = item.CuryTranAmt;

			return ret;
		}
		public static CARegister CARegister(CAAdj item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.TranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.CA;
			ret.TranType = item.AdjTranType;
			ret.Description = item.TranDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.TranDate;
			ret.ReferenceNbr = item.AdjRefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.CashAccountID;
			ret.CuryID = item.CuryID;
			ret.TranAmt = item.TranAmt;
			ret.CuryTranAmt = item.CuryTranAmt;

			return ret;
		}

		public static CARegister CARegister(GLTran item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.CATranID;
			ret.Hold = (item.Released != true);
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.GL;
			ret.TranType = item.TranType;
			ret.Description = item.TranDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.TranDate;
			ret.ReferenceNbr = item.RefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.AccountID;
			//ret.CuryID        = item.;
			ret.TranAmt = item.DebitAmt - item.CreditAmt;
			ret.CuryTranAmt = item.CuryDebitAmt - item.CuryCreditAmt;

			return ret;
		}

		public static CARegister CARegister(ARPayment item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.CATranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.AR;
			ret.TranType = item.DocType;
			ret.Description = item.DocDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.DocDate;
			ret.ReferenceNbr = item.RefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.CashAccountID;
			ret.CuryID = item.CuryID;
			ret.TranAmt = item.DocBal;
			ret.CuryTranAmt = item.DocBal;

			return ret;
		}

		public static CARegister CARegister(APPayment item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.CATranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.AP;
			ret.TranType = item.DocType;
			ret.Description = item.DocDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.DocDate;
			ret.ReferenceNbr = item.RefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.CashAccountID;
			ret.CuryID = item.CuryID;
			ret.TranAmt = item.DocBal;
			ret.CuryTranAmt = item.DocBal;

			return ret;
		}

		public static CARegister CARegister(CATransfer item, CATran tran)
		{
			CARegister ret = new CARegister();
			ret.TranID = tran.TranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.CA;
			ret.TranType = CATranType.CATransfer;
			ret.Description = item.Descr;
			ret.FinPeriodID = tran.FinPeriodID;
			ret.DocDate = item.OutDate;
			ret.ReferenceNbr = item.TransferNbr;
			ret.TranType = CATranType.CATransfer;
			ret.NoteID = item.NoteID;

			ret.CashAccountID = tran.CashAccountID;
			ret.CuryID = tran.CuryID;
			ret.CuryTranAmt = tran.CuryTranAmt;
			ret.TranAmt = tran.TranAmt;
			return ret;
		}

		public static void ReleaseDoc<TCADocument>(TCADocument _doc, int _item, List<Batch> externalPostList)
			where TCADocument : class, ICADocument, new()
		{
			CAReleaseProcess rg = PXGraph.CreateInstance<CAReleaseProcess>();
			JournalEntry je = CreateJournalEntry();

			bool skipPost = (externalPostList != null);
			List<Batch> batchlist = new List<Batch>();
			List<int> batchbind = new List<int>();

			bool failed = false;
			rg.Clear();
			rg.ReleaseDocProc(je, ref batchlist, _doc);

			for (int i = batchbind.Count; i < batchlist.Count; i++)
			{
				batchbind.Add(i);
			}
			if (skipPost)
			{
				if (rg.AutoPost)
					externalPostList.AddRange(batchlist);
			}
			else
			{
				PostGraph pg = PXGraph.CreateInstance<PostGraph>();
				for (int i = 0; i < batchlist.Count; i++)
				{
					Batch batch = batchlist[i];
					try
					{
						if (rg.AutoPost)
						{
							pg.Clear();
							pg.TimeStamp = batch.tstamp;
							pg.PostBatchProc(batch);
						}
					}
					catch (Exception e)
					{
						throw new PX.Objects.Common.PXMassProcessException(batchbind[i], e);
					}
				}
			}
			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}

		
		#endregion
	}
}
