using System;
using System.Linq;
using PX.Data;
using System.Collections.Generic;
using PX.Common;
using PX.Objects.GL;

namespace PX.Objects.FA
{
	public class AssetClassMaint : PXGraph<AssetClassMaint, FixedAsset>
	{
		[InjectDependency]
		public IFABookPeriodRepository FABookPeriodRepository { get; set; }

		#region DAC Overrides
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FARecordType.ClassType)]
		[PXUIField(DisplayName = "Record Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[FARecordType.List]
		protected virtual void FixedAsset_RecordType_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.Invisible, Enabled = false)]
		public virtual void FixedAsset_BranchID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Asset Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<FixedAsset.assetCD, Where<FixedAsset.recordType, Equal<FARecordType.classType>>>),
			typeof(FixedAsset.assetCD),
			typeof(FixedAsset.description),
			typeof(FixedAsset.depreciable),
			typeof(FixedAsset.assetTypeID),
			typeof(FixedAsset.usefulLife),
			Filterable = true)]
		protected virtual void FixedAsset_AssetCD_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.parentAssetID>>>>), UseCurrent = true, LeaveChildren = true)]
		[PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.assetID, NotEqual<Current<FixedAsset.assetID>>,
													 And<Where<FixedAsset.recordType, Equal<Current<FixedAsset.recordType>>,
														   And<Current<FixedAsset.recordType>, NotEqual<FARecordType.elementType>,
														   Or<Current<FixedAsset.recordType>, Equal<FARecordType.elementType>,
														   And<FixedAsset.recordType, Equal<FARecordType.assetType>>>>>>>>),
					typeof(FixedAsset.assetCD),
					typeof(FixedAsset.description),
					typeof(FixedAsset.assetTypeID),
					typeof(FixedAsset.usefulLife),
					SubstituteKey = typeof(FixedAsset.assetCD),
					DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Parent Class", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void FixedAsset_ParentAssetID_CacheAttached(PXCache sender) { }

		[PXString(1, IsFixed = true)]
		protected virtual void FixedAsset_Status_CacheAttached(PXCache sender) { }

		[PXDefault]
		[SubAccountMask(DisplayName = "Combine Fixed Asset Sub. from")]
		protected virtual void FixedAsset_FASubMask_CacheAttached(PXCache sender) { }

		[PXDefault]
		[SubAccountMask(DisplayName = "Combine Accumulated Depreciation Sub. from")]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		protected virtual void FixedAsset_AccumDeprSubMask_CacheAttached(PXCache sender) { }

		[PXDefault]
		[SubAccountMask(DisplayName = "Combine Depreciation Expense Sub. from")]
		[PXUIRequired(typeof(FixedAsset.depreciable))]
		protected virtual void FixedAsset_DeprExpenceSubMask_CacheAttached(PXCache sender) { }

		[SubAccountMask(DisplayName = "Combine Proceeds Sub. from")]
		protected virtual void FixedAsset_ProceedsSubMask_CacheAttached(PXCache sender) { }

		[PXDefault]
		[SubAccountMask(DisplayName = "Combine Gain/Loss Sub. from")]
		protected virtual void FixedAsset_GainLossSubMask_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXUIField(DisplayName = "Depreciate", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Switch<Case<Where<FixedAsset.assetTypeID, IsNotNull>, Selector<FixedAsset.assetTypeID, FAType.depreciable>>, True>))]
		protected virtual void FixedAsset_Depreciable_CacheAttached(PXCache sender) { }

		#endregion

		#region Selects Declaration

		public PXSelect<FixedAsset, Where<FixedAsset.recordType, Equal<FARecordType.classType>>> AssetClass;
		public PXSelect<FixedAsset, Where<FixedAsset.assetCD, Equal<Current<FixedAsset.assetCD>>>> CurrentAssetClass;
		public PXSelectJoin<FABookSettings,
			LeftJoin<FABook, On<FABook.bookID, Equal<FABookSettings.bookID>>>,
			Where<FABookSettings.assetID, Equal<Current<FixedAsset.assetID>>>> DepreciationSettings;
		public PXSetup<FASetup> FASetup;
		#endregion

		#region Ctor
		public AssetClassMaint()
		{
			PXCache cache = AssetClass.Cache;
			FASetup setup = FASetup.Current;

			PXUIFieldAttribute.SetRequired<FixedAsset.usefulLife>(cache, true);
		}
		#endregion

		#region FixedAsset Events
		protected virtual void FixedAsset_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			FixedAsset assetClass = (FixedAsset)e.Row;
			if (assetClass == null) return;

			if (assetClass.AssetCD != null)
			{
				foreach (FABookSettings settings in PXSelect<FABook>.Select(this).RowCast<FABook>().Select(book => new FABookSettings { BookID = book.BookID }))
				{
					DepreciationSettings.Insert(settings);
				}

				DepreciationSettings.Cache.IsDirty = false;
			}
		}

		protected virtual void FixedAsset_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FixedAsset cls = (FixedAsset)e.Row;
			if (cls == null) return;

			PXUIFieldAttribute.SetEnabled<FixedAsset.accumDeprSubMask>(sender, cls, cls.UseFASubMask != true);

			bool? UpdateGL = DepreciationSettings.Select().RowCast<FABookSettings>().Any(books => books.UpdateGL == true);

			PXUIFieldAttribute.SetEnabled<FixedAsset.holdEntry>(sender, e.Row, (UpdateGL != true));

			if (((FixedAsset)e.Row).HoldEntry == false && UpdateGL == true)
			{
				((FixedAsset)e.Row).HoldEntry = true;

				if (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
				{
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
					sender.IsDirty = true;
				}
			}
		}

		protected virtual void FixedAsset_RecordType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = FARecordType.ClassType;
			e.Cancel = true;
		}

		protected virtual void FixedAsset_Depreciable_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			AssetMaint.UpdateBalances<FABookSettings.depreciate, FixedAsset.depreciable>(sender, e);
		}

		protected virtual void FixedAsset_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FixedAsset header = (FixedAsset)e.Row;
			if (header == null) return;

			if (header.Depreciable == true && (header.UsefulLife ?? 0m) == 0m)
			{
				sender.RaiseExceptionHandling<FixedAsset.usefulLife>(header, header.UsefulLife, new PXSetPropertyException(CS.Messages.Entry_GT, PXErrorLevel.Error, 0m));
			}
			if (header.Active != true)
			{
				PXSelectBase<FixedAsset> selectStatement = new PXSelectJoin<FixedAsset,
										InnerJoin<FADetails, On<FixedAsset.assetID, Equal<FADetails.assetID>>>,
										Where<FixedAsset.classID, Equal<Required<FixedAsset.classID>>,
															And<Where<FADetails.status, Equal<FixedAssetStatus.active>>>>>(this);

				PXResult<FixedAsset, FADetails> res = (PXResult<FixedAsset, FADetails>)selectStatement.View.SelectSingle(header.AssetID);
				if ((FixedAsset)res != null)
					sender.RaiseExceptionHandling<FixedAsset.active>(header, header.Active, new PXSetPropertyException(Messages.FixedAssetClassCannotBeDeactivated));
			}
		}

		protected virtual void FixedAsset_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FixedAsset cls = (FixedAsset)e.Row;
			if (cls == null) return;

			FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.classID, Equal<Required<FixedAsset.assetID>>>>.Select(this, cls.AssetID);
			if (asset != null)
			{
				throw new PXSetPropertyException(Messages.FAClassUsedInAssets);
			}

			FixedAsset child = PXSelect<FixedAsset, Where<FixedAsset.parentAssetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, cls.AssetID);
			if (child != null)
			{
				throw new PXSetPropertyException(Messages.FAClassIsParent);
			}

		}

		protected virtual void FixedAsset_UsefulLife_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FixedAsset header = (FixedAsset)e.Row;
			if (header == null) return;

			foreach (FABookSettings newSettings in PXSelect<FABookSettings, Where<FABookSettings.assetID, Equal<Required<FABookSettings.assetID>>>>.Select(this, header.AssetID)
					.RowCast<FABookSettings>()
					.Select(set => (FABookSettings)DepreciationSettings.Cache.CreateCopy(set)))
			{
				FADepreciationMethod deprMethod = PXSelectorAttribute.Select<FABookSettings.depreciationMethodID>(DepreciationSettings.Cache, newSettings) as FADepreciationMethod;
				if (deprMethod?.DepreciationMethod != FADepreciationMethod.depreciationMethod.AustralianPrimeCost
					&& deprMethod?.DepreciationMethod != FADepreciationMethod.depreciationMethod.NewZealandStraightLine
					&& deprMethod?.DepreciationMethod != FADepreciationMethod.depreciationMethod.NewZealandStraightLineEvenly)
				{
					newSettings.UsefulLife = header.UsefulLife;
					DepreciationSettings.Update(newSettings);
				}
			}
		}

		protected virtual void FixedAsset_FASubMask_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FixedAsset cls = (FixedAsset)e.Row;
			if (cls == null) return;

			if (cls.UseFASubMask == true)
			{
				cls.AccumDeprSubMask = cls.FASubMask;
			}
		}

		protected virtual void FixedAsset_UseFASubMask_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FixedAsset_FASubMask_FieldUpdated(sender, e);
		}

		protected virtual void FixedAsset_ParentAssetID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FixedAsset asset = (FixedAsset)e.Row;
			if (asset == null) return;

			PXSelectBase<FixedAsset> cmd = new PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.parentAssetID>>>>(this);
			int? parentID = (int?)e.NewValue;
			string parentCD = null;
			while (parentID != null)
			{
				FixedAsset parent = cmd.Select(parentID);
				parentCD = parentCD ?? parent.AssetCD;
				if (parent.ParentAssetID == asset.AssetID)
				{
					e.NewValue = asset.ParentAssetID != null ? ((FixedAsset)cmd.Select(asset.ParentAssetID)).AssetCD : null;
					throw new PXSetPropertyException(Messages.CyclicParentRef, parentCD);
				}
				parentID = parent.ParentAssetID;
			}
		}
		#endregion
		#region FABookSettings Events

		protected virtual void FABookSettings_DepreciationMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<FABookSettings.averagingConvention>(e.Row);
			sender.SetDefaultExt<FABookSettings.percentPerYear>(e.Row);
		}

		protected virtual void FABookSettings_DepreciationMethodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FABookSettings bookSettings = (FABookSettings)e.Row;
			if (bookSettings == null) return;

			if (bookSettings.BookID != null)
			{
				IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(bookSettings.BookID);
				FADepreciationMethod deprMethod = PXSelectorAttribute.Select<FABookSettings.depreciationMethodID>(DepreciationSettings.Cache, bookSettings, e.NewValue) as FADepreciationMethod;

				if ((yearSetup.PeriodType == FinPeriodType.Week
						|| yearSetup.PeriodType == FinPeriodType.BiWeek
						|| yearSetup.PeriodType == FinPeriodType.FourWeek)
					&& (deprMethod?.IsNewZealandMethod == true))
				{
					e.NewValue = deprMethod?.MethodCD;
					
					string errorMessage = PXMessages.LocalizeFormat(Messages.WeeklyBooksDisabledForCalcMethod,
														PXStringListAttribute.GetLocalizedLabel<FADepreciationMethod.depreciationMethod>(Caches[typeof(FADepreciationMethod)], deprMethod));
					throw new PXSetPropertyException(errorMessage);
				}
			}
		}

		protected virtual void FABookSettings_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FABookSettings set = (FABookSettings)e.Row;
			if (set == null) return;

			PXUIFieldAttribute.SetEnabled<FABookSettings.bookID>(sender, set, set.BookID == null);

			IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(set.BookID);
			FADepreciationMethod method = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Current<FABookSettings.depreciationMethodID>>>>.SelectSingleBound(this, new object[] { set });

			List<KeyValuePair<object, Dictionary<object, string[]>>> parsList = new List<KeyValuePair<object, Dictionary<object, string[]>>>();
			if (method != null)
			{
				parsList.Add(method.IsTableMethod == true
					? new KeyValuePair<object, Dictionary<object, string[]>>(method.RecordType, FAAveragingConvention.RecordTypeDisabledValues)
					: new KeyValuePair<object, Dictionary<object, string[]>>(method.DepreciationMethod, FAAveragingConvention.DeprMethodDisabledValues));
			}
			if (yearSetup != null)
			{
				parsList.Add(new KeyValuePair<object, Dictionary<object, string[]>>(yearSetup.IsFixedLengthPeriod, FAAveragingConvention.FixedLengthPeriodDisabledValues));
			}

			FAAveragingConvention.SetAveragingConventionsList<FADepreciationMethod.averagingConvention>(sender, set, parsList.ToArray());
		}

		protected virtual void FABookSettings_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FixedAsset header = AssetClass.Current;
			FABookSettings set = (FABookSettings)e.Row;
			if (set == null || header == null) return;

			if (set.Depreciate == true)
			{
				if (set.AveragingConvention == null || string.IsNullOrEmpty(set.AveragingConvention.Trim()))
					sender.RaiseExceptionHandling<FABookSettings.averagingConvention>(set, set.AveragingConvention, new PXSetPropertyException(Messages.ValueCanNotBeEmpty));
				if (set.DepreciationMethodID == null)
					sender.RaiseExceptionHandling<FABookSettings.depreciationMethodID>(set, set.DepreciationMethodID, new PXSetPropertyException(Messages.ValueCanNotBeEmpty));
			}
		}

		#endregion
		#region Funcs

		public override void Persist()
		{
			foreach (FABookSettings set in DepreciationSettings.Cache.Inserted.Cast<FABookSettings>().Concat<FABookSettings>(DepreciationSettings.Cache.Updated.Cast<FABookSettings>()))
			{
				FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.SelectWindowed(this, 0, 1, set.BookID);
				IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(book);

				if (yearSetup == null || !yearSetup.IsFixedLengthPeriod)
				{
					FABookPeriodSetup period = PXSelect<FABookPeriodSetup, Where<FABookPeriodSetup.bookID, Equal<Required<FABookPeriodSetup.bookID>>>>.SelectWindowed(this, 0, 1, set.BookID);
					if (period == null && set.UpdateGL == false && book != null)
					{
						DepreciationSettings.Cache.RaiseExceptionHandling<FABookSettings.bookID>(set, book.BookCode,
							new PXSetPropertyException<FABookSettings.bookID>(Messages.NoCalendarDefined));
					}
				}

				FADepreciationMethod method = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Required<FABookSettings.depreciationMethodID>>>>.SelectWindowed(this, 0, 1, set.DepreciationMethodID);
				if (method != null && method.IsTableMethod == true && method.UsefulLife != set.UsefulLife)
				{
					DepreciationSettings.Cache.RaiseExceptionHandling<FABookSettings.usefulLife>(set, set.UsefulLife, new PXSetPropertyException<FABookSettings.usefulLife>(Messages.UsefulLifeNotMatchDeprMethod));
				}
			}
			base.Persist();
		}

		#endregion
	}
}