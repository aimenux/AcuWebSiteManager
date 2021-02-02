namespace PX.Objects.SO
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.IN;
	using PX.Objects.AR;

	public class SOPostOrder : PXGraph<SOPostOrder>
	{
		public PXCancel<SOPostShipmentFilter> Cancel;
		public PXFilter<SOPostShipmentFilter> Filter;
		public PXFilteredProcessing<SOOrder, SOPostShipmentFilter> Orders;

		public SOPostOrder()
		{
			Orders.SetSelected<SOShipment.selected>();
			Orders.SetProcessCaption(Messages.Process);
			Orders.SetProcessAllCaption(Messages.ProcessAll);
		}

		public virtual void SOPostShipmentFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOPostShipmentFilter filter = e.Row as SOPostShipmentFilter;

			PXProcessingStep[] targets = PXAutomation.GetProcessingSteps(this);
			if (targets.Length > 0)
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(filter);
				Orders.SetProcessTarget(targets[0].GraphName, targets[0].Name, targets[0].Actions[0].Name, targets[0].Actions[0].Menus[0], null, parameters);
			}
			else
			{
				Orders.SetProcessDelegate(delegate(List<SOOrder> list)
				{
					PostOrder(filter, list);
				});
			}
		}

		public virtual IEnumerable orders()
		{
			foreach (SOOrder order in Orders.Cache.Updated)
			{
				yield return order;
			}

			foreach (PXResult res in PXSelectJoinGroupBy<SOOrder,
			InnerJoin<SOLine, 
				On<SOLine.orderType, Equal<SOOrder.orderType>, And<SOLine.orderNbr, Equal<SOOrder.orderNbr>>>,
			InnerJoin<SOOrderType, 
				On<SOOrder.FK.OrderType>,
			InnerJoin<SOOrderTypeOperation, 
				On<SOLine.FK.OrderTypeOperation>,
			LeftJoin<ARTran, On<ARTran.sOOrderType, Equal<SOLine.orderType>, And<ARTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<ARTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>,
			LeftJoin<INTran, On<INTran.sOOrderType, Equal<SOLine.orderType>, And<INTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<INTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>>>>>>,
			Where<SOOrder.hold, Equal<boolFalse>,
			And<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.noUpdate>, 
			And2<Where<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>, Or<ARTran.refNbr, IsNotNull>>,
			And<INTran.refNbr, IsNull>>>>,
			Aggregate<GroupBy<SOOrder.orderType, GroupBy<SOOrder.orderNbr>>>>.Select(this))
			{
				SOOrder order;
				if ((order = (SOOrder)Orders.Cache.Locate(res[typeof(SOOrder)])) == null || Orders.Cache.GetStatus(order) == PXEntryStatus.Notchanged)
				{
					yield return res[typeof(SOOrder)];
				}
			}

			Orders.Cache.IsDirty = false;
		}

		public static void PostOrder(SOPostShipmentFilter filter, List<SOOrder> list)
		{
			SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
			DocumentList<INRegister> created = new DocumentList<INRegister>(docgraph);
			INIssueEntry ie = PXGraph.CreateInstance<INIssueEntry>();
			ie.FieldVerifying.AddHandler<INTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			ie.FieldVerifying.AddHandler<INTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			foreach (SOOrder order in list)
			{
				docgraph.PostOrder(ie, order, created);
			}
		}
	}

    [Serializable]
	public partial class SOPostShipmentFilter : IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr))]
		[PXDefault()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
	}

}
