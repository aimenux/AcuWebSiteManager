<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204004.aspx.cs" Inherits="Pages_SM_SM204004"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">

        function initTree(a, event) {
            var tree;
            switch (event.button.key) {
                case "btnDataField": {
                    tree = __px_all(a)["ctl00_phG_tab_t0_edDataField"];
                    break;
                }
                case "btnPrevDataField": {
                    tree = __px_all(a)["ctl00_phG_tab_t0_edPrevDataField"];
                    break;
                }
            }
            if (!tree)
                return;
            tree.showDropDown();
        }

        function copyVal(src, e) {
            var selectedVal = src.tree.selectedNode.value;
            if (!selectedVal || selectedVal == "")
                return false;
            selectedVal = src.element.id == "ctl00_phG_tab_t0_edPrevDataField" ? "PREV" + selectedVal : selectedVal;
            var textEdit = __px_all(src)['ctl00_phG_tab_t0_edBody'];
            var text = textEdit.elemText.value || "";
            var before = text.substr(0, textEdit.elemText.selectionStart);
            var after = text.substr(textEdit.elemText.selectionEnd);
            textEdit.updateValue(before + selectedVal + after);
            src.oldValue = "";
        }

    </script>
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Notifications"
		TypeName="PX.BusinessProcess.UI.MobileNotificationMaint">
		<DataTrees>  
			<px:PXTreeDataMember TreeView="EntityItems" TreeKeys="Key"/>
            <px:PXTreeDataMember TreeView="ScreenUserItems" TreeKeys="Key"/>
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" DataMember="Notifications"
		Width="100%" DefaultControlID="ednotificationID">
		<AutoSize Enabled="True" Container="Window" />
		<Template>
			<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="margin: 10px; padding: 10px;">

				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
                <px:PXSelector runat="server" ID="PXSelector1" DataField="NotificationID" FilterByAllFields="True" AutoRefresh="True" TextField="Name" NullText="<NEW>" DataSourceID="ds">
					<GridProperties>
						<Columns>
							<px:PXGridColumn DataField="NotificationID" Width="60px"  />
							<px:PXGridColumn DataField="Name" Width="120px"/>
                            <px:PXGridColumn DataField="Subject" Width="220px"/>
                            <px:PXGridColumn DataField="ScreenID" Width="60px"/>
						</Columns>
					</GridProperties>
				</px:PXSelector>
                <px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="True" CommitChanges="True" />
			    <px:PXTextEdit ID="edName" runat="server" DataField="Name" AlreadyLocalized="False" DefaultLocale="" />

				<px:PXLayoutRule runat="server" ColumnSpan="2" />
				<px:PXTreeSelector ID="edNTo" runat="server" DataField="NTo" 
					TreeDataSourceID="ds" CommitChanges="True" AllowEditValue="True"
					ShowRootNode="False" MinDropWidth="468" MaxDropWidth="600"
					AppendSelectedValue="True" AutoRefresh="True" TreeDataMember="ScreenUserItems">
					<DataBindings>
						<px:PXTreeItemBinding DataMember="ScreenUserItems" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
					</DataBindings>
				</px:PXTreeSelector>

                <px:PXLayoutRule runat="server" ColumnSpan="2" />
                <px:PXSelector runat="server" DataField="NFrom" ID="edNFrom" DataSourceID="ds" AllowEditValue="false"  AutoRefresh="True"  FilterByAllFields="True"/>

                <px:PXLayoutRule runat="server" ColumnSpan="2" />
				<px:PXTreeSelector ID="edsubject" runat="server" DataField="Subject" 
					TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0"
					ShowRootNode="False" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="True"
					AppendSelectedValue="True" AutoRefresh="True" TreeDataMember="EntityItems" >
					<DataBindings>
						<px:PXTreeItemBinding DataMember="EntityItems" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
					</DataBindings>
				</px:PXTreeSelector>

                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
				<px:PXDropDown ID="edDeliveryType" runat="server" DataField="DeliveryType" CommitChanges="True" />
                <px:PXTextEdit ID="edScreenIdRO" runat="server" DataField="ScreenIdValue" AlreadyLocalized="False"/>
			    <px:PXSelector runat="server" ID="edLocale" DataField="LocaleName" DisplayMode="Text" />

				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXSelector ID="edDestinationScreenID" runat="server" DataField="DestinationScreenID"  DisplayMode="Text" FilterByAllFields="True" CommitChanges="True" />
                <px:PXTreeSelector ID="edDestinationEntityID" runat="server" DataField="DestinationEntityID"  CommitChanges="True" AllowEditValue="True"
					TreeDataSourceID="ds" TreeDataMember="EntityItems" MinDropWidth="413" PopulateOnDemand="True" ShowRootNode="False"
                    AppendSelectedValue="False" AutoRefresh="True" >
					<DataBindings>
						<px:PXTreeItemBinding DataMember="EntityItems" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
					</DataBindings>
				</px:PXTreeSelector>
			    
			</px:PXPanel> 
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="100%" Style="z-index: 100" Width="100%"
		DataSourceID="ds" DataMember="CurrentNotification">
		<Items>
			<px:PXTabItem Text="Message">
				<Template>
                   <px:PXToolBar ID="textAreaBtns" runat="server" SkinID="">
				        <Items>
					        <px:PXToolBarButton Key="btnDataField" Text="Insert Data Field">
					        </px:PXToolBarButton>
                            <px:PXToolBarButton Key="btnPrevDataField" Text="Insert Previous Data Field">
					        </px:PXToolBarButton>
				        </Items>
                       <ClientEvents ButtonClick="initTree" />
			        </px:PXToolBar>

                    <px:PXTreeSelector ID="edDataField" runat="server" DataField=""  style="visibility:hidden; height: 0px; opacity:0;"
				        TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0"
				        ShowRootNode="false" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="false" AppendSelectedValue="false" 
					        AutoRefresh="true" TreeDataMember="EntityItems" Size="M" >
                        <ClientEvents ValueChanged="copyVal" />
				        <DataBindings>
					        <px:PXTreeItemBinding DataMember="EntityItems" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
				        </DataBindings>
			        </px:PXTreeSelector>
               
                    <px:PXTreeSelector ID="edPrevDataField" runat="server" DataField="" style="visibility:hidden;height: 0px; opacity:0;"
				        TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0"
				        ShowRootNode="false" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true"
				        AppendSelectedValue="true" AutoRefresh="true" TreeDataMember="EntityItems" Size="M">
                        <ClientEvents ValueChanged="copyVal" />
				        <DataBindings>
					        <px:PXTreeItemBinding DataMember="EntityItems" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
				        </DataBindings>
			        </px:PXTreeSelector>

                    <px:PXTextEdit ID="edBody" runat="server" DataField="Body" AlreadyLocalized="False" TextMode="MultiLine"
                        SuppressLabel="true" Width="100%" SelectOnFocus="false" >
                        <AutoSize Enabled="true" MinHeight="216" />    
                    </px:PXTextEdit>
				</Template>
			</px:PXTabItem>
        </Items>
        <AutoSize Container="Window" MinHeight="250" MinWidth="300" Enabled="True"  />
    </px:PXTab>
</asp:Content>

