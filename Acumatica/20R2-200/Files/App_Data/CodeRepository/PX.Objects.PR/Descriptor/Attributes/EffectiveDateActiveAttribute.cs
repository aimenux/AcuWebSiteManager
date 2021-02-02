using PX.Data;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	[PXBool]
	[PXUIField(DisplayName = "Active", Enabled = false)]
	public class EffectiveDateActiveAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber
	{
		private readonly Type _EffectiveDateField;
		private readonly BqlCommand _InactiveIfEmpty;
		private readonly Type[] _QueryParameters;

		public EffectiveDateActiveAttribute(Type effectiveDateField, Type inactiveIfEmptyQuery = null, params Type[] queryParameters)
		{
			_EffectiveDateField = effectiveDateField;

			if (inactiveIfEmptyQuery != null)
			{
				_InactiveIfEmpty = BqlCommand.CreateInstance(inactiveIfEmptyQuery);
				_QueryParameters = queryParameters;
			}
			else
			{
				_InactiveIfEmpty = null;
				_QueryParameters = null;
			}
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (_InactiveIfEmpty != null)
			{
				List<object> paramValues = new List<object>();
				if (_QueryParameters != null)
				{
					foreach (Type paramField in _QueryParameters)
					{
						paramValues.Add(sender.GetValue(e.Row, paramField.Name));
					}
				}

				if (!(new PXView(sender.Graph, true, _InactiveIfEmpty).SelectMulti(paramValues.ToArray()).Any()))
				{
					e.ReturnValue = false;
					return; 
				}
			}

			BqlCommand select = BqlCommand.CreateInstance(BqlCommand.Compose(
				typeof(Select<,,>),
				_EffectiveDateField.DeclaringType,
				typeof(Where<,>),
				_EffectiveDateField,
				typeof(LessEqual<>),
				typeof(Current<>),
				typeof(AccessInfo.businessDate),
				typeof(OrderBy<>),
				typeof(Desc<>),
				_EffectiveDateField));

			Func<object, bool> filter = result =>
			{
				foreach (string keyField in sender.Keys)
				{
					if (keyField.Equals(_EffectiveDateField.Name, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					if (sender.GetValue(e.Row, keyField)?.Equals(sender.GetValue(result, keyField)) != true)
					{
						return false;
					}
				}

				return true;
			};

			DateTime? latestValidDate = sender.GetValue(new PXView(sender.Graph, true, select).SelectMulti().Where(filter).FirstOrDefault(), _EffectiveDateField.Name) as DateTime?;
			DateTime? rowEffectiveDate = sender.GetValue(e.Row, _EffectiveDateField.Name) as DateTime?;
			bool isActive = latestValidDate?.Date == rowEffectiveDate?.Date;
			e.ReturnValue = isActive;
		}
	}
}
