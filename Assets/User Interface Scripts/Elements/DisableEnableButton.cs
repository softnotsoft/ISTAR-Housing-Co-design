using UnityEngine;
using UnityEngine.UI;

public class DisableEnableButton : MonoBehaviour
{

    public Button disableButton;

    public void whenButtonClicked() {
        if(disableButton.interactable == true) {
            disableButton.interactable = false;
        } else {
            disableButton.interactable = true;
        }
    }
}