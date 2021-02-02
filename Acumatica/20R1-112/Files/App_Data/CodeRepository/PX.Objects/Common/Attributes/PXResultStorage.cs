using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.CS;
using PX.Data;
using PX.DbServices.Model.Schema;

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
						//e.ReturnValue = PXReflectionSerializer.Deserialize(s);	
						e.ReturnValue = Deserialize(s, sender.Graph);
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
				//PXReflectionSerializer.Serialize(gZipStream, e.NewValue, false);
				Serialize(gZipStream, sender.Graph, e.NewValue);
				gZipStream.Close();
				fileStream.Close();
				e.NewValue = fileStream.GetBuf();
			}			
		}

		private void Serialize(System.IO.Stream stream, PXGraph graph, object obj)
		{
			if (!(obj is IEnumerable items)) return;
			using (var xw = System.Xml.XmlWriter.Create(stream))
			{
				xw.WriteStartElement("results");
				bool first = true;
				foreach (object item in items)
				{
					if (item is PXResult result)
					{
						if (first)
						{
							string types = string.Join(",", result.Tables.Select(t => t.FullName));
							xw.WriteAttributeString("types", types);
							first = false;
						}

						xw.WriteStartElement("result");
						foreach (var table in result.Tables)
						{
							var cache = graph.Caches[table];
							var record = result[table];
							string xml = cache.ToXml(record);
							xw.WriteRaw(xml);
						}
						xw.WriteEndElement();
					}
					else
					{
						var table = item.GetType();
						if (first)
						{
							xw.WriteAttributeString("types", table.FullName);
							first = false;
						}
						var cache = graph.Caches[table];
						string xml = cache.ToXml(item);
						xw.WriteRaw(xml);
					}
				}
				xw.WriteEndElement();
			}
		}

		private object Deserialize(System.IO.Stream stream, PXGraph graph)
		{
			var result = new List<object>();
			using (var xr = System.Xml.XmlReader.Create(stream))
			{
				if (xr.ReadToDescendant("results"))
				{
					var typenames = xr.GetAttribute("types");
					if (typenames == null) return result;

					var types = typenames.Split(',')
						.Select(type => Type.GetType(type))
						.ToArray();

					xr.Read();
					while (!xr.EOF)
					{
						object record = null;
						if (types.Length > 1)
						{
							if (xr.Name == "result")
							{
								Type resultType = GetResultType(types.Length);
								object[] items = new object[types.Length];
								xr.Read();
								for (int i = 0; i < types.Length; i++)
								{
									var recXml = xr.ReadOuterXml();
									items[i] = graph.Caches[types[i]].FromXml(recXml);
								}
								record = Activator.CreateInstance(resultType.MakeGenericType(types), items);
								//read next result
								xr.Read();
							}
						}
						else if(xr.Name == "Row")
						{
							var recXml = xr.ReadOuterXml();
							record = graph.Caches[types[0]].FromXml(recXml);
						}
						else
						{
							break;
						}
						result.Add(record);
						if (xr.Name == "results")
							break;
					}
				}
			}
			return result;
		}
		private Type GetResultType(int lenght)
		{
			switch (lenght)
			{
				case 1:
					return typeof(PXResult<>);
				case 2:
					return typeof(PXResult<,>);
				case 3:
					return typeof(PXResult<,,>);
				case 4:
					return typeof(PXResult<,,,>);
				case 5:
					return typeof(PXResult<,,,,>);
				case 6:
					return typeof(PXResult<,,,,,>);
				case 7:
					return typeof(PXResult<,,,,,,>);
				case 8:
					return typeof(PXResult<,,,,,,,>);
				case 9:
					return typeof(PXResult<,,,,,,,,>);
				case 10:
					return typeof(PXResult<,,,,,,,,,>);
				case 11:
					return typeof(PXResult<,,,,,,,,,,>);
				case 12:
					return typeof(PXResult<,,,,,,,,,,,>);
				case 13:
					return typeof(PXResult<,,,,,,,,,,,,>);
				case 14:
					return typeof(PXResult<,,,,,,,,,,,,,>);
				case 15:
					return typeof(PXResult<,,,,,,,,,,,,,,>);
				case 16:
					return typeof(PXResult<,,,,,,,,,,,,,,,>);
				case 17:
					return typeof(PXResult<,,,,,,,,,,,,,,,,>);
				case 18:
					return typeof(PXResult<,,,,,,,,,,,,,,,,,>);
				case 19:
					return typeof(PXResult<,,,,,,,,,,,,,,,,,,>);
				case 20:
					return typeof(PXResult<,,,,,,,,,,,,,,,,,,,>);
				case 21:
					return typeof(PXResult<,,,,,,,,,,,,,,,,,,,,>);
				case 22:
					return typeof(PXResult<,,,,,,,,,,,,,,,,,,,,,>);
				case 23:
					return typeof(PXResult<,,,,,,,,,,,,,,,,,,,,,,>);
				case 24:
					return typeof(PXResult<,,,,,,,,,,,,,,,,,,,,,,,>);
				case 25:
					return typeof(PXResult<,,,,,,,,,,,,,,,,,,,,,,,,>);
				default: return null;
			}
		}

	}
}
