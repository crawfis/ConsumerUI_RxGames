using System.Collections.Generic;

using UnityEngine;

namespace RxGames.ConsumerApps
{
    [CreateAssetMenu(fileName = "GameCardInfo", menuName = "RxGames/Game Info")]
    internal class GameInfoScriptableObject : ScriptableObject
    {
        [SerializeField] public int GameId;
        //[SerializeField] public string GameTitle;
        [SerializeField] public Sprite GameImage;
        //[SerializeField] public string Description;
        //[SerializeField] public string TherapyNotes;
        //[SerializeField] public List<string> Exercises = new List<string>();
        [SerializeField] public int Duration;
        [SerializeField] public int NumberOfSets;
        [SerializeField] public string TutorialUrl = "https://rxgames.com";
        [SerializeField] public string GameImagePath;
    }
}