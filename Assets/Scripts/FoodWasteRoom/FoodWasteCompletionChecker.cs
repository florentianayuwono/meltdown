using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class FoodWasteCompletionChecker : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 1.0f;
    private void Start()
    {
        // Ensure the object starts disabled
        gameObject.SetActive(false);

        // Subscribe to the food status changed event
        SceneNavigator.OnFoodStatusChanged += OnFoodStatusChanged;

        // Check current status in case items are already completed
        CheckCompletionStatus();
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when destroyed
        SceneNavigator.OnFoodStatusChanged -= OnFoodStatusChanged;
    }

    private void OnFoodStatusChanged(FoodItem foodItem, FoodStatus newStatus)
    {
        // Only check completion when food from the waste room changes status
        if (System.Array.IndexOf(SceneNavigator.foodWasteRoomFoodItems, foodItem) >= 0)
        {
            CheckCompletionStatus();
        }
    }

    private void CheckCompletionStatus()
    {
        bool allCompleted = true;

        // Check if all food waste room items are no longer in NotDone state
        foreach (FoodItem item in SceneNavigator.foodWasteRoomFoodItems)
        {
            if (SceneNavigator.Instance.GetFoodStatus(item) == FoodStatus.NotDone)
            {
                allCompleted = false;
                break;
            }
        }

        // Activate this GameObject when all items are completed
        if (allCompleted && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            ResultsManager.Instance.StopTrackingFoodWasteTime();
            ActivateSpecialLightOnly();
            // play door open voice after 1 seconds
            StartCoroutine(PlayDoorOpenVoice());
        }
    }
    public void ActivateSpecialLightOnly()
    {
        StartCoroutine(TransitionToSpecialLight());
    }

    private IEnumerator PlayDoorOpenVoice()
    {
        yield return new WaitForSeconds(1.0f);
        SoundManager.Instance.PlaySound(SoundType.DOOR_OPEN_VOICE, 2f);
    }

    private IEnumerator TransitionToSpecialLight()
    {
        // First, find and store all lights in the scene
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        List<Light> otherLights = new List<Light>();
        List<float> originalIntensities = new List<float>();

        float startTime = Time.time;
        float elapsedTime = 0f;

        // Separate the special light from other lights
        Light specialLight = null;
        specialLight = gameObject.GetComponentInChildren<Light>();

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
    }
}
