using PX.Data;
using PX.Data.BQL;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.CN
{
	[PXCacheName("Report Information")]
	public class ReportInformation : IBqlTable
	{
		[AnyInventory(typeof(Search<InventoryItem.inventoryID, Where2<Match<Current<AccessInfo.userName>>,
				And<InventoryItem.stkItem, Equal<False>,
					And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
						And<Where<InventoryItem.itemClassID, Equal<Optional<FilterItemByClass.itemClassID>>,
							Or<Optional<FilterItemByClass.itemClassID>, IsNull>>>>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
		public virtual int? InventoryIdNonStock
		{
			get;
			set;
		}

		[PXInt]
		[PXSelector(typeof(Search<PMProject.contractID,
				Where<PMProject.status, In3<ProjectStatus.active, planned>>>),
			typeof(PMProject.contractCD),
			typeof(PMProject.description),
			typeof(PMProject.status),
			typeof(PMProject.approverID),
			SubstituteKey = typeof(PMProject.contractCD))]
		public virtual int? ProjectId
		{
			get;
			set;
		}

		[PXInt]
		[PXSelector(typeof(Search5<EPEmployee.bAccountID,
				LeftJoin<PMProject, On<PMProject.approverID, Equal<EPEmployee.bAccountID>>>,
				Where<PMProject.contractID, Equal<Optional<projectId>>,
					Or<Optional<projectId>, IsNull>>,
				Aggregate<GroupBy<EPEmployee.bAccountID>>>),
			SubstituteKey = typeof(EPEmployee.acctCD))]
		public virtual int? ProjectManagerId
		{
			get;
			set;
		}

		[Project(typeof(Where<PMProject.baseType, Equal<CTPRType.project>>))]
		public virtual int? BudgetForecastProjectId
		{
			get;
			set;
		}

		[PXString]
		[PXSelector(typeof(Search<PMForecast.revisionID,
			Where<PMForecast.projectID, Equal<Optional<budgetForecastProjectId>>>,
			OrderBy<Desc<PMForecast.revisionID>>>))]
		public virtual string RevisionId
		{
			get;
			set;
		}

		public abstract class projectId : IBqlField
		{
		}

		public abstract class projectManagerId : IBqlField
		{
		}

		public abstract class budgetForecastProjectId : BqlInt.Field<budgetForecastProjectId>
		{
		}

		public abstract class revisionId : BqlString.Field<revisionId>
		{
		}

		public class planned : BqlType<IBqlString, string>.Constant<planned>
		{
			public planned()
				: base(ProjectStatus.Planned)
			{
			}
		}
	}
}