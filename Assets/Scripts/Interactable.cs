using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void Interact()
    {
        Debug.Log("Interactuaste con " + gameObject.name);
    }
}
