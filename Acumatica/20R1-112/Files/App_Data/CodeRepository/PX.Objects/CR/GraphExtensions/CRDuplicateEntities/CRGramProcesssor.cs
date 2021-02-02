using System;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;


namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	internal class CRGramProcessor : CRGrammProcess.Processor
	{
		public CRGramProcessor(PXGraph graph)
			: base(graph) { }

		public bool PersistGrams(DuplicateDocument document, bool requireRecreate = false)
		{
			if (graph.Caches[document.GetType()].GetStatus(document) == PXEntryStatus.Deleted)
			{
				PXDatabase.Delete<CRGrams>(new PXDataFieldRestrict<CRGrams.entityID>(PXDbType.Int, 4, document.ContactID, PXComp.EQ));

				return false;
			}

			if (!requireRecreate && GrammSourceUpdated(document))
				return false;

			PXDatabase.Delete<CRGrams>(new PXDataFieldRestrict<CRGrams.entityID>(PXDbType.Int, 4, document.ContactID, PXComp.EQ));

			foreach (CRGrams gram in DoCreateGramms(document))
			{
				PXDatabase.Insert<CRGrams>(
					new PXDataFieldAssign(typeof(CRGrams.entityID).Name, PXDbType.Int, 4, document.ContactID),
					new PXDataFieldAssign(typeof(CRGrams.fieldName).Name, PXDbType.NVarChar, 60, gram.FieldName),
					new PXDataFieldAssign(typeof(CRGrams.fieldValue).Name, PXDbType.NVarChar, 60, gram.FieldValue),
					new PXDataFieldAssign(typeof(CRGrams.score).Name, PXDbType.Decimal, 8, gram.Score),
					new PXDataFieldAssign(typeof(CRGrams.validationType).Name, PXDbType.NVarChar, 2, gram.ValidationType)
				);
			}

			document.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
			document.GrammValidationDateTime = PXTimeZoneInfo.Now;

			PXDatabase.Update<Contact>(
				new PXDataFieldAssign<Contact.duplicateStatus>(PXDbType.NVarChar, document.DuplicateStatus),
				new PXDataFieldAssign<Contact.grammValidationDateTime>(PXDbType.DateTime, PXTimeZoneInfo.ConvertTimeToUtc(document.GrammValidationDateTime.Value, LocaleInfo.GetTimeZone())),
				new PXDataFieldRestrict<Contact.contactID>(PXDbType.Int, document.ContactID)
			);

			return true;
		}

		public bool GrammSourceUpdated(DuplicateDocument document)
		{
			var main = graph.Caches[typeof(DuplicateDocument)].GetMain(document);

			PXCache cache = graph.Caches[main.GetType()];

			if (cache.GetStatus(main) == PXEntryStatus.Inserted || cache.GetStatus(main) == PXEntryStatus.Notchanged)
				return false;

			if (Definition.ContactRules(document.ContactType)
				.Any(rule => !String.Equals(
					cache.GetValue(main, rule.MatchingField)?.ToString(),
					cache.GetValueOriginal(main, rule.MatchingField)?.ToString(),
					StringComparison.InvariantCultureIgnoreCase)))
			{
				return false;
			}

			return true;
		}

		// copy-paste from base
		internal IEnumerable DoCreateGramms(DuplicateDocument document)
		{
			var main = graph.Caches[typeof(DuplicateDocument)].GetMain(document);

			string[] types = document.ContactType == ContactTypesAttribute.BAccountProperty ? AccountTypes : ContactTypes;

			foreach (string validationType in types)
			{
				PXCache cache = graph.Caches[main.GetType()];
				PXCache lCache = graph.Caches[typeof(Location)];

				Location defLocation = null;
				if (document.BAccountID != null)
				{
					BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<Contact.bAccountID>>>>.Select(graph, document.BAccountID);
					if (bAccount != null && bAccount.DefLocationID != null)
						defLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<BAccount.defLocationID>>>>>.Select(graph, bAccount.BAccountID, bAccount.DefLocationID);
				}


				string type = validationType;

				decimal total = 0, totalZero = 0;
				foreach (var rule in Definition.TypeRules[type])
				{
					if (type == ValidationTypesAttribute.Account && defLocation != null)
					{
						if (lCache.GetValue(defLocation, rule.MatchingField) == null && cache.GetValue(main, rule.MatchingField) == null)
							totalZero += rule.ScoreWeight.GetValueOrDefault();
						else
							total += rule.ScoreWeight.GetValueOrDefault();
					}
					else
					{
						if (cache.GetValue(main, rule.MatchingField) == null)
							totalZero += rule.ScoreWeight.GetValueOrDefault();
						else
							total += rule.ScoreWeight.GetValueOrDefault();
					}
				}

				if (total == 0) continue;

				foreach (CRValidationRules rule in Definition.TypeRules[type])
				{
					string fieldName = rule.MatchingField;
					string transformRule = rule.TransformationRule;
					Decimal sw = rule.ScoreWeight ?? 0;
					if (sw == 0) continue;

					if (sw > 0 && totalZero > 0)
						sw += totalZero * (sw / total);

					var value = cache.GetValue(main, fieldName);
					if (type == ValidationTypesAttribute.Account && value == null)
					{
						value = lCache.GetValue(defLocation, fieldName);
					}

					if (value == null) continue;

					if (transformRule.Equals(TransformationRulesAttribute.SplitWords))
					{
						int? id = document.ContactID;

						value = value.ToString().ToLower();
						string[] words = value.ToString().Split();

						foreach (string word in words)
						{
							Decimal score = Decimal.Round(sw / words.Length, 4);
							if (score <= 0) continue;

							CRGrams gram = new CRGrams
							{
								EntityID = id,
								ValidationType = validationType,
								FieldName = fieldName,
								FieldValue = word,
								Score = score
							};
							yield return gram;
						}
					}
					else
					{
						value = value.ToString().ToLower();
						if (transformRule.Equals(TransformationRulesAttribute.DomainName))
						{
							if (value.ToString().Contains('@'))
							{
								value = value.ToString().Split('@')[1];
							}
							else
							{
								try
								{
									value = new UriBuilder(value.ToString()).Host;
									if (value.ToString().Split('.')[0].Equals("www"))
									{
										value = value.ToString().Substring(value.ToString().IndexOf('.') + 1);
									}
								}
								catch (UriFormatException)
								{
									//Do nothing
								}

							}
						}

						CRGrams gram = new CRGrams
						{
							FieldValue = value.ToString(),
							EntityID = document.ContactID,
							ValidationType = validationType,
							FieldName = fieldName,
							Score = Decimal.Round(sw, 4)
						};

						yield return gram;
					}
				}
			}
		}
	}
}
