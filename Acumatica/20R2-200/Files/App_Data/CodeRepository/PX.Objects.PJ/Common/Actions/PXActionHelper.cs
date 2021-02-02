using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.PJ.Common.Actions
{
	public static class PXActionHelper
	{
		public static void InsertNoKeysFromUI(PXAdapter adapter)
		{
			Dictionary<string, object> vals = new Dictionary<string, object>();
			if (adapter.View.Cache.Insert(vals) == 1)
			{
				if (adapter.SortColumns == null)
				{
					adapter.SortColumns = adapter.View.Cache.Keys.ToArray();
				}
				else
				{
					List<string> cols = new List<string>(adapter.SortColumns);
					foreach (string key in adapter.View.Cache.Keys)
					{
						if (!CompareIgnoreCase.IsInList(cols, key))
						{
							cols.Add(key);
						}
					}
					adapter.SortColumns = cols.ToArray();
				}
				adapter.Searches = new object[adapter.SortColumns.Length];
				for (int i = 0; i < adapter.Searches.Length; i++)
				{
					object val;
					if (vals.TryGetValue(adapter.SortColumns[i], out val))
					{
						adapter.Searches[i] = val is PXFieldState ? ((PXFieldState)val).Value : val;
					}
				}
				adapter.StartRow = 0;
			}
		}
	}
}
