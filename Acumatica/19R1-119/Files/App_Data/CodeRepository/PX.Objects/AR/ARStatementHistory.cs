using System;
using System.Collections.Generic;
using PX.Data;
using System.Collections;
using System.Globalization;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARStatementHistory : PXGraph<ARStatementHistory>
	{
		[System.SerializableAttribute()]
		public partial class ARStatementHistoryParameters : IBqlTable
		{
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = Messages.StatementCycleID, Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(ARStatementCycle.statementCycleId))]
			public virtual string StatementCycleId
			{
				get;
				set;
			}
			#endregion
			#region StartDate
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = Messages.StartDate, Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? StartDate
			{
				get;
				set;
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = Messages.EndDate, Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? EndDate
			{
				get;
				set;
			}
			#endregion
			#region
			public abstract class includeOnDemandStatements : PX.Data.BQL.BqlBool.Field<includeOnDemandStatements> { }
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = Messages.IncludeOnDemandStatements, Visibility = PXUIVisibility.Visible)]
			public virtual bool? IncludeOnDemandStatements
			{
				get;
				set;
			}
			#endregion
		}

		[Serializable]
		public partial class HistoryResult : IBqlTable
		{
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
			protected String _StatementCycleId;
			[PXDBString(10, IsUnicode = true, IsKey = true)]
			[PXUIField(DisplayName = "Statement Cycle ID", Visibility = PXUIVisibility.Visible)]
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
			[PXDBDate(IsKey = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Statement Date")]
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
			#region Descr
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
			protected String _Descr;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#endregion
			#region NumberOfDocuments
			public abstract class numberOfDocuments : PX.Data.BQL.BqlInt.Field<numberOfDocuments> { }
			protected int? _NumberOfDocuments;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Number Of Documents")]
			public virtual int? NumberOfDocuments
			{
				get
				{
					return this._NumberOfDocuments;
				}
				set
				{
					this._NumberOfDocuments = value;
				}
			}
			#endregion
			#region DontPrintCount
			public abstract class dontPrintCount : PX.Data.BQL.BqlInt.Field<dontPrintCount> { }
			protected int? _DontPrintCount;
			[PXInt()]
			[PXDefault(0,PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Don't Print",Enabled=false)]
			public virtual int? DontPrintCount
			{
				get
				{
					return this._DontPrintCount;
				}
				set
				{
					this._DontPrintCount = value;
				}
			}
			#endregion
			#region PrintedCount
			public abstract class printedCount : PX.Data.BQL.BqlInt.Field<printedCount> { }
			protected int? _PrintedCount;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Printed", Enabled = false)]
			public virtual int? PrintedCount
			{
				get
				{
					return this._PrintedCount;
				}
				set
				{
					this._PrintedCount = value;
				}
			}
			#endregion
			#region DontEmailCount
			public abstract class dontEmailCount : PX.Data.BQL.BqlInt.Field<dontEmailCount> { }
			protected int? _DontEmailCount;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Don't Email",Enabled=false)]
			public virtual int? DontEmailCount
			{
				get
				{
					return this._DontEmailCount;
				}
				set
				{
					this._DontEmailCount = value;
				}
			}
			#endregion
			#region EmailedCount
			public abstract class emailedCount : PX.Data.BQL.BqlInt.Field<emailedCount> { }
			protected int? _EmailedCount;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Emailed",Enabled=false)]
			public virtual int? EmailedCount
			{
				get
				{
					return this._EmailedCount;
				}
				set
				{
					this._EmailedCount = value;
				}
			}
			#endregion			
			#region NoActionCount
			public abstract class noActionCount : PX.Data.BQL.BqlInt.Field<noActionCount> { }
			protected int? _NoActionCount;
			[PXInt()]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "No Action", Enabled = false)]
			public virtual int? NoActionCount
			{
				get
				{
					return this._NoActionCount;
				}
				set
				{
					this._NoActionCount = value;
				}
			}
			#endregion
			#region ToPrintCount
			public abstract class toPrintCount : PX.Data.BQL.BqlInt.Field<toPrintCount> { }
			
			[PXInt()]			
			[PXUIField(DisplayName = "To Print",Enabled=false)]
			public virtual int? ToPrintCount
			{
				[PXDependsOnFields(typeof(numberOfDocuments),typeof(dontPrintCount))]
				get
				{
					return this.NumberOfDocuments - this.DontPrintCount;
				}
				set
				{
					this._PrintedCount = value;
				}
			}
			#endregion
			#region ToEmailCount
			public abstract class toEmailCount : PX.Data.BQL.BqlInt.Field<toEmailCount> { }
			
			[PXInt()]			
			[PXUIField(DisplayName = "To Email",Enabled=false)]
			public virtual int? ToEmailCount
			{
				[PXDependsOnFields(typeof(numberOfDocuments),typeof(dontEmailCount))]
				get
				{
					return this._NumberOfDocuments - this._DontEmailCount;
				}
				set
				{
					
				}
			}
			#endregion
			#region PrintCompletion
			public abstract class printCompletion : PX.Data.BQL.BqlDecimal.Field<printCompletion> { }
			
			[PXDecimal(2)]			
			[PXUIField(DisplayName = "Print Completion %",Enabled=false)]
			public virtual Decimal? PrintCompletion
			{
				[PXDependsOnFields(typeof(toPrintCount),typeof(printedCount))]
				get
				{
					return (this.ToPrintCount.HasValue && this.ToPrintCount != 0) ? (((decimal)this.PrintedCount / (decimal)this.ToPrintCount) * 100.0m) : 100.0m;
				}
				set
				{
					
				}
			}
			#endregion
			#region EmailCompletion
			public abstract class emailCompletion : PX.Data.BQL.BqlDecimal.Field<emailCompletion> { }

			[PXDecimal(2)]
			[PXUIField(DisplayName = "Email Completion %", Enabled = false)]
			public virtual Decimal? EmailCompletion
			{
				[PXDependsOnFields(typeof(toEmailCount),typeof(emailedCount))]
				get
				{
					return (this.ToEmailCount.HasValue && this.ToEmailCount != 0) ? (((decimal)this.EmailedCount / (decimal)this.ToEmailCount) * 100.0m) : 100.0m;
				}
				set
				{

				}
			}
			#endregion
		}

		public partial class Customer : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBIdentity()]
			public virtual int? BAccountID { get; set; }
			#endregion
			#region PrintCuryStatements
			public abstract class printCuryStatements : PX.Data.BQL.BqlBool.Field<printCuryStatements> { }
			[PXDBBool()]
			public virtual bool? PrintCuryStatements { get; set; }
			#endregion
		}

		public PXFilter<ARStatementHistoryParameters> Filter;
		public PXCancel<ARStatementHistoryParameters> Cancel;
		[PXFilterable]
		public PXSelect<HistoryResult> History;

		public ARStatementHistory()
		{
			ARSetup setup = ARSetup.Current;
			History.Cache.AllowDelete = false;
			History.Cache.AllowInsert = false;
			History.Cache.AllowUpdate = false;
		}

		public PXSetup<ARSetup> ARSetup;
	
		protected virtual IEnumerable history()
		{
			this.History.Cache.Clear();

			ARStatementHistoryParameters filter = Filter.Current;

			if (filter == null)
			{
				yield break;
			}

			int? prevCustomerID = null;
			string prevCuryID = null;
			HistoryResult prevRec = null;

			PXResultset<ARStatement> res;
			List<object> stored = new List<object>();

			PXSelectBase<ARStatement> select = new PXSelectJoin<
				ARStatement,
					LeftJoin<ARStatementCycle, 
						On<ARStatementCycle.statementCycleId, Equal<ARStatement.statementCycleId>>,
					LeftJoin<Customer, 
						On<Customer.bAccountID, Equal<ARStatement.statementCustomerID>>>>,
				Where<
					ARStatement.statementCustomerID, Equal<ARStatement.customerID>,
					And<ARStatement.statementDate, GreaterEqual<Current<ARStatementHistoryParameters.startDate>>,
					And<ARStatement.statementDate, LessEqual<Current<ARStatementHistoryParameters.endDate>>,
					And2<Where<
						ARStatement.statementCycleId, Equal<Current<ARStatementHistoryParameters.statementCycleId>>,
						Or<Current<ARStatementHistoryParameters.statementCycleId>, IsNull>>,
					And<Where<
						ARStatement.onDemand, Equal<False>,
						Or<Current<ARStatementHistoryParameters.includeOnDemandStatements>, Equal<True>>>>>>>>,
				OrderBy<
					Asc<ARStatement.statementCycleId, 
					Asc<ARStatement.statementDate, 
					Asc<ARStatement.customerID,
					Asc<ARStatement.curyID>>>>>>(this);
			
			using (new PXFieldScope(
				select.View,
				typeof(ARStatement.branchID), 
				typeof(ARStatement.statementCycleId), 
				typeof(ARStatement.statementDate), 
				typeof(ARStatement.customerID),
				typeof(ARStatement.statementCustomerID), 
				typeof(ARStatement.curyID),
				typeof(ARStatement.dontPrint), 
				typeof(ARStatement.dontEmail), 
				typeof(ARStatement.printed), 
				typeof(ARStatement.emailed),
				typeof(ARStatementCycle.descr),
				typeof(Customer.printCuryStatements)))
			{
				res = select.Select();
			}

			foreach (PXResult<ARStatement, ARStatementCycle, Customer> item in res)
			{
				stored.Add(item);

				ARStatement st = item;
				ARStatementCycle cycle = item;
				Customer cust  = item;
				HistoryResult rec = new HistoryResult
				{
					StatementCycleId = st.StatementCycleId,
					StatementDate = st.StatementDate,
					Descr = cycle.Descr,
					DontPrintCount = 0,
					DontEmailCount = 0,
					PrintedCount = 0,
					EmailedCount = 0,
					NoActionCount = 0
				};

				rec = (HistoryResult)this.History.Cache.Locate(rec) ?? this.History.Insert(rec);
				if (rec != null)
				{
					if (prevRec != rec) { prevCustomerID = null; prevCuryID = null; }
					if (prevCustomerID != st.CustomerID || (cust.PrintCuryStatements == true && prevCuryID != st.CuryID))
					{
						rec.NumberOfDocuments += 1;
						rec.DontPrintCount += st.DontPrint == true ? 1 : 0;
						rec.DontEmailCount += st.DontEmail == true ? 1 : 0;
						rec.NoActionCount += st.DontEmail == true && st.DontPrint == true ? 1 : 0;
						rec.PrintedCount += st.Printed == true ? 1 : 0;
						rec.EmailedCount += st.Emailed == true ? 1 : 0;
					}

					prevRec = rec;
					prevCustomerID = st.CustomerID;
					prevCuryID = st.CuryID;
				}
			}

			select.StoreCached(new PXCommandKey(new object[] { filter.StartDate, filter.EndDate, filter.StatementCycleId, filter.StatementCycleId }), stored);

			this.History.Cache.IsDirty = false;
			foreach (HistoryResult ret in this.History.Cache.Inserted) 
				yield return ret;
		}

		protected virtual void ARStatementHistoryParameters_StartDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = new DateTime(Accessinfo.BusinessDate.Value.Year, 1, 1);
		}

		#region Sub-screen Navigation Button
		public PXAction<ARStatementHistoryParameters> viewDetails;
		[PXUIField(DisplayName = "Statement History Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			if (this.History.Current != null && this.Filter.Current != null)
			{
				HistoryResult res = this.History.Current;
				ARStatementDetails graph = PXGraph.CreateInstance<ARStatementDetails>();

				ARStatementDetails.ARStatementDetailsParameters filter = graph.Filter.Current;
				filter.StatementCycleId = res.StatementCycleId;
				filter.StatementDate = res.StatementDate;
				graph.Filter.Update(filter);
				filter = graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Statement Details");
			}
			return Filter.Select();
		}

		public PXAction<ARStatementHistoryParameters> printReport;
		[PXUIField(DisplayName = "Print Statements", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable PrintReport(PXAdapter adapter)
		{
            if (this.History.Current != null)
            {
			ARStatementPrint graph = PXGraph.CreateInstance<ARStatementPrint>();
			graph.Filter.Current.StatementCycleId = this.History.Current.StatementCycleId;
			graph.Filter.Current.StatementDate = this.History.Current.StatementDate;

			throw new PXRedirectRequiredException(graph, "Report Process");
		}

            return adapter.Get();
        }
		#endregion

		protected static void Copy(Dictionary<string, string> aDest,HistoryResult aSrc)
		{
			aDest[ARStatementReportParams.Parameters.StatementCycleID] = aSrc.StatementCycleId;
			aDest[ARStatementReportParams.Parameters.StatementDate] = aSrc.StatementDate.Value.ToString("d", CultureInfo.InvariantCulture);
		}
	}
}