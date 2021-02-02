using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.CA;
using System;
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

		/// <summary>The DAC field name that is used to display the error message.</summary>
		public string ErrorMappedFieldName { get; set; }

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
			string errorField = ErrorMappedFieldName ?? name;
			object val = sender.GetValue(e.Row, name);

			if (ChckVal == CheckVal.PmInstanceId)
			{
				int? id = val as int?;
				if (id > 0 && IsProcessingCenterNotSupported(sender.Graph, id, out var result))
				{
					CCProcessingCenter procCenter = GetProcessingCenterByPMInstance(sender.Graph, id);
					string message = InterpretPluginCheckResult(result);
					sender.RaiseExceptionHandling(errorField, e.Row, id, new PXSetPropertyException(message, PXErrorLevel.Warning, procCenter?.ProcessingCenterID));
					errorRised = true;
				}
				else
				{
					if (errorRised)
					{
						sender.RaiseExceptionHandling(errorField, e.Row, null, null);
						errorRised = false;
					}
				}
			}

			if (ChckVal == CheckVal.ProcessingCenterId)
			{
				string procCenterId = val as string;
				if (procCenterId != null && IsProcessingCenterNotSupported(sender.Graph, procCenterId, out var result))
				{
					string message = InterpretPluginCheckResult(result);
					sender.RaiseExceptionHandling(errorField, e.Row, procCenterId, new PXSetPropertyException(message, PXErrorLevel.Warning, procCenterId));
					errorRised = true;
				}
				else
				{
					if (errorRised)
					{
						sender.RaiseExceptionHandling(errorField, e.Row, null, null);
						errorRised = false;
					}
				}
			}

			if (ChckVal == CheckVal.ProcessingCenterType)
			{
				string typeStr = val as string;
				if (typeStr != null && IsProcessingCenterNotSupported(typeStr, out var result))
				{
					string message = InterpretPluginTypeCheckResult(result);
					sender.RaiseExceptionHandling(errorField, e.Row, typeStr, new PXSetPropertyException(message, PXErrorLevel.Warning, string.Empty));
					errorRised = true;
				}
				else
				{
					if (errorRised)
					{
						sender.RaiseExceptionHandling(errorField, e.Row, null, null);
						errorRised = false;
					}
				}
			}
		}
		
		private static string InterpretPluginCheckResult(CCPluginCheckResult result)
		{
			switch (result)
			{
				case CCPluginCheckResult.Empty:
					return AR.Messages.ERR_PluginTypeIsNotSelectedForProcessingCenter;
				case CCPluginCheckResult.Missing:
					return AR.Messages.PaymentProfileProcCenterMissing;
				case CCPluginCheckResult.Unsupported:
					return AR.Messages.PaymentProfileProcCenterNotSupported;
			}
			return string.Empty;
		}

		private static string InterpretPluginTypeCheckResult(CCPluginCheckResult result)
		{
			switch (result)
			{
				case CCPluginCheckResult.Empty:
					return AR.Messages.ERR_PluginTypeIsNotSelectedForProcessingCenter;
				case CCPluginCheckResult.Missing:
					return CA.Messages.ProcCenterUsesMissingPlugin;
				case CCPluginCheckResult.Unsupported:
					return CA.Messages.NotSupportedProcCenter;
			}
			return string.Empty;
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2021R1)]
		public static bool IsProcessingCenterNotSupported(PXGraph graph, string procCenterId)
		{
			return IsProcessingCenterNotSupported(graph, procCenterId, out _);
		}

		private static bool IsProcessingCenterNotSupported(PXGraph graph, string procCenterId, out CCPluginCheckResult result)
		{
			result = CCPluginCheckResult.NotPerformed;
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

			result = CCPluginTypeHelper.TryGetPluginTypeWithCheck(processingCenter.ProcessingTypeName, out _);
			return result != CCPluginCheckResult.Ok;
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2021R1)]
		public static bool IsProcessingCenterNotSupported(PXGraph graph, int? pmInstanceID)
		{
			return IsProcessingCenterNotSupported(graph, pmInstanceID, out _);
		}

		private static bool IsProcessingCenterNotSupported(PXGraph graph, int? pmInstanceID, out CCPluginCheckResult result)
		{
			result = CCPluginCheckResult.NotPerformed;
			if (pmInstanceID == null)
			{
				return false;
			}
			CCProcessingCenter processingCenter = GetProcessingCenterByPMInstance(graph, pmInstanceID);
			if (processingCenter == null)
			{
				return false;
			}

			result = CCPluginTypeHelper.TryGetPluginTypeWithCheck(processingCenter.ProcessingTypeName, out _);
			return result != CCPluginCheckResult.Ok;
		}

		private static bool IsProcessingCenterNotSupported(string typeName, out CCPluginCheckResult result)
		{
			result = CCPluginCheckResult.NotPerformed;
			if (string.IsNullOrEmpty(typeName))
			{
				return false;
			}

			result = CCPluginTypeHelper.TryGetPluginTypeWithCheck(typeName, out _);
			return result != CCPluginCheckResult.Ok;
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
