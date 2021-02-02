<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU204500.aspx.cs" Inherits="Page_AU204500"
	 %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">   
	
	<label runat="server" id="lblCaption" class="projectLink transparent border-box">Custom Files</label>
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.SM.ProjectFileMaintenance" PrimaryView="Items" Visible="True">
		<CallbackCommands>
		       
			<px:PXDSCallbackCommand CommitChanges="True" Name="edit" Visible="False"/>            
			<%--<px:PXDSCallbackCommand Name="actionAddFiles" CommitChanges="True" RepaintControls="Bound"  BlockPage="True" Visible="False"/>--%>            
		</CallbackCommands>		
	</px:PXDataSource>	
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phL" runat="Server">
    
    			<px:PXGrid ID="grid" runat="server" 
				Width="100%" 
                Height="350px"
				SkinID="Primary" 
				AutoAdjustColumns="True" 
				SyncPosition="True" 
				FilesIndicator="True" 
				NoteIndicator="False"
				AllowPaging="False"
				>
				<AutoSize Enabled="true" Container="Window" />
				<Mode AllowAddNew="False" />
				<ActionBar Position="None" >
					<Actions>
						<AddNew MenuVisible="False" ToolBarVisible="False"/>
						<NoteShow MenuVisible="False" ToolBarVisible="False" />
						<ExportExcel MenuVisible="False" ToolBarVisible="False" />
						<AdjustColumns ToolBarVisible="False"/>
					
					</Actions>
			
				</ActionBar>
				<Levels>
					<px:PXGridLevel DataMember="Items">
			
						<Columns>
							<px:PXGridColumn DataField="Name" Width="200px" LinkCommand="edit" />
						    <px:PXGridColumn DataField="IsThirdParty" Width="100px" Type="CheckBox"  />
							<px:PXGridColumn DataField="Description" Width="200px"  />

							<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username"/>
							<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedDateTime"  />
						</Columns>
					</px:PXGridLevel>
				</Levels>
                				
			</px:PXGrid>	
	

		
    <px:PXSmartPanel runat="server" ID="FilterSelectFile" Width="600px" Height="400px" CaptionVisible="True"
		Caption="Add Files" ShowMaximizeButton="True" Key="ViewSelectFile" ShowAfterLoad="true" AutoRepaint="True">
        
        		<px:PXGrid runat="server" 
			ID="gridAddFile" DataSourceID="ds" 
			Width="100%" BatchUpdate="True" 
			CaptionVisible="False" 
			Caption="Website Files"
			AutoAdjustColumns="True"
            AllowPaging="False"
			SkinID="Details"
			>
			<Levels>
				<px:PXGridLevel DataMember="ViewSelectFile">
					<Columns>
					    <px:PXGridColumn DataField="Selected" Type="CheckBox" Width="100px"/>						
					    <px:PXGridColumn DataField="Path" Width="300px"/>						
					    <px:PXGridColumn DataField="Modified" />						
					    <px:PXGridColumn DataField="Size"/>						
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
	<%--			<CustomItems>
					<px:PXToolBarButton >
						<AutoCallBack Target="ds" Command="actionAddFiles"/>
					</px:PXToolBarButton>
				</CustomItems>--%>
			</ActionBar>
		</px:PXGrid>			
	
         <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Save" />
                    <px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" CausesValidation="False" />
        </px:PXPanel>
	</px:PXSmartPanel>
    
    

    <px:PXSmartPanel runat="server" ID="PanelEditFile" Width="90%" Height="400px" CaptionVisible="True"
		Caption="Edit File" AutoCallBack-Enabled="True" AutoCallBack-Target="FormEditFile" AutoCallBack-Command="Refresh"
		ShowMaximizeButton="True" Overflow="Hidden"
		Key="FilterFileEdit">
		<px:PXFormView runat="server" ID="FormEditFile" DataMember="FilterFileEdit" Width="100%"
			Height="100%" CaptionVisible="False" Overflow="Hidden" OverflowY="Hidden">
			<AutoSize Enabled="true" />
			<Template>
				<px:PXTextEdit runat="server" ID="edContent" DataField="Content" DisableSpellcheck="True"  Style="font-family: Monospace;
					overflow: scroll;" Wrap="false" Width="100%" Height="100%" TextMode="MultiLine"
					Font-Size="10pt" SelectOnFocus="False" SuppressLabel="True" >
					<AutoSize Enabled="true" />
				</px:PXTextEdit>
			</Template>
		</px:PXFormView>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Save" />
                    <px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
	</px:PXSmartPanel>   
</asp:Content>


	

	

