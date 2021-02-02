using System;
using System.Collections.Generic;

namespace PX.Objects.CR.MassProcess
{	
	public interface IMassProcess
	{
		Action<List<object>> ItemsProcessDelegate { get; set; }
		Func<List<object>, bool> AskProcessDelegate { get; set; }		
	}
}