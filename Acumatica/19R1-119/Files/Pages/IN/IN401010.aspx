<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="IN401010.aspx.cs" Inherits="Page_IN401010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.INSiteSummaryEnq" PageLoadBehavior="PopulateSavedValues" PrimaryView="CurrentFilter" Visible="True"  >
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" 
        Style="z-index: 100" Width="100%" Caption="Selection" 
        DataMember="CurrentFilter" CaptionAlign="Justify" 
        DefaultControlID="edInventoryID" TabIndex="100">
	    <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" 
                DataSourceID="ds">
                <AutoCallBack Command="Save" Target="form" />
                <GridProperties FastFilterFields="Descr"> <Layout ColumnsMenu="False" /> </GridProperties>
                <Items>
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
                </Items>
            </px:PXSegmentMask>
            <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" DataSourceID="ds" >
                <AutoCallBack Command="Save" Target="form" />
                <GridProperties FastFilterFields="Descr"> <Layout ColumnsMenu="False" /> </GridProperties>
                <Items>
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
                </Items>
            </px:PXSegmentMask>           
	        <px:PXCheckBox ID="chkOnlyAvailable" runat="server" Checked="True" DataField="OnlyAvailable">
                 <AutoCallBack Command="Save" Target="form" />
            </px:PXCheckBox>
	        <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="S" StartColumn="True" />
            <px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" DataSourceID="ds">
                <GridProperties> <Layout ColumnsMenu="False" /> </GridProperties>
                <AutoCallBack Command="Save" Target="form" />
            </px:PXSelector>
            <px:PXSegmentMask ID="edSubItemCD" runat="server" DataField="SubItemCD" AutoRefresh="True" DataSourceID="ds">
                <AutoCallBack Command="Save" Target="form" />
                <GridProperties FastFilterFields="Descr">
                     <Layout ColumnsMenu="False" />
                 </GridProperties>
                <Items>
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="1" Separator="-" TextCase="Upper" />
                    <px:PXMaskItem EditMask="AlphaNumeric" Length="1" Separator="-" TextCase="Upper" />
                </Items>
            </px:PXSegmentMask>
	    </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" 
        Style="z-index: 100; left: 0px; top: 0px;" Width="100%" AdjustPageSize="Auto" 
        AllowPaging="True" AllowSearch="True" Caption="Site Summary" BatchUpdate="True" 
        SkinID="Inquire" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Records" >
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" Width="150px" />
                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" />
                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" Width="150px" />
                    <px:PXGridColumn AllowNull="False" DataField="QtyAvail" DataType="Decimal" Decimals="4"
                        DefValueText="0.0" TextAlign="Right" Width="100px" />
                     <px:PXGridColumn AllowNull="False" DataField="QtyHardAvail" DataType="Decimal" Decimals="4"
                        DefValueText="0.0" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" 
                        DataField="QtyOnHand" DataType="Decimal" Decimals="2" DefValueText="0.0" 
                        Label="Qty. On Hand" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" 
                        AutoGenerateOption="NotSet" DataField="InventoryItem__BasePrice" 
                        DataType="Decimal" Decimals="4" DefValueText="0.0" Label="Current Price" 
                        TextAlign="Right" Width="100px" />                    
                    <px:PXGridColumn AutoGenerateOption="NotSet" 
                        DataField="InventoryItem__SalesUnit" DisplayFormat="&gt;aaaaaa" 
                        Label="Sales Unit"  />
                   
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />        
	</px:PXGrid>
</asp:Content>
