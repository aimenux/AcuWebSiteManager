<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS202000.aspx.cs" Inherits=" Page_CS202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.DimensionMaint" PrimaryView="Header">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Delete" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Previous" PostData="Self" />
			<px:PXDSCallbackCommand Name="Next" PostData="Self" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="viewSegment" DependOnGrid="grid" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Header" Caption="Segmented Key Definition" TemplateContainer="" DataSourceID="ds" >
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edDimensionID" runat="server" DataField="DimensionID" />
			<px:PXSelector ID="edParentDimensionID" runat="server" DataField="ParentDimensionID" Enabled="False" />
		    <px:PXDropDown ID="edLookupMode" runat="server" DataField="LookupMode" CommitChanges="true"/>
			<px:PXCheckBox ID="chkValidate" runat="server" DataField="Validate" />
			<px:PXSelector ID="edSpecificModule" runat="server" DataField="SpecificModule" />
			<px:PXSelector ID="edNumberingID" runat="server" DataField="NumberingID" AllowEdit="True" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="S" LabelsWidth="S" />
            <px:PXNumberEdit ID="edMaxLength" runat="server" DataField="MaxLength" Enabled="False" />
			<px:PXNumberEdit ID="edLength" runat="server" DataField="Length" Enabled="False" />
			<px:PXNumberEdit ID="edSegments" runat="server" DataField="Segments" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="322px" Style="z-index: 100" Width="100%" Caption="Segment Definition" AllowSearch="true" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Detail">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
					<px:PXNumberEdit ID="edSegmentID" runat="server" DataField="SegmentID" />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
					<px:PXNumberEdit ID="edLength" runat="server" DataField="Length" />
					<px:PXCheckBox ID="chkValidate" runat="server" DataField="Validate" />
					<px:PXCheckBox ID="chkAutoNumber" runat="server" DataField="AutoNumber" />
					<px:PXTextEdit ID="edSeparator" runat="server" DataField="Separator" />
					<px:PXNumberEdit ID="edConsolOrder" runat="server" DataField="ConsolOrder" />
					<px:PXNumberEdit ID="edConsolNumChar" runat="server" DataField="ConsolNumChar" />
					<px:PXCheckBox ID="chkIsCosted" runat="server" DataField="IsCosted" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="SegmentID" Width="100px" AllowUpdate="False" LinkCommand="viewSegment" />
					<px:PXGridColumn DataField="Descr" Width="100px" AutoCallBack="true" />
					<px:PXGridColumn DataField="Inherited" AllowShowHide="False" Visible="false" Type="CheckBox" />
					<px:PXGridColumn DataField="IsOverrideForUI" Width="60px" AllowShowHide="Server" Type="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="Length" Width="100px" TextAlign="Right" CommitChanges="True" />
					<px:PXGridColumn DataField="Align" Type="DropDownList" Width="100px" />
					<px:PXGridColumn DataField="EditMask" Type="DropDownList" Width="100px" AutoCallBack="true" />
					<px:PXGridColumn DataField="CaseConvert" Type="DropDownList" Width="100px" />
					<px:PXGridColumn DataField="Validate" TextAlign="Center" Type="CheckBox" Width="60px" />
					<px:PXGridColumn DataField="IsCosted" TextAlign="Center" Type="CheckBox" Width="40px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="AutoNumber" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="Separator" Width="100px" />
					<px:PXGridColumn DataField="PromptCharacter" Width="100px" AllowShowHide="True" />
					<px:PXGridColumn DataField="ConsolOrder" TextAlign="Right" Width="54px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="ConsolNumChar" TextAlign="Right" Width="54px" AllowShowHide="Server" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Key="cmdView" CommandName="viewSegment" CommandSourceID="ds" Text="View Segment" />
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
