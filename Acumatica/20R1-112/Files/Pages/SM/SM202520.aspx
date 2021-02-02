<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM202520.aspx.cs"
    Inherits="Page_SM210001" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.SM.UploadFileInq">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="viewRevisions" Visible="False" DependOnGrid="grid" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Cancel" RepaintControls="Bound" />
            <px:PXDSCallbackCommand Name="getFile" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="deleteFile" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="getFileLink" Visible="False" DependOnGrid="grid" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="addLink" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="addLinkClose" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="clearFiles" StartNewGroup="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<script type="text/javascript">
		window.addEventListener('unload', function () {
			var origin = __win(window.openerPX);
			if (window['fileLinkDialog'] && origin && origin.px_all && origin.px_all[window['fileLinkDialog']]) {
				origin.px_all[window['fileLinkDialog']].refresh();
			}
		});
	</script>
    <px:PXSmartPanel ID="pnlDate" runat="server" DesignView="Content" Key="ClearingFilter" Caption="Choose date" CaptionVisible="True">
        <px:PXFormView ID="frmDate" runat="server" CaptionVisible="False" Width="100%" BorderStyle="None" DataMember="ClearingFilter"
            DataSourceID="ds">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edTill" runat="server" DataField="Till" Size="SM" />
                <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" />
                    <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close" />
                </px:PXPanel>
            </Template>
            <ContentStyle BorderStyle="None" />
        </px:PXFormView>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlGetLink" runat="server" CaptionVisible="true" Caption="File Link" ForeColor="Black" Style="position: static"
        AutoCallBack-Enabled="true" AutoCallBack-Target="ds" AutoCallBack-Command="getFileLink" Height="70px" Width="450px" ShowAfterLoad="true"
        Key="GetFileLinkFilter" Overflow="Hidden">
        <px:PXFormView ID="frmGetLink" runat="server" SkinID="Transparent" DataMember="GetFileLinkFilter" DataSourceID="ds" Width="342px">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
                <px:PXTextEdit ID="edWikiLink" runat="server" DataField="WikiLink" ReadOnly="True" /></Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="Cancel" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100;" Width="100%" Caption="Selection" DataMember="Filter"
        CheckChanges="False">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit CommitChanges="True" ID="edDocName" runat="server" DataField="DocName" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDateCreatedFrom" runat="server" DataField="DateCreatedFrom" DisplayFormat="g" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXSelector CommitChanges="True" ID="edAddedBy" runat="server" DataField="AddedBy" TextField="Username" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXSelector CommitChanges="True" ID="edCheckedOutBy" runat="server" DataField="CheckedOutBy" TextField="Username"
                DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDateCreatedTo" runat="server" DataField="DateCreatedTo" DisplayFormat="g" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edScreen" runat="server" DataField="ScreenID" DisplayMode="Text" FilterByAllFields="true" CommitChanges="true" DataSourceID="ds" />
            <px:PXCheckBox ID="chkShowUnassignedFiles" runat="server" DataField="ShowUnassignedFiles" CommitChanges="true" />  
        </Template>
        <Parameters>
            <px:PXQueryStringParam Name="screenID" OnLoadOnly="true" QueryStringField="screen" Type="String" />
        </Parameters>
        <CallbackCommands>
            <Save PostData="Page" />
        </CallbackCommands>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="100%" Style="z-index: 100;" Width="100%" ActionsPosition="Top"
                AdjustPageSize="Auto" AllowPaging="True" SkinID="Inquire" FastFilterFields="Name">
                <Levels>
                    <px:PXGridLevel DataMember="Files">
                        <RowTemplate>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXSelector Size="s" ID="edName" runat="server" DataField="Name" AllowEdit="true" />
                            <px:PXTextEdit Size="s" ID="edCheckedOutComment" runat="server" DataField="CheckedOutComment" />
                            <px:PXLayoutRule runat="server" Merge="False" />
                            <px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" Enabled="False" TextField="Username" />
                            <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" DisplayFormat="g" Enabled="False" />
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="Name" Width="800px" />
                            <px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" Width="90px" />
                            <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Width="108px" />
                            <px:PXGridColumn DataField="CheckedOutComment" Width="150px" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" MinHeight="650" />
                <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                <ActionBar DefaultAction="viewRevisions" PagerVisible="False">
                    <Actions>
                        <PageFirst MenuVisible="False" ToolBarVisible="False" />
                        <PageLast MenuVisible="False" ToolBarVisible="False" />
                    </Actions>
                    <CustomItems>
                        <px:PXToolBarButton Text="Get file" CollectParams="BeforeNavigate" Tooltip="Download selected file to your computer.">
                            <AutoCallBack Command="getFile" Target="ds" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Get link" CollectParams="BeforeNavigate" Tooltip="Get link to the attached file.">
                            <AutoCallBack Command="getFileLink" Target="ds" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Delete file" CollectParams="BeforeNavigate" Tooltip="Delete selected file.">
                            <AutoCallBack Command="deleteFile" Target="ds" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Add Link" CollectParams="BeforeNavigate" Tooltip="Add Link">
                            <AutoCallBack Command="addLink" Target="ds" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Add Link" CollectParams="BeforeNavigate" Tooltip="Add Link & Close">
                            <AutoCallBack Command="addLinkClose" Target="ds" />
                        </px:PXToolBarButton>
                    </CustomItems>
                </ActionBar>
                <CallbackCommands>
                    <Refresh CommitChanges="True" PostData="Page" RepaintControlsIDs="grid" />
                </CallbackCommands>
            </px:PXGrid>
</asp:Content>
