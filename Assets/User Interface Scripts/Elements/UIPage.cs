using UnityEngine;
using UnityEngine.Events;

public class UIPage : UIElement
{
    public UIManager uiManager;

    [SerializeField]
    protected UIPages pageName;

    public UnityEvent onEnter;
    public UnityEvent onExit;

    void Awake()
    {
        if(onEnter == null)
            onEnter = new UnityEvent();
        if(onExit == null) 
            onExit = new UnityEvent();

        gameObject.SetActive(false);

        uiManager.onPageChanged.AddListener(HandlePageChange);
        HandlePageChange(uiManager.GetCurrentPage());
    }

    void OnDestroy()
    {
        uiManager.onPageChanged.RemoveListener(HandlePageChange);
    }

    protected void HandlePageChange(UIPages newPage)
    {
        if (pageName == newPage)
        {
            ShowSelf();
            onEnter?.Invoke();
            return;
        }

        if (gameObject.activeSelf)
        {
            onExit?.Invoke();
            HideSelf();
        }
    }
}
