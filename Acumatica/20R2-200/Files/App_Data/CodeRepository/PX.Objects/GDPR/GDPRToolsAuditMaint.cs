using PX.Data;
using PX.Data.Process;
using PX.Objects.CR;
using PX.SM;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PX.Objects.GDPR
{
	public class GDPRToolsAuditMaint : PXGraph<GDPRToolsAuditMaint>
	{
		public PXSelectOrderBy<SMPersonalDataLog,
			OrderBy<Desc<SMPersonalDataLog.createdDateTime>>> Log;

		public PXCancel<SMPersonalDataLog> Cancel;

		public PXAction<SMPersonalDataLog> OpenContact;
		[PXButton]
		[PXUIField(DisplayName = Messages.OpenContact, Visible = false)]
		public virtual IEnumerable openContact(PXAdapter adapter)
		{
			var entity = this.Caches[typeof(SMPersonalDataLog)].Current as SMPersonalDataLog;
			if (entity == null)
				return adapter.Get();

			new EntityHelper(this).NavigateToRow(entity.TableName, entity.CombinedKey.Split(PXAuditHelper.SEPARATOR), PXRedirectHelper.WindowMode.New);


			return adapter.Get();
		}

		[PXIntList(
		new[] { 0, 1, 3 },
		new[] { "Restored",
				"Pseudonymized",
				"Erased" })]
		[PXUIField(DisplayName = "Status", Visible = true)]
		[PXMergeAttributes]
		protected virtual void _(Events.CacheAttached<SMPersonalDataLog.pseudonymizationStatus> e) { }
	}
}
