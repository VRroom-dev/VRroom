using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class SettingsManager : MonoBehaviour {
    public static event Action<Dictionary<string, object>> LoadDefaultSettings;
    private DateTime _lastModified;
    private Dictionary<string, object> _defaultSettings = new();
    private const string SettingsPath = "settings.txt";

    private void Start() {
        LoadDefaultSettings?.Invoke(_defaultSettings);
        
        WriteMissingSettings();

        foreach (KeyValuePair<string,object> setting in _defaultSettings) {
            GameStateManager.Set(setting.Key, setting.Value);
        }

        LoadSettings();
        StartCoroutine(CheckForChanges());
    }

    private IEnumerator CheckForChanges() {
        while (true) {
            yield return new WaitForSeconds(5);
            
            if (File.Exists(SettingsPath)) {
                DateTime currentModified = File.GetLastWriteTime(SettingsPath);
                if (_lastModified == currentModified) continue;
                LoadSettings();
            }
            else WriteMissingSettings();
        }
    }

    private void LoadSettings() {
        foreach (var line in File.ReadLines(SettingsPath)) {
            string[] parts = line.Split('=');
            if (parts.Length < 2) continue;
            string key = parts[0].Trim();
            if (!TryParse(parts[1].Trim(), out object value)) continue;
            GameStateManager.Instance[key] = value;
            GameStateManager.Subscribe(key, SaveSetting);
        }
        _lastModified = File.GetLastWriteTime(SettingsPath);
    }

    private void SaveSetting(string key, object value) {
        string text = File.ReadAllText(SettingsPath);
        Regex regex = new(@$"^\s*{Regex.Escape(key)}\s*=\s*.*\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
        
        if (regex.Match(text).Success) {
            text = regex.Replace(text, $"{key}={value}\n");
            File.WriteAllText(SettingsPath, text);
        } else {
            File.AppendAllText(SettingsPath, $"{key}={value}\n");
        }
        _lastModified = File.GetLastWriteTime(SettingsPath);
    }

    private void WriteMissingSettings() {
        Dictionary<string, string> currentSettings = new();
        if (File.Exists(SettingsPath)) {
            foreach (var line in File.ReadLines(SettingsPath)) {
                string[] parts = line.Split('=');
                if (parts.Length < 2) continue;
                currentSettings[parts[0].Trim()] = parts[1].Trim();
            }
        }
        using StreamWriter writer = new StreamWriter(SettingsPath, false);
        foreach (KeyValuePair<string, object> setting in _defaultSettings) {
            string value = currentSettings.GetValueOrDefault(setting.Key, setting.Value.ToString());
            writer.WriteLine($"{setting.Key}={value}");
        }
        _lastModified = File.GetLastWriteTime(SettingsPath);
    }

    private static bool TryParse(string value, out object result) {
        if (bool.TryParse(value, out bool boolResult)) {
            result = boolResult;
            return true;
        }
        if (int.TryParse(value, out int intResult)) {
            result = intResult;
            return true;
        }
        if (float.TryParse(value, out float floatResult)) {
            result = floatResult;
            return true;
        }

        result = value;
        return false;
    }
}