using System;
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSCustomFieldAppointment : PX.Data.IBqlTable
    {
        #region CustomFieldAppointmentID
        public abstract class customFieldAppointmentID : PX.Data.BQL.BqlInt.Field<customFieldAppointmentID> { }

        [PXDBIdentity(IsKey = true)]
        [PXUIField(Enabled = false)]
        public virtual int? CustomFieldAppointmentID { get; set; }
        #endregion
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active { get; set; }
        #endregion
        #region FieldDescr
        public abstract class fieldDescr : PX.Data.BQL.BqlString.Field<fieldDescr> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string FieldDescr { get; set; }
        #endregion
        #region FieldImg
        public abstract class fieldImg : PX.Data.BQL.BqlString.Field<fieldImg> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Field Image")]
        public virtual string FieldImg { get; set; }
        #endregion
        #region FieldName
        public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }

        [PXDBString(60, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Field Name")]
        public virtual string FieldName { get; set; }
        #endregion
        #region Position
        public abstract class position : PX.Data.BQL.BqlInt.Field<position> { }

        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Position")]
        public virtual int? Position { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "CreatedByID")]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
    }
}