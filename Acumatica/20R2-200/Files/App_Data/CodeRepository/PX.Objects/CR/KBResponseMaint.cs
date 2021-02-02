using System.Collections;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	public class KBResponseMaint : PXGraph<KBResponseMaint, KBResponse>
	{
		public PXSelect<KBResponse> Responses;

		public PXAction<KBResponse> submit;

		[PXUIField(DisplayName = "Submit")]
		[PXButton(Tooltip = Messages.SubmitResponseTooltip)]
		public virtual IEnumerable Submit(PXAdapter adapter)
		{
			if (Responses.Current != null)
			{
				Responses.Current.CreatedByID = null;
				Save.Press();
			}
			return adapter.Get();
		}
	}
}
