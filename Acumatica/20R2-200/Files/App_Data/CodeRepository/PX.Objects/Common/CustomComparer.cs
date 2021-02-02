using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;

namespace PX.Objects.Common
{
	public class CustomComparer<TDAC>: IEqualityComparer<TDAC>
		where TDAC: IBqlTable
	{
		private Func<object, int> _getHashCodeDelegate;

		private Func<object, object, bool> _equalsDelegate;

		public CustomComparer(Func<object, int> getHashCodeDelegate, Func<object, object, bool> equalsDelegate)
		{
			if (getHashCodeDelegate == null)
				throw new ArgumentNullException(nameof(getHashCodeDelegate));
			if (equalsDelegate == null)
				throw new ArgumentNullException(nameof(equalsDelegate));

			_getHashCodeDelegate = getHashCodeDelegate;
			_equalsDelegate = equalsDelegate;
		}

		public bool Equals(TDAC x, TDAC y)
		{
			return _equalsDelegate(x, y);
		}

		public int GetHashCode(TDAC obj)
		{
			return _getHashCodeDelegate(obj);
		}
	}
}
