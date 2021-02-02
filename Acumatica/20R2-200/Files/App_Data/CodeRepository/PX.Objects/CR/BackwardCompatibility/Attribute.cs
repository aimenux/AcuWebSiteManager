using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web.Compilation;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Data.SQLTree;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.TM;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.SM;
using PX.Objects.PM;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common.Interfaces;
using PX.Objects.EP;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using EPEmployee = PX.TM.PXOwnerSelectorAttribute.EPEmployee;

namespace PX.Objects.CR.BackwardCompatibility
{
	#region PXOwnerSelectorAttribute

	public class PXOwnerSelectorAttribute : PXAggregateAttribute
	{
		#region SelectorAttribute

		public class OwnerSubstituteSelectorAttribute : PXSelectorAttribute
		{
			public OwnerSubstituteSelectorAttribute(Type type, params Type[] fieldList)
				: base(type, fieldList)
			{
			}

			public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				if (e.NewValue != null && Guid.TryParse(e.NewValue.ToString(), out Guid value))
				{
					EP.EPEmployee employee = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.userID, Equal<Required<EP.EPEmployee.userID>>>>.SelectSingleBound(sender.Graph, new object[] { }, value);

					e.NewValue = employee?.DefContactID ?? e.NewValue;
				}
				if (e.NewValue != null && (!int.TryParse(e.NewValue.ToString(), out _) || e.NewValue.ToString().StartsWith("0")))
				{
					base.SubstituteKeyFieldUpdating(sender, e);
				}
			}

			public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				if (e.ReturnValue != null && Guid.TryParse(e.ReturnValue.ToString(), out Guid value))
				{
					EP.EPEmployee employee = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.userID, Equal<Required<EP.EPEmployee.userID>>>>.SelectSingleBound(sender.Graph, new object[] { }, value);

					e.ReturnValue = employee?.DefContactID ?? e.ReturnValue;
				}
				else
				{
					base.SubstituteKeyFieldSelecting(sender, e);
				}
			}

