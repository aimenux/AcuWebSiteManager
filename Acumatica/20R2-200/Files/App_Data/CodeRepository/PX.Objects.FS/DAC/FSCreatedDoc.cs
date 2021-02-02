using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSCreatedDoc : PX.Data.IBqlTable
    {
        #region BatchID
        public abstract class batchID : PX.Data.BQL.BqlInt.Field<batchID> { }

        [PXDBInt]
        public virtual int? BatchID { get; set; }
        #endregion
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? RecordID { get; set; }
        #endregion
        #region PostTo
        public abstract class postTo : PX.Data.BQL.BqlString.Field<postTo> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Document", Enabled = false)]
        public virtual string PostTo { get; set; }
        #endregion
        #region CreatedDocType
        public abstract class createdDocType : PX.Data.BQL.BqlString.Field<createdDocType> { }

        [PXDBString(4)]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
        public virtual string CreatedDocType { get; set; }
        #endregion
        #region CreatedRefNbr
        public abstract class createdRefNbr : PX.Data.BQL.BqlString.Field<createdRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Document Nbr.", Enabled = false)]
        public virtual string CreatedRefNbr { get; set; }
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
    }
}
