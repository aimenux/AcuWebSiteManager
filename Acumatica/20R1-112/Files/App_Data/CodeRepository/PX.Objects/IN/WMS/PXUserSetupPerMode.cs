using PX.Data;
using PX.Objects.Extensions;

namespace PX.Objects.IN
{
	public class PXUserSetupPerMode<TSelf, TGraph, THeader, TSetup, TUserIDField, TModeField, TModeValueField>
		: PXSetupBase<TSelf, TGraph, THeader, TSetup,
			Where<TUserIDField, Equal<Current<AccessInfo.userID>>, And<TModeField, Equal<TModeValueField>>>>
		where TSelf : PXUserSetupPerMode<TSelf, TGraph, THeader, TSetup, TUserIDField, TModeField, TModeValueField>
		where TGraph : PXGraph
		where THeader : class, IBqlTable, new()
		where TSetup : class, IBqlTable, new()
		where TUserIDField : IBqlField
		where TModeField : class, IBqlField
		where TModeValueField : IConstant, IBqlOperand, new()
	{
		public virtual void _(Events.FieldDefaulting<TSetup, TModeField> e) => e.NewValue = new TModeValueField().Value;
	}
}
