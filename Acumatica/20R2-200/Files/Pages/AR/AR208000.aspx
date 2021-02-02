<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AR208000.aspx.cs" Inherits="Page_AR208000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Records" TypeName="PX.Objects.AR.ARPriceClassMaint" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" 
        ActionsPosition="Top" AllowSearch="true" 
        SkinID="Primary">
        <Levels>
            <px:PXGridLevel  DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXMaskEdit ID="edPriceClassID" runat="server" DataField="PriceClassID" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description"  />
                    <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder"  />
                    </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="PriceClassID" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="SortOrder" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
