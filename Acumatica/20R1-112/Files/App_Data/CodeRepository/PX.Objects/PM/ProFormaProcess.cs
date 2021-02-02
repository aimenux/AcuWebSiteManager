using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.TM;

namespace PX.Objects.PM
{
	public class ProFormaProcess : PXGraph<ProFormaProcess>
	{
		public PXCancel<ProFormaFilter> Cancel;
		public PXFilter<ProFormaFilter> Filter;
		public PXAction<ProFormaFilter> viewDocumentRef;
		public PXAction<ProFormaFilter> viewDocumentProject;

		[PXFilterable]
		public PXFilteredProcessing<PMProforma, ProFormaFilter> Items; 

		protected bool _ActionChanged = false;
		public virtual void ProFormaFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ActionChanged = !sender.ObjectsEqual<ProFormaFilter.action>(e.Row, e.OldRow);
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocumentRef(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				ProformaEntry docgraph = PXGraph.CreateInstance<ProformaEntry>();
				docgraph.Document.Current = docgraph.Document.Search<PMProforma.refNbr>(Items.Current.RefNbr, Items.Current.ARInvoiceDocType);
				throw new PXRedirectRequiredException(docgraph, true, "Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocumentProject(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(Items.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public virtual IEnumerable items()
		{
			if (Filter.Current.Action == "<SELECT>")
			{
				yield break;
			}

			ProFormaFilter filter = Filter.Current;

			string actionID = Filter.Current.Action;

			if (_ActionChanged)
			{
				Items.Cache.Clear();
			}

			PXSelectBase<PMProforma> cmd;
			switch (actionID)
			{
				case "Release":
					cmd = new PXSelectJoin<PMProforma,
							InnerJoinSingleTable<Customer, On<PMProforma.customerID, Equal<Customer.bAccountID>>>,
						Where<PMProforma.hold, Equal<False>, And<PMProforma.released, Equal<False>, And<PMProforma.status, Equal<ProformaStatus.open>, And<Match<Customer, Current<AccessInfo.userName>>>>>>>(this);
					break;
				default:
					cmd = new PXSelectJoin<PMProforma,
						InnerJoinSingleTable<Customer, On<PMProforma.customerID, Equal<Customer.bAccountID>>>,
						Where<Match<Customer, Current<AccessInfo.userName>>>>(this);
					break;

			}

			cmd.WhereAnd<Where<PMProforma.invoiceDate, LessEqual<Current<ProFormaFilter.endDate>>>>();

			if (filter.BeginDate != null)
			{
				cmd.WhereAnd<Where<PMProforma.invoiceDate, GreaterEqual<Current<ProFormaFilter.beginDate>>>>();	
			}

			if (filter.OwnerID != null)
			{
				cmd.WhereAnd<Where<PMProforma.ownerID, Equal<Current<ProFormaFilter.currentOwnerID>>>>();
			}

			if (filter.WorkGroupID != null)
			{
				cmd.WhereAnd<Where<PMProforma.workgroupID, Equal<Current<ProFormaFilter.workGroupID>>>>();
			}

			if (Filter.Current.ShowAll == true)
			{
				cmd.WhereAnd<Where<PMProforma.hold, Equal <False>>> ();
			}

			int startRow = PXView.StartRow;
			int totalRows = 0;

			foreach (PXResult<PMProforma> res in cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				PMProforma item = res;

				PMProforma cached = (PMProforma)Items.Cache.Locate(item);
				if (cached != null)
					item.Selected = cached.Selected;
				yield return item;

				PXView.StartRow = 0;
			}
		}

		public ProFormaProcess()
		{
          	Items.SetProcessCaption(PM.Messages.Process);
			Items.SetProcessAllCaption(PM.Messages.ProcessAll);

			PXUIFieldAttribute.SetVisible<PMProforma.curyID>(Items.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
		}

		public virtual void ProFormaFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{

			ProFormaFilter filter = e.Row as ProFormaFilter;

			string actionID = Filter.Current.Action;

			Items.SetProcessDelegate<ProformaEntry>(
				   delegate (ProformaEntry engine, PMProforma item)
				   {
					   if (actionID == "Release")
					   {
						   try
						   {
							   engine.Clear();
							   engine.Document.Current = item;
							   engine.ReleaseDocument(item);
						   }
						   catch (Exception ex)
						   {
							   throw new PXSetPropertyException(ex.Message, PXErrorLevel.RowError);
						   }
					   }
				   });
		}

		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class ProFormaFilter : IBqlTable
		{
			#region Action
			public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
			protected string _Action;
			//[PXAutomationMenu]
			[PXString]
			[PMProFormaAction.List]
			[PXDefault(PMProFormaAction.Select, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual string Action
			{
				get
				{
					return this._Action;
				}
				set
				{
					this._Action = value;
				}
			}
			#endregion
			#region CurrentOwnerID
			public abstract class currentOwnerID : PX.Data.BQL.BqlGuid.Field<currentOwnerID> { }
			[PXDBGuid]
			public virtual Guid? CurrentOwnerID
			{
				get
				{
					return PXAccess.GetUserID();
				}
			}
			#endregion
			#region OwnerID
			public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
			protected Guid? _OwnerID;
			[PXDBGuid]
			[PXUIField(DisplayName = "Assigned To")]
			[PX.TM.PXSubordinateOwnerSelector]
			public virtual Guid? OwnerID
			{
				get
				{
					return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
				}
				set
				{
					_OwnerID = value;
				}
			}
			#endregion
			#region WorkGroupID
			public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
			protected Int32? _WorkGroupID;
			[PXDBInt]
			[PXUIField(DisplayName = "Workgroup")]
			[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
				Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
			 SubstituteKey = typeof(EPCompanyTree.description))]
			public virtual Int32? WorkGroupID
			{
				get
				{
					return (_MyWorkGroup == true) ? null : _WorkGroupID;
				}
				set
				{
					_WorkGroupID = value;
				}
			}
			#endregion
			#region MyOwner
			public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
			protected Boolean? _MyOwner;
			[PXDBBool()]
			[PXUIField(DisplayName = "Me")]
			[PXDefault(false)]
			public virtual Boolean? MyOwner
			{
				get
				{
					return this._MyOwner;
				}
				set
				{
					this._MyOwner = value;
				}
			}
			#endregion
			#region MyWorkGroup
			public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
			protected Boolean? _MyWorkGroup;
			[PXDBBool()]
			[PXUIField(DisplayName = "My")]
			[PXDefault(false)]
			public virtual Boolean? MyWorkGroup
			{
				get
				{
					return this._MyWorkGroup;
				}
				set
				{
					this._MyWorkGroup = value;
				}
			}
			#endregion
			#region ShowAll
			public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
			protected Boolean? _ShowAll;
			[PXDBBool()]
			[PXUIField(DisplayName = "Show All")]
			[PXDefault(false)]
			public virtual Boolean? ShowAll
			{
				get
				{
					return this._ShowAll;
				}
				set
				{
					this._ShowAll = value;
				}
			}
			#endregion
			#region BeginDate
			public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }
			protected DateTime? _BeginDate;
			[PXDate()]
			[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
			[PXUnboundDefault]
			public virtual DateTime? BeginDate
			{
				get
				{
					return this._BeginDate;
				}
				set
				{
					this._BeginDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			protected DateTime? _EndDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public static class PMProFormaAction
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Select, Release },
					new string[] { Messages.PMProForma_Select, Messages.PMProForma_Release })
				{; }
			}

			public const string Select = "<SELECT>";
			public const string Release = "Release";

			public class select : PX.Data.BQL.BqlString.Constant<select>
			{
				public select() : base(Select) {; }
			}

			public class release : PX.Data.BQL.BqlString.Constant<release>
			{
				public release() : base(Release) {; }
			}

		}

	}
}