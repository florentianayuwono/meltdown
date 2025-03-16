using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Scanner : MonoBehaviour
{
    public ParticleSystem scanningParticles;
    public float scanDistance = 1.0f;
    public LayerMask foodLayer;
    
    private GameObject currentTarget;
    
    void Start()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(x => StartScanning());
            grabInteractable.deactivated.AddListener(x => StopScanning());
        }
    }
    
    void Update()
    {
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Quaternion.Euler(0, -90, 0) * transform.forward, out hit, scanDistance, foodLayer))
        {
            currentTarget = hit.collider.gameObject;
        }
        else
        {
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
    }
}