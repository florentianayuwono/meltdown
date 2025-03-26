using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public enum FoodItem
{
    Tomatoes,
    Milk,
    Meat,
    Eggs
}

public enum FoodStatus
{
    NotDone,           // User hasn't interacted with this food yet
    WrongChoiceChosen, // User made an incorrect choice with this food
    RightChoiceChosen  // User made the correct choice with this food
}

public enum DisasterEventType
{
    NONE,
    WRONG_EGGS,
    WRONG_TOMATO,
    WRONG_MEAT,
    WRONG_MILK
}
public class SceneNavigator : MonoBehaviour
{
    // Event for food status changes
    public static event Action<FoodItem, FoodStatus> OnFoodStatusChanged;

    [SerializeField] private SceneReference supermarketScene;
    [SerializeField] private SceneReference foodWasteRoomScene;
    [SerializeField] private SceneReference tutorialRoomScene;
    [SerializeField] private SceneReference disasterRoomScene;
    [SerializeField] private SceneReference endScene;
    [SerializeField] private GameObject subtitleCanvasPrefab;

    public static DisasterEventType DISASTER_EVENT_TYPE;

    private GameObject currentSubtitleCanvas;
    private SceneReference currentScene;

    // Time when the player entered the supermarket - for data collection
    public static DateTime? supermarketEntryTime = null;
    public static DateTime? foodwasteEntryTime = null;
    private static SceneNavigator instance;
    public static SceneNavigator Instance => instance;

    // Persistent food state using enums
    private static Dictionary<FoodItem, FoodStatus> foodStatusDict = new Dictionary<FoodItem, FoodStatus>()
    {
        { FoodItem.Tomatoes, FoodStatus.NotDone },
        { FoodItem.Milk, FoodStatus.NotDone },
        { FoodItem.Meat, FoodStatus.NotDone },
        { FoodItem.Eggs, FoodStatus.NotDone }
    };

    private static bool isAnnouncementPlayed;
    private static bool isFoodWasteRoomAnnouncementPlayed;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToSupermarket()
    {
        LoadScene(supermarketScene);
    }

    public void GoToFoodWasteRoom()
    {
        LoadScene(foodWasteRoomScene);
    }

    public void GoToTutorialRoom()
    {
        LoadScene(tutorialRoomScene);
    }

    public void GoToEndScene()
    {
        LoadScene(endScene);
    }

    public void GoToDisasterRoom(DisasterEventType eventType)
    {
        DISASTER_EVENT_TYPE = eventType;
        LoadScene(disasterRoomScene);
    }

    public FoodStatus GetFoodStatus(FoodItem food)
    {
        return foodStatusDict[food];
    }

    public void SetFoodStatus(FoodItem food, FoodStatus status)
    {

        // Only update and trigger event if the status actually changed
        if (foodStatusDict[food] != status)
        {
            foodStatusDict[food] = status;

            // Trigger the event
            OnFoodStatusChanged?.Invoke(food, status);
        }
    }

    public bool IsFoodChoiceCorrect(FoodItem food)
    {
        return foodStatusDict[food] == FoodStatus.RightChoiceChosen;
    }

    public bool IsFoodChoiceWrong(FoodItem food)
    {
        return foodStatusDict[food] == FoodStatus.WrongChoiceChosen;
    }

    public bool HasFoodBeenProcessed(FoodItem food)
    {
        return foodStatusDict[food] != FoodStatus.NotDone;
    }

    private void LoadScene(SceneReference sceneRef)
    {
        currentScene = sceneRef;
        StartCoroutine(LoadSceneWithEvents(sceneRef));
    }

