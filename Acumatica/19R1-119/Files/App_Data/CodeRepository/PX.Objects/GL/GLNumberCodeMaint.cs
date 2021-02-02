using System;
using PX.Data;

namespace PX.Objects.GL
{
	[Obsolete(PX.Objects.Common.Messages.ClassIsObsoleteAndWillBeRemoved2019R2)]
	public class GLNumberCodeMaint : PXGraph<GLNumberCodeMaint, GLNumberCode>
	{        
		[PXImport(typeof(GLNumberCode))]
		public PXSelect<GLNumberCode> NumberCodes;
	}
}
