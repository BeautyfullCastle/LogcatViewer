# Gemini Project Guide: LogcatViewer

This document provides a guide for the Gemini assistant to effectively work with the LogcatViewer project.

## 1. Project Overview

LogcatViewer is a C# WPF application designed to view, filter, and search Android logcat output in real-time on a Windows machine. It interacts with Android devices using the Android Debug Bridge (ADB).

## 2. Key Technologies

- **Language:** C#
- **Framework:** Windows Presentation Foundation (WPF)
- **Platform:** .NET

## 3. Project Structure

- **`LogCatViewer.sln`**: The main Visual Studio solution file.
- **`LogCatViewer/`**: The main project directory.
  - **`LogCatViewer.csproj`**: The C# project file, contains dependencies and build settings.
  - **`MainWindow.xaml`**: The main UI layout file for the application.
  - **`MainWindow.xaml.cs`**: The code-behind for the main window. Logic is split into several partial classes, including `MainWindow.LogView.cs` which now handles saving filtered logs to a file.
    - `MainWindow.ApkInstaller.cs`
    - `MainWindow.AppManagement.cs`
    - `MainWindow.DeviceEvents.cs`
    - `MainWindow.Filtering.cs`
    - `MainWindow.LogView.cs`
    - `MainWindow.Search.cs`
    - `MainWindow.Recording.cs`
    - `MainWindow.Helpers.cs`
  - **`ApkInfo.cs`**: The data model representing an installed application. Also used for detailed APK information extracted from APK files.
  - **`LogcatManager.cs`**: Handles starting and stopping the `adb logcat` process and parsing its output. It also includes logic to limit the number of log entries to prevent excessive memory usage.
  - **`AdbWrapper.cs`**: A wrapper class for executing commands with `adb.exe` and `aapt2.exe` tools. Includes methods for installing/uninstalling APKs, taking screenshots, starting/stopping screen recordings, retrieving installed package information, and extracting detailed APK information from APK files.
  - **`LogEntry.cs`**: The data model representing a single logcat line.
  - **`adb/`**: Contains the bundled `adb.exe` and its required libraries. The application is self-contained.
  - **`aapt/`**: Contains the bundled `aapt2.exe` for parsing APK information.
  - **`ApkDetailWindow.xaml`**: The UI for displaying detailed APK information.
  - **`ApkDetailWindow.xaml.cs`**: The code-behind for the APK detail window.

## 4. How to Build and Run

### Build
The project can be built using the standard .NET CLI command from the root directory:
```bash
dotnet build LogCatViewer.sln
```

### Run
After a successful build, the application executable can be found in the `bin` folder. For a debug build, the path is typically:
```
C:\Users\user\Documents\Projects\LogcatViewer\LogCatViewer\bin\Debug\net<version>\LogCatViewer.exe
```
(Replace `<version>` with the actual .NET version used in the project).

## 5. Testing

No dedicated test project was found in the initial analysis. If a test project is added, tests can likely be run with the `dotnet test` command.
