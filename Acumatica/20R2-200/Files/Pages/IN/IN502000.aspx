<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN502000.aspx.cs" Inherits="Page_IN502000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INUpdateStdCost" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" 
		Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" 
        DefaultControlID="edSiteID" TabIndex="100">
		<Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" 
                ControlSize="XM" />
			<px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" 
                DataSourceID="ds">
				<GridProperties FastFilterFields="Descr">
                    <Layout ColumnsMenu="False" /> 
                </GridProperties>
				<Items>
					<px:PXMaskItem EditMask="AlphaNumeric" EmptyChar="_" Length="10" Separator="-" TextCase="Upper" />
				</Items>
				<AutoCallBack Target="form" Command="Save"></AutoCallBack>
			</px:PXSegmentMask>
			<px:PXDateTimeEdit ID="edPendingStdCostDate" runat="server" DataField="PendingStdCostDate">
				<AutoCallBack Target="form" Command="Save" />
			</px:PXDateTimeEdit>
			<px:PXCheckBox ID="chkRevalueInventory" runat="server" DataField="RevalueInventory" >
				<AutoCallBack Target="form" Command="Save" />
			</px:PXCheckBox>				
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" 
		Style="z-index: 100" Width="100%" ActionsPosition="top" BatchUpdate="True"
		SkinID="PrimaryInquire" Caption="Details" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="True" 
        TabIndex="300">	
	    <Levels>
			<px:PXGridLevel DataMember="INItemList" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
				    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" 
                                      Enabled="False" HintField="descr" Width="81px" AllowEdit="True">
						<Items>
							<px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
							<px:PXMaskItem Separator="-" />
							<px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
						</Items>
						<GridProperties FastFilterFields="Descr" />
					</px:PXSegmentMask>
				    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" 
                                      Enabled="False" HintField="descr" Width="81px" AllowEdit="True"/>
					<px:PXSegmentMask ID="edInvtAcctID" runat="server" DataField="InvtAcctID" Enabled="False" HintField="description">
						<Items>
							<px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
						</Items>
						<GridProperties FastFilterFields="Description" />
					</px:PXSegmentMask>
					<px:PXSegmentMask ID="edInvtSubID" runat="server" DataField="InvtSubID" Enabled="False" >
						<Items>
							<px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
							<px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
							<px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
						</Items>
						<GridProperties FastFilterFields="Descr" />
					</px:PXSegmentMask>
					<px:PXNumberEdit ID="edPendingStdCost" runat="server" DataField="PendingStdCost"
						Decimals="4" Enabled="False" ValueType="Decimal" />
				    <px:PXDateTimeEdit ID="edPendingStdCostDate" runat="server" DataField="PendingStdCostDate"
						Enabled="False" />
				    <px:PXNumberEdit ID="edStdCost" runat="server" DataField="StdCost"
						Decimals="4" Enabled="False" ValueType="Decimal" />
				    <px:PXCheckBox ID="chkStdCostOverride" runat="server" DataField="StdCostOverride"
						Enabled="False" Text="Std. Cost Override" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" AllowCheckAll="True" DataType="Boolean" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn AllowUpdate="False" DataField="InventoryID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SiteID" />
					<px:PXGridColumn AllowUpdate="False" DataField="InvtAcctID" />
					<px:PXGridColumn AllowUpdate="False" DataField="InvtSubID" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="PendingStdCost"
						DataType="Decimal" Decimals="4" DefValueText="0.0" TextAlign="Right" />
					<px:PXGridColumn AllowUpdate="False" DataField="PendingStdCostDate" DataType="DateTime" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="StdCost" DataType="Decimal"
						Decimals="4" DefValueText="0.0" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="StdCostOverride"
						DataType="Boolean" DefValueText="False" TextAlign="Center" Type="CheckBox" />
				</Columns>

			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
