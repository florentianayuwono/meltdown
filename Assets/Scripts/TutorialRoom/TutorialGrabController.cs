using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class TutorialGrabController : MonoBehaviour
{
    public float grabDistance = 0.2f;
    public Transform leftHand; 
    public Transform rightHand;   
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody rb;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (leftHand == null)
            leftHand = GameObject.Find("LeftHand Controller")?.transform;
        if (rightHand == null)
            rightHand = GameObject.Find("RightHand Controller")?.transform;

        // Store the original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();

        // Add listener for when object is released
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }
    }

    void Update()
    {
        if (grabInteractable == null || leftHand == null || rightHand == null) return;

        float leftDist = Vector3.Distance(transform.position, leftHand.position);
        float rightDist = Vector3.Distance(transform.position, rightHand.position);

        bool isClose = leftDist <= grabDistance || rightDist <= grabDistance;
        grabInteractable.enabled = isClose;
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        StartCoroutine(ReturnToOriginalPosition());
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

    private void OnDestroy()
    {
        // Clean up listener when destroyed
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.RemoveListener(OnSelectExit);
        }
    }
}