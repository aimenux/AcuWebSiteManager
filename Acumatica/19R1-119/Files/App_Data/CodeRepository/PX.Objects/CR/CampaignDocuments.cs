using System;
using System.Collections;
using PX.SM;
using PX.Data;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.CR
{
	public class CampaignDocuments : PXGraph<CampaignDocuments>
	{
		/*
		#region DACs

		[PXBreakInheritance]
		[Serializable]
		[PXProjection(typeof(Select<SOOrder>))]
		public partial class SOOrderAlias : IBqlTable
		{
			#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			protected String _OrderType;
			[PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">aa", BqlTable=typeof(SOOrder))]
			[PXDefault(SOOrderTypeConstants.SalesOrder, typeof(SOSetup.defaultOrderType))]
			[PXSelector(typeof(Search5<SOOrderType.orderType,
				InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
					LeftJoin<SOSetupApproval, On<SOOrderType.orderType, Equal<SOSetupApproval.orderType>>>>,
				Aggregate<GroupBy<SOOrderType.orderType>>>))]
			[PXRestrictor(typeof(Where<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, Or<FeatureInstalled<FeaturesSet.warehouse>>>), ErrorMessages.ElementDoesntExist, typeof(SOOrderType.orderType))]
			[PXRestrictor(typeof(Where<SOOrderType.requireAllocation, NotEqual<True>, Or<AllocationAllowed>>), ErrorMessages.ElementDoesntExist, typeof(SOOrderType.orderType))]
			[PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), null)]
			[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
			[PX.Data.EP.PXFieldDescription]
			public virtual String OrderType
			{
				get
				{
					return this._OrderType;
				}
				set
				{
					this._OrderType = value;
				}
			}
			#endregion		
			#region OrderNbr
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			protected String _OrderNbr;
			[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlTable = typeof(SOOrder))]
			[PXDefault()]
			[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
			[SO.SO.RefNbr(typeof(Search2<SOOrder.orderNbr,
				LeftJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>,
					And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
				Where<SOOrder.orderType, Equal<Optional<SOOrderAlias.orderType>>,
					And<Where<SOOrder.orderType, Equal<SOOrderTypeConstants.transferOrder>,
						Or<Customer.bAccountID, IsNotNull>>>>,
				OrderBy<Desc<SOOrder.orderNbr>>>), Filterable = true)]			
			[PX.Data.EP.PXFieldDescription]
			public virtual String OrderNbr
			{
				get
				{
					return this._OrderNbr;
				}
				set
				{
					this._OrderNbr = value;
				}
			}
			#endregion
			#region CampaignID
			public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }

			protected string _CampaignID;
			[PXDBString(10, IsUnicode = true, BqlTable = typeof(SOOrder))]
			[PXUIField(DisplayName = CR.Messages.Campaign, Visibility = PXUIVisibility.SelectorVisible, FieldClass = "CRM")]
			[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
			public virtual string CampaignID
			{
				get
				{
					return this._CampaignID;
				}
				set
				{
					this._CampaignID = value;
				}
			}
			#endregion
			#region OrigCampaignID
			public abstract class origCampaignID : PX.Data.BQL.BqlString.Field<origCampaignID> { }

			protected string _OrigCampaignID;
			[PXString(10, IsUnicode = true)]
			[PXDBCalced(typeof(SOOrder.campaignID), typeof(string))]
			[PXFormula(typeof(Selector<SOOrderAlias.orderNbr, SOOrder.campaignID>))]
			public virtual string OrigCampaignID
			{
				get
				{
					return this._OrigCampaignID;
				}
				set
				{
					this._OrigCampaignID = value;
				}
			}
			#endregion
		}

		[PXBreakInheritance]
		[Serializable]
		[PXProjection(typeof(Select<ARInvoice>))]
		public partial class ARInvoiceAlias : IBqlTable
		{
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

			[PXDBString(3, IsKey = true, IsFixed = true, BqlTable = typeof(ARInvoice))]
			[PXDefault(typeof(ARInvoiceType.invoice))]
			[ARInvoiceType.List()]
			[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]			
			public virtual String DocType
			{
				get; set;
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

			[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlTable = typeof(ARInvoice))]
			[PXDefault()]
			[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
			[ARInvoiceType.RefNbr(typeof(Search2<ARInvoice.refNbr,
					InnerJoinSingleTable<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
				Where<ARInvoice.docType, Equal<Optional<ARInvoiceAlias.docType>>,
					And2<Where<ARInvoice.origModule, Equal<BatchModule.moduleAR>,
							Or<ARInvoice.released, Equal<True>>>,
						And<Match<Customer, Current<AccessInfo.userName>>>>>,
				OrderBy<Desc<ARInvoice.refNbr>>>), Filterable = true, IsPrimaryViewCompatible = true)]
			[ARInvoiceNbr()]			
			public virtual String RefNbr
			{
				get;
				set;
			}
			#endregion
			#region CampaignID
			public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }

			[PXDBString(10, IsUnicode = true, BqlTable=typeof(ARInvoice))]
			[PXUIField(DisplayName = CR.Messages.Campaign, Visibility = PXUIVisibility.SelectorVisible, FieldClass = FeaturesSet.customerModule.FieldClass)]
			[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
			public virtual string CampaignID
			{
				get;
				set;				
			}
			#endregion
			#region OrigCampaignID
			public abstract class origCampaignID : PX.Data.BQL.BqlString.Field<origCampaignID> { }

			protected string _OrigCampaignID;
			[PXString(10, IsUnicode = true)]
			[PXDBCalced(typeof(ARInvoice.campaignID), typeof(string))]
			[PXFormula(typeof(Selector<ARInvoiceAlias.refNbr, ARInvoice.campaignID>))]
			public virtual string OrigCampaignID
			{
				get
				{
					return this._OrigCampaignID;
				}
				set
				{
					this._OrigCampaignID = value;
				}
			}
			#endregion
		}


		[Serializable]
		public partial class Filter : IBqlTable
		{
			#region DistFeatureInstalled
			public abstract class distFeatureInstalled : PX.Data.BQL.BqlBool.Field<distFeatureInstalled> { }

			[PXBool]
			[PXUIField(DisplayName = "Distribution Feature Enabled", Visible = false, Enabled = false)]
			public virtual bool? DistFeatureInstalled
			{
				get
				{
					return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
				}
			}
			#endregion
		}

		#endregion

		#region Views

		[PXViewName(Messages.Campaign)]
		public PXSelect<CRCampaign>
			Campaign;

		[PXHidden]
		public PXSelect<SOOrder>
			Order;
		[PXHidden]
		public PXSelect<ARInvoice>
			Invoice;

		[PXViewName(Messages.ViewSalesOrder)]
		[PXViewDetailsButton(typeof(CRCampaign), OnClosingPopup = PXSpecialButtonType.Refresh)]
		public PXSelectJoin<SOOrderAlias,
			LeftJoin<SOOrder,
				On<SOOrder.orderType, Equal<SOOrderAlias.orderType>,
				And<SOOrder.orderNbr, Equal<SOOrderAlias.orderNbr>>>>,
			Where<SOOrderAlias.campaignID, Equal<Current<CRCampaign.campaignID>>>>
			SalesOrders;
		
		[PXViewDetailsButton(typeof(CRCampaign), OnClosingPopup = PXSpecialButtonType.Refresh)]
		public PXSelectJoin<ARInvoiceAlias,
				LeftJoin<ARInvoice,
					On<ARInvoice.docType, Equal<ARInvoiceAlias.docType>,
						And<ARInvoice.refNbr, Equal<ARInvoiceAlias.refNbr>>>>,
				Where<ARInvoiceAlias.campaignID, Equal<Current<CRCampaign.campaignID>>>>
			ARInvoices;

		public PXFilter<Filter>
			FilterView;

		#region Hidden

		

		[PXHidden]
		public PXSelectJoin<SOOrderAlias,
				LeftJoin<SOOrder,
					On<SOOrder.orderType, Equal<SOOrderAlias.orderType>,
						And<SOOrder.orderNbr, Equal<SOOrderAlias.orderNbr>>>>,
				Where<SOOrderAlias.campaignID, Equal<Current<CRCampaign.campaignID>>>>
			SalesOrders_Select;

		[PXHidden]
		public PXSelectJoin<ARInvoiceAlias,
				LeftJoin<ARInvoice,
					On<ARInvoice.docType, Equal<ARInvoiceAlias.docType>,
						And<ARInvoice.refNbr, Equal<ARInvoiceAlias.refNbr>>>>,
				Where<ARInvoiceAlias.campaignID, Equal<Current<CRCampaign.campaignID>>>>
			ARInvoices_Select;

		#endregion

		#endregion

		#region Delegates 

		protected virtual IEnumerable salesOrders()
		{
			PXResultset<SOOrderAlias, SOOrder> ret = new PXResultset<SOOrderAlias, SOOrder>();

			int startRow = PXView.StartRow;
			int totalRows = 0;			
			foreach (PXResult<SOOrderAlias,SOOrder> rec in SalesOrders_Select.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				SOOrderAlias alias = rec;
				SOOrder order = rec;				
				alias.CampaignID = Campaign.Current?.CampaignID;				
				ret.Add(new PXResult<SOOrderAlias, SOOrder>(alias, order));
			}			

			PXView.StartRow = 0;
			PXView.SortClear();			
			return ret;
		}

		protected virtual IEnumerable aRInvoices()
		{
			PXResultset<ARInvoiceAlias, ARInvoice> ret = new PXResultset<ARInvoiceAlias, ARInvoice>();

			int startRow = PXView.StartRow;
			int totalRows = 0;
			foreach (PXResult<ARInvoiceAlias,ARInvoice> rec in ARInvoices_Select.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				ARInvoiceAlias alias = rec;
				ARInvoice invoice = rec;
				alias.CampaignID = Campaign.Current?.CampaignID;
				ret.Add(new PXResult<ARInvoiceAlias, ARInvoice>(alias, invoice));
			}			
			PXView.StartRow = 0;
			PXView.SortClear();
			return ret;
		}
		
		#endregion

		#region Actions

		public PXSave<CRCampaign> Save;
		public PXCancel<CRCampaign> Cancel;

		#endregion
		
		#region Events
				
		protected virtual void SOOrderAlias_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.SetValue<SOOrderAlias.campaignID>(e.Row, Campaign.Current?.CampaignID);
			sender.SetStatus(e.Row, PXEntryStatus.Inserted);
		}

		protected virtual void SOOrderAlias_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOOrderAlias row = e.Row as SOOrderAlias;
			if (row?.OrigCampaignID != null && row?.OrigCampaignID != row.CampaignID)
			{
				var oldCampaign = new PXSelect<CRCampaign,
					Where<CRCampaign.campaignID, Equal<Required<ARInvoice.campaignID>>>>(this).SelectSingle(row.OrigCampaignID);
				if (oldCampaign != null)
					this.Caches[typeof(SOOrderAlias)].RaiseExceptionHandling<SOOrderAlias.orderNbr>(row, null,
					new PXSetPropertyException(
						PXMessages.LocalizeFormat(Messages.CampaignLinkWarning, oldCampaign.CampaignName),
						PXErrorLevel.Warning));
			}
		}

		protected virtual void ARInvoiceAlias_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.SetValue<ARInvoiceAlias.campaignID>(e.Row, Campaign.Current?.CampaignID);
			sender.SetStatus(e.Row, PXEntryStatus.Inserted);
		}

		protected virtual void ARInvoiceAlias_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARInvoiceAlias row = e.Row as ARInvoiceAlias;
			if (row?.OrigCampaignID != null && row?.OrigCampaignID != row.CampaignID)
			{
				var oldCampaign = new PXSelect<CRCampaign,
					Where<CRCampaign.campaignID, Equal<Required<ARInvoice.campaignID>>>>(this).SelectSingle(row.OrigCampaignID);
				if (oldCampaign != null)
					this.Caches[typeof(ARInvoiceAlias)].RaiseExceptionHandling<ARInvoiceAlias.refNbr>(row, row.RefNbr,
						new PXSetPropertyException(
							PXMessages.LocalizeFormat(Messages.CampaignLinkWarning, oldCampaign.CampaignName),
							PXErrorLevel.Warning));
			}
		}
		protected virtual void SOOrderAlias_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrderAlias row = e.Row as SOOrderAlias;
			CRCampaign campaign = Campaign.Current;
			if (row == null || campaign == null)
				return;

			if (e.Operation == PXDBOperation.Insert)
			{
				PXDatabase.Update<SOOrder>(
					new PXDataFieldAssign<SOOrder.campaignID>(campaign.CampaignID),
					new PXDataFieldRestrict<SOOrder.orderType>(row.OrderType),
					new PXDataFieldRestrict<SOOrder.orderNbr>(row.OrderNbr)
				);
			}
			else if (e.Operation == PXDBOperation.Delete)
			{
				PXDatabase.Update<SOOrder>(
					new PXDataFieldAssign<SOOrder.campaignID>(null),
					new PXDataFieldRestrict<SOOrder.orderType>(row.OrderType),
					new PXDataFieldRestrict<SOOrder.orderNbr>(row.OrderNbr)
				);
			}

			sender.Clear();

			e.Cancel = true;
		}		
		protected virtual void ARInvoiceAlias_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARInvoiceAlias row = e.Row as ARInvoiceAlias;
			CRCampaign campaign = Campaign.Current;
			if (row == null || campaign == null)
				return;

			if (e.Operation == PXDBOperation.Insert)
			{
				PXDatabase.Update<ARInvoice>(
					new PXDataFieldAssign<ARInvoice.campaignID>(campaign.CampaignID),
					new PXDataFieldRestrict<ARInvoice.docType>(row.DocType),
					new PXDataFieldRestrict<ARInvoice.refNbr>(row.RefNbr)
				);
			}
			else if (e.Operation == PXDBOperation.Delete)
			{
				PXDatabase.Update<ARInvoice>(
					new PXDataFieldAssign<ARInvoice.campaignID>(null),
					new PXDataFieldRestrict<ARInvoice.docType>(row.DocType),
					new PXDataFieldRestrict<ARInvoice.refNbr>(row.RefNbr)
				);
			}

			sender.Clear();

			e.Cancel = true;
		}
		
		#endregion
		*/
	}
}