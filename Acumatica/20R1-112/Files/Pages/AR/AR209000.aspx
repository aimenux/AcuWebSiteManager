<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR209000.aspx.cs"
    Inherits="Page_AR209000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.AR.ARDiscountMaint" PrimaryView="Document" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ViewARDiscountSequence" DependOnGrid="grid" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="144px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
        AutoAdjustColumns="True" AllowSearch="true" DataSourceID="ds" SkinID="Primary" MatrixMode="True" AdjustPageSize="Auto">
        <Levels>
            <px:PXGridLevel DataMember="Document">
                <Mode InitNewRow="true"/>
                <Columns>
                    <px:PXGridColumn DataField="DiscountID" DisplayFormat="&gt;aaaaaaaaaa" LinkCommand="ViewARDiscountSequence" CommitChanges="true" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn AllowNull="False" DataField="Type" RenderEditorText="True" AutoCallBack="true" CommitChanges="true" />
                    <px:PXGridColumn AllowNull="False" DataField="ApplicableTo" RenderEditorText="True" />
					<px:PXGridColumn AllowNull="False" DataField="IsAppliedToDR" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="IsManual" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="ExcludeFromDiscountableAmt" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="SkipDocumentDiscounts" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="IsAutoNumber" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="LastNumber" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar/>
    </px:PXGrid>
</asp:Content>
