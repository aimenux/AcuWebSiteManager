using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.SM;
using static PX.Objects.Common.Extensions.CollectionExtensions;

namespace PX.Objects.AR
{
	public class ARDunningLetterProcess : PXGraph<ARDunningLetterProcess>
	{
		#region internal types definition
		[Serializable]
		[PXProjection(typeof(Select5<Standalone.ARInvoice,
					InnerJoin<ARRegister,
						On<Standalone.ARInvoice.docType, Equal<ARRegister.docType>,
						And<Standalone.ARInvoice.refNbr, Equal<ARRegister.refNbr>,
						And<ARRegister.released, Equal<True>,
						And<ARRegister.openDoc, Equal<True>,
						And<ARRegister.voided, Equal<False>,
						And<ARRegister.pendingPPD, NotEqual<True>,
						And<Where<ARRegister.docType, Equal<ARDocType.invoice>,
							Or<ARRegister.docType, Equal<ARDocType.finCharge>,
							Or<ARRegister.docType, Equal<ARDocType.debitMemo>>>>>>>>>>>,
					InnerJoin<Customer, 
						On<Customer.bAccountID, Equal<ARRegister.customerID>>,
					LeftJoin<ARDunningLetterDetail,
						On<ARDunningLetterDetail.dunningLetterBAccountID, Equal<Customer.sharedCreditCustomerID>,
						And<ARDunningLetterDetail.docType, Equal<ARRegister.docType>,
						And<ARDunningLetterDetail.refNbr, Equal<ARRegister.refNbr>,
						And<ARDunningLetterDetail.voided, Equal<False>>>>>,
					LeftJoin<ARDunningLetter,
						On<ARDunningLetter.dunningLetterID, Equal<ARDunningLetterDetail.dunningLetterID>,
						And<ARDunningLetter.voided, Equal<False>>>>>>>,
					Aggregate<
						GroupBy<ARRegister.refNbr,
						GroupBy<ARRegister.docType,
						Min<ARDunningLetterDetail.released>>>>>))]
		public partial class ARInvoiceWithDL : IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt(BqlField = typeof(ARRegister.customerID))]
			public virtual int? CustomerID
			{
				get;
				set;
			}
			#endregion
			#region SharedCreditCustomerID
			public abstract class sharedCreditCustomerID : PX.Data.BQL.BqlInt.Field<sharedCreditCustomerID> { }

