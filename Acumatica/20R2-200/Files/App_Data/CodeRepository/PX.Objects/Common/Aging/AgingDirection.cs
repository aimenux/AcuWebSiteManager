using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Aging
{
	public enum AgingDirection
	{
		/// <summary>
		/// Age test dates backwards from the current date.
		/// This method is usually used for aging overdue 
		/// documents.
		/// </summary>
		Backwards = 0,
		/// <summary>
		/// Age test dates forward from the current date.
		/// This method is usually used for aging outstanding 
		/// documents.
		/// </summary>
		Forward = 1
	}
}
