using System;
using PX.Data;
using PX.Objects.AP;
using PX.SM;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using PX.Reports.Controls;
using PX.Reports.Data;
using PX.Data.Reports;
using PX.Objects.PM;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using PX.Objects.CT;
using PX.Data.ReferentialIntegrity.Attributes;
using System.Reflection;

namespace PX.Objects.CN.ProjectAccounting.PM.Graphs
{
    public class SubstantiatedBilling : PXGraph<SubstantiatedBilling>
    {
        #region Report Constants
        public class string_AP : PX.Data.BQL.BqlString.Constant<string_AP>
        {
            public string_AP()
                : base("AP")
            {
            }
        }
        public class string_E : PX.Data.BQL.BqlString.Constant<string_E>
        {
            public string_E()
                : base("E")
            {
            }
        }
        public class string_ADR : PX.Data.BQL.BqlString.Constant<string_ADR>
        {
            public string_ADR()
                : base("ADR")
            {
            }
        }
        public class string_ACR : PX.Data.BQL.BqlString.Constant<string_ACR>
        {
            public string_ACR()
                : base("ACR")
            {
            }
        }
        public class string_INV : PX.Data.BQL.BqlString.Constant<string_INV>
        {
            public string_INV()
                : base("INV")
            {
            }
        }
        public class string_IN : PX.Data.BQL.BqlString.Constant<string_IN>
        {
            public string_IN()
                : base("IN")
            {
            }
        }
        public class string_GL : PX.Data.BQL.BqlString.Constant<string_GL>
        {
            public string_GL()
                : base("GL")
            {
            }
        }
        #endregion

        protected int queryTimeout;

        #region Views
        public PXCancel<Substantial> Cancel;

        public PXFilter<Substantial> MasterView;

        public PXSelectJoinGroupBy<PMTran,
                                   InnerJoin<PMAccountGroup, On<PMTran.accountGroupID, Equal<PMAccountGroup.groupID>>,
                                   InnerJoin<PMProject, On<PMTran.projectID, Equal<PMProject.contractID>>,
                                   InnerJoin<PMTask, On<PMTran.taskID, Equal<PMTask.taskID>, And<PMTran.projectID, Equal<PMTask.projectID>>>,
                                   LeftJoin<PMCostCode, On<PMTran.costCodeID, Equal<PMCostCode.costCodeID>>,
                                   LeftJoin<PMRegister, On<PMTran.refNbr, Equal<PMRegister.refNbr>, And<PMTran.tranType, Equal<PMRegister.module>>>,
                                   LeftJoin<APRegister, On<APRegister.docType, Equal<PMTran.origTranType>, And<APRegister.refNbr, Equal<PMTran.origRefNbr>>>,
                                   LeftJoin<NoteDoc, On<APRegister.noteID, Equal<NoteDoc.noteID>, Or<PMRegister.noteID, Equal<NoteDoc.noteID>>>,
                                   InnerJoin<UploadFile, On<NoteDoc.fileID, Equal<UploadFile.fileID>>,
                                   InnerJoin<UploadFileRevision, On<UploadFileRevision.fileID, Equal<UploadFile.fileID>>>>>>>>>>>,
                                              Where<PMProject.contractID, Equal<Current<Substantial.projectID>>,
                                                     And<PMTran.tranDate, GreaterEqual<Current<Substantial.fromDate>>,
                                                     And<PMTran.tranDate, LessEqual<Current<Substantial.toDate>>,
                                                     And<IsNull<PMTran.origTranType, string_INV>,
                                                     NotEqual<string_ADR>, And<IsNull<PMTran.origTranType, string_INV>, NotEqual<string_ACR>,
                                                     And<PMAccountGroup.type, Equal<string_E>, And<PMTran.tranType, NotEqual<string_GL>, And<PMTran.tranType,
                                                     NotEqual<string_IN>>>>>>>>>,
                                             Aggregate<GroupBy<PMCostCode.costCodeID, GroupBy<PMTran.tranType, GroupBy<PMTran.origRefNbr,
                                                      GroupBy<PMTran.refNbr, GroupBy<UploadFileRevision.fileID, GroupBy<UploadFileRevision.blobData>>>>>>>,
                                                           OrderBy<Asc<PMTran.projectID, Asc<PMCostCode.costCodeID, Asc<PMTran.tranType, Asc<PMTran.origRefNbr, Asc<PMTran.refNbr>>>>>>> AttachmentsView;

       
        #endregion
        #region Timeout
        public class ReflectionMethods
        {
            public static void SetPrivatePropertyValue<T>(T obj, string propertyName, object newValue)
            {
                // add a check here that the object obj and propertyName string are not null
                foreach (FieldInfo fi in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (fi.Name.ToLower().Contains(propertyName.ToLower()))
                    {
                        fi.SetValue(obj, newValue);
                        break;
                    }
                }
            }
        }
        #endregion

