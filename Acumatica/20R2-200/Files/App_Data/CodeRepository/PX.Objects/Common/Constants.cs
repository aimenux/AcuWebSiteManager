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
		public const string ProjectsWorkspaceID = "6dbfa68e-79e9-420b-9f64-e1036a28998c";
		public const string ProjectsWorkspaceIcon = "project";
		public const string ConstructionWorkspaceIcon = "cran";

		public class DACName<DAC> : PX.Data.BQL.BqlString.Constant<DACName<DAC>>
			where DAC : IBqlTable
		{
			public DACName() : base(typeof(DAC).FullName) { }
		}
	}
}
