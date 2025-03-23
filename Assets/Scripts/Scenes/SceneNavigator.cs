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

public class SceneNavigator : MonoBehaviour
{
    // Event for food status changes
    public static event Action<FoodItem, FoodStatus> OnFoodStatusChanged;
    
    [SerializeField] private SceneReference supermarketScene;
    [SerializeField] private SceneReference foodWasteRoomScene;
    [SerializeField] private SceneReference tutorialRoomScene;
    [SerializeField] private SceneReference disasterRoomScene;
    [SerializeField] private GameObject subtitleCanvasPrefab;

    public static string DISASTER_EVENT_TYPE;

    private GameObject currentSubtitleCanvas;
    private SceneReference currentScene;

    // Time when the player entered the supermarket - for data collection
    public static DateTime? supermarketEntryTime = null;
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

    public void GoToDisasterRoom(string eventType)
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

            if (!isAnnouncementPlayed) {
                isAnnouncementPlayed = true;
                currentSubtitleCanvas = Instantiate(subtitleCanvasPrefab);

                var subtitleDisplay = currentSubtitleCanvas.GetComponent<SubtitleDisplay>();
                SoundManager.Instance.PlaySound(SoundType.SUPERMARKET_ANNOUCEMENT);
                subtitleDisplay?.ShowSubtitles();

                AudioClip announcementClip = SoundManager.Instance.GetSoundClip(SoundType.SUPERMARKET_ANNOUCEMENT);
                if (announcementClip != null)
                    yield return new WaitForSeconds(announcementClip.length + 0.5f);
            }
            SoundManager.Instance.PlayBackgroundMusic(SoundType.SUPERMARKET_MUSIC, true);
        }
        else if (sceneRef == disasterRoomScene)
        {
            SoundManager.Instance.StopBackgroundMusic(true);
            SoundManager.Instance.StopSound();
            SoundManager.Instance.PlayBackgroundMusic(SoundType.DISASTER_MUSIC, true);
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