        #region  Action
        public PXAction<Substantial> GetFile;
        [PXButton]
        [PXUIField(DisplayName = "Get File")]
        protected IEnumerable getFile(PXAdapter a)
        {
            bool val = check();
            List<byte[]> reportBytes = new List<byte[]>();
            PX.SM.FileInfo file = null;
            using (Report report = PXReportTools.LoadReport("PM650000", null))
            {
                if (report == null)
                    throw new Exception("Unable to access Acumatica report writter for specified report : " + "PM650000");

                Dictionary<string, string> prams = new Dictionary<string, string>();
                PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<Substantial.projectID>>>>.Select(this);
                prams["ProjectID"] = project.ContractCD;
                prams["DateFrom"] = this.MasterView.Current.FromDate.ToString();
                prams["DateTo"] = this.MasterView.Current.ToDate.ToString();

                PXReportTools.InitReportParameters(report, prams, PXSettingProvider.Instance.Default);
                ReflectionMethods.SetPrivatePropertyValue(PXDatabase.Provider, "queryTimeout", 900);
                ReportNode repNode = ReportProcessor.ProcessReport(report);
                IRenderFilter renderFilter = ReportProcessor.GetRenderer(ReportProcessor.FilterPdf);

                using (StreamManager streamMgr = new StreamManager())
                {
                    renderFilter.Render(repNode, null, streamMgr);
                    string fileName = string.Format("SubstantiatedBilling #{0}.pdf", prams);
                    file = new PX.SM.FileInfo(fileName, null, streamMgr.MainStream.GetBytes());
                    reportBytes.Add(file.BinData);
                }
            }

            file = null;
            byte[] mergedBytes = MergePDFs(reportBytes);
            string date = DateTime.Today.ToShortDateString().Replace("/", "-");
            string fileNameSub = string.Format("SubstantiatedBilling_{0}.pdf", date);
            file = new PX.SM.FileInfo(fileNameSub, "", mergedBytes);
            if (file == null)
                return a.Get();

            throw new PXRedirectToFileException(file, true);
        }

        #endregion

        #region validation
        /// <summary>
        /// Validates the form on submit button click
        /// </summary>
        /// <returns>true for valid input and throws exception for invalid inputs</returns>
        public bool check()
        {
            if (MasterView.Current.ProjectID == null || string.IsNullOrEmpty(MasterView.Current.FromDate.ToString()) || string.IsNullOrEmpty(MasterView.Current.ToDate.ToString()))
                throw new Exception("Provide value into Paramaters");
            return true;
        }
        #endregion

