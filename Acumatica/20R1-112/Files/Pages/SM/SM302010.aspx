<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM302010.aspx.cs" Inherits="Page_SM302000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.BusinessProcess.UI.QueueDispatchersMonitor" SuspendUnloading="False" PrimaryView="Status">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="viewNqdLog"  Visible="False" PopupPanel="PanelNqdLogDetails" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="viewEqdLog"  Visible="False" PopupPanel="PanelEqdLogDetails" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="restartNqd"  Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="purgeNotificationQueue"  Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="restartEqd"  Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="purgeEventQueue"  Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="clearErrors" Visible="False" DependOnGrid="grid1"/>
            <px:PXDSCallbackCommand Name="showSourceData" Visible="False" DependOnGrid="grid1" PopupPanel="pnlViewCurrentError"/>
            <px:PXDSCallbackCommand Name="clearEqdLog"  Visible="False" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="clearNqdLog"  Visible="False" RepaintControls="All"/>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Status" TabIndex="5500">
		<Template>
			<px:PXPanel runat="server" ID="PXPanel1" Caption="Push Notification Queue">
                <px:PXLayoutRule runat="server" StartRow="True" ControlSize="XXL" LabelsWidth="SM" ColumnSpan="2"/>
                <px:PXTextEdit ID="edNqdQueueName" runat="server" DataField="NqdQueueName" AlreadyLocalized="False"/>
                <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" ColumnSpan="2"/>
                <px:PXDropDown ID="edNqdStatus" runat="server" DataField="NqdStatus" Enabled="False"/>
                <px:PXLayoutRule runat="server" />
                <px:PXDropDown ID="edPerformanceStatus" runat="server" DataField="NqdPerformanceStatus" Enabled="False"/>
                <px:PXLayoutRule runat="server" ColumnSpan="2" />
                <px:PXTextEdit ID="edNqdQueueCount" runat="server" DataField="NotificationQueueCount"/>
                <px:PXTextEdit ID="edNqdQueueSize" runat="server" DataField="NotificationQueueSize"/>
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                <px:PXButton runat="server" ID="bViewNotificationDetails" AlignLeft="false" AlreadyLocalized="False" AutoRefresh="True" AutoAdjustColumns="True" 
                             CommandName="viewNqdLog" CommandSourceID="ds" PopupPanel="PanelNqdLogDetails">
                </px:PXButton>
                <px:PXLayoutRule runat="server" StartRow="True" Merge="True" />
                <px:PXButton runat="server" ID="bPurgeNotificationQueue" AlignLeft="False" AlreadyLocalized="False" AutoRefresh="True" 
                             CommandName="purgeNotificationQueue" CommandSourceID="ds"></px:PXButton>
                <px:PXButton runat="server" ID="bRestartNotificationDispatcher" AlignLeft="False" AlreadyLocalized="False" AutoRefresh="True" 
                             CommandName="restartNqd" CommandSourceID="ds"></px:PXButton>
            </px:PXPanel>
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXPanel runat="server" ID="PXPanel2" Caption="Business Event Queue">
                <px:PXLayoutRule runat="server" StartRow="True" ControlSize="XXL" LabelsWidth="SM" ColumnSpan="2"/>
                <px:PXTextEdit ID="edEqdQueueName" runat="server" DataField="EqdQueueName" AlreadyLocalized="False"/>
                <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" ColumnSpan="2"/>
                <px:PXDropDown ID="edEqdStatus" runat="server" DataField="EqdStatus" Enabled="False"/>
                <px:PXLayoutRule runat="server" />
                <px:PXDropDown ID="edEventPerformanceStatus" runat="server" DataField="EqdPerformanceStatus" Enabled="False"/>
                <px:PXLayoutRule runat="server" ColumnSpan="2" />
                <px:PXTextEdit ID="edEqdQueueCount" runat="server" DataField="EventQueueCount"/>
                <px:PXTextEdit ID="edEqdQueueSize" runat="server" DataField="EventQueueSize"/>
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                <px:PXButton runat="server" ID="bViewEventDetails" Text="VIEW DETAILS" AlignLeft="false" AlreadyLocalized="False" AutoRefresh="True" AutoAdjustColumns="True" 
                             CommandName="viewEqdLog" CommandSourceID="ds" PopupPanel="PanelEqdLogDetails" >
                </px:PXButton>
                <px:PXLayoutRule runat="server" StartRow="True" Merge="True" />
                <px:PXButton runat="server" ID="bPurgeEventQueue" AlignLeft="False" AlreadyLocalized="False" AutoRefresh="True" 
                             CommandName="purgeEventQueue" CommandSourceID="ds"></px:PXButton>
                <px:PXButton runat="server" ID="bRestartEventDispatcher" AlignLeft="False" AlreadyLocalized="False" AutoRefresh="True" 
                             CommandName="restartEqd" CommandSourceID="ds"></px:PXButton>
            </px:PXPanel>
		</Template>
	</px:PXFormView>
    <px:PXSmartPanel ID="PanelNqdLogDetails" runat="server" style="height:460px;width:800px;" CaptionVisible="True" Caption="Queue Processing Log" Key="NotificationQueueDispatcherLogDetail"
         AutoReload="True" AutoRepaint="True">
        <px:PXFormView ID="NqdLogDetailView" runat="server" DataSourceID="ds" Style="z-index: 500" 
                       Width="100%" Height="400px" DataMember="NotificationQueueDispatcherLogDetail" TabIndex="5500">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" Merge="True" GroupCaption="Statistics"/>
                <px:PXTextEdit ID="edNProcessTime" runat="server" DataField="ProcessingTime" AlreadyLocalized="False"/>
                <px:PXTextEdit ID="edNDate" runat="server" TextMode="DateTimeLocal" DataField="Date" Enabled="False" />
                <px:PXLayoutRule runat="server" StartRow="True" GroupCaption="Detailed Log" />
                <px:PXTextEdit ID="edNLog" runat="server" DataField="Log" Enabled="False" TextMode="MultiLine" Height="290" SuppressLabel="True" ></px:PXTextEdit>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel5" SkinID="Buttons" runat="server">
            <px:PXButton ID="BtnClearNqd" runat="server" Text="Clear Log" CommandName="clearNqdLog" CommandSourceID="ds" />
            <px:PXButton ID="PanelNqdLogDetailsSubmit" runat="server" DialogResult="OK" Text="Close" />
		</px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelEqdLogDetails" runat="server" style="height:460px;width:800px;" CaptionVisible="True" Caption="Queue Processing Log" Key="EventQueueDispatcherLogDetail"
                     AutoReload="True" AutoRepaint="True">
        <px:PXFormView ID="EqdLogDetailView" runat="server" DataSourceID="ds" Style="z-index: 500" 
                       Width="100%" Height="400px" DataMember="EventQueueDispatcherLogDetail" TabIndex="5500">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" Merge="True" GroupCaption="Statistics"/>
                <px:PXTextEdit ID="edEProcessTime" runat="server" DataField="ProcessingTime" AlreadyLocalized="False"/>
                <px:PXTextEdit ID="edEDate" runat="server" TextMode="DateTimeLocal" DataField="Date" Enabled="False" />
                <px:PXLayoutRule runat="server" StartRow="True" GroupCaption="Detailed Log" />
                <px:PXTextEdit ID="edELog" runat="server" DataField="Log" Enabled="False" TextMode="MultiLine" Height="290" SuppressLabel="True" ></px:PXTextEdit>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel3" SkinID="Buttons" runat="server">
            <px:PXButton ID="BtnClearEqd" runat="server" Text="Clear Log" CommandName="clearEqdLog" CommandSourceID="ds"/>
            <px:PXButton ID="PanelEqdLogDetailsSubmit" runat="server" DialogResult="OK" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlViewCurrentError" runat="server" Height="600px" Width="800px" Caption="Source Data"
                     CaptionVisible="true" Key="CurrentError" AutoCallBack-Enabled="true"
                     AutoCallBack-Command="Refresh" AutoCallBack-Target="frmViewNotification" AllowResize="false">
        <px:PXFormView ID="frmViewNotification" runat="server" DataSourceID="ds" Width="100%"
                       CaptionVisible="False" DataMember="CurrentError">
            <ContentStyle BackColor="Transparent" BorderStyle="None">
            </ContentStyle>
            <Template>
                <px:PXTextEdit ID="edDetailsNotification" runat="server" DataField="SourceData" Height="550px" Style="z-index: 101; border-style: none;" TextMode="MultiLine"
                               Width="100%" SelectOnFocus="false">
					
                </px:PXTextEdit>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons" >
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Close" Width="63px" Height="20px">
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px">
		<Items>
			<px:PXTabItem Text="Errors">
				<Template>
	                <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Style="z-index: 100" 
		                Width="100%" Height="150px" SkinID="Details" TabIndex="5700" AutoAdjustColumns="true" SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto">
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton CommandSourceID="ds" CommandName="clearErrors" />
                                <px:PXToolBarButton CommandSourceID="ds" CommandName="showSourceData" PopupPanel="pnlViewCurrentError"/>
                            </CustomItems>
                            <Actions>
                                <AddNew Enabled="False" />
                                <Delete Enabled="False"/>
                            </Actions>
                        </ActionBar>
                        <Mode InitNewRow="True" />
		                <Levels>
			                <px:PXGridLevel  DataMember="Errors">
                                <Columns>
                                    <px:PXGridColumn DataField="HookId"/>
                                    <px:PXGridColumn DataField="Source" Width="150px" />
                                    <px:PXGridColumn DataField="SourceEvent" />
                                    <px:PXGridColumn DataField="ErrorMessage" />
                                    <px:PXGridColumn DataField="TimeStamp" DisplayFormat="G"  />
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	                </px:PXGrid>
                </Template>
		    </px:PXTabItem>
            <px:PXTabItem Text="Settings">
                <Template>
                    <px:PXFormView ID="SettingsForm" runat="server" DataSourceID="ds" Style="z-index: 100" 
                                   Width="100%" DataMember="settings" TabIndex="5900">
                        <Template>
                            <px:PXPanel runat="server" ID="PXPanel1" Caption="Push Notification Queue">
                                <px:PXLayoutRule runat="server" ControlSize="XXL" LabelsWidth="215" ColumnSpan="2"/>
                                <px:PXNumberEdit runat="server" ID="edNqdLongProcessingThreshold" DataField="NqdLongProcessingThreshold" CommitChanges="True"/>
                                <px:PXNumberEdit runat="server" ID="edNqdLogMaxLength" DataField="NqdLogMaxLength" CommitChanges="True"/>
                            </px:PXPanel>
                            <px:PXLayoutRule runat="server" StartColumn="True" />
                            <px:PXPanel runat="server" ID="PXPanel2" Caption="Business Event Queue" >
                                <px:PXLayoutRule runat="server" ControlSize="XXL" LabelsWidth="215" ColumnSpan="2"/>
                                <px:PXNumberEdit runat="server" ID="edEqdLongProcessingThreshold" DataField="EqdLongProcessingThreshold" CommitChanges="True"/>
                                <px:PXNumberEdit runat="server" ID="edEqdLogMaxLength" DataField="EqdLogMaxLength" CommitChanges="True"/>
                            </px:PXPanel>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
	    <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXTab>
</asp:Content>
