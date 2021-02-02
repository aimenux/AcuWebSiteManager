using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SP.DAC;
using PX.SM;
using PX.Web.UI;

public partial class Page_SP700006 : PXPage
{
    private PXImage PXImage1;
    private PXButton Next;
    private PXButton Prev;
    private string inventoryID;
    private string picturenumber;
    private int picturenumberint;
    private int picturenumberinttotal;

    protected void Page_Load(object sender, EventArgs e)
    {
        this.Master.PopupWidth = 700;
        this.Master.PopupHeight = 420;
        this.Master.FindControl("usrCaption").Visible = false;
        PXImageUploader imageuploader = this.form.FindControl("imgUploader") as PXImageUploader;
        if (imageuploader != null)
        {
            var setting = PortalSetup.Current;
            if (!String.IsNullOrEmpty(setting.ImageUrl))
                imageuploader.NoImageUrl = ControlHelper.GetAttachedFileUrlByName(setting.ImageUrl);
        }
    }

    /*[Serializable]
    public class InventoryLineGraph : PXGraph<InventoryLineGraph>
    {
        private PXSelect<InventoryItem, Where<InventoryItem.inventoryID,
            Equal<Required<InventoryItem.inventoryID>>>> Inventory;
    }

    protected void Page_Init(object sender, EventArgs e)
    {
        PXImage1 = form.FindControl("PXImage1") as PXImage;
        Prev = form.FindControl("edPrev") as PXButton;
        Next = form.FindControl("edNext") as PXButton;
        picturenumber = Request.QueryString["picturenumber"];
        inventoryID = Request.QueryString["inventoryID"];

        if (String.IsNullOrEmpty(picturenumber)) 
            picturenumber = "0";

        Int32.TryParse(picturenumber, out picturenumberint);
        InventoryLineGraph _inventoryLineGraph = new InventoryLineGraph();

        InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryID,
            Equal<Required<InventoryItem.inventoryID>>>>.SelectSingleBound(new PXGraph(), null, inventoryID);
        if (inventory != null)
        {
            Guid[] files = PXNoteAttribute.GetFileNotes(_inventoryLineGraph.Caches[typeof (InventoryItem)], inventory);
            picturenumberinttotal = files.Length;
            PXImage1.ImageUrl = ResolveUrl("~/Frames/GetFile.ashx") + "?fileID=" + files[picturenumberint];
        }

        Prev.Visible = !((picturenumberint + 1) == 1);
        Next.Visible = !((picturenumberint + 1) == picturenumberinttotal);
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        this.Master.PopupWidth = 400;
        this.Master.PopupHeight = 400;
        this.Master.FindControl("usrCaption").Visible = false;
    }

    protected void PrevMonth_Click(object sender, PXCallBackEventArgs e)
    {
        picturenumberint--;
        string path = PXUrl.SiteUrlWithPath();
        path += path.EndsWith("/") ? "" : "/";
        var url = string.Format("{0}Pages/SP/SP700006.aspx?inventoryID={1}&picturenumber={2}", path, inventoryID,
            picturenumberint);
        
        throw new PXRedirectToUrlException(url, PXBaseRedirectException.WindowMode.Same, "");
    }

    protected void NextMonth_Click(object sender, PXCallBackEventArgs e)
    {
        picturenumberint++;
        string path = PXUrl.SiteUrlWithPath();
        path += path.EndsWith("/") ? "" : "/";
        var url = string.Format("{0}Pages/SP/SP700006.aspx?inventoryID={1}&picturenumber={2}", path, inventoryID,
            picturenumberint);

        throw new PXRedirectToUrlException(url, PXBaseRedirectException.WindowMode.Same, "");
    }*/
}

