using PX.Data;
using PX.Objects.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.PO;

namespace PX.Objects.PM
{
	/// <summary>
	/// Extends AP Invoice Entry with Project related functionality. Requires Project Accounting feature.
	/// </summary>
	public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
		}

		[PXOverride]
		public virtual void CopyCustomizationFieldsToAPTran(APTran apTranToFill, IAPTranSource poSourceLine, bool areCurrenciesSame)
		{
			if (CopyProjectFromLine(poSourceLine))
			{
				apTranToFill.ProjectID = poSourceLine.ProjectID;
				apTranToFill.TaskID = poSourceLine.TaskID;
			}
			else
			{
				apTranToFill.ProjectID = ProjectDefaultAttribute.NonProject();
			}

			apTranToFill.CostCodeID = poSourceLine.CostCodeID;
		}

		protected virtual bool CopyProjectFromLine(IAPTranSource poSourceLine)
		{
			return true;
		}
	}
}
