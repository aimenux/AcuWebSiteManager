<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP301100.aspx.cs" Inherits="Page_AP301100" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <link rel="stylesheet" type="text/css" href='<%=ResolveClientUrl("~/Scripts/pdf.js/pdfRecognitionViewer.css")%>' />
    <script src='<%=ResolveClientUrl("~/Scripts/pdf.js/pdf.min.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/pdf.js/pdfRecognitionViewer.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/pdf.js/recognizedValue.js")%>' type="text/javascript"></script>
    <script src='<%=ResolveClientUrl("~/Scripts/pdf.js/recognitionCorrection.js")%>' type="text/javascript"></script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.InvoiceRecognition.APInvoiceRecognitionEntry" PrimaryView="Document">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" Visible="false" />
            <px:PXDSCallbackCommand Name="SaveContinue" Visible="true" />
            <px:PXDSCallbackCommand Name="SaveClose" Visible="false" />
            <px:PXDSCallbackCommand Name="Cancel" Visible="true" />
            <px:PXDSCallbackCommand Name="Insert" Visible="false" />
            <px:PXDSCallbackCommand Name="Delete" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
            <px:PXDSCallbackCommand Name="First" Visible="false" />
            <px:PXDSCallbackCommand Name="Previous" Visible="false" />
            <px:PXDSCallbackCommand Name="Next" Visible="false" />
            <px:PXDSCallbackCommand Name="Last" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXSplitContainer runat="server" ID="PXSplitContainer1" SplitterPosition="480">
        <Template1>
            <px:PXFormView ID="edDocument" runat="server" DataSourceID="ds" Width="100%" DataMember="Document" Height="100%">
                <Template>
                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowAddNew="True" AllowEdit="True" DataSourceID="ds" AutoRefresh="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" DataSourceID="ds" />
                    <pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" RateTypeView="_APRecognizedInvoice_CurrencyInfo_" DataMember="_Currency_" DataSourceID="ds"/>
                    <px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" DataSourceID="ds" AutoRefresh="true" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" Size="XM" />
                    <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="True" DataSourceID="ds" />
                    <px:PXTextEdit CommitChanges="True" ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />
                    <px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" DataSourceID="ds" />
                    <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
                    <px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" Size="XM" />
                    <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" Size="XM" />
                    <px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" Size="XM" />
                    <px:PXNumberEdit ID="edCuryOrigDocAmt" runat="server" CommitChanges="True" DataField="CuryOrigDocAmt" Size="XM" />

                    <px:PXLayoutRule runat="server" SuppressLabel="true" />
                    <px:PXTextEdit ID="edDocumentBoundingInfo" runat="server" CommitChanges="true" DataField="DocumentBoundingInfoJson" Style="display: none" />
                </Template>
                <ClientEvents Initialize="RegisterAfterLoad" />
            </px:PXFormView>
        </Template1>
        <Template2>
            <div id="pdfRecognitionViewerContainer" style="width: 100%; height: 100%" />
        </Template2>
        <AutoSize Enabled="true" Container="Window" />
    </px:PXSplitContainer>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="edItems" runat="server" DataSourceID="ds" Width="100%" Height="100%" NoteIndicator="false" FilesIndicator="false" AdjustPageSize="Auto" SkinID="Details" AutoAdjustColumns="true" AllowPaging="false">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn DataField="BranchID" AutoCallBack="True" />
                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" LinkCommand="ViewItem" />
                    <px:PXGridColumn DataField="TranDesc" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" AutoCallBack="True" />
                    <px:PXGridColumn DataField="UOM" AutoCallBack="True" />
                    <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" AutoCallBack="True" CommitChanges="true" />
                    <px:PXGridColumn DataField="CuryLineAmt" TextAlign="Right" AutoCallBack="True" CommitChanges="true" />
                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="TaxCategoryID" AutoCallBack="True" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="true" MinHeight="230" />
        <ClientEvents Initialize="CaptureDetailsGrid" />
    </px:PXGrid>
    <script type="text/javascript">
        let urlParams = new URLSearchParams(window.location.search);
        let fileId = urlParams.get('AttachmentFileId');

        let registered = false;
        let detailsGrid = null;

        function RegisterAfterLoad(form) {
            if (registered === true) {
                return;
            }

            registered = true;

            px_cm.registerAfterLoad(function () {
                form.onLoad();

                let feedbackControl = px_all[documentBoundingInfoId];
                if (!feedbackControl) {
                    return;
                }

                let feedbackElement = document.getElementById(documentBoundingInfoId);
                if (feedbackElement) {
                    feedbackElement.style.display = 'none';
                }

                AttachControlsToPdfViewer(form, feedbackControl);
            });
        }

        function AttachControlsToPdfViewer(form, feedbackControl) {
            let formMapping = {
                vendor: {
                    formControl: 'edVendorID',
                    recognitionField: 'Document.VendorID'
                },
                date: {
                    formControl: 'edDocDate',
                    recognitionField: 'Document.DocDate'
                },
                amount: {
                    formControl: 'edCuryOrigDocAmt',
                    recognitionField: 'Document.CuryOrigDocAmt'
                }
            };

            let gridMapping = {
                    inventory: {
                        gridField: 'InventoryID',
                        recognitionField: 'VendorItemID'
                    },
                    description: {
                        gridField: 'TranDesc',
                        recognitionField: 'Description'
                    },
                    uom: {
                        gridField: 'UOM',
                        recognitionField: 'UOM'
                    },
                    qty: {
                        gridField: 'Qty',
                        recognitionField: 'Qty'
                    },
                    unitPrice: {
                        gridField: 'CuryUnitCost',
                        recognitionField: 'UnitPrice'
                    }
                };

            renderPdf(form, formMapping, detailsGrid, gridMapping, feedbackControl);
        }

        function renderPdf(form, formMapping, grid, gridMapping, feedbackControl) {
            // Uncomment and paste your fileID for Debug
            //if (fileId === null) {
            //    fileId = '7CAB739D-3887-4D4F-8373-6BC704DE02C7';
            //}

            let pdfUrl = '../../Frames/GetFile.ashx?inmemory=1&fileID=' + fileId;

            // Loaded via <script> tag, create shortcut to access PDF.js exports.
            let pdfjsLib = window['pdfjs-dist/build/pdf'];

            // The workerSrc property shall be specified.
            pdfjsLib.GlobalWorkerOptions.workerSrc = '../../Scripts/pdf.js/pdf.worker.min.js';

            if (recognizedData !== null) {
                let pdfRecognitionContainer = document.getElementById('pdfRecognitionViewerContainer');
                let pdfRecognitionViewer = new PdfRecognitionViewer(pdfUrl, pdfjsLib, pdfRecognitionContainer, recognizedData, feedbackControl);

                pdfRecognitionViewer.trackFormControls(form, formMapping);
                pdfRecognitionViewer.trackGridControls(grid, gridMapping);
                pdfRecognitionViewer.renderPdf();
            }
        }

        function CaptureDetailsGrid(grid) {
            detailsGrid = grid;
        }
    </script>
</asp:Content>
