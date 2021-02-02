using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.DR
{
	public class DRBalanceValidation : PXGraph<DRBalanceValidation>
	{
		public PXFilter<DRBalanceValidationFilter> Filter;
		public PXCancel<DRBalanceValidationFilter> Cancel;
		[PXFilterable]
		public PXFilteredProcessing<DRBalanceType, DRBalanceValidationFilter> Items;

		public PXSetup<DRSetup> Setup;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public virtual IEnumerable items()
		{
			bool found = false;
			foreach (DRBalanceType item in Items.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;

			DRBalanceType revenue = new DRBalanceType();
			revenue.AccountType = DeferredAccountType.Income;
			yield return Items.Insert(revenue);


			DRBalanceType expense = new DRBalanceType();
			expense.AccountType = DeferredAccountType.Expense;
			yield return Items.Insert(expense);

			Items.Cache.IsDirty = false;
		}

		public DRBalanceValidation()
		{
			DRSetup setup = Setup.Current;

			Items.SetProcessCaption(GL.Messages.ProcValidate);
			Items.SetProcessAllCaption(GL.Messages.ProcValidateAll);
		}

		protected virtual bool PendingFullValidation => Setup.Current.PendingExpenseValidate == true || Setup.Current.PendingRevenueValidate == true;

		protected virtual void _(Events.RowSelected<DRBalanceValidationFilter> e)
		{
			bool errorsOnForm = PXUIFieldAttribute.GetErrors(e.Cache, null, PXErrorLevel.Error, PXErrorLevel.RowError).Any();

			Items.SetProcessEnabled(!errorsOnForm && !PendingFullValidation);
			Items.SetProcessAllEnabled(!errorsOnForm);

			DRBalanceValidationFilter filter = e.Row as DRBalanceValidationFilter;
			if (filter == null) return;

			Items.SetProcessDelegate(
				delegate (List<DRBalanceType> list)
				{
					DRProcess graph = CreateInstance<DRProcess>();
					graph.RunIntegrityCheck(list, filter.FinPeriodID);
				}
			);

			e.Cache
				.Adjust<PXUIFieldAttribute>(e.Row)
				.For<DRBalanceValidationFilter.finPeriodID>(a => a.Enabled = !PendingFullValidation);
		}

		protected virtual void _(Events.RowSelected<DRBalanceType> e)
		{
			if (Filter.Current == null || e.Row == null) return;

			object finPeriodID = Filter.Current.FinPeriodID;
			try
			{
				Filter.Cache.RaiseFieldVerifying<DRBalanceValidationFilter.finPeriodID>(Filter.Current, ref finPeriodID);
			}
			catch(PXSetPropertyException)
			{
				e.Cache
					.Adjust<PXUIFieldAttribute>(e.Row)
					.For<DRBalanceType.selected>(a => a.Enabled = false);
			}
		}

		#region Dirty hack for the first required full validation after 2018R2 upgrade
		protected virtual void _(Events.FieldDefaulting<DRBalanceValidationFilter.finPeriodID> e)
		{
			if (PendingFullValidation)
			{
				e.NewValue = FinPeriodRepository.FindFirstPeriod(FinPeriod.organizationID.MasterValue).FinPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowInserted<DRBalanceValidationFilter> e)
		{
			if (PendingFullValidation)
			{
				e.Cache.SetDefaultExt<DRBalanceValidationFilter.finPeriodID>(e.Row);
			}
		}
		#endregion

		[Serializable]
		public partial class DRBalanceType : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
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
			#region AccountType
			public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }
			protected string _AccountType;
			[PXString(1, IsKey=true)]
			[PXDefault(DeferredAccountType.Income)]
			[LabelList(typeof(DeferredAccountType))]
			[PXUIField(DisplayName = "Balance Type", Enabled=false)]
			public virtual string AccountType
			{
				get
				{
					return this._AccountType;
				}
				set
				{
					this._AccountType = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class DRBalanceValidationFilter : IBqlTable
		{
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			[FinPeriodNonLockedSelector]
			[PXUIField(DisplayName = GL.Messages.FinPeriod)]
			public virtual String FinPeriodID { get; set; }

			#endregion
		}
	}
}