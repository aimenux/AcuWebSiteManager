using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Tools
{
	public class Dumper
	{
		public static string Dump(params object[] items)
		{
			StringBuilder sb = new StringBuilder();

			foreach (object item in items)
			{
				sb.Append(item.Dump());

				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
