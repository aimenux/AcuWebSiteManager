using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AP
{	
	public class VendorContactType : NotificationContactType
	{
		public class ClassListAttribute : PXStringListAttribute
		{
			public ClassListAttribute()
				: base(new string[] { Primary, Remittance, Shipping, Employee },
							 new string[] { CR.Messages.Primary, Messages.Remittance, Messages.Shipping, EP.Messages.Employee })
			{
			}
		}
		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Primary, Remittance, Shipping, Employee, Contact },
							 new string[] { CR.Messages.Primary, Messages.Remittance, Messages.Shipping, EP.Messages.Employee, CR.Messages.Contact })
			{
			}
		}
	}	
}
