using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Common;
using PX.SM;
using PX.Data;
using PX.Web.UI;
using PX.Data.Wiki.Parser;

public partial class Page_ArticleEdit : EditPage<KBArticleMaint>
{
	protected void Page_PreInit(object sender, EventArgs e)
	{
		this.Master.FavoriteAvailable = false;
		this.Master.CustomizationAvailable = false;
	}

	protected override void Page_Init(object sender, EventArgs e)
	{
		base.Page_Init(sender, e);
		((PXLabel)this.tab1.FindControl("lblTags")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.VersionTags);
		if (Master is Master_TabView) (Master as Master_TabView).BranchAvailable = false;
		// hide base save
		// PXToolBarItem actionsave = this.ds1.ToolBar.Items[0] as PXToolBarItem;

	}
	
	protected override PXDataSource DataSource
	{
		get { return this.ds1; }
	}

	protected override PXTab TabView
	{
		get { return this.tab1; }
	}

	protected override string ScreenTitle
	{
		get { return this.Master.ScreenTitle; }
		set { this.Master.ScreenTitle = value; }
	}

	public static Guid GetGuidParameterValue(string valueStr)
	{
		if (!string.IsNullOrEmpty(valueStr))
		{
			string[] valueStrArr = valueStr.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < valueStrArr.Length; i++)
			{
				Guid value;
				if (GUID.TryParse(valueStrArr[i], out value)) return value;
			}
		}
		return Guid.Empty;
	}
}
