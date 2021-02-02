using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Discount;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public partial class FSDiscountDetail : PX.Data.IBqlTable, IDiscountDetail
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSDiscountDetail>.By<srvOrdType, refNbr, recordID>
        {
            public static FSDiscountDetail Find(PXGraph graph, string srvOrdType, string refNbr, int? recordID)
                => FindBy(graph, srvOrdType, refNbr, recordID);
        }
        public static class FK
        {
            public class FreeItem : InventoryItem.PK.ForeignKeyOf<FSDiscountDetail>.By<freeItemID> { }
        }
        #endregion

        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

        [PXDBIdentity]
        public virtual Int32? RecordID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBUShort()]
        public virtual ushort? LineNbr { get; set; }
         #endregion
        #region SkipDiscount
        public abstract class skipDiscount : PX.Data.BQL.BqlBool.Field<skipDiscount> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIEnabled(typeof(Where<type, NotEqual<DiscountType.ExternalDocumentDiscount>, And<discountID, IsNotNull>>))]
        [PXUIField(DisplayName = "Skip Discount", Enabled = true)]
        public virtual Boolean? SkipDiscount { get; set; }
        #endregion
        #region EntityType
        public abstract class entityType : ListField_PostDoc_EntityType
        {
        }

        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault]
        [entityType.ListAtrribute]
        [PXUIField(DisplayName = "EntityType", Visibility = PXUIVisibility.Visible, Visible = true)]
        public virtual String EntityType { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Enabled = false)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDBDefault(typeof(FSAppointment.refNbr))]
        [PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
        public virtual string RefNbr { get; set; }
        #endregion
        #region DiscountID
        public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Discount Code")]
        [PXUIEnabled(typeof(Where<type, NotEqual<DiscountType.ExternalDocumentDiscount>>))]
        [PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, NotEqual<DiscountType.LineDiscount>>>))]
        public virtual String DiscountID { get; set; }
        #endregion
        #region DiscountSequenceID
        public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Sequence ID")]
        [PXUIEnabled(typeof(Where<type, NotEqual<DiscountType.ExternalDocumentDiscount>>))]
        [PXSelector(typeof(Search<DiscountSequence.discountSequenceID, Where<DiscountSequence.isActive, Equal<True>, And<DiscountSequence.discountID, Equal<Current<discountID>>>>>))]
        public virtual String DiscountSequenceID { get; set; }
        #endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

        [PXDBString(1, IsFixed = true)]
        [PXDefault()]
        [DiscountType.List()]
        [PXUIField(DisplayName = "Type", Enabled = false)]
        public virtual string Type { get; set; }
        #endregion
        #region ManualOrder
        public abstract class manualOrder : PX.Data.BQL.BqlShort.Field<manualOrder> { }

        [PXDBShort()]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public virtual Int16? ManualOrder { get; set; }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        [PXDBLong()]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion
        #region DiscountableAmt
        public abstract class discountableAmt : PX.Data.BQL.BqlDecimal.Field<discountableAmt> { }

        [PXDBDecimal(4)]
        public virtual Decimal? DiscountableAmt { get; set; }
         #endregion
        #region CuryDiscountableAmt
        public abstract class curyDiscountableAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountableAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(discountableAmt))]
        [PXUIField(DisplayName = "Discountable Amt.", Enabled = false)]
        public virtual Decimal? CuryDiscountableAmt { get; set; }
        #endregion
        #region DiscountableQty
        public abstract class discountableQty : PX.Data.BQL.BqlDecimal.Field<discountableQty> { }

        [PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Discountable Qty.", Enabled = false)]
        public virtual Decimal? DiscountableQty { get; set; }
        #endregion
        #region DiscountAmt
        public abstract class discountAmt : PX.Data.BQL.BqlDecimal.Field<discountAmt> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscountAmt { get; set; }
        #endregion
        #region CuryDiscountAmt
        public abstract class curyDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(discountAmt))]
        [PXUIEnabled(typeof(Where<type, Equal<DiscountType.DocumentDiscount>, Or<type, Equal<DiscountType.ExternalDocumentDiscount>>>))]
        [PXUIField(DisplayName = "Discount Amt.")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryDiscountAmt { get; set; }
        #endregion
        #region DiscountPct
        public abstract class discountPct : PX.Data.BQL.BqlDecimal.Field<discountPct> { }

        [PXDBDecimal(2)]
        [PXUIEnabled(typeof(Where<type, Equal<DiscountType.DocumentDiscount>, Or<type, Equal<DiscountType.ExternalDocumentDiscount>>>))]
        [PXUIField(DisplayName = "Discount Percent")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? DiscountPct { get; set; }
        #endregion
        #region FreeItemID
        public abstract class freeItemID : PX.Data.BQL.BqlInt.Field<freeItemID> { }

        [Inventory(DisplayName = "Free Item", Enabled = false)]
        [PXForeignReference(typeof(FK.FreeItem))]
        public virtual Int32? FreeItemID { get; set; }
        #endregion
        #region FreeItemQty
        public abstract class freeItemQty : PX.Data.BQL.BqlDecimal.Field<freeItemQty> { }

        [PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Free Item Qty.", Enabled = false)]
        public virtual Decimal? FreeItemQty { get; set; }
        #endregion
        #region IsManual
        public abstract class isManual : PX.Data.BQL.BqlBool.Field<isManual> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Discount", Enabled = false)]
        public virtual Boolean? IsManual { get; set; }
        #endregion
        #region IsOrigDocDiscount
        public abstract class isOrigDocDiscount : PX.Data.BQL.BqlBool.Field<isOrigDocDiscount> { }

        [PXBool()]
        [PXFormula(typeof(False))]
        public virtual Boolean? IsOrigDocDiscount { get; set; }
        #endregion
        #region ExtDiscCode
        public abstract class extDiscCode : PX.Data.BQL.BqlString.Field<extDiscCode> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "External Discount Code")]
        public virtual String ExtDiscCode { get; set; }
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        [PXDefault(typeof(Search<DiscountSequence.description, Where<DiscountSequence.discountID, Equal<Current<discountID>>, 
                             And<DiscountSequence.discountSequenceID, Equal<Current<discountSequenceID>>>>>), 
                   PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String Description { get; set; }
        #endregion

        #region System Columns
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp()]
        public virtual Byte[] tstamp { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #endregion
    }
}
