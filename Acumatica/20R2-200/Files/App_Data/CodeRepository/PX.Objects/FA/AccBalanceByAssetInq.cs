using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.FA
{
	[TableAndChartDashboardType]
	public class AccBalanceByAssetInq : PXGraph<AccBalanceByAssetInq>
	{
		public PXCancel<AccBalanceByAssetFilter> Cancel;
		public PXFilter<AccBalanceByAssetFilter> Filter;
		public PXSelect<Amounts> Amts;

		public AccBalanceByAssetInq()
		{
			Amts.Cache.AllowInsert = false;
			Amts.Cache.AllowUpdate = false;
			Amts.Cache.AllowDelete = false;
		}

		protected virtual IEnumerable filter()
		{
			PXCache cache = this.Caches[typeof(AccBalanceByAssetFilter)];
			AccBalanceByAssetFilter filter = (AccBalanceByAssetFilter)cache.Current;
			if (filter != null)
			{
				filter.Balance = decimal.Zero;
				foreach (Amounts amt in Amts.Select())
				{
					filter.Balance += amt.ItdAmt;
				}
			}
			
			yield return cache.Current;
			cache.IsDirty = false;
		}

		public virtual IEnumerable amts(PXAdapter adapter)
		{
			AccBalanceByAssetFilter filter = Filter.Current;
			if (filter == null) yield break;

			PXSelectBase<FATran> select = new PXSelectJoin<FATran,
				InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FATran.assetID>, And<FixedAsset.recordType, Equal<FARecordType.assetType>>>,
				InnerJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>,
				InnerJoin<Branch, On<FixedAsset.branchID, Equal<Branch.branchID>>,
				InnerJoin<FALocationHistoryCurrent, On<FALocationHistoryCurrent.assetID, Equal<FixedAsset.assetID>>,
				InnerJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FixedAsset.assetID>,
					And<FALocationHistory.periodID, Equal<FALocationHistoryCurrent.lastPeriodID>,
					And<FALocationHistory.revisionID, Equal<FALocationHistoryCurrent.lastRevisionID>>>>>>>>>,
				Where<FATran.released, Equal<True>,
					And<FATran.finPeriodID, LessEqual<Current<AccBalanceByAssetFilter.periodID>>,
					And<FATran.bookID, Equal<Current<AccBalanceByAssetFilter.bookID>>,
				And2<Where<FALocationHistory.fAAccountID, Equal<Current<AccBalanceByAssetFilter.accountID>>,
					And<FALocationHistory.fASubID, Equal<Current<AccBalanceByAssetFilter.subID>>,
					Or<FALocationHistory.accumulatedDepreciationAccountID, Equal<Current<AccBalanceByAssetFilter.accountID>>,
					And<FALocationHistory.accumulatedDepreciationSubID, Equal<Current<AccBalanceByAssetFilter.subID>>>>>>,
				And<Where<FATran.debitAccountID, Equal<Current<AccBalanceByAssetFilter.accountID>>,
					And<FATran.debitSubID, Equal<Current<AccBalanceByAssetFilter.subID>>,
					Or<FATran.creditAccountID, Equal<Current<AccBalanceByAssetFilter.accountID>>,
					And<FATran.creditSubID, Equal<Current<AccBalanceByAssetFilter.subID>>>>>>>>>>>>(this);

			if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() || filter.OrganizationID != null)
			{
				select.WhereAnd<Where<Branch.organizationID, Equal<Current<AccBalanceByAssetFilter.organizationID>>, And<MatchWithBranch<Branch.branchID>>>>();
			}
			if (filter.BranchID != null)
			{
				select.WhereAnd<Where<Branch.branchID, Equal<Current<AccBalanceByAssetFilter.branchID>>>>();
			}

			Dictionary<int?, Amounts> dict = new Dictionary<int?, Amounts>();
			foreach (PXResult<FATran, FixedAsset, FADetails, Branch, FALocationHistoryCurrent, FALocationHistory> res in select.Select())
			{
				FATran tran = (FATran)res;
				FixedAsset asset = (FixedAsset)res;
				FADetails details = (FADetails)res;
				FALocationHistory location = (FALocationHistory)res;

				Amounts record = null;
				if (!dict.TryGetValue(asset.AssetID, out record))
				{
					record = new Amounts
					{
						AssetID = asset.AssetID,
						Description = asset.Description,
						Status = details.Status,
						ClassID = asset.ClassID,
						DepreciateFromDate = details.DepreciateFromDate,
						BranchID = location.BranchID,
						Department = location.Department,
						ItdAmt = decimal.Zero,
						YtdAmt = decimal.Zero,
						PtdAmt = decimal.Zero
					};
				}

				decimal tranAmt = tran.TranAmt ?? decimal.Zero;
				decimal amount = tran.DebitAccountID == tran.CreditAccountID && tran.DebitSubID == tran.CreditSubID
					? decimal.Zero
					: tran.DebitAccountID == filter.AccountID && tran.DebitSubID == filter.SubID ? tranAmt : -tranAmt;
				
				record.ItdAmt += amount;
				record.YtdAmt += (FinPeriodUtils.FinPeriodEqual(filter.PeriodID, tran.FinPeriodID, FinPeriodUtils.FinPeriodComparison.Year) ? amount : decimal.Zero);
				record.PtdAmt += (filter.PeriodID == tran.FinPeriodID ? amount : decimal.Zero);
				
				dict[asset.AssetID] = record;	
			}

			foreach (Amounts amt in dict.Values)
			{
				if (amt.ItdAmt != decimal.Zero || amt.YtdAmt != decimal.Zero || amt.PtdAmt != decimal.Zero)
				{
					yield return amt;
				}
			}
		}

		public PXAction<AccBalanceByAssetFilter> viewDetails;
		[PXUIField(DisplayName = "Asset Transaction History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			Amounts amt = Amts.Current;
			AccBalanceByAssetFilter filter = Filter.Current;
			
			if (amt != null && filter != null)
			{
				FACostDetailsInq graph = CreateInstance<FACostDetailsInq>();

				AccountFilter filterDetails =
				graph.Filter.Current =
					new AccountFilter()
					{
						AssetID = amt.AssetID,
						EndPeriodID = filter.PeriodID,
						AccountID = filter.AccountID,
						SubID = filter.SubID
					};
				filterDetails = graph.Filter.Insert(filterDetails);

				filterDetails.StartPeriodID = null;
				filterDetails.BookID = filter.BookID;
				graph.Filter.Cache.IsDirty = false;
				throw new PXRedirectRequiredException(graph, true, "ViewDetails") { Mode = PXBaseRedirectException.WindowMode.Same };
			}

			return adapter.Get();
		}

		[Serializable]
		public partial class AccBalanceByAssetFilter : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : IBqlField { }

			[Organization(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>>))]
			public int? OrganizationID { get; set; }
			#endregion
			#region BranchID
			public abstract class branchID : IBqlField { }

			[BranchOfOrganization(
				organizationFieldType: typeof(AccBalanceByAssetFilter.organizationID),
				onlyActive: true,
				featureFieldType: typeof(FeaturesSet.multipleCalendarsSupport),
				IsDetail = false,
				Required = false,
				PersistingCheck = PXPersistingCheck.Nothing)]
			public int? BranchID { get; set; }
			#endregion
			#region OrgBAccountID
			public abstract class orgBAccountID : IBqlField { }

			[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
			public int? OrgBAccountID { get; set; }
			#endregion
			#region BookID
			public abstract class bookID : IBqlField
			{
			}
			protected int? _BookID;
			[PXDBInt]
			[PXSelector(typeof(FABook.bookID),
				SubstituteKey = typeof(FABook.bookCode),
				DescriptionField = typeof(FABook.description))]
			[PXDefault(typeof(Search<FABook.bookID, Where<FABook.updateGL, Equal<True>>>))]
			[PXUIField(DisplayName = "Book", Enabled = false)]
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
			[PXDefault]
			[Account(DisplayName = "Account", DescriptionField = typeof(Account.description))]
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
			#region SubID
			public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			protected int? _SubID;
			[PXDefault]
			[SubAccount(typeof(accountID), DisplayName = "Subaccount")]
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
			#region PeriodID
			public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
			protected string _PeriodID;
			[PXDefault]
			[FABookPeriodExistingInGLSelector(
				dateType: typeof(AccessInfo.businessDate),
				organizationSourceType: typeof(AccBalanceByAssetFilter.organizationID),
				branchSourceType: typeof(AccBalanceByAssetFilter.branchID),
				isBookRequired: false)]
			[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
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
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
			protected decimal? _Balance;
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBBaseCury]
			[PXUIField(DisplayName = "Balance by Assets", Enabled = false)]
			public virtual decimal? Balance
			{
				get
				{
					return _Balance;
				}
				set
				{
					_Balance = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class Amounts : IBqlTable
		{
			#region AssetID
			public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
			protected int? _AssetID;
			[PXDBInt(IsKey = true)]
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
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected string _Description;
			[PXString(256, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 2)]
			[PX.Data.EP.PXFieldDescription]
			public virtual string Description
			{
				get
				{
					return _Description;
				}
				set
				{
					_Description = value;
				}
			}
			#endregion
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			protected string _Status;
			[PXString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Status", Enabled = false)]
			[FixedAssetStatus.List()]
			public virtual string Status
			{
				get
				{
					return _Status;
				}
				set
				{
					_Status = value;
				}
			}
			#endregion
			#region ClassID
			public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
			protected int? _ClassID;
			[PXInt]
			[PXSelector(typeof(Search<FAClass.assetID, Where<FAClass.recordType, Equal<FARecordType.classType>>>),
						typeof(FAClass.assetCD), typeof(FAClass.assetTypeID), typeof(FAClass.description), typeof(FAClass.usefulLife),
						SubstituteKey = typeof(FAClass.assetCD),
						DescriptionField = typeof(FAClass.description), CacheGlobal = true)]
			[PXUIField(DisplayName = "Asset Class", Visibility = PXUIVisibility.Visible, TabOrder = 3)]
			public int? ClassID
			{
				get
				{
					return _ClassID;
				}
				set
				{
					_ClassID = value;
				}
			}
			#endregion
			#region DepreciateFromDate
			public abstract class depreciateFromDate : PX.Data.BQL.BqlDateTime.Field<depreciateFromDate> { }
			protected DateTime? _DepreciateFromDate;
			[PXDate]
			[PXUIField(DisplayName = Messages.PlacedInServiceDate)]
			public virtual DateTime? DepreciateFromDate
			{
				get
				{
					return _DepreciateFromDate;
				}
				set
				{
					_DepreciateFromDate = value;
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
			#region Department
			public abstract class department : PX.Data.BQL.BqlString.Field<department> { }
			protected string _Department;
			[PXString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Department")]
			public virtual string Department
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
			#region ItdAmt
			public abstract class itdAmt : PX.Data.BQL.BqlDecimal.Field<itdAmt> { }
			protected decimal? _ItdAmt;
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXBaseCury()]
			[PXUIField(DisplayName = "Inception to Date", Enabled = false)]
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
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXBaseCury()]
			[PXUIField(DisplayName = "Year to Date", Enabled = false)]
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
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXBaseCury()]
			[PXUIField(DisplayName = "Period to Date", Enabled = false)]
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

		[PXProjection(typeof(Select5<FixedAsset,
		InnerJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FixedAsset.assetID>, And<FixedAsset.recordType, Equal<FARecordType.assetType>>>>,
		Where<FALocationHistory.periodID, LessEqual<CurrentValue<AccBalanceByAssetFilter.periodID>>>,
		Aggregate<GroupBy<FALocationHistory.assetID, Max<FALocationHistory.periodID, Max<FALocationHistory.revisionID>>>>>))]
		[Serializable]
		[PXHidden]
		public partial class FALocationHistoryCurrent : IBqlTable
		{
			#region AssetID
			public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
			protected int? _AssetID;
			[PXDBInt(IsKey = true, BqlField = typeof(FixedAsset.assetID))]
			[PXDefault()]
			public virtual int? AssetID
			{
				get
				{
					return this._AssetID;
				}
				set
				{
					this._AssetID = value;
				}
			}
			#endregion
			#region LastPeriodID
			public abstract class lastPeriodID : PX.Data.BQL.BqlString.Field<lastPeriodID> { }
			protected string _LastPeriodID;
			[FABookPeriodID(
				assetSourceType: typeof(assetID),
				BqlField = typeof(FALocationHistory.periodID))]
			[PXDefault()]
			public virtual string LastPeriodID
			{
				get
				{
					return this._LastPeriodID;
				}
				set
				{
					this._LastPeriodID = value;
				}
			}
			#endregion
			#region LastRevisionID
			public abstract class lastRevisionID : PX.Data.BQL.BqlInt.Field<lastRevisionID> { }
			protected int? _LastRevisionID;
			[PXDBInt(IsKey = true, BqlField = typeof(FALocationHistory.revisionID))]
			[PXDefault(0)]
			public virtual int? LastRevisionID
			{
				get
				{
					return this._LastRevisionID;
				}
				set
				{
					this._LastRevisionID = value;
				}
			}
			#endregion
		}
	}
}
