<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS203000.aspx.cs" Inherits="Page_CS203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.SegmentMaint" PrimaryView="Segment">
		<CallbackCommands>
			<%--<px:PXDSCallbackCommand Name="Insert" PostData="Self" />--%>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Segment" Caption="Segment Summary" NoteField="" TemplateContainer=""
		TabIndex="22900">
		<Parameters>
			<px:PXControlParam ControlID="form" Name="Segment.DimensionID" PropertyName="NewDataKey[&quot;DimensionID&quot;]" Type="String" />
			<px:PXQueryStringParam Name="Segment.DimensionID" QueryStringField="DimensionID" Type="String" OnLoadOnly="True" />
		</Parameters>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edDimensionID" runat="server" DataField="DimensionID" AllowEdit="True" />
			<px:PXSelector ID="edSegmentID" runat="server" DataField="SegmentID" AutoRefresh="True" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="200px" Style="z-index: 100;" Width="100%" Caption="Possible Values" SkinID="Details" 
		FastFilterFields="Value,Descr" TabIndex="23700" AutoGenerateColumns="AppendDynamic" RepaintColumns="True" GenerateColumnsBeforeRepaint="True">
		<Levels>
			<px:PXGridLevel DataMember="Values">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ColumnWidth="M" />
					<px:PXMaskEdit ID="edValue" runat="server" DataField="Value" />
					<px:PXTextEdit ID="edDimensionID" runat="server" DataField="DimensionID" />
					<px:PXNumberEdit ID="edSegmentID" runat="server" DataField="SegmentID" />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
					<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
					<px:PXCheckBox ID="chkIsConsolidatedValue" runat="server" DataField="IsConsolidatedValue" />
					<px:PXTextEdit ID="edMappedSegValue" runat="server" DataField="MappedSegValue" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="DimensionID" Visible="False" AllowShowHide="False" />
					<px:PXGridColumn DataField="SegmentID" Visible="False" AllowShowHide="False" />
					<px:PXGridColumn DataField="Value" Width="100px" RenderEditorText="True" />
					<px:PXGridColumn DataField="Descr" Width="250px" RenderEditorText="True" />
					<px:PXGridColumn DataField="Active" Type="CheckBox" Width="50px" Key="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="IsConsolidatedValue" TextAlign="Center" Type="CheckBox" Width="65px" />
					<px:PXGridColumn DataField="MappedSegValue" Width="100px" RenderEditorText="True" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<EditPageParams>
			<px:PXControlParam ControlID="grid" />
		</EditPageParams>
		<Mode InitNewRow="True" AllowUpload="True" />
		<ActionBar>
			<Actions>
				<Save Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
