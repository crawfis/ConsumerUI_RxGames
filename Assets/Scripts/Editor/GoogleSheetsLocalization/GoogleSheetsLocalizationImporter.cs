// Copyright 2025 Crawfis Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// --- REQUIRED PACKAGE ---
// This script requires the "Editor Coroutines" package.
// Please install it from the Unity Package Manager (Window > Package Manager).
using RxGames.Utility;

using System.Collections;
using System.Linq;

using Unity.EditorCoroutines.Editor;

using UnityEditor;
using UnityEditor.Localization;

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Networking;

/// <summary>
/// An Editor window to import localization data from Google Sheets.
/// This tool downloads a specified "index" sheet to find out which other sheets to download.
/// For each of those sheets, it reads the header row to identify languages, then downloads
/// the data and creates/updates a String Table for each language.
/// </summary>
public class GoogleSheetsLocalizationImporter : EditorWindow
{
    // --- PRIVATE FIELDS ---
    private string _googleSheetId = "1mCWFHo6rw3D60IcjQI9TmVKt6hMi0j5aKL7dMSMKqdk";
    private string _indexSheetName = "TabsToExport"; // The name of the tab that lists all other tabs to export
    private string _stringTableBaseName = "GameText";
    private Vector2 _scrollPosition;

    // The directory where assets will be saved
    private string _assetDirectory;
    private GoogleSheetsLocalizationImporterView _view;

    // --- MENU ITEM ---
    [MenuItem("Window/Localization/Google Sheets Importer")]
    public static void ShowWindow()
    {
        GetWindow<GoogleSheetsLocalizationImporter>("Google Sheets Importer");
    }

    private void OnEnable()
    {
        _view = new GoogleSheetsLocalizationImporterView();
    }

    // --- EDITOR WINDOW GUI ---
    private void OnGUI()
    {
        var result = _view.Draw(
            _googleSheetId,
            _indexSheetName,
            _stringTableBaseName,
            _scrollPosition
        );
        _googleSheetId = result.GoogleSheetId;
        _indexSheetName = result.IndexSheetName;
        _stringTableBaseName = result.StringTableBaseName;
        _scrollPosition = result.ScrollPosition;

        if (result.ImportRequested && ValidateInputs())
        {
            EditorCoroutineUtility.StartCoroutine(ImportProcess(), this);
        }
    }

    // --- CORE IMPORT LOGIC ---

    /// <summary>
    /// The main coroutine that orchestrates the entire import process.
    /// </summary>
    private IEnumerator ImportProcess()
    {
        Debug.Log("Starting localization import process...");

        // 0. Determine asset directory based on current selection and googleSheetId
        _assetDirectory = GoogleSheetsLocalizationUtils.GetOrCreateLocalizationAssetBaseDirectory(_indexSheetName);
        Debug.Log($"Assets will be saved to: {_assetDirectory}");

        // 1. Download the index sheet to get the list of tabs to import
        string indexUrl = GoogleSheetsLocalizationUtils.GetGoogleSheetCsvUrl(_googleSheetId, _indexSheetName);
        using UnityWebRequest indexRequest = UnityWebRequest.Get(indexUrl);
        yield return indexRequest.SendWebRequest();

        if (indexRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to download index sheet '{_indexSheetName}' from {indexUrl}: {indexRequest.error}");
            yield break;
        }

        string[] tabsToImport = GoogleSheetsLocalizationUtils.ParseCsv(indexRequest.downloadHandler.text)
                                .Select(row => row.FirstOrDefault()) // Get the first column
                                .Where(tabName => !string.IsNullOrWhiteSpace(tabName)) // Filter out empty lines
                                .ToArray();

        if (tabsToImport.Length <= 1)
        {
            Debug.LogWarning("Index sheet is empty or could not be read. No tabs will be imported.");
            yield break;
        }

        Debug.Log($"Found {tabsToImport.Length - 1} tabs to import: {string.Join(", ", tabsToImport)}");

        // 2. Get the active Localization Settings asset to ensure it exists
        var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
        if (settings == null)
        {
            Debug.LogError("Localization Settings asset not found. Please create one via 'Edit -> Project Settings -> Localization'.");
            yield break;
        }

        // 3. Loop through each tab and import its data
        // Skip the first row which is usually the header in the index sheet
        tabsToImport = tabsToImport.Skip(1).ToArray();
        foreach (string tabName in tabsToImport)
        {
            yield return this.StartCoroutine(ImportTab(tabName.Trim()));
        }

        // 4. Save assets and refresh the database
        Debug.Log("Localization import process finished. Saving assets...");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Assets saved and refreshed.");
    }

