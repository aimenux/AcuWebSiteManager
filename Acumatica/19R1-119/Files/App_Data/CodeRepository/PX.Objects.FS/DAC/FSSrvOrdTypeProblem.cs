using System;
using PX.Data;
﻿
namespace PX.Objects.FS
 {
     [System.SerializableAttribute]
     public class FSSrvOrdTypeProblem : PX.Data.IBqlTable
     {
         #region SrvOrdType
         public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
         [PXDBString(4, IsKey = true, IsFixed = true)]
         [PXParent(typeof(Select<FSSrvOrdType,Where<FSSrvOrdType.srvOrdType,Equal<Current<FSSrvOrdTypeProblem.srvOrdType>>>>))]
         [PXDBLiteDefault(typeof(FSSrvOrdType.srvOrdType))]
         public virtual string SrvOrdType { get; set; }
         #endregion
         #region ProblemID
         public abstract class problemID : PX.Data.BQL.BqlInt.Field<problemID> { }
         [PXDBInt(IsKey = true)]
         [PXDefault]
         [PXUIField(DisplayName = "Problem ID")]
         [PXSelector(typeof(Search<FSProblem.problemID>), SubstituteKey = typeof(FSProblem.problemCD))]
         public virtual int? ProblemID { get; set; }
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
     }
 }