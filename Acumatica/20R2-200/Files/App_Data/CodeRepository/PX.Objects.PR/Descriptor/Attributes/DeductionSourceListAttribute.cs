using PX.Data;
using PX.Data.BQL;
using System;

namespace PX.Objects.PR
{
	public abstract class DeductionSourceListAttribute : PXStringListAttribute
	{
		public DeductionSourceListAttribute() : base(
			new string[] { EmployeeSettings, CertifiedProject, Union, WorkCode },
			new string[] { Messages.EmployeeSettingsSource, Messages.CertifiedProjectSource, Messages.UnionSource, Messages.WorkCodeSource })
		{ }

		public const string EmployeeSettings = "SET";
		public const string CertifiedProject = "PRO";
		public const string Union = "UNI";
		public const string WorkCode = "WCC";

		public class employeeSetting : BqlString.Constant<employeeSetting>
		{
			public employeeSetting() : base(EmployeeSettings) { }
		}

		public class certifiedProject : BqlString.Constant<certifiedProject>
		{
			public certifiedProject() : base(CertifiedProject) { }
		}

		public class union : BqlString.Constant<union>
		{
			public union() : base(Union) { }
		}

		public class workCode : BqlString.Constant<workCode>
		{
			public workCode() : base(WorkCode) { }
		}

		public static string GetSource(PRDeductCode deductCode)
		{
			if (deductCode.IsWorkersCompensation == true)
			{
				return WorkCode;
			}
			if (deductCode.IsCertifiedProject == true)
			{
				return CertifiedProject;
			}
			if (deductCode.IsUnion == true)
			{
				return Union;
			}
			return EmployeeSettings;
		}
	}
}