        #region Merging Pdf
        /// <summary>
        /// Creates pdf document based on the input list. PDF is generated with the list of invoice attachments listed in the report.
        /// </summary>
        /// <param name="reportBytes">The input List of type byte[] contains info related to the report PM650000 (Substantial Billing Report).</param>
        /// <returns>pdf document in byte array format</returns>
        public byte[] MergePDFs(List<byte[]> reportBytes)
        {
            List<string> apStringList1 = new List<string>();
            List<string> apStringList2 = new List<string>();
            List<string> pmStringList3 = new List<string>();
            List<string> pmStringList4 = new List<string>();
            List<string> stringList5 = new List<string>();

            PdfDocument pdfDocument = new PdfDocument();
            PdfPage destinationPage = null;

            foreach (PdfPage page in PdfReader.Open(new MemoryStream(reportBytes[0]), PdfDocumentOpenMode.Import).Pages)
                destinationPage = pdfDocument.AddPage(page);

            PdfOutline pdfOutline = pdfDocument.Outlines.Add("Report Summary: ", pdfDocument.Pages[0], true, PdfOutlineStyle.Bold, XColors.DarkBlue);

            PreferencesEmail prefs = PXSelect<PreferencesEmail>.Select(this);
            string url = $"{prefs.NotificationSiteUrl}(W(2))/Frames/GetFile.ashx?CompanyID={Accessinfo.CompanyName}&fileID=";

            foreach (PXResult<PMTran, PMAccountGroup, PMProject, PMTask, PMCostCode, PMRegister, APRegister, NoteDoc, UploadFile, UploadFileRevision> pxResult in AttachmentsView.Select(Array.Empty<object>()))
            {
                UploadFileRevision uploadFileRevision = pxResult;
                UploadFile uploadFile = pxResult;
                PMTran pmTran = pxResult;
                if ((pmTran.OrigRefNbr != null || pmTran.RefNbr != null) && pmTran.TranType != "AR")
                {
                    string referenceNbr = pmTran.TranType == "AP" ? pmTran.OrigRefNbr : pmTran.RefNbr;
                    string extension = Path.GetExtension(uploadFile.Name);
                    var imageExtensions = new List<string> { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".tif" };
                    if (extension.ToLower() == ".pdf" || imageExtensions.Contains(extension.ToLower()))
                    {
                        if (pmTran.TranType == "AP")
                        {
                            if (!apStringList1.Contains(pmTran.OrigRefNbr) || !apStringList2.Contains(uploadFile.Name))
                            {
                                apStringList1.Add(pmTran.OrigRefNbr);
                                apStringList2.Add(uploadFile.Name);
                            }
                            else
                                continue;
                        }
                        if (pmTran.TranType == "PM")
                        {
                            if (!pmStringList4.Contains(pmTran.RefNbr) || !pmStringList3.Contains(uploadFile.Name))
                            {
                                pmStringList4.Add(pmTran.RefNbr);
                                pmStringList3.Add(uploadFile.Name);
                            }
                            else
                                continue;
                        }
                        if (!stringList5.Contains(referenceNbr))
                            pdfOutline = pdfDocument.Outlines.Add("RefNbr: " + referenceNbr, destinationPage, true, PdfOutlineStyle.Bold, XColors.DarkBlue);
                        stringList5.Add(referenceNbr);


                        if (extension.ToLower() == ".pdf")
                        {
                            try
                            {
                                var pdfDoc = PdfReader.Open(new MemoryStream(uploadFileRevision.BlobData), PdfDocumentOpenMode.Import);
                                foreach (PdfPage page in pdfDoc.Pages)
                                    destinationPage = pdfDocument.AddPage(page);
                            }
                            catch
                            {
                                PdfPage page = pdfDocument.AddPage();
                                AddLink(page, "The PDF file that is available by the following link has a specific structure and cannot be displayed in this report:",
                                    uploadFile.Name, url + uploadFile.FileID.ToString());
                            }
                        }
                        else
                        {
                            destinationPage = pdfDocument.AddPage();
                            AddImage(destinationPage, uploadFileRevision.BlobData);
                        }
                        pdfOutline.Outlines.Add(uploadFile.Name ?? "", destinationPage, true);
                    }
                }
            }
            using (MemoryStream memoryStream = new MemoryStream())
            {
                pdfDocument.Save(memoryStream, false);
                return memoryStream.ToArray();
            }
        }

        private void AddLink(PdfPage page, string message, string name, string url)
        {
            XGraphics gfx = XGraphics.FromPdfPage(page);
            var xrect = new XRect(new XPoint(30, 30), new XSize(600, 30));
            var rect = gfx.Transformer.WorldToDefaultPage(xrect);
            var pdfrect = new PdfRectangle(rect);
            page.AddWebLink(pdfrect, url);
            gfx.DrawString(message, new XFont("Calibri", 10, XFontStyle.Regular), XBrushes.Red, xrect, XStringFormats.TopLeft);
            gfx.DrawString(name, new XFont("Calibri", 10, XFontStyle.Underline), XBrushes.Blue, xrect, XStringFormats.BottomLeft);
        }

        private void AddImage(PdfPage page, byte[] imageData)
        {
            XGraphics graphics = XGraphics.FromPdfPage(page);
            XImage image = XImage.FromStream(new MemoryStream(imageData));
            double scaleWidth = page.Width / image.PixelWidth;
            double scaleHeight = page.Height / image.PixelHeight;
            double scale = Math.Min(scaleWidth, scaleHeight);
            graphics.DrawImage(image, 0, 0, image.PixelWidth * scale, image.PixelHeight * scale);
        }
        #endregion

        #region Parameters
        [PXHidden]
        [Serializable]
        public class Substantial : IBqlTable
        {
            /// <summary>
            /// ProjectID
            /// </summary>
            public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
            [PXDefault]
            [Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.nonProject, NotEqual<True>>>), WarnIfCompleted = false)]
            public virtual int? ProjectID { get; set; }

            /// <summary>
            /// FromDate
            /// </summary>
            public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }
            [PXDBDate]
            [PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
            public virtual DateTime? FromDate { get; set; }

            /// <summary>
            /// ToDate
            /// </summary>
            public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }
            [PXDBDate]
            [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "To Date", Required = true)]
            public virtual DateTime? ToDate { get; set; }
        }
        #endregion
    }
}