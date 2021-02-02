using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class ProformaAutoNumberAttribute : AutoNumberAttribute
	{
		public ProformaAutoNumberAttribute() : base(typeof(PMSetup.proformaNumbering), typeof(AccessInfo.businessDate)) { }
		
		public bool Disable { get; set; }

		public static void DisableAutonumbiring(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<PMProforma.refNbr>())
			{
				if (attr is ProformaAutoNumberAttribute)
				{
					((ProformaAutoNumberAttribute)attr).Disable = true;
					((ProformaAutoNumberAttribute)attr).UserNumbering = true;
				}
			}
		}

		protected override string GetNewNumberSymbol(string numberingID)
		{
			if (Disable)
				return NullString;

			return base.GetNewNumberSymbol(numberingID);
		}
	}
}
