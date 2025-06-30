using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LogcatViewer
{
    public static class AdbWrapper
    {
        public static readonly string AdbPath;
        private static readonly string AaptPath;

        static AdbWrapper()
        {
            string? assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(assemblyLocation))
            {
                throw new DirectoryNotFoundException("Assembly location not found.");
            }

            AdbPath = Path.Combine(assemblyLocation, "adb", "adb.exe");
            if (!File.Exists(AdbPath)) throw new FileNotFoundException("adb.exe를 찾을 수 없습니다!", AdbPath);
            
            AaptPath = Path.Combine(assemblyLocation, "aapt", "aapt2.exe");
            if (!File.Exists(AaptPath)) throw new FileNotFoundException("aapt2.exe를 찾을 수 없습니다! (libwinpthread-1.dll도 함께 복사했는지 확인해주세요)", AaptPath);
        }

        public static string ExecuteCommand(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(AdbPath, command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (Process? process = Process.Start(processInfo))
            {
                if (process == null)
                {
                    return "프로세스를 시작할 수 없습니다.";
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    return error;
                }
                return output;
            }
        }
        
        private static string ExecuteCommandWithTimeout(string command, int timeoutMilliseconds)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(AdbPath, command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (Process? process = Process.Start(processInfo))
            {
                if (process == null) return "프로세스를 시작할 수 없습니다.";

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                bool exited = process.WaitForExit(timeoutMilliseconds);

                if (!exited)
                {
                    process.Kill();
                    return "작업 시간 초과 (Timeout)";
                }

                if (!string.IsNullOrEmpty(error))
                {
                    return error;
                }
                return output;
            }
        }

        public static List<string> GetConnectedDevices()
        {
            string output = ExecuteCommand("devices");
            var devices = new List<string>();
            var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                              .Skip(1);
            foreach (var line in lines)
            {
                var parts = line.Split('\t');
                if (parts.Length == 2 && parts[1] == "device")
                {
                    devices.Add(parts[0]);
                }
            }
            return devices;
        }

        public static string InstallApk(string deviceSerial, string apkPath)
        {
            string command = $"-s {deviceSerial} install -r -d \"{apkPath}\"";
            return ExecuteCommandWithTimeout(command, 300000); 
        }

        public static string? GetPackageNameFromApk(string apkPath)
        {
            string command = $"dump packagename \"{apkPath}\"";
            string output = ExecuteAaptCommand(command);

            if (!string.IsNullOrWhiteSpace(output))
            {
                return output.Trim();
            }
            
            return null;
        }

        public static string UninstallPackage(string deviceSerial, string packageName)
        {
            string command = $"-s {deviceSerial} uninstall {packageName}";
            return ExecuteCommandWithTimeout(command, 60000);
        }

        public static string? TakeScreenshotAndSaveToFile(string deviceSerial, string savePath)
        {
            string devicePath = "/sdcard/screenshot.png";
            string screenshotCommand = $"-s {deviceSerial} shell screencap -p {devicePath}";
            string pullCommand = $"-s {deviceSerial} pull \"{devicePath}\" \"{savePath}\"";
            string cleanupCommand = $"-s {deviceSerial} shell rm {devicePath}";

            string result = ExecuteCommand(screenshotCommand);
            if (result.Contains("error")) return $"스크린샷 캡처 실패: {result}";

            result = ExecuteCommand(pullCommand);
            if (result.Contains("error")) return $"이미지 파일 가져오기 실패: {result}";
            
            ExecuteCommand(cleanupCommand);

            return null; // 성공
        }

        public static string? TakeScreenshotToTempFile(string deviceSerial, out string? tempFilePath)
        {
            tempFilePath = null;
            string devicePath = "/sdcard/screenshot.png";
            tempFilePath = Path.GetTempFileName();
            // Change extension to .png
            string newTempFilePath = Path.ChangeExtension(tempFilePath, ".png");
            File.Move(tempFilePath, newTempFilePath);
            tempFilePath = newTempFilePath;

            string screenshotCommand = $"-s {deviceSerial} shell screencap -p {devicePath}";
            string pullCommand = $"-s {deviceSerial} pull \"{devicePath}\" \"{tempFilePath}\"";
            string cleanupCommand = $"-s {deviceSerial} shell rm {devicePath}";

            string result = ExecuteCommand(screenshotCommand);
            if (result.Contains("error")) return $"스크린샷 캡처 실패: {result}";

            result = ExecuteCommand(pullCommand);
            if (result.Contains("error")) return $"이미지 파일 가져오기 실패: {result}";
            
            ExecuteCommand(cleanupCommand);

            return null; // 성공
        }

        public static Process? StartScreenRecordingProcess(string deviceSerial, string devicePath)
        {
            string command = $"-s {deviceSerial} shell screenrecord {devicePath}";
            ProcessStartInfo processInfo = new ProcessStartInfo(AdbPath, command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            return Process.Start(processInfo);
        }

        public static string PullFile(string deviceSerial, string devicePath, string localPath)
        {
            string command = $"-s {deviceSerial} pull \"{devicePath}\" \"{localPath}\"";
            return ExecuteCommand(command);
        }

        public static string DeleteFile(string deviceSerial, string devicePath)
        {
            string command = $"-s {deviceSerial} shell rm \"{devicePath}\"";
            return ExecuteCommand(command);
        }

        public static ApkInfo? GetApkDetailInfo(string apkPath)
        {
            if (!File.Exists(apkPath))
            {
                return null;
            }

            string command = $"dump badging \"{apkPath}\"";
            string output = ExecuteAaptCommand(command);

            ApkInfo apkInfo = new ApkInfo();

            // Package Name
            Match match = Regex.Match(output, "package: name='([^']+)'");
            if (match.Success) apkInfo.PackageName = match.Groups[1].Value;

            // Version Name
            match = Regex.Match(output, "versionName='([^']+)'");
            if (match.Success) apkInfo.VersionName = match.Groups[1].Value;

            // Version Code
            match = Regex.Match(output, "versionCode='([^']+)'");
            if (match.Success) apkInfo.VersionCode = match.Groups[1].Value;

            // Application Label (App Name)
            match = Regex.Match(output, "application: label='([^']+)'");
            if (match.Success) apkInfo.AppLabel = match.Groups[1].Value;

            // Icon Path
            match = Regex.Match(output, "application-icon-[0-9]+:'([^']+)'");
            if (match.Success) apkInfo.IconPath = match.Groups[1].Value;

            // Min SDK Version
            match = Regex.Match(output, "sdkVersion:'([^']+)'");
            if (match.Success) apkInfo.MinSdkVersion = match.Groups[1].Value;

            // Target SDK Version
            match = Regex.Match(output, "targetSdkVersion:'([^']+)'");
            if (match.Success) apkInfo.TargetSdkVersion = match.Groups[1].Value;

            // Permissions (simplified, just listing them)
            StringBuilder permissions = new StringBuilder();
            foreach (Match permMatch in Regex.Matches(output, "uses-permission: name='([^']+)'"))
            {
                permissions.AppendLine(permMatch.Groups[1].Value);
            }
            apkInfo.Permissions = permissions.ToString().Trim();

            return apkInfo;
        }

        private static string ExecuteAaptCommand(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(AaptPath, command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (Process? process = Process.Start(processInfo))
            {
                if (process == null) return "aapt2 프로세스를 시작할 수 없습니다.";
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }
    }
}