using PX.Data;
using PX.Data.BQL.Fluent;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRAcaEmployeeMonthlyInformation)]
	public class PRAcaEmployeeMonthlyInformation : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Obsolete]
		[PXDBInt]
		public virtual int? BranchID { get; set; }
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false)]
		[PXDBDefault(typeof(PRAcaCompanyYearlyInformation.orgBAccountID))]
		public virtual int? OrgBAccountID { get; set; }
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		[PXDBString(4, IsKey = true)]
		[PXUIField(DisplayName = Messages.Year)]
		[PXParent(typeof(
			Select<PRAcaCompanyYearlyInformation,
				Where<PRAcaCompanyYearlyInformation.year, Equal<Current<PRAcaEmployeeMonthlyInformation.year>>,
				And<PRAcaCompanyYearlyInformation.orgBAccountID, Equal<Current<PRAcaEmployeeMonthlyInformation.orgBAccountID>>>>>))]
		[PXDBDefault(typeof(PRAcaCompanyYearlyInformation.year))]
		public virtual string Year { get; set; }
		#endregion
		#region Month
		public abstract class month : PX.Data.BQL.BqlInt.Field<month> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = Messages.Month, Enabled = false)]
		[Month.List]
		public virtual int? Month { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee ID")]
		public virtual int? EmployeeID { get; set; }
		#endregion
		#region FTStatus
		public abstract class ftStatus : PX.Data.BQL.BqlString.Field<ftStatus> { }
		[PXDBString(3)]
		[PXUIField(DisplayName = "ACA FT Status")]
		[AcaFTStatus.List]
		public virtual string FTStatus { get; set; }
		#endregion
		#region OfferOfCoverage
		public abstract class offerOfCoverage : PX.Data.BQL.BqlString.Field<offerOfCoverage> { }
		[PXDBString(2)]
		[PXUIField(DisplayName = "Offer of Coverage")]
		[AcaOfferOfCoverage.List]
		public virtual string OfferOfCoverage { get; set; }
		#endregion
		#region Section4980H
		public abstract class section4980H : PX.Data.BQL.BqlString.Field<section4980H> { }
		[PXDBString(2)]
		[PXUIField(DisplayName = "Section 4980H")]
		[AcaSection4980H.List]
		public virtual string Section4980H { get; set; }
		#endregion
		#region MinimumIndividualContribution
		public abstract class minimumIndividualContribution : PX.Data.BQL.BqlDecimal.Field<minimumIndividualContribution> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Minimum Individual Contribution")]
		public virtual decimal? MinimumIndividualContribution { get; set; }
		#endregion
		#region HoursWorked
		public abstract class hoursWorked : PX.Data.BQL.BqlDecimal.Field<hoursWorked> { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Number of Hours Worked", Enabled = false)]
		public virtual decimal? HoursWorked { get; set; }
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
