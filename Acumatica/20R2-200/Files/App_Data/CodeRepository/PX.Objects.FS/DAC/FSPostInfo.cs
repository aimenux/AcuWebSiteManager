using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
	public class FSPostInfo : PX.Data.IBqlTable
	{
		#region PostID
		public abstract class postID : PX.Data.BQL.BqlInt.Field<postID> { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
        public virtual int? PostID { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt]
        public virtual int? SOID { get; set; }
        #endregion
        #region SOFields
        #region SOPosted
        public abstract class sOPosted : PX.Data.BQL.BqlBool.Field<sOPosted> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Invoiced through Sales Order")]
        public virtual bool? SOPosted { get; set; }
		#endregion
		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Sales Order Type")]
        public virtual string SOOrderType { get; set; }
		#endregion
		#region SOOrderNbr
		public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Sales Order Nbr.")]
        public virtual string SOOrderNbr { get; set; }
		#endregion
		#region SOLineNbr
		public abstract class sOLineNbr : PX.Data.BQL.BqlInt.Field<sOLineNbr> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Sales Order Line Nbr.")]
        public virtual int? SOLineNbr { get; set; }
		#endregion
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
		#endregion
        #region ARFields
        #region ARPosted
        public abstract class aRPosted : PX.Data.BQL.BqlBool.Field<aRPosted> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Invoiced through AR")]
        public virtual bool? ARPosted { get; set; }
        #endregion
        #region ARDocType
        public abstract class arDocType : PX.Data.BQL.BqlString.Field<arDocType> { }

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "AR Document Type")]
        public virtual string ARDocType { get; set; }
        #endregion
        #region ARRefNbr
        public abstract class arRefNbr : PX.Data.BQL.BqlString.Field<arRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "AR Ref. Nbr.")]
        public virtual string ARRefNbr { get; set; }
        #endregion
        #region ARLineNbr
        public abstract class aRLineNbr : PX.Data.BQL.BqlInt.Field<aRLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "AR Line Nbr.")]
        public virtual int? ARLineNbr { get; set; }
        #endregion
        #endregion
        #region APFields
        #region APPosted
        public abstract class aPPosted : PX.Data.BQL.BqlBool.Field<aPPosted> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Invoiced through AP")]
        public virtual bool? APPosted { get; set; }
        #endregion
        #region APDocType
        public abstract class apDocType : PX.Data.BQL.BqlString.Field<apDocType> { }

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "AP Document Type")]
        public virtual string APDocType { get; set; }
        #endregion
        #region APRefNbr
        public abstract class apRefNbr : PX.Data.BQL.BqlString.Field<apRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "AP Reference Nbr.")]
        public virtual string APRefNbr { get; set; }
        #endregion
        #region APLineNbr
        public abstract class aPLineNbr : PX.Data.BQL.BqlInt.Field<aPLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "AP Line Nbr.")]
        public virtual int? APLineNbr { get; set; }
        #endregion
        #endregion
        #region INFields
        #region INPosted
        public abstract class iNPosted : PX.Data.BQL.BqlBool.Field<iNPosted> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Invoiced through IN")]
        public virtual bool? INPosted { get; set; }
        #endregion
        #region INDocType
        public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }

        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "IN Document Type")]
        public virtual string INDocType { get; set; }
        #endregion
        #region INRefNbr
        public abstract class iNRefNbr : PX.Data.BQL.BqlString.Field<iNRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "IN Reference Nbr.")]
        public virtual string INRefNbr { get; set; }
        #endregion
        #region INLineNbr
        public abstract class iNLineNbr : PX.Data.BQL.BqlInt.Field<iNLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "IN Line Nbr.")]
        public virtual int? INLineNbr { get; set; }
        #endregion
        #endregion
        #region SOInvoiceFields
        #region SOInvoice
        #region SOInvPosted
        public abstract class sOInvPosted : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Invoiced through SO Invoice")]
        public virtual bool? SOInvPosted { get; set; }
        #endregion
        #region SOInvDocType
        public abstract class sOInvDocType : PX.Data.IBqlField
        {
        }

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "SO Invoice Document Type")]
        public virtual string SOInvDocType { get; set; }
        #endregion
        #region SOInvRefNbr
        public abstract class sOInvRefNbr : PX.Data.IBqlField
        {
        }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "SO Invoice Ref. Nbr.")]
        public virtual string SOInvRefNbr { get; set; }
        #endregion
        #region SOInvLineNbr
        public abstract class sOInvLineNbr : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "SO Invoice Line Nbr.")]
        public virtual int? SOInvLineNbr { get; set; }
        #endregion
        #endregion
        #endregion
        #region PMFields
        #region PMPosted
        public abstract class pMPosted : PX.Data.BQL.BqlBool.Field<pMPosted> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Invoiced through PM")]
        public virtual bool? PMPosted { get; set; }
        #endregion
        #region PMDocType
        public abstract class pMDocType : PX.Data.BQL.BqlString.Field<pMDocType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "PM Document Type")]
        public virtual string PMDocType { get; set; }
        #endregion
        #region PMRefNbr
        public abstract class pMRefNbr : PX.Data.BQL.BqlString.Field<pMRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PM Reference Nbr.")]
        public virtual string PMRefNbr { get; set; }
        #endregion
        #region PMTranID
        public abstract class pMTranID : PX.Data.BQL.BqlLong.Field<pMTranID> { }

        [PXDBLong()]
        [PXUIField(DisplayName = "PM Tran ID")]
        public virtual Int64? PMTranID { get; set; }
        #endregion
        #endregion

        #region Methods
        public virtual bool isPosted()
        {
            return APPosted == true || SOPosted == true || ARPosted == true || INPosted == true || PMPosted == true || SOInvPosted == true || PMPosted == true;
        }
        #endregion
    }
}
