using System;
using System.Collections.Generic;
using System.IO;

namespace {PluginID}.PublicInfos
{
    public class AppConfig : ConfigBase
    {
        public AppConfig(string path)
            : base(path)
        {
            LoadConfig();
            Instance = this;
        }

        public static AppConfig Instance { get; private set; }

        public static string CommandMenu { get; set; } = "";

        public override void LoadConfig()
        {
            CommandMenu = GetConfig("CommandMenu", "#菜单");
        }
    }
}