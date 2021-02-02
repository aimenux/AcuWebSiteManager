using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.Common.DAC.ReportParameters
{
	[PXHidden]
	public class CustomerReportParameters : IBqlTable
	{
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlInt.Field<customerClassID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CustomerClass.customerClassID>))]
		public string CustomerClassID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[Customer()]
		public int? CustomerID { get; set; }
		#endregion

		#region CustomerIDByCustomerClass
		public abstract class customerIDByCustomerClass : PX.Data.BQL.BqlInt.Field<customerIDByCustomerClass> { }

		[PXDBInt]
		[PXDimensionSelector(CustomerAttribute.DimensionName, typeof(Search<
			Customer.bAccountID, 
			Where<Customer.customerClassID, Equal<Optional<CustomerReportParameters.customerClassID>>,
				Or<Optional<CustomerReportParameters.customerClassID>, IsNull>>>),
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
