using System.Collections;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;

namespace PX.Objects.AR
{
	public class CCProcessingCenterSelectorAttribute : PXCustomSelectorAttribute
	{
		CCProcessingFeature feature;
		public CCProcessingCenterSelectorAttribute(CCProcessingFeature feature) : base( typeof(CCProcessingCenter.processingCenterID), typeof(CCProcessingCenter.processingCenterID) )
		{
			this.feature = feature;
		}

		public IEnumerable GetRecords()
		{
			PXSelectBase<CCProcessingCenter> query = new PXSelectReadonly<CCProcessingCenter>(_Graph);
			ICCPaymentProcessingRepository repository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();

			foreach (CCProcessingCenter item in query.Select())
			{
				string processingCenterId = item.ProcessingCenterID;
				CCProcessingCenter processingCenter = repository.GetCCProcessingCenter(processingCenterId);

				if (CCProcessingFeatureHelper.IsFeatureSupported(processingCenter,feature))
				{
					yield return item;
				}
			}
		}
	}
}