			public override void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
			{
				var origValue = sender.GetValue(e.Row, _FieldName);
				base.DescriptionFieldSelecting(sender, e, alias);
				if (origValue != null && e.ReturnValue == null)
				{
					using (var del = new PXReadDeletedScope())
					{
						Users user = PXSelect<Users, Where<Users.pKID, Equal<Required<Users.pKID>>>>.SelectSingleBound(sender.Graph, new object[] { }, origValue);
						e.ReturnValue = (user != null)
							? user.DisplayName
							: origValue;

						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, true, null, null, null, null, alias
							, null, null, PXLocalizer.Localize(TM.Messages.OwnerNotFound, typeof(Messages).FullName), PXErrorLevel.Warning, false, null, null, PXUIVisibility.Visible, null, null, null);
					}
				}
			}
		}

		#endregion

		#region State
		protected readonly int _SelAttrIndex;
		protected Type _workgroupType;
		#endregion

		public PXOwnerSelectorAttribute() : this(null)
		{

		}
		public PXOwnerSelectorAttribute(Type workgroupType)
			: this(workgroupType, null)
		{
		}

		protected PXOwnerSelectorAttribute(Type workgroupType, Type search, bool validateValue = true, bool inquiryMode = false)
		{
			PXSelectorAttribute selector;
			_Attributes.Add(selector = new OwnerSubstituteSelectorAttribute(search ?? CreateSelect(workgroupType),
			typeof(EPEmployee.acctName),
			typeof(EPEmployee.acctCD),
			typeof(EPEmployee.departmentID)));
			_SelAttrIndex = _Attributes.Count - 1;

			selector.DescriptionField = typeof(EPEmployee.acctName);
			selector.SubstituteKey = typeof(EPEmployee.acctCD);
			selector.ValidateValue = validateValue;
			selector.CacheGlobal = true;
			_workgroupType = workgroupType;

			if (!inquiryMode)
			{
				_Attributes.Add(new PXRestrictorAttribute(typeof(Where<EPEmployee.status, IsNull, Or<EPEmployee.status, NotEqual<BAccount.status.inactive>>>), Objects.EP.Messages.InactiveEpmloyee, typeof(EPEmployee.acctCD), typeof(EPEmployee.status)));
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_workgroupType != null)
			{
				sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), RowUpdated);
				sender.Graph.FieldVerifying.AddHandler(BqlCommand.GetItemType(_workgroupType), _workgroupType.Name, FieldVerifying);
			}
		}

		private static Type CreateSelect(Type workgroupType)
		{
			if (workgroupType == null)
				return typeof(Search<EPEmployee.defContactID, Where<EPEmployee.acctCD, IsNotNull>>);

			return BqlCommand.Compose(
							typeof(Search2<,,>), typeof(EPEmployee.defContactID),
							typeof(LeftJoin<,>), typeof(EPCompanyTreeMember),
							typeof(On<,,>), typeof(EPCompanyTreeMember.contactID), typeof(Equal<EPEmployee.defContactID>),
							typeof(And<,>), typeof(EPCompanyTreeMember.workGroupID), typeof(Equal<>), typeof(Optional<>), workgroupType,
							typeof(Where<,,>),
							typeof(Optional<>), workgroupType, typeof(IsNull),
							typeof(Or<EPCompanyTreeMember.contactID, IsNotNull>)
							);
		}

		protected virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{

		}

		protected virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row != null)
			{
				int? WorkGroupID = (int?)e.NewValue;
				int? oldWorkGroupID = (int?)sender.GetValue(e.Row, _workgroupType.Name);
				string owner =
					sender.GetValuePending(e.Row, _FieldName) as string ??
					(sender.GetValueExt(e.Row, _FieldName) as PXFieldState)?.Value as string;

				var selector = _Attributes[_SelAttrIndex] as PXSelectorAttribute;
				if (selector != null)
				{
					var copy_row = sender.CreateCopy(e.Row);
					var fu = new PXFieldUpdatingEventArgs(copy_row, owner);
					selector.SubstituteKeyFieldUpdating(sender, fu);
					int? OwnerID = fu.NewValue as int?;

					if (WorkGroupID != oldWorkGroupID && !BelongsToWorkGroup(sender.Graph, WorkGroupID, OwnerID))
					{
						// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [legacy, for BC]
						sender.SetValue(copy_row, _FieldName, OwnerWorkGroup(sender.Graph, WorkGroupID));
						sender.SetValuePending(e.Row, _FieldName, (string)(sender.GetValueExt(copy_row, _FieldName) as PXFieldState)?.Value);
					}
				}
			}
		}
		public static bool BelongsToWorkGroup(PXGraph graph, int? WorkGroupID, int? OwnerID)
		{
			if (WorkGroupID == null && OwnerID != null) return true;
			if (WorkGroupID != null && OwnerID == null) return false;

			return PXSelect<EPCompanyTreeMember,
					Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
					And<EPCompanyTreeMember.contactID, Equal<Required<EPCompanyTreeMember.contactID>>>>>
					.Select(graph, WorkGroupID, OwnerID).Count > 0;
		}

		public static int? OwnerWorkGroup(PXGraph graph, int? WorkGroupID)
		{
			EPCompanyTreeMember member = PXSelect<
					EPCompanyTreeMember,
				Where<
					EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
					And<EPCompanyTreeMember.isOwner, Equal<Required<EPCompanyTreeMember.isOwner>>>>>
				.Select(graph, WorkGroupID, 1);

			return member != null ? member.ContactID : null;
		}

		public static int? DefaultWorkgroup(PXGraph graph, int? contactID)
		{
			PXSelectBase<EPCompanyTreeMember> cmd = new PXSelectJoin<
					EPCompanyTreeMember,
				InnerJoin<EPCompanyTreeH,
					On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTreeH.workGroupID>>>,
				Where<
					EPCompanyTreeMember.contactID, Equal<Required<EPCompanyTreeMember.contactID>>>>(graph);

			EPCompanyTreeMember m = cmd.SelectSingle(contactID ?? graph.Accessinfo.ContactID);

			return m != null ? m.WorkGroupID : null;
		}
	}

	#endregion
}
