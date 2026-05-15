using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual void Interact()
    {
        Debug.Log("Interactuaste con " + gameObject.name);
    }
}
