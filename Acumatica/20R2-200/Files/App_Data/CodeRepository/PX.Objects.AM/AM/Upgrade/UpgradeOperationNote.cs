using System.Collections.Generic;
using Customization;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class UpgradeOperationNote : UpgradeProcessVersionBase
    {
        public UpgradeOperationNote(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public UpgradeOperationNote(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new UpgradeOperationNote(upgradeGraph, plugin) { _upgradeFromVersion = upgradeFrom };
            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();
                return true;
            }
            return false;
        }

        private int _upgradeFromVersion;
        public override int Version => UpgradeVersions.Version2018R2Ver17;
        private bool UpgradeRanInPreviousVersion => _upgradeFromVersion.BetweenInclusive(UpgradeVersions.Version2017R2Ver1015, UpgradeVersions.MaxVersionNumbers.Version2017R2) || _upgradeFromVersion.BetweenInclusive(UpgradeVersions.Version2018R1Ver1013, UpgradeVersions.MaxVersionNumbers.Version2018R1);

        public override void ProcessTables()
        {
            if (UpgradeRanInPreviousVersion)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"Process ran in previous version; Upgrading from {_upgradeFromVersion}");
#endif
                return;
            }

            SetProdOperNotes();
        }

        /// <summary>
        /// For the material allocation records to correctly link to the parent via refnote and display in inventory allocation detail we need to have a Note record on the operation
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<AMProdOper> GetProdOperWithoutNotes(PXGraph graph, int nbrOfRows)
        {
            return PXSelectJoin<
                    AMProdOper,
                    InnerJoin<AMProdItem,
                        On<AMProdOper.orderType, Equal<AMProdItem.orderType>,
                            And<AMProdOper.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                        LeftJoin<Note, On<AMProdOper.noteID, Equal<Note.noteID>>>>,
                    Where<AMProdItem.statusID, NotEqual<ProductionOrderStatus.cancel>,
                        And<AMProdItem.statusID, NotEqual<ProductionOrderStatus.closed>,
                            And<Note.noteID, IsNull>>>>
                .SelectWindowed(graph, 0, nbrOfRows)
                .ToFirstTable();
        }

        private void SetProdOperNotes()
        {
            var graph = PXGraph.CreateInstance<QtyAllocationUpgradeGraph>();

            var cntr = 0;
            var nbrOfRows = 5000;
            var foundOpers = true;
            long insertedNotes = 0;
            long totalInsertedNotes = 0;
            while (foundOpers)
            {
                cntr++;

                if(cntr >= 500)
                {
                    //Safety net in case the updates are not making it the query would continue to return the same results over and over
                    break;
                }

                if(TrySetProdOperNotes(graph, nbrOfRows, out insertedNotes))
                {
                    if (cntr == 1 || insertedNotes < nbrOfRows || cntr % 2 == 1)
                    {
                        WriteInfo("Inserting notes related to existing production operations"); 
                    }
                    totalInsertedNotes += insertedNotes;

                    graph.Actions.PressSave();

                    continue;
                }

                foundOpers = false;
            }

            if(totalInsertedNotes > 0)
            {
                WriteInfo($"Inserted {totalInsertedNotes} Notes");
            }
        }

        private static bool TrySetProdOperNotes(PXGraph graph, int nbrOfRows, out long insertedNotes)
        {
            insertedNotes = 0;
            var beforeNoteInserts = graph.Caches<Note>().Inserted.Count();
            var foundOpers = false;

            foreach (var prodOper in GetProdOperWithoutNotes(graph, nbrOfRows))
            {
                if (prodOper == null)
                {
                    continue;
                }

                var row = graph.Caches<AMProdOper>().LocateElse(prodOper);
                if (row == null)
                {
                    continue;
                }

                foundOpers = true;

                if (row.NoteID == null)
                {
                    row.NoteID = SequentialGuid.Generate();
                    row = (AMProdOper)graph.Caches<AMProdOper>().Update(row);
                }

                // This will call PXNoteAttribute EnsureNoteID which will insert the Note record if not existing
                PXNoteAttribute.GetNoteID<AMProdOper.noteID>(graph.Caches<AMProdOper>(), row);
            }

            insertedNotes = graph.Caches<Note>().Inserted.Count() - beforeNoteInserts;

            return foundOpers && insertedNotes > 0;
        }
    }
}