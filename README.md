### English

# 🚀 C# Logcat Viewer & Tools

**The Android log analysis and management tool.**

## ✅ Key Features

### 🖥️ Real-time Log Viewer

*   **Automatic Device Detection**: Connect a device with USB debugging enabled, and it will be detected in real-time, opening a new tab automatically.
*   **Multi-Device Support**: Connect multiple devices simultaneously and view logs from each device in separate tabs.
*   **Intelligent Log Grouping (Folding)**: Automatically groups complex error stack traces or multi-line logs into a single, foldable entry.
*   **Performance Optimized**: Ensures a smooth user experience even with thousands of logs per second by collecting logs in the background and updating the UI in batches.
*   **Flexible Layout**: The 'Message' column width adjusts automatically when the window is resized for optimal readability, with horizontal scrolling also supported.

### 🛠️ Powerful Utilities

*   **Real-time Filtering**: Filter logs in real-time by level (Verbose, Debug, Info, Warning, Error) and specific text content.
*   **Real-time Search & Highlighting**: Instantly highlights all log lines containing the search keyword, making them easy to spot.
*   **Intelligent Auto-Scroll**: Keeps the view at the latest log but smartly pauses when you scroll up to check older entries, so you don't lose your place.
*   **Perfect Clipboard Copy**: Copy logs using `Ctrl+C` or the context menu. Grouped/folded logs are copied completely, including all hidden lines.
*   **Clear Logs**: Cleanly clear all logs in the current tab with a single click.
*   **Log Truncation**: Limits the number of log entries in `LogcatManager.cs` to optimize memory usage.
*   **Save Filtered Logs**: Added functionality to save currently filtered logs to a text file.
*   **Screenshot**: Capture a screenshot of the connected Android device.
*   **Screen Recording**: Record the screen of the connected Android device.

### 📲 APK Installer

*   **Versatile File Selection**: Drag and drop an APK file onto the window or click to open a file selection dialog.
*   **Parallel Installation**: Install an APK on all connected devices simultaneously, drastically reducing setup time.
*   **Advanced Install Options**: Includes `aapt2` to automatically extract the package name from the APK, enabling a flawless "Uninstall and Reinstall" feature.
*   **Real-time Status Indicators**: Each device tab displays the installation status (In Progress, Success, Failure) with a color-coded bar for at-a-glance monitoring.
*   **APK Detailed Information Viewer**: View detailed information about an APK file (e.g., package name, version, permissions) before installation.

## 🛠️ Core Technologies

*   **C#**
*   **.NET 8**
*   **WPF (Windows Presentation Foundation)**

## ⚙️ How to Build

1.  Open a terminal in the solution's root folder.
2.  Run the following command:
    ```bash
    dotnet publish -c Release -r win-x64 --self-contained true
    ```
    *(Requires .NET 8 SDK)*
3.  Find and run `LogCatViewer.exe` in the following directory:
    `.\LogCatViewer\bin\Release\net8.0-windows\win-x64\publish`

---

### 한국어 (Korean)

# 🚀 C# Logcat Viewer & Tools

**안드로이드 로그 분석 및 관리 도구입니다.**

## ✅ 주요 기능 (Key Features)

### 🖥️ 실시간 로그 뷰어 (Real-time Log Viewer)

*   **자동 기기 감지**: USB 디버깅이 활성화된 기기를 연결하면, 별도의 조작 없이 실시간으로 감지하여 새 탭을 생성합니다.
*   **멀티-디바이스 지원**: 여러 대의 기기를 동시에 연결하고, 각 기기의 로그를 개별 탭에서 확인할 수 있습니다.
*   **지능형 로그 그룹화 (폴딩 기능)**: 복잡한 에러 스택 트레이스나 여러 줄에 걸친 로그를 자동으로 하나의 그룹으로 묶어주며, 클릭하여 접거나 펼 수 있습니다.
*   **성능 최적화**: 초당 수백, 수천 라인의 로그가 쏟아져도 UI가 멈추지 않도록, 백그라운드에서 로그를 수집하고 UI에는 일괄 처리(Batching) 방식으로 업데이트하여 부드러운 사용 경험을 제공합니다.
*   **유연한 레이아웃**: 창 크기를 조절하면 '메시지' 컬럼의 너비가 자동으로 변경되어 항상 최적의 가독성을 유지하며, 필요시 가로 스크롤도 지원합니다.

