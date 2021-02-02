using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using System.Globalization;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.CR.Standalone;
using PX.Objects.AR.Repositories;
using PX.Objects.AR.CustomerStatements;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	public class ARStatementPrint : PXGraph<ARStatementDetails>
	{
		#region Internal types definition
		[System.SerializableAttribute]
		public partial class PrintParameters : IBqlTable, PX.SM.IPrintable
		{
			#region BranchCD
			public abstract class branchCD : PX.Data.BQL.BqlString.Field<branchCD> { }
			protected String _BranchCD;
			[PXDefault("")]
			public virtual String BranchCD
			{
				get
				{
					return this._BranchCD;
				}
				set
				{
					this._BranchCD = value;
				}
			}
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[Branch()]
			public virtual Int32? BranchID
			{
				get
				{
					return this._BranchID;
				}
				set
				{
					this._BranchID = value;
				}
			}
			#endregion
			#region Action
			public abstract class action : PX.Data.BQL.BqlInt.Field<action> { }
			protected Int32? _Action;
			[PXDBInt]
			[PXDefault(0)]
			[PXUIField(DisplayName = "Actions")]
			[PXIntList(
				new int[] { Actions.Print, Actions.Email, Actions.MarkDontEmail, Actions.MarkDontPrint, Actions.Regenerate },
				new string[] { Messages.ProcessPrintStatement, Messages.ProcessEmailStatement, Messages.ProcessMarkDontEmail, Messages.ProcessMarkDontPrint, Messages.RegenerateStatement })]
			public virtual Int32? Action
			{
				get
				{
					return this._Action;
				}
				set
				{
					this._Action = value;
				}
			}
			#endregion
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
			protected String _StatementCycleId;
			[PXDBString(10, IsUnicode = true)]
			[PXDefault(typeof(ARStatementCycle))]
			[PXUIField(DisplayName = "Statement Cycle", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(ARStatementCycle.statementCycleId), DescriptionField = typeof(ARStatementCycle.descr))]
			public virtual String StatementCycleId
			{
				get
				{
					return this._StatementCycleId;
				}
				set
				{
					this._StatementCycleId = value;
				}
			}
			#endregion
			#region StatementDate
			public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
			protected DateTime? _StatementDate;
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Statement Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? StatementDate
			{
				get
				{
					return this._StatementDate;
				}
				set
				{
					this._StatementDate = value;
				}
			}
			#endregion
			#region Cury Statements
			public abstract class curyStatements : PX.Data.BQL.BqlBool.Field<curyStatements> { }
			protected Boolean? _CuryStatements;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Foreign Currency Statements")]
			public virtual Boolean? CuryStatements
			{
				get
				{
					return this._CuryStatements;
				}
				set
				{
					this._CuryStatements = value;
				}
			}
			#endregion
			#region ShowAll
			public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
			protected bool? _ShowAll = false;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show All")]
			public virtual bool? ShowAll
			{
				get
				{
					return _ShowAll;
				}
				set
				{
					_ShowAll = value;
				}
			}
			#endregion
			#region StatementMessage
			public abstract class statementMessage : PX.Data.BQL.BqlString.Field<statementMessage> { }
			[PXString(IsUnicode = true)]
			[PXUIField(DisplayName = Messages.Message)]
			public virtual string StatementMessage
			{
				get;
				set;
			}
			#endregion
			#region PrintWithDeviceHub
			public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
			protected bool? _PrintWithDeviceHub;
			[PXDBBool]
			[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
			[PXUIField(DisplayName = "Print with DeviceHub")]
			public virtual bool? PrintWithDeviceHub
			{
				get
				{
					return _PrintWithDeviceHub;
				}
				set
				{
					_PrintWithDeviceHub = value;
				}
			}
			#endregion
			#region DefinePrinterManually
			public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
			protected bool? _DefinePrinterManually = false;
			[PXDBBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Define Printer Manually")]
			public virtual bool? DefinePrinterManually
			{
				get
				{
					return _DefinePrinterManually;
				}
				set
				{
					_DefinePrinterManually = value;
				}
			}
			#endregion
			#region PrinterID
			public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
			protected Guid? _PrinterID;
			[PX.SM.PXPrinterSelector]
			public virtual Guid? PrinterID
			{
				get
				{
					return this._PrinterID;
				}
				set
				{
					this._PrinterID = value;
				}
			}
			#endregion
			#region NumberOfCopies
			public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
			protected int? _NumberOfCopies;
			[PXDBInt(MinValue = 1)]
			[PXDefault(1)]
			[PXFormula(typeof(Selector<PrintParameters.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
			[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual int? NumberOfCopies
			{
				get
				{
					return this._NumberOfCopies;
				}
				set
				{
					this._NumberOfCopies = value;
				}
			}
			#endregion

			public class Actions
			{
				public const int Print = 0;
				public const int Email = 1;
				public const int MarkDontEmail = 2;
				public const int MarkDontPrint = 3;
				public const int Regenerate = 4;
			}
		}

		[Serializable]
		public partial class DetailsResult : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected
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
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[Customer(DescriptionField = typeof(Customer.acctName), IsKey = true, DisplayName = "Customer")]
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
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, IsKey = true)]
			[PXSelector(typeof(CM.Currency.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Currency")]
			public virtual String CuryID
			{
				get
				{
					return this._CuryID;
				}
				set
				{
					this._CuryID = value;
				}
			}
			#endregion
			#region StatementBalance
			public abstract class statementBalance : PX.Data.BQL.BqlDecimal.Field<statementBalance> { }
			protected Decimal? _StatementBalance;
			[PXDBBaseCury()]
			[PXUIField(DisplayName = "Statement Balance")]
			public virtual Decimal? StatementBalance
			{
				get
				{
					return this._StatementBalance;
				}
				set
				{
					this._StatementBalance = value;
				}
			}
			#endregion
			#region CuryStatementBalance
			public abstract class curyStatementBalance : PX.Data.BQL.BqlDecimal.Field<curyStatementBalance> { }
			protected Decimal? _CuryStatementBalance;
			[PXCury(typeof(DetailsResult.curyID))]
			[PXUIField(DisplayName = "FC Statement Balance")]
			public virtual Decimal? CuryStatementBalance
			{
				get
				{
					return this._CuryStatementBalance;
				}
				set
				{
					this._CuryStatementBalance = value;
				}
			}
			#endregion
			#region UseCurrency
			public abstract class useCurrency : PX.Data.BQL.BqlBool.Field<useCurrency> { }
			protected Boolean? _UseCurrency;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "FC Statement")]
			public virtual Boolean? UseCurrency
			{
				get
				{
					return this._UseCurrency;
				}
				set
				{
					this._UseCurrency = value;
				}
			}
			#endregion
			#region AgeBalance00
			public abstract class ageBalance00 : PX.Data.BQL.BqlDecimal.Field<ageBalance00> { }
			protected Decimal? _AgeBalance00;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Age00 Balance")]
			public virtual Decimal? AgeBalance00
			{
				get
				{
					return this._AgeBalance00;
				}
				set
				{
					this._AgeBalance00 = value;
				}
			}
			#endregion
			#region CuryAgeBalance00
			public abstract class curyAgeBalance00 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance00> { }
			protected Decimal? _CuryAgeBalance00;
			[PXCury(typeof(DetailsResult.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "FC Age00 Balance")]
			public virtual Decimal? CuryAgeBalance00
			{
				get
				{
					return this._CuryAgeBalance00;
				}
				set
				{
					this._CuryAgeBalance00 = value;
				}
			}
			#endregion
			#region OverdueBalance
			public abstract class overdueBalance : PX.Data.BQL.BqlDecimal.Field<overdueBalance> { }
			[PXBaseCury()]
			[PXUIField(DisplayName = "Overdue Balance")]
			public virtual Decimal? OverdueBalance
			{
				[PXDependsOnFields(typeof(statementBalance), typeof(ageBalance00))]
				get
				{
					return this.StatementBalance - this.AgeBalance00;
				}
			}
			#endregion
			#region CuryOverdueBalance
			public abstract class curyOverdueBalance : PX.Data.BQL.BqlDecimal.Field<curyOverdueBalance> { }
			[PXCury(typeof(DetailsResult.curyID))]
			[PXUIField(DisplayName = "FC Overdue Balance")]
			public virtual Decimal? CuryOverdueBalance
			{
				[PXDependsOnFields(typeof(curyStatementBalance), typeof(curyAgeBalance00))]
				get
				{
					return (this._CuryStatementBalance) - (this.CuryAgeBalance00 ?? Decimal.Zero);
				}
			}
			#endregion
			#region DontPrint
			public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
			protected Boolean? _DontPrint;
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Don't Print")]
			public virtual Boolean? DontPrint
			{
				get
				{
					return this._DontPrint;
				}
				set
				{
					this._DontPrint = value;
				}
			}
			#endregion
			#region Printed
			public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
			protected Boolean? _Printed;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Printed")]
			public virtual Boolean? Printed
			{
				get
				{
					return this._Printed;
				}
				set
				{
					this._Printed = value;
				}
			}
			#endregion
			#region DontEmail
			public abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
			protected Boolean? _DontEmail;
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Don't Email")]
			public virtual Boolean? DontEmail
			{
				get
				{
					return this._DontEmail;
				}
				set
				{
					this._DontEmail = value;
				}
			}
			#endregion
			#region Emailed
			public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }
			protected Boolean? _Emailed;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Emailed")]
			public virtual Boolean? Emailed
			{
				get
				{
					return this._Emailed;
				}
				set
				{
					this._Emailed = value;
				}
			}
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[PXDBInt(IsKey = true)]
			[PXDefault()]
			[Branch()]
			[PXUIField(DisplayName = "Branch", Visible = false)]
			public virtual Int32? BranchID
			{
				get
				{
					return this._BranchID;
				}
				set
				{
					this._BranchID = value;
				}
			}
			#endregion

			[Obsolete("This method has been moved into " + nameof(ARStatementPrint.CreateDetailsResult) + " and will be removed in Acumatica ERP 8.0")]
			public virtual void Copy(ARStatement aSrc, Customer cust)
			{ }

			[Obsolete("This method has been moved into " + nameof(ARStatementPrint.AggregateDetailsResult) + " and will be removed in Acumatica ERP 8.0")]
			public virtual void Append(DetailsResult aSrc)
			{ }

			[Obsolete("This method has been moved into " + nameof(ARStatementPrint.ResetDetailsResultToBaseCurrency) + " and will be removed in Acumatica ERP 8.0")]
			public virtual void ResetToBaseCury(string aBaseCuryID)
			{ }
		}

		[Serializable]
		public class ContactR : Contact
		{
			public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
			public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		}
		#endregion

		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Customer", IsReadOnly = true)]
		[PXSelector(typeof(Search<Customer.bAccountID, Where<Customer.bAccountID, Equal<Current<ARStatement.customerID>>>>), SubstituteKey = typeof(Customer.acctCD), DescriptionField = typeof(Customer.acctCD), ValidateValue = false)]
		public virtual void ARStatement_CustomerID_CacheAttached() { }

		#region Ctor
		public ARStatementPrint()
		{
			ARSetup setup = ARSetup.Current;

			Details.Cache.AllowDelete = false;
			Details.Cache.AllowInsert = false;

			Details.SetSelected<ARInvoice.selected>();
			Details.SetProcessCaption(IN.Messages.Process);
			Details.SetProcessAllCaption(IN.Messages.ProcessAll);

			InquiriesFolder.MenuAutoOpen = true;
			InquiriesFolder.AddMenuAction(ViewDetails);

			StatementCycleRepository = new ARStatementCycleRepository(this);
		}
		#endregion

		#region Public Members
		public PXCancel<PrintParameters> Cancel;
		public PXAction<PrintParameters> prevStatementDate;
		public PXAction<PrintParameters> nextStatementDate;


		public PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<ARStatement.customerID>>>> dummyBaccountView;
		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<ARStatement.customerID>>>> dummyCustomerView;
		public PXSelect<Vendor> dummyVendorView;
		public PXSelect<EPEmployee> dummyEmployeeView;

		[PXViewName(Messages.Statement)]
		public PXSelect<ARStatement> Statement;
		[PXViewName(Messages.Customer)]
		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<ARStatement.customerID>>>> Customer;

		public PXFilter<PrintParameters> Filter;

		[PXFilterable]
		[PXVirtualDAC]
		public PXFilteredProcessing<DetailsResult, PrintParameters> Details;

		public PXSetup<ARSetup> ARSetup;
		public PXAction<PrintParameters> InquiriesFolder;
		public PXAction<PrintParameters> ViewDetails;

		public ARStatementCycleRepository StatementCycleRepository;

		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PrevStatementDate(PXAdapter adapter)
		{
			PrintParameters filter = this.Filter.Current;

			if (filter != null && !string.IsNullOrEmpty(filter.StatementCycleId))
			{
				ARStatement statement = PXSelect<
					ARStatement,
					Where<
						ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
						And<Where<
							ARStatement.statementDate, Less<Required<ARStatement.statementDate>>,
							Or<Required<ARStatement.statementDate>, IsNull>>>>,
					OrderBy<
						Desc<ARStatement.statementDate>>>
					.SelectWindowed(this, 0, 1, filter.StatementCycleId, filter.StatementDate, filter.StatementDate);

				if (statement != null)
				{
					filter.StatementDate = statement.StatementDate;
				}
			}

			Details.Cache.Clear();

			return adapter.Get();
		}

		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextStatementDate(PXAdapter adapter)
		{
			PrintParameters filter = this.Filter.Current;

			if (filter != null && !string.IsNullOrEmpty(filter.StatementCycleId))
			{
				ARStatement statement = PXSelect<
					ARStatement,
					Where<
						ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
						And<Where<
							ARStatement.statementDate, Greater<Required<ARStatement.statementDate>>,
							Or<Required<ARStatement.statementDate>, IsNull>>>>,
					OrderBy<
						Asc<ARStatement.statementDate>>>
					.SelectWindowed(this, 0, 1, filter.StatementCycleId, filter.StatementDate, filter.StatementDate);

				if (statement != null)
				{
					filter.StatementDate = statement.StatementDate;
				}
			}

			Details.Cache.Clear();

			return adapter.Get();
		}

		[PXViewName(Messages.BillingContactView)]
		public PXSelectJoin<
			ContactR,
				InnerJoin<Customer,
				On<ContactR.contactID, Equal<Customer.defBillContactID>>>,
			Where<
				Customer.bAccountID, Equal<Current<ARStatement.customerID>>>>
			CustomerDefaultBillingContact;

		#region Delegates
		protected virtual IEnumerable details()
		{
			ARSetup setup = ARSetup.Current;

			PrintParameters parameters = Filter.Current;

			if (parameters == null) yield break;

			ARStatementCycle statementCycle = StatementCycleRepository.FindByID(parameters.StatementCycleId);

			if (statementCycle == null) yield break;

			List<DetailsResult> detailsList = new List<DetailsResult>();

			Company company = PXSelect<Company>.Select(this);

			foreach (PXResult<ARStatement, Customer> result in PXSelectJoin<
				ARStatement,
					InnerJoin<Customer,
						On<Customer.bAccountID, Equal<ARStatement.statementCustomerID>>>,
				Where<
					ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
					And<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>>>,
				OrderBy<
					Asc<ARStatement.statementCustomerID,
					Asc<ARStatement.curyID>>>>
				.Select(this, parameters.StatementDate, parameters.StatementCycleId))
			{
				ARStatement statement = result;
				Customer customer = result;

				DetailsResult detail = CreateDetailsResult(statement, customer);

				bool skipStatement = false;

				skipStatement = skipStatement || (
					setup.ConsolidatedStatement != true
					&& statement.BranchID != parameters.BranchID);

				skipStatement = skipStatement || (
					Filter.Current.Action == PrintParameters.Actions.Print
					&& parameters.ShowAll != true
					&& (statement.DontPrint == true || statement.Printed == true));

				skipStatement = skipStatement || (
					(Filter.Current.Action == PrintParameters.Actions.Email || Filter.Current.Action == PrintParameters.Actions.MarkDontEmail)
					&& parameters.ShowAll != true
					&& (statement.DontEmail == true || statement.Emailed == true));

				skipStatement = skipStatement || (
					customer.PrintCuryStatements == true
					&& Filter.Current.CuryStatements != true);

				skipStatement = skipStatement || (
					customer.PrintCuryStatements != true
					&& Filter.Current.CuryStatements == true);

				if (skipStatement) continue;

				if (customer.PrintCuryStatements == true)
				{
					DetailsResult lastDetail = detailsList.LastOrDefault();

					if (lastDetail?.CustomerID == detail.CustomerID && lastDetail?.CuryID == detail.CuryID)
					{
						AggregateDetailsResult(lastDetail, detail);
					}
					else
					{
						detailsList.Add(detail);
					}
				}
				else
				{
					ResetDetailsResultToBaseCurrency(detail, company.BaseCuryID);

					DetailsResult lastDetail = detailsList.LastOrDefault();

					if (lastDetail?.CustomerID == detail.CustomerID)
					{
						AggregateDetailsResult(lastDetail, detail);
					}
					else
					{
						detailsList.Add(detail);
					}
				}
			}

			foreach (DetailsResult detail in detailsList)
			{
				var located = Details.Cache.Locate(detail);

				if (located != null)
				{
					yield return located;
				}
				else
				{
					Details.Cache.SetStatus(detail, PXEntryStatus.Held);
					yield return detail;
				}
			}

			Details.Cache.IsDirty = false;
		}

		[Obsolete("This view is not used anymore and will be removed in Acumatica ERP 8.0. Please use " + nameof(defaultCompanyContact) + " instead.")]
		public PXSelectJoin<
			Contact, 
				InnerJoin<Customer, On<Contact.contactID, Equal<Customer.defBillContactID>>>, 
			Where<
				Customer.bAccountID, Equal<Current<ARStatement.customerID>>>> 
			contact;

		[PXViewName(CR.Messages.MainContact)]
		public PXSelect<Contact> DefaultCompanyContact;
		protected virtual IEnumerable defaultCompanyContact()
		{
			return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
		}
		#endregion
		#endregion

		public override bool IsDirty => false;

        #region Filter Events
        protected virtual void PrintParameters_Action_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetStatementDateFromCycle((PrintParameters)e.Row);
		}

		protected virtual void PrintParameters_StatementCycleId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetStatementDateFromCycle((PrintParameters)e.Row);
		}

		private void SetStatementDateFromCycle(PrintParameters filter)
		{
			if (string.IsNullOrEmpty(filter.StatementCycleId)) return;

			ARStatementCycle statementCycle = StatementCycleRepository.FindByID(filter.StatementCycleId);

			if (statementCycle != null)
			{
				filter.StatementDate = statementCycle.LastStmtDate;
			}
		}

		protected virtual void PrintParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARSetup setup = ARSetup.Current;

			bool isStatementByBranch = setup.ConsolidatedStatement != true;

			PrintParameters row = (PrintParameters)e.Row;

			if (row == null)
			{
				return;
			}

			row.BranchCD = null;

			if (!isStatementByBranch)
			{
				// Force null for consolidated statements.
				// -
				row.BranchID = null;
			}

			if (row.BranchID != null)
			{
				Branch branch = PXSelect<
					Branch,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
					.Select(this, row.BranchID);

				if (branch != null)
				{
					row.BranchCD = branch.BranchCD;
				}
			}

			PXUIFieldAttribute.SetVisible<PrintParameters.branchID>(sender, null, isStatementByBranch);
			PXUIFieldAttribute.SetEnabled<PrintParameters.statementDate>(sender, row, row.Action != PrintParameters.Actions.Regenerate);

			PrintParameters filter = Filter.Cache.CreateCopy(row) as PrintParameters;

			switch (row.Action)
			{
				case PrintParameters.Actions.Print:
					Details.SetProcessDelegate(list => PrintStatements(filter, list));
					break;
				case PrintParameters.Actions.Email:
					Details.SetProcessDelegate(list => EmailStatements(filter, list, false));
					break;
				case PrintParameters.Actions.MarkDontEmail:
					Details.SetProcessDelegate(list => EmailStatements(filter, list, true));
					break;
				case PrintParameters.Actions.MarkDontPrint:
					Details.SetProcessDelegate(list => MarkAsDoNotPrint(filter, list));
					break;
				case PrintParameters.Actions.Regenerate:
					Details.SetProcessDelegate(list => RegenerateStatements(filter, list));
					break;
				default:
					throw new PXException(Common.Messages.IncorrectActionSpecified);
			}

			if (row != null && row.PrinterID == null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>())
			{
				row.PrinterID = new NotificationUtility(this).SearchPrinter(
					ARNotificationSource.Customer,
					(Filter.Current.CuryStatements == true ? ARStatementReportParams.CS_CuryStatementReportID : ARStatementReportParams.CS_StatementReportID), Accessinfo.BranchID);
			}
			bool showPrintSettings = IsPrintingAllowed(row);

			PXUIFieldAttribute.SetVisible<PrintParameters.printWithDeviceHub>(sender, row, showPrintSettings);
			PXUIFieldAttribute.SetVisible<PrintParameters.definePrinterManually>(sender, row, showPrintSettings);
			PXUIFieldAttribute.SetVisible<PrintParameters.printerID>(sender, row, showPrintSettings);
			PXUIFieldAttribute.SetVisible<PrintParameters.numberOfCopies>(sender, row, showPrintSettings);

			PXUIFieldAttribute.SetEnabled<PrintParameters.definePrinterManually>(sender, row, row.PrintWithDeviceHub == true);
			PXUIFieldAttribute.SetEnabled<PrintParameters.numberOfCopies>(sender, row, row.PrintWithDeviceHub == true);
			PXUIFieldAttribute.SetEnabled<PrintParameters.printerID>(sender, row, row.PrintWithDeviceHub == true && row.DefinePrinterManually == true);

			if (row.PrintWithDeviceHub != true || row.DefinePrinterManually != true)
			{
				row.PrinterID = null;
			}
		}

		protected virtual bool IsPrintingAllowed(PrintParameters row)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
				(row != null && row.Action == PrintParameters.Actions.Print);
		}

		protected virtual void PrintParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Details.Cache.Clear();

			if ((!sender.ObjectsEqual<PrintParameters.action>(e.Row, e.OldRow) || !sender.ObjectsEqual<PrintParameters.definePrinterManually>(e.Row, e.OldRow) || !sender.ObjectsEqual<PrintParameters.printWithDeviceHub>(e.Row, e.OldRow)) 
				&& Filter.Current != null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true)
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(
					ARNotificationSource.Customer,
					(Filter.Current.CuryStatements == true ? ARStatementReportParams.CS_CuryStatementReportID : ARStatementReportParams.CS_StatementReportID), Accessinfo.BranchID);
			}
		}

		protected virtual void DetailsResult_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PrintParameters_PrinterName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PrintParameters row = (PrintParameters)e.Row;
			if (row != null)
			{
				if (!IsPrintingAllowed(row))
					e.NewValue = null;
			}
		}

		#endregion

		#region Sub-screen Navigation Button
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.InquiriesFolder, VisibleOnProcessingResults = true)]
		protected virtual IEnumerable inquiriesFolder(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.CustomerStatementHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewDetails(PXAdapter adapter)
		{
			if (this.Details.Current != null && this.Filter.Current != null)
			{
				DetailsResult res = this.Details.Current;
				ARStatementForCustomer graph = PXGraph.CreateInstance<ARStatementForCustomer>();

				ARStatementForCustomer.ARStatementForCustomerParameters filter = graph.Filter.Current;
				filter.CustomerID = res.CustomerID;
				graph.Filter.Update(filter);
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, Messages.CustomerStatementHistory);
			}
			return Filter.Select();
		}
		#endregion

		#region Utility Functions

		protected static void Export(Dictionary<string, string> aRes, PrintParameters aSrc)
		{
			aRes[ARStatementReportParams.Fields.StatementCycleID] = aSrc.StatementCycleId;
			aRes[ARStatementReportParams.Fields.StatementDate] = aSrc.StatementDate.Value.ToString("d", CultureInfo.InvariantCulture);
		}

		protected static void Export(Dictionary<string, string> aRes, int index, PrintParameters aSrc)
		{
			aRes[ARStatementReportParams.Fields.StatementCycleID + index] = aSrc.StatementCycleId;
			aRes[ARStatementReportParams.Fields.StatementDate + index] = aSrc.StatementDate.Value.ToString("d", CultureInfo.InvariantCulture);
		}

		protected virtual DetailsResult CreateDetailsResult(ARStatement statement, Customer customer) => new DetailsResult
		{
			CustomerID = customer.BAccountID,
			UseCurrency = customer.PrintCuryStatements,
			StatementBalance = statement.EndBalance ?? decimal.Zero,
			AgeBalance00 = statement.AgeBalance00 ?? decimal.Zero,
			CuryID = statement.CuryID,
			CuryStatementBalance = statement.CuryEndBalance ?? decimal.Zero,
			CuryAgeBalance00 = statement.CuryAgeBalance00 ?? decimal.Zero,
			DontEmail = statement.DontEmail,
			DontPrint = statement.DontPrint,
			Emailed = statement.Emailed,
			Printed = statement.Printed,
			BranchID = statement.BranchID
		};

		protected virtual void AggregateDetailsResult(DetailsResult destination, DetailsResult source)
		{
			destination.StatementBalance += source.StatementBalance;
			destination.AgeBalance00 += source.AgeBalance00;

			if (destination.CuryID == source.CuryID)
			{
				destination.CuryStatementBalance += source.CuryStatementBalance;
				destination.CuryAgeBalance00 += source.CuryAgeBalance00;
			}
			else
			{
				destination.CuryStatementBalance = decimal.Zero;
				destination.CuryAgeBalance00 = decimal.Zero;
			}
		}

		protected virtual void ResetDetailsResultToBaseCurrency(DetailsResult detailsResult, string baseCurrencyID)
		{
			detailsResult.CuryID = baseCurrencyID;
			detailsResult.CuryStatementBalance = detailsResult.StatementBalance;
			detailsResult.CuryAgeBalance00 = detailsResult.AgeBalance00;
		}

		#endregion

		#region Process Delegates
		public static void MarkAsDoNotPrint(PrintParameters filter, IEnumerable<DetailsResult> list)
		{
			PXGraph graph = new PXGraph();
			PXView selectStatementsView = GetSelectStatementsForPrintingView(graph, filter);

			graph.Views["_ARStatement_"] = selectStatementsView;
			graph.Views.Caches.Add(typeof(ARStatement));

			ARSetup arSetup = PXSelect<ARSetup>.Select(graph);

			foreach (DetailsResult detailsResult in list)
			{
				if (filter.ShowAll != true)
				{
					foreach (ARStatement statement in selectStatementsView.SelectMulti(
						filter.StatementCycleId, 
						filter.StatementDate, 
						detailsResult.CustomerID, 
						detailsResult.CuryID))
					{
						if (arSetup?.ConsolidatedStatement != true
							&& statement.BranchID != filter.BranchID)
						{
							continue;
						}

						statement.DontPrint = true;
						selectStatementsView.Cache.Update(statement);
					}
				}
			}

			graph.Actions.PressSave();
		}

		public static void PrintStatements(PrintParameters filter, IEnumerable<DetailsResult> list)
		{
			PXGraph graph = new PXGraph();

			ARStatementCycle statementCycle = 
				new ARStatementCycleRepository(graph).FindByID(filter.StatementCycleId);

			PXView selectStatementsView = GetSelectStatementsForPrintingView(graph, filter);

			graph.Views["_ARStatement_"] = selectStatementsView;
			graph.Views.Caches.Add(typeof(ARStatement));

			ARSetup arSetup = PXSetup<ARSetup>.Select(graph);

			string reportID = filter.CuryStatements == true
				? ARStatementReportParams.CS_CuryStatementReportID
				: ARStatementReportParams.CS_StatementReportID;

			PXReportRequiredException reportRequiredException = null;
			Dictionary<PX.SM.PrintSettings, PXReportRequiredException> reportsToPrint = new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

			foreach (DetailsResult detailsResult in list)
			{
				Dictionary<string, string> parametersDictionary = new Dictionary<string, string>();

				parametersDictionary[ARStatementReportParams.Parameters.BranchID] = filter.BranchCD;
				parametersDictionary[ARStatementReportParams.Parameters.StatementMessage] = filter.StatementMessage;
				parametersDictionary[ARStatementReportParams.Fields.StatementCycleID] = filter.StatementCycleId;
				parametersDictionary[ARStatementReportParams.Fields.StatementDate] = filter.StatementDate.Value.ToString("d", CultureInfo.InvariantCulture);
				parametersDictionary[ARStatementReportParams.Fields.CustomerID] = detailsResult.CustomerID.ToString();

				if (filter.ShowAll != true)
				{
					parametersDictionary[ARStatementReportParams.Fields.PrintStatements] = ARStatementReportParams.BoolValues.True;
				}

				if (filter.CuryStatements == true)
				{
					parametersDictionary[ARStatementReportParams.Fields.CuryID] = detailsResult.CuryID;
				}
				
				foreach (ARStatement statement in selectStatementsView.SelectMulti(
					filter.StatementCycleId,
					filter.StatementDate,
					detailsResult.CustomerID,
					detailsResult.CuryID))
				{
					if (arSetup?.ConsolidatedStatement != true
						&& statement.BranchID != filter.BranchID)
					{
						continue;
					}

					if (statement.Printed != true)
					{
						statement.Printed = true;
						selectStatementsView.Cache.Update(statement);
					}
				}

				string actualReportID = GetCustomerReportID(graph, reportID, detailsResult);

				reportRequiredException = PXReportRequiredException.CombineReport(
					reportRequiredException,
					actualReportID,
					parametersDictionary);

				//Workaround for PDF printing from screen-based API
				if (PXAccess.FeatureInstalled<FeaturesSet.deviceHub>())
				{
					AR.Customer customer =
						PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(graph, detailsResult.CustomerID);
					if (customer != null)
					{
						parametersDictionary[ARStatementReportParams.Fields.CustomerID] = parametersDictionary[ARStatementReportParams.Fields.CustomerID] + PX.SM.SMPrintJobMaint.DeviceHubValueIdentifier + customer.AcctCD.ToString();
					}
				}

				reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, parametersDictionary, filter, new NotificationUtility(graph).SearchPrinter, ARNotificationSource.Customer, reportID, actualReportID, detailsResult.BranchID);
			}

			graph.Actions.PressSave();

			if (reportRequiredException != null)
			{
				PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint);

				throw reportRequiredException;
			}
		}

		private static PXView GetSelectStatementsForPrintingView(PXGraph viewGraph, PrintParameters filter)
		{
			Type selectStatementsType = filter.CuryStatements == true
				? typeof(Select<
					ARStatement,
					Where<
						ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
						And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
						And<ARStatement.statementCustomerID, Equal<Required<ARStatement.customerID>>,
						And<ARStatement.curyID, Equal<Required<ARStatement.curyID>>>>>>>)
				: typeof(Select<
					ARStatement,
					Where<
						ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
						And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
						And<ARStatement.statementCustomerID, Equal<Required<ARStatement.customerID>>>>>>);

			return new PXView(viewGraph, false, BqlCommand.CreateInstance(selectStatementsType));
		}

		[Obsolete("This method is not used anymore and will be removed in Acumatica 8.0.")]
		public static void PrintStatements(PrintParameters filter, List<DetailsResult> list, bool markOnly)
		{
			if (markOnly)
			{
				MarkAsDoNotPrint(filter, list);
			}
			else
			{
				PrintStatements(filter, list);
			}
		}

		private static string GetCustomerReportID(PXGraph graph, string reportID, DetailsResult statement)
		{
            return GetCustomerReportID(graph, reportID, statement.CustomerID, statement.BranchID);
		}

        public static string GetCustomerReportID(PXGraph graph, string reportID, int? customerID, int? branchID)
        {
            Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.SelectWindowed(graph, 0, 1, customerID);
            return new NotificationUtility(graph).SearchReport(ARNotificationSource.Customer, customer, reportID, branchID);
        }

        public static void EmailStatements(PrintParameters filter, List<DetailsResult> list, bool markOnly) 
        {
            ARStatementUpdate graph = CreateInstance<ARStatementUpdate>();

			int elementIndex = 0;
            bool anyFailed = false;

			foreach (DetailsResult detail in list) 
            {                
                try
                {
                    graph.EMailStatement(
						filter.BranchID,
						filter.BranchCD, 
						detail.CustomerID, 
						filter.StatementDate, 
						filter.CuryStatements == true? detail.CuryID : null, 
						filter.StatementMessage,
						markOnly, 
						filter.ShowAll == true);

					if (!markOnly)
					{
						detail.Emailed = true;
					}

                    PXFilteredProcessing<DetailsResult, PrintParameters>.SetCurrentItem(detail);
                    PXFilteredProcessing<DetailsResult, PrintParameters>.SetProcessed();
                }
                catch (Exception e) 
                {
                    PXFilteredProcessing<DetailsResult, PrintParameters>.SetError(elementIndex, e);
                    anyFailed = true;
                }

				++elementIndex;
            }

			if (anyFailed)
			{
				throw new PXException(ErrorMessages.MailSendFailed);
			}
		}

		private static void RegenerateStatements(PrintParameters pars, List<DetailsResult> statements)
		{
			var process = PXGraph.CreateInstance<StatementCycleProcessBO>();
			var cycle = process.CyclesList.SelectSingle(pars.StatementCycleId);
			var customerSelect = new PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<DetailsResult.customerID>>>>(process);
			
			int detailIndex = 0;
			List<Customer> customers = new List<Customer>();
			
			foreach (DetailsResult res in statements)
			{
				if (ARStatementProcess.CheckForUnprocessedPPD(process, pars.StatementCycleId, pars.StatementDate, res.CustomerID))
				{
					PXFilteredProcessing<DetailsResult, PrintParameters>.SetError(
						detailIndex, 
						new PXSetPropertyException(Messages.UnprocessedPPDExists, PXErrorLevel.RowError));
				}
				else
				{
					Customer customer = customerSelect.SelectSingle(res.CustomerID);

					if (customer != null)
					{
						customers.Add(customer);
					}
				}

				detailIndex++;
			}

			if (cycle == null || !customers.Any()) return;

			StatementCycleProcessBO.RegenerateStatements(process, cycle, customers);
		}

		#endregion

	}

    public class ARStatementUpdate : PXGraph<ARStatementUpdate,Customer> 
    {
        public PXSelect<Customer, Where<Customer.bAccountID, Equal<Optional<ARStatement.customerID>>>> Customer;
        [PXViewName(Messages.ARContact)]
        public PXSelectJoin<Contact, InnerJoin<Customer, On<Contact.contactID, Equal<Customer.defBillContactID>>>, Where<Customer.bAccountID, Equal<Current<ARStatement.customerID>>>> contact;

		public PXSelect<
			ARStatement, 
			Where<
				ARStatement.statementCustomerID, Equal<Optional<Customer.bAccountID>>,
				And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,														
				And<Where<
					ARStatement.curyID, Equal<Required<ARStatement.curyID>>,
					Or<Required<ARStatement.curyID>, IsNull>>>>>> 
			StatementMC;

		[Obsolete("This view is not used anymore and will be removed in Acumatica 8.0")]
		public PXSelect<
			ARStatement, 
			Where<
				ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
				And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
				And<ARStatement.curyID, Equal<Required<ARStatement.curyID>>>>>> 
			Statement;

		public ARStatementCycleRepository StatementCycleRepository; 

        [CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARStatement.customerID>>>>))]
		[CRDefaultMailTo(typeof(Select<Contact, Where<Contact.contactID, Equal<Current<Customer.defBillContactID>>>>))]
        public CRActivityList<ARStatement>
            Activity;

		public PXSetup<ARSetup> ARSetup;

		public ARStatementUpdate()
		{
			StatementCycleRepository = new ARStatementCycleRepository(this);
		}

		[Obsolete("This method is not used anymore and will be removed in Acumatica 8.0. Use other method overloads.")]
		public virtual void EMailStatement(string branchCD, int? customerID, DateTime? statementDate, string currency, string statementMessage, bool markOnly, bool showAll)
		{
			Branch branch = PXSelect<Branch, Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.Select(this, branchCD);
			EMailStatement(branch.BranchID, branchCD, customerID, statementDate, currency, null, markOnly, showAll);
		}

        public virtual void EMailStatement(
			int? branchID, 
			string branchCD, 
			int? customerID, 
			DateTime? statementDate, 
			string currency, 
			string statementMessage,
			bool markOnly, 
			bool showAll) 
        {
            Customer customer = Customer.Search<Customer.bAccountID>(customerID, customerID);
			Customer.Current = customer;

			ARSetup arSetup = ARSetup.Current;

			ARStatementCycle statementCycle = StatementCycleRepository.FindByID(customer.StatementCycleId);

			bool useCurrency = !string.IsNullOrEmpty(currency);

			ICollection<ARStatementKey> sentStatements = new HashSet<ARStatementKey>();

			foreach (ARStatement statement in StatementMC
				.Select(customerID, statementDate, currency, currency)
				.RowCast<ARStatement>()
				.Where(statement => statement.BranchID == branchID || arSetup.ConsolidatedStatement == true))
            {
                if (markOnly)
                {
                    statement.DontEmail = true;
                    StatementMC.Cache.Update(statement);
                }
				else 
				{
					if (statement.IsParentCustomerStatement)
					{
						Dictionary<string, string> parameters = new Dictionary<string, string>();

						parameters[ARStatementReportParams.Parameters.BranchID] = branchCD;

						if (!showAll)
						{
							parameters[ARStatementReportParams.Fields.SendStatementsByEmail] = ARStatementReportParams.BoolValues.True;
						}

						if (useCurrency)
						{
							parameters[ARStatementReportParams.Fields.CuryID] = statement.CuryID;
						}

						if (statementMessage != null)
						{
							parameters[ARStatementReportParams.Parameters.StatementMessage] = statementMessage;
						}

						StatementMC.Current = statement;

						ARStatementKey statementKey = new ARStatementKey(
							arSetup.ConsolidatedStatement == true ? int.MaxValue : statement.BranchID.Value,
							useCurrency ? statement.CuryID : string.Empty,
							statement.StatementCustomerID.Value,
							statement.StatementDate.Value);

						if (!sentStatements.Contains(statementKey))
						{
							parameters[ARStatementReportParams.Fields.StatementCycleID] = statement.StatementCycleId;
							parameters[ARStatementReportParams.Fields.StatementDate] = statement.StatementDate.Value.ToString("d", CultureInfo.InvariantCulture);
							parameters[ARStatementReportParams.Fields.CustomerID] = statement.CustomerID.ToString();

							string notificationCD = (useCurrency ? "STATEMENTMC" : "STATEMENT");
							Activity.SendNotification(ARNotificationSource.Customer, notificationCD, statement.BranchID, parameters);

							sentStatements.Add(statementKey);
						}
					}

                    statement.Emailed = true;
                    StatementMC.Cache.Update(statement);
                }
            }

			Save.Press();
        }

		[Obsolete("This method is not used anymore and will be removed in Acumatica ERP 8.0.")]
        protected static void Export(Dictionary<string, string> aRes, ARStatement aSrc)
        { }
    }

	public static class ARStatementReportParams
	{
		public static class Fields
		{
			public static readonly string BranchID = $"{nameof(ARStatement)}.{nameof(ARStatement.BranchID)}";
			public static readonly string StatementCycleID = $"{nameof(ARStatement)}.{nameof(ARStatement.StatementCycleId)}";
			public static readonly string StatementDate = $"{nameof(ARStatement)}.{nameof(ARStatement.StatementDate)}";
			public static readonly string CustomerID = $"{nameof(ARStatement)}.{nameof(ARStatement.StatementCustomerID)}";
			public static readonly string CuryID = $"{nameof(ARStatement)}.{nameof(ARStatement.CuryID)}";
			public static readonly string SendStatementsByEmail = $"{nameof(Customer)}.{nameof(Customer.SendStatementByEmail)}";
			public static readonly string PrintStatements = $"{nameof(Customer)}.{nameof(Customer.PrintStatements)}";			
		}

		public static class Parameters
		{
            public const string BranchID = "BranchID";
            public const string StatementCycleID = "StatementCycleId";
			public const string StatementDate = "StatementDate";
			public const string CustomerID = "StatementCustomerId";
			[Obsolete("This constant is not used anymore and will be removed in Acumatica ERP 8.0.")]
			public const string SendByEmail = "SendByEmail";
			[Obsolete("This constant is not used anymore and will be removed in Acumatica ERP 8.0.")]
			public const string PrintOnPaper = "PrintOnPaper";
			[Obsolete("This constant is not used anymore and will be removed in Acumatica ERP 8.0.")]
			public const string IgnorePrintFlags = "IgnorePrintFlags";
			public const string StatementMessage = "StatementMessage";
		}

		public static class BoolValues
		{
			public const string True = "true";
			public const string False = "false";
		}

		public const string CS_StatementReportID = "AR641500";
		public const string CS_CuryStatementReportID = "AR642000";

		[Obsolete("This method is not used anymore and will be removed in Acumatica ERP 8.0")]
		public static void Export(Dictionary<string, string> aRes, Customer aAcct)
		{ }

		[Obsolete("This method is not used anymore and will be removed in Acumatica ERP 8.0")]
		public static void Export(Dictionary<string, string> aRes, int index, Customer aAcct)
		{ }

        public static Dictionary<string, string> FromCustomer(Customer customer)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters[Parameters.CustomerID] = customer.AcctCD;
            parameters[Parameters.StatementCycleID] = customer.StatementCycleId;
			
            return parameters;
        }

        public static string ReportIDForCustomer(PXGraph graph, Customer customer, int? branchID)
        {
            string reportID = customer.PrintCuryStatements == true ? CS_CuryStatementReportID : CS_StatementReportID;
            return ARStatementPrint.GetCustomerReportID(graph, reportID, customer.BAccountID, branchID);
		}
	}

}
