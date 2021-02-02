using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using PX.Data;

using PX.Objects.CM;
using PX.Objects.Common;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARStatementDetails : PXGraph<ARStatementDetails>
    {
		#region Internal types definition
		[System.SerializableAttribute()]
		public partial class ARStatementDetailsParameters : IBqlTable
		{
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
			[PXDate()]
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
		}

		[Serializable]
		public partial class DetailsResult : IBqlTable
		{
			#region CustomerId
			public abstract class customerId : PX.Data.BQL.BqlInt.Field<customerId> { }
			protected Int32? _CustomerId;
			[PXInt(IsKey = true)]			
			[CustomerActive(DescriptionField = typeof(Customer.acctName), DisplayName = "Customer")]
			public virtual Int32? CustomerId
			{
				get
				{
					return this._CustomerId;
				}
				set
				{
					this._CustomerId = value;
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
			
			[PXBaseCury()]
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
			[PXDBBool()]
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
			#region DontPrint
			public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
			protected Boolean? _DontPrint;
			[PXBool()]
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
			[PXBool()]
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
				[PXDependsOnFields(typeof(statementBalance),typeof(ageBalance00))]
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
				[PXDependsOnFields(typeof(curyStatementBalance),typeof(curyAgeBalance00))]
				get
				{
					return (this._CuryStatementBalance ) - (this.CuryAgeBalance00??Decimal.Zero);
				}
			}
			#endregion
			#region OnDemand
			public abstract class onDemand : PX.Data.BQL.BqlBool.Field<onDemand> { }
			[PXBool]
			[PXUIField(DisplayName = Messages.OnDemandStatement)]
			public virtual bool? OnDemand
			{
				get;
				set;
			}
			#endregion
			#region PreparedOn
			public abstract class preparedOn : PX.Data.BQL.BqlDateTime.Field<preparedOn> { }
			[PXDate]
			[PXUIField(DisplayName = Messages.PreparedOn)]
			public virtual DateTime? PreparedOn
			{
				get;
				set;
			}
			#endregion

			public virtual void Copy(ARStatement aSrc, Customer cust)
			{
				this.CustomerId = cust.BAccountID;
				this.UseCurrency = cust.PrintCuryStatements;
				this.StatementBalance = aSrc.EndBalance ?? decimal.Zero;
				this.AgeBalance00 = aSrc.AgeBalance00 ?? decimal.Zero;
				this.CuryID = aSrc.CuryID;
				this.CuryStatementBalance = aSrc.CuryEndBalance ?? decimal.Zero;
				this.CuryAgeBalance00 = aSrc.CuryAgeBalance00 ?? decimal.Zero;
				this.DontPrint = aSrc.DontPrint;
				this.Printed = aSrc.Printed;
				this.DontEmail = aSrc.DontEmail;
				this.Emailed = aSrc.Emailed;
				this.OnDemand = aSrc.OnDemand;
				this.PreparedOn = aSrc.LastModifiedDateTime;
			}
			public virtual void Append(DetailsResult aSrc)
			{
				this.StatementBalance += aSrc.StatementBalance;
				this.AgeBalance00 += aSrc.AgeBalance00;
				if (this.CuryID == aSrc.CuryID)
				{
					this.CuryStatementBalance += aSrc.CuryStatementBalance;
					this.CuryAgeBalance00 += aSrc.CuryAgeBalance00;
				}
				else
				{
					this.CuryStatementBalance = Decimal.Zero;
					this.CuryAgeBalance00 = Decimal.Zero;
				}
				if (aSrc.DontEmail == false)
					this.DontEmail = false;
				if (aSrc.DontPrint == false)
					this.DontPrint = false;
				if (aSrc.Emailed == false)
					this.Emailed = false;
				if (aSrc.Printed == false)
					this.Printed = false;
			}
			public virtual void ResetToBaseCury(string aBaseCuryID)
			{
				this._CuryID = aBaseCuryID;
				this._CuryStatementBalance = this._StatementBalance;
				this._CuryAgeBalance00 = this._AgeBalance00;
			}

		} 
		#endregion

		#region Ctor
		public ARStatementDetails()
		{
			ARSetup setup = ARSetup.Current;
			Details.Cache.AllowDelete = false;
			Details.Cache.AllowInsert = false;
			Details.Cache.AllowUpdate = false;
		} 
		#endregion

		#region Public Members
		
		public PXCancel<ARStatementDetailsParameters> Cancel;
		public PXAction<ARStatementDetailsParameters> prevStatementDate;
		public PXAction<ARStatementDetailsParameters> nextStatementDate;
		
		public PXFilter<ARStatementDetailsParameters> Filter;
		[PXFilterable]
		public PXSelect<DetailsResult> Details;
		public PXSetup<ARSetup> ARSetup;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PrevStatementDate(PXAdapter adapter)
		{

			ARStatementDetailsParameters filter = this.Filter.Current;

			if (filter != null && !string.IsNullOrEmpty(filter.StatementCycleId))
			{
				ARStatement statement = PXSelect<ARStatement, Where<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
			                                    And<
												Where<ARStatement.statementDate, Less<Required<ARStatement.statementDate>>,
														Or<Required<ARStatement.statementDate>, IsNull>>>>,
			                                    OrderBy<Desc<ARStatement.statementDate>>>.Select(this,filter.StatementCycleId,filter.StatementDate,filter.StatementDate);

				if (statement != null)
				{
					filter.StatementDate = statement.StatementDate;
				}
			}

			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextStatementDate(PXAdapter adapter)
		{

			ARStatementDetailsParameters filter = this.Filter.Current;

			if (filter != null && !string.IsNullOrEmpty(filter.StatementCycleId))
			{
				ARStatement statement = PXSelect<ARStatement, Where<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
												And<
												Where<ARStatement.statementDate, Greater<Required<ARStatement.statementDate>>,
														Or<Required<ARStatement.statementDate>, IsNull>>>>,
												OrderBy<Asc<ARStatement.statementDate>>>.Select(this, filter.StatementCycleId, filter.StatementDate, filter.StatementDate);

				if (statement != null)
				{
					filter.StatementDate = statement.StatementDate;
				}
			}

			return adapter.Get();
		}





		#endregion

		#region Delegates
		protected virtual IEnumerable details()
		{
			ARStatementDetailsParameters header = Filter.Current;
			List<DetailsResult> result = new List<DetailsResult>();
			if (header == null)
			{
				return result;
			}
			GL.Company company = PXSelect<GL.Company>.Select(this);
			foreach (PXResult<ARStatement, Customer> it in PXSelectJoin<ARStatement,
					InnerJoin<Customer, On<Customer.bAccountID, Equal<ARStatement.statementCustomerID>>>,
					Where<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
						And<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>>>,
					OrderBy<Asc<ARStatement.statementCustomerID, Asc<ARStatement.curyID>>>>
					.Select(this, header.StatementDate, header.StatementCycleId))
			{
				DetailsResult res = new DetailsResult();
				ARStatement st = (ARStatement)it;
				Customer cust = (Customer)it;
				res.Copy(st, cust);				
				if (cust.PrintCuryStatements ?? false)
				{
                    DetailsResult last = result.Count > 0 ? result[result.Count - 1] : null;
                    if (last != null && last.CustomerId == res.CustomerId && last.CuryID == res.CuryID)
                    {
                        last.Append(res);
                    }
                    else
                    {
                        result.Add(res);
                    }					
				}
				else
				{
					res.ResetToBaseCury(company.BaseCuryID);
					DetailsResult last = result.Count > 0 ? result[result.Count - 1] : null;
					if (last != null && last.CustomerId == res.CustomerId)
					{
						last.Append(res);
					}
					else
					{
						result.Add(res);
					}
				}
			}
			return result;
		} 
		#endregion

		#region Filter Events
		protected virtual void ARStatementDetailsParameters_StatementCycleId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARStatementDetailsParameters row = (ARStatementDetailsParameters)e.Row;
			if (!string.IsNullOrEmpty(row.StatementCycleId))
			{
				ARStatementCycle cycle = PXSelect<ARStatementCycle,
							Where<ARStatementCycle.statementCycleId,
							Equal<Required<ARStatementCycle.statementCycleId>>>>.Select(this, row.StatementCycleId);
				row.StatementDate = cycle.LastStmtDate;
			}
		} 
		#endregion

		#region Sub-screen Navigation Button
		public PXAction<ARStatementDetailsParameters> viewDetails;
        [PXUIField(DisplayName = Messages.CustomerStatementHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			if (this.Details.Current != null && this.Filter.Current != null)
			{
				DetailsResult res = this.Details.Current;
				ARStatementForCustomer graph = PXGraph.CreateInstance<ARStatementForCustomer>();

				ARStatementForCustomer.ARStatementForCustomerParameters filter = graph.Filter.Current;
				filter.CustomerID = res.CustomerId;
				graph.Filter.Update(filter);
				filter = graph.Filter.Select();
                throw new PXRedirectRequiredException(graph, Messages.CustomerStatementHistory);

			}
			return Filter.Select();
		}

		public PXAction<ARStatementDetailsParameters> printReport;
		[PXUIField(DisplayName = "Print Statement", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable PrintReport(PXAdapter adapter)
		{
			if (this.Details.Current != null && this.Filter.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				DetailsResult res = this.Details.Current;

				Customer customer = PXSelect<Customer,
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(this, res.CustomerId);

				Export(parameters, this.Filter.Current);
				parameters[ARStatementReportParams.Parameters.CustomerID] = customer.AcctCD;				

				string reportID = (res.UseCurrency ?? false) ? ARStatementReportParams.CS_CuryStatementReportID : ARStatementReportParams.CS_StatementReportID;
				throw new PXReportRequiredException(parameters, reportID, "AR Statement Report");
			}
			return Filter.Select();
		}

		#endregion

		#region Utility Functions
		
		protected static void Export(Dictionary<string, string> aRes, ARStatementDetailsParameters aSrc)
		{
			aRes[ARStatementReportParams.Parameters.StatementCycleID] = aSrc.StatementCycleId;
			aRes[ARStatementReportParams.Parameters.StatementDate] = aSrc.StatementDate.Value.ToString("d", CultureInfo.InvariantCulture);
		}  
		#endregion


	}	
}
