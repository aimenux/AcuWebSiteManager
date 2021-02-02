using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using Microsoft.SqlServer.Server;
using PX.Api;
using PX.Data;
using System.Web.UI;
using PX.Data.Update;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.WZ;
using PX.SM;
using PX.Common;
using System.Collections;
using System.Resources;
using System.Threading;

namespace PX.Objects.WZ
{
    public class PXWizardSiteMapProvider : PXDatabaseSiteMapProvider
    {
        public const string WizardRootNode = "WZ000000";
        public const string OrganizationNode = "OG000000";

		public class WizardDefinition : DatabaseDefinition, IPrefetchable<PXWizardSiteMapProvider>, IInternable
        {
            private readonly Dictionary<Guid, PXSiteMapNode> nodesByID = new Dictionary<Guid, PXSiteMapNode>();
      
            protected override void AddNode(PXSiteMapNode node, Guid parentID)
            {
                base.AddNode(node, parentID);
                if (!nodesByID.ContainsKey(((PXSiteMapNode)node).NodeID))
                    nodesByID.Add(((PXSiteMapNode)node).NodeID, (PXSiteMapNode)node);
            }


            public PXSiteMapNode FindScenarioNodeFromKey(Guid key)
            {
                return nodesByID.ContainsKey(key) ? nodesByID[key] : null;
            }

            void IPrefetchable<PXWizardSiteMapProvider>.Prefetch(PXWizardSiteMapProvider provider)
            {
	            PXContext.SetSlot("PrefetchSiteMap", true);

				System.Globalization.CultureInfo prevCulture = null;
				if (Thread.CurrentThread.CurrentCulture.Name != Thread.CurrentThread.CurrentUICulture.Name)
				{
					prevCulture = Thread.CurrentThread.CurrentCulture;
					Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
				}

                Dictionary<Guid, WZScenario> wizardScenarios = GetWizardScenarios();

                base.Prefetch(provider); // Main site map

                if (!PXSiteMap.IsPortal)
                {
                    foreach (Guid scenarioID in wizardScenarios.Keys)
                    {
                        List<string> scenarioRole = new List<string>();
                        if (!String.IsNullOrEmpty(wizardScenarios[scenarioID].Rolename))
                            scenarioRole.Add(wizardScenarios[scenarioID].Rolename);

                        // If scenario is not active we should use different graph
                        bool scenarioIsActive = wizardScenarios[scenarioID].Status == WizardScenarioStatusesAttribute._ACTIVE;

                        PXSiteMapNode node, rootScenarioNode;

                        if (scenarioIsActive)
                        {
							node = base.FindSiteMapNodesFromGraphType(typeof(WizardScenarioMaint).FullName, false).FirstOrDefault();
                        }
                        else
                        {
							node = base.FindSiteMapNodesFromGraphType(typeof(WizardNotActiveScenario).FullName, false).FirstOrDefault();
                        }


                        string url = PXSiteMap.DefaultFrame;
                        if (node != null)
                        {
                            url = String.Format("{0}?ScenarioID={1}", node.Url,
                                System.Web.HttpUtility.UrlEncode(scenarioID.ToString()));
                        }

                        rootScenarioNode = base.FindSiteMapNodesFromScreenID(WizardRootNode, false).FirstOrDefault();

                        PXSiteMapNode scenarioNode = CreateWizardNode(provider,
                            scenarioID,
                            url,
                            wizardScenarios[scenarioID].Name,
                            wizardScenarios[scenarioID].Name,
                            new PXRoleList(scenarioRole.Count == 0 ? null : scenarioRole, null, null),
                            null,
                            "WZ201500");

                        if (wizardScenarios[scenarioID].NodeID != Guid.Empty)
                        {
                            if (scenarioIsActive)
                            {
                                if (rootScenarioNode != null &&
                                    rootScenarioNode.NodeID == (Guid)wizardScenarios[scenarioID].NodeID)
                                {
                                    PXSiteMapNode DummyNode = base.FindSiteMapNodesFromScreenID("WZ000001", false).FirstOrDefault();

                                    if (DummyNode == null)
                                    {
                                        DummyNode = CreateWizardNode(provider,
                                            Guid.NewGuid(),
                                            PXSiteMap.DefaultFrame,
                                            "Work Area",
                                            "Work Area",
                                            null,
                                            null,
                                            "WZ000001");
                                        AddNode(DummyNode, rootScenarioNode.NodeID);
                                    }

                                    AddNode(scenarioNode, DummyNode.NodeID);

                                }
                                else
                                {
                                    AddNode(scenarioNode, (Guid)wizardScenarios[scenarioID].NodeID);
                                }
                            }
                            else
                            {
                                AddNode(scenarioNode, Guid.Empty);
                            }
                        }
                        else
                        {
                            if (scenarioIsActive)
                            {
                                if (rootScenarioNode != null)
                                {
                                    PXSiteMapNode DummyNode =
                                        base.FindSiteMapNodesFromScreenID("WZ000001", false).FirstOrDefault();

                                    if (DummyNode == null)
                                    {
                                        DummyNode = CreateWizardNode(provider,
                                            Guid.NewGuid(),
                                            PXSiteMap.DefaultFrame,
                                            "Work Area",
                                            "Work Area",
                                            null,
                                            null,
                                            "WZ000001");

                                        AddNode(DummyNode, rootScenarioNode.NodeID);
                                    }

                                    AddNode(scenarioNode, DummyNode.NodeID);
                                }
                            }
                            else
                            {
                                AddNode(scenarioNode, Guid.Empty);
                            }
                        }

                    }
                }

				if (prevCulture != null)
				{
					Thread.CurrentThread.CurrentCulture = prevCulture;
				}

				PXContext.SetSlot("PrefetchSiteMap", false);
            }

