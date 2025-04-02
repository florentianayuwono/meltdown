using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PosterInteraction : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable; // Assign in Inspector (XR Simple Interactable)
    public GameObject highlightObject; // Assign in Inspector
    public GameObject overlay; // Assign in Inspector

    public Button closeButton;
    public InputActionReference triggerAction;

    public float hoverHapticIntensity = 0.2f;
    public float clickHapticIntensity = 0.8f;
    public float hapticDuration = 0.1f;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor interactor; // Reference to XR Interactor

    private PosterOverlayManager overlayManager;
    private bool isHovered = false;
    void Start()
    {
        if (interactable == null)
        {
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        }

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }
        else
        {
            Debug.LogWarning("XR Simple Interactable not found on " + gameObject.name);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideOverlay);
        }
        else
        {
            Debug.LogWarning("Close Button is not assigned!");
        }

        // Hide highlight and overlay at start
        if (highlightObject != null) highlightObject.SetActive(false);
        if (overlay != null) overlay.SetActive(false);

        overlayManager = Object.FindFirstObjectByType<PosterOverlayManager>();
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (highlightObject != null) highlightObject.SetActive(true);

        isHovered = true;
        interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
        if (interactor != null) TriggerHapticFeedback(interactor, hoverHapticIntensity);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (highlightObject != null) highlightObject.SetActive(false);
        isHovered = false;
    }

    // private void OnTriggerPressed(SelectEnterEventArgs args)
    // {

    //     if (overlay != null) overlay.SetActive(true);

    //     interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
    //     if (interactor != null) TriggerHapticFeedback(interactor, clickHapticIntensity);

    // }

    void Update()
    {
        if (isHovered && triggerAction.action.triggered)
        {
            if (overlayManager != null)
            {
                overlayManager.HideAllOverlays();
            }


            if (overlay != null)
            {
                overlay.SetActive(true);
            }
        }
    }

    private void TriggerHapticFeedback(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor interactor, float intensity)
    {
        if (interactor.xrController != null)
        {
            interactor.xrController.SendHapticImpulse(intensity, hapticDuration);
        }
    }

    public void HideOverlay()
    {
        if (overlay != null)
        {
            overlay.SetActive(false);
        }
    }
}
