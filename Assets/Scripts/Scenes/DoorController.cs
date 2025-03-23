using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    [SerializeField] private bool isOpen;
    [SerializeField] private TeleportationType teleportType;
    [SerializeField] private GameObject ConfirmAlertDialog;
    [SerializeField] private ConfirmationModal confirmationModal;

    public InputActionReference triggerAction;

    public enum TeleportationType
    {
        Supermarket,
        FoodWasteRoom,
        TutorialRoom,
        DisasterRoom
    }

    void Awake()
    {
        // Get reference to XR Simple Interactable
        if (interactable == null)
        {
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        if (confirmationModal != null)
        {
            confirmationModal.onConfirm.AddListener(TeleportToSelectedScene);
        }

        if (ConfirmAlertDialog != null)
        {
            ConfirmAlertDialog.SetActive(false);
        }
    }

    void Update()
    {
        if (triggerAction.action.triggered)
        {
            if (isOpen)
            {
                if (ConfirmAlertDialog != null)
                {
                    ConfirmAlertDialog.SetActive(true);
                }
            }
        }
    }

    private void TeleportToSelectedScene()
    {
        // Start coroutine for sound and teleportation
        StartCoroutine(PlaySoundAndTeleport());
    }

    private IEnumerator PlaySoundAndTeleport()
    {
        // First, play the door open sound
        FindObjectOfType<SoundManager>().PlaySound(SoundType.DOOR_OPEN);

        // Optional: Wait a short time to let the sound play
        yield return new WaitForSeconds(0.5f);

        // Make sure SceneNavigator singleton exists
        if (SceneNavigator.Instance == null)
        {
            Debug.LogError("SceneNavigator instance not found!");
            yield break;
        }

        // Call the appropriate method based on selected teleport type
        switch (teleportType)
        {
            case TeleportationType.Supermarket:
                SceneNavigator.Instance.GoToSupermarket();
                break;
            case TeleportationType.FoodWasteRoom:
                SceneNavigator.Instance.GoToFoodWasteRoom();
                break;
            case TeleportationType.TutorialRoom:
                SceneNavigator.Instance.GoToTutorialRoom();
                break;
            case TeleportationType.DisasterRoom:
                SceneNavigator.Instance.GoToDisasterRoom("");
                break;
        }
    }
}
