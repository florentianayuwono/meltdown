using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorController : MonoBehaviour
{
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    [SerializeField] private bool isOpen;
    [SerializeField] private TeleportationType teleportType;
    [SerializeField] private GameObject ConfirmAlertDialog;
    [SerializeField] private ConfirmationModal confirmationModal;

    public enum TeleportationType
    {
        Supermarket,
        Kitchen,
        TutorialRoom,
        DisasterRoom
    }

    void Awake()
    {
        // Get reference to XR Simple Interactable
        if (interactable == null) {
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        // Subscribe to the select event (trigger press)
        interactable.selectEntered.AddListener(OnTriggerPressed);

        if (confirmationModal != null)
        {
            confirmationModal.onConfirm.AddListener(TeleportToSelectedScene);
        }
        
        if (ConfirmAlertDialog != null)
        {
            ConfirmAlertDialog.SetActive(false);
        }
    }

    private void OnTriggerPressed(SelectEnterEventArgs args)
    {
        // Action when trigger is pressed
        if (isOpen) {
            if (ConfirmAlertDialog != null)
            {
                ConfirmAlertDialog.SetActive(true);
            }
        }
        
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            // Unsubscribe to prevent memory leaks
            interactable.selectEntered.RemoveListener(OnTriggerPressed);
        }
    }

    private void TeleportToSelectedScene()
    {
        // Make sure SceneNavigator singleton exists
        if (SceneNavigator.Instance == null)
        {
            Debug.LogError("SceneNavigator instance not found!");
            return;
        }
        
        // Call the appropriate method based on selected teleport type
        switch (teleportType)
        {
            case TeleportationType.Supermarket:
                SceneNavigator.Instance.GoToSupermarket();
                break;
            case TeleportationType.Kitchen:
                SceneNavigator.Instance.GoToKitchen();
                break;
            case TeleportationType.TutorialRoom:
                SceneNavigator.Instance.GoToTutorialRoom();
                break;
            case TeleportationType.DisasterRoom:
                SceneNavigator.Instance.GoToDisasterRoom();
                break;
        }
    }
}
