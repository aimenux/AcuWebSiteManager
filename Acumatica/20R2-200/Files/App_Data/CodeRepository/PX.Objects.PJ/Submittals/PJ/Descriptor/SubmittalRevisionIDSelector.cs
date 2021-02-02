using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.PJ.Submittals.PJ.DAC;

namespace PX.Objects.PJ.Submittals.PJ.Descriptor
{
	public class SubmittalRevisionIDSelector: PXSelectorAttribute
	{
		public SubmittalRevisionIDSelector(Type submittalIDFieldType)
			: this(BqlTemplate.OfCommand<
							Search<PJSubmittal.revisionID,
								Where<PJSubmittal.submittalID, Equal<Optional2<BqlPlaceholder.A>>>,
								OrderBy<Asc<PJSubmittal.submittalID,
										Desc<PJSubmittal.revisionID>>>>>
							.Replace<BqlPlaceholder.A>(submittalIDFieldType)
							.ToType(),
					typeof(PJSubmittal.revisionID),
					typeof(PJSubmittal.summary))
		{
		}

		public SubmittalRevisionIDSelector(Type type, params Type[] fieldList) : base(type, fieldList)
		{
		}

		public SubmittalRevisionIDSelector(Type type, Type lookupJoin, bool cacheGlobal, Type[] fieldList) : base(type, lookupJoin, cacheGlobal, fieldList)
		{
		}
	}
}
