using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Common;
using PX.Objects.CS;
using PX.Objects.Extensions.PerUnitTax;

namespace PX.Objects.SO
{
	/// <summary>
	/// A per-unit tax graph extension for <see cref="SOOrderEntry"/> which will forbid edit of per-unit taxes in UI.
	/// </summary>
	public class PerUnitTaxDisableExt : PerUnitTaxDataEntryGraphExtension<SOOrderEntry, SOTaxTran>
	{
		public static bool IsActive() => IsActiveBase();
	}
}
