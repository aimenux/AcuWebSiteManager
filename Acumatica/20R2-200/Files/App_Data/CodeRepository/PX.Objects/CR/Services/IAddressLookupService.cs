using PX.AddressValidator;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Services
{
	public interface IAddressLookupService : IAddressConnectedService
	{
		string GetClientScript(PXGraph graph);
	}
}
