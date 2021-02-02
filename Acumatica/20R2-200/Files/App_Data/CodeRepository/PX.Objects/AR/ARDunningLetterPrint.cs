using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR.MigrationMode;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using Branch = PX.Objects.GL.Branch;

namespace PX.Objects.AR
{
    public class ARDunningLetterPrint : PXGraph<ARDunningLetterPrint>
    {
        #region Internal types definitions
		protected class ActionTypes
		{
			public const int Print = 0;
			public const int Email = 1;
			public const int MarkDontEmail = 2;
			public const int MarkDontPrint = 3;
			public const int Release = 4;
			public class ListAttribute : PXIntListAttribute
			{
				public ListAttribute() :
					base(new int[] { Print, Email, MarkDontEmail, MarkDontPrint, Release },
						new string[] { Messages.ProcessPrintDL, Messages.ProcessEmailDL, Messages.ProcessMarkDontEmail, Messages.ProcessMarkDontPrint, Messages.ProcessReleaseDunningLetter })
				{ }
			}
		}
		[Serializable]
        public partial class PrintParameters : IBqlTable, PX.SM.IPrintable
		{
            #region Action
			public abstract class action : PX.Data.BQL.BqlInt.Field<action> { }
            [PXDBInt]
			[PXDefault(ActionTypes.Release)]
            [PXUIField(DisplayName = "Action")]
			[ActionTypes.List]
			public virtual int? Action
                {
				get;
				set;
            }
            #endregion

