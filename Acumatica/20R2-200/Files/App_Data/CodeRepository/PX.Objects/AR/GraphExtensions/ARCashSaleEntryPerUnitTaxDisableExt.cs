using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Common;
using PX.Objects.CS;
using PX.Objects.Extensions.PerUnitTax;

namespace PX.Objects.AR
{
	/// <summary>
	/// A per-unit tax graph extension for <see cref="ARCashSaleEntry"/> which will forbid edit of per-unit taxes in UI.
	/// </summary>
	public class ARCashSaleEntryPerUnitTaxDisableExt : PerUnitTaxDataEntryGraphExtension<ARCashSaleEntry, ARTaxTran>
	{
		public static bool IsActive() => IsActiveBase();
	}
}
