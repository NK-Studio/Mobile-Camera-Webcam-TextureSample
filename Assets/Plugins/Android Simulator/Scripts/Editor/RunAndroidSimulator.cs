#if UNITY_ANDROID
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class RunAndroidSimulator : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTreeAsset;

    private int _checkCount;
    private bool _checkIsRun;

    private Button _runButton;

    [MenuItem("Tools/Android/Simulator Window")]
    public static void ShowExample()
    {
        RunAndroidSimulator wnd = GetWindow<RunAndroidSimulator>();
        wnd.titleContent = new GUIContent("Simulator Window");
        wnd.minSize = new Vector2(360, 200);
        wnd.maxSize = new Vector2(360, 200);
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement uxml = visualTreeAsset.Instantiate();
        root.Add(uxml);

        var titleLabel = root.Q<Label>("Title");
        _runButton = root.Q<Button>("button-run");
        var adbLabel = root.Q<Label>("label-adb");
        var scrcpyLabel = root.Q<Label>("label-scrcpy");
        var deviceLabel = root.Q<Label>("label-connected");

        UpdateGUI(adbLabel, scrcpyLabel, deviceLabel, titleLabel);

        _runButton.clickable.clicked += () =>
        {
            RunScrcpy();
            _runButton.SetEnabled(false);
            titleLabel.text = "Android Simulator (Running)";
        };

        root.schedule.Execute(() => UpdateGUI(adbLabel, scrcpyLabel, deviceLabel, titleLabel)).Every(1000);
    }

    private void UpdateGUI(Label adbLabel, Label scrcpyLabel, Label deviceLabel, Label titleLabel)
    {
        _checkCount = 0;

        CheckScrcpy(scrcpyLabel);

        if (CheckADB(adbLabel))
            CheckOneAndroidDevice(deviceLabel);
        else
        {
            deviceLabel.style.color = new StyleColor(Color.red);
            deviceLabel.tooltip = "adb가 존재하지 않습니다.";
        }

        bool isScrcpyRunning = IsScrcpyRunning();
        bool allowRun = _checkCount >= 3 && !isScrcpyRunning;

        if (allowRun)
            _runButton.SetEnabled(true);
        else
            _runButton.SetEnabled(false);

        if (isScrcpyRunning)
            titleLabel.text = "Android Simulator (Running)";
        else
            titleLabel.text = "Android Simulator";
    }

    /// <summary>
    /// ADB 설치 여부를 확인합니다.
    /// </summary>
    /// <param name="adbLabel">ADB 상태를 표시할 라벨</param>
    private bool CheckADB(Label adbLabel)
    {
        string androidSDKPath = AndroidExternalToolsSettings.sdkRootPath;

        if (string.IsNullOrEmpty(androidSDKPath))
        {
            Debug.LogError("SDK 경로가 비어 있습니다.");
            return false;
        }
        
#if UNITY_EDITOR_WIN
        var adbPath = $"{androidSDKPath}\\platform-tools\\adb.exe";
#elif UNITY_EDITOR_OSX
        var adbPath = $"{androidSDKPath}/platform-tools/adb";
#endif
        
        if (File.Exists(adbPath))
        {
            adbLabel.style.color = new StyleColor(Color.green);
            adbLabel.tooltip = "adb 확인 됨";
            _checkCount += 1;
            return true;
        }

        adbLabel.style.color = new StyleColor(Color.red);
        adbLabel.tooltip = "adb를 찾지 못했습니다.";
        return false;
    }

    /// <summary>
    /// scrcpy 설치 여부를 확인합니다.
    /// </summary>
    /// <param name="scrcpyLabel">scrcpy 상태를 표시할 라벨</param>
    private void CheckScrcpy(Label scrcpyLabel)
    {
        if (IsScrcpyInstalled())
        {
            scrcpyLabel.style.color = new StyleColor(Color.green);
            scrcpyLabel.tooltip = "scrcpy 확인 됨";
            _checkCount += 1;
        }
        else
        {
            scrcpyLabel.style.color = new StyleColor(Color.red);
#if UNITY_EDITOR_WIN
            scrcpyLabel.tooltip = "scrcpy 설치가 필요합니다.";
#elif UNITY_EDITOR_OSX
            scrcpyLabel.tooltip = "brew를 통해 scrcpy 설치가 필요합니다.";
#endif
            scrcpyLabel.AddToClassList("clickLabel");

            scrcpyLabel.RegisterCallback<ClickEvent>(evt =>
            {
#if UNITY_EDITOR_WIN
                CopyText("https://github.com/Genymobile/scrcpy/blob/master/doc/windows.md");
                Debug.Log("복사된 주소로 이동하여 scrcpy-win을 설치하고 환경 변수를 등록하십시오.");
#elif UNITY_EDITOR_OSX
                CopyText("brew install scrcpy");
                Debug.Log("<b>brew install scrcpy</b> 명령어가 복사되었습니다.");
#endif
            });
        }
    }

    /// <summary>
    /// 하나 이상의 안드로이드 디바이스가 연결되어 있는지 확인합니다.
    /// </summary>
    /// <param name="deviceLabel">디바이스 상태를 표시할 라벨</param>
    private void CheckOneAndroidDevice(Label deviceLabel)
    {
        string adbPath = $"{AndroidExternalToolsSettings.sdkRootPath}/platform-tools/adb";
        int deviceCount = GetADBDeviceCount(adbPath);

        bool hasOneDevice = deviceCount >= 1;

        if (hasOneDevice)
        {
            deviceLabel.style.color = new StyleColor(Color.green);
            deviceLabel.tooltip = "연결된 디바이스가 하나 이상 있음";
            _checkCount += 1;
            return;
        }

        deviceLabel.style.color = new StyleColor(Color.red);
        deviceLabel.tooltip = "연결된 디바이스가 없습니다.";
    }

    /// <summary>
    /// ADB에 연결된 디바이스의 개수를 가져옵니다.
    /// </summary>
    private static bool IsScrcpyInstalled()
    {
        try
        {
            Process process = new Process();
#if UNITY_EDITOR_WIN
            string command = "where scrcpy";
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {command}";
#elif UNITY_EDITOR_OSX
        var command = "which /opt/homebrew/bin/scrcpy";
        process.StartInfo.FileName = "/bin/zsh";
        process.StartInfo.Arguments = $"-c \" {command} \"";
#endif
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;

            process.Start();

            // Read the command output
            string result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            // Check if the path contains invalid characters
            return File.Exists(result);
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 텍스트를 클립보드에 복사합니다.
    /// </summary>
    /// <param name="text">복사할 텍스트</param>
    private static void CopyText(string text)
    {
        TextEditor te = new TextEditor();
        te.text = text;
        te.SelectAll();
        te.Copy();
    }

    /// <summary>
    /// ADB에 연결된 디바이스의 개수를 가져옵니다.
    /// </summary>
    /// <param name="adbPath">adb 실행 파일의 경로</param>
    /// <returns>연결된 ADB 디바이스의 개수, 오류 발생 시 -1 반환</returns>
    private static int GetADBDeviceCount(string adbPath)
    {
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = "devices",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            }
        };

        try
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Split the output into lines and count the number of device lines
            string[] lines = output.Split('\n');
            return lines.Count(line => line.Contains("\tdevice"));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to get ADB device count: " + e.Message);
            return -1; // Return -1 to indicate an error
        }
    }

    /// <summary>
    /// scrcpy를 실행합니다.
    /// </summary>
    private static void RunScrcpy()
    {
        string androidSDKPath = AndroidExternalToolsSettings.sdkRootPath;

        if (string.IsNullOrEmpty(androidSDKPath))
        {
            Debug.LogError("SDK 경로가 비어 있습니다.");
            return;
        }

        var adbPath = $"{androidSDKPath}/platform-tools/adb";

        int count = GetADBDeviceCount(adbPath);
        if (count == 0)
        {
            Debug.LogError("연결된 ADB 장치가 없습니다.");
            return;
        }

        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
#if UNITY_EDITOR_WIN
                FileName = "cmd.exe",
                Arguments = "/c start scrcpy",
#elif UNITY_EDITOR_OSX
                FileName = "osascript",
                Arguments = "-e 'tell application \"Terminal\" to do script \"scrcpy\"'",
#endif
                RedirectStandardOutput = false,
                UseShellExecute = true,
            }
        };

        try
        {
            process.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to start scrcpy: " + e.Message);
        }
    }

    private static bool IsScrcpyRunning()
    {
        try
        {
            Process process = new Process();

#if UNITY_EDITOR_WIN
            // cmd.exe 사용
            process.StartInfo.FileName = "cmd.exe";
            // 'tasklist' 명령으로 모든 프로세스를 나열하고, 그 중 scrcpy를 검색
            process.StartInfo.Arguments = "/c tasklist | findstr scrcpy";
#elif UNITY_EDITOR_OSX
            // /bin/bash 대신 /bin/zsh 사용
            process.StartInfo.FileName = "/bin/zsh";
            // 'ps aux' 명령으로 모든 프로세스를 나열하고, 그 중 scrcpy를 검색
            process.StartInfo.Arguments = "-c \"ps aux | grep scrcpy | grep -v grep\"";
#endif
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            // 명령어 실행 결과 읽기
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var trimmedResult = result.Trim();

            if (string.IsNullOrEmpty(trimmedResult))
            {
                return false;
            }

            return true;
        }
        catch (System.Exception e)
        {
            // 예외 발생 시 오류 로그 출력
            Debug.LogError("Error while checking if scrcpy is running: " + e.Message);
            return false;
        }
    }
}
#endif