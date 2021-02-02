using System;
using Customization;
using PX.Data;
using PX.SM;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// Manufacturing wiki upgrade process
    /// </summary>
    public sealed class UpgradeWiki
    {
        private WikiSetupMaint _wikiGraph;
        private CustomizationPlugin _customizationPlugin;
        public static string MfgWikiName = "HelpJAMS_User";

        public UpgradeWiki(CustomizationPlugin customizationPlugin)
        {
            _customizationPlugin = customizationPlugin ?? throw new ArgumentNullException(nameof(customizationPlugin));
            _wikiGraph = PXGraph.CreateInstance<WikiSetupMaint>();
        }

        public WikiDescriptor GetWikiByName(string wikiName)
        {
            return PXSelect<WikiDescriptor,
                Where<WikiDescriptor.name, Equal<Required<WikiDescriptor.name>>>>.Select(_wikiGraph, wikiName);
        }

        public void Process(string name, string title, string dashDescription)
        {
#if DEBUG
            AMDebug.TraceWriteMethodName($"name = {name}; title = {title}; dashDescription = {dashDescription}; Company = {Common.Current.CompanyName}");
#endif
            var wikiDesc = GetWikiByName(name);
            if (wikiDesc == null)
            {
                return;
            }

            _wikiGraph.Clear();

            _wikiGraph.Wikis.Current = wikiDesc;
            _wikiGraph.CurrentWiki.Select();

            if (_wikiGraph.CurrentWiki.Current == null)
            {
                return;
            }

            var companyMsg = string.IsNullOrWhiteSpace(Common.Current.CompanyName) ? string.Empty : Messages.GetLocal(Messages.InCompany, Common.Current.CompanyName);

            try
            {
                var updated = false;
                var row = _wikiGraph.Wikis.Current;

                if (!row.IsActive.GetValueOrDefault())
                {
                    row.IsActive = true;
                    row.WikiTitle = title;
                    row.WikiDescription = dashDescription;
                    updated = true;
                }

                if (row.DefaultUrl == null)
                {
                    row.DefaultUrl = @"8c3533cc-6b98-494d-91eb-6e3d3891b7fe";
                    updated = true;
                }

                if (row.Category == null)
                {
                    row.Category = @"EU";
                    updated = true;
                }

                if (!updated)
                {
                    return;
                }

                _wikiGraph.Wikis.Update(row);

                _wikiGraph.Actions.PressSave();
            }
            catch (Exception e)
            {
                _customizationPlugin.WriteLog($"*** Unable to upgraded wiki {name} - {title}{companyMsg} ***");
                _customizationPlugin.WriteLog($"*** Upgrade error {e.Message} ***");
                return;
            }

            _customizationPlugin.WriteLog($"Upgraded wiki {name} - {title}{companyMsg}");
        }

        private bool SetActive(string title, string dashDescription)
        {
            var row = _wikiGraph.Wikis.Current;

            if (row.IsActive.GetValueOrDefault())
            {
                return false;
            }

            row.IsActive = true;
            row.WikiTitle = title;
            row.WikiDescription = dashDescription;
            _wikiGraph.Wikis.Update(row);
            return true;
        }

        private bool SetDefaultArticle()
        {
            var row = _wikiGraph.Wikis.Current;

            if (row.DefaultUrl != null)
            {
                return false;
            }

            row.DefaultUrl = @"8c3533cc-6b98-494d-91eb-6e3d3891b7fe";
            _wikiGraph.Wikis.Update(row);
            return true;
        }

        private bool SetSection()
        {
            var row = _wikiGraph.Wikis.Current;

            if (row.DefaultUrl != null)
            {
                return false;
            }

            row.DefaultUrl = @"8c3533cc-6b98-494d-91eb-6e3d3891b7fe";
            _wikiGraph.Wikis.Update(row);
            return true;
        }
    }
}