    private IEnumerator LoadSceneWithEvents(SceneReference sceneRef)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneRef.BuildIndex);
        while (!asyncLoad.isDone) yield return null;

        if (sceneRef == supermarketScene)
        {
            if (!supermarketEntryTime.HasValue)
            {
                supermarketEntryTime = DateTime.Now;
                Debug.Log("User entered supermarket at: " + supermarketEntryTime.Value);
            }
            SoundManager.Instance.StopBackgroundMusic(true);
            SoundManager.Instance.StopSound();

            if (!isAnnouncementPlayed)
            {
                isAnnouncementPlayed = true;
                currentSubtitleCanvas = Instantiate(subtitleCanvasPrefab);

                var subtitleDisplay = currentSubtitleCanvas.GetComponent<SubtitleDisplay>();
                SoundManager.Instance.PlaySound(SoundType.SUPERMARKET_ANNOUCEMENT);
                subtitleDisplay?.ShowSubtitles();

                AudioClip announcementClip = SoundManager.Instance.GetSoundClip(SoundType.SUPERMARKET_ANNOUCEMENT);
                if (announcementClip != null)
                    yield return new WaitForSeconds(announcementClip.length + 0.5f);
            }

            // Check to see if the scene has changed - if it has then don't play the supermarket bgm
            bool hasSceneChanged = sceneRef.BuildIndex != SceneManager.GetActiveScene().buildIndex;
            if (!hasSceneChanged)
            {
                SoundManager.Instance.PlayBackgroundMusic(SoundType.SUPERMARKET_MUSIC, true);
            }

        }
        else if (sceneRef == disasterRoomScene)
        {
            SoundManager.Instance.StopBackgroundMusic(true);
            SoundManager.Instance.StopSound();
            SoundManager.Instance.PlayBackgroundMusic(SoundType.DISASTER_MUSIC, true);
            switch (DISASTER_EVENT_TYPE)
            {
                case DisasterEventType.WRONG_TOMATO:
                    SoundManager.Instance.PlaySound(SoundType.WRONG_TOMATO_AUDIO, 2f);
                    break;
                case DisasterEventType.WRONG_MILK:
                    SoundManager.Instance.PlaySound(SoundType.WRONG_MILK_AUDIO, 2f);
                    break;
                case DisasterEventType.WRONG_EGGS:
                    SoundManager.Instance.PlaySound(SoundType.WRONG_EGGS_AUDIO, 2f);
                    break;
                case DisasterEventType.WRONG_MEAT:
                    SoundManager.Instance.PlaySound(SoundType.WRONG_MEAT_AUDIO, 2f);
                    break;
                default:
                    break;
            }
        }
        else if (sceneRef == endScene)
        {
            SoundManager.Instance.StopBackgroundMusic(true);
            SoundManager.Instance.StopSound();
            SoundManager.Instance.PlayBackgroundMusic(SoundType.END_MUSIC, true);
            SoundManager.Instance.PlaySound(SoundType.END_AUDIO, 2f);
        } else if (sceneRef == foodWasteRoomScene)
        {
            if (!foodwasteEntryTime.HasValue)
            {
                foodwasteEntryTime = DateTime.Now;
                Debug.Log("User entered food waste room at: " + foodwasteEntryTime.Value);
            }
            SoundManager.Instance.StopBackgroundMusic(true);
            SoundManager.Instance.StopSound();
            SoundManager.Instance.PlaySound(SoundType.FOODWASTE_ANNOUNCEMENT, 2f);
            SoundManager.Instance.PlayBackgroundMusic(SoundType.FOODWASTE_MUSIC, true);
            if (!isFoodWasteRoomAnnouncementPlayed)
            {
                isFoodWasteRoomAnnouncementPlayed = true;
                currentSubtitleCanvas = Instantiate(subtitleCanvasPrefab);

                SubtitleLine[] subtitleLines = new SubtitleLine[]
                {
                    new SubtitleLine { text = "Time to tidy up!", startTime = 0.0f, duration = 1.0f },
                    new SubtitleLine { text = "Not all waste is trash", startTime = 1.0f, duration = 1.5f },
                    new SubtitleLine { text = "—sort wisely, and let’s turn scraps into something useful.", startTime = 2.5f, duration = 4.0f }
                };

                var subtitleDisplay = currentSubtitleCanvas.GetComponent<SubtitleDisplay>();
                subtitleDisplay?.SetSubtitles(subtitleLines);
                subtitleDisplay?.ShowSubtitles();

                AudioClip announcementClip = SoundManager.Instance.GetSoundClip(SoundType.FOODWASTE_ANNOUNCEMENT);
                if (announcementClip != null)
                    yield return new WaitForSeconds(announcementClip.length + 0.5f);
            }
        }
        else
        {
            if (currentSubtitleCanvas != null)
            {
                Destroy(currentSubtitleCanvas);
                currentSubtitleCanvas = null;
            }

            SoundManager.Instance.StopBackgroundMusic(true);
            SoundManager.Instance.StopSound();
        }
    }
}