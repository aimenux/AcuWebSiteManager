using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using PX.Data;

using PX.Objects.CM;
using PX.Objects.AR.Repositories;
using PX.Objects.GL;

namespace PX.Objects.AR
{
    [PX.Objects.GL.TableAndChartDashboardType]
    public class ARStatementForCustomer : PXGraph<ARStatementForCustomer>
    {
        [System.SerializableAttribute()]
        public partial class ARStatementForCustomerParameters : IBqlTable
        {
            #region CustomerID
            public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
            protected Int32? _CustomerID;
            [PXInt()]
            [PXDefault()]
            [PXUIField(DisplayName = "Customer")]
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
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			[Branch(Required = false)]
			public virtual Int32? BranchID { get; set; }
			#endregion
		}

		[Serializable]
        public partial class DetailsResult : IBqlTable
        {
            #region StatementCycleId
            public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
            protected String _StatementCycleId;
            [PXString(10, IsUnicode = true)]
            [PXUIField(DisplayName = "Statement Cycle")]
            [PXSelector(typeof(ARStatementCycle.statementCycleId))]
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
            [PXDate(IsKey = true)]
            [PXDefault()]
            [PXUIField(DisplayName = "Statement Date")]
            [PXSelector(typeof(Search4<ARStatement.statementDate, Aggregate<GroupBy<ARStatement.statementDate>>>))]
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
            #region CuryID
            public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
            protected String _CuryID;
            [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
            [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
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
            #region CuryStatementBalance
            public abstract class curyStatementBalance : PX.Data.BQL.BqlDecimal.Field<curyStatementBalance> { }
            protected Decimal? _CuryStatementBalance;
            [PXCury(typeof(DetailsResult.curyID))]
            [PXDefault(TypeCode.Decimal, "0.0")]
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
                    return (this._CuryStatementBalance ?? Decimal.Zero) - (this.CuryAgeBalance00 ?? Decimal.Zero);
                }
            }
            #endregion
            #region BranchID
            public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
            protected Int32? _BranchID;
			[Branch(IsKey = true, Required = false, PersistingCheck = PXPersistingCheck.Nothing)]
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
                this.StatementCycleId = aSrc.StatementCycleId;
                this.StatementDate = aSrc.StatementDate;
                this.StatementBalance = aSrc.EndBalance ?? decimal.Zero;
                this.AgeBalance00 = aSrc.AgeBalance00 ?? decimal.Zero;
                this.CuryID = aSrc.CuryID;
                this.CuryStatementBalance = aSrc.CuryEndBalance ?? decimal.Zero;
                this.CuryAgeBalance00 = aSrc.CuryAgeBalance00 ?? decimal.Zero;
                this.DontPrint = aSrc.DontPrint;
                this.Printed = aSrc.Printed;
                this.DontEmail = aSrc.DontEmail;
                this.Emailed = aSrc.Emailed;
                this.BranchID = aSrc.BranchID;
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

                if (aSrc.DontPrint == false)
                    this.DontPrint = false;
                if (aSrc.DontEmail == false)
                    this.DontEmail = false;
                if (aSrc.Printed == false)
                    this.Printed = false;
                if (aSrc.Emailed == false)
                    this.Emailed = false;

            }
            public virtual void ResetToBaseCury(string aBaseCuryID)
            {
                this._CuryID = aBaseCuryID;
                this._CuryStatementBalance = this._StatementBalance;
                this._CuryAgeBalance00 = this._AgeBalance00;
            }
        }
        
		public class DetailKey : AP.Triplet<DateTime, string, Int32>, IEquatable<DetailKey>
		{
			public DetailKey(DateTime aFirst, string aSecond, Int32 aThird)
				: base(aFirst, aSecond, aThird)
			{

			}

			#region IComparable<CashAcctKey> Members


			public override int GetHashCode()
			{
				return (this.first.GetHashCode()) ^ (this.second.GetHashCode()) ^ (this.third.GetHashCode()); //Force to call the CompareTo methods in dicts
			}
			#endregion



			#region IEquatable<DetailKey> Members

			public virtual bool Equals(DetailKey other)
			{
				return base.Equals(other);
			}

			#endregion
		}

		public PXFilter<ARStatementForCustomerParameters> Filter;
        public PXCancel<ARStatementForCustomerParameters> Cancel;
        [PXFilterable]
        public PXSelect<DetailsResult> Details;

		protected readonly CustomerRepository CustomerRepository;

        public ARStatementForCustomer()
        {
            ARSetup setup = ARSetup.Current;

            Details.Cache.AllowDelete = false;
            Details.Cache.AllowInsert = false;
            Details.Cache.AllowUpdate = false;

			CustomerRepository = new CustomerRepository(this);
        }

        public PXSetup<ARSetup> ARSetup;

