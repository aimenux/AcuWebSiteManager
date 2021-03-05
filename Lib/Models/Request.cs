using System.Text.Json.Serialization;

namespace Lib.Models
{
    public class Request
    {
        [JsonIgnore]
        public string Password { get; set; }

        public string ConfigXmlFile { get; set; }

        public string ConfigExeFile { get; set; }

        public string ConfigExeArguments { get; set; }

        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        [JsonIgnore]
        public string DatabaseUserName { get; set; }

        [JsonIgnore]
        public string DatabasePassword { get; set; }

        public string SwitchDatabaseName { get; set; }

        public string BacPacFilePath { get; set; }

        public string AppPoolName { get; set; }

        public string RootDirectory { get; set; }

        public string SiteDirectoryName { get; set; }

        public string SiteDirectoryPath { get; set; }

        public string SiteVirtualDirectoryName { get; set; }

        [JsonIgnore]
        public string[] DefaultDirectories { get; set; }

        [JsonIgnore]
        public string[] AbsoluteDirectories { get; set; }
    }
}