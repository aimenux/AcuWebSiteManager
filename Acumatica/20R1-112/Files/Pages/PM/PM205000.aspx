<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM205000.aspx.cs"
    Inherits="Page_PM205000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.RateDefinitionMaint" PrimaryView="Filter">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewRate" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        EmailingGraph="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edRateTableID" runat="server"  DataField="RatetableID"/>
            <px:PXSelector CommitChanges="True" ID="edRateTypeID" runat="server" DataField="RateTypeID" />
		</Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" Caption="Sequences" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="RateDefinitions">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXNumberEdit ID="edSequence" runat="server" DataField="Sequence" />
                    <px:PXCheckBox ID="chkProject" runat="server" DataField="Project" />
                    <px:PXCheckBox ID="chkTask" runat="server" DataField="Task" />
                    <px:PXCheckBox ID="chkAccountGroup" runat="server" DataField="AccountGroup" />
                    <px:PXCheckBox ID="chkRateItem" runat="server" DataField="RateItem" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Sequence" Label="Sequence" SortDirection="Ascending" TextAlign="Right" />
                    <px:PXGridColumn DataField="Description" Label="Description" />
                    <px:PXGridColumn  DataField="Project" Label="Project" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn  DataField="Task" Label="Task" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn  DataField="AccountGroup" Label="Account Group" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn  DataField="RateItem" Label="Inventory" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn  DataField="Employee" Label="Employee" TextAlign="Center" Type="CheckBox" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar DefaultAction="cmdViewRate">
            <CustomItems>
                <px:PXToolBarButton Text="View Rates" Key="cmdViewRate">
                    <AutoCallBack Command="ViewRate" Target="ds">
                    </AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Mode InitNewRow="True" />
    </px:PXGrid>
</asp:Content>
