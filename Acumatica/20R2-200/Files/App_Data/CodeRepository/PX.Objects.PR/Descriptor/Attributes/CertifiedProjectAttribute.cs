using PX.Data;
using PX.Objects.CT;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	public class CertifiedProjectAttribute : ProjectAttribute
	{
		public CertifiedProjectAttribute() : base(typeof(
				Where<PMProject.baseType.IsEqual<CTPRType.project>.
					And<PMProject.nonProject.IsNotEqual<True>>.
					And<PMProject.certifiedJob.IsEqual<True>>.
					And<PMProject.status.IsNotEqual<ProjectStatus.completed>>.
					And<PMProject.status.IsNotEqual<Contract.status.canceled>>>))
			{ }
	}
}
