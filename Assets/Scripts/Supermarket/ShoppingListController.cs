using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingListController : MonoBehaviour
{
    [Header("Teleportation")]
    [SerializeField] private GameObject teleportationTarget;
    [SerializeField] private float transitionDuration = 1.0f;

    [Header("UI References")]
    [SerializeField] private Image tomatoImage;
    [SerializeField] private Image eggsImage;
    [SerializeField] private Image meatImage;
    [SerializeField] private Image milkImage;
    
    [Header("Status Colors")]
    [SerializeField] private Color defaultColor = Color.blue;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    
    private string saveFileName = "supermarket_data.csv";
    
    private Dictionary<FoodItem, Image> foodImages = new Dictionary<FoodItem, Image>();
    private bool allChoicesMade = false;

    private void Awake()
    {
        // Set up the dictionary mapping food types to their UI images
        foodImages[FoodItem.Tomatoes] = tomatoImage;
        foodImages[FoodItem.Eggs] = eggsImage;
        foodImages[FoodItem.Meat] = meatImage;
        foodImages[FoodItem.Milk] = milkImage;

        if (teleportationTarget != null)
        {
            teleportationTarget.SetActive(false);
        }
    
    }

    private void Start()
    {
        // Initialize UI with default colors
        ResetAllColors();
        
        // Register for food status change events in SceneNavigator
        SceneNavigator.OnFoodStatusChanged += HandleFoodStatusChanged;
        
        // Update UI based on any existing food statuses
        UpdateAllFoodStatuses();
    }

    private void OnDestroy()
    {
        // Unregister from events when this object is destroyed
        if (SceneNavigator.Instance != null)
        {
            SceneNavigator.OnFoodStatusChanged -= HandleFoodStatusChanged;
        }
    }
    
    private void ResetAllColors()
    {
        foreach (Image image in foodImages.Values)
        {
            if (image != null)
            {
                image.color = defaultColor;
            }
        }
    }
    
    public void UpdateAllFoodStatuses()
    {
        if (SceneNavigator.Instance == null) return;
        
        foreach (FoodItem foodItem in foodImages.Keys)
        {
            Image image = foodImages[foodItem];
            if (image == null) continue;
            
            FoodStatus status = SceneNavigator.Instance.GetFoodStatus(foodItem);
            UpdateFoodImageColor(foodItem, status);
        }
        
        CheckAllChoicesMade();
    }
    
    private void HandleFoodStatusChanged(FoodItem foodItem, FoodStatus status)
    {
        // Update UI color for the changed food item
        UpdateFoodImageColor(foodItem, status);
        
        // Check if all choices have been made
        CheckAllChoicesMade();
    }
    
    private void UpdateFoodImageColor(FoodItem foodItem, FoodStatus status)
    {
        if (!foodImages.TryGetValue(foodItem, out Image image) || image == null)
            return;
            
        switch (status)
        {
            case FoodStatus.RightChoiceChosen:
                image.color = correctColor;
                break;
            case FoodStatus.WrongChoiceChosen:
                image.color = wrongColor;
                break;
            case FoodStatus.NotDone:
                image.color = defaultColor;
                break;
        }
    }
    
    private void CheckAllChoicesMade()
    {
        // If we already determined all choices were made, no need to check again
        if (allChoicesMade) return;
        
        if (SceneNavigator.Instance == null) return;
        
        bool allDone = true;
        
        // Check if all food items have a status other than NotDone
        foreach (FoodItem foodItem in SceneNavigator.foodStatusDict.Keys)
        {
            FoodStatus status = SceneNavigator.Instance.GetFoodStatus(foodItem);
            if (status == FoodStatus.NotDone)
            {
                allDone = false;
                break;
            }
        }
        
        if (allDone)
        {
            allChoicesMade = true;
            
            // Save the data to a file
            SaveDataToFile();

            ActivateSpecialLightOnly();
        }
    }


public void SaveDataToFile()
{
    try
    {
        // Create the directory if it doesn't exist
        string directory = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Data");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Full path with platform-safe directory separators
        string filePath = Path.Combine(directory, saveFileName);
        bool fileExists = File.Exists(filePath);
        
        // Create a StringBuilder to build our CSV content
        StringBuilder sb = new StringBuilder();
        
        // If file doesn't exist, add header row first
        if (!fileExists)
        {
            sb.AppendLine("tomatoes,meat,eggs,milk,total,time_seconds,entry_timestamp,save_timestamp");
        }

        // Calculate elapsed time since entering supermarket
        TimeSpan? elapsedTime = null;
        if (SceneNavigator.supermarketEntryTime != null)
        {
            elapsedTime = DateTime.Now - SceneNavigator.supermarketEntryTime.Value;
        }
        
        // Add the current data to the CSV, 0 if the food item is wrong and 1 if it's correct
        int tomatoes = SceneNavigator.Instance.GetFoodStatus(FoodItem.Tomatoes) == FoodStatus.RightChoiceChosen ? 1 : 0;
        int meat = SceneNavigator.Instance.GetFoodStatus(FoodItem.Meat) == FoodStatus.RightChoiceChosen ? 1 : 0;
        int eggs = SceneNavigator.Instance.GetFoodStatus(FoodItem.Eggs) == FoodStatus.RightChoiceChosen ? 1 : 0;
        int milk = SceneNavigator.Instance.GetFoodStatus(FoodItem.Milk) == FoodStatus.RightChoiceChosen ? 1 : 0;
        
        int total = tomatoes + meat + eggs + milk;

        string entryTime = SceneNavigator.supermarketEntryTime.HasValue ? SceneNavigator.supermarketEntryTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A";
        string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string timeSeconds = elapsedTime.HasValue ? elapsedTime.Value.TotalSeconds.ToString("F1") : "N/A";
        
        sb.AppendLine($"{tomatoes},{meat},{eggs},{milk},{total},{timeSeconds},{entryTime},{saveTime}");
        
        // Append to file (create if doesn't exist)
        File.AppendAllText(filePath, sb.ToString());
        
        Debug.Log($"Data successfully saved to {filePath}");
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to save data: {e.Message}");
    }
}

    public void ActivateSpecialLightOnly()
    {
        StartCoroutine(TransitionToSpecialLight());
    }

    private IEnumerator TransitionToSpecialLight()
    {
        // First, find and store all lights in the scene
        Light[] allLights = FindObjectsOfType<Light>();
        List<Light> otherLights = new List<Light>();
        List<float> originalIntensities = new List<float>();

        float startTime = Time.time;
        float elapsedTime = 0f;
        
        // Separate the special light from other lights
        Light specialLight = null;
        if (teleportationTarget != null)
        {
            specialLight = teleportationTarget.GetComponentInChildren<Light>();
            
            // Activate the special light object if it was inactive
            if (!teleportationTarget.activeSelf)
            {
                teleportationTarget.SetActive(true);
            }
        }
        
        // Store all other lights and their original intensities
        foreach (Light light in allLights)
        {
            if (light != specialLight)
            {
                otherLights.Add(light);
                originalIntensities.Add(light.intensity);
            }
        }
        
        // Gradually fade out other lights while fading in the special light
        while (elapsedTime < transitionDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = elapsedTime / transitionDuration;

            // Fade in special light
            if (specialLight != null)
            {
                specialLight.intensity = Mathf.Lerp(0f, 10f, t);
            }
            
            // Fade out other lights
            for (int i = 0; i < otherLights.Count; i++)
            {
                if (otherLights[i] != null)
                {
                    otherLights[i].intensity = Mathf.Lerp(originalIntensities[i], 0f, t);
                }
            }
            
            yield return null;
        }
        
        // Ensure all other lights are completely off
        foreach (Light light in otherLights)
        {
            if (light != null)
            {
                light.intensity = 0f;
            }
        }
        
        Debug.Log("All lights turned off except for the special light.");
    }
}