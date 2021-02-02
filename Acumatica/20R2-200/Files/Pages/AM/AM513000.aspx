<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM513000.aspx.cs" Inherits="Page_AM513000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" 
        TypeName="PX.Objects.AM.AMTimeCardCreate" PrimaryView="Items" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="True" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="TimeCardFilter" Caption="Selection"> 
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowAll" runat="server" DataField="ShowAll" AlignLeft ="True" TextAlign="Left" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" 
        AllowSearch="true" DataSourceID="ds" BatchUpdate="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Items">
			    <RowTemplate>
			        <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected"/>
			        <px:PXSelector ID="edBatNbr" runat="server" DataField="BatNbr" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" />
                    <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" />
                    <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AllowEdit="True" />
                    <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" />
                    <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" AllowEdit="True" />
                    <px:PXTimeSpan ID="edLaborTime" TimeMode="True" runat="server" DataField="LaborTime" InputMask="hh:mm" />
                    <px:PXNumberEdit ID="edLaborRate" runat="server" DataField="LaborRate"  />
                    <px:PXNumberEdit ID="edExtCost" runat="server" DataField="ExtCost"  />
                    <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AllowEdit="True" />
                    <px:PXTextEdit ID="edCostCodeID" runat="server" DataField="CostCodeID" AllowEdit="True" />
                    <px:PXDropDown ID="edTimeCardStatus" runat="server" DataField="TimeCardStatus"  />
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                    <px:PXSegmentMask ID="InventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                    <px:PXSelector ID="edLaborCodeID" runat="server" DataField="LaborCodeID" AllowEdit="True" />
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" Width="30px" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="BatNbr" Width="120px" />
                    <px:PXGridColumn DataField="TranDate" Width="120px" />
                    <px:PXGridColumn DataField="OrderType" Width="60px" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="135px" />
                    <px:PXGridColumn DataField="OperationID" Width="85px" />
                    <px:PXGridColumn DataField="EmployeeID" Width="120px" />
                    <px:PXGridColumn DataField="LaborTime" Width="75px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="LaborRate" Width="75px" />
                    <px:PXGridColumn DataField="ExtCost" Width="100px" />
                    <px:PXGridColumn DataField="ProjectID" Width="120px" />
                    <px:PXGridColumn DataField="TaskID" Width="120px" />
                    <px:PXGridColumn DataField="CostCodeID" Width="120px" />
                    <px:PXGridColumn DataField="TimeCardStatus" Width="120px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                    <px:PXGridColumn DataField="InventoryID" Width="120px" />
                    <px:PXGridColumn DataField="LaborCodeID" Width="120px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
