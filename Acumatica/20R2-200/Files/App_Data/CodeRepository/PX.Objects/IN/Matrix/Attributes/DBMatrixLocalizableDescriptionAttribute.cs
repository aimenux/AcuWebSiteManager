using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.Matrix.Attributes
{
	/// <exclude/>
	public class DBMatrixLocalizableDescriptionAttribute : PXDBLocalizableStringAttribute
	{
		public DBMatrixLocalizableDescriptionAttribute(int length) : base(length)
		{
		}

		public static void CopyTranslations<TSourceField, TDestinationField>(
			PXCache sourceCache, object sourceData, PXCache destinationCache, object destinationData,
			Func<string, string> processTranslation)
			where TSourceField : IBqlField
			where TDestinationField : IBqlField
		{
			if (IsEnabled)
			{
				var translationAttribute = sourceCache.GetAttributesReadonly<TSourceField>(sourceData)
					.OfType<DBMatrixLocalizableDescriptionAttribute>().FirstOrDefault();

				string[] translations = translationAttribute.GetTranslations(sourceCache, sourceData)
					.Select(t => processTranslation(t)).ToArray();

				destinationCache.Adjust<DBMatrixLocalizableDescriptionAttribute>(destinationData).For<TDestinationField>(a =>
					a.SetTranslations(destinationCache, destinationData, translations));
			}
		}
	}
}
