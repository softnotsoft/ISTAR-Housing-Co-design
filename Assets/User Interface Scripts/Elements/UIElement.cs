using UnityEngine;

public class UIElement : MonoBehaviour
{
    private const string showAnimationName = "Open";
    private const string hideAnimationName = "Closed";

    protected void ShowSelf()
    {
        gameObject.SetActive(true);

        Animator anim = gameObject.GetComponent<Animator>();
        if (anim != null)
            anim.Play(showAnimationName);
    }

    protected void HideSelf()
    {
        Animator anim = gameObject.GetComponent<Animator>();
        if (anim != null)
            anim.Play(hideAnimationName);

        gameObject.SetActive(false);
    }
}
