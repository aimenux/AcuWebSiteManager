using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Discount;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [PXProjection(typeof(Select<FSDiscountDetail,
                            Where<FSDiscountDetail.entityType, Equal<FSDiscountDetail.entityType.Service_Order>>>),
                    Persistent = true)]
    [Serializable]
    [PXBreakInheritance]
    public partial class FSServiceOrderDiscountDetail : FSDiscountDetail, IDiscountDetail
    {
        #region Keys
        public new class PK : PrimaryKeyOf<FSServiceOrderDiscountDetail>.By<srvOrdType, refNbr, recordID>
        {
            public static FSServiceOrderDiscountDetail Find(PXGraph graph, string srvOrdType, string refNbr, int? recordID)
                => FindBy(graph, srvOrdType, refNbr, recordID);
        }
        public new static class FK
        {
            public class FreeItem : InventoryItem.PK.ForeignKeyOf<FSServiceOrderDiscountDetail>.By<freeItemID> { }
        }
        #endregion

        #region RecordID
        public new abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

        [PXDBIdentity]
        public override Int32? RecordID { get; set; }
        #endregion
        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBUShort()]
        [PXLineNbr(typeof(FSServiceOrder))]
        //[PXLineNbr(typeof(SOOrder), ReuseGaps = true)]
        public override ushort? LineNbr { get; set; }
         #endregion
        #region SkipDiscount
        public new abstract class skipDiscount : PX.Data.BQL.BqlBool.Field<skipDiscount> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIEnabled(typeof(Where<type, NotEqual<DiscountType.ExternalDocumentDiscount>, And<discountID, IsNotNull>>))]
        [PXUIField(DisplayName = "Skip Discount", Enabled = true)]
        public override Boolean? SkipDiscount { get; set; }
        #endregion
        #region EntityType
        public new abstract class entityType : ListField_PostDoc_EntityType
        {
        }

        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault]
        [entityType.ListAtrribute]
        [PXUIField(DisplayName = "EntityType", Visibility = PXUIVisibility.Visible, Visible = true)]
        public override String EntityType { get; set; }
        #endregion
        #region SrvOrdType
        public new abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXDBDefault(typeof(FSServiceOrder.srvOrdType))]
        [PXUIField(DisplayName = "Order Type", Enabled = false)]
        public override string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDBDefault(typeof(FSServiceOrder.refNbr))]
        [PXParent(typeof(Select<FSServiceOrder, Where<FSServiceOrder.srvOrdType, Equal<Current<srvOrdType>>, And<FSServiceOrder.refNbr, Equal<Current<refNbr>>>>>))]
        //[PXParent(typeof(FK.Order))]
        [PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
        public override string RefNbr { get; set; }
        #endregion
        #region DiscountID
        public new abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Discount Code")]
        [PXUIEnabled(typeof(Where<type, NotEqual<DiscountType.ExternalDocumentDiscount>>))]
        [PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, NotEqual<DiscountType.LineDiscount>>>))]
        public override String DiscountID { get; set; }
        #endregion
        #region DiscountSequenceID
        public new abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Sequence ID")]
        [PXUIEnabled(typeof(Where<type, NotEqual<DiscountType.ExternalDocumentDiscount>>))]
        [PXSelector(typeof(Search<DiscountSequence.discountSequenceID, Where<DiscountSequence.isActive, Equal<True>, And<DiscountSequence.discountID, Equal<Current<discountID>>>>>))]
        public override String DiscountSequenceID { get; set; }
        #endregion
        #region Type
        public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }

        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        [DiscountType.List()]
        [PXUIField(DisplayName = "Type", Enabled = false)]
        public override string Type { get; set; }
        #endregion
        #region ManualOrder
        public new abstract class manualOrder : PX.Data.BQL.BqlShort.Field<manualOrder> { }

        [PXDBShort()]
        [PXLineNbr(typeof(FSServiceOrder))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public override Int16? ManualOrder { get; set; }
        #endregion
        #region CuryInfoID
        public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        [PXDBLong()]
        [CurrencyInfo(typeof(FSServiceOrder.curyInfoID))]
        public override Int64? CuryInfoID { get; set; }
        #endregion
        #region DiscountableAmt
        public new abstract class discountableAmt : PX.Data.BQL.BqlDecimal.Field<discountableAmt> { }

        [PXDBDecimal(4)]
        public override Decimal? DiscountableAmt { get; set; }
         #endregion
        #region CuryDiscountableAmt
        public new abstract class curyDiscountableAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountableAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(discountableAmt))]
        [PXUIField(DisplayName = "Discountable Amt.", Enabled = false)]
        public override Decimal? CuryDiscountableAmt { get; set; }
        #endregion
        #region DiscountableQty
        public new abstract class discountableQty : PX.Data.BQL.BqlDecimal.Field<discountableQty> { }

        [PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Discountable Qty.", Enabled = false)]
        public override Decimal? DiscountableQty { get; set; }
        #endregion
        #region DiscountAmt
        public new abstract class discountAmt : PX.Data.BQL.BqlDecimal.Field<discountAmt> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? DiscountAmt { get; set; }
        #endregion
        #region CuryDiscountAmt
        public new abstract class curyDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(discountAmt))]
        [PXUIEnabled(typeof(Where<type, Equal<DiscountType.DocumentDiscount>, Or<type, Equal<DiscountType.ExternalDocumentDiscount>>>))]
        [PXUIField(DisplayName = "Discount Amt.")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? CuryDiscountAmt { get; set; }
        #endregion
        #region DiscountPct
        public new abstract class discountPct : PX.Data.BQL.BqlDecimal.Field<discountPct> { }

        [PXDBDecimal(2)]
        [PXUIEnabled(typeof(Where<type, Equal<DiscountType.DocumentDiscount>, Or<type, Equal<DiscountType.ExternalDocumentDiscount>>>))]
        [PXUIField(DisplayName = "Discount Percent")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public override Decimal? DiscountPct { get; set; }
        #endregion
        #region FreeItemID
        public new abstract class freeItemID : PX.Data.BQL.BqlInt.Field<freeItemID> { }

        [Inventory(DisplayName = "Free Item", Enabled = false)]
        [PXForeignReference(typeof(FK.FreeItem))]
        public override Int32? FreeItemID { get; set; }
        #endregion
        #region FreeItemQty
        public new abstract class freeItemQty : PX.Data.BQL.BqlDecimal.Field<freeItemQty> { }

        [PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Free Item Qty.", Enabled = false)]
        public override Decimal? FreeItemQty { get; set; }
        #endregion
        #region IsManual
        public new abstract class isManual : PX.Data.BQL.BqlBool.Field<isManual> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Discount", Enabled = false)]
        public override Boolean? IsManual { get; set; }
        #endregion
        #region IsOrigDocDiscount
        public new abstract class isOrigDocDiscount : PX.Data.BQL.BqlBool.Field<isOrigDocDiscount> { }

        [PXBool()]
        [PXFormula(typeof(False))]
        public override Boolean? IsOrigDocDiscount { get; set; }
        #endregion
        #region ExtDiscCode
        public new abstract class extDiscCode : PX.Data.BQL.BqlString.Field<extDiscCode> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "External Discount Code")]
        public override String ExtDiscCode { get; set; }
        #endregion
        #region Description
        public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        [PXDefault(typeof(Search<DiscountSequence.description, Where<DiscountSequence.discountID, Equal<Current<discountID>>, 
                             And<DiscountSequence.discountSequenceID, Equal<Current<discountSequenceID>>>>>), 
                          PersistingCheck = PXPersistingCheck.Nothing)]
        public override String Description { get; set; }
        #endregion

        #region System Columns
        #region tstamp
        public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp()]
        public override Byte[] tstamp { get; set; }
        #endregion
        #region CreatedByID
        public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID()]
        public override Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID()]
        public override String CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime()]
        public override DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID()]
        public override Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID()]
        public override String LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime()]
        public override DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #endregion
    }
}
