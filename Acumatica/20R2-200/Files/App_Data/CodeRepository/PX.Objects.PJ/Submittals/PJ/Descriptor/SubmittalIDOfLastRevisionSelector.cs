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
	public class SubmittalIDOfLastRevisionSelector : PXSelectorAttribute
	{
		public SubmittalIDOfLastRevisionSelector(Type searchType = null)
			: this(searchType ?? typeof(Search<PJSubmittal.submittalID,
										       Where<PJSubmittal.isLastRevision, Equal<True>>>),
										typeof(PJSubmittal.submittalID),
										typeof(PJSubmittal.revisionID),
										typeof(PJSubmittal.summary))
		{
		}

		public SubmittalIDOfLastRevisionSelector(Type type, params Type[] fieldList) : base(type, fieldList)
		{
		}

		public SubmittalIDOfLastRevisionSelector(Type type, Type lookupJoin, bool cacheGlobal, Type[] fieldList) : base(type, lookupJoin, cacheGlobal, fieldList)
		{
		}
	}
}
