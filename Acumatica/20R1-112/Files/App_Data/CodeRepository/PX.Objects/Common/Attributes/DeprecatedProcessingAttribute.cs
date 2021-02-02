
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using System.Linq;
namespace PX.Objects.Common.Attributes
{
	/// <summary>The attribute that informs a user that the processing center uses an unsupported plug-in.</summary>
	public class DeprecatedProcessingAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
	{
		/// <summary>Defines the types of the values of the field that is annotated with the attribute.</summary>
		public enum CheckVal
		{
			PmInstanceId,
			ProcessingCenterId,
			ProcessingCenterType
		}

		public CheckVal ChckVal { get; set; } = CheckVal.PmInstanceId;

		private bool errorRised;

		public DeprecatedProcessingAttribute() : base()
		{

		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			string name = this.FieldName;
			object val = sender.GetValue(e.Row, name);

			if (ChckVal == CheckVal.PmInstanceId)
			{
				int? id = val as int?;
				if (id != null && id > 0 && IsProcessingCenterNotSupported(sender.Graph, id))
				{
					CCProcessingCenter procCenter = GetProcessingCenterByPMInstance(sender.Graph, id);
					sender.RaiseExceptionHandling(name, e.Row, id, new PXSetPropertyException(AR.Messages.PaymentProfileProcCenterNotSupported, PXErrorLevel.Warning, procCenter?.ProcessingCenterID));
					errorRised = true;
				}
				else
				{
					if (errorRised)
					{
						sender.RaiseExceptionHandling(name, e.Row, null, null);
						errorRised = false;
					}
				}
			}

			if (ChckVal == CheckVal.ProcessingCenterId)
			{
				string procCenterId = val as string;
				if (procCenterId != null && IsProcessingCenterNotSupported(sender.Graph, procCenterId))
				{
					sender.RaiseExceptionHandling(name, e.Row, procCenterId, new PXSetPropertyException(AR.Messages.PaymentProfileProcCenterNotSupported, PXErrorLevel.Warning, procCenterId));
					errorRised = true;
				}
				else
				{
					if (errorRised)
					{
						sender.RaiseExceptionHandling(name, e.Row, null, null);
						errorRised = false;
					}
				}
			}

			if (ChckVal == CheckVal.ProcessingCenterType)
			{
				string typeStr = val as string;
				if (typeStr != null && CCPluginTypeHelper.IsUnsupportedPluginType(typeStr))
				{
					sender.RaiseExceptionHandling(name, e.Row, typeStr, new PXSetPropertyException(CA.Messages.NotSupportedProcCenter, PXErrorLevel.Warning));
					errorRised = true;
				}
				else
				{
					if (errorRised)
					{
						sender.RaiseExceptionHandling(name, e.Row, null, null);
						errorRised = false;
					}
				}
			}
		}

		public static bool IsProcessingCenterNotSupported(PXGraph graph, string procCenterId)
		{
			if (procCenterId == null)
			{
				return false;
			}
			CCProcessingCenter processingCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>
				.Select(graph, procCenterId);

			if (processingCenter == null)
			{
				return false;
			}
			return CCPluginTypeHelper.IsUnsupportedPluginType(processingCenter.ProcessingTypeName);
		}

		public static bool IsProcessingCenterNotSupported(PXGraph graph, int? pmInstanceID)
		{
			if (pmInstanceID == null)
			{
				return false;
			}
			CCProcessingCenter processingCenter = GetProcessingCenterByPMInstance(graph, pmInstanceID);
			if (processingCenter == null)
			{
				return false;
			}
			return CCPluginTypeHelper.IsUnsupportedPluginType(processingCenter?.ProcessingTypeName);
		}

		private static CCProcessingCenter GetProcessingCenterByPMInstance(PXGraph graph, int? pmInstanceID)
		{
			CCProcessingCenter processingCenter = (CCProcessingCenter)PXSelectJoin<CCProcessingCenter,
				InnerJoin<CustomerPaymentMethod, On<CCProcessingCenter.processingCenterID, Equal<CustomerPaymentMethod.cCProcessingCenterID>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>
				.Select(graph, pmInstanceID);
			return processingCenter;
		}
	}
}
