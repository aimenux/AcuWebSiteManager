using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;

namespace PX.Objects.Common.Attributes
{
	public class PXResultStorageAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXFieldUpdatingSubscriber
	{
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{			
			if (e.ReturnValue != null)
			{
				byte[][] buffer = (byte[][])e.ReturnValue;
				using (var s = new GZipStream(
					new SparseMemoryStream(buffer), CompressionMode.Decompress))
				{
					try
					{
						e.ReturnValue = PXReflectionSerializer.Deserialize(s);						
					}
					catch
					{
					}
				}
			}
			e.ReturnState = PXStringState.CreateInstance(
				e.ReturnState, //value
				typeof(byte[][]), //dataType			
				false, //isKey				
				true, //nullable			
				null, //required		
				null, //precision
				-1, //length
				null, //defaultValue
				_FieldName, //fieldName
				null, //descriptionName
				null, //displayName
				null, //error
				PXErrorLevel.Undefined, //errorLevel
				null, //enabled
				null, //visible
				null, //readOnly	
				PXUIVisibility.Undefined, //visibility
				null, //viewName
				null, //fieldList
				null //headerList
			);			
		}

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null)
			{
				if (e.NewValue is byte[][]) return;

				var fileStream = new SparseMemoryStream();
				var gZipStream = new GZipStream(fileStream, CompressionMode.Compress);
				PXReflectionSerializer.Serialize(gZipStream, e.NewValue, false);

				gZipStream.Close();
				fileStream.Close();
				e.NewValue = fileStream.GetBuf();
			}			
		}
	}
}
