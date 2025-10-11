using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class UIButton : UIElement
{
    private Button button;

    void Awake()
    {
        button = gameObject.GetComponent<Button>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(HandleButtonClick);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(HandleButtonClick);
    }

    public abstract void HandleButtonClick();
}
