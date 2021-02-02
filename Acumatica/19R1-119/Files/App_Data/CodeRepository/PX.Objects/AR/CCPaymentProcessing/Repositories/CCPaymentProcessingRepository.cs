using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR.Repositories;
using PX.Objects.CA.Repositories;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class CCPaymentProcessingRepository : ICCPaymentProcessingRepository
	{
		public static ICCPaymentProcessingRepository GetCCPaymentProcessingRepository()
		{
			CCPaymentProcessingRepository repository = new CCPaymentProcessingRepository();
			CCPaymentHelperGraph newGraph = PXGraph.CreateInstance<CCPaymentHelperGraph>();
			repository.InitializeRepositories(newGraph);
			return repository;
		}

		public static ICCPaymentProcessingRepository GetCCPaymentProcessingRepository(PXGraph graph)
		{
			CCPaymentProcessingRepository repository = new CCPaymentProcessingRepository();
			repository.InitializeRepositories(graph);
			return repository;
		}

		private void InitializeRepositories(PXGraph graph)
		{
			_graph = graph;
			_cctranRepository = new CCProcTranRepository(graph);
			_cpmRepository = new CustomerPaymentMethodRepository(graph);
			_cpmDetailRepository = new CustomerPaymentMethodDetailRepository(graph);
			_processingCenterRepository = new CCProcessingCenterRepository(graph);
			_processingCenterDetailRepository = new CCProcessingCenterDetailRepository(graph);
			_cashAccountRepository = new CashAccountRepository(graph);
		}

		private PXGraph _graph;

		public PXGraph Graph
		{
			get
			{
				return _graph;
			}
		}

		private CCProcTranRepository _cctranRepository;
		private CustomerPaymentMethodRepository _cpmRepository;
		private CustomerPaymentMethodDetailRepository _cpmDetailRepository;
		private CCProcessingCenterRepository _processingCenterRepository;
		private CCProcessingCenterDetailRepository _processingCenterDetailRepository;
		private CashAccountRepository _cashAccountRepository;

		public PXSelect<CCProcTran> CCProcTrans;
		public PXSelect<CustomerPaymentMethod> CustomerPaymentMethods;
		public PXSelect<CustomerPaymentMethodDetail> CustomerPaymentMethodDetails;

		public CCProcTran GetCCProcTran(int? tranID)
		{
			return _cctranRepository.GetCCProcTran(tranID);
		}

		public CCProcTran InsertCCProcTran(CCProcTran transaction)
		{
			return _cctranRepository.InsertCCProcTran(transaction);
		}

		public CCProcTran UpdateCCProcTran(CCProcTran transaction)
		{
			return _cctranRepository.UpdateCCProcTran(transaction);
		}

		public CustomerPaymentMethod GetCustomerPaymentMethod(int? pMInstanceId)
		{
			return _cpmRepository.GetCustomerPaymentMethod(pMInstanceId);
		}

		public CustomerPaymentMethod UpdateCustomerPaymentMethod(CustomerPaymentMethod paymentMethod)
		{
			return _cpmRepository.UpdateCustomerPaymentMethod(paymentMethod);
		}

		public CustomerPaymentMethodDetail GetCustomerPaymentMethodDetail(int? pMInstanceId, string detailID)
		{
			return _cpmDetailRepository.GetCustomerPaymentMethodDetail(pMInstanceId, detailID);			
		}

		public void DeletePaymentMethodDetail(CustomerPaymentMethodDetail detail)
		{
			_cpmDetailRepository.DeletePaymentMethodDetail(detail);
		}

		public void Save()
		{
			_graph.Actions.PressSave();
		}

		public CCProcTran FindReferencedCCProcTran(int? pMInstanceID, string refTranNbr)
		{
			return _cctranRepository.FindReferencedCCProcTran(pMInstanceID, refTranNbr);
		}

		public CashAccount GetCashAccount(int? cashAccountId)
		{
			return _cashAccountRepository.GetCashAccount(cashAccountId);			
		}

		public CCProcessingCenter GetCCProcessingCenter(string processingCenterID)
		{
			return _processingCenterRepository.GetCCProcessingCenter(processingCenterID);
		}

		public CCProcessingCenter FindProcessingCenter(int? pMInstanceID, string aCuryId)
		{
			return _processingCenterRepository.FindProcessingCenter(pMInstanceID, aCuryId);
		}

		public CCProcTran FindVerifyingCCProcTran(int? pMInstanceID)
		{
			return _cctranRepository.FindVerifyingCCProcTran(pMInstanceID);
		}

		public PXResult<CustomerPaymentMethod, Customer> FindCustomerAndPaymentMethod(int? pMInstanceID)
		{
			return _cpmRepository.FindCustomerAndPaymentMethod(pMInstanceID);
		}

		public PXResultset<CCProcessingCenterDetail> FindAllProcessingCenterDetails(string processingCenterID)
		{
			return _processingCenterDetailRepository.FindAllProcessingCenterDetails(processingCenterID);
		}
	}

	public class CCPaymentHelperGraph : PXGraph<CCPaymentHelperGraph>
	{
		public PXSelect<CCProcTran> CCProcTrans;
		public PXSelect<CustomerPaymentMethod> CustomerPaymentMethods;
		public PXSelect<CustomerPaymentMethodDetail> CustomerPaymentMethodDetails;
	}
}
