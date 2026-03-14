using Avalonia;
using Avalonia.Controls;

namespace NovaProject.CustomControls;

public partial class ChatBody : UserControl
{
    public ChatBody()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ChatScroll.ScrollToEnd();
    }
}