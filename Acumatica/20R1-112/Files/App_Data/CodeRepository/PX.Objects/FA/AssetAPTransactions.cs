using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.FA.Overrides.AssetProcess;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.SM;
using Branch = PX.Objects.GL.Branch;

namespace PX.Objects.FA
{
	[Serializable]
	public class AssetGLTransactions : PXGraph<AssetGLTransactions>
	{

		public partial class Error : IBqlTable
		{
			public abstract class errorMessage : PX.Data.BQL.BqlString.Field<errorMessage> { }
			[PXString]
			public virtual string ErrorMessage { get; set; }

			#region GLTranID
			public abstract class gLTranID : PX.Data.BQL.BqlInt.Field<gLTranID> { }
			[PXDBInt]
			public virtual int? GLTranID
			{
				get;
				set;
			}
			#endregion

		}

		private readonly Dictionary<int?, FixedAsset> _PersistedAssets = new Dictionary<int?, FixedAsset>();
		#region Public Selects
		public PXCancel<GLTranFilter> Cancel;
		public PXFilter<GLTranFilter> Filter;

		public PXSelect<BAccount> BAccount;
		public PXSelect<Vendor> Vendor;
		public PXSelect<EPEmployee> Employee;

		[PXFilterable] 
		public PXFilteredProcessing<FAAccrualTran, GLTranFilter> GLTransactions;

		public PXSelect<FixedAsset> Assets;
		public PXSelect<FALocationHistory> Locations;
		public PXSelect<FADetails> Details;
		public PXSelect<FABookBalance> Balances;
		public PXSelect<FARegister> Register;
		public PXSelect<FATran, Where<FATran.gLtranID, Equal<Optional<FAAccrualTran.tranID>>, And<FATran.Tstamp, IsNull>>> FATransactions;
		public PXSelect<FABookHist> bookhist;
		public PXSelect<Sub> Subaccounts;

		public PXSetup<Company> company;
		public PXSetup<FASetup> fasetup;
		public PXSetup<GLSetup> glsetup;

		public PXSelectJoin<Numbering, InnerJoin<FASetup, On<FASetup.assetNumberingID, Equal<Numbering.numberingID>>>> assetNumbering;

		[PXHidden]
		public PXFilter<Error> StoredError;
		#endregion

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		[InjectDependency]
		public IFABookPeriodRepository FABookPeriodRepository { get; set; }

		[InjectDependency]
		public IFABookPeriodUtils FABookPeriodUtils { get; set; }
		#region Ctor

		public AssetGLTransactions()
		{
			fasetup.Current = null;
			object record = fasetup.Current;
			record = glsetup.Current;

			if (fasetup.Current.UpdateGL != true)
			{
				throw new PXSetupNotEnteredException<FASetup>(Messages.OperationNotWorkInInitMode, PXUIFieldAttribute.GetDisplayName<FASetup.updateGL>(fasetup.Cache)); 
			}

			Numbering numbering = PXSelect<Numbering, Where<Numbering.numberingID, Equal<Current<FASetup.registerNumberingID>>>>.Select(this);
			if (numbering == null)
			{
				throw new PXSetPropertyException(CS.Messages.NumberingIDNull);
			}
			if (numbering.UserNumbering == true)
			{
				throw new PXSetPropertyException(CS.Messages.CantManualNumber, numbering.NumberingID);
			}

			PXUIFieldAttribute.SetEnabled<FAAccrualTran.classID>(GLTransactions.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<FAAccrualTran.branchID>(GLTransactions.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<FAAccrualTran.employeeID>(GLTransactions.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<FAAccrualTran.department>(GLTransactions.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<FAAccrualTran.reconciled>(GLTransactions.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<FATran.tranType>(FATransactions.Cache, null, false);
		}
		#endregion

		#region View Delegates
		public static IEnumerable additions(PXGraph graph, GLTranFilter filter, PXCache accrualCache)
		{
			PXSelectBase<FAAccrualTran> cmd = new PXSelectJoin<FAAccrualTran, LeftJoin<Account, On<Account.accountID, Equal<FAAccrualTran.gLTranAccountID>>>>(graph);
			if (filter.AccountID != null)
			{
				cmd.WhereAnd<Where<FAAccrualTran.gLTranAccountID, Equal<Current<GLTranFilter.accountID>>>>();
			}
			else
			{
				cmd.WhereAnd<Where2<Match<Account, Current<AccessInfo.userName>>,
									And<Account.active, Equal<True>,
									And2<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
										Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>,
									And<Where<Account.curyID, IsNull,
										Or<Account.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>>>>>();
			}
			cmd.WhereAnd<Where<FAAccrualTran.closedAmt, Less<FAAccrualTran.gLTranAmt>, Or<FAAccrualTran.tranID, IsNull>>>();
			cmd.WhereAnd<Where<Current<GLTranFilter.showReconciled>, Equal<True>, Or<FAAccrualTran.reconciled, NotEqual<True>, Or<FAAccrualTran.reconciled, IsNull>>>>();
			if (filter.ReconType == GLTranFilter.reconType.Addition)
			{
				cmd.WhereAnd<Where<FAAccrualTran.gLTranDebitAmt, Greater<decimal0>>>();
			}
			else
			{
				cmd.WhereAnd<Where<FAAccrualTran.gLTranCreditAmt, Greater<decimal0>>>();
			}

			if (filter.SubID != null)
			{
				cmd.WhereAnd<Where<FAAccrualTran.gLTranSubID, Equal<Current<GLTranFilter.subID>>>>();
			}

			int startRow = PXView.StartRow;
			int totalRows = 0;

			List<FAAccrualTran> list = new List<FAAccrualTran>();
			foreach (PXResult<FAAccrualTran> res in cmd.View.Select(PXView.Currents, null, PXView.Searches, null, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				FAAccrualTran ext = res;
				if (ext.GLTranAmt == null)
				{
					ext.GLTranAmt = ext.GLTranDebitAmt + ext.GLTranCreditAmt;
					ext.GLTranQty = ext.GLTranOrigQty;
					ext.SelectedAmt = 0m;
					ext.SelectedQty = 0m;
					ext.OpenAmt = ext.GLTranAmt;
					ext.OpenQty = ext.GLTranOrigQty;
					ext.ClosedAmt = 0m;
					ext.ClosedQty = 0m;
					ext.UnitCost = ext.GLTranOrigQty > 0 ? ext.GLTranAmt / ext.GLTranOrigQty : ext.GLTranAmt;
					ext.Reconciled = false;

					accrualCache.SetStatus(ext, PXEntryStatus.Inserted);
					accrualCache.RaiseRowInserting(ext);
				}
				list.Add(ext);
			}
			PXView.StartRow = 0;
			return list;
		}

		public virtual IEnumerable gltransactions()
		{
			return additions(this, Filter.Current, GLTransactions.Cache);
		}
		#endregion

		#region Events

		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);
            Caches.AddCacheMappingsWithInheritance(this, typeof(GLTranFilter));
		    Caches.AddCacheMappingsWithInheritance(this,typeof(GLTran));
		    Caches.AddCacheMappingsWithInheritance(this,typeof(FATran));
		    Caches.AddCacheMappingsWithInheritance(this,typeof(FABookBalance));
		    Caches.AddCacheMappingsWithInheritance(this,typeof(FixedAsset));
			//var glTranFilterCache = Caches[typeof(GLTranFilter)];
			//var glTranCache = Caches[typeof(GLTran)];
			//var faTranCache = Caches[typeof(FATran)];
			//var faBookCache = Caches[typeof(FABookBalance)];
			//var fixedAssetCache = Caches[typeof(FixedAsset)];
		}

		protected void SetProcessDelegate()
		{
			if (!PXLongOperation.Exists(UID))
			{
				PXGraph new_graph = this.Clone();
				GLTransactions.SetProcessDelegate((List<FAAccrualTran> list) => new_graph.Actions.PressSave());
			}
		}

		#region Filter
		protected virtual void GLTranFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SetProcessDelegate();
			GLTransactions.SetProcessAllVisible(false);

			var account = (Account)PXSelectorAttribute.Select<GLTranFilter.accountID>(sender, e.Row);
			sender.RaiseExceptionHandling<GLTranFilter.accountID>(e.Row, account?.AccountCD, null);
			try
			{
				AccountAttribute.VerifyAccountIsNotControl(account);
				GLTransactions.SetProcessEnabled(true);
			}
			catch (PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling<GLTranFilter.accountID>(e.Row, account?.AccountCD, ex);
				GLTransactions.SetProcessEnabled(ex.ErrorLevel < PXErrorLevel.Error);
			}
		}

		protected virtual void GLTranFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<GLTranFilter.accountID, GLTranFilter.subID>(e.Row, e.OldRow))
				ClearInserted();
			if (!sender.ObjectsEqual<GLTranFilter.branchID, GLTranFilter.employeeID, GLTranFilter.department>(e.Row, e.OldRow))
			{
				GLTranFilter filter = (GLTranFilter)e.Row;
				foreach (FAAccrualTran tran in GLTransactions.Cache.Cached)
				{
					FAAccrualTran copy = (FAAccrualTran)GLTransactions.Cache.CreateCopy(tran);
					copy.BranchID = filter.BranchID;
					copy.EmployeeID = filter.EmployeeID;
					copy.Department = filter.Department;
					GLTransactions.Update(copy);
				}
			}
		}

		protected virtual void GLTranFilter_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			ClearInserted();
		}