			private bool isInterned = false;
			private object internObjectLock = new object();

			public new object Intern()
			{
				var result = this;

				var definitionIntern = new PxObjectsIntern<WizardDefinition>();
				WizardDefinition tempDefinition;
				if (definitionIntern.TryIntern(result, out tempDefinition))
					result = tempDefinition;

				if (result.isInterned) return result;

				lock (result.internObjectLock)
				{
					if (!result.isInterned)
					{
						var refs = new Dictionary<PXSiteMapNode, PXSiteMapNode>(new PXReflectionSerializer.ObjectComparer<object>());

						var nodesIntern = new PxObjectsIntern<PXSiteMapNode>();

						PXSiteMapNode internNode;
						if (nodesIntern.TryIntern(RootNode, out internNode, refs))
							RootNode = internNode;

						InternDictionary(nodesByID, nodesIntern, refs);

						InternMultiDictionary(GraphTypeTable, nodesIntern, refs);
						InternMultiDictionary(ScreenIDTable, nodesIntern, refs);

						InternDictionary(UrlTable, nodesIntern, refs);
						InternDictionary(KeyTable, nodesIntern, refs);

						var keys = ChildNodeCollectionTable.Keys.ToArray();

						for (int i = 0; i < keys.Length; i++)
						{
							var multiKey = keys[i];
							var oldCollection = new SiteMapNodeCollection(ChildNodeCollectionTable[multiKey]);

							foreach (var node in oldCollection)
							{
								if (nodesIntern.TryIntern((PXSiteMapNode) node, out internNode, refs))
								{
									ChildNodeCollectionTable[multiKey].Remove((SiteMapNode) node);
									ChildNodeCollectionTable[multiKey].Add(internNode);
								}
							}
						}

						result.isInterned = true;
					}
				}
				return result;
			}

			private Dictionary<Guid, WZScenario> GetWizardScenarios()
            {
                Dictionary<Guid, WZScenario> result = new Dictionary<Guid, WZScenario>();

                foreach (PXDataRecord record in PXDatabase.SelectMulti(typeof(WZScenario),
                    new PXDataField<WZScenario.scenarioID>(),
                    new PXDataField<WZScenario.nodeID>(),
                    new PXDataField<WZScenario.name>(),
                    new PXDataField<WZScenario.rolename>(),
                    new PXDataField<WZScenario.status>(),
                    new PXDataField<WZScenario.scenarioOrder>()))
                {
                    WZScenario row = new WZScenario
                    {
                        ScenarioID = (Guid)record.GetGuid(0),
                        NodeID = (Guid)record.GetGuid(1),
                        Name = record.GetString(2),
                        Rolename = record.GetString(3),
                        Status = record.GetString(4),
                        ScenarioOrder = record.GetInt32(5)
                    };
                    result.Add((Guid)row.ScenarioID, row);
                }
                result = result.OrderBy(x => x.Value.ScenarioOrder).ToDictionary(x => x.Key, x => x.Value);
                return result;
            }

            private PXSiteMapNode CreateWizardNode(PXWizardSiteMapProvider provider,
                Guid nodeID,
                string url,
                string title,
                string description,
                PXRoleList roles,
                string graphType,
                string screenID
                )
            {

                return new PXSiteMapNode(provider, nodeID, url, title, description, roles, graphType, screenID);
            }
		}

        public override void Initialize(string name, NameValueCollection config)
        {
            if (!string.IsNullOrEmpty(config["table"]))
            {
                Table = config["table"];
                config.Remove("table");
            }

            base.Initialize(name, config);
        }

        public PXSiteMapNode FindScenarioNodeFromKey(Guid key)
        {
            return ((WizardDefinition)Definitions).FindScenarioNodeFromKey(key);
        }

        protected override Definition GetSlot(string slotName)
        {
            return PXDatabase.GetSlot<WizardDefinition, PXWizardSiteMapProvider>(slotName + Thread.CurrentThread.CurrentUICulture.Name, this, Tables);
        }
        protected override void ResetSlot(string slotName)
        {
            PXDatabase.ResetSlot<WizardDefinition>(slotName + Thread.CurrentThread.CurrentUICulture.Name, Tables);
        }

	    protected new static readonly Type[] Tables = PXDatabaseSiteMapProvider.Tables.Append(typeof (WZScenario));
    }

}
