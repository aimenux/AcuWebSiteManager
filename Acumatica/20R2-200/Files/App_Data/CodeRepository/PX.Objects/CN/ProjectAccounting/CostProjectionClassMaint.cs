using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting
{
	public class CostProjectionClassMaint : PXGraph<CostProjectionClassMaint>
	{
		[PXImport(typeof(PMCostProjectionClass))]
		public PXSelect<PMCostProjectionClass> Items;
		public PXSavePerRow<PMCostProjectionClass> Save;
		public PXCancel<PMCostProjectionClass> Cancel;

		protected virtual void _(Events.FieldVerifying<PMCostProjectionClass, PMCostProjectionClass.accountGroupID> e)
		{
			if (e.Row.AccountGroupID != (bool?)e.NewValue)
			{
				VerifyAndRaiseException();
			}
		}

		protected virtual void _(Events.FieldVerifying<PMCostProjectionClass, PMCostProjectionClass.taskID> e)
		{
			if (e.Row.TaskID != (bool?)e.NewValue)
			{
				VerifyAndRaiseException();
			}
		}

		protected virtual void _(Events.FieldVerifying<PMCostProjectionClass, PMCostProjectionClass.inventoryID> e)
		{
			if (e.Row.InventoryID != (bool?)e.NewValue)
			{
				VerifyAndRaiseException();
			}
		}

		protected virtual void _(Events.FieldVerifying<PMCostProjectionClass, PMCostProjectionClass.costCodeID> e)
		{
			if (e.Row.CostCodeID != (bool?)e.NewValue)
			{
				VerifyAndRaiseException();
			}
		}

		protected virtual void VerifyAndRaiseException()
		{
			var select = new PXSelect<PMCostProjection, Where<PMCostProjection.classID, Equal<Current<PMCostProjectionClass.classID>>>>(this);

			PMCostProjection first = select.SelectWindowed(0, 1);

			if (first != null)
			{
				throw new PXSetPropertyException(ProjectAccountingMessages.CostProjectionClassIsNotValid);
			}
		}
	}
}
