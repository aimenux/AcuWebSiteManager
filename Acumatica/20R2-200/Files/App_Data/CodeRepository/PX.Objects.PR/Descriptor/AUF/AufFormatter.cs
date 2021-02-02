using PX.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class AufFormatter
	{
		public AufFormatter()
		{
			_Ver = new VerRecord(AufConstants.AufVersionNumber);
		}

		public byte[] GenerateAufFile()
		{
			if (_Ver == null || Dat == null || Cmp == null)
			{
				throw new PXException(Messages.AatrixReportMissingAufInfo);
			}

			StringBuilder builder = new StringBuilder(_Ver.ToString());
			builder.Append(Dat.ToString());
			PimList?.ForEach(pim => builder.Append(pim.ToString()));
			builder.Append(Cmp.ToString());
			EmpList?.ForEach(emp => builder.Append(emp.ToString()));

			return Encoding.UTF8.GetBytes(builder.ToString());
		}

		private VerRecord _Ver;
		public DatRecord Dat { private get; set; }
		public List<PimRecord> PimList { private get; set; }
		public CmpRecord Cmp { private get; set; }
		public List<EmpRecord> EmpList { private get; set; }
	}
}
