using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Tools;
using PX.SM;

namespace PX.Objects.Common.Tools
{
	public static class UploadFileHelper
	{
		/// <summary>
		/// AttachDataAsFile
		/// </summary>
		/// <remarks>Invoke before persisting of DAC</remarks>
		public static void AttachDataAsFile(string fileName, string data, IDACWithNote dac, PXGraph graph)
		{
			var file = new FileInfo(Guid.NewGuid(), fileName, null, SerializationHelper.GetBytes(data));

			var uploadFileMaintGraph = PXGraph.CreateInstance<UploadFileMaintenance>();

			if (uploadFileMaintGraph.SaveFile(file) || file.UID == null)
			{
				var fileNote = new NoteDoc { NoteID = dac.NoteID, FileID = file.UID };

				graph.Caches[typeof(NoteDoc)].Insert(fileNote);

				graph.Persist(typeof(NoteDoc), PXDBOperation.Insert);
			}
		}
	}
}
