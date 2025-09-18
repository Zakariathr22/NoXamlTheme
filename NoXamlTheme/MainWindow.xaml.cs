using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace NoXamlTheme;

public sealed partial class MainWindow : Window
{
    int counter;
    public MainWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(500, 500));

        // Title
        ThemeResourceHelper.ApplyThemeResource(
            TitleText,
            "SystemFillColorAttentionBrush",
            TextBlock.ForegroundProperty);

        // Description
        ThemeResourceHelper.ApplyThemeResource(
            DescText,
            "TextFillColorSecondaryBrush",
            TextBlock.ForegroundProperty);

        // Accent button background & foreground
        ThemeResourceHelper.ApplyThemeResource(
            AccentButton,
            "AccentAcrylicBackgroundFillColorBaseBrush",
            Button.BackgroundProperty);

        ThemeResourceHelper.ApplyThemeResource(
            AccentButton,
            "TextFillColorPrimaryBrush",
            Button.ForegroundProperty);

        // Adaptive border background
        ThemeResourceHelper.ApplyThemeResource(
            AdaptiveBorder,
            "SystemFillColorSuccessBackgroundBrush",
            Border.BackgroundProperty);

        ThemeResourceHelper.ApplyThemeResource(
            AdaptiveBorderText,
            "TextFillColorPrimaryBrush",
            TextBlock.ForegroundProperty);

        ThemeDebugHelper.DebugBrush(new ColorPaletteResources(), "SystemFillColorSuccessBackgroundBrush");
    }

    private void AccentButton_Click(object sender, RoutedEventArgs e)
    {
        if (counter++ % 2 == 0)
        {
            ThemeResourceHelper.ApplyThemeResource(
                AdaptiveBorder,
                "CardBackgroundFillColorDefaultBrush",
                Border.BackgroundProperty);
        }
        else
        {
            ThemeResourceHelper.ApplyThemeResource(
                AdaptiveBorder,
                "CardBackgroundFillColorSecondaryBrush",
                Border.BackgroundProperty);
        }
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            // Change window theme
            if (Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = tag switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default,
                };
            }
        }
    }
}
