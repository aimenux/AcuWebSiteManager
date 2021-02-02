<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU204000.aspx.cs" Inherits="Page_AU204000"
	 %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<label class="projectLink transparent border-box">CODE</label>
	<pxa:AUDataSource ID="ds" runat="server" Width="100%" TypeName="PX.SM.ProjectCodeMaintenance" PrimaryView="Items" Visible="true">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />            
            <px:PXDSCallbackCommand CommitChanges="True" Name="insert" />            
            <px:PXDSCallbackCommand CommitChanges="True" Name="edit" />            
		</CallbackCommands>		
	</pxa:AUDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">

	<px:PXGrid ID="grid" runat="server" Width="100%"
		SkinID="Primary" AutoAdjustColumns="True" SyncPosition="True" 
		FilesIndicator="False" NoteIndicator="False">
		<AutoSize Enabled="true" Container="Window" />
		<Mode AllowAddNew="False" />
		<ActionBar Position="Top" ActionsVisible="false">
			<Actions>
				<AddNew MenuVisible="False" ToolBarVisible="False" />
				<NoteShow MenuVisible="False" ToolBarVisible="False" />
				<ExportExcel MenuVisible="False" ToolBarVisible="False" />
				<AdjustColumns MenuVisible="False" ToolBarVisible="False" />
			</Actions>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit SuppressLabel="True" Height="100%" runat="server" ID="edSource" TextMode="MultiLine"
						DataField="Content" Font-Size="10pt" Font-Names="Courier New" Wrap="False" SelectOnFocus="False">
						<AutoSize Enabled="True" />
					</px:PXTextEdit>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Name" Width="108px" LinkCommand="edit"/>
				
					<px:PXGridColumn DataField="Description" Width="108px" />
					<%--<px:PXGridColumn AllowNull="False" DataField="IsDisabled" TextAlign="Center" Type="CheckBox" />--%>
		<%--			<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username"
						Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" Width="90px" />--%>
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username"
						Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>

	</px:PXGrid>

	<px:PXSmartPanel ID="PanelNewFile" runat="server" 
		Caption="Create Code File"  
		Key="FilterNewFile"
		CaptionVisible="True" 
		Width="300px"
		AutoRepaint="True"
		>
		<px:PXFormView ID="FormNewFile" runat="server" CaptionVisible="False" DataMember="FilterNewFile"
			DataSourceID="ds"
			Width="100%" AutoRepaint="False" SkinID="Transparent">			
		    <Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXDropDown CommitChanges="True" ID="edFileTemplateName" runat="server" AllowNull="False"
					DataField="FileTemplateName" Required="True" />
				<px:PXSelector ID="BaseDac" runat="server" DataField="BaseDac" />
				<px:PXSelector ID="BaseGraph" runat="server" DataField="BaseGraph" />

   				<px:PXTextEdit ID="edFileClassName" runat="server" DataField="FileClassName" Required="True" />

				<px:PXCheckBox runat="server" ID="edGenerateDac" DataField="GenerateDacMembers" />
				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartRow="true" />			
                <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK" />
					<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>				
			</Template>
		</px:PXFormView>        
	</px:PXSmartPanel>               	     		
</asp:Content>
