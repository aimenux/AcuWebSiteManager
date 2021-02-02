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
	/// A per-unit tax graph extension for <see cref="ARInvoiceEntry"/> which will forbid edit of per-unit taxes in UI.
	/// </summary>
	public class ARInvoiceEntryPerUnitTaxDisableExt : PerUnitTaxDataEntryGraphExtension<ARInvoiceEntry, ARTaxTran>
	{
		public static bool IsActive() => IsActiveBase();
	}
}
