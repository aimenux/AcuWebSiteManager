using System.Collections;
using System.Collections.Generic;
using PX.Data;
using System;
using PX.Objects.CR;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	[GL.TableDashboardType]
	public class TimeCardRelease : PXGraph<TimeCardRelease>
	{
		public PXCancel<EPTimeCardRow> Cancel;
		public PXAction<EPTimeCardRow> viewDetails;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Approver Name", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void EPEmployeeEx_AcctName_CacheAttached(PXCache sender)
		{
		}

		#region Selects

		public PXSelect<EPEmployeeEx> Dummy; //CacheAttached support for Joined Table

		[PXViewName(Messages.Timecards)]
		[PXFilterable]
		public PXProcessingJoin<EPTimeCardRow,
			LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPTimeCardRow.employeeID>>,
			LeftJoin<EPApprovalLast, On<EPApprovalLast.refNoteID, Equal<EPTimeCardRow.noteID>>,
			LeftJoin<EPApprovalEx, On<EPApprovalLast.approvalID, Equal<EPApprovalEx.approvalID>>,
			LeftJoin<EPEmployeeEx, On<EPEmployeeEx.userID, Equal<EPApprovalEx.approvedByID>>>>>>,
			Where<EPTimeCardRow.isApproved, Equal<True>,
				And2<Where<EPTimeCardRow.isReleased, NotEqual<True>, Or<EPTimeCardRow.isReleased, IsNull>>,
				And2<Where<EPTimeCardRow.isHold, NotEqual<True>, Or<EPTimeCardRow.isHold, IsNull>>,
				And<Where<EPTimeCardRow.isRejected, NotEqual<True>, Or<EPTimeCardRow.isRejected, IsNull>>>>>>,
			OrderBy<Asc<EPTimeCardRow.timeCardCD>>> FilteredItems;
		
		public PXSetup<EPSetup> Setup;

		#endregion

		#region Ctors

		public TimeCardRelease()
		{
			FilteredItems.SetProcessCaption(Messages.Release);
			FilteredItems.SetProcessAllCaption(Messages.ReleaseAll);
			FilteredItems.SetSelected<EPTimeCard.selected>();

			FilteredItems.SetProcessDelegate(TimeCardRelease.Release);

		}

		#endregion
		#region Actions
		
		[PXUIField(DisplayName = "")]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			var row = FilteredItems.Current;
			if (row != null)
			{

				TimeCardMaint graph = PXGraph.CreateInstance<TimeCardMaint>();
				graph.Document.Current = graph.Document.Search<EPTimeCard.timeCardCD>(row.TimeCardCD);
				throw new PXRedirectRequiredException(graph, true, Messages.ViewDetails)
				{
					Mode = PXBaseRedirectException.WindowMode.NewWindow
				};


			}
			return adapter.Get();
		}

		#endregion

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns,
			bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (viewName == "FilteredItems")
			{
				for (int i = 0; i < sortcolumns.Length; i++)
				{
					if (string.Compare(sortcolumns[i], "WeekID_description", true) == 0)
					{
						sortcolumns[i] = "WeekID";
					}
				}
			}

			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}


		public static void Release(List<EPTimeCardRow> timeCards)
		{
			TimeCardMaint timeCardMaint = PXGraph.CreateInstance<TimeCardMaint>();
			for (int i = 0; i < timeCards.Count; i++)
			{
				timeCardMaint.Clear();
				timeCardMaint.Document.Current = timeCardMaint.Document.Search<EPTimeCard.timeCardCD>(timeCards[i].TimeCardCD);

				if (timeCardMaint.Document.Current == null)
				{
					PXProcessing<EPTimeCardRow>.SetError(i, Messages.TimecardCannotBeReleased_NoRights);
				}
				else
				{
					try
					{
						timeCardMaint.release.Press();
						PXProcessing<EPTimeCardRow>.SetInfo(i, ActionsMessages.RecordProcessed);
					}
					catch (Exception e)
					{
						PXProcessing<EPTimeCardRow>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
					}
				}
			}
		}

		[PXHidden]
		[Serializable]
		[PXBreakInheritance]
		public partial class EPApprovalEx : EPApproval
		{
			public new abstract class approvalID : PX.Data.BQL.BqlInt.Field<approvalID> { }

			#region RefNoteID
			public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
			#endregion
			#region ApprovedByID
			public new abstract class approvedByID : PX.Data.BQL.BqlGuid.Field<approvedByID> { }
			[PXDBGuid()]
			[PX.TM.PXOwnerSelector]
			[PXUIField(DisplayName = "Approved by", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override Guid? ApprovedByID
			{
				get
				{
					return this._ApprovedByID;
				}
				set
				{
					this._ApprovedByID = value;
				}
			}
			#endregion
			#region ApproveDate
			public new abstract class approveDate : PX.Data.BQL.BqlDateTime.Field<approveDate> { }
			[PXDBDate()]
			[PXUIField(DisplayName = "Approve Date", Enabled = false)]
			public override DateTime? ApproveDate
			{
				get
				{
					return this._ApproveDate;
				}
				set
				{
					this._ApproveDate = value;
				}
			}
			#endregion
		}

		[Serializable]
		[PXCacheName(Messages.TimeCard)]
		public class EPTimeCardRow : EPTimeCard
		{
			public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }
			public new abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
			public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			public new abstract class isApproved : PX.Data.BQL.BqlBool.Field<isApproved> { }
			public new abstract class isReleased : PX.Data.BQL.BqlBool.Field<isReleased> { }
			public new abstract class isHold : PX.Data.BQL.BqlBool.Field<isHold> { }
			public new abstract class isRejected : PX.Data.BQL.BqlBool.Field<isRejected> { }


			#region ApprovedByID
			public abstract class approvedByID : PX.Data.BQL.BqlString.Field<approvedByID> { }
			protected string _ApprovedByID;
			[PXString(30, IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "Approved by", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string ApprovedByID
			{
				get
				{
					return this._ApprovedByID;
				}
				set
				{
					this._ApprovedByID = value;
				}
			}
			#endregion
			#region ApprovedByName
			public abstract class approvedByName : PX.Data.BQL.BqlString.Field<approvedByName> { }
			protected string _ApprovedByName;
			[PXString(30, IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "Approver Name", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string ApprovedByName
			{
				get
				{
					return this._ApprovedByName;
				}
				set
				{
					this._ApprovedByName = value;
				}
			}
			#endregion

			#region ApproveDate
			public abstract class approveDate : PX.Data.BQL.BqlDateTime.Field<approveDate> { }
			protected DateTime? _ApproveDate;
			[PXDate()]
			[PXUIField(DisplayName = "Approve Date", Enabled = false)]
			public virtual DateTime? ApproveDate
			{
				get
				{
					return this._ApproveDate;
				}
				set
				{
					this._ApproveDate = value;
				}
			}
			#endregion
		}

		[PXHidden]
		[System.Serializable]
		[PXProjection(typeof(Select4<EPApproval,
			Where<EPApproval.approvalID, IsNotNull>,
			Aggregate<Max<EPApproval.approvalID,
				GroupBy<EPApproval.refNoteID>>>>), Persistent = false)]
		public class EPApprovalLast : IBqlTable
		{
			#region ApprovalID
			public abstract class approvalID : PX.Data.BQL.BqlInt.Field<approvalID> { }
			protected int? _ApprovalID;
			[PXDBInt(IsKey = true, BqlField = typeof(EPApproval.approvalID))]
			[PXUIField(DisplayName = "ApprovalID", Visibility = PXUIVisibility.Service)]
			public virtual int? ApprovalID
			{
				get
				{
					return this._ApprovalID;
				}
				set
				{
					this._ApprovalID = value;
				}
			}
			#endregion
			#region RefNoteID
			public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
			protected Guid? _RefNoteID;
			[PXDBGuid(BqlField = typeof(EPApproval.refNoteID))]
			[PXUIField(DisplayName = "References Nbr.")]
			public virtual Guid? RefNoteID
			{
				get
				{
					return this._RefNoteID;
				}
				set
				{
					this._RefNoteID = value;
				}
			}
			#endregion
		}

	}
}
