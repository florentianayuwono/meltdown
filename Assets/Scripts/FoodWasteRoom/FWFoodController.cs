using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FWFoodController : MonoBehaviour
{
    [Header("Food Properties")]
    public FoodItem foodType;

    [Header("References")]
    public Transform CorrectPlaceToThrow;
    public Transform IncorrectPlaceToThrow;
    public Transform[] IncorrectPlacements;
    public GameObject confettiPrefab;
    public DisasterEventType eventType;

    public GameObject successVersion;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Collider[] colliders;

    void Awake()
    {
        if (successVersion != null)
        {
            successVersion.SetActive(false); // hide the success version at the start
        }
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();

        // Check current food status
        if (SceneNavigator.Instance != null)
        {
            FoodStatus status = SceneNavigator.Instance.GetFoodStatus(foodType);

            // wrong choice == not chosen food here in food waste room will all go back to their original position and resume functionality
            // so only right choice food will be made non-interactable
            if (status == FoodStatus.RightChoiceChosen)
            {
                StartCoroutine(UpdateCorrectChoice());
            } 
            else if (status == FoodStatus.WrongChoiceChosen)
            {
                Destroy(gameObject); // destroy the game object
            }
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }
    }

    public void MakeNonInteractable()
    {
        // Disable the grab interactable
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // Make the rigidbody kinematic
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Disable colliders or set them to trigger
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        Vector3 foodPos = transform.position;
        Vector3 correctPos = CorrectPlaceToThrow.position;

        float xThreshold = 0.5f;
        float zThreshold = 0.5f;

        bool isAboveCorrectXZ = Mathf.Abs(foodPos.x - correctPos.x) <= xThreshold &&
                             Mathf.Abs(foodPos.z - correctPos.z) <= zThreshold;

        Vector3 incorrectPlace = IncorrectPlaceToThrow.position;

        bool isAboveIncorrectXZ = Mathf.Abs(foodPos.x - incorrectPlace.x) <= xThreshold &&
                             Mathf.Abs(foodPos.z - incorrectPlace.z) <= zThreshold;


        if (isAboveCorrectXZ)
        {
            StartCoroutine(HandleCorrectDropSequence());
        }
        else if (isAboveIncorrectXZ)
        {
            HandleIncorrectDrop();
        }
        else
        {
            bool isAboveAnyIncorrect = false;
            foreach (Transform incorrectPlacement in IncorrectPlacements)
            {
                Vector3 incorrectPos = incorrectPlacement.position;
                if (Mathf.Abs(foodPos.x - incorrectPos.x) <= xThreshold &&
                    Mathf.Abs(foodPos.z - incorrectPos.z) <= zThreshold)
                {
                    isAboveAnyIncorrect = true;
                    break;
                }
            }
            if (isAboveAnyIncorrect)
            {
                SubtitleLine[] subtitleLines = new SubtitleLine[]
                {
                    new SubtitleLine { text = "Oops wrong spot, try look around?", startTime = 0.0f, duration = 2.0f }
                };
                SceneNavigator.Instance?.ShowSubtitleCanvas(subtitleLines);
            }
            StartCoroutine(ReturnToOriginalPosition());
        }
    }

    private IEnumerator HandleCorrectDropSequence()
    {
        SoundManager.Instance.PlaySound(SoundType.CORRECT_ITEM_PLACED);
        ResultsManager.Instance?.UpdateScore(foodType, true);

        GameObject confetti = Instantiate(confettiPrefab, transform.position, Quaternion.identity);

        SceneNavigator.Instance?.SetFoodStatus(foodType, FoodStatus.RightChoiceChosen);

        yield return new WaitForSeconds(1.0f);
        Destroy(confetti, 1.0f);

        StartCoroutine(UpdateCorrectChoice());
    }

    private IEnumerator UpdateCorrectChoice()
    {
        MakeNonInteractable();
        if (foodType == FoodItem.CoffeeGrounds)
        {
            // stay on the tray but not interactable
            // update original position and rotation to the current position and rotation
            Destroy(gameObject); // destroy the game object
            successVersion.SetActive(true); // show the success version

        } else
        {
            Destroy(gameObject); 
        }
        // added to fix compile error
        yield return null;
    }

    private void HandleIncorrectDrop()
    {
        // Destroy this wrong food item
        Destroy(gameObject);

        SceneNavigator.Instance?.SetFoodStatus(foodType, FoodStatus.WrongChoiceChosen);

        ResultsManager.Instance?.UpdateScore(foodType, false);
        SceneNavigator.Instance?.GoToDisasterFWRoom(eventType);
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        if (rb != null) rb.isKinematic = true;

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, originalPosition, smoothT);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (rb != null) rb.isKinematic = false;
    }
}