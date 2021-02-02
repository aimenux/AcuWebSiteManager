<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SP100000.aspx.cs" Inherits="Pages_SP100000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%" PrimaryView="CustomerSummary" TypeName="SP.Objects.SP.SPSummaryMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="gridAnnouncements" Name="AnnouncementViewDetails" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="gridCustomerSummary" Name="SummaryViewDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="NewCase" Visible="False" />
            <px:PXDSCallbackCommand Name="PrintCustomerStatement" Visible="False" />
            <px:PXDSCallbackCommand Name="AccountSettings" Visible="False" />
            <px:PXDSCallbackCommand Name="PrintAgedBalanceReport" Visible="False" />
            <px:PXDSCallbackCommand Name="ManageUsers" Visible="False" />
            <px:PXDSCallbackCommand Name="BrowseDocumentsHistory" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" AllowCollapse="False" DataMember="CustomerSummary">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" ControlSize="XXS" LabelsWidth="M"  StartColumn="True" />
            <px:PXPanel ID="PXPanel2" runat="server" RenderStyle="Simple" Style="background-color: White; border: 1px solid #BBBBBB; padding-left: 10px; padding-right: 10px; padding-top: 20px; padding-bottom: 20px;">
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" SuppressLabel="True" />
                <px:PXButton runat="server" ID="edButton1" ImageSet="main" ImageKey="Calendar" Text="Enter New Support Case" ImageAlign="Top" Height="45px" Width="170px" CommandName="NewCase" CommandSourceID="ds"/>
                <px:PXButton runat="server" ID="edButton2" ImageSet="main" ImageKey="Calendar" Text="Reprint Customer Statement" ImageAlign="Top" Height="45px" Width="170px" CommandName="PrintCustomerStatement" CommandSourceID="ds"/>
               
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" SuppressLabel="True" />
                <px:PXButton runat="server" ID="edButton4" ImageSet="main" ImageKey="Calendar" Text="Company Profile" ImageAlign="Top" Height="45px" Width="170px" CommandName="AccountSettings" CommandSourceID="ds"/>
                <px:PXButton runat="server" ID="edButton5" ImageSet="main" ImageKey="Calendar" Text="Print Aged Balance Report" ImageAlign="Top" Height="45px" Width="170px" CommandName="PrintAgedBalanceReport" CommandSourceID="ds"/>
                
                <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" SuppressLabel="True" />
                <px:PXButton runat="server" ID="edButton8" ImageSet="main" ImageKey="Calendar" Text="Contacts" ImageAlign="Top" Height="45px" Width="170px" CommandName="ManageUsers" CommandSourceID="ds"/>
                <px:PXButton runat="server" ID="edButton7" ImageSet="main" ImageKey="Calendar" Text="Browse Documents History" ImageAlign="Top" Height="45px" Width="170px" CommandName="BrowseDocumentsHistory" CommandSourceID="ds"/>
            </px:PXPanel>

            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" SuppressLabel="True" ControlSize="XL" />
            <px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="background-color: White; border: 1px solid #BBBBBB; padding: 10px">
                <px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
                <px:PXLabel ID="PXLabelCustomerSummary" runat="server" Text="Customer Summary" Style="margin-top: 2px; font-weight: bold" />
                <px:PXLayoutRule ID="PXLayoutRule12" runat="server" />
                <px:PXGrid ID="gridCustomerSummary" runat="server" DataSourceID="ds" ActionsPosition="Top" 
                    AllowSearch="True" SkinID="Details" Width="300px" MatrixMode="True"  Height="77px" FilesIndicator="False"
                    NoteIndicator="False"  CssClass="GridMainTransparent" SyncPosition="True">
                    <Layout RowSelectorsVisible="False" HeaderVisible="false" />
                    <Levels>
                        <px:PXGridLevel DataMember="CustomerSummary">
                            <Columns>
                                <px:PXGridColumn DataField="Name" Width="140px" />
                                <px:PXGridColumn DataField="Data" Width="140px" TextAlign="Right" LinkCommand="SummaryViewDetails" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <LevelStyles>
                        <Row CssClass="GridTransparentRow" />
                        <AlternateRow CssClass="GridTransparentRow" />
                        <SelectedRow CssClass="GridTransparentRow" />
                        <ActiveRow CssClass="GridTransparentRow" />
                        <ActiveCell CssClass="GridTransparentRow" />
                    </LevelStyles>
                    <ActionBar PagerVisible="False" DefaultAction="cmdViewSummaryDetails" ActionsVisible="False">
                        <CustomItems>
                            <px:PXToolBarButton Key="cmdViewSummaryDetails">
                                <ActionBar GroupIndex="0" />
                                <AutoCallBack Command="SummaryViewDetails" Target="ds" />
                            </px:PXToolBarButton>
                        </CustomItems>
                        <Actions>
                            <FilterSet Enabled="False" />
                            <FilterShow Enabled="False" />
                            <AdjustColumns Enabled="False" />
                            <ExportExcel Enabled="False" />
                        </Actions>
                    </ActionBar>
                    <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                </px:PXGrid>
            </px:PXPanel>
        </Template>
    </px:PXFormView>

   <px:PXFormView ID="PXFormView2" runat="server" DataSourceID="ds" AllowCollapse="False" DataMember="Announcements" Width="100%" Style="border: 0px">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
            <px:PXPanel ID="PXPanel11" runat="server" RenderStyle="Simple" Style="background-color: White; border: 1px solid #BBBBBB; padding: 10px">
                <px:PXLayoutRule ID="PXLayoutRule111" runat="server" />
                <px:PXLabel ID="PXLabelAnnouncementSummary" runat="server" Text="Announcements Summary" Style="margin-top: 2px; font-weight: bold" />
                <px:PXLayoutRule ID="PXLayoutRule112" runat="server" />
                <px:PXGrid ID="gridAnnouncements" runat="server" DataSourceID="ds" ActionsPosition="Top"
                    AllowSearch="true" SkinID="Details" Width="940px" MatrixMode="True" FilesIndicator="False"
                    NoteIndicator="False" CssClass="GridMainTransparent" SyncPosition="True">
                    <Layout RowSelectorsVisible="False" HeaderVisible="False" FooterVisible="False" />
                    <Levels>
                        <px:PXGridLevel DataMember="Announcements">
                            <Columns>
                                <px:PXGridColumn AllowUpdate="False" DataField="PublishedDateTime" Width="120px" />
                                <px:PXGridColumn DataField="Subject" Width="300px" LinkCommand="AnnouncementViewDetails" />
                                <px:PXGridColumn DataField="Smallbody" Width="500px" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <LevelStyles>
                        <Row CssClass="GridTransparentRow" />
                        <AlternateRow CssClass="GridTransparentRow" />
                        <SelectedRow CssClass="GridTransparentRow" />
                        <ActiveRow CssClass="GridTransparentRow" />
                        <ActiveCell CssClass="GridTransparentRow" />
                    </LevelStyles>
                    <ActionBar PagerVisible="False" ActionsVisible="False" DefaultAction="cmdAnnouncementViewDetails">
                        <CustomItems>
                            <px:PXToolBarButton Text="View Details" Tooltip="View Announcements" Key="cmdAnnouncementViewDetails">
                                <AutoCallBack Command="AnnouncementViewDetails" Target="ds"/>
                                <ActionBar GroupIndex="0" Order="0" />
                            </px:PXToolBarButton>
                        </CustomItems>
                        <Actions>
                            <FilterSet Enabled="False" />
                            <FilterShow Enabled="False" />
                            <AdjustColumns Enabled="False" />
                            <ExportExcel Enabled="False" />
                        </Actions>
                    </ActionBar>
                    <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                </px:PXGrid>
            </px:PXPanel>
        </Template>
    </px:PXFormView>
</asp:Content>
