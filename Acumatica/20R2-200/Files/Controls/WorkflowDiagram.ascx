<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowDiagram.ascx.cs" Inherits="Controls_WorkflowDiagram" %>
<%@ Import Namespace="PX.Data" %>

<px:PXSmartPanel ID="pnlWorkFlow" runat="server" CaptionVisible="True" DesignView="Hidden" Height="600" Key="WorkflowView"
                 Width="800" Caption="State diagram" AutoRepaint="True" ShowMaximizeButton="True"
                 AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel"
                 AutoCallBack-Enabled="true" AutoCallBack-Target="WorkflowFictive" AutoCallBack-Command="Refresh">
    <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButtonCustomize" runat="server"  Text="Customize" >
            <AutoCallBack Target="DsControlProps" Command="menuCustomizeWorkflow"/>
        </px:PXButton>
    </px:PXPanel>
    <px:PXFormView ID="WorkflowFictiveDiagram" runat="server" Width="100%"
                   AutoSize="True"
                   SkinID="Transparent"
                   DataMember="WorkflowView"
                   AutoRepaint="True">
        <Template>
            <px:PXWorkflowDiagram runat="server" Enabled="False" ID="diagram" DataField="Layout" AutoSize="True"></px:PXWorkflowDiagram>
        </Template>
        <AutoSize Container="Parent" Enabled="True" MinHeight="400" />
    </px:PXFormView>
    
</px:PXSmartPanel>
