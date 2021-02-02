using PX.Common;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
/// <summary>
/// Provides client script configuration
/// </summary>
public class ClientSideAppsHelper
{
	[PXInternalUseOnly]
	[Obsolete]
	public static string RenderScriptConfiguration()
	{

		var localizedNoResults = PX.Data.PXMessages.LocalizeNoPrefix(PX.Web.UI.Msg.SuggesterNothingFound);
		var cacheKey = "ClientSideAppsConfig:" + localizedNoResults;
		string cachedResult = HttpContext.Current.Cache[cacheKey] as string;
		if (cachedResult != null)
		{
			return cachedResult;
		}

		const string clientSideAppsRoot = "/scripts/ca/";
		var root = HttpContext.Current.Request.ApplicationPath;

		if (root.Equals("/"))
		{
			root = "";
		}
		root = root + clientSideAppsRoot;

		var scriptDir = System.Web.HttpContext.Current.Server.MapPath("~" + clientSideAppsRoot);
		var bundleFiles = Directory.GetFiles(scriptDir, "*-bundle.js", SearchOption.AllDirectories);

		var vendorBundleTicks = File.GetLastWriteTime(Path.Combine(scriptDir, "vendor-bundle.js")).Ticks;

		var latestBundleTicks = bundleFiles.Select(f => File.GetLastWriteTime(f).Ticks).Max();

		var bundles = bundleFiles
			.Select(x => x.Replace(scriptDir, "").Replace("\\", "/"))
			.ToArray()
			;

		var apps = bundles.Where(b => b.Contains("app-")).ToList();
		var resources = bundles.Where(b => b.Contains("resources-")).ToArray();
		var controls = bundles.Where(b => b.Contains("controls-")).ToArray();
		var controlBundles = controls.Select(a =>
		{
			var bundleName = a.Replace(".js", "");
			var moduleName = bundleName.Replace("controls-bundle", "index");
			return string.Format(@"""{0}"":[""{1}""]", bundleName, moduleName);
		}).ToArray();



		var sb = new StringBuilder();
		sb.Append(@"
<script>
window.ClientLocalizedStrings = {
");

		sb.AppendFormat("noResultsFound: '{0}',\n", HttpUtility.HtmlDecode(localizedNoResults));
		sb.AppendLine("lastUpdate: {");
		sb.AppendFormat("JustNow: '{0}',\n", HttpUtility.HtmlDecode(PX.Data.PXMessages.LocalizeNoPrefix(PX.Web.UI.Msg.LastUpdateJustNow)));
		sb.AppendFormat("MinsAgo: '{0}',\n", HttpUtility.HtmlDecode(PX.Data.PXMessages.LocalizeNoPrefix(PX.Web.UI.Msg.LastUpdateMinsAgo)));
		sb.AppendFormat("HoursAgo: '{0}',\n", HttpUtility.HtmlDecode(PX.Data.PXMessages.LocalizeNoPrefix(PX.Web.UI.Msg.LastUpdateHoursAgo)));
		sb.AppendFormat("DaysAgo: '{0}',\n", HttpUtility.HtmlDecode(PX.Data.PXMessages.LocalizeNoPrefix(PX.Web.UI.Msg.LastUpdateDaysAgo)));
		sb.AppendFormat("LongAgo: '{0}'\n", HttpUtility.HtmlDecode(PX.Data.PXMessages.LocalizeNoPrefix(PX.Web.UI.Msg.LastUpdateLongAgo)));

		sb.AppendLine("}}");
	



		sb.AppendFormat(@"
window.globalControlsModules=[""{0}""];
", String.Join("\",\"", controls.Select(x => x.Replace(".js", "").Replace("/controls-bundle", ""))));


		var bundleArray = apps.Select(a =>
		{
			var bundleName = a.Replace(".js", "");
			var moduleName = bundleName.Replace("app-bundle", "main");
			return string.Format(@"""{0}"":[""{1}""]", bundleName, moduleName);
		}).Union(controlBundles)

		.ToArray();


		sb.AppendFormat(@"
requirejs = {{
	baseUrl: '{0}',
	paths: {{
		root: """"
	}},
	waitSeconds: 30,
	urlArgs: ""b={2}"",
	packages: [],
	stubModules: [
	'text'
	],
	shim: {{}},
	bundles: {{{1}}}
}}
</script>", root, string.Join(",\n", bundleArray), latestBundleTicks);

		sb.AppendFormat(@"<script src=""{0}vendor-bundle.js?b={1}"" data-main=""apps/enhance/main"" defer></script>", root, vendorBundleTicks);

		sb.AppendFormat(@"<!--{0}-->", System.DateTime.UtcNow);

		var result = sb.ToString();

		HttpContext.Current.Cache.Insert(cacheKey, result, new System.Web.Caching.CacheDependency(bundleFiles), System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration);


		return result;
	}
}