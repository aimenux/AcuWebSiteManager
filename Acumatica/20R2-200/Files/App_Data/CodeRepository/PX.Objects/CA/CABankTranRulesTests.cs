using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using PX.Data;

namespace PX.Objects.CA
{
    [Serializable]
    public partial class RuleTest : IBqlTable
    {
        [PXString(IsKey = true)]
        [PXUIField(DisplayName = "Test", Enabled = false)]
        public string TestName { get; set; }

        public abstract class result : PX.Data.BQL.BqlBool.Field<result> { }

        [PXBool]
        [PXUIField(DisplayName = "Result", Enabled = false)]
        public bool? Result { get; set; }
    }

    public class RuleTestDefinition
    {
        public RuleTest TestInfo { get; set;}

        public CABankTranRule Rule { get; set; }
        public IEnumerable<CABankTran> Matches { get; set; }
        public IEnumerable<CABankTran> NotMatches { get; set; }
    }

    public class CABankTranRulesTests : CABankTransactionsMaint
    {
        public override bool IsDirty { get { return false; } }

        public PXSelect<RuleTest> Tests;

        public IEnumerable tests()
        {
            if(Tests.Cache.IsInsertedUpdatedDeleted == false)
            {
                foreach(var test in TestsDefinitions)
                {
                    Tests.Insert(test.TestInfo);
                }
            }

            foreach (var test in Tests.Cache.Inserted)
                yield return test;
        }

        public PXAction<RuleTest> Run;

        [PXUIField(DisplayName = "Run tests")]
        [PXButton]
        public void run()
        {
            foreach(var test in Tests.Cache.Inserted)
            {
                RunTest((RuleTest)test);
            }
        }

        private void RunTest(RuleTest test)
        {
            var definition = TestsDefinitions.First(td => td.TestInfo.TestName == test.TestName);

            bool result = true;
            string message = null;

            try
            {
                Rules.Cache.Insert(definition.Rule);

                if (definition.Matches != null)
                {
                    foreach (var transaction in definition.Matches)
                    {
                        var match = CheckRuleMatches(transaction, definition.Rule);
                        if (match == false)
                        {
                            result = false;
                            message += String.Format("Did not match {0}\n", transaction.TranID);
                        }
                    }
                }

                if (definition.NotMatches != null)
                {
                    foreach (var transaction in definition.NotMatches)
                    {
                        var match = CheckRuleMatches(transaction, definition.Rule);
                        if (match == true)
                        {
                            result = false;
                            message += String.Format("Matched {0}\n", transaction.TranID);
                        }
                    }
                }

                Rules.Cache.Clear();
            }
            catch(Exception exc)
            {
                result = false;
                message = String.Format("Exception occured: {0}\n{1}", exc.GetType().Name, exc.Message);
            }

            test.Result = result;
            Tests.Cache.Update(test);

            if(result == false)
            {
                PXUIFieldAttribute.SetError<RuleTest.result>(Tests.Cache, test, message);
            }
        }

