<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM206500.aspx.cs" Inherits="Page_AM206500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.AM.LaborCodeMaint" BorderStyle="NotSet" PrimaryView="LaborCodeRecords" Visible="true">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" 
        SkinID="Primary">
	    <Levels>
			<px:PXGridLevel DataMember="LaborCodeRecords" DataKeyNames="LaborCodeID">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXDropDown ID="edLaborType" runat="server" DataField="LaborType"/>
                    <px:PXMaskEdit ID="edLaborCodeID" runat="server" AllowNull="False" DataField="LaborCodeID" InputMask="&gt;AAAAAAAAAA" />
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
                    <px:PXSegmentMask CommitChanges="True" ID="edLaborAccountID" runat="server" DataField="LaborAccountID" AutoRefresh="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edLaborSubID" runat="server" DataField="LaborSubID" />
                    <px:PXSegmentMask CommitChanges="True" ID="edOverheadAccountID" runat="server" DataField="OverheadAccountID" AutoRefresh="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edOverheadSubID" runat="server" DataField="OverheadSubID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="LaborType" Width="65px" AutoCallBack="True" DisplayFormat="&gt;AAAAAAAAAA"  />
                    <px:PXGridColumn DataField="LaborCodeID" Width="130px"/>
                    <px:PXGridColumn DataField="Descr" Width="175px"/>
                    <px:PXGridColumn DataField="LaborAccountID" Width="120px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="LaborSubID" Width="108px" />
                    <px:PXGridColumn DataField="OverheadAccountID" Width="120px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="OverheadSubID" Width="108px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar ActionsText="False" >
        </ActionBar>
	</px:PXGrid>
</asp:Content>