        protected virtual IEnumerable details()
        {
            ARStatementForCustomerParameters header = Filter.Current;
            Dictionary<DetailKey, DetailsResult> result = new Dictionary<DetailKey, DetailsResult>(EqualityComparer<DetailKey>.Default);
            List<DetailsResult> curyResult = new List<DetailsResult>();

			if (header == null)
            {
                return curyResult;
            }

			Customer customer = CustomerRepository.FindByID(header.CustomerID);

			if (customer != null)
            {
                bool useCurrency = customer.PrintCuryStatements ?? false;
                GL.Company company = PXSelect<GL.Company>.Select(this);

				PXSelectBase<ARStatement> sel = new PXSelect<ARStatement,
					   Where<ARStatement.statementCustomerID, Equal<Required<ARStatement.customerID>>>,
					   OrderBy<Asc<ARStatement.statementCycleId, Asc<ARStatement.statementDate, Asc<ARStatement.curyID>>>>>(this);

				if (header.BranchID != null)
				{
					sel.WhereAnd<Where<ARStatement.branchID, Equal<Required<ARStatement.branchID>>>>();
				}

				foreach (ARStatement st in sel.Select(header.CustomerID, header.BranchID))
				{
                    DetailsResult res = new DetailsResult();
                    res.Copy(st, customer);
                    if (useCurrency)
                    {
                        DetailsResult last = curyResult.Count > 0 ? curyResult[curyResult.Count - 1] : null;
                        if (last != null
                            && last.StatementCycleId == res.StatementCycleId
                            && last.StatementDate == res.StatementDate && last.CuryID == res.CuryID)
                        {
                            last.Append(res);
                        }
                        else
                        {
                            curyResult.Add(res);
                        }
                    }
                    else
                    {
                        DetailKey key = new DetailKey(res.StatementDate.Value, res.StatementCycleId, res.BranchID.Value);
                        res.ResetToBaseCury(company.BaseCuryID);
                        if (!result.ContainsKey(key))
                        {
                            result[key] = res;
                        }
                        else
                        {
                            result[key].Append(res);
                        }
                    }
                }

                return useCurrency ? (curyResult as IEnumerable) : (result.Values as IEnumerable);
            }

			return curyResult;
        }

        #region Sub-screen Navigation Button
        public PXAction<ARStatementForCustomerParameters> printReport;
        [PXUIField(DisplayName = "Print Statement", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable PrintReport(PXAdapter adapter)
        {
            if (this.Details.Current != null && this.Filter.Current != null)
            {
                if (this.Filter.Current.CustomerID.HasValue)
                {
                    Customer customer = PXSelect<
						Customer,
                        Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                        .Select(this, Filter.Current.CustomerID);

					Branch branch = null;
					branch = PXSelect<
						Branch,
						Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
						.Select(this, Filter.Current.BranchID);

					if (customer != null)
                    {
                        Dictionary<string, string> parameters = ARStatementReportParams.FromCustomer(customer);
						parameters[ARStatementReportParams.Parameters.BranchID] = branch?.BranchCD;
						Export(parameters, this.Details.Current);

                        string reportID = ARStatementReportParams.ReportIDForCustomer(this, customer, this.Details.Current.BranchID);

						var reportRequired = PXReportRequiredException.CombineReport(
                            null,
                            reportID,
                            parameters);

						if (reportRequired != null)
						{
							throw reportRequired;
						}
                    }
                }
            }

            return Filter.Select();
        }
		#endregion

		#region Event Handlers
		protected virtual void ARStatementForCustomerParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARStatementForCustomerParameters filter = e.Row as ARStatementForCustomerParameters;

			if (filter == null) return;

			Customer customer = CustomerRepository.FindByID(filter.CustomerID);

			if (customer == null) return;

			bool useCurrency = customer.PrintCuryStatements ?? false;

			PXUIFieldAttribute.SetVisible<DetailsResult.curyID>(Details.Cache, null, useCurrency);
			PXUIFieldAttribute.SetVisible<DetailsResult.curyStatementBalance>(Details.Cache, null, useCurrency);
			PXUIFieldAttribute.SetVisible<DetailsResult.curyOverdueBalance>(Details.Cache, null, useCurrency);
			PXUIFieldAttribute.SetVisible<DetailsResult.statementBalance>(Details.Cache, null, !useCurrency);
			PXUIFieldAttribute.SetVisible<DetailsResult.overdueBalance>(Details.Cache, null, !useCurrency);
		}
		#endregion

        protected static void Export(Dictionary<string, string> aRes, DetailsResult aDetail)
        {
            aRes[ARStatementReportParams.Parameters.StatementCycleID] = aDetail.StatementCycleId;
            aRes[ARStatementReportParams.Parameters.StatementDate] = aDetail.StatementDate.Value.ToString("d", CultureInfo.InvariantCulture);
        }
    }
}