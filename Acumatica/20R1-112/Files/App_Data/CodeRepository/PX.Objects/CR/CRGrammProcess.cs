using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Common.Extensions;
using PX.Data;
using PX.Objects.CS;
using PX.SM;
using PX.Objects.CR.Extensions.CRDuplicateEntities;

namespace PX.Objects.CR
{
	[Serializable]
	public class CRGrammProcess : PXGraph<CRGrammProcess>
	{
		public PXCancel<Contact> Cancel;
		#region Processor

		internal class Processor
		{
			protected readonly PXGraph graph;
			private long processedItems;			
			private DateTime? track;

			protected static readonly string[] ContactTypes = {ValidationTypesAttribute.LeadAccount,ValidationTypesAttribute.LeadContact};
			protected static readonly string[] AccountTypes = {ValidationTypesAttribute.LeadAccount, ValidationTypesAttribute.Account };
			
			public Processor()
				:this(new PXGraph())
			{				
			}
			public Processor(PXGraph graph)
			{
				this.graph = graph;
				processedItems = 0;
				track = null;
			}
			
			public void CreateGrams(Contact contact)
			{
				PersistGrams(contact, true);
			}
			public bool PersistGrams(Contact contact, bool requireRecreate = false)
			{
				try
				{
					if(track == null) track = DateTime.Now;
					if (graph.Caches[contact.GetType()].GetStatus(contact) == PXEntryStatus.Deleted)
					{
						PXDatabase.Delete<CRGrams>(new PXDataFieldRestrict("EntityID", PXDbType.Int, 4, contact.ContactID, PXComp.EQ));
						return false;
					}

					if (!requireRecreate && GrammSourceUpdated(contact)) return false;
					
					PXDatabase.Delete<CRGrams>(new PXDataFieldRestrict("EntityID", PXDbType.Int, 4, contact.ContactID, PXComp.EQ));
					foreach (CRGrams gram in DoCreateGramms(contact))
					{
						PXDatabase.Insert<CRGrams>(
							new PXDataFieldAssign(typeof(CRGrams.entityID).Name, PXDbType.Int, 4, contact.ContactID),
							new PXDataFieldAssign(typeof(CRGrams.fieldName).Name, PXDbType.NVarChar, 60, gram.FieldName),
							new PXDataFieldAssign(typeof(CRGrams.fieldValue).Name, PXDbType.NVarChar, 60, gram.FieldValue),
							new PXDataFieldAssign(typeof(CRGrams.score).Name, PXDbType.Decimal, 8, gram.Score),
							new PXDataFieldAssign(typeof(CRGrams.validationType).Name, PXDbType.NVarChar, 2, gram.ValidationType)					
							);
						//CRGrams row = (CRGrams)graph.Caches[typeof(CRGrams)].Insert(gram);
						//graph.Caches[typeof(CRGrams)].PersistInserted(row);						
					}
					contact.GrammValidationDateTime = PXTimeZoneInfo.Now;
					PXDatabase.Update<Contact>
						(
							new PXDataFieldAssign(typeof(Contact.grammValidationDateTime).Name, PXTimeZoneInfo.ConvertTimeToUtc(contact.GrammValidationDateTime.Value, LocaleInfo.GetTimeZone())),
							new PXDataFieldRestrict(typeof(Contact.contactID).Name, contact.ContactID)
						);
					processedItems += 1;
					return true;
				}
				finally
				{					
					if (processedItems % 100 == 0)
					{
						TimeSpan taken = DateTime.Now - (DateTime)track;
						System.Diagnostics.Debug.WriteLine("Items count:{0}, increment taken {1}", processedItems, taken);
						track = DateTime.Now;
					}
				}
			}		

			public bool GrammSourceUpdated(Contact contact)
			{
				PXCache cache = graph.Caches[contact.GetType()];
				if (cache.GetStatus(contact) == PXEntryStatus.Inserted || 
						cache.GetStatus(contact) == PXEntryStatus.Notchanged) return false;
				if (Definition.ContactRules(contact.ContactType).Any(rule => !object.Equals(cache.GetValue(contact, rule.MatchingField), 
					cache.GetValueOriginal(contact,rule.MatchingField))))				
					return false;
				return true;
			}

			public bool IsRulesDefined
			{
				get { return Definition.IsRulesDefined; }
			}

			protected class ValiadtionDefinition
			{
				public List<CRValidationRules> Rules;
				public Dictionary<string, List<CRValidationRules>> TypeRules;

				private List<CRValidationRules> Contacts;
				private List<CRValidationRules> Accounts;

								
				public void Fill(PXGraph graph)
				{
					PXResultset<CRValidationRules> source = PXSelect<CRValidationRules>.Select(graph);
					Rules = new List<CRValidationRules>();
					Contacts = new List<CRValidationRules>();
					Accounts = new List<CRValidationRules>();
					TypeRules = new Dictionary<string, List<CRValidationRules>>();
					foreach (CRValidationRules rule in source)
					{
						Rules.Add(rule);
						if (!TypeRules.ContainsKey(rule.ValidationType)) TypeRules[rule.ValidationType] = new List<CRValidationRules>();
						TypeRules[rule.ValidationType].Add(rule);

						switch (rule.ValidationType)
						{
							case ValidationTypesAttribute.LeadContact:
								Contacts.Add(rule);
								break;
							case ValidationTypesAttribute.LeadAccount:
								Accounts.Add(rule);
								Contacts.Add(rule);
								break;
							case ValidationTypesAttribute.Account:
								Accounts.Add(rule);
								break;
						}
					}
				}

