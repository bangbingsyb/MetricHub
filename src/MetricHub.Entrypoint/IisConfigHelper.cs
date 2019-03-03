using Microsoft.Web.Administration;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MetricHub.Entrypoint
{
    public static class IisConfigHelper
    {
        private static IDictionary<string, string> _envFilter = null;

        public static void UpdateEnvironmentVariables()
        {
            SetEnvironmentVariablesToConfig(GetEnvironmentVariables());
        }

        private static IDictionary<string, string> GetEnvironmentVariables()
        {
            InitEnvironmentVariableFilter();

            IDictionary envDict = Environment.GetEnvironmentVariables();
            var envDictRet = new Dictionary<string, string>();

            foreach (DictionaryEntry ev in envDict)
            {
                // Windows environment variable names are case-insensitive.
                if (_envFilter.TryGetValue(((string)ev.Key).ToUpper(), out string value))
                {
                    if (value == null || string.Compare(value, (string)ev.Value, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }
                }

                envDictRet[(string)ev.Key] = (string)ev.Value;
            }

            return envDictRet;
        }

        private static void InitEnvironmentVariableFilter()
        {
            if (_envFilter == null)
            {
                _envFilter = new Dictionary<string, string>()
                {
                    { "ALLUSERSPROFILE", "C:\\ProgramData" },
                    { "APPDATA", "C:\\Users\\ContainerAdministrator\\AppData\\Roaming" },
                    { "COMMONPROGRAMFILES", null },
                    { "COMMONPROGRAMFILES(X86)", null },
                    { "COMMONPROGRAMW6432", null },
                    { "COMPUTERNAME", null },
                    { "COMSPEC", null },
                    { "LOCALAPPDATA", "C:\\Users\\ContainerAdministrator\\AppData\\Local" },
                    { "NUMBER_OF_PROCESSORS", null },
                    { "OS", null },
                    { "PATH", "C:\\Windows\\system32;C:\\Windows;C:\\Windows\\System32\\Wbem;C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\;C:\\Users\\ContainerAdministrator\\AppData\\Local\\Microsoft\\WindowsApps" },
                    { "PATHEXT", ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC" },
                    { "PROCESSOR_ARCHITECTURE", null },
                    { "PROCESSOR_IDENTIFIER", null },
                    { "PROCESSOR_LEVEL", null },
                    { "PROCESSOR_REVISION", null },
                    { "PROGRAMDATA", "C:\\ProgramData" },
                    { "PROGRAMFILES", null },
                    { "PROGRAMFILES(X86)", null },
                    { "PROGRAMW6432", null },
                    { "PSMODULEPATH", "%ProgramFiles%\\WindowsPowerShell\\Modules;C:\\Windows\\system32\\WindowsPowerShell\\v1.0\\Modules" },
                    { "PUBLIC", "C:\\Users\\Public" },
                    { "SYSTEMDRIVE", null },
                    { "SYSTEMROOT", null },
                    { "TEMP", "C:\\Users\\ContainerAdministrator\\AppData\\Local\\Temp" },
                    { "TMP", "C:\\Users\\ContainerAdministrator\\AppData\\Local\\Temp" },
                    { "USERDOMAIN", "User Manager" },
                    { "USERNAME", "ContainerAdministrator" },
                    { "USERPROFILE", "C:\\Users\\ContainerAdministrator" },
                    { "WINDIR", null },
                };
            }
        }

        private static void SetEnvironmentVariablesToConfig(IDictionary<string, string> envDict)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationSection applicationPoolsSection = config.GetSection("system.applicationHost/applicationPools");

                ConfigurationElement applicationPoolDefaultsElement = applicationPoolsSection.GetChildElement("applicationPoolDefaults");

                ConfigurationElementCollection environmentVariablesCollection = applicationPoolDefaultsElement.GetCollection("environmentVariables");

                foreach (var env in envDict)
                {
                    ConfigurationElement addElement = environmentVariablesCollection.CreateElement("add");
                    addElement["name"] = env.Key;
                    addElement["value"] = env.Value;
                    environmentVariablesCollection.Add(addElement);
                }

                serverManager.CommitChanges();
            }
        }
    }
}
