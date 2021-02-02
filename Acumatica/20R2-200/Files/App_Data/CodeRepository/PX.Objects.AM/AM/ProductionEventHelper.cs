using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manage creating the production event messages
    /// </summary>
    public class ProductionEventHelper
    {
        #region TRANSACTION EVENTS

        /// <summary>
        /// Convert a batch and production order id into a new production event record
        /// </summary>
        public static AMProdEvnt BuildTransactionEvent(AMBatch amBatch, AMProdItem prodOrdId)
        {
            if (amBatch == null)
            {
                throw new PXArgumentException("amBatch");
            }

            if (prodOrdId == null)
            {
                throw new PXArgumentException("prodOrdId");
            }

            var eventDescription = $"{AMDocType.GetDocTypeDesc(amBatch.DocType)} {ProductionEventType.Desc.Transaction}";

            var eventDescritpionAddition = string.Empty;
            if (!string.IsNullOrWhiteSpace(amBatch.OrigBatNbr))
            {
                eventDescritpionAddition = Messages.GetLocal(Messages.CreatedByDocTypeBatchNbr, AMDocType.GetDocTypeDesc(amBatch.OrigDocType), amBatch.OrigBatNbr);
                eventDescription = $"{eventDescription} {eventDescritpionAddition}";
            }

            return new AMProdEvnt
            {
                EventType = ProductionEventType.Transaction,
                Description = eventDescription,
                OrderType = prodOrdId.OrderType,
                ProdOrdID = prodOrdId.ProdOrdID,
                RefBatNbr = amBatch.BatNbr,
                RefDocType = amBatch.DocType
            };
        }

        /// <summary>
        /// Convert a batch and collection of production order keys into a list of new production event records
        /// </summary>
        public static List<AMProdEvnt> BuildTransactionEvents(AMBatch amBatch, params AMProdItem[] prodItemList)
        {

            if (amBatch == null)
            {
                return null;
            }

            var list = new List<AMProdEvnt>();

            foreach (var prodItem in prodItemList)
            {
                list.Add(BuildTransactionEvent(amBatch, prodItem));
            }

            return list;
        }

        #endregion

        #region STATUS EVENTS

        /// <summary>
        /// Convert a production status ID to the related production event type
        /// </summary>
        /// <param name="productionStatusID">ProductionOrderStatus</param>
        /// <returns>ProductionEventType</returns>
        public static int GetEventTypeFromProdStatusID(string productionStatusID)
        {
            switch (productionStatusID)
            {
                case ProductionOrderStatus.Planned:
                    return ProductionEventType.OrderResetToPlan;
                case ProductionOrderStatus.Released:
                    return ProductionEventType.OrderReleased;
                case ProductionOrderStatus.Completed:
                    return ProductionEventType.OrderCompleted;
                case ProductionOrderStatus.Cancel:
                    return ProductionEventType.OrderCancelled;
                case ProductionOrderStatus.Closed:
                    return ProductionEventType.OrderClosed;
            }

            //All other production status do not have a related event
            return ProductionEventType.Info; //General info event
        }

        /// <summary>
        /// Create, insert, and persist the new production status event
        /// </summary>
        public static AMProdEvnt InsertPersistStatusEvent(AMProdItem amProdItem, string oldStatusID, string newStatusID)
        {
            var graph = PXGraph.CreateInstance<PXGraph>();

            var newEvent = InsertStatusEvent(graph, amProdItem, oldStatusID, newStatusID);

            if (graph.IsDirty)
            {
                graph.Caches[typeof (AMProdEvnt)].Persist(PXDBOperation.Insert);
            }

            return newEvent;
        }

        /// <summary>
        /// Create and insert new production status events
        /// </summary>
        /// <param name="graph">calling graph</param>
        public static AMProdEvnt InsertStatusEvent(PXGraph graph, AMProdItem amProdItem, string oldStatusID,
            string newStatusID)
        {
            if (graph == null)
            {
                return null;
            }

            var newEvent = BuildStatusEvent(amProdItem, oldStatusID, newStatusID);

            if (newEvent != null)
            {
                SetCurrentProdItem(graph, newEvent);
                newEvent = (AMProdEvnt)graph.Caches[typeof (AMProdEvnt)].Insert(newEvent);
#if DEBUG
                AMDebug.TraceWriteMethodName(newEvent.DebuggerDisplay);
#endif
            }

            return newEvent;
        }

        /// <summary>
        /// Convert the new status change to a production event record
        /// </summary>
        /// <param name="amProdItem">Production Item Record</param>
        /// <param name="oldStatusID">Old ProductionOrderStatus</param>
        /// <param name="newStatusID">New ProductionOrderStatus</param>
        /// <returns></returns>
        public static AMProdEvnt BuildStatusEvent(AMProdItem amProdItem, string oldStatusID, string newStatusID)
        {
            if (amProdItem == null || string.IsNullOrWhiteSpace(oldStatusID) || string.IsNullOrWhiteSpace(newStatusID))
            {
                return null;
            }

            int eventType;
            if (oldStatusID == ProductionOrderStatus.Hold && newStatusID != ProductionOrderStatus.Hold)
            {
                eventType = ProductionEventType.OrderRemoveFromHold;
            }
            else if (oldStatusID != ProductionOrderStatus.Hold && newStatusID == ProductionOrderStatus.Hold)
            {
                eventType = ProductionEventType.OrderPlaceOnHold;
            }
            else
            {
                eventType = GetEventTypeFromProdStatusID(newStatusID);
            }

            return new AMProdEvnt
            {
                EventType = eventType,
                Description = Messages.GetLocal(Messages.ProductionStatusFromTo,
                    ProductionOrderStatus.GetStatusDescription(oldStatusID),
                    ProductionOrderStatus.GetStatusDescription(newStatusID)),
                OrderType = amProdItem.OrderType,
                ProdOrdID = amProdItem.ProdOrdID
            };
        }

        #endregion

        private static void SetCurrentProdItem(PXGraph graph, IProdOrder order)
        {
            if (order?.OrderType == null)
            {
                return;
            }

            var current = (AMProdItem)graph.Caches[typeof(AMProdItem)]?.Current;
            if (current == null || current.OrderType != order.OrderType || current.ProdOrdID != order.ProdOrdID)
            {
                graph.Caches[typeof(AMProdItem)].Current = AMProdItem.PK.Find(graph, order.OrderType, order.ProdOrdID);
#if DEBUG
                AMDebug.TraceWriteMethodName($"Set Current [{order.OrderType}:{order.ProdOrdID}] LineCntrEvnt = {((AMProdItem)graph.Caches[typeof(AMProdItem)]?.Current)?.LineCntrEvnt}");
#endif
                Common.Cache.AddCacheView<AMProdItem>(graph);
            }
        }

        public static AMProdEvnt InsertInformationEvent(PXGraph graph, string infoMessage, string prodOrdID, string orderType)
        {
            return InsertInformationEvent(graph, infoMessage, prodOrdID, orderType, true);
        }

        public static AMProdEvnt InsertInformationEvent(PXGraph graph, string infoMessage, string prodOrdID, string orderType, bool checkForDuplicate)
        {
            if (graph == null)
            {
                return null;
            }

            if (checkForDuplicate && ProdEventExist(graph, prodOrdID, orderType, ProductionEventType.Info, infoMessage))
            {
                return null;
            }

            var newEvent = BuildEvent(ProductionEventType.Info, infoMessage, prodOrdID, orderType);

            if (newEvent != null)
            {
                SetCurrentProdItem(graph, newEvent);
                newEvent = (AMProdEvnt)graph.Caches[typeof(AMProdEvnt)].Insert(newEvent);
            }

            return newEvent;
        }

        public static AMProdEvnt BuildEvent(int eventType, string eventDescription, AMProdItem amProdItem)
        {
            return BuildEvent(eventType, eventDescription, amProdItem, null);
        }

        public static AMProdEvnt BuildEvent(int eventType, string eventDescription, AMProdItem amProdItem, Guid? refNoteID)
        {
            if (amProdItem == null || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                return null;
            }

            return BuildEvent(eventType, eventDescription, amProdItem.ProdOrdID, amProdItem.OrderType, refNoteID);
        }

        public static AMProdEvnt BuildEvent(int eventType, string eventDescription)
        {
            return new AMProdEvnt
            {
                EventType = eventType,
                Description = string.IsNullOrWhiteSpace(eventDescription)
                        ? ProductionEventType.GetEventTypeDescription(eventType)
                        : eventDescription
            };
        }

        public static AMProdEvnt BuildEvent(int eventType, string eventDescription, string prodOrdId, string orderType)
        {
            return BuildEvent(eventType, eventDescription, prodOrdId, orderType, null);
        }

        public static AMProdEvnt BuildEvent(int eventType, string eventDescription, string prodOrdId, string orderType, Guid? refNoteID)
        {
            if (string.IsNullOrWhiteSpace(prodOrdId) || string.IsNullOrWhiteSpace(orderType))
            {
                return null;
            }

            var newProdEvent = BuildEvent(eventType, eventDescription);
            newProdEvent.OrderType = orderType;
            newProdEvent.ProdOrdID = prodOrdId;
            newProdEvent.RefNoteID = refNoteID;
            return newProdEvent;
        }

        public static bool ProdEventExist(PXGraph graph, string prodOrdId, string orderType, int eventType, string eventDescription)
        {
            return GetAmProdEvnt(graph, prodOrdId, orderType, eventType, eventDescription) != null;
        }

        protected static AMProdEvnt GetAmProdEvnt(PXGraph graph, string prodOrdId, string orderType, int eventType, string eventDescription)
        {
            return PXSelect<AMProdEvnt,
                Where<AMProdEvnt.orderType, Equal<Required<AMProdEvnt.orderType>>,
                And<AMProdEvnt.prodOrdID, Equal<Required<AMProdEvnt.prodOrdID>>,
                    And <AMProdEvnt.eventType, Equal<Required<AMProdEvnt.eventType>>,
                        And<AMProdEvnt.description, Equal<Required<AMProdEvnt.description>>>>>>>.Select(graph, orderType, prodOrdId, eventType, eventDescription);
        }

        public static AMProdEvnt InsertReportPrintedEvent(PXGraph graph, AMProdItem amProdItem)
        {
            if (graph == null || amProdItem?.OrderType == null)
            {
                return null;
            }

            var newEvent = BuildEvent(ProductionEventType.ReportPrinted, null, amProdItem.ProdOrdID, amProdItem.OrderType);

            if (newEvent != null)
            {
                SetCurrentProdItem(graph, newEvent);
                newEvent = (AMProdEvnt)graph.Caches[typeof(AMProdEvnt)].Insert(newEvent);
            }

            return newEvent;
        }

        public static bool ValueChanged<TField>(PXCache cache, object row1, object row2, out string changeMsg)
            where TField : IBqlField
        {
            changeMsg = null;
            var value1 = cache.GetValue<TField>(row1);
            var value2 = cache.GetValue<TField>(row2);
            if (object.Equals(value1, value2))
            {
                return false;
            }

            changeMsg = Messages.GetLocal(Messages.FieldValueChanged, 
                PXUIFieldAttribute.GetDisplayName<TField>(cache),
                value1,
                value2);

            return true;
        }
    }
}
