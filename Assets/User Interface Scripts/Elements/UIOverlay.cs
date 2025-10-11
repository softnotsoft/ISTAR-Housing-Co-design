using System.Collections.Generic;
using UnityEngine;

public class UIOverlay : UIElement
{
    public UIManager uiManager;

    [SerializeField]
    protected UIOverlays overlayName;

    void Awake()
    {
        gameObject.SetActive(false);

        uiManager.onOverlayChanged.AddListener(HandleOverlayChange);
    }

    void OnDestroy()
    {
        uiManager.onOverlayChanged.RemoveListener(HandleOverlayChange);
    }

    protected void HandleOverlayChange(List<UIOverlays> newOverlay)
    {
        if (newOverlay.Contains(overlayName))
        {
            ShowSelf();
            return;
        }

        if (gameObject.activeSelf)
        {
            HideSelf();
        }
    }
}
