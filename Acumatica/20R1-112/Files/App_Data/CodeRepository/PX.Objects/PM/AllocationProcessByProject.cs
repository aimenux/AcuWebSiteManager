using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	public class AllocationProcessByProject : PXGraph<AllocationProcessByProject>
	{
		public PXCancel<AllocationFilter> Cancel;
		public PXFilter<AllocationFilter> Filter;

		//Current implementation (analysing the allocation steps - can be slow under certain conditions):
		//public PXFilteredProcessingJoin<
		//	PMProject, AllocationFilter, InnerJoin<PMProjectForAllocation, On<PMProject.contractID, Equal<PMProjectForAllocation.contractID>>>,
		//	Where2<
		//	Where<Current<AllocationFilter.allocationID>, IsNull, Or<PMProject.allocationID, Equal<Current<AllocationFilter.allocationID>>>>,
		//	And2<Where<Current<AllocationFilter.projectID>, IsNull, Or<PMProject.contractID, Equal<Current<AllocationFilter.projectID>>>>,
		//	And2<Where<Current<AllocationFilter.customerID>, IsNull, Or<PMProject.customerID, Equal<Current<AllocationFilter.customerID>>>>,
		//	And2<Where<Current<AllocationFilter.customerClassID>, IsNull, Or<PMProjectForAllocation.customerClassID, Equal<Current<AllocationFilter.customerClassID>>>>,
		//	And<Match<Current<AccessInfo.userName>>>>
		//	>>>> Items;

		//All projects (displays all projects - should be used as customization if Current Implementation (above) is too slow for current configuration:
		public PXFilteredProcessingJoin<PMProject, AllocationFilter, LeftJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>>,
		Where2<
		Where<Current<AllocationFilter.allocationID>, IsNull, Or<PMProject.allocationID, Equal<Current<AllocationFilter.allocationID>>>>,
		And2<Where<Current<AllocationFilter.projectID>, IsNull, Or<PMProject.contractID, Equal<Current<AllocationFilter.projectID>>>>,
		And2<Where<Current<AllocationFilter.customerID>, IsNull, Or<PMProject.customerID, Equal<Current<AllocationFilter.customerID>>>>,
		And2<Where<Current<AllocationFilter.customerClassID>, IsNull, Or<Customer.customerClassID, Equal<Current<AllocationFilter.customerClassID>>>>,
		And2<Where<Current<AllocationFilter.customerClassID>, IsNull, Or<Customer.customerClassID, Equal<Current<AllocationFilter.customerClassID>>>>,
		And2<Match<Current<AccessInfo.userName>>,
		And<PMProject.nonProject, Equal<False>,
		And<PMProject.baseType, Equal<CT.CTPRType.project>>>>>
		>>>>> Items;

        public AllocationProcessByProject()
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
			graph.Project.Current = PXSelect<PMProject, Where<PMProject.contractCD, Equal<Current<PMProject.contractCD>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewProject) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
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
					delegate(PMAllocator engine, PMProject item)
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

		public static void Run(PMAllocator graph, PMProject item, DateTime? date, DateTime? fromDate, DateTime? toDate)
		{
			PXSelectBase<PMTask> select = new PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
				And<PMTask.allocationID, IsNotNull,
				And<PMTask.isActive, Equal<True>>>>>(graph);

			List<PMTask> tasks = new List<PMTask>();
			foreach (PMTask pmTask in select.Select(item.ContractID))
			{
				tasks.Add(pmTask);
			}

			graph.Clear();
			graph.FilterStartDate = fromDate;
		    graph.FilterEndDate = toDate;
			graph.PostingDate = date;
			graph.Execute(tasks);
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
			[PXSelector(typeof(AR.CustomerClass.customerClassID), DescriptionField = typeof(AR.CustomerClass.descr), CacheGlobal = true)]
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
			[AR.Customer()]
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

		[PXHidden]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class PMAccountGroupFrom : PMAccountGroup
		{
			#region GroupID
			public new abstract class groupID : PX.Data.BQL.BqlInt.Field<groupID> { }
			#endregion
			#region GroupCD
			public new abstract class groupCD : PX.Data.BQL.BqlString.Field<groupCD> { }
			#endregion
		}

		[PXHidden]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class PMAccountGroupTo : PMAccountGroup
		{
			#region GroupID
			public new abstract class groupID : PX.Data.BQL.BqlInt.Field<groupID> { }
			#endregion
			#region GroupCD
			public new abstract class groupCD : PX.Data.BQL.BqlString.Field<groupCD> { }
			#endregion
		}

		[PXHidden]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class Customer : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			protected Int32? _BAccountID;
			[PXDBInt()]
			public virtual Int32? BAccountID
			{
				get
				{
					return this._BAccountID;
				}
				set
				{
					this._BAccountID = value;
				}
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true)]
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
		}

		[PXHidden]
		[Serializable]
		[PXProjection(typeof(
			Select5<PMProject
			, InnerJoin<PMTask, On<PMProject.contractID, Equal<PMTask.projectID>, And<Where<PMTask.status, Equal<ProjectTaskStatus.active>, Or<PMTask.status, Equal<ProjectTaskStatus.completed>>>>>
				, InnerJoin<PMAllocation, On<PMAllocation.allocationID, Equal<PMTask.allocationID>, And<PMAllocation.isActive, Equal<True>>>
					, InnerJoin<PMAllocationDetail, On<PMAllocationDetail.allocationID, Equal<PMAllocation.allocationID>, And<PMAllocationDetail.selectOption, Equal<PMSelectOption.transaction>>>
						, LeftJoin<PMAccountGroupFrom, On<PMAccountGroupFrom.groupID, Equal<PMAllocationDetail.accountGroupFrom>>
							, LeftJoin<PMAccountGroupTo, On<PMAccountGroupTo.groupID, Equal<PMAllocationDetail.accountGroupTo>>
								, LeftJoin<PMAccountGroup, On<PMAccountGroup.groupCD, Equal<PMAccountGroupFrom.groupCD>, Or<PMAccountGroup.groupCD, Equal<PMAccountGroupTo.groupCD>, Or<PMAccountGroup.groupCD, Between<PMAccountGroupFrom.groupCD, PMAccountGroupTo.groupCD>>>>
									, LeftJoin<PMTran, On<PMTran.projectID, Equal<PMProject.contractID>, And<PMTran.taskID, Equal<PMTask.taskID>, And<PMTran.allocated, Equal<False>, And<PMTran.excludedFromAllocation, Equal<False>, And<PMTran.released, Equal<True>, And<PMTran.accountGroupID, Equal<PMAccountGroup.groupID>>>>>>>
										, LeftJoin<AllocationProcessByProject.Customer, On<PMProject.customerID, Equal<AllocationProcessByProject.Customer.bAccountID>>>
										>
									>
								>
							>
						>
					>
				>
			, Where<PMProject.baseType, Equal<CT.CTPRType.project>
					, And<PMProject.isActive, Equal<True>
						, And<PMProject.nonProject, Equal<False>
							, And2<Where<PMProject.status, Equal<ProjectStatus.active>, Or<PMProject.status, Equal<ProjectStatus.completed>>>
								, And<PMAllocationDetail.method, Equal<PMMethod.budget>, Or<PMTran.tranID, IsNotNull>>
								>
							>
						>
					>
			, Aggregate<
				GroupBy<PMProject.contractID
					, GroupBy<PMProject.contractCD
						, GroupBy<PMProject.description
							, GroupBy<PMProject.allocationID
								, GroupBy<PMProject.customerID
									, GroupBy<AllocationProcessByProject.Customer.customerClassID>
									>
								>
							>
						>
					>
				>
			>
			), Persistent = false)]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class PMProjectForAllocation : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXUnboundDefault(false)]
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

			#region ContractID
			public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
			protected Int32? _ContractID;
			[PXDBInt(IsKey = true, BqlField = typeof(PMProject.contractID))]
			public virtual Int32? ContractID
			{
				get
				{
					return this._ContractID;
				}
				set
				{
					this._ContractID = value;
				}
			}
			#endregion
			#region ContractCD
			public abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
			protected String _ContractCD;
			[PXDBString(IsUnicode = true, InputMask = "", BqlField = typeof(PMProject.contractCD))]
			[PXDefault()]
			[PXUIField(DisplayName = "Project ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PX.Data.EP.PXFieldDescription]
			public virtual String ContractCD
			{
				get
				{
					return this._ContractCD;
				}
				set
				{
					this._ContractCD = value;
				}
			}
			#endregion
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected String _Description;
			[PXDBString(60, IsUnicode = true, BqlField = typeof(PMProject.description))]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Description
			{
				get
				{
					return _Description;
				}
				set
				{
					_Description = value;
				}
			}
			#endregion
			#region AllocationID
			public abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID> { }
			private String _AllocationID;
			[PXSelector(typeof(Search<PMAllocation.allocationID, Where<PMAllocation.isActive, Equal<True>>>))]
			[PXUIField(DisplayName = "Allocation Rule")]
			[PXDBString(PMAllocation.allocationID.Length, IsUnicode = true, BqlField = typeof(PMProject.allocationID))]
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
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[CustomerActive(DescriptionField = typeof(AR.Customer.acctName), BqlField = typeof(PMProject.customerID))]
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
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true, BqlField = typeof(AllocationProcessByProject.Customer.customerClassID))]
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

		}
	}
}
