using PX.Data;

namespace PX.Objects.Extensions
{
	public class PXUserSetup<TSelf, TGraph, THeader, TSetup, TUserIDField>
		: PXSetupBase<TSelf, TGraph, THeader, TSetup, Where<TUserIDField, Equal<Current<AccessInfo.userID>>>>
		where TSelf : PXUserSetup<TSelf, TGraph, THeader, TSetup, TUserIDField>
		where TGraph : PXGraph
		where THeader : class, IBqlTable, new()
		where TSetup : class, IBqlTable, new()
		where TUserIDField : IBqlField
	{
	}
}