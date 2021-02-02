using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using PX.Api;
using PX.Common;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Maintenance.GI;
using PX.Data.SQLTree;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.GL.Descriptor;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using PX.Objects.SO;
using PX.SM;

namespace PX.Objects.IN
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CS;
	using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
	using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
	using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
	using ItemLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.ItemLotSerial;
    using SiteLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.SiteLotSerial;
	using ReadOnlyLotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.ReadOnlyLotSerialStatus;
	using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
	using IQtyAllocatedBase = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocatedBase;
	using IQtyAllocatedSeparateReceipts = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocatedSeparateReceipts;
	using PX.Objects.Common.Bql;
	using PX.Objects.Common;
    using PX.Objects.Common.Exceptions;
    using PX.Objects.Common.Scopes;

	public class CommonSetupDecPl : IPrefetchable
	{
		protected int _Qty = CommonSetup.decPlQty.Default;
		protected int _PrcCst = CommonSetup.decPlPrcCst.Default;
		void IPrefetchable.Prefetch()
		{
			using (PXDataRecord record = PXDatabase.SelectSingle<CommonSetup>(
				new PXDataField(typeof(CommonSetup.decPlQty).Name),
				new PXDataField(typeof(CommonSetup.decPlPrcCst).Name)))
			{
				if (record != null)
				{
					_Qty = (int)record.GetInt16(0);
					_PrcCst = (int)record.GetInt16(1);
				}
			}
		}

		public static int Qty
		{
			get
			{
				CommonSetupDecPl definition = PXDatabase.GetSlot<CommonSetupDecPl>(typeof(CommonSetupDecPl).Name, typeof(CommonSetup));
				if (definition != null)
				{
					return definition._Qty;
				}
				return CommonSetup.decPlQty.Default;
			}
		}

		public static int PrcCst
		{
			get
			{
				CommonSetupDecPl definition = PXDatabase.GetSlot<CommonSetupDecPl>(typeof(CommonSetupDecPl).Name, typeof(CommonSetup));
				if (definition != null)
				{
					return definition._PrcCst;
				}
				return CommonSetup.decPlPrcCst.Default;
			}
		}
	}

	public class PXUnitConversionException : PXSetPropertyException
	{
		public PXUnitConversionException()
			: base(Messages.MissingUnitConversion)
		{
		}

		public PXUnitConversionException(string UOM)
			: base(Messages.MissingUnitConversionVerbose, UOM)
		{
		}
		public PXUnitConversionException(SerializationInfo info, StreamingContext context)
			: base(info, context){}

	}

	/// <summary>
	/// This exception type raised when Unit of Inventory Item is indivisible(<see cref="InventoryItem.DecimalBaseUnit"/>, <see cref="InventoryItem.DecimalSelesUnit"/> or <see cref="InventoryItem.DecimalPurchaseUnit"/> is set <c>false</c>)
	/// and the entered value is non integer
	/// </summary>
	public class PXNotDecimalUnitException: PXSetPropertyException
	{
		private static string GetMessageFormat(InventoryUnitType unitType)
		{
			switch(unitType)
			{
				case InventoryUnitType.BaseUnit:
					return Messages.NotDecimalBaseUnit;
				case InventoryUnitType.SalesUnit:
					return Messages.NotDecimalSalesUnit;
				case InventoryUnitType.PurchaseUnit:
					return Messages.NotDecimalPurchaseUnit;
				default:
					throw new ArgumentOutOfRangeException(nameof(unitType));
			}
		}

		public bool IsLazyThrow { get; set; }

		public PXNotDecimalUnitException(InventoryUnitType unitType, string inventoryCD, string unitID, PXErrorLevel errorLevel)
			: base(GetMessageFormat(unitType), errorLevel, unitID, inventoryCD)
		{
		}

		public PXNotDecimalUnitException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
	}

	public interface IQtyPlanned
	{
		bool? Reverse { get; set; }
		decimal? PlanQty { get; set; }
	}

	#region INPrecision

	public enum INPrecision
	{
		NOROUND = 0,
		QUANTITY = 1,
		UNITCOST = 2
	}

	#endregion

    #region INPlanLevel

    public class INPlanLevel
    {
        public const int Site = 0;
        public const int Location = 1;
        public const int LotSerial = 2;
        public const int LocationLotSerial = Location | LotSerial;

		public const int ExcludeSite = 1 << 16;
		public const int ExcludeLocation = 1 << 17;
		public const int ExcludeLotSerial = 1 << 18;
		public const int ExcludeSiteLotSerial = ExcludeSite | ExcludeLotSerial;
		public const int ExcludeLocationLotSerial = ExcludeLocation | ExcludeLotSerial;
    }
    #endregion

	#region PXDBCostScalarAttribute
	public class PXDBCostScalarAttribute : PXDBScalarAttribute
	{
		public PXDBCostScalarAttribute(Type search)
			: base(search)
		{
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
	}
	#endregion

	#region PXExistance

	public class PXExistance : PXBoolAttribute, IPXRowSelectingSubscriber
	{
		#region Implementation
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			foreach (string key in sender.Keys)
			{
				if (sender.GetValue(e.Row, key) == null)
				{
					return;
				}
			}
			sender.SetValue(e.Row, _FieldOrdinal, true);
		}
		#endregion
	}

	#endregion

    #region PXNonExistence

    public class PXNonExistence : PXBoolAttribute, IPXRowSelectingSubscriber
    {
        #region Implementation
        public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            foreach (string key in sender.Keys)
            {
                if (sender.GetValue(e.Row, key) != null)
                {
                    return;
                }
            }
            sender.SetValue(e.Row, _FieldOrdinal, true);
        }
        #endregion
    }

    #endregion

    #region INTranSplitPlanIDAttribute

    public class INTranSplitPlanIDAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentTransferType;
		#endregion
		#region Ctor
		public INTranSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentTransferType)
			: base(ParentNoteID, ParentHoldEntry)
		{
			_ParentTransferType = ParentTransferType;
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Parent_RowUpdated(sender, e);

			if (!sender.ObjectsEqual<INRegister.hold, INRegister.transferType>(e.Row, e.OldRow))
			{
				bool transfertypeupdated = !sender.ObjectsEqual<INRegister.transferType>(e.Row, e.OldRow);
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];

				//preload plans in cache first
				var not_inserted = PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<INRegister.noteID>>>>.Select(sender.Graph).ToList();

				foreach (INTranSplit split in PXSelect<INTranSplit, Where<INTranSplit.docType, Equal<Current<INRegister.docType>>, And<INTranSplit.refNbr, Equal<Current<INRegister.refNbr>>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Cached)
					{
						if (object.Equals(plan.PlanID, split.PlanID) && plancache.GetStatus(plan) != PXEntryStatus.Deleted && plancache.GetStatus(plan) != PXEntryStatus.InsertedDeleted)
						{
							if (transfertypeupdated)
							{
								split.TransferType = (string)sender.GetValue<INRegister.transferType>(e.Row);
								sender.Graph.Caches[typeof(INTranSplit)].MarkUpdated(split);

								INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);
								copy = DefaultValues(sender, copy, split);
								plancache.Update(copy);
							}
							else
							{
								plan.Hold = (bool?)sender.GetValue<INRegister.hold>(e.Row);
							}
						}
					}
				}
			}
		}

		public static bool? IsTwoStepTransferPlanValid(PXCache sender, INTranSplit split, INItemPlan plan)
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is INTranSplitPlanIDAttribute)
				{
					return ((INTranSplitPlanIDAttribute)attr).IsTwoStepTransferPlanValid(split, plan);
				}
			}

			return null;
		}

		public virtual bool? IsTwoStepTransferPlanValid(INTranSplit split, INItemPlan plan)
		{
			if (split.DocType != INDocType.Transfer || split.TranType != INTranType.Transfer
				|| split.TransferType != INTransferType.TwoStep || split.InvtMult != -1m || split.Released == true)
				return null;

			if (split.SOLineType == null)
				return plan?.PlanType == INPlanConstants.Plan41;
			
			return plan?.PlanType == INPlanConstants.Plan62;
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];

            INTran parent = null;
			INTranSplit split_Row;
			object doc_Row;

			if (orig_Row is PXResult)
			{
				split_Row = (INTranSplit)((PXResult)orig_Row)[typeof(INTranSplit)];
				doc_Row = ((PXResult)orig_Row)[BqlCommand.GetItemType(_ParentNoteID)];
			}
			else
			{
				split_Row = (INTranSplit)orig_Row;
				doc_Row = cache.Current;
                parent = (INTran)PXParentAttribute.SelectParent(sender, orig_Row, typeof(INTran));
			}

            plan_Row.OrigPlanType = split_Row.OrigPlanType;
			plan_Row.InventoryID = split_Row.InventoryID;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;
			plan_Row.LocationID = split_Row.LocationID;
			plan_Row.LotSerialNbr = split_Row.LotSerialNbr;
			if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
			{
				plan_Row.LotSerialNbr = null;
			}
			plan_Row.PlanQty = split_Row.BaseQty;
			// if Plan Type is SO Shipped then we keep original Plan and Shipment Date
			plan_Row.PlanDate = !string.IsNullOrEmpty(split_Row.SOLineType) ? split_Row.TranDate : new DateTime(1900, 1, 1);
			plan_Row.BAccountID = parent?.BAccountID;
			if (doc_Row is INRegister)
			{
				var inDoc = (INRegister)doc_Row;
				if (inDoc.DocType == INDocType.Transfer && inDoc.TransferType == INTransferType.OneStep && inDoc.SiteID == inDoc.ToSiteID)
				{
					plan_Row.ExcludePlanLevel = !string.IsNullOrEmpty(plan_Row.LotSerialNbr) ? INPlanLevel.ExcludeSiteLotSerial : INPlanLevel.ExcludeSite;
				}
				else
				{
					plan_Row.ExcludePlanLevel = null;
				}
			}

			plan_Row.RefNoteID = (Guid?)cache.GetValue(doc_Row, _ParentNoteID.Name);
			plan_Row.Hold = (bool?)cache.GetValue(doc_Row, _ParentHoldEntry.Name);

			switch (split_Row.TranType)
			{
				case INTranType.Receipt:
				case INTranType.Return:
				case INTranType.CreditMemo:
					if (split_Row.Released == true)
					{
						return null;
					}

					plan_Row.PlanType =
						(split_Row.SOLineType != null) ? INPlanConstants.Plan62 :
						(split_Row.POLineType == PO.POLineType.GoodsForSalesOrder) ? INPlanConstants.Plan77 :
                        (split_Row.POLineType == PO.POLineType.GoodsForServiceOrder) ? INPlanConstants.PlanF9 :
						(split_Row.POLineType == PO.POLineType.GoodsForDropShip) ? INPlanConstants.Plan75 :
						INPlanConstants.Plan10;
					break;
				case INTranType.Issue:
				case INTranType.Invoice:
				case INTranType.DebitMemo:
					if (split_Row.Released == true)
					{
						return null;
					}

					plan_Row.PlanType = (split_Row.SOLineType == null) ? INPlanConstants.Plan20 : INPlanConstants.Plan62;
					break;
				case INTranType.Transfer:
					if (split_Row.InvtMult == -1)
					{
						if (split_Row.TransferType == INTransferType.OneStep)
						{
                            if(split_Row.Released == true)
							{
								return null;
							}

							plan_Row.PlanType = INPlanConstants.Plan40;
						}
						else if (split_Row.Released == true)
						{
							plan_Row.PlanType = split_Row.IsFixedInTransit == true ? INPlanConstants.Plan44 : INPlanConstants.Plan42;
							plan_Row.SiteID = split_Row.ToSiteID;
							plan_Row.LocationID = split_Row.ToLocationID;
						}
						else
						{
							plan_Row.PlanType = (split_Row.SOLineType == null) ? INPlanConstants.Plan41 : INPlanConstants.Plan62;
						}
					}
					else
					{
						if (split_Row.Released == true)
						{
							return null;
						}

						plan_Row.PlanType = INPlanConstants.Plan43;
                        if (string.IsNullOrEmpty(plan_Row.OrigPlanType))
                        {
                            plan_Row.OrigPlanType = INPlanConstants.Plan42;
					}
					}
					break;
				case INTranType.Assembly:
				case INTranType.Disassembly:
					if (split_Row.Released == true)
					{
						return null;
					}

					if (split_Row.InvtMult == (short)-1)
					{
						plan_Row.PlanType = INPlanConstants.Plan50;
					}
					else
					{
						plan_Row.PlanType = INPlanConstants.Plan51;
					}
					break;
				case INTranType.Adjustment:
				case INTranType.StandardCostAdjustment:
				case INTranType.NegativeCostAdjustment:
				default:
					return null;
			}

            if (parent != null && parent.OrigTranType == INTranType.Transfer && plan_Row.OrigNoteID == null)
            {
				plan_Row.OrigNoteID = parent.OrigNoteID;
				plan_Row.OrigPlanLevel = 
					(parent.OrigToLocationID != null ? INPlanLevel.Location : INPlanLevel.Site)
					| (parent.OrigIsLotSerial == true ? INPlanLevel.LotSerial : INPlanLevel.Site);
            }

			return plan_Row;
		}

		#endregion
	}

	#endregion

	#region INItemPlanIDAttribute

	public class PlanningHelper<TNode> : PXSelectReadonly<TNode>
		where TNode : class, IBqlTable, new()
	{
		#region Ctor
		public PlanningHelper(PXGraph graph)
			: base(graph)
		{
			graph.Initialized += sender => Initialize(sender);
		}
		#endregion
		#region Initialization
		public virtual void Initialize(PXGraph graph)
		{
			foreach (INItemPlanIDAttribute attr in graph.Caches[typeof(TNode)].GetAttributesReadonly(null).OfType<INItemPlanIDAttribute>())
			{
				//handler need to be executed after EPApprovalAutomation.
				graph.RowUpdated.RemoveHandler(BqlCommand.GetItemType(attr._ParentNoteID), attr.Parent_RowUpdated);
				graph.RowUpdated.AddHandler(BqlCommand.GetItemType(attr._ParentNoteID), attr.Parent_RowUpdated);
				break;
			}
		}
		#endregion
	}

	public abstract class INItemPlanIDAttribute : INItemPlanIDBaseAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowDeletedSubscriber
	{
		#region State
		internal protected Type _ParentNoteID;
		protected Type _ParentHoldEntry;
		protected ObjectRef<bool> _ReleaseMode;
		#endregion
		#region Ctor
		public INItemPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
		{
			_ParentNoteID = ParentNoteID;
			_ParentHoldEntry = ParentHoldEntry;
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ReleaseMode = new ObjectRef<bool>();

			sender.Graph.RowInserted.AddHandler(BqlCommand.GetItemType(_ParentNoteID), Parent_RowInserted);
			sender.Graph.RowUpdated.AddHandler(BqlCommand.GetItemType(_ParentNoteID), Parent_RowUpdated);

			if (!sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
			{
				sender.Graph.RowInserting.AddHandler<INItemPlan>(Plan_RowInserting);
				sender.Graph.RowInserted.AddHandler<INItemPlan>(Plan_RowInserted);
				sender.Graph.RowUpdated.AddHandler<INItemPlan>(Plan_RowUpdated);
				sender.Graph.RowDeleted.AddHandler<INItemPlan>(Plan_RowDeleted);

                sender.Graph.CommandPreparing.AddHandler<INItemPlan.planID>(Parameter_CommandPreparing);

				if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
					{
						sender.Graph.FieldDefaulting.AddHandler<INItemPlan.siteID>(Feature_FieldDefaulting);
						sender.Graph.FieldDefaulting.AddHandler<LocationStatus.siteID>(Feature_FieldDefaulting);
						sender.Graph.FieldDefaulting.AddHandler<LotSerialStatus.siteID>(Feature_FieldDefaulting);
					}

					if (!PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
					{
						sender.Graph.FieldDefaulting.AddHandler<INItemPlan.locationID>(Feature_FieldDefaulting);
						sender.Graph.FieldDefaulting.AddHandler<LocationStatus.locationID>(Feature_FieldDefaulting);
						sender.Graph.FieldDefaulting.AddHandler<LotSerialStatus.locationID>(Feature_FieldDefaulting);
					}
				}
			}

			sender.Graph.Caches.AddCacheMapping(typeof(INSiteStatus), typeof(INSiteStatus));
			sender.Graph.Caches.AddCacheMapping(typeof(INLocationStatus), typeof(INLocationStatus));
			sender.Graph.Caches.AddCacheMapping(typeof(INLotSerialStatus), typeof(INLotSerialStatus));
			sender.Graph.Caches.AddCacheMapping(typeof(INItemLotSerial), typeof(INItemLotSerial));
			sender.Graph.Caches.AddCacheMapping(typeof(INSiteLotSerial), typeof(INSiteLotSerial));

			sender.Graph.Caches.AddCacheMapping(typeof(SiteStatus), typeof(SiteStatus));
			sender.Graph.Caches.AddCacheMapping(typeof(LocationStatus), typeof(LocationStatus));
			sender.Graph.Caches.AddCacheMapping(typeof(LotSerialStatus), typeof(LotSerialStatus));
			sender.Graph.Caches.AddCacheMapping(typeof(ItemLotSerial), typeof(ItemLotSerial));
			sender.Graph.Caches.AddCacheMapping(typeof(SiteLotSerial), typeof(SiteLotSerial));
			//PXCache dummy_cache;
			//dummy_cache = sender.Graph.Caches[typeof(INSiteStatus)];
			//dummy_cache = sender.Graph.Caches[typeof(INLocationStatus)];
			//dummy_cache = sender.Graph.Caches[typeof(INLotSerialStatus)];
			//         dummy_cache = sender.Graph.Caches[typeof(INItemLotSerial)];
			//         dummy_cache = sender.Graph.Caches[typeof(INSiteLotSerial)];

			//dummy_cache = sender.Graph.Caches[typeof(SiteStatus)];
			//dummy_cache = sender.Graph.Caches[typeof(LocationStatus)];
			//dummy_cache = sender.Graph.Caches[typeof(LotSerialStatus)];
			//         dummy_cache = sender.Graph.Caches[typeof(ItemLotSerial)];
			//         dummy_cache = sender.Graph.Caches[typeof(SiteLotSerial)];

			if (sender.Graph.IsImport || sender.Graph.UnattendedMode)
            {
                if (!sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
                    sender.Graph.Views.Caches.Add(typeof(INItemPlan));
                if (sender.Graph.Views.Caches.Contains(_ItemType))
                    sender.Graph.Views.Caches.Remove(_ItemType);
                sender.Graph.Views.Caches.Add(_ItemType);
            }
            else
            {
                //plan source should go before plan
                //to bind errors from INItemPlan.SubItemID -> SOLine.SubItemID or SOLineSplit.SubItemID
                if (!sender.Graph.Views.Caches.Contains(_ItemType))
                    sender.Graph.Views.Caches.Add(_ItemType);
                if (!sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
                    sender.Graph.Views.Caches.Add(typeof(INItemPlan));
            }

			if (!sender.Graph.Views.Caches.Contains(typeof(LotSerialStatus)))
				sender.Graph.Views.Caches.Add(typeof(LotSerialStatus));
            if (!sender.Graph.Views.Caches.Contains(typeof(ItemLotSerial)))
                sender.Graph.Views.Caches.Add(typeof(ItemLotSerial));
            if (!sender.Graph.Views.Caches.Contains(typeof(SiteLotSerial)))
                sender.Graph.Views.Caches.Add(typeof(SiteLotSerial));
			if (!sender.Graph.Views.Caches.Contains(typeof(LocationStatus)))
				sender.Graph.Views.Caches.Add(typeof(LocationStatus));
			if (!sender.Graph.Views.Caches.Contains(typeof(SiteStatus)))
				sender.Graph.Views.Caches.Add(typeof(SiteStatus));

			sender.Graph.FieldVerifying.AddHandler<SiteStatus.subItemID>(SurrogateID_FieldVerifying);
			sender.Graph.FieldVerifying.AddHandler<LocationStatus.subItemID>(SurrogateID_FieldVerifying);
			sender.Graph.FieldVerifying.AddHandler<LotSerialStatus.subItemID>(SurrogateID_FieldVerifying);
			sender.Graph.FieldVerifying.AddHandler<LocationStatus.locationID>(SurrogateID_FieldVerifying);
			sender.Graph.FieldVerifying.AddHandler<LotSerialStatus.locationID>(SurrogateID_FieldVerifying);

			sender.Graph.RowPersisted.AddHandler<SiteStatus>(Accumulator_RowPersisted);
			sender.Graph.RowPersisted.AddHandler<LocationStatus>(Accumulator_RowPersisted);
			sender.Graph.RowPersisted.AddHandler<LotSerialStatus>(Accumulator_RowPersisted);
            sender.Graph.RowPersisted.AddHandler<ItemLotSerial>(Accumulator_RowPersisted);
            sender.Graph.RowPersisted.AddHandler<SiteLotSerial>(Accumulator_RowPersisted);

            sender.Graph.CommandPreparing.AddHandler(_ItemType, _FieldName, Parameter_CommandPreparing);
		}

		#endregion
		#region Implementation

		protected Type GetEntityType(PXGraph graph, Guid? noteID)
		{
			if (noteID == null) return null;

			Note note = PXSelect<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>.SelectWindowed(graph, 0, 1, noteID);

			if (note == null || string.IsNullOrEmpty(note.EntityType)) return null;

			return System.Web.Compilation.PXBuildManager.GetType(note.EntityType, false);
		}

        protected virtual void Parameter_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            long? Key;
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.Option) != PXDBOperation.External &&
                (e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null && (Key = e.Value as long?) != null)
            {
                if (Key < 0L)
                {
                    e.DataValue = null;
                    e.Cancel = true;
                }
            }
        }

		public static void SetReleaseMode<Field>(PXCache cache,  bool releaseMode)
			where Field : IBqlField
		{
			foreach (INItemPlanIDAttribute attr in cache.GetAttributes<Field>(null).OfType<INItemPlanIDAttribute>())
				(attr)._ReleaseMode.Value = releaseMode;
		}

		public abstract INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row);

        protected INItemPlan DefaultValuesInt(PXCache sender, INItemPlan plan_Row, object orig_Row)
        {
            INItemPlan info = DefaultValues(sender, plan_Row, orig_Row);
            if (info != null && info.InventoryID != null && info.SiteID != null)
            {
				info.RefEntityType = GetRefEntityType();
                return info;
            }
            return null;
        }

		protected virtual string GetRefEntityType()
		{
			return BqlCommand.GetItemType(_ParentNoteID)?.FullName;
		}

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		public virtual void SurrogateID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue is Int32)
			{
				e.Cancel = true;
			}
		}

        protected Dictionary<Type, List<PXView>> _views;

        protected void Clear<TNode>(PXGraph graph)
			where TNode : class, IBqlTable
		{
            if (_views == null)
            {
                _views = new Dictionary<Type, List<PXView>>();
            }

            List<PXView> views;
            if (!_views.TryGetValue(typeof(TNode), out views))
            {
                views = _views[typeof(TNode)] = new List<PXView>();

			    List<PXView> namedviews = new List<PXView>(graph.Views.Values);
			    foreach (PXView view in namedviews)
			    {
				    if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
				    {
					    views.Add(view);
				    }
			    }

			    List<PXView> typedviews = new List<PXView>(graph.TypedViews.Values);
			    foreach (PXView view in typedviews)
			    {
				    if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
				    {
					    views.Add(view);
				    }
			    }

			    List<PXView> readonlyviews = new List<PXView>(graph.TypedViews.ReadOnlyValues);
			    foreach (PXView view in readonlyviews)
			    {
				    if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
				    {
					    views.Add(view);
				    }
			    }
            }

            foreach(PXView view in views)
            {
                view.Clear();
			    view.Cache.Clear();            
            }
		}

		public virtual void Accumulator_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation != PXDBOperation.Delete && e.TranStatus == PXTranStatus.Completed)
			{
				if (sender.GetItemType() == typeof(SiteStatus))
				{
					Clear<INSiteStatus>(sender.Graph);
				}
				if (sender.GetItemType() == typeof(LocationStatus))
				{
					Clear<INLocationStatus>(sender.Graph);
				}
				if (sender.GetItemType() == typeof(LotSerialStatus))
				{
					Clear<INLotSerialStatus>(sender.Graph);
				}
                if (sender.GetItemType() == typeof(ItemLotSerial))
                {
                    Clear<ItemLotSerial>(sender.Graph);
                }
                if (sender.GetItemType() == typeof(SiteLotSerial))
                {
                    Clear<SiteLotSerial>(sender.Graph);
                }
			}
		}

		public virtual void Parent_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _ParentNoteID.Name);
			sender.Graph.Caches[typeof(Note)].IsDirty = false;
		}

		public virtual void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _ParentNoteID.Name);
		}

		public static TNode ConvertPlan<TNode>(INItemPlan item)
			where TNode : class, IQtyAllocatedBase
		{
			if (typeof(TNode) == typeof(SiteStatus))
			{
				return (SiteStatus)item as TNode;
			}
			if (typeof(TNode) == typeof(LocationStatus))
			{
				return (LocationStatus)item as TNode;
			}
			if (typeof(TNode) == typeof(LotSerialStatus))
			{
				return (LotSerialStatus)item as TNode;
			}
            if (typeof(TNode) == typeof(ItemLotSerial))
            {
                return (ItemLotSerial)item as TNode;
            }
            if (typeof(TNode) == typeof(SiteLotSerial))
            {
                return (SiteLotSerial)item as TNode;
            }

			return null;
		}

		protected virtual void GetInclQtyAvail<TNode>(PXGraph graph, object data,
			out decimal signQtyAvail, out decimal signQtyHardAvail, out decimal signQtyActual)
			where TNode : class, IQtyAllocatedBase, IBqlTable, new()
		{
			INItemPlan plan = DefaultValuesInt(graph.Caches[_ItemType], new INItemPlan() { IsTemporary = true }, data);
			if (plan != null)
			{
				INPlanType plantype = INPlanType.PK.Find(graph, plan.PlanType);
				plantype = GetTargetPlanType<TNode>(graph, plan, plantype);

				GetInclQtyAvail<TNode>(graph, plan, plantype, out signQtyAvail, out signQtyHardAvail, out signQtyActual);
				return;
			}

			signQtyAvail = 0m;
			signQtyHardAvail = 0m;
			signQtyActual = 0m;
		}

		public virtual void GetInclQtyAvail<TNode>(PXGraph graph, object data, out decimal signQtyAvail, out decimal signQtyHardAvail)
			where TNode : class, IQtyAllocatedBase, IBqlTable, new()
		{
			decimal signQtyActual;
			GetInclQtyAvail<TNode>(graph, data, out signQtyAvail, out signQtyHardAvail, out signQtyActual);
		}

		public static void GetInclQtyAvail<TNode>(PXCache sender, object data,
			out decimal signQtyAvail, out decimal signQtyHardAvail, out decimal signQtyActual)
			where TNode : class, IQtyAllocatedBase, IBqlTable, new()
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is INItemPlanIDAttribute)
				{
					((INItemPlanIDAttribute)attr).GetInclQtyAvail<TNode>(sender.Graph, data, out signQtyAvail, out signQtyHardAvail, out signQtyActual);
					return;
				}
			}
			signQtyAvail = 0m;
			signQtyHardAvail = 0m;
			signQtyActual = 0m;
		}

		public static void GetInclQtyAvail<TNode>(PXCache sender, object data, out decimal signQtyAvail, out decimal signQtyHardAvail)
			where TNode : class, IQtyAllocatedBase, IBqlTable, new()
		{
			decimal signQtyActual;
			GetInclQtyAvail<TNode>(sender, data, out signQtyAvail, out signQtyHardAvail, out signQtyActual);
		}

		public static INItemPlan DefaultValues(PXCache sender, object data)
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is INItemPlanIDAttribute)
				{
					return ((INItemPlanIDAttribute)attr).DefaultValuesInt(sender, new INItemPlan(), data);
				}
			}
			return null;
		}

		protected T InsertWith<T>(PXGraph graph, T row, PXRowInserted handler)
			where T : class, IBqlTable, new()
		{
			graph.RowInserted.AddHandler<T>(handler);
			try
			{
				return PXCache<T>.Insert(graph, row);
			}
			finally
			{
				graph.RowInserted.RemoveHandler<T>(handler);
			}
		}

		protected void GetInclQtyAvail<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype,
			out decimal signQtyAvail, out decimal signQtyHardAvail, out decimal signQtyActual)
			where TNode : class, IQtyAllocatedBase, IBqlTable, new()
		{

			TNode target = InsertWith<TNode>(graph, ConvertPlan<TNode>(plan),
				(cache, e) => { cache.SetStatus(e.Row, PXEntryStatus.Notchanged); cache.IsDirty = false; });

			signQtyAvail = 0m;

			if (plan.Reverse != true || target.InclQtySOReverse == true || !IsSORelated(plantype))
			{
				signQtyAvail -= target.InclQtyINIssues == true ? (decimal)plantype.InclQtyINIssues : 0m;
				signQtyAvail += target.InclQtyINReceipts == true ? (decimal)plantype.InclQtyINReceipts : 0m;
				signQtyAvail += target.InclQtyInTransit == true ? (decimal)plantype.InclQtyInTransit : 0m;
				signQtyAvail += target.InclQtyPOPrepared == true ? (decimal)plantype.InclQtyPOPrepared : 0m;
				signQtyAvail += target.InclQtyPOOrders == true ? (decimal)plantype.InclQtyPOOrders : 0m;
				signQtyAvail += target.InclQtyPOReceipts == true ? (decimal)plantype.InclQtyPOReceipts : 0m;
				signQtyAvail += target.InclQtyINAssemblySupply == true ? (decimal)plantype.InclQtyINAssemblySupply : 0m;
                signQtyAvail += target.InclQtyProductionSupplyPrepared == true ? (decimal)plantype.InclQtyProductionSupplyPrepared : 0m;
                signQtyAvail += target.InclQtyProductionSupply == true ? (decimal)plantype.InclQtyProductionSupply : 0m;
                signQtyAvail -= target.InclQtySOBackOrdered == true ? (decimal)plantype.InclQtySOBackOrdered : 0m;
				signQtyAvail -= target.InclQtySOPrepared == true ? (decimal)plantype.InclQtySOPrepared : 0m;
				signQtyAvail -= target.InclQtySOBooked == true ? (decimal)plantype.InclQtySOBooked : 0m;
				signQtyAvail -= target.InclQtySOShipped == true ? (decimal)plantype.InclQtySOShipped : 0m;
				signQtyAvail -= target.InclQtySOShipping == true ? (decimal)plantype.InclQtySOShipping : 0m;
				signQtyAvail -= target.InclQtyINAssemblyDemand == true ? (decimal)plantype.InclQtyINAssemblyDemand : 0m;
				signQtyAvail -= target.InclQtyProductionDemandPrepared == true ? (decimal)plantype.InclQtyProductionDemandPrepared : 0m;
				signQtyAvail -= target.InclQtyProductionDemand == true ? (decimal)plantype.InclQtyProductionDemand : 0m;
				signQtyAvail -= target.InclQtyProductionAllocated == true ? (decimal)plantype.InclQtyProductionAllocated : 0m;
                
                signQtyAvail -= target.InclQtyFSSrvOrdPrepared == true ? (decimal)plantype.InclQtyFSSrvOrdPrepared : 0m;
                signQtyAvail -= target.InclQtyFSSrvOrdBooked == true ? (decimal)plantype.InclQtyFSSrvOrdBooked : 0m;
                signQtyAvail -= target.InclQtyFSSrvOrdAllocated == true ? (decimal)plantype.InclQtyFSSrvOrdAllocated : 0m;

				if (plan.Reverse == true)
				{
					signQtyAvail = -signQtyAvail;
				}
			}

			signQtyHardAvail = 0m;

			if (plan.Reverse != true)
			{
				signQtyHardAvail -= (decimal)plantype.InclQtySOShipped;
				signQtyHardAvail -= (decimal)plantype.InclQtySOShipping;
				signQtyHardAvail -= (decimal)plantype.InclQtyINIssues;
			    signQtyHardAvail -= (decimal)plantype.InclQtyProductionAllocated;

                signQtyHardAvail -= (decimal)plantype.InclQtyFSSrvOrdAllocated;
			}

			signQtyActual = (plan.Reverse != true) ? -(decimal)plantype.InclQtySOShipped : 0m;
		}

		public static TNode UpdateAllocatedQuantitiesBase<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype, bool InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			bool isDirty = graph.Caches[typeof(TNode)].IsDirty;
			TNode target = (TNode)graph.Caches[typeof(TNode)].Insert(ConvertPlan<TNode>(plan));
			graph.Caches[typeof(TNode)].IsDirty = isDirty;

			return UpdateAllocatedQuantitiesBase<TNode>(graph, target, plan, plantype, InclQtyAvail);
		}

		public static TNode UpdateAllocatedQuantitiesBase<TNode>(PXGraph graph, TNode target, IQtyPlanned plan, INPlanType plantype, bool? InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			decimal qty = plan.PlanQty ?? 0;
			if (plan.Reverse == true)
			{
				if (target.InclQtySOReverse != true && IsSORelated(plantype))
					return target;
				else
					qty = -qty;
			}

            IQtyAllocated exttarget = target as IQtyAllocated;
            if (exttarget != null)
            {
                exttarget.QtyINIssues += (plantype.InclQtyINIssues ?? 0) * qty;
                exttarget.QtyINReceipts += (plantype.InclQtyINReceipts ?? 0) * qty;
                exttarget.QtyPOPrepared += (plantype.InclQtyPOPrepared ?? 0) * qty;
                exttarget.QtyPOOrders += (plantype.InclQtyPOOrders ?? 0) * qty;
                exttarget.QtyPOReceipts += (plantype.InclQtyPOReceipts ?? 0) * qty;

                exttarget.QtyFSSrvOrdPrepared += (plantype.InclQtyFSSrvOrdPrepared ?? 0) * qty;
                exttarget.QtyFSSrvOrdBooked += (plantype.InclQtyFSSrvOrdBooked ?? 0) * qty;
                exttarget.QtyFSSrvOrdAllocated += (plantype.InclQtyFSSrvOrdAllocated ?? 0) * qty;

                exttarget.QtySOBackOrdered += (plantype.InclQtySOBackOrdered ?? 0) * qty;
				exttarget.QtySOPrepared += (plantype.InclQtySOPrepared ?? 0) * qty;
				exttarget.QtySOBooked += (plantype.InclQtySOBooked ?? 0) * qty;
                exttarget.QtySOShipped += (plantype.InclQtySOShipped ?? 0) * qty;
                exttarget.QtySOShipping += (plantype.InclQtySOShipping ?? 0) * qty;
                exttarget.QtyINAssemblySupply += (plantype.InclQtyINAssemblySupply ?? 0) * qty;
                exttarget.QtyINAssemblyDemand += (plantype.InclQtyINAssemblyDemand ?? 0) * qty;
                exttarget.QtyInTransitToProduction += (plantype.InclQtyInTransitToProduction ?? 0) * qty;
                exttarget.QtyProductionSupplyPrepared += (plantype.InclQtyProductionSupplyPrepared ?? 0) * qty;
                exttarget.QtyProductionSupply += (plantype.InclQtyProductionSupply ?? 0) * qty;
                exttarget.QtyPOFixedProductionPrepared += (plantype.InclQtyPOFixedProductionPrepared ?? 0) * qty;
                exttarget.QtyPOFixedProductionOrders += (plantype.InclQtyPOFixedProductionOrders ?? 0) * qty;
                exttarget.QtyProductionDemandPrepared += (plantype.InclQtyProductionDemandPrepared ?? 0) * qty;
                exttarget.QtyProductionDemand += (plantype.InclQtyProductionDemand ?? 0) * qty;
                exttarget.QtyProductionAllocated += (plantype.InclQtyProductionAllocated ?? 0) * qty;
                exttarget.QtySOFixedProduction += (plantype.InclQtySOFixedProduction ?? 0) * qty;
                exttarget.QtyProdFixedPurchase += (plantype.InclQtyProdFixedPurchase ?? 0) * qty;
                exttarget.QtyProdFixedProduction += (plantype.InclQtyProdFixedProduction ?? 0) * qty;
                exttarget.QtyProdFixedProdOrdersPrepared += (plantype.InclQtyProdFixedProdOrdersPrepared ?? 0) * qty;
                exttarget.QtyProdFixedProdOrders += (plantype.InclQtyProdFixedProdOrders ?? 0) * qty;
                exttarget.QtyProdFixedSalesOrdersPrepared += (plantype.InclQtyProdFixedSalesOrdersPrepared ?? 0) * qty;
                exttarget.QtyProdFixedSalesOrders += (plantype.InclQtyProdFixedSalesOrders ?? 0) * qty;
                exttarget.QtyINReplaned += (plantype.InclQtyINReplaned ?? 0) * qty;

                exttarget.QtyFixedFSSrvOrd += (plantype.InclQtyFixedFSSrvOrd ?? 0) * qty;
                exttarget.QtyPOFixedFSSrvOrd += (plantype.InclQtyPOFixedFSSrvOrd ?? 0) * qty;
                exttarget.QtyPOFixedFSSrvOrdPrepared += (plantype.InclQtyPOFixedFSSrvOrdPrepared ?? 0) * qty;
                exttarget.QtyPOFixedFSSrvOrdReceipts += (plantype.InclQtyPOFixedFSSrvOrdReceipts ?? 0) * qty;


                exttarget.QtySOFixed += (plantype.InclQtySOFixed ?? 0) * qty;
                exttarget.QtyPOFixedOrders += (plantype.InclQtyPOFixedOrders ?? 0) * qty;
                exttarget.QtyPOFixedPrepared += (plantype.InclQtyPOFixedPrepared ?? 0) * qty;
                exttarget.QtyPOFixedReceipts += (plantype.InclQtyPOFixedReceipts ?? 0) * qty;
                exttarget.QtySODropShip += (plantype.InclQtySODropShip ?? 0) * qty;
                exttarget.QtyPODropShipOrders += (plantype.InclQtyPODropShipOrders ?? 0) * qty;
                exttarget.QtyPODropShipPrepared += (plantype.InclQtyPODropShipPrepared ?? 0) * qty;
                exttarget.QtyPODropShipReceipts += (plantype.InclQtyPODropShipReceipts ?? 0) * qty;
                exttarget.QtyInTransitToSO += (plantype.InclQtyInTransitToSO ?? 0) * qty;
            }
            target.QtyInTransit += (plantype.InclQtyInTransit ?? 0) * qty;

			decimal avail = 0m, hardAvail = 0m, actual = 0m, receipts = 0m;

			avail -= target.InclQtyINIssues == true ? (plantype.InclQtyINIssues ?? 0) * qty : 0m;
			avail += target.InclQtyINReceipts == true ? (plantype.InclQtyINReceipts ?? 0) * qty : 0m;
			avail += target.InclQtyInTransit == true ? (plantype.InclQtyInTransit ?? 0) * qty : 0m;
			avail += target.InclQtyPOPrepared == true ? (plantype.InclQtyPOPrepared ?? 0) * qty : 0m;
			avail += target.InclQtyPOOrders == true ? (plantype.InclQtyPOOrders ?? 0) * qty : 0m;
			avail += target.InclQtyPOReceipts == true ? (plantype.InclQtyPOReceipts ?? 0) * qty : 0m;
			avail += target.InclQtyINAssemblySupply == true ? (plantype.InclQtyINAssemblySupply ?? 0) * qty : 0m;
            avail += target.InclQtyProductionSupplyPrepared == true ? (plantype.InclQtyProductionSupplyPrepared ?? 0) * qty : 0m;
            avail += target.InclQtyProductionSupply == true ? (plantype.InclQtyProductionSupply ?? 0) * qty : 0m;

            avail -= target.InclQtyFSSrvOrdPrepared == true ? (plantype.InclQtyFSSrvOrdPrepared ?? 0) * qty : 0m;
            avail -= target.InclQtyFSSrvOrdBooked == true ? (plantype.InclQtyFSSrvOrdBooked ?? 0) * qty : 0m;
            avail -= target.InclQtyFSSrvOrdAllocated == true ? (plantype.InclQtyFSSrvOrdAllocated ?? 0) * qty : 0m;

            avail -= target.InclQtySOBackOrdered == true ? (plantype.InclQtySOBackOrdered ?? 0) * qty : 0m;
			avail -= target.InclQtySOPrepared == true ? (plantype.InclQtySOPrepared ?? 0) * qty : 0m;
			avail -= target.InclQtySOBooked == true ? (plantype.InclQtySOBooked ?? 0) * qty : 0m;
			avail -= target.InclQtySOShipped == true ? (plantype.InclQtySOShipped ?? 0) * qty : 0m;
			avail -= target.InclQtySOShipping == true ? (plantype.InclQtySOShipping ?? 0) * qty : 0m;
			avail -= target.InclQtyINAssemblyDemand == true ? (plantype.InclQtyINAssemblyDemand ?? 0) * qty : 0m;
            avail -= target.InclQtyProductionDemandPrepared == true ? (plantype.InclQtyProductionDemandPrepared ?? 0) * qty : 0m;
            avail -= target.InclQtyProductionDemand == true ? (plantype.InclQtyProductionDemand ?? 0) * qty : 0m;
            avail -= target.InclQtyProductionAllocated == true ? (plantype.InclQtyProductionAllocated ?? 0) * qty : 0m;
            avail += target.InclQtyPOFixedReceipt == true ? (plantype.InclQtyPOFixedReceipts ?? 0) * qty : 0m;

			var receiptTarget = target as IQtyAllocatedSeparateReceipts;
			if (receiptTarget != null)
			{
				if (plan.Reverse != true)
				{
					receipts += target.InclQtyINReceipts == true ? (plantype.InclQtyINReceipts ?? 0) * qty : 0m;
					receipts += target.InclQtyPOPrepared == true ? (plantype.InclQtyPOPrepared ?? 0) * qty : 0m;
					receipts += target.InclQtyPOOrders == true ? (plantype.InclQtyPOOrders ?? 0) * qty : 0m;
					receipts += target.InclQtyPOReceipts == true ? (plantype.InclQtyPOReceipts ?? 0) * qty : 0m;
					receipts += target.InclQtyPOFixedReceipt == true ? (plantype.InclQtyPOFixedReceipts ?? 0) * qty : 0m;
					receipts += target.InclQtyINAssemblySupply == true ? (plantype.InclQtyINAssemblySupply ?? 0) * qty : 0m;
				}
				else
				{
					receipts -= target.InclQtySOBackOrdered == true ? (plantype.InclQtySOBackOrdered ?? 0) * qty : 0m;
					receipts -= target.InclQtySOPrepared == true ? (plantype.InclQtySOPrepared ?? 0) * qty : 0m;
					receipts -= target.InclQtySOBooked == true ? (plantype.InclQtySOBooked ?? 0) * qty : 0m;
					receipts -= target.InclQtySOShipped == true ? (plantype.InclQtySOShipped ?? 0) * qty : 0m;
					receipts -= target.InclQtySOShipping == true ? (plantype.InclQtySOShipping ?? 0) * qty : 0m;

                    receipts -= target.InclQtyFSSrvOrdPrepared == true ? (plantype.InclQtyFSSrvOrdPrepared ?? 0) * qty : 0m;
                    receipts -= target.InclQtyFSSrvOrdBooked == true ? (plantype.InclQtyFSSrvOrdBooked ?? 0) * qty : 0m;
                    receipts -= target.InclQtyFSSrvOrdAllocated == true ? (plantype.InclQtyFSSrvOrdAllocated ?? 0) * qty : 0m;
				}
			}
			
			if (plan.Reverse != true)
			{
				hardAvail -= (plantype.InclQtySOShipped ?? 0) * qty;
				hardAvail -= (plantype.InclQtySOShipping ?? 0) * qty;
				hardAvail -= (plantype.InclQtyINIssues ?? 0) * qty;
				hardAvail -= (plantype.InclQtyProductionAllocated ?? 0) * qty;

                hardAvail -= (plantype.InclQtyFSSrvOrdAllocated ?? 0) * qty;

				actual -= (plantype.InclQtySOShipped ?? 0) * qty;
			}

			if (InclQtyAvail == true)
			{
				target.QtyAvail += avail;
				target.QtyHardAvail += hardAvail;
				target.QtyActual += actual;
				if (receiptTarget != null)
				{
					receiptTarget.QtyOnReceipt += receipts;
				}
			}
			else if (InclQtyAvail == false)
			{
				target.QtyNotAvail += avail;
			}
            
			return target;
		}

		protected virtual INPlanType GetTargetPlanType<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
			where TNode : class, IQtyAllocatedBase
		{
			return GetTargetPlanTypeBase<TNode>(graph, plan, plantype);
		}

		public static INPlanType GetTargetPlanTypeBase<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
			where TNode : class, IQtyAllocatedBase
		{
			if (plan.ExcludePlanLevel != null && 
				(typeof(TNode) == typeof(SiteStatus) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeSite) == INPlanLevel.ExcludeSite ||
				typeof(TNode) == typeof(LocationStatus) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeLocation) == INPlanLevel.ExcludeLocation ||
				typeof(TNode) == typeof(LotSerialStatus) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeLocationLotSerial) == INPlanLevel.ExcludeLocationLotSerial ||
				typeof(TNode) == typeof(ItemLotSerial) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeLotSerial) == INPlanLevel.ExcludeLotSerial ||
				typeof(TNode) == typeof(SiteLotSerial) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeSiteLotSerial) == INPlanLevel.ExcludeSiteLotSerial))
			{
				return (INPlanType)0;
			}

			if (plan.IgnoreOrigPlan != true && !string.IsNullOrEmpty(plan.OrigPlanType))
			{
				if (typeof(TNode) == typeof(SiteStatus) ||
					typeof(TNode) == typeof(LocationStatus) && (plan.OrigPlanLevel & INPlanLevel.Location) == INPlanLevel.Location ||
					typeof(TNode) == typeof(LotSerialStatus) && (plan.OrigPlanLevel & INPlanLevel.LocationLotSerial) == INPlanLevel.LocationLotSerial ||
					typeof(TNode) == typeof(ItemLotSerial) && (plan.OrigPlanLevel & INPlanLevel.LotSerial) == INPlanLevel.LotSerial ||
					typeof(TNode) == typeof(SiteLotSerial) && (plan.OrigPlanLevel & INPlanLevel.LotSerial) == INPlanLevel.LotSerial)
				{
					INPlanType origPlanType = INPlanType.PK.Find(graph, plan.OrigPlanType);
					return plantype > 0 ? plantype - origPlanType : plantype + origPlanType;
				}
			}

			return plantype;
		}

		protected TNode UpdateAllocatedQuantities<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype, bool InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			INPlanType targettype = GetTargetPlanType<TNode>(graph, plan, plantype);
			return UpdateAllocatedQuantitiesBase<TNode>(graph, plan, targettype, InclQtyAvail);
		}

        public static TNode Sum<TNode>(TNode a, TNode b)
            where TNode : class, IQtyAllocatedBase, IBqlTable, new()
        {
            TNode ret = PXCache<TNode>.CreateCopy(a);

            ret.QtyOnHand += b.QtyOnHand;
            ret.QtyAvail += b.QtyAvail;
            ret.QtyHardAvail += b.QtyHardAvail;
            ret.QtyInTransit += b.QtyInTransit;
            ret.QtyNotAvail += b.QtyNotAvail;

            return ret;
        }

		public virtual void Plan_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null && ((INItemPlan)e.Row).InventoryID == null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void UpdateAllocatedQuantitiesWithPlan(PXCache sender, INItemPlan plan, bool revert = false)
        {
                INPlanType plantype = INPlanType.PK.Find(sender.Graph, plan.PlanType);
			plantype = revert ? -plantype : plantype;
			InventoryItem stkitem = InventoryItem.PK.Find(sender.Graph, plan.InventoryID);

                if (plan.InventoryID != null &&
                        plan.SubItemID != null &&
                        plan.SiteID != null &&
						stkitem != null && stkitem.StkItem == true)
                {
                    if (plan.LocationID != null)
                    {
                        LocationStatus item = UpdateAllocatedQuantities<LocationStatus>(sender.Graph, plan, plantype, true);
                        UpdateAllocatedQuantities<SiteStatus>(sender.Graph, plan, plantype, (bool)item.InclQtyAvail);
                        if (!string.IsNullOrEmpty(plan.LotSerialNbr))
                        {
                            UpdateAllocatedQuantities<LotSerialStatus>(sender.Graph, plan, plantype, true);
                            UpdateAllocatedQuantities<ItemLotSerial>(sender.Graph, plan, plantype, true);
                            UpdateAllocatedQuantities<SiteLotSerial>(sender.Graph, plan, plantype, true);
                        }
                    }
                    else
                    {
                        UpdateAllocatedQuantities<SiteStatus>(sender.Graph, plan, plantype, true);
                        if (!string.IsNullOrEmpty(plan.LotSerialNbr))
                        {
                            //TODO: check if LotSerialNbr was allocated on OrigPlanType
                            UpdateAllocatedQuantities<ItemLotSerial>(sender.Graph, plan, plantype, true);
                            UpdateAllocatedQuantities<SiteLotSerial>(sender.Graph, plan, plantype, true);
                        }
                    }
                }
            }


		public virtual void Plan_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
						{
			UpdateAllocatedQuantitiesWithPlan(sender, (INItemPlan)e.Row);
                        }

		public virtual void Plan_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
                        {                        
			UpdateAllocatedQuantitiesWithPlan(sender, (INItemPlan)e.Row, revert: true);
		}

		public virtual void Plan_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			UpdateAllocatedQuantitiesWithPlan(sender, (INItemPlan)e.Row);
			UpdateAllocatedQuantitiesWithPlan(sender, (INItemPlan)e.OldRow, revert: true);
		}

		public static INItemPlan FetchPlan<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly<Field>(data))
			{
				if (attr is INItemPlanIDAttribute)
				{
					return ((INItemPlanIDAttribute)attr).FetchPlan(sender, data);
				}
			}
			return null;
		}

		public virtual INItemPlan FetchPlan(PXCache sender, object orig_Row, bool ReturnCopy = true)
		{
			object key = sender.GetValue(orig_Row, _FieldOrdinal);
			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			INItemPlan info = null;

			if (key != null)
			{
				long id = Convert.ToInt64(key);
				if (id < 0)
				{
					info = new INItemPlan();
					info.PlanID = id;
                    info.InventoryID = ((IItemPlanMaster)orig_Row).InventoryID;
                    info.SiteID = ((IItemPlanMaster)orig_Row).SiteID;
					if (info != null)
					{
						info = (INItemPlan)cache.Locate(info);
					}
				}
				if (info == null)
				{
					if (id < 0)
					{
						foreach (INItemPlan plan in cache.Inserted)
						{
							if (plan.PlanID == id)
							{
								info = plan;
								break;
							}
						}
					}
					else
					{
						info = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, key);
					}
				}
				if (info == null)
				{
					key = null;
					sender.SetValue(orig_Row, _FieldOrdinal, null);
				}
				else if (ReturnCopy)
				{
					return PXCache<INItemPlan>.CreateCopy(info);
				}
			}

			return info;
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_ReleaseMode.Value) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			INItemPlan info = FetchPlan(sender, e.Row);

			if (info == null)
			{
				info = DefaultValuesInt(sender, new INItemPlan(), e.Row);
				if (info == null)
				{
					return;
				}

				info = (INItemPlan)cache.Insert(info);
				SetPlanID(sender, e.Row, info.PlanID);
			}
			else
			{
				INItemPlan old_info = PXCache<INItemPlan>.CreateCopy(info);
				info = DefaultValuesInt(sender, info, e.Row);
				if (info != null)
				{
					if (!cache.ObjectsEqual(info, old_info))
					{
						info.PlanID = null;
						cache.Delete(old_info);
						info = (INItemPlan)cache.Insert(info);
						SetPlanID(sender, e.Row, info.PlanID);
					}
					else
					{
						info = (INItemPlan)cache.Update(info);
					}
				}
			}
		}

		public static void RaiseRowUpdated(PXCache cache, object row, object oldrow)
		{
			foreach (INItemPlanIDAttribute attr in cache.GetAttributes(row, null).OfType<INItemPlanIDAttribute>())
				attr.RowUpdated(cache, new PXRowUpdatedEventArgs(row, oldrow, false));
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
            if (_ReleaseMode.Value) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			INItemPlan info = FetchPlan(sender, e.Row);

			if (info == null)
			{
				info = DefaultValuesInt(sender, new INItemPlan(), e.Row);
				if (info == null)
				{
					return;
				}

				info = (INItemPlan)cache.Insert(info);
				SetPlanID(sender, e.Row, info.PlanID);
			}
			else
			{
				INItemPlan old_info = PXCache<INItemPlan>.CreateCopy(info);
				info = DefaultValuesInt(sender, info, e.Row);
                if (info != null)
				{
					if (!cache.ObjectsEqual(info, old_info) || !string.Equals(info.LotSerialNbr, old_info.LotSerialNbr) && (!string.IsNullOrEmpty(info.LotSerialNbr) || !string.IsNullOrEmpty(old_info.LotSerialNbr)))
					{
						info.PlanID = null;
						cache.Delete(old_info);
						info = (INItemPlan)cache.Insert(info);
						SetPlanID(sender, e.Row, info.PlanID);

						foreach (INItemPlan demand_info in PXSelect<INItemPlan, Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(sender.Graph, old_info.PlanID))
						{
							demand_info.SupplyPlanID = info.PlanID;
							cache.MarkUpdated(demand_info);
						}
					}
					else
					{
						info = (INItemPlan)cache.Update(info);
					}
				}
				else
				{
					cache.Delete(old_info);
					ClearPlanID(sender, e.Row);
				}
			}
		}

		protected virtual void SetPlanID(PXCache sender, object row, long? planID)
		{
			sender.SetValue(row, _FieldOrdinal, planID);
		}

		protected virtual void ClearPlanID(PXCache sender, object row)
		{
			sender.SetValue(row, _FieldOrdinal, null);
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_ReleaseMode.Value) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			INItemPlan info = FetchPlan(sender, e.Row);

			if (info != null)
			{
				cache.Delete(info);
			}
		}

		public static bool IsSORelated(INPlanType plantype)
		{
			return plantype.InclQtySOBackOrdered.GetValueOrDefault() != 0
				|| plantype.InclQtySOBooked.GetValueOrDefault() != 0
				|| plantype.InclQtySODropShip.GetValueOrDefault() != 0
				|| plantype.InclQtySOFixed.GetValueOrDefault() != 0
				|| plantype.InclQtySOFixedProduction.GetValueOrDefault() != 0
				|| plantype.InclQtySOPrepared.GetValueOrDefault() != 0
				|| plantype.InclQtySOShipped.GetValueOrDefault() != 0
				|| plantype.InclQtySOShipping.GetValueOrDefault() != 0;
		}
		#endregion
	}

	public class INItemPlanIDSimpleAttribute : INItemPlanIDBaseAttribute
	{ 
		#region Ctor
		public INItemPlanIDSimpleAttribute()
		{
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (sender.Graph.IsImport || sender.Graph.UnattendedMode)
			{
				if (!sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
					sender.Graph.Views.Caches.Add(typeof(INItemPlan));
				if (sender.Graph.Views.Caches.Contains(_ItemType))
					sender.Graph.Views.Caches.Remove(_ItemType);
				sender.Graph.Views.Caches.Add(_ItemType);
			}
			else
			{
				//plan source should go before plan
				//to bind errors from INItemPlan.SubItemID -> SOLine.SubItemID or SOLineSplit.SubItemID
				if (!sender.Graph.Views.Caches.Contains(_ItemType))
					sender.Graph.Views.Caches.Add(_ItemType);
				if (!sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
					sender.Graph.Views.Caches.Add(typeof(INItemPlan));
			}
		}
		#endregion
	}

	public abstract class INItemPlanIDBaseAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		#region State
		protected long? _KeyToAbort;
		protected Type _ItemType;
		#endregion
		#region Ctor
		public INItemPlanIDBaseAttribute()
		{
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_ItemType = sender.GetItemType();

			if (!typeof(IItemPlanMaster).IsAssignableFrom(_ItemType))
			{
				throw new PXArgumentException(nameof(_ItemType), Messages.TypeMustImplementInterface, _ItemType.GetLongName(), typeof(IItemPlanMaster).GetLongName());
			}

			sender.Graph.RowPersisting.AddHandler<INItemPlan>(Plan_RowPersisting);
			sender.Graph.RowPersisted.AddHandler<INItemPlan>(Plan_RowPersisted);

			_persisted = new Dictionary<long?, object>();

			if (!sender.Graph.Views.RestorableCaches.Contains(typeof(INItemPlan)))
				sender.Graph.Views.RestorableCaches.Add(typeof(INItemPlan));
		}
		#endregion
		#region Implementation

		protected object _SelfKeyToAbort;
		protected Dictionary<long?, object> _persisted;

		public virtual void Plan_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_SelfKeyToAbort = sender.GetValue<INItemPlan.planID>(e.Row);
			}
		}

		Dictionary<long?, object> _inserted = null;
		Dictionary<long?, object> _updated = null;

		public virtual void PlanForItem_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				PXCache cache = sender.Graph.Caches[_ItemType];
				long? NewKey;

				if (e.TranStatus == PXTranStatus.Open && _SelfKeyToAbort != null)
				{
					NewKey = (long?)sender.GetValue<INItemPlan.planID>(e.Row);

					if (!_persisted.ContainsKey(NewKey))
					{
						_persisted.Add(NewKey, _SelfKeyToAbort);
					}

					if (_inserted == null)
					{
						_inserted = new Dictionary<long?, object>();

						foreach (object item in cache.Inserted)
						{
							if (cache.GetValue(item, _FieldOrdinal) != null)
								_inserted.Add((long?)cache.GetValue(item, _FieldOrdinal), item);
						}
					}

					object row;
					if (_inserted.TryGetValue((long?)_SelfKeyToAbort, out row))
					{
						cache.SetValue(row, _FieldOrdinal, NewKey);
					}

					if (_updated == null)
					{
						_updated = new Dictionary<long?, object>();

						foreach (object item in cache.Updated)
						{
							if (cache.GetValue(item, _FieldOrdinal) != null)
								_updated.Add((long?)cache.GetValue(item, _FieldOrdinal), item);
						}
					}

					if (_updated.TryGetValue((long?)_SelfKeyToAbort, out row))
					{
						cache.SetValue(row, _FieldOrdinal, NewKey);
					}
				}

				if (e.TranStatus == PXTranStatus.Aborted)
				{
					foreach (object item in cache.Inserted)
					{
						if ((NewKey = (long?)cache.GetValue(item, _FieldOrdinal)) != null && _persisted.TryGetValue(NewKey, out _SelfKeyToAbort))
						{
							cache.SetValue(item, _FieldOrdinal, _SelfKeyToAbort);
						}
					}

					foreach (object item in cache.Updated)
					{
						if ((NewKey = (long?)cache.GetValue(item, _FieldOrdinal)) != null && _persisted.TryGetValue(NewKey, out _SelfKeyToAbort))
						{
							cache.SetValue(item, _FieldOrdinal, _SelfKeyToAbort);
						}
					}
				}

				if (e.TranStatus == PXTranStatus.Completed || e.TranStatus == PXTranStatus.Aborted)
				{
					_inserted = null;
					_updated = null;
				}
			}
		}

		Dictionary<long?, List<object>> _selfinserted = null;
		Dictionary<long?, List<object>> _selfupdated = null;

		public virtual void PlanForSupply_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				long? NewKey;
				List<object> references;
				if (e.TranStatus == PXTranStatus.Open && _SelfKeyToAbort != null)
				{
					NewKey = (long?)sender.GetValue<INItemPlan.planID>(e.Row);
					if (_selfinserted == null)
					{
						_selfinserted = new Dictionary<long?, List<object>>();

						foreach (object item in sender.Inserted)
						{
							long? SupplyPlanID = (long?)sender.GetValue<INItemPlan.supplyPlanID>(item);
							if (SupplyPlanID != null)
							{
								if (!_selfinserted.TryGetValue(SupplyPlanID, out references))
								{
									_selfinserted[SupplyPlanID] = references = new List<object>();
								}
								references.Add(item);
							}
						}
					}

					if (_selfinserted.TryGetValue((long?)_SelfKeyToAbort, out references))
					{
						foreach (object item in references)
						{
							sender.SetValue<INItemPlan.supplyPlanID>(item, NewKey);
						}
					}

					if (_selfupdated == null)
					{
						_selfupdated = new Dictionary<long?, List<object>>();

						foreach (object item in sender.Updated)
						{
							long? SupplyPlanID = (long?)sender.GetValue<INItemPlan.supplyPlanID>(item);
							if (SupplyPlanID != null)
							{
								if (!_selfupdated.TryGetValue(SupplyPlanID, out references))
								{
									_selfupdated[SupplyPlanID] = references = new List<object>();
								}
								references.Add(item);
							}
						}
					}

					if (_selfupdated.TryGetValue((long?)_SelfKeyToAbort, out references))
					{
						foreach (object item in references)
						{
							sender.SetValue<INItemPlan.supplyPlanID>(item, NewKey);
						}
					}
				}
				else if (e.TranStatus == PXTranStatus.Aborted)
				{
					foreach (INItemPlan poplan in sender.Inserted)
					{
						if (poplan.SupplyPlanID != null && _persisted.TryGetValue(poplan.SupplyPlanID, out _SelfKeyToAbort))
						{
							poplan.SupplyPlanID = (long?)_SelfKeyToAbort;
						}
					}

					foreach (INItemPlan poplan in sender.Updated)
					{
						if (poplan.SupplyPlanID != null && _persisted.TryGetValue(poplan.SupplyPlanID, out _SelfKeyToAbort))
						{
							poplan.SupplyPlanID = (long?)_SelfKeyToAbort;
						}
					}
				}
			}

			if (e.TranStatus == PXTranStatus.Completed || e.TranStatus == PXTranStatus.Aborted)
			{
				_selfinserted = null;
				_selfupdated = null;
			}
		}

		public virtual void Plan_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PlanForSupply_RowPersisted(sender, e);
			PlanForItem_RowPersisted(sender, e);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			object key = sender.GetValue(e.Row, _FieldOrdinal);

			if (key != null)
			{
				if (Convert.ToInt64(key) < 0L)
				{
                    bool isplanfound = false;
					foreach (INItemPlan data in cache.Inserted)
					{
						if (Equals(key, data.PlanID))
						{
                            isplanfound = true;
							_KeyToAbort = data.PlanID;
							try
							{
							cache.PersistInserted(data);
							}
							catch (PXOuterException ex)
							{
								_KeyToAbort = null;
								for (int i = 0; i < ex.InnerFields.Length; i++)
								{
									if (sender.RaiseExceptionHandling(ex.InnerFields[i], e.Row, null, new PXSetPropertyKeepPreviousException(ex.InnerMessages[i])))
									{
										throw new PXRowPersistingException(ex.InnerFields[i], null, ex.InnerMessages[i]);
									}
								}
								return;
							}
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());
							sender.SetValue(e.Row, _FieldOrdinal, id);
							data.PlanID = id;
							cache.Normalize();
							break;
						}
					}
                    if (!isplanfound && sender.GetStatus(e.Row) != PXEntryStatus.Deleted && sender.GetStatus(e.Row) != PXEntryStatus.InsertedDeleted)
                        throw new PXException(Messages.InvalidPlan);
				}
				else
				{
					foreach (INItemPlan data in cache.Updated)
					{
						if (Equals(key, data.PlanID))
						{
							cache.PersistUpdated(data);
							break;
						}
					}
				}
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];

			if (e.TranStatus == PXTranStatus.Aborted)
			{
				if (_KeyToAbort != null && _KeyToAbort < 0L)
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
					foreach (INItemPlan data in cache.Inserted)
					{
						if (Equals(key, data.PlanID))
						{
							data.PlanID = _KeyToAbort;
							break;
						}
					}
				}
				else
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					foreach (INItemPlan data in cache.Updated)
					{
						if (Equals(key, data.PlanID))
						{
							cache.ResetPersisted(data);
						}
					}
				}
				cache.Normalize();
			}
			else if (e.TranStatus == PXTranStatus.Completed)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				foreach (INItemPlan data in cache.Inserted)
				{
					if (Equals(key, data.PlanID))
					{
						cache.SetStatus(data, PXEntryStatus.Notchanged);
						PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
						cache.ResetPersisted(data);
					}
				}
				foreach (INItemPlan data in cache.Updated)
				{
					if (Equals(key, data.PlanID))
					{
						cache.SetStatus(data, PXEntryStatus.Notchanged);
						PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
						cache.ResetPersisted(data);
					}
				}
				//cache.IsDirty = false;
				cache.Normalize();
				_KeyToAbort = null;
			}
		}
		#endregion
	}


    #endregion

    #region INUnitType

    public class INUnitType
	{
		public const short Global = 3;
		public const short ItemClass = 2;
		public const short InventoryItem = 1;

		public class global : PX.Data.BQL.BqlShort.Constant<global>
		{
			public global() : base(Global) { ;}
		}
		public class itemClass : PX.Data.BQL.BqlShort.Constant<itemClass>
		{
			public itemClass() : base(ItemClass) { ;}
		}
		public class inventoryItem : PX.Data.BQL.BqlShort.Constant<inventoryItem>
		{
			public inventoryItem() : base(InventoryItem) { ;}
		}
	}

	#endregion

	#region Conversion info

	public class ConversionInfo
	{
		public INUnit Conversion { get; }

		public short Type
		{
			get { return Conversion.UnitType ?? 0; }
		}

		public InventoryItem Inventory { get; }
		
		public ConversionInfo(INUnit conversion)
		{
			Conversion = conversion;
		}

		public ConversionInfo(INUnit conversion, InventoryItem inventory):this(conversion)
		{
			Inventory = inventory;
		}
	}

	/// <summary>
	/// Type of unit for <see cref="InventoryItem">InventoryItem<see/>.
	/// Corresponds to type(purpose) of units for InventoryItem as <see cref="InventoryItem.BaseUnit"/>, <see cref="InventoryItem.SalesUnit"/> or <see cref="InventoryItem.PurchaseUnit"/>
	/// </summary>
	[Flags]
	public enum InventoryUnitType : byte
	{
		/// <summary>
		/// Default(unknown) type of unit
		/// </summary>
		None = 0,
		/// <summary>
		/// Corresponds to unit which set as <see cref="InventoryItem.BaseUnit"/>
		/// </summary>
		BaseUnit = 1,
		/// <summary>
		/// Corresponds to unit which set as <see cref="InventoryItem.SalesUnit"/>
		/// </summary>
		SalesUnit = 2,
		/// <summary>
		/// Corresponds to unit which set as <see cref="InventoryItem.PurchaseUnit"/>
		/// </summary>
		PurchaseUnit = 4
	}

	#endregion

	#region AcctSub2Attribute

	public class AcctSub2Attribute : AcctSubAttribute
	{
		public PXSelectorAttribute SelectorAttr => _SelAttrIndex == -1 ? null : (PXSelectorAttribute) _Attributes[_SelAttrIndex];

		public override Type DescriptionField
		{
			get { return SelectorAttr?.DescriptionField; }
			set
			{
				if (SelectorAttr != null)
					SelectorAttr.DescriptionField = value;
			}
		}

		public override bool DirtyRead
		{
			get { return SelectorAttr?.DirtyRead ?? false; }
			set
			{
				if (SelectorAttr != null)
					SelectorAttr.DirtyRead = value;
			}
		}

		public override bool CacheGlobal
		{
			get { return SelectorAttr?.CacheGlobal ?? false; }
			set
			{
				if (SelectorAttr != null)
					SelectorAttr.CacheGlobal = value;
			}
		}


	}

	#endregion

	#region INAcctSubDefault

	public class INAcctSubDefault
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;
			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels) : base(AllowedValues, AllowedLabels) {}
			public CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) {}
			}

		public class ClassListAttribute : CustomListAttribute
		{
			public ClassListAttribute() : base(
				new[]
			{
					Pair(MaskItem, Messages.MaskItem),
					Pair(MaskSite, Messages.MaskSite),
					Pair(MaskClass, Messages.MaskClass),
				}) {}
		}

		public class ReasonCodeListAttribute : CustomListAttribute
		{
			public ReasonCodeListAttribute() : base(
				new[]
			{
					Pair(MaskReasonCode, Messages.MaskReasonCode),
					Pair(MaskItem, Messages.MaskItem),
					Pair(MaskSite, Messages.MaskSite),
					Pair(MaskClass, Messages.MaskClass),
				}) {}
		}

        public class POAccrualListAttribute : CustomListAttribute
        {
			public POAccrualListAttribute() : base(
				new[]
            {
					Pair(MaskItem, Messages.MaskItem),
					Pair(MaskSite, Messages.MaskSite),
					Pair(MaskClass, Messages.MaskClass),
					Pair(MaskVendor, Messages.MaskVendor),
				}) {}
        }

		public const string MaskReasonCode = "0";
		public const string MaskItem = "I";
		public const string MaskSite = "W";
		public const string MaskClass = "P";
        public const string MaskVendor = "V";

		public static void Required(PXCache sender, PXRowSelectedEventArgs e)
		{
			AcctSubRequired required = new AcctSubRequired(sender, e);
		}

		public static void Required(PXCache sender, PXRowPersistingEventArgs e)
		{
			AcctSubRequired required = new AcctSubRequired(sender, e);
		}

		public class AcctSubRequired
		{
			protected enum AcctSubDefaultClass { FromItem = 0, FromSite = 1, FromClass = 2 }
			protected enum AcctSubDefaultReasonCode { FromReasonCode = 0, FromItem = 1, FromSite = 2, FromClass = 3 }

			#region State
			public bool InvtAcct = false;
			public bool InvtSub = false;
			public bool SalesAcct = false;
			public bool SalesSub = false;
			public bool COGSAcct = false;
			public bool COGSSub = false;
			public bool ReasonCodeSub = false;
			public bool StdCstVarAcct = false;
			public bool StdCstVarSub = false;
			public bool StdCstRevAcct = false;
			public bool StdCstRevSub = false;
			public bool POAccrualAcct = false;
			public bool POAccrualSub = false;

			protected string[] _sources = new ClassListAttribute().AllowedValues;
			protected string[] _rcsources = new ReasonCodeListAttribute().AllowedValues;
			#endregion

			#region Initialization
			protected virtual void Populate(INPostClass postclass, AcctSubDefaultClass option)
			{
				if (postclass != null)
				{
					InvtAcct = InvtAcct || (postclass.InvtAcctDefault == _sources[(int)option]);
					InvtSub = InvtSub || (string.IsNullOrEmpty(postclass.InvtSubMask) == false && postclass.InvtSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					SalesAcct = SalesAcct || (postclass.SalesAcctDefault == _sources[(int)option]);
					SalesSub = SalesSub || (string.IsNullOrEmpty(postclass.SalesSubMask) == false && postclass.SalesSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					COGSAcct = COGSAcct || (postclass.COGSAcctDefault == _sources[(int)option]);
					COGSSub = COGSSub || (postclass.COGSSubFromSales == false && string.IsNullOrEmpty(postclass.COGSSubMask) == false && postclass.COGSSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					StdCstVarAcct = StdCstVarAcct || (postclass.StdCstVarAcctDefault == _sources[(int)option]);
					StdCstVarSub = StdCstVarSub || (string.IsNullOrEmpty(postclass.StdCstVarSubMask) == false && postclass.StdCstVarSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					StdCstRevAcct = StdCstRevAcct || (postclass.StdCstRevAcctDefault == _sources[(int)option]);
					StdCstRevSub = StdCstRevSub || (string.IsNullOrEmpty(postclass.StdCstRevSubMask) == false && postclass.StdCstRevSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					POAccrualAcct = POAccrualAcct || (postclass.POAccrualAcctDefault == _sources[(int)option]);
					POAccrualSub = POAccrualSub || (string.IsNullOrEmpty(postclass.POAccrualSubMask) == false && postclass.POAccrualSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);
				}
			}

			protected virtual void Populate(ReasonCode reasoncode, AcctSubDefaultReasonCode option)
			{
				if (reasoncode != null)
				{
					ReasonCodeSub = ReasonCodeSub || (string.IsNullOrEmpty(reasoncode.SubMask) == false && reasoncode.SubMask.IndexOf(char.Parse(_rcsources[(int)option])) > -1);
				}
			}
			#endregion

			#region Ctor
			public AcctSubRequired(PXCache sender, object data)
			{
				if (sender.GetItemType() == typeof(InventoryItem))
				{
					PXCache cache = sender.Graph.Caches[typeof(INPostClass)];
					Populate((INPostClass)cache.Current, AcctSubDefaultClass.FromItem);

					StdCstVarAcct = StdCstVarAcct && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstVarSub = StdCstVarSub && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstRevAcct = StdCstRevAcct && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstRevSub = StdCstRevSub && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromItem);
					}
				}

				else if (sender.GetItemType() == typeof(INPostClass))
				{
					//class accounts are used for defaulting, combine requirements.
					Populate((INPostClass)data, AcctSubDefaultClass.FromClass);

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromClass);
					}
				}

				else if (sender.GetItemType() == typeof(INSite) && PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
				{
					foreach (INPostClass postclass in PXSelectReadonly<INPostClass>.Select(sender.Graph))
					{
						Populate(postclass, AcctSubDefaultClass.FromSite);
					}

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromSite);
					}
				}
			}

			public AcctSubRequired(PXCache sender, PXRowSelectedEventArgs e)
				: this(sender, e.Row)
			{
				OnRowSelected(sender, e);
			}

			public AcctSubRequired(PXCache sender, PXRowPersistingEventArgs e)
				: this(sender, e.Row)
			{
				OnRowPersisting(sender, e);
			}
			#endregion

			#region Implementation
			public virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				PXUIFieldAttribute.SetRequired<INPostClass.invtAcctID>(sender, this.InvtAcct || this.InvtSub);
				PXUIFieldAttribute.SetRequired<INPostClass.invtSubID>(sender, this.InvtSub);
				PXUIFieldAttribute.SetRequired<INPostClass.salesAcctID>(sender, this.SalesAcct || this.SalesSub);
				PXUIFieldAttribute.SetRequired<INPostClass.salesSubID>(sender, this.SalesSub);
				PXUIFieldAttribute.SetRequired<INPostClass.cOGSAcctID>(sender, this.COGSAcct || this.COGSSub);
				PXUIFieldAttribute.SetRequired<INPostClass.cOGSSubID>(sender, this.COGSSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstVarAcctID>(sender, this.StdCstVarAcct || this.StdCstVarSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstVarSubID>(sender, this.StdCstVarSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstRevAcctID>(sender, this.StdCstRevAcct || this.StdCstRevSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstRevSubID>(sender, this.StdCstRevSub);
				PXUIFieldAttribute.SetRequired<INPostClass.pOAccrualAcctID>(sender, this.POAccrualAcct || this.POAccrualSub);
				PXUIFieldAttribute.SetRequired<INPostClass.pOAccrualSubID>(sender, this.POAccrualSub);
				PXUIFieldAttribute.SetRequired<INPostClass.reasonCodeSubID>(sender, this.ReasonCodeSub);
			}

			public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
				where Field : IBqlField
			{
				if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
				{
					throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
				}
			}

			public virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
				{
					if (this.InvtAcct && sender.GetValue<INPostClass.invtAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.invtAcctID>(sender, e.Row);
					}
					if (this.InvtSub && sender.GetValue<INPostClass.invtSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.invtSubID>(sender, e.Row);
					}
					if (this.SalesAcct && sender.GetValue<INPostClass.salesAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.salesAcctID>(sender, e.Row);
					}
					if (this.SalesSub && sender.GetValue<INPostClass.salesSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.salesSubID>(sender, e.Row);
					}
					if (this.COGSAcct && sender.GetValue<INPostClass.cOGSAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.cOGSAcctID>(sender, e.Row);
					}
					if (this.COGSSub && sender.GetValue<INPostClass.cOGSSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.cOGSSubID>(sender, e.Row);
					}
					if (this.StdCstVarAcct && sender.GetValue<INPostClass.stdCstVarAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstVarAcctID>(sender, e.Row);
					}
					if (this.StdCstVarSub && sender.GetValue<INPostClass.stdCstVarSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstVarSubID>(sender, e.Row);
					}
					if (this.StdCstRevAcct && sender.GetValue<INPostClass.stdCstRevAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstRevAcctID>(sender, e.Row);
					}
					if (this.StdCstRevSub && sender.GetValue<INPostClass.stdCstRevSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstRevSubID>(sender, e.Row);
					}
					if (this.POAccrualAcct && sender.GetValue<INPostClass.pOAccrualAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.pOAccrualAcctID>(sender, e.Row);
					}
					if (this.POAccrualSub && sender.GetValue<INPostClass.pOAccrualSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.pOAccrualSubID>(sender, e.Row);
					}
					if (this.ReasonCodeSub && sender.GetValue<INPostClass.reasonCodeSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.reasonCodeSubID>(sender, e.Row);
					}
				}
			}
			#endregion
		}
	}

	#endregion

	#region SubAccountMaskAttribute

	[PXDBString(30, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZZ";

		public SubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, INAcctSubDefault.MaskItem, new INAcctSubDefault.ClassListAttribute().AllowedValues, new INAcctSubDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, params object[] sources)
			where Field : IBqlField
		{
			return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			if (string.IsNullOrEmpty(mask))
			{
				object newval;
				graph.Caches[BqlCommand.GetItemType(typeof(Field))].RaiseFieldDefaulting<Field>(null, out newval);
				mask = (string)newval;
			}

			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(ex, new INAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, bool? stkItem, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			if (string.IsNullOrEmpty(mask))
			{
				object newval;
				graph.Caches[BqlCommand.GetItemType(typeof(Field))].RaiseFieldDefaulting<Field>(null, out newval);
				mask = (string)newval;
			}

			try
			{
					return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, stkItem, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(ex, new INAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

    #region POAccrualSubAccountMaskAttribute

    [PXDBString(30, InputMask = "")]
    [PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    public sealed class POAccrualSubAccountMaskAttribute : AcctSubAttribute
    {
        private const string _DimensionName = "SUBACCOUNT";
				private const string _MaskName = "ZZZZZZZZZX";
        public POAccrualSubAccountMaskAttribute()
            : base()
        {
            PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, INAcctSubDefault.MaskItem, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, new INAcctSubDefault.POAccrualListAttribute().AllowedLabels);
            attr.ValidComboRequired = false;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, params object[] sources)
            where Field : IBqlField
        {
            return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, sources);
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
            where Field : IBqlField
        {
            if (string.IsNullOrEmpty(mask))
            {
                object newval;
                graph.Caches[typeof(Field).DeclaringType].RaiseFieldDefaulting<Field>(null, out newval);
                mask = (string)newval;
            }

            try
            {
                return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, sources);
            }
            catch (PXMaskArgumentException ex)
            {
                PXCache cache = graph.Caches[fields[ex.SourceIdx].DeclaringType];
                string fieldName = fields[ex.SourceIdx].Name;
                throw new PXMaskArgumentException(ex, new INAcctSubDefault.POAccrualListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
            }
        }
    }

    #endregion

	#region ReasonCodeSubAccountMaskAttribute

	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class ReasonCodeSubAccountMaskAttribute : AcctSubAttribute
	{
		public const string ReasonCode = "R";
		public const string InventoryItem = "I";
		public const string PostingClass = "P";
		public const string Warehouse = "W";

		private static readonly string[] writeOffValues = new string[] { ReasonCode, InventoryItem, Warehouse, PostingClass };
		private static readonly string[] writeOffLabels = new string[] { CS.Messages.ReasonCode, Messages.InventoryItem, Messages.Warehouse, Messages.PostingClass };

		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ReasonCodeIN";
		public ReasonCodeSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, ReasonCode, writeOffValues, writeOffLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, writeOffValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(writeOffLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

	#region ILSDetail

	public interface ILSDetail : ILSMaster
	{
		Int32? SplitLineNbr
		{
			get;
			set;
		}
		string AssignedNbr
		{
			get;
			set;
		}
		string LotSerClassID
		{
			get;
			set;
		}
		bool? IsStockItem
		{
			get;
			set;
		}
	}

	#endregion

	#region ILSPrimary
	public interface ILSPrimary : ILSMaster
	{
		decimal? UnassignedQty
		{
			get;
			set;
		}
		bool? HasMixedProjectTasks { get; set;  }
	}
	#endregion

	#region ILSMaster

	public interface ILSMaster : IItemPlanMaster
	{
		string TranType
		{
			get;
		}
		DateTime? TranDate 
		{ 
			get; 
		}
		Int16? InvtMult
		{
			get;
			set;
		}
	    new Int32? InventoryID
		{
			get;
			set;
		}
	    new Int32? SiteID
		{
			get;
			set;
		}
		Int32? LocationID
		{
			get;
			set;
		}
		Int32? SubItemID
		{
			get;
			set;
		}
		string LotSerialNbr
		{
			get;
			set;
		}
		DateTime? ExpireDate
		{
			get;
			set;
		}
		string UOM
		{
			get;
			set;
		}
		Decimal? Qty
		{
			get;
			set;
		}
		decimal? BaseQty
		{
			get;
			set;
		}
		int? ProjectID { get; set; }
		int? TaskID { get; set; }
	}

	#endregion

    #region IItemPlanMaster
    public interface IItemPlanMaster
    {
        Int32? InventoryID
        {
            get;
            set;
        }
        Int32? SiteID
        {
            get;
            set;
        }
    }
    #endregion

    #region INUnitSelect2

    public class INUnitSelect2<Table, itemClassID, salesUnit, purchaseUnit, baseUnit, lotSerClass> : PXSelect<INUnit, Where<INUnit.itemClassID, Equal<Optional<itemClassID>>, And<INUnit.toUnit, Equal<Optional<baseUnit>>, And<INUnit.fromUnit, NotEqual<Optional<baseUnit>>>>>>
		where Table : INUnit
		where itemClassID : IBqlField
		where salesUnit : IBqlField
		where purchaseUnit : IBqlField
		where baseUnit : IBqlField
		where lotSerClass : IBqlField
	{
		#region State
		protected PXCache TopCache;
		#endregion

		#region Ctor
		public INUnitSelect2(PXGraph graph)
			: base(graph)
		{
			TopCache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(itemClassID))];

			graph.FieldVerifying.AddHandler<salesUnit>(SalesUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<purchaseUnit>(PurchaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<baseUnit>(BaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<lotSerClass>(LotSerClass_FieldVerifying);

			graph.FieldUpdated.AddHandler<salesUnit>(SalesUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<purchaseUnit>(PurchaseUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<baseUnit>(BaseUnit_FieldUpdated);

			graph.RowInserted.AddHandler(TopCache.GetItemType(), Top_RowInserted);
			graph.RowPersisting.AddHandler(TopCache.GetItemType(), Top_RowPersisting);

			graph.FieldDefaulting.AddHandler<INUnit.itemClassID>(INUnit_ItemClassID_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.itemClassID>(INUnit_ItemClassID_FieldVerifying);
			graph.FieldDefaulting.AddHandler<INUnit.toUnit>(INUnit_ToUnit_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldVerifying);
            graph.FieldVerifying.AddHandler<INUnit.unitRate>(INUnit_UnitRate_FieldVerifying);
			graph.RowSelected.AddHandler<INUnit>(INUnit_RowSelected);
			graph.RowPersisting.AddHandler<INUnit>(INUnit_RowPersisting);
			graph.RowPersisted.AddHandler<INUnit>(INUnit_RowPersisted);

            if (this.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
			{
				graph.FieldVerifying.AddHandler<INUnit.fromUnit>(INUnit_FromUnit_FieldVerifying);
			}
			else
            {
                graph.ExceptionHandling.AddHandler<salesUnit>((sender, e) => { e.Cancel = true; });
                graph.ExceptionHandling.AddHandler<purchaseUnit>((sender, e) => { e.Cancel = true; });
		}
		}
		#endregion

		#region Implementation
		protected object TopGetValue<Field>(object data)
			where Field : IBqlField
		{
			if (BqlCommand.GetItemType(typeof(Field)) == TopCache.GetItemType() || TopCache.GetItemType().IsAssignableFrom(BqlCommand.GetItemType(typeof(Field))))
			{
				return this.TopCache.GetValue<Field>(data);
			}
			else
			{
				PXCache cache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(Field))];
				return cache.GetValue<Field>(cache.Current);
			}
		}

		protected DataType TopGetValue<Field, DataType>(object data)
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>(data);
		}

		protected object TopGetValue<Field>()
			where Field : IBqlField
		{
			return TopGetValue<Field>(this.TopCache.Current);
		}

		protected DataType TopGetValue<Field, DataType>()
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>();
		}

		protected virtual void SalesUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SalesUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InsertConversionIfNotExists<salesUnit>(sender, e.Row);
		}

		protected virtual void PurchaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PurchaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InsertConversionIfNotExists<purchaseUnit>(sender, e.Row);
		}

		protected virtual void BaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		private void InsertConversionIfNotExists<TFromUnit>(PXCache sender, object row) where TFromUnit : IBqlField
		{
			string fromUnit = TopGetValue<TFromUnit, string>(row);
			if (string.IsNullOrEmpty(fromUnit))
				return;
			int? itemClassID = TopGetValue<itemClassID, int?>(row);
			INUnit conv = INUnit.UK.ByItemClass.FindDirty(sender.Graph, itemClassID, fromUnit);
			if (conv == null)
			{
				conv = ResolveItemClassConversion(itemClassID, TopGetValue<baseUnit, string>(row), fromUnit);

				this.Cache.Insert(conv);
			}
		}

		protected virtual void LotSerClass_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INLotSerClass lotSerClass = INLotSerClass.PK.FindDirty(sender.Graph, (string)e.NewValue);
			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				foreach (INUnit unit in this.Select())
				{
					if (INUnitAttribute.IsFractional(unit))
					{
						this.Cache.MarkUpdated(unit);
						this.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(unit, ((INUnit)unit).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
					}
				}
			}
		}

		protected virtual void BaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var newValue = TopGetValue<baseUnit, string>(e.Row);
			if (string.Equals((string)e.OldValue, newValue) == false)
			{
				if (string.IsNullOrEmpty((string)e.OldValue) == false)
				{
					var itemClassId = TopGetValue<itemClassID, int?>(e.Row);
					INUnit baseunit = ResolveItemClassConversion(
						itemClassId,
						(string)e.OldValue,
						(string)e.OldValue);

					this.Cache.Delete(baseunit);

					foreach (INUnit oldunits in this.Select(itemClassId, (string)e.OldValue, (string)e.OldValue))
					{
						this.Cache.Delete(oldunits);
					}
				}

				if (string.IsNullOrEmpty(newValue) == false)
				{
				foreach (INUnit globalunit in PXSelect<INUnit, 
					Where<INUnit.unitType, Equal<INUnitType.global>, 
						And<INUnit.toUnit, Equal<Required<baseUnit>>, 
						And<INUnit.fromUnit, NotEqual<Required<baseUnit>>>>>>.Select(sender.Graph, newValue, newValue))
				{
					INUnit classunit = PXCache<INUnit>.CreateCopy(globalunit);
					classunit.ItemClassID = null;
					classunit.UnitType = null;
					classunit.RecordID = null;

					this.Cache.Insert(classunit);
				}
			}
			}

			if (string.IsNullOrEmpty(newValue) == false)
			{
				InsertConversionIfNotExists<baseUnit>(sender, e.Row);

				sender.RaiseFieldUpdated<salesUnit>(e.Row, TopGetValue<salesUnit>(e.Row));
				sender.RaiseFieldUpdated<purchaseUnit>(e.Row, TopGetValue<purchaseUnit>(e.Row));
			}
		}

		protected virtual void Top_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (string.IsNullOrEmpty(TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Cache))
				{
					sender.RaiseFieldUpdated<baseUnit>(e.Row, null);
				}
			}
		}

		protected virtual void Top_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				sender.RaiseFieldUpdated<baseUnit>(e.Row, TopGetValue<baseUnit>(e.Row));
			}
		}

		protected virtual void INUnit_ToUnit_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				((INUnit)e.Row).SampleToUnit = TopGetValue<baseUnit, string>();
				e.NewValue = TopGetValue<baseUnit, string>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_ItemClassID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<itemClassID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_ItemClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<itemClassID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.ItemClass;
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.ItemClass;
				e.Cancel = true;
			}
		}

        protected virtual void INUnit_UnitRate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            Decimal? conversion = (Decimal?)e.NewValue;
            if (conversion <= 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_GT, "0");
            }
        }

		protected virtual void INUnit_FromUnit_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var conversion = (INUnit)e.Row;
			if (conversion == null || !e.ExternalCall)
				return;
			if (!string.IsNullOrEmpty((string)e.NewValue) && (string)e.NewValue == TopGetValue<baseUnit, string>())
				throw new PXSetPropertyException(Messages.FromUnitCouldNotBeEqualBaseUnit);
		}

		protected virtual void INUnit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INLotSerClass lotSerClass = ReadLotSerClass();

			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, ((INUnit)e.Row).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
			}
			else
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, null, null);
			}
		}

		protected virtual void INUnit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var inUnit = (INUnit)e.Row;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(INLotSerClass)];

				if (cache.Current != null && ((INLotSerClass)cache.Current).LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
				{
					sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, inUnit.UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && inUnit.ItemClassID < 0 && TopCache.Current != null)
			{
				int? _KeyToAbort = TopGetValue<itemClassID, Int32?>();
				if (!_persisted.ContainsKey(_KeyToAbort))
				{
					_persisted.Add(_KeyToAbort, inUnit.ItemClassID);
				}
				inUnit.ItemClassID = _KeyToAbort;
				sender.Normalize();
			}

			if ((e.Operation & PXDBOperation.Command).IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				if (inUnit.UnitType == INUnitType.InventoryItem && inUnit.InventoryID < 0)
				{
					throw new PXInvalidOperationException(CS.Messages.FieldShouldNotBeNegative, PXUIFieldAttribute.GetDisplayName<INUnit.inventoryID>(sender));
				}

				if (inUnit.UnitType == INUnitType.ItemClass && inUnit.ItemClassID < 0)
				{
					throw new PXInvalidOperationException(CS.Messages.FieldShouldNotBeNegative, PXUIFieldAttribute.GetDisplayName<INUnit.itemClassID>(sender));
				}
			}
		}

		Dictionary<int?, int?> _persisted = new Dictionary<int?, int?>();

		protected virtual void INUnit_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				int? _KeyToAbort;
				if (_persisted.TryGetValue(((INUnit)e.Row).ItemClassID, out _KeyToAbort))
				{
					((INUnit)e.Row).ItemClassID = _KeyToAbort;
				}
			}
		}

		private INLotSerClass ReadLotSerClass()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(typeof(lotSerClass))];
			return INLotSerClass.PK.FindDirty(_Graph, (string)cache.GetValue(cache.Current, typeof(lotSerClass).Name));
		}

		private INUnit ResolveItemClassConversion(int? itemClassID, string baseUnit, string fromUnit) => new INUnit
		{
			UnitType = INUnitType.ItemClass,
			ItemClassID = itemClassID,
			InventoryID = 0,
			FromUnit = fromUnit,
			ToUnit = baseUnit,
			UnitRate = 1m,
			PriceAdjustmentMultiplier = 1m,
			UnitMultDiv = MultDiv.Multiply
		};

		#endregion
	}

	#endregion

	#region INUnitSelect

	public class INUnitSelect<Table, inventoryID, itemClassID, salesUnit, purchaseUnit, baseUnit, lotSerClass> : PXSelect<INUnit, Where<INUnit.inventoryID, Equal<Current<inventoryID>>, And<INUnit.toUnit, Equal<Optional<baseUnit>>, And<INUnit.fromUnit, NotEqual<Optional<baseUnit>>>>>>
		where Table : INUnit
		where inventoryID : IBqlField
		where itemClassID : IBqlField
		where salesUnit : IBqlField
		where purchaseUnit : IBqlField
		where baseUnit : IBqlField
		where lotSerClass : IBqlField
	{
		#region State
		protected PXCache TopCache;
		#endregion

		#region Ctor
		public INUnitSelect(PXGraph graph)
			: base(graph)
		{
			TopCache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(inventoryID))];

			graph.FieldVerifying.AddHandler<salesUnit>(SalesUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<purchaseUnit>(PurchaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<baseUnit>(BaseUnit_FieldVerifying);

			graph.FieldUpdated.AddHandler<salesUnit>(SalesUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<purchaseUnit>(PurchaseUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<baseUnit>(BaseUnit_FieldUpdated);
			graph.FieldVerifying.AddHandler<lotSerClass>(LotSerClass_FieldVerifying);
			graph.RowInserted.AddHandler(TopCache.GetItemType(), Top_RowInserted);
			graph.RowPersisting.AddHandler(TopCache.GetItemType(), Top_RowPersisting);

			graph.FieldDefaulting.AddHandler<INUnit.inventoryID>(INUnit_InventoryID_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.inventoryID>(INUnit_InventoryID_FieldVerifying);
			graph.RowPersisting.AddHandler<INUnit>(INUnit_RowPersisting);
			graph.RowPersisted.AddHandler<INUnit>(INUnit_RowPersisted);
			graph.FieldDefaulting.AddHandler<INUnit.toUnit>(INUnit_ToUnit_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldVerifying);
            graph.FieldVerifying.AddHandler<INUnit.unitRate>(INUnit_UnitRate_FieldVerifying);
			graph.RowSelected.AddHandler<INUnit>(INUnit_RowSelected);
            graph.RowInserting.AddHandler<INUnit>(INUnit_RowInserting);
			graph.RowInserted.AddHandler<INUnit>(INUnit_RowInserted);
			graph.RowDeleted.AddHandler<INUnit>(INUnit_RowDeleted);

			if (this.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
			{
				graph.FieldVerifying.AddHandler<INUnit.fromUnit>(INUnit_FromUnit_FieldVerifying);
			}
			else
            {
                graph.ExceptionHandling.AddHandler<salesUnit>((sender, e) => { e.Cancel = true; });
                graph.ExceptionHandling.AddHandler<purchaseUnit>((sender, e) => { e.Cancel = true; });
		}
		}
		#endregion

		#region Implementation
		protected object TopGetValue<Field>(object data)
			where Field : IBqlField
		{
			if (BqlCommand.GetItemType(typeof(Field)) == TopCache.GetItemType() || TopCache.GetItemType().IsAssignableFrom(BqlCommand.GetItemType(typeof(Field))))
			{
				return this.TopCache.GetValue<Field>(data);
			}
			else
			{
				PXCache cache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(Field))];
				return cache.GetValue<Field>(cache.Current);
			}
		}

		protected DataType TopGetValue<Field, DataType>(object data)
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>(data);
		}

		protected object TopGetValue<Field>()
			where Field : IBqlField
		{
			return TopGetValue<Field>(this.TopCache.Current);
		}

		protected DataType TopGetValue<Field, DataType>()
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>();
		}

		protected virtual void SalesUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SalesUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InsertConversion<salesUnit>(sender, e.Row, (string)e.OldValue);
		}

		protected virtual void PurchaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PurchaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InsertConversion<purchaseUnit>(sender, e.Row, (string)e.OldValue);
		}

		protected virtual void BaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		private void InsertConversion<TFromUnit>(PXCache cache, object row, string oldFromValue) where TFromUnit : IBqlField
		{
			var fromUnit = TopGetValue<TFromUnit, string>(row);
			if (!string.IsNullOrEmpty(fromUnit))
			{
				var inventoryID = TopGetValue<inventoryID, int?>(row);
				INUnit conv = INUnit.UK.ByInventory.FindDirty(cache.Graph, inventoryID, fromUnit);
				if (conv == null)
				{
					var toUnit = TopGetValue<baseUnit, string>(row);
					if ((conv = INUnit.UK.ByGlobal.FindDirty(cache.Graph, fromUnit, toUnit)) != null)
					{
						conv = PXCache<INUnit>.CreateCopy(conv);
						conv.UnitType = INUnitType.InventoryItem;
						conv.ItemClassID = 0;
						conv.InventoryID = inventoryID;
						conv.RecordID = null;
					}
					else
					{
						conv = ResolveInventoryConversion(inventoryID, fromUnit, toUnit);
					}
					this.Cache.Insert(conv);
				}
			}

			//try to delete conversions added when changing base unit copied from item class
			//if purchaseunit is not equal to oldvalue -> delete it
			if (string.IsNullOrEmpty(oldFromValue) == false
				&& string.Equals(oldFromValue, TopGetValue<purchaseUnit, string>(row)) == false
				&& string.Equals(oldFromValue, TopGetValue<salesUnit, string>(row)) == false
				&& string.Equals(oldFromValue, TopGetValue<baseUnit, string>(row)) == false)
			{
				INUnit oldConv = ResolveInventoryConversion(
					TopGetValue<inventoryID, int?>(row),
					oldFromValue,
					TopGetValue<baseUnit, string>(row));

				if (this.Cache.GetStatus(oldConv) == PXEntryStatus.Inserted)
				{
					this.Cache.Delete(oldConv);
				}
			}
		}

		protected virtual void LotSerClass_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INLotSerClass lotSerClass = INLotSerClass.PK.FindDirty(sender.Graph, (string)e.NewValue);
			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				foreach (INUnit unit in this.Select())
				{
					if (INUnitAttribute.IsFractional(unit))
					{
						this.Cache.MarkUpdated(unit);
						this.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(unit, unit.UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
					}
				}
			}
		}

		protected virtual void BaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var newValue = TopGetValue<baseUnit, string>(e.Row);
			if (string.Equals((string)e.OldValue, newValue) == false)
			{
				if (string.IsNullOrEmpty((string)e.OldValue) == false)
				{
					INUnit baseunit = ResolveInventoryConversion(
						TopGetValue<inventoryID, int?>(e.Row),
						(string)e.OldValue,
						(string)e.OldValue);

					this.Cache.Delete(baseunit);

					foreach (INUnit oldunits in this.Select((string)e.OldValue, (string)e.OldValue))
					{
						this.Cache.Delete(oldunits);
					}
				}

				if (string.IsNullOrEmpty(newValue) == false)
				{
				foreach (INUnit classunit in PXSelect<INUnit, 
					Where<INUnit.unitType, Equal<INUnitType.itemClass>, 
					And<INUnit.itemClassID, Equal<Current<itemClassID>>, 
						And<INUnit.toUnit, Equal<Required<baseUnit>>,
						And<INUnit.fromUnit, NotEqual<Required<baseUnit>>>>>>>.Select(sender.Graph, newValue, newValue))
				{
					INUnit itemunit = PXCache<INUnit>.CreateCopy(classunit);
					itemunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
					itemunit.ItemClassID = 0;
					itemunit.UnitType = INUnitType.InventoryItem;
					itemunit.RecordID = null;

					this.Cache.Insert(itemunit);
				}
			}
			}

			if (string.IsNullOrEmpty(newValue) == false)
			{
				INUnit baseunit = INUnit.UK.ByInventory.FindDirty(
					sender.Graph, 
					TopGetValue<inventoryID, int?>(e.Row), 
					newValue);

                if (baseunit == null)
                {
					baseunit = ResolveInventoryConversion(
						TopGetValue<inventoryID, int?>(e.Row),
						newValue,
						newValue);

					this.Cache.Insert(baseunit);
                }

				sender.RaiseFieldUpdated<salesUnit>(e.Row, TopGetValue<salesUnit, string>(e.Row));
				sender.RaiseFieldUpdated<purchaseUnit>(e.Row, TopGetValue<purchaseUnit, string>(e.Row));
			}
		}

		protected virtual void Top_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (string.IsNullOrEmpty(TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Cache))
				{
					sender.RaiseFieldUpdated<baseUnit>(e.Row, null);
				}
			}
		}

		protected virtual void Top_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				sender.RaiseFieldUpdated<baseUnit>(e.Row, TopGetValue<baseUnit, string>(e.Row));
			}
		}

		protected virtual void INUnit_ToUnit_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				((INUnit)e.Row).SampleToUnit = TopGetValue<baseUnit, string>();
				e.NewValue = TopGetValue<baseUnit, string>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_InventoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<inventoryID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<inventoryID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.InventoryItem;
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.InventoryItem;
				e.Cancel = true;
			}
		}

        protected virtual void INUnit_UnitRate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            Decimal? conversion = (Decimal?)e.NewValue;
            if (conversion <= 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_GT, "0");
            }
            
        }

		protected virtual void INUnit_FromUnit_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var conversion = (INUnit)e.Row;
			if (conversion == null || !e.ExternalCall)
				return;
			if (!string.IsNullOrEmpty((string)e.NewValue) && (string)e.NewValue == TopGetValue<baseUnit, string>())
				throw new PXSetPropertyException(Messages.FromUnitCouldNotBeEqualBaseUnit);
		}

		protected virtual void INUnit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var inUnit = (INUnit) e.Row;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(INLotSerClass)];

				if (cache.Current != null && ((INLotSerClass)cache.Current).LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
				{
					sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, inUnit.UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && inUnit.InventoryID < 0 && TopCache.Current != null)
			{
				int? _KeyToAbort = TopGetValue<inventoryID, Int32?>();
				if (!_persisted.ContainsKey(_KeyToAbort))
				{
					_persisted.Add(_KeyToAbort, inUnit.InventoryID);
				}
				inUnit.InventoryID = _KeyToAbort;
				sender.Normalize();
			}

			if ((e.Operation & PXDBOperation.Command).IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				if (inUnit.UnitType == INUnitType.InventoryItem && inUnit.InventoryID < 0)
				{
					throw new PXInvalidOperationException(CS.Messages.FieldShouldNotBeNegative, PXUIFieldAttribute.GetDisplayName<INUnit.inventoryID>(sender));
				}

				if (inUnit.UnitType == INUnitType.ItemClass && inUnit.ItemClassID < 0)
				{
					throw new PXInvalidOperationException(CS.Messages.FieldShouldNotBeNegative, PXUIFieldAttribute.GetDisplayName<INUnit.itemClassID>(sender));
				}
			}
		}

		Dictionary<int?, int?> _persisted = new Dictionary<int?, int?>();

		protected virtual void INUnit_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				int? _KeyToAbort;
				if (_persisted.TryGetValue(((INUnit)e.Row).InventoryID, out _KeyToAbort))
				{
					((INUnit)e.Row).InventoryID = _KeyToAbort;
				}
			}
		}

		protected virtual void INUnit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INLotSerClass lotSerClass = ReadLotSerClass();

			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, ((INUnit)e.Row).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
			}
			else
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, null, null);
			}
		}

        protected virtual void INUnit_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            INUnit unit = (INUnit)e.Row;
            if (unit != null && unit.ToUnit == null)
                e.Cancel = true;

			if (unit != null)
			{
				foreach (INUnit item in sender.Deleted)
				{
					if (sender.ObjectsEqual(item, unit))
					{
						//since this item (although previously was deleted) will eventually be updated restore tstamp and recordID fields:
						unit.RecordID = item.RecordID;
						unit.tstamp = item.tstamp;
						break;
					}
				}
			}
        }
        
        protected virtual void INUnit_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INUnit unit = (INUnit)e.Row;

			if (unit.FromUnit != null && unit.UnitType == INUnitType.InventoryItem)
			{
				INUnit global = INUnit.UK.ByGlobal.FindDirty(sender.Graph, unit.FromUnit, unit.FromUnit);
				if (global == null)
				{
					global = ResolveGlobalConversion(unit.FromUnit);

					sender.RaiseRowInserting(global);

					sender.SetStatus(global, PXEntryStatus.Inserted);
					sender.ClearQueryCacheObsolete();
				}
			}
		}

		protected virtual void INUnit_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			INUnit unit = (INUnit)e.Row;

			if (unit.UnitType == INUnitType.InventoryItem)
			{
				INUnit global = ResolveGlobalConversion(unit.FromUnit);

				if (sender.GetStatus(global) == PXEntryStatus.Inserted)
				{
					sender.SetStatus(global, PXEntryStatus.InsertedDeleted);
					sender.ClearQueryCacheObsolete();
				}
			}
		}

		private INLotSerClass ReadLotSerClass()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(typeof(lotSerClass))];
			return INLotSerClass.PK.FindDirty(_Graph, (string)cache.GetValue(cache.Current, typeof(lotSerClass).Name));
		}

		private INUnit ResolveInventoryConversion(int? inventoryID, string fromUnit, string baseUnit) => new INUnit
		{
			UnitType = INUnitType.InventoryItem,
			ItemClassID = 0,
			InventoryID = inventoryID,
			FromUnit = fromUnit,
			ToUnit = baseUnit,
			UnitRate = 1m,
			PriceAdjustmentMultiplier = 1m,
			UnitMultDiv = MultDiv.Multiply
		};

		private INUnit ResolveGlobalConversion(string fromUnit) => new INUnit
		{
			UnitType = INUnitType.Global,
			ItemClassID = 0,
			InventoryID = 0,
			FromUnit = fromUnit,
			ToUnit = fromUnit,
			UnitRate = 1m,
			PriceAdjustmentMultiplier = 1m,
			UnitMultDiv = MultDiv.Multiply
		};

		#endregion
	}

	#endregion

	#region LSParentAttribute

	public class LSParentAttribute : PXParentAttribute
	{
		public LSParentAttribute(Type selectParent)
			: base(selectParent)
		{
		}

		public new static object SelectParent(PXCache cache, object row, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr).ParentType == ParentType)
					{
						parents.Insert(0, attr);
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr).ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];
				PXView parentview = attr.GetParentSelect(cache);

				if (!(parentview.CacheGetItemType() == ParentType || ParentType.IsAssignableFrom(parentview.CacheGetItemType())))
				{
					return null;
				}

				//clear view cache
				parentview.Clear();
				return parentview.SelectSingleBound(new object[] { row });
			}
			return null;
		}
	}

	#endregion

	#region LSDynamicButton
	public class LSDynamicButton : PXDynamicButtonAttribute
	{
		public LSDynamicButton(string[] dynamicButtonNames, string[] dynamicButtonDisplayNames) :
			base(dynamicButtonNames, dynamicButtonDisplayNames)
		{ }

		public override List<PXActionInfo> GetDynamicActions(Type graphType, Type viewType)
		{
			List<PXActionInfo> actions = new List<PXActionInfo>();

			foreach (var action in base.GetDynamicActions(graphType, viewType))
				actions.Add(new PXActionInfo(string.Format("{0}_{1}", viewType.Name, action.Name), 
											 action.DisplayName,
											 GraphHelper.GetPrimaryCache(graphType.FullName).CacheType));
			return actions;
		}
	}
	#endregion

	#region LSSelect

    [Serializable]	
	public abstract class LSSelect
	{
        [Serializable]
        [PXHidden]
        public partial class LotSerOptions : IBqlTable
        {
            #region StartNumVal
            public abstract class startNumVal : PX.Data.BQL.BqlString.Field<startNumVal> { }
            protected string _StartNumVal;
            [PXDBString(30)]
            [PXUIField(DisplayName = "Start Lot/Serial Number", FieldClass="LotSerial")]
            public virtual string StartNumVal
            {
                get
                {
                    return _StartNumVal;
                }
                set
                {
                    _StartNumVal = value;
                }
            }
            #endregion
            #region UnassignedQty
            public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }
            protected decimal? _UnassignedQty;
            [PXDBDecimal]
            [PXUIField(DisplayName = "Unassigned Qty.", Enabled = false)]
            public virtual decimal? UnassignedQty
            {
                get
                {
                    return _UnassignedQty;
                }
                set
                {
                    _UnassignedQty = value;
                }
            }
            #endregion
            #region Qty
            public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
            protected decimal? _Qty;
            [PXDBDecimal]
			[PXUIField(DisplayName = "Quantity to Generate", FieldClass = "LotSerial")]
            public virtual decimal? Qty
            {
                get
                {
                    return _Qty;
                }
                set
                {
                    _Qty = value;
                }
            }
            #endregion
            #region AllowGenerate
            public abstract class allowGenerate : PX.Data.BQL.BqlBool.Field<allowGenerate> { }
            protected bool? _AllowGenerate;
            [PXDBBool]
            public virtual bool? AllowGenerate
            {
                get
                {
                    return _AllowGenerate;
                }
                set
                {
                    _AllowGenerate = value;
                }
            }
            #endregion
            #region IsSerial
            public abstract class isSerial : PX.Data.BQL.BqlBool.Field<isSerial> { }
            protected bool? _IsSerial;
            [PXDBBool]
            public virtual bool? IsSerial
            {
                get
                {
                    return _IsSerial;
                }
                set
                {
                    _IsSerial = value;
                }
            }
            #endregion
        }

		public class Counters
		{
			public int RecordCount;
			public decimal BaseQty;
			public Dictionary<DateTime?, int> ExpireDates = new Dictionary<DateTime?, int>();
			public int ExpireDatesNull;
			public DateTime? ExpireDate;
			public Dictionary<int?, int> SubItems = new Dictionary<int?, int>();
			public int SubItemsNull;
			public int? SubItem;
			public Dictionary<int?, int> Locations = new Dictionary<int?, int>();
			public int LocationsNull;
			public int? Location;
			public Dictionary<string, int> LotSerNumbers = new Dictionary<string, int>();
			public int LotSerNumbersNull;
			public string LotSerNumber;
			public int UnassignedNumber;
			public Dictionary<KeyValuePair<int?, int?>, int> ProjectTasks = new Dictionary<KeyValuePair<int?, int?>, int>();
			public int ProjectTasksNull;
			public int? ProjectID;
			public int? TaskID;
		}

		public static DateTime? ExpireDateByLot(PXGraph graph, ILSMaster item, ILSMaster master)
		{
			if (master != null && master.ExpireDate != null && master.InvtMult > 0)
				return master.ExpireDate;

			var rec = (PXResult<INSite, InventoryItem, INItemRep, S.INItemSite, INItemLotSerial>)
				SelectFrom<INSite>
				.CrossJoin<InventoryItem>
				.LeftJoin<INItemRep>.On<
					INItemRep.replenishmentClassID.IsEqual<INSite.replenishmentClassID>
					.And<INItemRep.inventoryID.IsEqual<InventoryItem.inventoryID>>>
				.LeftJoin<S.INItemSite>.On<
					S.INItemSite.inventoryID.IsEqual<InventoryItem.inventoryID>
					.And<S.INItemSite.siteID.IsEqual<INSite.siteID>>>
				.LeftJoin<INItemLotSerial>.On<
					INItemLotSerial.inventoryID.IsEqual<InventoryItem.inventoryID>
					.And<INItemLotSerial.lotSerialNbr.IsEqual<@P.AsString>
					.And<INItemLotSerial.expireDate.IsNotNull>>>
				.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>
					.And<INSite.siteID.IsEqual<@P.AsInt>>>
				.View.SelectWindowed(graph, 0, 1, item.LotSerialNbr, item.InventoryID, item.SiteID);

			if (rec == null)
				return master?.ExpireDate ?? item.ExpireDate;

			int? shelfLife = ((S.INItemSite)rec).MaxShelfLife ?? ((INItemRep)rec).MaxShelfLife;

			DateTime? defaultExpireDate = shelfLife > 0 && item.TranDate != null
				? item.TranDate.Value.AddDays(shelfLife.Value)
				: (DateTime?)null;

			INItemLotSerial status = rec;
			return (status.InventoryID == null ? null : status.ExpireDate)
				?? master?.ExpireDate
				?? item.ExpireDate
				?? defaultExpireDate;
		}
	}

	[Flags]
	public enum AvailabilityFetchMode
	{
		None = 0,
		ExcludeCurrent = 1,
		TryOptimize = 2
	}

	[LSDynamicButton(new string[] { "generateLotSerial", "binLotSerial" },
					new string[] { Messages.Generate, Messages.BinLotSerial },
					TranslationKeyType = typeof(Messages))]
    public abstract class LSSelect<TLSMaster, TLSDetail, Where> : PXSelect<TLSMaster>, IEqualityComparer<TLSMaster>
		where TLSMaster : class, IBqlTable, ILSPrimary, new()
		where TLSDetail : class, IBqlTable, ILSDetail, new()
		where Where : IBqlWhere, new()
	{

        #region IEqualityComparer<TLSMaster> Members

        public bool Equals(TLSMaster x, TLSMaster y)
        {
            return this.Cache.ObjectsEqual(x, y) && x.InventoryID == y.InventoryID;
        }

        public int GetHashCode(TLSMaster obj)
        {
            unchecked
            {
                return this.Cache.GetObjectHashCode(obj) * 37 + obj.InventoryID.GetHashCode();
            }
        }
        #endregion

		#region State
		protected bool _InternallCall = false;
        protected PXDBOperation _Operation = PXDBOperation.Normal;
		protected string _MasterQtyField = "qty";
		private Type _MasterQtyFieldType;
		protected string _AvailField = PX.Objects.IN.Messages.Availability_Field;
		protected string _AvailFieldDisplayName = PX.Objects.IN.Messages.Availability_Field;
        protected bool _UnattendedMode = false;

        protected BqlCommand _detailbylotserialstatus;

		public string AvailabilityField
		{
			get
			{
				return _AvailField;
			}
		}

		protected Type MasterQtyField
		{
			get { return _MasterQtyFieldType; }
			set
			{
				if (!value.IsNested)
					throw new PXArgumentException("value", "Nested type is expected.");
				if (BqlCommand.GetItemType(value) != typeof(TLSMaster))
					throw new PXArgumentException("value", "'{0}' is expected.", typeof(TLSMaster).GetLongName());
				_MasterQtyFieldType = value;
				this._MasterQtyField = value.Name.ToLower();
				this._Graph.FieldVerifying.AddHandler(MasterCache.GetItemType(), _MasterQtyField, Master_Qty_FieldVerifying);
			}
		}

		protected PXCache MasterCache
		{
			get
			{
				return this._Graph.Caches[typeof(TLSMaster)];
			}
		}
		
		protected TLSMaster MasterCurrent
		{
			get
			{
				return (TLSMaster)MasterCache.Current;
			}
		}

		protected PXCache DetailCache
		{
			get
			{
				return this._Graph.Caches[typeof(TLSDetail)];
			}
		}

		protected virtual void SetEditMode()
		{
		}

		public virtual void SetEnabled(bool isEnabled)
		{
			this._Graph.Actions[Prefixed("binLotSerial")].SetEnabled(isEnabled);
		}

		protected bool _AllowInsert = true;
		public override bool AllowInsert
		{
			get
			{
				return this._AllowInsert;
			}
			set
			{
				this._AllowInsert = value;
				this.MasterCache.AllowInsert = value;

				SetEditMode();
			}
		}

        protected Type _MasterInventoryType;

		protected bool _AllowUpdate = true;
		public override bool AllowUpdate
		{
			get
			{
				return this._AllowUpdate;
			}
			set
			{
				this._AllowUpdate = value;
				this.MasterCache.AllowUpdate = value;
				this.DetailCache.AllowInsert = value;
				this.DetailCache.AllowUpdate = value;
				this.DetailCache.AllowDelete = value;

				SetEditMode();
			}
		}


		protected bool _AllowDelete = true;
		public override bool AllowDelete
		{
			get
			{
				return this._AllowDelete;
			}
			set
			{
				this._AllowDelete = value;
				this.MasterCache.AllowDelete = value;

				SetEditMode();
			}
		}

		protected bool Initialized;
		protected bool PrevCorrectionMode;
		protected bool PrevFullMode;

		protected bool CorrectionMode
		{
			get 
			{
				return this._AllowUpdate && this._AllowInsert == false && this._AllowDelete == false;
			}
		}

		protected bool FullMode
		{
			get
			{
				return this._AllowUpdate && this._AllowInsert && this._AllowDelete;
			}
		}

		protected Type PrimaryViewType
		{
			get
			{
				if (_Graph.PrimaryItemType == null)
					throw new PXException(Messages.CantGetPrimaryView, _Graph.GetType().FullName);
				return _Graph.PrimaryItemType;
			}
		}

		/// <summary>
		/// Suppresses logic specific to UI
		/// </summary>
        public bool UnattendedMode
        {
            get
            {
                return _UnattendedMode;
            }
            set
            {
                _UnattendedMode = value;
            }
        }

		/// <summary>
		/// Suppresses major internal logic
		/// </summary>
		public bool SuppressedMode
		{
			get
			{
				return _InternallCall;
			}
			set
			{
				_InternallCall = value;
			}
		}

		/// <summary>
		/// Create a scope for suppressing of the major internal logic
		/// </summary>
		/// <returns>null if <paramref name="suppress"/> is false</returns>
		public IDisposable SuppressedModeScope(bool suppress)
		{
			return suppress
				? new LSSelectSuppressedModeScope<TLSMaster, TLSDetail, Where>(this)
				: null;
		}

        protected Dictionary<object, Counters> DetailCounters;

		#endregion

		#region Ctor

		public LSSelect(PXGraph graph)
			: base(graph)
		{
			graph.OnBeforePersist += OnBeforePersist;

			graph.RowSelected.AddHandler<TLSMaster>(Master_RowSelected);
			graph.RowInserted.AddHandler<TLSMaster>(Master_RowInserted);
			graph.RowUpdated.AddHandler<TLSMaster>(Master_RowUpdated);
			graph.RowDeleted.AddHandler<TLSMaster>(Master_RowDeleted);
			graph.RowPersisting.AddHandler<TLSMaster>(Master_RowPersisting);
			graph.RowPersisted.AddHandler<TLSMaster>(Master_RowPersisted);

			graph.RowInserting.AddHandler<TLSDetail>(Detail_RowInserting);
			graph.RowInserted.AddHandler<TLSDetail>(Detail_RowInserted);
			graph.RowUpdated.AddHandler<TLSDetail>(Detail_RowUpdated);
			graph.RowDeleted.AddHandler<TLSDetail>(Detail_RowDeleted);
			graph.RowPersisting.AddHandler<TLSDetail>(Detail_RowPersisting);
			graph.RowPersisted.AddHandler<TLSDetail>(Detail_RowPersisted);

			Type inventoryType = null;
			Type subItemType = null;
			Type siteType = null;
			Type locationType = null;
			Type lotSerialNbrType = null;
            UnattendedMode = graph.UnattendedMode;

            foreach (PXEventSubscriberAttribute attr in this.MasterCache.GetAttributesReadonly(null))
            {
                if (attr is BaseInventoryAttribute)
                {
                    _MasterInventoryType = this.DetailCache.GetBqlField(attr.FieldName);
                }
            }

            foreach (PXEventSubscriberAttribute attr in this.DetailCache.GetAttributesReadonly(null))
			{
				if (attr is INUnitAttribute)
				{
					graph.FieldDefaulting.AddHandler(this.DetailCache.GetItemType(), attr.FieldName, Detail_UOM_FieldDefaulting);
				}

				if (attr is BaseInventoryAttribute)
				{
					inventoryType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is SubItemAttribute)
				{
					subItemType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is SiteAttribute)
				{
					siteType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is LocationAttribute)
				{
					locationType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is INLotSerialNbrAttribute)
				{
					lotSerialNbrType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is PXDBQuantityAttribute && ((PXDBQuantityAttribute)attr).KeyField != null)
				{
					graph.FieldVerifying.AddHandler(this.DetailCache.GetItemType(), attr.FieldName, Detail_Qty_FieldVerifying);
				}
			}

			_detailbylotserialstatus = BqlCommand.CreateInstance(
				typeof(Select<,>),
				typeof(TLSDetail),
				typeof(Where<,,>),
				inventoryType,
				typeof(Equal<>),
				typeof(Required<>),
				inventoryType,
				typeof(And<,,>),
				subItemType,
				typeof(Equal<>),
				typeof(Required<>),
				subItemType,
				typeof(And<,,>),
				siteType,
				typeof(Equal<>),
				typeof(Required<>),
				siteType,
				typeof(And<,,>),
				locationType,
				typeof(Equal<>),
				typeof(Required<>),
				locationType,
				typeof(And<,,>),
				lotSerialNbrType,
				typeof(Equal<>),
				typeof(Required<>),
				lotSerialNbrType,
				typeof(And<>),
				typeof(Where));

			_AvailFieldDisplayName = PXMessages.LocalizeNoPrefix(PX.Objects.IN.Messages.Availability_Field);
			graph.Caches[typeof(TLSMaster)].Fields.Add(_AvailField);
			graph.FieldSelecting.AddHandler(typeof(TLSMaster), _AvailField, Availability_FieldSelecting);

			graph.Views.Add(Prefixed("lotseropts"), new PXView(graph, false, new Select<LotSerOptions>(), new PXSelectDelegate(GetLotSerialOpts)));
			graph.RowPersisting.AddHandler<LotSerOptions>(LotSerOptions_RowPersisting);
			graph.RowSelected.AddHandler<LotSerOptions>(LotSerOptions_RowSelected);
			graph.FieldSelecting.AddHandler(typeof(LotSerOptions), "StartNumVal", LotSerOptions_StartNumVal_FieldSelecting);
			graph.FieldVerifying.AddHandler(typeof(LotSerOptions), "StartNumVal", LotSerOptions_StartNumVal_FieldVerifying);

			if (!graph.Views.Caches.Contains(typeof(INLotSerClassLotSerNumVal)))
				graph.Views.Caches.Add(typeof(INLotSerClassLotSerNumVal));
			if (!graph.Views.Caches.Contains(typeof(InventoryItemLotSerNumVal)))
				graph.Views.Caches.Add(typeof(InventoryItemLotSerNumVal));

			if (_MasterInventoryType != null)
            {
				graph.FieldVerifying.AddHandler(typeof(TLSMaster), _MasterInventoryType.Name, Master_InventoryID_FieldVerifying);
            }

			AddAction(Prefixed("generateLotSerial"), Messages.Generate, true, GenerateLotSerial, PXCacheRights.Update);
			AddAction(Prefixed("binLotSerial"), Messages.BinLotSerial, true, BinLotSerial, PXCacheRights.Select);

            DetailCounters = new Dictionary<object, Counters>();

			PXParentAttribute.SetLeaveChildren(DetailCache, true, typeof(TLSMaster));
		}
		#endregion

		#region Implementation

		protected string Prefixed(string name)
		{
			return string.Format("{0}_{1}", GetType().Name, name);
		}

		protected void AddAction(string name, string displayName, bool visible, PXButtonDelegate handler, PXCacheRights EnableRights)
		{
			var uiAtt = new PXUIFieldAttribute
			            	{
			            		DisplayName = PXMessages.LocalizeNoPrefix(displayName),
			            		MapEnableRights = EnableRights,
							};
			if (!visible) uiAtt.Visible = false;
			var buttAttr = new PXButtonAttribute();
			var addAttrs = new List<PXEventSubscriberAttribute> { uiAtt, buttAttr };
			_Graph.Actions[name] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(
				new []{ PrimaryViewType }), new object[] { _Graph, name, handler, addAttrs.ToArray() });
		}

		public virtual IEnumerable BinLotSerial(PXAdapter adapter)
		{
			View.AskExt(true);
			return adapter.Get();
		}

		protected virtual void LotSerOptions_StartNumVal_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if(MasterCurrent == null) return;
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(MasterCache, MasterCurrent.InventoryID);
			LotSerOptions opt = (LotSerOptions)_Graph.Caches[typeof(LotSerOptions)].Current;
			if(item == null || opt == null) return;

			ILotSerNumVal lotSerNum = ReadLotSerNumVal(MasterCache, item);
			INLotSerialNbrAttribute.LSParts parts = INLotSerialNbrAttribute.GetLSParts(MasterCache, item, lotSerNum);
			if (string.IsNullOrEmpty(((string)e.NewValue)) || ((string)e.NewValue).Length < parts.len)
			{
				opt.StartNumVal = null;
				throw new PXSetPropertyException(Messages.TooShortNum, parts.len);
			}
		}

		public virtual IEnumerable GenerateLotSerial(PXAdapter adapter)
		{
			LotSerOptions opt = (LotSerOptions) _Graph.Caches[typeof (LotSerOptions)].Current;
			if (opt.StartNumVal == null || opt.Qty == null)
				return adapter.Get();

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(MasterCache, MasterCurrent.InventoryID);
			var lsClass = (INLotSerClass)item;
			if (lsClass == null)
				return adapter.Get();

			ILotSerNumVal lotSerNum = ReadLotSerNumVal(MasterCache, item);

			string lotSerialNbr = null;
			INLotSerialNbrAttribute.LSParts parts = INLotSerialNbrAttribute.GetLSParts(MasterCache, lsClass, lotSerNum);
			string numVal = opt.StartNumVal.Substring(parts.nidx, parts.nlen); 
			string numStr = opt.StartNumVal.Substring(0, parts.flen) + new string('0', parts.nlen) + opt.StartNumVal.Substring(parts.lidx, parts.llen);

			try
			{
				MasterCurrent.LotSerialNbr = null;

                List<TLSDetail> existingSplits = new List<TLSDetail>();
                if (lsClass.LotSerTrack == INLotSerTrack.LotNumbered)
                {
                    foreach (TLSDetail split in PXParentAttribute.SelectSiblings(DetailCache, null, typeof(TLSMaster)))
                    {
                        existingSplits.Add(split);
                    }
                }
                
				if (lsClass.LotSerTrack != INLotSerTrack.LotNumbered || (opt.Qty != 0 && MasterCurrent.BaseQty != 0m))
				{
					CreateNumbers(MasterCache, MasterCurrent, (decimal)opt.Qty, true);
				}

				foreach (TLSDetail split in PXParentAttribute.SelectSiblings(DetailCache, null, typeof (TLSMaster)))
				{
					if (string.IsNullOrEmpty(split.AssignedNbr) ||
					    !INLotSerialNbrAttribute.StringsEqual(split.AssignedNbr, split.LotSerialNbr)) continue;

                    TLSDetail copy = PXCache<TLSDetail>.CreateCopy(split);

					if (lotSerialNbr != null)
						numVal = AutoNumberAttribute.NextNumber(numVal);

                    if ((decimal)opt.Qty != split.Qty && lsClass.LotSerTrack == INLotSerTrack.LotNumbered && !existingSplits.Contains(split))
					{
						split.BaseQty = (decimal)opt.Qty;
						split.Qty = (decimal)opt.Qty;
					}

					lotSerialNbr = INLotSerialNbrAttribute.UpdateNumber(split.AssignedNbr, numStr, numVal);
					split.LotSerialNbr = lotSerialNbr;
					DetailCache.RaiseRowUpdated(split, copy);
				}
			}
			catch(Exception)
			{
				UpdateParent(MasterCache, MasterCurrent);
			}

			if (lotSerialNbr != null)
				UpdateLotSerNumVal(lotSerNum, numVal, item);
			return adapter.Get();
		}

		/// <summary>
		/// Save new auto-incremental value
		/// </summary>
		/// <param name="lotSerNum">object with auto-incremental value</param>
		/// <param name="value">new auto-incremental value</param>
		/// <param name="item">settings</param>
		protected virtual void UpdateLotSerNumVal(ILotSerNumVal lotSerNum, string value, PXResult<InventoryItem, INLotSerClass> item)
		{
			Type type;
			PXCache cache;
			if (lotSerNum == null)
			{
				lotSerNum = ((INLotSerClass)item).LotSerNumShared ?? false
					? (ILotSerNumVal)new INLotSerClassLotSerNumVal { LotSerClassID = ((INLotSerClass)item).LotSerClassID }
					: new InventoryItemLotSerNumVal { InventoryID = ((InventoryItem)item).InventoryID };
				lotSerNum.LotSerNumVal = value;

				type = lotSerNum.GetType();
				cache = _Graph.Caches[type];
				cache.Insert(lotSerNum);
			}
			else
			{
				type = lotSerNum.GetType();
				cache = _Graph.Caches[type];

				var copy = (ILotSerNumVal)cache.CreateCopy(lotSerNum);
				copy.LotSerNumVal = value;
				cache.Update(copy);
			}
		}

		public virtual IEnumerable GetLotSerialOpts()
		{
			LotSerOptions opt = new LotSerOptions();
			PXResult<InventoryItem, INLotSerClass> item = null;
			if (MasterCurrent != null)
			{
				opt.UnassignedQty = MasterCurrent.UnassignedQty;
				item = ReadInventoryItem(MasterCache, MasterCurrent.InventoryID);
			}
			if (item != null && (INLotSerClass)item != null)
			{
				var lsClass = (INLotSerClass)item;
				bool disabled;
				bool allowGernerate;
				using (InvtMultScope<TLSMaster> ms = new InvtMultScope<TLSMaster>(MasterCurrent))
				{
					INLotSerTrack.Mode mode = GetTranTrackMode(MasterCurrent, lsClass);
                    disabled = (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Manual) != 0);
                    allowGernerate = (mode & INLotSerTrack.Mode.Create) != 0;
				}
				if (!disabled && AllowUpdate)
				{
					var lotSerNum = ReadLotSerNumVal(MasterCache, item);
					
					string numval = AutoNumberAttribute.NextNumber(lotSerNum == null || string.IsNullOrEmpty(lotSerNum.LotSerNumVal) 
						? new string('0', INLotSerialNbrAttribute.GetNumberLength(null))
						: lotSerNum.LotSerNumVal);
					string emptynbr = INLotSerialNbrAttribute.GetNextNumber(MasterCache, lsClass, lotSerNum);
					string format = INLotSerialNbrAttribute.GetNextFormat(lsClass, lotSerNum);
					opt.StartNumVal = INLotSerialNbrAttribute.UpdateNumber(format, emptynbr, numval);
					opt.AllowGenerate = allowGernerate;
                    if (lsClass.LotSerTrack == INLotSerTrack.SerialNumbered)
                        opt.Qty = (int)(MasterCurrent.UnassignedQty ?? 0);
                    else opt.Qty = (MasterCurrent.UnassignedQty ?? 0);
                    opt.IsSerial = lsClass.LotSerTrack == INLotSerTrack.SerialNumbered;
                }
			}
			_Graph.Caches[typeof(LotSerOptions)].Clear();
			opt = (LotSerOptions)_Graph.Caches[typeof(LotSerOptions)].Insert(opt);
			_Graph.Caches[typeof (LotSerOptions)].IsDirty = false;
			yield return opt;
		}

        public virtual void Master_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (sender.Graph.UnattendedMode || sender.Graph.IsImport)
                return;
	        var item = InventoryItem.PK.Find(sender.Graph, (int?) e.NewValue);
            if (item != null && item.KitItem == true && item.StkItem == false && 
                ((INKitSpecStkDet)PXSelectReadonly<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, e.NewValue)) == null &&
                ((INKitSpecNonStkDet)PXSelectReadonly<INKitSpecNonStkDet, Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, e.NewValue)) == null)
            {
                e.NewValue = null;
                throw new PXSetPropertyException(Messages.EmptyKitNotAllowed, PXErrorLevel.Error);
            }
        }

		protected virtual void LotSerOptions_StartNumVal_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			LotSerOptions opt = (LotSerOptions)e.Row;
			if (opt == null || opt.StartNumVal == null || MasterCurrent == null) return;
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(MasterCache, MasterCurrent.InventoryID);
			var lotSerNum = ReadLotSerNumVal(MasterCache, item);
			string mask = INLotSerialNbrAttribute.GetDisplayMask(MasterCache, item, lotSerNum);
			if(mask == null) return;
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, mask.Length, true, "StartNumVal", false, 1, mask, null, null, null, null);
		}

		protected virtual void LotSerOptions_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			LotSerOptions opt = (LotSerOptions)e.Row;

			bool enabled = IsLotSerOptionsEnabled(sender, opt);

			PXUIFieldAttribute.SetEnabled<LotSerOptions.startNumVal>(sender, opt, enabled);
			PXUIFieldAttribute.SetEnabled<LotSerOptions.qty>(sender, opt, enabled);
			PXDBDecimalAttribute.SetPrecision(sender, opt, "Qty", (opt.IsSerial == true ? 0 : CommonSetupDecPl.Qty));
            _Graph.Actions[Prefixed("generateLotSerial")].SetEnabled(opt != null && opt.AllowGenerate == true && enabled);
		}

		protected virtual bool IsLotSerOptionsEnabled(PXCache sender, LotSerOptions opt)
		{
			return opt != null && opt.StartNumVal != null;
		}

		protected virtual void LotSerOptions_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass)
		{
			return INLotSerialNbrAttribute.TranTrackMode(lotSerClass, row.TranType, row.InvtMult);
		}

		public class InvtMultScope<TNode> : IDisposable
			where TNode : class, ILSMaster
		{
			private TNode _item;
			private TNode _olditem;
			private bool? _Reverse;
			private bool? _ReverseOld;

			public InvtMultScope(TNode item)
			{
				_Reverse = (item.Qty < 0m);
				_item = item;

				if (_Reverse == true)
				{
					_item.InvtMult *= (short)-1;
					_item.Qty = -1m * (decimal)_item.Qty;
					_item.BaseQty = -1m * (decimal)_item.BaseQty;
				}
			}

			public InvtMultScope(TNode item, TNode olditem)
				: this(item)
			{
				_ReverseOld = (olditem.Qty < 0m);
				_olditem = olditem;

				if (_ReverseOld == true)
				{
					_olditem.InvtMult *= (short)-1;
					_olditem.Qty = -1m * (decimal)_olditem.Qty;
					_olditem.BaseQty = -1m * (decimal)_olditem.BaseQty;
				}
			}

			void IDisposable.Dispose()
			{
				if (_Reverse == true)
				{
					_item.InvtMult *= (short)-1;
					_item.Qty = -1m * (decimal)_item.Qty;
					_item.BaseQty = -1m * (decimal)_item.BaseQty;
				}

				if (_ReverseOld == true)
				{
					_olditem.InvtMult *= (short)-1;
					_olditem.Qty = -1m * (decimal)_olditem.Qty;
					_olditem.BaseQty = -1m * (decimal)_olditem.BaseQty;
				}
			}
		}

		public class PXRowInsertedEventArgs<Table>
			where Table : class, IBqlTable
		{
			protected Table _Row;
			protected bool Cancel;
			public readonly bool ExternalCall;

			public Table Row
			{
				get
				{
					return this._Row;
				}
				set
				{
					this._Row = value;
				}
			}

			public PXRowInsertedEventArgs(Table Row, bool ExternalCall)
			{
				this.Row = Row;
				this.ExternalCall = ExternalCall;
			}

			public PXRowInsertedEventArgs(PXRowInsertedEventArgs e)
				: this((Table)e.Row, e.ExternalCall)
			{
			}
		}

		public class PXRowUpdatedEventArgs<Table>
			where Table : class, IBqlTable
		{
			protected Table _Row;
			protected Table _OldRow;
			protected bool Cancel;
			public readonly bool ExternalCall;

			public Table Row
			{
				get
				{
					return this._Row;
				}
				set
				{
					this._Row = value;
				}
			}

			public Table OldRow
			{
				get
				{
					return this._OldRow;
				}
				set
				{
					this._OldRow = value;
				}
			}

			public PXRowUpdatedEventArgs(Table Row, Table OldRow, bool ExternalCall)
			{
				this.Row = Row;
				this.OldRow = OldRow;
				this.ExternalCall = ExternalCall;
			}

			public PXRowUpdatedEventArgs(PXRowUpdatedEventArgs e)
				: this((Table)e.Row, (Table)e.OldRow, e.ExternalCall)
			{
			}
		}

		public class PXRowDeletedEventArgs<Table>
			where Table : class, IBqlTable
		{
			protected Table _Row;
			protected bool Cancel;
			public readonly bool ExternalCall;

			public Table Row
			{
				get
				{
					return this._Row;
				}
				set
				{
					this._Row = value;
				}
			}

			public PXRowDeletedEventArgs(Table Row, bool ExternalCall)
			{
				this.Row = Row;
				this.ExternalCall = ExternalCall;
			}

			public PXRowDeletedEventArgs(PXRowDeletedEventArgs e)
				: this((Table)e.Row, e.ExternalCall)
			{
			}
		}

		public abstract TLSDetail Convert(TLSMaster item);

		protected virtual INLotSerialStatus INLotSerialStatus(ILSMaster item)
		{
			INLotSerialStatus ret = new INLotSerialStatus();
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.LocationID = item.LocationID;
			ret.SubItemID = item.SubItemID;
			ret.LotSerialNbr = item.LotSerialNbr;

			return ret;
		}

		public virtual string FormatQty(decimal? value)
		{
			return (value == null) ? string.Empty : ((decimal)value).ToString("N" + CommonSetupDecPl.Qty.ToString(), System.Globalization.NumberFormatInfo.CurrentInfo);
		}

		protected virtual TLSMaster SelectMaster(PXCache sender, TLSDetail row)
		{
			return (TLSMaster)PXParentAttribute.SelectParent(sender, row, typeof(TLSMaster));
		}

		protected virtual bool SameInventoryItem(ILSMaster a, ILSMaster b)
		{
			return a.InventoryID == b.InventoryID;
		}

		protected virtual object[] SelectDetail(PXCache sender, TLSMaster row)
		{
			object[] ret = PXParentAttribute.SelectChildren(sender, row, typeof(TLSMaster));

			return Array.FindAll<object>(ret, new Predicate<object>(delegate(object a)
			{
				return SameInventoryItem((ILSMaster)a, (ILSMaster)row);
			}));
		}

		protected virtual object[] SelectDetail(PXCache sender, TLSDetail row)
		{
			object[] ret = PXParentAttribute.SelectSiblings(sender, row, typeof(TLSMaster));

			return Array.FindAll<object>(ret, new Predicate<object>(delegate(object a)
			{
				return SameInventoryItem((ILSMaster)a, (ILSMaster)row);
			}));
		}

		protected object[] SelectDetailOrdered(PXCache sender, TLSMaster row)
		{
			return SelectDetailOrdered(sender, Convert(row));
		}

		protected virtual object[] SelectDetailOrdered(PXCache sender, TLSDetail row)
		{
			object[] ret = SelectDetail(sender, row);

			Array.Sort<object>(ret, new Comparison<object>(delegate(object a, object b)
			{
				object aSplitLineNbr = ((ILSDetail)a).SplitLineNbr;
				object bSplitLineNbr = ((ILSDetail)b).SplitLineNbr;

				return ((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
			}));

			return ret;
		}

		protected object[] SelectDetailReversed(PXCache sender, TLSMaster row)
		{
			return SelectDetailReversed(sender, Convert(row));
		}

		protected virtual object[] SelectDetailReversed(PXCache sender, TLSDetail row)
		{
			object[] ret = SelectDetail(sender, row);

			Array.Sort<object>(ret, new Comparison<object>(delegate(object a, object b)
			{
				object aSplitLineNbr = ((ILSDetail)a).SplitLineNbr;
				object bSplitLineNbr = ((ILSDetail)b).SplitLineNbr;

				return -((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
			}));

			return ret;
		}

		protected virtual void ExpireCached(PXCache sender, object item)
		{
			object cached = sender.Locate(item);

			if (cached != null && (sender.GetStatus(cached) == PXEntryStatus.Held || sender.GetStatus(cached) == PXEntryStatus.Notchanged))
			{
				sender.SetStatus(cached, PXEntryStatus.Notchanged);
				sender.Remove(cached);
				sender.ClearQueryCache();
			}
		}

		protected virtual PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? inventoryID)
		{
			if (inventoryID == null)
				return null;
			var inventory = InventoryItem.PK.Find(sender.Graph, inventoryID);
			if (inventory == null)
				throw new PXException(ErrorMessages.ValueDoesntExistOrNoRights, Messages.InventoryItem, inventoryID);
			INLotSerClass lotSerClass;
			if (inventory.StkItem == true)
			{
				lotSerClass = INLotSerClass.PK.Find(sender.Graph, inventory.LotSerClassID);
				if (lotSerClass == null)
					throw new PXException(ErrorMessages.ValueDoesntExistOrNoRights, Messages.LotSerClass, inventory.LotSerClassID);
			}
			else
			{
				lotSerClass = new INLotSerClass();
			}
			return new PXResult<InventoryItem, INLotSerClass>(inventory, lotSerClass);
		}

		/// <summary>
		/// Read ILotSerNumVal implemented object which store auto-incremental value
		/// </summary>
		/// <param name="sender">cache</param>
		/// <param name="item">settings</param>
		/// <returns></returns>
		protected virtual ILotSerNumVal ReadLotSerNumVal(PXCache sender, PXResult<InventoryItem, INLotSerClass> item)
		{
			return INLotSerialNbrAttribute.ReadLotSerNumVal(sender.Graph, item);
		}

		public virtual void CreateNumbers(PXCache sender, TLSMaster Row, decimal BaseQty)
		{
			CreateNumbers(sender, Row, BaseQty, false);
		}

		public virtual void CreateNumbers(PXCache sender, TLSMaster Row, decimal BaseQty, bool ForceAutoNextNbr)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			TLSDetail split = Convert(Row);

			if (Row != null)
				DetailCounters.Remove(Row);

			if (!ForceAutoNextNbr && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered &&
				((INLotSerClass)item).AutoSerialMaxCount > 0 && ((INLotSerClass)item).AutoSerialMaxCount < BaseQty)
			{
				BaseQty = ((INLotSerClass) item).AutoSerialMaxCount.GetValueOrDefault();
			}

			INLotSerTrack.Mode mode = GetTranTrackMode(Row, item);
			ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender, item);
			foreach (TLSDetail lssplit in INLotSerialNbrAttribute.CreateNumbers<TLSDetail>(sender, item, lotSerNum, mode, ForceAutoNextNbr, BaseQty))
			{
				string LotSerTrack = (mode & INLotSerTrack.Mode.Create) > 0 ? ((INLotSerClass)item).LotSerTrack : INLotSerTrack.NotNumbered;

				split.SplitLineNbr = null;
				split.LotSerialNbr = lssplit.LotSerialNbr;
				split.AssignedNbr = lssplit.AssignedNbr;
				split.LotSerClassID = lssplit.LotSerClassID;

				if (!string.IsNullOrEmpty(Row.LotSerialNbr) &&
					((LotSerTrack == INLotSerTrack.SerialNumbered && Row.Qty == 1m) ||
						LotSerTrack == INLotSerTrack.LotNumbered))
				{
					split.LotSerialNbr = Row.LotSerialNbr;
				}

				if (LotSerTrack == "S")
				{
					split.UOM = null;
					split.Qty = 1m;
					split.BaseQty = 1m;
				}
				else 
				{
					split.UOM = null;
					split.BaseQty = BaseQty;
					split.Qty = BaseQty;
				}
				if (((INLotSerClass)item).LotSerTrackExpiration == true)
					split.ExpireDate = ExpireDateByLot(sender, split, Row);

				sender.Graph.Caches[typeof(TLSDetail)].Insert(PXCache<TLSDetail>.CreateCopy(split));
				BaseQty -= (decimal)split.BaseQty;
			}

			if (BaseQty > 0m && (((INLotSerClass)item).LotSerTrack != "S" || decimal.Remainder(BaseQty, 1m) == 0m))
			{
				Row.UnassignedQty += BaseQty;
			}
			else if (BaseQty > 0m)
			{
				TLSMaster oldrow = PXCache<TLSMaster>.CreateCopy(Row);

				Row.BaseQty -= BaseQty;
				Row.Qty = INUnitAttribute.ConvertFromBase(sender, Row.InventoryID, Row.UOM, (decimal)Row.BaseQty, INPrecision.QUANTITY);

				if (Math.Abs((Decimal)oldrow.Qty - (Decimal)Row.Qty) >= 0.0000005m)
				{
				sender.RaiseFieldUpdated(_MasterQtyField, Row, oldrow.Qty);
				sender.RaiseRowUpdated(Row, oldrow);
			}
		}
			if(Row.UnassignedQty > 0)
				sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.BinLotSerialNotAssigned, PXErrorLevel.Warning));
		}

		public virtual void CreateNumbers(PXCache sender, TLSMaster Row)
		{
			CreateNumbers(sender, Row, (decimal)Row.BaseQty);
		}

		public virtual void TruncateNumbers(PXCache sender, TLSMaster Row, decimal BaseQty)
		{
			PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];
			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

			if (((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered && Math.Abs(Decimal.Floor(BaseQty) - BaseQty) > 0.0000005m)
			{
				TLSMaster oldrow = PXCache<TLSMaster>.CreateCopy(Row);
				Row.BaseQty += BaseQty - Decimal.Truncate(BaseQty);
				Row.Qty = INUnitAttribute.ConvertFromBase(sender, Row.InventoryID, Row.UOM, (decimal)Row.BaseQty, INPrecision.QUANTITY);

				sender.RaiseFieldUpdated(_MasterQtyField, Row, oldrow.Qty);
				sender.RaiseRowUpdated(Row, oldrow);

				BaseQty = Decimal.Truncate(BaseQty);
			}

			if (Row != null)
				DetailCounters.Remove(Row);
			if (Row.UnassignedQty > 0m)
			{
				if (Row.UnassignedQty >= BaseQty)
				{
					Row.UnassignedQty -= BaseQty;
					BaseQty = 0m;
				}
				else
				{
					BaseQty -= (decimal)Row.UnassignedQty;
					Row.UnassignedQty = 0m;
				}
			}

			foreach (object detail in SelectDetailReversed(cache, (TLSMaster)Row))
			{
				if (BaseQty >= ((ILSDetail)detail).BaseQty)
				{
					BaseQty -= (decimal)((ILSDetail)detail).BaseQty;
					cache.Delete(detail);

					ExpireCached(lscache, INLotSerialStatus((TLSDetail)detail));
				}
				else
				{
					TLSDetail newdetail = PXCache<TLSDetail>.CreateCopy((TLSDetail)detail);
					newdetail.BaseQty -= BaseQty;
					newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

					cache.Update(newdetail);

					ExpireCached(lscache, INLotSerialStatus((TLSDetail)detail));
					break;
				}
			}
		}

		public virtual void UpdateNumbers(PXCache sender, object Row)
		{
			PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];

			if (Row is TLSMaster)
				DetailCounters.Remove((TLSMaster)Row);
			foreach (object detail in SelectDetail(cache, (TLSMaster)Row))
			{
				TLSDetail newdetail = PXCache<TLSDetail>.CreateCopy((TLSDetail)detail);

				if (((ILSMaster)Row).LocationID == null && newdetail.LocationID != null && cache.GetStatus(newdetail) == PXEntryStatus.Inserted && newdetail.Qty == 0m)
				{
					cache.Delete(newdetail);
				}
				else
				{
					newdetail.SubItemID = ((ILSMaster)Row).SubItemID ?? newdetail.SubItemID;
					newdetail.SiteID = ((ILSMaster)Row).SiteID;
					newdetail.LocationID = ((ILSMaster)Row).LocationID ?? newdetail.LocationID;
					newdetail.ExpireDate = ExpireDateByLot(sender, newdetail, (ILSMaster)Row);
                    
					cache.Update(newdetail);
				}
			}
		}

		public virtual void UpdateNumbers(PXCache sender, object Row, decimal BaseQty)
		{
			PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];
			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];

			bool deleteflag = false;

			if (Row is TLSMaster)
				DetailCounters.Remove((TLSMaster)Row);

            if (_Operation == PXDBOperation.Update)
            {
			foreach (object detail in SelectDetail(cache, (TLSMaster)Row))
			{
				if (deleteflag)
				{
					cache.Delete(detail);
					ExpireCached(lscache, INLotSerialStatus((TLSDetail)detail));
				}
				else
				{
					TLSDetail newdetail = PXCache<TLSDetail>.CreateCopy((TLSDetail)detail);

					newdetail.SubItemID = ((ILSMaster)Row).SubItemID;
					newdetail.SiteID = ((ILSMaster)Row).SiteID;
					newdetail.LocationID = ((ILSMaster)Row).LocationID;
					newdetail.LotSerialNbr = ((ILSMaster)Row).LotSerialNbr;
					newdetail.ExpireDate = ExpireDateByLot(sender, newdetail, (ILSMaster)Row);

					newdetail.BaseQty = ((ILSMaster)Row).BaseQty;
					newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

					cache.Update(newdetail);

					ExpireCached(lscache, INLotSerialStatus((TLSDetail)detail));

					deleteflag = true;
				}
			}
            }

			if (!deleteflag)
			{
				TLSDetail newdetail = Convert((TLSMaster)Row);
				newdetail.ExpireDate = ExpireDateByLot(sender, newdetail, (TLSMaster)Row);
				DefaultLotSerialNbr(cache, newdetail);

				if (string.IsNullOrEmpty(newdetail.LotSerialNbr) && !string.IsNullOrEmpty(((TLSMaster) Row).LotSerialNbr))
				{
					newdetail.LotSerialNbr = ((TLSMaster) Row).LotSerialNbr;
				}

				cache.Insert(newdetail);

				ExpireCached(lscache, INLotSerialStatus((TLSDetail)newdetail));
			}
		}

		protected virtual DateTime? ExpireDateByLot(PXCache sender, ILSMaster item, ILSMaster master) => LSSelect.ExpireDateByLot(sender.Graph, item, master);

		public virtual void Summarize(PXCache sender, object Row, INLotSerialStatus LSRow)
		{
			PXView view = sender.Graph.TypedViews.GetView(_detailbylotserialstatus, false);
			foreach (TLSDetail det in view.SelectMultiBound(new object[] { Row }, LSRow.InventoryID, LSRow.SubItemID, LSRow.SiteID, LSRow.LocationID, LSRow.LotSerialNbr))
			{
				LSRow.QtyOnHand += (decimal?)det.InvtMult * det.BaseQty;
			}
			sender.SetStatus(LSRow, PXEntryStatus.Held);
		}

		public virtual PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, TLSMaster Row, PXResult<InventoryItem, INLotSerClass> item)
		{
			PXSelectBase<INLotSerialStatus> cmd = this.GetSerialStatusCmdBase(sender, Row, item);
			this.AppendSerialStatusCmdWhere(cmd, Row, item);
			this.AppendSerialStatusCmdOrderBy(cmd, Row, item);

			return cmd;
		}

		protected virtual PXSelectBase<INLotSerialStatus> GetSerialStatusCmdBase(PXCache sender, TLSMaster Row, PXResult<InventoryItem, INLotSerClass> item)
		{
			return new PXSelectJoin<INLotSerialStatus,
				InnerJoin<INLocation, On<INLocation.locationID, Equal<INLotSerialStatus.locationID>>>,
				Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>,
					And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>,
					And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>(sender.Graph);
		}

		protected virtual void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatus> cmd, TLSMaster Row, INLotSerClass lotSerClass)
		{
			if (Row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (Row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
			}

			if (lotSerClass.IsManualAssignRequired == true)
			{
				if(string.IsNullOrEmpty(Row.LotSerialNbr))
				cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
				else
					cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			}
		}

		public virtual void AppendSerialStatusCmdOrderBy(PXSelectBase<INLotSerialStatus> cmd, TLSMaster Row, INLotSerClass lotSerClass)
		{
			switch (lotSerClass.LotSerIssueMethod)
			{
				case INLotSerIssueMethod.FIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.LIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Desc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Expiration:
					cmd.OrderByNew<OrderBy<Asc<INLotSerialStatus.expireDate, Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Sequential:
				case INLotSerIssueMethod.UserEnterable:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>();
					break;
				default:
					throw new PXException();
			}
		}

		public virtual void IssueNumbers(PXCache sender, TLSMaster Row, decimal BaseQty)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

			PXDBOperation prevOperation = _Operation;
			if (_Operation == PXDBOperation.Update && ((INLotSerClass)item).LotSerTrack == "S" && SelectDetail(DetailCache, Row).Count() == 0)
			{
				_Operation = PXDBOperation.Normal;
			}

			try
			{
				IssueNumbersInt(sender, Row, item, BaseQty);
			}
			finally
			{
				_Operation = prevOperation;
			}
		}

		protected void IssueNumbersInt(PXCache sender, TLSMaster Row, PXResult<InventoryItem, INLotSerClass> item, decimal BaseQty)
		{
			PXSelectBase<INLotSerialStatus> cmd = GetSerialStatusCmd(sender, Row, item);

			TLSDetail split = Convert(Row);

			INLotSerialStatus pars = INLotSerialStatus(Row);

			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];
			
			if (Row != null)
				DetailCounters.Remove(Row);
			var lotSerClass = (INLotSerClass)item;
			if ((GetTranTrackMode(Row, lotSerClass) & INLotSerTrack.Mode.Issue) > 0)
			{
				List<object> lotSerialStatuses = cmd.View.SelectMultiBound(new object[] { pars });

				if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
				{
					int lotSerialIndex = lotSerialStatuses.FindIndex(
						x => string.Equals(
							PXResult.Unwrap<INLotSerialStatus>(x).LotSerialNbr?.Trim(),
							Row.LotSerialNbr?.Trim(),
							StringComparison.InvariantCultureIgnoreCase));
					if (lotSerialIndex > 0)
					{
						object lotSerialStatus = lotSerialStatuses[lotSerialIndex];
						lotSerialStatuses.RemoveAt(lotSerialIndex);
						lotSerialStatuses.Insert(0, lotSerialStatus);
					}
				}

				foreach (PXResult res in lotSerialStatuses)
			{
				INLotSerialStatus lsmaster = (INLotSerialStatus)res[typeof(INLotSerialStatus)];

				split.SplitLineNbr = null;
				split.SubItemID = lsmaster.SubItemID;
				split.LocationID = lsmaster.LocationID;
				split.LotSerialNbr = lsmaster.LotSerialNbr;
				split.ExpireDate = lsmaster.ExpireDate;
				split.UOM = ((InventoryItem)item).BaseUnit;

				decimal SignQtyAvail;
				decimal SignQtyHardAvail;
				INItemPlanIDAttribute.GetInclQtyAvail<SiteLotSerial>(_Graph.Caches[typeof(TLSDetail)], split, out SignQtyAvail, out SignQtyHardAvail);

				if (SignQtyAvail < 0m)
				{
					LotSerialStatus accumavail = new LotSerialStatus();
					PXCache<INLotSerialStatus>.RestoreCopy(accumavail, lsmaster);

					accumavail = (LotSerialStatus)_Graph.Caches[typeof(LotSerialStatus)].Insert(accumavail);

					decimal? AvailableQty = lsmaster.QtyAvail + accumavail.QtyAvail;

					if (AvailableQty <= 0m)
					{
						continue;
					}

					if (AvailableQty <= BaseQty)
					{
						split.BaseQty = AvailableQty;
						BaseQty -= (decimal)AvailableQty;
					}
					else
					{
						if (lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
						{
							split.BaseQty = 1m;
							BaseQty -= 1m;
						}
						else
						{
							split.BaseQty = BaseQty;
							BaseQty = 0m;
						}
					}
				}
				else
				{
					if (lscache.GetStatus(lsmaster) == PXEntryStatus.Notchanged)
					{
						Summarize(lscache, Row, lsmaster);
					}

					if (lsmaster.QtyOnHand <= 0m)
					{
						continue;
					}

					if (lsmaster.QtyOnHand <= BaseQty)
					{
						split.BaseQty = lsmaster.QtyOnHand;
						BaseQty -= (decimal)lsmaster.QtyOnHand;
					}
					else
					{
						if (lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
						{
							split.BaseQty = 1m;
							BaseQty -= 1m;
						}
						else
						{
							split.BaseQty = BaseQty;
							BaseQty = 0m;
						}
					}

					lsmaster.QtyOnHand -= split.BaseQty;
					sender.Graph.Caches[typeof(INLotSerialStatus)].SetStatus(lsmaster, PXEntryStatus.Held);
				}

				split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
				sender.Graph.Caches[typeof(TLSDetail)].Insert(PXCache<TLSDetail>.CreateCopy(split));

				if (BaseQty <= 0m)
				{
					break;
				}
			}
			}

			if (BaseQty > 0m && Row.InventoryID != null && Row.SubItemID != null && Row.SiteID != null && Row.LocationID != null && !string.IsNullOrEmpty(Row.LotSerialNbr))
			{
				if (lotSerClass.IsManualAssignRequired == true || lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
				{
					
				}
				else
				{
					split.BaseQty = BaseQty;

					split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
					split.ExpireDate = ExpireDateByLot(sender, split, null);

					try
					{
					sender.Graph.Caches[typeof(TLSDetail)].Insert(PXCache<TLSDetail>.CreateCopy(split));
				}
					catch
					{
						Row.UnassignedQty += BaseQty;
						sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.BinLotSerialNotAssigned, PXErrorLevel.Warning));
					}
					finally
					{
						BaseQty = 0m;
					}
				}
			}

			if (BaseQty != 0)
			{
				var haveRemainder = lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && decimal.Remainder(BaseQty, 1m) != 0m;
				if(haveRemainder || BaseQty < 0)
			{
				TLSMaster oldrow = PXCache<TLSMaster>.CreateCopy(Row);

				Row.BaseQty -= BaseQty;
				Row.Qty = INUnitAttribute.ConvertFromBase(sender, Row.InventoryID, Row.UOM, (decimal)Row.BaseQty, INPrecision.QUANTITY);

				sender.RaiseFieldUpdated(_MasterQtyField, Row, oldrow.Qty);
				sender.RaiseRowUpdated(Row, oldrow);

					if (haveRemainder)
				{
					sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.SerialItem_LineQtyUpdated, PXErrorLevel.Warning));
				}
				else
				{
					sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.InsuffQty_LineQtyUpdated, PXErrorLevel.Warning));
				}
			}
				else
				{
					Row.UnassignedQty += BaseQty;
					sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.BinLotSerialNotAssigned, PXErrorLevel.Warning));
				}
			}
		}

		public virtual void IssueNumbers(PXCache sender, TLSMaster Row)
		{
			IssueNumbers(sender, Row, (decimal)Row.BaseQty);
		}

		protected virtual void UpdateCounters(PXCache sender, Counters counters, TLSDetail detail)
		{
			counters.RecordCount += 1;
			detail.BaseQty = INUnitAttribute.ConvertToBase(sender, detail.InventoryID, detail.UOM, (decimal)detail.Qty, detail.BaseQty, INPrecision.QUANTITY);
			counters.BaseQty += (decimal)detail.BaseQty;
			if (detail.ExpireDate == null)
			{
				counters.ExpireDatesNull += 1;
			}
			else
			{
				if (counters.ExpireDates.ContainsKey(detail.ExpireDate))
				{
					counters.ExpireDates[detail.ExpireDate] += 1;
				}
				else
				{
					counters.ExpireDates[detail.ExpireDate] = 1;
				}
				counters.ExpireDate = detail.ExpireDate;
			}
			if (detail.SubItemID == null)
			{
				counters.SubItemsNull += 1;
			}
			else
			{
				if (counters.SubItems.ContainsKey(detail.SubItemID))
				{
					counters.SubItems[detail.SubItemID] += 1;
				}
				else
				{
					counters.SubItems[detail.SubItemID] = 1;
				}
				counters.SubItem = detail.SubItemID;
			}
			if (detail.LocationID == null)
			{
				counters.LocationsNull += 1;
			}
			else
			{
				if (counters.Locations.ContainsKey(detail.LocationID))
				{
					counters.Locations[detail.LocationID] += 1;
				}
				else
				{
					counters.Locations[detail.LocationID] = 1;
				}
				counters.Location = detail.LocationID;
			}
			if (detail.TaskID == null)
			{
				counters.ProjectTasksNull += 1;
			}
			else
			{
				var kv = new KeyValuePair<int?, int?>(detail.ProjectID, detail.TaskID);
				if (counters.ProjectTasks.ContainsKey(kv))
				{
					counters.ProjectTasks[kv] += 1;
				}
				else
				{
					counters.ProjectTasks[kv] = 1;
				}
				counters.ProjectID = detail.ProjectID;
				counters.TaskID = detail.TaskID;
			}
			if (detail.LotSerialNbr == null)
			{
				counters.LotSerNumbersNull += 1;
			}
			else
			{
				if (string.IsNullOrEmpty(detail.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(detail.AssignedNbr, detail.LotSerialNbr))
				{
					counters.UnassignedNumber++;
				}

				if (counters.LotSerNumbers.ContainsKey(detail.LotSerialNbr))
				{
					counters.LotSerNumbers[detail.LotSerialNbr] += 1;
				}
				else
				{
					counters.LotSerNumbers[detail.LotSerialNbr] = 1;
				}
				counters.LotSerNumber = detail.LotSerialNbr;
			}
			
		}

		protected Counters counters;

		public virtual void UpdateParent(PXCache sender, TLSMaster Row, TLSDetail Det, TLSDetail OldDet, out decimal BaseQty)
		{
			counters = null;
			if (!DetailCounters.TryGetValue(Row, out counters))
			{
				DetailCounters[Row] = counters = new Counters();
				foreach (TLSDetail detail in SelectDetail(sender.Graph.Caches[typeof(TLSDetail)], Row))
				{
					UpdateCounters(sender, counters, detail);
				}
			}
			else
			{
				if (Det != null)
				{
					UpdateCounters(sender, counters, Det);
				}
				if (OldDet != null)
				{
					TLSDetail detail = OldDet;
					counters.RecordCount -= 1;
					detail.BaseQty = INUnitAttribute.ConvertToBase(sender, detail.InventoryID, detail.UOM, (decimal)detail.Qty, detail.BaseQty, INPrecision.QUANTITY);
					counters.BaseQty -= (decimal)detail.BaseQty;
					if (detail.ExpireDate == null)
					{
						counters.ExpireDatesNull -= 1;
					}
					else if (counters.ExpireDates.ContainsKey(detail.ExpireDate))
					{
						if ((counters.ExpireDates[detail.ExpireDate] -= 1) == 0)
						{
							counters.ExpireDates.Remove(detail.ExpireDate);
						}
					}
					if (detail.SubItemID == null)
					{
						counters.SubItemsNull -= 1;
					}
					else if (counters.SubItems.ContainsKey(detail.SubItemID))
					{
						if ((counters.SubItems[detail.SubItemID] -= 1) == 0)
						{
							counters.SubItems.Remove(detail.SubItemID);
						}
					}
					if (detail.LocationID == null)
					{
						counters.LocationsNull -= 1;
					}
					else if (counters.Locations.ContainsKey(detail.LocationID))
					{
						if ((counters.Locations[detail.LocationID] -= 1) == 0)
						{
							counters.Locations.Remove(detail.LocationID);
						}
					}
					if (detail.TaskID == null)
					{
						counters.ProjectTasksNull -= 1;
					}
					else
					{
						var kv = new KeyValuePair<int?, int?>(detail.ProjectID, detail.TaskID);
						if (counters.ProjectTasks.ContainsKey(kv))
						{
							if ((counters.ProjectTasks[kv] -= 1) == 0)
							{
								counters.ProjectTasks.Remove(kv);
							}
						}
					}
					if (detail.LotSerialNbr == null)
					{
						counters.LotSerNumbersNull -= 1;
					}
					else if (counters.LotSerNumbers.ContainsKey(detail.LotSerialNbr))
					{
						if (string.IsNullOrEmpty(detail.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(detail.AssignedNbr, detail.LotSerialNbr))
						{
							counters.UnassignedNumber--;
						}
						if ((counters.LotSerNumbers[detail.LotSerialNbr] -= 1) == 0)
						{
							counters.LotSerNumbers.Remove(detail.LotSerialNbr);
						}
					}
				}
				if (Det == null && OldDet != null)
				{
					if (counters.ExpireDates.Count == 1 && counters.ExpireDatesNull == 0)
					{
						foreach (DateTime? key in counters.ExpireDates.Keys)
						{
							counters.ExpireDate = key;
						}
					}
					if (counters.SubItems.Count == 1 && counters.SubItemsNull == 0)
					{
						foreach (int? key in counters.SubItems.Keys)
						{
							counters.SubItem = key;
						}
					}
					if (counters.Locations.Count == 1 && counters.LocationsNull == 0)
					{
						foreach (int? key in counters.Locations.Keys)
						{
							counters.Location = key;
						}
					}
					if (counters.ProjectTasks.Count == 1 && counters.ProjectTasksNull == 0)
					{
						foreach (KeyValuePair<int?, int?> key in counters.ProjectTasks.Keys)
						{
							counters.ProjectID = key.Key;
							counters.TaskID = key.Value;
						}
					}
					if (counters.LotSerNumbers.Count == 1 && counters.LotSerNumbersNull == 0)
					{
						foreach (string key in counters.LotSerNumbers.Keys)
						{
							counters.LotSerNumber = key;
						}
					}
				}
			}

			BaseQty = counters.BaseQty;

			switch (counters.RecordCount)
			{
				case 0:
					Row.LotSerialNbr = string.Empty;
					Row.HasMixedProjectTasks = false;
					break;
				case 1:
					Row.ExpireDate = counters.ExpireDate;
					Row.SubItemID = counters.SubItem;
					Row.LocationID = counters.Location;
					Row.LotSerialNbr = counters.LotSerNumber;
					Row.HasMixedProjectTasks = false;
					if (counters.ProjectTasks.Count > 0 && Det != null && counters.ProjectID != null)
					{
						Row.ProjectID = counters.ProjectID;
						Row.TaskID = counters.TaskID;
					}
					break;
				default:
					Row.ExpireDate = counters.ExpireDates.Count == 1 && counters.ExpireDatesNull == 0 ? counters.ExpireDate : null;
					Row.SubItemID = counters.SubItems.Count == 1 && counters.SubItemsNull == 0 ? counters.SubItem : null;
					Row.LocationID = counters.Locations.Count == 1 && counters.LocationsNull == 0 ? counters.Location : null;
					Row.HasMixedProjectTasks = counters.ProjectTasks.Count + (counters.ProjectTasks.Count > 0 ? counters.ProjectTasksNull : 0) > 1;
					if (Row.HasMixedProjectTasks != true && Det != null && counters.ProjectID != null)
					{
						Row.ProjectID = counters.ProjectID;
						Row.TaskID = counters.TaskID;
					}

					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
					INLotSerTrack.Mode mode = GetTranTrackMode(Row, item);
					if (mode == INLotSerTrack.Mode.None)
					{
						Row.LotSerialNbr = string.Empty;
					}
					else if ((mode & INLotSerTrack.Mode.Create) > 0 || (mode & INLotSerTrack.Mode.Issue) > 0)
					{
						//if more than 1 split exist at lotserial creation time ignore equilness and display <SPLIT>
						Row.LotSerialNbr = null;
					}
					else
					{
						Row.LotSerialNbr = counters.LotSerNumbers.Count == 1 && counters.LotSerNumbersNull == 0 ? counters.LotSerNumber : null;
					}
					break;
			}
		}

		public virtual void UpdateParent(PXCache sender, TLSMaster Row)
		{
			decimal BaseQty;
			UpdateParent(sender, Row, null, null, out BaseQty);
			Row.UnassignedQty = PXDBQuantityAttribute.Round((decimal)(Row.BaseQty - BaseQty));
		}

		public virtual void UpdateParent(PXCache sender, TLSDetail Row, TLSDetail OldRow)
		{
			TLSMaster parent = (TLSMaster)PXParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(TLSMaster));

			if (parent != null && (Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
			{
				TLSMaster oldrow = PXCache<TLSMaster>.CreateCopy(parent);
				decimal BaseQty;

				UpdateParent(sender, parent, Row, OldRow, out BaseQty);

				using (InvtMultScope<TLSMaster> ms = new InvtMultScope<TLSMaster>(parent))
				{
					if (BaseQty < parent.BaseQty)
					{
						parent.UnassignedQty = PXDBQuantityAttribute.Round((decimal)(parent.BaseQty - BaseQty));
					}
					else
					{
						parent.UnassignedQty = 0m;
						parent.BaseQty = BaseQty;
						parent.Qty = INUnitAttribute.ConvertFromBase(sender, parent.InventoryID, parent.UOM, (decimal)parent.BaseQty, INPrecision.QUANTITY);
					}
				}

				sender.Graph.Caches[typeof(TLSMaster)].MarkUpdated(parent);

				if (Math.Abs((Decimal)oldrow.Qty - (Decimal)parent.Qty) >= 0.0000005m)
				{
				sender.Graph.Caches[typeof(TLSMaster)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
				sender.Graph.Caches[typeof(TLSMaster)].RaiseRowUpdated(parent, oldrow);
			}
		}
		}

		public virtual void DefaultLotSerialNbr(PXCache sender, TLSDetail row)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

			if (item != null)
			{
				INLotSerTrack.Mode mode = GetTranTrackMode(row, item);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender, item);
					foreach (TLSDetail lssplit in INLotSerialNbrAttribute.CreateNumbers<TLSDetail>(sender, item, lotSerNum, mode, 1m))
					{
						if (string.IsNullOrEmpty(row.LotSerialNbr))
							row.LotSerialNbr = lssplit.LotSerialNbr;

						row.AssignedNbr = lssplit.AssignedNbr;
						row.LotSerClassID = lssplit.LotSerClassID;
					}
				}
			}
		}

		protected virtual bool Detail_ObjectsEqual(TLSDetail a, TLSDetail b)
		{
			if (a != null && b != null)
			{
				return (a.InventoryID == b.InventoryID && (a.IsStockItem != true ||
								a.SubItemID == b.SubItemID &&
								a.LocationID == b.LocationID &&
								(string.Equals(a.LotSerialNbr, b.LotSerialNbr) || string.IsNullOrEmpty(a.LotSerialNbr) && string.IsNullOrEmpty(b.LotSerialNbr)) &&
								(string.IsNullOrEmpty(a.AssignedNbr) || INLotSerialNbrAttribute.StringsEqual(a.AssignedNbr, a.LotSerialNbr) == false) &&
								(string.IsNullOrEmpty(b.AssignedNbr) || INLotSerialNbrAttribute.StringsEqual(b.AssignedNbr, b.LotSerialNbr) == false)));
			}
			else
			{
				return (a != null);
			}
		}

		protected virtual void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			TLSDetail a = (TLSDetail)e.Row;

			if (!string.IsNullOrEmpty(a.AssignedNbr) && INLotSerialNbrAttribute.StringsEqual(a.AssignedNbr, a.LotSerialNbr))
			{
				return;
			}

            if (_InternallCall && _Operation == PXDBOperation.Insert)
            {
                Counters counters;
                if (!DetailCounters.TryGetValue((TLSMaster)MasterCache.Current, out counters))
                {
                    DetailCounters[(TLSMaster)MasterCache.Current] = counters = new Counters();
                }
                UpdateCounters(MasterCache, counters, a);
            }

			if (_InternallCall && _Operation == PXDBOperation.Update)
			{
				foreach (object item in SelectDetail(sender, (TLSDetail)e.Row))
				{
					TLSDetail detailitem = (TLSDetail)item;

					if (Detail_ObjectsEqual((TLSDetail)e.Row, detailitem))
					{
						PXResult<InventoryItem, INLotSerClass> invtitem = ReadInventoryItem(sender, a.InventoryID);

						if (((INLotSerClass)invtitem).LotSerTrack != "S" || detailitem.BaseQty == 0m)
						{
							object oldDetailItem = PXCache<TLSDetail>.CreateCopy(detailitem);
							detailitem.BaseQty += ((TLSDetail)e.Row).BaseQty;
							detailitem.Qty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseQty, INPrecision.QUANTITY);

							sender.RaiseRowUpdated(detailitem, oldDetailItem);
							sender.MarkUpdated(detailitem);
							PXDBQuantityAttribute.VerifyForDecimal(sender, detailitem);
						}
						e.Cancel = true;
						break;
					}
				}
			}

			if (((TLSDetail)e.Row).InventoryID == null || string.IsNullOrEmpty(((TLSDetail)e.Row).UOM))
			{
				e.Cancel = true;
			}

			if (!e.Cancel)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((TLSDetail)e.Row).InventoryID);
				INLotSerTrack.Mode mode = GetTranTrackMode((TLSDetail) e.Row, item);

				if (mode != INLotSerTrack.Mode.None && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered && ((TLSDetail) e.Row).Qty == 0 && MasterCurrent.UnassignedQty >= 1)				
					((TLSDetail) e.Row).Qty = 1;

				if (((TLSDetail)e.Row).BaseQty == null || ((TLSDetail)e.Row).BaseQty == 0m || ((TLSDetail)e.Row).BaseQty != ((TLSDetail)e.Row).Qty || ((TLSDetail)e.Row).UOM != ((InventoryItem)item).BaseUnit)
				{
					((TLSDetail)e.Row).BaseQty = INUnitAttribute.ConvertToBase(sender, ((TLSDetail)e.Row).InventoryID, ((TLSDetail)e.Row).UOM, ((TLSDetail)e.Row).Qty ?? 0m, ((TLSDetail)e.Row).BaseQty, INPrecision.QUANTITY);
				}

				((TLSDetail)e.Row).UOM = ((InventoryItem)item).BaseUnit;
				((TLSDetail)e.Row).Qty = ((TLSDetail)e.Row).BaseQty;
			}
		}

		protected virtual void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_InternallCall)
			{
				return;
			}
			((TLSDetail)e.Row).BaseQty = INUnitAttribute.ConvertToBase(sender, ((TLSDetail)e.Row).InventoryID, ((TLSDetail)e.Row).UOM, (decimal)((TLSDetail)e.Row).Qty, ((TLSDetail)e.Row).BaseQty, INPrecision.QUANTITY);

			DefaultLotSerialNbr(sender, (TLSDetail)e.Row);

            if (!UnattendedMode)
            {
                ((TLSDetail)e.Row).ExpireDate = ExpireDateByLot(sender, ((TLSDetail)e.Row), null);
            }

			try
			{
				_InternallCall = true;
				UpdateParent(sender, (TLSDetail)e.Row, null);

                if (!UnattendedMode)
                {
                    AvailabilityCheck(sender, (TLSDetail)e.Row);
                }
			}
			finally
			{
				_InternallCall = false;
			}
		}

		protected virtual void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];
			ExpireCached(lscache, INLotSerialStatus((TLSDetail)e.OldRow));

			if (_InternallCall)
			{
				return;
			}

			if (((TLSDetail)e.Row).LotSerialNbr != ((TLSDetail)e.OldRow).LotSerialNbr)
			{
				((TLSDetail)e.Row).ExpireDate = ExpireDateByLot(sender, ((TLSDetail)e.Row), null);
            }

			((TLSDetail)e.Row).BaseQty = INUnitAttribute.ConvertToBase(sender, ((TLSDetail)e.Row).InventoryID, ((TLSDetail)e.Row).UOM, (decimal)((TLSDetail)e.Row).Qty, ((TLSDetail)e.Row).BaseQty, INPrecision.QUANTITY);

			try
			{
				_InternallCall = true;
				UpdateParent(sender, (TLSDetail)e.Row, (TLSDetail)e.OldRow);

                if (!UnattendedMode)
                {
                    AvailabilityCheck(sender, (TLSDetail)e.Row);
                }
			}
			finally
			{
				_InternallCall = false;
			}
		}

		protected virtual void Detail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];
			ExpireCached(lscache, INLotSerialStatus((TLSDetail)e.Row));

			if (_InternallCall)
			{
				return;
			}

			try
			{
				_InternallCall = true;
				UpdateParent(sender, null, (TLSDetail)e.Row);
			}
			finally
			{
				_InternallCall = false;
			}
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		protected virtual void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
			=> RaiseQtyExceptionHandling(sender, row, newValue, new PXExceptionInfo(e.ErrorLevel, e.MessageNoPrefix));

		protected virtual void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei) { }

		public virtual void AvailabilityCheck(PXCache sender, ILSMaster Row)
		{
			if (Row != null && Row.InvtMult == (short)-1 && Row.BaseQty > 0m)
			{
				IQtyAllocated availability = AvailabilityFetch(sender, Row, AvailabilityFetchMode.ExcludeCurrent);

				AvailabilityCheck(sender, Row, availability);
			}
		}

		public virtual void AvailabilityCheck(PXCache sender, ILSMaster Row, IQtyAllocated availability)
		{
			foreach(var errorInfo in GetAvailabilityCheckErrors(sender, Row, availability))
				RaiseQtyExceptionHandling(sender, Row, Row.Qty, errorInfo);
		}

		public virtual IEnumerable<PXExceptionInfo> GetAvailabilityCheckErrors(PXCache sender, ILSMaster Row)
		{
			if (Row != null && Row.InvtMult == -1 && Row.BaseQty > 0m)
			{
				IQtyAllocated availability = AvailabilityFetch(sender, Row, AvailabilityFetchMode.ExcludeCurrent);

				return GetAvailabilityCheckErrors(sender, Row, availability);
			}
			return Array.Empty<PXExceptionInfo>();
		}

		protected virtual IEnumerable<PXExceptionInfo> GetAvailabilityCheckErrors(PXCache sender, ILSMaster Row, IQtyAllocated availability)
		{
			if (Row.InvtMult == -1 && Row.BaseQty > 0m && availability != null)
			{
				if (availability.QtyNotAvail < 0m && (availability.QtyAvail + availability.QtyNotAvail) < 0m)
				{
					if (availability is LotSerialStatus)
						yield return new PXExceptionInfo(PXErrorLevel.Warning, Messages.StatusCheck_QtyLotSerialNegative);
					else if (availability is LocationStatus)
						yield return new PXExceptionInfo(PXErrorLevel.Warning, Messages.StatusCheck_QtyLocationNegative);
					else if (availability is SiteStatus)
						yield return new PXExceptionInfo(PXErrorLevel.Warning, Messages.StatusCheck_QtyNegative);
				}				
			}
		}

		public virtual void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 255, null, _AvailField, false, null, null, null, null, null, null);
			((PXFieldState)e.ReturnState).Visible = false;
			((PXFieldState)e.ReturnState).Visibility = PXUIVisibility.Invisible;
			((PXFieldState)e.ReturnState).DisplayName = _AvailFieldDisplayName;
		}

		public virtual IQtyAllocated AvailabilityFetch(PXCache sender, ILSMaster Row, AvailabilityFetchMode fetchMode)
		{
			if (Row != null)
			{
				TLSDetail copy = Row as TLSDetail;
				if (copy == null)
				{
					copy = Convert(Row as TLSMaster);

					PXParentAttribute.SetParent(DetailCache, copy, typeof(TLSMaster), Row);

					if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
					{
						DefaultLotSerialNbr(sender.Graph.Caches[typeof(TLSDetail)], copy);
					}
				}
				return AvailabilityFetch(sender, copy, fetchMode);
			}
			return null;
		}

		protected T InsertWith<T>(PXGraph graph, T row, PXRowInserted handler)
			where T: class, IBqlTable, new()
		{
			graph.RowInserted.AddHandler<T>(handler);
			try
			{
				return PXCache<T>.Insert(graph, row);
			}
			finally
			{
				graph.RowInserted.RemoveHandler<T>(handler);
			}

		}

		public virtual IQtyAllocated AvailabilityFetch(PXCache sender, ILSDetail Row, AvailabilityFetchMode fetchMode)
		{
			if (Row?.InventoryID == null) return null;
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			if (item == null || (INLotSerClass) item == null || ((INLotSerClass) item).LotSerTrack == null) return null;

			if (Row.SubItemID != null
			    && Row.SiteID != null
			    && Row.LocationID != null
			    && string.IsNullOrEmpty(Row.LotSerialNbr) == false
			    && (string.IsNullOrEmpty(Row.AssignedNbr) || INLotSerialNbrAttribute.StringsEqual(Row.AssignedNbr, Row.LotSerialNbr) == false)
			    && ((INLotSerClass)item).LotSerAssign == INLotSerAssign.WhenReceived)
			{
				return AvailabilityFetchLotSerial(sender, Row, fetchMode);
			}
			else if (Row.SubItemID != null && Row.SiteID != null && Row.LocationID != null)
				{ 
				return AvailabilityFetchLocation(sender, Row, fetchMode);
				}
			else if (Row.SubItemID != null && Row.SiteID != null)
				{
				return AvailabilityFetchSite(sender, Row, fetchMode);
			}
			else return null;
		}

		private IQtyAllocated AvailabilityFetchLotSerial(PXCache sender, ILSDetail Row, AvailabilityFetchMode fetchMode)
		{
			LotSerialStatus acc = new LotSerialStatus
			{
				InventoryID = Row.InventoryID,
				SubItemID = Row.SubItemID,
				SiteID = Row.SiteID,
				LocationID = Row.LocationID,
				LotSerialNbr = Row.LotSerialNbr
			};

			acc = InsertWith(sender.Graph, acc,
				(cache, e) =>
				{
					cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
					cache.IsDirty = false;
				});

			INLotSerialStatus status = IN.INLotSerialStatus.PK.Find(sender.Graph, Row.InventoryID, Row.SubItemID, Row.SiteID, Row.LocationID, Row.LotSerialNbr);

			return AvailabilityFetch<LotSerialStatus>(Row as TLSDetail, PXCache<LotSerialStatus>.CreateCopy(acc), status, fetchMode);
				}

		private IQtyAllocated AvailabilityFetchLocation(PXCache sender, ILSDetail Row, AvailabilityFetchMode fetchMode)
		{
			LocationStatus acc = new LocationStatus
				{
				InventoryID = Row.InventoryID,
				SubItemID = Row.SubItemID,
				SiteID = Row.SiteID,
				LocationID = Row.LocationID
			};

			acc = InsertWith(sender.Graph, acc,
				(cache, e) =>
				{
					cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
					cache.IsDirty = false;
				});

			INLocationStatus status = INLocationStatus.PK.Find(sender.Graph, Row.InventoryID, Row.SubItemID, Row.SiteID, Row.LocationID);

			return AvailabilityFetch<LocationStatus>(Row as TLSDetail, PXCache<LocationStatus>.CreateCopy(acc), status, fetchMode);
				}

		private IQtyAllocated AvailabilityFetchSite(PXCache sender, ILSDetail Row, AvailabilityFetchMode fetchMode)
		{
			SiteStatus acc = new SiteStatus
				{
				InventoryID = Row.InventoryID,
				SubItemID = Row.SubItemID,
				SiteID = Row.SiteID
			};

			acc = InsertWith(sender.Graph, acc,
				(cache, e) =>
				{
					cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
					cache.IsDirty = false;
				});

			INSiteStatus status = INSiteStatus.PK.Find(sender.Graph, Row.InventoryID, Row.SubItemID, Row.SiteID);

			return AvailabilityFetch<SiteStatus>(Row as TLSDetail, PXCache<SiteStatus>.CreateCopy(acc), status, fetchMode);
		}

		protected virtual IQtyAllocated AvailabilityFetch<TNode>(ILSDetail Row, IQtyAllocated allocated, IStatus status, AvailabilityFetchMode fetchMode)
			where TNode : class, IQtyAllocated, IBqlTable, new()
		{
			if (status != null)
			{
				allocated.QtyOnHand += status.QtyOnHand;
				allocated.QtyAvail += status.QtyAvail;
				allocated.QtyHardAvail += status.QtyHardAvail;
				allocated.QtyActual += status.QtyActual;
			}

			if (fetchMode.HasFlag(AvailabilityFetchMode.ExcludeCurrent))
			{
				decimal SignQtyAvail;
				decimal SignQtyHardAvail;
				decimal SignQtyActual;
				INItemPlanIDAttribute.GetInclQtyAvail<TNode>(DetailCache, Row, out SignQtyAvail, out SignQtyHardAvail, out SignQtyActual);

				if (SignQtyAvail != 0)
				{
					allocated.QtyAvail -= SignQtyAvail * (Row.BaseQty ?? 0m);
					allocated.QtyNotAvail += SignQtyAvail * (Row.BaseQty ?? 0m);
				}

				if (SignQtyHardAvail != 0)
				{
					allocated.QtyHardAvail -= SignQtyHardAvail * (Row.BaseQty ?? 0m);					
				}

				if (SignQtyActual != 0)
				{
					allocated.QtyActual -= SignQtyActual * (Row.BaseQty ?? 0m);
				}
			}
			return allocated;			
		}


		public virtual TLSMaster CloneMaster(TLSMaster item)
		{
			return PXCache<TLSMaster>.CreateCopy(item);
		}

		protected virtual void Master_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			
			//Logic located in child entities
		}

		protected virtual void _Master_RowInserted(PXCache sender, PXRowInsertedEventArgs<TLSMaster> e)
		{
			e.Row.BaseQty = INUnitAttribute.ConvertToBase(sender, e.Row.InventoryID, e.Row.UOM, (decimal)e.Row.Qty, e.Row.BaseQty, INPrecision.QUANTITY);

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

			if (item != null && (((InventoryItem)item).StkItem == true || ((InventoryItem)item).KitItem != true))
			{
				INLotSerTrack.Mode mode = GetTranTrackMode(e.Row, item);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
          //count for ZERO serial items only here
					if (string.IsNullOrEmpty(e.Row.LotSerialNbr) == false && (e.Row.BaseQty == 0m || e.Row.BaseQty == 1m || ((INLotSerClass)item).LotSerTrack != INLotSerTrack.SerialNumbered))
					{
						UpdateNumbers(sender, e.Row, (decimal)e.Row.BaseQty );
					}
					else
					{
						CreateNumbers(sender, e.Row);
          }
					UpdateParent(sender, e.Row);
				}
				else if ((mode & INLotSerTrack.Mode.Issue) > 0 && e.Row.BaseQty > 0m)
				{
					IssueNumbers(sender, e.Row);

					//do not set Zero LotSerial which will prevent IssueNumbers() on quantity update
					if (e.Row.BaseQty > 0)
					{
						UpdateParent(sender, e.Row);
					}
				}
				else if (e.Row.BaseQty == 0m && e.Row.UnassignedQty != 0m)
					e.Row.UnassignedQty = 0m;

				//PCB AvailabilityCheck(sender, e.Row);
			}
			else if (item != null)
			{
				KitInProcessing = item;
				try
				{
					foreach (PXResult<INKitSpecStkDet, InventoryItem> res in PXSelectJoin<INKitSpecStkDet,
						InnerJoin<InventoryItem, On<INKitSpecStkDet.FK.CompInventoryItem>>,
						Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Select(sender.Graph, e.Row.InventoryID))
					{
						INKitSpecStkDet kititem = (INKitSpecStkDet)res;
						InventoryItem item2 = (InventoryItem)res;

						if (item2.ItemStatus == INItemStatus.Inactive)
						{
							throw new PXException(SO.Messages.KitComponentIsInactive, item2.InventoryCD);
						}

						TLSMaster copy = CloneMaster(e.Row);

						copy.InventoryID = kititem.CompInventoryID;
						copy.SubItemID = kititem.CompSubItemID;
						copy.UOM = kititem.UOM;
						copy.Qty = kititem.DfltCompQty * copy.BaseQty;

						try
						{
							_Master_RowInserted(sender, new PXRowInsertedEventArgs<TLSMaster>(copy, e.ExternalCall));
						}
						catch (PXException ex)
						{
							throw new PXException(ex, Messages.FailedToProcessComponent, item2.InventoryCD, ((InventoryItem)item).InventoryCD, ex.MessageNoPrefix);
						}
					}
				}
				finally
				{
					KitInProcessing = null;
				}

				foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in PXSelectJoin<INKitSpecNonStkDet,
					InnerJoin<InventoryItem, On<INKitSpecNonStkDet.FK.CompInventoryItem>>,
					Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
						And<Where<InventoryItem.kitItem, Equal<True>, Or<InventoryItem.nonStockShip, Equal<True>>>>>>.Select(sender.Graph, e.Row.InventoryID))
				{
					INKitSpecNonStkDet kititem = res;
					InventoryItem item2 = res;

					TLSMaster copy = CloneMaster(e.Row);

					copy.InventoryID = kititem.CompInventoryID;
					copy.SubItemID = null;
					copy.UOM = kititem.UOM;
					copy.Qty = kititem.DfltCompQty * copy.BaseQty;

					try
					{
					_Master_RowInserted(sender, new PXRowInsertedEventArgs<TLSMaster>(copy, e.ExternalCall));
				}
					catch (PXException ex)
					{
						throw new PXException(ex, Messages.FailedToProcessComponent, item2.InventoryCD, ((InventoryItem)item).InventoryCD, ex.MessageNoPrefix);
					}
				}
			}
		}

		protected virtual void _Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs<TLSMaster> e)
		{
			PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];
			if (e.Row != null)
				DetailCounters.Remove(e.Row);
			foreach (object detail in SelectDetail(cache, e.Row))
			{
				cache.Delete(detail);
			}
		}

		public virtual void RaiseRowInserted(PXCache sender, TLSMaster Row)
		{
			_Master_RowInserted(sender, new PXRowInsertedEventArgs<TLSMaster>(Row, false));
		}

		public virtual void RaiseRowDeleted(PXCache sender, TLSMaster Row)
		{
			_Master_RowDeleted(sender, new PXRowDeletedEventArgs<TLSMaster>(Row, false));
		}

		protected virtual void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<TLSMaster> e)
		{
			//Debug.Print("_Master_RowUpdated");
			if (e.OldRow != null && (e.OldRow.InventoryID != e.Row.InventoryID || e.OldRow.InvtMult != e.Row.InvtMult || (e.OldRow.UOM != null && e.Row.UOM == null) || (e.OldRow.UOM == null && e.Row.UOM != null)))
			{
				if (!sender.Graph.IsContractBasedAPI)
				{
					if (e.OldRow.InventoryID != e.Row.InventoryID)
					{
						((TLSMaster)e.Row).LotSerialNbr = null;
						((TLSMaster)e.Row).ExpireDate = null;
					}
					else if (e.OldRow.InvtMult != e.Row.InvtMult)
					{
						if (((TLSMaster)e.Row).LotSerialNbr == ((TLSMaster)e.OldRow).LotSerialNbr)
						{
							((TLSMaster)e.Row).LotSerialNbr = null;
						}
						if (((TLSMaster)e.Row).ExpireDate == ((TLSMaster)e.OldRow).ExpireDate)
						{
							((TLSMaster)e.Row).ExpireDate = null;
						}
					}
				}

				RaiseRowDeleted(sender, e.OldRow);
				RaiseRowInserted(sender, e.Row);
			}
			else
			{
				e.Row.BaseQty = INUnitAttribute.ConvertToBase(sender, e.Row.InventoryID, e.Row.UOM, (decimal)e.Row.Qty, e.Row.BaseQty, INPrecision.QUANTITY);

				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

				if (e.Row.ExpireDate == e.OldRow.ExpireDate && e.OldRow.LotSerialNbr != e.Row.LotSerialNbr)
				{
					((TLSMaster)e.Row).ExpireDate = null;
				}
				if (item != null && (((InventoryItem)item).StkItem == true || ((InventoryItem)item).KitItem != true))
				{
					string itemLotSerTrack = ((INLotSerClass)item).LotSerTrack;

					INLotSerTrack.Mode mode = GetTranTrackMode(e.Row, item);
					if (mode == INLotSerTrack.Mode.None ||(mode & INLotSerTrack.Mode.Create) > 0 )
					{
						if (e.Row.SubItemID != e.OldRow.SubItemID || e.Row.SiteID != e.OldRow.SiteID || e.Row.LocationID != e.OldRow.LocationID || e.Row.ExpireDate != e.OldRow.ExpireDate)
						{
							if (CorrectionMode == false && (itemLotSerTrack == INLotSerTrack.NotNumbered || itemLotSerTrack == null))
							{
								RaiseRowDeleted(sender, e.OldRow);
								RaiseRowInserted(sender, e.Row);
								return;
							}
							else
							{
								UpdateNumbers(sender, e.Row);
							}
						}
						//count for ZERO serial items only here
						if (string.IsNullOrEmpty(e.Row.LotSerialNbr) == false && (e.Row.BaseQty == 0m || e.Row.BaseQty == 1m || itemLotSerTrack != INLotSerTrack.SerialNumbered))
						{
							UpdateNumbers(sender, e.Row, (decimal)e.Row.BaseQty - (decimal)e.OldRow.BaseQty);
						}
						else if (e.Row.BaseQty > e.OldRow.BaseQty)
						{
							CreateNumbers(sender, e.Row, (decimal)e.Row.BaseQty - (decimal)e.OldRow.BaseQty);
						}
						//do not truncate ZERO quantity lotserials
						else if (e.Row.BaseQty < e.OldRow.BaseQty)
						{
							TruncateNumbers(sender, e.Row, (decimal)e.OldRow.BaseQty - (decimal)e.Row.BaseQty);
						}

						UpdateParent(sender, e.Row);
					}
					else if ((mode & INLotSerTrack.Mode.Issue) > 0)
					{
						if (e.Row.SubItemID != e.OldRow.SubItemID || e.Row.SiteID != e.OldRow.SiteID || e.Row.LocationID != e.OldRow.LocationID || string.Equals(e.Row.LotSerialNbr, e.OldRow.LotSerialNbr) == false)
						{
							RaiseRowDeleted(sender, e.OldRow);
							RaiseRowInserted(sender, e.Row);
						}
						else if (string.IsNullOrEmpty(e.Row.LotSerialNbr) == false &&
							(e.Row.BaseQty == 1m || (itemLotSerTrack != INLotSerTrack.SerialNumbered && e.OldRow.UnassignedQty == 0)))
						{
							UpdateNumbers(sender, e.Row, (decimal)e.Row.BaseQty - (decimal)e.OldRow.BaseQty);
						}
						else if (e.Row.BaseQty > e.OldRow.BaseQty)
						{
							IssueNumbers(sender, e.Row, (decimal)e.Row.BaseQty - (decimal)e.OldRow.BaseQty);
						}
						else if (e.Row.BaseQty < e.OldRow.BaseQty)
						{
							TruncateNumbers(sender, e.Row, (decimal)e.OldRow.BaseQty - (decimal)e.Row.BaseQty);
						}

						//do not set Zero LotSerial which will prevent IssueNumbers() on quantity update
						if (e.Row.BaseQty > 0)
						{
							UpdateParent(sender, e.Row);
						}
					}
					//PCB AvailabilityCheck(sender, e.Row);
				}
				else if (item != null)
				{
					KitInProcessing = item;
					try
					{
						foreach (PXResult<INKitSpecStkDet, InventoryItem> res in PXSelectJoin<INKitSpecStkDet, InnerJoin<InventoryItem, On<INKitSpecStkDet.FK.CompInventoryItem>>, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Select(sender.Graph, e.Row.InventoryID))
						{
							INKitSpecStkDet kititem = (INKitSpecStkDet)res;
							InventoryItem item2 = (InventoryItem)res;

							if (item2.ItemStatus == INItemStatus.Inactive)
							{
								throw new PXException(SO.Messages.KitComponentIsInactive, item2.InventoryCD);
							}

							TLSMaster copy = CloneMaster(e.Row);

							copy.InventoryID = kititem.CompInventoryID;
							copy.SubItemID = kititem.CompSubItemID;
							copy.UOM = kititem.UOM;
							copy.Qty = kititem.DfltCompQty * copy.BaseQty;

							TLSMaster oldcopy = CloneMaster(e.OldRow);

							oldcopy.InventoryID = kititem.CompInventoryID;
							oldcopy.SubItemID = kititem.CompSubItemID;
							oldcopy.UOM = kititem.UOM;
							oldcopy.Qty = kititem.DfltCompQty * oldcopy.BaseQty;

							if (!DetailCounters.TryGetValue(copy, out counters))
							{
								DetailCounters[copy] = counters = new Counters();
								foreach (TLSDetail detail in SelectDetail(sender.Graph.Caches[typeof(TLSDetail)], copy))
								{
									UpdateCounters(sender, counters, detail);
								}
							}

							//oldcopy.BaseQty = INUnitAttribute.ConvertToBase(sender, oldcopy.InventoryID, oldcopy.UOM, (decimal)oldcopy.Qty, INPrecision.QUANTITY);
							oldcopy.BaseQty = counters.BaseQty;

							try
							{
								_Master_RowUpdated(sender, new PXRowUpdatedEventArgs<TLSMaster>(copy, oldcopy, e.ExternalCall));
							}
							catch (PXException ex)
							{
								throw new PXException(ex, Messages.FailedToProcessComponent, item2.InventoryCD, ((InventoryItem)item).InventoryCD, ex.MessageNoPrefix);
							}

						}
					}
					finally
					{
						KitInProcessing = null;
					}

                    
					foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in PXSelectJoin<INKitSpecNonStkDet, 
						InnerJoin<InventoryItem, On<INKitSpecNonStkDet.FK.CompInventoryItem>>, 
						Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
							And<Where<InventoryItem.kitItem, Equal<True>, Or<InventoryItem.nonStockShip, Equal<True>>>>>>.Select(sender.Graph, e.Row.InventoryID))
					{
						INKitSpecNonStkDet kititem = res;
						InventoryItem item2 = res;

						if (item2.ItemStatus == INItemStatus.Inactive)
						{
							throw new PXException(SO.Messages.KitComponentIsInactive, item2.InventoryCD);
						}

						TLSMaster copy = CloneMaster(e.Row);

						copy.InventoryID = kititem.CompInventoryID;
						copy.SubItemID = null;
						copy.UOM = kititem.UOM;
						copy.Qty = kititem.DfltCompQty * copy.BaseQty;

						TLSMaster oldcopy = CloneMaster(e.OldRow);

						oldcopy.InventoryID = kititem.CompInventoryID;
						oldcopy.SubItemID = null;
						oldcopy.UOM = kititem.UOM;
						oldcopy.Qty = kititem.DfltCompQty * oldcopy.BaseQty;
						oldcopy.BaseQty = INUnitAttribute.ConvertToBase(sender, oldcopy.InventoryID, oldcopy.UOM, (decimal)oldcopy.Qty, oldcopy.BaseQty, INPrecision.QUANTITY);

						try
						{
						_Master_RowUpdated(sender, new PXRowUpdatedEventArgs<TLSMaster>(copy, oldcopy, e.ExternalCall));
					}
						catch (PXException ex)
						{
							throw new PXException(ex, Messages.FailedToProcessComponent, item2.InventoryCD, ((InventoryItem)item).InventoryCD, ex.MessageNoPrefix);
						}
					}
				}
			}
		}

		protected virtual void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_InternallCall)
			{
				return;
			}

			try
			{
				_InternallCall = true;
                _Operation = PXDBOperation.Insert;
				using (InvtMultScope<TLSMaster> ms = new InvtMultScope<TLSMaster>((TLSMaster)e.Row))
				{
					_Master_RowInserted(sender, new PXRowInsertedEventArgs<TLSMaster>(e));
				}
			}
			finally
			{
				_InternallCall = false;
                _Operation = PXDBOperation.Normal;
			}
		}

		protected virtual void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_InternallCall)
			{
				return;
			}

			try
			{
				_InternallCall = true;
                _Operation = PXDBOperation.Update;
				using (InvtMultScope<TLSMaster> ms = new InvtMultScope<TLSMaster>((TLSMaster)e.Row, (TLSMaster)e.OldRow))
				{
					_Master_RowUpdated(sender, new PXRowUpdatedEventArgs<TLSMaster>(e));
				}
			}
			finally
			{
				_InternallCall = false;
                _Operation = PXDBOperation.Normal;
			}
		}

		protected virtual void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			try
			{
				_InternallCall = true;
				_Operation = PXDBOperation.Delete;
				foreach (var split in PXParentAttribute.SelectChildren(DetailCache, e.Row, typeof(TLSMaster)))
				{
					DetailCache.Delete(split);
				}
			}
			finally
			{
				_InternallCall = false;
				_Operation = PXDBOperation.Normal;
			}
		}

		public override TLSMaster Insert(TLSMaster item)
		{
			try
			{
				_InternallCall = true;
                _Operation = PXDBOperation.Delete;
				return (TLSMaster)MasterCache.Insert(item);
			}
			finally
			{
				_InternallCall = false;
                _Operation = PXDBOperation.Normal;
			}
		}

		protected virtual void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];

                Counters counters;
                if (DetailCounters.TryGetValue((TLSMaster)e.Row, out counters) && counters.UnassignedNumber == 0)
                {
                    return;
                }

				TLSMaster master = (TLSMaster)e.Row;
				object[] selected = SelectDetail(cache, master);
				if (master != null)
				{
					_selected[master] = selected;
				}
				foreach (object detail in selected)
				{
					try
					{
						_InternallCall = true;
						Detail_RowPersisting(cache, new PXRowPersistingEventArgs(e.Operation, detail));
					}
					finally
					{
						_InternallCall = false;
					}
					if (string.IsNullOrEmpty(((TLSMaster)e.Row).LotSerialNbr) == false)
					{
						((TLSMaster)e.Row).LotSerialNbr = ((TLSDetail)detail).LotSerialNbr;
						break;
					}
                   
                    //if (((TLSDetail)detail).ExpireDate == null)
                    //{
                    //    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((TLSMaster)e.Row).InventoryID);
                    //    if (item != null && ((INLotSerClass)item).LotSerTrackExpiration == true && 
                    //        sender.RaiseExceptionHandling<INComponentTran.inventoryID>(e.Row, ((InventoryItem)item).InventoryCD, new PXSetPropertyException(Messages.OneOrMoreExpDateIsEmpty)))
                    //    {
                    //        throw new PXRowPersistingException(typeof(INComponentTran.inventoryID).Name, null, Messages.OneOrMoreExpDateIsEmpty);
                    //    }
                    //}
				}
			}
		}

		protected virtual void Master_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			TLSMaster master = (TLSMaster)e.Row;
			if (e.TranStatus == PXTranStatus.Aborted)
			{
				RestoreLotSerNumbers(sender.Graph);

				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
				{
					PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];

					object[] selected;
					if (master == null || !_selected.TryGetValue(master, out selected))
					{
						selected = SelectDetail(cache, master);
					}
					foreach (object detail in selected)
					{
						Detail_RowPersisted(cache, new PXRowPersistedEventArgs(detail, e.Operation, e.TranStatus, e.Exception));
						if (string.IsNullOrEmpty(((TLSMaster)e.Row).LotSerialNbr) == false)
						{
							((TLSMaster)e.Row).LotSerialNbr = ((TLSDetail)detail).LotSerialNbr;
							break;
						}
					}
				}
			}
			if (master != null && e.TranStatus != PXTranStatus.Open)
			{
				_selected.Remove(master);
			}
		}

		public virtual void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			VerifySNQuantity(sender, e, (ILSMaster)e.Row, _MasterQtyField);
		}

		public virtual void Detail_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSDetail)e.Row).InventoryID);

			if (item != null)
			{
				e.NewValue = ((InventoryItem)item).BaseUnit;
				e.Cancel = true;
				//otherwise default via attribute
			}
		}

		public virtual void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var detail = (ILSDetail)e.Row;
			if (IsTrackSerial(sender, detail)
				&& detail.LotSerialNbr != Messages.Unassigned)	// crutch for AC-97716
			{
				if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m)
				{
					e.NewValue = 1m;
				}
			}
		}

		public virtual void VerifySNQuantity(PXCache sender, PXFieldVerifyingEventArgs e, ILSMaster line, string qtyFieldName)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, line.InventoryID);

			if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				if (e.NewValue != null)
				{
					try
					{
						decimal BaseQty = INUnitAttribute.ConvertToBase(sender, line.InventoryID, line.UOM, (decimal)e.NewValue, INPrecision.NOROUND);
						if (decimal.Remainder(BaseQty, 1m) > 0m)
						{
							decimal power = (decimal)Math.Pow(10, (double)CommonSetupDecPl.Qty);
							for (decimal i = Math.Floor(BaseQty); ; i++)
							{
								e.NewValue = INUnitAttribute.ConvertFromBase(sender, line.InventoryID, line.UOM, i, INPrecision.NOROUND);

								if (decimal.Remainder((decimal)e.NewValue * power, 1m) == 0m)
									break;
							}
							sender.RaiseExceptionHandling(qtyFieldName, e.Row, null, new PXSetPropertyException(IN.Messages.SerialItem_LineQtyUpdated, PXErrorLevel.Warning));
						}
					}
					catch (PXUnitConversionException) { }
				}
			}
		}

		public virtual bool IsTrackSerial(PXCache sender, ILSDetail row)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

			if (item == null)
				return false;

			return INLotSerialNbrAttribute.IsTrackSerial(item, row.TranType, row.InvtMult);
		}

		protected Dictionary<object, string> _persisted = new Dictionary<object, string>();
		protected Dictionary<object, object[]> _selected = new Dictionary<object, object[]>();

		protected virtual void ThrowEmptyLotSerNumVal(PXCache sender, object data)
		{
			string _ItemFieldName = null;
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is InventoryAttribute)
				{
					_ItemFieldName = attr.FieldName;
					break;
				}
			}
			//the only reason can be overflow in serial numbering which will cause '0000' number to be treated like not-generated
			throw new PXException(Messages.LSCannotAutoNumberItem, sender.GetValueExt(data, _ItemFieldName));
		}       

		public virtual void Detail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (string.IsNullOrEmpty(((ILSDetail)e.Row).AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(((ILSDetail)e.Row).AssignedNbr, ((ILSDetail)e.Row).LotSerialNbr))
				{
					string numVal = string.Empty;
					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSDetail)e.Row).InventoryID);
					ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender, item);
					try
					{
						numVal = AutoNumberAttribute.NextNumber(lotSerNum.LotSerNumVal);
					}
					catch (AutoNumberException)
					{
						ThrowEmptyLotSerNumVal(sender, e.Row);
					}

					string _KeyToAbort = INLotSerialNbrAttribute.UpdateNumber(
						((ILSDetail)e.Row).AssignedNbr,
						((ILSDetail)e.Row).LotSerialNbr,
						numVal);

					((ILSDetail)e.Row).LotSerialNbr = _KeyToAbort;

					try
					{
						_persisted.Add(e.Row, _KeyToAbort);
					}
					catch (ArgumentException)
					{
						//the only reason can be overflow in serial numbering which will cause '0000' number to be treated like not-generated
						ThrowEmptyLotSerNumVal(sender, e.Row);
					}
					UpdateLotSerNumVal(lotSerNum, numVal, item);
					sender.RaiseRowUpdated(e.Row, PXCache<TLSDetail>.CreateCopy((TLSDetail)e.Row));
				}
			}
		}

		public virtual void Detail_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted)
			{
				string _KeyToAbort = null;

				if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
					_persisted.TryGetValue(e.Row, out _KeyToAbort))
				{
					((ILSDetail)e.Row).LotSerialNbr = INLotSerialNbrAttribute.MakeNumber(((ILSDetail)e.Row).AssignedNbr, ((ILSDetail)e.Row).LotSerialNbr, sender.Graph.Accessinfo.BusinessDate.GetValueOrDefault());
					_persisted.Remove(e.Row);
				}
				PXOuterException exception = e.Exception as PXOuterException;
				if (exception != null && object.ReferenceEquals(e.Row, exception.Row) && !UnattendedMode)
				{
					TLSDetail row = (TLSDetail)e.Row;
					TLSMaster master = SelectMaster(sender, row);

					for (int i = 0; i < exception.InnerFields.Length; i++)
					{
						if (!MasterCache.RaiseExceptionHandling(exception.InnerFields[i], master, null, new PXSetPropertyException(exception.InnerMessages[i])))
						{
							exception.InnerRemove(exception.InnerFields[i]);
						}
					}
				}
			}
			else if (e.TranStatus == PXTranStatus.Completed)
			{
				_persisted.Remove(e.Row);
			}

		}

		#endregion

		#region Store & Restore Lot/Serial numbers after aborted

		private Dictionary<ILotSerNumVal, ILotSerNumVal> _lotSerNumVals;

		private void StoreLotSerNumVals(PXCache cache, IEnumerable numbersCollection)
		{
			foreach (ILotSerNumVal lotSerNumVal in numbersCollection)
				_lotSerNumVals.Add(lotSerNumVal, (ILotSerNumVal)cache.CreateCopy(lotSerNumVal));
		}

		private void RestoreLotSerNumVals(PXCache cache, IEnumerable numbersCollection)
		{
			var numbers = numbersCollection.OfType<ILotSerNumVal>().ToList();
			foreach (var newNumber in numbers)
			{
				ILotSerNumVal oldNumber;
				if (_lotSerNumVals.TryGetValue(newNumber, out oldNumber))
				{
					cache.RestoreCopy(newNumber, oldNumber);
					_lotSerNumVals.Remove(newNumber);
				}
				else
					cache.Remove(newNumber);
			}
		}

		private bool RestoreLotSerNumbers(PXGraph graph)
		{
			if (_lotSerNumVals == null)
				return false;

			var cache = graph.Caches[typeof(INLotSerClassLotSerNumVal)];
			cache.Current = null;
			RestoreLotSerNumVals(cache, cache.Cached);
			cache.Normalize();
			cache.ClearQueryCache();

			cache = graph.Caches[typeof(InventoryItemLotSerNumVal)];
			cache.Current = null;
			RestoreLotSerNumVals(cache, cache.Cached);
			cache.Normalize();
			cache.ClearQueryCache();

			_lotSerNumVals = null;
			return true;
		}

		private void OnBeforePersist(PXGraph graph)
		{
			_lotSerNumVals = new Dictionary<ILotSerNumVal, ILotSerNumVal>();

			var cache = graph.Caches[typeof(INLotSerClassLotSerNumVal)];
			StoreLotSerNumVals(cache, cache.Inserted);
			StoreLotSerNumVals(cache, cache.Updated);

			cache = graph.Caches[typeof(InventoryItemLotSerNumVal)];
			StoreLotSerNumVals(cache, cache.Inserted);
			StoreLotSerNumVals(cache, cache.Updated);
		}

		#endregion

		#region Inner Types
		[Serializable]
        public partial class LotSerOptions : LSSelect.LotSerOptions
        {
        }

		public class Counters : LSSelect.Counters
		{
		}

		protected virtual NotDecimalUnitErrorRedirectorScope<TDetailQty> ResolveNotDecimalUnitErrorRedirectorScope<TDetailQty>(object row)
			where TDetailQty : IBqlField
		{
			if (MasterQtyField == null)
				throw new PXArgumentException(nameof(MasterQtyField));
			return new NotDecimalUnitErrorRedirectorScope<TDetailQty>(MasterCache, row, MasterQtyField);
		}

		#endregion

		public virtual InventoryItem KitInProcessing { get; set; }

		public virtual bool IsIndivisibleComponent(InventoryItem inventory) => KitInProcessing != null && inventory.DecimalBaseUnit != true;
	}

	public interface ILotSerNumVal
	{
		String LotSerNumVal
		{
			get;
			set;
		}
	}



	#endregion

	#region LSINTran

	public class LSINTran : LSSelect<INTran, INTranSplit,
		Where<INTranSplit.docType, Equal<Current<INRegister.docType>>,
		And<INTranSplit.refNbr, Equal<Current<INRegister.refNbr>>>>>
	{
		#region Ctor
		public LSINTran(PXGraph graph)
			: base(graph)
		{
			this.MasterQtyField = typeof(INTran.qty);
			graph.FieldDefaulting.AddHandler<INTranSplit.subItemID>(INTranSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INTranSplit.locationID>(INTranSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INTranSplit.invtMult>(INTranSplit_InvtMult_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INTranSplit.lotSerialNbr>(INTranSplit_LotSerialNbr_FieldDefaulting);
            graph.RowPersisting.AddHandler<INTranSplit>(INTranSplit_RowPersisting);
			graph.RowUpdated.AddHandler<INRegister>(INRegister_RowUpdated);
            graph.RowSelected.AddHandler<INTran>(INTran_RowSelected);
		}

        #endregion

        #region Implementation
        protected virtual void INRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<INRegister.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<INRegister.hold>(e.Row) == false)
			{ 
				PXCache cache = sender.Graph.Caches[typeof(INTran)];

				foreach (INTran item in PXParentAttribute.SelectSiblings(cache, null, typeof(INRegister)))
				{
					if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
					{
						cache.RaiseExceptionHandling<INTran.qty>(item, item.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

						cache.MarkUpdated(item);
					}
				}
			}
		}

        private void INTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (INTran)e.Row;
            if (row == null)
                return;

            InventoryItem ii = InventoryItem.PK.Find(MasterCache.Graph, row.InventoryID);
            PXUIFieldAttribute.SetReadOnly<INTranSplit.inventoryID>(DetailCache, null, ii==null || !(ii.StkItem==false && (ii.KitItem ?? false)));
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(INRegister)];
				object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(INRegister)) ?? cache.Current;

				bool? OnHold = (bool?)cache.GetValue<INRegister.hold>(doc);

				if (OnHold == false && Math.Abs((decimal)((INTran)e.Row).BaseQty) >= 0.0000005m && (((INTran)e.Row).UnassignedQty >= 0.0000005m || ((INTran)e.Row).UnassignedQty <= -0.0000005m))
				{
					if (sender.RaiseExceptionHandling<INTran.qty>(e.Row, ((INTran)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
					{
						throw new PXRowPersistingException(typeof(INTran.qty).Name, ((INTran)e.Row).Qty, Messages.BinLotSerialNotAssigned); 
					}
				}
			}
			base.Master_RowPersisting(sender, e);
		}

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (!PXLongOperation.Exists(sender.Graph.UID))
			{
				IQtyAllocated availability = AvailabilityFetch(sender, (INTran)e.Row, ((INTran) e.Row)?.Released == true ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent);

				if (availability != null)
				{
					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INTran)e.Row).InventoryID);

					availability.QtyOnHand = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
					availability.QtyAvail = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
					availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
					availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);
					availability.QtyActual = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyActual, INPrecision.QUANTITY);

					e.ReturnValue = string.Format(
						PXMessages.LocalizeNoPrefix(Messages.Availability_ActualInfo), 
                        sender.GetValue<INTran.uOM>(e.Row), 
                        FormatQty(availability.QtyOnHand), 
                        FormatQty(availability.QtyAvail), 
						FormatQty(availability.QtyHardAvail),
						FormatQty(availability.QtyActual));
					
					AvailabilityCheck(sender, (INTran)e.Row, availability);
				}
				else
				{
					e.ReturnValue = string.Empty;
				}
			}
			else
			{
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}

		public override INTranSplit Convert(INTran item)
		{
			using (InvtMultScope<INTran> ms = new InvtMultScope<INTran>(item))
			{
				INTranSplit ret = item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}

		public virtual void INTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				if (((INTranSplit)e.Row).BaseQty != 0m && ((INTranSplit)e.Row).LocationID == null)
				{
					ThrowFieldIsEmpty<INTranSplit.locationID>(sender, e.Row);
				}
			}
		}

		public virtual void INTranSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INTran)];
			if (cache.Current != null && (e.Row == null || ((INTran)cache.Current).LineNbr == ((INTranSplit)e.Row).LineNbr))
			{
				e.NewValue = ((INTran)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void INTranSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INTran)];
			if (cache.Current != null && (e.Row == null || ((INTran)cache.Current).LineNbr == ((INTranSplit)e.Row).LineNbr))
			{
				e.NewValue = ((INTran)cache.Current).LocationID;
				e.Cancel = true;
			}
		}

		public virtual void INTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INTran)];
			if (cache.Current != null && (e.Row == null || ((INTran)cache.Current).LineNbr == ((INTranSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<INTran> ms = new InvtMultScope<INTran>((INTran)cache.Current))
				{
					e.NewValue = ((INTran)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		public virtual void INTranSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INTranSplit)e.Row).InventoryID);

			if (item != null)
			{
				object InvtMult = ((INTranSplit)e.Row).InvtMult;
				if (InvtMult == null)
				{
					sender.RaiseFieldDefaulting<INTranSplit.invtMult>(e.Row, out InvtMult);
				}

				object TranType = ((INTranSplit)e.Row).TranType;
				if (TranType == null)
				{
					sender.RaiseFieldDefaulting<INTranSplit.tranType>(e.Row, out TranType);
				}

				INLotSerTrack.Mode mode = GetTranTrackMode((ILSMaster)e.Row, item);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender, item);
					foreach (INTranSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<INTranSplit>(sender, item, lotSerNum, mode, 1m))
					{
						e.NewValue = lssplit.LotSerialNbr;
						e.Cancel = true;
					}
				}
				//otherwise default via attribute
			}
		}

		public override void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, PXErrorLevel.Error, (int)0);
			}
		}

		protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
            var row = (INTran)e.Row;
            if (row.InvtMult != (short)0)			{
				base.Master_RowInserted(sender, e);
			}
			else
			{
                //this piece of code supposed to support dropships and landed costs for dropships. ReceiptCostAdjustment is generated for landedcosts and ppv adjustments, so we need actual lotSerialNbr, thats why it has to stay
                if (row.TranType != INTranType.ReceiptCostAdjustment)
                {
				sender.SetValue<INTran.lotSerialNbr>(e.Row, null);
				sender.SetValue<INTran.expireDate>(e.Row, null);
			}
		}
		}

		protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (((INTran)e.Row).InvtMult != (short)0)
			{
				base.Master_RowDeleted(sender, e);
			}
		}

		protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (((INTran)e.Row).InvtMult != (short)0)
			{
				if (Equals(((INTran)e.Row).TranType, ((INTran)e.OldRow).TranType) == false)
				{
					sender.SetDefaultExt<INTran.invtMult>(e.Row);

					PXCache cache = sender.Graph.Caches[typeof(INTranSplit)];
					foreach (INTranSplit split in PXParentAttribute.SelectSiblings(cache, (INTranSplit)(INTran)e.Row, typeof(INTran)))
					{
						INTranSplit copy = PXCache<INTranSplit>.CreateCopy(split);

						split.TranType = ((INTran)e.Row).TranType;

						cache.MarkUpdated(split);
						cache.RaiseRowUpdated(split, copy);
					}
				}

				base.Master_RowUpdated(sender, e);
			}
			else
			{
				sender.SetValue<INTran.lotSerialNbr>(e.Row, null);
				sender.SetValue<INTran.expireDate>(e.Row, null);
			}
		}

		protected override IEnumerable<PXExceptionInfo> GetAvailabilityCheckErrors(PXCache sender, ILSMaster Row, IQtyAllocated availability)
		{
			foreach (var errorInfo in base.GetAvailabilityCheckErrors(sender, Row, availability))
				yield return errorInfo;
			if (Row.InvtMult == -1 && Row.BaseQty > 0m && availability != null)
			{
				INRegister doc = (INRegister)sender.Graph.Caches[typeof(INRegister)].Current;
				if (availability.QtyOnHand - Row.Qty < 0m && doc != null && doc.Released == false)
				{
					if (availability is LotSerialStatus)
						yield return new PXExceptionInfo(PXErrorLevel.RowWarning, Messages.StatusCheck_QtyLotSerialOnHandNegative);
					else if (availability is LocationStatus)
						yield return new PXExceptionInfo(PXErrorLevel.RowWarning, Messages.StatusCheck_QtyLocationOnHandNegative);
					else if (availability is SiteStatus)
						yield return new PXExceptionInfo(PXErrorLevel.RowWarning, Messages.StatusCheck_QtyOnHandNegative);
				}
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
		{
			object[] arguments;
			if (row is INTran)
			{
				arguments = new object[]
		{
					sender.GetStateExt<INTran.inventoryID>(row),
					sender.GetStateExt<INTran.subItemID>(row),
					sender.GetStateExt<INTran.siteID>(row),
					sender.GetStateExt<INTran.locationID>(row),
					sender.GetValue<INTran.lotSerialNbr>(row)
				};
				sender.RaiseExceptionHandling<INTran.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, errorLevel: ei.ErrorLevel ?? PXErrorLevel.Warning, args: arguments));
			}
			else
			{
				arguments = new object[]
				{
					sender.GetStateExt<INTranSplit.inventoryID>(row),
					sender.GetStateExt<INTranSplit.subItemID>(row),
					sender.GetStateExt<INTranSplit.siteID>(row),
					sender.GetStateExt<INTranSplit.locationID>(row),
					sender.GetValue<INTranSplit.lotSerialNbr>(row)
				};
				sender.RaiseExceptionHandling<INTranSplit.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, errorLevel: ei.ErrorLevel ?? PXErrorLevel.Warning, args: arguments));
			}
		}
		public override void DefaultLotSerialNbr(PXCache sender, INTranSplit row)
		{
			if (row.DocType == INDocType.Receipt && row.TranType == INTranType.Transfer
				|| (!string.IsNullOrEmpty(row.OrigModule) && row.OrigModule != BatchModule.IN))
				row.AssignedNbr = null;
			else
				base.DefaultLotSerialNbr(sender, row);
		}
		#endregion
        protected override void SetEditMode()
        {
            if (!Initialized || PrevCorrectionMode != CorrectionMode || PrevFullMode != FullMode)
            {
                PXUIFieldAttribute.SetEnabled(MasterCache, null, false);
                PXUIFieldAttribute.SetEnabled(DetailCache, null, false);

                if (PrevCorrectionMode = CorrectionMode)
                {
                    PXUIFieldAttribute.SetEnabled(MasterCache, null, nameof(INTran.LocationID), true);
                    PXUIFieldAttribute.SetEnabled(MasterCache, null, nameof(INTran.LotSerialNbr), true);
                    PXUIFieldAttribute.SetEnabled(MasterCache, null, nameof(INTran.ExpireDate), true);
                    PXUIFieldAttribute.SetEnabled(MasterCache, null, nameof(INTran.ReasonCode), true);

                    PXUIFieldAttribute.SetEnabled(DetailCache, null, nameof(INTranSplit.SubItemID), true);
                    PXUIFieldAttribute.SetEnabled(DetailCache, null, nameof(INTranSplit.Qty), true);
                    PXUIFieldAttribute.SetEnabled(DetailCache, null, nameof(INTranSplit.LocationID), true);
                    PXUIFieldAttribute.SetEnabled(DetailCache, null, nameof(INTranSplit.LotSerialNbr), true);
                    PXUIFieldAttribute.SetEnabled(DetailCache, null, nameof(INTranSplit.ExpireDate), true);
                }

                if (PrevFullMode = FullMode)
                {
                    PXUIFieldAttribute.SetEnabled(MasterCache, null, true);
					PXUIFieldAttribute.SetEnabled(MasterCache, null, nameof(INTran.POReceiptNbr), false);
					PXUIFieldAttribute.SetEnabled(MasterCache, null, nameof(INTran.POReceiptType), false);
					PXUIFieldAttribute.SetEnabled(MasterCache, null, nameof(INTran.SOOrderNbr), false);
					PXUIFieldAttribute.SetEnabled(MasterCache, null, nameof(INTran.SOShipmentNbr), false);

                    PXUIFieldAttribute.SetEnabled(DetailCache, null, true);
                    PXUIFieldAttribute.SetEnabled(DetailCache, null, nameof(INTranSplit.UOM), false);
					PXUIFieldAttribute.SetEnabled(DetailCache, null, nameof(INTranSplit.UnitCost), false);
                }

                Initialized = true;
            }
        }

		protected override void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatus> cmd, INTran Row, INLotSerClass lotSerClass)
		{
			if (Row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (Row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				switch (Row.TranType)
				{
					case INTranType.Issue:
						cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<boolTrue>>>();
						break;
					case INTranType.Transfer:
						cmd.WhereAnd<Where<INLocation.transfersValid, Equal<boolTrue>>>();
						break;
					default:
						cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
						break;
				}
			}

			if (lotSerClass.IsManualAssignRequired == true)
			{
				if (string.IsNullOrEmpty(Row.LotSerialNbr))
						cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
				else
					cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			}
		}
	}

	#endregion

	#region LSINAdjustmentTran
	public class LSINAdjustmentTran : LSINTran
	{
		public LSINAdjustmentTran(PXGraph graph)
			: base(graph)
		{
			graph.FieldVerifying.AddHandler<INTran.uOM>(INTran_UOM_FieldVerifying);
		}

		public virtual void INTran_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INTran)e.Row).InventoryID);
			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((INTran)e.Row).TranType, ((INTran)e.Row).InvtMult))
			{
				object newval;

				sender.RaiseFieldDefaulting<INTran.uOM>(e.Row, out newval);

				if (object.Equals(newval, e.NewValue) == false)
				{
					e.NewValue = newval;
					sender.RaiseExceptionHandling<INTran.uOM>(e.Row, null, new PXSetPropertyException(Messages.SerialItemAdjustment_UOMUpdated, PXErrorLevel.Warning, newval));
				}
			}
		}

		public override void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INTran)e.Row).InventoryID);

			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((INTran)e.Row).TranType, ((INTran)e.Row).InvtMult))
			{
				if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m && (decimal)e.NewValue != -1m)
				{
					e.NewValue = (decimal)e.NewValue > 0 ? 1m : -1m;
					sender.RaiseExceptionHandling<INTran.qty>(e.Row, null, new PXSetPropertyException(Messages.SerialItemAdjustment_LineQtyUpdated, PXErrorLevel.Warning, ((InventoryItem)item).BaseUnit));
				}
			}
		}

		public override void CreateNumbers(PXCache sender, INTran Row, decimal BaseQty, bool AlwaysAutoNextNbr)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			INLotSerClass itemclass = item;

			if(itemclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				 itemclass.LotSerAssign == INLotSerAssign.WhenReceived &&
				 (Row.SubItemID == null || Row.LocationID == null))
				return;

			base.CreateNumbers(sender, Row, BaseQty, AlwaysAutoNextNbr);
		}
		public override void IssueNumbers(PXCache sender, INTran Row, decimal BaseQty)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			INLotSerClass itemclass = item;

			if (itemclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				 itemclass.LotSerAssign == INLotSerAssign.WhenReceived &&
				 (Row.LotSerialNbr == null || Row.SubItemID == null || Row.LocationID == null))
				return;

			base.IssueNumbers(sender, Row, BaseQty);
		}
	}

	#endregion

	#region INExpireDateAttribute

	[PXDBDate(InputMask = "d", DisplayMask = "d")]
	[PXUIField(DisplayName = "Expiration Date", FieldClass="LotSerial")]
	[PXDefault()]
	public class INExpireDateAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber
	{
		protected Type _InventoryType;

		public INExpireDateAttribute(Type InventoryType)
			:base()
		{
			_InventoryType = InventoryType;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var itemType = sender.GetItemType();
			if (!typeof(ILSMaster).IsAssignableFrom(itemType))
			{
				throw new PXArgumentException(nameof(itemType), Messages.TypeMustImplementInterface, sender.GetItemType().GetLongName(), typeof(ILSMaster).GetLongName());
			}
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXCache.TryDispose(sender.GetAttributes(e.Row, _FieldName));
			if (PXGraph.ProxyIsActive)
			{
				sender.SetAltered(_FieldName, true);
			}
		}

		protected virtual PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
		{
			InventoryItem item = InventoryItem.PK.Find(sender.Graph, InventoryID);

			if (item != null)
			{
				INLotSerClass lsclass = INLotSerClass.PK.Find(sender.Graph, item.LotSerClassID);

				return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
			}

			return null;
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				((PXUIFieldAttribute) _Attributes[_UIAttrIndex]).Enabled = sender.AllowUpdate && IsTrackExpiration(sender, (ILSMaster) e.Row);
			}
		}

		protected virtual bool IsTrackExpiration(PXCache sender, ILSMaster row)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

			if (item == null) return false;

			return INLotSerialNbrAttribute.IsTrackExpiration(item, row.TranType, row.InvtMult);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null) return;

			if (IsTrackExpiration(sender, (ILSMaster)e.Row) && ((ILSMaster)e.Row).BaseQty != 0m)
			{							
				((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			((IPXFieldDefaultingSubscriber)_Attributes[_DefAttrIndex]).FieldDefaulting(sender, e);
		}
	}

	#endregion

	#region PXEmptyAutoIncValueException

	public class PXEmptyAutoIncValueException : PXException
	{
		public PXEmptyAutoIncValueException(string Source)
			: base(Messages.EmptyAutoIncValue, Source)
		{
		}

		public PXEmptyAutoIncValueException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

	}

	#endregion

	#region LotSerialNbrAttribute

	[PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
	public class LotSerialNbrAttribute : PXAggregateAttribute
	{

		protected int _DBAttrIndex = -1;

		public LotSerialNbrAttribute()
		{
			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{
				if (attr is PXDBFieldAttribute)
				{
					_DBAttrIndex = _Attributes.IndexOf(attr);
					break;
				}
			}
		}

		public virtual bool IsKey
		{
			get
			{
				return ((PXDBStringAttribute)_Attributes[_DBAttrIndex]).IsKey;
			}
			set
			{
				((PXDBStringAttribute)_Attributes[_DBAttrIndex]).IsKey = value;
			}
		}

	}

	#endregion

	#region INLotSerialNbrAttribute

	[PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
	[PXDefault("")]
	public class INLotSerialNbrAttribute : AcctSubAttribute, IPXFieldVerifyingSubscriber, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber
	{
		private const string _NumFormatStr = "{0}";

		protected Type _InventoryType;
		protected Type _SubItemType;
		protected Type _LocationType;

		public virtual bool ForceDisable
		{
			get;
			set;
		}

		public INLotSerialNbrAttribute(){}

		public INLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType)
            : base()
        {
			var itemType = BqlCommand.GetItemType(InventoryType);
			if (!typeof(ILSMaster).IsAssignableFrom(itemType))
            {
				throw new PXArgumentException(nameof(itemType), Messages.TypeMustImplementInterface, itemType.GetLongName(), typeof(ILSMaster).GetLongName());
            }

            _InventoryType = InventoryType;
            _SubItemType = SubItemType;
            _LocationType = LocationType;

            Type SearchType = BqlCommand.Compose(
                typeof(Search<,>),
                typeof(INLotSerialStatus.lotSerialNbr),
                typeof(Where<,,>),
                typeof(INLotSerialStatus.inventoryID),
                typeof(Equal<>),
                typeof(Optional<>),
                InventoryType,
                typeof(And<,,>),
                typeof(INLotSerialStatus.subItemID),
                typeof(Equal<>),
                typeof(Optional<>),
                SubItemType,
                typeof(And<,,>),
                typeof(INLotSerialStatus.locationID),
                typeof(Equal<>),
                typeof(Optional<>),
                LocationType,
                typeof(And<,>),
                typeof(INLotSerialStatus.qtyOnHand),
                typeof(Greater<>),
                typeof(decimal0)
                );

            {
                PXSelectorAttribute attr = new PXSelectorAttribute(SearchType,
                                                                     typeof(INLotSerialStatus.lotSerialNbr),
                                                                     typeof(INLotSerialStatus.siteID),
                                                                     typeof(INLotSerialStatus.locationID),
                                                                     typeof(INLotSerialStatus.qtyOnHand),
                                                                     typeof(INLotSerialStatus.qtyAvail),
                                                                     typeof(INLotSerialStatus.expireDate));
                _Attributes.Add(attr);
                _SelAttrIndex = _Attributes.Count - 1;
            }
        }

        public INLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType)
            : this(InventoryType, SubItemType, LocationType)
        {
            _Attributes[_DefAttrIndex] = new PXDefaultAttribute(ParentLotSerialNbrType) { PersistingCheck = PXPersistingCheck.NullOrBlank };
        }

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) || typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
            {
                subscribers.Add(this as ISubscriber);
            }
            else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
            {
                base.GetSubscriber<ISubscriber>(subscribers);

                subscribers.Remove(this as ISubscriber);
                subscribers.Add(this as ISubscriber);
                subscribers.Reverse();
            }
            else
            {
                base.GetSubscriber<ISubscriber>(subscribers);
            }
        }

        public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
            if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null)
            {
                return;
            }

			if (((INLotSerClass)item).LotSerTrack == INLotSerTrack.NotNumbered && !string.IsNullOrEmpty((string)e.NewValue))
			{
				RaiseFieldIsDisabledException();
			}

			decimal QtyValue = ((ILSMaster)e.Row).Qty.GetValueOrDefault();
			object pendingValue = sender.GetValuePending(e.Row, nameof(ILSMaster.Qty));
			if ( pendingValue != null && pendingValue  != PXCache.NotSetValue)
			{
				string strValue = pendingValue.ToString();//pending value can be both either decimal or it's string representation.
				decimal.TryParse(strValue, out QtyValue);
			}
			
			bool decreasingStock = ((ILSMaster)e.Row).InvtMult * QtyValue < 0;
			if (((INLotSerClass)item).LotSerTrack != INLotSerTrack.NotNumbered && decreasingStock && ((INLotSerClass)item).LotSerAssign == INLotSerAssign.WhenReceived && string.IsNullOrEmpty((string)e.NewValue) == false)
            {
                ((IPXFieldVerifyingSubscriber)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
            }
        }

		protected virtual void RaiseFieldIsDisabledException()
		{
			throw new PXSetPropertyException(ErrorMessages.GIFieldIsDisabled, FieldName);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
				if (item == null)
				{
					return;
				}

				if (((INLotSerClass)item).LotSerTrack != INLotSerTrack.SerialNumbered)
				{
					((IPXFieldDefaultingSubscriber)_Attributes[_DefAttrIndex]).FieldDefaulting(sender, e);
				}
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null)
			{
				return;
			}

			if (IsTracked((ILSMaster)e.Row, item, ((ILSMaster)e.Row).TranType, ((ILSMaster)e.Row).InvtMult) && ((ILSMaster)e.Row).Qty != 0)
			{
				((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
			}
		}

		protected virtual PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
		{
			InventoryItem item = InventoryItem.PK.Find(sender.Graph, InventoryID);

			if (item != null)
			{
				INLotSerClass lsclass = INLotSerClass.PK.Find(sender.Graph, item.LotSerClassID);

				return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
			}

			return null;
		}

		protected virtual bool IsTracked(ILSMaster row, INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			return INLotSerialNbrAttribute.IsTrack(lotSerClass, tranType, invMult);
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXCache.TryDispose(sender.GetAttributes(e.Row, _FieldName));
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var master = (ILSMaster)e.Row;
			if (master != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, master.InventoryID);
				((PXUIFieldAttribute) _Attributes[_UIAttrIndex]).Enabled =
					ForceDisable != true &&
					item != null && sender.AllowUpdate &&
					IsTracked(master, item, master.TranType, master.InvtMult);
			}
		}

		public static string MakeFormatStr(PXCache sender, INLotSerClass lsclass)
		{
			StringBuilder format = new StringBuilder();

			if (lsclass != null)
			{
				foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, 
					Where<INLotSerSegment.lotSerClassID, Equal<Required<INLotSerSegment.lotSerClassID>>>, 
					OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.Select(sender.Graph, lsclass.LotSerClassID))
				{
					switch (seg.SegmentType)
					{
						case INLotSerSegmentType.FixedConst:
							format.Append(seg.SegmentValue);
							break;
						case INLotSerSegmentType.NumericVal:
							format.Append("{1}");
							break;
						case INLotSerSegmentType.DateConst:
							format.Append("{0");
							if (!string.IsNullOrEmpty(seg.SegmentValue))
								format.Append(":").Append(seg.SegmentValue);
							format.Append("}");
							break;
						case INLotSerSegmentType.DayConst:
							format.Append("{0:dd}");
							break;
						case INLotSerSegmentType.MonthConst:
							format.Append("{0:MM}");
							break;
						case INLotSerSegmentType.MonthLongConst:
							format.Append("{0:MMM}");
							break;
						case INLotSerSegmentType.YearConst:
							format.Append("{0:yy}");
							break;
						case INLotSerSegmentType.YearLongConst:
							format.Append("{0:yyyy}");
							break;
						default:
							throw new PXException();
					}
				}
			}
			return format.ToString();
		}

		/// <summary>
		/// Read ILotSerNumVal implemented object which store auto-incremental value
		/// </summary>
		/// <param name="graph">graph</param>
		/// <param name="item">settings</param>
		/// <returns></returns>
		public static ILotSerNumVal ReadLotSerNumVal(PXGraph graph, PXResult<InventoryItem, INLotSerClass> item)
		{
			if (item == null || (INLotSerClass)item == null)
				return null;
			if (((INLotSerClass)item).LotSerNumShared == true)
				return INLotSerClassLotSerNumVal.PK.FindDirty(graph, ((INLotSerClass)item).LotSerClassID);
			return InventoryItemLotSerNumVal.PK.FindDirty(graph, ((InventoryItem)item).InventoryID);
		}

		public static string AssignNumber<LSTable, LSKey>(PXCache sender, string LSKeyValue)
			where LSTable : class, IBqlTable, ILotSerNumVal
			where LSKey : class, IBqlField
		{
			string LSNumVal = "";
			string LSNewVal = "";

			using (PXDataRecord record = PXDatabase.SelectSingle<LSTable>(
				new PXDataField("LotSerNumVal"),
				new PXDataFieldValue(typeof(LSKey).Name, PXDbType.VarChar, 255, LSKeyValue)
				))
			{
				if (record == null)
				{
					throw new AutoNumberException();
				}
				LSNumVal = record.GetString(0);
			}

			try
			{
				LSNewVal = AutoNumberAttribute.NextNumber(LSNumVal);
			}
			catch (NullReferenceException)
			{
				throw new AutoNumberException();
			}

			PXDatabase.Update<LSTable>(
				new PXDataFieldAssign("LotSerNumVal", LSNewVal),
				new PXDataFieldRestrict(typeof(LSKey).Name, LSKeyValue),
				new PXDataFieldRestrict("LotSerNumVal", LSNumVal));

			return LSNewVal;
		}

		/// <summary>
		/// Return the length of auto-incremental number
		/// </summary>
		/// <param name="lotSerNum">auto-incremental number value</param>
		/// <returns></returns>
		public static int GetNumberLength(ILotSerNumVal lotSerNum)
			=>  lotSerNum == null || string.IsNullOrEmpty(lotSerNum.LotSerNumVal) ? 6 : lotSerNum.LotSerNumVal.Length;

		/// <summary>
		/// Return default(empty) auto-incremental number value
		/// </summary>
		/// <param name="sender">cache</param>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <returns></returns>
		public static string GetNextNumber(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum)
		{
			string numval = new string('0', GetNumberLength(lotSerNum));
			return string.Format(lsClass.LotSerFormatStr, sender.Graph.Accessinfo.BusinessDate, numval).ToUpper();
		}

		/// <summary>
		/// Return  auto-incremental number format
		/// </summary>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <returns></returns>
		public static string GetNextFormat(INLotSerClass lsClass, ILotSerNumVal lotSerNum)
		{
			string numFormat = "{1:" + new string('0', GetNumberLength(lotSerNum)) + "}";
			return lsClass.LotSerFormatStr.Replace("{1}", numFormat);
		}

		/// <summary>
		/// Return shared Lot\Ser Class identifier
		/// </summary>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <returns></returns>
		public static string GetNextClassID(INLotSerClass lsClass)
		{
			return (bool)lsClass.LotSerNumShared
							? lsClass.LotSerClassID
							: null;
		}

		protected class PXUnknownSegmentTypeException: PXException
		{
			public PXUnknownSegmentTypeException():base(Messages.UnknownSegmentType){}

			public PXUnknownSegmentTypeException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		/// <summary>
		/// Return auto-incremental number mask
		/// </summary>
		/// <param name="sender">cache</param>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <returns></returns>
		public static string GetDisplayMask(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum)
		{
			if (lsClass == null)
				return null;
			StringBuilder mask = new StringBuilder();
			foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, 
				Where<INLotSerSegment.lotSerClassID, Equal<Required<INLotSerSegment.lotSerClassID>>>, 
				OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.Select(sender.Graph, lsClass.LotSerClassID))
			{
				switch (seg.SegmentType)
				{
					case INLotSerSegmentType.FixedConst:
						mask.Append(!string.IsNullOrEmpty(seg.SegmentValue) ? new string('C', seg.SegmentValue.Length) : string.Empty);
						break;
					case INLotSerSegmentType.NumericVal:
						mask.Append( new string('9', GetNumberLength(lotSerNum)));
						break;
					case INLotSerSegmentType.DateConst:
						mask.Append(!string.IsNullOrEmpty(seg.SegmentValue)
						            	? new string('C', seg.SegmentValue.Length)
						            	: new string('C', string.Format("{0}", sender.Graph.Accessinfo.BusinessDate).Length));
						break;
					case INLotSerSegmentType.DayConst:
					case INLotSerSegmentType.MonthConst:
					case INLotSerSegmentType.YearConst:
						mask.Append(new string('9', 2));
						break;
					case INLotSerSegmentType.MonthLongConst:
						mask.Append(new string('C', 3));
						break;
					case INLotSerSegmentType.YearLongConst:
						mask.Append(new string('9', 4));
						break;
					default:
						throw new PXUnknownSegmentTypeException();
				}
			} 
			return mask.ToString();
		}

		public class LSParts
		{
			public LSParts(int flen, int nlen, int llen)
			{
				_flen = flen;
				_nlen = nlen;
				_llen = llen;
			}

			private readonly int _flen;
			private readonly int _nlen;
			private readonly int _llen;

			public int flen
			{
				get { return _flen; }
			}

			public int nlen
			{
				get { return _nlen; }
			}
			
			public int llen
			{
				get { return _llen; }
			}
			
			public int len
			{
				get { return _flen + _nlen + _llen; }
			}

			public int nidx
			{
				get { return _flen; }
			}

			public int lidx
			{
				get { return _flen + _nlen; }
			}
		}

		/// <summary>
		/// Get auto-incremantal number parts settings
		/// </summary>
		/// <param name="sender">cache</param>
		/// <param name="lsclass">Lot/Ser class</param>
		/// <param name="lotSerNum">auto-incremantal number value</param>
		/// <returns></returns>
		public static LSParts GetLSParts(PXCache sender, INLotSerClass lsclass, ILotSerNumVal lotSerNum)
		{
			if (lsclass == null)
				return null;
			int flen = 0, nlen = 0, llen = 0;
			foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, 
				Where<INLotSerSegment.lotSerClassID, Equal<Required<INLotSerSegment.lotSerClassID>>>, 
				OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.Select(sender.Graph, lsclass.LotSerClassID))
			{
				int tmp = 0;
				switch (seg.SegmentType)
				{
					case INLotSerSegmentType.FixedConst:
						tmp = seg.SegmentValue.Length;
						break;
					case INLotSerSegmentType.NumericVal:
						nlen = GetNumberLength(lotSerNum);
						break;
					case INLotSerSegmentType.DateConst:
						tmp = !string.IsNullOrEmpty(seg.SegmentValue)
										? seg.SegmentValue.Length
										: string.Format("{0}", sender.Graph.Accessinfo.BusinessDate).Length;
						break;
					case INLotSerSegmentType.DayConst:
					case INLotSerSegmentType.MonthConst:
					case INLotSerSegmentType.YearConst:
						tmp = 2;
						break;
					case INLotSerSegmentType.MonthLongConst:
						tmp = 3;
						break;
					case INLotSerSegmentType.YearLongConst:
						tmp = 4;
						break;
					default:
						throw new PXUnknownSegmentTypeException();
				}
				if (nlen == 0)
					flen += tmp;
				else
					llen += tmp;
			}
			return new LSParts(flen, nlen, llen);
		}

		public static string GetNextNumber(PXCache sender, int inventoryID)
		{
			var inventory = InventoryItem.PK.FindDirty(sender.Graph, inventoryID);
			if (inventory == null || inventory.LotSerClassID == null)
				return null;
			var lsClass = INLotSerClass.PK.Find(sender.Graph, inventory.LotSerClassID);
			if (lsClass == null)
				return null;
			
            if (lsClass.LotSerTrack == null || lsClass.LotSerTrack == INLotSerTrack.NotNumbered)
				return null;

			ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender.Graph, new PXResult<InventoryItem, INLotSerClass>(inventory, lsClass));
			string number = GetNextNumber(sender, lsClass, lotSerNum);
			string format = GetNextFormat(lsClass, lotSerNum);

			string newval = GetNextClassID(lsClass) != null
			                	? AssignNumber<INLotSerClassLotSerNumVal, INLotSerClassLotSerNumVal.lotSerClassID>(sender, inventory.LotSerClassID)
			                	: AssignNumber<InventoryItemLotSerNumVal, InventoryItemLotSerNumVal.inventoryID>(sender, inventory.InventoryID.ToString());

			return UpdateNumber(format, number, newval);
		}

		/// <summary>
		/// Return the new child(detail) object with filled properties for further generation of lot/ser number
		/// </summary>
		/// <typeparam name="TLSDetail"></typeparam>
		/// <param name="sender"></param>
		/// <param name="lsClass"></param>
		/// <param name="lotSerNum"></param>
		/// <returns></returns>
		public static TLSDetail GetNextSplit<TLSDetail>(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum)
			where TLSDetail : class, ILSDetail, new()
		{
			TLSDetail det = new TLSDetail();
			det.LotSerialNbr = GetNextNumber(sender, lsClass, lotSerNum);
			det.AssignedNbr = GetNextFormat(lsClass, lotSerNum);
			det.LotSerClassID = GetNextClassID(lsClass);
			return det;
		}

		public static string MakeNumber(string FormatStr, string NumberStr, DateTime date)
		{
			if (FormatStr.Contains(_NumFormatStr))
			{
				string numval = new string('0', NumberStr.Length - FormatStr.Length + _NumFormatStr.Length);
				return string.Format(FormatStr, date, numval).ToUpper();
			}
			else
				return string.Format(FormatStr, date, 0).ToUpper();
		}

		public static bool StringsEqual(string FormatStr, string NumberStr)
		{
			int numIndex = 0;
			for (int i = 0; i < FormatStr.Length; i++)
			{
				if (FormatStr[i] == '{' && i + 5 <= FormatStr.Length && FormatStr[i + 2] == ':')
				{
					int lenIndex = FormatStr.IndexOf("}", i + 3);
					if (lenIndex != -1)
					{
						int lenght = lenIndex - i - 3;
						if (FormatStr[i + 1] == '1')
						{
							if (NumberStr.Length < numIndex + lenght)
								return false;

							for (int n = 0; n < lenght; n++)
								if (NumberStr[numIndex + n] != '0')
									return false;
						}
						numIndex += lenght;
						i = lenIndex;

						if (i >= FormatStr.Length - 1 && numIndex < NumberStr.Length)
							return false;

						continue;
					}
				}

                if (NumberStr == null || NumberStr.Length <= numIndex) return false;
				if (char.ToUpper(FormatStr[i]) != char.ToUpper(NumberStr[numIndex++])) return false;
			}
			return true;
		}

		public static string UpdateNumber(string FormatStr, string NumberStr, string number)
		{
			int numIndex = 0;
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < FormatStr.Length; i++)
			{
				if (FormatStr[i] == '{' && i + 5 <= FormatStr.Length && FormatStr[i + 2] == ':')
				{
					int lenIndex = FormatStr.IndexOf("}", i + 3);
					if (lenIndex != -1)
					{
						int lenght = lenIndex - i - 3;
						if (FormatStr[i + 1] == '1')
							result.Append(number);
						else
							result.Append(NumberStr.Substring(numIndex, lenght));
						numIndex += lenght;
						i = lenIndex;

						continue;
					}
				}
				if (NumberStr.Length <= numIndex) break;
				result.Append(NumberStr[numIndex++]);
			}
			return result.ToString().ToUpper();
		}

		/// <summary>
		/// Return child(detail) objects list with filled properties for further generation of lot/ser number
		/// </summary>
		/// <typeparam name="TLSDetail">child(detail) entity type</typeparam>
		/// <param name="sender">cache</param>
		/// <param name="item">settings</param>
		/// <param name="mode">Track mode</param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static List<TLSDetail> CreateNumbers<TLSDetail>(PXCache sender, PXResult<InventoryItem, INLotSerClass> item, INLotSerTrack.Mode mode, decimal count)
			where TLSDetail : class, ILSDetail, new()
		{
			return CreateNumbers<TLSDetail>(sender, item, ReadLotSerNumVal(sender.Graph, item), mode, false, count);
		}

		/// <summary>
		/// Return child(detail) objects list with filled properties for further generation of lot/ser number
		/// </summary>
		/// <typeparam name="TLSDetail">child(detail) entity type</typeparam>
		/// <param name="sender">cache</param>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <param name="mode">Track mode</param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static List<TLSDetail> CreateNumbers<TLSDetail>(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum, INLotSerTrack.Mode mode, decimal count)
			where TLSDetail : class, ILSDetail, new()
		{
			return CreateNumbers<TLSDetail>(sender, lsClass, lotSerNum, mode, false, count);
		}

		/// <summary>
		/// Return child(detail) objects list with filled properties for further generation of lot/ser number
		/// </summary>
		/// <typeparam name="TLSDetail">child(detail) entity type</typeparam>
		/// <param name="sender">cache</param>
		/// <param name="lsClass">Lot/Ser class</param>
		/// <param name="lotSerNum">Auto-incremental number value</param>
		/// <param name="mode">Track mode</param>
		/// <param name="ForceAutoNextNbr"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static List<TLSDetail> CreateNumbers<TLSDetail>(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum, INLotSerTrack.Mode mode, bool ForceAutoNextNbr, decimal count)
			where TLSDetail : class, ILSDetail, new()
		{
			List<TLSDetail> ret = new List<TLSDetail>();

			if (lsClass != null)
			{
				string LotSerTrack = (mode & INLotSerTrack.Mode.Create) > 0 ? lsClass.LotSerTrack : INLotSerTrack.NotNumbered;
				bool AutoNextNbr = (mode & INLotSerTrack.Mode.Manual) == 0 && (lsClass.AutoNextNbr == true || ForceAutoNextNbr);

				switch (LotSerTrack)
				{
					case "N":
						TLSDetail detail = new TLSDetail();
						detail.AssignedNbr = string.Empty;
						detail.LotSerialNbr = string.Empty;
						detail.LotSerClassID = string.Empty;

						ret.Add(detail);
						break;
					case "L":
						if (AutoNextNbr)
						{
							ret.Add(GetNextSplit<TLSDetail>(sender, lsClass, lotSerNum));
						}
						break;
					case "S":
						if (AutoNextNbr)
						{
							for (int i = 0; i < (int)count; i++)
							{
								ret.Add(GetNextSplit<TLSDetail>(sender, lsClass, lotSerNum));
							}
						}
						break;
				}
			}
			return ret;
		}

		public static INLotSerTrack.Mode TranTrackMode(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			if (lotSerClass == null || lotSerClass.LotSerTrack == null || lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered) return INLotSerTrack.Mode.None;

			switch (tranType)
			{
				case INTranType.Invoice:
				case INTranType.DebitMemo:
				case INTranType.Issue:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.Create
						: invMult == 1 ? INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual : INLotSerTrack.Mode.Issue;

				case INTranType.Transfer:
				case INTranType.Disassembly:
					return
						lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None
							: invMult ==  1 ? INLotSerTrack.Mode.Create
							: invMult == -1 ? INLotSerTrack.Mode.Issue
							: INLotSerTrack.Mode.Manual;
				case INTranType.Assembly:
					if (invMult == -1)//component 
					{
						return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.Create : INLotSerTrack.Mode.Issue;
					}
					else if (invMult == 1) //kit
					{
						return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None : INLotSerTrack.Mode.Create;
					}
					else
					{
						return INLotSerTrack.Mode.Manual;
					}
				case INTranType.Adjustment:
				case INTranType.StandardCostAdjustment:
				case INTranType.NegativeCostAdjustment:
				case INTranType.ReceiptCostAdjustment:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None
						: invMult ==  1 ? INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual
						: invMult == -1 ? INLotSerTrack.Mode.Issue | INLotSerTrack.Mode.Manual
						: INLotSerTrack.Mode.Manual;;					

				case INTranType.Receipt:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None : INLotSerTrack.Mode.Create;

				case INTranType.Return:
				case INTranType.CreditMemo:
					return INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual;

				default:
					return INLotSerTrack.Mode.None;
			}
		}
		public static bool IsTrack(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			return TranTrackMode(lotSerClass, tranType, invMult) != INLotSerTrack.Mode.None;
		}
		public static bool IsTrackExpiration(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			return lotSerClass.LotSerTrackExpiration == true && IsTrack(lotSerClass, tranType, invMult);
		}
		public static bool IsTrackSerial(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			return lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && IsTrack(lotSerClass, tranType, invMult);		
		}
	}

	#endregion

	#region INUnitAttribute

	[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
	[PXUIField(DisplayName = "UOM", Visibility = PXUIVisibility.Visible)]
	public class INUnitAttribute : AcctSub2Attribute, IPXFieldVerifyingSubscriber, IPXRowSelectedSubscriber, IPXRowPersistingSubscriber
	{
        public enum VerifyingMode
        {
			Custom,
			UnitCatalog,
			GlobalUnitConversion,
			InventoryUnitConversion
		}
        #region Fields & Properties
        private readonly VerifyingMode _verifyingMode;

		protected Type InventoryType = null;
		protected Type BaseUnitType = null;

		private string _AccountIDField = null;
		private string _AccountRequireUnitsField = null;

		private PXSelectorAttribute _selectorWithAggregate;
		private PXSelectorAttribute _selectorNoAggregate;

		/// <summary>
		/// run verifying process if inventory was setted
		/// </summary>
		private readonly bool _verifyOnSettedInventory = true;

		public override bool DirtyRead
		{
			get { return base.DirtyRead; }
			set
			{
				if (value != base.DirtyRead && AttributeLevel == PXAttributeLevel.Type)
				{
					_Attributes[_SelAttrIndex] = value ? _selectorNoAggregate : _selectorWithAggregate;
					base.DirtyRead = value;
				}
			}
		}

		public bool VerifyOnCopyPaste { get; set; } = true;

		#endregion

		#region Constructors

		protected INUnitAttribute(VerifyingMode verifyingMode):base()
        {
            _verifyingMode = verifyingMode;
        }

		public INUnitAttribute()
			: this(VerifyingMode.UnitCatalog)
		{
            Init(typeof(Search4<INUnit.fromUnit, 
				Where<INUnit.unitType, Equal<INUnitType.global>>, 
				Aggregate<GroupBy<INUnit.fromUnit>>>));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dummy">Is dummy parameter. Was created for constructor type identification</param>
        /// <param name="BaseUnitType"></param>
		public INUnitAttribute(Type dummy, Type BaseUnitType)
			: this(VerifyingMode.GlobalUnitConversion)
		{
			this.BaseUnitType = BaseUnitType;
			Init(BqlTemplate.OfCommand<
				Search<INUnit.fromUnit,
					Where<INUnit.unitType, Equal<INUnitType.global>,
				And<INUnit.toUnit, Equal<Optional<BqlPlaceholder.A>>>>>>
				.Replace<BqlPlaceholder.A>(BaseUnitType).ToType());
		}

		public INUnitAttribute(Type InventoryType, Type AccountIDType, Type AccountRequireUnitsType)
			: this()
		{
			this.InventoryType = InventoryType;
			_AccountIDField = AccountIDType.Name;
			_AccountRequireUnitsField = AccountRequireUnitsType.Name;
            _verifyOnSettedInventory = false;
		}

		//it is assumed that only conversions to BASE item unit exist.
		public INUnitAttribute(Type InventoryType)
			: this(VerifyingMode.InventoryUnitConversion)
		{
			this.InventoryType = InventoryType;
			Init(BqlTemplate.OfCommand<
				Search<INUnit.fromUnit,
				Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
					And<INUnit.inventoryID, Equal<Optional<BqlPlaceholder.A>>,
				Or<INUnit.unitType, Equal<INUnitType.global>,
					And<Optional<BqlPlaceholder.A>, IsNull>>>>>>
				.Replace<BqlPlaceholder.A>(InventoryType).ToType());
		}
		#endregion

		private void Init(Type searchType)
			=> Init(searchType, BqlCommand.CreateInstance(searchType).AggregateNew<Aggregate<GroupBy<INUnit.fromUnit>>>().GetType());

		protected void Init(Type searchNoAggregate, Type searchWithAggregate)
		{
			_selectorNoAggregate = new PXSelectorAttribute(searchNoAggregate);
			_selectorWithAggregate = searchNoAggregate == searchWithAggregate 
				? _selectorNoAggregate
				: new PXSelectorAttribute(searchWithAggregate);
			_Attributes.Add(DirtyRead ? _selectorNoAggregate : _selectorWithAggregate);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		#region Implementation
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || string.IsNullOrEmpty(_AccountIDField) || string.IsNullOrEmpty(_AccountRequireUnitsField))
				return;

			object AccountID = sender.GetValue(e.Row, _AccountIDField);
			object AccountRequireUnits = sender.GetValue(e.Row, _AccountRequireUnitsField);

			if (AccountRequireUnits == null)
			{
                Account account = (Account)PXSelectReadonly<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, AccountID);

                if (account != null)
				{
					sender.SetValue(e.Row, _AccountRequireUnitsField, account.RequireUnits);
				}
				else
				{
					sender.SetValue(e.Row, _AccountRequireUnitsField, null);
				}
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (string.IsNullOrEmpty(_AccountRequireUnitsField))
				return;

            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

			object AccountRequireUnits = sender.GetValue(e.Row, _AccountRequireUnitsField);
			string FieldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

			if (AccountRequireUnits != null && (bool)AccountRequireUnits && string.IsNullOrEmpty(FieldValue))
			{
				var acctID = sender.GetValue(e.Row, _AccountIDField);
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, acctID);

				if (account == null)
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				else
					throw new PXRowPersistingException(_FieldName, null, Messages.UOMRequiredForAccount, _FieldName, account.AccountCD);
			}
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            if (e.Row == null || e.NewValue == null)
                return;
			if(_FieldName.IsIn(nameof(INUnit.FromUnit), nameof(INUnit.ToUnit)))
                return;
            if (!VerifyOnCopyPaste && sender.Graph.IsCopyPasteContext)
                return;
            if (!_verifyOnSettedInventory && sender.GetValue(e.Row, InventoryType.Name) != null)
                return;
            var unit = ReadUnit(sender, e.Row, (string)e.NewValue);
			UnitVerifying(sender, e, unit);
        }

		protected virtual void UnitVerifying(PXCache cache, PXFieldVerifyingEventArgs e, INUnit unit)
		{
			if (unit == null)
			{
				if (e.ExternalCall)
					throw new PXSetPropertyException(_verifyingMode == VerifyingMode.UnitCatalog ? ErrorMessages.ElementDoesntExist : ErrorMessages.ElementDoesntExistOrNoRights, _FieldName);
				throw new PXSetPropertyException(_verifyingMode == VerifyingMode.UnitCatalog ? ErrorMessages.ValueDoesntExist : ErrorMessages.ValueDoesntExistOrNoRights, _FieldName, e.NewValue);
			}
		}

		protected virtual INUnit ReadUnit(PXCache sender, object data, string fromUnitID)
		{
			INUnit unit = null;
			switch (_verifyingMode)
					{
				case VerifyingMode.UnitCatalog:
					unit = ReadGlobalUnit(sender, fromUnitID);
					break;
				case VerifyingMode.GlobalUnitConversion:
					var toUnitID = GetBaseUnit(sender, data);
					unit = INUnit.UK.ByGlobal.Find(sender.Graph, fromUnitID, toUnitID);
					break;
				case VerifyingMode.InventoryUnitConversion:
					var inventory = InventoryItem.PK.Find(sender.Graph, (int?)GetSelectorParameterValue(sender, data, InventoryType));
					if (inventory == null)
						unit = ReadGlobalUnit(sender, fromUnitID);
					else
						unit = INUnit.UK.ByInventory.Find(sender.Graph, inventory.InventoryID, fromUnitID);
					break;
				}
			return unit;
		}

		public virtual INUnit ReadConversion(PXCache cache, object data, string fromUnitID)
		{
			var conversionInfo = ReadConversionInfo(cache, data, fromUnitID);
			return conversionInfo == null ? null : conversionInfo.Conversion;
		}

		public virtual InventoryItem ReadInventoryItem(PXCache cache, object data)
		{
			if(_verifyingMode == VerifyingMode.InventoryUnitConversion)
				return InventoryItem.PK.Find(cache.Graph, (int?)GetSelectorParameterValue(cache, data, InventoryType));
			return null;
		}

		public virtual ConversionInfo ReadConversionInfo(PXCache cache, object data, string fromUnitID)
		{
			if (string.IsNullOrEmpty(fromUnitID))
				return null;
			INUnit conversion;
			InventoryItem inventory = null;
			switch (_verifyingMode)
			{
				case VerifyingMode.Custom:
					return null;
				case VerifyingMode.UnitCatalog:
					conversion = EmptyConversion(fromUnitID);
					break;
				case VerifyingMode.GlobalUnitConversion:
					var toUnitID = GetBaseUnit(cache, data);
					conversion = fromUnitID == toUnitID
						? EmptyConversion(fromUnitID)
						: INUnit.UK.ByGlobal.Find(cache.Graph, fromUnitID, toUnitID);
					break;
				case VerifyingMode.InventoryUnitConversion:
					inventory = InventoryItem.PK.Find(cache.Graph, (int?)GetSelectorParameterValue(cache, data, InventoryType));
					if (inventory == null)
						conversion = EmptyConversion(fromUnitID);
					else
					{
						if (fromUnitID == inventory.BaseUnit)
						{
							conversion = EmptyConversion(fromUnitID);
							conversion.UnitType = INUnitType.InventoryItem;
							conversion.InventoryID = inventory.InventoryID;
						}
						else
							conversion = INUnit.UK.ByInventory.Find(cache.Graph, inventory.InventoryID, fromUnitID);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(_verifyingMode));
			}
			return new ConversionInfo(conversion, inventory);
		}

		private INUnit EmptyConversion(string unit) => new INUnit
		{
			UnitType = INUnitType.Global,
			FromUnit = unit,
			ToUnit = unit,
			UnitRate = 1,
			PriceAdjustmentMultiplier = 1,
			UnitMultDiv = MultDiv.Multiply
		};

		#endregion

		protected virtual string GetBaseUnit(PXCache cache, object data)
		{
			if(BaseUnitType == null)
				return null;
			string unit = (string)GetSelectorParameterValue(cache, data, BaseUnitType);
			if(unit == null)
			{
				var itemType = BqlCommand.GetItemType(BaseUnitType);
				var itemCache = cache.Graph.Caches[itemType];
				if(itemCache.Keys.Count == 0)
				{
					var cmd = BqlTemplate.OfCommand<Select<BqlPlaceholder.A>>.Replace<BqlPlaceholder.A>(itemType).ToCommand();
					var view = cache.Graph.TypedViews.GetView(cmd, true);
					var item = view.SelectSingle();
					unit = (string)itemCache.GetValue(item, BaseUnitType.Name);
				}
			}
			return unit;
		}

		protected virtual object GetSelectorParameterValue(PXCache cache, object data, Type parameterFieldType)
		{
			var view = cache.Graph.TypedViews.GetView(SelectorAttr.GetSelect(), !SelectorAttr.DirtyRead);
			object[] values = view.PrepareParameters(new[] { data }, null);
			return values == null || values.Length == 0
				? null
				: values[0];
		}

		#region Runtime
		private static INUnit ReadGlobalUnit(PXCache sender, string fromUnitID)
		{
			return PXSelectReadonly<INUnit,
					Where<INUnit.unitType, Equal<INUnitType.global>,
						And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>.SelectWindowed(sender.Graph, 0, 1, fromUnitID);
		}
		public static decimal ConvertFromBase<InventoryIDField, UOMField>(PXCache sender, object Row, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
			where UOMField : IBqlField
		{
			if (value == 0) return 0;

			string ToUnit = (string)sender.GetValue<UOMField>(Row);
			try
			{
				return ConvertFromBase<InventoryIDField>(sender, Row, ToUnit, value, prec);
			}
			catch (PXUnitConversionException ex)
			{
				sender.RaiseExceptionHandling<UOMField>(Row, ToUnit, ex);
			}
			return 0m;
		}
		public static decimal ConvertFromBase<InventoryIDField>(PXCache sender, object Row, string ToUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			return Convert<InventoryIDField>(sender, Row, ToUnit, value, prec, true);
		}

		public static decimal ConvertToBase<InventoryIDField, UOMField>(PXCache sender, object Row, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
			where UOMField : IBqlField
		{
			if (value == 0) return 0;

			string FromUnit = (string)sender.GetValue<UOMField>(Row);
			try
			{
				return ConvertToBase<InventoryIDField>(sender, Row, FromUnit, value, prec);
			}
			catch (PXUnitConversionException ex)
			{
				sender.RaiseExceptionHandling<UOMField>(Row, FromUnit, ex);
			}
			return 0m;
		}
		public static decimal ConvertToBase<InventoryIDField>(PXCache sender, object Row, string FromUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			return Convert<InventoryIDField>(sender, Row, FromUnit, value, prec, false);
		}
		public static decimal ConvertFromTo<InventoryIDField>(PXCache sender, object Row, string FromUnit, string ToUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			if (string.Equals(FromUnit, ToUnit))
			{
				return value;
			}
			decimal baseValue = ConvertToBase<InventoryIDField>(sender, Row, FromUnit, value, prec);
			return ConvertFromBase<InventoryIDField>(sender, Row, ToUnit, baseValue, prec);
		}
		private static decimal Convert<InventoryIDField>(PXCache sender, object Row, string FromUnit, decimal value, INPrecision prec, bool ViceVersa)
			where InventoryIDField : IBqlField
		{
			if (value == 0 || FromUnit == null)
				return value;

			object InventoryID = sender.GetValue<InventoryIDField>(Row);
			if (InventoryID == null)
				return value;

			var inventory = InventoryItem.PK.Find(sender.Graph, (int?)InventoryID);
			if (inventory == null)
			{
				PXTrace.WriteError(ErrorMessages.ValueDoesntExistOrNoRights, Messages.InventoryItem, InventoryID);
				throw new PXUnitConversionException();
			}

            if (FromUnit == inventory.BaseUnit)
                return Round(value, prec);

			INUnit unit = INUnit.UK.ByInventory.Find(sender.Graph, inventory.InventoryID, FromUnit);
			if (unit == null)
				throw new PXUnitConversionException();

				return ConvertValue(value, unit, prec, ViceVersa, UsePriceAdjustmentMultiplier(sender.Graph)); 
			}

		public static decimal ConvertFromBase(PXCache sender, Int32? InventoryID, string ToUnit, decimal value, INPrecision prec)
		{
			return Convert(sender, InventoryID, ToUnit, value, prec, true);
		}

		public static decimal ConvertToBase(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, INPrecision prec)
		{
			return Convert(sender, InventoryID, FromUnit, value, prec, false);
		}

		public static decimal ConvertToBase(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, decimal? baseValue, INPrecision prec)
		{
			return Convert(sender, InventoryID, FromUnit, value, baseValue, prec, false);
		}

		private static decimal Convert(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, INPrecision prec, bool ViceVersa)
		{
			return Convert(sender, InventoryID, FromUnit, value, null, prec, ViceVersa);
		}

		private static decimal Convert(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, decimal? baseValue, INPrecision prec, bool ViceVersa)
		{
			if (value == 0 || FromUnit == null)
				return value;

            InventoryItem item = item = InventoryItem.PK.Find(sender.Graph, InventoryID);

            if(item == null)
			{
				PXTrace.WriteError(ErrorMessages.ValueDoesntExistOrNoRights, Messages.InventoryItem, InventoryID);
				throw new PXUnitConversionException();
			}

            if (FromUnit == item.BaseUnit)
				{
                if (baseValue != null)
			{
                    decimal revValue = Round((decimal)baseValue, prec);
                    if (revValue == value)
                        return (decimal)baseValue;
			}
                return Round(value, prec);
			}

			INUnit unit = INUnit.UK.ByInventory.Find(sender.Graph, item.InventoryID, FromUnit);
			if (unit == null)
				throw new PXUnitConversionException();

				bool usePriceAdjustmentMultiplier = UsePriceAdjustmentMultiplier(sender.Graph);
				if (baseValue != null)
				{
					decimal revValue = ConvertValue((decimal) baseValue, unit, prec, !ViceVersa, usePriceAdjustmentMultiplier);
					if (revValue == value)
						return (decimal) baseValue;
				}
				return ConvertValue(value, unit, prec, ViceVersa, usePriceAdjustmentMultiplier);
			}

		internal static decimal Convert(PXCache sender, INUnit unit, decimal value, INPrecision prec, bool ViceVersa)
		{
			if (value == 0) return 0;
			if (unit == null) return value;
			return ConvertValue(value, unit, prec, ViceVersa, UsePriceAdjustmentMultiplier(sender.Graph));
		}

		public static decimal ConvertFromBase(PXCache sender, INUnit unit, decimal value, INPrecision prec)
		{
			return Convert(sender, unit, value, prec, true);
		}

		public static decimal ConvertToBase(PXCache sender, INUnit unit, decimal value, INPrecision prec)
		{
			return Convert(sender, unit, value, prec, false);
		}

		public static bool IsFractional(INUnit conv)
		{
			return conv != null && (conv.UnitMultDiv == MultDiv.Divide && conv.UnitRate != 1m || decimal.Remainder((decimal)conv.UnitRate, 1m) != 0m);
		}

		/// <summary>
		/// Converts units using Global converion Table.
		/// </summary>
		/// <exception cref="PXException">Is thrown if converion is not found in the table.</exception>
		public static decimal ConvertGlobalUnits(PXGraph graph, string from, string to, decimal value, INPrecision prec)
		{
			decimal result = 0;

			if (TryConvertGlobalUnits(graph, from, to, value, prec, out result))
			{
				return result;
			}
			else
			{
				throw new PXException(Messages.ConversionNotFound, from, to);
			}

		}

		public static bool TryConvertGlobalUnits(PXGraph graph, string from, string to, decimal value, INPrecision prec, out decimal result)
		{
			if (value == 0)
			{
				result = 0;
				return true;
			}

			result = 0;
			if (from == to)
			{
				result = value;
				return true;
			}
						
			var unit = INUnit.UK.ByGlobal.Find(graph, from, to);

			if (unit == null)
				return false;
			result = ConvertValue(value, unit, prec);
			return true;
		}

		public static decimal ConvertValue(decimal value, INUnit unit, INPrecision prec, bool viceVersa = false, bool usePriceAdjustmentMultiplier = false)
		{
			if (unit.UnitMultDiv == MultDiv.Multiply && !viceVersa || unit.UnitMultDiv == MultDiv.Divide && viceVersa)
				value *= (decimal) unit.UnitRate;
			else
				value /= (decimal) unit.UnitRate;

			if (usePriceAdjustmentMultiplier && prec == INPrecision.UNITCOST)
			{
				if (viceVersa)
					value /= (decimal)unit.PriceAdjustmentMultiplier;
				else
					value *= (decimal)unit.PriceAdjustmentMultiplier;
			}

			return Round(value, prec);
		}

        private static decimal Round(decimal value, INPrecision prec)
        {
			if (prec == INPrecision.NOROUND)
				return value;

			int precision = DefinePrecision(prec);
			return Math.Round(value, precision, MidpointRounding.AwayFromZero);
		}

		private static int DefinePrecision(INPrecision prec)
		{
			int precision = 6;

			switch (prec)
			{
				case INPrecision.QUANTITY:
					precision = CommonSetupDecPl.Qty;
					break;
				case INPrecision.UNITCOST:
					precision = CommonSetupDecPl.PrcCst;
					break;
			}
			return precision;
		}

		public static bool UsePriceAdjustmentMultiplier(PXGraph graph)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>() == false)
				return false;

			if (graph is ARInvoiceEntry || graph is SOOrderEntry || graph is ARCashSaleEntry)
			{
				SOSetup soSetup = PXSelect<SOSetup>.SelectSingleBound(graph, null);
				bool usePriceAdjustmentMultiplier = soSetup?.UsePriceAdjustmentMultiplier ?? false;
				return usePriceAdjustmentMultiplier;
		}

			return false;
		}
		#endregion
	}

	public class INUnboundUnitAttribute: INUnitAttribute
	{
		public INUnboundUnitAttribute()
		{
			ReplaceDBField();
		}

		private void ReplaceDBField()
		{
			var dbStringAttribute = _Attributes.OfType<PXDBStringAttribute>().FirstOrDefault();
			if (dbStringAttribute == null)
				throw new PXArgumentException(nameof(dbStringAttribute));
			var index = _Attributes.IndexOf(dbStringAttribute);
			_Attributes.RemoveAt(index);
			_Attributes.Insert(index, new PXStringAttribute(dbStringAttribute.Length)
			{
				IsUnicode = dbStringAttribute.IsUnicode,
				InputMask = dbStringAttribute.InputMask
			});
		}
	}

	#endregion

	#region InventoryRawAttribute

	[PXDBString(InputMask = "", IsUnicode = true)]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class InventoryRawAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INVENTORY";

		private Type _whereType;

		public InventoryRawAttribute()
			: base()
		{
			Type SearchType = typeof(Search<InventoryItem.inventoryCD, Where2<Match<Current<AccessInfo.userName>>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(InventoryItem.inventoryCD));
			attr.DescriptionField = typeof(InventoryItem.descr);
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public InventoryRawAttribute(Type WhereType)
			: this()
		{
			if (WhereType != null)
			{
				_whereType = WhereType;

				Type SearchType = BqlCommand.Compose(
					typeof(Search<,>),
					typeof(InventoryItem.inventoryCD),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And<,,>),
					typeof(InventoryItem.itemStatus),
					typeof(NotEqual<InventoryItemStatus.unknown>),
					typeof(And<,,>),
					typeof(InventoryItem.isTemplate),
					typeof(Equal<False>),
					typeof(And<>),
					_whereType);
				PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(InventoryItem.inventoryCD));
				attr.DescriptionField = typeof(InventoryItem.descr);
				attr.CacheGlobal = true;
				_Attributes[_SelAttrIndex] = attr;
			}
		}
	}

	#endregion

	#region TemplateInventoryRawAttribute

	[PXDBString(InputMask = "", IsUnicode = true)]
	[PXUIField(DisplayName = "Template ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class TemplateInventoryRawAttribute : AcctSubAttribute
	{
		public TemplateInventoryRawAttribute()
			: base()
		{
			Type SearchType = typeof(Search<InventoryItem.inventoryCD, Where2<Match<Current<AccessInfo.userName>>,
				And<InventoryItem.isTemplate, Equal<True>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(InventoryRawAttribute.DimensionName, SearchType, typeof(InventoryItem.inventoryCD));
			attr.DescriptionField = typeof(InventoryItem.descr);
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region INPrimaryAlternateType

	public enum INPrimaryAlternateType
	{
		/// <summary>Vendor part number</summary>
		VPN,
		/// <summary>Customer part number</summary>
		CPN
	}

	#endregion

	#region CrossItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class CrossItemAttribute : InventoryIncludingTemplatesAttribute, IPXFieldVerifyingSubscriber
	{
		protected class FoundInventory : Tuple<string, string, string, string, bool>
		{
			public static FoundInventory Found(string alternateID, string inventoryCD, string subItemCD, string uom, bool uniqueReference)
				=> new FoundInventory(alternateID, inventoryCD, subItemCD, uom, uniqueReference);

			public static FoundInventory NotFound(String alternateID) 
				=> new FoundInventory(alternateID, null, null, null, false);

			private FoundInventory(string alternateID, string inventoryCD, string subItemCD, string uom, bool uniqueReference) 
				: base(alternateID, inventoryCD, subItemCD, uom, uniqueReference) { }
			public string AlternateID => Item1;
			public string InventoryCD => Item2;
			public string SubItemCD => Item3;
			public string UOM => Item4;
			public bool UniqueReference => Item5;

			public bool IsFound => InventoryCD.IsNullOrEmpty() == false;
		}

		[PXLocalizable]
		public class CrossItemMessages
		{
			public const string ElementDoesntExist = "The specified inventory ID or alternate ID cannot be found in the system.";
			public const string ValueDoesntExist = "The specified inventory ID or alternate ID \"{1}\" cannot be found in the system.";
			public const string ElementDoesntExistOrNoRights = "The specified inventory ID or alternate ID cannot be found in the system. Please verify whether you have proper access rights to this object.";
			public const string ValueDoesntExistOrNoRights = "The specified inventory ID or alternate ID \"{1}\" cannot be found in the system. Please verify whether you have proper access rights to this object.";
			public const string ManyItemsForCurrentAlternateID = "The specified alternate ID is assigned to multiple inventory items. Please make sure that the correct inventory ID has been specified in the row.";
		}

		#region State
		protected INPrimaryAlternateType? _PrimaryAltType;
		protected string _AlternateID = "AlternateID";
		protected string _SubItemID = "SubItemID";
		protected string _UOM = "UOM";


		private static readonly ReadOnlyDictionary<INPrimaryAlternateType?, Type> AltTypeToDefaultBAccountFieldMap =
			new ReadOnlyDictionary<INPrimaryAlternateType?, Type>(
				new Dictionary<INPrimaryAlternateType?, Type>
				{
					[INPrimaryAlternateType.CPN] = typeof(Customer.bAccountID),
					[INPrimaryAlternateType.VPN] = typeof(Vendor.bAccountID),
				});

		public Type BAccountField
		{
			get { return _bAccountField ?? AltTypeToDefaultBAccountFieldMap.GetOrDefault(_PrimaryAltType, null); }
			set { _bAccountField = value; }
		}
		private Type _bAccountField;

		public string[] AlternateTypePriority { get; set; } = { INAlternateType.CPN, INAlternateType.VPN, INAlternateType.Barcode, INAlternateType.Global };

		public bool EnableAlternateSubstitution { get; set; } = true;

		protected Type[] InventoryRestrictingConditions
		{
			get
			{
				if (_inventoryRestrictingConditions == null)
					_inventoryRestrictingConditions =
						GetAttributes()
							.OfType<PXRestrictorAttribute>()
							.Select(r => r.RestrictingCondition)
							.Where(r => BqlCommand.Decompose(r).All(c => typeof(IBqlField).IsAssignableFrom(c) == false || c.DeclaringType.IsIn(typeof(InventoryItem), typeof(FeaturesSet))))
							.ToArray();

				return _inventoryRestrictingConditions;
			}
		}
		private Type[] _inventoryRestrictingConditions;

		public bool WarningOnNonUniqueSubstitution { get; set; }

		protected int _templateItemsRestrictorIndex = -1;
		public virtual bool AllowTemplateItems
		{
			get
			{
				return _templateItemsRestrictorIndex < 0;
			}
			set
			{
				if (value && _templateItemsRestrictorIndex >= 0)
				{
					_Attributes.RemoveAt(_templateItemsRestrictorIndex);
					_templateItemsRestrictorIndex = -1;
				}
				else if (!value && _templateItemsRestrictorIndex < 0)
				{
					_Attributes.Add(new PXRestrictorAttribute(typeof(Where<InventoryItem.isTemplate, Equal<False>>), Messages.InventoryItemIsATemplate)
					{
						ShowWarning = true
					});
					_templateItemsRestrictorIndex = _Attributes.Count - 1;
				}
			}
		}

		#endregion
		#region Ctor
		public CrossItemAttribute() : base(
			typeof(Search<InventoryItem.inventoryID, Where2<Match<Current<AccessInfo.userName>>, And<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>>>>), 
			typeof(InventoryItem.inventoryCD), 
			typeof(InventoryItem.descr))
		{
			ReplaceSelectorMessages(SelectorAttribute);
			AllowTemplateItems = false;
		}
		
		public CrossItemAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, INPrimaryAlternateType PrimaryAltType)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
			_PrimaryAltType = PrimaryAltType;
			ReplaceSelectorMessages(SelectorAttribute);
			AllowTemplateItems = false;
		}

		public CrossItemAttribute(INPrimaryAlternateType PrimaryAltType)
			: this()
		{
			_PrimaryAltType = PrimaryAltType;
		}
		
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, SelectorAttribute.FieldUpdating);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, this.FieldUpdating);
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Remove(_Attributes[_SelAttrIndex] as ISubscriber);
			}
		}

		public static void ReplaceSelectorMessages(PXDimensionSelectorAttribute selector)
		{
			selector.CustomMessageElementDoesntExist = CrossItemMessages.ElementDoesntExist;
			selector.CustomMessageElementDoesntExistOrNoRights = CrossItemMessages.ElementDoesntExistOrNoRights;
			selector.CustomMessageValueDoesntExist = CrossItemMessages.ValueDoesntExist;
			selector.CustomMessageValueDoesntExistOrNoRights = CrossItemMessages.ValueDoesntExistOrNoRights;
		}
		#endregion
		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			try
			{
				SelectorAttribute.FieldUpdating(sender, e);
				return;
			}
			catch (PXSetPropertyException)
			{
			}

			var foundAlternate = FindAlternate(sender, e.NewValue as string);
			e.NewValue = foundAlternate?.InventoryCD ?? e.NewValue;

			SelectorAttribute.FieldUpdating(sender, e);
			SetValuesPending(sender, e.Row, foundAlternate);

			RaiseWarningIfReferenceIsNotUnique(sender, e.Row, foundAlternate);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs a)
		{
			try
			{
				SelectorAttribute.FieldVerifying(sender, a);
				return;
			}
			catch (PXSetPropertyException)
			{
				if (a.Row == null)
				{
					throw;
				}
			}

			PXFieldUpdatingEventArgs e = new PXFieldUpdatingEventArgs(a.Row, sender.GetValuePending(a.Row, _FieldName));

			var foundAlternate = FindAlternate(sender, e.NewValue as string);
			e.NewValue = foundAlternate?.InventoryCD ?? e.NewValue;

			SelectorAttribute.FieldUpdating(sender, e);
			a.NewValue = e.NewValue;
			SelectorAttribute.FieldVerifying(sender, a);
			SetValuesPending(sender, e.Row, foundAlternate);

			RaiseWarningIfReferenceIsNotUnique(sender, a.Row, foundAlternate);
		}

		private void RaiseWarningIfReferenceIsNotUnique(PXCache cache, object row, FoundInventory foundAlternate)
			{
			if (WarningOnNonUniqueSubstitution && foundAlternate?.UniqueReference == false)
				cache.RaiseExceptionHandling(_FieldName, row, foundAlternate.AlternateID, new PXSetPropertyException(CrossItemMessages.ManyItemsForCurrentAlternateID, PXErrorLevel.Warning));
			}

		private void SetValuesPending(PXCache cache, object row, FoundInventory foundInventory)
			{
			if (foundInventory == null)
				return;

			if (foundInventory.AlternateID != null && cache.Fields.Contains(_AlternateID))
			{
				cache.SetValuePending(row, _AlternateID, foundInventory.AlternateID);
			}
			if (foundInventory.SubItemCD != null && cache.Fields.Contains(_SubItemID))
			{
				cache.SetValuePending(row, _SubItemID, foundInventory.SubItemCD);
		}
			if (foundInventory.UOM != null && cache.Fields.Contains(_UOM))
			{
				cache.SetValuePending(row, _UOM, foundInventory.UOM);
			}
		}

		protected FoundInventory FindAlternate(PXCache sender, string alternateID) 
			=> GenericCall.Of(() => findAlternate<IBqlField>(sender, alternateID)).ButWith(BAccountField);

		protected virtual FoundInventory findAlternate<BAccountID>(PXCache sender, string alternateID)
			where BAccountID : IBqlField
		{
			if (EnableAlternateSubstitution == false)
				return FoundInventory.NotFound(alternateID);

			PXResult<INItemXRef> mostSuitableXRef = null;
			bool severalRefs = false;

			if (String.IsNullOrEmpty(alternateID) == false)
			{
				//Sorting order is important for correct alternateType pick-up. Default attribute takes records from the tail
				PXSelectBase<INItemXRef> cmd =
					new PXSelectJoin<INItemXRef,
						InnerJoin<InventoryItem, On<INItemXRef.FK.InventoryItem>,
						LeftJoin<INSubItem, On<INItemXRef.FK.SubItem>>>,
						Where2<Match<Current<AccessInfo.userName>>, 
							And<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>>>>(sender.Graph);

				foreach (Type restriction in InventoryRestrictingConditions)
					cmd.WhereAnd(restriction);

				switch (_PrimaryAltType)
				{
					case INPrimaryAlternateType.CPN:
						cmd.WhereAnd<
							Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
							And<INItemXRef.bAccountID, Equal<Current<BAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
						break;
					case INPrimaryAlternateType.VPN:
						cmd.WhereAnd<
							Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
							And<INItemXRef.bAccountID, Equal<Current<BAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
						break;
					default:
						cmd.WhereAnd<
							Where<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
							And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>();
						break;
				}

				var refs = cmd.Select(alternateID).OrderBy(r => ((INItemXRef) r).AlternateType, AlternateTypePriority.ToArray()).ToArray();
				mostSuitableXRef = refs.FirstOrDefault();
				severalRefs = refs.Length > 1;
			}

			if (mostSuitableXRef == null)
				return FoundInventory.NotFound(alternateID);

			InventoryItem item = mostSuitableXRef.GetItem<InventoryItem>();
			INItemXRef itemXRef = mostSuitableXRef.GetItem<INItemXRef>();
			string inventoryCD = item.InventoryCD;
			string uom = itemXRef.UOM;

			if (_PrimaryAltType != null)
				{
					string alternateType = INAlternateType.ConvertFromPrimary(this._PrimaryAltType.Value);
				if (String.IsNullOrEmpty(alternateType) == false && itemXRef.AlternateType == alternateType)
					{
					alternateID = itemXRef.AlternateID; //Place typed value
					}
					else if(alternateType == INAlternateType.CPN || alternateType == INAlternateType.VPN)
					{
					PXSelectBase<INItemXRef> cmd =
						new PXSelect<INItemXRef,
							Where<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
							And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
							And<INItemXRef.alternateType, Equal<Required<INItemXRef.alternateType>>,
							And<INItemXRef.uOM, Equal<Required<INItemXRef.uOM>>,
							And<INItemXRef.bAccountID, Equal<Current<BAccountID>>>
							>>>>>(sender.Graph);

					INItemXRef itemX = cmd.SelectSingle(itemXRef.InventoryID, itemXRef.SubItemID, alternateType, itemXRef.UOM);
						if (itemX == null)
					{
						alternateID = itemXRef.AlternateID;
					}
					}
				}
				else
			{
				alternateID = itemXRef.AlternateID;
			}

			string subItemCD = null;
				//Skip assignment for the case when AlternateID is the same as InventoryID
			if (alternateID != null && string.Equals(inventoryCD.Trim(), alternateID.Trim()))
				{
				alternateID = null;
				uom = null;
				}
			else if (item.StkItem == true)
				{
				subItemCD = mostSuitableXRef.GetItem<INSubItem>().SubItemCD;
				}

			return FoundInventory.Found(alternateID, inventoryCD, subItemCD, uom, severalRefs == false);
			}

		#endregion
		#region Runtime
		public static void SetEnableAlternateSubstitution<TField>(PXCache cache, object row, bool enableAlternateSubstitution)
			where TField : IBqlField
		{
			foreach (var crossItemAttribute in cache.GetAttributes<TField>(row).OfType<CrossItemAttribute>())
				crossItemAttribute.EnableAlternateSubstitution = enableAlternateSubstitution;
		}

		public static void SetEnableAlternateSubstitution(PXCache cache, object row, Type field, bool enableAlternateSubstitution)
		{
			foreach (var crossItemAttribute in cache.GetAttributes(row, field.Name).OfType<CrossItemAttribute>())
				crossItemAttribute.EnableAlternateSubstitution = enableAlternateSubstitution;
		}
		#endregion
	}

	#endregion

	public enum AlternateIDOnChangeAction
	{
		StoreLocally,
		InsertNew,
		UpdateOriginal,
		AskUser,
	}

	#region AlternativeItemAttribute
	[PXUIField(DisplayName = "Alternate ID")]
	[PXDBString(50, IsUnicode = true, InputMask = "")]
	public class AlternativeItemAttribute : PXAggregateAttribute, IPXRowUpdatingSubscriber, IPXRowDeletingSubscriber, IPXRowInsertingSubscriber
	{
		#region State
		protected INPrimaryAlternateType? _PrimaryAltType;
		protected Type _InventoryID;
		protected Type _SubItemID;
		protected Type _UOM;
		protected Type _BAccountID;
		protected Type _AlternateIDChangeAction;
		
		protected AlternateIDOnChangeAction? _OnChangeAction;
		protected bool _KeepSinglePrimaryAltID = true;

		private PXView _xRefView;

		#endregion

		#region Ctor
		public AlternativeItemAttribute(INPrimaryAlternateType PrimaryAltType, Type InventoryID, Type SubItemID, Type uom)
			: this(PrimaryAltType, null, InventoryID, SubItemID, uom) {}

		public AlternativeItemAttribute(INPrimaryAlternateType PrimaryAltType, Type BAccountID, Type InventoryID, Type SubItemID, Type uom)
		{
			_PrimaryAltType = PrimaryAltType;
			_BAccountID = BAccountID;
			_InventoryID = InventoryID;
			_SubItemID = SubItemID;
			_UOM = uom;

			Type defaultType = GenericCall.Of(() => BuildDefaultRuleType<IBqlField, IBqlField, IBqlField, BqlNone>())
										  .ButWith(InventoryID, SubItemID, uom, CreateWhereAltType(PrimaryAltType, BAccountID));

			Type formulaType =
				_BAccountID != null
				? BqlCommand.Compose(typeof(Default<,,,>), InventoryID, SubItemID, uom, BAccountID)
				: BqlCommand.Compose(typeof(Default<,,>), InventoryID, SubItemID, uom);
		
			this._Attributes.Add(new PXDefaultAttribute(defaultType) { PersistingCheck = PXPersistingCheck.Nothing });
			this._Attributes.Add(new PXFormulaAttribute(formulaType));
		}

		private static Type BuildDefaultRuleType<TInventoryID, TSubItemID, TUOM, TWhereAltType>() 
			where TInventoryID : IBqlField
			where TSubItemID : IBqlField
			where TUOM: IBqlField
			where TWhereAltType : IBqlWhere, new()
			=> typeof(
				Coalesce<
					Coalesce<
						Search<INItemXRef.alternateID,
							Where<INItemXRef.inventoryID, Equal<Current<TInventoryID>>,
								And<INItemXRef.subItemID, Equal<Current<TSubItemID>>,
								And2<Where<INItemXRef.uOM, Equal<Current2<TUOM>>, Or<INItemXRef.uOM, IsNull, And<Current2<TUOM>, IsNull>>>,
								And<TWhereAltType>>>>,
							OrderBy<Asc<INItemXRef.alternateType>>>,
						Search<INItemXRef.alternateID,
							Where<INItemXRef.inventoryID, Equal<Current<TInventoryID>>,
								And<INItemXRef.subItemID, Equal<Current<TSubItemID>>,
								And<INItemXRef.uOM, IsNull,
								And<TWhereAltType>>>>,
							OrderBy<Asc<INItemXRef.alternateType>>>>,
					Coalesce<
						Search2<INItemXRef.alternateID,
							InnerJoin<InventoryItem, On<INItemXRef.FK.InventoryItem>>,
							Where<INItemXRef.inventoryID, Equal<Current<TInventoryID>>,
								And<INItemXRef.subItemID, Equal<InventoryItem.defaultSubItemID>,
								And2<Where<INItemXRef.uOM, Equal<Current2<TUOM>>, Or<INItemXRef.uOM, IsNull, And<Current2<TUOM>, IsNull>>>,
								And<TWhereAltType>>>>,
							OrderBy<Asc<INItemXRef.alternateType>>>,
						Search2<INItemXRef.alternateID,
							InnerJoin<InventoryItem, On<INItemXRef.FK.InventoryItem>>,
							Where<INItemXRef.inventoryID, Equal<Current<TInventoryID>>,
								And<INItemXRef.subItemID, Equal<InventoryItem.defaultSubItemID>,
								And<INItemXRef.uOM, IsNull,
								And<TWhereAltType>>>>,
							OrderBy<Asc<INItemXRef.alternateType>>>>>);

		private static Type CreateWhereAltType(INPrimaryAlternateType? primaryAltType, Type bAccountID)
		{
			Type whereAltType;
			switch (primaryAltType)
			{
				case INPrimaryAlternateType.CPN:
					whereAltType = GenericCall.Of(() => CreateWhereForNonGlobalAltType<IBqlOperand, IBqlField>())
											  .ButWith(typeof(INAlternateType.cPN), bAccountID ?? typeof(Customer.bAccountID));
					break;
				case INPrimaryAlternateType.VPN:
					whereAltType = GenericCall.Of(() => CreateWhereForNonGlobalAltType<IBqlOperand, IBqlField>())
											  .ButWith(typeof(INAlternateType.vPN), bAccountID ?? typeof(Vendor.bAccountID));
					break;
				default:
					whereAltType = typeof(Where<INItemXRef.alternateType, Equal<INAlternateType.global>>);
					break;
			}
			return whereAltType;
		}

		private static Type CreateWhereForNonGlobalAltType<TAltType, TBAccountField>() 
			where TAltType : IBqlOperand 
			where TBAccountField : IBqlField
			=> typeof(Where<INItemXRef.alternateType, Equal<TAltType>,
				And<INItemXRef.bAccountID, Equal<Current<TBAccountField>>,
					Or<INItemXRef.alternateType, Equal<INAlternateType.global>>>>);
		
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (!sender.Graph.Views.TryGetValue("_" + typeof(INItemXRef) + "_", out _xRefView))
			{
				_xRefView = new PXView(sender.Graph, false, new Select<INItemXRef>());
				sender.Graph.Views.Add("_" + typeof(INItemXRef) + "_", _xRefView);
			}

			if (!sender.Graph.Views.Caches.Contains(typeof(INItemXRef)))
				sender.Graph.Views.Caches.Add(typeof(INItemXRef));

		}
		#endregion

		#region Implementation

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			UpdateAltNumber(
				sender,
				GetBAccountID(sender, e.Row),
				GetInventoryID(sender, e.Row),
				GetSubItemID(sender, e.Row), 
				GetUOM(sender, e.Row),
				null,
				GetAlternateID(sender, e.Row));
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			bool isChangedInvID = IsChanged(sender, _InventoryID, e.Row, e.NewRow);
			bool isChangedSubID = IsChanged(sender, _SubItemID, e.Row, e.NewRow);
			bool isChangedBAccountID = IsChanged(sender, _BAccountID, e.Row, e.NewRow);
			bool isChangedUOM = IsChanged(sender, _UOM, e.Row, e.NewRow);
			bool isChangedAltID = IsChanged(sender, _FieldName, e.Row, e.NewRow);

			if (isChangedInvID || isChangedSubID || isChangedBAccountID || isChangedUOM || isChangedAltID)
			{
				DeleteUnsavedNumber(
					sender,
					GetBAccountID(sender, e.Row),
					GetInventoryID(sender, e.Row),
					GetSubItemID(sender, e.Row), 
					GetUOM(sender, e.Row),
					GetAlternateID(sender, e.Row));

				if (!(isChangedInvID || isChangedSubID || isChangedBAccountID || isChangedUOM) && isChangedAltID)
				{
					UpdateAltNumber(
						sender,
						GetBAccountID(sender, e.NewRow),
						GetInventoryID(sender, e.NewRow),
						GetSubItemID(sender, e.NewRow), 
						GetUOM(sender, e.NewRow),
						GetAlternateID(sender, e.Row),
						GetAlternateID(sender, e.NewRow));
				}
			}
		}

		public virtual void RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			DeleteUnsavedNumber(sender,
				GetBAccountID(sender, e.Row),
				GetInventoryID(sender, e.Row),
				GetSubItemID(sender, e.Row), 
				GetUOM(sender, e.Row),
				GetAlternateID(sender, e.Row));
		}


		private void DeleteUnsavedNumber(PXCache sender, int? bAccountID, int? inventoryId, int? subItem, string uom, string altId)
		{
			if (inventoryId == null || subItem == null || altId == null) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemXRef)];
			foreach (INItemXRef item in cache.Inserted)
			{
				if (item.BAccountID == bAccountID &&
						item.AlternateID == altId &&
						item.InventoryID == inventoryId &&
						item.SubItemID == subItem &&
						item.UOM == uom)
					cache.Delete(item);
			}
		}

		private void UpdateAltNumber(PXCache cache, int? bAccountID, int? inventoryId, int? subItemId, string uom, string oldAltID, string newAltID)
		{
			if (inventoryId == null || subItemId == null || newAltID == null || _PrimaryAltType == null || string.IsNullOrWhiteSpace(newAltID) || cache.Graph.IsImport || cache.Graph.IsCopyPasteContext) return;
			AlternateIDOnChangeAction action = this.GetOnChangeAction(cache.Graph);

			if (action == AlternateIDOnChangeAction.StoreLocally || cache.Graph.IsCopyPasteContext) return;
			PXSelectBase<INItemXRef> cmdFullSearch = 
				new PXSelect<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
					And<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
                    And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
					And<Where<INItemXRef.uOM, Equal<Required<INItemXRef.uOM>>, Or<INItemXRef.uOM, IsNull>>>>>>,
				OrderBy<Asc<INItemXRef.alternateType, Desc<INItemXRef.alternateID>>>>(cache.Graph);
			AddAlternativeTypeWhere(cmdFullSearch, _PrimaryAltType, false);
			INItemXRef existing = cmdFullSearch.Select(newAltID, inventoryId, subItemId, uom, bAccountID);
            if (existing != null)
                return; //Applicable record with new AlternateID exists - no need to update Xref

			// Uniqueness validation
			PXSelectBase<INItemXRef> cmdAlt = 
				new PXSelect<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
					And<INItemXRef.inventoryID, NotEqual<Required<INItemXRef.inventoryID>>>>>(cache.Graph);
			AddAlternativeTypeWhere(cmdAlt, _PrimaryAltType, false);
			if (cmdAlt.Select(newAltID, inventoryId, bAccountID).RowCast<INItemXRef>().Any(it => it.InventoryID != inventoryId || it.SubItemID != subItemId || it.UOM != uom))
				throw new PXSetPropertyException(Messages.AlternatieIDNotUnique, PXErrorLevel.Error, newAltID);

			INItemXRef xref = null;
			if(action == AlternateIDOnChangeAction.UpdateOriginal || action == AlternateIDOnChangeAction.AskUser)
			{
				PXSelectBase<INItemXRef> cmdInv =
					new PXSelect<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
					And<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
					And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
					And<Where<INItemXRef.uOM, Equal<Required<INItemXRef.uOM>>, Or<INItemXRef.uOM, IsNull>>>>>>, 
					OrderBy<Asc<INItemXRef.alternateType, Desc<INItemXRef.alternateID, Desc<INItemXRef.uOM>>>>>(cache.Graph);
				AddAlternativeTypeWhere(cmdInv, _PrimaryAltType, true);
				xref = cmdInv.Select(oldAltID, inventoryId, subItemId, uom, bAccountID);

				if (xref != null)
				{
					if (string.IsNullOrEmpty(xref.AlternateID) || action != AlternateIDOnChangeAction.AskUser || UserWantsToUpdateXRef())
						_xRefView.Cache.Delete(xref);
					else
							return; // Store locally						
					}
				else
				{
					if (this._KeepSinglePrimaryAltID)
					{
						PXSelectBase<INItemXRef> cmdOtherInv = 
							new PXSelect<INItemXRef,
							Where<INItemXRef.alternateID, NotEqual<Required<INItemXRef.alternateID>>,
							And<INItemXRef.alternateID, NotEqual<Empty>,
							And<INItemXRef.alternateID, IsNotNull,
							And<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
							And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
							And<Where<INItemXRef.uOM, Equal<Required<INItemXRef.uOM>>, Or<INItemXRef.uOM, IsNull>>>>>>>>>(cache.Graph);
						AddAlternativeTypeWhere(cmdOtherInv, _PrimaryAltType, true);
						if (cmdOtherInv.Select(newAltID, inventoryId, subItemId, uom, bAccountID).Any())
							return; // There is another
					}
				}
			}

			bool createNew = xref == null;

			xref = createNew
				? new INItemXRef()
				: (INItemXRef) _xRefView.Cache.CreateCopy(xref);
			xref.InventoryID = inventoryId;
			xref.SubItemID = subItemId;
			xref.BAccountID = bAccountID;
			xref.AlternateID = newAltID;
			if (createNew || xref.UOM != null)
				xref.UOM = uom;
			xref.AlternateType = INAlternateType.ConvertFromPrimary(_PrimaryAltType.Value);
			_xRefView.Cache.Update(xref);
			_xRefView.Answer = WebDialogResult.None;
		} 

		private Boolean UserWantsToUpdateXRef() => _xRefView.Ask(Messages.ConfirmationXRefUpdate, MessageButtons.YesNo, false) == WebDialogResult.Yes;

		public static void AddAlternativeTypeWhere(PXSelectBase<INItemXRef> cmd, INPrimaryAlternateType? primaryAlternateType, bool typeExclusive)
		{
			switch (primaryAlternateType)
			{
				case INPrimaryAlternateType.CPN:
					if (typeExclusive)
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>>>>();
					}
					else
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
					}
					break;
				case INPrimaryAlternateType.VPN:
					if (typeExclusive)
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
							And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>>>>();
					}
					else
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
					}
					break;
				default:
					cmd.WhereAnd<Where<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
							And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>();
					break;
			}
		}

		#region Fields value getters
		private string GetAlternateID(PXCache sender, object row) => (string)sender.GetValue(row, _FieldName);
		private int? GetSubItemID(PXCache cache, object row) => GetCurrentValue<int?>(cache, _SubItemID, row);
		private int? GetInventoryID(PXCache cache, object row) => GetCurrentValue<int?>(cache, _InventoryID, row);
		private string GetUOM(PXCache cache, object row) => GetCurrentValue<string>(cache, _UOM, row);
		private int? GetBAccountID(PXCache cache, object row)
			=> _BAccountID == null
				? (_PrimaryAltType == INPrimaryAlternateType.VPN
					? GetCurrentValue<int?>(cache, typeof(Vendor.bAccountID))
					: GetCurrentValue<int?>(cache, typeof(Customer.bAccountID)))
				: GetCurrentValue<int?>(cache, _BAccountID, row);

		private TOut GetCurrentValue<TOut>(PXCache cache, Type field, object row)
		{
			PXCache source = cache.Graph.Caches[BqlCommand.GetItemType(field)];
			return (TOut)source.GetValue(row, field.Name);
		}

		private TOut GetCurrentValue<TOut>(PXCache cache, Type field)
		{
			PXCache source = cache.Graph.Caches[BqlCommand.GetItemType(field)];
			return (TOut)source.GetValue(source.Current, field.Name);
		} 
		#endregion

		private bool IsChanged(PXCache cache, Type fieldSource, object row, object newrow)
			=> fieldSource != null
				&& BqlCommand.GetItemType(fieldSource).IsAssignableFrom(cache.GetItemType())
				&& IsChanged(cache, fieldSource.Name, row, newrow);

		private bool IsChanged(PXCache cache, string fieldName, object row, object newrow) 
			=> Equals(cache.GetValue(newrow, fieldName), cache.GetValue(row, fieldName)) == false;

		private AlternateIDOnChangeAction GetOnChangeAction(PXGraph caller) 
		{
			if (this._OnChangeAction == null) 
			{
				this._OnChangeAction = AlternateIDOnChangeAction.AskUser;
			}
			return this._OnChangeAction.Value;
		}
		#endregion
	}
	#endregion

	#region PriceWorksheetAlternateItemAttribute
	[PXDBString(50, IsUnicode = true, InputMask = "")]
	[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
	[PXUIField(DisplayName = "Alternate ID", Visibility = PXUIVisibility.Dynamic)]
	public class PriceWorksheetAlternateItemAttribute : PXAggregateAttribute
	{
		public class PriceWrapper
		{
			public PriceWrapper(object priceWorksheetDetail)
			{
				if (priceWorksheetDetail == null)
					throw new ArgumentNullException(nameof(priceWorksheetDetail));

				var arPrice = priceWorksheetDetail as ARPriceWorksheetDetail;
				if (arPrice != null)
				{
					PriceWorksheetDetail = arPrice;

					AlternateID = arPrice.AlternateID;
					BAccountID = arPrice.CustomerID;
					InventoryID = arPrice.InventoryID;
					SubItemID = arPrice.SubItemID;

					AlternateType = arPrice.PriceType == PriceTypeList.Customer ? INPrimaryAlternateType.CPN.AsNullable() : null;

					UOMField = typeof(ARPriceWorksheetDetail.uOM);
					InventoryIDField = typeof(ARPriceWorksheetDetail.inventoryID);
					AlternateIDField = typeof(ARPriceWorksheetDetail.alternateID);
					RestrictInventoryByAlternateIDField = typeof(ARPriceWorksheetDetail.restrictInventoryByAlternateID);

					return;
				}

				var apPrice = priceWorksheetDetail as APPriceWorksheetDetail;
				if (apPrice != null)
				{
					PriceWorksheetDetail = apPrice;

					AlternateID = apPrice.AlternateID;
					BAccountID = apPrice.VendorID;
					InventoryID = apPrice.InventoryID;
					SubItemID = apPrice.SubItemID;

					AlternateType = INPrimaryAlternateType.VPN;

					UOMField = typeof(APPriceWorksheetDetail.uOM);
					InventoryIDField = typeof(APPriceWorksheetDetail.inventoryID);
					AlternateIDField = typeof(APPriceWorksheetDetail.alternateID);
					RestrictInventoryByAlternateIDField = typeof(APPriceWorksheetDetail.restrictInventoryByAlternateID);

					return;
				}

				throw new PXArgumentException("Attribute supports only {0} and {1} entities", typeof(ARPriceWorksheetDetail), typeof(APPriceWorksheetDetail));
			}

			public object PriceWorksheetDetail { get; }

			public string AlternateID { get; }
			public int? BAccountID { get; }
			public int? InventoryID { get; }
			public int? SubItemID { get; }

			public INPrimaryAlternateType? AlternateType { get; }

			public Type UOMField { get; }
			public Type InventoryIDField { get; }
			public Type AlternateIDField { get; }
			public Type RestrictInventoryByAlternateIDField { get; }
		}

		public string[] AlternateTypePriority { get; set; } = { INAlternateType.CPN, INAlternateType.VPN, INAlternateType.Barcode, INAlternateType.Global };


		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldVerifying.AddHandler(typeof(INItemXRef), nameof(INItemXRef.BAccountID), INItemXRef_BAccountID_FieldVerifying);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), "InventoryID", PriceWorksheetDetail_InventoryID_FieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), "AlternateID", PriceWorksheetDetail_AlternateID_FieldUpdated);
			sender.Graph.RowSelected.AddHandler(sender.GetItemType(), PriceWorksheetDetail_RowSelected);
		}


		protected virtual void INItemXRef_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var xRef = (INItemXRef)e.Row;
			if (xRef.AlternateType != INAlternateType.VPN && xRef.AlternateType != INAlternateType.CPN)
				e.Cancel = true;
		}

		protected virtual void PriceWorksheetDetail_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var priceWrapper = new PriceWrapper(e.Row);
			PresetUOMWithSpecialUnits(sender, priceWrapper);
			UpdateUOMFromCrossReference(sender, priceWrapper, false);
		}

		protected virtual void PriceWorksheetDetail_AlternateID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateUOMFromCrossReference(sender, new PriceWrapper(e.Row), false);
		}

		protected virtual void PriceWorksheetDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			UpdateUOMFromCrossReference(sender, new PriceWrapper(e.Row), true);
		}


		private static void PresetUOMWithSpecialUnits(PXCache sender, PriceWrapper priceWrapper)
		{
			var inventory = InventoryItem.PK.Find(sender.Graph, priceWrapper.InventoryID);
			if (priceWrapper.AlternateType == INPrimaryAlternateType.VPN)
		{
				if (inventory?.PurchaseUnit != null)
					sender.SetValueExt(priceWrapper.PriceWorksheetDetail, priceWrapper.UOMField.Name, inventory.PurchaseUnit);
			}
			else
			{
				if (inventory?.SalesUnit != null)
					sender.SetValueExt(priceWrapper.PriceWorksheetDetail, priceWrapper.UOMField.Name, inventory.SalesUnit);
			}
		}

		private static void UpdateUOMFromCrossReference(PXCache cache, PriceWrapper priceWorksheetDetail, bool warningSettingOnly)
		{
			bool loadSalesPricesUsingAlternateID =
				priceWorksheetDetail.AlternateType != INPrimaryAlternateType.VPN
				&& PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
				&& new PXSetupOptional<ARSetup>(cache.Graph).Current.LoadSalesPricesUsingAlternateID == true;
			bool loadVendorsPricesUsingAlternateID =
				priceWorksheetDetail.AlternateType == INPrimaryAlternateType.VPN
				&& PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
				&& new PXSetupOptional<APSetup>(cache.Graph).Current.LoadVendorsPricesUsingAlternateID == true;

			if (loadVendorsPricesUsingAlternateID == false && loadSalesPricesUsingAlternateID == false)
				return;

			ClearWarning(cache, priceWorksheetDetail);
			RestrictInventoryByAlternateID(cache, priceWorksheetDetail, false);

			if (priceWorksheetDetail.AlternateID.IsNullOrEmpty())
				return;

			var xRefs = SelectXRefs(cache, priceWorksheetDetail);

			if (priceWorksheetDetail.InventoryID == null)
			{
				if (xRefs.Length == 0)
				{
					SetWarning(cache, priceWorksheetDetail, Messages.NoSpecifiedAltID);
		}
				else if (xRefs.Length == 1)
				{
					if (warningSettingOnly) return;
					var xref = xRefs.Single();

					cache.SetValueExt(priceWorksheetDetail.PriceWorksheetDetail, priceWorksheetDetail.InventoryIDField.Name, xref.InventoryID);
					if (xref.UOM != null)
						cache.SetValueExt(priceWorksheetDetail.PriceWorksheetDetail, priceWorksheetDetail.UOMField.Name, xref.UOM);
				}
				else
				{
					SetWarning(cache, priceWorksheetDetail, Messages.ManyAltIDsForSingleInventoryID);
					RestrictInventoryByAlternateID(cache, priceWorksheetDetail, true);
				}
			}
			else
		{
				var xref = xRefs.FirstOrDefault();
				if (xref == null)
			{
					if (PXAccess.FeatureInstalled<FeaturesSet.crossReferenceUniqueness>() && ExistsGlobalXRefWithSameAltID(cache, priceWorksheetDetail))
						SetWarning(cache, priceWorksheetDetail, Messages.AltIDIsNotDefinedAndWillNotBeAddedOnRelease);
					else
						SetWarning(cache, priceWorksheetDetail, Messages.AltIDIsNotDefinedAndWillBeAddedOnRelease);
				}
				else if (warningSettingOnly == false && xref.UOM != null)
				{
					cache.SetValueExt(priceWorksheetDetail.PriceWorksheetDetail, priceWorksheetDetail.UOMField.Name, xref.UOM);
				}
			}
		}

		private static bool ExistsGlobalXRefWithSameAltID(PXCache cache, PriceWrapper priceWorksheetDetail)
		{
			PXSelectBase<INItemXRef> cmdInv = 
				new PXSelectReadonly<INItemXRef, 
				Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
				And<INItemXRef.alternateType, Equal<INAlternateType.global>>>>(cache.Graph);

			return cmdInv
				.Select(priceWorksheetDetail.AlternateID)
				.RowCast<INItemXRef>()
				.Any(xr => (xr.InventoryID == priceWorksheetDetail.InventoryID && xr.SubItemID == priceWorksheetDetail.SubItemID) == false);
		}

		private static INItemXRef[] SelectXRefs(PXCache cache, PriceWrapper priceWorksheetDetail)
		{
			PXSelectBase<INItemXRef> cmdInv =
				new PXSelectReadonly<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>>,
					OrderBy<Asc<INItemXRef.alternateType, Desc<INItemXRef.alternateID>>>>(cache.Graph);
			var parameters = new List<object> { priceWorksheetDetail.AlternateID };

			AlternativeItemAttribute.AddAlternativeTypeWhere(cmdInv, priceWorksheetDetail.AlternateType, false);
			if (priceWorksheetDetail.AlternateType != null)
				parameters.Add(priceWorksheetDetail.BAccountID);

			if (priceWorksheetDetail.InventoryID != null)
		{
				cmdInv.WhereAnd<
					Where<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>>>();
				parameters.Add(priceWorksheetDetail.InventoryID);
			}

			if (priceWorksheetDetail.SubItemID != null)
			{
				cmdInv.WhereAnd<
					Where<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>>>();
				parameters.Add(priceWorksheetDetail.SubItemID);
			}

			var xRefs = cmdInv.Select(parameters.ToArray()).RowCast<INItemXRef>().ToArray();

			string[] alternateTypePriority =
				cache.GetAttributesOfType<PriceWorksheetAlternateItemAttribute>(
					priceWorksheetDetail.PriceWorksheetDetail,
					priceWorksheetDetail.AlternateIDField.Name)
					.FirstOrDefault()?
					.AlternateTypePriority;

			var typeGroups = xRefs.GroupBy(x => x.AlternateType).OrderBy(gx => gx.Key, alternateTypePriority);

			return (typeGroups.FirstOrDefault(gx => gx.Any()) ?? Enumerable.Empty<INItemXRef>()).ToArray();
			}

		private static void SetWarning(PXCache cache, PriceWrapper priceWorksheetDetail, string message)
		{
			cache.RaiseExceptionHandling(
				priceWorksheetDetail.AlternateIDField.Name,
				priceWorksheetDetail.PriceWorksheetDetail,
				priceWorksheetDetail.AlternateID,
				message.IsNullOrEmpty()
					? null
					: new PXSetPropertyException(message, PXErrorLevel.Warning));
		}

		private static void ClearWarning(PXCache cache, PriceWrapper priceWorksheetDetail) => SetWarning(cache, priceWorksheetDetail, null);
		private static void RestrictInventoryByAlternateID(PXCache cache, PriceWrapper priceWorksheetDetail, bool enable)
			=> cache.SetValue(priceWorksheetDetail.PriceWorksheetDetail, priceWorksheetDetail.RestrictInventoryByAlternateIDField.Name, enable);

		public static bool XRefsExists(PXCache cache, object priceWorksheetDetail) => SelectXRefs(cache, new PriceWrapper(priceWorksheetDetail)).Any();
	}
	#endregion

	#region StockItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<InventoryItem.stkItem, Equal<boolTrue>>), Messages.InventoryItemIsNotAStock)]
    public class StockItemAttribute : InventoryAttribute
	{
		public StockItemAttribute()
            : base()
        {
        }
	}

	#endregion

	#region NonStockItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class NonStockItemAttribute : InventoryAttribute
	{
		public static Type Search => typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>);

		public static PXRestrictorAttribute CreateRestrictor()
			=> new PXRestrictorAttribute(typeof(Where<InventoryItem.stkItem, Equal<boolFalse>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>>>>), Messages.InventoryItemIsAStock);

		public static PXRestrictorAttribute CreateRestrictorDependingOnFeature<TFeature>()
			where TFeature : IBqlField
			=> new PXRestrictorAttribute(
				typeof(Where2<FeatureInstalled<TFeature>, Or<InventoryItem.stkItem, Equal<boolFalse>>>),
				Messages.InventoryItemIsAStock);

		public NonStockItemAttribute() : base(Search, typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{			
			_Attributes.Add(CreateRestrictor());
		}
	}

	#endregion

    #region NonStockNonKitItemAttribute

    public class NonStockNonKitItemAttribute : NonStockItemAttribute
    {
		public static PXRestrictorAttribute CreateStandardRestrictor()
			=> new PXRestrictorAttribute(
				typeof(Where<InventoryItem.stkItem, NotEqual<False>, Or<InventoryItem.kitItem, NotEqual<True>>>),
				Messages.CannotAddNonStockKit);

		public static PXRestrictorAttribute CreateCustomRestrictor<TOrigNbrField>(String aErrorMsg)
			where TOrigNbrField : IBqlField
			=> new PXRestrictorAttribute(
				typeof(Where<Current<TOrigNbrField>, IsNotNull, Or<InventoryItem.stkItem, NotEqual<False>, Or<InventoryItem.kitItem, NotEqual<True>>>>),
				aErrorMsg);

		public NonStockNonKitItemAttribute()
		{
			_Attributes.Add(CreateStandardRestrictor());
		}

		public NonStockNonKitItemAttribute(String aErrorMsg, Type origNbrField)
		{
			_Attributes.Add(GenericCall.Of(() => CreateCustomRestrictor<IBqlField>(aErrorMsg)).ButWith(origNbrField));
		}
        }

	#endregion

	#region NonStockNonKitCrossItemAttribute

	public class NonStockNonKitCrossItemAttribute : CrossItemAttribute
	{
		private NonStockNonKitCrossItemAttribute(Type search, INPrimaryAlternateType primaryAltType) : base(
			search,
			typeof(InventoryItem.inventoryCD),
			typeof(InventoryItem.descr),
			primaryAltType) {}

		public NonStockNonKitCrossItemAttribute(INPrimaryAlternateType primaryAltType)
			: this(NonStockItemAttribute.Search, primaryAltType)
		{
			_Attributes.Add(NonStockItemAttribute.CreateRestrictor());
			_Attributes.Add(NonStockNonKitItemAttribute.CreateStandardRestrictor());
    }

		public NonStockNonKitCrossItemAttribute(INPrimaryAlternateType primaryAltType, String aErrorMsg, Type origNbrField)
			: this(NonStockItemAttribute.Search, primaryAltType)
		{
			_Attributes.Add(NonStockItemAttribute.CreateRestrictor());
			_Attributes.Add(GenericCall.Of(() => NonStockNonKitItemAttribute.CreateCustomRestrictor<IBqlField>(aErrorMsg)).ButWith(origNbrField));
		}

		public NonStockNonKitCrossItemAttribute(INPrimaryAlternateType primaryAltType, String aErrorMsg, Type origNbrField, Type allowStkFeature)
			: this(NonStockItemAttribute.Search, primaryAltType)
		{
			_Attributes.Add(GenericCall.Of(() => NonStockItemAttribute.CreateRestrictorDependingOnFeature<IBqlField>()).ButWith(allowStkFeature));
			_Attributes.Add(GenericCall.Of(() => NonStockNonKitItemAttribute.CreateCustomRestrictor<IBqlField>(aErrorMsg)).ButWith(origNbrField));
		}
	}
	#endregion

	#region BaseInventoryAttribute

	/// <summary>
	/// Provides a base selector for the Inventory Items. The list is filtered by the user access rights.
	/// </summary>
	[PXDBInt]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public abstract class BaseInventoryAttribute : AcctSubAttribute
	{
		#region State
		public const string DimensionName = "INVENTORY";

		public class dimensionName : BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) {; }
		}
		#endregion
		#region Ctor
		public BaseInventoryAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}

		public BaseInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base()
		{
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, SubstituteKey);
			attr.CacheGlobal = true;
			attr.DescriptionField = DescriptionField;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public BaseInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, Type[] fields)
		{
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, SubstituteKey, fields);
			attr.CacheGlobal = true;
			attr.DescriptionField = DescriptionField;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
		#endregion
	}

	#endregion

	#region InventoryIncludingTemplatesAttribute

	/// <summary>
	/// Provides a selector for the Inventory Items including Template Items.
	/// The list is filtered by the user access rights and Inventory Item status - inactive and marked to delete items are not shown.
	/// </summary>
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>), PM.Messages.ReservedForProject, ShowWarning = true)]
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
		And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>),
		Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
	public class InventoryIncludingTemplatesAttribute : BaseInventoryAttribute
	{
		#region Ctor
		public InventoryIncludingTemplatesAttribute()
			: base()
		{
		}

		public InventoryIncludingTemplatesAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
		}

		public InventoryIncludingTemplatesAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, Type[] fields)
			: base(SearchType, SubstituteKey, DescriptionField, fields)
		{
		}
		#endregion
	}
	#endregion

	#region AnyInventoryAttribute

	/// <summary>
	/// Provides a base selector for the Inventory Items. The list is filtered by the user access rights and excludes Template and Unknown items.
	/// </summary>
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>), PM.Messages.ReservedForProject, ShowWarning = true)]
	[PXRestrictor(typeof(Where<InventoryItem.isTemplate, Equal<False>>), Messages.InventoryItemIsATemplate, ShowWarning = true)]
	public class AnyInventoryAttribute : BaseInventoryAttribute
	{
		#region Ctor
		public AnyInventoryAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}

		public AnyInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
		}

		public AnyInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, Type[] fields)
			: base(SearchType, SubstituteKey, DescriptionField, fields)
		{
		}
		#endregion
	}

	#endregion

	#region TemplateInventoryAttribute

	/// <summary>
	/// Provides a base selector for the Template Inventory Items. The list is filtered by the user access rights.
	/// </summary>
	[PXUIField(DisplayName = "Template ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<InventoryItem.isTemplate, Equal<True>>), Messages.InventoryItemIsNotATemplate, ShowWarning = true)]
	public class TemplateInventoryAttribute : BaseInventoryAttribute
	{
		#region Ctor
		public TemplateInventoryAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}

		public TemplateInventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
		}
		#endregion
	}

	#endregion

	#region InventoryAttribute

	/// <summary>
	/// Provides a selector for the Inventory Items.
	/// The list is filtered by the user access rights and Inventory Item status - inactive and marked to delete items are not shown.
	/// </summary>
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
		And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>),
		Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
	public class InventoryAttribute : AnyInventoryAttribute
	{
		#region Ctor
		public InventoryAttribute()
			: base()
		{
		}

		public InventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base(SearchType, SubstituteKey, DescriptionField)
		{
		}

		public InventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField, Type[] fields)
			: base(SearchType, SubstituteKey, DescriptionField, fields)
		{
		}
		#endregion
	}
	#endregion

	#region PriceWorksheetInventoryAttribute

	public class PriceWorksheetInventoryAttribute : InventoryIncludingTemplatesAttribute
	{
		public class INItemXRefExt : INItemXRef
		{
			[PXDBString(6, IsUnicode = true), PXUIField(DisplayName = "Alt. ID Unit", Visibility = PXUIVisibility.Visible)]
			public override string UOM { get; set; }
			public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
			public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			public new abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
			public new abstract class alternateType : PX.Data.BQL.BqlString.Field<alternateType> { }
		}

		public PriceWorksheetInventoryAttribute(Type bAccount, Type alternateID, Type alternateType, Type restrictInventoryByAlternateID)
			: base(
				GenericCall.Of(() => BuildSearch<IBqlField, IBqlField, IBqlOperand, IBqlField>())
							.ButWith(bAccount, alternateID, alternateType, restrictInventoryByAlternateID),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr),
				new[]
				{
					typeof(InventoryItem.inventoryCD),
					typeof(InventoryItem.descr),
					typeof(InventoryItem.itemClassID),
					typeof(InventoryItem.itemStatus),
					typeof(InventoryItem.itemType),
					typeof(InventoryItem.baseUnit),
					typeof(InventoryItem.salesUnit),
					typeof(InventoryItem.purchaseUnit),
					typeof(InventoryItem.basePrice),
					typeof(INItemXRefExt.uOM),
				})
		{
			CrossItemAttribute.ReplaceSelectorMessages(SelectorAttribute);
		}

		private static Type BuildSearch<TBAccount, TAlternateID, TAlternateType, TRestrictInventoryByAlternateID>()
			where TBAccount : IBqlField
			where TAlternateID : IBqlField
			where TAlternateType : IBqlOperand
			where TRestrictInventoryByAlternateID : IBqlField
			=> typeof(
			Search5<InventoryItem.inventoryID,
			LeftJoin<INItemXRefExt, On<
				InventoryItem.inventoryID, Equal<INItemXRefExt.inventoryID>, 
				And<Current<TRestrictInventoryByAlternateID>, Equal<True>>>>,
			Where2<
				Match<Current<AccessInfo.userName>>,
				And<Where<
					Current<TRestrictInventoryByAlternateID>, Equal<False>,
					Or<Where<
						Current<TAlternateID>, Equal<INItemXRefExt.alternateID>,
						And<Where<
							INItemXRefExt.alternateType, Equal<TAlternateType>,
							And<INItemXRefExt.bAccountID, Equal<Current<TBAccount>>,
							Or<INItemXRefExt.alternateType, NotEqual<INAlternateType.cPN>,
							And<INItemXRefExt.alternateType, NotEqual<INAlternateType.vPN>>>>
						>>
					>>
				>>
			>,
			Aggregate<GroupBy<InventoryItem.inventoryID, Max<INItemXRefExt.uOM>>>>);
	}
	#endregion

	#region SubItemRawAttribute
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subitem ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = DimensionName)]
	public class SubItemRawAttribute : AcctSubAttribute
	{
		public bool SuppressValidation;
		public const string DimensionName = "INSUBITEM";

		public SubItemRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(DimensionName);
			attr.ValidComboRequired = false;

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region SubItemRawExtAttribute
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subitem ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = DimensionName)]
	public class SubItemRawExtAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INSUBITEM";

		#region Ctors
		public SubItemRawExtAttribute()
			: base()
		{
		}

		public SubItemRawExtAttribute(Type inventoryItem)
			: this()
		{
			if (inventoryItem != null)
			{
				Type SearchType = BqlCommand.Compose(
					typeof(Search<,>),
					typeof(INSubItem.subItemCD),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And<>),
					typeof(Where<,,>),
					typeof(Optional<>), inventoryItem, typeof(IsNull),
					typeof(Or<>),
					typeof(Where<>),
					typeof(Match<>),
					typeof(Optional<>),
					inventoryItem);

				var attr = new PXDimensionSelectorAttribute(DimensionName, SearchType);
				attr.ValidComboRequired = false;
				_Attributes.Add(attr);
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).SetSegmentDelegate(null);
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = this.Definitions.DefaultSubItemCD;
			e.Cancel = true;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Clear();
			}
		}


		#endregion

		#region Default SubItemID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSubItem.DefinitionCD", typeof(INSubItem)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			private string _DefaultSubItemCD;
			public string DefaultSubItemCD
			{
				get { return _DefaultSubItemCD; }
			}

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<INSubItem>(
					new PXDataField<INSubItem.subItemCD>(),
					new PXDataFieldOrder<INSubItem.subItemID>()))
				{
					_DefaultSubItemCD = null;
					if (record != null)
						_DefaultSubItemCD = record.GetString(0);
				}
			}
		}
		#endregion

	}
	#endregion

	#region inventoryModule

	public sealed class inventoryModule : PX.Data.BQL.BqlString.Constant<inventoryModule>
	{
		public inventoryModule()
			: base(typeof(inventoryModule).Namespace)
		{
		}
	}

	#endregion

	#region warehouseType

	public sealed class warehouseType : PX.Data.BQL.BqlString.Constant<warehouseType>
	{
		public warehouseType()
			: base(typeof(PX.Objects.IN.INSite).FullName)
		{
		}
	}

	#endregion

	#region itemType

	public sealed class itemType : PX.Data.BQL.BqlString.Constant<itemType>
	{
		public itemType()
			: base(typeof(PX.Objects.IN.InventoryItem).FullName)
		{
		}
	}

	#endregion

	#region itemClassType

	public sealed class itemClassType : PX.Data.BQL.BqlString.Constant<itemClassType>
	{
		public itemClassType()
			: base(typeof(PX.Objects.IN.INItemClass).FullName)
		{
		}
	}

	#endregion

	#region INSetupSelect

	public sealed class INSetupSelect : Data.PXSetupSelect<INSetup>
	{
		public INSetupSelect(PXGraph graph) : base(graph) { }
	}

	#endregion

	#region SubItemAttribute
	[PXDBInt]
	[PXUIField(DisplayName = "Subitem", Visibility = PXUIVisibility.Visible, FieldClass = SubItemAttribute.DimensionName)]
	public class SubItemAttribute : AcctSubAttribute
	{
		const string DefaultSubItemValue = "0";

		#region dimensionName

		public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) { }
		}

		#endregion

		private class INSubItemDimensionAttribute : PXDimensionAttribute
		{
			Type _inventoryID;

			public INSubItemDimensionAttribute(Type inventoryID, string dimension)
				: base(dimension)
			{
				_inventoryID = inventoryID;
			}

			protected override bool FindValueBySegmentDelegate(PXCache sender, object row, string segmentDescr, short segmentID, string val, string currentValue)
			{
				bool match = base.FindValueBySegmentDelegate(sender, row, segmentDescr, segmentID, val, currentValue);

				if (!match)
				{
					Dictionary<string, ValueDescr> values = _Definition.Values[_Dimension][segmentID];
					if (values.ContainsKey(currentValue))
					{
						int? inventoryID = (int?)sender.GetValue(row, _inventoryID.Name);

						if (inventoryID != null)
						{
							var inventoryItem = InventoryItem.PK.Find(sender.Graph, inventoryID);
							if (val == DefaultSubItemValue && inventoryItem != null && inventoryItem.StkItem != true)
								return true;

							throw new PXSetPropertyException(Messages.SubItemIsDisabled, values[currentValue].Descr ?? currentValue,
								segmentDescr ?? segmentID.ToString(), inventoryItem?.InventoryCD ?? inventoryID.ToString());
						}
					}
				}

				return match;
			}
		}

		#region INSubItemDimensionSelector
		private class INSubItemDimensionSelector: PXDimensionSelectorAttribute
		{
			private readonly Type _inventoryID = null;

			private bool _ValidateValueOnFieldUpdating;
			public bool ValidateValueOnFieldUpdating
			{
				get
				{
					return _ValidateValueOnFieldUpdating;
				}
				set
				{
					this.SetSegmentDelegate(value ? (PXSelectDelegate<short?>)DoSegmentSelect : null);

					_ValidateValueOnFieldUpdating = value;
				}
			}

			public bool ValidateValueOnPersisting
			{
				get;
				set;
			}

			public INSubItemDimensionSelector(Type inventoryID, Type search)
				: base(DimensionName, search, typeof(INSubItem.subItemCD))
			{				
				this._inventoryID = inventoryID;				
				if (this._inventoryID != null)
				{
					int dimensionAttributeIndex = _Attributes.IndexOf(DimensionAttribute);
					_Attributes[dimensionAttributeIndex] = new INSubItemDimensionAttribute(inventoryID, DimensionName);

					this.CacheGlobal = false;
					this.ValidateValueOnFieldUpdating = true;
				}				 
			}		

			private IEnumerable DoSegmentSelect([PXShort] short? segment)
			{
				PXGraph graph = PXView.CurrentGraph;
				if (_inventoryID == null) yield break;

                int? inventoryID = null;
                if (PXView.Currents != null)
                    foreach (object item in PXView.Currents)
                    {
                        if (item.GetType() == _inventoryID.DeclaringType)
                            inventoryID = (int?)graph.Caches[_inventoryID.DeclaringType].GetValue(item, _inventoryID.Name);
                    }

				int startRow = PXView.StartRow;
				int totalRows = 0;
				
				PXView intView = new PXView(PXView.CurrentGraph, false,
					BqlCommand.CreateInstance(
						typeof(Select2<,,>),
						typeof(INSubItemSegmentValue),
						typeof(InnerJoin<SegmentValue,
										On<SegmentValue.segmentID, Equal<INSubItemSegmentValue.segmentID>,
									And<SegmentValue.value, Equal<INSubItemSegmentValue.value>,
									And<SegmentValue.dimensionID, Equal<SubItemAttribute.dimensionName>>>>>),
						typeof(Where<,,>), typeof(INSubItemSegmentValue.segmentID), typeof(Equal<Required<SegmentValue.segmentID>>),
						typeof(And<,>), typeof(INSubItemSegmentValue.inventoryID), typeof(Equal<>), typeof(Optional<>), _inventoryID));

				foreach (PXResult<INSubItemSegmentValue, SegmentValue> rec in intView
					.Select(PXView.Currents, new object[]{segment, inventoryID}, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								 ref startRow, PXView.MaximumRows, ref totalRows))
				{
					SegmentValue value = rec;
					PXDimensionAttribute.SegmentValue ret = new PXDimensionAttribute.SegmentValue();					
					ret.Value = value.Value;					
					ret.Descr = value.Descr;
					ret.IsConsolidatedValue = value.IsConsolidatedValue;
					yield return ret;
				}
				PXView.StartRow = 0;
			}

			public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				base.RowPersisting(sender, e);

				if (ValidateValueOnPersisting &&
					e.Row != null && e.Operation == PXDBOperation.Insert)
				{
					try
					{
						object subItem = sender.GetValue(e.Row, _FieldOrdinal);
						sender.RaiseFieldVerifying(_FieldName, e.Row, ref subItem);
					}
					catch (PXSetPropertyException exc)
					{
						object value = sender.GetValue(e.Row, _FieldOrdinal);
						if (sender.RaiseExceptionHandling(_FieldName, e.Row, value, exc))
						{
							throw new PXRowPersistingException(_FieldName, value, exc.Message);
						}
					}
				}
			}
		}
		#endregion 

		#region Fields

		public const string DimensionName = "INSUBITEM";

        private bool _Disabled = false;
        public bool Disabled
        {
            get { return _Disabled; }
            set { _Disabled = value; }
        }

		public bool ValidateValueOnFieldUpdating
		{
			get
			{
				var selector = (INSubItemDimensionSelector)SelectorAttribute;
				return selector?.ValidateValueOnFieldUpdating ?? false;
			}
			set
			{
				var selector = (INSubItemDimensionSelector)SelectorAttribute;
				if (selector != null)
				{
					selector.ValidateValueOnFieldUpdating = value;
				}
			}
		}

		public bool ValidateValueOnPersisting
		{
			get
			{
				var selector = (INSubItemDimensionSelector)SelectorAttribute;
				return selector?.ValidateValueOnPersisting ?? false;
			}
			set
			{
				var selector = (INSubItemDimensionSelector)SelectorAttribute;
				if (selector != null)
				{
					selector.ValidateValueOnPersisting = value;
				}
			}
		}

		#endregion

		#region Ctors

		public SubItemAttribute()
			: base()	
		{
			//var attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSubItem.subItemCD));
			//attr.CacheGlobal = true;
			
			var attr = new PXDimensionSelectorAttribute(DimensionName, typeof(Search<INSubItem.subItemID, Where<Match<Current<AccessInfo.userName>>>>), typeof(INSubItem.subItemCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public SubItemAttribute(Type inventoryID)
			: this(inventoryID, null)
		{					
		}

		public SubItemAttribute(Type inventoryID, Type JoinType)
			: base()
		{
			Type SearchType =
				JoinType == null ? typeof(Search<INSubItem.subItemID, Where<Match<Current<AccessInfo.userName>>>>) :
				BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(INSubItem.subItemID),
				JoinType,
				typeof(Where<>),
				typeof(Match<>),
				typeof(Current<AccessInfo.userName>));

			var attr =
				new INSubItemDimensionSelector(inventoryID, SearchType);
				
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;	
		}

		#endregion

		#region Implementation

		public override void CacheAttached(PXCache sender)
		{
			if (_Disabled || !PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				((PXDimensionSelectorAttribute) this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).CacheGlobal = true;
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).SetSegmentDelegate(null);
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Definitions.DefaultSubItemID == null)
			{
				object newValue = DefaultSubItemValue;
				sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
				e.NewValue = newValue;
			}
			else
			{
				e.NewValue = this.Definitions.DefaultSubItemID;
			}

			e.Cancel = true;
		}

		#endregion

		#region Default SubItemID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSubItem.Definition", typeof(INSubItem)));
				}
				return defs;
			}
		}
		
		protected class Definition : IPrefetchable
		{
			private int? _DefaultSubItemID;
			public int? DefaultSubItemID
			{
				get { return _DefaultSubItemID; }
			}

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<INSubItem>(
					new PXDataField<INSubItem.subItemID>(),
					new PXDataFieldOrder<INSubItem.subItemID>()))
				{
					_DefaultSubItemID = null;
					if (record != null)
						_DefaultSubItemID = record.GetInt32(0);
				}
			}
		}
		#endregion
	}
	#endregion

	#region SiteRawAttribute

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Warehouse ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class SiteRawAttribute : AcctSubAttribute
	{
		public string DimensionName = "INSITE";
		public SiteRawAttribute(bool isTransitAllowed)
			: base()
		{
			Type SearchType = isTransitAllowed ? 
                typeof(Search<INSite.siteCD, Where<Match<Current<AccessInfo.userName>>>>) 
                : typeof(Search<INSite.siteCD, Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>, And<Match<Current<AccessInfo.userName>>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSite.siteCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region SiteAvailAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
    public class POSiteAvailAttribute : SiteAvailAttribute
	{
		#region Ctor
		public POSiteAvailAttribute(Type InventoryType, Type SubItemType)
			: base(InventoryType, SubItemType)
		{
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.RemoveHandler(sender.GetItemType(), _inventoryType.Name, InventoryID_FieldUpdated);
		}
		#endregion
		#region Implementation
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
		}
        #endregion
	}

	[PXDBInt]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
	public class SiteAvailAttribute : SiteAttribute, IPXFieldDefaultingSubscriber
	{
		private class InventoryPh : BqlPlaceholderBase { }
		private class SubItemPh : BqlPlaceholderBase { }

		#region State
		protected Type _inventoryType;
		protected Type _subItemType;
		#endregion

		#region Ctor
        public SiteAvailAttribute(Type InventoryType)
        {
            _inventoryType = InventoryType;

			Type SearchType = BqlCommand.AppendJoin<
				LeftJoin<Address, On<INSite.FK.Address>,
				LeftJoin<Country, On<Country.countryID, Equal<Address.countryID>>,
				LeftJoin<State, On<State.stateID, Equal<Address.state>>>>>
				>(Search);

			Type lookupJoin = BqlTemplate.OfJoin<
				LeftJoin<INSiteStatus,
					On<INSiteStatus.siteID, Equal<INSite.siteID>,
					And<INSiteStatus.inventoryID, Equal<Optional<InventoryPh>>>>,
				LeftJoin<INItemSiteSettings,
					On<INItemSiteSettings.siteID, Equal<INSite.siteID>,
					And<INItemSiteSettings.inventoryID, Equal<Optional<InventoryPh>>>>>>>
				.Replace<InventoryPh>(InventoryType)
				.ToType();

			Type[] colsType = { typeof(INSite.siteCD), typeof(INSiteStatus.qtyOnHand), typeof(INSite.descr), typeof(Address.addressLine1), typeof(Address.addressLine2), typeof(Address.city), typeof(Country.description), typeof(State.name) };
			_Attributes[_SelAttrIndex] = CreateSelector(SearchType, lookupJoin, colsType);
        }

		public SiteAvailAttribute(Type InventoryType, Type SubItemType)
		{
			_inventoryType = InventoryType;
			_subItemType = SubItemType;

			Type SearchType = BqlCommand.AppendJoin<
				LeftJoin<Address, On<INSite.FK.Address>>
				>(Search);

			Type lookupJoin =
				BqlTemplate.OfJoin<
					LeftJoin<INSiteStatus,
						On<INSiteStatus.siteID, Equal<INSite.siteID>,
						And<INSiteStatus.inventoryID, Equal<Optional<InventoryPh>>,
						And<INSiteStatus.subItemID, Equal<Optional<SubItemPh>>>>>,
					LeftJoin<INItemStats,
						On<INItemStats.inventoryID, Equal<Optional<InventoryPh>>,
						And<INItemStats.siteID, Equal<INSite.siteID>>>>>>
				.Replace<InventoryPh>(InventoryType)
				.Replace<SubItemPh>(SubItemType)
				.ToType();

			Type[] colsType = {typeof(INSite.siteCD), typeof(INSiteStatus.qtyOnHand), typeof(INSiteStatus.active), typeof(INSite.descr)};
			_Attributes[_SelAttrIndex] = CreateSelector(SearchType, lookupJoin, colsType);
		}

		public SiteAvailAttribute(Type InventoryType, Type SubItemType, Type[] colsType)
		{
			_inventoryType = InventoryType;
			_subItemType = SubItemType;

			Type lookupJoin = BqlTemplate.OfJoin<
				LeftJoin<INSiteStatus,
					On<INSiteStatus.siteID, Equal<INSite.siteID>,
					And<INSiteStatus.inventoryID, Equal<Optional<InventoryPh>>,
					And<INSiteStatus.subItemID, Equal<Optional<SubItemPh>>>>>>>
				.Replace<InventoryPh>(InventoryType)
				.Replace<SubItemPh>(SubItemType)
				.ToType();

			_Attributes[_SelAttrIndex] = CreateSelector(Search, lookupJoin, colsType);
		}

		private static PXDimensionSelectorAttribute CreateSelector(Type searchType, Type lookupJoin, Type[] colsType)
			=> new PXDimensionSelectorAttribute(DimensionName, searchType, lookupJoin, typeof(INSite.siteCD), true, colsType) { DescriptionField = typeof(INSite.descr) };

		private static Type Search { get; } = typeof(
			Search<INSite.siteID,
			Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
				And<Match<Current<AccessInfo.userName>>>>>);
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _inventoryType.Name, InventoryID_FieldUpdated);
		}
		#endregion

		#region Implementation
		public virtual void InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				sender.SetDefaultExt(e.Row, _FieldName);
			}
			catch (PXUnitConversionException) { }
			catch (PXSetPropertyException)
			{
				PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
				sender.SetValue(e.Row, _FieldOrdinal, null);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
            if (e.Cancel || e.Row == null)
                return;
            var inventoryID = sender.GetValue(e.Row, _inventoryType.Name);
            if (inventoryID == null)
                return;
            var inventory = InventoryItem.PK.Find(sender.Graph, (int)inventoryID);
            if (!inventory.DfltSiteID.HasValue)
                return;
            INSite site = PXSelectReadonly<INSite,
                    Where<INSite.siteID, Equal<Required<INSite.siteID>>,
                    And<Match<INSite, Current<AccessInfo.userName>>>>>
                    .Select(sender.Graph, inventory.DfltSiteID);
            if(site != null)
                e.NewValue = site.SiteID;
		}
		#endregion
	}
	#endregion

	#region SiteAttribute
	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
	[PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true, ShowWarning = true)]
	public class SiteAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INSITE";
		public bool SetDefaultValue = true;

        public class transitSiteID : PX.Data.BQL.BqlInt.Constant<transitSiteID>
        {

			public transitSiteID() : base(-1) { }

            public override int Value
            {
                get
                {
                    Definition defs = PX.Common.PXContext.GetSlot<Definition>();
                    if (defs == null)
                    {
                        defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSite.Definition", typeof(INSite), typeof(INSetup)));
                    }
                    return defs?.TransitSiteID ?? 0;
                }
            }
        }

		public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) { ;}
		}

		protected Type _whereType;

        public SiteAttribute()
            : this(false)
        {
        }

        public SiteAttribute(bool allowTransit)
			: this(typeof(Where<Match<Current<AccessInfo.userName>>>), false, allowTransit)
		{
		}
		public SiteAttribute(Type WhereType, bool allowTransit)
			: this(WhereType, true, allowTransit)
		{			
		}

		public SiteAttribute(Type WhereType, bool validateAccess, bool allowTransit)
		{			
			if (WhereType != null)
			{
				_whereType = WhereType;

                List<Type> bql = new List<Type>();

				if (validateAccess)
                {
                    bql.Add(typeof(Search<,>));
                    bql.Add(typeof(INSite.siteID));
                    bql.Add(typeof(Where2<,>));
                    bql.Add(typeof(Match<>));
                    bql.Add(typeof(Current<AccessInfo.userName>));
                    if(allowTransit)
                    {
                        bql.Add(typeof(And<>));
                    }
                    else
                    {
                        bql.Add(typeof(And<,,>));
                        bql.Add(typeof(INSite.siteID));
                        bql.Add(typeof(NotEqual<transitSiteID>));
						bql.Add(typeof(And<>));
					}
                    bql.Add(_whereType);
                }
                else
                {
                    bql.Add(typeof(Search<,>));
                    bql.Add(typeof(INSite.siteID));
                    if(!allowTransit)
                    {
                        bql.Add(typeof(Where2<,>));
                        bql.Add(typeof(Where<,>));
                        bql.Add(typeof(INSite.siteID));
                        bql.Add(typeof(NotEqual<transitSiteID>));
                        bql.Add(typeof(And<>));
                    }
                    bql.Add(_whereType);
                }

                Type SearchType = BqlCommand.Compose(bql.ToArray());

                PXDimensionSelectorAttribute attr;
				_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSite.siteCD), 
                    new Type[]
                    {
                        typeof (INSite.siteCD),typeof (INSite.descr)
                    }));
				attr.CacheGlobal = true;
				attr.DescriptionField = typeof(INSite.descr);
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}

		#region Implemetation
		public override void CacheAttached(PXCache sender)
		{
			if (SetDefaultValue && (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && PXAccess.FeatureInstalled<FeaturesSet.inventory>()) && sender.Graph.GetType() != typeof(PXGraph))
			{
				if (Definitions.DefaultSiteID == null)
				{
					((PXDimensionSelectorAttribute)this._Attributes[_SelAttrIndex]).ValidComboRequired = false;
				}
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				if (Definitions.DefaultSiteID == null)
				{
					object newValue = INSite.Main;
					sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
					e.NewValue = newValue;
				}
				else
				{
					e.NewValue = Definitions.DefaultSiteID;
				}

				e.Cancel = true;
			}
		}
        #endregion

        #region Default SiteID
        protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSite.Definition", typeof(INSite), typeof(INSetup)));
				}
				return defs;
			}
		}

		public class Definition : IPrefetchable
		{
			private int? _DefaultSiteID;
			public int? DefaultSiteID
			{
				get { return _DefaultSiteID; }
			}

            private int? _TransitSiteID;
            public int? TransitSiteID
            {
                get { return _TransitSiteID; }
            }

            public void Prefetch()
			{
                using (PXDataRecord record = PXDatabase.SelectSingle<INSetup>(
                    new PXDataField<INSetup.transitSiteID>()))
                {
                    _TransitSiteID = -1;
                    if (record != null)
                        _TransitSiteID = record.GetInt32(0);
                }

                var dflst = new List<PXDataField>();                
                dflst.Add(new PXDataField<INSite.siteID>());
				if (_TransitSiteID != null)
				{
                dflst.Add(new PXDataFieldValue("SiteID", PXDbType.Int, 4, _TransitSiteID, PXComp.NE));
				}
				dflst.Add(new PXDataFieldValue<INSite.active>(true));
                dflst.Add(new PXDataFieldOrder<INSite.siteID>());
                
                using (PXDataRecord record = PXDatabase.SelectSingle<INSite>(dflst.ToArray()))
                {
                    _DefaultSiteID = null;
                    if (record != null)
                        _DefaultSiteID = record.GetInt32(0);
                }
            }
		}
		#endregion

	}

    [PXDBInt()]
    [PXUIField(DisplayName = "To Site ID", Visibility = PXUIVisibility.Visible)]
    public class ToSiteAttribute : SiteAttribute
	{
		protected class BranchScopeDimensionSelector : PXDimensionSelectorAttribute
		{
			private int InterbranchRestrictorAttributeId = -1;
			protected BqlCommand _BranchScopeCondition;

			protected PXRestrictorAttribute InterbranchRestrictor => (PXRestrictorAttribute)_Attributes[InterbranchRestrictorAttributeId];

			public BranchScopeDimensionSelector(Type restrictionBranchId, string dimension, Type type, Type substituteKey, BqlCommand branchScopeCondition, params Type[] fieldList)
				: base(dimension, type, substituteKey, fieldList)
			{
				_BranchScopeCondition = branchScopeCondition;

				var interBranchRestrictionCondition = BqlTemplate.OfCondition<
					Where<SameOrganizationBranch<INSite.branchID, Current<BqlPlaceholder.A>>>>
					.Replace<BqlPlaceholder.A>(restrictionBranchId)
					.ToType();

				_Attributes.Add(new InterBranchRestrictorAttribute(interBranchRestrictionCondition));
				InterbranchRestrictorAttributeId = _Attributes.Count - 1;
			}

			public BranchScopeDimensionSelector(string dimension, Type type, Type substituteKey, BqlCommand branchScopeCondition, params Type[] fieldList)
				: this(typeof(AccessInfo.branchID), dimension, type, substituteKey, branchScopeCondition, fieldList)
			{
			}

			public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (_BranchScopeCondition.Meet(sender, sender.Current))
				{
					using (new PXReadBranchRestrictedScope())
					{
						base.FieldVerifying(sender, e);
					}
				}
				else
				{
					base.FieldVerifying(sender, e);
				}
			}

			public void SelectorFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (_BranchScopeCondition.Meet(sender, sender.Current))
				{
					using (new PXReadBranchRestrictedScope())
					{
						SelectorAttribute.FieldVerifying(sender, e);
						InterbranchRestrictor.FieldVerifying(sender, e);
					}
					e.Cancel = true;
				}
			}

			protected bool RaiseFieldSelectingInternal(PXCache sender, object row, ref object returnValue, bool forceState)
			{
				var args = new PXFieldSelectingEventArgs(null, null, true, true);
				this.FieldSelecting(sender, args);
				returnValue = args.ReturnState;
				return !args.Cancel;
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				sender.Graph.FieldVerifying.AddHandler(BqlTable, _FieldName, SelectorFieldVerifying);
				sender.Graph.FieldUpdating.RemoveHandler(BqlTable, _FieldName, base.FieldUpdating);
				sender.Graph.FieldUpdating.AddHandler(BqlTable, _FieldName, this.FieldUpdating);

				object state = null;
				RaiseFieldSelectingInternal(sender, null, ref state, true);

				string viewName = ((PX.Data.PXFieldState)state).ViewName;
				PXView view = sender.Graph.Views[viewName];

				PXView outerview = new PXView(sender.Graph, true, view.BqlSelect);
				view = sender.Graph.Views[viewName] = new PXView(sender.Graph, true, view.BqlSelect, (PXSelectDelegate)delegate()
				{
					int startRow = PXView.StartRow;
					int totalRows = 0;
					List<object> res;

					if (_BranchScopeCondition.Meet(sender, sender.Current))
					{
						using (new PXReadBranchRestrictedScope())
						{
							res = outerview.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
							PXView.StartRow = 0;
						}
					}
					else
					{
						res = outerview.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
						PXView.StartRow = 0;
					}

					return res;

				});

				if (_DirtyRead)
				{
					view.IsReadOnly = false;
				}
			}

			public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				if (_BranchScopeCondition.Meet(sender, sender.Current))
				{
					using (new PXReadBranchRestrictedScope())
					{
						base.FieldUpdating(sender, e);
					}
				}
				else
				{
					base.FieldUpdating(sender, e);
				}
			}

		}


		public ToSiteAttribute()
			: this(typeof(INTransferType.twoStep), typeof(AccessInfo.branchID))
		{			
		}

		public ToSiteAttribute(Type transferTypeField)
			: this(transferTypeField, typeof(AccessInfo.branchID))
		{
		}

		public ToSiteAttribute(Type transferTypeField, Type restrictionBranchId)
		{
			BranchScopeDimensionSelector selectorAttr = PrepareSelectorAttr(transferTypeField, restrictionBranchId);
			_Attributes[_SelAttrIndex] = selectorAttr;
			selectorAttr.CacheGlobal = true;
			selectorAttr.DescriptionField = typeof(INSite.descr);
		}

		private BranchScopeDimensionSelector PrepareSelectorAttr(Type transferTypeField, Type restrictionBranchId)
		{
			Type selectorType = BqlTemplate.OfCommand<Search<INSite.siteID,
				Where2<Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>, And<Match<Current<AccessInfo.userName>>>>,
					Or<BqlPlaceholder.A, Equal<INTransferType.twoStep>, And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>>>>>
						.Replace<BqlPlaceholder.A>(typeof(IBqlField).IsAssignableFrom(transferTypeField)
							? typeof(Current<>).MakeGenericType(transferTypeField)
							: transferTypeField)
						.ToType();

			var branchScopeCondition = BqlTemplate.OfCommand<
				Select<INRegister,
				Where<BqlPlaceholder.A, Equal<INTransferType.twoStep>>>>
				.Replace<BqlPlaceholder.A>(transferTypeField).ToCommand();

			return new BranchScopeDimensionSelector(
				restrictionBranchId,
				DimensionName,
				selectorType,
				typeof(INSite.siteCD),
				branchScopeCondition,
				new Type[] { typeof(INSite.siteCD), typeof(INSite.descr) });
		}
	}

	/// <summary>
	/// Version of <see cref="SiteAttribute"/> that does not create default Warehouse if there are no warehouses
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
	[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
	public class NullableSiteAttribute : SiteAttribute
	{
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldDefaulting.RemoveHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
		}
		#endregion
	}
	#endregion

	#region ItemSiteAttribute
	
	public class ItemSiteAttribute : PXSelectorAttribute
	{
		public ItemSiteAttribute()
			:base(typeof(Search2<INItemSite.siteID,				
				InnerJoin<INSite, On<INSite.siteID, Equal<INItemSite.siteID>,
				      And<Where<CurrentMatch<INSite, AccessInfo.userName>>>>>,
				Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>>>))
		{
			this._SubstituteKey = typeof (INSite.siteCD);			
			this._UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteID, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>));
			this._NaturalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteCD, Where<INSite.siteCD, Equal<Required<INSite.siteCD>>>>));
		}
		public override void SubstituteKeyCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
            if (ShouldPrepareCommandForSubstituteKey(e))
            {
				e.Cancel = true;
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(_FieldName))
				{
					if (attr is PXDBFieldAttribute)
					{
						SimpleTable siteExt = new SimpleTable<INSite>(_Type.Name+"Ext");

						var siteQuery = new Query()
							.Select(siteExt.Column(this._SubstituteKey))
							.From(siteExt)
							.Where(siteExt.Column<INSite.siteID>()
								.EQ(new Column(((PXDBFieldAttribute)attr).DatabaseFieldName,
									e.Table ?? _BqlTable)));

						e.Expr = new SubQuery(siteQuery).Embrace();


						if (e.Value != null)
						{
							e.DataValue = e.Value;
							e.DataType = PXDbType.NVarChar;
							e.DataLength = ((string)e.Value).Length;
						}
						break;
					}
				}
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}
	}
	#endregion

	#region ReplenishmentSourceSiteAttribute
	public class ReplenishmentSourceSiteAttribute : SiteAttribute
	{
		public ReplenishmentSourceSiteAttribute(Type replenishmentSource)
			:base()
		{
			DescriptionField = typeof (INSite.descr);
			this.source = replenishmentSource;
		}
		private Type source;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowSelected.AddHandler(sender.GetItemType(), OnRowSelected);
			sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), OnRowUpdated);
		}
		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName,
				e.Row != null &&
				INReplenishmentSource.IsTransfer((string)sender.GetValue(e.Row, this.source.Name)) );
		}
		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			if (!INReplenishmentSource.IsTransfer((string)sender.GetValue(e.Row, this.source.Name)))
				sender.SetValue(e.Row, _FieldName, null);				
		}		

	}
	#endregion

	#region LocationRawAttribute

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class LocationRawAttribute : AcctSubAttribute
	{
		public string DimensionName = "INLOCATION";
		public LocationRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(DimensionName);
			attr.ValidComboRequired = false;

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region LocationAvailAttribute

    public class LocationRestrictorAttribute : PXRestrictorAttribute
    {
        protected Type _IsReceiptType;
        protected Type _IsSalesType;
        protected Type _IsTransferType;

        public LocationRestrictorAttribute(Type IsReceiptType, Type IsSalesType, Type IsTransferType)
            : base(typeof(Where<True>), string.Empty)
        {
            _IsReceiptType = IsReceiptType;
            _IsSalesType = IsSalesType;
            _IsTransferType = IsTransferType;
        }

        protected override BqlCommand WhereAnd(PXCache sender, PXSelectorAttribute selattr, Type Where)
        {
            return selattr.PrimarySelect.WhereAnd(Where);
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INLocation location = null;
            try
            {
				location = INLocation.PK.Find(sender.Graph, (int?)e.NewValue);
            }
            catch (FormatException) { }

            if (_AlteredCmd != null && location != null)
            {
                bool? IsReceipt = VerifyExpr(sender, e.Row, _IsReceiptType);
                bool? IsSales = VerifyExpr(sender, e.Row, _IsSalesType);
                bool? IsTransfer = VerifyExpr(sender, e.Row, _IsTransferType);

                if (IsReceipt == true && location.ReceiptsValid == false)
                {
                    ThrowErrorItem(Messages.LocationReceiptsInvalid, e, location.LocationCD);
                }

                if (IsSales == true)
                {
                    if (location.SalesValid == false && (e.ExternalCall || location.IsSorting == false))
                    {
                        ThrowErrorItem(Messages.LocationSalesInvalid, e, location.LocationCD);
                    }
                }

                if (IsTransfer == true)
                {
                    if (location.TransfersValid == false && (e.ExternalCall || location.IsSorting == false))
                    {
                        ThrowErrorItem(Messages.LocationTransfersInvalid, e, location.LocationCD);
                    }
                }
            }
        }

        public virtual void ThrowErrorItem(string message, PXFieldVerifyingEventArgs e, object ErrorValue)
        {
            e.NewValue = ErrorValue;
            throw new PXSetPropertyException(message);
        }

        protected bool? VerifyExpr(PXCache cache, object data, Type whereType)
        {
            object value = null;
            bool? ret = null;
            IBqlWhere where = (IBqlWhere)Activator.CreateInstance(whereType);
            where.Verify(cache, data, new List<object>(), ref ret, ref value);

            return ret;
        }
    }

    public class PrimaryItemRestrictorAttribute : PXRestrictorAttribute
    {
        public bool IsWarning;

        protected Type _InventoryType;
        protected Type _IsReceiptType;
        protected Type _IsSalesType;
        protected Type _IsTransferType;

        public PrimaryItemRestrictorAttribute(Type InventoryType, Type IsReceiptType, Type IsSalesType, Type IsTransferType)
            : base(typeof(Where<True>), string.Empty)
        {
            _InventoryType = InventoryType;
            _IsReceiptType = IsReceiptType;
            _IsSalesType = IsSalesType;
            _IsTransferType = IsTransferType;
        }

        protected override BqlCommand WhereAnd(PXCache sender, PXSelectorAttribute selattr, Type Where)
        {
            return selattr.PrimarySelect.WhereAnd(Where);
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INLocation location = null;
            try
            {
                location = INLocation.PK.Find(sender.Graph, (int?)e.NewValue);
            }
            catch (FormatException) { }

            if (_AlteredCmd != null && location != null && location.PrimaryItemValid != INPrimaryItemValid.PrimaryNothing)
            {
                bool? IsReceipt = VerifyExpr(sender, e.Row, _IsReceiptType);
                bool? IsSales = VerifyExpr(sender, e.Row, _IsSalesType);
                bool? IsTransfer = VerifyExpr(sender, e.Row, _IsTransferType);

                if (IsReceipt == true || IsTransfer == true)
                {
					var ItemID = (int?)sender.GetValue(e.Row, _InventoryType.Name);
					if (ItemID == null)
						return;
					InventoryItem item;

					switch (location.PrimaryItemValid)
                    {
                        case INPrimaryItemValid.PrimaryItemError:
                            if (Equals(ItemID, location.PrimaryItemID) == false)
                            {
                                ThrowErrorItem(Messages.NotPrimaryLocation, e, location.LocationCD);
                            }
                            break;
                        case INPrimaryItemValid.PrimaryItemClassError:
                            item = InventoryItem.PK.Find(sender.Graph, ItemID);
                            if (item != null && item.ItemClassID != location.PrimaryItemClassID)
                            {
                                ThrowErrorItem(Messages.NotPrimaryLocation, e, location.LocationCD);
                            }
                            break;
                        case INPrimaryItemValid.PrimaryItemWarning:
                            if (Equals(ItemID, location.PrimaryItemID) == false)
                            {
                                sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, new PXSetPropertyException(Messages.NotPrimaryLocation, PXErrorLevel.Warning));
                            }
                            break;
                        case INPrimaryItemValid.PrimaryItemClassWarning:
                            item = InventoryItem.PK.Find(sender.Graph, ItemID);
                            if (item != null && item.ItemClassID != location.PrimaryItemClassID)
                            {
                                sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, new PXSetPropertyException(Messages.NotPrimaryLocation, PXErrorLevel.Warning));
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public virtual void ThrowErrorItem(string message, PXFieldVerifyingEventArgs e, object ErrorValue)
        {
            e.NewValue = ErrorValue;
            throw new PXSetPropertyException(message);
        }

        protected bool? VerifyExpr(PXCache cache, object data, Type whereType)
        {
            object value = null;
            bool? ret = null;
            IBqlWhere where = (IBqlWhere)Activator.CreateInstance(whereType);
            where.Verify(cache, data, new List<object>(), ref ret, ref value);

            return ret;
        }
    }

	[PXDBInt()]
    [PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.Visible, FieldClass = LocationAttribute.DimensionName)]
    [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
	public class LocationAvailAttribute : LocationAttribute, IPXFieldDefaultingSubscriber
	{
		private class InventoryPh : BqlPlaceholderBase { }
		private class SubItemPh : BqlPlaceholderBase { }
		private class SiteIDPh : BqlPlaceholderBase { }

		#region State
		protected Type _InventoryType;
		protected Type _IsSalesType;
		protected Type _IsReceiptType;
		protected Type _IsTransferType;
		protected Type _IsStandardCostAdjType;
		protected BqlCommand _Select;
		#endregion

		#region Ctor
        
        public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, bool IsSalesType, bool IsReceiptType, bool IsTransferType)
			: this(InventoryType, SubItemType, SiteIDType, null, null, null)
		{
			_IsSalesType = IsSalesType ? typeof(Where<True>) : typeof(Where<False>);
			_IsReceiptType = IsReceiptType ? typeof(Where<True>) : typeof(Where<False>);
			_IsTransferType = IsTransferType ? typeof(Where<True>) : typeof(Where<False>);
			_IsStandardCostAdjType = typeof(Where<False>);

            this._Attributes.Add(new PrimaryItemRestrictorAttribute(InventoryType, _IsReceiptType, _IsSalesType, _IsTransferType));
            this._Attributes.Add(new LocationRestrictorAttribute(_IsReceiptType, _IsSalesType, _IsTransferType));
		}

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type TranType, Type InvtMultType)
			: this(InventoryType, SubItemType, SiteIDType, TranType, InvtMultType, true)
		{
		}

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type TranType, Type InvtMultType, bool VerifyAllowedOperations)
			: this(InventoryType, SubItemType, SiteIDType, null, null, null)
		{
			_IsSalesType = BqlCommand.Compose(typeof(Where<,>), TranType, typeof(In3<INTranType.invoice, INTranType.debitMemo>));
			_IsReceiptType = BqlCommand.Compose(typeof(Where<,>), TranType, typeof(In3<INTranType.receipt, INTranType.issue, INTranType.return_, INTranType.creditMemo>));
			_IsTransferType = BqlCommand.Compose(typeof(Where<,,>), TranType, typeof(Equal<INTranType.transfer>), typeof(And<,>), InvtMultType, typeof(In3<short1, shortMinus1>));
			_IsStandardCostAdjType = BqlCommand.Compose(typeof(Where<,>), TranType, typeof(In3<INTranType.standardCostAdjustment, INTranType.negativeCostAdjustment>));

            this._Attributes.Add(new PrimaryItemRestrictorAttribute(InventoryType, _IsReceiptType, _IsSalesType, _IsTransferType));

			if (VerifyAllowedOperations)
            this._Attributes.Add(new LocationRestrictorAttribute(_IsReceiptType, _IsSalesType, _IsTransferType));
        }

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type IsSalesType, Type IsReceiptType, Type IsTransferType)
			: base(SiteIDType)
		{
			_InventoryType = InventoryType;
			_IsSalesType = IsSalesType;
			_IsReceiptType = IsReceiptType;
			_IsTransferType = IsTransferType;
			_IsStandardCostAdjType = typeof(Where<False>);

			Type search = BqlTemplate.OfCommand<
				Search<INLocation.locationID,
				Where<INLocation.siteID, Equal<Optional<SiteIDPh>>>>>
				.Replace<SiteIDPh>(SiteIDType)
				.ToType();

			Type lookupJoin = BqlTemplate.OfJoin<
				LeftJoin<INLocationStatus,
					On<INLocationStatus.locationID, Equal<INLocation.locationID>,
					And<INLocationStatus.inventoryID, Equal<Optional<InventoryPh>>,
					And<INLocationStatus.subItemID, Equal<Optional<SubItemPh>>>>>>>
				.Replace<InventoryPh>(InventoryType)
				.Replace<SubItemPh>(SubItemType)
				.ToType();

			Type[] fieldList =
			{
				typeof(INLocation.locationCD),
				typeof(INLocationStatus.qtyOnHand),
				typeof(INLocationStatus.active),
				typeof(INLocation.primaryItemID),
				typeof(INLocation.primaryItemClassID),
				typeof(INLocation.receiptsValid),
				typeof(INLocation.salesValid),
				typeof(INLocation.transfersValid),
				typeof(INLocation.projectID),
				typeof(INLocation.taskID)
			};
			var attr = new LocationDimensionSelectorAttribute(search, GetSiteIDKeyRelation(SiteIDType), lookupJoin, fieldList);
			_Attributes[_SelAttrIndex] = attr;

			_Select = BqlTemplate.OfCommand<
				Select<INItemSite,
				Where<INItemSite.inventoryID, Equal<Current<InventoryPh>>,
					And<INItemSite.siteID, Equal<Current2<SiteIDPh>>>>>>
				.Replace<InventoryPh>(_InventoryType)
				.Replace<SiteIDPh>(_SiteIDType)
				.ToCommand();

            if (IsReceiptType != null && IsSalesType != null && IsTransferType != null)
            {
                this._Attributes.Add(new PrimaryItemRestrictorAttribute(InventoryType, IsReceiptType, IsSalesType, IsTransferType));
                this._Attributes.Add(new LocationRestrictorAttribute(IsReceiptType, IsSalesType, IsTransferType));
            }
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			//sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _SiteIDType.Name, SiteID_FieldUpdated);
		}

		#endregion

		#region Implementation
		public override void SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) != null)
			{
				base.SiteID_FieldUpdated(sender, e);

				object locationid;
				if ((locationid = sender.GetValue(e.Row, _FieldOrdinal)) != null && (int?)locationid > 0)
					return;
					PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
			}

					try
					{
				if (e.ExternalCall)
				{
					//SetValuePending are works only for IDictionary as first 'data' parameter
					sender.SetValuePending(e.Row, _FieldName, PXCache.NotSetValue);
						sender.SetDefaultExt(e.Row, _FieldName);
					}
				else
				{
					object newValue;
					sender.RaiseFieldDefaulting(_FieldName, e.Row, out newValue);
					if (newValue != null)
						sender.SetValueExt(e.Row, _FieldName, newValue);
				}
			}
					catch (PXSetPropertyException)
					{
						PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
						sender.SetValue(e.Row, _FieldOrdinal, null);
					}
				}

		protected bool? VerifyExpr(PXCache cache, object data, Type whereType)
		{
			if(whereType == null)
			{
				return false;
			}

			object value = null;
			bool? ret = null;
			IBqlWhere where = (IBqlWhere)Activator.CreateInstance(whereType);
			where.Verify(cache, data, new List<object>(), ref ret, ref value);

			return ret;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			bool IsStandardCostAdj = (bool)VerifyExpr(sender, e.Row, _IsStandardCostAdjType);

			if (IsStandardCostAdj)
			{
				e.NewValue = null;
				e.Cancel = true;
				return;
			}
			
			PXView view = sender.Graph.TypedViews.GetView(_Select, false);

			var itemsite = (INItemSite)view.SelectSingleBound(new object[] { e.Row });

			if (!UpdateDefault<INItemSite.dfltReceiptLocationID, INItemSite.dfltShipLocationID>(sender, e, itemsite))
			{
				var insite = (INSite)PXSelectorAttribute.Select(sender, e.Row, _SiteIDType.Name);
				UpdateDefault<INSite.receiptLocationID, INSite.shipLocationID>(sender, e, insite);
			}
		}

		private bool UpdateDefault<ReceiptLocationID, ShipLocationID>(PXCache sender, PXFieldDefaultingEventArgs e,
		                                                              object source)
			where ReceiptLocationID : IBqlField
			where	ShipLocationID : IBqlField
		{
			if(source == null) return false;
			PXCache cache = sender.Graph.Caches[source.GetType()];

			if(cache.Keys.Exists(key => cache.GetValue(source, key) == null)) return false;				

			bool IsReceipt = (bool)VerifyExpr(sender, e.Row, _IsReceiptType);			
			
			object newvalue = (IsReceipt) ? cache.GetValue<ReceiptLocationID>(source) : cache.GetValue<ShipLocationID>(source);
			object val = (IsReceipt) ? cache.GetValueExt<ReceiptLocationID>(source) : cache.GetValueExt<ShipLocationID>(source);

			if (val is PXFieldState)
			{
				e.NewValue = ((PXFieldState)val).Value;
			}
			else
			{
				e.NewValue = val;
			}

			try
			{
				sender.RaiseFieldVerifying(_FieldName, e.Row, ref newvalue);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = null;
			}
			return true;
		}

		#endregion
	}

	#endregion

	#region LocationAttribute

    public interface IFeatureAccessProvider
    {
        bool IsFeatureInstalled<TFeature>();
    }


	[PXDBInt()]
    [PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.Visible, FieldClass = LocationAttribute.DimensionName)]
    public class LocationAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INLOCATION";

		public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) { ;}
		}

		protected Type _SiteIDType;

		protected bool _KeepEntry = true;
		protected bool _ResetEntry = true;

		public bool KeepEntry
		{
			get
			{
				return this._KeepEntry;
			}
			set
			{
				this._KeepEntry = value;
			}
		}

		public bool ResetEntry
		{
			get
			{
				return this._ResetEntry;
			}
			set
			{
				this._ResetEntry = value;
			}
		}

		public LocationAttribute()
			: base()
		{
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, typeof(Search<INLocation.locationID>), typeof(INLocation.locationCD))
			{
				CacheGlobal = true,
				DescriptionField = typeof(INLocation.descr)
			};
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public LocationAttribute(Type SiteIDType)
			: base()
		{
			_SiteIDType = SiteIDType ?? throw new PXArgumentException(nameof(SiteIDType), ErrorMessages.ArgumentNullException);

			Type search = BqlTemplate.OfCommand<
					Search<INLocation.locationID,
					Where<INLocation.siteID, Equal<Optional<BqlPlaceholder.A>>>>>
				.Replace<BqlPlaceholder.A>(_SiteIDType)
				.ToType();

			var attr = new LocationDimensionSelectorAttribute(search, GetSiteIDKeyRelation(SiteIDType));
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		protected Type GetSiteIDKeyRelation(Type siteIDField) => typeof(Field<>.IsRelatedTo<>).MakeGenericType(siteIDField, typeof(INLocation.siteID));

        public bool IsWarehouseLocationEnabled(PXCache sender)
        {
            return ((sender.Graph is IFeatureAccessProvider && ((IFeatureAccessProvider)sender.Graph).IsFeatureInstalled<FeaturesSet.warehouseLocation>())
                ||
                (!(sender.Graph is IFeatureAccessProvider) && PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
                );
        }

		public override void CacheAttached(PXCache sender)
		{
			if (_SiteIDType != null && !IsWarehouseLocationEnabled(sender)
                && sender.Graph.GetType() != typeof(PXGraph))
			{
				((PXDimensionSelectorAttribute)this._Attributes[_SelAttrIndex]).ValidComboRequired = false;

				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
				sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldUpdating);
				sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating);

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && sender.GetItemType() == typeof(IN.INSite))
				{
					_JustPersisted = new Dictionary<int?, int?>();
					sender.Graph.RowPersisting.AddHandler<INLocation>(Feature_RowPersisting);
					sender.Graph.RowPersisted.AddHandler<INLocation>(Feature_RowPersisted);
				}

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && !sender.Graph.Views.Caches.Contains(typeof(INSite)))
				{
					sender.Graph.Views.Caches.Add(typeof(INSite));
				}

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && !sender.Graph.Views.Caches.Contains(typeof(INLocation)))
				{
					sender.Graph.Views.Caches.Add(typeof(INLocation));
				}
			}

			base.CacheAttached(sender);

			if (_SiteIDType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _SiteIDType.Name, SiteID_FieldUpdated);

				if (IsWarehouseLocationEnabled(sender))
				{
					string name = _FieldName.ToLower();
					sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), name, FieldUpdating);
					sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), name, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating);

					sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), name, FieldSelecting);
					sender.Graph.FieldSelecting.RemoveHandler(sender.GetItemType(), name, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldSelecting);

					PXDimensionSelectorAttribute.SetValidCombo(sender, name, false);
				}
			}
		}

		protected Dictionary<Int32?, Int32?> _JustPersisted;

		public virtual void Feature_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Int32? _KeyToAbort = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _KeyToAbort < 0)
			{ 
				PXCache cache = sender.Graph.Caches[typeof(INSite)];
				INSite record = ((IEnumerable<INSite>)cache.Inserted).First();

				sender.SetValue(e.Row, _SiteIDType.Name, record.SiteID);

				_JustPersisted.Add(record.SiteID, _KeyToAbort);
			}
		}

		public virtual void Feature_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Int32? _NewKey = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				Int32? _KeyToAbort;
				if (_JustPersisted.TryGetValue(_NewKey, out _KeyToAbort))
				{
					sender.SetValue(e.Row, _SiteIDType.Name, _KeyToAbort);
				}
			}
		}

		public virtual void Feature_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null || e.Cancel == true) return;

			Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
			PXCache sitecache = sender.Graph.Caches[typeof(INSite)];
			INSite _current;
			if ((_current = INSite.PK.Find(sender.Graph, siteval)) == null)
			{
				_current = (object.ReferenceEquals(sitecache, sender) ? e.Row : siteval == null ? null : ((IEnumerable<INSite>)sitecache.Inserted).First(a => a.SiteID == siteval)) as INSite;
			}

			PXFieldUpdating fu = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating;

			PXFieldDefaulting siteid_fielddefaulting = (cache, args) =>
			{
				INLocation row = args.Row as INLocation;
				if (row != null && _current != null)
				{
					args.NewValue = _current.SiteID;
					args.Cancel = true;
				}
			};

			var dummy_cache = sender.Graph.Caches<INLocation>();
			sender.Graph.FieldDefaulting.AddHandler<INLocation.siteID>(siteid_fielddefaulting);

			try
			{
				fu(sender, e);
			}
			finally
			{
				sender.Graph.FieldDefaulting.RemoveHandler<INLocation.siteID>(siteid_fielddefaulting);
			}
		}

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
				object newValue = null;
				if (siteval != null)
				{

					if (!this.Definitions.DefaultLocations.TryGetValue(siteval, out newValue))
					{
						try
						{
							newValue = INLocation.Main;
							sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
						}
						catch (InvalidOperationException)
						{
						}
					}
				}
				e.NewValue = newValue;
				e.Cancel = true;
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Cancel) return;

			PXDimensionSelectorAttribute attr = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]);
			attr.DirtyRead = true;
			attr.FieldSelecting(sender, e, attr.SuppressViewCreation, true);

			var state = e.ReturnState as PXSegmentedState;
			if (state != null)
			{
				state.ValidCombos = true;
			}

			if ((int?)sender.GetValue(e.Row, _FieldOrdinal) < 0)
			{
				Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
				PXCache cache = sender.Graph.Caches[typeof(INSite)];
				INSite site = INSite.PK.Find(sender.Graph, siteval);

				if ((string)cache.GetValue<INSite.locationValid>(site) == INLocationValid.Warn)
				{
					sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyException(ErrorMessages.ElementDoesntExist, PXErrorLevel.Warning, Messages.Location));
				}
			}
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null || e.Cancel == true) return;

			Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
			PXCache sitecache = sender.Graph.Caches[typeof(INSite)];
			INSite _current = INSite.PK.Find(sender.Graph, siteval);
			if (_current == null)
			{
				_current = sitecache.Current as INSite;
			}

			PXFieldUpdating fu = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating;

			PXFieldDefaulting siteid_fielddefaulting = (cache, args) =>
				{
					INLocation row = args.Row as INLocation;
					if (row != null && _current != null)
					{
						args.NewValue = _current.SiteID;
						args.Cancel = true;
					}
				};

			PXRowInserting location_inserting = (cache, args) =>
				{
					INLocation row = args.Row as INLocation;
					if (row != null)
					{
						if (_current == null || _current.LocationValid == INLocationValid.Validate)
						{
							args.Cancel = true;
						}
					}
				};

			sender.Graph.RowInserting.AddHandler<INLocation>(location_inserting);
			sender.Graph.FieldDefaulting.AddHandler<INLocation.siteID>(siteid_fielddefaulting);

			try
			{
				fu(sender, e);
			}
			catch (PXSetPropertyException ex)
			{
				ex.ErrorValue = e.NewValue;
				throw;
			}
			finally
			{
				sender.Graph.RowInserting.RemoveHandler<INLocation>(location_inserting);
				sender.Graph.FieldDefaulting.RemoveHandler<INLocation.siteID>(siteid_fielddefaulting);
			}
		}

		public virtual void SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_KeepEntry)
			{
				object val = sender.GetValueExt(e.Row, _FieldName);
				sender.SetValue(e.Row, _FieldOrdinal, null);


				PXRowInserting location_inserting = (cache, args) =>
				{
					args.Cancel = true;
				};

				sender.Graph.RowInserting.AddHandler<INLocation>(location_inserting);

				try
				{
					object newval = val is PXFieldState ? ((PXFieldState)val).Value : val;
					sender.SetValueExt(e.Row, _FieldName, newval);
				}
				catch (PXException) { }
				finally
				{
					sender.Graph.RowInserting.RemoveHandler<INLocation>(location_inserting);
				}
			}
			else if (_ResetEntry)
			{
				sender.SetValueExt(e.Row, _FieldName, null);							 
			}
		}

		#region Default LocationID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INLocation.Definition", typeof(INLocation)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			public Dictionary<int?, object> DefaultLocations;

			public void Prefetch()
			{
				DefaultLocations = new Dictionary<int?, object>(); 
				
				foreach (PXDataRecord record in PXDatabase.SelectMulti<INLocation>(
					new PXDataField<INLocation.siteID>(),
					new PXDataField<INLocation.locationID>(),
					new PXDataFieldOrder<INLocation.siteID>(),
					new PXDataFieldOrder<INLocation.locationID>()))
				{
					int? siteID = record.GetInt32(0);

					if (!DefaultLocations.ContainsKey(siteID))
					{
						DefaultLocations.Add(siteID, record.GetInt32(1));
					}
				}
			}
		}
		#endregion

		#region Location Selector and Dimension attributes
		public class LocationSelectorAttribute : PXSelectorAttribute.WithCachingByCompositeKeyAttribute
		{
			public LocationSelectorAttribute(Type search, Type additionalKeysRelation)
				: base(search, additionalKeysRelation)
			{
				SubstituteKey = typeof(INLocation.locationCD);
			}

			public LocationSelectorAttribute(Type search, Type additionalKeysRelation, Type lookupJoin, Type[] fieldList)
				: base(search, additionalKeysRelation, lookupJoin, fieldList)
			{
				SubstituteKey = typeof(INLocation.locationCD);
			}

			protected override void OnItemCached(PXCache foreignCache, object foreignItem, bool isItemDeleted)
			{
				base.OnItemCached(foreignCache, foreignItem, isItemDeleted);
				if(!isItemDeleted)
					INLocation.PK.PutToCache(foreignCache.Graph, (INLocation)foreignItem);
			}
		}

		public class LocationDimensionSelectorAttribute: PXDimensionSelector.WithCachingByCompositeKeyAttribute
		{
			public LocationDimensionSelectorAttribute(Type search, Type additionalKeysRelation) : base(DimensionName)
			{
				Initialize(new LocationSelectorAttribute(search, additionalKeysRelation));
			}

			public LocationDimensionSelectorAttribute(Type search, Type additionalKeysRelation, Type lookupJoin, Type[] fieldList) : base(DimensionName)
			{
				Initialize(new LocationSelectorAttribute(search, additionalKeysRelation, lookupJoin, fieldList));
				DirtyRead = true;
			}

			private void Initialize(LocationSelectorAttribute locationSelectorAttribute)
			{
				RegisterSelector(locationSelectorAttribute);
				ValidComboRequired = true;
				OnlyKeyConditions = true;
				DescriptionField = typeof(INLocation.descr);
			}
		}
		#endregion


	}

	#endregion

	#region PXDBPriceCostAttribute

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBPriceCostAttribute : PXDBDecimalAttribute
	{
		protected bool _keepNullValue;
		#region Implementation
		public static decimal Round(decimal value)
		{
			return (decimal)Math.Round(value, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (!_keepNullValue && sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
		#endregion
		#region Initialization
		public PXDBPriceCostAttribute()
			: this(false)
		{
		}

		public PXDBPriceCostAttribute(bool keepNullValue)
		{
			_keepNullValue = keepNullValue;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_Precision = CommonSetupDecPl.PrcCst;
		}
		#endregion
	}

	#endregion

	#region PXDBPriceCostCalced
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBPriceCostCalcedAttribute : PXDBCalcedAttribute
	{
		#region Ctor
		public PXDBPriceCostCalcedAttribute(Type operand, Type type)
			: base(operand, type)
		{
		}
		#endregion
		#region Implementation
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
		#endregion
	}
	#endregion

	#region PXPriceCostAttribute

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXPriceCostAttribute : PXDecimalAttribute
	{
		#region Implementation
		public static decimal Round(decimal value)
		{
			return (decimal)Math.Round(value, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
		}
		#endregion
		#region Static methods
		public static Decimal MinPrice(InventoryItem ii, INItemCost cost)
		{					
			if (ii.ValMethod != INValMethod.Standard)
				{
				if (cost.AvgCost != 0m)
					return (cost.AvgCost ?? 0) + ((cost.AvgCost ?? 0) * 0.01m * (ii.MinGrossProfitPct ?? 0));
				else
					return (cost.LastCost ?? 0) + ((cost.LastCost ?? 0) * 0.01m * (ii.MinGrossProfitPct ?? 0));
				}
				else
				{
					return (ii.StdCost ?? 0) + ((ii.StdCost ?? 0) * 0.01m * (ii.MinGrossProfitPct ?? 0));
				}
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = CommonSetupDecPl.PrcCst;
		}
		#endregion
	}

	#endregion

	#region PXBaseQuantityAttribute

	public class PXBaseQuantityAttribute : PXQuantityAttribute
	{
		internal override INUnit ReadConversion(PXCache cache, object data)
		{
			var conversion = base.ReadConversion(cache, data);
			if (conversion != null && conversion.RecordID != null)
			{
				conversion = PXCache<INUnit>.CreateCopy(conversion);
				conversion.UnitMultDiv = (conversion.UnitMultDiv == MultDiv.Multiply) ? MultDiv.Divide : MultDiv.Multiply;
			}
			return conversion;
		}

		public PXBaseQuantityAttribute()
			: base()
		{
		}

		public PXBaseQuantityAttribute(Type keyField, Type resultField)
			: base(keyField, resultField)
		{
		}
	}

	#endregion

	#region PXQuantityAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXQuantityAttribute : PXDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber, IPXRowPersistingSubscriber
	{
		#region State
		protected int _ResultOrdinal;
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _ResultField = null;
		protected bool _HandleEmptyKey = false; 
		protected int? _OverridePrecision = null;


		#endregion

		#region Ctor
		public PXQuantityAttribute()
		{
		}
		public PXQuantityAttribute(byte precision)
		{
			_OverridePrecision = precision;
		}

		public PXQuantityAttribute(Type keyField, Type resultField)
		{
			_KeyField = keyField;
			_ResultField = resultField;
		}

		public bool HandleEmptyKey
		{
			set { this._HandleEmptyKey = value; }
			get { return this._HandleEmptyKey; }
		}
		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = _OverridePrecision ?? CommonSetupDecPl.Qty;

			if (_ResultField != null)
			{
				_ResultOrdinal = sender.GetFieldOrdinal(_ResultField.Name);
			}

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
			}
		}
		#endregion

		#region Implementation
		internal virtual INUnit ReadConversion(PXCache cache, object data)
				{
			var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, _KeyField.Name).FirstOrDefault();
			return unitAttribute == null
				? null
				: unitAttribute.ReadConversion(cache, data, (string)cache.GetValue(data, _KeyField.Name));
		}

		protected virtual void CalcBaseQty(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				if (e.NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						object value = sender.GetValue(e.Row, _KeyField.Name);
						if (String.IsNullOrEmpty((String)value))
						{
							resultval = (decimal)e.NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
                        INUnit conv = ReadConversion(sender, e.Row);
                        if(conv != null)
                        {
                            if(conv.FromUnit == conv.ToUnit)
                            {
                                _ensurePrecision(sender, e.Row);
                                resultval = Math.Round((decimal)e.NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
                            }
                            else if (conv.UnitRate != 0m)
						{
							_ensurePrecision(sender, e.Row);
							resultval = Math.Round((decimal)e.NewValue * (conv.UnitMultDiv == MultDiv.Multiply ? (decimal)conv.UnitRate : 1 / (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
						}
                        }
                        else
						{
                            if (!e.ExternalCall)
							throw new PXUnitConversionException((string)sender.GetValue(e.Row, _KeyField.Name));
						}
					}
				}
				sender.SetValue(e.Row, _ResultOrdinal, resultval);
			}
		}

		protected virtual void CalcBaseQty(PXCache sender, object data)
		{
			object NewValue = sender.GetValue(data, _FieldName);
			try
			{
				CalcBaseQty(sender, new PXFieldVerifyingEventArgs(data, NewValue, false));
			}
			catch (PXUnitConversionException)
			{
				sender.SetValue(data, _ResultField.Name, null);
			}
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				object NewValue = sender.GetValue(data, _ResultOrdinal);

				if (NewValue != null)
				{
                    var conv = ReadConversion(sender, data);
                    if(conv != null)
                    {
                        if(conv.FromUnit == conv.ToUnit)
                        {
                            _ensurePrecision(sender, data);
                            resultval = Math.Round((decimal)NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
                        }
                        else if (conv.UnitRate != 0m)
					{
						_ensurePrecision(sender, data);
						resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == MultDiv.Multiply ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
					}
				}
				}
				sender.SetValue(data, _FieldOrdinal, resultval);
			}
		}

		public static void CalcBaseQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXQuantityAttribute)attr).CalcBaseQty(cache, data);
					break;
				}
			}
		}

		public static void CalcTranQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXQuantityAttribute)attr).CalcTranQty(cache, data);
					break;
				}
			}
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			CalcBaseQty(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, false));
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CalcBaseQty(sender, e);
		}
		#endregion
	}

	#endregion

	#region PXDBBaseQuantityAttribute

	public class PXDBBaseQuantityAttribute : PXDBQuantityAttribute
	{
        internal override ConversionInfo ReadConversionInfo(PXCache cache, object data)
		{
			var convInfo = base.ReadConversionInfo(cache, data);
			if (convInfo?.Conversion != null && convInfo.Conversion.FromUnit != convInfo.Conversion.ToUnit)
			{
				var conversion = PXCache<INUnit>.CreateCopy(convInfo.Conversion);
				conversion.UnitMultDiv = (convInfo.Conversion.UnitMultDiv == MultDiv.Multiply) ? MultDiv.Divide : MultDiv.Multiply;
				convInfo = new ConversionInfo(conversion, convInfo.Inventory);
			}
			return convInfo;
		}

		public PXDBBaseQuantityAttribute()
			: base()
		{
		}

		public PXDBBaseQuantityAttribute(Type keyField, Type resultField)
			: base(keyField, resultField)
		{
		}
	}

	#endregion

	#region PXDBQuantityAttribute

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBQuantityAttribute : PXDBDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber
	{
		#region State
		private Dictionary<object, PXDBQuantityAttribute> _rowAttributes;
		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		protected int _ResultOrdinal;
		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _ResultField = null;
		protected bool _HandleEmptyKey = false;
		protected int? _OverridePrecision = null;

		public Type KeyField
		{
			get
			{
				return _KeyField;
			}
		}

		public InventoryUnitType DecimalVerifyUnits { get; set; }

		public DecimalVerifyMode DecimalVerifyMode { get; set; }

		/// <summary>
		/// Enable conversion other units to specified units for decimal verifying(<see cref="DecimalVerifyUnits"/>)
		/// </summary>
		public bool ConvertToDecimalVerifyUnits { get; set; }

		#endregion

		#region Ctor
		public PXDBQuantityAttribute()
		{
			ConvertToDecimalVerifyUnits = true;
			DecimalVerifyMode = DecimalVerifyMode.Error;
		}

		public PXDBQuantityAttribute(byte precision):this()
		{
			_OverridePrecision = precision;
		}

		public PXDBQuantityAttribute(Type keyField, Type resultField):this()
		{
			_KeyField = keyField;
			_ResultField = resultField;
		}

		public PXDBQuantityAttribute(Type keyField, Type resultField, InventoryUnitType decimalVerifyUnits) : this(keyField, resultField)
		{
			DecimalVerifyUnits = decimalVerifyUnits;
		}

		public PXDBQuantityAttribute(int precision, Type keyField, Type resultField)
			: this(keyField, resultField)
		{
			_OverridePrecision = precision;
		}

		public PXDBQuantityAttribute(int precision, Type keyField, Type resultField, InventoryUnitType decimalVerifyUnits)
			: this(keyField, resultField, decimalVerifyUnits)
		{
			_OverridePrecision = precision;
		}

		public bool HandleEmptyKey
		{
			set { this._HandleEmptyKey = value; }
			get { return this._HandleEmptyKey; }
		}

		#endregion

		#region Runtime

		public static PXNotDecimalUnitException VerifyForDecimal(PXCache cache, object data)
		{
			PXNotDecimalUnitException ex = null;
			cache.Adjust<PXDBQuantityAttribute>(data).ForAllFields(a =>
			{
				var newEx = a.VerifyForDecimalValue(cache, data);
				if (newEx != null && (ex == null || newEx.ErrorLevel > ex.ErrorLevel))
					ex = newEx;
			});
			return ex;
		}

		public static PXNotDecimalUnitException VerifyForDecimal<TField>(PXCache cache, object data)
			where TField : IBqlField
		{
			PXNotDecimalUnitException error = null;
			cache.Adjust<PXDBQuantityAttribute>(data).For<TField>(a => error = a.VerifyForDecimalValue(cache, data));
			return error;
		}

		#endregion

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = _OverridePrecision ?? CommonSetupDecPl.Qty;

			if (_ResultField == null)
				return;

			_ResultOrdinal = sender.GetFieldOrdinal(_ResultField.Name);

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
				_rowAttributes = new Dictionary<object, PXDBQuantityAttribute>(sender.GetComparer());
				_rowAttributes.Add(0, this);
			}
		}

		private string GetFromUnit(PXCache cache, object data) => (string)cache.GetValue(data, _KeyField.Name);

		internal virtual ConversionInfo ReadConversionInfo(PXCache cache, object data)
		{
			var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, _KeyField.Name).FirstOrDefault();
			return unitAttribute == null
				? null
				: unitAttribute.ReadConversionInfo(cache, data, GetFromUnit(cache, data));
		}

		internal virtual InventoryItem ReadInventoryItem(PXCache cache, object data)
		{
			var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, _KeyField.Name).FirstOrDefault();
			return unitAttribute == null
				? null
				: unitAttribute.ReadInventoryItem(cache, data);
		}

		protected virtual void CalcBaseQty(PXCache sender, QtyConversionArgs e)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				if (e.NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						if (string.IsNullOrEmpty(GetFromUnit(sender, e.Row)))
						{
							resultval = (decimal)e.NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
						if ((decimal)e.NewValue == 0)
						{
							resultval = 0m;
						}
						else
						{
							ConversionInfo convInfo = ReadConversionInfo(sender, e.Row);
							if (convInfo?.Conversion != null)
                            {
								resultval = ConvertValue(sender, e.Row, (decimal)e.NewValue, convInfo.Conversion);
								var exception = VerifyForDecimalValue(sender, convInfo.Inventory, e.Row, (decimal)e.NewValue, resultval);
								if(exception?.ErrorLevel == PXErrorLevel.Error && e.ThrowNotDecimalUnitException && !exception.IsLazyThrow)
									throw exception;
							}
                            else
							{
								if (!e.ExternalCall)
									throw new PXUnitConversionException(GetFromUnit(sender, e.Row));
							}
						}
					}
				}
				if (e.ExternalCall)
				{
					sender.SetValueExt(e.Row, this._ResultField.Name, resultval);
				}
				else
				{
					sender.SetValue(e.Row, this._ResultField.Name, resultval);
				}
				//sender.SetStatus(e.Row, PXEntryStatus.Updated);
			}
		}

		private decimal? ConvertValue(PXCache cache, object row, decimal? value, INUnit conv)
		{
			decimal? resultval = null;
			if (conv.FromUnit == conv.ToUnit)
			{
				resultval = ConvertValue(cache, row, (decimal)value, (v, invert) => v);
			}
			else if (conv.UnitRate != 0m)
			{
				resultval = ConvertValue(cache, row, (decimal)value, 
					(v, invert) => v * (conv.UnitMultDiv == MultDiv.Multiply == invert ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate));
			}
			return resultval;
		}

		private decimal? ConvertValue(PXCache cache, object row, decimal value, Func<decimal, bool, decimal> converter)
		{
			_ensurePrecision(cache, row);

			decimal? resultFieldCurrentValue = (decimal?)cache.GetValue(row, _ResultField.Name);
			if (resultFieldCurrentValue != null)
			{
				decimal revValue = Math.Round(converter(resultFieldCurrentValue ?? 0m, true), (int)_Precision, MidpointRounding.AwayFromZero);
				if (revValue == value)
					return resultFieldCurrentValue;
			}
			return Math.Round(converter(value, false), (int)_Precision, MidpointRounding.AwayFromZero);
		}

		protected virtual void CalcBaseQty(PXCache sender, object data)
		{
			object NewValue = sender.GetValue(data, _FieldName);
			try
			{
				CalcBaseQty(sender, new QtyConversionArgs(data, NewValue, false));
			}
			catch (PXUnitConversionException)
			{
				sender.SetValue(data, _ResultField.Name, null);
			}
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				object NewValue = sender.GetValue(data, _ResultField.Name);

				if (NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						if (string.IsNullOrEmpty(GetFromUnit(sender, data)))
						{
							resultval = (decimal)NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
						if ((decimal)NewValue == 0m)
						{
							resultval = 0m;
						}
						else
						{
							ConversionInfo convInfo = ReadConversionInfo(sender, data);
							if (convInfo?.Conversion != null)
							{
								INUnit conv = convInfo.Conversion;
								if (conv.FromUnit == conv.ToUnit)
								{
									_ensurePrecision(sender, data);
									resultval = Math.Round((decimal)NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
								}
								else if(conv.UnitRate != 0m)
								{
								_ensurePrecision(sender, data);
								resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == MultDiv.Multiply ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
								}
								VerifyForDecimalValue(sender, convInfo.Inventory, data, resultval, (decimal)NewValue);
							}
						}
					}
				}
				sender.SetValue(data, _FieldOrdinal, resultval);
			}
		}

		public static void CalcBaseQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXDBQuantityAttribute)attr).CalcBaseQty(cache, data);
					break;
				}
			}
		}

		public static void CalcTranQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXDBQuantityAttribute)attr).CalcTranQty(cache, data);
					break;
				}
			}
		}

		public static decimal Round(decimal? value)
		{
			decimal value0 = value ?? 0m;
			return Math.Round(value0, CommonSetupDecPl.Qty, MidpointRounding.AwayFromZero);
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			CalcBaseQty(sender, new QtyConversionArgs(e.Row, NewValue, false) { ThrowNotDecimalUnitException = true });
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXFieldUpdatingEventArgs args = new PXFieldUpdatingEventArgs(e.Row, e.NewValue);
			if (!e.ExternalCall)
			{
				base.FieldUpdating(sender, args);
			}
			CalcBaseQty(sender, new QtyConversionArgs(args.Row, args.NewValue, true));
			e.NewValue = args.NewValue;
		}

		public void SetDecimalVerifyMode(object row, DecimalVerifyMode mode)
		{
			if (AttributeLevel == PXAttributeLevel.Type)
				return;
			if (row == null)
			{
				if (AttributeLevel == PXAttributeLevel.Cache)
					DecimalVerifyMode = mode;
			}
			else
			{
				if (AttributeLevel == PXAttributeLevel.Item)
				{
					DecimalVerifyMode = mode;
					if (_rowAttributes != null)
						_rowAttributes.First().Value._rowAttributes[row] = this;
				}
			}
		}

		private DecimalVerifyMode GetDecimalVerifyMode(object data)
		{
			PXDBQuantityAttribute rowAttribute;
			if (AttributeLevel == PXAttributeLevel.Item || _rowAttributes == null || !_rowAttributes.TryGetValue(data, out rowAttribute))
				return DecimalVerifyMode;
			return rowAttribute.DecimalVerifyMode;
		}

		public virtual PXNotDecimalUnitException VerifyForDecimalValue(PXCache cache, object data)
		{
			if (_KeyField == null
				|| data == null
				|| DecimalVerifyUnits == InventoryUnitType.None
				|| GetDecimalVerifyMode(data) == DecimalVerifyMode.Off)
				return null;
			var qty = (decimal?)cache.GetValue(data, _FieldOrdinal);
			if((qty ?? 0) == 0)
				return null;
			InventoryItem inventory = ReadInventoryItem(cache, data);
			var baseQty = (decimal?)cache.GetValue(data, _ResultField.Name);
			return VerifyForDecimalValue(cache, inventory, data, qty, baseQty);
		}

		protected virtual PXNotDecimalUnitException VerifyForDecimalValue(PXCache cache, InventoryItem inventory, object data, decimal? originalValue, decimal? baseValue)
		{
			if ((baseValue ?? 0) == 0
				|| DecimalVerifyUnits == InventoryUnitType.None
				|| inventory == null
				|| (originalValue ?? 0m) == 0m)
				return null;
			var verifyMode = GetDecimalVerifyMode(data);
			if (verifyMode == DecimalVerifyMode.Off)
				return null;
			InventoryUnitType integerUnits = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>()
				? inventory.ToIntegerUnits()
				: (inventory.DecimalBaseUnit == true ? InventoryUnitType.None : InventoryUnitType.BaseUnit | InventoryUnitType.SalesUnit | InventoryUnitType.PurchaseUnit);
			if (integerUnits == InventoryUnitType.None)
				return null;
			string fromUnit = GetFromUnit(cache, data);
			InventoryUnitType unitTypes = inventory.ToUnitTypes(fromUnit);
			foreach (var verifyUnitType in DecimalVerifyUnits.Split())
			{
				if ((verifyUnitType & integerUnits) == 0)
					continue;
				string unitID;
				decimal value;
				bool needToConvert = false;
				if (verifyUnitType == InventoryUnitType.BaseUnit)
				{
					unitID = inventory.BaseUnit;
					value = (decimal)baseValue;
				}
				else if((verifyUnitType & unitTypes) > 0)
				{
					unitID = fromUnit;
					value = (decimal)originalValue;
				}
				else
				{
					unitID = inventory.GetUnitID(verifyUnitType);
					value = (decimal)baseValue;
					needToConvert = true;
				}
				if (fromUnit != unitID && !ConvertToDecimalVerifyUnits)
					continue;

				if (needToConvert)
				{
					var conv = INUnit.UK.ByInventory.Find(cache.Graph, inventory.InventoryID, unitID);
					_ensurePrecision(cache, data);
					value = Math.Round(value * (conv.UnitMultDiv == MultDiv.Multiply ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate),
						(int)_Precision, MidpointRounding.AwayFromZero);
				}
				if (value % 1 != 0)
				{
					var exception = new PXNotDecimalUnitException(verifyUnitType, inventory.InventoryCD, unitID, 
						verifyMode == DecimalVerifyMode.Error ? PXErrorLevel.Error: PXErrorLevel.Warning);
					cache.RaiseExceptionHandling(FieldName, data, originalValue, exception);
					return exception;
				}
			}
			return null;
		}

		#endregion

		protected class QtyConversionArgs
		{
			public object Row { get; }

			public object NewValue { get; }

			public bool ExternalCall { get; }

			public bool ThrowNotDecimalUnitException { get; set; }

			public QtyConversionArgs(object row, object newValue, bool externalCall)
			{
				Row = row;
				NewValue = newValue;
				ExternalCall = externalCall;
			}
		}
	}

	/// <summary>
	/// Decimal verifying modes
	/// </summary>
	public enum DecimalVerifyMode
	{
		/// <summary>
		/// Verifying is off
		/// </summary>
		Off,
		/// <summary>
		/// Generate warning
		/// </summary>
		Warning,
		/// <summary>
		/// Generate error
		/// </summary>
		Error
	}

	#endregion

	#region PXSetupOptional

	public class PXSetupOptional<Table> : PXSelectReadonly<Table>
		where Table : class, IBqlTable, new()
	{
		protected Table _Record;
		public PXSetupOptional(PXGraph graph)
			: base(graph)
		{
			graph.Defaults[typeof(Table)] = getRecord;
		}
		private object getRecord()
		{
			if (_Record == null)
			{
				_Record = base.Select();
				if (_Record == null)
				{
					_Record = new Table();
					PXCache cache = this._Graph.Caches[typeof(Table)];
					foreach (Type field in cache.BqlFields)
					{
						object newvalue;
						cache.RaiseFieldDefaulting(field.Name.ToLower(), _Record, out newvalue);
						cache.SetValue(_Record, field.Name.ToLower(), newvalue);
					}
					base.StoreCached(new PXCommandKey(new object[] { }), new List<object>{ _Record });
				}
			}
			return _Record;
		}
	}

	public class PXSetupOptional<Table, Where> : PXSelectReadonly<Table, Where>
		where Table : class, IBqlTable, new()
		where Where : IBqlWhere, new()
	{
		protected Table _Record;
		public PXSetupOptional(PXGraph graph)
			: base(graph)
		{
			graph.Defaults[typeof(Table)] = getRecord;
		}
		private object getRecord()
		{
			if (_Record == null)
			{
				_Record = base.Select();
				if (_Record == null)
				{
					_Record = new Table();
					PXCache cache = this._Graph.Caches[typeof(Table)];
					foreach (Type field in cache.BqlFields)
					{
						object newvalue;
						cache.RaiseFieldDefaulting(field.Name.ToLower(), _Record, out newvalue);
						cache.SetValue(_Record, field.Name.ToLower(), newvalue);
					}
					base.StoreCached(new PXCommandKey(new object[] { }), new List<object>{ _Record });
				}
			}
			return _Record;
		}
	}

	#endregion

	#region PXCalcQuantityAttribute
	public class PXCalcQuantityAttribute : PXDecimalAttribute
	{
		#region State
		protected int _SourceOrdinal;
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _SourceField = null;
		protected bool _LegacyBehavior;

		#endregion

		#region Ctor
		public PXCalcQuantityAttribute()
		{			
		}

		/// <summary>
		/// Calculates TranQty using BaseQty and UOM. TranQty will be calculated on RowSelected event only.
		/// </summary>
		/// <param name="keyField">UOM field</param>
		/// <param name="sourceField">BaseQty field</param>
		public PXCalcQuantityAttribute(Type keyField, Type sourceField) : this(keyField, sourceField, true)
		{
		}

		/// <summary>
		/// Calculates TranQty using BaseQty and UOM.
		/// </summary>
		/// <param name="keyField">UOM field</param>
		/// <param name="sourceField">BaseQty field</param>
		/// <param name="legacyBehavior">When set to True, TranQty will be calculated on RowSelected and on FieldSelecting (when needed) events.</param>
		public PXCalcQuantityAttribute(Type keyField, Type sourceField, bool legacyBehavior)
		{
			_KeyField = keyField;
			_SourceField = sourceField;
			_LegacyBehavior = legacyBehavior;
		}
		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = CommonSetupDecPl.Qty;

			if (_SourceField != null)
			{
				_SourceOrdinal = sender.GetFieldOrdinal(_SourceField.Name);
			}

			sender.Graph.RowSelected.AddHandler(sender.GetItemType(), RowSelected);

			if (!_LegacyBehavior)
				sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName, TranQtyFieldSelecting);

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
			}

		}
		#endregion

		#region Implementation
		internal virtual INUnit ReadConversion(PXCache cache, object data)
				{
			var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, _KeyField.Name).FirstOrDefault();
			return unitAttribute == null
				? null
				: unitAttribute.ReadConversion(cache, data, (string)cache.GetValue(data, _KeyField.Name));
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = GetTranQty(sender, data);

			sender.SetValue(data, _FieldOrdinal, resultval ?? 0m);
		}

		protected virtual decimal? GetTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_SourceField != null)
			{
				object NewValue = sender.GetValue(data, _SourceOrdinal);

				if (NewValue != null)
				{
					INUnit conv = ReadConversion(sender, data);
					if (conv != null)
					{
						if (conv.FromUnit == conv.ToUnit)
						{
							_ensurePrecision(sender, data);
							resultval = Math.Round((decimal)NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
						}
						else if (conv.UnitRate != 0m)
						{
							_ensurePrecision(sender, data);
							resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == MultDiv.Multiply ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
						}
					}
				}
			}
			return resultval;
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcTranQty(sender, e.Row);
		}
		
		protected virtual void TranQtyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;
			if (e.Row != null && e.ReturnValue == null)
			{
				e.ReturnValue = GetTranQty(sender, e.Row);
			}
		}
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
				CalcTranQty(sender, e.Row);
		}
		#endregion
	}
    #endregion    

    #region INSiteStatusLookup
    public class INSiteStatusLookup<Status, StatusFilter> : PXSelectBase<Status>
		where Status : class, IBqlTable, new()
		where StatusFilter : class, IBqlTable, new()
	{
		protected class LookupView : PXView
		{
			public LookupView(PXGraph graph, BqlCommand command)
				: base(graph, true, command)
			{				
			}

			public LookupView(PXGraph graph, BqlCommand command, Delegate handler)
				: base(graph, true, command, handler)
			{
			}

			protected PXSearchColumn CorrectFieldName(PXSearchColumn orig, bool idFound)
			{
				switch (orig.Column.ToLower())
				{
					case "inventoryid":
						if (!idFound)
							return new PXSearchColumn("InventoryCD", orig.Descending, orig.OrigSearchValue ?? orig.SearchValue);
						else
							return null;
					case "subitemid":
						return new PXSearchColumn("SubItemCD", orig.Descending, orig.OrigSearchValue ?? orig.SearchValue);
					case "siteid":
						return new PXSearchColumn("SiteCD", orig.Descending, orig.OrigSearchValue ?? orig.SearchValue);
					case "locationid":
						return new PXSearchColumn("LocationCD", orig.Descending, orig.OrigSearchValue ?? orig.SearchValue);
				}
				return orig;
			}

			protected override List<object> InvokeDelegate(object[] parameters)
			{
				var context = PXView._Executing.Peek();
				var orig = context.Sorts;

				bool idFound = false;
				var result = new List<PXSearchColumn>();
				const string iD = "InventoryCD";
				for (int i = 0; i < orig.Length - this.Cache.Keys.Count; i++)
				{
					result.Add(CorrectFieldName(orig[i], false));
					if (orig[i].Column == iD)
						idFound = true;
				}

				for (int i = orig.Length - this.Cache.Keys.Count; i < orig.Length; i++)
				{
					var col = CorrectFieldName(orig[i], idFound);
					if (col != null)
						result.Add(col);
				}
				context.Sorts = result.ToArray();

				return base.InvokeDelegate(parameters);
			}
		}

		public const string Selected = "Selected";
		public const string QtySelected = "QtySelected";
		private PXView intView;
		#region Ctor
		public INSiteStatusLookup(PXGraph graph)
		{
			this.View = new PXView(graph, false, 
				BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<>), typeof(Status))), 
				new PXSelectDelegate(viewHandler));
			InitHandlers(graph);
		}

		public INSiteStatusLookup(PXGraph graph, Delegate handler)
		{
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(typeof(Select<>), typeof(Status)),
				handler);			
			InitHandlers(graph);
		}
		#endregion

		#region Implementations

		private void InitHandlers(PXGraph graph)
		{
			graph.RowSelected.AddHandler(typeof(StatusFilter), OnFilterSelected);
			graph.RowSelected.AddHandler(typeof(Status), OnRowSelected);
			graph.FieldUpdated.AddHandler(typeof(Status), Selected, OnSelectedUpdated);
		}

		protected virtual void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INSiteStatusFilter row = e.Row as INSiteStatusFilter;
			if (row != null)
				PXUIFieldAttribute.SetVisible(sender.Graph.Caches[typeof(Status)], typeof(INSiteStatus.siteID).Name, row.SiteID == null);
		}

		
		protected virtual void OnSelectedUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool? selected = (bool?)sender.GetValue(e.Row, Selected);
			decimal? qty = (decimal?)sender.GetValue(e.Row, QtySelected);
			if (selected == true)
			{
				if(qty == null || qty == 0m)
					sender.SetValue(e.Row, QtySelected, 1m);
			}
			else
				sender.SetValue(e.Row, QtySelected, 0m);
		}

		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, Selected, true);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, QtySelected, true);
		}	

		protected virtual PXView CreateIntView(PXGraph graph)
		{
			PXCache cache = graph.Caches[typeof(Status)];

			List<Type> select = new List<Type>();
			select.Add(typeof(Select<,>));
			select.Add(typeof(Status));
			select.Add(CreateWhere(graph));

            //select.Add(typeof(Aggregate<>));
            /*
			List<Type> groupFields = cache.BqlKeys;
			groupFields.AddRange(cache.BqlFields.Where(field => field.IsDefined(typeof (PXExtraKeyAttribute), false)));			

			for (int i = 0; i < groupFields.Count; i++)
			{
				select.Add((i != groupFields.Count - 1) ? typeof(GroupBy<,>) : typeof(GroupBy<>));
				select.Add(groupFields[i]);
			}
			*/
            Type selectType = BqlCommand.Compose(select.ToArray());

			return new LookupView(graph, BqlCommand.CreateInstance(selectType));
		}

		protected virtual IEnumerable viewHandler()
		{
			if (intView == null) intView = CreateIntView(this.View.Graph);
			var startRow = PXView.StartRow;
			var totalRows = 0;

			var rows = new PXDelegateResult();

			foreach (var rec in intView.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								 ref startRow, PXView.MaximumRows, ref totalRows))
			{
				Status item = PXResult.Unwrap<Status>(rec);
				Status result = item;
				Status updated = this.Cache.Locate(item) as Status;
				if (updated != null && this.Cache.GetValue(updated, Selected) as bool? == true)
				{
					Decimal? qty = this.Cache.GetValue(updated, QtySelected) as Decimal?;
					this.Cache.RestoreCopy(updated,item);
					this.Cache.SetValue(updated, Selected, true);
					this.Cache.SetValue(updated, QtySelected, qty);
					result = updated;
				}
				rows.Add(result);
			}
			PXView.StartRow = 0;

			if (PXView.ReverseOrder)
				rows.Reverse();

			rows.IsResultSorted = true;

			return rows;
		}

		protected static Type CreateWhere(PXGraph graph)
		{
			PXCache filter = graph.Caches[typeof(INSiteStatusFilter)];
			PXCache cache = graph.Caches[typeof(Status)];

			Type where = typeof(Where<boolTrue, Equal<boolTrue>>);
			foreach (string field in filter.Fields)
			{
				if (cache.Fields.Contains(field))
				{
					if (filter.Fields.Contains(field + "Wildcard")) continue;
					if (field.Contains("SubItem") && !PXAccess.FeatureInstalled<FeaturesSet.subItem>()) continue;
					if (field.Contains("Site") && !PXAccess.FeatureInstalled<FeaturesSet.warehouse>()) continue;
					if (field.Contains("Location") && !PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()) continue;
					Type sourceType = filter.GetBqlField(field);
					Type destinationType = cache.GetBqlField(field);
					if (sourceType != null && destinationType != null)
					{
						where = BqlCommand.Compose(
							typeof(Where2<,>),
							typeof(Where<,,>),
							typeof(Current<>), sourceType, typeof(IsNull),
							typeof(Or<,>), destinationType, typeof(Equal<>), typeof(Current<>), sourceType,
							typeof(And<>), where
						);
					}
				}
				string f;
				if (field.Length > 8 && field.EndsWith("Wildcard") && cache.Fields.Contains(f = field.Substring(0, field.Length - 8)))
				{
					if (field.Contains("SubItem") && !PXAccess.FeatureInstalled<FeaturesSet.subItem>()) continue;
					if (field.Contains("Site") && !PXAccess.FeatureInstalled<FeaturesSet.warehouse>()) continue;
					if (field.Contains("Location") && !PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()) continue;
					Type like = filter.GetBqlField(field);
					Type dest = cache.GetBqlField(f);
					where = BqlCommand.Compose(
						typeof(Where2<,>),
						typeof(Where<,,>), typeof(Current<>), like, typeof(IsNull),
						typeof(Or<,>), dest, typeof(Like<>), typeof(Current<>), like,
						typeof(And<>), where
						);
				}
			}		
			return where;
		}

		protected static Type GetTypeField<Source>(string field)
		{
			Type sourceType = typeof(Source);
			Type fieldType = null;
			while (fieldType == null && sourceType != null)
			{
				fieldType = sourceType.GetNestedType(field, System.Reflection.BindingFlags.Public);
				sourceType = sourceType.BaseType;
			}
			return fieldType;
		}

		private class Zero : PX.Data.BQL.BqlDecimal.Constant<Zero>
		{
			public Zero() : base(0m) { }
		}
		#endregion
	}
	#endregion

	#region INBarCodeItemLookup
	public class INBarCodeItemLookup<Filter> : PXFilter<Filter>
		where Filter : INBarCodeItem, new()
	{
		#region Ctor
		public INBarCodeItemLookup(PXGraph graph)
			:base(graph)
		{
			InitHandlers(graph);
		}

		public INBarCodeItemLookup(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			InitHandlers(graph);
		}
		#endregion

		private void InitHandlers(PXGraph graph)
		{
			graph.RowSelected.AddHandler(typeof(Filter), OnFilterSelected);

			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.barCode).Name, Filter_BarCode_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.inventoryID).Name, Filter_InventoryID_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.byOne).Name, Filter_ByOne_FieldUpdated);					
		}

		public virtual void Reset(bool keepDescription)
		{
			Filter s = this.Current;
			this.Cache.Remove(s);
			this.Cache.Insert(this.Cache.CreateInstance());
			this.Current.ByOne = s.ByOne;
			this.Current.AutoAddLine = s.AutoAddLine;		
			if(keepDescription)
				this.Current.Description = s.Description;							
		}
		
		#region Filter Events
		protected virtual void Filter_BarCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.ExternalCall)
			{
				var rec = (PXResult<INItemXRef, InventoryItem, INSubItem>)
									PXSelectJoin<INItemXRef,
										InnerJoin<InventoryItem,
														On2<INItemXRef.FK.InventoryItem,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noRequest>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>>,
										InnerJoin<INSubItem,
													 On<INItemXRef.FK.SubItem>>>,
										Where<INItemXRef.alternateID, Equal<Current<INBarCodeItem.barCode>>,
											And<INItemXRef.alternateType, Equal<INAlternateType.barcode>>>>
										.SelectSingleBound(this._Graph, new object[] { e.Row });
				if (rec != null)
				{
					sender.SetValue<INBarCodeItem.inventoryID>(e.Row, null);
					sender.SetValuePending<INBarCodeItem.inventoryID>(e.Row, ((InventoryItem)rec).InventoryCD);
					sender.SetValuePending<INBarCodeItem.subItemID>(e.Row, ((INSubItem)rec).SubItemCD);
				}
				else
				{
					sender.SetValuePending<INBarCodeItem.inventoryID>(e.Row, null);
					sender.SetValuePending<INBarCodeItem.subItemID>(e.Row, null);
				}
			}
		}
		protected virtual void Filter_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.ExternalCall)
			{
				Filter row = e.Row as Filter;
				if (e.OldValue != null && row.InventoryID != null)
					row.BarCode = null;
				sender.SetDefaultExt<INBarCodeItem.subItemID>(e);
				sender.SetDefaultExt<INBarCodeItem.qty>(e);
			}
		}
		protected virtual void Filter_ByOne_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if(e.ExternalCall)
			{
				Filter row = e.Row as Filter;				
				if (row != null && row.ByOne == true)
					row.Qty = 1m;
			}
		}
		
		protected virtual void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Filter row = e.Row as Filter;
			if (row != null)
			{
				var item = InventoryItem.PK.Find(_Graph, row.InventoryID);
				var lotclass = INLotSerClass.PK.Find(_Graph, item?.LotSerClassID);

				bool requestLotSer = lotclass != null && lotclass.LotSerTrack != INLotSerTrack.NotNumbered &&
														 lotclass.LotSerAssign == INLotSerAssign.WhenReceived;

				PXUIFieldAttribute.SetEnabled<INBarCodeItem.lotSerialNbr>(sender, null, requestLotSer);
				PXDefaultAttribute.SetPersistingCheck<INBarCodeItem.lotSerialNbr>(sender, null, requestLotSer ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.expireDate>(sender, null, requestLotSer && lotclass.LotSerTrackExpiration == true);
				PXDefaultAttribute.SetPersistingCheck<INBarCodeItem.expireDate>(sender, null, requestLotSer && lotclass.LotSerTrackExpiration == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.uOM>(sender, null, !(requestLotSer && lotclass.LotSerTrack == INLotSerTrack.SerialNumbered) && row.ByOne != true && row.InventoryID != null);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.inventoryID>(sender, null, (string.IsNullOrEmpty(row.BarCode) || row.InventoryID == null));
			}
		}
		#endregion
	}
	#endregion

	#region INOpenPeriod
	public class INOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public INOpenPeriodAttribute()
			: this(null)
		{
		}	

		public INOpenPeriodAttribute(Type sourceType,
			Type branchSourceType = null,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			SelectionModesWithRestrictions selectionModeWithRestrictions = SelectionModesWithRestrictions.Undefined,
			Type masterFinPeriodIDType = null)
			: base(typeof(Search<FinPeriod.finPeriodID,
					Where<FinPeriod.iNClosed, Equal<False>,
						And<FinPeriod.status, Equal<FinPeriod.status.open>>>>),
					sourceType,
					branchSourceType: branchSourceType,
					branchSourceFormulaType: branchSourceFormulaType,
					organizationSourceType: organizationSourceType,
					useMasterCalendarSourceType: useMasterCalendarSourceType,
					defaultType: defaultType,
					redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
					selectionModeWithRestrictions: selectionModeWithRestrictions,
					masterFinPeriodIDType: masterFinPeriodIDType)
		{
		}

		#endregion


		#region Implementation

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError && finPeriod.INClosed == true)
			{
				result = HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: Messages.FinancialPeriodClosedInIN);
			}

			return result;
		}

		#endregion
	}
	#endregion
	
	#region INParentItemClassAttribute

	/// <summary>
	/// The attribute is supposed to find and assign Parent Item Class for a newly created Child Item Class.
	/// </summary>
	public class INParentItemClassAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		protected readonly bool _DefaultStkItemFromParent;

		public INParentItemClassAttribute(bool defaultStkItemFromParent = false)
		{
			_DefaultStkItemFromParent = defaultStkItemFromParent;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_DefaultStkItemFromParent)
			{
				sender.Graph.FieldDefaulting.AddHandler<INItemClass.stkItem>(StkItemDefaulting);
			}
		}

		protected virtual void StkItemDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INItemClass parent = LookupNearestParent(sender, e);
			e.NewValue = (parent != null) ? parent.StkItem : true;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INItemClass parent = LookupNearestParent(sender, e);
			if (parent != null)
			{
				e.NewValue = parent.ItemClassID;
			}
			else
			{
				ItemClassDefaulting(sender, e);
			}
		}

		protected virtual void ItemClassDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INSetup inSetup = PXSelectReadonly<INSetup>.Select(sender.Graph);
			if (inSetup != null)
			{
				bool? stkItem = (bool?)sender.GetValue<INItemClass.stkItem>(e.Row);
				e.NewValue = (stkItem == false) ? inSetup.DfltNonStkItemClassID : inSetup.DfltStkItemClassID;
			}
		}

		protected virtual INItemClass LookupNearestParent(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			string segmentedKey = (string)sender.GetValue<INItemClass.itemClassCD>(e.Row) ?? string.Empty;
			segmentedKey = segmentedKey.Trim();
			if (string.IsNullOrEmpty(segmentedKey))
			{
				return null;
			}

			Segment[] segments = PXSelectReadonly<Segment,
				Where<Segment.dimensionID, Equal<INItemClass.dimension>>, OrderBy<Asc<Segment.segmentID>>>
				.Select(sender.Graph).RowCast<Segment>().ToArray();
			if (segments.Length == 0)
			{
				return null;
			}

			int[] lengthsBySegments = new int[segments.Length];
			int filledSegmentsCnt = 0;
			for (int i = 0; i < segments.Length; i++)
			{
				lengthsBySegments[i] = segments[i].Length.Value + (i == 0 ? 0 : lengthsBySegments[i - 1]);
				if (segmentedKey.Length > lengthsBySegments[i])
				{
					filledSegmentsCnt++;
				}
			}

			INItemClass parent = null;
			while (parent == null && filledSegmentsCnt > 0)
			{
				int length = lengthsBySegments[filledSegmentsCnt - 1];
				string partOfSegmentedKey = segmentedKey.Substring(0, length);
				parent = PXSelectReadonly<INItemClass, Where<INItemClass.itemClassCD, Equal<Required<INItemClass.itemClassCD>>>>
					.Select(sender.Graph, partOfSegmentedKey);

				filledSegmentsCnt--;
			}

			return parent;
		}
	}

	#endregion

	#region INSyncUomsAttribute

	/// <summary>
	/// The attribute is supposed to synchronize values of different units of measure settings in the case if the Multiple Units of Measure feature is disabled.
	/// </summary>
	public class INSyncUomsAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		protected readonly Type[] _uomFieldList;

		public INSyncUomsAttribute(params Type[] uomFieldList)
		{
			_uomFieldList = uomFieldList;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
			{
				var newValue = sender.GetValue(e.Row, _FieldOrdinal);
				foreach (Type uomField in _uomFieldList)
				{
					sender.SetValue(e.Row, uomField.Name, newValue);
				}
			}
		}
	}

	#endregion

	/// <summary>
	/// FinPeriod selector that extends <see cref="FinPeriodSelectorAttribute"/>. 
	/// Displays and accepts only Closed Fin Periods. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// </summary>
	public class INClosedPeriodAttribute : FinPeriodSelectorAttribute
	{
		public INClosedPeriodAttribute(Type aSourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, 
				Where<FinPeriod.status, NotEqual<FinPeriod.status.inactive>,
						Or<FinPeriod.iNClosed, Equal<True>>>, 
				OrderBy<Desc<FinPeriod.finPeriodID>>>))
		{
		}

		public INClosedPeriodAttribute()
			: this(null)
		{

		}
	}
	
	#region SubItemStatusVeryfier
	public class SubItemStatusVeryfierAttribute : PXEventSubscriberAttribute
	{
		protected readonly Type inventoryID;
		protected readonly Type siteID;
		protected readonly string[] statusrestricted;

		public SubItemStatusVeryfierAttribute(Type inventoryID, Type siteID, params string[] statusrestricted)
		{
			this.inventoryID = inventoryID;
			this.siteID = siteID;
			this.statusrestricted = statusrestricted;
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), _FieldName, OnSubItemFieldVerifying);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), siteID.Name, OnSiteFieldVerifying);
		}
		
		protected virtual void OnSubItemFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{			
			if(!Validate(sender, 
				(int?)sender.GetValue(e.Row, inventoryID.Name),
				(int?)e.NewValue,
				(int?)sender.GetValue(e.Row, siteID.Name)))
				throw new PXSetPropertyException(Messages.RestictedSubItem);
		}

		protected virtual void OnSiteFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{			
			if (!Validate(sender,
				(int?)sender.GetValue(e.Row, inventoryID.Name),
				(int?)sender.GetValue(e.Row, _FieldName),
				(int?)e.NewValue))
				throw new PXSetPropertyException(Messages.RestictedSubItem);			
		}

		private bool Validate(PXCache sender, int? invetroyID, int? subitemID, int? siteID)
		{
			INItemSiteReplenishment settings =
				PXSelect<INItemSiteReplenishment,
					Where<INItemSiteReplenishment.inventoryID, Equal<Required<INItemSiteReplenishment.inventoryID>>,
						And<INItemSiteReplenishment.subItemID, Equal<Required<INItemSiteReplenishment.subItemID>>,
							And<INItemSiteReplenishment.siteID, Equal<Required<INItemSiteReplenishment.siteID>>>>>>.SelectWindowed(
								sender.Graph, 0, 1, invetroyID, subitemID, siteID);
			if(settings != null)
				foreach (string status in statusrestricted)
				{
					if(status == settings.ItemStatus) return false;
				}
			return true;
		}
	}
	#endregion

	#region PXSelectorWithoutVerificationAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PXSelectorWithoutVerificationAttribute : PXSelectorAttribute
	{
        public PXSelectorWithoutVerificationAttribute(Type type)
            : base(type)
		{
		}

        public PXSelectorWithoutVerificationAttribute(Type type, params Type[] fieldList)
            : base(type, fieldList)
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			//base.FieldVerifying(sender, e);
		}
	}

	#endregion

	#region INRegisterCacheNameAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class INRegisterCacheNameAttribute : PX.Data.PXCacheNameAttribute
	{
		public INRegisterCacheNameAttribute(string name)
			: base(name)
		{
		}

		public override string GetName(object row)
		{
			var register = row as INRegister;
			if (register == null) return base.GetName();

			var result = Messages.Receipt;
			switch (register.DocType)
			{
				case INDocType.Issue:
					result = Messages.Issue;
					break;
				case INDocType.Transfer:
					result = Messages.Transfer;
					break;
				case INDocType.Adjustment:
					result = Messages.Adjustment;
					break;
				case INDocType.Production:
					result = Messages.Production;
					break;
				case INDocType.Disassembly:
					result = Messages.Disassembly;
					break;
			}
			return result;
		}
	}

	#endregion


	public class INSubItemSegmentValueList : 
		PXSelect<INSubItemSegmentValue, Where<INSubItemSegmentValue.inventoryID, Equal<Current<InventoryItem.inventoryID>>>>		
	{
        [PXHidden]
		public class SValue : IBqlTable
		{
			#region SegmentID
			public abstract class segmentID : PX.Data.BQL.BqlShort.Field<segmentID> { }
			protected Int16? _SegmentID;
			[PXDBShort(IsKey = true)]			
			[PXUIField(DisplayName = "Segment ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
			public virtual Int16? SegmentID
			{
				get
				{
					return this._SegmentID;
				}
				set
				{
					this._SegmentID = value;
				}
			}
			#endregion
			#region Value
			public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
			protected String _Value;
			[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXDefault()]
			[PXUIField(DisplayName = "Value", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]			
			public virtual String Value
			{
				get
				{
					return this._Value;
				}
				set
				{
					this._Value = value;
				}
			}
			#endregion
			#region Descr
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
			protected String _Descr;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#endregion
			#region Active
			public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
			protected Boolean? _Active;
			[PXDBBool()]
			[PXDefault((bool)false)]
			[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? Active
			{
				get
				{
					return this._Active;
				}
				set
				{
					this._Active = value;
				}
			}
			#endregion			
		}

		/// <summary>
		/// String pattern for dynamic Subitem views
		/// </summary>
		public const string SubItemViewsPattern = "SubItem_";

		/// <summary>
		/// Gets the number of Subitem segments if the appropriate feature is on,
		/// otherwise returns <c>null</c>
		/// </summary>
		public int? SegmentsNumber
		{
			get;
			protected set;
		}

		public INSubItemSegmentValueList(PXGraph graph)
			:base(graph)
		{
			graph.Caches[typeof (SValue)].AllowInsert = graph.Caches[typeof (SValue)].AllowDelete = false;
			if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				this.AllowSelect = false;
				return;
			}

			SegmentsNumber = 0;
			foreach (Segment s in PXSelect<Segment,
				Where<Segment.dimensionID, Equal<SubItemAttribute.dimensionName>>,
				OrderBy<Asc<Segment.segmentID>>>.Select(graph))
			{
				SegmentsNumber++;
				int? segmentID = s.SegmentID;
				graph.Views.Add("DimensionsSubItem",
					new PXView(graph, false, BqlCommand.CreateInstance(typeof(Select<Segment, 
						Where<Segment.dimensionID, Equal<SubItemAttribute.dimensionName>>,
						OrderBy<Asc<Segment.segmentID>>>)))
					);
				graph.Views.Add(SubItemViewsPattern + s.SegmentID, new PXView(graph, false,
					BqlCommand.CreateInstance(typeof (Select<SValue>)),
					(PXSelectDelegate) delegate()
					{
						PXCache cache = graph.Caches[typeof (SValue)];
						List<SValue> list = new List<SValue>();
						foreach (PXResult<SegmentValue, INSubItemSegmentValue> r in 
							PXSelectJoin<SegmentValue,							
								LeftJoin<INSubItemSegmentValue, 
											On<INSubItemSegmentValue.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
										 And<INSubItemSegmentValue.segmentID, Equal<SegmentValue.segmentID>,
										 And<INSubItemSegmentValue.value, Equal<SegmentValue.value>>>>>,
							Where<SegmentValue.dimensionID, Equal<SubItemAttribute.dimensionName>,
 								And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>>>>.Select(graph,  segmentID))
						{
							SegmentValue segValue = r;
							INSubItemSegmentValue itemValue = r;
							SValue result = (SValue)cache.CreateInstance();
							result.SegmentID = segValue.SegmentID;
							result.Value = segValue.Value;
							result.Descr = segValue.Descr;
							if (itemValue.InventoryID != null)
								result.Active = true;
							result = (SValue) (cache.Insert(result) ?? cache.Locate(result));
							list.Add(result);
						}
						return list;
					}));			
				graph.RowUpdated.AddHandler<SValue>(OnRowUpdated);
			}
		}

		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SValue row = e.Row as SValue;
			if (row == null) return;
			INSubItemSegmentValue result = (INSubItemSegmentValue)this.Cache.CreateInstance();
			this.Cache.SetDefaultExt<INSubItemSegmentValue.inventoryID>(result);			
			result.SegmentID = row.SegmentID;
			result.Value = row.Value;
			if(row.Active == true)			
				this.Cache.Update(result);
			else
				this.Cache.Delete(result);							
		}
	}
}
