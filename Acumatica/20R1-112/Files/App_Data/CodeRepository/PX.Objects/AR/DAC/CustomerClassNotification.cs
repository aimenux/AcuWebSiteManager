using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.SM;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.RQ;

namespace PX.Objects.AR
{	
	public class CustomerContactType : NotificationContactType
	{
        /// <summary>
        /// Defines a list of the possible ContactType for the AR Customer <br/>
        /// Namely: Primary, Billing, Shipping, Employee <br/>
        /// Mostly, this attribute serves as a container <br/>
        /// </summary>		
		public class ClassListAttribute : PXStringListAttribute
		{
			public ClassListAttribute()
				: base(new string[] { Primary, Billing, Shipping, Employee },
							 new string[] { CR.Messages.Primary, Messages.Billing, Messages.Shipping, EP.Messages.Employee })
			{
			}
		}
		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Primary, Billing, Shipping, Employee, Contact },
							 new string[] { CR.Messages.Primary, Messages.Billing, Messages.Shipping, EP.Messages.Employee, CR.Messages.Contact })
			{
			}
		}
	}
}
