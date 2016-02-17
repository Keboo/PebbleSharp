using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PebbleSharp.Core
{
    [DataContract]
    public class AppInfo
    {
        //does exist in sdk3
        [DataMember(Name = "versionCode", IsRequired = false)]
        public int VersionCode { get; private set; }

        [DataMember(Name = "sdkVersion", IsRequired = true)]
        public string SdkVersion { get; private set; }

        //[DataMember(Name = "capabilities", IsRequired = true)]
        //public string[] Capabilities { get; private set; }

        [DataMember(Name = "shortName", IsRequired = true)]
        public string ShortName { get; private set; }

        /// <summary> The manifest for the resources contained in this bundle. </summary>
        //resources

        //appKeys
        //[DataMember(Name = "appKeys", IsRequired = true)]
        //public Dictionary<string, int> AppKeys { get; set; }

        [DataMember(Name = "uuid", IsRequired = true)]
        public string UUID { get; private set; }

        [DataMember(Name = "versionLabel", IsRequired = true)]
        public string VersionLabel { get; private set; }

        [DataMember(Name = "longName", IsRequired = true)]
        public string LongName { get; private set; }

        //watchapp

        [DataMember(Name = "projectType", IsRequired = true)]
        public string ProjectType { get; private set; }
    }
}