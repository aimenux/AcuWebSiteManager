namespace PX.Objects.AR.CCPaymentProcessing.Specific
{
	public static class AuthnetProcCenterTypeName
	{
		public class APIPluginFullName : PX.Data.BQL.BqlString.Constant<APIPluginFullName>
		{
			public APIPluginFullName() : base(AuthnetConstants.APIPluginFullName) { }
		}
		public class CIMPluginFullName : PX.Data.BQL.BqlString.Constant<CIMPluginFullName>
		{
			public CIMPluginFullName() : base(AuthnetConstants.CIMPluginFullName) { }
		}
		public class AIMPluginFullName : PX.Data.BQL.BqlString.Constant<AIMPluginFullName>
		{
			public AIMPluginFullName() : base(AuthnetConstants.AIMPluginFullName) { }
		}
	}
}
