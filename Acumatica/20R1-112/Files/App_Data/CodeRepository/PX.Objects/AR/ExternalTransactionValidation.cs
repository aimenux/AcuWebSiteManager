using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.CA;
using PX.Objects.GL;
using PX.Objects.SO;

namespace PX.Objects.AR
{
	public class ExternalTransactionValidation : PXGraph<ExternalTransactionValidation>
	{
		#region CTor + public Member Decalaration
		public PXFilter<ExternalTransactionFilter> Filter;
		public PXCancel<ExternalTransactionFilter> Cancel;

		public PXAction<ExternalTransactionFilter> ViewDocument;
		public PXAction<ExternalTransactionFilter> ViewOrigDocument;
		public PXAction<ExternalTransactionFilter> ViewProcessingCenter;
		public PXAction<ExternalTransactionFilter> ViewExternalTransaction;

		public PXFilteredProcessingJoin<ExternalTransactionExt, ExternalTransactionFilter,
								LeftJoin<Customer, On<Customer.bAccountID, Equal<ExternalTransactionExt.bAccountID>>>,
								Where<ExternalTransactionExt.active, Equal<True>,
										And<ExternalTransactionExt.refNbr, IsNotNull,
										And<ExternalTransactionExt.docType, In<Required<ExternalTransactionExt.docType>>>>>,
								OrderBy<Desc<ExternalTransactionExt.refNbr>>> PaymentTrans;

		public ExternalTransactionValidation()
		{
			PaymentTrans.SetProcessCaption(AR.Messages.Validate);
			PaymentTrans.SetProcessAllCaption(Messages.ValidateAll);
		}
		#endregion

		#region View delegate
		public virtual IEnumerable paymentTrans()
		{
			ExternalTransactionFilter filter = this.Filter.Current;

			if (filter != null)
			{
				PXSelectBase<ExternalTransactionExt> select = new PXSelectJoin<ExternalTransactionExt,
								LeftJoin<Customer, On<Customer.bAccountID, Equal<ExternalTransactionExt.bAccountID>>>,
								Where<ExternalTransactionExt.active, Equal<True>,
										And<ExternalTransactionExt.refNbr, IsNotNull,
										And<ExternalTransactionExt.docType, In<Required<ExternalTransactionExt.docType>>,
										And<ExternalTransactionExt.cCProcessingStatus, In<Required<ExternalTransactionExt.cCProcessingStatus>>>>>>,
								OrderBy<Desc<ExternalTransactionExt.refNbr>>>(this);

				if (!string.IsNullOrEmpty(filter.ProcessingCenterID))
				{
					select.WhereAnd<Where<ExternalTransactionExt.cCProcessingCenterID, Equal<Current<ExternalTransactionFilter.processingCenterID>>>>();
				}

				string[] docTypes = new string[] { ARDocType.CashSale, ARDocType.Payment, ARDocType.Prepayment };
				string[] statuses = new string[] { ExtTransactionProcStatusCode.AuthorizeHeldForReview, ExtTransactionProcStatusCode.CaptureHeldForReview };
				
				foreach (PXResult<ExternalTransactionExt, Customer> it in select.SelectWithViewContext(new object[] { docTypes, statuses }))
				{
					yield return it;
				}
			}

			yield break;
		}
		#endregion

		#region Action