            #region BeginDate
			public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }
            [PXDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? BeginDate
            {
				get;
				set;
            }
            #endregion
            #region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
            [PXDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? EndDate
            {
				get;
				set;
            }
            #endregion

            #region ShowAll
            public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Show All", Enabled = false)]
            public virtual bool? ShowAll
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
		}

		[Serializable]
        public partial class DetailsResult : IBqlTable
        {
            #region Selected
            public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
            [PXBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Selected")]
            public virtual bool? Selected
            {
				get;
				set;
            }
            #endregion

            #region DunningLetterID
			public abstract class dunningLetterID : PX.Data.BQL.BqlInt.Field<dunningLetterID> { }
            [PXDBInt(IsKey = true)]
            [PXUIField(Enabled = false)]
			public virtual int? DunningLetterID
                {
				get;
				set;
            }
            #endregion

            #region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			[PXDefault]
            [Branch(DescriptionField = typeof(PX.Objects.GL.Branch.branchID))]
            [PXUIField(DisplayName = "Branch")]
			public virtual int? BranchID
            {
				get;
				set;
            }
            #endregion
            #region CustomerId
			public abstract class customerId : PX.Data.BQL.BqlInt.Field<customerId> { }
            [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
            [Customer(DescriptionField = typeof(Customer.acctName))]
			public virtual int? CustomerId
            {
				get;
				set;
            }
            #endregion

            #region DunningLetterDate
			public abstract class dunningLetterDate : PX.Data.BQL.BqlDateTime.Field<dunningLetterDate> { }
			[PXDBDate]
            [PXDefault(TypeCode.DateTime, "01/01/1900")]
            [PXUIField(DisplayName = "Dunning Letter Date")]
            public virtual DateTime? DunningLetterDate
            {
				get;
				set;
            }
            #endregion

            #region DunningLetterLevel
			public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
			[PXDBInt]
			[PXDefault]
            [PXUIField(DisplayName = Messages.DunningLetterLevel)]
			public virtual int? DunningLetterLevel
                {
				get;
				set;
            }
            #endregion

            #region DocBal
			public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
			[PXDBBaseCury]
            [PXUIField(DisplayName = "Overdue Balance")]
			public virtual decimal? DocBal
                {
				get;
				set;
            }
            #endregion
            #region LastLevel
			public abstract class lastLevel : PX.Data.BQL.BqlBool.Field<lastLevel> { }
			[PXDBBool]
            [PXUIField(DisplayName = "Final Reminder")]
			public virtual bool? LastLevel
            {
				get;
				set;
            }
            #endregion

            #region DontPrint
			public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
			[PXDBBool]
            [PXDefault(true)]
            [PXUIField(DisplayName = "Don't Print")]
			public virtual bool? DontPrint
                {
				get;
				set;
            }
            #endregion
            #region Printed
			public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
			[PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Printed")]
			public virtual bool? Printed
                {
				get;
				set;
            }
            #endregion
            #region DontEmail
			public abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
			[PXDBBool]
            [PXDefault(true)]
            [PXUIField(DisplayName = "Don't Email")]
			public virtual bool? DontEmail
            {
				get;
				set;
            }
            #endregion
            #region Emailed
			public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }
			[PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Emailed")]
			public virtual bool? Emailed
                {
				get;
				set;
            }
            #endregion

        }
        #endregion

        #region Public members

        public ARDunningLetterPrint()
        {
            ARSetup setup = ARSetup.Current;
            Consolidated = setup.ConsolidatedDunningLetter.GetValueOrDefault(false);
            if (setup.AutoReleaseDunningLetter == true)
            {
                PXDefaultAttribute.SetDefault<PrintParameters.action>(Filter.Cache, ActionTypes.Print);
                PXUIFieldAttribute.SetEnabled<PrintParameters.showAll>(Filter.Cache, null, true);
            }

            Details.Cache.AllowDelete = false;
            Details.Cache.AllowInsert = false;
            Details.SetSelected<ARInvoice.selected>();
            Details.SetProcessCaption(IN.Messages.Process);
            Details.SetProcessAllCaption(IN.Messages.ProcessAll);
        }

        public bool Consolidated = false;

        public PXCancel<PrintParameters> Cancel;

        public PXFilter<PrintParameters> Filter;

		//For notification templates:
		[PXViewName(Messages.DunningLetter)]
		public PXSelect<ARDunningLetter> docs;

        [PXFilterable]
        public PXFilteredProcessing<DetailsResult, PrintParameters> Details;

        public ARSetupNoMigrationMode ARSetup;

		[PXViewName(Messages.ARContact)]
		public PXSelectJoin<Contact,
			InnerJoin<Customer,
				On<Contact.contactID, Equal<Customer.defBillContactID>>>,
			Where<Customer.bAccountID, Equal<Current<ARDunningLetter.bAccountID>>>> contact;
        #endregion
        #region Actions
        public PXAction<PrintParameters> ViewDocument;
        [PXUIField(DisplayName = Messages.ViewDunningLetter, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnProcessingResults = true)]
        public virtual IEnumerable viewDocument(PXAdapter adapter)
        {
            if (Details.Current != null)
            {
				ARDunningLetter doc = PXSelect<ARDunningLetter, 
					Where<ARDunningLetter.dunningLetterID, Equal<Required<DetailsResult.dunningLetterID>>>>.Select(this, Details.Current.DunningLetterID);
                if (doc != null)
                {
                    ARDunningLetterMaint graph = PXGraph.CreateInstance<ARDunningLetterMaint>();
                    graph.Document.Current = doc;
                    PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
                }
            }
            return adapter.Get();
        }
        #endregion
        #region Delegates

        /// <summary>
        /// Generates a list of documents that meet the filter criteria.
        /// This list is used for display in the processing screen
        /// </summary>
        /// <returns>List of Dunning Letters</returns>
        protected virtual IEnumerable details()
        {
            Details.Cache.Clear();
            PrintParameters header = Filter.Current;
            if (header == null)
			{
                yield break;
			}
			List<DetailsResult> results = new List<DetailsResult>();

			PXSelectBase<ARDunningLetter> cmd = new PXSelectJoinGroupBy<ARDunningLetter, 
				InnerJoin<ARDunningLetterDetail, 
					On<ARDunningLetterDetail.dunningLetterID, Equal<ARDunningLetter.dunningLetterID>>>,
                    Where<ARDunningLetter.dunningLetterDate,
                    Between<Required<ARDunningLetter.dunningLetterDate>, Required<ARDunningLetter.dunningLetterDate>>,
                        And<ARDunningLetter.consolidated, Equal<Required<ARDunningLetter.consolidated>>>>,
				Aggregate<GroupBy<ARDunningLetter.dunningLetterID, Sum<ARDunningLetterDetail.overdueBal>>>,
                    OrderBy<Asc<ARDunningLetter.bAccountID>>>(this);
			if (Filter.Current.Action == ActionTypes.Release)
            {
                cmd.WhereAnd<Where<ARDunningLetter.released, Equal<False>, And<ARDunningLetter.voided, Equal<False>>>>();
            }
            else
            {
                cmd.WhereAnd<Where<ARDunningLetter.released, Equal<True>, And<ARDunningLetter.voided, Equal<False>>>>();
            }
			foreach (PXResult<ARDunningLetter, ARDunningLetterDetail> res in cmd.Select(header.BeginDate, header.EndDate, this.Consolidated))
            {
				ARDunningLetter dunningLetter = res;
				ARDunningLetterDetail detailSum = res;

				if (Filter.Current.Action == ActionTypes.Print
					&& header.ShowAll == false
					&& (dunningLetter.DontPrint == true || dunningLetter.Printed == true))
				{
                        continue;
				}

				if (Filter.Current.Action == ActionTypes.Email
					&& header.ShowAll == false
					&& (dunningLetter.DontEmail == true || dunningLetter.Emailed == true))
				{
                        continue;
				}

				if (Filter.Current.Action == ActionTypes.MarkDontEmail
					&& header.ShowAll == false
					&& (dunningLetter.DontEmail == true || dunningLetter.Emailed == true))
				{
                        continue;
				}

				DetailsResult row = new DetailsResult();
				row.BranchID = dunningLetter.BranchID;
				row.CustomerId = dunningLetter.BAccountID;
				row.DunningLetterID = dunningLetter.DunningLetterID;
				row.DunningLetterDate = dunningLetter.DunningLetterDate;
				row.DunningLetterLevel = dunningLetter.DunningLetterLevel;
				row.LastLevel = dunningLetter.LastLevel;
				row.DontEmail = dunningLetter.DontEmail;
				row.DontPrint = dunningLetter.DontPrint;
				row.Emailed = dunningLetter.Emailed;
				row.Printed = dunningLetter.Printed;
				row.DocBal = detailSum.OverdueBal;
				results.Add(row);
            }
			foreach (DetailsResult item in results)
            {
                Details.Cache.SetStatus(item, PXEntryStatus.Held);
                yield return item;
            }
            Details.Cache.IsDirty = false;
        }

        [PXViewName(CR.Messages.MainContact)]
        public PXSelect<Contact> DefaultCompanyContact;

        protected virtual IEnumerable defaultCompanyContact()
        {
	        return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
        }
        #endregion
        #region Filter Events

        protected virtual void PrintParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
			PrintParameters row = (PrintParameters)e.Row;
			if (row != null)
            {
				PrintParameters filter = (PrintParameters)this.Filter.Cache.CreateCopy(row);
				switch (row.Action)
                {
					case ActionTypes.Print:
                        Details.SetProcessDelegate(list => Print(filter, list, false));
                        break;
					case ActionTypes.Email:
                        Details.SetProcessDelegate(list => Email(filter, list, false));
                        break;
					case ActionTypes.MarkDontEmail:
                        Details.SetProcessDelegate(list => Email(filter, list, true));
                        break;
					case ActionTypes.MarkDontPrint:
                        Details.SetProcessDelegate(list => Print(filter, list, true));
                        break;
					case ActionTypes.Release:
                        Details.SetProcessDelegate(list => Release(filter, list));
                    row.ShowAll = false;
                        break;
                }
				PXUIFieldAttribute.SetEnabled<PrintParameters.showAll>(sender, row, row.Action != ActionTypes.Release);
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
        }

		protected virtual bool IsPrintingAllowed(PrintParameters row)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && (row != null && row.Action == ActionTypes.Print);
		}

		protected virtual void PrintParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if ((!sender.ObjectsEqual<PrintParameters.action>(e.Row, e.OldRow) || !sender.ObjectsEqual<PrintParameters.definePrinterManually>(e.Row, e.OldRow) || !sender.ObjectsEqual<PrintParameters.printWithDeviceHub>(e.Row, e.OldRow)) 
				&& Filter.Current != null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true)
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(
					ARNotificationSource.Customer, ARReports.DunningLetterReportID, Accessinfo.BranchID);
			}
		}

		protected virtual void DetailsResult_RowPersisting(PXCache sedner, PXRowPersistingEventArgs e)
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
        #region Process Delegates
        public static void Print(PrintParameters filter, List<DetailsResult> list, bool markOnly)
        {
			bool failed = false;
            ARDunningLetterUpdate graph = PXGraph.CreateInstance<ARDunningLetterUpdate>();
			PXReportRequiredException reportRedirectException = null;
			Dictionary<PX.SM.PrintSettings, PXReportRequiredException> reportsToPrint = new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

			foreach (DetailsResult res in list)
            {
				int? letterID = res.DunningLetterID;
				ARDunningLetter doc = graph.DL.Select(letterID.Value);
				PXFilteredProcessing<DetailsResult, PrintParameters>.SetCurrentItem(res);
				if (doc.Released == false || doc.Voided == true)
				{
					PXFilteredProcessing<DetailsResult, PrintParameters>.SetError(CA.Messages.DocumentStatusInvalid);
					failed = true;
					continue;
				}
                if (markOnly)
                {
                    if (filter.ShowAll != true)
                    {
                        doc.DontPrint = true;
                        graph.docs.Cache.Update(doc);
                        PXFilteredProcessing<DetailsResult, PrintParameters>.SetProcessed();
                    }
                }
                else
                {
					Dictionary<string, string> reportParameters = new Dictionary<string, string>();
					reportParameters["ARDunningLetter.DunningLetterID"] = letterID.ToString();

                    if (doc.Printed != true)
                    {
                        doc.Printed = true;
                        graph.docs.Cache.Update(doc);
                        PXFilteredProcessing<DetailsResult, PrintParameters>.SetProcessed();
                    }

					string actualReportID = GetCustomerReportID(graph, ARReports.DunningLetterReportID, res);

					reportRedirectException = PXReportRequiredException.CombineReport(reportRedirectException, actualReportID, reportParameters);

					reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, reportParameters, filter, new NotificationUtility(graph).SearchPrinter, ARNotificationSource.Customer, ARReports.DunningLetterReportID, actualReportID, doc.BranchID);
				}
            }

            graph.Save.Press();

			if (reportRedirectException != null)
			{
				PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint);

				throw reportRedirectException;
			}

