using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN
{
	public abstract class BaseNotDecimalUnitErrorRedirectorScope<TDetailQty> : IDisposable
		where TDetailQty : IBqlField
	{
		protected PXCache MasterCache { get; }

		protected Dictionary<object, PXNotDecimalUnitException> RedirectedSplits { get; }

		protected Type DetailType => BqlCommand.GetItemType<TDetailQty>();

		public BaseNotDecimalUnitErrorRedirectorScope(PXCache masterCache)
		{
			MasterCache = masterCache;
			RedirectedSplits = new Dictionary<object, PXNotDecimalUnitException>();

			masterCache.Graph.ExceptionHandling.AddHandler<TDetailQty>(SplitExceptionHandling);
		}

		protected virtual void SplitExceptionHandling(PXCache splitCache, PXExceptionHandlingEventArgs args)
		{
			var notDecimalException = args.Exception as PXNotDecimalUnitException;
			if (notDecimalException != null)
				RedirectedSplits[args.Row] = notDecimalException;
		}

		public virtual void Dispose()
		{
			MasterCache.Graph.ExceptionHandling.RemoveHandler<TDetailQty>(SplitExceptionHandling);
			if (RedirectedSplits.Any())
				HandleErrors();
		}

		protected abstract void HandleErrors();
	}

	/// <summary>
	/// Redirect an exception with type <see cref="PXNotDecimalUnitException"/> from  detail quantity field to master quantity field
	/// </summary>
	/// <typeparam name="TDetailQty">detail type quantity field</typeparam>
	public class NotDecimalUnitErrorRedirectorScope<TDetailQty> : BaseNotDecimalUnitErrorRedirectorScope<TDetailQty>
		where TDetailQty: IBqlField
	{
		private readonly Type _masterQtyField;
		private readonly object _masterRow;

		public NotDecimalUnitErrorRedirectorScope(PXCache masterCache, object masterRow, Type masterQtyField): base(masterCache)
		{
			_masterQtyField = masterQtyField;
			_masterRow = masterRow;
		}

		protected override void HandleErrors()
		{
			if (!string.IsNullOrEmpty(PXUIFieldAttribute.GetErrorOnly(MasterCache, _masterRow, _masterQtyField.Name)))
				return;

			PXCache detailCache = MasterCache.Graph.Caches[DetailType];
			foreach (var redirected in RedirectedSplits)
			{
				if (detailCache.Cached.OfType<object>().Contains(redirected.Key))
				{
					if (PXDBQuantityAttribute.VerifyForDecimal<TDetailQty>(detailCache, redirected.Key) != null)
					{
						MasterCache.RaiseExceptionHandling(_masterQtyField.Name, _masterRow, MasterCache.GetValue(_masterRow, _masterQtyField.Name), redirected.Value);
						break;
					}
				}
			}
		}
	}

	public class NotDecimalUnitErrorRedirectorScope<TDetailQty, TMasterQty> : NotDecimalUnitErrorRedirectorScope<TDetailQty>
		where TDetailQty : IBqlField
		where TMasterQty : IBqlField
	{
		public NotDecimalUnitErrorRedirectorScope(PXCache masterCache, object masterRow) : base(masterCache, masterRow, typeof(TMasterQty))
		{ }
	}

	public class NotDecimalUnitErrorRedirectorScope<TDetail, TDetailQty, TMaster, TMasterQty> : BaseNotDecimalUnitErrorRedirectorScope<TDetailQty>
		where TDetail : class, IBqlTable, new()
		where TDetailQty : IBqlField
		where TMaster : class, IBqlTable, new()
		where TMasterQty : IBqlField
	{
		private Func<TDetail, TMaster> GetMasterRow;

		public NotDecimalUnitErrorRedirectorScope(PXCache masterCache, Func<TDetail, TMaster> getMasterRow): base(masterCache)
		{
			GetMasterRow = getMasterRow ?? throw new PXArgumentException(nameof(getMasterRow));
		}

		protected override void SplitExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs args)
		{
			var notDecimalException = args.Exception as PXNotDecimalUnitException;
			if (notDecimalException != null)
			{
				notDecimalException.IsLazyThrow = true;
				RedirectedSplits[args.Row] = notDecimalException;
				args.Cancel = true;
			}
		}

		protected override void HandleErrors()
		{
			PXNotDecimalUnitException firstError = null;
			PXNotDecimalUnitException linedError = null;
			foreach (var pair in RedirectedSplits)
			{
				var split = (TDetail)pair.Key;
				var error = pair.Value;

				firstError = firstError ?? (error.ErrorLevel >= PXErrorLevel.Error ? error : null);
				TMaster row = GetMasterRow(split);
				if (row != null && string.IsNullOrEmpty(PXUIFieldAttribute.GetErrorOnly<TMasterQty>(MasterCache, row)))
				{
					linedError = linedError ?? (error.ErrorLevel >= PXErrorLevel.Error ? error : null);
					MasterCache.RaiseExceptionHandling<TMasterQty>(row, MasterCache.GetValue<TMasterQty>(row), error);
				}
			}

			if ((linedError ?? firstError) != null)
				throw linedError ?? firstError;
		}
	}
}
