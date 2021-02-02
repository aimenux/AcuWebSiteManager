using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.Common.DAC.ReportParameters
{
	public sealed class CustomerReportParametersVisibilityRestriction : PXCacheExtension<CustomerReportParameters>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerClassID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerClassByUserBranches]
		public string CustomerClassID { get; set; }
		#endregion

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? CustomerID { get; set; }
		#endregion

		#region CustomerIDByCustomerClass
		public abstract class customerIDByCustomerClass : PX.Data.BQL.BqlInt.Field<customerIDByCustomerClass> { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDimensionSelector(CustomerAttribute.DimensionName,
			typeof(Search<Customer.bAccountID,
				Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
				And<Where<Customer.customerClassID, Equal<Optional<CustomerReportParameters.customerClassID>>,
					Or<Optional<CustomerReportParameters.customerClassID>, IsNull>>>>>),
				typeof(BAccountR.acctCD),
				typeof(BAccountR.acctCD),
				typeof(Customer.acctName),
				typeof(Customer.customerClassID),
				typeof(Customer.status),
				typeof(Contact.phone1),
				typeof(Address.city),
				typeof(Address.countryID))]
		public int? CustomerIDByCustomerClass { get; set; }
		#endregion
	}
}
