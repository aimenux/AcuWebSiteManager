using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using PX.Common;

namespace PX.Objects.GDPR
{
	public class GDPREraseProcess : GDPRPseudonymizeProcess
	{
		#region ctor

		public GDPREraseProcess()
		{
			GetPseudonymizationStatus = typeof(PXPseudonymizationStatusListAttribute.notPseudonymized);
			SetPseudonymizationStatus = PXPseudonymizationStatusListAttribute.Erased;

			SelectedItems.SetProcessDelegate(delegate (List<ObfuscateEntity> entries)
			{
				var graph = PXGraph.CreateInstance<GDPREraseProcess>();

				Process(entries, graph);
			});

			SelectedItems.SetProcessCaption(Messages.Erase);
			SelectedItems.SetProcessAllCaption(Messages.EraseAll);
		}

		#endregion

		#region Implementation

		protected override void TopLevelProcessor(string combinedKey, Guid? topParentNoteID, string info)
		{
			DeleteSearchIndex(topParentNoteID);
		}

		protected override void ChildLevelProcessor(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs, Guid? topParentNoteID)
		{
			PseudonymizeChilds(processingGraph, childTable, fields, childs);
			
			WipeAudit(processingGraph, childTable, fields, childs);
		}
		
		#endregion
	}
}
