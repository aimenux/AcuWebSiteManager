using PX.CloudServices.DocumentRecognition.InvoiceRecognition;
using PX.Common;
using PX.Data;
using PX.Objects.AP.InvoiceRecognition;
using PX.Web.UI;
using System;
using System.Threading.Tasks;
using System.Web;

public partial class Page_AP301100 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		SetRecognitionInfoToGraph();
	}

	private void SetRecognitionInfoToGraph()
	{
		if (IsCallback)
		{
			return;
		}

		var graph = ds.DataGraph as APInvoiceRecognitionEntry;
		var isInfoSet = graph.Document.Current != null && graph.Document.Current.RecognitionResult != null;
		if (isInfoSet)
		{
			return;
		}

		SetRecognitionResult(graph);
	}

	private void SetRecognitionResult(APInvoiceRecognitionEntry graph)
	{
		var fileIdEncoded = Request.QueryString[APInvoiceRecognitionEntry.FileIdParameter];
		if (fileIdEncoded == null)
		{
			return;
		}

		var fileIdString = HttpUtility.UrlDecode(fileIdEncoded);
		Guid fileId;
		if (!Guid.TryParse(fileIdString, out fileId))
		{
			return;
		}

		var file = graph.GetSystemFile(fileId);
		if (file == null)
		{
			return;
		}

		var fileInfoInMemory = new PX.SM.FileInfo(fileId, file.Name, null, file.Data);
		PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[fileInfoInMemory.UID.ToString()] = fileInfoInMemory;

		var uriEncoded = Request.QueryString[APInvoiceRecognitionEntry.RecognitionUriParameter];
		if (uriEncoded == null)
		{
			return;
		}
        
		var uriString = HttpUtility.UrlDecode(uriEncoded);
				var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);

		var recognitionTask = Task.Run(() => graph.InvoiceRecognitionClient.GetRecognitionResult(uri));
		recognitionTask.Wait();

		graph.StoreRecognitionResultIntoSession(recognitionTask.Result, fileIdString);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (IsCallback)
		{
			return;
		}

		RegisterRecognizedDataJavaScriptVariable();

		var form = (PXFormView)PXSplitContainer1.FindControl("edDocument");
		var documentBoundingInfo = (PXTextEdit)form.FindControl("edDocumentBoundingInfo");
		Page.ClientScript.RegisterClientScriptBlock(GetType(), "documentBoundingInfoId", string.Format("var documentBoundingInfoId = '{0}';", documentBoundingInfo.ClientID), true);
	}

	private void RegisterRecognizedDataJavaScriptVariable()
	{
		var graph = ds.DataGraph as APInvoiceRecognitionEntry;
		var recognizedData = graph.GetRecognitionResultJsonFromSession();
		var recognizedDataJson = recognizedData ?? "null";

		Page.ClientScript.RegisterClientScriptBlock(GetType(), "recognizedDataJson", string.Format("var recognizedData = {0};", recognizedDataJson), true);
	}
}