		protected virtual void GLTranFilter_EmployeeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<GLTranFilter.branchID>(e.Row);
			sender.SetDefaultExt<GLTranFilter.department>(e.Row);
		}

		#endregion

		#region FAAccrualTran
		protected virtual void FAAccrualTran_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			FAAccrualTran row = (FAAccrualTran)e.Row;
			if (row == null || Filter.Current == null) return;

			row.BranchID = Filter.Current.BranchID;
			row.EmployeeID = Filter.Current.EmployeeID;
			row.Department = Filter.Current.Department;
		}

		protected virtual void FAAccrualTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FATransactions.Cache.AllowInsert = e.Row != null;
		}

		public static void SetCurrentRegister(PXSelect<FARegister> _Register, int BranchID)
		{
			FARegister curRegister = null;
			foreach (FARegister reg in _Register.Cache.Inserted)
			{
				if (reg.BranchID == BranchID)
				{
					curRegister = reg;
					break;
				}
			}
			if (curRegister != null)
			{
				_Register.Current = curRegister;
			}
			else
			{
				string tmpRefNbr = GetTempKey<FARegister.refNbr>(_Register.Cache);
				FARegister reg = _Register.Insert(new FARegister { BranchID = BranchID, Origin = FARegister.origin.Reconcilliation });
				reg.RefNbr = tmpRefNbr;
				_Register.Cache.Normalize();

			}
		}

		protected virtual void FAAccrualTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			FAAccrualTran tran = (FAAccrualTran)e.Row;
			if (tran == null) return;

			sender.RaiseExceptionHandling<FAAccrualTran.selectedAmt>(
				tran, 
				tran.SelectedAmt,
				tran.Selected == true && tran.ClassID != null && tran.SelectedAmt != null && tran.SelectedAmt > tran.GLTranAmt 
					? new PXSetPropertyException(CS.Messages.Entry_LE, tran.GLTranAmt) 
					: null);

			if (!sender.ObjectsEqual<FAAccrualTran.selected,
				FAAccrualTran.classID>(e.Row, e.OldRow))
			{
				if (tran.Selected == true && tran.ClassID != null)
				{
					decimal? qty = tran.OpenQty > 0 ? tran.OpenQty : 1m;
					for (int i = 0; i < qty; i++)
						FATransactions.Insert(new FATran());
				}
				else
				{
					foreach (FATran fatran in PXSelect<FATran, Where<FATran.gLtranID, Equal<Current<FAAccrualTran.tranID>>>>.Select(this)
						.RowCast<FATran>()
						.Where(fatran => FATransactions.Cache.GetStatus(fatran) == PXEntryStatus.Inserted))
					{
						FATransactions.Delete(fatran);
					}
				}
			}
			else if (!sender.ObjectsEqual<FAAccrualTran.branchID,
									FAAccrualTran.employeeID,
									FAAccrualTran.department>(e.Row, e.OldRow))
			{
				foreach (FATran fatran in PXSelect<FATran, Where<FATran.gLtranID, Equal<Current<FAAccrualTran.tranID>>>>.Select(this)
					.RowCast<FATran>()
					.Where(fatran => FATransactions.Cache.GetStatus(fatran) == PXEntryStatus.Inserted))
				{
					fatran.BranchID = tran.BranchID;
					fatran.EmployeeID = tran.EmployeeID;
					fatran.Department = tran.Department;
					FATransactions.Update(fatran);
				}
			}
		}

		protected virtual void FAAccrualTran_BranchID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null)
			{
				e.NewValue = ((FAAccrualTran)(e.Row)).BranchID;
			}
		}
		protected virtual void FAAccrualTran_EmployeeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null)
			{
				e.NewValue = ((FAAccrualTran)(e.Row)).EmployeeID;
			}
		}
		protected virtual void FAAccrualTran_Department_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null)
			{
				e.NewValue = ((FAAccrualTran)(e.Row)).Department;
			}
		}
		
		#endregion
		#region FADetails
		protected virtual void FixedAsset_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FixedAsset asset = (FixedAsset) e.Row;
			if(asset == null) return;

			_PersistedAssets[asset.AssetID] = asset;
		}


		protected virtual void FADetails_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FADetails det = (FADetails)e.Row;
			if (det != null && fasetup.Current.CopyTagFromAssetID == true && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				det.TagNbr = _PersistedAssets[det.AssetID].AssetCD;
			}
		}

		protected virtual void FASetup_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				FASetup setup = (FASetup)e.Row;
				if (setup.CopyTagFromAssetID == true)
				{
					setup.TagNumberingID = null;
				}
			}
		}

		#endregion

		#region FATran
		protected virtual void FATran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SetProcessDelegate();
	
			FATran tran = (FATran) e.Row;
			if(tran == null) return;

			PXUIFieldAttribute.SetEnabled<FATran.component>(sender, tran, tran.NewAsset ?? false);
			PXUIFieldAttribute.SetEnabled<FATran.newAsset>(sender, tran, !(tran.Component ?? false));
			PXUIFieldAttribute.SetEnabled<FATran.targetAssetID>(sender, tran, !(tran.NewAsset ?? false) || (tran.Component ?? false));
			PXUIFieldAttribute.SetEnabled<FATran.classID>(sender, tran, (tran.NewAsset ?? false) || (tran.Component ?? false));
			PXUIFieldAttribute.SetEnabled<FATran.branchID>(sender, tran, (tran.NewAsset ?? false) || (tran.Component ?? false));
			PXUIFieldAttribute.SetEnabled<FATran.employeeID>(sender, tran, (tran.NewAsset ?? false) || (tran.Component ?? false));
			PXUIFieldAttribute.SetEnabled<FATran.department>(sender, tran, (tran.NewAsset ?? false) || (tran.Component ?? false));
			PXUIFieldAttribute.SetEnabled<FATran.receiptDate>(sender, tran, tran.NewAsset == true);
			PXUIFieldAttribute.SetEnabled<FATran.deprFromDate>(sender, tran, tran.NewAsset == true);
			PXUIFieldAttribute.SetEnabled<FATran.qty>(sender, tran, tran.NewAsset == true);
			Numbering nbr = assetNumbering.Select();
			PXUIFieldAttribute.SetEnabled<FATran.assetCD>(sender, tran, nbr == null || nbr.UserNumbering == true);

		}

		protected virtual void FATran_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			FATran tran = (FATran)e.NewRow;
			if (tran == null) return;

			FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, tran.AssetID);
			GLTran gltran = PXSelect<GLTran, Where<GLTran.tranID, Equal<Required<FAAccrualTran.tranID>>>>.Select(this, tran.GLTranID);
			FAAccrualTran ext = PXSelect<FAAccrualTran, Where<FAAccrualTran.tranID, Equal<Required<FAAccrualTran.tranID>>>>.Select(this, tran.GLTranID);
			FixedAsset cls;
			if (tran.NewAsset == true)
			{
				cls = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, tran.ClassID);
			}
			else
			{
				FixedAsset target = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, tran.TargetAssetID);
				cls = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>.SelectSingleBound(this, new object[] { target });
			}

			if (asset == null || cls == null || gltran == null) return;

			if (tran.ClassID == null)
			{
				tran.ClassID = cls.AssetID;
			}

			if(!string.IsNullOrEmpty(tran.TranDesc))
			{
				gltran.TranDesc = tran.TranDesc;
			}

			bool hasError = false;
			foreach (string field in sender.Fields)
			{
				PXFieldState state = sender.GetStateExt(tran, field) as PXFieldState;
				if (state != null && !string.IsNullOrEmpty(state.Error))
					hasError = true;
			}

			tran.Selected = !hasError;

			if(sender.ObjectsEqual<FATran.newAsset, 
									FATran.component, 
									FATran.classID, 
									FATran.targetAssetID, 
									FATran.branchID,
									FATran.employeeID,
									FATran.department,
									FATran.qty>(e.NewRow, e.Row) &&
				sender.ObjectsEqual<FATran.receiptDate,
									FATran.deprFromDate,
									FATran.tranDesc,
									FATran.tranAmt,
									FATran.tranDate,
									FATran.finPeriodID,
									FATran.assetCD>(e.NewRow, e.Row)) return;

			gltran.TranDesc = tran.TranDesc;
			bool isNew = Assets.Cache.GetStatus(asset) == PXEntryStatus.Inserted;
			if (tran.NewAsset == true) // new asset (default) or new component
			{
				if (isNew)
				{
					DeleteAsset(asset);
				}
				int? assetID = tran.AssetID;
				try
				{
					InsertAsset(
						this,
						tran.ClassID,
						tran.Component == true ? tran.TargetAssetID : null,
						tran.AssetCD,
						cls.AssetTypeID,
						tran.ReceiptDate ?? gltran.TranDate,
						tran.DeprFromDate ?? gltran.TranDate,
						isNew ? tran.TranAmt : ext.UnitCost,
						cls.UsefulLife,
						tran.Qty,
						gltran,
						tran,
						out assetID);
				}
				catch (PXException exc)
				{
					this.Caches<Error>().Clear();
					this.Caches<Error>().Insert(new Error
					{
						ErrorMessage = exc.MessageNoPrefix,
						GLTranID = ext?.GLTranID
					});
					throw;
				}
				finally
				{
					tran.AssetID = assetID;
					tran.TargetAssetID = tran.Component == true ? tran.TargetAssetID : null;
				}
				this.Caches<Error>().Clear();
			}
			else // existing asset
			{
				if (tran.TargetAssetID != null)
				{
					if (isNew)
					{
						DeleteAsset(asset);
					}
					tran.AssetID = tran.TargetAssetID;

					FABookBalance bal = PXSelectJoin<FABookBalance, 
											LeftJoin<FABook, On<FABook.bookID, Equal<FABookBalance.bookID>>>,
											Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>,
												And<FABook.updateGL, Equal<True>>>>.SelectSingleBound(this, new object[]{asset});
						
					FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FADetails.assetID>>>>.Select(this, tran.AssetID);
					FALocationHistory loc = PXSelect<FALocationHistory, Where<FALocationHistory.assetID, Equal<Current<FADetails.assetID>>, And<FALocationHistory.revisionID, Equal<Current<FADetails.locationRevID>>>>>.SelectSingleBound(this, new object[]{det});
                    tran.EmployeeID = loc.EmployeeID;
					tran.Department = loc.Department;

					if (bal != null && string.IsNullOrEmpty(bal.InitPeriod))
					{
						tran.TranAmt = det.AcquisitionCost;
					}
				}
			}
			sender.SetDefaultExt<FATran.bookID>(tran);
			sender.SetDefaultExt<FATran.finPeriodID>(tran);
			sender.SetDefaultExt<FATran.debitAccountID>(tran);
			sender.SetDefaultExt<FATran.debitSubID>(tran);

			SetProcessDelegate();
		}

		protected virtual void FATran_AssetCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FATran tran = (FATran) e.Row;
			if(tran == null) return;

			Numbering nbr = PXSelectJoin<Numbering, InnerJoin<FASetup, On<FASetup.assetNumberingID, Equal<Numbering.numberingID>>>>.Select(this);
			if (tran.NewAsset == true && (nbr == null || nbr.UserNumbering == true) && string.IsNullOrEmpty(e.NewValue as string))
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(FATran.assetCD).Name);

			FixedAsset fixedAsset = PXSelectReadonly<
				FixedAsset,
				Where<FixedAsset.assetCD, Equal<Required<FixedAsset.assetCD>>>>
				.Select(this, e.NewValue);
			if (fixedAsset != null)
			{
				throw new PXSetPropertyException(Messages.AssetIDAlreadyExists);
			}
		}

		protected virtual void FATran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			FATran tran = (FATran)e.Row;
			if (tran == null) return;

			FAAccrualTran ext = GLTransactions.Current;
			if (ext == null)
			{
				throw new PXException(Messages.GLTranNotSelected);
			}

			if (Register.Current == null)
			{
				SetCurrentRegister(Register, (int)Filter.Current.BranchID);
			}

			ext.Selected = true;

			GLTran gltran = PXSelect<GLTran, Where<GLTran.tranID, Equal<Current<FAAccrualTran.tranID>>>>.SelectSingleBound(this, new[] { ext });
			FixedAsset cls = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FAAccrualTran.classID>>>>.Select(this, tran.ClassID ?? ext.ClassID);

			decimal tranAmt = tran.TranAmt ?? GetGLRemainder(ext);

			int? assetID = tran.AssetID;
			try
			{
				FixedAsset asset = InsertAsset(
					this,
					cls?.AssetID,
					null,
					tran.AssetCD,
					cls?.AssetTypeID,
					gltran.TranDate,
					gltran.TranDate,
					tranAmt,
					cls?.UsefulLife,
					tran.Qty,
					gltran,
					new FALocationHistory
					{
						BranchID = tran.BranchID ?? ext.BranchID,
						EmployeeID = tran.EmployeeID ?? ext.EmployeeID,
						Department = tran.Department ?? ext.Department
					},
					out assetID);
			}
			finally
			{
				tran.AssetID = assetID;
			}
			if (ext.Selected == false)
			{
				e.Cancel = true;
				return;
			}
			sender.SetDefaultExt<FATran.bookID>(tran);
			tran.TranDate = gltran.TranDate;
			tran.ReceiptDate = gltran.TranDate;
			tran.DeprFromDate = tran.ReceiptDate;
			sender.SetDefaultExt<FATran.finPeriodID>(tran);
			tran.TranAmt = tranAmt;
			tran.GLTranID = gltran.TranID;
			tran.CreditAccountID = gltran.AccountID;
			tran.CreditSubID = gltran.SubID;
			tran.TranDesc = gltran.TranDesc;
			tran.Origin = Register.Current.Origin;
			if (cls != null)
			{
				tran.ClassID = cls.AssetID;
				sender.SetDefaultExt<FATran.debitAccountID>(tran);
				sender.SetDefaultExt<FATran.debitSubID>(tran);
			}
		}

		protected virtual void FATran_BookID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void FATran_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void FATran_TranDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<FATran.finPeriodID>(e.Row);
		}

		protected virtual void FATran_ReceiptDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FATran transaction = (FATran)e.Row;
			if (transaction == null) return;

			sender.SetDefaultExt<FATran.deprFromDate>(transaction);
			transaction.TranDate = transaction.ReceiptDate;
		}

		protected virtual void FATran_DeprFromDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FATran transaction = (FATran)e.Row;

			if (transaction?.ReceiptDate != null && e.NewValue != null && transaction.ReceiptDate.Value.CompareTo(e.NewValue) > 0)
			{
				throw new PXSetPropertyException(Messages.PlacedInServiceDateIsEarlierThanReceiptDate,
					PXUIFieldAttribute.GetDisplayName<FATran.deprFromDate>(sender),
					PXUIFieldAttribute.GetDisplayName<FATran.receiptDate>(sender));
			}
		}

		protected virtual void _(Events.FieldVerifying<FATran.tranDate> e)
		{
			FATran transaction = ((FATran)e.Row);
			if (e.NewValue == null || transaction == null) return;

			string tranPeriodID = FABookPeriodRepository.GetFABookPeriodIDOfDate((DateTime?)e.NewValue, transaction.BookID, transaction.AssetID);
			string receiptDatePeriodID = FABookPeriodRepository.GetFABookPeriodIDOfDate(transaction.ReceiptDate, transaction.BookID, transaction.AssetID);
			string deprFromDatePeriodID = FABookPeriodRepository.GetFABookPeriodIDOfDate(transaction.DeprFromDate, transaction.BookID, transaction.AssetID);

			FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.Select(this, transaction.BookID);

			if(string.IsNullOrEmpty(tranPeriodID))
			{
				throw new PXSetPropertyException(Messages.FABookPeriodsNotDefinedForDate, book.BookCode ?? (object)transaction.BookID, (DateTime?)e.NewValue);
			}
			if (string.IsNullOrEmpty(receiptDatePeriodID) && transaction.ReceiptDate != null)
			{
				throw new PXSetPropertyException(Messages.FABookPeriodsNotDefinedForDate, book.BookCode ?? (object)transaction.BookID, transaction.ReceiptDate);
			}
			if (string.IsNullOrEmpty(deprFromDatePeriodID) && transaction.DeprFromDate != null)
			{
				throw new PXSetPropertyException(Messages.FABookPeriodsNotDefinedForDate, book.BookCode ?? (object)transaction.BookID, transaction.DeprFromDate);
			}

			if((string.CompareOrdinal(receiptDatePeriodID, deprFromDatePeriodID) < 0 && string.CompareOrdinal(tranPeriodID, receiptDatePeriodID) < 0)
				||(string.CompareOrdinal(receiptDatePeriodID, deprFromDatePeriodID) >= 0 && string.CompareOrdinal(tranPeriodID, deprFromDatePeriodID) < 0))
			{
				//hack for not to cancel row_updated and set the red error point to the field
				PXUIFieldAttribute.SetVisible<APTran.tranDate>(e.Cache, null);

				throw new PXSetPropertyException(
					Messages.IncorrectPurchasingPeriod,
					FABookPeriodIDAttribute.FormatForError(tranPeriodID),
					PXUIFieldAttribute.GetDisplayName<FATran.receiptDate>(e.Cache),
					FABookPeriodIDAttribute.FormatForError(receiptDatePeriodID));
			}

			if (transaction.DeprFromDate.Value.CompareTo(e.NewValue) < 0
				|| transaction.ReceiptDate.Value.CompareTo(e.NewValue) > 0)
			{
				//hack for not to cancel row_updated and set the red error point to the field
				PXUIFieldAttribute.SetVisible<APTran.tranDate>(e.Cache, null);

				throw new PXSetPropertyException(Messages.IncorrectPurchasingDate,
					PXUIFieldAttribute.GetDisplayName<FATran.tranDate>(e.Cache),
					PXUIFieldAttribute.GetDisplayName<FATran.receiptDate>(e.Cache),
					PXUIFieldAttribute.GetDisplayName<FATran.deprFromDate>(e.Cache));
			}
		}

		protected virtual void FATran_NewAsset_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FATran tran = (FATran) e.Row;
			if(tran == null) return;

			object assetID = tran.TargetAssetID;
			try
			{
				sender.RaiseFieldVerifying<FATran.targetAssetID>(tran, ref assetID);
			}
			catch(PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling<FATran.targetAssetID>(tran, assetID, ex);
			}
		}

		protected virtual void FATran_TargetAssetID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FATran row = (FATran)e.Row;
			if (e.NewValue == null && row.NewAsset != true)
			{
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
					PXUIFieldAttribute.GetDisplayName<FATran.targetAssetID>(sender));
			}

			FATran unreleasedTran = PXSelectReadonly<
				FATran,
				Where<FATran.assetID, Equal<Required<FATran.assetID>>,
					And<FATran.released, NotEqual<True>>>>
				.Select(this, e.NewValue);
			if (unreleasedTran != null)
			{
				FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FATran.assetID>>>>.Select(this, e.NewValue);
				e.NewValue = asset.AssetCD;

				throw new PXSetPropertyException(Messages.AssetHasUnreleasedTran);
			}
		}

		protected virtual void FATran_ClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null)
			{
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
					PXUIFieldAttribute.GetDisplayName<FATran.classID>(sender));
			}
		}

		protected virtual void FATran_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			FATran tran = (FATran) e.Row;
			if (tran?.TranDate == null || tran.BookID == null) return;

			e.NewValue = FABookPeriodIDAttribute.FormatPeriod(FABookPeriodRepository.GetFABookPeriodIDOfDate(tran.TranDate, tran.BookID, tran.AssetID));

		}

		protected virtual void FATran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			FATran tran = (FATran)e.Row;
			if (tran == null) return;

			FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, tran.AssetID);
			DeleteAsset(asset);
		}
		#endregion

		#endregion

		#region Functions
		private const string keyPrefix = "*##@";

		public static string GetTempKey<Field>(PXCache cache) where Field : IBqlField
		{
			int key = (cache.Inserted
				.Cast<object>()
				.Select(inserted => Convert.ToInt32(((string) cache.GetValue<Field>(inserted)).Substring(4))))
				.Concat(new[] {0})
				.Max();
			return $"{keyPrefix}{Convert.ToString(++key)}";
		}

		public static bool IsTempKey(string key)
		{
			return key != null && key.StartsWith(keyPrefix);
		}

		public static FixedAsset InsertAsset(
			PXGraph graph, 
			int? _classID, 
			int? _parentID, 
			string _assetCD, 
			string _assetTypeID,
			DateTime? _recDate, 
			DateTime? _deprFromDate, 
			decimal? _cost, 
			decimal? _usefulLife, 
			decimal? _qty, 
			GLTran _gltran, 
			IFALocation _loc, 
			out int? assetID)
		{
			if (_assetCD == null)
			{
				_assetCD = GetTempKey<FixedAsset.assetCD>(graph.Caches[typeof(FixedAsset)]);
			}

			FixedAsset asset = null;
			FALocationHistory location = null;
			FADetails det = null;
			assetID = null;

			try
			{
				asset = (FixedAsset)graph.Caches[typeof(FixedAsset)]
					.Insert(new FixedAsset
					{
						BranchID = _loc.BranchID,
						ClassID = _classID,
						ParentAssetID = _parentID,
						RecordType = FARecordType.AssetType,
						AssetTypeID = _assetTypeID,
						UsefulLife = _usefulLife,
						Description = _gltran != null ? _gltran.TranDesc : string.Empty,
						Qty = _qty
					});
				asset.AssetCD = _assetCD;
				graph.Caches[typeof(FixedAsset)].Normalize();
				assetID = asset.AssetID;

				location = (FALocationHistory)graph.Caches[typeof(FALocationHistory)]
				.Insert(new FALocationHistory
				{
					AssetID = asset.AssetID,
					BranchID = _loc.BranchID,
					EmployeeID = _loc.EmployeeID,
					Department = _loc.Department,
					TransactionDate = _recDate
				});
				int? revID = location.RevisionID;

				APTran aptran = PXSelect<APTran, Where<APTran.refNbr, Equal<Current<GLTran.refNbr>>,
					And<APTran.lineNbr, Equal<Current<GLTran.tranLineNbr>>,
				And<APTran.tranType, Equal<Current<GLTran.tranType>>>>>>.SelectSingleBound(graph, new object[]{_gltran});

				det = (FADetails)graph.Caches[typeof(FADetails)]
					.Insert(new FADetails
					{
						AssetID = asset.AssetID,
						ReceiptDate = _recDate,
						DepreciateFromDate = _deprFromDate,
						AcquisitionCost = _cost,
						LocationRevID = revID,
						BillNumber = _gltran?.RefNbr,
						PONumber = aptran?.PONbr,
						ReceiptNbr = aptran?.ReceiptNbr,
						ReceiptType = aptran?.ReceiptType,
					});
			}
			catch (PXException ex)
			{
				FAAccrualTran currentFAAccrualTran = (FAAccrualTran)graph.Caches[typeof(FAAccrualTran)].Current;
				if (currentFAAccrualTran != null)
				{
					graph.Caches<FAAccrualTran>().RaiseExceptionHandling(null, currentFAAccrualTran, null,
						new PXSetPropertyException<FAAccrualTran.selected>(ex.Message, PXErrorLevel.RowError));

					currentFAAccrualTran.Selected = false;
				}

				FATran currentFATran = (FATran)graph.Caches[typeof(FATran)].Current;
				if (currentFATran != null)
				{
					object fieldValue = (graph.Caches<FATran>().GetStateExt<FATran.assetID>(currentFATran) as PXFieldState)?.Value;
					graph.Caches<FATran>().RaiseExceptionHandling<FATran.assetID>(currentFATran, fieldValue,
						new PXSetPropertyException<FATran.assetID>(ex.Message));

					currentFATran.Selected = false;
				}
			}

			int? postingBookID = null;
			try
			{
				foreach (FABookBalance bal in PXSelect<FABookSettings,
						Where<FABookSettings.assetID, Equal<Current<FixedAsset.classID>>>,
						OrderBy<Desc<FABookSettings.updateGL>>>.Select(graph)
					.RowCast<FABookSettings>()
					.Select(sett => (FABookBalance)graph.Caches[typeof(FABookBalance)]
					.Insert(new FABookBalance
					{
						AssetID = asset.AssetID,
						ClassID = _classID,
						BookID = sett.BookID,
						UsefulLife = sett.UsefulLife,
					})))
				{
					if (string.IsNullOrEmpty(location.PeriodID))
					{
						location.PeriodID = bal.DeprFromPeriod;
					}
				if(postingBookID == null || bal.UpdateGL == true)
					{
						postingBookID = bal.BookID;
					}
				}
			}
			catch (PXException ex)
			{
				FAAccrualTran currentFAAccrualTran = (FAAccrualTran)graph.Caches[typeof(FAAccrualTran)].Current;
				if (currentFAAccrualTran != null)
				{
					graph.Caches<FAAccrualTran>().RaiseExceptionHandling(null, currentFAAccrualTran, null,
						new PXSetPropertyException<FAAccrualTran.selected>(ex.Message, PXErrorLevel.RowError));

					currentFAAccrualTran.Selected = false;
				}

				FATran currentFATran = (FATran)graph.Caches[typeof(FATran)].Current;
				if (currentFATran != null)
				{
					object fieldValue = (graph.Caches<FATran>().GetStateExt<FATran.classID>(currentFATran) as PXFieldState)?.Value;
					graph.Caches<FATran>().RaiseExceptionHandling<FATran.classID>(currentFATran, fieldValue,
						new PXSetPropertyException<FATran.classID>(ex.Message));

					currentFATran.Selected = false;
				}
			}

			if (asset == null || location == null || det == null)
				return null;

			asset.FASubID = AssetMaint.MakeSubID<FixedAsset.fASubMask, FixedAsset.fASubID>(graph.Caches[typeof (FixedAsset)], asset);
			asset.AccumulatedDepreciationSubID = AssetMaint.MakeSubID<FixedAsset.accumDeprSubMask, FixedAsset.accumulatedDepreciationSubID>(graph.Caches[typeof(FixedAsset)], asset);
			asset.DepreciatedExpenseSubID = AssetMaint.MakeSubID<FixedAsset.deprExpenceSubMask, FixedAsset.depreciatedExpenseSubID>(graph.Caches[typeof(FixedAsset)], asset);
			asset.DisposalSubID = AssetMaint.MakeSubID<FixedAsset.proceedsSubMask, FixedAsset.disposalSubID>(graph.Caches[typeof(FixedAsset)], asset);
			asset.GainSubID = AssetMaint.MakeSubID<FixedAsset.gainLossSubMask, FixedAsset.gainSubID>(graph.Caches[typeof(FixedAsset)], asset);
			asset.LossSubID = AssetMaint.MakeSubID<FixedAsset.gainLossSubMask, FixedAsset.lossSubID>(graph.Caches[typeof(FixedAsset)], asset);

			location.ClassID = asset.ClassID;
			location.FAAccountID = asset.FAAccountID;
			location.FASubID = asset.FASubID;
			location.AccumulatedDepreciationAccountID = asset.AccumulatedDepreciationAccountID;
			location.AccumulatedDepreciationSubID = asset.AccumulatedDepreciationSubID;
			location.DepreciatedExpenseAccountID = asset.DepreciatedExpenseAccountID;
			location.DepreciatedExpenseSubID = asset.DepreciatedExpenseSubID;
			location.DisposalAccountID = asset.DisposalAccountID;
			location.DisposalSubID = asset.DisposalSubID;
			location.GainAcctID = asset.GainAcctID;
			location.GainSubID = asset.GainSubID;
			location.LossAcctID = asset.LossAcctID;
			location.LossSubID = asset.LossSubID;
			location.LocationID = asset.BranchID;
			location.TransactionDate = det.ReceiptDate;

			IFABookPeriodRepository fABookPeriodRepository = graph.GetService<IFABookPeriodRepository>();
			if (postingBookID != null)
			{
				location.PeriodID = fABookPeriodRepository.FindFABookPeriodOfDate(det.ReceiptDate, postingBookID, asset.AssetID)?.FinPeriodID;
			}

			return asset;
		}

		protected virtual void DeleteAsset(FixedAsset asset)
		{
			if(asset.AssetID < 0) Assets.Delete(asset);
		}


		protected virtual void ClearInserted()
		{
			FATransactions.Cache.Clear();
			Balances.Cache.Clear();
			Details.Cache.Clear();
			Locations.Cache.Clear();
			Assets.Cache.Clear();
		}

		protected virtual decimal GetGLRemainder(FAAccrualTran ex)
		{
			decimal remainder = ex.OpenAmt ?? 0m - ex.SelectedAmt ?? 0m;
			return remainder > 0m ? Math.Min(remainder, ex.UnitCost ?? 0m) : 0m;
		}
		#endregion

		#region Overrides


		public override void Persist()
		{
			foreach (Error err in this.Caches<Error>().Inserted
				.Cast<Error>()
				.Where(err => !err.ErrorMessage.IsNullOrEmpty()))
			{
				if(err.GLTranID != null)
				{
					FAAccrualTran errorItem = new FAAccrualTran { GLTranID = err.GLTranID };
					errorItem = (FAAccrualTran)this.Caches<FAAccrualTran>().Locate(errorItem);
					if(errorItem != null)
					{
						PXProcessing<FAAccrualTran>.SetCurrentItem(errorItem);
						PXProcessing<FAAccrualTran>.SetError(err.ErrorMessage);
					}
				}
				throw new PXException(err.ErrorMessage);
			}

			foreach (FATran tran in FATransactions.Cache.Inserted)
			{
				if (tran.Selected != true)
				{
					throw new PXException(ErrorMessages.SeveralItemsFailed);
				}

				if (tran.NewAsset != true && 
					tran.TargetAssetID != null)
				{
					try
					{
						AssetProcess.RestrictAdditonDeductionForCalcMethod(this, tran.TargetAssetID, FADepreciationMethod.depreciationMethod.AustralianPrimeCost);
						AssetProcess.RestrictAdditonDeductionForCalcMethod(this, tran.TargetAssetID, FADepreciationMethod.depreciationMethod.NewZealandStraightLine);
						AssetProcess.RestrictAdditonDeductionForCalcMethod(this, tran.TargetAssetID, FADepreciationMethod.depreciationMethod.NewZealandStraightLineEvenly);
					}
					catch (Exception ex)
					{
						PXProcessing<FAAccrualTran>.SetError(ex.Message);
						throw new PXException(ex.Message);
					}
				}

				try
				{
					object val = tran.AssetCD;
					FATransactions.Cache.RaiseFieldVerifying<FATran.assetCD>(tran, ref val);
				}
				catch (PXSetPropertyException)
				{
					throw new PXException(Messages.CannotCreateAsset);
				}
			}

			foreach (FixedAsset asset in Caches[typeof(FixedAsset)].Cached)
			{
				FATran tran = PXSelect<FATran, Where<FATran.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectSingleBound(this, new object[]{ asset });

				if (tran == null)
				{
					DeleteAsset(asset);
				}
			}

			foreach (FAAccrualTran ex in GLTransactions.Select())
			{
				PXProcessing<FAAccrualTran>.SetCurrentItem(ex);

				foreach (FATran tran in FATransactions.Select(ex.TranID))
				{
					object assetID = tran.TargetAssetID;
					FATransactions.Cache.RaiseFieldVerifying<FATran.targetAssetID>(tran, ref assetID);
					object classID = tran.ClassID;
					FATransactions.Cache.RaiseFieldVerifying<FATran.classID>(tran, ref classID);
				}

				if (ex.Selected == true && ex.SelectedAmt > ex.GLTranAmt)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ex.GLTranAmt);
				}
			}

			foreach (FixedAsset asset in Caches[typeof(FixedAsset)].Inserted)
			{
				FABookBalance postingBookBalance = SelectFrom<FABookBalance>
					.Where<FABookBalance.assetID.IsEqual<@P.AsInt>>
					.OrderBy<Asc<FABookBalance.updateGL>>
					.View
					.SelectSingleBound(this, new object[] { }, asset.AssetID);

				FADetails assetdet = new FADetails { AssetID = asset.AssetID };
				assetdet = Details.Locate(assetdet);
				if (assetdet != null)
				{
					FALocationHistory newhist = new FALocationHistory { AssetID = assetdet.AssetID, RevisionID = assetdet.LocationRevID };
					newhist = Locations.Locate(newhist);
					if (newhist != null)
					{
						newhist.FAAccountID = asset.FAAccountID;
						newhist.FASubID = asset.FASubID;
						newhist.AccumulatedDepreciationAccountID = asset.AccumulatedDepreciationAccountID;
						newhist.AccumulatedDepreciationSubID = asset.AccumulatedDepreciationSubID;
						newhist.DepreciatedExpenseAccountID = asset.DepreciatedExpenseAccountID;
						newhist.DepreciatedExpenseSubID = asset.DepreciatedExpenseSubID;
						newhist.DisposalAccountID = asset.DisposalAccountID;
						newhist.DisposalSubID = asset.DisposalSubID;
						newhist.GainAcctID = asset.GainAcctID;
						newhist.GainSubID = asset.GainSubID;
						newhist.LossAcctID = asset.LossAcctID;
						newhist.LossSubID = asset.LossSubID;
						newhist.LocationID = asset.BranchID;
						newhist.TransactionDate = assetdet.ReceiptDate;
						newhist.PeriodID = FABookPeriodRepository.FindFABookPeriodOfDate(assetdet.ReceiptDate, postingBookBalance.BookID, asset.AssetID)?.FinPeriodID;
					}
				}
			}

			foreach (FAAccrualTran ext in GLTransactions.Select().RowCast<FAAccrualTran>().Where(ext => ext.Selected == true))
			{
				PXProcessing<FAAccrualTran>.SetCurrentItem(ext);

				foreach (FATran tran in FATransactions.Select(ext.TranID))
				{
					if (tran.NewAsset == true)
					{
						FABookBalance glbal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FATran.assetID>>,
							And<FABookBalance.bookID, Equal<Current<FATran.bookID>>>>>.SelectMultiBound(this, new object[] { tran });
						tran.TranPeriodID = glbal.DeprFromPeriod;
					}
					foreach (FABookBalance bal in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FATran.assetID>>, 
						And<FABookBalance.bookID, NotEqual<Current<FATran.bookID>>>>>.SelectMultiBound(this, new object[]{tran}))
					{
						FATran newtrn = (FATran)FATransactions.Cache.CreateCopy(tran);
						FATransactions.Cache.SetDefaultExt<FATran.noteID>(newtrn);
						newtrn.BookID = bal.BookID;
						newtrn.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<FATran.lineNbr>(FATransactions.Cache, Register.Current);
						FATransactions.Cache.SetDefaultExt<FATran.finPeriodID>(newtrn);
						if (tran.NewAsset == true)
						{
							newtrn.TranPeriodID = bal.DeprFromPeriod;
						}
						else
						{
							FATransactions.Cache.SetDefaultExt<FATran.tranPeriodID>(newtrn);
						}
						FATransactions.Cache.SetStatus(newtrn, PXEntryStatus.Inserted);
					}
				}
				ext.ClosedAmt = ext.GLTranAmt - ext.OpenAmt;
				ext.ClosedQty = ext.GLTranQty - ext.OpenQty;
				if(GLTransactions.Cache.GetStatus(ext) == PXEntryStatus.Notchanged)
				{
					GLTransactions.Cache.SetStatus(ext, PXEntryStatus.Updated);
				}
			}

			List<FATran> ptrans = new List<FATran>((IEnumerable<FATran>)FATransactions.Cache.Inserted);
			foreach (FATran tran in ptrans)
			{
				FATran reconTran = (FATran)FATransactions.Cache.CreateCopy(tran);
				FATransactions.Cache.SetDefaultExt<FATran.noteID>(reconTran);
				reconTran.TranType = FATran.tranType.ReconciliationPlus;
				reconTran.DebitAccountID = fasetup.Current.FAAccrualAcctID;
				reconTran.DebitSubID = fasetup.Current.FAAccrualSubID;
				reconTran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<FATran.lineNbr>(FATransactions.Cache, Register.Current);
				FATransactions.Cache.SetStatus(reconTran, PXEntryStatus.Inserted);

				tran.CreditAccountID = reconTran.DebitAccountID;
				tran.CreditSubID = reconTran.DebitSubID;
				tran.GLTranID = null;

				GLTran gltran = PXSelect<
					GLTran, 
					Where<GLTran.tranID, Equal<Current<FATran.gLtranID>>>>
					.SelectSingleBound(this, new object[] { reconTran });

				FABookBalance bal = PXSelect<
					FABookBalance, 
					Where<FABookBalance.assetID, Equal<Current<FATran.assetID>>, 
						And<FABookBalance.bookID, Equal<Current<FATran.bookID>>>>>
					.SelectSingleBound(this, new object[] { tran });

				FADepreciationMethod method = PXSelect<
					FADepreciationMethod, 
					Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>
					.Select(this, bal.DepreciationMethodID);

				FixedAsset asset = PXSelect<
					FixedAsset, 
					Where<FixedAsset.assetID, Equal<Current<FATran.assetID>>>>
					.SelectSingleBound(this, new object[] { tran });

				OrganizationFinPeriod period = 
					FABookPeriodUtils.GetNearestOpenOrganizationMappedFABookPeriodInSubledger<OrganizationFinPeriod.fAClosed>(
						bal.BookID,
						gltran.BranchID,
						gltran.FinPeriodID,
						tran.BranchID);

				reconTran.FinPeriodID = period?.FinPeriodID;

				if (reconTran.FinPeriodID == null)
				{
					FATransactions.Cache.SetDefaultExt<FATran.finPeriodID>(reconTran);
				}
				reconTran.TranPeriodID = bal.DeprFromPeriod;

				if (bal.UpdateGL != true)
					reconTran.GLTranID = null;

				if (bal.Status == FixedAssetStatus.FullyDepreciated && method.IsPureStraightLine)
				{
					FATran deprtran = (FATran)FATransactions.Cache.CreateCopy(tran);
					FATransactions.Cache.SetDefaultExt<FATran.noteID>(deprtran);
					deprtran.TranType = FATran.tranType.CalculatedPlus;
					deprtran.CreditAccountID = asset.AccumulatedDepreciationAccountID;
					deprtran.CreditSubID = asset.AccumulatedDepreciationSubID;
					deprtran.DebitAccountID = asset.DepreciatedExpenseAccountID;
					deprtran.DebitSubID = asset.DepreciatedExpenseSubID;
					deprtran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<FATran.lineNbr>(FATransactions.Cache, Register.Current);
					deprtran.GLTranID = null;
					FATransactions.Cache.SetStatus(deprtran, PXEntryStatus.Inserted);
				}
			}

			// segregate transactions by BranchID
			foreach (FATran tran in FATransactions.Cache.Inserted)
			{
				SetCurrentRegister(Register, (int)tran.BranchID);
				if (tran.NewAsset == true)
				{
					Register.Current.Origin = FARegister.origin.Purchasing;
				}
				if (tran.RefNbr != Register.Current.RefNbr)
				{
					tran.RefNbr = Register.Current.RefNbr;
					tran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<FATran.lineNbr>(FATransactions.Cache, Register.Current);
					FATransactions.Cache.Normalize();
				}
			}

			//Delete empty inserted documents
			List<FARegister> docs = new List<FARegister>((IEnumerable<FARegister>)Register.Cache.Inserted);
			for(int i = docs.Count - 1; i >= 0; --i)
			{
				FARegister doc = docs[i];
				FATran t = PXSelect<FATran, Where<FATran.refNbr, Equal<Current<FARegister.refNbr>>>>.SelectSingleBound(this, new object[]{doc});
				if (t == null)
				{
					Register.Delete(doc);
					docs.RemoveAt(i);
				}
			}

			DocumentList<Batch> batchlist = new DocumentList<Batch>(this);
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				var locations = new List<FALocationHistory>((IEnumerable<FALocationHistory>)Locations.Cache.Inserted);
				base.Persist();
				foreach(var loc in locations)
				{
					loc.RefNbr = Register.Current.RefNbr;
					Locations.Cache.MarkUpdated(loc);
				}

				foreach (FATran tran in ptrans)
				{
					FABookBalance bookbal = new FABookBalance { AssetID = tran.AssetID, BookID = tran.BookID };
					if ((bookbal = Balances.Locate(bookbal)) != null)
					{
						FABookHist hist = new FABookHist
						{
							AssetID = bookbal.AssetID,
							BookID = bookbal.BookID,
							FinPeriodID = bookbal.DeprFromPeriod
						};

						hist = bookhist.Insert(hist);

						if (string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && string.IsNullOrEmpty(bookbal.LastDeprPeriod))
						{
							bookbal.CurrDeprPeriod = bookbal.DeprFromPeriod;
							Balances.Update(bookbal);
						}
					}
				}
				base.Persist();

				if (fasetup.Current.AutoReleaseAsset == true)
				{
					SelectTimeStamp();
					batchlist = AssetTranRelease.ReleaseDoc(docs, false, false);
				}
				ts.Complete(this);
			}

			PostGraph pg = CreateInstance<PostGraph>();
			foreach (Batch batch in batchlist)
			{
				pg.Clear();
				pg.PostBatchProc(batch);
			}

		}
		#endregion


		#region DAC Overrides
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBLiteDefault(typeof(FARegister.refNbr), DefaultForUpdate = false)]
		[PXParent(typeof(Select<FARegister, Where<FARegister.refNbr, Equal<Current<FATran.refNbr>>>>))]
		[PXUIField(DisplayName = "Reference Number", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXSelector(typeof(FARegister.refNbr))]
		protected virtual void FATran_RefNbr_CacheAttached(PXCache sender)
		{
		}
		
		[PXDBInt]
		[PXDBLiteDefault(typeof(FixedAsset.assetID))]
		[PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.recordType, Equal<FARecordType.assetType>>>),
			SubstituteKey = typeof(FixedAsset.assetCD), DescriptionField = typeof(FixedAsset.description), DirtyRead = true)]
		[PXUIField(DisplayName = "Asset", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void FATran_AssetID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "New Asset ID")]
		protected virtual void FATran_AssetCD_CacheAttached(PXCache sender)
		{
		}

		[PXDBBaseCury]
		[PXUIField(DisplayName = "Transaction Amount")]
		[PXFormula(null, typeof(AddCalc<FAAccrualTran.selectedAmt>))]
		protected virtual void FATran_TranAmt_CacheAttached(PXCache sender)
		{
		}

		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "1.0")]
		[PXUIField(DisplayName = "Quantity")]
		//[PXMergeAttributes(Method = MergeMethod.Merge)] // Does not work. Waiting for fix by Alex Ignatin
		[PXFormula(null, typeof(AddCalc<FAAccrualTran.selectedQty>))]
		protected virtual void FATran_Qty_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), false)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visibility), PXUIVisibility.Visible)]
		protected virtual void FATran_TranDate_CacheAttached(PXCache sender)
		{
		}

		[PXUIField(DisplayName = "Tran. Period", Enabled = false)]
		[PeriodID]
		protected virtual void FATran_FinPeriodID_CacheAttached(PXCache sender)
		{
		}

		[PeriodID]
		[PXFormula(typeof(RowExt<FATran.finPeriodID>))]
		protected virtual void FATran_TranPeriodID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt]
		[PXDefault(typeof(Search<FABookBalance.bookID, Where<FABookBalance.assetID, Equal<Current<FATran.assetID>>>, OrderBy<Desc<FABookBalance.updateGL>>>))]
		[PXUIField(DisplayName = "Book", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void FATran_BookID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(typeof(Search<FixedAsset.fAAccountID, Where<FixedAsset.assetID, Equal<Current<FATran.assetID>>>>))]
		protected virtual void FATran_DebitAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(typeof(Search<FixedAsset.fASubID, Where<FixedAsset.assetID, Equal<Current<FATran.assetID>>>>))]
		protected virtual void FATran_DebitSubID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Transaction Type", Visibility = PXUIVisibility.Visible)]
		[PXDefault(FATran.tranType.PurchasingPlus)]
		[FATran.tranType.List]
		protected virtual void FATran_TranType_CacheAttached(PXCache sender)
		{
		}

		[PXInt]
		[PXSelector(typeof(Search2<FAClass.assetID,
			LeftJoin<FABookSettings, On<FAClass.assetID, Equal<FABookSettings.assetID>>,
			LeftJoin<FABook, On<FABookSettings.bookID, Equal<FABook.bookID>>>>,
			Where<FAClass.recordType, Equal<FARecordType.classType>,
			And<FABook.updateGL, Equal<True>>>>),
					SubstituteKey = typeof(FAClass.assetCD),
					DescriptionField = typeof(FAClass.description))]
		[PXDefault]
		[PXUIField(DisplayName = "Asset Class", Required = true)]
		protected virtual void FATran_ClassID_CacheAttached(PXCache sender)
		{
		}

		[PXInt]
		[PXSelector(typeof(Search2<FixedAsset.assetID,
			LeftJoin<FABookBalance, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
			LeftJoin<FABook, On<FABookBalance.bookID, Equal<FABook.bookID>>>>,
			Where<FixedAsset.recordType, Equal<FARecordType.assetType>,
			And<FABook.updateGL, Equal<True>>>>),
					SubstituteKey = typeof(FixedAsset.assetCD),
					DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Asset for Reconciliation")]
		protected virtual void FATran_TargetAssetID_CacheAttached(PXCache sender)
		{
		}

		[Branch(typeof(FAAccrualTran.branchID), Required = true)]
		protected virtual void FATran_BranchID_CacheAttached(PXCache sender)
		{
		}

		[PXInt]
		[PXSelector(typeof(EPEmployee.bAccountID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
        [PXDefault(typeof(FAAccrualTran.employeeID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Custodian")]
        protected virtual void FATran_EmployeeID_CacheAttached(PXCache sender)
		{
		}

		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(FAAccrualTran.department))]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department", Required = true)]
		protected virtual void FATran_Department_CacheAttached(PXCache sender)
		{
		}


		[Serializable]
		public partial class GLTran : GL.GLTran
		{
			#region BranchID
			[Branch(IsDetail = false, DisplayName = "Transaction Branch", Enabled = false)]
			public override Int32? BranchID
			{
				get
				{
					return this._BranchID;
				}
				set
				{
					this._BranchID = value;
				}
			}
			#endregion
		}

		#endregion

	}

	public class AdditionsFATran : PXGraph<AdditionsFATran>
	{
		#region Selects
		public PXSelect<FixedAsset> Assets;
		public PXSelect<FALocationHistory> Locations;
		public PXSelect<FADetails> Details;
		public PXSelect<FABookBalance> Balances;
		public PXSelect<FARegister> Register;
		public PXSelect<FAAccrualTran> Additions;
		public PXSelect<FATran> FATransactions;

		public PXSetup<FASetup> fasetup;
		public PXSetup<GLSetup> glsetup;
		public PXSetup<Company> company;

		#endregion

		#region Ctor
		public AdditionsFATran()
		{
			object setup = fasetup.Current;
			setup = glsetup.Current;
		}
		#endregion

		#region Funcs

		protected virtual void InsertFATransactions(FixedAsset asset, DateTime? date, Decimal? amt, GLTran gltran)
		{
			foreach (FABookBalance bal in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectMultiBound(this, new object[] { asset }))
			{
				FATran tran = new FATran
				{
					AssetID = asset.AssetID,
					BookID = bal.BookID,
					TranAmt = amt,
					TranDate = date,
					TranType = FATran.tranType.PurchasingPlus,
					CreditAccountID = asset.FAAccrualAcctID,
					CreditSubID = asset.FAAccrualSubID,
					DebitAccountID = asset.FAAccountID,
					DebitSubID = asset.FASubID,
					TranDesc = gltran.TranDesc
				};
				FATransactions.Insert(tran);
				
				tran = new FATran
				{
					AssetID = asset.AssetID,
					BookID = bal.BookID,
					TranAmt = amt,
					TranDate = date,
					GLTranID = bal.UpdateGL == true ? gltran.TranID : null,
					TranType = FATran.tranType.ReconciliationPlus,
					CreditAccountID = gltran.AccountID,
					CreditSubID = gltran.SubID,
					DebitAccountID = asset.FAAccrualAcctID,
					DebitSubID = asset.FAAccrualSubID,
					TranDesc = gltran.TranDesc
				};
				FATransactions.Insert(tran);
			}

		}

		public virtual void InsertNewComponent(FixedAsset parentAsset, FixedAsset cls, DateTime? date, decimal? amt, decimal? qty, IFALocation loc, AssetGLTransactions.GLTran gltran)
		{
			date = date ?? gltran.TranDate;
			int? assetID;
			FixedAsset comp = AssetGLTransactions.InsertAsset(this, cls.AssetID, parentAsset.AssetID, null, cls.AssetTypeID, date, date, amt, cls.UsefulLife, qty, gltran, loc, out assetID);
			if (Register.Current == null)
			{
				AssetGLTransactions.SetCurrentRegister(Register, (int)comp.BranchID);
				Register.Current.Origin = FARegister.origin.Purchasing;
			}
			InsertFATransactions(comp, date, amt, gltran);
		}
		#endregion

		#region Overrides
		public override void Persist()
		{
			base.Persist();
			if (fasetup.Current.AutoReleaseAsset == true && Register.Current != null)
			{
				SelectTimeStamp();
				AssetTranRelease.ReleaseDoc(new List<FARegister> { Register.Current }, false);
			}

		}
		#endregion

		#region Events
		protected virtual void FATran_AssetID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void FATran_BookID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion
	}


	public interface IFALocation
	{
		int? BranchID { get; set; }
		int? EmployeeID { get; set; }
		string Department { get; set; }
	}

	[Serializable]
	public partial class GLTranFilter : IBqlTable
	{
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(null, typeof(Search<Account.accountID,
				Where2<Match<Current<AccessInfo.userName>>,
				And<Account.active, Equal<True>,
				And2<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
				Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>,
				And<Where<Account.curyID, IsNull, Or<Account.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>>>>>),
				DisplayName = "Account", Visibility = PXUIVisibility.Visible, Filterable = false, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(FASetup.fAAccrualAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? AccountID
		{
			get
			{
				return _AccountID;
			}
			set
			{
				_AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, Filterable = true)]
		[PXDefault(typeof(FASetup.fAAccrualSubID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SubID
		{
			get
			{
				return _SubID;
			}
			set
			{
				_SubID = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(useDefaulting: false, IsDetail = false)]
		[PXDefault(typeof(Coalesce<
					Search2<Location.vBranchID, InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<Location.bAccountID>, And<EPEmployee.defLocationID, Equal<Location.locationID>>>>, Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>,
					Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>))]
		public virtual Int32? BranchID
		{
			get
			{
				return _BranchID;
			}
			set
			{
				_BranchID = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected int? _EmployeeID;
		[PXDBInt]
		[PXSelector(typeof(EPEmployee.bAccountID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Custodian")]
		public virtual int? EmployeeID
		{
			get
			{
				return _EmployeeID;
			}
			set
			{
				_EmployeeID = value;
			}
		}
		#endregion
		#region Department
		public abstract class department : PX.Data.BQL.BqlString.Field<department> { }
		protected String _Department;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<EPEmployee.departmentID, Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department")]
		public virtual String Department
		{
			get
			{
				return _Department;
			}
			set
			{
				_Department = value;
			}
		}
		#endregion

		#region AcquisitionCost
		public abstract class acquisitionCost : PX.Data.BQL.BqlDecimal.Field<acquisitionCost> { }
		protected Decimal? _AcquisitionCost;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Acquisition Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<FADetails.acquisitionCost, Where<FADetails.assetID, Equal<Current<FixedAsset.assetID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public Decimal? AcquisitionCost
		{
			get
			{
				return _AcquisitionCost;
			}
			set
			{
				_AcquisitionCost = value;
			}
		}
		#endregion
		#region CurrentCost
		public abstract class currentCost : PX.Data.BQL.BqlDecimal.Field<currentCost> { }
		protected Decimal? _CurrentCost;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Current Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<FABookHistory.ytdAcquired, LeftJoin<FABook, On<FABook.bookID, Equal<FABookHistory.bookID>>>, Where<FABookHistory.assetID, Equal<Current<FixedAsset.assetID>>>, OrderBy<Desc<FABook.updateGL, Desc<FABookHistory.finPeriodID>>>>))]
		public Decimal? CurrentCost
		{
			get
			{
				return _CurrentCost;
			}
			set
			{
				_CurrentCost = value;
			}
		}
		#endregion
		#region AccrualBalance
		public abstract class accrualBalance : PX.Data.BQL.BqlDecimal.Field<accrualBalance> { }
		protected Decimal? _AccrualBalance;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Accrual Balance", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<FABookHistory.ytdReconciled, 
			Where<FABookHistory.assetID, Equal<Current<FixedAsset.assetID>>>, 
			OrderBy<Desc<FABookHistory.finPeriodID>>>))]
		public Decimal? AccrualBalance
		{
			get
			{
				return _AccrualBalance;
			}
			set
			{
				_AccrualBalance = value;
			}
		}
		#endregion
		#region UnreconciledAmt
		public abstract class unreconciledAmt : PX.Data.BQL.BqlDecimal.Field<unreconciledAmt> { }
		protected Decimal? _UnreconciledAmt;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Unreconciled Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public Decimal? UnreconciledAmt
		{
			get
			{
				return _UnreconciledAmt;
			}
			set
			{
				_UnreconciledAmt = value;
			}
		}
		#endregion
		#region SelectionAmt
		public abstract class selectionAmt : PX.Data.BQL.BqlDecimal.Field<selectionAmt> { }
		protected Decimal? _SelectionAmt;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Selection Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public Decimal? SelectionAmt
		{
			get
			{
				return _SelectionAmt;
			}
			set
			{
				_SelectionAmt = value;
			}
		}
		#endregion		
		#region ExpectedCost
		public abstract class expectedCost : PX.Data.BQL.BqlDecimal.Field<expectedCost> { }
		protected Decimal? _ExpectedCost;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Expected Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<FABookHistory.ytdAcquired, 
			LeftJoin<FABook, On<FABook.bookID, Equal<FABookHistory.bookID>>>, 
			Where<FABookHistory.assetID, Equal<Current<FixedAsset.assetID>>, 
				And<FABook.updateGL, Equal<True>>>, 
			OrderBy<Desc<FABookHistory.finPeriodID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public Decimal? ExpectedCost
		{
			get
			{
				return _ExpectedCost;
			}
			set
			{
				_ExpectedCost = value;
			}
		}
		#endregion		
		#region ExpectedAccrualBal
		public abstract class expectedAccrualBal : PX.Data.BQL.BqlDecimal.Field<expectedAccrualBal> { }
		protected Decimal? _ExpectedAccrualBal;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Expected Accrual Balance", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<FABookHistory.ytdReconciled, 
			LeftJoin<FABook, On<FABook.bookID, Equal<FABookHistory.bookID>>>, 
			Where<FABookHistory.assetID, Equal<Current<FixedAsset.assetID>>, 
				And<FABook.updateGL, Equal<True>>>, 
			OrderBy<Desc<FABookHistory.finPeriodID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public Decimal? ExpectedAccrualBal
		{
			get
			{
				return _ExpectedAccrualBal;
			}
			set
			{
				_ExpectedAccrualBal = value;
			}
		}
		#endregion		
		#region ReconType
		public abstract class reconType : PX.Data.BQL.BqlString.Field<reconType>
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Addition, Deduction },
					new string[] { Messages.Addition, Messages.Deduction }) { }
			}

			public const string Addition = "+";
			public const string Deduction = "-";

			public class addition : PX.Data.BQL.BqlString.Constant<addition>
			{
				public addition() : base(Addition) { ;}
			}
			public class deduction : PX.Data.BQL.BqlString.Constant<deduction>
			{
				public deduction() : base(Deduction) { ;}
			}
			#endregion
		}
		protected String _ReconType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(reconType.Addition)]
		[PXUIField(DisplayName = "Reconciliation Type")]
		[reconType.List]
		public virtual String ReconType
		{
			get
			{
				return this._ReconType;
			}
			set
			{
				this._ReconType = value;
			}
		}
		#endregion

		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate]
		[PXDefault(typeof(Search<FADetails.depreciateFromDate, Where<FADetails.assetID, Equal<Current<FixedAsset.assetID>>>>))]
		[PXUIField(DisplayName = "Tran. Date")]
		public virtual DateTime? TranDate
		{
			get
			{
				return _TranDate;
			}
			set
			{
				_TranDate = value;
			}
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
		protected string _PeriodID;
		[PXUIField(DisplayName = "Addition Period")]
		[FinPeriodSelector(
			searchType: null,
			sourceType: typeof(GLTranFilter.tranDate),
			branchSourceType: typeof(FixedAsset.branchID))]
		public virtual string PeriodID
		{
			get
			{
				return _PeriodID;
			}
			set
			{
				_PeriodID = value;
			}
		}
		#endregion
		#region ShowReconciled
		public abstract class showReconciled : PX.Data.BQL.BqlBool.Field<showReconciled> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Show Transactions Marked as Reconciled")]
		[PXDefault(false)]
		public bool? ShowReconciled { get; set; }
		#endregion
	}
}
