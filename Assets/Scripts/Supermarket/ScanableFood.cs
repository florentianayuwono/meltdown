using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanableFood : MonoBehaviour
{
    [Tooltip("Material to apply when being scanned")]
    public Material scanningMaterial;
    [SerializeField] private GameObject[] scanLabels;
    
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private bool isBeingScanned = false;
    
    public void StartScan()
    {
        if (!isBeingScanned)
        {
            // Apply scanning material
            ApplyScanningMaterial();
            isBeingScanned = true;
            
            // Play scan sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(SoundType.SUPERMARKET_SCANNER);
            }

            // Show scan labels
            foreach (GameObject scanLabel in scanLabels)
            {
                scanLabel.SetActive(true);
            }
        }
    }
    
    public void StopScan()
    {
        if (isBeingScanned)
        {
            // Restore original materials
            RestoreOriginalMaterials();
            isBeingScanned = false;
            
            // Hide scan labels
            foreach (GameObject scanLabel in scanLabels)
            {
                scanLabel.SetActive(false);
            }
        }
    }
    
    private void ApplyScanningMaterial()
    {
        // Store original materials and apply scanning material
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Save original materials
            originalMaterials[renderer] = renderer.materials;
            
            // Create new materials array of the same length
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = scanningMaterial;
            }
            
            // Apply new materials
            renderer.materials = newMaterials;
        }
    }
    
    private void RestoreOriginalMaterials()
    {
        // Restore original materials
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && originalMaterials.ContainsKey(renderer))
            {
                renderer.materials = originalMaterials[renderer];
            }
        }
        
        // Clear the dictionary
        originalMaterials.Clear();
    }
    
    // In case the object is destroyed while being scanned
    private void OnDestroy()
    {
        if (isBeingScanned)
        {
            RestoreOriginalMaterials();
        }
    }
}