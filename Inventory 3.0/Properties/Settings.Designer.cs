﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Inventory_3._0.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.4.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool useNetworkPrinter {
            get {
                return ((bool)(this["useNetworkPrinter"]));
            }
            set {
                this["useNetworkPrinter"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool limitSearchResults {
            get {
                return ((bool)(this["limitSearchResults"]));
            }
            set {
                this["limitSearchResults"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool printReceipts {
            get {
                return ((bool)(this["printReceipts"]));
            }
            set {
                this["printReceipts"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool deductSalesFromInventory {
            get {
                return ((bool)(this["deductSalesFromInventory"]));
            }
            set {
                this["deductSalesFromInventory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool addTradeToInventory {
            get {
                return ((bool)(this["addTradeToInventory"]));
            }
            set {
                this["addTradeToInventory"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=tcp:TRADERJOE\\SQLEXPRESS,49172;Initial Catalog=\"C:\\PROGRAM FILES (X86" +
            ")\\INVENTORY\\STOREDATABASE.MDF\";Persist Security Info=True;User ID=inventory;Pass" +
            "word=inventory")]
        public string SQLServerConnectionString {
            get {
                return ((string)(this["SQLServerConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=tcp:TRADERJOE\\SQLEXPRESS,49172;Initial Catalog=\"StoreInventory\";Persi" +
            "st Security Info=True;User ID=inventory;Password=inventory")]
        public string SQLServerConnectionString2 {
            get {
                return ((string)(this["SQLServerConnectionString2"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=COMPY\\SQLEXPRESS;Initial Catalog=StoreInventory;Integrated Security=T" +
            "rue")]
        public string HomeInventoryConnectionString {
            get {
                return ((string)(this["HomeInventoryConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=CG-SIMP;Initial Catalog=StoreInventory;User ID=inventory;Password=inv" +
            "entory")]
        public string CgSimpConnectionString {
            get {
                return ((string)(this["CgSimpConnectionString"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool useSimpInventory {
            get {
                return ((bool)(this["useSimpInventory"]));
            }
            set {
                this["useSimpInventory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool useNashuaConnectionString {
            get {
                return ((bool)(this["useNashuaConnectionString"]));
            }
            set {
                this["useNashuaConnectionString"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=tcp:CG1,49172;Initial Catalog=StoreInventory;Persist Security Info=Tr" +
            "ue;User ID=inventory;Password=inventory")]
        public string NashuaConnectionString {
            get {
                return ((string)(this["NashuaConnectionString"]));
            }
        }
    }
}
