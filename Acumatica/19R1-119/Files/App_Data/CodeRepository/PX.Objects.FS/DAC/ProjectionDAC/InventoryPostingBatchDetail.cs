using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    #region PXProjection
    [Serializable]
    [PXProjection(typeof(
            Select2<FSAppointmentInventoryItem,
                InnerJoin<FSAppointment, 
                    On<FSAppointment.appointmentID, Equal<FSAppointmentInventoryItem.appointmentID>>,
                InnerJoin<FSAppointmentDet,
                    On<FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>,
                        And<FSAppointmentDet.sODetID, Equal<FSAppointmentInventoryItem.sODetID>>>,
                InnerJoin<FSPostInfo,
                    On<FSAppointmentInventoryItem.postID, Equal<FSPostInfo.postID>>,
                InnerJoin<FSPostDet,
                    On<FSPostDet.postID, Equal<FSPostInfo.postID>>,
                InnerJoin<FSServiceOrder,
                    On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSServiceOrder.billCustomerID>>,
                InnerJoin<FSAddress,
                    On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
                LeftJoin<FSGeoZonePostalCode,
                    On<FSGeoZonePostalCode.postalCode, Equal<FSAddress.postalCode>>,
                LeftJoin<FSGeoZone,
                    On<FSGeoZone.geoZoneID, Equal<FSGeoZonePostalCode.geoZoneID>>>>>>>>>>>,
                Where<FSAppointmentInventoryItem.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>>>))]
    #endregion
    public class InventoryPostingBatchDetail : PostingBatchDetail
    {
        #region AppointmentInventoryItemID
        public abstract class appointmentInventoryItemID : PX.Data.BQL.BqlInt.Field<appointmentInventoryItemID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentInventoryItem.appDetID))]
        public virtual int? AppointmentInventoryItemID { get; set; }
        #endregion
        #region SODetID
        public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentDet.sODetID))]
        [PXUIField(DisplayName = "Line Ref.")]
        [PXSelector(typeof(Search<FSSODet.sODetID,
                       Where<
                           FSSODet.sOID, Equal<Current<InventoryPostingBatchDetail.appointmentID>>,
                           And<FSSODet.status, NotEqual<ListField_Status_AppointmentDet.Canceled>,
                           And<FSSODet.status, NotEqual<ListField_Status_AppointmentDet.Completed>,
                       And<
                           Where<
                               FSSODet.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Service>,
                               Or<FSSODet.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Comment_Service>,
                               Or<FSSODet.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Instruction_Service>,
                               Or<FSSODet.lineType, Equal<ListField_LineType_Service_ServiceTemplate.NonStockItem>>>>>>>>>>), SubstituteKey = typeof(FSSODet.lineRef))]
        public virtual int? SODetID { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentInventoryItem.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID")]
        [PXSelector(typeof(Search<InventoryItem.inventoryID>), SubstituteKey = typeof(InventoryItem.inventoryCD))]
        public virtual int? InventoryID { get; set; }
        #endregion
    }
}