			if (failed)
			{
				throw new PXException(Messages.OneOrMoreItemsAreNotProcessed);
        }
		}

        public static string GetCustomerReportID(PXGraph graph, string reportID, DetailsResult statement)
        {
			return GetCustomerReportID(graph, reportID, statement.CustomerId, statement.BranchID);
        }

		public static string GetCustomerReportID(PXGraph graph, string reportID, int? customerID, int? branchID)
		{
			Customer customer = PXSelect<Customer,
				Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.SelectWindowed(graph, 0, 1, customerID);
			return new NotificationUtility(graph).SearchReport(ARNotificationSource.Customer, customer, reportID, branchID);
		}

        public static void Email(PrintParameters filter, List<DetailsResult> list, bool markOnly)
        {
            ARDunningLetterUpdate graph = CreateInstance<ARDunningLetterUpdate>();
            int i = 0;
            bool failed = false;
            foreach (DetailsResult it in list)
            {
                try
                {
                    graph.EMailDL(it.DunningLetterID.Value, markOnly, filter.ShowAll == true);
                    PXFilteredProcessing<DetailsResult, PrintParameters>.SetCurrentItem(it);
                    PXFilteredProcessing<DetailsResult, PrintParameters>.SetProcessed();
                }
                catch (Exception e)
                {
                    PXFilteredProcessing<DetailsResult, PrintParameters>.SetError(i, e);
                    failed = true;
                }
                i++;
            }
            if (failed)
			{
                throw new PXException(ErrorMessages.MailSendFailed);
        }
		}

