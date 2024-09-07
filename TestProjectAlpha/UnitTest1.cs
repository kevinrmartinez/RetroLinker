using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

namespace TestProjectAlpha;

public class UnitTest1
{
    [AvaloniaFact]
    public void Test1()
    {
        var textBox = new TextBox();
        var label = new Label();
        var stackPanel = new StackPanel();
        stackPanel.Children.Add(textBox);
        stackPanel.Children.Add(label);
        var window = new Window() { Content = stackPanel };
        
        // Open window:
        window.Show();

        // Focus text box:
        textBox.Focus();

        // Simulate text input:
        window.KeyTextInput("Notepad");
        
        // print to label
        
        var ext = (OperatingSystem.IsWindows()) ? RetroLinker.Models.FileOps.WinLinkExt : RetroLinker.Models.FileOps.LinLinkExt;
        var outDir = RetroLinker.Models.FileOps.UserDesktop;
        var result = RetroLinker.Models.FileOps.GetDefinedLinkPath(textBox.Text, outDir) + ext;

        // Assert:
        string lnk = Path.Combine(outDir, $"Notepad{ext}");
        Assert.Equal(lnk, result);
    }
}