				public List<CRValidationRules> ContactRules(string contactType)
				{
					return contactType == ContactTypesAttribute.BAccountProperty ? Accounts : Contacts;
				}

				public bool IsRulesDefined
				{
					get { return Contacts.Count > 0 && Accounts.Count > 0; }
				}
			}

			protected ValiadtionDefinition Definition
			{
				get
				{
					ValiadtionDefinition definition = PXDatabase.GetSlot<ValiadtionDefinition>("ValidationRules", typeof(CRValidationRules));
					if (definition.Rules == null)
						lock (definition)
						{
							definition.Fill(graph);
						}
					return definition;
				}
			}		
				
			private IEnumerable DoCreateGramms(Contact contact)
			{
				string[] types = contact.ContactType == ContactTypesAttribute.BAccountProperty ? AccountTypes : ContactTypes;

				foreach (string validationType in types)
				{
					PXCache cache = graph.Caches[contact.GetType()];
                    PXCache lCache = graph.Caches[typeof(Location)];

                    Location defLocation = null;
                    if (contact.BAccountID != null)
                    {
                        BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<Contact.bAccountID>>>>.Select(graph, contact.BAccountID);
                        if(bAccount != null && bAccount.DefLocationID != null)
                            defLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<BAccount.defLocationID>>>>>.Select(graph, bAccount.BAccountID, bAccount.DefLocationID);
                    }
                    

					string type = validationType; // TODO: remove duplicate?
					if (!Definition.TypeRules.ContainsKey(validationType)) continue;

					decimal total = 0, totalZero = 0;
                    foreach (var rule in Definition.TypeRules[type])
                    {
                        if (type == ValidationTypesAttribute.Account && defLocation != null)
                        {
                            if (lCache.GetValue(defLocation, rule.MatchingField) == null && cache.GetValue(contact, rule.MatchingField) == null)
                                totalZero += rule.ScoreWeight.GetValueOrDefault();
                            else
                                total += rule.ScoreWeight.GetValueOrDefault();
                        }
                        else
                        {
                            if (cache.GetValue(contact, rule.MatchingField) == null)
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

                        var value = cache.GetValue(contact, fieldName);
                        if (type == ValidationTypesAttribute.Account && value == null)
                        {
                            value = lCache.GetValue(defLocation, fieldName);
                        }
                        
						if (value == null) continue;
                        string stringValue = value.ToString().ToLower();

                        if (transformRule.Equals(TransformationRulesAttribute.SplitWords))
						{
							int? id = contact.ContactID;
							string[] words = stringValue.Split();

							foreach (string word in words)
							{
								Decimal score = Decimal.Round(sw/words.Length, 4);
								if (score <= 0) continue;
								CRGrams gram = new CRGrams();
								gram.EntityID = id;
								gram.ValidationType = validationType;
								gram.FieldName = fieldName;
								gram.FieldValue = word;
								gram.Score = score;								
								yield return gram;
							}
						}
						else
						{
							if (transformRule.Equals(TransformationRulesAttribute.DomainName))
							{
								if (stringValue.Contains('@'))
								{
                                    stringValue = stringValue.Segment('@', 1);
								}
								else
								{
									try
									{
                                        stringValue = new UriBuilder(stringValue).Host;
                                        int index = stringValue.IndexOf('.');
                                        string firstSegment = index < 0 ? stringValue : stringValue.Substring(0, index);
                                        if (firstSegment.Equals("www"))
										{
                                            stringValue = stringValue.Substring(index + 1);
										}
									}
									catch (UriFormatException)
									{
										//Do nothing
									}
										
								}
							}

							CRGrams gram = new CRGrams();
							gram.FieldValue = stringValue;
							gram.EntityID = contact.ContactID;
							gram.ValidationType = validationType;
							gram.FieldName = fieldName;
							gram.Score = Decimal.Round(sw, 4);

							yield return gram;
						}
					}
				}
			}
		}

		#endregion

		[PXHidden] 
		public PXSelect<BAccount> baccount;
		
		[PXViewDetailsButton(typeof(Contact))]
		[PXViewDetailsButton(typeof(Contact),
			typeof(Select<BAccount,
				Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>>>))]
		public PXProcessing<Contact,
			Where<Contact.grammValidationDateTime, Less<Current<CRSetup.grammValidationDateTime>>>> Items;
	

		public PXSetup<CRSetup> Setup;

		#region Ctors

		public CRGrammProcess()
		{
			Processor processor = new Processor();

			if (!processor.IsRulesDefined)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(CRSetup), typeof(CRSetup).Name);

			Items.Cache.AllowInsert = true;

			PXUIFieldAttribute.SetDisplayName<Contact.displayName>(Items.Cache, Messages.Contact);

			Items.SetProcessDelegate((PXGraph graph, Contact contact) =>
			{
				PersistGrams(graph, contact);
			});

			Items.ParallelProcessingOptions =
				settings =>
				{
					settings.IsEnabled = true;
				};
		}

		#endregion	

		public static bool PersistGrams(PXGraph graph, Contact contact)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>()) return false;
			if (contact != null && contact.ContactID > 1)
			{
				Processor processor = new Processor(graph);
				return processor.PersistGrams(contact);
			}
			return false;
		}
	}
}
