using PX.Data;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.PhysicalInventory
{
	public class BqlCommandWithParameters
	{
		public BqlCommand Command { get; set; }
		public List<object> JoinParameters { get; set; }
		public List<object> WhereParameters { get; set; }

		public BqlCommandWithParameters()
			: this(null)
		{
		}

		public BqlCommandWithParameters(BqlCommand command)
		{
			Command = command;
			JoinParameters = new List<object>();
			WhereParameters = new List<object>();
		}

		public object[] GetParameters()
		{
			return JoinParameters.Union(WhereParameters).ToArray();
		}
	}
}
