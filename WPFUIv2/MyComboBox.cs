using System.Windows.Controls;
using System.Windows.Input;
public class MyComboBox : ComboBox
{
    public event MouseEventHandler ItemHover;

    public ScrollViewer ScrollViewer
    {
        get { return GetTemplateChild("ScrollViewer") as ScrollViewer; }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        ScrollViewer.MouseMove += new MouseEventHandler(scrollviewer_MouseMove);
    }

    private void scrollviewer_MouseMove(object sender, MouseEventArgs e)
    {
        if (ItemHover != null)
        {
            ItemHover(this, e);
        }
    }
}