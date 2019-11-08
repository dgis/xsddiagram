using System.ComponentModel;

namespace XSDDiagram {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }

        [BrowsableAttribute(false)]
        [Category("UI"), DisplayName("Display Right Panel"), Description("Display the right panel.")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DisplayPanel
        {
            get
            {
                return ((bool)(this["DisplayPanel"]));
            }
            set
            {
                this["DisplayPanel"] = value;
            }
        }

        [BrowsableAttribute(false)]
        [Category("Diagram"), DisplayName("Zoom"), Description("The zoom level.")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public int Zoom
        {
            get
            {
                return ((int)(this["Zoom"]));
            }
            set
            {
                this["Zoom"] = value;
            }
        }

        [BrowsableAttribute(false)]
        [Category("Diagram"), DisplayName("Alignement"), Description("The item alignement.")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int Alignement {
            get {
                return ((int)(this["Alignement"]));
            }
            set {
                this["Alignement"] = value;
            }
        }

        [BrowsableAttribute(false)]
        [Category("Diagram"), DisplayName("Show Documentation"), Description("Show the documentation under the item.")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowDocumentation {
            get {
                return ((bool)(this["ShowDocumentation"]));
            }
            set {
                this["ShowDocumentation"] = value;
            }
        }

        [Category("Diagram"), DisplayName("Always Show Occurence"), Description("Always show the occurence of an item even if it is different from 1..1.")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AlwaysShowOccurence {
            get {
                return ((bool)(this["AlwaysShowOccurence"]));
            }
            set {
                this["AlwaysShowOccurence"] = value;
            }
        }

        [Category("Diagram"), DisplayName("Show Type"), Description("Show the type of an item if it exist.")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowType {
            get {
                return ((bool)(this["ShowType"]));
            }
            set {
                this["ShowType"] = value;
            }
        }

        [Category("Diagram"), DisplayName("CompactLayoutDensity"), Description("Display the diagram with a compact density.")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool CompactLayoutDensity
        {
            get
            {
                return ((bool)(this["CompactLayoutDensity"]));
            }
            set
            {
                this["CompactLayoutDensity"] = value;
            }
        }
    }
}
