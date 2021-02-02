using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.Submittals.PJ.DAC;

namespace PX.Objects.PM
{
	public class SubmittalAutoNumberAttribute : AutoNumberAttribute
	{
		public SubmittalAutoNumberAttribute() : base(typeof(ProjectManagementSetup.submittalNumberingId), typeof(PJSubmittal.dateCreated)) { }

		public bool Disable { get; set; }

		public static void DisableAutonumbiring<TField>(PXCache cache)
			where TField : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<TField>(cache))
			{
				if (attr is SubmittalAutoNumberAttribute)
				{
					((SubmittalAutoNumberAttribute)attr).Disable = true;
					((SubmittalAutoNumberAttribute)attr).UserNumbering = true;
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