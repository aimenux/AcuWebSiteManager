using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// The class allows to display references to various documents without additional queries
	/// to <see cref="Note"/> and other tables and also to create appropriate navigation links.
	/// </summary>
	public class PXRefNoteBaseAttribute : PXRefNoteAttribute
	{
		public class PXLinkState : PXStringState
		{
			protected object[] _keys;
			protected Type _target;

			public object[] keys
			{
				get { return _keys; }
			}

			public Type target
			{
				get { return _target; }
			}

			public PXLinkState(object value)
				: base(value)
			{
			}

			public static PXFieldState CreateInstance(object value, Type target, object[] keys)
			{
				PXLinkState state = value as PXLinkState;
				if (state == null)
				{
					PXFieldState field = value as PXFieldState;
					if (field != null && field.DataType != typeof(object) && field.DataType != typeof(string))
					{
						return field;
					}
					state = new PXLinkState(value);
				}
				state._DataType = typeof(string);
				if (target != null)
				{
					state._target = target;
				}
				if (keys != null)
				{
					state._keys = keys;
				}

				return state;
			}
		}

		public PXRefNoteBaseAttribute()
			: base()
		{
		}

		public virtual object GetEntityRowID(PXCache cache, object[] keys)
		{
			return GetEntityRowID(cache, keys, ", ");
		}

		public static object GetEntityRowID(PXCache cache, object[] keys, string separator)
		{
			StringBuilder result = new StringBuilder();
			int i = 0;
			foreach (string key in cache.Keys)
			{
				if (i >= keys.Length) break;
				object val = keys[i++];
				cache.RaiseFieldSelecting(key, null, ref val, true);

				if (val != null)
				{
					if (result.Length != 0) result.Append(separator);
					result.Append(val.ToString().TrimEnd());
				}
			}
			return result.ToString();
		}
	}
}
