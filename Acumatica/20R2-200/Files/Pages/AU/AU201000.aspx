<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU201000.aspx.cs" Inherits="Page_AU201000"
	 %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<label class="projectLink transparent border-box">Customized Screens</label>
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.SM.AUProjectScreenMaint" PrimaryView="Pages" Visible="true">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="viewAction" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="actionNewScreen" />
            

		</CallbackCommands>		
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" SkinID="Primary" BatchUpdate="False"
		Width="100%" AdjustPageSize="Auto" AllowSearch="True" SyncPosition="true"
		AllowPaging="false" AutoAdjustColumns="true"
		FilesIndicator="False"
		NoteIndicator="False">
		<Levels>
			<px:PXGridLevel DataMember="Pages">
				<Columns>
					<px:PXGridColumn LinkCommand="viewAction" DataField="ScreenID" Width="80px" CommitChanges="true" />
					<px:PXGridColumn DataField="Title" Width="150px" />
					<px:PXGridColumn DataField="IsNew" Width="80px" Type="CheckBox"/>
					<%--<px:PXGridColumn DataField="IsDisabled" Type="CheckBox"  />--%>
					<%--       <px:PXGridColumn DataField="CreatedByID" Width="100px" />
                                <px:PXGridColumn DataField="CreatedDateTime" Width="100px" />--%>

					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username"
						Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedDateTime" Width="90px" />

					<%--        <px:PXGridColumn DataField="LastModifiedByID" Width="100px" />                                
                                <px:PXGridColumn DataField="LastModifiedDateTime" Width="100px" />--%>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Enabled="True" MinHeight="150" Container="Window" />
		<ActionBar ActionsVisible="False">
			<Actions>
				<ExportExcel MenuVisible="False" ToolBarVisible="False" />
				<AdjustColumns MenuVisible="False" ToolBarVisible="False" />
				<Delete Order="2" /> <AddNew Order="3" />
			</Actions>

		</ActionBar>
	</px:PXGrid>


	<px:PXSmartPanel ID="PanelPageWizard" runat="server" Caption="Create New Screen" CaptionVisible="True" Key="ViewPageWizard" AutoRepaint="True" AutoReload="True">
            <px:PXFormView ID="formPageWizard" runat="server" Width="100%" DataSourceID="ds" SkinID="Transparent" DataMember="ViewPageWizard">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXMaskEdit ID="ScreenID" runat="server" DataField="ScreenID" />
                    <px:PXTextEdit ID="GraphName" runat="server" DataField="GraphName" CommitChanges="True"/>
                    <px:PXTextEdit ID="GraphNamespace" runat="server" DataField="GraphNamespace" CommitChanges="True" />
                    <px:PXTextEdit ID="PageTitle" runat="server" DataField="PageTitle" />
                    <px:PXDropDown ID="Template" runat="server" DataField="Template" />
                    

                   
                </Template>
            </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" >
               
            </px:PXButton>
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False" />
        </px:PXPanel>
    </px:PXSmartPanel>   
	
	    
      <px:PXSmartPanel ID="PanelCustScreen" runat="server" Caption="Customize Existing Screen" CaptionVisible="True" LoadOnDemand="true" Key="ViewSelectScreen" >
            <px:PXFormView ID="ViewSelectScreen" runat="server" Width="100%" DataSourceID="ds" SkinID="Transparent" DataMember="ViewSelectScreen">
                <Template>
	                <px:PXLayoutRule runat="server" ControlSize="L"/>
                   	<px:PXSelector ID="SiteMap" runat="server" DataField="SiteMap"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="true" />
                    

                   
                </Template>
            </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" >
               
            </px:PXButton>
            <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>  
    
       	     		
</asp:Content>
