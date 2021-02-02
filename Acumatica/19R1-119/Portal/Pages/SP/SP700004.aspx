<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP700004.aspx.cs" Inherits="Pages_SP700004"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" Width="100%" TypeName="SP.Objects.IN.SOShipmentInquire"
		PrimaryView="SOOrderView" PageLoadBehavior="PopulateSavedValues">
	    <CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewShipment" CommitChanges="true" Visible="false" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="SOOrderView" Caption="Selection" Width="100%" RenderStyle="Simple">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="XS" LabelsWidth="S" />
            <px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr"/>
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" 
        AllowPaging="true" SkinID="Attributes" FastFilterFields="OrderNbr" FilesIndicator="False" 
        NoteIndicator="False" AutoAdjustColumns="false">
		<Levels>
			<px:PXGridLevel DataMember="shipmentlist">
				<Columns>
				    <px:PXGridColumn DataField="ShipmentNbr" Width="100px" LinkCommand="ViewShipment"/>
                    <px:PXGridColumn DataField="SOShipment__StatusIsNull" Width="81px" />
                    <px:PXGridColumn DataField="ShipDate" Width="90px" />
                    <px:PXGridColumn DataField="CustomerLocationID" Width="100px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        <ActionBar DefaultAction="cmdViewShipment" PagerVisible="False">
            <CustomItems>
				<px:PXToolBarButton Key="cmdViewShipment">
					<AutoCallBack Command="ViewShipment" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
				</px:PXToolBarButton>
                </CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
            </Template>
        <AutoSize Container="Window"/>
    </px:PXFormView>
</asp:Content>
