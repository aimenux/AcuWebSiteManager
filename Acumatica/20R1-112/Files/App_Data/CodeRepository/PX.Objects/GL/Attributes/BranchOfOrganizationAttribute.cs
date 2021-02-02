using System;

using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL.DAC;

namespace PX.Objects.GL.Attributes
{
	public class BranchOfOrganizationAttribute : BranchBaseAttribute
	{
		public readonly Type OrganizationFieldType;
		public readonly Type FeatureFieldType;

		public BranchOfOrganizationAttribute(Type organizationFieldType, bool onlyActive = true, Type sourceType = null, Type featureFieldType = null) :
			this(organizationFieldType, onlyActive, addDefaultAttribute: true, sourceType: sourceType, featureFieldType: featureFieldType)
		{
		}

		public BranchOfOrganizationAttribute(Type organizationFieldType, bool onlyActive, bool addDefaultAttribute, Type sourceType = null, Type featureFieldType = null) :
			base(sourceType ?? typeof(AccessInfo.branchID), addDefaultAttribute: addDefaultAttribute)
		{
			OrganizationFieldType = organizationFieldType;
			FeatureFieldType = featureFieldType;
			InitializeAttributeRestrictions(onlyActive, OrganizationFieldType, FeatureFieldType);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (OrganizationFieldType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(OrganizationFieldType), OrganizationFieldType.Name, OrganizationFieldUpdated);
				sender.Graph.RowSelected.AddHandler(BqlCommand.GetItemType(OrganizationFieldType), OrganizationRowSelected);
			}
		}

		private void OrganizationFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetValueExt(e.Row, _FieldName, null);
		}

		private void OrganizationRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			bool enableBranchSelector = true;
			int? organizationID = (int?)sender.GetValue(e.Row, OrganizationFieldType.Name);
			Organization organization = OrganizationMaint.FindOrganizationByID(sender.Graph, organizationID);
			if (organization != null)
			{
				enableBranchSelector = organization.OrganizationType != OrganizationTypes.WithoutBranches;
			}

			PXUIFieldAttribute.SetEnabled(sender, _FieldName, enableBranchSelector);
		}

		private void InitializeAttributeRestrictions(bool onlyActive, Type organizationFieldType, Type featureFieldType)
		{
			if (onlyActive)
			{
				_Attributes.Add(new PXRestrictorAttribute(typeof(Where<Branch.active, Equal<True>>), Messages.BranchInactive));
			}

			PXRestrictorAttribute organizationRestrictor;
			if (featureFieldType == null)
			{
				organizationRestrictor = new PXRestrictorAttribute(BqlCommand.Compose(
					typeof(Where<,,>),
					typeof(Branch.organizationID), typeof(Equal<>), typeof(Optional2<>), organizationFieldType,
					typeof(Or<,>), typeof(Optional2<>), organizationFieldType, typeof(IsNull)),
					Messages.TheSpecifiedBranchDoesNotBelongToTheSelectedCompany);
			}
			else
			{
				organizationRestrictor = new PXRestrictorAttribute(BqlCommand.Compose(
					typeof(Where<,,>),
					typeof(Branch.organizationID), typeof(Equal<>), typeof(Optional2<>), organizationFieldType,
						typeof(And<,,>), typeof(Optional2<>), organizationFieldType, typeof(IsNotNull),
					typeof(Or<>), typeof(Where<,,>), typeof(Optional2<>), organizationFieldType, typeof(IsNull),
						typeof(And<>), typeof(Not<>), typeof(FeatureInstalled<>), featureFieldType),
					Messages.TheSpecifiedBranchDoesNotBelongToTheSelectedCompany);
			}
			_Attributes.Add(organizationRestrictor);
		}
	}
}