using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common
{
	public class Constants
	{
		public const int TranDescLength = 256;

		public class DACName<DAC> : PX.Data.BQL.BqlString.Constant<DACName<DAC>>
			where DAC : IBqlTable
		{
			public DACName() : base(typeof(DAC).FullName) { }
		}
	}
}
