<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM514000.aspx.cs" Inherits="Page_AM514000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" 
        TypeName="PX.Objects.AM.CreateECOsProcess" PrimaryView="ECRRecords" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="True" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" Caption="Selection"> 
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />
            <px:PXCheckBox ID="chkMerge" runat="server" DataField="MergeECRs" AlignLeft ="True" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" 
        AllowSearch="true" DataSourceID="ds" BatchUpdate="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="ECRRecords">
			    <RowTemplate>
			        <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected"/>
			        <px:PXSelector ID="edECRID" runat="server" DataField="ECRID" AllowEdit="True" />
                    <px:PXTextEdit ID="edNewRev" runat="server" DataField="RevisionID" />
                    <px:PXSelector ID="edBOMID" runat="server" DataField="BOMID" />
                    <px:PXSelector ID="edBOMRev" runat="server" DataField="BOMRevisionID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                    <px:PXTextEdit ID="edInventoryDesc" runat="server" DataField="InventoryItem__Descr" />
                    <px:PXSegmentMask runat="server" ID="edSiteID" DataField="SiteID" AllowEdit="True" />
                    <px:PXNumberEdit ID="edPriority" runat="server" DataField="Priority" />
                    <px:PXSelector ID="edRequestor" runat="server" DataField="Requestor" />
                    <px:PXDateTimeEdit ID="edRequestDate" runat="server" DataField="RequestDate" />
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" Width="30px" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="ECRID" Width="120px" />
                    <px:PXGridColumn DataField="RevisionID" Width="120px" />
                    <px:PXGridColumn DataField="BOMID" Width="120px" />
                    <px:PXGridColumn DataField="BOMRevisionID" Width="80px" />
                    <px:PXGridColumn DataField="InventoryID" Width="120px" />
                    <px:PXGridColumn DataField="InventoryItem__Descr" Width="200px" />
                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                    <px:PXGridColumn DataField="Priority" Width="80px" />
                    <px:PXGridColumn DataField="RequestDate" Width="120px" />
                    <px:PXGridColumn DataField="EffectiveDate" Width="120px" />
                    <px:PXGridColumn DataField="Requestor" Width="120px" />
                    <px:PXGridColumn DataField="Descr" Width="200px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>