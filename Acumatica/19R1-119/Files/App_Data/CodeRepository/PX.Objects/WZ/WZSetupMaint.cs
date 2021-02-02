using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.WZ
{
    public class WZSetupMaint : PXGraph<WZSetupMaint>
    {
        public PXSelect<WZSetup> Setup;

        protected virtual IEnumerable setup()
        {
            PXCache cache = Setup.Cache;
            PXResultset<WZSetup> ret = PXSelect<WZSetup>.SelectSingleBound(this, null);

            if (ret.Count == 0)
            {
                WZSetup setup = (WZSetup)cache.Insert(new WZSetup());
                cache.IsDirty = false;
                ret.Add(new PXResult<WZSetup>(setup));
            }
            else if (cache.Current == null)
            {
                cache.SetStatus((WZSetup)ret, PXEntryStatus.Notchanged);
            }

            return ret;
        }

        public PXAction<WZSetup> enableWizards;
        public PXAction<WZSetup> disableWizards;

        public virtual IEnumerable EnableWizards(PXAdapter adapter)
        {
            WZSetup setup = this.Setup.Select();
            setup.WizardsStatus = true;
            this.Setup.Update(setup);
            this.Actions.PressSave();

            SiteMapMaint siteMapGraph = PXGraph.CreateInstance<SiteMapMaint>();
            
            SiteMap wizardsSiteMapNode = PXSelect<SiteMap, Where<SiteMap.screenID, Equal<Required<SiteMap.screenID>>>>.Select(this, "WZ000000");
            SiteMap organizationSiteMapNode = PXSelect<SiteMap, Where<SiteMap.screenID, Equal<Required<SiteMap.screenID>>>>.Select(this, "OG000000");
            siteMapGraph.SiteMap.Select();
            siteMapGraph.SiteMap.Current = wizardsSiteMapNode;
            siteMapGraph.Children.Select();
            siteMapGraph.Cut.Press();
            siteMapGraph.SiteMap.Current = organizationSiteMapNode;
            siteMapGraph.Children.Select();
            siteMapGraph.Paste.Press();
            siteMapGraph.Save.Press();
            //Activate main scenario
            PXSiteMap.Provider.Clear();
            WZTaskEntry wzGraph = PXGraph.CreateInstance<WZTaskEntry>();
            foreach (WZScenario activeScenario in PXSelect<WZScenario, Where<WZScenario.nodeID, Equal<Required<WZScenario.nodeID>>>>.Select(this, Guid.Empty))
            {
                wzGraph.Scenario.Current = activeScenario;
                wzGraph.activateScenarioWithoutRefresh.Press();
            }
            return adapter.Get();
        }

        public virtual IEnumerable DisableWizards(PXAdapter adapter)
        {
            WZSetup setup = this.Setup.Select();
            setup.WizardsStatus = false;
            this.Setup.Update(setup);
            this.Actions.PressSave();
            SiteMapMaint siteMapGraph = PXGraph.CreateInstance<SiteMapMaint>();

            SiteMap wizardsSiteMapNode = PXSelect<SiteMap, Where<SiteMap.screenID, Equal<Required<SiteMap.screenID>>>>.Select(this, "WZ000000");
            SiteMap hiddenSiteMapNode = PXSelect<SiteMap, Where<SiteMap.screenID, Equal<Required<SiteMap.screenID>>>>.Select(this, "HD000000");
            siteMapGraph.SiteMap.Select();
            siteMapGraph.SiteMap.Current = wizardsSiteMapNode;
            siteMapGraph.Children.Select();
            siteMapGraph.Cut.Press();
            siteMapGraph.SiteMap.Current = hiddenSiteMapNode;
            siteMapGraph.Children.Select();
            siteMapGraph.Paste.Press();
            siteMapGraph.Save.Press();
            //Deactivate all scenarios
            PXSiteMap.Provider.Clear();

            WZTaskEntry wzGraph = PXGraph.CreateInstance<WZTaskEntry>();
            foreach (WZScenario activeScenario in PXSelect<WZScenario, Where<WZScenario.status, Equal<Required<WZScenario.status>>>>.Select(this, WizardScenarioStatusesAttribute._ACTIVE))
            {
                wzGraph.Scenario.Current = activeScenario;
                wzGraph.completeScenarioWithoutRefresh.Press();
            }
            return adapter.Get();
        }
    }
}
