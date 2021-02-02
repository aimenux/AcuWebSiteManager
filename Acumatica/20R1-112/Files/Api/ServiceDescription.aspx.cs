using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI.WebControls;
using PX.Api;
using PX.Api.Soap.Screen;
using PX.Data;
using PX.Export.Excel;
using System.Web.Compilation;
using System.Web.Security;
using SiteMap=PX.SM.SiteMap;
public partial class Api_ServiceDescription : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{

		if(Request.QueryString.ToString().StartsWith("WSDL"))
		{
			Response.ContentType = "text/xml";
			GetWsdl(Response.Output);
			Response.End();

			
		}

		string content = Request["content"];
		if(content == "reference")
			RenderSiteMap();
	
		if(content == "row")
			RenderRow(Request["id"]);
	
		if(content == "graph")
			RenderGraph(Request["id"]);
	

	}

	void GetWsdl(TextWriter w)
	{
		Uri u = new Uri(this.Request.Url, ResolveUrl("~/Api/ServiceGate.asmx"));

		int? maxItems = null;
		string mx = Request["M"];
		if (!string.IsNullOrEmpty(mx))
			maxItems = 10;

		WsdlSchemaBuilder.WriteWsdl(w, u, maxItems, typeof(ServiceGate));
	}

	private static IEnumerable<T> Select<T>(System.Collections.IEnumerable coll)
	{
		foreach (object item in coll)
			yield return (T) item;
	}

	private static IList<string> ToLookup(IEnumerable<SiteMap> coll)
	{
		IList<string> res = new List<string>();
		foreach (SiteMap item in coll)
			res.Add(item.Graphtype);
		return res;
	}

	private static IEnumerable<SiteMap> GetHiddenScreens(IList<string> exludeList)
	{
		foreach (Type graph in ServiceManager.AllGraphsTypesNotCustomized)
			if (!exludeList.Contains(graph.FullName))
			{
				SiteMap screen = new SiteMap();
				screen.ScreenID = "[none]";
				yield return screen;
			}
	}

	private static IEnumerable<SiteMap> Concat(IEnumerable<SiteMap> first, IEnumerable<SiteMap> second)
	{
		foreach (SiteMap item in first)
			yield return item;
		foreach (SiteMap item in second)
			yield return item;
	}

	void RenderSiteMap()
	{
		
		DivGraphs.Visible = true;
		IEnumerable<SiteMap> siteMap = Select<SiteMap>(PXSelectOrderBy<SiteMap, OrderBy<Asc<SiteMap.screenID>>>.Select(new PXGraph()));

		IList<string> lookup = ToLookup(siteMap);
		IEnumerable<SiteMap> hidden = GetHiddenScreens(lookup);
		
		HtmlTableBuilder b = new HtmlTableBuilder();
		b.Table = TableGraphs;

		foreach (SiteMap rowSiteMap in Concat(siteMap, hidden))
		{
			b.AddRow();

			b.AddCellHref(rowSiteMap.ScreenID, rowSiteMap.Url);
			b.AddCell(rowSiteMap.Graphtype);

			bool isTypeFound = false;
			if(!String.IsNullOrEmpty(rowSiteMap.Graphtype))
			{
				Type t = PXBuildManager.GetType(rowSiteMap.Graphtype, false);
				if(t != null)
				{
					try
					{
						string apiGraphName = ServiceManager.GetGraphNameFromType(t);
						isTypeFound = true;

						try
						{
							//WsdlSchemaBuilder.CreateGraph(t);

							//Activator.CreateInstance(t);
							string graphUrl = GetUrl("graph", apiGraphName);
							b.AddCellHref("Graph" + apiGraphName, graphUrl);


						}
						catch 
						{
							b.AddCell("Graph" + apiGraphName + " - login?");
							
						}
					}
					catch (ArgumentException){}
					catch (KeyNotFoundException) { }

				

				}
			}
			if(!isTypeFound)
			{
				b.AddCell(null);
				b.Row.Style.Add("display", "none");
				

			}



			b.AddCell(rowSiteMap.Title);


		}


	}
	static string GetUrl(string content, string id)
	{

		return String.Format("~/Api/ServiceDescription.aspx?content={0}&id={1}", content, id);

	}

	void RenderRow(string id)
	{
		DivMembers.Visible = true;
		HeaderMembers.InnerText = String.Format("Row{0} members",  id);
		HtmlTableBuilder b = new HtmlTableBuilder();
		b.Table = TableMembers;


		Type table = ServiceManager.GetTableType(id);
		PXGraph g = new PXGraph();
		PXCache cache = g.Caches[table];
		bool isOdd = false;
		foreach (string field in cache.Fields)
		{
			ApiFieldInfo f = ApiFieldInfo.Create(cache, field);
			if (f == null)
			{
				continue;
			}




			b.AddRow();
			isOdd = !isOdd;
			if(isOdd)
				b.Row.Style.Add("background-color", "white");
			b.AddCell(field);

			string descr = "";

			PXFieldState state = (PXFieldState)cache.GetStateExt(null, field);
			if (state != null)
			{
				StringBuilder doc = new StringBuilder();
				if (state.PrimaryKey) doc.Append("PrimaryKey ");
				if (state.PrimaryKey) b.Cell.Style.Add("font-weight", "bold");
				if (state.Required == true) doc.Append("Required ");
				if (!state.Enabled) doc.Append("NotEnabled ");
				if (!state.Enabled) b.Cell.Style.Add("font-style", "italic");
				if (state.IsReadOnly) doc.Append("IsReadOnly ");
				//if (f.Visible) doc.Append("Visible ");
				doc.Append(f.Visibility + " ");
				descr = doc.ToString();


			}
			b.AddCell(f.DataType.Name);

			b.AddCell(descr);
			b.AddCell(	state == null? "" :state.DisplayName );

			b.AddCell(Customization.RuntimeUtils.GetDacFieldAttributes(table, field));





		}




	}

	void RenderGraph(string id)
	{


		DivGraph.Visible = true;
		HeaderGraph.InnerText = "Graph" + id + " members";
		HtmlTableBuilder viewsBuilder = new HtmlTableBuilder();
		viewsBuilder.Table = TableGraphViews;


		Type t = ServiceManager.GetGraphType(id);
		PXGraph g;
		try
		{
			g = (PXGraph)PXGraph.CreateInstance(t);

		}
		catch 
		{

			if (this.Context.User.Identity.IsAuthenticated) 
				throw;

			PX.Export.Authentication.FormsAuthenticationModule.RedirectToLoginPage();			
			return;
		}

		#region views
		MemberInfo[] views = ServiceManager.GetViews(t);
		foreach (FieldInfo view in views)
		{
			viewsBuilder.AddRow();
			viewsBuilder.AddCell("View" + view.Name);

			PXView v = g.Views[view.Name];
			Type[] items = v.GetItemTypes();

			viewsBuilder.AddCell(null);

			foreach (Type row in items)
			{
				try
				{
					string apiName = ServiceManager.GetTableNameFromType(row);
					HyperLink h = new HyperLink();
					h.NavigateUrl = GetUrl("row", apiName);
					h.Text = "[" + apiName + "]";
					viewsBuilder.Cell.Controls.Add(h);
					continue;
				}
				catch
				{

				}

				HyperLink hyperLink = new HyperLink();
				hyperLink.Text = row.Name;
				viewsBuilder.Cell.Controls.Add(hyperLink);




			}
			string strParams = "";
			List<PXViewParameter> pinfo = v.EnumParameters();

			List<string> list = new List<string>();
			foreach (PXViewParameter p in pinfo)
			{
				string opt = "";
					
				string type = GetParamTypeName(g, p);
				if(p.Bql != null && p.Bql.HasDefault)
					opt = "opt ";


				list.Add(String.Format("{0}{1} {2}", opt, type, p.Name));
			}
			//var parameters = v.GetParameterNames();


			if (pinfo.Count > 0)
			{
				strParams = "(" + String.Join(", ", list.ToArray()) + ")";

			}

			viewsBuilder.AddCell(strParams);





		}
		#endregion

		#region actions
		HtmlTableBuilder actionsBuilder = new HtmlTableBuilder();
		actionsBuilder.Table = TableGraphActions;

		string[] actions = ServiceManager.GetActions(t);
		foreach (string action in actions)
		{
			actionsBuilder.AddRow();
			actionsBuilder.AddCell("Action" + action);
			PXAction a = g.Actions[action];
			Type r = a.GetRowType();
			string table = ServiceManager.GetTableNameFromType(r);

			actionsBuilder.AddCell(table);



		}

		#endregion


	}

	private static string GetParamTypeName(PXGraph g, PXViewParameter p)
	{
		string type = "";

		if(p.Bql != null)
		{
			Type rt = p.Bql.GetReferencedType();
			Type cacheType = BqlCommand.GetItemType(rt);
			if(cacheType != null)
			{
			
				PXFieldState state = (PXFieldState)g.Caches[cacheType].GetStateExt(null, rt.Name);
				if(state != null)
					return state.DataType.Name;

			}

		}
		
		if (p.Argument != null)
		{
			Type pt = p.Argument.ParameterType;
			if (pt.IsGenericType)
			{
				Type gd = pt.GetGenericTypeDefinition();
				if(gd == typeof(Nullable<>))
				{

					type = Nullable.GetUnderlyingType(pt).Name;
				}
				else
				{

					Type[] ga = pt.GetGenericArguments();
					List<string> args = new List<string>();
					foreach (Type a in ga)
						args.Add(a.Name);


					type = gd.Name + "<" + String.Join(",", args.ToArray()) + ">";

				}

			}
			else
			{
				type = pt.Name;
			}
			//pt = Nullable.GetUnderlyingType(pt);
			//type = pt.ToString();

		}
		return type;
	}


	
}

