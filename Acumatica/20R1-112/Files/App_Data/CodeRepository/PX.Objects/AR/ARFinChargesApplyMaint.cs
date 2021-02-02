using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.AR.MigrationMode;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	[Serializable]
	public class ARFinChargesApplyMaint : PXGraph<ARFinChargesApplyMaint>
	{
		#region Internal Types

		protected class MessageWithLevel
		{
			public string Message;
			public PXErrorLevel Level;

			public MessageWithLevel(string message, PXErrorLevel level)
			{
				this.Message = message;
				this.Level = level;
			}
		}

		[Serializable]
		public partial class ARFinChargesApplyParameters : IBqlTable
		{
			#region StatementCycle
			public abstract class statementCycle : PX.Data.BQL.BqlString.Field<statementCycle> { }
			[PXDBString(10, IsUnicode = true)]
			[PXDefault(typeof(ARStatementCycle.statementCycleId), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Statement Cycle", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(ARStatementCycle.statementCycleId), DescriptionField = typeof(ARStatementCycle.descr))]
			public virtual string StatementCycle
			{
				get;
				set;
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			[PXDBString(10, IsUnicode = true)]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr))]
			[PXUIField(DisplayName = "Customer Class")]

			public virtual string CustomerClassID
			{
				get;
				set;
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt]
			[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
			[CustomerActive(DescriptionField = typeof(Customer.acctName))]
			[PXRestrictor(typeof(Where<Customer.status, NotEqual<BAccount.status.inactive>,
				And<Customer.status, NotEqual<BAccount.status.hold>>>), Messages.CustomerIsInStatus, typeof(Customer.status), ReplaceInherited = true)]
			public virtual int? CustomerID
			{
				get;
				set;
			}
			#endregion
			#region FinChargeDate
			public abstract class finChargeDate : PX.Data.BQL.BqlDateTime.Field<finChargeDate> { }
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Null)]
			[PXUIField(DisplayName = "Overdue Charge Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? FinChargeDate
			{
				get;
				set;
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			[FinPeriodSelector(typeof(ARFinChargesApplyParameters.finChargeDate))]
			[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
			public virtual string FinPeriodID
			{
				get;
				set;
			}
			#endregion

		}

		[Serializable]
		public partial class ARFinChargesDetails : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(true)]
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
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXDBString(3, IsKey = true, IsFixed = true)]
			[PXDefault]
			[ARInvoiceType.List]
			[PXUIField(DisplayName = "Type", Enabled = false)]
			public virtual string DocType
			{
				get;
				set;
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
			public virtual string RefNbr
			{
				get;
				set;
			}
			#endregion
			#region DocDate
			public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			[PXDate]
			[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual DateTime? DocDate
			{
				get;
				set;
			}
			#endregion
			#region DueDate
			public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			[PXDate]
			[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual DateTime? DueDate
			{
				get;
				set;
			}
			#endregion
			#region LastPaymentDate
			public abstract class lastPaymentDate : PX.Data.BQL.BqlDateTime.Field<lastPaymentDate> { }
			[PXDate]
			[PXUIField(DisplayName = "Last Payment Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual DateTime? LastPaymentDate
			{
				get;
				set;
			}
			#endregion
			#region LastChargeDate
			public abstract class lastChargeDate : PX.Data.BQL.BqlDateTime.Field<lastChargeDate> { }
			[PXDate]
			[PXUIField(DisplayName = "Last Charge Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual DateTime? LastChargeDate
			{
				get;
				set;
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Enabled = false)]
			[PXRestrictor(typeof(Where<Customer.status, NotEqual<BAccount.status.inactive>,
				And<Customer.status, NotEqual<BAccount.status.hold>>>), Messages.CustomerIsInStatus, typeof(Customer.status), ReplaceInherited = true)]
			[PXDefault]
			public virtual int? CustomerID
			{
				get;
				set;
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXSelector(typeof(Currency.curyID))]
			public virtual string CuryID
			{
				get;
				set;
			}
			#endregion
			#region CuryOrigDocAmt
			public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
			[PXCury(typeof(curyID))]
			[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual decimal? CuryOrigDocAmt
			{
				get;
				set;
			}
			#endregion

			#region CuryDocBal
			public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
			[PXCury(typeof(curyID))]
			[PXUIField(DisplayName = "Open Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual decimal? CuryDocBal
			{
				get;
				set;
			}
			#endregion
			#region OverdueDays
			public abstract class overdueDays : PX.Data.BQL.BqlShort.Field<overdueDays> { }
			[PXShort]
			[PXDefault((short)0)]
			[PXUIField(DisplayName = "Overdue Days", Enabled = false)]
			public virtual short? OverdueDays
			{
				get;
				set;
			}
			#endregion
			#region FinChargeID
			public abstract class finChargeID : PX.Data.BQL.BqlString.Field<finChargeID> { }
			[PXString(10, IsUnicode = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Overdue Charge ID", Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
			public virtual string FinChargeID
			{
				get;
				set;
			}
			#endregion
			#region TermsID
			public string TermsID;
			#endregion
			#region FinChargeCuryID
			public abstract class finChargeCuryID : PX.Data.BQL.BqlString.Field<finChargeCuryID> { }
			[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Charge Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXDefault(typeof(Search<Company.baseCuryID>))]
			[PXSelector(typeof(Currency.curyID))]
			public virtual string FinChargeCuryID
			{
				get;
				set;
			}
			#endregion
			#region FinChargeAmt
			public abstract class finChargeAmt : PX.Data.BQL.BqlDecimal.Field<finChargeAmt> { }
			[PXDBCury(typeof(ARFinChargesDetails.curyID))]
			[PXUIField(DisplayName = "Charge Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual decimal? FinChargeAmt
			{
				get;
				set;
			}
			#endregion
			#region ARAccountID
			public abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }
			[PXDefault]
			[Account(DisplayName = "AR Account", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual int? ARAccountID
			{
				get;
				set;
			}
			#endregion
			#region ARSubID
			public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
			[PXDefault]
			[SubAccount(DisplayName = "AR Sub.", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual int? ARSubID
			{
				get;
				set;
			}
			#endregion
			#region CuryRate
			public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
			[PXDBDecimal(6)]
			[PXDefault(TypeCode.Decimal, "1.0")]
			public virtual decimal? CuryRate
			{
				get;
				set;
			}
			#endregion
			#region CuryMultDiv
			public abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
			[PXDBString(1, IsFixed = true)]
			[PXDefault("M")]
			public virtual string CuryMultDiv
			{
				get;
				set;
			}
			#endregion
			#region CuryRateTypeID
			public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
			[PXDBString(6, IsUnicode = true)]
			public virtual string CuryRateTypeID
			{
				get;
				set;
			}
			#endregion
			#region SampleCuryRate
			public abstract class sampleCuryRate : PX.Data.BQL.BqlDecimal.Field<sampleCuryRate> { }
			[PXDBDecimal(6)]
			[PXUIField(DisplayName = "Currency Rate")]
			public virtual decimal? SampleCuryRate
			{
				[PXDependsOnFields(typeof(curyMultDiv), typeof(curyRate))]
				get
				{
					return (this.CuryMultDiv == "M") ? this.CuryRate : 1 / this.CuryRate;
				}
				set
				{
				}
			}
			#endregion
			#region HasUnreleasedCharges
			public abstract class hasUnreleasedCharges : PX.Data.BQL.BqlBool.Field<hasUnreleasedCharges> { }
			[PXDBBool]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual bool? HasUnreleasedCharges
			{
				get;
				set;
			}
			#endregion
			#region LastFCDocType
			public abstract class lastFCDocType : PX.Data.BQL.BqlString.Field<lastFCDocType> { }
			[PXDBString(3, IsFixed = true)]
			[PXDefault]
			public virtual string LastFCDocType
			{
				get;
				set;
			}
			#endregion
			#region LastFCRefNbr
			public abstract class lastFCRefNbr : PX.Data.BQL.BqlString.Field<lastFCRefNbr> { }
			[PXDBString(15, IsUnicode = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Last Fin. Charge", Enabled = false)]
			public virtual string LastFCRefNbr
			{
				get;
				set;
			}
			#endregion
			#region FinChargeDate
			public abstract class finChargeDate : PX.Data.BQL.BqlDateTime.Field<finChargeDate> { }
			[PXDate]
			[PXUIField(DisplayName = "Overdue Charge Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? FinChargeDate
			{
				get;
				set;
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			[FinPeriodID]
			[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
			public virtual string FinPeriodID
			{
				get;
				set;
			}
			#endregion
			#region CustomerHasOpenPayments
			public abstract class customerHasOpenPayments : PX.Data.BQL.BqlBool.Field<customerHasOpenPayments> { }
			[PXBool]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual bool? CustomerHasOpenPayments
			{
				get;
				set;
			}
			#endregion
		}

		protected class ARDocKey : AP.Pair<string, string>
		{
			public ARDocKey(string aFirst, string aSecond) : base(aFirst, aSecond) { }
		}
		#endregion

		#region Ctor + public members
		public ARFinChargesApplyMaint()
		{
			ARSetup setup = ARSetup.Current;
			ARFinChargesApplyParameters filter = Filter.Current;
			ARFinChargeRecords.SetProcessDelegate(
				delegate(List<ARFinChargesDetails> list)
				{
					CreateFCDoc(list, filter);
				}
			);
		}

        public override bool IsProcessing
        {
            get { return false;}
            set { }
        }


		[PXReadOnlyView]
		public PXSelect<ARInvoice, Where<True, Equal<False>>> ARInvoices;

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		public PXAction<ARFinChargesApplyParameters> cancel;
		public PXAction<ARFinChargesApplyParameters> calculate;

		public PXFilter<ARFinChargesApplyParameters> Filter;
		[PXFilterable]
		public PXFilteredProcessing<ARFinChargesDetails, ARFinChargesApplyParameters> ARFinChargeRecords;
		public ARSetupNoMigrationMode ARSetup;

		public PXAction<ARFinChargesApplyParameters> viewDocument;
		public PXAction<ARFinChargesApplyParameters> viewLastFinCharge;

		#region Sub-screen Navigation Buttons

		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			ARFinChargesDetails row = this.ARFinChargeRecords.Current;
			if (row != null)
			{
				ARInvoice doc = PXSelect<ARInvoice>.Search<ARInvoice.docType, ARInvoice.refNbr>(this, row.DocType, row.RefNbr);
				if (doc != null)
				{
					ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
					graph.Document.Current = doc;
					throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.ViewLastCharge, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(VisibleOnProcessingResults = true)]
		public virtual IEnumerable ViewLastFinCharge(PXAdapter adapter)
		{
			ARFinChargesDetails row = this.ARFinChargeRecords.Current;
			if (row != null)
			{
				if (!string.IsNullOrEmpty(row.LastFCRefNbr))
				{
					ARInvoice doc = PXSelect<ARInvoice>.Search<ARInvoice.docType, ARInvoice.refNbr>(this, row.LastFCDocType, row.LastFCRefNbr);
					if (doc != null)
					{
						ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
						graph.Document.Current = doc;
						throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
			}
			return Filter.Select();
		}

		#endregion
		#endregion

		#region Delegates
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			ARFinChargeRecords.Cache.Clear();
			TimeStamp = null;
			PXLongOperation.ClearStatus(UID);
			return adapter.Get();
		}

		protected virtual IEnumerable aRFinChargeRecords()
		{
			return ARFinChargeRecords.Cache.Inserted;
		}

		[PXUIField(DisplayName = Messages.Calculate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Calculate(PXAdapter adapter)
		{
			ARFinChargesApplyParameters filter = Filter.Current;

			if (!CheckRequiredFieldsFilled(Filter.Cache, filter))
			{
				return adapter.Get();
			}

			Company company = PXSelect<Company>.Select(this);
			ARSetup ARSetup = PXSelect<ARSetup>.Select(this);

			ARFinChargeRecords.Cache.Clear();
			ARFinChargeRecords.SetProcessAllEnabled(false);
			ARFinChargeRecords.SetProcessEnabled(false);

			PXLongOperation.StartOperation(this, () => 
			{
				ARFinChargesApplyMaint financialChargesGraph = PXGraph.CreateInstance<ARFinChargesApplyMaint>();

				financialChargesGraph.CalculateFinancialCharges(filter);
				PXLongOperation.SetCustomInfo(financialChargesGraph);
			});

			return adapter.Get();
		}
		#endregion

		#region Processing Functions
		protected virtual void CalculateFinancialCharges(ARFinChargesApplyParameters filter)
		{
			this.Filter.Cache.Clear();
			this.Filter.Insert(filter);

			Company company = PXSelect<Company>.Select(this);
			ARSetup ARSetup = PXSelect<ARSetup>.Select(this);

			PXSelectBase<Customer> customersInStatementCycle = new PXSelectJoin<
				Customer,
				InnerJoin<Location, 
					On<Location.bAccountID, Equal<Customer.bAccountID>, 
					And<Location.locationID, Equal<Customer.defLocationID>>>,
				InnerJoin<CustomerClass, 
					On<CustomerClass.customerClassID, Equal<Customer.customerClassID>>,
				InnerJoin<ARStatementCycle, 
					On<ARStatementCycle.statementCycleId, Equal<Customer.statementCycleId>>>>>,
				Where<
					Customer.finChargeApply, Equal<True>, 
					And<Customer.status, NotEqual<BAccount.status.inactive>, 
					And<Customer.status, NotEqual<BAccount.status.hold>,
					And<Match<Current<AccessInfo.userName>>>>>>>
				(this);

			AppendFilterRestrictions(customersInStatementCycle);

			ICollection<int?> customerIDsToSkip = GetCustomerIDsWithoutChargeableInvoices();

			foreach (PXResult<Customer, Location, CustomerClass, ARStatementCycle> it in customersInStatementCycle.Select())
			{
				Customer cust = (Customer)it;
				Location defLocation = (Location)it;
				CustomerClass cClass = (CustomerClass)it;
				ARStatementCycle sCycle = (ARStatementCycle)it;

				if (customerIDsToSkip.Contains(cust.BAccountID)) continue;

				ARFinCharge ARfc = null;
				if (ARSetup.DefFinChargeFromCycle ?? false)
				{
					if (!(sCycle.FinChargeApply ?? false))
						continue; //Fin. charge is not applicable
					ARfc = PXSelect<ARFinCharge, Where<ARFinCharge.finChargeID, Equal<Required<ARFinCharge.finChargeID>>>>.
										Select(this, sCycle.FinChargeID);
				}
				else
				{
					if (!(cClass.FinChargeApply ?? false))
						continue; //Fin. charge is not applicable
					ARfc = PXSelect<ARFinCharge, Where<ARFinCharge.finChargeID, Equal<Required<ARFinCharge.finChargeID>>>>.
										Select(this, cClass.FinChargeID);
				}

				if ((ARfc == null) || (ARfc.FinChargeID == null))
					continue;

				//bool hasUnreleased = CheckForUnreleasedCharges(this, cust.BAccountID.Value);
				bool hasOpenPayments = CheckForOpenPayments(this, cust.BAccountID);


				Dictionary<ARDocKey, ARInvoice> result = new Dictionary<ARDocKey, ARInvoice>();
				Dictionary<ARDocKey, ARInvoice> writeOffs = new Dictionary<ARDocKey, ARInvoice>();
				// then we take invoices and 
				if (ARfc.CalculationMethod == OverdueCalculationMethod.InterestOnBalance)
				{
					foreach (PXResult<ARInvoice, ARAdjust> iDoc in PXSelectJoin<
						ARInvoice,
							InnerJoin<ARAdjust,
								On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>,
								And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>>,
						Where<
							ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
							And<ARInvoice.dueDate, Less<Required<ARInvoice.dueDate>>,
							And<ARAdjust.adjgDocDate, GreaterEqual<Required<ARAdjust.adjgDocDate>>,
                            And<ARInvoice.released, Equal<True>,
							And<ARInvoice.openDoc, Equal<False>,
							And<ARInvoice.applyOverdueCharge, Equal<True>>>>>>>>
						.Select(this, cust.BAccountID, filter.FinChargeDate, filter.FinChargeDate))
					{
						ARInvoice inv = iDoc;
						ARAdjust adj = iDoc;
						if ((inv.DocType == ARDocType.Invoice) || ((inv.DocType == ARDocType.DebitMemo) ||
								(ARSetup.FinChargeOnCharge == true) && (inv.DocType == ARDocType.FinCharge))
								)
						{
							ARDocKey docKey = new ARDocKey(inv.DocType, inv.RefNbr);
							if (inv.LastFinChargeDate >= filter.FinChargeDate) continue;
							if (writeOffs.ContainsKey(docKey))
								continue; //Ignore write offs
							ARInvoice doc = null;
							if (result.ContainsKey(docKey))
							{
								doc = result[docKey];
								if (adj.AdjgDocType == ARDocType.SmallBalanceWO)
								{
									writeOffs[docKey] = inv;
									result.Remove(docKey); // Closed Invoices, having write offs - are not subject for charging
									continue;
								}
							}
							else
							{
								doc = PXCache<ARInvoice>.CreateCopy(inv);
                                doc.CuryDocBal = decimal.Zero;
								doc.DocBal = decimal.Zero;
                                result[docKey] = doc;
							}
                            doc.DocBal += AdjustDocumentBalance(adj);
                            doc.CuryDocBal += AdjustCuryDocumentBalance(adj);
                        }
					}
				}
				PXResultset<ARInvoice> docsForCharge;
				if (ARfc.CalculationMethod == OverdueCalculationMethod.InterestOnBalance)
				{
					docsForCharge = PXSelect<ARInvoice, Where<ARInvoice.released, Equal<True>,
															And<ARInvoice.pendingPPD, NotEqual<True>, 
															And<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
															And<ARInvoice.dueDate, LessEqual<Required<ARInvoice.dueDate>>,
															And<ARRegister.openDoc, Equal<True>,
															And<ARInvoice.applyOverdueCharge, Equal<True>>>>>>>>.
												 Select(this, cust.BAccountID, filter.FinChargeDate);
				}
				else if (ARfc.CalculationMethod == OverdueCalculationMethod.InterestOnProratedBalance)
				{
					docsForCharge = PXSelect<ARInvoice, Where<ARInvoice.released, Equal<True>,
															And<ARInvoice.pendingPPD, NotEqual<True>, 
															And<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
															And<ARInvoice.dueDate, LessEqual<Required<ARInvoice.dueDate>>,
															And<ARInvoice.applyOverdueCharge, Equal<True>>>>>>>.
												 Select(this, cust.BAccountID, filter.FinChargeDate);
				}
				else
				{
					docsForCharge = PXSelect<ARInvoice, Where<ARInvoice.released, Equal<True>,
															 And<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
															 And<ARInvoice.dueDate, LessEqual<Required<ARInvoice.dueDate>>,
															 And<ARRegister.openDoc, Equal<False>,
															 And<ARInvoice.applyOverdueCharge, Equal<True>>>>>>>.
												 Select(this, cust.BAccountID, filter.FinChargeDate);
				}
				foreach (ARInvoice inv in docsForCharge)
				{
					if ((inv.DocType == ARDocType.Invoice) || ((inv.DocType == ARDocType.DebitMemo) ||
							(ARSetup.FinChargeOnCharge == true) && (inv.DocType == ARDocType.FinCharge))
							)
					{
						if (inv.LastFinChargeDate >= filter.FinChargeDate) continue;
						ARDocKey docKey = new ARDocKey(inv.DocType, inv.RefNbr);
						ARInvoice doc = result.ContainsKey(docKey) ? result[docKey] : PXCache<ARInvoice>.CreateCopy(inv);
						result[docKey] = doc;

						foreach (ARAdjust adjustment in PXSelect<ARAdjust, Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
															  And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
															  And<ARAdjust.adjgDocDate, Greater<Required<ARAdjust.adjgDocDate>>,
															  And<ARAdjust.released, Equal<True>>>>>>.
																Select(this, inv.DocType, inv.RefNbr, filter.FinChargeDate))
                        {
						    doc.DocBal += AdjustDocumentBalance(adjustment);
							doc.CuryDocBal += AdjustCuryDocumentBalance(adjustment);
                        }
                    }
				}

				foreach (ARInvoice inv in result.Values)
				{
					decimal docBalance = (decimal)inv.DocBal;
					decimal curdocBalance = (decimal)inv.CuryDocBal;

					CurrencyRate rate;
					CurrencyInfo invinfo = PXSelect<CurrencyInfo,
												Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.
												Select(this, inv.CuryInfoID);

					if (invinfo.CuryID == invinfo.BaseCuryID || ARfc.BaseCurFlag == true)
					{
						rate = new CurrencyRate();
						rate.CuryRate = 1m;
						rate.CuryMultDiv = "M";
					}
					else if ((rate = PXSelect<CurrencyRate,
									Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
									And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
									And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
									And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
									OrderBy<Desc<CurrencyRate.curyEffDate>>>.
									Select(this,
													invinfo.CuryID,
													invinfo.BaseCuryID,
													cust.CuryRateTypeID,
													filter.FinChargeDate)) == null)
					{
						throw new PXException(PX.Objects.CM.Messages.RateIsNotDefinedForThisDate, cust.CuryRateTypeID, invinfo.CuryID);
					}

					if (( ARfc.CalculationMethod == OverdueCalculationMethod.InterestOnBalance && inv.CuryDocBal > 0m ) 
                        || ARfc.CalculationMethod == OverdueCalculationMethod.InterestOnProratedBalance 
                        || (ARfc.CalculationMethod == OverdueCalculationMethod.InterestOnArrears && inv.CuryDocBal == 0m))
					{
						ARFinChargesDetails finCharge = new ARFinChargesDetails();
						Copy(finCharge, inv);
						finCharge.Selected = true;
						finCharge.FinChargeID = ARfc.FinChargeID;
						finCharge.TermsID = ARfc.TermsID;
						finCharge.ARAccountID = defLocation.ARAccountID;
						finCharge.ARSubID = defLocation.ARSubID;
						finCharge.LastPaymentDate = inv.LastPaymentDate;
						finCharge.LastChargeDate = inv.LastFinChargeDate;
						finCharge.DueDate = inv.DueDate;

						if (inv.CuryDocBal == 0m && inv.LastFinChargeDate != null && finCharge.LastPaymentDate !=null && inv.LastFinChargeDate > finCharge.LastPaymentDate)
							continue;
						TimeSpan diff;
						
						if (inv.CuryDocBal == 0m && inv.LastPaymentDate <= filter.FinChargeDate)
						{
							diff = (TimeSpan)(inv.LastPaymentDate - (inv.LastFinChargeDate ?? inv.DueDate));
						}
						else
						{
							diff = (TimeSpan)(filter.FinChargeDate - (inv.LastFinChargeDate ?? inv.DueDate));
						}
						finCharge.OverdueDays = (short?)(diff.Days > 0 ? diff.Days : 0);

						if (ARfc.BaseCurFlag == true)
						{
							finCharge.FinChargeCuryID = invinfo.BaseCuryID;
							finCharge.CuryRate = 1m;
							finCharge.CuryMultDiv = "M";
							finCharge.CuryRateTypeID = cust.CuryRateTypeID;

							CalcFCAmount(finCharge, ARfc, (decimal)inv.DocBal, this, filter.FinChargeDate.Value);
							//Rounding to the correct precision - for display 
							finCharge.FinChargeAmt = PXCurrencyAttribute.Round(this.Caches[typeof(ARInvoice)], inv, (decimal)finCharge.FinChargeAmt, CMPrecision.BASECURY);
						}
						else
						{
							finCharge.FinChargeCuryID = invinfo.CuryID;
							finCharge.CuryRate = rate.CuryRate;
							finCharge.CuryMultDiv = rate.CuryMultDiv;
							finCharge.CuryRateTypeID = cust.CuryRateTypeID;

							CalcFCAmount(finCharge, ARfc, (decimal)inv.CuryDocBal, this, filter.FinChargeDate.Value);
							//Rounding to the correct precision - for display 
							finCharge.FinChargeAmt = PXCurrencyAttribute.Round(this.Caches[typeof(ARInvoice)], inv, (decimal)finCharge.FinChargeAmt, CMPrecision.TRANCURY);
						}

						if (finCharge.FinChargeAmt <= Decimal.Zero)
							continue;

						finCharge.LastFCRefNbr = FindUnreleasedChargeForDoc(this, inv.DocType, inv.RefNbr);
						finCharge.LastFCDocType = ARDocType.FinCharge;
						finCharge.FinChargeDate = filter.FinChargeDate;
						finCharge.FinPeriodID = filter.FinPeriodID;

						bool hasUnreleased = !string.IsNullOrEmpty(finCharge.LastFCRefNbr);

						finCharge.HasUnreleasedCharges = hasUnreleased;
						finCharge.CustomerHasOpenPayments = hasOpenPayments;

						finCharge.Selected = !(hasUnreleased || hasOpenPayments);
						finCharge = this.ARFinChargeRecords.Insert(finCharge);
					}
				}
			}
		}

	    private decimal? AdjustDocumentBalance(ARAdjust adjustment)
	    {
            return  adjustment.AdjAmt +
                    adjustment.AdjDiscAmt +
                    adjustment.AdjWOAmt +
                    (adjustment.ReverseGainLoss == true ? -adjustment.RGOLAmt : adjustment.RGOLAmt);
	    }

	    private decimal? AdjustCuryDocumentBalance(ARAdjust adjustment)
	    {
            return adjustment.CuryAdjdAmt +
                    adjustment.CuryAdjdDiscAmt +
                    adjustment.CuryAdjdWOAmt;
	    }

        private void AppendFilterRestrictions(PXSelectBase<Customer> query)
		{
			if (Filter.Current?.CustomerID != null)
			{
				query.WhereAnd<Where<Customer.bAccountID, Equal<Current<ARFinChargesApplyParameters.customerID>>>>();
			}

			if (Filter.Current?.CustomerClassID != null)
			{
				query.WhereAnd<Where<Customer.customerClassID, Equal<Current<ARFinChargesApplyParameters.customerClassID>>>>();
			}

			if (Filter.Current?.StatementCycle != null)
			{
				query.WhereAnd<Where<Customer.statementCycleId, Equal<Current<ARFinChargesApplyParameters.statementCycle>>>>();
			}
		}

		/// <remarks>
		/// Used for coarse filtration of customers in the inner loop of <see cref="Calculate"/> 
		/// so as to skip (and avoid redundant DB selects / processing) those customers who 
		/// don't have any chargeable documents. Intentionally done using a separate query
		/// so as not to overload performance / memory consumption of the <see cref="Calculate"/>'s 
		/// main query.
		/// </remarks>
		/// <returns>
		/// IDs of the customers that do not have any released chargeable invoices and that
		/// satisfy the current filter values.
		/// </returns>
		private ICollection<int?> GetCustomerIDsWithoutChargeableInvoices()
		{
			PXSelectBase<Customer> customersWithoutChargeableInvoices = new PXSelectJoin<
				Customer,
				LeftJoin<ARInvoice,
					On<ARInvoice.customerID, Equal<Customer.bAccountID>,
					And<ARInvoice.released, Equal<True>,
					And<ARInvoice.dueDate, LessEqual<Required<ARInvoice.dueDate>>,
					And<ARInvoice.applyOverdueCharge, Equal<True>>>>>>,
				Where<
					Customer.finChargeApply, Equal<True>,
					And<Customer.status, NotEqual<BAccount.status.inactive>,
					And<Customer.status, NotEqual<BAccount.status.hold>,
					And<ARInvoice.refNbr, IsNull>>>>>
				(this);

			AppendFilterRestrictions(customersWithoutChargeableInvoices);

			IEnumerable<int?> customerIDs;

			using (new PXFieldScope(
				customersWithoutChargeableInvoices.View,
				typeof(Customer.bAccountID),
				typeof(Customer.acctCD)))
			{
				customerIDs = customersWithoutChargeableInvoices
					.Select(Filter.Current?.FinChargeDate)
					.RowCast<Customer>()
					.Select(customer => customer.BAccountID);
			}

			return new HashSet<int?>(customerIDs);
		}

		private static void CreateFCDoc(List<ARFinChargesDetails> list, ARFinChargesApplyParameters filter)
		{
			ARInvoiceEntry ie = PXGraph.CreateInstance<ARInvoiceEntry>();

			FinPeriod finPeriod = ie.FinPeriodRepository.FindByID(PXAccess.GetParentOrganizationID(PXAccess.GetBranchID()), filter.FinPeriodID);
			ie.FinPeriodUtils.CanPostToPeriod(finPeriod).RaiseIfHasError();

			ie.IsProcessingMode = true;
			ie.FieldVerifying.AddHandler<ARInvoice.customerID>((sender, e) => { e.Cancel = true; });
			ie.ARSetup.Current.RequireControlTotal = false;
			Customer cust = null;
			ARFinCharge arFC = null;
			bool failed = false;
			Dictionary<string, MessageWithLevel> listMessages = new Dictionary<string, MessageWithLevel>();
			List<string> processedKeys = new List<string>();
			PXLongOperation.SetCustomInfo(listMessages);
			list.Sort(Compare);
			Dictionary<string, DateTime> updatedCycles = new Dictionary<string, DateTime>();
			for (int i = 0; i < list.Count; i++)
			{
				ARFinChargesDetails fcDetail = list[i];
				if (!fcDetail.FinChargeDate.HasValue || string.IsNullOrEmpty(fcDetail.FinPeriodID))
				{
					throw new PXException(Messages.OverdueChargeDateAndFinPeriodAreRequired); //Stop processing other docs - they all have the same date
				}
				try
				{
					if (fcDetail.HasUnreleasedCharges ?? false)
					{
						throw new PXException(Messages.ERR_UnreleasedFinChargesForDocument);
					}


					if (arFC == null || arFC.FinChargeID != fcDetail.FinChargeID)
						arFC = PXSelect<ARFinCharge, Where<ARFinCharge.finChargeID, Equal<Required<ARFinCharge.finChargeID>>>>.
															   Select(ie, fcDetail.FinChargeID);

					if (ie.Document.Current == null)
					{
						if (cust == null || cust.BAccountID != fcDetail.CustomerID)
						{
							cust = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(ie, fcDetail.CustomerID);
						}

						ARInvoice inv = new ARInvoice();
						inv.DocType = ARDocType.FinCharge;

						inv = PXCache<ARInvoice>.CreateCopy(ie.Document.Insert(inv));
						inv.ARAccountID = fcDetail.ARAccountID;
						inv.ARSubID = fcDetail.ARSubID;
						inv.CustomerID = fcDetail.CustomerID;
						inv = ie.Document.Update(inv);
						inv.CuryID = fcDetail.FinChargeCuryID;
						inv.DocDate = filter.FinChargeDate;
						using (new PXLocaleScope(cust.LocaleName))
							inv.DocDesc = PXMessages.LocalizeNoPrefix(Messages.FinCharge);

						FinPeriodIDAttribute.SetPeriodsByMaster<ARInvoice.finPeriodID>(ie.Document.Cache, inv, filter.FinPeriodID);
						inv.TermsID = arFC.TermsID;
						inv = ie.Document.Update(inv);
					}
					if (fcDetail.FinChargeAmt >= arFC.MinFinChargeAmount)
					{
						ARTran det = new ARTran();

						det.AccountID = arFC.FinChargeAccountID;
						det.SubID = arFC.FinChargeSubID;

						det.TranType = ARDocType.FinCharge;
						det.TranDate = filter.FinChargeDate;
						det.CuryTranAmt = fcDetail.FinChargeAmt;
						det.CuryUnitPrice = fcDetail.FinChargeAmt;
						using (new PXLocaleScope(cust.LocaleName))
						{
							det.TranDesc =
							string.Concat(PXStringListAttribute.GetLocalizedLabel<ARFinChargesDetails.docType>(ie.Caches[typeof(ARFinChargesDetails)], fcDetail),
							" ", fcDetail.RefNbr);
						}
						det.LineType = "";
						FinPeriodIDAttribute.SetPeriodsByMaster<ARTran.finPeriodID>(ie.Transactions.Cache, det, filter.FinPeriodID);
						det.Qty = (decimal)1.0;
						det.Released = false;
						det.DrCr = DrCr.Credit;
						det.TranClass = "";
						det.TaxCategoryID = arFC.TaxCategoryID;
						det.Commissionable = false;
						det = ie.Transactions.Insert(det);
						if (string.IsNullOrEmpty(arFC.TaxCategoryID))
						{
							//In this case special update is required, to prevent normal document's default
							det = PXCache<ARTran>.CreateCopy(det);
							det.TaxCategoryID = arFC.TaxCategoryID;
							det = ie.Transactions.Update(det);
						}

						ARFinChargeTran fcTran = new ARFinChargeTran();
						fcTran.TranType = det.TranType;
						fcTran.RefNbr = det.RefNbr;
						fcTran.LineNbr = det.LineNbr;
						fcTran.OrigDocType = fcDetail.DocType;
						fcTran.OrigRefNbr = fcDetail.RefNbr;
						fcTran.FinChargeID = arFC.FinChargeID;
						fcTran = ie.finChargeTrans.Insert(fcTran);
						processedKeys.Add(fcDetail.RefNbr + fcDetail.DocType);
					}
					else
					{
						listMessages.Add(fcDetail.RefNbr + fcDetail.DocType, new MessageWithLevel(Messages.LineAmountBelowMin, PXErrorLevel.RowWarning));
						if (fcDetail.CuryDocBal == 0m)
						{
							PXDatabase.Update<ARInvoice>(new PXDataFieldAssign<ARInvoice.applyOverdueCharge>(false),
														new PXDataFieldRestrict<ARInvoice.docType>(PXDbType.VarChar, 3, fcDetail.DocType, PXComp.EQ),
														new PXDataFieldRestrict<ARInvoice.refNbr>(PXDbType.NVarChar, 15, fcDetail.RefNbr, PXComp.EQ));
						}
					}
					if ((i == list.Count - 1) || (Compare(fcDetail, list[i + 1]) != 0))
					{
						if (ie.finChargeTrans.Any())
						{
							ARInvoice inv = ie.Document.Current;
							if (arFC.MinChargeDocumentAmt > 0m && arFC.MinChargeDocumentAmt > inv.CuryDocBal)
							{
								PXException e = new PXException(Messages.DocumentAmountBelowMin);
								throw e;
							}
							if (arFC.FeeAmount != null && arFC.FeeAmount != 0m)
							{
								ARTran feeDet = new ARTran();

								feeDet.AccountID = arFC.FeeAccountID;
								feeDet.SubID = arFC.FeeSubID;

								feeDet.TranDate = filter.FinChargeDate;
								feeDet.CuryTranAmt = arFC.FeeAmount;
								feeDet.CuryUnitPrice = arFC.FeeAmount;
								feeDet.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(ie.Caches[typeof(ARFinCharge)], arFC, typeof(ARFinCharge.feeDesc).Name, cust.LocaleName);
								feeDet.LineType = "";
								FinPeriodIDAttribute.SetPeriodsByMaster<ARTran.finPeriodID>(ie.Transactions.Cache, feeDet, filter.FinPeriodID);
								feeDet.Qty = (decimal)1.0;
								feeDet.Released = false;
								feeDet.DrCr = DrCr.Credit;
								feeDet.TranClass = "";
								feeDet.TaxCategoryID = arFC.TaxCategoryID;
								feeDet.Commissionable = false;
								feeDet = ie.Transactions.Insert(feeDet);
								if (string.IsNullOrEmpty(arFC.TaxCategoryID))
								{
									//In this case special update is required, to prevent normal document's default
									feeDet = PXCache<ARTran>.CreateCopy(feeDet);
									feeDet.TaxCategoryID = arFC.TaxCategoryID;
									feeDet = ie.Transactions.Update(feeDet);
								}
							}
							inv = ie.Document.Current;
							inv.CuryOrigDocAmt = inv.CuryDocBal;
							ARSetup setup = PXSelect<ARSetup>.Select(ie);
							inv.ApplyOverdueCharge = inv.ApplyOverdueCharge.Value && setup.FinChargeOnCharge.Value;
							ie.Save.Press();
						}
						if (!updatedCycles.ContainsKey(cust.StatementCycleId))
						{
							updatedCycles[cust.StatementCycleId] = fcDetail.FinChargeDate.Value;
						}
						ie.Clear();
						foreach (string key in processedKeys)
						{
							if (listMessages.ContainsKey(key))
							{
								listMessages[key].Message = ActionsMessages.RecordProcessed;
								listMessages[key].Level = PXErrorLevel.RowInfo;
							}
							else
							{
								listMessages.Add(key, new MessageWithLevel(ActionsMessages.RecordProcessed, PXErrorLevel.RowInfo));
							}
						}
						processedKeys.Clear();
					}
				}
				catch (Exception e)
				{
					ie.Clear();
					if (!processedKeys.Contains(fcDetail.RefNbr + fcDetail.DocType))
					{
						processedKeys.Add(fcDetail.RefNbr + fcDetail.DocType);
					}
					foreach (string key in processedKeys)
					{
						if (listMessages.ContainsKey(key))
						{
							listMessages[key].Message = e.Message;
							listMessages[key].Level = PXErrorLevel.RowError;
						}
						else
						{
							listMessages.Add(key, new MessageWithLevel(e.Message, PXErrorLevel.RowError));
						}
					}
					processedKeys.Clear();
					failed = true;
				}
			}
			ARStatementMaint cycleMaintGraph = PXGraph.CreateInstance<ARStatementMaint>();
			foreach (KeyValuePair<string, DateTime> iCycle in updatedCycles)
			{
				ARStatementCycle row = cycleMaintGraph.ARStatementCycleRecord.Search<ARStatementCycle.statementCycleId>(iCycle.Key);
				cycleMaintGraph.ARStatementCycleRecord.Current = row;
				if (!(row.LastFinChrgDate.HasValue && row.LastFinChrgDate >= iCycle.Value))
					row.LastFinChrgDate = iCycle.Value;
				row = cycleMaintGraph.ARStatementCycleRecord.Update(row);
				cycleMaintGraph.Save.Press();
			}
			if (failed)
			{
				throw new PXException(ErrorMessages.SeveralItemsFailed);
			}
		}

		private static bool CheckForUnreleasedCharges(PXGraph aGraph, int aCustomerID)
		{
			ARRegister doc = PXSelect<ARRegister,
							 Where<ARRegister.docType, Equal<ARDocType.finCharge>,
							 And<ARRegister.released, Equal<BQLConstants.BitOff>,
							 And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>>>>>.SelectWindowed(aGraph, 0, 1, aCustomerID);
			return (doc != null);
		}

		private static string FindUnreleasedChargeForDoc(PXGraph aGraph, string aDocType, string aRefNbr)
		{
			ARInvoice doc = (ARInvoice)PXSelectJoin<ARInvoice, InnerJoin<ARFinChargeTran,
							 On<ARInvoice.docType, Equal<ARFinChargeTran.tranType>,
								And<ARInvoice.refNbr, Equal<ARFinChargeTran.refNbr>>>>,
							 Where<ARFinChargeTran.origDocType, Equal<Required<ARFinChargeTran.origDocType>>,
								And<ARFinChargeTran.origRefNbr, Equal<Required<ARFinChargeTran.origRefNbr>>,
								And<ARInvoice.released, Equal<False>>>>>.SelectWindowed(aGraph, 0, 1, aDocType, aRefNbr);
			if (doc != null && !string.IsNullOrEmpty(doc.RefNbr))
				return doc.RefNbr;
			return null;
		}

		private static bool CheckForOpenPayments(PXGraph aGraph, int? aCustomerID)
		{
			ARRegister doc = PXSelect<ARPayment, Where<ARPayment.customerID, Equal<Required<ARPayment.customerID>>,
				And<ARPayment.openDoc, Equal<True>>>>.SelectWindowed(aGraph, 0, 1, aCustomerID);
			return (doc != null);
		}

		static void CalcFCAmount(ARFinChargesDetails aDest, ARFinCharge aDef, decimal aDocBalance, PXGraph graph, DateTime calculationDate)
		{
			decimal amtFC = decimal.Zero;
			decimal sampleCuryRate = (decimal)aDest.SampleCuryRate;
			DateTime startDate = aDest.LastChargeDate != null && aDest.LastChargeDate.Value > aDest.DueDate.Value ?
				aDest.LastChargeDate.Value : aDest.DueDate.Value;

			if ((bool)aDef.PercentFlag == true)
			{
				List<ARFinChargePercent> percentList = new List<ARFinChargePercent>();
				ARFinChargePercent firstRow = PXSelect<ARFinChargePercent,
				Where<ARFinChargePercent.finChargeID, Equal<Required<ARFinCharge.finChargeID>>,
					And<ARFinChargePercent.beginDate, LessEqual<Required<ARFinChargePercent.beginDate>>>>,
				OrderBy<Desc<ARFinChargePercent.beginDate>>>.SelectWindowed(graph, 0, 1, aDef.FinChargeID, startDate);
				if (firstRow != null)
				{
					percentList.Add(firstRow);
				}
				//In case when percent was changed in the middle of calculation period
				foreach (ARFinChargePercent percentRow in PXSelect<ARFinChargePercent,
				Where<ARFinChargePercent.finChargeID, Equal<Required<ARFinCharge.finChargeID>>,
					And<ARFinChargePercent.beginDate, Greater<Required<ARFinChargesDetails.docDate>>,
					And<ARFinChargePercent.beginDate, LessEqual<Required<ARFinChargePercent.beginDate>>>>>>.Select(graph, aDef.FinChargeID, startDate, calculationDate))
				{
					percentList.Add(percentRow);
				} 
				if (percentList.Count < 1)
				{
					throw new PXException(Messages.PercentForDateNotFound);
				}
				for (int i = 0; i < percentList.Count; i++)
				{

					DateTime startDateForCurrentPercent = (percentList[i].BeginDate.Value.AddDays(-1) > startDate ? percentList[i].BeginDate.Value.AddDays(-1) : startDate);
					DateTime endDateForCurrentPercent;
					if (i + 1 < percentList.Count)
					{
						endDateForCurrentPercent = percentList[i + 1].BeginDate.Value.AddDays(-1);
					}
					else
					{
						endDateForCurrentPercent = calculationDate;
					}
					amtFC += (decimal)(aDocBalance * (endDateForCurrentPercent - startDateForCurrentPercent).Days * percentList[i].FinChargePercent / (365 * 100));

					if (aDef.CalculationMethod != OverdueCalculationMethod.InterestOnBalance)
					{
						foreach (ARAdjust adj in PXSelect<ARAdjust,
							Where<ARAdjust.adjdDocType, Equal<Required<ARInvoice.docType>>,
							And<ARAdjust.adjdRefNbr, Equal<Required<ARInvoice.refNbr>>,
							And<ARAdjust.adjgDocDate, LessEqual<Required<ARInvoice.docDate>>,
							And<ARAdjust.adjgDocDate, GreaterEqual<Required<ARInvoice.lastFinChargeDate>>,
							And<ARAdjust.released, Equal<True>>>>>>>.Select(graph, aDest.DocType, aDest.RefNbr, calculationDate, startDateForCurrentPercent))
						{
							DateTime endDateForCurrentPaymentAndPercent = endDateForCurrentPercent > adj.AdjgDocDate.Value ? adj.AdjgDocDate.Value : endDateForCurrentPercent;
							int overdueDaysForCurrentPayment = (endDateForCurrentPaymentAndPercent - startDateForCurrentPercent).Days;
							amtFC += (decimal)((adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt + (adj.ReverseGainLoss == true ? -adj.RGOLAmt : adj.RGOLAmt))
								/ sampleCuryRate * overdueDaysForCurrentPayment * percentList[i].FinChargePercent / (365 * 100));
                        }
					}
				}
			}

			//Fixed values in ARFinCharge definition are defined in BaseCurrency - so they need to be converted to currency of current Fin. charge.
			if (amtFC < aDef.MinFinChargeAmount * sampleCuryRate)
			{
				if (aDef.MinFinChargeFlag == true && (calculationDate - startDate).Days > 0)
				{
					amtFC = (decimal)aDef.MinFinChargeAmount;
				}
				else if (aDocBalance != 0m)
				{
					amtFC = decimal.Zero;
				}
			}
			aDest.FinChargeAmt = amtFC;
		}
		#endregion

		#region Events
		protected virtual void ARFinChargesDetails_RefNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			ARFinChargesDetails row = (ARFinChargesDetails)e.Row;
			if (row != null)
			{
				Dictionary<string, MessageWithLevel> listMessages = PXLongOperation.GetCustomInfo(this.UID) as Dictionary<string, MessageWithLevel>;
				TimeSpan timespan;
				Exception ex;
				PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);
				if ((status == PXLongRunStatus.Aborted || status == PXLongRunStatus.Completed) && listMessages != null)
				{
					MessageWithLevel message = null;
					if (listMessages.ContainsKey(row.RefNbr + row.DocType))
						message = listMessages[row.RefNbr + row.DocType];
					string fieldName = typeof(GLTranDoc.refNbr).Name;
					if (message != null)
					{
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
									null, null, message.Message, message.Level, null, null, null, PXUIVisibility.Undefined, null, null, null);

						e.IsAltered = true;
					}
				}
			}
		}

		protected virtual void ARFinChargesDetails_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARFinChargesDetails overdueCharge = e.Row as ARFinChargesDetails;

			if (overdueCharge == null) return;

			if (overdueCharge.HasUnreleasedCharges == true)
			{
				sender.RaiseExceptionHandling<ARFinChargesDetails.refNbr>(
					overdueCharge, 
					overdueCharge.RefNbr, 
					new PXSetPropertyException(Messages.ERR_UnreleasedFinChargesForDocument, PXErrorLevel.RowError));
			}
			else if (overdueCharge.CustomerHasOpenPayments == true)
			{
				sender.RaiseExceptionHandling<ARFinChargesDetails.refNbr>(
					overdueCharge, 
					overdueCharge.RefNbr, 
					new PXSetPropertyException(Messages.WRN_FinChargeCustomerHasOpenPayments, PXErrorLevel.RowInfo));
			}
		}

		protected virtual void ARFinChargesApplyParameters_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARFinChargesApplyParameters row = (ARFinChargesApplyParameters)e.Row;
			bool defaultFromCycle = this.ARSetup.Current.DefFinChargeFromCycle ?? false;
			PXUIFieldAttribute.SetRequired<ARFinChargesApplyParameters.finPeriodID>(cache, true);
			PXUIFieldAttribute.SetRequired<ARFinChargesApplyParameters.statementCycle>(cache, defaultFromCycle);
			PXDefaultAttribute.SetPersistingCheck<ARFinChargesApplyParameters.statementCycle>(cache, row, defaultFromCycle ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetVisible<ARFinChargesApplyParameters.customerClassID>(cache, e.Row, !defaultFromCycle);
			PXUIFieldAttribute.SetVisible<ARFinChargesApplyParameters.customerID>(cache, e.Row, !defaultFromCycle);
			bool hasDocs = (this.ARFinChargeRecords.Any());
			this.ARFinChargeRecords.SetProcessAllEnabled(hasDocs);
			this.ARFinChargeRecords.SetProcessEnabled(hasDocs);

		}

		protected virtual void ARFinChargesApplyParameters_StatementCycle_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			ARFinChargesApplyParameters row = (ARFinChargesApplyParameters)e.Row;
			bool defaultFromCycle = this.ARSetup.Current.DefFinChargeFromCycle ?? false;
			if (defaultFromCycle)
			{
				ARStatementCycle cycleDef = PXSelect<ARStatementCycle>.Search<ARStatementCycle.statementCycleId>(this, row.StatementCycle);
				if (cycleDef != null)
				{
					row.FinChargeDate = ARStatementProcess.FindNextStatementDate(this, this.Accessinfo.BusinessDate.Value, cycleDef);
					cache.SetDefaultExt<ARFinChargesApplyParameters.finPeriodID>(e.Row);
				}
			}

		}

		protected virtual void ARFinChargesApplyParameters_FinChargeDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<ARFinChargesApplyParameters.finPeriodID>(e.Row);
		}
		#endregion

		#region CacheAttached
		#region ARInvoice_LastFinChargeDate_CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[LastFinchargeDate]
		public virtual void ARInvoice_LastFinChargeDate_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region ARInvoice_LastPaymentDate_CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[LastPaymentDate]
		public virtual void ARInvoice_LastPaymentDate_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		#region Utility
		private static int Compare(ARFinChargesDetails aT0, ARFinChargesDetails aT1)
		{
			if (aT0.CustomerID != aT1.CustomerID) return aT0.CustomerID.Value.CompareTo(aT1.CustomerID.Value);
			if (aT0.ARAccountID != aT1.ARAccountID) return aT0.ARAccountID.Value.CompareTo(aT1.ARAccountID.Value);
			if (aT0.ARSubID != aT1.ARSubID) return aT0.ARSubID.Value.CompareTo(aT1.ARSubID.Value);
			return String.Compare(aT0.FinChargeCuryID, aT1.FinChargeCuryID);
		}

		private static void Copy(ARFinChargesDetails aDest, ARInvoice aSrc)
		{
			aDest.CustomerID = aSrc.CustomerID;
			aDest.DocType = aSrc.DocType;
			aDest.RefNbr = aSrc.RefNbr;
			aDest.DocDate = aSrc.DocDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.CuryOrigDocAmt = aSrc.CuryOrigDocAmt;
			aDest.CuryDocBal = aSrc.CuryDocBal;
		}

		private static bool CheckRequiredFieldsFilled(PXCache aCache, IBqlTable aFilter)
		{
			bool allFilled = true;
			foreach (var field in aCache.BqlFields)
			{
				PXFieldState state = aCache.GetStateExt(aFilter, field.Name) as PXFieldState;
				if (state != null)
				{
					object value = aCache.GetValue(aFilter, field.Name);
					if (state.Required == true && value == null)
					{
						aCache.RaiseExceptionHandling(field.Name, aFilter, null,
							new PXSetPropertyException(Messages.RequiredField));
						allFilled = false;
					}
				}
			}
			return allFilled;
		}
		#endregion
	}
}