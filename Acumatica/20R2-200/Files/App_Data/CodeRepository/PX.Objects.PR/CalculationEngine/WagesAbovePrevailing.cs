using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class WagesAbovePrevailing
	{
		private decimal _WagesAtPrevailing = 0;
		private decimal _ActualWages = 0;
		private decimal _WageBaseHours = 0;
		
		public decimal ExcessWageAmount => _ActualWages - _WagesAtPrevailing;
		public decimal EffectivePrevailingRate => _WageBaseHours != 0 ? _WagesAtPrevailing / _WageBaseHours : 0m;
		public decimal EffectivePayRate => _WageBaseHours != 0 ? _ActualWages / _WageBaseHours : 0m;

		public void Add(decimal? prevailingRate, decimal? actualWages, decimal? hours)
		{
			_WagesAtPrevailing += prevailingRate.GetValueOrDefault() * hours.GetValueOrDefault();
			_ActualWages += actualWages.GetValueOrDefault();
			_WageBaseHours += hours.GetValueOrDefault();
		}

	}
}
