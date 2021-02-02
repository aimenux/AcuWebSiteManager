using System;
using System.Collections.Generic;
using PX.Data;
using System.Collections;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	[Serializable]
	public sealed class SegmentFilter : IBqlTable
	{
		public abstract class dimensionID : PX.Data.BQL.BqlString.Field<dimensionID> { }
		private string _DimensionID;
		[PXDBString]
		[PXDefault(SubAccountAttribute.DimensionName)]
		public string DimensionID
		{
			get
			{
				return _DimensionID;
			}
			set
			{
				_DimensionID = value;
			}
		}
		public abstract class segmentID : PX.Data.BQL.BqlShort.Field<segmentID> { }
		private short? _SegmentID;
		[PXDBShort]
		[PXDefault(typeof(Search<Segment.segmentID,
			Where<Segment.dimensionID, Equal<Current<SegmentFilter.dimensionID>>,
			And<Segment.validate, Equal<True>>>,
			OrderBy<Desc<Segment.segmentID>>>))]
		[PXSelector(typeof(Search<Segment.segmentID,
			Where<Segment.dimensionID, Equal<Current<SegmentFilter.dimensionID>>,
			And<Segment.validate, Equal<True>>>>),
			DescriptionField = typeof(Segment.descr))]
		[PXUIField(DisplayName = "Segment ID")]
		public short? SegmentID
		{
			get
			{
				return _SegmentID;
			}
			set
			{
				_SegmentID = value;
			}
		}
		public abstract class validCombos : PX.Data.BQL.BqlBool.Field<validCombos> { }
		private bool? _ValidCombos;
		[PXBool]
		[PXUIField(Visible = false, Enabled = false)]
		public bool? ValidCombos
		{
			get
			{
				return _ValidCombos;
			}
			set
			{
				_ValidCombos = value;
			}

		}
	}

	public class GLAccessDetail : PX.SM.UserAccess
	{
		[PXDefault]
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(AccountAttribute.DimensionName, typeof(Account.accountCD), typeof(Account.accountCD))]
		protected virtual void Account_AccountCD_CacheAttached(PXCache sender)
		{
		}

		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
		[PXDimensionSelector(SubAccountAttribute.DimensionName, typeof(Sub.subCD), typeof(Sub.subCD))]
		protected virtual void Sub_SubCD_CacheAttached(PXCache sender)
		{
		}

		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector("BRANCH", typeof(Search<Branch.branchCD, Where<Match<Current<AccessInfo.userName>>>>), typeof(Branch.branchCD))]
		protected virtual void Branch_BranchCD_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(SubAccountAttribute.DimensionName)]
		[PXUIField(DisplayName = "Segmented Key ID", Visible = false)]
		protected virtual void SegmentValue_DimensionID_CacheAttached(PXCache sender)
		{
		}

		[PXDBShort(IsKey = true)]
		[PXUIField(DisplayName = "Segment ID", Required = true)]
		[PXSelector(typeof(Search<Segment.segmentID,
			Where<Segment.dimensionID, Equal<Current<SegmentValue.dimensionID>>,
			And<Segment.validate, Equal<True>>>>),
			DescriptionField = typeof(Segment.descr))]
		[PXDefault(typeof(SegmentFilter.segmentID))]
		protected virtual void SegmentValue_SegmentID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Segment Value", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<SegmentValue.value,
			Where<SegmentValue.dimensionID, Equal<Current<SegmentFilter.dimensionID>>,
			And<SegmentValue.segmentID, Equal<Current<SegmentFilter.segmentID>>>>>),
			DescriptionField = typeof(SegmentValue.descr))]
		protected virtual void SegmentValue_Value_CacheAttached(PXCache sender)
		{
		}

		#region GroupID
		[PXDBGuid(IsKey = true)]
		//[PXDefault()]
		[PXUIField(DisplayName = "Budget Article", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search3<GLBudgetTree.groupID, OrderBy<Desc<GLBudgetTree.isGroup, Asc<GLBudgetTree.description, Asc<GLBudgetTree.accountID, Asc<GLBudgetTree.groupID>>>>>>),
		new Type[] {typeof(GLBudgetTree.isGroup), typeof(GLBudgetTree.description), typeof(GLBudgetTree.accountID), typeof(GLBudgetTree.subID),
		typeof(GLBudgetTree.accountMask), typeof(GLBudgetTree.subMask) }, DescriptionField = typeof(GLBudgetTree.description))]
		protected virtual void GLBudgetTree_GroupID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region SMPrinter
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<PX.SM.SMPrinter.printerID>), new Type[] { typeof(PX.SM.SMPrinter.printerName), typeof(PX.SM.SMPrinter.deviceHubID), typeof(PX.SM.SMPrinter.description), typeof(PX.SM.SMPrinter.isActive) },
			DescriptionField = typeof(PX.SM.SMPrinter.printerName))]
		[PXUIField(DisplayName = "Printer", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void SMPrinter_PrinterID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(30)]
		[PXDefault]
		[PXUIField(DisplayName = "Device Hub", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void SMPrinter_DeviceHubID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(20)]
		[PXDefault]
		[PXUIField(DisplayName = "Printer", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void SMPrinter_PrinterName_CacheAttached(PXCache sender)
		{
		}
#endregion


		public GLAccessDetail()
		{
			GLSetup setup = GLSetup.Current;
			Account.Cache.AllowDelete = false;
			Account.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Account.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<GL.Account.accountCD>(Account.Cache, null);

			Sub.Cache.AllowDelete = false;
			Sub.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Sub.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Sub.subCD>(Sub.Cache, null);

			Branch.Cache.AllowDelete = false;
			Branch.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(Branch.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<GL.Branch.branchCD>(Branch.Cache, null);

			Segment.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Segment.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<SegmentValue.segmentID>(Segment.Cache, null);
			PXUIFieldAttribute.SetEnabled<SegmentValue.value>(Segment.Cache, null);

			Views.Caches.Remove(typeof(PX.SM.RelationGroup));
			Views.Caches.Add(typeof(PX.SM.RelationGroup));
		}
		public PXSetup<GLSetup> GLSetup;

		public class subaccount : PX.Data.BQL.BqlString.Constant<subaccount>
		{
			public subaccount()
				: base(SubAccountAttribute.DimensionName)
			{
			}
		}

		public PXSelect<Account> Account;
		public PXSelect<Sub> Sub;
		public PXSelect<Branch> Branch;
		public PXSelect<SegmentValue,
			Where<SegmentValue.dimensionID, Equal<subaccount>,
			And<SegmentValue.segmentID, Equal<Optional<SegmentValue.segmentID>>>>>
			Segment;
		public PXSelectOrderBy<GLBudgetTree, OrderBy<Asc<GLBudgetTree.description, Asc<GLBudgetTree.groupID, Asc<GLBudgetTree.subID>>>>> BudgetTree;
		public PXSelect<PX.SM.SMPrinter> Printers;

		public PXSave<Account> SaveAccount;
		public PXCancel<Account> CancelAccount;
		public PXFirst<Account> FirstAccount;
		public PXPrevious<Account> PrevAccount;
		public PXNext<Account> NextAccount;
		public PXLast<Account> LastAccount;

		public PXSave<Sub> SaveSub;
		public PXCancel<Sub> CancelSub;
		public PXFirst<Sub> FirstSub;
		public PXPrevious<Sub> PrevSub;
		public PXNext<Sub> NextSub;
		public PXLast<Sub> LastSub;

		public PXSave<Branch> SaveBranch;
		public PXCancel<Branch> CancelBranch;
		public PXFirst<Branch> FirstBranch;
		public PXPrevious<Branch> PrevBranch;
		public PXNext<Branch> NextBranch;
		public PXLast<Branch> LastBranch;

		public PXSave<SegmentValue> SaveSegment;
		public PXCancel<SegmentValue> CancelSegment;
		public PXFirst<SegmentValue> FirstSegment;
		public PXPrevious<SegmentValue> PrevSegment;
		public PXNext<SegmentValue> NextSegment;
		public PXLast<SegmentValue> LastSegment;

		public PXSave<GLBudgetTree> SaveTree;
		public PXCancel<GLBudgetTree> CancelTree;
		public PXFirst<GLBudgetTree> FirstTree;
		public PXPrevious<GLBudgetTree> PrevTree;
		public PXNext<GLBudgetTree> NextTree;
		public PXLast<GLBudgetTree> LastTree;

		public PXSave<PX.SM.SMPrinter> SavePrinter;
		public PXCancel<PX.SM.SMPrinter> CancelPrinter;
		public PXFirst<PX.SM.SMPrinter> FirstPrinter;
		public PXPrevious<PX.SM.SMPrinter> PrevPrinter;
		public PXNext<PX.SM.SMPrinter> NextPrinter;
		public PXLast<PX.SM.SMPrinter> LastPrinter;

		public PXFilter<SegmentFilter> Filter;

		protected override IEnumerable groups()
		{
			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(this))
			{
				if (group.SpecificModule == null || group.SpecificModule == typeof(Account).Namespace
					|| PX.SM.UserAccess.IsIncluded(getMask(), group))
				{
					Groups.Current = group;
					yield return group;
				}
			}
		}

		protected override byte[] getMask()
		{
			byte[] mask = null;
			if (Account.Current != null)
			{
				mask = Account.Current.GroupMask;
			}
			else if (Sub.Current != null)
			{
				mask = Sub.Current.GroupMask;
			}
			else if (Branch.Current != null)
			{
				mask = Branch.Current.GroupMask;
			}
			else if (BudgetTree.Current != null)
			{
				mask = BudgetTree.Current.GroupMask;
			}
			else if (Printers.Current != null)
			{
				mask = Printers.Current.GroupMask;
			}
			else if (Segment.Current != null)
			{
				mask = Segment.Current.GroupMask;
			}
			return mask;
		}

		public virtual void SegmentValue_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SegmentValue seg = e.Row as SegmentValue;
			if (seg != null)
			{
				Filter.Current.SegmentID = seg.SegmentID;
			}
		}

		public virtual void SegmentValue_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SegmentValue seg = e.Row as SegmentValue;
			if (seg != null)
			{
				sender.Clear();
				sender.Current = seg;
				SegmentValue val = PXSelect<SegmentValue,
									Where<SegmentValue.dimensionID, Equal<subaccount>,
									And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>,
									And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>
									.SelectWindowed(this, 0, 1, seg.SegmentID, seg.Value);
				if (val == null)
				{
					val = PXSelect<SegmentValue,
							Where<SegmentValue.dimensionID, Equal<subaccount>,
							And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>>>>
							.SelectWindowed(this, 0, 1, seg.SegmentID);
				}
				if (val == null)
				{
					val = PXSelect<SegmentValue,
							Where<SegmentValue.dimensionID, Equal<subaccount>>>
							.SelectWindowed(this, 0, 1);
				}
				if (val != null)
				{
					seg.SegmentID = val.SegmentID;
					seg.Value = val.Value;
				}
				else
				{
					seg.Value = null;
				}
			}
		}

		protected virtual void GLBudgetTree_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			GLBudgetTree node = e.NewRow as GLBudgetTree;
			PXResultset<GLBudgetTree> childGroups = PXSelect<GLBudgetTree, Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>>>.Select(this, node.GroupID);
			foreach (GLBudgetTree childGroup in childGroups)
			{
				PopulateNeighbours<GLBudgetTree>(sender, childGroup, new List<byte[]>().ToArray(), Groups);
			}
		}

		public override void Persist()
		{
			Groups.View.Clear();

			if (Account.Current != null)
			{
				PopulateNeighbours<Account>(Account, Groups);
				PXSelectorAttribute.ClearGlobalCache<Account>();
			}
			else if (Sub.Current != null)
			{
				PopulateNeighbours<Sub>(Sub, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.Objects.GL.Sub>();
			}
			else if (Branch.Current != null)
			{
				PopulateNeighbours<Branch>(Branch, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.Objects.GL.Branch>();
			}
			else if (BudgetTree.Current != null)
			{
				PopulateNeighbours<GLBudgetTree>(BudgetTree, Groups);
			}
			else if (Printers.Current != null)
			{
				PopulateNeighbours<PX.SM.SMPrinter>(Printers, Groups);
			}
			else if (Segment.Current != null)
			{
				PopulateNeighbours<SegmentValue>(Segment, Groups);
				PXSelectorAttribute.ClearGlobalCache<PX.Objects.CS.SegmentValue>();
			}
			else
			{
				return;
			}
			base.Persist();
		}
	}

	public class GLAccessByAccount : GLAccessDetail
	{
	}

	public class GLAccessBySub : GLAccessDetail
	{
	}

	public class GLAccessByBranch : GLAccessDetail
	{
	}

	public class GLAccessByArticle : GLAccessDetail
	{
	}

	public class GLAccessByBudgetNode : GLAccessDetail
	{
	}

	public class GLAccessByPrinter : GLAccessDetail
	{
	}

	public class GLAccess : PX.SM.BaseAccess
	{
		[PXDefault]
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(AccountAttribute.DimensionName, typeof(Account.accountCD), typeof(Account.accountCD))]
		protected virtual void Account_AccountCD_CacheAttached(PXCache sender)
		{
		}

		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
		[PXDimensionSelector(SubAccountAttribute.DimensionName, typeof(Sub.subCD), typeof(Sub.subCD))]
		protected virtual void Sub_SubCD_CacheAttached(PXCache sender)
		{
		}

		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector("BRANCH", typeof(Search<Branch.branchCD, Where<Match<Current<AccessInfo.userName>>>>), typeof(Branch.branchCD))]
		protected virtual void Branch_BranchCD_CacheAttached(PXCache sender)
		{
		}

		public class GLRelationGroupAccountSelectorAttribute : PXCustomSelectorAttribute
		{
			public GLRelationGroupAccountSelectorAttribute(Type type)
				: base(type)
			{
			}

			public virtual IEnumerable GetRecords()
			{
				return GLAccess.GroupDelegate(_Graph, false);
			}
		}
		[PXDBString(128, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible)]
		[GLRelationGroupAccountSelectorAttribute(typeof(PX.SM.RelationGroup.groupName), Filterable = true)]
		protected virtual void RelationGroup_GroupName_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(SubAccountAttribute.DimensionName)]
		[PXUIField(DisplayName = "Segmented Key ID", Visible = false)]
		protected virtual void SegmentValue_DimensionID_CacheAttached(PXCache sender)
		{
		}

		[PXDBShort(IsKey = true)]
		[PXUIField(DisplayName = "Segment ID", Required = true)]
		[PXSelector(typeof(Search<Segment.segmentID,
			Where<Segment.dimensionID, Equal<Current<SegmentValue.dimensionID>>,
			And<Segment.validate, Equal<True>>>>),
			DescriptionField = typeof(Segment.descr))]
		[PXDefault(typeof(SegmentFilter.segmentID))]
		protected virtual void SegmentValue_SegmentID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Segment Value", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<SegmentValue.value,
			Where<SegmentValue.dimensionID, Equal<Current<SegmentFilter.dimensionID>>,
			And<SegmentValue.segmentID, Equal<Current<SegmentFilter.segmentID>>>>>),
			DescriptionField = typeof(SegmentValue.descr))]
		protected virtual void SegmentValue_Value_CacheAttached(PXCache sender)
		{
		}

		#region GroupID
		[PXDBGuid(IsKey = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search3<GLBudgetTree.groupID, OrderBy<Desc<GLBudgetTree.isGroup, Asc<GLBudgetTree.description, Asc<GLBudgetTree.accountID, Asc<GLBudgetTree.groupID>>>>>>),
		new Type[] {typeof(GLBudgetTree.isGroup), typeof(GLBudgetTree.description), typeof(GLBudgetTree.accountID), typeof(GLBudgetTree.subID),
		typeof(GLBudgetTree.accountMask), typeof(GLBudgetTree.subMask) }, DescriptionField = typeof(GLBudgetTree.description))]
		protected virtual void GLBudgetTree_GroupID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region SMPrinter
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<PX.SM.SMPrinter.printerID>), new Type[] { typeof(PX.SM.SMPrinter.printerName), typeof(PX.SM.SMPrinter.deviceHubID), typeof(PX.SM.SMPrinter.description), typeof(PX.SM.SMPrinter.isActive) },
			DescriptionField = typeof(PX.SM.SMPrinter.printerName))]
		[PXUIField(DisplayName = "Printer", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void SMPrinter_PrinterID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(30)]
		[PXDefault]
		[PXUIField(DisplayName = "Device Hub", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void SMPrinter_DeviceHubID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(20)]
		[PXDefault]
		[PXUIField(DisplayName = "Printer", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void SMPrinter_PrinterName_CacheAttached(PXCache sender)
		{
		}
		#endregion

		public GLAccess()
		{
			GLSetup setup = GLSetup.Current;
			Account.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Account.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Account.included>(Account.Cache, null);
			PXUIFieldAttribute.SetEnabled<Account.accountCD>(Account.Cache, null);
			Sub.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Sub.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Sub.included>(Sub.Cache, null);
			PXUIFieldAttribute.SetEnabled<Sub.subCD>(Sub.Cache, null);
			Branch.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Branch.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Branch.included>(Branch.Cache, null);
			Segment.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Segment.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<SegmentValue.included>(Segment.Cache, null);
			PXUIFieldAttribute.SetEnabled<SegmentValue.value>(Segment.Cache, null);
			BudgetTree.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(BudgetTree.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLBudgetTree.included>(BudgetTree.Cache, null);
			PXUIFieldAttribute.SetEnabled<GLBudgetTree.groupID>(BudgetTree.Cache, null);
			Printers.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Printers.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<PX.SM.SMPrinter.included>(Printers.Cache, null);
			PXUIFieldAttribute.SetEnabled<PX.SM.SMPrinter.printerID>(Printers.Cache, null);
		}
		protected virtual void SegmentFilter_ValidCombos_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			Dimension dim = PXSelect<Dimension,
					Where<Dimension.dimensionID, Equal<Current<SegmentFilter.dimensionID>>>>
					.Select(this);
			if (dim != null)
			{
				e.ReturnValue = dim.Validate;
			}
		}
		public PXSetup<GLSetup> GLSetup;
		public PXSelect<Account> Account;
		protected virtual IEnumerable account(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (Account item in PXSelect<Account,
					Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or2<Match<Required<Account.groupMask>>, Or<Account.groupMask, IsNull>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted || item.Included == true)
					{
						Account.Current = item;
						yield return item;
					}
					else if (item.GroupMask != null)
					{
						PX.SM.RelationGroup group = Group.Current;
						bool anyGroup = false;
						for (int i = 0; i < item.GroupMask.Length && i < group.GroupMask.Length; i++)
						{
							if (group.GroupMask[i] != 0x00 && (item.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								Account.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Account.Current = item;
							yield return item;
						}
					}
				}
			}
			else
			{
				yield break;
			}
		}
		public PXSelect<Sub> Sub;
		protected virtual IEnumerable sub(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (Sub item in PXSelect<Sub,
					Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or2<Match<Required<Sub.groupMask>>, Or<Sub.groupMask, IsNull>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted || item.Included == true)
					{
						Sub.Current = item;
						yield return item;
					}
					else if (item.GroupMask != null)
					{
						PX.SM.RelationGroup group = Group.Current;
						bool anyGroup = false;
						for (int i = 0; i < item.GroupMask.Length && i < group.GroupMask.Length; i++)
						{
							if (group.GroupMask[i] != 0x00 && (item.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								Sub.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Sub.Current = item;
							yield return item;
						}
					}
				}
			}
			else
			{
				yield break;
			}
		}
		public PXSelect<Branch> Branch;
		protected virtual IEnumerable branch(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (Branch item in PXSelect<Branch,
					Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or<Match<Required<Branch.groupMask>>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted)
					{
						Branch.Current = item;
						yield return item;
					}
					else if (item.GroupMask != null)
					{
						PX.SM.RelationGroup group = Group.Current;
						bool anyGroup = false;
						for (int i = 0; i < item.GroupMask.Length && i < group.GroupMask.Length; i++)
						{
							if (group.GroupMask[i] != 0x00 && (item.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								Branch.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Branch.Current = item;
							yield return item;
						}
					}
				}
			}
			else
			{
				yield break;
			}
		}

		public PXFilter<SegmentFilter> SegmentFilter;
		public PXSelect<SegmentValue> SegmentAll;
		protected virtual IEnumerable segmentAll(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (SegmentValue item in PXSelect<SegmentValue,
					Where<SegmentValue.dimensionID, Equal<Current<SegmentFilter.dimensionID>>,
					And<Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or<Match<Required<SegmentValue.groupMask>>>>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted)
					{
						Segment.Current = item;
						yield return item;
					}
					else if (item.GroupMask != null)
					{
						PX.SM.RelationGroup group = Group.Current;
						bool anyGroup = false;
						for (int i = 0; i < item.GroupMask.Length && i < group.GroupMask.Length; i++)
						{
							if (group.GroupMask[i] != 0x00 && (item.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								Segment.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Segment.Current = item;
							yield return item;
						}
					}
				}
			}
			else
			{
				yield break;
			}
		}
		public PXSelect<SegmentValue> Segment;
		protected virtual IEnumerable segment(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (SegmentValue item in PXSelect<SegmentValue,
					Where<SegmentValue.dimensionID, Equal<Current<SegmentFilter.dimensionID>>,
					And<SegmentValue.segmentID, Equal<Current<SegmentFilter.segmentID>>,
					And<Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or<Match<Required<SegmentValue.groupMask>>>>>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted)
					{
						Segment.Current = item;
						yield return item;
					}
					else if (item.GroupMask != null)
					{
						PX.SM.RelationGroup group = Group.Current;
						bool anyGroup = false;
						for (int i = 0; i < item.GroupMask.Length && i < group.GroupMask.Length; i++)
						{
							if (group.GroupMask[i] != 0x00 && (item.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								Segment.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Segment.Current = item;
							yield return item;
						}
					}
				}
			}
			else
			{
				yield break;
			}
		}
		public PXSelectOrderBy<GLBudgetTree, OrderBy<Desc<GLBudgetTree.isGroup, Asc<GLBudgetTree.description>>>> BudgetTree;
		protected virtual IEnumerable budgetTree(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (GLBudgetTree item in PXSelect<GLBudgetTree,
					Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or2<Match<Required<GLBudgetTree.groupMask>>, Or<GLBudgetTree.groupMask, IsNull>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted)
					{
						BudgetTree.Current = item;
						yield return item;
					}
					else if (item.GroupMask != null)
					{
						PX.SM.RelationGroup group = Group.Current;
						bool anyGroup = false;
						for (int i = 0; i < item.GroupMask.Length && i < group.GroupMask.Length; i++)
						{
							if (group.GroupMask[i] != 0x00 && (item.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								BudgetTree.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							BudgetTree.Current = item;
							yield return item;
						}
					}
				}
			}
			else
			{
				yield break;
			}
		}

		public PXSelect<PX.SM.SMPrinter> Printers;
		protected virtual IEnumerable printers(
		)
		{
			if (Group.Current != null && !String.IsNullOrEmpty(Group.Current.GroupName))
			{
				bool inserted = (Group.Cache.GetStatus(Group.Current) == PXEntryStatus.Inserted);
				foreach (PX.SM.SMPrinter item in PXSelect<PX.SM.SMPrinter,
					Where2<Match<Current<PX.SM.RelationGroup.groupName>>,
					Or2<Match<Required<PX.SM.SMPrinter.groupMask>>, Or<PX.SM.SMPrinter.groupMask, IsNull>>>>
					.Select(this, new byte[0]))
				{
					if (!inserted)
					{
						Printers.Current = item;
						yield return item;
					}
					else if (item.GroupMask != null)
					{
						PX.SM.RelationGroup group = Group.Current;
						bool anyGroup = false;
						for (int i = 0; i < item.GroupMask.Length && i < group.GroupMask.Length; i++)
						{
							if (group.GroupMask[i] != 0x00 && (item.GroupMask[i] & group.GroupMask[i]) == group.GroupMask[i])
							{
								Printers.Current = item;
								yield return item;
							}
							anyGroup |= item.GroupMask[i] != 0x00;
						}
						if (!anyGroup)
						{
							Printers.Current = item;
							yield return item;
						}
					}
				}
			}
			else
			{
				yield break;
			}
		}

		protected override void RelationGroup_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			base.RelationGroup_RowInserted(cache, e);
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificModule = typeof(generalLedgerModule).Namespace;
		}
		protected virtual void RelationGroup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PX.SM.RelationGroup group = e.Row as PX.SM.RelationGroup;
			if (group != null)
			{
				if (String.IsNullOrEmpty(group.GroupName))
				{
					Save.SetEnabled(false);
					Account.Cache.AllowInsert = false;
					Sub.Cache.AllowInsert = false;
				}
				else
				{
					Save.SetEnabled(true);
					Account.Cache.AllowInsert = true;
					Sub.Cache.AllowInsert = true;
				}
			}
		}
		protected virtual void Account_AccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if(PXSelectorAttribute.Select<Account.accountCD>(sender, e.Row, e.NewValue) == null)
			{
				throw new PXSetPropertyException(ErrorMessages.ElementDoesntExist, "AccountCD");
			}
		}
		protected virtual void Account_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				Account acct = PXSelectorAttribute.Select<Account.accountCD>(sender, e.Row) as Account;
				if (acct != null)
				{
					acct.Included = true;
					PXCache<Account>.RestoreCopy((Account)e.Row, acct);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}
		protected virtual void Account_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Account acct = e.Row as Account;
			if (acct != null && sender.GetStatus(acct) == PXEntryStatus.Notchanged)
			{
				if (acct.GroupMask != null)
				{
					foreach (byte b in acct.GroupMask)
					{
						if (b != 0x00)
						{
							acct.Included = true;
							return;
						}
					}
				}
				else
				{
					acct.Included = true;
				}
			}
		}
		protected virtual void Account_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Account acct = e.Row as Account;
			PX.SM.RelationGroup group = Group.Current;
			if (acct != null && acct.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (acct.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = acct.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					acct.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (acct.Included == true)
					{
						acct.GroupMask[i] = (byte)(acct.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						acct.GroupMask[i] = (byte)(acct.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}
		protected virtual void Sub_SubCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXSelectorAttribute.Select<Sub.subCD>(sender, e.Row, e.NewValue) == null)
			{
				throw new PXSetPropertyException(ErrorMessages.ElementDoesntExist, "SubCD");
			}
		}
		protected virtual void Sub_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				Sub sub = PXSelectorAttribute.Select<Sub.subCD>(sender, e.Row) as Sub;
				if (sub != null)
				{
					sub.Included = true;
					PXCache<Sub>.RestoreCopy((Sub)e.Row, sub);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}
		protected virtual void Sub_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Sub sub = e.Row as Sub;
			if (sub != null && sender.GetStatus(sub) == PXEntryStatus.Notchanged)
			{
				if (sub.GroupMask != null)
				{
					foreach (byte b in sub.GroupMask)
					{
						if (b != 0x00)
						{
							sub.Included = true;
							return;
						}
					}
				}
				else
				{
					sub.Included = true;
				}
			}
		}
		protected virtual void Sub_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Sub sub = e.Row as Sub;
			PX.SM.RelationGroup group = Group.Current;
			if (sub != null && sub.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (sub.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = sub.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					sub.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (sub.Included == true)
					{
						sub.GroupMask[i] = (byte)(sub.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						sub.GroupMask[i] = (byte)(sub.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}
		protected virtual void Branch_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				Branch branch = PXSelectorAttribute.Select<Branch.branchCD>(sender, e.Row) as Branch;
				if (branch != null)
				{
					branch.Included = true;
					PXCache<Branch>.RestoreCopy((Branch)e.Row, branch);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}
		protected virtual void Branch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Branch branch = e.Row as Branch;
			if (branch != null && sender.GetStatus(branch) == PXEntryStatus.Notchanged)
			{
				if (branch.GroupMask != null)
				{
					foreach (byte b in branch.GroupMask)
					{
						if (b != 0x00)
						{
							branch.Included = true;
							return;
						}
					}
				}
				else
				{
					branch.Included = true;
				}
			}
		}
		protected virtual void Branch_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Branch branch = e.Row as Branch;
			PX.SM.RelationGroup group = Group.Current;
			if (branch != null && branch.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (branch.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = branch.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					branch.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (branch.Included == true)
					{
						branch.GroupMask[i] = (byte)(branch.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						branch.GroupMask[i] = (byte)(branch.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}

		protected virtual void GLBudgetTree_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			GLBudgetTree node = e.Row as GLBudgetTree;
			if (node != null && sender.GetStatus(node) == PXEntryStatus.Notchanged)
			{
				if (node.GroupMask != null)
				{
					foreach (byte b in node.GroupMask)
					{
						if (b != 0x00)
						{
							node.Included = true;
							return;
						}
					}
				}
				else
				{
					node.Included = true;
				}
			}
		}

		protected virtual void GLBudgetTree_Description_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null)
			{
				GLBudgetTree node = e.Row as GLBudgetTree;
				e.NewValue = node.Description;
			}
		}

		protected virtual void GLBudgetTree_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			GLBudgetTree node = e.NewRow as GLBudgetTree;
			PX.SM.RelationGroup group = Group.Current;

			if (node != null && node.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (node.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = node.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					node.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (node.Included == true)
					{
						node.GroupMask[i] = (byte)(node.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						node.GroupMask[i] = (byte)(node.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
				UpdateChildGroupMask(node.GroupID, node.Included ?? false);
			}
		}

		protected virtual void GLBudgetTree_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				GLBudgetTree node = PXSelectorAttribute.Select<GLBudgetTree.groupID>(sender, e.Row) as GLBudgetTree;
				if (node != null)
				{
					node.Included = true;
					PXCache<GLBudgetTree>.RestoreCopy((GLBudgetTree)e.Row, node);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}

		protected virtual void GLBudgetTree_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			GLBudgetTree treeNode = e.Row as GLBudgetTree;
			PX.SM.RelationGroup group = Group.Current;

			if (treeNode != null && treeNode.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (treeNode.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = treeNode.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					treeNode.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (treeNode.Included == true)
					{
						treeNode.GroupMask[i] = (byte)(treeNode.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						treeNode.GroupMask[i] = (byte)(treeNode.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}

		private void UpdateChildGroupMask(Guid? GroupID, bool Included)
		{
			PXResultset<GLBudgetTree> childGroups = PXSelect<GLBudgetTree, Where<GLBudgetTree.parentGroupID, Equal<Required<GLBudgetTree.parentGroupID>>>>.Select(this, GroupID);
			foreach (GLBudgetTree childGroup in childGroups)
			{
				if (childGroup.GroupID != childGroup.ParentGroupID)
				{
					childGroup.Included = Included;
					BudgetTree.Update(childGroup);
					UpdateChildGroupMask(childGroup.GroupID, Included);
				}
			}
		}

		protected virtual void SMPrinter_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				PX.SM.SMPrinter printer = PXSelectorAttribute.Select<PX.SM.SMPrinter.printerID>(sender, e.Row) as PX.SM.SMPrinter;
				if (printer != null)
				{
					printer.Included = true;
					PXCache<PX.SM.SMPrinter>.RestoreCopy((PX.SM.SMPrinter)e.Row, printer);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}
		protected virtual void SMPrinter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PX.SM.SMPrinter printer = e.Row as PX.SM.SMPrinter;
			if (printer != null && sender.GetStatus(printer) == PXEntryStatus.Notchanged)
			{
				if (printer.GroupMask != null)
				{
					foreach (byte b in printer.GroupMask)
					{
						if (b != 0x00)
						{
							printer.Included = true;
							return;
						}
					}
				}
				else
				{
					printer.Included = true;
				}
			}
		}
		protected virtual void SMPrinter_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PX.SM.SMPrinter printer = e.Row as PX.SM.SMPrinter;
			PX.SM.RelationGroup group = Group.Current;
			if (printer != null && printer.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (printer.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = printer.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					printer.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (printer.Included == true)
					{
						printer.GroupMask[i] = (byte)(printer.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						printer.GroupMask[i] = (byte)(printer.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}


		protected virtual void SegmentValue_Value_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXSelectorAttribute.Select<SegmentValue.value>(sender, e.Row, e.NewValue) == null)
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, "Value"));
			}
		}
		protected virtual void SegmentValue_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				SegmentValue seg = PXSelectorAttribute.Select<SegmentValue.value>(sender, e.Row) as SegmentValue;
				if (seg != null)
				{
					seg.Included = true;
					PXCache<SegmentValue>.RestoreCopy((SegmentValue)e.Row, seg);
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
				else
				{
					sender.Delete(e.Row);
				}
			}
		}
		protected virtual void SegmentValue_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SegmentValue seg = e.Row as SegmentValue;
			if (seg != null && sender.GetStatus(seg) == PXEntryStatus.Notchanged)
			{
				if (seg.GroupMask != null)
				{
					foreach (byte b in seg.GroupMask)
					{
						if (b != 0x00)
						{
							seg.Included = true;
							return;
						}
					}
				}
				else
				{
					seg.Included = true;
				}
			}
		}
		protected virtual void SegmentValue_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SegmentValue seg = e.Row as SegmentValue;
			PX.SM.RelationGroup group = Group.Current;
			if (seg != null && seg.GroupMask != null && group != null && group.GroupMask != null)
			{
				if (seg.GroupMask.Length < group.GroupMask.Length)
				{
					byte[] mask = seg.GroupMask;
					Array.Resize<byte>(ref mask, group.GroupMask.Length);
					seg.GroupMask = mask;
				}
				for (int i = 0; i < group.GroupMask.Length; i++)
				{
					if (group.GroupMask[i] == 0x00)
					{
						continue;
					}
					if (seg.Included == true)
					{
						seg.GroupMask[i] = (byte)(seg.GroupMask[i] | group.GroupMask[i]);
					}
					else
					{
						seg.GroupMask[i] = (byte)(seg.GroupMask[i] & ~group.GroupMask[i]);
					}
				}
			}
		}

		public override void Persist()
		{
			populateNeighbours<PX.SM.Users>(Users);
			populateNeighbours<Account>(Account);
			populateNeighbours<Sub>(Sub);
			populateNeighbours<Branch>(Branch);
			populateNeighbours<SegmentValue>(SegmentAll);
			populateNeighbours<Sub>(Sub);
			populateNeighbours<Account>(Account);
			populateNeighbours<PX.SM.Users>(Users);
            populateNeighbours<GLBudgetTree>(BudgetTree);
			populateNeighbours<PX.SM.SMPrinter>(Printers);
			base.Persist();
			PXSelectorAttribute.ClearGlobalCache<PX.SM.Users>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.GL.Account>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.GL.Sub>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.GL.Branch>();
			PXSelectorAttribute.ClearGlobalCache<PX.Objects.CS.SegmentValue>();
			PXDimensionAttribute.Clear();
		}

		static public IEnumerable GroupDelegate(PXGraph graph, bool inclInserted)
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<accountType>,
					Or<PX.SM.Neighbour.leftEntityType, Equal<subType>,
					Or<PX.SM.Neighbour.leftEntityType, Equal<segmentValueType>>>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(graph);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(graph))
			{
				if ((!string.IsNullOrEmpty(group.GroupName) || inclInserted) &&
					(group.SpecificModule == null || group.SpecificModule == typeof(Account).Namespace)
					|| PX.SM.UserAccess.InNeighbours(set, group))
				{
					yield return group;
				}
			}
		}

		protected virtual IEnumerable group()
		{
			return GLAccess.GroupDelegate(this, true);
		}

	}

	public class GLAccessBudget : GLAccess
	{
		public class GLRelationGroupBudgetSelectorAttribute : PXCustomSelectorAttribute
		{
			public GLRelationGroupBudgetSelectorAttribute(Type type)
				: base(type)
			{
			}

			public virtual IEnumerable GetRecords()
			{
				return GLAccessBudget.GroupDelegate(_Graph, false);
			}
		}

		[PXDBString(128, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible)]
		[GLRelationGroupBudgetSelectorAttribute(typeof(PX.SM.RelationGroup.groupName), Filterable = true)]
		protected override void RelationGroup_GroupName_CacheAttached(PXCache sender)
		{
		}

		#region AccountID
		[Account(DisplayName = "Account", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void GLBudgetTree_AccountID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		new static public IEnumerable GroupDelegate(PXGraph graph, bool inclInserted)
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<budgetType>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(graph);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(graph))
			{
				if ((!string.IsNullOrEmpty(group.GroupName) || inclInserted) &&
					(group.SpecificModule == null || group.SpecificModule == typeof(Account).Namespace)
					|| PX.SM.UserAccess.InNeighbours(set, group))
				{
					yield return group;
				}
			}
		}

		protected override IEnumerable group()
		{
			return GLAccessBudget.GroupDelegate(this, true);
		}
	}

	public class GLAccessPrinter : GLAccess
	{
		public class GLRelationGroupPrinterSelectorAttribute : PXCustomSelectorAttribute
		{
			public GLRelationGroupPrinterSelectorAttribute(Type type)
				: base(type)
			{
			}

			public virtual IEnumerable GetRecords()
			{
				return GLAccessPrinter.GroupDelegate(_Graph, false);
			}
		}

		[PXDBString(128, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible)]
		[GLRelationGroupPrinterSelectorAttribute(typeof(PX.SM.RelationGroup.groupName), Filterable = true)]
		protected override void RelationGroup_GroupName_CacheAttached(PXCache sender)
		{
		}

		new static public IEnumerable GroupDelegate(PXGraph graph, bool inclInserted)
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<printerType>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(graph);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(graph))
			{
				if ((!string.IsNullOrEmpty(group.GroupName) || inclInserted) &&
					(group.SpecificModule == null || group.SpecificModule == typeof(Account).Namespace)
					|| PX.SM.UserAccess.InNeighbours(set, group))
				{
					yield return group;
				}
			}
		}

		protected override IEnumerable group()
		{
			return GLAccessPrinter.GroupDelegate(this, true);
		}
	}

	public class GLBranchAccess : GLAccess
	{
		public class GLRelationGroupBranchSelectorAttribute : PXCustomSelectorAttribute
		{
			public GLRelationGroupBranchSelectorAttribute(Type type)
				: base(type)
			{
			}

			public virtual IEnumerable GetRecords()
			{
				return GLBranchAccess.GroupDelegate(_Graph, false);
			}
		}
		[PXDBString(128, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible)]
		[GLRelationGroupBranchSelectorAttribute(typeof(PX.SM.RelationGroup.groupName), Filterable = true)]
		protected override void RelationGroup_GroupName_CacheAttached(PXCache sender)
		{
		}

		new static public IEnumerable GroupDelegate(PXGraph graph, bool inclInserted)
		{
			PXResultset<PX.SM.Neighbour> set = PXSelectGroupBy<PX.SM.Neighbour,
				Where<PX.SM.Neighbour.leftEntityType, Equal<branchType>>,
				Aggregate<GroupBy<PX.SM.Neighbour.coverageMask,
					GroupBy<PX.SM.Neighbour.inverseMask,
					GroupBy<PX.SM.Neighbour.winCoverageMask,
					GroupBy<PX.SM.Neighbour.winInverseMask>>>>>>.Select(graph);

			foreach (PX.SM.RelationGroup group in PXSelect<PX.SM.RelationGroup>.Select(graph))
			{
				if ((!string.IsNullOrEmpty(group.GroupName) || inclInserted) &&
					(group.SpecificModule == null || group.SpecificModule == typeof(Account).Namespace)
					|| PX.SM.UserAccess.InNeighbours(set, group))
				{
					yield return group;
				}
			}
		}

		protected override IEnumerable group()
		{
			return GLBranchAccess.GroupDelegate(this, true);
		}

		protected override void RelationGroup_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			base.RelationGroup_RowInserted(cache, e);
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificType = typeof(Branch).FullName;
		}
	}

	public class GLBranchAccessSub : GLBranchAccess
	{
	}

}
