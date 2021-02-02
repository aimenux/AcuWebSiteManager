using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.CT
{
	public class CTPRType
	{
		public const string ContractModule = "CT";

		public const string Contract = "C";
		public const string Project = "P";
		public const string ContractTemplate = "T";
		public const string ProjectTemplate = "R";

		public class contract : PX.Data.BQL.BqlString.Constant<contract>
		{
			[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
			public const string Contract = CTPRType.Contract;
			public contract() : base(CTPRType.Contract) { }
		}
		public class project : PX.Data.BQL.BqlString.Constant<project>
		{
			[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
			public const string Project = CTPRType.Project;
			public project() : base(CTPRType.Project) { }
		}
		public class contractTemplate : PX.Data.BQL.BqlString.Constant<contractTemplate>
		{
			[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
			public const string ContractTemplate = CTPRType.ContractTemplate;
			public contractTemplate() : base(CTPRType.ContractTemplate) { }
		}
		public class projectTemplate : PX.Data.BQL.BqlString.Constant<projectTemplate>
		{
			[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
			public const string ProjectTemplate = CTPRType.ProjectTemplate;
			public projectTemplate() : base(CTPRType.ProjectTemplate) { }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() :
				base(new string[] { Contract, Project, ContractTemplate, ProjectTemplate },
					new string[] { Messages.Contract, PM.Messages.Project, Messages.ContractTemplate, PM.Messages.PMProjectTemplate })
			{ }
		}
		public static bool IsTemplate(string baseType)
		{
			if (baseType == ContractTemplate || baseType == ProjectTemplate)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
