<%@ Page Title="Code Editor" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
    AutoEventWireup="true" CodeFile="AU220010.aspx.cs" Inherits="Page_AU220010"
    EnableViewStateMac="False" EnableViewState="False" ValidateRequest="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
    <style type="text/css">
        .CodeMirror-wrapping {
            background-color: White;
            border: 1px solid gray;
            margin: 0px;
            border-right-width: 0px;
            border-left-width: 0px;
        }

        .CodeMirror-line-numbers {
            font-family: monospace;
            font-size: 10pt;
            color: gray;
            padding: .4em;
            background-color: #eee;
            border-right: 1px solid gray;
            text-align: right;
            padding-left: 15pt;
        }

        [data-class=invisible] {
            display: none;
        }

        html, [name="aspnetForm"] {
            height: 100%
        }

        .phF {
            height: calc(100% - 87px);
        }

        #SourcePlaceholder, #ResultPlaceholder {
            height: calc(100% - 37px);
        }

        .fog {
            filter: opacity(0.7);
        }

        #ctl00_phF_FormEditContent_Errors {
            resize: none;
        }

        #ctl00_phF_FormEditContent_s0 {
            display: block !important;
        }

        textarea[id*=Errors] { 
            resize: none;
        }

        [id$=PXFormView5] {
            border-top: 1px solid #D2D4D7;
            border-bottom: 1px solid #D2D4D7;
        }

    </style>
    <script type="text/javascript" language="javascript">
        var IsCodeEditorWindow = true;
        function FireResize() {
            px_cm.notifyOnResize();
        }
    </script>
    <script src='<%=GetScriptName("codemirror.js")%>' type="text/javascript">
    </script>
    <script type="text/javascript" language="javascript">

        window.IsEditorActive = false;
        window.proxy = null;

        function ActivateCsEditor(editor) {
            var isFirstEditor = editor.ID.indexOf("EventEditBox") > -1;
            var id = isFirstEditor ? "SourcePlaceholder" : "ResultPlaceholder";
            var isReadOnly = editor.getReadOnly();
            document.getElementById(id).className = isReadOnly ? 'fog' : '';
            setTimeout(function () {
                var objId = id + "_obj";
                if (window[objId]) return;
                window[objId] = new EditorWrapper(editor, id, isReadOnly);
            }, 0)
        }

        function EditorWrapper(editor, divId, isReadOnly) {
            this.init = function () {
                if (this.proxy.initialValue) this.proxy.setCode(this.proxy.initialValue);

                if (this.readOnly) return;
                var cm = __px_cm(editor);
                var ds = cm && cm.findDataSource();
                if (!ds) {
                    setTimeout(this.init.bind(this), 100);
                    return;
                }

                this.proxy.win.document.documentElement.addEventListener('keydown', function (e) {
                    if ((e.keyCode == 83 || e.keyCode == 115) && (e.ctrlKey)) {
                        ds.pressSave();
                        __px(ds).cancelEvent(e);
                        return false;
                    }

                    if (e.keyCode == 32 && e.ctrlKey) {
                        window.top.DoPublish();
                        return false;
                    }

                    if (e.keyCode == 27) {
                        ds.executeCallback('Cancel');
                        return false;
                    }

                    switch (e.keyCode) {
                        case 8: case 46: case 13:
                            ds.setClientChanged();
                            break;
                    }
                });
                this.proxy.win.document.documentElement.addEventListener('keypress', function (e) {
                    if ((e.charCode == 115 || e.charCode == 83) && (e.ctrlKey || e.metaKey)) {
                        ds.pressSave();
                        __px(ds).cancelEvent(e);
                        return false;
                    }

                    ds.setClientChanged();
                });

                this.proxy.editor.subscribe('paste', function (e) {
                    ds.setClientChanged();
                });
                this.proxy.editor.subscribe('keydown', function (e) {
                    if (e.keyCode == 86 && e.ctrlKey && !e.altKey && !e.shiftKey)
                        ds.setClientChanged();
                });

            }
            var config = {
                parserfile: [
                    "<%=GetScriptName("tokenizemsdl.js")%>",
                    "<%=GetScriptName("parsemsdl.js")%>"
                ],

                stylesheet: "<%=GetScriptName("msdlcolors.css")%>",
                height: "100%",
                autoMatchParens: true,
                textWrapping: false,
                lineNumbers: true,
                tabMode: "shift",
                enterMode: "keep",
                content: editor.getValue(),
                basefiles: [
                    "<%=GetScriptName("util.js")%>",
                    "<%=GetScriptName("stringstream.js")%>",
                    "<%=GetScriptName("select.js")%>",
                    "<%=GetScriptName("undo.js")%>",
                    "<%=GetScriptName("editor.js")%>",
                    "<%=GetScriptName("tokenize.js")%>"],
                initCallback: this.init.bind(this)
            };
            if (isReadOnly) config.readOnly = this.readOnly = true;

            var p = document.getElementById(divId);

            var replace = function (newElement) {
                p.appendChild(newElement);
            };
            this.proxy = new CodeMirror(replace, config);
            var that = this;
            editor.onCallback = function () {
                if (that.proxy.editor != null) {
                    var code = that.proxy.getCode();
                    editor.updateValue(code);

                }
            };

            editor.baseRepaintText = editor.repaintText;
            editor.repaintText = function (v) {

                if (v === null || v === undefined)
                    v = "";
                if (that.proxy.editor) that.proxy.setCode(v); else that.proxy.initialValue = v;
                editor.baseRepaintText(v);

            };

            that.proxy.wrapping.id = "csproxywrapping";
        }

    </script>

    <px:PXFormView runat="server" SkinID="transparent" ID="formTitle"
        DataSourceID="ds" DataMember="ViewPageTitle" Width="100%">
        <Template>
            <px:PXTextEdit runat="server" ID="PageTitle" DataField="PageTitle" SelectOnFocus="False"
                SkinID="Label" SuppressLabel="true"
                Width="90%"
                Style="padding: 10px">
                <font size="14pt" names="Arial,sans-serif;" />
            </px:PXTextEdit>
        </Template>
    </px:PXFormView>

    <px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.SM.MobileSiteMapEditor"
        PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSplitContainer ID="splt1" runat="server" Height="100%" PositionInPercent="true" SplitterPosition="60">
        <Template1>
            <px:PXSplitContainer ID="splt2" runat="server" Height="100%" PositionInPercent="true" SplitterPosition="70"  FixedPanel="Panel2" Panel2MinSize="200" SkinID="Horizontal">
                <Template1>
                    <px:PXFormView ID="PXFormView2" runat="server" DataMember="Filter" DataSourceID="ds" SkinID="transparent">
                        <Template>
                            <px:PXTextEdit
                                runat="server"
                                ID="CommandsLabel"
                                SelectOnFocus="False"
                                SkinID="Label"
                                SuppressLabel="true"
                                Style="padding: 10px"
                                Text="Commands:">
                            </px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                    <div id="SourcePlaceholder" style="width: 100%;"></div>
                    <px:PXFormView ID="PXFormView3" runat="server" DataMember="MobileSiteMaps" DataSourceID="ds" SkinID="transparent">
                        <AutoSize Enabled="true" Container="Parent" MinWidth="300" />
                        <Template>
                            <px:PXTextEdit
                                SuppressLabel="True"
                                data-class="invisible"
                                Height="20px"
                                Style="display: none"
                                ID="EventEditBox"
                                runat="server"
                                DataField="Script"
                                TextMode="MultiLine"
                                Font-Names="Courier New"
                                Font-Size="10pt"
                                Wrap="False"
                                SelectOnFocus="False">
                                <ClientEvents Initialize="ActivateCsEditor" />
                            </px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                </Template1>
                <Template2>
                    <px:PXFormView ID="PXFormView5" runat="server" DataMember="Filter" DataSourceID="ds" SkinID="transparent">
                        <Template>
                            <px:PXTextEdit
                                runat="server"
                                ID="ErrorsLabel"
                                SelectOnFocus="False"
                                SkinID="Label"
                                SuppressLabel="true"
                                Style="padding: 10px"
                                Text="Errors:">
                            </px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="PXFormView7" runat="server" DataMember="MobileSiteMaps" DataSourceID="ds" SkinID="transparent">
                        <AutoSize Enabled="true" Container="Parent" MinWidth="300" />
                        <Template>
                            <px:PXTextEdit
                                SuppressLabel="True"
                                ID="Errors"
                                runat="server"
                                DataField="Errors"
                                TextMode="MultiLine"
                                Font-Names="Courier New"
                                Font-Size="10pt"
                                SelectOnFocus="False"
                                Height="100px"
                                Width="100%"
                                Enabled="false">
                                <AutoSize Enabled="true" />
                            </px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                </Template2>
            </px:PXSplitContainer>
        </Template1>
        <Template2>
            <px:PXFormView ID="PXFormView6" runat="server" DataMember="MobileSiteMaps" DataSourceID="ds" SkinID="transparent">
                <Template>
                    <px:PXTextEdit
                        runat="server"
                        ID="ResultsLabel"
                        SelectOnFocus="False"
                        SkinID="Label"
                        SuppressLabel="true"
                        Style="padding: 10px"
                        Text="Result Preview:">
                    </px:PXTextEdit>
                </Template>
            </px:PXFormView>
            <div id="ResultPlaceholder" style="width: 100%;"></div>
            <px:PXFormView ID="PXFormView4" runat="server" DataMember="MobileSiteMaps" DataSourceID="ds" SkinID="transparent">
                <AutoSize Enabled="true" Container="Parent" MinWidth="300" />
                <Template>
                    <px:PXTextEdit
                        SuppressLabel="True"
                        data-class="invisible"
                        Height="20px"
                        ID="ResultEditBox"
                        runat="server"
                        DataField="PreviewResult"
                        TextMode="MultiLine"
                        Font-Names="Courier New"
                        Font-Size="10pt"
                        Wrap="False"
                        SelectOnFocus="False">
                        <ClientEvents Initialize="ActivateCsEditor" />
                    </px:PXTextEdit>
                </Template>
            </px:PXFormView>
        </Template2>
    </px:PXSplitContainer>
</asp:Content>
