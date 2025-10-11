 using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    public GameObject EnablingObject;

    public void whenButtonClicked() {
        if (EnablingObject.activeInHierarchy == true) {
            EnablingObject.SetActive(false);
        } else {
            EnablingObject.SetActive(true);
        }
    }
}
