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

		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2020R2)]
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

		public static void SetTranslations<TSourceField, TDestinationField>(
			PXCache sourceCache, object sourceData, PXCache destinationCache, object destinationData,
			Func<string, string> processTranslation)
			where TSourceField : IBqlField
			where TDestinationField : IBqlField
		{
			if (IsEnabled)
			{
				string[] translations = new string[EnabledLocales.Count];

				for (int translationIndex = 0; translationIndex < EnabledLocales.Count; translationIndex++)
				{
					string locale = EnabledLocales[translationIndex];
					translations[translationIndex] = processTranslation(locale);
				}

				destinationCache.Adjust<DBMatrixLocalizableDescriptionAttribute>(destinationData).For<TDestinationField>(a =>
					a.SetTranslations(destinationCache, destinationData, translations));
			}
		}

	}
}
