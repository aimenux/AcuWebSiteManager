<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
     ValidateRequest="false" CodeFile="SF205030.aspx.cs" 
    Inherits="Page_SF205030" Title="Real-Time Sync" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="RStoLSSyncProc" TypeName="PX.Salesforce.SFSyncMaint" >
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont4" ContentPlaceHolderID="phL" runat="Server">
     <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
          Width="100%" Height="150px" CaptionVisible="False"
          AllowPaging="True" AdjustPageSize="Auto" NoteIndicator="False" FilesIndicator="False"
	 	  SkinID="PrimaryInquire" TabIndex="500" SyncPosition="True" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="RStoLSSyncProc">
                                <Columns>
                                    <px:PXGridColumn DataField="Selected" Width="70px" AllowCheckAll="true" Type="CheckBox" TextAlign="Center"/> 
                                    <px:PXGridColumn DataField="ProcStatus" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="EntityType" DisplayMode="Text" TextAlign="Left" Width="120px"/>
                                    <px:PXGridColumn DataField="ImportScenario" Width="220px" />
                                    <px:PXGridColumn DataField="ExportScenario" Width="220px" />                                    
                                    <px:PXGridColumn DataField="LastNotificationTime" DisplayFormat="yy-MM-dd HH:mm:ss" TimeMode="true" Width="140px" Visible="False" />
                                    <px:PXGridColumn DataField="LastNotification" Width="300px" Visible="False" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar PagerVisible="False" ActionsVisible="False"/>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
						<Mode AllowAddNew="False" AllowDelete="False" />
                    </px:PXGrid>
</asp:Content>