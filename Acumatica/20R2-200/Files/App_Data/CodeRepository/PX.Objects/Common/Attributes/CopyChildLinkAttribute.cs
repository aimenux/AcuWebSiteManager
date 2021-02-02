using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Attributes
{
	public class CopyChildLinkAttribute : PXUnboundFormulaAttribute
	{
		#region Members

		protected Type _counterField;
		protected List<Type> _linkChildKeys;
		protected List<Type> _linkParentKeys;
		protected Type _parentType;
		protected Type _amountField;

		#endregion // Members

		#region Initialize

		public CopyChildLinkAttribute(Type counterField, Type amountField, Type[] linkChildKeys, Type[] linkParentKeys)
			: base(typeof(IIf<,,>).MakeGenericType(typeof(Where<,>).MakeGenericType(amountField, typeof(Equal<decimal0>)), typeof(Zero), typeof(One)),
					typeof(SumCalc<>).MakeGenericType(counterField))
		{
			_counterField = counterField ?? throw new ArgumentNullException(nameof(counterField));
			_amountField = amountField ?? throw new ArgumentNullException(nameof(amountField));
			_linkChildKeys = linkChildKeys?.ToList() ?? throw new ArgumentNullException(nameof(linkChildKeys));
			_linkParentKeys = linkChildKeys?.ToList() ?? throw new ArgumentNullException(nameof(linkParentKeys));

			if (_linkChildKeys.Count == 0 || _linkParentKeys.Count == 0 || _linkChildKeys.Count != _linkParentKeys.Count)
				throw new ArgumentOutOfRangeException(nameof(linkParentKeys));

			if (!typeof(IBqlField).IsAssignableFrom(_counterField))
				throw new PXArgumentException(nameof(_counterField), ErrorMessages.InvalidField, _counterField.Name);

			_parentType = BqlCommand.GetItemType(_counterField);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			ForcePersistParent(sender);
		}

		protected virtual void ForcePersistParent(PXCache sender)
		{
			if (!sender.Graph.Views.Caches.Contains(_parentType))
			{
				sender.Graph.Views.Caches.Add(_parentType);
			}
		}

		#endregion Initialize

		#region Events

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.RowInserted(sender, e);

			if (e.Row == null)
				return;

			OnRowInserted(sender, e.Row);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			base.RowDeleted(sender, e);

			if (e.Row == null)
				return;

			OnRowDeleted(sender, e.Row);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.RowUpdated(sender, e);

			if (e.Row == null)
				return;

			OnRowUpdated(sender, e.Row, e.OldRow);
		}

		#endregion // Events

		#region Implementation

		#region OnRowInserted / OnRowDeleted / OnRowUpdated

		protected virtual void OnRowInserted(PXCache sender, object row)
		{
			if (IsAmountZero(sender, row))
				return;
			object parentRow = PXParentAttribute.SelectParent(sender, row, _parentType);
			if (parentRow == null)
				return;

			var counterValue = GetCounterValue(sender.Graph, parentRow);
			if (counterValue == 1)
			{
				var childKeys = GetChildLinkKeys(sender, row);
				SetParentLinkKeys(sender, row, childKeys, parentRow);
			}
			else if (counterValue > 1)
			{
				var childKeys = GetChildLinkKeys(sender, row);
				var parentKeys = GetParentLinkKeys(sender, row, parentRow);
				if (!childKeys.SequenceEqual(parentKeys) && parentKeys.Any(k => k != null))
				{
					var newKeys = parentKeys.Select(k => (object)null).ToArray();
					SetParentLinkKeys(sender, row, newKeys, parentRow);
				}
			}
			else
			{
				OnWrongCounter(sender, row, counterValue);
			}
		}

		protected virtual void OnRowDeleted(PXCache sender, object row)
		{
			if (IsAmountZero(sender, row))
				return;

			var parentRow = PXParentAttribute.SelectParent(sender, row, _parentType);
			if (parentRow == null)
				return;

			var counterValue = GetCounterValue(sender.Graph, parentRow);
			if (counterValue == 0)
			{
				var newKeys = _linkParentKeys.Select(k => (object)null).ToArray();
				SetParentLinkKeys(sender, row, newKeys, parentRow);
			}
			else if (counterValue > 0)
			{
				var childKeys = GetChildLinkKeys(sender, row);
				var parentKeys = GetParentLinkKeys(sender, row, parentRow);
				if (!childKeys.SequenceEqual(parentKeys) || parentKeys.Any(k => k == null))
				{
					VerifyAllChildren(sender, parentRow);
				}
			}
			else
			{
				OnWrongCounter(sender, row, counterValue);
			}
		}
		
		protected virtual void OnRowUpdated(PXCache sender, object row, object oldRow)
		{
			if (oldRow != null)
			{
				if (IsAmountZero(sender, row) && IsAmountZero(sender, oldRow))
					return;

				if (IsAmountZero(sender, row) != IsAmountZero(sender, oldRow))
				{
					OnRowDeleted(sender, oldRow);
					OnRowInserted(sender, row);
					return;
				}

				var childKeys = GetChildLinkKeys(sender, row);

				var parentRow = PXParentAttribute.SelectParent(sender, row, _parentType);
				if (parentRow == null)
					return;

				var counterValue = GetCounterValue(sender.Graph, parentRow);
				if (counterValue == 1)
				{
					SetParentLinkKeys(sender, row, childKeys, parentRow);
					return;
				}
				else if (counterValue > 1)
				{
					var oldChildKeys = GetChildLinkKeys(sender, oldRow);
					if (childKeys.SequenceEqual(oldChildKeys))
						return;

					var oldParentRow = PXParentAttribute.SelectParent(sender, oldRow, _parentType);

					if (parentRow != oldParentRow)
					{
						OnRowDeleted(sender, oldRow);
						OnRowInserted(sender, row);
						return;
					}

					var parentKeys = GetParentLinkKeys(sender, row, parentRow);
					if (parentKeys.SequenceEqual(oldChildKeys) && parentKeys.All(k => k != null))
					{
						var newKeys = _linkParentKeys.Select(k => (object)null).ToArray();
						SetParentLinkKeys(sender, row, newKeys, parentRow);
					}
					else
					{
						VerifyAllChildren(sender, parentRow);
					}
				}
				else
				{
					OnWrongCounter(sender, row, counterValue);
				}
			}
			else
			{
				VerifyAllChildren(sender, null);
			}
		}

		#endregion // OnRowInserted / OnRowDeleted / OnRowUpdated

		#region Get/Set values

		protected virtual bool IsAmountZero(PXCache cache, object row)
			=> ((decimal?)cache.GetValue(row, _amountField.Name) ?? 0m) == 0m;

		protected virtual int? GetCounterValue(PXGraph graph, object row)
			=> (int?)graph.Caches[_parentType].GetValue(row, _counterField.Name);

		protected virtual object[] GetChildLinkKeys(PXCache cache, object row)
			=> _linkChildKeys.Select(k => cache.GetValue(row, k.Name)).ToArray();

		protected virtual object[] GetParentLinkKeys(PXCache childCache, object childRow, out object parentRow)
		{
			var parent = parentRow = PXParentAttribute.SelectParent(childCache, childRow, _parentType);

			return GetParentLinkKeys(childCache, childRow, parentRow);
		}

		protected virtual object[] GetParentLinkKeys(PXCache childCache, object childRow, object parentRow)
		{
			PXCache parentCache = childCache.Graph.Caches[_parentType];

			return _linkParentKeys.Select(k => parentCache.GetValue(parentRow, k.Name)).ToArray();
		}

		protected virtual object SetParentLinkKeys(PXCache childCache, object childRow, object[] values, object parentRow)
		{
			PXCache parentCache = childCache.Graph.Caches[_parentType];

			object parentCopy = parentCache.CreateCopy(parentRow);
			bool changed = false;

			foreach (var keyValue in _linkParentKeys.Zip(values, (k, v) => new { Key = k, Value = v }))
			{
				if (!object.Equals(parentCache.GetValue(parentCopy, keyValue.Key.Name), keyValue.Value))
				{
					parentCache.SetValueExt(parentCopy, keyValue.Key.Name, keyValue.Value);
					changed = true;
				}
			}

			if (changed)
			{
				return parentCache.Update(parentCopy);
			}
			else
			{
				return parentRow;
			}
		}

		#endregion // Get/Set values

		#region Recalculation

		protected virtual int VerifyAllChildren(PXCache childCache, object parentRow)
		{
			object[] firstValues = null;
			bool theSameValues = false;
			int rowCounter = 0;

			foreach (var child in PXParentAttribute.SelectChildren(childCache, parentRow, _parentType))
			{
				if (IsAmountZero(childCache, child))
					continue;

				var childKeys = GetChildLinkKeys(childCache, child);
				if (firstValues != null && !firstValues.SequenceEqual(childKeys))
				{
					theSameValues = false;
					break;
				}
				else
				{
					theSameValues = true;
					firstValues = childKeys;
				}
				rowCounter++;
			}

			var newKeys = theSameValues ? firstValues : _linkParentKeys.Select(k => (object)null).ToArray();

			SetParentLinkKeys(childCache, null, newKeys, parentRow);

			return rowCounter;
		}

		protected virtual void OnWrongCounter(PXCache cache, object row, int? currentValue,
			[System.Runtime.CompilerServices.CallerMemberName] string memberName = null)
		{
			PXCache parentCache = cache.Graph.Caches[_parentType];
			var parentRow = PXParentAttribute.SelectParent(cache, row, _parentType);

			var rowCount = VerifyAllChildren(cache, parentRow);

			string counterError = string.Concat(nameof(CopyChildLinkAttribute), ".", memberName ?? nameof(OnWrongCounter), ": ",
				PXLocalizer.LocalizeFormat(ErrorMessages.ErrorFieldValueProcessing, _counterField.Name, currentValue, rowCount));
			PXTrace.WriteError(counterError);

			object parentCopy = parentCache.CreateCopy(parentRow);
			parentCache.SetValueExt(parentRow, _counterField.Name, rowCount);
			parentRow = parentCache.Update(parentCopy);
		}

		#endregion // Recalculation

		#endregion // Implementation
	}
}
