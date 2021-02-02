using PX.SM;
using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CR
{
    [Serializable]
    public class CROpportunityClassProbability : IBqlTable
    {
        #region ClassID
        public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

        [PXDBString(10, IsKey = true, IsUnicode = true)]
        [PXDBDefault(typeof(CROpportunityClass.cROpportunityClassID))]
        [PXParent(typeof(Select<CROpportunityClass,
            Where<CROpportunityClass.cROpportunityClassID, Equal<Current<classID>>>>))]
        public virtual string ClassID { get; set; }
        #endregion

        #region StageID

        public abstract class stageID : PX.Data.BQL.BqlString.Field<stageID> { }

        [PXDBString(2, IsKey = true)]
        [PXDBDefault(typeof(CROpportunityProbability.stageCode))]
        [PXParent(typeof(Select<CROpportunityProbability,
            Where<CROpportunityProbability.stageCode, Equal<Current<stageID>>>>))]
        public virtual string StageID { get; set; }

        #endregion

        #region tstamp

        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual Byte[] tstamp { get; set; }

        #endregion

        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        public virtual String CreatedByScreenID { get; set; }

        #endregion

        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }

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
        public virtual String LastModifiedByScreenID { get; set; }

        #endregion

        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }

        #endregion
    }
}