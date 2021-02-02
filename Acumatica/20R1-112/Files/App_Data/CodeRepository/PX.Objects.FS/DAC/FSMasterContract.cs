using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.GL;
﻿
namespace PX.Objects.FS
{	
	[System.SerializableAttribute]
    [PXPrimaryGraph(typeof(MasterContractMaint))]
    public class FSMasterContract : PX.Data.IBqlTable
	{
		#region MasterContractID
		public abstract class masterContractID : PX.Data.BQL.BqlInt.Field<masterContractID> { }
		[PXDBIdentity]
		[PXUIField(Enabled = false, Visible = false)]
        public virtual int? MasterContractID { get; set; }
		#endregion
		#region MasterContractCD
		public abstract class masterContractCD : PX.Data.BQL.BqlString.Field<masterContractCD> { }
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Master Contract ID")]
        [PXSelector(typeof(Search2<FSMasterContract.masterContractCD,
                LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSMasterContract.customerID>>>,
                Where<Customer.bAccountID, IsNull,
                    Or<Match<Customer, Current<AccessInfo.userName>>>>>),
                    new Type[]{
                        typeof(FSMasterContract.masterContractCD),
                        typeof(FSMasterContract.descr),
                        typeof(FSMasterContract.customerID),
                        typeof(FSMasterContract.branchID)})]
        public virtual string MasterContractCD { get; set; }
		#endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        [PXDBLocalizableString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
        #endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDBInt]
        [PXDefault]
		[PXUIField(DisplayName = "Customer")]
        [FSSelectorBAccountTypeCustomerOrCombined]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
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
        [PXUIField(DisplayName = "Created On")]
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
        [PXUIField(DisplayName = "Last Modified On")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
		#endregion
	}
}
