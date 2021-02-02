using PX.Common;
using PX.Data;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	[PXRestrictor(typeof(Where<PMProject.baseType, NotEqual<CT.CTPRType.projectTemplate>,
		And<PMProject.baseType, NotEqual<CT.CTPRType.contractTemplate>>>), PM.Messages.TemplateContract, typeof(PMProject.contractCD))]
	public class ProjectWithWarningsAttribute : ProjectAttribute
	{
		/// <summary>
		/// When true, a warning will be displayed to warn that the project has a status different than Active.
		/// </summary>
		public bool WarnOfStatus { get; set; } = false;

		public ProjectWithWarningsAttribute() : base()
		{
		}

		public ProjectWithWarningsAttribute(Type customerField) : base(customerField)
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			base.FieldVerifying(sender, e);
			if (WarnOfStatus)
			{
				PMProject project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
				if (project != null && project.Status != ProjectStatus.Active)
				{
					var listAttribute = new ProjectStatus.ListAttribute();
					string status = listAttribute.ValueLabelDic[project.Status];
					sender.RaiseExceptionHandling(FieldName, e.Row, e.NewValue,
						new PXSetPropertyException(Messages.ProjectStatusWarning, PXErrorLevel.Warning, status));
				}
			}
		}
	}
}
