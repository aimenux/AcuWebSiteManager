<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP301100.aspx.cs" Inherits="Page_AP301100" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <link rel="stylesheet" type="text/css" href='<%=ResolveClientUrl("~/Scripts/documentRecognition/pdfViewer/pdfViewer.css")%>' />
    <link rel="stylesheet" type="text/css" href='<%=ResolveClientUrl("~/Scripts/documentRecognition/recognition/recognition.css")%>' />
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/pdf.js/pdf.min.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/pdfViewer/pdfViewer.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/recognition/recognizedRectangle.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/recognition/recognizedValue.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/recognition/recognizedColumn.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/recognition/recognizedTable.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/recognition/recognizedValueScroller.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/recognition/recognizedValueMapper.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/documentRecognition/recognition/feedbackCollector.js")%>' type="text/javascript"></script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.InvoiceRecognition.APInvoiceRecognitionEntry" PrimaryView="Document">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" Visible="true" />
            <px:PXDSCallbackCommand Name="Cancel" Visible="true" />
            <px:PXDSCallbackCommand Name="ContinueSave" Visible="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Delete" Visible="true" />

            <px:PXDSCallbackCommand Name="Save" Visible="false" />
            <px:PXDSCallbackCommand Name="SaveClose" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
            <px:PXDSCallbackCommand Name="First" Visible="false" />
            <px:PXDSCallbackCommand Name="Previous" Visible="false" />
            <px:PXDSCallbackCommand Name="Next" Visible="false" />
            <px:PXDSCallbackCommand Name="Last" Visible="false" />

            <px:PXDSCallbackCommand Name="DumpTableFeedback" Visible="false" />
		</CallbackCommands>
        <ClientEvents Initialize="captureDatasource" CommandPerformed="commandPerformed" />
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXSplitContainer runat="server" ID="PXSplitContainer1" SplitterPosition="480">
        <Template1>
            <px:PXFormView ID="edDocument" runat="server" DataSourceID="ds" Width="100%" DataMember="Document" Height="100%" FilesIndicator="True">
                <Template>
                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXTextEdit ID="RecognizedRecordRefNbr" runat="server" DataField="RecognizedRecordRefNbr" IsClientControl="False" />
                    <px:PXDropDown ID="RecognitionStatus" runat="server" DataField="RecognitionStatus" IsClientControl="False" />
                    <px:PXDropDown ID="DocType" runat="server" DataField="DocType" IsClientControl="False" CommitChanges="true" />
                    <px:PXSegmentMask CommitChanges="True" ID="VendorID" runat="server" DataField="VendorID" AllowAddNew="True" AllowEdit="True" DataSourceID="ds" AutoRefresh="True" IsClientControl="False" />
                    <px:PXSegmentMask CommitChanges="True" ID="VendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" DataSourceID="ds" IsClientControl="False" />
                    <pxa:PXCurrencyRate ID="CuryID" DataField="CuryID" runat="server" RateTypeView="_APRecognizedInvoice_CurrencyInfo_" DataMember="_Currency_" DataSourceID="ds"/>
                    <px:PXSelector CommitChanges="True" ID="TermsID" runat="server" DataField="TermsID" DataSourceID="ds" AutoRefresh="true" IsClientControl="False" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="DocDate" runat="server" DataField="DocDate" Size="XM" IsClientControl="False" />
                    <px:PXSelector CommitChanges="True" ID="FinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="True" DataSourceID="ds" IsClientControl="False" />
                    <px:PXTextEdit ID="InvoiceNbr" runat="server" DataField="InvoiceNbr" IsClientControl="False" />
                    <px:PXTextEdit ID="DocDesc" runat="server" DataField="DocDesc" IsClientControl="False" />
                    <px:PXNumberEdit ID="CuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" Size="XM" IsClientControl="False" />
                    <px:PXNumberEdit ID="CuryOrigDocAmt" runat="server" CommitChanges="True" DataField="CuryOrigDocAmt" Size="XM" IsClientControl="False" />

                    <px:PXLayoutRule runat="server" SuppressLabel="true" />
                    <px:PXTextEdit ID="AllowFiles" runat="server" CommitChanges="true" DataField="AllowFiles" IsClientControl="False" Enabled="false" Style="display: none;" />
                    <px:PXTextEdit ID="AllowFilesMsg" runat="server" CommitChanges="true" DataField="AllowFilesMsg" IsClientControl="False" Enabled="false" Style="display: none;" />
                    <px:PXCheckBox ID="AllowUploadFile" runat="server" CommitChanges="true" DataField="AllowUploadFile" IsClientControl="False" Enabled="false" Style="display: none;" />
                    <px:PXTextEdit ID="FileID" runat="server" CommitChanges="true" DataField="FileID" IsClientControl="False" Enabled="false" Style="display: none;" />
                    <px:PXTextEdit ID="RecognizedDataJson" runat="server" CommitChanges="true" DataField="RecognizedDataJson" IsClientControl="False" Enabled="false" Style="display: none;" />
                    <px:PXTextEdit ID="VendorTermJson" runat="server" CommitChanges="true" DataField="VendorTermJson" IsClientControl="False" Enabled="false" Style="display: none;" />
                    <px:PXFormView ID="edRecognizedRecord" runat="server" DataSourceID="ds"  Width="100%" DataMember="RecognizedRecords" Height="100%">
                        <Template>
                            <px:PXTextEdit ID="hiddenRefNbr" runat="server" DataField="RefNbr" IsClientControl="False" />
                        </Template>
                    </px:PXFormView>
                </Template>
                <ClientEvents Initialize="capturePrimaryForm" AfterRepaint="renderPdf" />
            </px:PXFormView>
        </Template1>
        <Template2>
            <px:PXSplitContainer runat="server" ID="PXSplitContainer2" Orientation="Horizontal" SplitterPosition="582" SkinID="Horizontal" Panel1MinSize="582" Panel2MinSize="0">
                <Template1>
                    <div id="dragNdropContainer" style="width: 96%; border: 1px dashed; height: 92%; margin: 2%; display: flex; align-items: center; justify-content: center;">
                        <img src='<%=ResolveClientUrl("~/Content/svg_icons/dragNdrop.svg")%>' style="transform: scale(2.0);" />
                    </div>
                    <div id="pdfRecognitionViewerContainer" style="width: 100%; height: 100%; overflow: auto;">
                    </div>
                    <px:PXLabel runat="server" ID="toShowSplitter2" Text=" " />
                </Template1>
                <Template2>
                </Template2>
                <AutoSize Enabled="true" Container="Window" />
            </px:PXSplitContainer>
        </Template2>
        <AutoSize Enabled="true" Container="Window" />
    </px:PXSplitContainer>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="edItems" runat="server" DataSourceID="ds" Width="100%" Height="100%" NoteIndicator="false" FilesIndicator="false" AdjustPageSize="Auto" SkinID="Details" AutoAdjustColumns="true" AllowPaging="false">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" LinkCommand="ViewItem" />
                    <px:PXGridColumn DataField="TranDesc" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" AutoCallBack="True" />
                    <px:PXGridColumn DataField="UOM" AutoCallBack="True" />
                    <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" AutoCallBack="True" CommitChanges="true" />
                    <px:PXGridColumn DataField="CuryLineAmt" TextAlign="Right" AutoCallBack="True" CommitChanges="true" />
                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="true" MinHeight="230" />
        <ClientEvents Initialize="captureDetailsGrid" />
    </px:PXGrid>
    <px:PXSmartPanel ID="edFeedbackPanel" runat="server" Key="BoundFeedback" Width="500px" Height="135px"
        Caption="Bound Feedback" CaptionVisible="true">
        <px:PXFormView ID="edFeedback" runat="server" DataSourceID="ds" Width="100%" DataMember="BoundFeedback" CaptionVisible="False">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXTextEdit ID="edFieldBound" runat="server" CommitChanges="true" DataField="FieldBound" IsClientControl="False" Enabled="false" />
                <px:PXTextEdit ID="edTableRelated" runat="server" CommitChanges="true" DataField="TableRelated" IsClientControl="False" Enabled="false" />
            </Template>
            <AutoSize Enabled="true" />
        </px:PXFormView>
    </px:PXSmartPanel>
    <script type="text/javascript">
        let datasource = null;
        let detailsGrid = null;
        let primaryForm = null;
        let viewBar = null;
        let pdfRecognitionViewer = null;
        let callbackManager = null;

        let isPdfRendered = false;
        let isDataProcessed = false;
        let prevFilesCount = null;

        function captureDatasource(ds) {
            datasource = ds;
        }

        function capturePrimaryForm(form) {
            hideSynchronizationControls();

            primaryForm = form;

            subscribeOnViewBarEvents(form);
        }

        function findViewBar(form) {
            if (viewBar) {
                return;
            }

            const all = __px_all(form);

            for (var c in all) {
                if (all[c].__className !== "PXDataViewBar") {
                    continue;
                }

                viewBar = all[c];
                break;
            }

            callbackManager = __px_callback(viewBar);
            if (!callbackManager) {
                return;
            }

            callbackManager.addHandler(function (context) {
                if (context.controlID !== viewBar.ID) {
                    return;
                }

                if (context.info.name !== 'FilesMenuShow') {
                    return;
                }

                let fileUploader = px_all[viewBar.ID + "_fb_upld2"];
                if (fileUploader) {
                    let allowedFilesControl = px_all[allowFilesId];
                    if (allowedFilesControl) {
                        fileUploader.allowedFiles = [allowedFilesControl.getValue()];
                    }
                }
            });
        }

        function subscribeOnViewBarEvents(form) {
            findViewBar(form);

            if (!viewBar) {
                return;
            }

            const prevFilesStateCallback = viewBar.filesStateCallback;
            let refreshing = false;

            // Render file after uploading
            viewBar.filesStateCallback = function (filesCount) {
                if (prevFilesStateCallback) {
                    prevFilesStateCallback(filesCount);
                }

                let allowedFilesControl = px_all[allowFilesId];
                if (allowedFilesControl) {
                    primaryForm.allowedFiles = allowedFilesControl.getValue();
                }

                let allowedFilesMsgControl = px_all[allowFilesMsgId];
                if (allowedFilesMsgControl) {
                    primaryForm.allowedFilesMsg = allowedFilesMsgControl.getValue();
                }

                if (!filesCount && prevFilesCount) {
                    reset();
                }

                if (!filesCount || prevFilesCount === filesCount) {
                    return;
                }

                let allowUploadFileControl = px_all[allowUploadFileId];
                if (allowUploadFileControl) {
                    let allowUpload = allowUploadFileControl.getValue();

                    setEnabledFileAttach(allowUpload);
                }

                if (refreshing === true) {
                    return;
                }

                refreshing = true;

                setTimeout(function () {
                    form.refresh();
                    prevFilesCount = filesCount;
                    refreshing = false;
                }, 100);
            }
        }

        function setEnabledFileAttach(enabled) {
            let filesMenuBtn = null;

            for (var i = 0; i < viewBar.items.length; i++) {
                if (viewBar.items[i].commandName == "FilesMenuShow") {
                    filesMenuBtn = viewBar.items[i];
                    break;
                }
            }

            if (filesMenuBtn) {
                filesMenuBtn.setEnabled(enabled);
            }

            primaryForm.canAddFiles = enabled == true ? 2 : 0;
        }

        function captureDetailsGrid(grid) {
            detailsGrid = grid;
        }

        function commandPerformed(ds, e) {
            // Reset file
            if (e.command.toLowerCase() === 'insert' ||
                e.command.toLowerCase() === 'delete' ||
                e.command.toLowerCase() === 'save') {
                reset();

                return;
            }

            // Reset boxes
            if (e.command.toLowerCase() === 'cancel') {
                renderBoundingBoxes(true);

                return;
            }

            if (e.command.toLowerCase() === 'processrecognition') {
                isDataProcessed = false;

                const enableFile = ds.longRunMessage && ds.longRunCompleted === null && ds.longRunInProcess === null;
                setEnabledFileAttach(enableFile);

                return;
            }

            hideSynchronizationControls();

            if (ds.longRunCompleted === null && ds.longRunAborted === null) {
                return;
            }

            if (isDataProcessed === true) {
                return;
            }

            isDataProcessed = true;

            setEnabledFileAttach(false);
            renderBoundingBoxes(false);
        }

        function renderBoundingBoxes(isReloading) {
            const recognizedDataControl = px_all[recognizedDataId];
            if (!recognizedDataControl) {
                return;
            }

            let recognizedDataEncodedJson = recognizedDataControl.getValue();
            if (!recognizedDataEncodedJson) {
                return;
            }

            recognizedDataEncodedJson = recognizedDataEncodedJson.replace(/\+/g, '%20');

            const recognizedDataJson = decodeURIComponent(recognizedDataEncodedJson);
            if (!recognizedDataJson) {
                return;
            }

            let vendorTermData = null;
            const vendorTermControl = px_all[vendorTermId];
            if (vendorTermControl) {
                let vendorTermEncodedJson = vendorTermControl.getValue();

                if (vendorTermEncodedJson) {
                    vendorTermEncodedJson = vendorTermEncodedJson.replace(/\+/g, '%20');
                    const vendorTermJson = decodeURIComponent(vendorTermEncodedJson);

                    if (vendorTermJson) {
                        vendorTermData = JSON.parse(vendorTermJson);
                    }
                }
            }

            const recognizedData = JSON.parse(recognizedDataJson);
            pdfRecognitionViewer.processRecognizedData(recognizedData, vendorTermData, isReloading);
        }

        function hideSynchronizationControls() {
            const recognizedDataElement = document.getElementById(recognizedDataId);
            if (recognizedDataElement) {
                recognizedDataElement.style.display = 'none';
            }

            const vendorTermElement = document.getElementById(vendorTermId);
            if (vendorTermElement) {
                vendorTermElement.style.display = 'none';
            }

            const feedbackElement = document.getElementById(fieldBoundFeedbackId);
            if (feedbackElement) {
                feedbackElement.style.display = 'none';
            }
        }

        function renderPdf() {
            if (isPdfRendered === true) {
                return;
            }

            let fieldBoundFeedbackControl = px_all[fieldBoundFeedbackId];
            if (!fieldBoundFeedbackControl) {
                return;
            }

            const tableRelatedFeedbackControl = px_all[tableRelatedFeedbackId];
            if (!tableRelatedFeedbackControl) {
                return;
            }

            let fileId = null;

            let fileIdControl = px_all[fileIDControlID];
            if (fileIdControl) {
                fileId = fileIdControl.getValue();
            }

            if (!fileId) {
                return;
            }

            let pdfUrl = '../../Frames/GetFile.ashx?inmemory=1&fileID=' + fileId;

            // Loaded via <script> tag, create shortcut to access PDF.js exports.
            let pdfjsLib = window['pdfjs-dist/build/pdf'];

            // The workerSrc property shall be specified.
            pdfjsLib.GlobalWorkerOptions.workerSrc = '../../Scripts/documentRecognition/pdf.js/pdf.worker.min.js';

            enableDragNDropContainer(false);

            let pdfRecognitionContainer = getPdfRecognitionContainer();
            const dumpTableFeedbackCallback = function () {
                datasource.executeCommand('DumpTableFeedback');
            };
            pdfRecognitionViewer = new PdfRecognitionViewer(pdfUrl, pdfjsLib, pdfRecognitionContainer,
                fieldBoundFeedbackControl, tableRelatedFeedbackControl, dumpTableFeedbackCallback);

            pdfRecognitionViewer.trackFormControls(primaryForm);
            pdfRecognitionViewer.trackGridControls(detailsGrid);

            const callback = function () {
                if (isDataProcessed === true) {
                    return;
                }

                isDataProcessed = true;
                renderBoundingBoxes(false);
            }

            pdfRecognitionViewer.renderPdf(callback);

            isPdfRendered = true;
        }

        function getPdfRecognitionContainer() {
            return document.getElementById('pdfRecognitionViewerContainer');
        }

        function reset() {
            pdfRecognitionViewer = null;
            isPdfRendered = false;
            isDataProcessed = false;
            prevFilesCount = null;

            setEnabledFileAttach(true);

            let pdfRecognitionContainer = getPdfRecognitionContainer();
            while (pdfRecognitionContainer.firstChild) {
                pdfRecognitionContainer.removeChild(pdfRecognitionContainer.firstChild);
            }

            enableDragNDropContainer(true);
        }

        function enableDragNDropContainer(enable) {
            const container = document.getElementById('dragNdropContainer');
            if (!container) {
                return;
            }

            container.style.display = enable ? 'flex' : 'none';
        }
    </script>
</asp:Content>
