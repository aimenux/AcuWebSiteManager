using System;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class OpportunityFilter : IBqlTable, IClassIdFilter
	{
		#region CloseDate

		public abstract class closeDate : PX.Data.BQL.BqlDateTime.Field<closeDate> { }

		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXDBDateAndTime]
		[PXUIField(DisplayName = "Estimation", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual DateTime? CloseDate { get; set; }

		#endregion

		#region Subject

		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDefault]
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual String Subject { get; set; }

		#endregion

		#region OpportunityClass

		public abstract class opportunityClass : PX.Data.BQL.BqlString.Field<opportunityClass> { }

		[PXDefault]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Opportunity Class")]
		[PXSelector(typeof(CROpportunityClass.cROpportunityClassID),
			DescriptionField = typeof(CROpportunityClass.description))]
		public virtual string OpportunityClass { get; set; }

		string IClassIdFilter.ClassID => OpportunityClass;

		#endregion
	}
}