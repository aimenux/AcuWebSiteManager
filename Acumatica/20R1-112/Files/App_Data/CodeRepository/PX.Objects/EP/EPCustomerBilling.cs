using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Objects.AR.MigrationMode;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Extensions;

namespace PX.Objects.EP
{
	[TableDashboardType]
	public class EPCustomerBilling : PXGraph<EPCustomerBilling>
	{
		public PXCancel<BillingFilter> Cancel;
		public PXFilter<BillingFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<CustomersList, BillingFilter> Customers;


		public EPCustomerBilling()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			Customers.SetProcessCaption(Messages.Process);
			Customers.SetProcessAllCaption(Messages.ProcessAll);
			Customers.SetSelected<CustomersList.selected>();
		}

		protected virtual IEnumerable customers()
		{
			BillingFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}
			bool found = false;
			foreach (CustomersList item in Customers.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
			{
				yield break;
			}

			PXSelectBase<EPExpenseClaimDetails> sel = new PXSelectJoinGroupBy<EPExpenseClaimDetails, InnerJoin<Customer, On<EPExpenseClaimDetails.customerID, Equal<Customer.bAccountID>>,
																									 LeftJoin<Contract, On<EPExpenseClaimDetails.contractID, Equal<Contract.contractID>, And<Where<Contract.baseType, Equal<CTPRType.contract>, Or<Contract.nonProject, Equal<True>>>>>>>,
																									 Where<EPExpenseClaimDetails.released, Equal<boolTrue>,
																									 And<EPExpenseClaimDetails.billable, Equal<boolTrue>,
																									 And<EPExpenseClaimDetails.billed, Equal<boolFalse>,
																									 And<EPExpenseClaimDetails.expenseDate, LessEqual<Current<BillingFilter.endDate>>,
																									 And<Where<EPExpenseClaimDetails.contractID, Equal<Contract.contractID>, Or<EPExpenseClaimDetails.contractID, IsNull>>>
																									 >>>>,
																									 Aggregate<GroupBy<EPExpenseClaimDetails.customerID,
																											   GroupBy<EPExpenseClaimDetails.customerLocationID>>>>(this);
			if (filter.CustomerClassID != null)
			{
				sel.WhereAnd<Where<Customer.customerClassID, Equal<Current<BillingFilter.customerClassID>>>>();
			}
			if (filter.CustomerID != null)
			{
				sel.WhereAnd<Where<Customer.bAccountID, Equal<Current<BillingFilter.customerID>>>>();
			}

			foreach (PXResult<EPExpenseClaimDetails, Customer, Contract> res in sel.Select())
			{
				CustomersList retitem = new CustomersList();
				Customer customer = res;
				EPExpenseClaimDetails claimdetaisl = res;
				retitem.CustomerID = customer.BAccountID;
				retitem.LocationID = claimdetaisl.CustomerLocationID;
				retitem.CustomerClassID = customer.CustomerClassID;

				retitem.Selected = false;

				yield return Customers.Insert(retitem);

			}
		}

		public static void Bill(EPCustomerBillingProcess docgraph, CustomersList customer, BillingFilter filter)
		{
			docgraph.Bill(customer, filter);
		}

