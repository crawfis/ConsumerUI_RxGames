using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UIElements;

namespace RxGames.UI.ConsumerApps
{
    [UxmlElement("game-ui-card")]
    public partial class GameUICard : VisualElement
    {
        const string LocalizedTableBaseName = "GameUICardG";
        private string _localizedTable = "GameUICardG2";
        const string _staticLocalizedTable = "ConsumerAppLabels";

        public int GameID { get; set; }

        [UxmlAttribute]
        public string GameTitle { get; set; }
        [UxmlAttribute]
        public Sprite GameImage { get; set; }
        [UxmlAttribute]
        public string Description { get; set; }
        [UxmlAttribute]
        public string TherapyNotes { get; set; }
        //[UxmlAttribute("ListOfExercises")]
        //[SerializeField] public List<string> _exercises = new List<string>();
        [UxmlAttribute("GameDuration")]
        [SerializeField] public int _duration;
        [UxmlAttribute("Sets")]
        [SerializeField] public int _numberOfSets;
        [UxmlAttribute("TutorialUrl")]
        [SerializeField] public string _tutorialUrl = "https://rxgames.com";

        public event System.Action<int> OnSetsChanged;

        public GameUICard()
        {
            // Initialize the card with default styles or properties
            pickingMode = PickingMode.Ignore;
        }

        /// <summary>
        /// Creates a VisualElement from GameUICard.uxml and populates it with this instance's data.
        /// </summary>
        public VisualElement CreateVisualElementFromUXML()
        {
            // Load the UXML asset (must be in a Resources folder for runtime loading)
            var visualTree = Resources.Load<VisualTreeAsset>("GameUICard");
            if (visualTree == null)
            {
                Debug.LogError("GameUICard.uxml not found in Resources");
                return new Label("UXML not found");
            }
            var root = visualTree.CloneTree();
            _localizedTable = LocalizedTableBaseName + GameID;
            // Find and populate UI elements by name
            var titleLabel = root.Q<Label>("game-title-label");
            //if (titleLabel != null) titleLabel.text = GameTitle ?? "Default Game Title";

            if (titleLabel != null)
            {
                var localizedString = new LocalizedString(_localizedTable, "GameTitle");
                titleLabel.SetBinding("text", localizedString);
            }

            var descriptionLabel = root.Q<Label>("game-description-label");
            //if (descriptionLabel != null) descriptionLabel.text = Description ?? "Junk";
            if (descriptionLabel != null)
            {
                var localizedString = new LocalizedString(_localizedTable, "DescriptionText");
                descriptionLabel.SetBinding("text", localizedString);
            }

            var therapyLabel = root.Q<Label>("therapy-label");
            //if (therapyLabel != null) therapyLabel.text = TherapyNotes ?? "";
            if (therapyLabel != null)
            {
                var localizedString = new LocalizedString(_localizedTable, "TherapyNotes");
                therapyLabel.SetBinding("text", localizedString);
            }

            var exerciseLabel = root.Q<Label>("game-exercise-list-label");
            if (exerciseLabel != null)
            {
                var localizedLabel = new LocalizedString(_staticLocalizedTable, "ExercisesLabel");
                var localizedString = new LocalizedString(_localizedTable, "ExerciseList");
                StringBuilder stringBuilder = new StringBuilder($"<b>{localizedLabel.GetLocalizedString()}</b>");
                var exercises = localizedString.GetLocalizedString().Split(';');
                foreach (var exercise in exercises)
                {
                    stringBuilder.Append("<br>  ");
                    stringBuilder.Append('•');
                    stringBuilder.Append(" ");
                    //stringBuilder.Append(localizedString.GetLocalizedString());
                    stringBuilder.Append(exercise.Trim());
                }
                exerciseLabel.text = stringBuilder.ToString();
                //exerciseLabel.SetBinding("text", stringBuilder.ToString());
            }
            var durationLabel = root.Q<Label>("duration-label");
            bool plural = this._duration > 1 ? true : false;
            //if (durationLabel != null) durationLabel.text = $"Set duration: {this._duration} minute{plural}" ?? "Error";
            if (durationLabel != null)
            {
                LocalizedString localizedString = (plural) ? new LocalizedString(_staticLocalizedTable, "DurationLabelPlural") : new LocalizedString(_staticLocalizedTable, "DurationLabel");
                //var dict = new Dictionary<string, string>() { { "duration", this._duration.ToString() } };
                localizedString.Arguments = new object[] { this._duration };
                //var duration = localizedString["duration"] as IntVariable;
                //duration.Value = this._duration;
                var durationLocalized = localizedString.GetLocalizedString();
                durationLabel.SetBinding("text", localizedString);
            }

            // You can add more fields as needed, e.g. duration, sets, tutorialUrl
            // For images, you may need to set the backgroundImage style of a VisualElement
            var imageContainer = root.Q<VisualElement>("game-demo-image");
            if (imageContainer != null && GameImage != null)
                imageContainer.style.backgroundImage = new StyleBackground(GameImage);

            // Duration and sets (if you add labels for these in UXML)
            //var durationLabel = root.Q<SliderInt>("game-duration-slider");
            //if (durationLabel != null) durationLabel.value = _duration;

            var setsSlider = root.Q<SliderInt>("game-sets-slider");
            if (setsSlider != null)
            {
                setsSlider.value = _numberOfSets;
                setsSlider.SetEnabled(true);
                setsSlider.RegisterValueChangedCallback(evt =>
                {
                    _numberOfSets = evt.newValue;
                    OnSetsChanged?.Invoke(_numberOfSets);
                });
            }
            // Tutorial URL (if you add a label for this in UXML)
            var tutorialLink = root.Q<Label>("game-tutorial-label");
            if (tutorialLink != null)
            {
                var localizedString = new LocalizedString(_staticLocalizedTable, "TutorialsLabel");
                //tutorialLink.SetBinding("text", localizedString);
                string tutorialText = @"<a href=""" + _tutorialUrl + @"""><u>" + localizedString.GetLocalizedString() + @"</u></a>";
                tutorialLink.text = tutorialText;
                tutorialLink.tooltip = "Click to view tutorials";
            }
            return root;
        }
    }
}