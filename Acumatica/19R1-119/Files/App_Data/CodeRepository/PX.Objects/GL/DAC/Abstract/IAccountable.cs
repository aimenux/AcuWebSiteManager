using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.GL.DAC.Abstract
{
	public interface IAccountable
	{
		string FinPeriodID { get; set; }

		int? BranchID { get; set; }
	}
}
