using System;
using PX.Data;

namespace PX.Objects.AR
{
	/// <summary>
	/// Header of Dunning Letter
	/// </summary>
	[Serializable]
    [PXPrimaryGraph(typeof(ARDunningLetterUpdate))] //for notification
    [PXEMailSource]
	[PXCacheName(Messages.DunningLetter)]
    public partial class ARDunningLetter : PX.Data.IBqlTable
	{
		#region DunningLetterID
		public abstract class dunningLetterID : PX.Data.BQL.BqlInt.Field<dunningLetterID> { }
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Dunning Letter ID", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(ARDunningLetter.dunningLetterID), 
			new Type[]
			{
				typeof(ARDunningLetter.dunningLetterID),
				typeof(ARDunningLetter.branchID_Branch_branchCD),
				typeof(ARDunningLetter.bAccountID_Customer_acctCD),
				typeof(ARDunningLetter.dunningLetterDate),
				typeof(ARDunningLetter.dunningLetterLevel),
				typeof(ARDunningLetter.deadline)
			})]
		public virtual Int32? DunningLetterID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public abstract class branchID_Branch_branchCD : PX.Data.BQL.BqlString.Field<branchID_Branch_branchCD> { }
		[PXDBInt()]
		[PXDefault()]
        [PXUIField(DisplayName = "Branch")]
		[PXSelector(typeof(Search<GL.Branch.branchID, Where<GL.Branch.branchID, Equal<Current<ARDunningLetter.branchID>>>>), DescriptionField = typeof(GL.Branch.branchCD), ValidateValue = false)]
        public virtual Int32? BranchID
		{
			get;
			set;
		}
		#endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public abstract class bAccountID_Customer_acctCD : PX.Data.BQL.BqlString.Field<bAccountID_Customer_acctCD> { }
        [PXDBInt()]
        [PXDefault()]
		[PXUIField(DisplayName = "Customer", IsReadOnly=true)]
		[PXSelector(typeof(Search<Customer.bAccountID, Where<Customer.bAccountID, Equal<Current<ARDunningLetter.bAccountID>>>>), DescriptionField = typeof(Customer.acctCD), ValidateValue = false)]
		public virtual Int32? BAccountID
		{
			get;
			set;
		}
        #endregion
        #region DunningLetterDate
		public abstract class dunningLetterDate : PX.Data.BQL.BqlDateTime.Field<dunningLetterDate> { }
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Dunning Letter Date", IsReadOnly=true)]
		public virtual DateTime? DunningLetterDate
		{
			get;
			set;
		}
		#endregion
        #region Deadline
        public abstract class deadline : PX.Data.BQL.BqlDateTime.Field<deadline> { }
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Deadline")]
        public virtual DateTime? Deadline
		{
			get;
			set;
		}
        #endregion
        #region DunningLetterLevel
        public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
		[PXDBInt()]
		[PXDefault()]
        [PXUIField(DisplayName = Messages.DunningLetterLevel, IsReadOnly = true)]
        public virtual Int32? DunningLetterLevel
		{
			get;
			set;
		}
		#endregion
        #region Printed
        public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Printed
		{
			get;
			set;
		}
        #endregion
        #region DontPrint
        public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Don't Print")]
        public virtual Boolean? DontPrint
		{
			get;
			set;
		}
        #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Released
		{
			get;
			set;
		}
        #endregion
        #region Voided
        public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Voided
		{
			get;
			set;
		}
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        [PXString(1, IsFixed = true)]
        [PXDefault("D")]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXStringList(new string[] { "D", "R", "V" },
                new string[] { Messages.Draft, Messages.Released, Messages.Voided })]
        public virtual String Status
        {
            get
            {
                if (Voided == true)
                    return "V";
                if (Released == true)
                    return "R";
                return "D";
            }
            set { }
        }
        #endregion
        #region DetailsCount
        public abstract class detailsCount : PX.Data.BQL.BqlInt.Field<detailsCount> { }
        [PXInt]
        [PXUIField(DisplayName="Number of Documents")]
        public virtual Int32? DetailsCount
		{
			get;
			set;
		}
        #endregion
        #region FeeDocType
        public abstract class feeDocType : PX.Data.BQL.BqlString.Field<feeDocType> { }
        [PXDBString(3, IsFixed = true)]
        [ARDocType.List()]
        [PXUIField(DisplayName = "Fee Type")]
        public virtual String FeeDocType
		{
			get;
			set;
		}
        #endregion
        #region FeeRefNbr
        public abstract class feeRefNbr : PX.Data.BQL.BqlString.Field<feeRefNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXSelector(typeof(Search<ARInvoice.refNbr, Where<ARInvoice.docType, Equal<Current<ARDunningLetter.feeDocType>>, And<ARInvoice.refNbr, Equal<Current<ARDunningLetter.feeRefNbr>>>>>),ValidateValue=false)]
        [PXUIField(DisplayName = "Fee Reference Nbr.", Enabled=false)]
        public virtual String FeeRefNbr
		{
			get;
			set;
		}
        #endregion

        #region Emailed
        public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Emailed
		{
			get;
			set;
		}
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        [PXNote(new Type[0])]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
        #endregion
        #region DontEmail
        public abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Don't Email")]
        public virtual Boolean? DontEmail
		{
			get;
			set;
		}
        #endregion

        #region Consolidated
        public abstract class consolidated : PX.Data.BQL.BqlBool.Field<consolidated> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Consolidated
		{
			get;
			set;
		}
        #endregion

        #region LastLevel
        public abstract class lastLevel : PX.Data.BQL.BqlBool.Field<lastLevel> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? LastLevel
		{
			get;
			set;
		}
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
		{
			get;
			set;
		}
        #endregion
    }

}