		#region EventHandlers
		protected virtual void BillingFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Customers.Cache.Clear();
		}

		protected virtual void BillingFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			BillingFilter filter = Filter.Current;
			Customers.SetProcessDelegate<EPCustomerBillingProcess>((docgraph, customer) => docgraph.Bill(customer, filter));
		}
		#endregion


		#region Internal Types
		[Serializable]
		public partial class BillingFilter : PX.Data.IBqlTable
		{
			#region InvoiceDate
			public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
			protected DateTime? _InvoiceDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Invoice Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? InvoiceDate
			{
				get
				{
					return this._InvoiceDate;
				}
				set
				{
					this._InvoiceDate = value;
				}
			}
			#endregion
			#region InvFinPeriodID
			public abstract class invFinPeriodID : PX.Data.BQL.BqlString.Field<invFinPeriodID> { }
			protected string _InvFinPeriodID;
			[AROpenPeriod(typeof(BillingFilter.invoiceDate))]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
			public virtual String InvFinPeriodID
			{
				get
				{
					return this._InvFinPeriodID;
				}
				set
				{
					this._InvFinPeriodID = value;
				}
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Customer Class")]
			public virtual String CustomerClassID
			{
				get
				{
					return this._CustomerClassID;
				}
				set
				{
					this._CustomerClassID = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[Customer(DescriptionField = typeof(Customer.acctName))]
			public virtual Int32? CustomerID
			{
				get
				{
					return this._CustomerID;
				}
				set
				{
					this._CustomerID = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			protected DateTime? _EndDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Load Claims Up to", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
		}
		

		#endregion
	}

	[Serializable]
	public partial class CustomersList : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
		protected String _CustomerClassID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer Class")]
		public virtual String CustomerClassID
		{
			get
			{
				return this._CustomerClassID;
			}
			set
			{
				this._CustomerClassID = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(DescriptionField = typeof(Customer.acctName), IsKey = true)]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CustomersList.customerID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, IsKey = true)]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		
	}

	public class EPCustomerBillingProcess : PXGraph<EPCustomerBillingProcess>
	{

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		protected virtual void EPExpenseClaimDetails_RefNbr_CacheAttached(PXCache sender)
		{ }

		public PXSelect<EPExpenseClaimDetails> Transactions;
		public PXSetup<EPSetup> Setup;

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public virtual void Bill(CustomersList customer, EPCustomerBilling.BillingFilter filter)
		{
			ARInvoiceEntry arGraph = PXGraph.CreateInstance<ARInvoiceEntry>();
			RegisterEntry pmGraph = PXGraph.CreateInstance<RegisterEntry>();

			arGraph.Clear();
			pmGraph.Clear();

			PMRegister pmDoc = null;
			ARInvoice arDoc = null;

			List<ARRegister> doclist = new List<ARRegister>();
			List<EPExpenseClaimDetails> listOfDirectBilledClaims = new List<EPExpenseClaimDetails>();

			PXSelectBase<EPExpenseClaimDetails> select = new PXSelectJoin<EPExpenseClaimDetails,
				LeftJoin<Contract, On<EPExpenseClaimDetails.contractID, Equal<Contract.contractID>, 
					And<Where<Contract.baseType, Equal<CTPRType.contract>, 
						Or<Contract.nonProject, Equal<True>>>>>, 
				LeftJoin<Account, On<EPExpenseClaimDetails.expenseAccountID, Equal<Account.accountID>>>>,
															Where<EPExpenseClaimDetails.released, Equal<boolTrue>,
															And<EPExpenseClaimDetails.billable, Equal<boolTrue>,
															And<EPExpenseClaimDetails.billed, Equal<boolFalse>,
															And<EPExpenseClaimDetails.customerID, Equal<Required<EPExpenseClaimDetails.customerID>>,
															And<EPExpenseClaimDetails.customerLocationID, Equal<Required<EPExpenseClaimDetails.customerLocationID>>,
															And<EPExpenseClaimDetails.expenseDate, LessEqual<Required<EPExpenseClaimDetails.expenseDate>>,
					And<Where<EPExpenseClaimDetails.contractID, Equal<Contract.contractID>, 
						Or<EPExpenseClaimDetails.contractID, IsNull>>>>>>>>>,
															OrderBy<Asc<EPExpenseClaimDetails.branchID>>>(this);

			arGraph.RowPersisted.AddHandler<ARInvoice>(
			delegate(PXCache sender, PXRowPersistedEventArgs e)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					foreach (EPExpenseClaimDetails row in listOfDirectBilledClaims.Select(claimdetail => Transactions.Locate(claimdetail)))
					{
						row.ARDocType = ((ARInvoice)e.Row).DocType;
						row.ARRefNbr = ((ARInvoice)e.Row).RefNbr;
					}
				}
			});

            decimal signOperation = 1m;
            decimal tipQty = 1m;

			var resultset = select.Select(customer.CustomerID, customer.LocationID, filter.EndDate).AsEnumerable();

			FinPeriodUtils.ValidateFinPeriod<EPExpenseClaimDetails>(
				resultset.RowCast<EPExpenseClaimDetails>(), 
				m => filter.InvFinPeriodID, 
				m => m.BranchID.SingleToArray());

			foreach (PXResult<EPExpenseClaimDetails, Contract, Account> res in resultset)
			{
				EPExpenseClaimDetails row = (EPExpenseClaimDetails)res;

				if (row.ContractID != null && !ProjectDefaultAttribute.IsNonProject( row.ContractID))
				{
					if (pmDoc == null)
					{
						EPExpenseClaim claim = PXSelect<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(this, row.RefNbr);
						pmDoc = (PMRegister)pmGraph.Document.Cache.Insert();
						pmDoc.OrigDocType = PMOrigDocType.ExpenseClaim;
						pmDoc.OrigNoteID = claim.NoteID;
					}

					PMTran usage = InsertPMTran(pmGraph, res);
					if (usage.Released == true) //contract trans are created as released
					{
						UsageMaint.AddUsage(pmGraph.Transactions.Cache, usage.ProjectID, usage.InventoryID, usage.BillableQty ?? 0m, usage.UOM);
					}
				}
				else
				{
					if (arDoc == null || arDoc.BranchID != row.BranchID)
					{
						if (arDoc != null)
						{
							arDoc.CuryOrigDocAmt = arDoc.CuryDocBal;
							arGraph.Document.Update(arDoc);
							arGraph.Save.Press();
							listOfDirectBilledClaims.Clear();
						}

						EPExpenseClaimDetails summDetail = PXSelectJoinGroupBy<EPExpenseClaimDetails,
							LeftJoin<Contract, On<EPExpenseClaimDetails.contractID, Equal<Contract.contractID>, 
								And<Where<Contract.baseType, Equal<CTPRType.contract>, 
									Or<Contract.nonProject, Equal<True>>>>>>,
							Where<EPExpenseClaimDetails.released, Equal<boolTrue>,
								And<EPExpenseClaimDetails.billable, Equal<boolTrue>,
								And<EPExpenseClaimDetails.billed, Equal<boolFalse>,
								And<EPExpenseClaimDetails.customerID, Equal<Required<EPExpenseClaimDetails.customerID>>,
								And<EPExpenseClaimDetails.customerLocationID, Equal<Required<EPExpenseClaimDetails.customerLocationID>>,
								And<EPExpenseClaimDetails.expenseDate, LessEqual<Required<EPExpenseClaimDetails.expenseDate>>,
								And<EPExpenseClaimDetails.branchID, Equal<Required<EPExpenseClaimDetails.branchID>>,
								And<Where<Contract.nonProject, Equal<True>, 
									Or<EPExpenseClaimDetails.contractID, IsNull>>>>>>>>>>, 
							Aggregate<Sum<EPExpenseClaimDetails.curyTranAmt>>>
							.Select(this, customer.CustomerID, customer.LocationID, filter.EndDate, row.BranchID);

						signOperation = summDetail.CuryTranAmt < 0 ? -1 : 1;

						// OrigModule should be set before Insert() method
						// to organize proper defaulting for any other fields
						// which depend on OrigModule value.
						//
						arDoc = new ARInvoice();
						arDoc.OrigModule = BatchModule.EP;
						arGraph.Document.Cache.SetValueExt<ARInvoice.docType>(arDoc, 
							signOperation < 0 ? ARDocType.CreditMemo : ARDocType.Invoice);
                       
						arDoc = (ARInvoice)arGraph.Document.Cache.Insert(arDoc);

						arGraph.Document.Cache.SetValueExt<ARInvoice.customerID>(arDoc, row.CustomerID);
						arGraph.Document.Cache.SetValueExt<ARInvoice.customerLocationID>(arDoc, row.CustomerLocationID);
						arGraph.Document.Cache.SetValueExt<ARInvoice.docDate>(arDoc, filter.InvoiceDate);
						arGraph.Document.Cache.SetValueExt<ARInvoice.branchID>(arDoc, row.BranchID);
						arDoc.OrigRefNbr = row.RefNbr;
						arDoc = arGraph.Document.Update(arDoc);
						arDoc.FinPeriodID = filter.InvFinPeriodID;
						doclist.Add(arDoc);
					}
                    
					// Insert ARTran.
					//
					InsertARTran(arGraph, row, signOperation);
					if ((row.CuryTipAmt ?? 0) != 0)
					{
						tipQty = signOperation < 0 == row.ClaimCuryTranAmtWithTaxes < 0 ? 1 : -1;
						InsertARTran(arGraph, row, signOperation, tipQty, true);
					}

					listOfDirectBilledClaims.Add(row);
				}

				row.Billed = true;
				Transactions.Update(row);
			}

			if (arDoc != null)
			{
				arDoc.CuryOrigDocAmt = arDoc.CuryDocBal;
				if (arGraph.ARSetup.Current.HoldEntry == false || Setup.Current.AutomaticReleaseAR == true)
				{
					arDoc = PXCache<ARInvoice>.CreateCopy(arDoc);
					arDoc.Hold = false;
					arDoc = arGraph.Document.Update(arDoc);
				}
				arGraph.Document.Update(arDoc);
				arGraph.Save.Press();
			}

			if (pmDoc != null)
			{
				pmGraph.Save.Press();
			}

			Persist(typeof(EPExpenseClaimDetails), PXDBOperation.Update);

			if (Setup.Current.AutomaticReleaseAR == true)
			{
				ARDocumentRelease.ReleaseDoc(doclist, false);
			}
		}

		protected virtual void InsertARTran(ARInvoiceEntry arGraph, EPExpenseClaimDetails row, decimal signOperation, decimal tipQty=1m, bool isTipTransaction = false)
		{
			CurrencyInfo curyInfo = PXSelect<CurrencyInfo>.Search<CurrencyInfo.curyInfoID>(arGraph, row.CuryInfoID);
			EPSetup epsetup = PXSelectReadonly<EPSetup>.Select(arGraph);

		
			ARTran tran = arGraph.Transactions.Insert();
			if (isTipTransaction)
			{
				IN.InventoryItem tipItem = PXSelect<IN.InventoryItem,
					Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(arGraph, epsetup.NonTaxableTipItem);
				tran.InventoryID = epsetup.NonTaxableTipItem;
				tran.Qty = tipQty;
				tran.UOM = tipItem.BaseUnit;
				tran.TranDesc = tipItem.Descr;

				SetAmount(arGraph, row.CuryTipAmt, row.TipAmt, tipQty, signOperation, curyInfo, tran);
				tran = arGraph.Transactions.Update(tran);

				if (epsetup.UseReceiptAccountForTips == true)
				{
					tran.AccountID = row.SalesAccountID;
					tran.SubID = row.SalesSubID;
				}
				else
				{
					Location companyloc = (Location)PXSelectJoin<Location,
																			InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>,
																								And<Location.locationID, Equal<BAccountR.defLocationID>>>,
																			InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>,
																Where<GL.Branch.branchID, Equal<Current<ARTran.branchID>>>>.Select(arGraph);
					Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, row.ContractID);
					PMTask task = PXSelect<PMTask,
											Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
											And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(arGraph, row.ContractID, row.TaskID);
					EPEmployee employee = (EPEmployee)PXSelect<EPEmployee>.Search<EPEmployee.bAccountID>(this, row != null ? row.EmployeeID : null);
					Location customerLocation = (Location)PXSelectorAttribute.Select<EPExpenseClaimDetails.customerLocationID>(arGraph.Caches[typeof(EPExpenseClaimDetails)], row);

					int? employee_SubID = (int?)arGraph.Caches[typeof(EPEmployee)].GetValue<EPEmployee.salesSubID>(employee);
					int? item_SubID = (int?)arGraph.Caches[typeof(IN.InventoryItem)].GetValue<IN.InventoryItem.salesSubID>(tipItem);
					int? company_SubID = (int?)arGraph.Caches[typeof(Location)].GetValue<Location.cSalesSubID>(companyloc);
					int? project_SubID = (int?)arGraph.Caches[typeof(Contract)].GetValue<Contract.defaultSubID>(contract);
					int? task_SubID = (int?)arGraph.Caches[typeof(PMTask)].GetValue<PMTask.defaultSubID>(task);
					int? location_SubID = (int?)arGraph.Caches[typeof(Location)].GetValue<Location.cSalesSubID>(customerLocation);

					object value = SubAccountMaskAttribute.MakeSub<EPSetup.salesSubMask>(arGraph, epsetup.SalesSubMask,
						new object[] { employee_SubID, item_SubID, company_SubID, project_SubID, task_SubID, location_SubID },
						new Type[] { typeof(EPEmployee.salesSubID), typeof(IN.InventoryItem.salesSubID), typeof(Location.cSalesSubID), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID), typeof(Location.cSalesSubID) });

					arGraph.Caches[typeof(ARTran)].RaiseFieldUpdating<ARTran.subID>(tran, ref value);
					tran.SubID = (int?)value;
				}
			}
			else
			{
				tran.InventoryID = row.InventoryID;
				tran.Qty = row.Qty * signOperation;
				tran.UOM = row.UOM;
				tran = arGraph.Transactions.Update(tran);
				tran.AccountID = row.SalesAccountID;
				tran.SubID = row.SalesSubID;
				tran.TranDesc = row.TranDesc;

				//For gross taxes we can't put tranAmt. So we should use taxable amount
				EPTaxTran firstLevelTaxTran = null;
				foreach (EPTaxTran taxRow in PXSelect<EPTaxTran,
					Where<EPTaxTran.claimDetailID, Equal<Required<EPTaxTran.claimDetailID>>,
					And<EPTaxTran.isTipTax, Equal <False>>>>.Select(this, row.ClaimDetailID))
				{
					if (firstLevelTaxTran == null || Math.Abs(firstLevelTaxTran.CuryTaxableAmt ?? 0m) > Math.Abs(taxRow.CuryTaxableAmt ?? 0m))
					{
						firstLevelTaxTran = taxRow;
					}
				}
				decimal? curyAmt = firstLevelTaxTran?.CuryTaxableAmt ?? row.CuryTaxableAmt;
				decimal? amt = firstLevelTaxTran?.TaxableAmt ?? row.TaxableAmt;
				SetAmount(arGraph,curyAmt, amt, row.Qty, signOperation, curyInfo, tran);
				tran = arGraph.Transactions.Update(tran);
				if (tran.CuryTaxableAmt != 0 && tran.CuryTaxableAmt != curyAmt) // indicates that we have gross/ iclusive taxes. In this case recalculation is required
				{
					curyAmt = row.CuryTaxableAmt;
					amt = row.TaxableAmt;
					SetAmount(arGraph, curyAmt, amt, row.Qty, signOperation, curyInfo, tran);
				}
			}
			tran.Date = row.ExpenseDate;
			tran.ManualPrice = true;
			tran = arGraph.Transactions.Update(tran);
			PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(EPExpenseClaimDetails)], row, arGraph.Transactions.Cache, tran, Setup.Current.GetCopyNoteSettings<PXModule.ar>());

		}

		private static void SetAmount(ARInvoiceEntry arGraph, decimal? curyTranAmt, decimal? tranAmt, decimal? qty, decimal signOperation, CurrencyInfo curyInfo, ARTran tran)
		{
			decimal curyamount;
			if (arGraph.currencyinfo.Current != null && curyInfo != null && arGraph.currencyinfo.Current.CuryID == curyInfo.CuryID)
			{
				curyamount = curyTranAmt.GetValueOrDefault(0);
			}
			else
			{
				PXCurrencyAttribute.CuryConvCury(arGraph.Document.Cache, arGraph.currencyinfo, tranAmt.GetValueOrDefault(), out curyamount);
			}
			tran.CuryTranAmt = curyamount * signOperation;
			tran.CuryUnitPrice = Math.Abs((curyamount / (qty.GetValueOrDefault(1m) != 0m ? qty.GetValueOrDefault(1m) : 1m)));
			tran.CuryExtPrice = curyamount * signOperation;
		}

		protected virtual PMTran InsertPMTran(RegisterEntry pmGraph, PXResult<EPExpenseClaimDetails, Contract, Account> res)
		{
			EPExpenseClaimDetails detail = res;
			Contract contract = res;
			Account account = res;

			if (account.AccountGroupID == null && contract.BaseType == CTPRType.Project)
				throw new PXException(Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD);

			bool released = contract.BaseType == CTPRType.Contract; //contract trans are created as released

			PMTran tran = (PMTran)pmGraph.Transactions.Cache.Insert();
			tran.AccountGroupID = account.AccountGroupID;
			tran.BAccountID = detail.CustomerID;
			tran.LocationID = detail.CustomerLocationID;
			tran.ProjectID = detail.ContractID;
			tran.TaskID = detail.TaskID;
			tran.CostCodeID = detail.CostCodeID;
			tran.InventoryID = detail.InventoryID;
			tran.Qty = detail.Qty;
			tran.Billable = true;
			tran.BillableQty = detail.Qty;
			tran.UOM = detail.UOM;
			tran.TranCuryID = detail.CuryID;
			tran.BaseCuryInfoID = detail.CuryInfoID;
			tran.TranCuryAmount = detail.ClaimCuryTranAmt;
			tran.Amount = detail.ClaimTranAmt;
			tran.TranCuryUnitRate = detail.CuryUnitCost;
			tran.UnitRate = detail.UnitCost;

			//TODO: Review the following code (exists only to support MA302 case when UnitCost is not entered):
			if (detail.CuryUnitCost == 0m && detail.Qty != 0)
			{
				tran.TranCuryUnitRate = detail.ClaimCuryTranAmt / detail.Qty;
				tran.UnitRate = detail.ClaimTranAmt / detail.Qty;
			}			
			tran.AccountID = detail.ExpenseAccountID;
			tran.SubID = detail.ExpenseSubID;
			tran.StartDate = detail.ExpenseDate;
			tran.EndDate = detail.ExpenseDate;
			tran.Date = detail.ExpenseDate;
			tran.ResourceID = detail.EmployeeID;
			tran.Released = released;

			tran = pmGraph.Transactions.Update(tran);

			pmGraph.Document.Current.Released = released;
			if (released)
			{
				pmGraph.Document.Current.Status = PMRegister.status.Released;
			}

			PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(EPExpenseClaimDetails)], detail, pmGraph.Transactions.Cache, tran, Setup.Current.GetCopyNoteSettings<PXModule.pm>());

			return tran;
		}
	}
}
