namespace LogcatViewer
{
    public class ApkInfo
    {
        public string PackageName { get; set; }
        public string AppLabel { get; set; }
        public string VersionName { get; set; }
        public string VersionCode { get; set; }
        public string IconPath { get; set; }
        public string MinSdkVersion { get; set; }
        public string TargetSdkVersion { get; set; }
        public string Permissions { get; set; }

        public ApkInfo()
        {
            PackageName = "";
            AppLabel = "";
            VersionName = "";
            VersionCode = "";
            IconPath = "";
            MinSdkVersion = "";
            TargetSdkVersion = "";
            Permissions = "";
        }
    }
}