			[PXDBInt(BqlField = typeof(Customer.sharedCreditCustomerID))]
			public virtual int? SharedCreditCustomerID
			{
				get;
				set;
			}
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			[PXDBInt(BqlField = typeof(ARRegister.branchID))]
			public virtual int? BranchID
			{
				get;
				set;
			}
			#endregion
			#region CustomerLocationID
			public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		
			[PXDBInt(BqlField = typeof(ARRegister.customerLocationID))]
			public virtual Int32? CustomerLocationID
			{
				get;
				set;
			}
			#endregion
			#region DocBal
			public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
			[PXDBDecimal(BqlField = typeof(ARRegister.docBal))]
			public virtual decimal? DocBal
			{
				get;
				set;
			}
			#endregion
			#region DueDate
			public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			[PXDBDate(BqlField = typeof(ARRegister.dueDate))]
			public virtual DateTime? DueDate
			{
				get;
				set;
			}
			#endregion
			#region Released
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			[PXDBBool(BqlField = typeof(ARRegister.released))]
			public virtual bool? Released
			{
				get;
				set;
			}
			#endregion
			#region OpenDoc
			public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
			[PXDBBool(BqlField = typeof(ARRegister.openDoc))]
			public virtual bool? OpenDoc
			{
				get;
				set;
			}
			#endregion
			#region Voided
			public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
			[PXDBBool(BqlField = typeof(ARRegister.voided))]
			public virtual bool? Voided
			{
				get;
				set;
			}
			#endregion
			#region Revoked
			public abstract class revoked : PX.Data.BQL.BqlBool.Field<revoked> { }
			[PXDBBool(BqlField = typeof(AR.Standalone.ARInvoice.revoked))]
			public virtual bool? Revoked
			{
				get;
				set;
			}
			#endregion
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
			public virtual string DocType
			{
				get;
				set;
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARRegister.refNbr))]
			public virtual string RefNbr
			{
				get;
				set;
			}
			#endregion
			#region DunningLetterLevel
			public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
			[PXDBInt(BqlField = typeof(ARDunningLetterDetail.dunningLetterLevel))]
			public virtual int? DunningLetterLevel
			{
				get;
				set;
			}
			#endregion
			#region DunningLetterDate
			public abstract class dunningLetterDate : PX.Data.BQL.BqlDateTime.Field<dunningLetterDate> { }
			[PXDBDate(BqlField = typeof(ARDunningLetter.dunningLetterDate))]
			public virtual DateTime? DunningLetterDate
			{
				get;
				set;
			}
			#endregion
			#region DocDate
			public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			[PXDBDate(BqlField = typeof(ARRegister.docDate))]
			public virtual DateTime? DocDate
			{
				get;
				set;
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			[PXDBString(5, IsUnicode = true, BqlField = typeof(ARRegister.curyID))]
			public virtual string CuryID
			{
				get;
				set;
			}
			#endregion
			#region CuryOrigDocAmt
			public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
			[PXDBDecimal(BqlField = typeof(ARRegister.curyOrigDocAmt))]
			public virtual decimal? CuryOrigDocAmt
			{
				get;
				set;
			}
			#endregion
			#region OrigDocAmt
			public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
			[PXDBDecimal(BqlField = typeof(ARRegister.origDocAmt))]
			public virtual decimal? OrigDocAmt
			{
				get;
				set;
			}
			#endregion
			#region CuryDocBal
			public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
			[PXDBDecimal(BqlField = typeof(ARRegister.curyDocBal))]
			public virtual decimal? CuryDocBal
			{
				get;
				set;
			}
			#endregion
			#region DLReleased
			public abstract class dLReleased : PX.Data.BQL.BqlBool.Field<dLReleased> { }
			[PXDBBool(BqlField = typeof(ARDunningLetterDetail.released))]
			public virtual bool? DLReleased
			{
				get;
				set;
			}
			#endregion
        }

		protected class IncludeTypes
		{
			public const int IncludeAll = 0;
			public const int IncludeLevels = 1;

			public class ListAttribute : PXIntListAttribute
			{
				public ListAttribute() :
					base(new int[] { IncludeAll, IncludeLevels },
						new string[] { Messages.IncludeAllToDL, Messages.IncludeLevelsToDL })
				{ }
			}
		}
		[Serializable]
		public partial class ARDunningLetterRecordsParameters : IBqlTable
		{
			#region CustomerClass
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Customer Class", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(CustomerClass.customerClassID))]
			public virtual string CustomerClassID
			{
				get;
				set;
			}
			#endregion
			#region DocDate
			public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Dunning Letter Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? DocDate
			{
				get;
				set;
			}
			#endregion

			#region IncludeNonOverdueDunning
			public abstract class includeNonOverdueDunning : PX.Data.BQL.BqlBool.Field<includeNonOverdueDunning> { }
			[PXDBBool]
			[PXDefault(typeof(Search<ARSetup.includeNonOverdueDunning>))]
			[PXUIField(DisplayName = Messages.IncludeNonOverdue, Visibility = PXUIVisibility.Visible)]
			public virtual bool? IncludeNonOverdueDunning
			{
				get;
				set;
			}
			#endregion
			#region IncludeType
			public abstract class includeType : PX.Data.BQL.BqlInt.Field<includeType> { }
			[PXInt]
			[PXDefault(IncludeTypes.IncludeAll)]
			[PXUIField(DisplayName = "Include Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
			[IncludeTypes.List]
			public virtual int? IncludeType
			{
				get;
				set;
			}
			#endregion
			#region LevelFrom
			public abstract class levelFrom : PX.Data.BQL.BqlInt.Field<levelFrom> { }
			[PXInt]
			[PXDefault(1, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "From", Enabled = false)]
			public virtual int? LevelFrom
			{
				get;
				set;
			}
			#endregion
			#region LevelTo
			public abstract class levelTo : PX.Data.BQL.BqlInt.Field<levelTo> { }
			[PXInt]
			[PXDefault(typeof(Search<ARDunningSetup.dunningLetterLevel, Where<True, Equal<True>>, OrderBy<Desc<ARDunningSetup.dunningLetterLevel>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "To", Enabled = false)]
			public virtual int? LevelTo
			{
				get;
				set;
			}
			#endregion
		}

		[Serializable]
		public partial class ARDunningLetterList : IBqlTable
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
			#region CustomerClass
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Customer Class", Visibility = PXUIVisibility.Visible)]
			public virtual string CustomerClassID
			{
				get;
				set;
			}
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[Branch(IsKey = true)]
			public virtual int? BranchID
			{
				get;
				set;
			}
			#endregion
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBInt(IsKey = true)]
			[PXDefault]
			[Customer(DescriptionField = typeof(Customer.acctName))]
			[PXUIField(DisplayName = "Customer")]
			public virtual int? BAccountID
			{
				get;
				set;
			}
			#endregion

			#region DueDate
			public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			[PXDBDate(IsKey = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Earliest Due Date")]
			public virtual DateTime? DueDate
			{
				get;
				set;
			}
			#endregion
			#region NumberOfDocuments
			public abstract class numberOfDocuments : PX.Data.BQL.BqlInt.Field<numberOfDocuments> { }
			[PXInt]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Number of Documents")]
			public virtual int? NumberOfDocuments
			{
				get;
				set;
			}
			#endregion
			#region NumberOfOverdueDocuments
			public abstract class numberOfOverdueDocuments : PX.Data.BQL.BqlInt.Field<numberOfOverdueDocuments> { }
			[PXInt]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Number of Overdue Documents")]
			public virtual int? NumberOfOverdueDocuments
			{
				get;
				set;
			}
			#endregion
			#region OrigDocAmt
			public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
			[PXDBBaseCury]
			[PXUIField(DisplayName = "Customer Balance")]
			public virtual decimal? OrigDocAmt
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

			#region DunningLetterLevel
			public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
			[PXInt]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Dunning Letter Level")]
			public virtual int? DunningLetterLevel
			{
				get;
				set;
			}
			#endregion
			#region LastDunningLetterDate
			public abstract class lastDunningLetterDate : PX.Data.BQL.BqlDateTime.Field<lastDunningLetterDate> { }
			[PXDBDate(IsKey = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Last Dunning Letter Date")]
			public virtual DateTime? LastDunningLetterDate
			{
				get;
				set;
			}
			#endregion
			#region DueDays
			public abstract class dueDays : PX.Data.BQL.BqlInt.Field<dueDays> { }
			[PXDBInt]
			[PXDefault]
			[PXUIField(DisplayName = "Due Days")]
			public virtual int? DueDays
			{
				get;
				set;
			}
			#endregion
		}

		#endregion

		#region selects+ctor
		public PXFilter<ARDunningLetterRecordsParameters> Filter;
		public PXCancel<ARDunningLetterRecordsParameters> Cancel;

		[PXFilterable]
		public PXFilteredProcessing<ARDunningLetterList, ARDunningLetterRecordsParameters> DunningLetterList;

		public PXSelect<ARSetup> arsetup;

		public readonly List<ARDunningSetup> DunningSetupList = new List<ARDunningSetup>();

		public ARDunningLetterProcess()
		{
			DunningLetterList.Cache.AllowDelete = false;
			DunningLetterList.Cache.AllowInsert = false;
			DunningLetterList.Cache.AllowUpdate = true;

			bool processByCustomer = ((ARSetup)arsetup.Select()).DunningLetterProcessType == DunningProcessType.ProcessByCustomer;
			PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.includeType>(Filter.Cache, null, !processByCustomer);
			PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.levelFrom>(Filter.Cache, null, !processByCustomer);
			PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.levelTo>(Filter.Cache, null, !processByCustomer);

			foreach (ARDunningSetup setup in PXSelectOrderBy<ARDunningSetup, OrderBy<Asc<ARDunningSetup.dunningLetterLevel>>>.Select(this))
			{
				this.DunningSetupList.Add(setup);
			}

			DunningLetterList.SetProcessDelegate(list => DunningLetterProc(list, Filter.Current));
			PXUIFieldAttribute.SetEnabled(DunningLetterList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARDunningLetterList.selected>(DunningLetterList.Cache, null, true);
			DunningLetterList.SetSelected<ARDunningLetterList.selected>();
		}
		#endregion

		#region events
		protected virtual void ARDunningLetterRecordsParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARDunningLetterRecordsParameters row = (ARDunningLetterRecordsParameters)e.Row;
			if (row != null)
			{
				ARDunningLetterRecordsParameters filter = (ARDunningLetterRecordsParameters)this.Filter.Cache.CreateCopy(row);
				DunningLetterList.SetProcessDelegate(list => DunningLetterProc(list, filter));

				bool includeAll = row.IncludeType == IncludeTypes.IncludeAll;
				PXUIFieldAttribute.SetEnabled<ARDunningLetterRecordsParameters.levelFrom>(sender, null, !includeAll);
				PXUIFieldAttribute.SetEnabled<ARDunningLetterRecordsParameters.levelTo>(sender, null, !includeAll);
			}
		}

		protected virtual void ARDunningLetterRecordsParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DunningLetterList.Cache.Clear();
		}
		#endregion

		#region Delegate for select
		protected virtual IEnumerable dunningLetterList()
		{
			ARDunningLetterRecordsParameters header = Filter.Current;
			if (header == null || header.DocDate == null)
			{
				yield break;
			}
			IEnumerable<ARDunningLetterList> result = PrepareList(header);

			foreach (ARDunningLetterList item in result)
			{
				item.Selected = (DunningLetterList.Locate(item) ?? item).Selected;
				PXCache cache = DunningLetterList.Cache;
				cache.Hold(item);
				yield return item;
			}
		}

		private IEnumerable<ARDunningLetterList> PrepareList(ARDunningLetterRecordsParameters header)
		{
			int processType = ((ARSetup)arsetup.Select()).DunningLetterProcessType.Value;
			List<int> dueDaysByLevel = DunningSetupList.Select(setup => setup.DueDays ?? 0).ToList();

			IEnumerable<PXResult<Customer>> rows = GetData(this, header, processType, dueDaysByLevel);
			return ComposeResult(this, header.DocDate, DunningSetupList, dueDaysByLevel, rows);
		}

		private static IEnumerable<PXResult<Customer>> GetData(PXGraph graph, ARDunningLetterRecordsParameters header, int processType, List<int> dueDaysByLevel)
		{
			if (processType == DunningProcessType.ProcessByCustomer)
			{
				return PXSelectJoinGroupBy<Customer,
					InnerJoin<ARInvoiceWithDL,
						On<ARInvoiceWithDL.customerID, Equal<Customer.bAccountID>,
						And<ARInvoiceWithDL.dueDate, Less<Required<ARDunningLetterRecordsParameters.docDate>>>>,
					LeftJoin<ARBalances,
						On<ARBalances.customerID, Equal<Customer.sharedCreditCustomerID>,
						And<ARBalances.branchID, Equal<ARInvoiceWithDL.branchID>,
						And<ARBalances.customerLocationID, Equal<ARInvoiceWithDL.customerLocationID>>>>>>,
					Where2<Where<Customer.printDunningLetters, Equal<True>,
							Or<Customer.mailDunningLetters, Equal<True>>>,
							And2<Where<ARInvoiceWithDL.dLReleased, Equal<True>,
							Or<ARInvoiceWithDL.dLReleased, IsNull>>,
						And2<Match<Current<AccessInfo.userName>>,
						And<Where<Customer.customerClassID, Equal<Required<ARDunningLetterRecordsParameters.customerClassID>>,
							Or<Required<ARDunningLetterRecordsParameters.customerClassID>, IsNull>>>>>>,
					Aggregate<GroupBy<Customer.sharedCreditCustomerID,
						GroupBy<ARInvoiceWithDL.branchID,
						Min<ARInvoiceWithDL.dueDate,
						Sum<ARInvoiceWithDL.docBal,
						Count<ARInvoiceWithDL.refNbr>>>>>>>.Select(graph, header.DocDate, header.CustomerClassID, header.CustomerClassID);
			}
			else if (processType == DunningProcessType.ProcessByDocument)
			{
				var cmd = new PXSelectJoinGroupBy<Customer,
					InnerJoin<ARInvoiceWithDL,
						On<ARInvoiceWithDL.customerID, Equal<Customer.bAccountID>,
						And<ARInvoiceWithDL.revoked, Equal<False>,
						And<ARInvoiceWithDL.dueDate, Less<Required<ARDunningLetterRecordsParameters.docDate>>,
						And<Where<ARInvoiceWithDL.dunningLetterLevel, Equal<Required<ARInvoiceWithDL.dunningLetterLevel>>,
							Or<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull, And<Required<ARInvoiceWithDL.dunningLetterLevel>, Equal<int0>>>>>>>>>,
					LeftJoin<ARBalances,
						On<ARBalances.customerID, Equal<Customer.sharedCreditCustomerID>,
						And<ARBalances.branchID, Equal<ARInvoiceWithDL.branchID>,
						And<ARBalances.customerLocationID, Equal<ARInvoiceWithDL.customerLocationID>>>>>>,
					Where2<Where<Customer.printDunningLetters, Equal<True>,
							Or<Customer.mailDunningLetters, Equal<True>>>,
							And2<Where<ARInvoiceWithDL.dLReleased, Equal<True>,
							Or<ARInvoiceWithDL.dLReleased, IsNull>>,
					And2<Match<Current<AccessInfo.userName>>,
						And<Where<Customer.customerClassID, Equal<Required<ARDunningLetterRecordsParameters.customerClassID>>,
							Or<Required<ARDunningLetterRecordsParameters.customerClassID>, IsNull>>>>>>,
					Aggregate<GroupBy<ARInvoiceWithDL.dunningLetterLevel,
						GroupBy<Customer.sharedCreditCustomerID,
						GroupBy<ARInvoiceWithDL.branchID,
						Min<ARInvoiceWithDL.dueDate,
						Sum<ARInvoiceWithDL.docBal,
						Count<ARInvoiceWithDL.refNbr>>>>>>>>(graph);
				if (header.IncludeType == 1)
				{
					cmd.WhereAnd<Where2<Where<ARInvoiceWithDL.dunningLetterLevel, GreaterEqual<Required<ARDunningLetterRecordsParameters.levelFrom>>,
										And<ARInvoiceWithDL.dunningLetterLevel, LessEqual<Required<ARDunningLetterRecordsParameters.levelTo>>>>,
					Or<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull, And<Required<ARDunningLetterRecordsParameters.levelFrom>, Less<int1>>>>>>();
				}

				List<int> levels = new List<int>();
				for (int i = 0; i < dueDaysByLevel.Count; i++)
				{
					levels.Add(i);
				}
				List<PXResult<Customer>> results = new List<PXResult<Customer>>();
				foreach (int level in levels)
				{
					results = results.Concat(cmd.Select(
					header.DocDate.Value.AddDays(-1 * dueDaysByLevel[level]),
					level,
					level,
					header.CustomerClassID,
					header.CustomerClassID,
					header.LevelFrom - 1,
					header.LevelTo - 1,
					header.LevelFrom - 1)).ToList();
				}
				return results;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		private static List<ARDunningLetterList> ComposeResult(PXGraph graph, DateTime? date, List<ARDunningSetup> dunningSetupList, List<int> dueDaysByLevel, IEnumerable<PXResult<Customer>> rows)
		{
			int maxDunningLevel = dunningSetupList.Count;
			List<ARDunningLetterList> returnList = new List<ARDunningLetterList>();
			foreach (PXResult<Customer, ARInvoiceWithDL, ARBalances> row in rows)
			{
				Customer customer = row;
				ARInvoiceWithDL invoice = row;
				ARBalances balance = row;
				int currentLevel = 0;
				if (invoice == null)
					continue;
				currentLevel = invoice.DunningLetterLevel ?? 0;
				if (currentLevel == maxDunningLevel)
					continue;
				if (invoice.DueDate.Value.AddDays(dueDaysByLevel[currentLevel]) >= date)
					continue;
				if (currentLevel > 0 && invoice.DunningLetterDate.Value.AddDays(dueDaysByLevel[currentLevel] - dueDaysByLevel[currentLevel - 1]) >= date)
					continue;
				if (currentLevel == 0)
				{
					ARDunningLetterList duplicate = returnList.Where(
						dl => dl.BAccountID == customer.BAccountID
								&& dl.BranchID == invoice.BranchID
								&& dl.DunningLetterLevel == 1).FirstOrDefault<ARDunningLetterList>();
					if (duplicate != null)
					{
						duplicate.NumberOfOverdueDocuments += row.RowCount;
						duplicate.DocBal += invoice.DocBal;
						duplicate.LastDunningLetterDate = duplicate.LastDunningLetterDate ?? invoice.DunningLetterDate;
						duplicate.DueDate = duplicate.DueDate > invoice.DueDate ? invoice.DueDate : duplicate.DueDate;
						continue;
					}
				}
				ARDunningLetterList item = new ARDunningLetterList();
				item.BAccountID = customer.SharedCreditCustomerID;
				item.BranchID = invoice.BranchID;
				item.CustomerClassID = customer.CustomerClassID;
				item.DocBal = invoice.DocBal;
				item.DueDate = invoice.DueDate;
				item.DueDays = (dunningSetupList[currentLevel].DaysToSettle ?? 0);
				item.DunningLetterLevel = currentLevel + 1;
				item.LastDunningLetterDate = invoice.DunningLetterDate;
				item.NumberOfOverdueDocuments = row.RowCount;
				item.OrigDocAmt = PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>()
					? PXSelectJoinGroupBy<ARBalances,
						InnerJoin<Customer, On<Customer.bAccountID, Equal<ARBalances.customerID>>>,
						Where<ARBalances.branchID, Equal<Required<ARBalances.branchID>>,
							And<Customer.sharedCreditCustomerID, Equal<Required<Customer.sharedCreditCustomerID>>>>,
						Aggregate<GroupBy<ARBalances.customerID,
							Sum<ARBalances.currentBal>>>>.Select(graph, item.BranchID, item.BAccountID).AsEnumerable()
						.Sum(cons => ((ARBalances)cons).CurrentBal)
					: balance.CurrentBal;
				item.NumberOfDocuments = PXSelectJoin<Customer,
					InnerJoin<ARRegister, On<ARRegister.customerID, Equal<Customer.bAccountID>>>,
					Where<Customer.sharedCreditCustomerID, Equal<Required<Customer.sharedCreditCustomerID>>,
						And<ARRegister.released, Equal<True>,
						And<ARRegister.openDoc, Equal<True>,
						And<ARRegister.voided, Equal<False>,
						And<ARRegister.pendingPPD, NotEqual<True>,
						And2<Where<ARRegister.docType, Equal<ARDocType.invoice>,
							Or<ARRegister.docType, Equal<ARDocType.finCharge>,
							Or<ARRegister.docType, Equal<ARDocType.debitMemo>>>>,
						And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>>>>>>>>>.Select(graph, item.BAccountID, date).Count();

				returnList.Add(item);
			}

			return returnList;
		}

		#endregion

		#region Processing
		private static void DunningLetterProc(List<ARDunningLetterList> list, ARDunningLetterRecordsParameters filter)
		{
			DunningLetterMassProcess graph = PXGraph.CreateInstance<DunningLetterMassProcess>();
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool errorsInProcessing = false;
				ARSetup arsetup = PXSelect<ARSetup>.Select(graph);
				bool autoRelease = arsetup.AutoReleaseDunningLetter == true;
				bool processByCutomer = arsetup.DunningLetterProcessType == DunningProcessType.ProcessByCustomer;
				List<int> dueDaysByLevel = new List<int>();
				foreach (ARDunningSetup setup in PXSelectOrderBy<ARDunningSetup, OrderBy<Asc<ARDunningSetup.dunningLetterLevel>>>.Select(graph))
				{
					dueDaysByLevel.Add(setup.DueDays ?? 0);
				}
				bool consolidateBranch = arsetup.ConsolidatedDunningLetter == true;
				int? consolidationBranch = null;
				if (consolidateBranch)
				{
					consolidationBranch = arsetup.DunningLetterBranchID;
				}

				List<ARDunningLetterList> uniqueList = consolidateBranch
					? DistinctBy(list, a => a.BAccountID).ToList()
					: DistinctBy(list, a => new { a.BAccountID, a.BranchID }).ToList();

				foreach (ARDunningLetterList uniqueItem in uniqueList)
				{
					int? bAccountID = uniqueItem.BAccountID;
					int? branchID = consolidateBranch ? consolidationBranch : uniqueItem.BranchID;
					int? dueDays = uniqueItem.DueDays;
					List<int> levels = new List<int>();
					List<ARDunningLetterList> listToMerge = consolidateBranch ?
						list.Where((item) => item.BAccountID == bAccountID).ToList() :
						list.Where((item) => item.BAccountID == bAccountID && item.BranchID == branchID).ToList();
					foreach (ARDunningLetterList item in listToMerge)
					{
						dueDays = dueDays < item.DueDays ? dueDays : item.DueDays;
						levels.Add(item.DunningLetterLevel ?? 0);
					}
					try
					{
						ARDunningLetter letterToRelease = CreateDunningLetter(
							graph, 
							bAccountID, 
							branchID, 
							filter.DocDate, 
							dueDays, 
							levels,
							filter.IncludeNonOverdueDunning ?? false, 
							processByCutomer, 
							consolidateBranch, 
							dueDaysByLevel);
						try
						{
							if (autoRelease)
							{
								ARDunningLetterMaint.ReleaseProcess(letterToRelease);
							}
							foreach (ARDunningLetterList item in listToMerge)
							{
								PXProcessing.SetCurrentItem(item);
								PXProcessing.SetProcessed();
							}
						}
						catch (Exception exc)
						{

							string delimiter = "; ";
							string message = exc is PXOuterException outerExc
								? $"{outerExc.MessageNoPrefix}{delimiter}{string.Join(delimiter, outerExc.InnerMessages)}"
								: exc is PXException pxExc 
									? pxExc.MessageNoPrefix
									: exc.Message;

							foreach (ARDunningLetterList item in listToMerge)
							{
								PXProcessing.SetCurrentItem(item);
								PXProcessing.SetWarning(string.Format(Messages.DunningLetterNotReleased, message));
							}
						}
					}
					catch (Exception e)
					{
						foreach (ARDunningLetterList item in listToMerge)
						{
							PXProcessing.SetCurrentItem(item);
							PXProcessing.SetError(e);
							errorsInProcessing = true;
						}
					}
				}
				if (errorsInProcessing)
				{
					throw new PXException(Messages.DunningLetterNotCreated);
				}
			});
		}

		public static ARDunningLetter CreateDunningLetter(DunningLetterMassProcess graph, int? bAccountID, int? branchID, DateTime? docDate, int? dueDays, List<int> includedLevels, bool includeNonOverdue, bool processByCutomer, bool consolidateBranch, List<int> dueDaysByLevel)
		{
			graph.Clear();

			int maxDunningLevel = dueDaysByLevel.Count;
			ARDunningLetter doc = CreateDunningLetterHeader(graph, bAccountID, branchID, docDate, dueDays, consolidateBranch);
			doc = graph.docs.Insert(doc);
			foreach (PXResult<ARInvoiceWithDL> result in GetInvoiceList(graph, bAccountID, branchID, docDate, includedLevels, includeNonOverdue, processByCutomer, consolidateBranch, dueDaysByLevel))
			{
				ARDunningLetterDetail docDet = CreateDunningLetterDetail(docDate, processByCutomer, result, dueDaysByLevel);
				doc.DunningLetterLevel = Math.Max(doc.DunningLetterLevel ?? 0, docDet.DunningLetterLevel ?? 0);
				if (doc.DunningLetterLevel == maxDunningLevel)
				{
					doc.LastLevel = true;
				}
				graph.docsDet.Insert(docDet);
			}

			graph.docs.Update(doc);
			graph.Actions.PressSave();
			return doc;
		}

		private static List<PXResult<ARInvoiceWithDL>> GetInvoiceList(PXGraph graph, int? bAccountID, int? branchID, DateTime? docDate, List<int> includedLevels, bool includeNonOverdue, bool processByCutomer, bool consolidateBranch, List<int> dueDaysByLevel)
		{
			if (processByCutomer)
			{
				var cmd = new PXSelectGroupBy<ARInvoiceWithDL,
					Where<ARInvoiceWithDL.sharedCreditCustomerID, Equal<Required<ARInvoiceWithDL.customerID>>,
					   And<ARInvoiceWithDL.docDate, LessEqual<Required<ARInvoiceWithDL.docDate>>>>,
				   Aggregate<GroupBy<ARInvoiceWithDL.released,
					   GroupBy<ARInvoiceWithDL.refNbr,
					   GroupBy<ARInvoiceWithDL.docType>>>>>(graph);

				if (!includeNonOverdue)
				{
					cmd.WhereAnd<Where<ARInvoiceWithDL.dueDate, Less<Required<ARInvoice.dueDate>>>>();
				}
				else
				{
					cmd.WhereAnd<Where<Required<ARInvoice.dueDate>, IsNotNull>>();
				}
				if (!consolidateBranch)
				{
					cmd.WhereAnd<Where<ARInvoiceWithDL.branchID, Equal<Required<ARInvoiceWithDL.branchID>>>>();
				}
				return cmd.Select(bAccountID, docDate, docDate, branchID).ToList();
			}
			else
			{
				List<PXResult<ARInvoiceWithDL>> results = new List<PXResult<ARInvoiceWithDL>>();

				foreach (int level in includedLevels)
				{
					var cmd = new PXSelectGroupBy<ARInvoiceWithDL,
						Where<ARInvoiceWithDL.sharedCreditCustomerID, Equal<Required<ARInvoiceWithDL.customerID>>,
							And<ARInvoiceWithDL.revoked, Equal<False>,
							And<ARInvoiceWithDL.dueDate, Less<Required<ARInvoice.dueDate>>,
							And<ARInvoiceWithDL.docDate, LessEqual<Required<ARInvoiceWithDL.docDate>>,
							And<Where<ARInvoiceWithDL.dunningLetterLevel, Equal<Required<ARDunningLetter.dunningLetterLevel>>,
								Or<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull,
									And<Required<ARDunningLetter.dunningLetterLevel>, Equal<int0>>>>>>>>>>,
						Aggregate<GroupBy<ARInvoiceWithDL.dunningLetterLevel,
							 GroupBy<ARInvoiceWithDL.released,
							 GroupBy<ARInvoiceWithDL.refNbr,
							 GroupBy<ARInvoiceWithDL.docType>>>>>>(graph);
					if (!consolidateBranch)
					{
						cmd.WhereAnd<Where<ARInvoiceWithDL.branchID, Equal<Required<ARInvoiceWithDL.branchID>>>>();
					}
					results = results.Concat(cmd.Select(bAccountID, docDate.Value.AddDays(-1 * dueDaysByLevel[level - 1]), docDate, level - 1, level - 1, branchID)).ToList();
					if (level == 1 && includeNonOverdue)
					{
						var cmdLvl1 = new PXSelectGroupBy<ARInvoiceWithDL,
					   Where<ARInvoiceWithDL.sharedCreditCustomerID, Equal<Required<ARInvoiceWithDL.customerID>>,
						   And<ARInvoiceWithDL.revoked, Equal<False>,
						   And2<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull,
							Or<ARInvoiceWithDL.dunningLetterLevel, Equal<int0>>>,
						   And<ARInvoiceWithDL.dueDate, Less<Required<ARInvoiceWithDL.docDate>>,
						   And<ARInvoiceWithDL.dueDate, GreaterEqual<Required<ARInvoiceWithDL.docDate>>,
						   And<ARInvoiceWithDL.docDate, LessEqual<Required<ARInvoiceWithDL.docDate>>>>>>>>,
					   Aggregate<GroupBy<ARInvoiceWithDL.dunningLetterLevel,
							GroupBy<ARInvoiceWithDL.released,
							GroupBy<ARInvoiceWithDL.refNbr,
							GroupBy<ARInvoiceWithDL.docType>>>>>>(graph);
						if (!consolidateBranch)
						{
							cmdLvl1.WhereAnd<Where<ARInvoiceWithDL.branchID, Equal<Required<ARInvoiceWithDL.branchID>>>>();
						}
						results = results.Concat(cmdLvl1.Select(bAccountID, docDate, docDate.Value.AddDays(-1 * dueDaysByLevel[0]), docDate, branchID)).ToList();
					}
				}
				if (includeNonOverdue)
				{
					var cmdNonOverdue = new PXSelectGroupBy<ARInvoiceWithDL,
						Where<ARInvoiceWithDL.sharedCreditCustomerID, Equal<Required<ARInvoiceWithDL.customerID>>,
							And<ARInvoiceWithDL.revoked, Equal<False>,
							And2<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull,
								Or<ARInvoiceWithDL.dunningLetterLevel, Equal<int0>>>,
							And<ARInvoiceWithDL.dueDate, GreaterEqual<Required<ARInvoiceWithDL.docDate>>,
							And<ARInvoiceWithDL.docDate, LessEqual<Required<ARInvoiceWithDL.docDate>>>>>>>,
						Aggregate<GroupBy<ARInvoiceWithDL.dunningLetterLevel,
							 GroupBy<ARInvoiceWithDL.released,
							 GroupBy<ARInvoiceWithDL.refNbr,
							 GroupBy<ARInvoiceWithDL.docType>>>>>>(graph);
					if (!consolidateBranch)
					{
						cmdNonOverdue.WhereAnd<Where<ARInvoiceWithDL.branchID, Equal<Required<ARInvoiceWithDL.branchID>>>>();
					}
					results = results.Concat(cmdNonOverdue.Select(bAccountID, docDate, docDate, branchID)).ToList();
				}
				return results;
			}
		}

		private static ARDunningLetter CreateDunningLetterHeader(PXGraph graph, int? bAccountID, int? branchID, DateTime? docDate, int? dueDays, bool consolidated)
		{
			ARDunningLetter doc = new ARDunningLetter();
			doc.BAccountID = bAccountID;
			doc.BranchID = branchID;
			doc.DunningLetterDate = docDate;
			doc.Deadline = docDate.Value.AddDays(dueDays.Value);
			doc.Consolidated = consolidated;
			doc.Released = false;
			doc.Printed = false;
			doc.Emailed = false;
			doc.LastLevel = false;
			Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(graph, bAccountID);
			doc.DontPrint = customer.PrintDunningLetters == false;
			doc.DontEmail = customer.MailDunningLetters == false;
			return doc;
		}

		private static ARDunningLetterDetail CreateDunningLetterDetail(DateTime? docDate, bool processByCutomer, ARInvoiceWithDL invoice, List<int> dueDaysByLevel)
		{
			ARDunningLetterDetail detail = new ARDunningLetterDetail();

			detail.CuryOrigDocAmt = invoice.CuryOrigDocAmt;
			detail.CuryDocBal = invoice.CuryDocBal;
			detail.CuryID = invoice.CuryID;
			detail.OrigDocAmt = invoice.OrigDocAmt;
			detail.DocBal = invoice.DocBal;
			detail.DueDate = invoice.DueDate;
			detail.DocType = invoice.DocType;
			detail.RefNbr = invoice.RefNbr;
			detail.BAccountID = invoice.CustomerID;
			detail.DunningLetterBAccountID = invoice.SharedCreditCustomerID;
			detail.DocDate = invoice.DocDate;
			detail.Overdue = invoice.DueDate < docDate;
			if ((processByCutomer && invoice.DueDate >= docDate) || invoice.DueDate.Value.AddDays(dueDaysByLevel[invoice.DunningLetterLevel ?? 0]) >= docDate)
			{
				detail.DunningLetterLevel = 0;
			}
			else
			{
				detail.DunningLetterLevel = (invoice.DunningLetterLevel ?? 0) + 1;
			}
			return detail;
		}
		#endregion
	}

	[PXHidden]
	public class DunningLetterMassProcess : PXGraph<DunningLetterMassProcess>
	{
		[PXViewName("DunningLetter")]
		public PXSelect<ARDunningLetter> docs;
		[PXViewName("DunningLetterDetail")]
		public PXSelect<ARDunningLetterDetail, 
			Where<ARDunningLetterDetail.dunningLetterID, Equal<Required<ARDunningLetter.dunningLetterID>>>> docsDet;
	}
}