using UnityEditor;

using UnityEngine;

public class GoogleSheetsLocalizationImporterView
{
    public class ViewResult
    {
        public string GoogleSheetId;
        public string IndexSheetName;
        public string StringTableBaseName;
        public Vector2 ScrollPosition;
        public bool ImportRequested;
    }

    public ViewResult Draw(string googleSheetId, string indexSheetName, string stringTableBaseName, Vector2 scrollPosition)
    {
        var result = new ViewResult
        {
            GoogleSheetId = googleSheetId,
            IndexSheetName = indexSheetName,
            StringTableBaseName = stringTableBaseName,
            ScrollPosition = scrollPosition,
            ImportRequested = false
        };

        EditorGUILayout.LabelField("Google Sheets Localization Importer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool imports localization data from a public Google Sheet.\n" +
            "1. Install the 'Editor Coroutines' package from the Package Manager.\n" +
            "2. Make your Google Sheet public ('File' -> 'Share' -> 'Publish to web').\n" +
            "3. Enter your Sheet ID and the name of the 'index' tab.\n" +
            "4. The 'index' tab should list the names of other tabs to import.\n" +
            "5. Each data tab should have 'Key' in the first column, and language names (e.g., 'English', 'Spanish') as headers for subsequent columns.",
            MessageType.Info);

        result.ScrollPosition = EditorGUILayout.BeginScrollView(result.ScrollPosition);
        result.GoogleSheetId = EditorGUILayout.TextField(new GUIContent("Google Sheet ID", "The long ID from your Google Sheet URL."), result.GoogleSheetId);
        result.IndexSheetName = EditorGUILayout.TextField(new GUIContent("Index Tab Name", "The name of the tab that lists other tabs to import."), result.IndexSheetName);
        result.StringTableBaseName = EditorGUILayout.TextField(new GUIContent("String Table Base Name", "The base name for the created String Table assets (e.g., 'GameText')."), result.StringTableBaseName);
        EditorGUILayout.Space();
        if (GUILayout.Button("Import From Google Sheets", GUILayout.Height(40)))
        {
            result.ImportRequested = true;
        }
        EditorGUILayout.EndScrollView();
        return result;
    }
}