using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.FA
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class FixedAssetCostEnq : PXGraph<FixedAssetCostEnq>
	{
		public PXCancel<FixedAssetFilter> Cancel;
		public PXFilter<FixedAssetFilter> Filter;
		public PXSelect<Amounts> Amts;

		public FixedAssetCostEnq()
		{
			Amts.Cache.AllowInsert = false;
			Amts.Cache.AllowUpdate = false;
			Amts.Cache.AllowDelete = false;
		}

		protected virtual bool IsAccrualAccount<TAccountID, TSubID>(FATran tran, FixedAsset asset)
			where TAccountID: IBqlField
			where TSubID : IBqlField
		{
			return (int?)this.Caches<FATran>().GetValue<TAccountID>(tran) == asset.FAAccrualAcctID &&
				(int?)this.Caches<FATran>().GetValue<TSubID>(tran) == asset.FAAccrualSubID;
		}

		public virtual IEnumerable amts(PXAdapter adapter)
		{
			FixedAssetFilter filter = Filter.Current;

			if (filter == null || filter.AssetID == null || filter.PeriodID == null)
				yield break;

			FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAssetFilter.assetID>>>>.Select(this);

			PXSelectBase<FATran> select = new PXSelectJoin<FATran, 
				LeftJoin<Account, On<FATran.debitAccountID, Equal<Account.accountID>>, 
				LeftJoin<Sub, On<FATran.debitSubID, Equal<Sub.subID>>, 
				LeftJoin<CreditAccount, On<FATran.creditAccountID, Equal<CreditAccount.accountID>>, 
				LeftJoin<CreditSub, On<FATran.creditSubID, Equal<CreditSub.subID>>,
				LeftJoin<Branch, On<FATran.branchID, Equal<Branch.branchID>>,
				LeftJoin<FABook, On<FATran.bookID, Equal<FABook.bookID>>,
				LeftJoin<FABookPeriod, On<FATran.bookID, Equal<FABookPeriod.bookID>, 
					And<FABookPeriod.organizationID, Equal<IIf<Where<FABook.updateGL, Equal<True>>, Branch.organizationID, FinPeriod.organizationID.masterValue>>,
					And<FATran.finPeriodID, Equal<FABookPeriod.finPeriodID>>>>>>>>>>>, 
				Where<FATran.assetID, Equal<Current<FixedAssetFilter.assetID>>, 
					And<FATran.finPeriodID, LessEqual<Current<FixedAssetFilter.periodID>>, 
					And<FATran.released, Equal<True>, 
				And<Where<FATran.tranType, NotEqual<FATran.tranType.calculatedPlus>,
					And<FATran.tranType, NotEqual<FATran.tranType.calculatedMinus>,
					And<FATran.tranType, NotEqual<FATran.tranType.reconcilliationPlus>,
					And<FATran.tranType, NotEqual<FATran.tranType.reconcilliationMinus>>>>>>>>>>(this);

			if (filter.BookID != null)
			{
				select.WhereAnd<Where<FATran.bookID, Equal<Current<FixedAssetFilter.bookID>>>>();
			}

			Dictionary<string, Amounts> dict = new Dictionary<string, Amounts>();
			foreach (PXResult<FATran, Account, Sub, CreditAccount, CreditSub, Branch, FABook, FABookPeriod> res in select.Select())
			{
				Account dacct = res;
				Sub dsub = res;
				CreditAccount cacct = res;
				CreditSub csub = res;
				FATran tran = res;
				FABookPeriod period = res;

				if (!IsAccrualAccount<FATran.debitAccountID, FATran.debitSubID>(tran, asset))
				{
					Amounts record = null;
					string dkey = string.Format("{0}{1}", dacct.AccountCD, dsub.SubCD);
					if (!dict.TryGetValue(dkey, out record))
					{
						record = new Amounts
						{
							BookID = tran.BookID,
							AccountID = tran.DebitAccountID,
							SubID = tran.DebitSubID,
							AcctDescr = dacct.Description,
							SubDescr = dsub.Description,
							BranchID = tran.BranchID,
							ItdAmt = 0m,
							YtdAmt = 0m,
							PtdAmt = 0m
						};
					}
					record.ItdAmt += tran.TranAmt;
					if (filter.PeriodID.Substring(0, 4) == period.FinYear)
					{
						record.YtdAmt += tran.TranAmt;
					}
					if (filter.PeriodID == tran.FinPeriodID)
					{
						record.PtdAmt += tran.TranAmt;
					}
					dict[dkey] = record;
				}

				if (!IsAccrualAccount<FATran.creditAccountID, FATran.creditSubID>(tran, asset))
				{
					Amounts record = null;
					string ckey = string.Format("{0}{1}", cacct.AccountCD, csub.SubCD);
					if (!dict.TryGetValue(ckey, out record))
					{
						record = new Amounts
						{
							BookID = tran.BookID,
							AccountID = tran.CreditAccountID,
							SubID = tran.CreditSubID,
							AcctDescr = cacct.Description,
							SubDescr = csub.Description,
							BranchID = tran.BranchID,
							ItdAmt = 0m,
							YtdAmt = 0m,
							PtdAmt = 0m
						};
					}
					record.ItdAmt -= tran.TranAmt;
					if (filter.PeriodID.Substring(0, 4) == period.FinYear)
					{
						record.YtdAmt -= tran.TranAmt;
					}
					if (filter.PeriodID == tran.FinPeriodID)
					{
						record.PtdAmt -= tran.TranAmt;
					}
					dict[ckey] = record;
				}
			}

			foreach (Amounts amt in dict.Values)
			{
				if (amt.ItdAmt != 0m || amt.YtdAmt != 0m || amt.PtdAmt != 0m)
					yield return amt;
			}
		}

		public PXAction<FixedAssetFilter> viewDetails;
		[PXUIField(DisplayName = "Asset Transaction History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			Amounts amt = Amts.Current;
			FixedAssetFilter filter = Filter.Current;
			if (amt != null && filter != null)
			{
				FACostDetailsInq graph = CreateInstance<FACostDetailsInq>();
				graph.Filter.Current = new AccountFilter() 
				{ 
					AssetID = filter.AssetID, 
					EndPeriodID = filter.PeriodID, 
					AccountID = amt.AccountID, 
					SubID = amt.SubID,
				};
				graph.Filter.Insert(graph.Filter.Current);

				graph.Filter.Current.StartPeriodID = null;
				graph.Filter.Current.BookID = filter.BookID;
				graph.Filter.Cache.IsDirty = false;
				throw new PXRedirectRequiredException(graph, true, "ViewDetails") { Mode = PXBaseRedirectException.WindowMode.Same };
			}
			return adapter.Get();
		}

		[Serializable]
		public partial class FixedAssetFilter : IBqlTable
		{
			#region AssetID
			public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
			protected int? _AssetID;
			[PXDBInt]
			[PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.recordType, Equal<FARecordType.assetType>>>),
				SubstituteKey = typeof(FixedAsset.assetCD),
				DescriptionField = typeof(FixedAsset.description))]
			[PXUIField(DisplayName = "Asset ID")]
			[PXDefault]
			public virtual int? AssetID
			{
				get
				{
					return _AssetID;
				}
				set
				{
					_AssetID = value;
				}
			}
			#endregion
			#region PeriodID
			public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
			protected string _PeriodID;
			[PXDefault()]
			[FABookPeriodExistingInGLSelector(
				dateType: typeof(AccessInfo.businessDate),
				assetSourceType: typeof(FixedAssetFilter.assetID),
				isBookRequired: false)]
			[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
			public virtual string PeriodID
			{
				get
				{
					return this._PeriodID;
				}
				set
				{
					this._PeriodID = value;
				}
			}
			#endregion
			#region BookID
			public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
			protected int? _BookID;
			[PXDBInt]
			[PXSelector(typeof(FABook.bookID),
						SubstituteKey = typeof(FABook.bookCode),
						DescriptionField = typeof(FABook.description))]
			[PXDefault(typeof(Search<FABook.bookID, Where<FABook.updateGL, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Book")]
			public virtual int? BookID
			{
				get
				{
					return _BookID;
				}
				set
				{
					_BookID = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class Amounts : IBqlTable
		{
			#region BookID
			public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
			protected int? _BookID;
			[PXDBInt]
			[PXSelector(typeof(FABook.bookID),
						SubstituteKey = typeof(FABook.bookCode),
						DescriptionField = typeof(FABook.description))]
			[PXUIField(DisplayName = "Book")]
			public virtual int? BookID
			{
				get
				{
					return _BookID;
				}
				set
				{
					_BookID = value;
				}
			}
			#endregion
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected int? _AccountID;
			[Account(IsKey = true, DisplayName = "Account", DescriptionField = typeof(Account.description))]
			public virtual int? AccountID
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
			#region AcctDescr
			public abstract class acctDescr : PX.Data.BQL.BqlString.Field<acctDescr> { }
			protected string _AcctDescr;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Account Description")]
			public virtual string AcctDescr
			{
				get
				{
					return _AcctDescr;
				}
				set
				{
					_AcctDescr = value;
				}
			}
			#endregion
			#region SubID
			public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			protected int? _SubID;
			[SubAccount(IsKey = true, DisplayName = "Subaccount")]
			public virtual int? SubID
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
			#region SubDescr
			public abstract class subDescr : PX.Data.BQL.BqlString.Field<subDescr> { }
			protected string _SubDescr;
			[PXDBString(255, IsUnicode = true)]
			[PXUIField(DisplayName = "Subaccount Description", FieldClass = SubAccountAttribute.DimensionName)]
			public virtual string SubDescr
			{
				get
				{
					return _SubDescr;
				}
				set
				{
					_SubDescr = value;
				}
			}
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected int? _BranchID;
			[Branch(typeof(Search<FixedAsset.branchID, Where<FixedAsset.assetID, Equal<Current<FATran.assetID>>>>), Required = false)]
			public virtual int? BranchID
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
			#region ItdAmt
			public abstract class itdAmt : PX.Data.BQL.BqlDecimal.Field<itdAmt> { }
			protected decimal? _ItdAmt;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Inception to Date")]
			public virtual decimal? ItdAmt
			{
				get
				{
					return _ItdAmt;
				}
				set
				{
					_ItdAmt = value;
				}
			}
			#endregion
			#region YtdAmt
			public abstract class ytdAmt : PX.Data.BQL.BqlDecimal.Field<ytdAmt> { }
			protected decimal? _YtdAmt;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Year to Date")]
			public virtual decimal? YtdAmt
			{
				get
				{
					return _YtdAmt;
				}
				set
				{
					_YtdAmt = value;
				}
			}
			#endregion
			#region PtdAmt
			public abstract class ptdAmt : PX.Data.BQL.BqlDecimal.Field<ptdAmt> { }
			protected decimal? _PtdAmt;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Period to Date")]
			public virtual decimal? PtdAmt
			{
				get
				{
					return _PtdAmt;
				}
				set
				{
					_PtdAmt = value;
				}
			}
			#endregion
		}

		[Serializable]
		[PXHidden]
		public partial class CreditAccount : Account
		{
			public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		}

		[Serializable]
		[PXHidden]
		public partial class CreditSub : Sub
		{
			public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		}
	}
}