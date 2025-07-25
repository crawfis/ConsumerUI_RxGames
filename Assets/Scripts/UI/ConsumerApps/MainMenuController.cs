using RxGames.ConsumerApps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RxGames.UI.ConsumerApps
{
    public class MainMenuController : MonoBehaviour
    {
        private const string LocalePrefKey = "RXGames.SelectedLocale";
        private const string UserPrefsKey = "UserGameInfoList";
        private const string PlaySceneName = "PlayConsumerGames";
        [SerializeField] private UIDocument _mainMenuDocument;
        [SerializeField] private string _prescriptionMenuScene;
        [SerializeField] private List<GameInfoScriptableObject> _defaultGameInfoList;

        private DropdownField _languageSelector;
        void Awake()
        {
            var root = _mainMenuDocument.rootVisualElement;
            InitializeGameInfoList();
            if (root != null)
            {
                var prescriptionButton = root.Q<Button>("prescription-button");
                if (prescriptionButton != null) prescriptionButton.clicked += PrescriptionButton_clicked;
                var playButton = root.Q<Button>("play-button"); // corrected variable name
                if (playButton != null) playButton.clicked += PlayButton_clicked;
                var quitButton = root.Q<Button>("quit-button");
                if (quitButton != null) quitButton.clicked += () => Application.Quit();

                _languageSelector = root.Q<DropdownField>("locale-selection-dropdown");
                if (_languageSelector != null)
                {
                    var locales = LocalizationSettings.AvailableLocales.Locales;
                    var localeNames = new System.Collections.Generic.List<string>();
                    foreach (var locale in locales)
                    {
                        // Strip the (Simplified) from Chinese.
                        localeNames.Add(TrimExtraParens($"{locale.LocaleName}").displayName);
                    }
                    _languageSelector.choices = localeNames;

                    _languageSelector.RegisterValueChangedCallback<string>(LanguageChanged);
                    // Load saved locale from PlayerPrefs
                    string savedLocale = PlayerPrefs.GetString(LocalePrefKey, null);
                    string initialDropdownValue = null;
                    if (!string.IsNullOrEmpty(savedLocale))
                    {
                        var localeObj = LocalizationSettings.AvailableLocales.GetLocale(savedLocale);
                        if (localeObj != null)
                        {
                            LocalizationSettings.SelectedLocale = localeObj;
                            initialDropdownValue = $"{localeObj.LocaleName}";
                        }
                    }
                    var currentLocale = LocalizationSettings.SelectedLocale;
                    if (currentLocale != null)
                    {
                        initialDropdownValue = $"{currentLocale.LocaleName}";
                    }
                    else if (localeNames.Count > 0)
                    {
                        initialDropdownValue = localeNames[0];
                    }
                    //_languageSelector.value = TrimExtraParens(initialDropdownValue);
                    _languageSelector.value = initialDropdownValue;
                }
            }
        }

        private void PlayButton_clicked()
        {
            // Read the PlayerPrefs for the exerciseList and create a prescription
            if (!PlayerPrefs.HasKey(UserPrefsKey))
            {
                Debug.LogError("No game information found in PlayerPrefs. Please select a game first.");
                return;
            }
            string json = PlayerPrefs.GetString(UserPrefsKey);
            GameInfoListWrapper wrapper = JsonUtility.FromJson<GameInfoListWrapper>(json);
            if (wrapper == null || wrapper.List == null || wrapper.List.Count == 0)
            {
                Debug.LogError("No game information found in PlayerPrefs. Please select a game first.");
                return;
            }
            foreach (var gameInfo in wrapper.List)
            {
                Debug.Log($"Game ID: {gameInfo.GameId}, Duration: {gameInfo.Duration}, Sets: {gameInfo.NumberOfSets}");
            }
            // Load Play Scene
        }

        private void InitializeGameInfoList()
        {
            if (true)// (!PlayerPrefs.HasKey(UserPrefsKey))
            {
                // Save initial list to PlayerPrefs
                var wrapper = new GameInfoListWrapper
                {
                    List = _defaultGameInfoList.Select(so => new GameInfoDTO
                    {
                        GameId = so.GameId,
                        //GameTitle = so.GameTitle,
                        //Description = so.Description,
                        //TherapyNotes = so.TherapyNotes,
                        //Exercises = so.Exercises,
                        Duration = so.Duration,
                        NumberOfSets = so.NumberOfSets,
                        TutorialUrl = so.TutorialUrl,
                        GameImagePath = so.GameImagePath
                    }).ToList()
                };
                string json = JsonUtility.ToJson(wrapper);
                PlayerPrefs.SetString(UserPrefsKey, json);
                PlayerPrefs.Save();
            }
        }
        private void LanguageChanged(ChangeEvent<string> evt)
        {
            var languageString = evt.newValue;
            string languageCode = TrimExtraParens(languageString).languageCode;
            //string languageCode = evt.newValue;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
            // Save this to PlayerPrefs
            if (!string.IsNullOrEmpty(languageCode))
            {
                PlayerPrefs.SetString(LocalePrefKey, languageCode);
                PlayerPrefs.Save();
            }
        }

        private static (string displayName, string languageCode) TrimExtraParens(string languageString)
        {
            // Extract the part before any parentheses (trimmed)
            string baseName = languageString.Split('(')[0].Trim();
            // Find all groups of parentheses and only keep the last one if there are multiple
            var matches = Regex.Matches(languageString, @"\(([^()]*)\)");
            string languageCode = string.Empty;
            if (matches.Count > 0)
            {
                languageCode = matches[matches.Count - 1].Groups[1].Value;
            }
            string combined = baseName;
            if (!string.IsNullOrEmpty(languageCode))
            {
                combined += $" ({languageCode})";
            }

            return (combined, languageCode);
        }

        private void PrescriptionButton_clicked()
        {
            SceneManager.LoadSceneAsync(_prescriptionMenuScene, LoadSceneMode.Additive);
        }
    }
}