        protected virtual IEnumerable<RuleTestDefinition> TestsDefinitions
        {
            get
            {
                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Description only matches description in all positions" },
                    Rule = new CABankTranRule { BankTranDescription = "BANK FEE" },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, TranDesc = "wow such BANK FEE asda"},
                        new CABankTran { TranID = 1, TranDesc = "BANK FEE asda"},
                        new CABankTran { TranID = 2, TranDesc = "wow such BANK FEE"},
                        new CABankTran { TranID = 3, TranDesc = "BANK FEE"}
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Description does not match missing/partial/split" },
                    Rule = new CABankTranRule { BankTranDescription = "BANK FEE" },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, TranDesc = "wow such asda"},
                        new CABankTran { TranID = 1, TranDesc = "BANK asda"},
                        new CABankTran { TranID = 2, TranDesc = "wow such FEE"},
                        new CABankTran { TranID = 3, TranDesc = "BANKFEE"},
                        new CABankTran { TranID = 4, TranDesc = "BA NKFEE"},
                        new CABankTran { TranID = 5, TranDesc = "BANK 111 FEE"},
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Amount Equal matches exact transaction amount" },
                    Rule = new CABankTranRule { AmountMatchingMode = MatchingMode.Equal, CuryTranAmt = 150 },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CuryTranAmt = 150 },
                        new CABankTran { TranID = 1, CuryTranAmt = 150, TranDesc = "BANK FEE asda"}
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CuryTranAmt = 149.99m },
                        new CABankTran { TranID = 1, CuryTranAmt = 400 },
                        new CABankTran { TranID = 2, CuryTranAmt = 0 }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Amount Equal matches together with description" },
                    Rule = new CABankTranRule { AmountMatchingMode = MatchingMode.Equal, CuryTranAmt = 150, BankTranDescription = "BANK FEE" },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, CuryTranAmt = 150, TranDesc = "BANK FEE asda"}
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CuryTranAmt = 150 },
                        new CABankTran { TranID = 1, TranDesc = "BANK FEE asda"},
                        new CABankTran { TranID = 2, CuryTranAmt = 151, TranDesc = "BANK FEE asda"},
                        new CABankTran { TranID = 3, CuryTranAmt = 150, TranDesc = "B@NK FEE asda"}
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Rule with currency matches only trans in the specified currency" },
                    Rule = new CABankTranRule { TranCuryID = "EUR" },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, CuryID = "EUR" },
                        new CABankTran { TranID = 2, CuryID = "EUR", CuryTranAmt = 100.0m, TranDesc = "Something" }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CuryTranAmt = 150 },
                        new CABankTran { TranID = 1, TranDesc = "EUR"},
                        new CABankTran { TranID = 2, CuryID = "USD" },
                        new CABankTran { TranID = 3, CuryTranAmt = 150, TranDesc = "B@NK FEE asda", CuryID = "GBP" }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Rule with currency matches trans in the specified currency even if there are extra spaces in currency field" },
                    Rule = new CABankTranRule { TranCuryID = "EUR" },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, CuryID = "EUR  " },
                        new CABankTran { TranID = 1, CuryID = "  EUR" },
                        new CABankTran { TranID = 1, CuryID = "  EUR   " }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 2, CuryID = " USD " },
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Rule with CashAccount matches only trans on the specified cash account" },
                    Rule = new CABankTranRule { BankTranCashAccountID = 2 },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, CashAccountID = 2 },
                        new CABankTran { TranID = 2, CashAccountID = 2, CuryID = "EUR", CuryTranAmt = 100.0m, TranDesc = "Something" }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CuryTranAmt = 150 },
                        new CABankTran { TranID = 1, TranDesc = "EUR"},
                        new CABankTran { TranID = 2, CashAccountID = 12, CuryID = "USD" },
                        new CABankTran { TranID = 3, CashAccountID = 12, CuryTranAmt = 150, TranDesc = "B@NK FEE asda", CuryID = "GBP" }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "CashAccount matching works together with Description and Amount matching" },
                    Rule = new CABankTranRule { BankTranCashAccountID = 2, AmountMatchingMode = MatchingMode.Equal, CuryTranAmt = 20, BankTranDescription = "FEE" },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, CashAccountID = 2, CuryTranAmt = 20.0m, TranDesc = "FEE" },
                        new CABankTran { TranID = 2, CashAccountID = 2, CuryTranAmt = 20.0m, TranDesc = "BANK FEE" }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CashAccountID = 2, CuryTranAmt = 20.0m, TranDesc = "BANK EE" },
                        new CABankTran { TranID = 1, CashAccountID = 2, CuryTranAmt = 21.0m, TranDesc = "BANK FEE" },
                        new CABankTran { TranID = 2, CashAccountID = 1, CuryTranAmt = 20.0m, TranDesc = "BANK FEE" }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Amount Between matches transactions with amount in the specified range (inclusive)" },
                    Rule = new CABankTranRule { AmountMatchingMode = MatchingMode.Between, CuryTranAmt = 100, MaxCuryTranAmt = 200 },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CuryTranAmt = 150 },
                        new CABankTran { TranID = 1, CuryTranAmt = 100 },
                        new CABankTran { TranID = 2, CuryTranAmt = 200 },
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CuryTranAmt = 99.99m },
                        new CABankTran { TranID = 0, CuryTranAmt = 200.01m },
                        new CABankTran { TranID = 1, CuryTranAmt = 400 },
                        new CABankTran { TranID = 2, CuryTranAmt = 0 }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Amount None ignores both Amount Criteria when matching" },
                    Rule = new CABankTranRule { AmountMatchingMode = MatchingMode.None, CuryTranAmt = 100, MaxCuryTranAmt = 200 },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, CuryTranAmt = 150 },
                        new CABankTran { TranID = 1, CuryTranAmt = 100 },
                        new CABankTran { TranID = 2, CuryTranAmt = 200 },
                        new CABankTran { TranID = 0, CuryTranAmt = 99.99m },
                        new CABankTran { TranID = 0, CuryTranAmt = 200.01m },
                        new CABankTran { TranID = 1, CuryTranAmt = 400 },
                        new CABankTran { TranID = 2, CuryTranAmt = 0 }
                    },
                    NotMatches = new List<CABankTran>
                    {
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Description Criteria ignores case by default" },
                    Rule = new CABankTranRule { BankTranDescription = "BANK FEE" },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, TranDesc = "Bank fee" },
                        new CABankTran { TranID = 1, TranDesc = "bank fee" },
                        new CABankTran { TranID = 2, TranDesc = "BaNk FEE" },
                        new CABankTran { TranID = 3, TranDesc = "BANK FEE" }
                    },
                    NotMatches = new List<CABankTran>
                    {
                    }
                };



                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Description Criteria matches case if asked to" },
                    Rule = new CABankTranRule { BankTranDescription = "BANK FEE", MatchDescriptionCase = true },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 3, TranDesc = "BANK FEE" }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 0, TranDesc = "Bank fee" },
                        new CABankTran { TranID = 1, TranDesc = "bank fee" },
                        new CABankTran { TranID = 2, TranDesc = "BaNk FEE" }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Tran Code criterion matches only equal tran codes (case ignored)" },
                    Rule = new CABankTranRule { TranCode = "BANKT" },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT" },
                        new CABankTran { TranID = 2, TranCode = "bankT" },
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "NOTBANK" },
                        new CABankTran { TranID = 2, TranCode = "SOME BANKT" },
                        new CABankTran { TranID = 3, TranCode = " BANKT" },
                        new CABankTran { TranID = 4, TranCode = "BANKT " },
                        new CABankTran { TranID = 5, TranCode = "", TranDesc = "BANKT" }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Tran Code criterion works well with Description and Amount criteria" },
                    Rule = new CABankTranRule { TranCode = "BANKT", BankTranDescription = "fee", AmountMatchingMode = MatchingMode.Equal, CuryTranAmt = 100.0m },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "my bank fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "bankT", TranDesc = "FEE", CuryTranAmt = 100.0m },
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "NOTBANK", TranDesc = "my bank fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "SOME BANKT", TranDesc = "my bank fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 3, TranCode = "BANKT", TranDesc = "my bank", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 4, TranCode = "BANKT", TranDesc = "my bank fee", CuryTranAmt = 60.0m },
                        new CABankTran { TranID = 5, TranDesc = "my bank fee", CuryTranAmt = 100.0m }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Properly handles the '?' wildcard in the Description criterion" },
					Rule = new CABankTranRule { BankTranDescription = "f?e", UseDescriptionWildcards = true },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "BANKT", TranDesc = "foe", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 3, TranCode = "BANKT", TranDesc = "FEE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 4, TranCode = "BANKT", TranDesc = "FOE", CuryTranAmt = 100.0m }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "fooe", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "BANKT", TranDesc = "fe2e some", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 3, TranCode = "BANKT", TranDesc = " few", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 4, TranCode = "BANKT", TranDesc = "F?EE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 5, TranCode = "BANKT", TranDesc = "bank fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 6, TranCode = "BANKT", TranDesc = "fee some", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 7, TranCode = "BANKT", TranDesc = " fee", CuryTranAmt = 100.0m }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Properly handles the '?' wildcard in the Description criterion with Match Case" },
					Rule = new CABankTranRule { BankTranDescription = "f?e", MatchDescriptionCase = true, UseDescriptionWildcards = true },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "BANKT", TranDesc = "foe", CuryTranAmt = 100.0m }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "fooe", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "BANKT", TranDesc = "FEE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 3, TranCode = "BANKT", TranDesc = "FOE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 4, TranCode = "BANKT", TranDesc = "F?E", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 5, TranCode = "BANKT", TranDesc = "fee some", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 6, TranCode = "BANKT", TranDesc = " fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 7, TranCode = "BANKT", TranDesc = "bank fee", CuryTranAmt = 100.0m }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Properly handles the '*' wildcard in the Description criterion" },
					Rule = new CABankTranRule { BankTranDescription = "f*e", UseDescriptionWildcards = true },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "BANKT", TranDesc = "foe", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 3, TranCode = "BANKT", TranDesc = "fooe", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 4, TranCode = "BANKT", TranDesc = "FEE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 5, TranCode = "BANKT", TranDesc = "FE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 6, TranCode = "BANKT", TranDesc = "F-qew12E", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 7, TranCode = "BANKT", TranDesc = "FOE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 8, TranCode = "BANKT", TranDesc = "F?E", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 9, TranCode = "BANKT", TranDesc = "F*E", CuryTranAmt = 100.0m }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "bank 11ee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "BANKT", TranDesc = "bank f111ee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 3, TranCode = "BANKT", TranDesc = "ee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 4, TranCode = "BANKT", TranDesc = " fff", CuryTranAmt = 100.0m }
                    }
                };

                yield return new RuleTestDefinition
                {
                    TestInfo = new RuleTest { TestName = "Properly handles the '*' wildcard in the Description criterion with Match Case" },
                    Rule = new CABankTranRule { BankTranDescription = "f*E", MatchDescriptionCase = true, UseDescriptionWildcards = true },
                    Matches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "feE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "BANKT", TranDesc = "foE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 3, TranCode = "BANKT", TranDesc = "fooE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 4, TranCode = "BANKT", TranDesc = "feE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 5, TranCode = "BANKT", TranDesc = "fE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 6, TranCode = "BANKT", TranDesc = "f-qew12E", CuryTranAmt = 100.0m }
                    },
                    NotMatches = new List<CABankTran>
                    {
                        new CABankTran { TranID = 1, TranCode = "BANKT", TranDesc = "FEE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 2, TranCode = "BANKT", TranDesc = "bank fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 3, TranCode = "BANKT", TranDesc = "fee som", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 4, TranCode = "BANKT", TranDesc = " fee", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 5, TranCode = "BANKT", TranDesc = "fe", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 6, TranCode = "BANKT", TranDesc = "F-qew12e", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 7, TranCode = "BANKT", TranDesc = "FOE", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 8, TranCode = "BANKT", TranDesc = "F?E", CuryTranAmt = 100.0m },
                        new CABankTran { TranID = 9, TranCode = "BANKT", TranDesc = "F*E", CuryTranAmt = 100.0m },
                    }
                };

				yield return new RuleTestDefinition
				{
					TestInfo = new RuleTest { TestName = "Won't apply wildcards if not explicitly asked to" },
					Rule = new CABankTranRule { BankTranDescription = "f*o" },
					Matches = new List<CABankTran>
					{
						new CABankTran { TranID = 1, TranDesc = "f*o" },
						new CABankTran { TranID = 2, TranDesc = "wow f*o"},
						new CABankTran { TranID = 2, TranDesc = "wow F*o wow"}
					},
					NotMatches = new List<CABankTran>
					{
						new CABankTran { TranID = 1, TranDesc = "foo" },
						new CABankTran { TranID = 2, TranDesc = "wow foo"},
						new CABankTran { TranID = 2, TranDesc = "wow foo wow"}
					}
				};

				yield return new RuleTestDefinition
				{
					TestInfo = new RuleTest { TestName = "Matches description in 'Equals' mode when UseDescriptionWildcards is on" },
					Rule = new CABankTranRule { BankTranDescription = "f*o", UseDescriptionWildcards = true },
					Matches = new List<CABankTran>
					{
						new CABankTran { TranID = 1, TranDesc = "foo" }
					},
					NotMatches = new List<CABankTran>
					{
						new CABankTran { TranID = 1, TranDesc = "wow f*o"},
						new CABankTran { TranID = 2, TranDesc = "wow foo"},
						new CABankTran { TranID = 3, TranDesc = "f*o*" },
						new CABankTran { TranID = 4, TranDesc = "F*o wow"},
						new CABankTran { TranID = 5, TranDesc = " foo wow"},
						new CABankTran { TranID = 6, TranDesc = " foo"}
					}
				};

				yield return new RuleTestDefinition
				{
					TestInfo = new RuleTest { TestName = "Start wildcards can be used for 'Contains' mode when UseDescriptionWildcards is on" },
					Rule = new CABankTranRule { BankTranDescription = "*f*o*", UseDescriptionWildcards = true },
					Matches = new List<CABankTran>
					{
						new CABankTran { TranID = 1, TranDesc = "foo" },
						new CABankTran { TranID = 2, TranDesc = "wow f*o"},
						new CABankTran { TranID = 3, TranDesc = "wow foo"},
						new CABankTran { TranID = 4, TranDesc = "f*o*" },
						new CABankTran { TranID = 5, TranDesc = "F*o wow"},
						new CABankTran { TranID = 6, TranDesc = " foo wow"},
						new CABankTran { TranID = 7, TranDesc = " foo"}
					},
					NotMatches = new List<CABankTran>
					{
						new CABankTran {TranID = 1, TranDesc = "ooo fff"},
						new CABankTran {TranID = 2, TranDesc = ""}
					}
				};
            }
        }
    }
}