### 🛠️ 강력한 유틸리티 (Powerful Utilities)

*   **실시간 필터링**: 로그 레벨(Verbose, Debug, Info, Warning, Error)과 특정 텍스트를 기준으로 보고 싶은 로그만 실시간으로 필터링할 수 있습니다.
*   **실시간 검색 및 하이라이팅**: 검색창에 키워드를 입력하면, 모든 로그 중에서 해당 키워드를 포함한 라인의 배경색을 즉시 변경하여 눈에 잘 띄게 만듭니다.
*   **지능형 자동 스크롤**: '자동 스크롤'을 켜면 항상 최신 로그를 보여주지만, 사용자가 직접 위로 스크롤하여 이전 로그를 확인하는 순간, 자동 스크롤 기능은 사용자를 방해하지 않도록 잠시 멈춥니다.
*   **완벽한 클립보드 복사**: `Ctrl+C` 또는 마우스 오른쪽 클릭 메뉴를 통해 로그를 복사할 수 있으며, 여러 줄로 그룹화된 로그(스택 트레이스)의 경우 접혀있는 모든 내용까지 완벽하게 복사합니다.
*   **로그 지우기**: 버튼 클릭 한 번으로 현재 탭의 모든 로그를 깔끔하게 지울 수 있습니다.
*   **로그 수 제한**: `LogcatManager.cs`에 로그 항목 수를 제한하는 기능이 추가되어 메모리 사용량을 최적화합니다.
*   **필터링된 로그 저장**: 현재 필터링된 로그를 텍스트 파일로 저장하는 기능이 추가되었습니다.
*   **스크린샷**: 연결된 안드로이드 기기의 스크린샷을 캡처합니다.
*   **화면 녹화**: 연결된 안드로이드 기기의 화면을 녹화합니다.

### 📲 APK 설치 도구 (APK Installer)

*   **다양한 파일 선택 방식**: APK 파일을 탐색기에서 직접 **드래그 앤 드롭**하거나, **클릭하여 파일 선택 대화상자**를 통해 선택할 수 있습니다.
*   **병렬 동시 설치**: 여러 대의 기기가 연결된 상태에서, 모든 기기에 동시에(병렬로) APK를 설치하여 작업 시간을 획기적으로 단축합니다.
*   **전문가용 설치 옵션**: `aapt2`를 내장하여 APK 파일에서 **패키지 이름을 자동으로 추출**하며, 이를 통해 **'삭제 후 재설치'** 옵션을 완벽하게 지원합니다.
*   **실시간 상태 표시**: 각 기기 탭에 설치 진행 상태(진행 중, 성공, 실패)가 색상 막대로 실시간 표시되어 작업 현황을 한눈에 파악할 수 있습니다.
*   **APK 상세 정보 뷰어**: APK 파일의 상세 정보(예: 패키지 이름, 버전, 권한 등)를 설치 전에 확인할 수 있습니다.

## 🛠️ 사용 기술 (Core Technologies)

*   **C#**
*   **.NET 8**
*   **WPF (Windows Presentation Foundation)**

## ⚙️ 실행파일 생성 방법 (How to build)

1.  솔루션이 있는 폴더에서 터미널 실행
2.  아래 커맨드 실행
    ```bash
    dotnet publish -c Release -r win-x64 --self-contained true
    ```
    *(.Net 8 SDK 필요)*
3.  아래 경로 확인 및 'LogCatViewer.exe' 실행
    `.\LogCatViewer\bin\Release\net8.0-windows\win-x64\publish`