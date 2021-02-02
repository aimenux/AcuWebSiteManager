using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Extensions
{
	public static class GraphExtensionHelpers
	{
		public static Extension GetProcessingExtension<Extension>(this PXGraph processingGraph)
			where Extension : PXGraphExtension
		{
			var processingExtesion = processingGraph.FindImplementation<Extension>();
			if (processingExtesion == null)
				throw new PXException(Messages.ExtensionCannotBeFound, typeof(Extension).ToString(), processingGraph.GetType().ToString());

			return processingExtesion;
		}
	}
}
