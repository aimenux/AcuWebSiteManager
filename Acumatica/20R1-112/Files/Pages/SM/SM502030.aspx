<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM502030.aspx.cs" Inherits="Page_SM302060" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.BusinessProcess.UI.BusinessProcessEventHistoryMaint" SuspendUnloading="False" PrimaryView="Filter">
         <CallbackCommands>
            <px:PXDSCallbackCommand Name="viewBPEvent"  CommitChanges="True" Visible="False" DependOnGrid="grid"/>
            <px:PXDSCallbackCommand Name="viewSubscriber"  CommitChanges="True"  Visible="False" DependOnGrid="grid2"/>
            <px:PXDSCallbackCommand Name="restartFromLastFailedSubscriber" CommitChanges="True" Visible="True" RepaintControls="All" DependOnGrid="grid"/>
            <px:PXDSCallbackCommand Name="viewEventDetails" CommitChanges="True"  Visible="True" RepaintControls="All" DependOnGrid="grid" StateColumn="Status"/>
            <px:PXDSCallbackCommand Name="executeSubscriber" CommitChanges="True" Visible="False" RepaintControls="All" DependOnGrid="grid2"/>
            <px:PXDSCallbackCommand Name="deleteAll" CommitChanges="True" Visible="True" RepaintControls="All" DependOnGrid="grid"/>
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXSmartPanel ID="pnlViewDetails" runat="server" Height="600px" Width="800px" Caption="Show Event Data"
                     CaptionVisible="true" Key="EventDetails" AutoCallBack-Enabled="true"
                     AutoCallBack-Command="Refresh" AutoCallBack-Target="frmViewDetails" AllowResize="false">
        <px:PXFormView ID="frmViewDetails" runat="server" DataSourceID="ds" Width="100%" CaptionVisible="False" DataMember="EventDetails">
            <ContentStyle BackColor="Transparent" BorderStyle="None"/>
            <Template>
                <px:PXTextEdit ID="edDetails" runat="server" DataField="Details" Height="550px" Style="z-index: 101; border-style: none;" TextMode="MultiLine"
                               Width="100%" SelectOnFocus="false" LabelWidth="0px">
                </px:PXTextEdit>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons" >
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Close" Width="63px" Height="20px"/>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>

<asp:Content ContentPlaceHolderID="phF" ID="cont3" runat="server">
    <px:PXFormView runat="server" ID="form" DataSourceID="ds" DataMember="Filter" Width="100%">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXSelector ID="edDefinitionId" runat="server" DataField="DefinitionId" AutoRefresh="True"  DataSourceID="ds" CommitChanges="True">
                <GridProperties>
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSelector>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M"></px:PXLayoutRule>
            <px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" CommitChanges="True"/>
            <px:PXDateTimeEdit ID="edToDate" runat="server" DataField="ToDate" CommitChanges="True"/>
        </Template>
    </px:PXFormView>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
      <px:PXSplitContainer runat="server" ID="sp1" SkinID="Horizontal" SplitterPosition="400">
        <AutoSize Enabled="True" Container="Window" />
        <Template1>
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" AllowFilter="True"  ActionsPosition="Top"
                SkinID="PrimaryInquire"  SyncPosition="true" AdjustPageSize="Auto" AllowPaging="True" Caption="Business Events" KeepPosition="True"  
                CaptionVisible="true" AutoAdjustColumns="True" PreserveSortsAndFilters="True" PreservePageIndex="True" >
                <Levels>
                    <px:PXGridLevel DataMember="Events">
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="True" AllowRowSelect="True" />
                        <Columns>
                            <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="70px" AllowResize="false"/>
                            <px:PXGridColumn DataField="LastRunStatus" Width="60" Type="Icon" TextAlign="Center"  />
                            <px:PXGridColumn DataField="Status" Width="100"></px:PXGridColumn>
                            <px:PXGridColumn DataField="BPEvent__Name" Width="200" LinkCommand="viewBPEvent" CommitChanges="True"/>
                            <px:PXGridColumn DataField="BPEvent__Type" Width="250" />
                            <px:PXGridColumn DataField="BPEvent__Active" Width="70" Type="CheckBox" AllowResize="false"/>
                            <px:PXGridColumn DataField="BPEvent__ScreenIdValue"  Width="100"/>
                            <px:PXGridColumn DataField="CreatedDateTime"  DisplayFormat="g"  Width="150"/>
                            <px:PXGridColumn DataField="LastModifiedDateTime"  DisplayFormat="g"  Width="150"/>
                            <px:PXGridColumn DataField="ErrorText"  Width="350"/>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" Container="Parent" />
                <AutoCallBack Command="Refresh" Target="grid2"/>
                <ActionBar>
                    <Actions>
                        <Refresh ToolBarVisible="Top"/>
                    </Actions>
                </ActionBar>
            </px:PXGrid>
        </Template1>
        <Template2>
            <px:PXGrid ID="grid2" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="260px"
                SkinID="Inquire" AdjustPageSize="Auto" AllowPaging="True" Caption="Subscribers" CaptionVisible="true" SyncPosition="True" AutoAdjustColumns="True">
                <Levels>
                    <px:PXGridLevel DataMember="EventSubscribers">
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="True" AllowRowSelect="False" />
                        <Columns>
                            <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AllowResize="false" CommitChanges="True"/>
                            <px:PXGridColumn DataField="LastRunStatus" Width="60" Type="Icon" TextAlign="Center" />
                            <px:PXGridColumn DataField="HandlerID" TextField="HandlerID_Description" Width="150" LinkCommand="viewSubscriber" CommitChanges="True"/>
                            <px:PXGridColumn DataField="Type" Width="50" AllowUpdate="False"/>
                            <px:PXGridColumn DataField="ErrorText" Width="350" AllowUpdate="False"/>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" Container="Parent" MinHeight="150" />
                <ActionBar>
                    <CustomItems>
                        <px:PXToolBarButton CommandName="executeSubscriber" CommandSourceId="ds"  DependOnGrid="grid2">
                            <AutoCallBack>
                                <Behavior  RepaintControls="All"></Behavior>
                            </AutoCallBack>
                        </px:PXToolBarButton>
                    </CustomItems>
                </ActionBar>
            </px:PXGrid>
        </Template2>
    </px:PXSplitContainer>
</asp:Content>
