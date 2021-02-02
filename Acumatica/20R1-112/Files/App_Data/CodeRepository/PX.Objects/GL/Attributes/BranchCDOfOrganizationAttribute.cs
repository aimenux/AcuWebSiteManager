using System;

using PX.Data;

namespace PX.Objects.GL.Attributes {

	[PXDBString(255, IsUnicode = true)]
	[PXString(255, IsUnicode = true)]
	[PXUIField(DisplayName = "Branch", FieldClass = _FieldClass)]
	public class BranchCDOfOrganizationAttribute : AcctSubAttribute 
	{
		public const string _FieldClass = "BRANCH";
		public const string _DimensionName = "BRANCH";

		public readonly Type OrganizationFieldType;

		public virtual PXSelectorMode SelectorMode { get; set; } = PXSelectorMode.DisplayModeValue;

		public BranchCDOfOrganizationAttribute(Type organizationFieldType, bool onlyActive = true, Type searchType = null)
		{
			Initialize();

			OrganizationFieldType = organizationFieldType;

			Type selectorSource = typeof(Search<Branch.branchCD, Where<MatchWithBranch<Branch.branchID>>>);

			PXSelectorAttribute attr =
				new PXSelectorAttribute(selectorSource)
				{
					DescriptionField = typeof(Branch.acctName),
					SelectorMode= SelectorMode
				};

			_Attributes.Add(attr);

			_Attributes.Add(new PXRestrictorAttribute(BqlCommand.Compose(
															typeof(Where<,,>),
															typeof(Branch.organizationID), typeof(Equal<>), typeof(Optional2<>), OrganizationFieldType,
															typeof(Or<,>), typeof(Optional2<>), OrganizationFieldType, typeof(IsNull)),
														Messages.TheSpecifiedBranchDoesNotBelongToTheSelectedCompany));
			if (onlyActive)
			{
				_Attributes.Add(new PXRestrictorAttribute(typeof(Where<Branch.active, Equal<True>>), Messages.BranchInactive));
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (OrganizationFieldType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(OrganizationFieldType), OrganizationFieldType.Name, OrganizationFieldUpdated);
			}
		}

		private void OrganizationFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetValueExt(e.Row, _FieldName, null);
		}
	}
}
