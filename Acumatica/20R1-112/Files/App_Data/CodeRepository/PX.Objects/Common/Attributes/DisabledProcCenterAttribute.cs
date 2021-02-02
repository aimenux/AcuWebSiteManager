using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Data;
namespace PX.Objects.Common.Attributes
{
	/// <summary>The attribute that informs a user that the processing center is deactivated.</summary>
	public class DisabledProcCenterAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
	{
		/// <summary>Defines the types of the values of the field that is annotated with the attribute.</summary>
		public enum CheckFieldVal
		{
			PmInstanceId,
			ProcessingCenterId,
		}

		public CheckFieldVal CheckFieldValue = CheckFieldVal.PmInstanceId;
		/// <summary>The DAC field name that is used to display the error message.</summary>
		public string ErrorMappedFieldName { get; set; }

		private bool errorRised;

		public DisabledProcCenterAttribute() : base()
		{

		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			string name = this.FieldName;
			string marked = ErrorMappedFieldName != null ? ErrorMappedFieldName : name;
			object val = sender.GetValue(e.Row, name);
			int? id = val as int?;
			if (CheckFieldValue == CheckFieldVal.PmInstanceId && id != null)
			{
				if (IsDisabledProcCenter(sender, id))
				{
					sender.RaiseExceptionHandling(marked, e.Row, id, new PXSetPropertyException(AR.Messages.PaymentProfileInactiveProcCenter, PXErrorLevel.Warning));
					errorRised = true;
				}
				else
				{
					if (errorRised)
					{
						sender.RaiseExceptionHandling(marked, e.Row, null, null);
						errorRised = false;
					}
				}
			}

			string procCenterId = val as string;
			if (CheckFieldValue == CheckFieldVal.ProcessingCenterId && procCenterId != null)
			{
				if (IsDisabledProcCenter(sender, procCenterId))
				{
					sender.RaiseExceptionHandling(marked, e.Row, procCenterId, new PXSetPropertyException(CA.Messages.ProcessingCenterInactive, PXErrorLevel.Warning));
					errorRised = true;
				}
				else
				{
					if (errorRised)
					{
						sender.RaiseExceptionHandling(marked, e.Row, null, null);
						errorRised = false;
					}
				}
			}

		}

		private bool IsDisabledProcCenter(PXCache sender, string procCenterId)
		{
			PXGraph graph = sender.Graph;
			CCProcessingCenter procCenter = new PXSelect<CCProcessingCenter, 
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>(graph).SelectSingle(procCenterId);
			if (procCenter != null && procCenter.IsActive == false)
			{
				return true;
			}
			return false;
		}

		private bool IsDisabledProcCenter(PXCache sender, int? pmInstanceId)
		{
			PXGraph graph = sender.Graph;
			if (pmInstanceId == PaymentTranExtConstants.NewPaymentProfile)
			{
				return false;
			}
			CCProcessingCenter procCenter = new PXSelectJoin<CCProcessingCenter, 
				InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.cCProcessingCenterID ,Equal<CCProcessingCenter.processingCenterID>>>,
				Where<CustomerPaymentMethod.pMInstanceID,Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>(graph).SelectSingle(pmInstanceId);
			if (procCenter != null && procCenter.IsActive == false)
			{
				return true;
			}
			return false;
		}
	}
}
