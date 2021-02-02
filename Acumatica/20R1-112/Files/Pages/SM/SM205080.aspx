<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/MasterPages/FormDetail.master" CodeFile="SM205080.aspx.cs" Inherits="Pages_SM_SM205080" ValidateRequest="False" %>


<asp:Content ID="Content1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource runat="server"
	ID="ds"
	Visible="True"
	TypeName="PX.SM.SessionListMaint"
	PrimaryView="Filter"
	Width="100%">
		<CallbackCommands>
		     
			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>




<asp:Content ID="Content2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView runat="server" 
	ID="form"
	Width="100%"
	
	DataMember="Filter" CaptionVisible="False">
		<Template>
		    
            	
			<px:PXLayoutRule ID="Column2" runat="server" StartColumn="True" GroupCaption="Memory Usage"/>
			<px:PXTextEdit runat="server" DataField="GCTotalMemory" ID="GCTotalMemory" />		
			<px:PXTextEdit runat="server" DataField="WorkingSet" ID="WorkingSet" />		
					
			<px:PXTextEdit runat="server" DataField="GCCollection" ID="GCCollection" />		
            <px:PXButton ID="PXButton1" runat="server" NavigateUrl="http://technet.microsoft.com/en-us/sysinternals/dd535533.aspx" Target="_blank" Text="Inspect memory usage"></px:PXButton>
           


	
		</Template>
		
	</px:PXFormView>
</asp:Content>



<asp:Content ID="Content3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid runat="server" ID="grid" SkinID="Details" Width="100%" Height="400px"
         SyncPosition="True" 
        AutoAdjustColumns="True" AllowPaging="True" AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel DataMember="List">
				<Columns>
					<px:PXGridColumn DataField="Source" />
					<px:PXGridColumn DataField="User" />
					<px:PXGridColumn DataField="Created" DisplayFormat="dd MMM HH:mm" />
                    <px:PXGridColumn DataField="ID"  />
					
					<px:PXGridColumn DataField="Size" DisplayFormat="0,0" />
					
				</Columns>
				
			</px:PXGridLevel>
		</Levels>
<%--        <CallbackCommands>
            <Refresh RepaintControls="Bound" />
        </CallbackCommands>--%>
	    <AutoSize Enabled="True" Container="Window"/>
        <ActionBar PagerVisible="False" DefaultAction="Details">
            <CustomItems>
                <px:PXToolBarButton Text="Details" PopupPanel="PanelDetails" Key="Details"/>
       

            </CustomItems>
        </ActionBar>
		

	</px:PXGrid>
    
    
    
     <px:PXSmartPanel runat="server" ID="PanelDetails" Width="100%" Height="650px"
        ShowMaximizeButton="True"
        CaptionVisible="True"
		Caption="Details"
        AutoSize-Enabled="True"
         AutoRepaint="True" Key="Details">
       

        <px:PXGrid runat="server" ID="GridDetails"
            Width="100%"
            SkinID="Details"
           PageSize="25"
          
           AllowPaging="True">
            <Mode AllowFormEdit="True"></Mode>
            <Levels>
                
			<px:PXGridLevel DataMember="Details" >
				<Columns>

				    <px:PXGridColumn DataField="TypeName" Width="400"/>
				    <px:PXGridColumn  DataField="Count" />
				    <px:PXGridColumn  DataField="Size"/>

				</Columns>
				
			</px:PXGridLevel>
		</Levels>
	    <AutoSize Enabled="True" Container="Parent"/>
        <ActionBar PagerVisible="False"/>    

        </px:PXGrid>
    </px:PXSmartPanel>    
    

<%--     <px:PXSmartPanel runat="server" ID="PanelStatic" Width="100%" Height="650px"
        ShowMaximizeButton="True"
        CaptionVisible="True"
		Caption="Details"
        AutoSize-Enabled="True"
         AutoRepaint="True" Key="StaticVars">
       

        <px:PXGrid runat="server" ID="GridStaticVars"
            Width="100%"
            SkinID="Details"
           PageSize="25"
          
           AllowPaging="True">
            <Mode AllowFormEdit="True"></Mode>
            <Levels>
                
			<px:PXGridLevel DataMember="StaticVars" >
				<Columns>

				    <px:PXGridColumn DataField="TypeName" Width="400"/>
				    <px:PXGridColumn DataField="Field" />
				    <px:PXGridColumn  DataField="Size"/>

				</Columns>
				
			</px:PXGridLevel>
		</Levels>
	    <AutoSize Enabled="True" Container="Parent"/>
        <ActionBar PagerVisible="False"/>    

        </px:PXGrid>
    </px:PXSmartPanel>--%>
    


</asp:Content>

