<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU208000.aspx.cs" Inherits="Page_AU208000" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">

	<label class="projectLink transparent border-box">Site Map</label>

	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.SM.ProjectSiteMapMaintenance" PrimaryView="ProjectItems" Visible="true">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" SkinID="Primary"
		Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SyncPosition="true"
		AllowPaging="false" AutoAdjustColumns="true" FilesIndicator="False" NoteIndicator="False">
		<AutoSize Enabled="True" MinHeight="150" Container="Window" />
        <Mode AllowAddNew="False" />
		<ActionBar Position="Top" ActionsVisible="false">
			<Actions>
                <AddNew ToolBarVisible="False" MenuVisible="False"/>
				<ExportExcel  ToolBarVisible="False" />
				<AdjustColumns ToolBarVisible="False"/>
				<NoteShow MenuVisible="False" ToolBarVisible="False" />
			</Actions>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="ProjectItems">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit SuppressLabel="True" Height="100%" runat="server" ID="edSource" TextMode="MultiLine"
						DataField="Content" Font-Size="10pt" Font-Names="Courier New" Wrap="False" SelectOnFocus="False">
						<AutoSize Enabled="True" />
					</px:PXTextEdit>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="ObjectName" Width="100px" />
					<px:PXGridColumn DataField="Description" Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username" Width="108px" />
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>
    
    <px:PXSmartPanel runat="server" ID="FilterSelectSiteMap" Width="600px" Height="400px" CaptionVisible="True"
		Caption="Add Site Map Item(s)" ShowMaximizeButton="True" Key="ViewSelectSiteMap" ShowAfterLoad="true" AutoRepaint="True">
        
        		<px:PXGrid runat="server" 
			ID="gridAddSiteMap" DataSourceID="ds" 
			Width="100%" BatchUpdate="True" 
			CaptionVisible="False" 
			Caption="SiteMap"
			AutoAdjustColumns="True"
            AllowPaging="False"
			SkinID="Details"
            FilesIndicator="False"
            NoteIndicator="False"
			>
			<Levels>
				<px:PXGridLevel DataMember="ViewSelectSiteMap">
					<Columns>
					    <px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" Width="100px"/>						
    					<px:PXGridColumn DataField="ScreenID" Width="100px" />
	    				<px:PXGridColumn DataField="Title" Width="150px" />
                        <px:PXGridColumn DataField="LastModifiedDateTime" Width="100px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
		    <AutoSize Enabled="True"/>
			<ActionBar Position="Top">
				<Actions>
					<AddNew MenuVisible="False" ToolBarVisible="False"/>
					<Delete MenuVisible="False" ToolBarVisible="False"/>
					<AdjustColumns  ToolBarVisible="False"/>
					<ExportExcel  ToolBarVisible="False"/>
				</Actions>
			</ActionBar>
		</px:PXGrid>			
	
         <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Save" />
                    <px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" CausesValidation="False" />
        </px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
