using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ScriptEditorWindow : EditorWindow
{
    private string scriptContent = "";
    private string scriptPath = "";
    private Vector2 scrollPosition;
    private MonoScript targetScript;
    private bool autoSave = false;
    private double lastSaveTime;
    private const double AUTO_SAVE_INTERVAL = 5.0; // segundos

    [MenuItem("Tools/Script Editor")]
    public static void ShowWindow()
    {
        GetWindow<ScriptEditorWindow>("Script Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // Campo para seleccionar el script
        targetScript = EditorGUILayout.ObjectField("Script", targetScript, typeof(MonoScript), false) as MonoScript;

        if (GUILayout.Button("Load", GUILayout.Width(60)))
        {
            LoadScript();
        }

        EditorGUILayout.EndHorizontal();

        // Opciones de guardado automático
        EditorGUILayout.BeginHorizontal();
        autoSave = EditorGUILayout.Toggle("Auto Save", autoSave);
        if (autoSave)
        {
            EditorGUILayout.LabelField($"(Every {AUTO_SAVE_INTERVAL}s)");
        }
        EditorGUILayout.EndHorizontal();

        // Editor de texto
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 70));
        scriptContent = EditorGUILayout.TextArea(scriptContent, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        // Botones de acción
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save"))
        {
            SaveScript();
        }

        if (GUILayout.Button("Save & Compile"))
        {
            SaveScript();
            AssetDatabase.Refresh();
            EditorApplication.isPlaying = false; // Asegura que no esté en play mode
        }

        if (GUILayout.Button("Force Compile"))
        {
            SaveScript();
            AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);
        }

        EditorGUILayout.EndHorizontal();

        // Auto-guardado
        if (autoSave && EditorApplication.timeSinceStartup - lastSaveTime > AUTO_SAVE_INTERVAL)
        {
            SaveScript();
            lastSaveTime = EditorApplication.timeSinceStartup;
        }
    }

    private void LoadScript()
    {
        if (targetScript == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a script to load.", "OK");
            return;
        }

        scriptPath = AssetDatabase.GetAssetPath(targetScript);
        
        try
        {
            scriptContent = File.ReadAllText(scriptPath, Encoding.UTF8);
            lastSaveTime = EditorApplication.timeSinceStartup;
            Debug.Log($"Loaded script: {scriptPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to load script: {e.Message}", "OK");
            scriptContent = "";
        }
    }

    private void SaveScript()
    {
        if (string.IsNullOrEmpty(scriptPath))
        {
            EditorUtility.DisplayDialog("Error", "No script selected to save.", "OK");
            return;
        }

        try
        {
            File.WriteAllText(scriptPath, scriptContent, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"Saved script: {scriptPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to save script: {e.Message}", "OK");
        }
    }
}