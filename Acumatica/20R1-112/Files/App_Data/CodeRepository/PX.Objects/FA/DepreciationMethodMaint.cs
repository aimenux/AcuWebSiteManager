using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.IN;

namespace PX.Objects.FA
{
	public class DepreciationMethodMaint : PXGraph<DepreciationMethodMaint, FADepreciationMethod>
	{
		#region Cache Attached

		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<FADepreciationMethod.methodCD, Where<FADepreciationMethod.isTableMethod, NotEqual<True>>>), DescriptionField = typeof(FADepreciationMethod.description))]
		[PXUIField(DisplayName = "Depreciation Method ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXFieldDescription]
		public void FADepreciationMethod_MethodCD_CacheAttached(PXCache cache) { }
		
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FARecordType.BothType, PersistingCheck = PXPersistingCheck.Nothing)]
		[FARecordType.MethodList]
		public void FADepreciationMethod_RecordType_CacheAttached(PXCache cache) { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Table Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public void FADepreciationMethod_IsTableMethod_CacheAttached(PXCache cache) { }
		#endregion

		#region Selects Declaration
		public PXSelect<FADepreciationMethod, Where<FADepreciationMethod.isTableMethod, Equal<False>>> Method;
		public PXSetupOptional<FASetup> FASetup;
		#endregion

		#region Ctor
		#endregion

		#region Events

		protected virtual void FADepreciationMethod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;

			bool editable = meth.IsPredefined != true || FASetup.Current.AllowEditPredefinedDeprMethod == true;

			sender.AllowUpdate = editable;
			sender.AllowDelete = editable;

			this.Caches<FADepreciationMethodLines>().AllowInsert = editable;
			this.Caches<FADepreciationMethodLines>().AllowUpdate = editable;
			this.Caches<FADepreciationMethodLines>().AllowDelete = editable;

			PXUIFieldAttribute.SetVisible<FADepreciationMethod.yearlyAccountancy>(sender, null, !meth.IsNewZealandMethod);

			FAAveragingConvention.SetAveragingConventionsList<FADepreciationMethod.averagingConvention>(sender, meth,
				new KeyValuePair<object, Dictionary<object, string[]>>(meth.DepreciationMethod, FAAveragingConvention.DeprMethodDisabledValues));
		}

		protected virtual void FADepreciationMethod_UsefulLife_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
            if(meth == null) return;
			meth.RecoveryPeriod = (int)((meth.UsefulLife ?? 0m) * 12);
        }

		protected virtual void FADepreciationMethod_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;

			FABookSettings sett = PXSelect<FABookSettings, Where<FABookSettings.depreciationMethodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(this, meth.MethodID);
			FABookBalance bal = PXSelect<FABookBalance, Where<FABookBalance.depreciationMethodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(this, meth.MethodID);
			if (sett != null || bal != null)
			{
				throw new PXSetPropertyException(Messages.FADeprMethodUsedInAssets);
			}
		}
		#endregion

		public override void Persist()
		{
			foreach (FADepreciationMethod method in Method.Cache.Updated)
			{
				method.IsPredefined = false;
				Method.Update(method);
			}

			base.Persist();
		}
	}

	public class DepreciationTableMethodMaint : PXGraph<DepreciationTableMethodMaint, FADepreciationMethod>
	{
		#region Cache Attached
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<FADepreciationMethod.methodCD, Where<FADepreciationMethod.isTableMethod, Equal<True>>>), DescriptionField = typeof(FADepreciationMethod.description))]
		[PXUIField(DisplayName = "Depreciation Method ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXFieldDescription]
		public void FADepreciationMethod_MethodCD_CacheAttached(PXCache cache) { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Yearly Accountancy", Visibility = PXUIVisibility.SelectorVisible)]
		public void FADepreciationMethod_YearlyAccountancy_CacheAttached(PXCache cache) { }

		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Useful Life, Years", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[PXDefault]
		public void FADepreciationMethod_UsefulLife_CacheAttached(PXCache cache) { }
		#endregion

		#region Selects Declaration
		public PXSelect<FADepreciationMethod, Where<FADepreciationMethod.isTableMethod, Equal<True>>> Method;
		public PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Current<FADepreciationMethod.methodID>>>> details;
		public PXSetupOptional<FASetup> FASetup;
		#endregion

		#region Ctor
		public DepreciationTableMethodMaint()
		{
			FASetup setup = FASetup.Current;
			details.Cache.AllowInsert = false;
			details.Cache.AllowDelete = false;
		}
		#endregion

		#region Events
		protected virtual void FADepreciationMethod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FADepreciationMethod method = (FADepreciationMethod)e.Row;
			if (method == null) return;

			PXUIFieldAttribute.SetEnabled<FADepreciationMethod.parentMethodID>(sender, method, method.RecordType == FARecordType.AssetType);
			PXUIFieldAttribute.SetEnabled<FADepreciationMethod.averagingConvPeriod>(sender, method, method.AveragingConvention != FAAveragingConvention.FullYear);
			FAAveragingConvention.SetAveragingConventionsList<FADepreciationMethod.averagingConvention>(sender, method,
				new KeyValuePair<object, Dictionary<object, string[]>>(method.RecordType, FAAveragingConvention.RecordTypeDisabledValues));

			bool editable = method.IsPredefined != true || FASetup.Current.AllowEditPredefinedDeprMethod == true;

			sender.AllowUpdate = editable;
			sender.AllowDelete = editable;

			this.Caches<FADepreciationMethodLines>().AllowUpdate = editable;
		}

		[PXDBString(2, IsFixed = true)]
		[PXDefault(FAAveragingConvention.FullPeriod, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Averaging Convention", Visibility = PXUIVisibility.SelectorVisible)]
		[FAAveragingConvention.List]
		[PXFormula(typeof(Switch<Case<Where<FADepreciationMethod.recordType, Equal<FARecordType.bothType>>, FAAveragingConvention.fullYear>, FADepreciationMethod.averagingConvention>))]
		protected virtual void FADepreciationMethod_AveragingConvention_CacheAttached(PXCache sender) {}

		protected virtual void AdjustMethodLines(FADepreciationMethod meth)
		{
			if (meth.UsefulLife == null || meth.AveragingConvention == null || meth.AveragingConvPeriod == null) return;
			int periodFactor;
			switch (meth.AveragingConvention)
			{
				case FAAveragingConvention.HalfYear:
				case FAAveragingConvention.FullYear:
					periodFactor = 12;
					break;
				case FAAveragingConvention.HalfQuarter:
				case FAAveragingConvention.FullQuarter:
					periodFactor = 3;
					break;
				default:
					periodFactor = 1;
					break;
			}
			int offsetPeriods = ((int)meth.AveragingConvPeriod - 1) * periodFactor;
			int periods = (int)Math.Ceiling((decimal)meth.UsefulLife * 12 + offsetPeriods);
			if (meth.AveragingConvention == FAAveragingConvention.HalfYear ||
				 meth.AveragingConvention == FAAveragingConvention.HalfQuarter ||
				 meth.AveragingConvention == FAAveragingConvention.HalfPeriod ||
				 meth.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
				 meth.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
				 meth.AveragingConvention == FAAveragingConvention.NextPeriod
				)
			{
				periods++;
			}

			int years = (int)Math.Ceiling(periods / 12d);
			List<FADepreciationMethodLines> lines = PXSelect<FADepreciationMethodLines, 
				Where<FADepreciationMethodLines.methodID, Equal<Current<FADepreciationMethod.methodID>>>>
				.Select(this)
				.RowCast<FADepreciationMethodLines>()
				.ToList();
			if (years != lines.Count)
			{
				decimal oldTotalRate = 0m;
				decimal newTotalRate = 0m;
				int count = 0;
				FADepreciationMethodLines lastLine = null;
				lines.Sort((line1, line2) => (line1.Year ?? 0m).CompareTo(line2.Year ?? 0m));
				foreach (FADepreciationMethodLines line in lines)
				{
					oldTotalRate += line.RatioPerYear ?? 0m;
					count++;
					if (count > years)
					{
						details.Delete(PXCache<FADepreciationMethodLines>.CreateCopy(line));
					}
					else
					{
						lastLine = PXCache<FADepreciationMethodLines>.CreateCopy(line);
						newTotalRate += line.RatioPerYear ?? 0m;
					}
				}
				if (count > years && lastLine != null)
				{
					lastLine.RatioPerYear += oldTotalRate - newTotalRate;
					lastLine = details.Update(lastLine);
					details.Cache.SetDefaultExt<FADepreciationMethodLines.displayRatioPerYear>(lastLine);
				}
				else
				{
					for (int year_num = count + 1; year_num <= years; year_num++)
					{
						details.Insert(new FADepreciationMethodLines { Year = year_num, RatioPerYear = 0m });
					}
				}
			}
		}

		protected virtual void FADepreciationMethod_AveragingConvention_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;
			if (Method.Current.AveragingConvention == FAAveragingConvention.FullYear)
			{
				sender.SetDefaultExt<FADepreciationMethod.recoveryPeriod>(Method.Current);
			}
			AdjustMethodLines(meth);
		}

		protected virtual void FADepreciationMethod_UsefulLife_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;
			meth.RecoveryPeriod = (int)((meth.UsefulLife ?? 0) * 12);
			AdjustMethodLines(meth);
		}

		protected virtual void FADepreciationMethod_AveragingConvPeriod_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;
			AdjustMethodLines(meth);
		}

		protected virtual void FADepreciationMethod_RecordType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;
			if (meth.RecordType == FARecordType.AssetType)
			{
				PXDefaultAttribute.SetPersistingCheck<FADepreciationMethod.parentMethodID>(sender, meth, PXPersistingCheck.NullOrBlank);
				PXUIFieldAttribute.SetRequired<FADepreciationMethod.parentMethodID>(sender, true);
			}
			else
			{
				PXDefaultAttribute.SetPersistingCheck<FADepreciationMethod.parentMethodID>(sender, meth, PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetRequired<FADepreciationMethod.parentMethodID>(sender, false);
			}
		}

		protected virtual void FADepreciationMethod_AveragingConvPeriod_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null)
				e.Cancel = true;
			else
			{
				short newValue = (short)e.NewValue;
				if (newValue < 1)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, PXErrorLevel.Error, 1);
				}
				switch (Method.Current.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
						if (newValue > 12)
						{
							throw new PXSetPropertyException(CS.Messages.Entry_LE, PXErrorLevel.Error, 12);
						}
						break;
					case FAAveragingConvention.HalfQuarter:
						if (newValue > 4)
						{
							throw new PXSetPropertyException(CS.Messages.Entry_LE, PXErrorLevel.Error, 4);
						}
						break;
					case FAAveragingConvention.HalfYear:
						if (newValue > 2)
						{
							throw new PXSetPropertyException(CS.Messages.Entry_LE, PXErrorLevel.Error, 2);
						}
						break;
				}
			}
		}

		protected virtual void FADepreciationMethod_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;

			FABookSettings sett = PXSelect<FABookSettings, Where<FABookSettings.depreciationMethodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(this, meth.MethodID);
			FABookBalance bal = PXSelect<FABookBalance, Where<FABookBalance.depreciationMethodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(this, meth.MethodID);
			if (sett != null || bal != null)
			{
				throw new PXSetPropertyException(Messages.FADeprMethodUsedInAssets);
			}
		}

		protected virtual void FADepreciationMethod_DisplayTotalPercents_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal?)e.NewValue > 100m && !e.ExternalCall)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, 100);
			}
		}
		#endregion

		protected virtual void FADepreciationMethodLines_DisplayRatioPerYear_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal?) e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, 0.ToString());
			}
			if ((decimal?)e.NewValue > 100m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, 100.ToString());
			}
		}

		public override void Persist()
		{
			foreach (FADepreciationMethod method in Method.Cache.Updated)
			{
				method.IsPredefined = false;
				Method.Update(method);
			}
			foreach (FADepreciationMethod method in new HashSet<int?>(details.Cache.Inserted.ToArray<FADepreciationMethodLines>().
																	Concat(details.Cache.Updated.ToArray<FADepreciationMethodLines>().
																	Concat((IEnumerable<FADepreciationMethodLines>) details.Cache.Deleted))
																	.Select(line => line.MethodID))
				.Select(methodID => (FADepreciationMethod)PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(this, methodID))
				.Where(method => method != null && method.IsPredefined == true))
			{
				method.IsPredefined = false;
				Method.Update(method);
			}
			base.Persist();
		}

	}

	public class DepreciationMethodCancelExt<TGraph, TPrimary, TWhere> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
		where TWhere : class, IBqlWhere, new()
	{
		public PXSelect<TPrimary> View;
		public virtual string Message { get; }

		public static bool IsActive()
		{
			return true;
		}

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected new virtual IEnumerable Cancel(PXAdapter a)
		{
			foreach (TPrimary e in (new PXCancel<TPrimary>(Base, "Cancel")).Press(a))
			{
				if (e is FADepreciationMethod method && !string.IsNullOrEmpty(method.MethodCD))
				{
					if (View.Cache.GetStatus(e) == PXEntryStatus.Inserted)
					{
						if (PXSelect<TPrimary, TWhere>.Select(Base).Any())
						{
							View.Cache.RaiseExceptionHandling<FADepreciationMethod.methodCD>(e, method.MethodCD, new PXSetPropertyException(Message));
						}
					}
				}
				yield return e;
			}
		}
	}

	public class DepreciationMethodExt : DepreciationMethodCancelExt<DepreciationMethodMaint, 
		FADepreciationMethod,  
		Where<FADepreciationMethod.methodCD, Equal<Current<FADepreciationMethod.methodCD>>,
										And<FADepreciationMethod.isTableMethod, Equal<True>>>>
	{
		public override string Message { get => Messages.FAMethodCDTableBasedExists; }
	}

	public class DepreciationTableMethodMaintExt : DepreciationMethodCancelExt<DepreciationTableMethodMaint,
		FADepreciationMethod,
		Where<FADepreciationMethod.methodCD, Equal<Current<FADepreciationMethod.methodCD>>,
										And<FADepreciationMethod.isTableMethod, Equal<False>>>>
	{
		public override string Message { get => Messages.FAMethodCDFormulaBasedExists; }
	}
}
