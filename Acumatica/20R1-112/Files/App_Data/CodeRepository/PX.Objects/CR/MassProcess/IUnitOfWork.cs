using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.CR.MassProcess
{
	public interface IUnitOfWork:IDisposable
	{
		void Commit();
		void Rollback();
	}
}
