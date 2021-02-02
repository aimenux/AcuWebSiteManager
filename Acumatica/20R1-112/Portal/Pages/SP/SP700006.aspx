<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SP700006.aspx.cs" Inherits="Page_SP700006"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="False" Width="100%" TypeName="SP.Objects.IN.ImageViewer"
        PrimaryView="InventoryItem">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="AddToCart" Visible="False" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="InventoryItem"
        AllowCollapse="False" Width="100%" Height="100%">
        <Template>
            <table runat="server" style="width: 100%">
                <tr>
                    <td style="min-width: 300px">
                        <px:PXTextEdit ID="PXTextEdit2" runat="server" DataField="InventoryItemDetails.InventoryDescription"
                            SuppressLabel="True" Enabled="False" Width="100%" />
                    </td>
                    <td style="min-width: 30px">
                        <px:PXLabel ID="PXLabel6" runat="server" Width="100%" />
                    </td>
                    <td style="width: 100%;">
                        <px:PXLabel ID="PXLabel7" runat="server" Width="100%" />
                    </td>
                </tr>
                <tr>
                    <td style="min-width: 300px">
                        <px:PXImageUploader Height="200px" ID="imgUploader" runat="server" DataField="ImageUrl"
                            ShowComment="true" ArrowsOutside="true" ViewOnly="true" NoSelectImage="true"
                            MagnifyDefault="True" Width="300px" SuppressLabel="True"/>
                    </td>
                    <td style="min-width: 30px">
                        <px:PXLabel ID="PXLabel1" runat="server" Width="100%" />
                    </td>
                    <td style="width: 100%;">
                        <px:PXHtmlView ID="edBody" runat="server" DataField="InventoryItemDetails.InventoryItem__Body"
                            DataFieldKey="InventoryItem__InventoryID" TextMode="MultiLine" Enable="False"
                            Width="100%" Height="200px" >
                             <AutoSize Container="Parent" Enabled="true" />
                            </px:PXHtmlView>
                    </td>
                </tr>
                <tr>
                    <td style="min-width: 300px; height: 10px;">
                        <px:PXLabel ID="PXLabel4" runat="server" Width="100%" />
                    </td>
                    <td style="min-width: 30px; height: 10px;">
                        <px:PXLabel ID="PXLabel2" runat="server" Width="100%" />
                    </td>
                    <td style="width: 100%; height: 10px;">
                        <px:PXLabel ID="PXLabel5" runat="server" Width="100%" />
                    </td>
                </tr>
                <tr>
                    <td style="min-width: 300px">
                         <px:PXGrid ID="PXGridAnswers" runat="server" Caption="Attributes" DataSourceID="ds" Height="100px" 
                         MatrixMode="True" Width="100%" SkinID="Attributes">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="AttributeID,EntityType,EntityID" DataMember="Answers">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
                                    <px:PXTextEdit ID="edParameterID" runat="server" DataField="AttributeID" Enabled="False" />
                                    <px:PXTextEdit ID="edAnswerValue" runat="server" DataField="Value" Enabled="False"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowShowHide="False" DataField="AttributeID" TextField="AttributeID_description" TextAlign="Left" Width="170px" />
                                    <px:PXGridColumn DataField="Value" Width="130px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                    </td>
                    <td style="min-width: 30px">
                        <px:PXLabel ID="PXLabel3" runat="server" Width="100%" />
                    </td>
                    <td style="width: 100%">
                        <table runat="server" style="width: 100%">
                            <tr>
                                <td style="min-width: 100%">
                                    <px:PXLabel ID="PXLabel8" runat="server" Width="100%" />
                                </td>
                                <td style="width: 300px">
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="InventoryItemDetails.Qty" TextAlign="Right"
                                        Width="100%" />
                                </td>
                            </tr>
                            <tr>
                                <td style="min-width: 100%">
                                    <px:PXLabel ID="PXLabel9" runat="server" Width="100%" />
                                </td>
                                <td style="width: 300px">
                                    <px:PXSelector ID="edUOM" runat="server" DataField="InventoryItemDetails.UOM" TextAlign="Right"
                                        Width="100%" />
                                </td>
                            </tr>
                            <tr>
                                <td style="min-width: 100%">
                                    <px:PXLabel ID="PXLabel10" runat="server" Width="100%" />
                                </td>
                                <td style="width: 300px">
                                    <px:PXSelector ID="edWarehouse" runat="server" DataField="InventoryItemDetails.SiteID"
                                        SelectOnly="True" DisplayMode="Text" Width="100%" />
                                </td>
                            </tr>
                            <tr>
                                <td style="min-width: 100%">
                                    <px:PXLabel ID="PXLabel11" runat="server" Width="100%" />
                                </td>
                                <td style="min-width: 300px">
                                    <table runat="server" style="width: 100%">
                                        <tr>                                            
                                            <td style="width: 100%;">
                                                <px:PXLabel ID="PXLabel12" runat="server" Width="100%" />
                                            </td>
                                            <td style="width: 100px">
                                                <px:PXButton ID="PXButton2" runat="server" CommandName="AddToCart" CommandSourceID="ds"
                                                    Text="Add To Cart" Height="24" TextAlign="Right" Width="100%" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </Template>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXFormView>
</asp:Content>
