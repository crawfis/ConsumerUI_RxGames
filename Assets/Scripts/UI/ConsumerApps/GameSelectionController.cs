using RxGames.ConsumerApps;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RxGames.UI.ConsumerApps
{

    internal partial class GameSelectionController : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;
        [SerializeField] private string _visualElementName = "game-ui-card";

        private List<GameInfoScriptableObject> _gameInfoList;
        private const string UserPrefsKey = "UserGameInfoList";
        private VisualElement _root;
        private Button _backButton;
        private Button _nextButton;
        private Button _previousButton;
        VisualElement _gameCardContainer;
        private List<GameUICard> _gameUITestCards = new List<GameUICard>();
        private int _gameCardIndex = 0;

        private void Awake()
        {
            LoadGameInfoList();
            if (_gameInfoList.Count <= 0) return;

            foreach (var gameInfo in _gameInfoList)
            {
                Sprite gameImage = Resources.Load<Sprite>(gameInfo.GameImagePath); // Load the image from Resources
                GameUICard card = new GameUICard
                {
                    GameID = gameInfo.GameId,
                    //GameTitle = gameInfo.GameTitle,
                    //Description = gameInfo.Description,
                    //TherapyNotes = gameInfo.TherapyNotes,
                    //_exercises = gameInfo.Exercises,
                    _duration = gameInfo.Duration,
                    _numberOfSets = gameInfo.NumberOfSets,
                    _tutorialUrl = gameInfo.TutorialUrl,
                    GameImage = gameImage
                };
                _gameUITestCards.Add(card);
            }
        }

        private void LoadGameInfoList()
        {
            if (PlayerPrefs.HasKey(UserPrefsKey))
            {
                string json = PlayerPrefs.GetString(UserPrefsKey);
                var wrapper = JsonUtility.FromJson<GameInfoListWrapper>(json);
                if (wrapper != null && wrapper.List != null)
                {
                    _gameInfoList = wrapper.List.Select(dto =>
                    {
                        var so = ScriptableObject.CreateInstance<GameInfoScriptableObject>();
                        so.GameId = dto.GameId;
                        //so.GameTitle = dto.GameTitle;
                        //so.Description = dto.Description;
                        //so.TherapyNotes = dto.TherapyNotes;
                        //so.Exercises = dto.Exercises;
                        so.Duration = dto.Duration;
                        so.NumberOfSets = dto.NumberOfSets;
                        so.TutorialUrl = dto.TutorialUrl;
                        // Load Sprite if needed from Resources or AssetDatabase
                        //so.GameImage = Resources.Load<Sprite>(dto.GameImagePath); // Load the image from Resources
                        so.GameImagePath = dto.GameImagePath;
                        return so;
                    }).ToList();
                }
            }
            else
            {
                Debug.LogError($"No game info found in PlayerPrefs under key '{UserPrefsKey}'. Please ensure the game info is initialized properly.");
            }
        }

        private void Start()
        {
            _document = GetComponent<UIDocument>();
            _root = _document.rootVisualElement;
            _nextButton = _root.Q<Button>("next-button");
            _previousButton = _root.Q<Button>("previous-button");
            // Get the placeholder for the game card
            _gameCardContainer = _root.Q<VisualElement>(_visualElementName);
            // Set the initial card
            ShowCard(_gameCardIndex);
            // Register callbacks for next and previous buttons
            if (_nextButton != null)
                _nextButton.clicked += OnNextButtonClicked;
            if (_previousButton != null)
                _previousButton.clicked += OnPreviousButtonClicked;
        }

        private void BackButton_clicked()
        {
            SaveGameInfoListToPrefs(); // Save to PlayerPrefs when Back is clicked
            var scene = gameObject.scene.name;
            SceneManager.UnloadSceneAsync(scene);
        }

        private void ShowCard(int index)
        {
            if (_gameCardContainer == null) return;
            _gameCardContainer.Clear();
            var card = _gameUITestCards[index];
            var cardElement = card.CreateVisualElementFromUXML();
            // Subscribe to sets change event
            card.OnSetsChanged += (newValue) =>
            {
                var gameInfo = _gameInfoList.FirstOrDefault(g => g.GameId == card.GameID);
                if (gameInfo != null)
                {
                    gameInfo.NumberOfSets = newValue;
                    // SaveGameInfoListToPrefs(); // Remove immediate save for efficiency
                }
            };
            _gameCardContainer.Add(cardElement);
            if (_backButton != null) _backButton.clicked -= BackButton_clicked;
            _backButton = cardElement.Q<Button>("back-button");
            _backButton.clicked += BackButton_clicked;
        }

        private void SaveGameInfoListToPrefs()
        {
            var wrapper = new GameInfoListWrapper
            {
                List = _gameInfoList.Select(so => new GameInfoDTO
                {
                    GameId = so.GameId,
                    Duration = so.Duration,
                    NumberOfSets = so.NumberOfSets,
                    TutorialUrl = so.TutorialUrl,
                    GameImagePath = null
                }).ToList()
            };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(UserPrefsKey, json);
            PlayerPrefs.Save();
        }

        private void OnNextButtonClicked()
        {
            _gameCardIndex = (_gameCardIndex + 1) % _gameUITestCards.Count;
            ShowCard(_gameCardIndex);
        }

        private void OnPreviousButtonClicked()
        {
            _gameCardIndex = (_gameCardIndex - 1 + _gameUITestCards.Count) % _gameUITestCards.Count;
            ShowCard(_gameCardIndex);
        }
    }
}