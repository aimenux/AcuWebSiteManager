using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.EP
{
	public class EPTimecardTypeAttribute : PXStringAttribute, IPXRowSelectingSubscriber, IPXFieldDefaultingSubscriber
	{
		public const string Normal = "N";
		public const string Correction = "C";
		public const string NormalCorrected = "D";

		#region Implementation
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			string val;
			using (new PXConnectionScope())
			{
				val = CalculateTimecardType(sender, (EPTimeCard)e.Row);
			}

			sender.SetValue(e.Row, _FieldOrdinal, val);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = CalculateTimecardType(sender, (EPTimeCard)e.Row);
		}
		#endregion

		protected virtual string CalculateTimecardType(PXCache sender, EPTimeCard row)
		{			
			string val = Normal;
			if (row != null)
			{
				if (!string.IsNullOrEmpty(row.OrigTimeCardCD))
					val = Correction;

				if (row.IsReleased == true)
				{
					EPTimeCard correction = PXSelect<EPTimeCard, Where<EPTimeCard.origTimeCardCD, Equal<Required<EPTimeCard.origTimeCardCD>>>>.Select(sender.Graph, row.TimeCardCD);
					if (correction != null)
					{
						val = NormalCorrected;
					}
				}
			}

			return val;
		}
	}
}
