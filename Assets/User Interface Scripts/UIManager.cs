using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UI Manager", menuName = "Scriptable Objects/UI Manager")]
public class UIManager : ScriptableObject
{
    [SerializeField]
    private UIPages initialPage;
    [SerializeField]
    private UIPages currentPage;
    [SerializeField]
    private Stack<UIPages> previousPages = new Stack<UIPages>();
    [SerializeField]
    private List<UIOverlays> openedOverlays = new List<UIOverlays>();

    public UnityEvent<UIPages> onPageChanged;
    public UnityEvent<List<UIOverlays>> onOverlayChanged;

    void OnEnable()
    {
        if (onPageChanged == null)
        {
            onPageChanged = new UnityEvent<UIPages>();
        }

        if (onOverlayChanged == null)
        {
            onOverlayChanged = new UnityEvent<List<UIOverlays>>();
        }

        ChangePage(initialPage);
    }

    void OnDisable()
    {
        onPageChanged = null;
        currentPage = initialPage;
        openedOverlays.Clear();
    }

    public void ChangePage(UIPages newPage)
    {
        if (currentPage == newPage)
        {
            return;
        }

        previousPages.Push(currentPage);

        currentPage = newPage;

        onPageChanged?.Invoke(currentPage);
    }

    public void OpenOverlay(UIOverlays newOverlay)
    {
        
        if (openedOverlays.Contains(newOverlay))
        {
            return;
        }
        
        openedOverlays.Add(newOverlay);

        // if its UIOverlays.None then it closes all overlays
        if (newOverlay == UIOverlays.None)
            openedOverlays.Clear();

        onOverlayChanged?.Invoke(openedOverlays);
    }

    public void CloseMostRecentOverlay()
    {
        int listSize = openedOverlays.Count;

        if (listSize == 0)
            return;

        openedOverlays.RemoveAt(listSize - 1);
        onOverlayChanged?.Invoke(openedOverlays);
    }
    
    public UIPages GetPreviousPage()
    {
        return previousPages.Peek();
    }

    public void BackToPreviousPage()
    {
        if (previousPages.Count == 0)
        {
            Debug.Log("No previous pages found. Stack is empty.");
            return;
        }

        currentPage = previousPages.Pop();

        onPageChanged?.Invoke(currentPage);
    }

    public UIPages GetCurrentPage()
    {
        return currentPage;
    }

    public void ClearPageHistory()
    {
        previousPages.Clear();
    }

    public bool IsOverlayOpened() {  return openedOverlays.Count != 0; }

    public void ResetFlags()
    {
        PlayerPrefs.DeleteAll();
    }

    public void ResetManager()
    {
        ChangePage(initialPage);
        onPageChanged = new UnityEvent<UIPages>();
        onOverlayChanged = new UnityEvent<List<UIOverlays>>();
        previousPages = new Stack<UIPages>();
        openedOverlays = new List<UIOverlays>();
    }
}
