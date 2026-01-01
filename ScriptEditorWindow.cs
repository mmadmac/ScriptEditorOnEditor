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
    private MonoScript previousTargetScript; // <-- Para detectar cambios en el campo
    private bool autoSave = false;
    private double lastSaveTime;
    private const double AUTO_SAVE_INTERVAL = 5.0; // segundos
    private bool scriptIsLoaded = false;

    [MenuItem("Tools/Script Editor")]
    public static void ShowWindow()
    {
        GetWindow<ScriptEditorWindow>("Script Editor");
    }

    private void OnGUI()
    {
        // --- DETECCIÓN DE CAMBIO AUTOMÁTICO ---
        // Comprobamos si el objeto en el campo ha cambiado desde el último frame.
        if (targetScript != previousTargetScript)
        {
            // Si ha cambiado, cargamos el nuevo script automáticamente.
            // Si el campo se vació (targetScript es null), LoadScript() se encargará de resetear el estado.
            LoadScript();
            // Actualizamos la referencia para el siguiente frame.
            previousTargetScript = targetScript;
        }

        EditorGUILayout.BeginHorizontal();
        // El campo de script sigue ahí, pero el botón "Load" ahora es redundante.
        // Podríamos eliminarlo, pero lo dejamos por si se quiere recargar a mano.
        targetScript = EditorGUILayout.ObjectField("Script", targetScript, typeof(MonoScript), false) as MonoScript;
        if (GUILayout.Button("Reload", GUILayout.Width(60))) // <-- Cambié "Load" por "Reload"
        {
            LoadScript();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        autoSave = EditorGUILayout.Toggle("Auto Save", autoSave);
        if (autoSave)
        {
            EditorGUILayout.LabelField($"(Saves to disk every {AUTO_SAVE_INTERVAL}s)");
        }
        EditorGUILayout.EndHorizontal();

        // Desactivamos la interfaz si no hay un script cargado.
        EditorGUI.BeginDisabledGroup(!scriptIsLoaded);
        
        EditorGUI.BeginChangeCheck();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 70));
        scriptContent = EditorGUILayout.TextArea(scriptContent, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        if (EditorGUI.EndChangeCheck())
        {
            hasUnsavedChanges = true;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            SaveScript();
        }
        if (GUILayout.Button("Save & Compile"))
        {
            SaveAndCompile();
        }
        if (GUILayout.Button("Force Compile"))
        {
            SaveAndCompile();
        }
        EditorGUILayout.EndHorizontal();

        // Auto-guardado
        if (autoSave && scriptIsLoaded && hasUnsavedChanges && EditorApplication.timeSinceStartup - lastSaveTime > AUTO_SAVE_INTERVAL)
        {
            SaveScript();
            lastSaveTime = EditorApplication.timeSinceStartup;
        }

        // Fin del grupo desactivado
        EditorGUI.EndDisabledGroup();
    }

    private void LoadScript()
    {
        // Reseteamos el estado por si acaso
        scriptIsLoaded = false;
        scriptPath = "";

        if (targetScript == null)
        {
            // Si el campo se vacía, limpiamos el contenido.
            scriptContent = "";
            hasUnsavedChanges = false;
            return;
        }

        scriptPath = AssetDatabase.GetAssetPath(targetScript);
        try
        {
            scriptContent = File.ReadAllText(scriptPath, Encoding.UTF8);
            scriptIsLoaded = true;
            hasUnsavedChanges = false;
            lastSaveTime = EditorApplication.timeSinceStartup;
            Debug.Log($"Loaded script: {scriptPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to load script: {e.Message}", "OK");
            scriptContent = "";
            hasUnsavedChanges = false;
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
            hasUnsavedChanges = false;
            Debug.Log($"Script saved to disk: {scriptPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to save script: {e.Message}", "OK");
        }
    }

    private void SaveAndCompile()
    {
        if (string.IsNullOrEmpty(scriptPath))
        {
            EditorUtility.DisplayDialog("Error", "No script selected to save.", "OK");
            return;
        }

        try
        {
            File.WriteAllText(scriptPath, scriptContent, Encoding.UTF8);
            AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            hasUnsavedChanges = false;
            Debug.Log($"Script saved and compiled: {scriptPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to save and compile script: {e.Message}", "OK");
        }
    }
}