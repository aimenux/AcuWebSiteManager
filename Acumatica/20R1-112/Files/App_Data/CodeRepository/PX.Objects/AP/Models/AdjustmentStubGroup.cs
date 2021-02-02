using PX.Objects.Common;
using System.Linq;

namespace PX.Objects.AP
{
	public class AdjustmentStubGroup
	{
		public IGrouping<AdjustmentGroupKey, IAdjustmentStub> GroupedStubs { get; set; }
	}
}