    /// <summary>
    /// Imports a single tab's data, creating tables for all languages found in its header.
    /// </summary>
    private IEnumerator ImportTab(string tabName)
    {
        Debug.Log($"--- Importing tab: {tabName} ---");
        string tabUrl = GoogleSheetsLocalizationUtils.GetGoogleSheetCsvUrl(_googleSheetId, tabName);
        using UnityWebRequest tabRequest = UnityWebRequest.Get(tabUrl);
        yield return tabRequest.SendWebRequest();

        if (tabRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to download tab '{tabName}': {tabRequest.error}");
            yield break;
        }

        var csvData = GoogleSheetsLocalizationUtils.ParseCsv(tabRequest.downloadHandler.text);
        if (csvData.Count < 2) // Must have at least a header and one data row
        {
            Debug.LogWarning($"Tab '{tabName}' is empty or contains no data rows. Skipping.");
            yield break;
        }

        var headerRow = csvData[0];
        if (headerRow.Count < 2)
        {
            Debug.LogWarning($"Tab '{tabName}' header is malformed (needs at least a Key and one Language column). Skipping.");
            yield break;
        }

        var tableName = $"{_stringTableBaseName}{tabName}";
        string localizationFolder = GoogleSheetsLocalizationUtils.GetOrCreateLocalizationAssetTabDirectory(_assetDirectory, tabName);
        StringTableCollection collection = LocalizationEditorSettings.CreateStringTableCollection(tableName, localizationFolder);
        // Loop through each language column (starting from the second column, index 1)
        for (int langIndex = 1; langIndex < headerRow.Count; langIndex++)
        {
            string localeCode = headerRow[langIndex];
            if (string.IsNullOrWhiteSpace(localeCode)) continue; // Skip empty header columns

            string languageName = GoogleSheetsLocalizationUtils.GetLocaleCodeForLanguageName(localeCode);
            if (string.IsNullOrEmpty(languageName))
            {
                Debug.LogWarning($"Could not find a locale code for language '{languageName}' in tab '{tabName}'. Skipping this column.");
                continue;
            }

            var localeId = new LocaleIdentifier(localeCode);

            // Directly get the table for the specific locale from the collection.
            // Use correct overload: GetTable(LocaleIdentifier localeIdentifier)
            StringTable table = collection.GetTable(localeId) as StringTable;

            // If the table for this locale doesn't exist, create it.
            if (table == null)
            {
                Debug.Log($"Creating new String Table: '{tableName}' for locale '{localeCode}'");
                table = collection.AddNewTable(tableName, localeId.Code) as StringTable;
            }
            else
            {
                // Clear existing entries for a clean re-import
                Debug.Log($"Clearing existing entries for table: '{tableName}' for locale '{localeCode}'");
                table.Clear();
            }

            if (table == null)
            {
                Debug.LogError($"Failed to create or find String Table '{tableName}' for locale '{localeCode}'.");
                continue;
            }

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(table);

            // Populate the table with data from the CSV (skip header row)
            for (int rowIndex = 1; rowIndex < csvData.Count; rowIndex++)
            {
                var row = csvData[rowIndex];
                if (row.Count <= langIndex || string.IsNullOrWhiteSpace(row[0]))
                {
                    continue; // Skip malformed rows or rows with an empty key
                }

                string key = row[0].Trim();
                string value = row[langIndex].Trim();

                table.AddEntry(key, value);
            }
            Debug.Log($"Successfully imported {table.Count} entries into '{tableName}' for locale '{localeCode}'.");
        }
        yield return null; // Wait a frame after processing all languages in a tab
    }

    /// <summary>
    /// Validates that all required input fields have values.
    /// </summary>
    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(_googleSheetId))
        {
            EditorUtility.DisplayDialog("Validation Error", "Google Sheet ID cannot be empty.", "OK");
            return false;
        }
        if (string.IsNullOrWhiteSpace(_indexSheetName))
        {
            EditorUtility.DisplayDialog("Validation Error", "Index Tab Name cannot be empty.", "OK");
            return false;
        }
        if (string.IsNullOrWhiteSpace(_stringTableBaseName))
        {
            EditorUtility.DisplayDialog("Validation Error", "String Table Base Name cannot be empty.", "OK");
            return false;
        }
        return true;
    }
}
