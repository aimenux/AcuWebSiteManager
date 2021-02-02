<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM206026.aspx.cs" Inherits="Page_SM206026" Title="Substitution Lists" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Api.SYSubstitutionMaint" PrimaryView="Substitution" Visible="True" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Substitution" Caption="Substitution Lists">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="110" ControlSize="XM" />
            <px:PXSelector ID="selector1" DataField="SubstitutionID" runat="server" AllowNull="False" AutoRefresh="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Height="150px" Style="z-index: 100" Width="100%"
        ActionsPosition="Top" AllowSearch="True" AutoAdjustColumns="True">
        <Mode InitNewRow="True" AllowUpload="true"/>
        <Levels>
            <px:PXGridLevel DataMember="SubstitutionValues">
                <Columns>
                    <px:PXGridColumn DataField="OriginalValue" Required="True" />
                    <px:PXGridColumn DataField="SubstitutedValue" Required="True" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
