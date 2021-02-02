using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.TX
{
	public class TaxCalculationMode
	{
		public const string Net = "N";
		public const string Gross = "G";
		public const string TaxSetting = "T";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(new[] { TaxSetting, Gross, Net }, new[] { AP.Messages.TaxSetting, AP.Messages.TaxGross, AP.Messages.TaxNet }) { }
		}

		public class gross : PX.Data.BQL.BqlString.Constant<gross>
		{
			public gross() : base(Gross) { }
		}

		public class net : PX.Data.BQL.BqlString.Constant<net>
		{
			public net() : base(Net) { }
		}

		public class taxSetting : PX.Data.BQL.BqlString.Constant<taxSetting>
		{
			public taxSetting() : base(TaxSetting) { }
		}
	}
}
