<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SP203010.aspx.cs" Inherits="Pages_SP203010"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.CRCaseMaint"
        PrimaryView="Case">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="CloseCase" PopupVisible="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ReopenCase" PopupVisible="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="AddComment" PopupVisible="True"
                PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="NewCase" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Next" PostData="Self" StartNewGroup="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Previous" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
            <px:PXDSCallbackCommand Name="Release" Visible="False" />
            <px:PXDSCallbackCommand Name="TakeCase" Visible="False" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
            <px:PXDSCallbackCommand Name="Delete" Visible="False" />
            <px:PXDSCallbackCommand Name="Assign" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewInvoice" Visible="False" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="true"
                DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True"
                DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True"
                DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True"
                DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True"
				DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="CaseRefs_CRCase_ViewDetails" Visible="False" CommitChanges="True"
                DependOnGrid="gridCaseReferencesDependsOn" />
            <px:PXDSCallbackCommand Name="Action" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Inquiry" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="syncSalesforce" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="PanelView" runat="server" AcceptButtonID="PXButtonOK" AutoReload="true"
        CancelButtonID="PXButtonCancel" Caption="Add New Comment" CaptionVisible="True"
        ShowMaximizeButton="false" DesignView="Content" HideAfterAction="false" Key="Comment"
        LoadOnDemand="true" Height="440px" Width="450px" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="formview" AllowResize="False">
        <px:PXFormView ID="formview" runat="server" CaptionVisible="False" DataMember="Comment"
            Width="100%" Height="100%">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
                <px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple">
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" ControlSize="XXL"
                        SuppressLabel="True" />
                    <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
                    <px:PXTextEdit ID="edBody" runat="server" DataField="Body" TextMode="MultiLine" Style="width: 100%;
                        height: 300px" />
                </px:PXPanel>
                <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="OK" CommandName="OK"
                        CommandSourceID="ds" />
                    <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Cancel" />
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Case" Width="100%"
        AllowCollapse="False">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="120px"
                ControlSize="M" />
            <px:PXSelector ID="edCaseCD" runat="server" DataField="CaseCD" FilterByAllFields="True"
                AutoRefresh="True" Size="S" />
            <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime"
                Enabled="False" Size="SM" />
            <px:PXDateTimeEdit ID="edLastActivity" runat="server" DataField="LastActivity" Enabled="False"
                Size="SM" />
            <px:PXTextEdit ID="edSupporter" runat="server" DataField="Supporter.FullName" Enabled="False" Size="SM"/>				
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="120px"
                ControlSize="M" />
            <px:PXSelector ID="edcaseClassID" runat="server" DataField="caseClassID" CommitChanges="True"
                DisplayMode="Text" />
            <px:PXTextEdit ID="edAcctName" runat="server" DataField="BAccountName.AcctName" Enable="false"/>
            <px:PXSelector ID="edContractID" runat="server" DataField="ContractID" FilterByAllFields="True"
                AutoRefresh="True" DisplayMode="Text" AutoAdjustColumns="True" />
            <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" FilterByAllFields="True"
                AutoRefresh="True" DisplayMode="Text" AutoAdjustColumns="True" />
            <px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="XS"
                ControlSize="SM" />
            <px:PXDropDown ID="PXTextEdit1" runat="server" DataField="Status" Enabled="False"
                Size="M" />
            <px:PXDropDown ID="PXTextEdit2" runat="server" DataField="Resolution" Enabled="False"
                AllowNull="False" Size="M" />
            <px:PXDropDown ID="PXTextEdit4" runat="server" DataField="Severity" Enabled="False" />
            <px:PXDropDown ID="PXTextEdit3" runat="server" DataField="Priority" Enabled="True"  CommitChanges="True"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="CaseCurrent">
        <Items>
            <px:PXTabItem Text="Activities">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%"
                        AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                        filesfield="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray"
                        PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
                        BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
                                <Columns>
                                    <px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" Width="60px" />
                                    <px:PXGridColumn DataField="Subject" Width="450px" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false"
                                        Width="108px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <GridMode AllowAddNew="False" AllowUpdate="False" />
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100px" SkinID="Label">
                                <AutoSize Enabled="true" />
                            </px:PXHtmlView>
                        </PreviewPanelTemplate>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False"
                            AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Case Description">
                <Template>
                    <px:PXHtmlView ID="edDescription" runat="server" Style="border-width: 0px; width: 100%;"
                        DataField="Description">
                        <AutoSize Enabled="True" />
                    </px:PXHtmlView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
                        Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="IsRequired" Width="80px" AllowShowHide="False" AllowSort="False"
                                        Type="CheckBox" />
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False"
                                        TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="Value" Width="300px" AllowShowHide="False" AllowSort="False" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="450" MinWidth="300" />
    </px:PXTab>
</asp:Content>
