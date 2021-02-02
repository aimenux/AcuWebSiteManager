using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;

namespace PX.Objects.TX.Descriptor
{
	public class TaxPeriodFilterBranchAttribute: BranchBaseAttribute, IPXRowSelectedSubscriber, IPXFieldDefaultingSubscriber, IPXFieldUpdatingSubscriber
	{
		public bool HideBranchField { get; set; }

		public Type OrganizationFieldType { get; set; }

		public TaxPeriodFilterBranchAttribute(Type organizationFieldType, bool hideBranchField = true) 
			: base(organizationFieldType, addDefaultAttribute: false)
		{
			HideBranchField = hideBranchField;
			OrganizationFieldType = organizationFieldType;

			(_Attributes[_UIAttrIndex] as PXUIFieldAttribute).Required = true;

			_Attributes.Add(new PXRestrictorAttribute(BqlCommand.Compose(
					typeof(Where<,>),
					typeof(Branch.organizationID), typeof(Equal<>), typeof(Optional2<>), organizationFieldType),
				GL.Messages.TheSpecifiedBranchDoesNotBelongToTheSelectedCompany));

			_Attributes.Add(new PXRestrictorAttribute(typeof(Where<Organization.fileTaxesByBranches, Equal<True>>), 
														Messages.TheBranchCanBeSpecifiedOnlyForCompaniesForWhichFileTaxesByBranchesOptionIsEnabled));
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || HideBranchField == false)
				return;

			int? organizationID = (int?)sender.GetValue(e.Row, OrganizationFieldType.Name);

			PXUIFieldAttribute.SetVisible(sender, _FieldName, organizationID != null &&
																				  OrganizationMaint.FindOrganizationByID(sender.Graph, organizationID).FileTaxesByBranches == true);
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

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row == null)
				return;

			int? organizationID = (int?)sender.GetValue(e.Row, OrganizationFieldType.Name);

			if (organizationID == null)
				return;

			Organization organization = OrganizationMaint.FindOrganizationByID(sender.Graph, organizationID);

			if (organization.FileTaxesByBranches != true && e.NewValue != null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null)
				return;

			int? organizationID = (int?)sender.GetValue(e.Row, OrganizationFieldType.Name);

			if (organizationID == null)
				return;

			Organization organization = OrganizationMaint.FindOrganizationByID(sender.Graph, organizationID);

			if (organization.FileTaxesByBranches == true)
			{
				e.NewValue = sender.Graph.Accessinfo.BranchID;
			}
		}
	}
}
