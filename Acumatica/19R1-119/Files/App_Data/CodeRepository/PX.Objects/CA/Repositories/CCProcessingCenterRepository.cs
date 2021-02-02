using PX.Data;
using PX.Objects.AR;
using System;

namespace PX.Objects.CA.Repositories
{
	public class CCProcessingCenterRepository
	{
		protected readonly PXGraph _graph;
		public CCProcessingCenterRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public CCProcessingCenter GetCCProcessingCenter(string processingCenterID)
		{
			return PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID,
					Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(_graph, processingCenterID);			
		}

		public CCProcessingCenter FindProcessingCenter(int? pMInstanceID, string aCuryId)
		{
			CCProcessingCenter result;
			result = PXSelectJoin<CCProcessingCenter, InnerJoin<CustomerPaymentMethod,
				On<CustomerPaymentMethod.cCProcessingCenterID, Equal<CCProcessingCenter.processingCenterID>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(_graph, pMInstanceID);
			if (result != null)
			{
				return result;
			}
			if (aCuryId != null)
			{
				result = PXSelectJoin<CCProcessingCenter,
					InnerJoin<CCProcessingCenterPmntMethod,
						On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>,
							And<CCProcessingCenter.isActive, Equal<BQLConstants.BitOn>>>,
						InnerJoin<CustomerPaymentMethod,
							On<CustomerPaymentMethod.paymentMethodID,
								Equal<CCProcessingCenterPmntMethod.paymentMethodID>,
								And<CCProcessingCenterPmntMethod.isActive, Equal<BQLConstants.BitOn>>>,
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CCProcessingCenter.cashAccountID>>>>>,
					Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>,
						And<CashAccount.curyID, Equal<Required<CashAccount.curyID>>>>,
					OrderBy<Desc<CCProcessingCenterPmntMethod.isDefault>>>.Select(_graph, pMInstanceID, aCuryId);
			}
			else
			{
				result = PXSelectJoin<CCProcessingCenter,
					InnerJoin<CCProcessingCenterPmntMethod,
						On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>,
							And<CCProcessingCenter.isActive, Equal<BQLConstants.BitOn>>>,
						InnerJoin<CustomerPaymentMethod,
							On<CustomerPaymentMethod.paymentMethodID,
								Equal<CCProcessingCenterPmntMethod.paymentMethodID>,
								And<CCProcessingCenterPmntMethod.isActive, Equal<BQLConstants.BitOn>>>,
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CCProcessingCenter.cashAccountID>>>>>,
					Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>,
					OrderBy<Desc<CCProcessingCenterPmntMethod.isDefault>>>.Select(_graph, pMInstanceID);
			}
			return result;
		}
	}
}
