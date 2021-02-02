using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;

namespace PX.Objects.FS
{
	[System.SerializableAttribute]
	public class FSPostDet : PX.Data.IBqlTable
	{
		#region BatchID
        public abstract class batchID : PX.Data.BQL.BqlInt.Field<batchID> { }

		[PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<FSPostBatch, Where<FSPostBatch.batchID, Equal<Current<FSPostDet.batchID>>>>))]
        [PXDBLiteDefault(typeof(FSPostBatch.batchID))]
        [PXUIField(DisplayName = "Batch ID")]
        public virtual int? BatchID { get; set; }
		#endregion
		#region PostDetID
        public abstract class postDetID : PX.Data.BQL.BqlInt.Field<postDetID> { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
        public virtual int? PostDetID { get; set; }
		#endregion
        #region PostID
        public abstract class postID : PX.Data.BQL.BqlInt.Field<postID> { }

        [PXDBInt]
        [PXUIField(Enabled = false)]
        public virtual int? PostID { get; set; }
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
        [PXUIField(DisplayName = "AR Reference Nbr.")]
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
        #region apDocType
        public abstract class apDocType : PX.Data.BQL.BqlString.Field<apDocType> { }

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "AP Document Type")]
        public virtual string APDocType { get; set; }
        #endregion
        #region apRefNbr
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
		#region Tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region MemoryHelper
        #region Mem_DocNbr
        public abstract class mem_DocNbr : PX.Data.BQL.BqlString.Field<mem_DocNbr> { }

        [PXString(15)]
        [PXUIField(DisplayName = "Document Nbr.", Enabled = false)]
        public virtual string Mem_DocNbr
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (APPosted == true)
                {
                    return APRefNbr;
                }
                else if (ARPosted == true)
                {
                    return ARRefNbr;
                }
                else if (INPosted == true)
                {
                    return INRefNbr;
                }
                else if (SOPosted == true)
                {
                    return SOOrderNbr;
                }
                else if (SOInvPosted == true)
                {
                    return SOInvRefNbr;
                }
                else if (PMPosted == true)
                {
                    return PMRefNbr;
                }

                return string.Empty;
            }
        }
        #endregion
        #region Mem_DocType
        public abstract class mem_DocType : PX.Data.BQL.BqlString.Field<mem_DocType> { }

        [PXString(3)]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
        public virtual string Mem_DocType
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (APPosted == true)
                {
                    return APDocType;
                }
                else if (ARPosted == true)
                {
                    return ARDocType;
                }
                else if (INPosted == true)
                {
                    return INDocType;
                }
                else if (SOPosted == true)
                {
                    return SOOrderType;
                }
                else if (SOInvPosted == true)
                {
                    return SOInvDocType;
                }
                else if (PMPosted == true)
                {
                    return PMDocType;
                }

                return string.Empty;
            }
        }
        #endregion
        #region PostDocType
        public abstract class postDocType : PX.Data.BQL.BqlString.Field<postDocType> { }

        [PXString]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
        public virtual string PostDocType
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (APPosted == true)
                {
                    return PX.Objects.CR.Messages.APInvoice;
                }
                else if (ARPosted == true)
                {
                    return PX.Objects.CR.Messages.ARInvoice;
                }
                else if (SOPosted == true)
                {
                    return PX.Objects.CR.Messages.SOrder;
                }
                else if (SOInvPosted == true)
                {
                    return PX.Objects.SO.Messages.SOInvoice;
                }
                else if (PMPosted == true)
                {
                    return PX.Objects.PM.Messages.Project;
                }
                else if (INPosted == true)
                {
                    return PX.Objects.IN.Messages.Issue;
                }

                return string.Empty;
            }
        }
        #endregion
        #region ReferenceNbr
        public abstract class postDocReferenceNbr : PX.Data.BQL.BqlString.Field<postDocReferenceNbr> { }

        [PXString]
        [PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
        public virtual string PostDocReferenceNbr
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (APPosted == true)
                {
                    return APDocType + ", " + APRefNbr;
                }
                else if (ARPosted == true)
                {
                    return ARDocType + ", " + ARRefNbr;
                }
                else if (SOPosted == true)
                {
                    return SOOrderType + ", " + SOOrderNbr;
                }
                else if (SOInvPosted == true)
                {
                    return SOInvDocType + ", " + SOInvRefNbr;
                }
                else if (PMPosted == true)
                {
                    return PMDocType + ", " + PMRefNbr;
                }
                else if (INPosted == true)
                {
                    return INDocType + ", " + INRefNbr;
                }

                return string.Empty;
            }
        }
        #endregion
        #region INReferenceNbr
        public abstract class iNPostDocReferenceNbr : PX.Data.BQL.BqlString.Field<iNPostDocReferenceNbr> { }

        [PXString]
        [PXUIField(DisplayName = "Issue Reference Nbr.", Enabled = false)]
        public virtual string INPostDocReferenceNbr
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (INPosted == true)
                {
                    return INDocType + ", " + INRefNbr;
                }

                return string.Empty;
            }
        }
        #endregion
        #region Mem_PostedIn
        public abstract class mem_PostedIn : PX.Data.BQL.BqlString.Field<mem_PostedIn> { }

        [PXString(2)]
        [PXUIField(DisplayName = "Post To", Enabled = false)]
        public virtual string Mem_PostedIn
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (APPosted == true)
                {
                    return ID.Batch_PostTo.AP;
                }
                else if (ARPosted == true)
                {
                    return ID.Batch_PostTo.AR;
                }
                else if (INPosted == true)
                {
                    return ID.Batch_PostTo.IN;
                }
                else if (SOPosted == true)
                {
                    return ID.Batch_PostTo.SO;
                }
                else if (SOInvPosted == true)
                {
                    return ID.Batch_PostTo.SI;
                }
                else if (PMPosted == true)
                {
                    return ID.Batch_PostTo.PM;
                }

                return string.Empty;
            }
        }
        #endregion
        #region InvoiceRefNbr
        public abstract class invoiceRefNbr : PX.Data.BQL.BqlString.Field<invoiceRefNbr> { }

        [PXString(15, IsUnicode = true)]
        [PXUIField(Visible = false)]
        public virtual string InvoiceRefNbr { get; set; }
        #endregion
        #region InvoiceDocType
        public abstract class invoiceDocType : PX.Data.IBqlField
        {
        }

        [PXString]
        [PXUIField(Visible = false)]
        public virtual string InvoiceDocType { get; set; }
        #endregion
        #region InvoiceReferenceNbr
        public abstract class invoiceReferenceNbr : PX.Data.BQL.BqlString.Field<invoiceReferenceNbr> { }

        [PXString]
        [PXUIField(DisplayName = "Invoice Nbr.", Enabled = false)]
        public virtual string InvoiceReferenceNbr
        {
            get
            {
                return (InvoiceDocType != null ? InvoiceDocType.Trim() + ", " : "") +
                       (InvoiceRefNbr != null ? InvoiceRefNbr.Trim() : "");
            }
        }
        #endregion
        #region BatchNbr
        public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

        [PXString]
        [PXUIField(DisplayName = "Batch Number", Enabled = false)]
        public virtual string BatchNbr { get; set; }
        #endregion
        #endregion
    }
}