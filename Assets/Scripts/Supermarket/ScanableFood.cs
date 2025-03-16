using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanableFood : MonoBehaviour
{
    [Tooltip("Material to apply when being scanned")]
    public Material scanningMaterial;
    
    [Tooltip("Duration to show the scanning material")]
    public float scanDuration = 0.3f;
    
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Coroutine scanCoroutine;
    
    public void StartScan()
    {
        // If already scanning, stop the current scan
        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
        }
        
        // Start new scan
        scanCoroutine = StartCoroutine(ShowScanEffect());
        
        // Play scan sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.SUPERMARKET_SCANNER);
        }
    }
    
    private IEnumerator ShowScanEffect()
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
        
        // Wait for scan duration
        yield return new WaitForSeconds(scanDuration);
        
        // Restore original materials
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && originalMaterials.ContainsKey(renderer))
            {
                renderer.materials = originalMaterials[renderer];
            }
        }
        
        // Clear the dictionary
        originalMaterials.Clear();
        scanCoroutine = null;
    }
}