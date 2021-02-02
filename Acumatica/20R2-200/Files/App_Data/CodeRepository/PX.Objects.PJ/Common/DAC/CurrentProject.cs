using PX.Data;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PJ.Common.DAC
{
	[Serializable]
	public class CurrentProject : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : IBqlField { }

		[PXInt]
		public int? ProjectID
		{
			get;
			set;
		}
		#endregion
	}
}
