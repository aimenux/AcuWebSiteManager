using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.SM
{
	public class acumaticaStudio : PX.Data.BQL.BqlString.Constant<acumaticaStudio>
	{
		public acumaticaStudio()
			: base("%studio%")
		{
		}
	}
}