		public static void Release(PrintParameters filter, List<DetailsResult> list)
		{
			if (list.Count > 0)
			{
				bool failed = false;
				ARDunningLetterMaint graph = PXGraph.CreateInstance<ARDunningLetterMaint>();

				int i = 0;
				foreach (DetailsResult res in list)
				{
					try
					{
						ARDunningLetter doc = PXSelect<ARDunningLetter,
							Where<ARDunningLetter.dunningLetterID, Equal<Required<DetailsResult.dunningLetterID>>>>.Select(graph, res.DunningLetterID);
						ARDunningLetterMaint.ReleaseProcess(graph, doc);
						PXFilteredProcessing<DetailsResult, PrintParameters>.SetProcessed();
					}
					catch (Exception e)
					{
						failed = true;
						PXFilteredProcessing<DetailsResult, PrintParameters>.SetError(i, e);
					}

					i++;
				}

				if (failed)
				{
					throw new PXException(Messages.OneOrMoreItemsAreNotReleased);
				}
			}
		}
		#endregion
	}

    public class ARDunningLetterUpdate : PXGraph<ARDunningLetterUpdate, Customer>
    {
		public const string notificationCD = "DUNNINGLETTER";

		//For notification templates:
        [PXViewName(Messages.DunningLetter)]
        public PXSelect<ARDunningLetter> docs;

        public PXSelect<ARDunningLetter, 
                Where<ARDunningLetter.dunningLetterID, Equal<Required<ARDunningLetter.dunningLetterID>>>>   DL;

		public PXSelect<Customer,
			Where<Customer.bAccountID, Equal<Optional<ARDunningLetter.bAccountID>>>> Customer;

        [PXViewName(Messages.ARContact)]
		public PXSelectJoin<Contact,
			InnerJoin<Customer, On<Contact.contactID, Equal<Customer.defBillContactID>>>,
			Where<Customer.bAccountID, Equal<Current<ARDunningLetter.bAccountID>>>> contact;
            
        [CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARDunningLetter.bAccountID>>>>))]
        [CRDefaultMailTo(typeof(Select<ARContact, Where<ARContact.contactID, Equal<Current<Customer.defBillContactID>>>>))]
		public CRActivityList<ARDunningLetter> Activity;

		public virtual void EMailDL(int dunningLetterID, bool markOnly, bool showAll)
        {
			ARDunningLetter dunningLetter = DL.Select(dunningLetterID);
			if (dunningLetter.Released == false || dunningLetter.Voided == true)
			{
				throw new PXException(CA.Messages.DocumentStatusInvalid);
			}
			Customer customer = this.Customer.Select(dunningLetter.BAccountID);
            this.Customer.Current = customer;

			if (markOnly)
            {
				dunningLetter.DontEmail = true;
            }
            else
            {
				this.DL.Current = dunningLetter;
				Activity.SendNotification(ARNotificationSource.Customer, notificationCD, dunningLetter.BranchID, new Dictionary<string, string>() { { "DunningLetterID", dunningLetterID.ToString() } });
				dunningLetter.Emailed = true;
			}
			DL.Cache.Update(dunningLetter);
			this.Save.Press();
		}
    }
}