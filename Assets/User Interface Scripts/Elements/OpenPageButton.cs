public class OpenPageButton : UIButton
{
    public UIManager manager;
    public UIPages newPage;

    public override void HandleButtonClick()
    {
        manager.ChangePage(newPage);
    }
}
