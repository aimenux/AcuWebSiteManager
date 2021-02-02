using System;
using System.Collections;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	public class AllocationProcess : PXGraph<AllocationProcess>
	{
		[PXSelector(typeof(Search<PMAllocation.allocationID, Where<PMAllocation.isActive, Equal<True>>>), DescriptionField = typeof(PMAllocation.description))]
		[PXUIField(DisplayName = "Allocation Rule")]
		[PXDBString(15, IsUnicode = true)]
		protected virtual void _(Events.CacheAttached<PMTask.allocationID> e)
		{ }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Project")]
		[PXSelector(typeof(PMProject.contractID), SubstituteKey=typeof(PMProject.contractCD))]
		protected virtual void _(Events.CacheAttached<PMTask.projectID> e){}


		public PXCancel<AllocationFilter> Cancel;
		public PXFilter<AllocationFilter> Filter;
		
		public PXFilteredProcessingJoin<PMTask,
			AllocationFilter,
			InnerJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>,
			LeftJoin<Customer, On<PMTask.customerID, Equal<Customer.bAccountID>>>>,
			Where<PMTask.isActive, Equal<True>,
			And<PMProject.baseType, Equal<CT.CTPRType.project>,
			And<PMTask.allocationID, IsNotNull,
			And2<Where<Current<AllocationFilter.allocationID>, IsNull, Or<PMTask.allocationID, Equal<Current<AllocationFilter.allocationID>>>>,
			And2<Where<Current<AllocationFilter.projectID>, IsNull, Or<PMTask.projectID, Equal<Current<AllocationFilter.projectID>>>>,
			And2<Where<Current<AllocationFilter.taskID>, IsNull, Or<PMTask.taskID, Equal<Current<AllocationFilter.taskID>>>>,
			And2<Where<Current<AllocationFilter.customerID>, IsNull, Or<PMTask.customerID, Equal<Current<AllocationFilter.customerID>>>>,
			And2<Where<Current<AllocationFilter.customerClassID>, IsNull, Or<Customer.customerClassID, Equal<Current<AllocationFilter.customerClassID>>>>,
			And<CurrentMatch<PMProject, AccessInfo.userName>>>>>>>>>>> Items;

        public AllocationProcess()
		{
			Items.SetProcessCaption(PM.Messages.ProcAllocate);
			Items.SetProcessAllCaption(PM.Messages.ProcAllocateAll);
		}

		public PXAction<AllocationFilter> viewProject;
		[PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProject(PXAdapter adapter)
		{
			ProjectEntry graph = CreateInstance<ProjectEntry>();
			graph.Project.Current = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewProject) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<AllocationFilter> viewTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMTask.taskID>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		#region EventHandlers
		protected virtual void AllocationFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (!cache.ObjectsEqual<AllocationFilter.date, AllocationFilter.allocationID, AllocationFilter.customerClassID, AllocationFilter.customerID, AllocationFilter.projectID, AllocationFilter.taskID>(e.Row, e.OldRow))
				Items.Cache.Clear();
		}
		protected virtual void AllocationFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			AllocationFilter filter = Filter.Current;

			Items.SetProcessDelegate<PMAllocator>(
					delegate(PMAllocator engine, PMTask item)
					{
						Run(engine, item, filter.Date, filter.DateFrom, filter.DateTo);
					});
		}
        #endregion

        private PMSetup setup;

		public bool AutoReleaseAllocation
		{
			get
			{
				if (setup == null)
				{
					setup = PXSelect<PMSetup>.Select(this);
				}

				return setup.AutoReleaseAllocation == true;
			}
		}

		public static void Run(PMAllocator graph, PMTask item, DateTime? date, DateTime? fromDate, DateTime? toDate)
		{
			graph.Clear();
			graph.FilterStartDate = fromDate;
		    graph.FilterEndDate = toDate;
			graph.PostingDate = date;
			graph.Execute(item);
			if (graph.Document.Current != null)
			{
                graph.Actions.PressSave();
				PMSetup setup = PXSelect<PMSetup>.Select(graph);
				PMRegister doc = graph.Caches[typeof(PMRegister)].Current as PMRegister;
				if (doc != null && setup.AutoReleaseAllocation == true)
				{
					RegisterRelease.Release(doc);
				}

			}
			else
			{
				throw new PXSetPropertyException(Warnings.NothingToAllocate, PXErrorLevel.RowWarning);
			}

		}

		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class AllocationFilter : IBqlTable
		{
			#region Date
			public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
			protected DateTime? _Date;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Posting Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? Date
			{
				get
				{
					return this._Date;
				}
				set
				{
					this._Date = value;
				}
			}
			#endregion
			#region AllocationID
			public abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID> { }
			protected String _AllocationID;
			[PXSelector(typeof(PMAllocation.allocationID), DescriptionField = typeof(PMAllocation.description))]
			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Allocation Rule")]
			public virtual String AllocationID
			{
				get
				{
					return this._AllocationID;
				}
				set
				{
					this._AllocationID = value;
				}
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Customer Class")]
			public virtual String CustomerClassID
			{
				get
				{
					return this._CustomerClassID;
				}
				set
				{
					this._CustomerClassID = value;
				}
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[Customer()]
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
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			protected Int32? _ProjectID;
			[Project()]
			public virtual Int32? ProjectID
			{
				get
				{
					return this._ProjectID;
				}
				set
				{
					this._ProjectID = value;
				}
			}
			#endregion
			#region TaskID
			public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
			protected Int32? _TaskID;
			[ProjectTask(typeof(AllocationFilter.projectID))]
			public virtual Int32? TaskID
			{
				get
				{
					return this._TaskID;
				}
				set
				{
					this._TaskID = value;
				}
			}
			#endregion
            #region DateFrom
            public abstract class dateFrom : PX.Data.BQL.BqlDateTime.Field<dateFrom> { }
            protected DateTime? _DateFrom;
            [PXDBDate()]
            [PXUIField(DisplayName = "From", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? DateFrom
            {
                get
                {
                    return this._DateFrom;
                }
                set
                {
                    this._DateFrom = value;
                }
            }
            #endregion
            #region DateTo
            public abstract class dateTo : PX.Data.BQL.BqlDateTime.Field<dateTo> { }
            protected DateTime? _DateTo;
            [PXDBDate()]
            [PXUIField(DisplayName = "To", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? DateTo
            {
                get
                {
                    return this._DateTo;
                }
                set
                {
                    this._DateTo = value;
                }
            }
            #endregion

		}
	}
}
