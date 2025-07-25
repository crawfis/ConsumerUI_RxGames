using System.Collections.Generic;

using UnityEditor;

namespace RxGames.Utility
{
    public static class GoogleSheetsLocalizationUtils
    {
        /// <summary>
        /// Maps a language name from the Google Sheet header to a standard locale code.
        /// </summary>
        public static string GetLocaleCodeForLanguageName(string languageName)
        {
            // This is a case-insensitive mapping. It can be expanded as needed.
            switch (languageName.Trim().ToLower())
            {
                case "english": return "en";
                case "spanish": return "es";
                case "french": return "fr";
                case "german": return "de";
                case "italian": return "it";
                case "japanese": return "ja";
                case "korean": return "ko";
                case "portuguese": return "pt";
                case "russian": return "ru";
                case "chinese (simplified)": return "zh-Hans";
                case "chinese (traditional)": return "zh-Hant";
                case "arabic": return "ar";
                case "hindi": return "hi";
                case "swahili": return "sw";
                case "turkish": return "tr";
                case "vietnamese": return "vi";
                case "polish": return "pl";
                case "dutch": return "nl";
                case "thai": return "th";
                case "ukrainian": return "uk";
                // Add more mappings as needed
                default: return null;
            }
        }

        /// <summary>
        /// Maps a locale code to a language name with code, e.g., "en" => "English (en)".
        /// </summary>
        public static string GetLanguageNameForLocaleCode(string localeCode)
        {
            switch (localeCode.Trim().ToLower())
            {
                case "ar": return "Arabic (ar)";
                case "zh-hans": return "Chinese (Simplified) (zh-Hans)";
                case "en": return "English (en)";
                case "fr": return "French (fr)";
                case "de": return "German (de)";
                case "hi": return "Hindi (hi)";
                case "it": return "Italian (it)";
                case "ja": return "Japanese (ja)";
                case "ko": return "Korean (ko)";
                case "pt": return "Portuguese (pt)";
                case "ru": return "Russian (ru)";
                case "es": return "Spanish (es)";
                case "sw": return "Swahili (sw)";
                case "zh-hant": return "Chinese (Traditional) (zh-Hant)";
                case "tr": return "Turkish (tr)";
                case "vi": return "Vietnamese (vi)";
                case "pl": return "Polish (pl)";
                case "nl": return "Dutch (nl)";
                case "th": return "Thai (th)";
                case "uk": return "Ukrainian (uk)";
                default: return null;
            }
        }

        /// <summary>
        /// Constructs the URL to download a specific sheet from a Google Sheet as a CSV file.
        /// </summary>
        public static string GetGoogleSheetCsvUrl(string docId, string sheetName)
        {
            string url = @"https://docs.google.com/spreadsheets/d/";
            url += $"{docId}/gviz/tq?tqx=out:csv&sheet=";
            url += $"{sheetName}";
            return url;
        }

        /// <summary>
        /// A CSV parser that handles quoted fields and commas within them.
        /// </summary>
        public static List<List<string>> ParseCsv(string csvText)
        {
            var result = new List<List<string>>();
            var lines = csvText.Replace("\r\n", "\n").Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var row = new List<string>();
                int i = 0;
                while (i < line.Length)
                {
                    if (line[i] == '"')
                    {
                        // Quoted field
                        i++; // Skip opening quote
                        var start = i;
                        var value = "";
                        bool inQuotes = true;
                        while (inQuotes && i < line.Length)
                        {
                            if (line[i] == '"')
                            {
                                if (i + 1 < line.Length && line[i + 1] == '"')
                                {
                                    // Escaped quote
                                    value += line.Substring(start, i - start) + '"';
                                    i += 2;
                                    start = i;
                                }
                                else
                                {
                                    // End of quoted field
                                    value += line.Substring(start, i - start);
                                    i++;
                                    inQuotes = false;
                                }
                            }
                            else
                            {
                                i++;
                            }
                        }
                        // Skip comma after quoted field
                        while (i < line.Length && line[i] != ',') i++;
                        if (i < line.Length && line[i] == ',') i++;
                        row.Add(value);
                    }
                    else
                    {
                        // Unquoted field
                        int start = i;
                        while (i < line.Length && line[i] != ',') i++;
                        row.Add(line.Substring(start, i - start).Trim());
                        if (i < line.Length && line[i] == ',') i++;
                    }
                }
                result.Add(row);
            }
            return result;
        }

        /// <summary>
        /// Gets the currently selected folder in the Project window, or defaults to "Assets".
        /// Creates a subfolder named after googleSheetId if it doesn't exist.
        /// </summary>
        public static string GetOrCreateLocalizationAssetBaseDirectory(string indexSheetName)
        {
            string folder = "Assets";
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(path))
            {
                folder = path;
                //break;
            }
            string newFolderName = indexSheetName;
            string newFolderPath = System.IO.Path.Combine(folder, newFolderName);
            if (!AssetDatabase.IsValidFolder(newFolderPath))
            {
                AssetDatabase.CreateFolder(folder, newFolderName);
            }
            return newFolderPath;
        }

        public static string GetOrCreateLocalizationAssetTabDirectory(string assetDirectory, string itemIDLocalization)
        {
            string folder = assetDirectory;
            string newFolderPath = System.IO.Path.Combine(folder, itemIDLocalization);
            if (!AssetDatabase.IsValidFolder(newFolderPath))
            {
                AssetDatabase.CreateFolder(folder, itemIDLocalization);
            }
            return newFolderPath;
        }
    }
}
