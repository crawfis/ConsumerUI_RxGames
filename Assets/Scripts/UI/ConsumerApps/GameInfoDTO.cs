using System;

namespace RxGames.UI.ConsumerApps
{
    [Serializable]
    public class GameInfoDTO
    {
        public int GameId;
        //public string GameTitle;
        public string GameImagePath; // Texture2D cannot be serialized, so store path or name
        //public string Description;
        //public string TherapyNotes;
        //public List<string> Exercises = new List<string>();
        public int Duration;
        public int NumberOfSets;
        public string TutorialUrl;
    }
}