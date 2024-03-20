using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _Project.Common
{
    public sealed class ConsoleController : MonoBehaviour
    {
        [SerializeField] private KeyCode _toggleEnableKey;
        private bool _enableConsoleWindow;
        private bool _enableDebugMode;

        private string _debugText = "";
        private string _inputText = "";

        private const string _fileName = "game_log";
        private string _filePath = Path.Combine(Application.dataPath, _fileName);

        private Vector2 _scrollPosition;

        private Command _activeDebugging;
        private List<CommandBase> _commands;

        private Rect _windowRect;

        private void Awake()
        {
            _activeDebugging = new Command("/debug", "", () => { _enableDebugMode = !_enableDebugMode; });

            _commands = new List<CommandBase>
            {
                _activeDebugging,
            };
        }

        private void Start()
        {
            CreateLogFile(_debugText);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            _windowRect = new Rect(0, 0, screenWidth * 0.8f, screenHeight * 0.8f);
            _windowRect.center = new Vector2(screenWidth / 2, screenHeight / 2);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleEnableKey))
            {
                _enableConsoleWindow = !_enableConsoleWindow;
            }
        }

        private void OnEnable() => Application.logMessageReceived += HandleLog;
        private void OnDisable() => Application.logMessageReceived -= HandleLog;

        private void OnGUI()
        {
            if (!_enableConsoleWindow)
            {
                _inputText = "";
                return;
            }

            _windowRect = GUILayout.Window(0, _windowRect, DebugWindow, "Console");
        }

        private void DebugWindow(int windowID)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            GUILayout.Label(_debugText);
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            _inputText = GUILayout.TextField(_inputText, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Send", GUILayout.Width(80)) || Event.current.keyCode == KeyCode.Return)
            {
                if (!string.IsNullOrEmpty(_inputText))
                {
                    AddDebugMessage(_inputText);
                    _inputText = "";
                }
            }
            else if (GUILayout.Button("Close", GUILayout.Width(80), GUILayout.Height(20)))
            {
                _enableConsoleWindow = false;
            }

            GUILayout.EndHorizontal();
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string log = type switch
            {
                LogType.Warning => $"<color=yellow>{logString}</color>",
                LogType.Error => $"<color=red>{logString}</color>",
                LogType.Log => $"<color=blue>{logString}</color>",
                _ => $"<color=red>{logString}</color>"
            };

            string info = $"[SYSTEM: {DateTime.Now}] {log}";

            CreateLogFile(_debugText);
            AddDebugMessage(type == LogType.Log ? log : info, false);
        }

        private void AddDebugMessage(string message, bool lessThan = true)
        {
            string[] properties = message.Split(' ');
            string debugMessage = lessThan ? "> " + message + "\n" : message + "\n";
            _debugText += debugMessage;

            CommandBase commandBase = _commands.FirstOrDefault(r => r.Id == properties[0]);

            if ((commandBase == null ||
                 (!_enableDebugMode) && properties[0].StartsWith("/") && commandBase.Id != "/debug"))
            {
                if (message.StartsWith("/"))
                    Debug.LogWarning($"Command {message} not found or you do not have permission to use it.");

                return;
            }

            if (commandBase is Command<int> argumentCommand)
            {
                if (properties.Length > 1 && int.TryParse(properties[1], out int intValue))
                    argumentCommand.Use(intValue);
                else
                    Debug.LogWarning("Invalid or missing argument for command: " + properties[0]);
            }
            else if (commandBase is Command command)
                command.Use();
        }

        private void CreateLogFile(string content)
        {
            try
            {
                if (CheckFileExists())
                {
                    File.WriteAllText(_filePath, string.Empty);
                }

                File.AppendAllText(_filePath, content);
                Debug.Log("The log file has been created. " + _filePath);
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred while creating the log file. " + e.ToString());
                throw;
            }
        }

        private bool CheckFileExists()
        {
            string filePath = Path.Combine(Application.dataPath, "game_log");
            return File.Exists(_filePath);
        }
    }
}