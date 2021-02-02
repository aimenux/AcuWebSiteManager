using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.Matrix.Interfaces
{
	public interface IMatrixItemLine
	{
		int? InventoryID { get; set; }
		decimal? Qty { get; set; }
		string UOM { get; set; }
		int? SiteID { get; set; }
	}
}
