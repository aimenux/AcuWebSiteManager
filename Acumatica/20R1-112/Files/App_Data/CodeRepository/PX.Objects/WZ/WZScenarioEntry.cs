using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using PX.Data;
using PX.SM;

namespace PX.Objects.WZ
{
    [Serializable]
    [PX.Objects.GL.TableAndChartDashboardType]
    public class WZScenarioEntry : PXGraph<WZScenarioEntry, WZScenario>
    {
        public PXSelectJoinOrderBy<WZScenario, LeftJoin<SiteMap, On<WZScenario.nodeID, Equal<SiteMap.nodeID>>>, OrderBy<Asc<WZScenario.scenarioOrder>>> Scenarios;
        public PXSelectSiteMapTree<False, False, False, False, False> SiteMap;
        //public PXSelect<SiteMap> SiteMapView;

        public PXSelect<WZTask> Task; 
        public PXSelect<WZTaskRelation> TaskRelation;
        public PXSelect<WZTaskFeature> TaskFeature;

        public PXAction<WZScenario> activateScenario;
        public PXAction<WZScenario> suspendScenario;
        public PXAction<WZScenario> completeScenario;
        public PXAction<WZScenario> viewScenarioDetails;

        public bool IsSiteMapAltered { get; internal set; }

        public override bool CanClipboardCopyPaste() { return false; }

        protected virtual IEnumerable scenarios()
        {
            foreach (PXResult<WZScenario, SiteMap> result in PXSelectJoinOrderBy
                <WZScenario, LeftJoin<SiteMap, On<WZScenario.nodeID, Equal<SiteMap.nodeID>>>,
                    OrderBy<Asc<WZScenario.scenarioOrder>>>.Select(this))
            {
                WZScenario scenario = result;
                SiteMap siteMap = result;
                if (scenario.NodeID != null && siteMap.NodeID == null)
                {
                    siteMap.NodeID = scenario.NodeID;
                    WZScenario parentScenario =
                        PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>
                            .Select(this, scenario.NodeID);
                    if (parentScenario != null)
                        siteMap.Title = parentScenario.Name;
                }

                yield return new PXResult<WZScenario, SiteMap>(scenario, siteMap); ;
            }
        }

        protected virtual void WZScenario_ScenarioID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            WZScenario row = e.Row as WZScenario;
            if (row == null) return;
            e.NewValue = Guid.NewGuid();
        }

        protected virtual void WZScenario_NodeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            WZScenario row = e.Row as WZScenario;
            if (row == null) return;
			e.NewValue = Guid.NewGuid();
        }

        protected virtual void WZScenario_NodeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            WZScenario row = e.Row as WZScenario;
            if (row == null) return;

            PXSiteMap.Provider.Clear();
        }

        protected virtual void WZScenario_NodeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            WZScenario row = e.Row as WZScenario;
            if (row != null && e.NewValue != null)
            {
                WZScenario result = PXSelect<WZScenario, 
                                       Where<WZScenario.nodeID, Equal<Required<WZScenario.nodeID>>>>
                    .Select(this, Guid.Empty);
                if (result != null && result.ScenarioID != row.ScenarioID && (Guid)e.NewValue == Guid.Empty)
                {
                    e.Cancel = true;
                    throw new PXSetPropertyException(Messages.ScenarioRootLocationError, PXErrorLevel.Error);
                }

            }
        }

        protected virtual void WZScenario_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            Scenarios.View.RequestRefresh();
        }

        [PXUIField(DisplayName = "Activate Scenario", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton]
        public virtual IEnumerable ActivateScenario(PXAdapter adapter)
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();
            
            WZScenario row = Scenarios.Current;
            if (row != null)
            {
                graph.Scenario.Current = row;
                graph.activateScenario.Press();
            }
            
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Suspend Scenario", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton]
        public virtual IEnumerable SuspendScenario(PXAdapter adapter)
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();

            WZScenario row = Scenarios.Current;
            if (row != null)
            {
                graph.Scenario.Current = row;
                graph.suspendScenario.Press();
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = "Complete Scenario", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton]
        public virtual IEnumerable CompleteScenario(PXAdapter adapter)
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();

            WZScenario row = Scenarios.Current;
            if (row != null)
            {
                graph.Scenario.Current = row;
                graph.completeScenario.Press();
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = "View Scenario Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton]
        public virtual IEnumerable ViewScenarioDetails(PXAdapter adapter)
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();

            WZScenario row = Scenarios.Current;
            if (row != null)
            {
                graph.Scenario.Current = row;
                throw new PXRedirectRequiredException(graph, "Scenario Tasks");
            }
            return adapter.Get();
        }
    }
}
