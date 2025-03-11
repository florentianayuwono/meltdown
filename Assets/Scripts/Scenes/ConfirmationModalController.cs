using UnityEngine;
using UnityEngine.Events;

public class ConfirmationModal : MonoBehaviour
{
    public UnityEvent onConfirm;
    public UnityEvent onCancel;

    public void Confirm()
    {
        onConfirm?.Invoke();
        gameObject.SetActive(false); // Hide modal
    }

    public void Cancel()
    {
        onCancel?.Invoke();
        gameObject.SetActive(false); // Hide modal
    }
}
