using System;
using PX.Data;
using PX.Data.EP;
using System.Diagnostics;
using PX.TM;
using PX.Objects.EP;
using PX.SM;
using PX.Data.BQL;

namespace PX.Objects.EP
{
	[Serializable]
	public class EPRuleTree : EPRule
	{
		public string ExtName
		{
			get
			{
				return IsActive == true ? Name : string.Concat("(Inactive) ", Name);
			}
		}
	}

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(EPAssignmentMaint))]
	[DebuggerDisplay("Name={Name} WorkgroupID={WorkgroupID} RuleID={RuleID}")]
	[PXCacheName(Messages.EPRule)]
	public partial class EPRule : IBqlTable
	{
		#region RuleID
		public abstract class ruleID : PX.Data.BQL.BqlGuid.Field<ruleID> { }
		
		[PXSequentialNote(new Type[0], SuppressActivitiesCount = true, IsKey = true)]
		[PXUIField(DisplayName = "Rule ID")]
		public virtual Guid? RuleID { get; set; }
		#endregion

		#region AssignmentMapID
		public abstract class assignmentMapID : PX.Data.BQL.BqlInt.Field<assignmentMapID> { }
		
		[PXDBInt]
		[PXDBDefault(typeof(EPAssignmentMap.assignmentMapID))]
		[PXParent(typeof(Select<EPAssignmentMap, Where<EPAssignmentMap.assignmentMapID, Equal<Current<EPRule.assignmentMapID>>>>))]
		public virtual int? AssignmentMapID { get; set; }
		#endregion
		#region StepID
		public abstract class stepID : PX.Data.BQL.BqlGuid.Field<stepID> { }
		
		[PXDBGuid]
		[PXUIField(DisplayName = "Step ID")]
		public virtual Guid? StepID { get; set; }
		#endregion

		#region Sequence
		public abstract class sequence : PX.Data.BQL.BqlInt.Field<sequence> { }
		
		[PXDBInt]
		[PXUIField(DisplayName = "Seq.", Enabled = false)]
		[PXDefault(1)]
		public virtual int? Sequence { get; set; }
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual string Name { get; set; }
        #endregion

        #region StepName
        public abstract class stepname : PX.Data.BQL.BqlString.Field<stepname> { }
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Step")]
        public virtual string StepName { get; set; }
        #endregion

        #region Icon
        public abstract class icon : PX.Data.BQL.BqlString.Field<icon> { }

		[PXString(250)]
		public virtual string Icon { get; set; }
		#endregion
		#region RuleType
		public abstract class ruleType : PX.Data.BQL.BqlString.Field<ruleType> { }
		
		[PXDBString(1, IsFixed = true)]
		[EPRuleType.List()]
		[PXUIField(DisplayName = "Approver")]
		[PXDefault(EPRuleType.Direct)]
		public virtual string RuleType { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region ApproveType
		public abstract class approveType : PX.Data.BQL.BqlString.Field<approveType> { }

		[PXDBString(1, IsFixed = true)]
		[EPApproveType.List()]
		[PXUIField(DisplayName = "On Approval")]
		[PXDefault(EPApproveType.Wait)]
		public virtual string ApproveType { get; set; }
		#endregion
		#region EmptyStepType
		public abstract class emptyStepType : PX.Data.BQL.BqlString.Field<emptyStepType> { }

		[PXDBString(1, IsFixed = true)]
		[EPEmptyStepType.List()]
		[PXUIField(DisplayName = "If No Approver Found")]
		[PXDefault(EPEmptyStepType.Next)]
		public virtual string EmptyStepType { get; set; }
		#endregion
		#region ExecuteStep
		public abstract class executeStep : PX.Data.BQL.BqlString.Field<executeStep> { }

		[PXDBString(1, IsFixed = true)]
		[EPExecuteStep.List()]
		[PXUIField(DisplayName = "Execute Step")]
		[PXDefault(EPExecuteStep.Always)]
		public virtual string ExecuteStep { get; set; }
		#endregion

		#region ReasonForApprove
		public abstract class reasonForApprove : PX.Data.BQL.BqlString.Field<reasonForApprove> { }

		[PXDBString(1, IsFixed = true)]
		[EPReasonSettings.List()]
		[PXUIField(DisplayName = "Reason for Approval")]
		[PXDefault(EPReasonSettings.NotPrompted)]
		public virtual string ReasonForApprove { get; set; }
		#endregion

		#region ReasonForReject
		public abstract class reasonForReject : PX.Data.BQL.BqlString.Field<reasonForReject> { }

		[PXDBString(1, IsFixed = true)]
		[EPReasonSettings.List()]
		[PXUIField(DisplayName = "Reason for Rejection")]
		[PXDefault(EPReasonSettings.NotPrompted)]
		public virtual string ReasonForReject { get; set; }
		#endregion

		#region WaitTime
		public abstract class waitTime : PX.Data.BQL.BqlInt.Field<waitTime> { }
		
		[PXDefault(0)]
		[PXUIField(DisplayName = "Decision Wait Time", Visibility = PXUIVisibility.Visible)]
		[PXDBTimeSpanLong(Format = TimeSpanFormatType.DaysHoursMinites)]
		public virtual int? WaitTime { get; set; }
		#endregion		

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		public virtual int? WorkgroupID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXDBGuid()]
		[PXOwnerSelector(typeof(EPRule.workgroupID))]
		[PXUIField(DisplayName = "Employee")]
		public virtual Guid? OwnerID { get; set; }
		#endregion
		#region OwnerSource
		public abstract class ownerSource : PX.Data.BQL.BqlString.Field<ownerSource> { }

		[PXDBString(250)]
		[PXUIField(DisplayName = "Employee", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual string OwnerSource { get; set; }
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

	public static class EPAssignRuleType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { Direct, Document },
					new string[] { Messages.DirectEmployee, Messages.DocumentEmployee })
			{
				;
			}
		}

		public const string Direct = "D";
		public const string Document = "E";

		public class direct : PX.Data.BQL.BqlString.Constant<direct>
		{
			public direct() : base(Direct)
			{
			}
		}
		public class document : PX.Data.BQL.BqlString.Constant<document>
		{
			public document() : base(Document)
			{
			}
		}
	}

	public static class EPRuleType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { Direct, Document, Filter },
					new string[] { Messages.DirectEmployee, Messages.DocumentEmployee, Messages.FilterEmployee })
			{
				;
			}
		}

		public const string Direct = "D";
		public const string Document = "E";
		public const string Filter = "F";

		public class direct : PX.Data.BQL.BqlString.Constant<direct>
		{
			public direct() : base(Direct)
			{
			}
		}
		public class document : PX.Data.BQL.BqlString.Constant<document>
		{
			public document() : base(Document)
			{
			}
		}
		public class filter : PX.Data.BQL.BqlString.Constant<filter>
		{
			public filter() : base(Filter)
			{
			}
		}

	}

	public static class EPApproveType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new [] { Wait, Complete, Approve },
					new [] { Messages.WaitForApproval, Messages.CompleteStep, Messages.ApproveEntireDoc })
			{
			}
		}

		public const string Wait = "W";
		public const string Complete = "C";
		public const string Approve = "A";

		public class wait : PX.Data.BQL.BqlString.Constant<wait>
		{
			public wait() : base(Wait)
			{
			}
		}
		public class complete : PX.Data.BQL.BqlString.Constant<complete>
		{
			public complete() : base(Complete)
			{
			}
		}
		public class approve : PX.Data.BQL.BqlString.Constant<approve>
		{
			public approve() : base(Approve)
			{
			}
		}

	}

	public static class EPReasonSettings
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new [] { Optional, Required, NotPrompted },
					new [] {
						Messages.ApprovalReasonIsOptional,
						Messages.ApprovalReasonIsRequired,
						Messages.ApprovalReasonIsNotPrompted })
			{
			}
		}
		public const string Optional = "O";
		public const string Required = "R";
		public const string NotPrompted = "N";

		public class notPrompted : BqlString.Constant<notPrompted>
		{
			public notPrompted() : base(NotPrompted) { }
		}

		public class optional : BqlString.Constant<optional>
		{
			public optional() : base(Optional)
			{
			}
		}
		public class required : BqlString.Constant<required>
		{
			public required() : base(Required)
			{
			}
		}

		#region Obsolete

		[Obsolete("Use " + nameof(NotPrompted))]
		public const string NotRequired = NotPrompted;

		[Obsolete("Use " + nameof(notPrompted))]
		public class notRequired : Constant<string>
		{
			public notRequired() : base(NotRequired)
			{
			}
		}

		#endregion
	}

	public static class EPEmptyStepType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { Reject, Approve, Next },
					new string[] { Messages.RejectDoc, Messages.ApproveDoc, Messages.NextStep })
			{
			}
		}

		public const string Reject = "R";
		public const string Approve = "A";
		public const string Next = "N";

		public class reject : PX.Data.BQL.BqlString.Constant<reject>
		{
			public reject() : base(Reject)
			{
			}
		}
		public class approve : PX.Data.BQL.BqlString.Constant<approve>
		{
			public approve() : base(Approve)
			{
			}
		}
		public class next : PX.Data.BQL.BqlString.Constant<next>
		{
			public next() : base(Next)
			{
			}
		}

	}

	public static class EPExecuteStep
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { Always, IfNoApproversFoundatPreviousSteps },
					new string[] { Messages.Always, Messages.IfNoApproversFoundatPreviousSteps })
			{
			}
		}

		public const string Always = "A";
		public const string IfNoApproversFoundatPreviousSteps = "O";

		public class always : Constant<string>
		{
			public always() : base(Always)
			{
			}
		}
		public class ifNoApproversFoundatPreviousSteps : Constant<string>
		{
			public ifNoApproversFoundatPreviousSteps() : base(IfNoApproversFoundatPreviousSteps)
			{
			}
		}
	}
}
