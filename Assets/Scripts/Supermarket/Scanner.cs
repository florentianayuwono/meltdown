using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Scanner : MonoBehaviour
{
    public ParticleSystem scanningParticles;
    public float scanDistance = 1.0f;
    public LayerMask foodLayer;
    
    private GameObject currentTarget;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Awake()
    {
        // Store the original position and rotation when the object is created
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }
    
    void Start()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null)
        {
            // Add listeners for selection events
            interactable.selectEntered.AddListener(OnSelectEnter);
            interactable.selectExited.AddListener(OnSelectExit);
            
            // Keep the activation listeners for scanning functionality
            var grabInteractable = interactable as UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable;
            if (grabInteractable != null)
            {
                grabInteractable.activated.AddListener(x => StartScanning());
                grabInteractable.deactivated.AddListener(x => StopScanning());
            }
        }
    }
    
    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        // Get the interactor from the args
        var interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;
        if (interactor != null)
        {
            // For newer XR Interaction Toolkit versions
            if (interactor.GetType().GetProperty("selectActionTrigger") != null)
            {
                // Use reflection to set the property if it exists
                interactor.GetType().GetProperty("selectActionTrigger").SetValue(interactor, 3);
            }
            
            // For poke interactors or other types
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor pokeInteractor)
            {
                // Set appropriate properties for the poke interactor to make it sticky
                var field = pokeInteractor.GetType().GetField("m_SelectActionTrigger", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(pokeInteractor, 3); 
                }
            }
        }
    }
    
    private void OnSelectExit(SelectExitEventArgs args)
    {
        // Get the interactor from the args
        var interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;
        if (interactor != null)
        {
            // For newer XR Interaction Toolkit versions
            if (interactor.GetType().GetProperty("selectActionTrigger") != null)
            {
                // Use reflection to set the property if it exists
                interactor.GetType().GetProperty("selectActionTrigger").SetValue(interactor, 0); // 0 = StateChange in enum
            }
            
            // For poke interactors or other types
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor pokeInteractor)
            {
                // Reset appropriate properties for the poke interactor
                var field = pokeInteractor.GetType().GetField("m_SelectActionTrigger", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(pokeInteractor, 0); // 0 = StateChange in enum
                }
            }
        }
        StartCoroutine(ReturnToPosition());
    }

    private IEnumerator ReturnToPosition()
    {
        // Make sure any physics interactions are disabled during the return
        Rigidbody rb = GetComponent<Rigidbody>();
        bool wasKinematic = false;
        
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }
        
        // Animate the return over a short time
        float duration = 0.3f;
        float elapsed = 0;
        
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Use smoothstep or other easing function for smoother motion
            float smoothT = t * t * (3f - 2f * t);
            
            transform.position = Vector3.Lerp(startPosition, originalPosition, smoothT);
            transform.rotation = Quaternion.Slerp(startRotation, originalRotation, smoothT);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure it ends exactly at the original position
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        // Restore the original rigidbody settings
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }
    }
    
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Quaternion.Euler(0, -90, 0) * transform.forward, out hit, scanDistance, foodLayer))
        {
            if (currentTarget != hit.collider.gameObject)
            {
                if (currentTarget != null)
                {
                    StopScanning();
                }
            }
            currentTarget = hit.collider.gameObject;
        }
        else
        {
            if (currentTarget != null)
            {
                StopScanning();
            }
            currentTarget = null;
        }
    }

    public void StartScanning()
    {
        scanningParticles.Play();
        
        // If we have a valid food item target
        if (currentTarget != null)
        {
            // Try to get ScanableFood component from hit object or its parents
            ScanableFood scanableFood = currentTarget.GetComponent<ScanableFood>();
            if (scanableFood == null)
            {
                scanableFood = currentTarget.GetComponentInParent<ScanableFood>();
            }
            
            // If we found a scanable food, tell it to start scanning
            if (scanableFood != null)
            {
                scanableFood.StartScan();
            }
        }
    }

    public void StopScanning()
    {
        scanningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // If we have a valid food item target
        if (currentTarget != null)
        {
            // Try to get ScanableFood component from hit object or its parents
            ScanableFood scanableFood = currentTarget.GetComponent<ScanableFood>();
            if (scanableFood == null)
            {
                scanableFood = currentTarget.GetComponentInParent<ScanableFood>();
            }
            
            // If we found a scanable food, tell it to start scanning
            if (scanableFood != null)
            {
                scanableFood.StopScan();
            }
        }
    }
}