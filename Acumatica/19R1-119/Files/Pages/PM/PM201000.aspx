<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM201000.aspx.cs"
    Inherits="Page_PM201000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.AccountGroupMaint" PrimaryView="AccountGroup"
        BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="AccountGroup" Caption="Account Group Summary"
        FilesIndicator="True" NoteIndicator="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXSegmentMask ID="edGroupCD" runat="server" DataField="GroupCD" DataSourceID="ds" DisplayMode="Value" AutoRefresh="true"/>
            <px:PXDropDown CommitChanges="True" ID="edType" runat="server" DataField="Type" AllowNull="False"  />
            <px:PXCheckBox ID="chkIsExpense" runat="server" DataField="IsExpense" CommitChanges="true" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="SM" />
            <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" />
            <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" AlignLeft="True" />
            <px:PXSelector ID="edRevenueAccountGroupID" runat="server" DataField="RevenueAccountGroupID" DataSourceID="ds" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="269px" DataSourceID="ds" DataMember="AccountGroupProperties">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Accounts">
                <Template>
                    <px:PXGrid runat="server" ID="AccountGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Accounts">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="true"/>
                                    <px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
                                    <px:PXSelector ID="edAccountClassID" runat="server" DataField="AccountClassID" Enabled="False" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" /></RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AccountID" Label="Account ID" Width="108px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="Type" Label="Type" Type="DropDownList" RenderEditorText="True" Width="91px" />
                                    <px:PXGridColumn DataField="AccountClassID" Label="Account Class" Width="135px" />
                                    <px:PXGridColumn DataField="Description" Label="Description" Width="351px" />
                                    <px:PXGridColumn DataField="CuryID" Label="Currency" Width="63px" />
                                    <px:PXGridColumn DataField="IsDefault"  Width="63" AutoCallBack="true" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="220px" AllowShowHide="False" TextField="AttributeID_description" />
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
                                    <px:PXGridColumn DataField="Value" Width="250px" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