		[PXUIField(DisplayName = Messages.ViewDocument, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			ExternalTransactionExt tran = this.PaymentTrans.Current;
			if (tran != null)
			{
				PXGraph target = CCTransactionsHistoryEnq.FindSourceDocumentGraph(tran.DocType, tran.RefNbr, null, null);
				if (target != null)
					throw new PXRedirectRequiredException(target, true, Messages.ViewDocument) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.ViewOrigDocument, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewOrigDocument(PXAdapter adapter)
		{
			ExternalTransactionExt tran = this.PaymentTrans.Current;
			if (tran != null && !string.IsNullOrWhiteSpace(tran.OrigRefNbr))
			{
				SO.SOOrderEntry graph = PXGraph.CreateInstance<SO.SOOrderEntry>();
				graph.Document.Current = graph.Document.Search<SO.SOOrder.orderNbr>(tran.OrigRefNbr, tran.OrigDocType);

				if (graph.Document.Current != null)
					throw new PXRedirectRequiredException(graph, true, Messages.ViewOrigDocument) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.ViewProcessingCenter, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewProcessingCenter(PXAdapter adapter)
		{
			if (this.PaymentTrans.Current != null)
			{
				ExternalTransactionExt row = this.PaymentTrans.Current;
				CustomerPaymentMethod pmInstance = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(this, row.PMInstanceID);
				CCProcessingCenterMaint graph = PXGraph.CreateInstance<CCProcessingCenterMaint>();
				graph.ProcessingCenter.Current = graph.ProcessingCenter.Search<CCProcessingCenter.processingCenterID>(pmInstance.CCProcessingCenterID);
				if (graph.ProcessingCenter.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, Messages.ViewProcessingCenter) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.ViewExternalTransaction, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewExternalTransaction(PXAdapter adapter)
		{
			if (this.PaymentTrans.Current != null)
			{
				ExternalTransactionExt row = this.PaymentTrans.Current;
				ExternalTransactionMaint graph = PXGraph.CreateInstance<ExternalTransactionMaint>();
				graph.CurrentTransaction.Current = row;
				if (graph.CurrentTransaction.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, Messages.ViewExternalTransaction) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return Filter.Select();
		}
		#endregion

		#region Internal Types
		[Serializable]
		[PXHidden]
		public partial class ExternalTransactionFilter : IBqlTable
		{
			#region ProcessingCenterID
			public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }

			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>), DescriptionField = typeof(CCProcessingCenter.name))]
			[PXUIField(DisplayName = "Proc. Center ID", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String ProcessingCenterID { get; set; }
			#endregion
			#region DisplayType
			public abstract class displayType : PX.Data.BQL.BqlString.Field<displayType> { }

			[PXDBString(IsUnicode = false)]
			[DisplayTypes.List()]
			[PXDefault(DisplayTypes.HeldForReview)]
			[PXUIField(DisplayName = "Display Transactions")]
			public virtual String DisplayType { get; set; }
			#endregion
		}

		private static class DisplayTypes
		{
			public const string HeldForReview = "HELDFORREVIEW";

			[PXLocalizable]
			public class UI
			{
				public const string HeldForReview = "Held for Review";
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					new string[] { HeldForReview },
					new string[] { UI.HeldForReview })
				{; }
			}
		}

		/// <summary>
		/// Ñlass for use in processing. 
		/// Hides ProcessingStatus field matching the name of the system field
		/// </summary>
		[PXHidden]
		[PXProjection(typeof(Select2<ExternalTransaction,
		LeftJoin<CustomerPaymentMethod,
			On<CustomerPaymentMethod.pMInstanceID, Equal<ExternalTransaction.pMInstanceID>>>>))]
		public class ExternalTransactionExt : ExternalTransaction
		{
			public new abstract class transactionID : PX.Data.BQL.BqlInt.Field<transactionID> { }

			[PXString(50)]
			[PXUIField(DisplayName = "Proc. Status", Visibility = PXUIVisibility.Visible)]
			public override string ProcessingStatus
			{
				get; set;
			}

			#region ProcessingStatus
			public abstract class cCProcessingStatus : PX.Data.BQL.BqlString.Field<cCProcessingStatus> { }
			[PXDBString(3, IsFixed = true, DatabaseFieldName = "ProcessingStatus")]
			[ExtTransactionProcStatusCode.List()]
			[PXUIField(DisplayName = "Proc. Status", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string CCProcessingStatus { get; set; }
			#endregion

			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
			[PXDBString(10, IsUnicode = true, BqlField = typeof(CustomerPaymentMethod.paymentMethodID))]
			[PXUIField(DisplayName = "Payment Method")]
			public string PaymentMethodID { get; set; }
			#endregion

			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

			/// <summary>
			/// The identifier of <see cref="Customer">customer</see> to
			/// which the payment method belongs. This field is a part
			/// of the compound key of the record.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="BAccount.BAccountID"/> field.
			/// </value>
			[Customer(DescriptionField = typeof(Customer.acctName), DirtyRead = true, BqlField = typeof(CustomerPaymentMethod.bAccountID))]
			public virtual Int32? BAccountID
			{
				get; set;
			}
			#endregion

			#region CCProcessingCenterID
			public abstract class cCProcessingCenterID : PX.Data.BQL.BqlString.Field<cCProcessingCenterID> { }
			/// <summary>
			/// The identifier of the credit card processing center.
			/// </summary>
			/// <value>
			/// The field has a value if the customer payment method is configured
			/// to process payments through a payment gateway. The value corresponds to the
			/// value of the <see cref="CCProcessingCenterPmntMethod.processingCenterID"/> field.
			/// </value>
			[PXDBString(10, IsUnicode = true, BqlField = typeof(CustomerPaymentMethod.cCProcessingCenterID))]
			[PXUIField(DisplayName = "Proc. Center ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, Visible = true)]
			public virtual string CCProcessingCenterID { get; set; }
			#endregion
		}

		#endregion

		#region Filter Event Handlers

		protected virtual void ExternalTransactionFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ExternalTransactionFilter filter = e.Row as ExternalTransactionFilter;
			if (filter == null) return;

			PaymentTrans.SetProcessDelegate(delegate (List<ExternalTransactionExt> list)
			{
				ExternalTransactionValidation graph = CreateInstance<ExternalTransactionValidation>();
				List<IExternalTransaction> newList = new List<IExternalTransaction>();
				newList.AddRange(list);
				ValidateCCPayment(graph, newList, true);
			});
		}
		#endregion

		#region Bussines logic

		public static void ValidateCCPayment(PXGraph graph, List<IExternalTransaction> list, bool isMassProcess)
		{
			bool failed = false;
			ARCashSaleEntry arCashSaleGraph = null;
			ARPaymentEntry arPaymentGraph = null;
			SOInvoiceEntry soInvoiceGraph = null;

			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == null)
					continue;

				if ((i % 100) == 0)
				{
					if (arCashSaleGraph != null)
						arCashSaleGraph.Clear();
					if (arPaymentGraph != null)
						arPaymentGraph.Clear();
					if (soInvoiceGraph != null)
						soInvoiceGraph.Clear();
				}

				IExternalTransaction tran = list[i];

				var resultSet = PXSelectJoin<Standalone.ARRegister,
							LeftJoin<ARPayment,
								On<ARPayment.refNbr, Equal<Standalone.ARRegister.refNbr>,
									And<ARPayment.docType, Equal<Standalone.ARRegister.docType>>>,
							LeftJoin<ARInvoice,
								On<ARInvoice.refNbr, Equal<Standalone.ARRegister.refNbr>,
									And<ARInvoice.docType, Equal<Standalone.ARRegister.docType>>>,
							LeftJoin<SOInvoice,
								On<SOInvoice.refNbr, Equal<Standalone.ARRegister.refNbr>,
									And<SOInvoice.docType, Equal<Standalone.ARRegister.docType>>>,
							LeftJoin<Standalone.ARCashSale,
								On<Standalone.ARCashSale.refNbr, Equal<Standalone.ARRegister.refNbr>,
									And<Standalone.ARCashSale.docType, Equal<Standalone.ARRegister.docType>>>>>>>,
							Where<Standalone.ARRegister.refNbr, Equal<Required<Standalone.ARRegister.refNbr>>,
								And<Standalone.ARRegister.docType, Equal<Required<Standalone.ARRegister.docType>>>>>
								.SelectSingleBound(graph, null, tran.RefNbr, tran.DocType);

				foreach (PXResult<Standalone.ARRegister, ARPayment, ARInvoice, SOInvoice, Standalone.ARCashSale> arDoc in resultSet)
				{
					if (arDoc == null)
						continue;

					try
					{
						if ((ARInvoice)arDoc is ARInvoice arInvoice
								&& arInvoice != null && arInvoice.RefNbr != null)
						{
							if ((SOInvoice)arDoc is SOInvoice soInvoice
								&& soInvoice != null && soInvoice.RefNbr != null)
							{
								soInvoiceGraph = soInvoiceGraph != null ? soInvoiceGraph : PXGraph.CreateInstance<SOInvoiceEntry>();
								SOInvoiceEntry.PaymentTransaction ext = soInvoiceGraph.GetExtension<SOInvoiceEntry.PaymentTransaction>();
								soInvoiceGraph.Document.Current = arInvoice;
								soInvoiceGraph.SODocument.Current = soInvoice;
								if (ext.CanValidate(arInvoice))
								{
									ext.validateCCPayment.Press();
								}
							}
							else if ((Standalone.ARCashSale)arDoc is Standalone.ARCashSale arCashSale
								&& arCashSale != null && arCashSale.RefNbr != null)
							{
								arCashSaleGraph = arCashSaleGraph != null ? arCashSaleGraph : PXGraph.CreateInstance<ARCashSaleEntry>();
								ARCashSaleEntry.PaymentTransaction ext = arCashSaleGraph.GetExtension<ARCashSaleEntry.PaymentTransaction>();
								arCashSaleGraph.Document.Current = arCashSale;
								if (ext.CanValidate(arCashSaleGraph.Document.Current))
								{
									ext.validateCCPayment.Press();
								}
							}
						}
						else if ((ARPayment)arDoc is ARPayment arPayment
									&& arPayment != null && arPayment.RefNbr != null)
						{
							arPaymentGraph = arPaymentGraph != null ? arPaymentGraph : PXGraph.CreateInstance<ARPaymentEntry>();
							ARPaymentEntry.PaymentTransaction ext = arPaymentGraph.GetExtension<ARPaymentEntry.PaymentTransaction>();
							arPaymentGraph.Document.Current = arPayment;
							if (ext.CanValidate(arPaymentGraph.Document.Current))
							{
								ext.validateCCPayment.Press();
							}
						}

						if (isMassProcess)
						{
							PXProcessing<ExternalTransaction>.SetInfo(i, ActionsMessages.RecordProcessed);
						}
					}
					catch (Exception e)
					{
						failed = true;

						if (isMassProcess)
						{
							PXProcessing<ExternalTransaction>.SetError(i, e);
						}
						else
						{
							throw new Common.PXMassProcessException(i, e);
						}
					}
				}
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(AR.Messages.DocumentsNotValidated);
			}
		}
		#endregion
	}
}