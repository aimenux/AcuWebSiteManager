<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM204535.aspx.cs" Inherits="Page_SM204535" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.SM.GraphFieldEditor"
        PrimaryView="Filter">
		<CallbackCommands>
			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" >
	    <Template>
		    <px:PXLayoutRule runat="server" ControlSize="L"/>
	        <px:PXDropDown runat="server" ID="Container" DataField="Container"/>
             <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"/>
	        <px:PXTextEdit runat="server" ID="DataMember" DataField="DataMember"/>        

	    </Template>

	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AutoGenerateColumns="Append">
		<Levels>
			<px:PXGridLevel DataMember="AspxFields">
			    <Columns>
			        
			    </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="True">
		</ActionBar>
	</px:PXGrid>
</asp:Content>


<asp:Content ID="dialogs" ContentPlaceHolderID="phDialogs" Runat="Server">
<%--    <px:PXSmartPanel runat="server" ID="PanelAddFields"
        CaptionVisible="True"
        Caption="Add Fields"
        Key="ViewAddFields"
        Width="800px">
        
        
        <px:PXGrid runat="server" ID="gridAddFields" AutoGenerateColumns="Append">
            <Levels>
                <px:PXGridLevel DataMember="ViewAddFields">
                    

                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        
     <px:PXPanel ID="PXPanel8" runat="server" SkinID="Buttons">
	<px:PXButton ID="PanelAddFieldsOK" runat="server" Text="OK" DialogResult="OK" >
		
	</px:PXButton>
	<px:PXButton ID="PanelAddFieldsCancel" runat="server" DialogResult="Cancel" Text="Cancel">
	</px:PXButton>
	</px:PXPanel>
    </px:PXSmartPanel>--%>
    
<%--       <px:PXSmartPanel runat="server" ID="PanelDataMember"
        CaptionVisible="True"
        Caption="New Data Member"
        Key="ViewDataMember"
        Width="800px">
        
        
        <px:PXFormView runat="server" ID="FormDataMember" DataMember="ViewDataMember" Width="100%" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True"/>    

            </Template>
            

        </px:PXFormView>
        
     <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
	<px:PXButton ID="PXButton1" runat="server" Text="OK" DialogResult="OK" >
		
	</px:PXButton>
	<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel">
	</px:PXButton>
	</px:PXPanel>
    </px:PXSmartPanel>--%>
</asp:Content>