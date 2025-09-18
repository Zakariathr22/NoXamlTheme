using Microsoft.UI.System;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using Windows.UI.ViewManagement;

namespace NoXamlTheme;

/// <summary>
/// A helper class that automatically applies theme-based resources
/// (colors, brushes, etc.) to elements and keeps them updated
/// whenever the app theme or system settings (like high contrast
/// or accent colors) change.
/// </summary>
public static class ThemeResourceHelper
{
    /// <summary>
    /// Global UISettings instance used to detect system-wide changes,
    /// like when the user changes the accent color or switches high contrast on/off.
    /// </summary>
    private static readonly UISettings _uiSettings = new();

    /// <summary>
    /// Ensures we only hook the <see cref="UISettings.ColorValuesChanged"/> event once.
    /// </summary>
    private static bool _uiSettingsHooked;

    /// <summary>
    /// Tracks all elements that are currently bound to theme resources.
    /// We need this so when the theme changes, we know which elements to update.
    /// </summary>
    private static readonly HashSet<FrameworkElement> _trackedelements = new();

    /// <summary>
    /// Stores the binding state (resource keys and dependency properties) for a single element.
    /// We use an attached property so each element can hold its own state.
    /// </summary>
    private sealed class BindingState
    {
        public readonly List<(string key, DependencyProperty prop)> Bindings = new();
    }

    /// <summary>
    /// Attached property that links a <see cref="BindingState"/> to each element.
    /// </summary>
    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached(
            "ThemeResourceHelper_State",
            typeof(BindingState),
            typeof(ThemeResourceHelper),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets the <see cref="BindingState"/> for a element,
    /// or creates one if it doesn't exist yet.
    /// </summary>
    private static BindingState GetOrCreateState(FrameworkElement element)
    {
        var state = (BindingState)element.GetValue(StateProperty);
        if (state == null)
        {
            state = new BindingState();
            element.SetValue(StateProperty, state);

            // Hook once per element
            element.Loaded += OnLoaded;
            element.Unloaded += OnUnloaded;
            element.ActualThemeChanged += (_, __) =>
            {
                ApplyAll(element);
            };
        }
        return state;
    }

    /// <summary>
    /// Binds a theme resource (by key) to a specific DependencyProperty on a element.
    /// Example: ApplyThemeResource(myButton, "ButtonBackground", Button.BackgroundProperty)
    /// </summary>
    public static void ApplyThemeResource(
        FrameworkElement element,
        string resourceKey,
        DependencyProperty property)
    {
        var state = GetOrCreateState(element);

        // Replace existing binding if found
        var existing = state.Bindings.FindIndex(b => b.prop == property);
        if (existing >= 0)
            state.Bindings[existing] = (resourceKey, property);
        else
            state.Bindings.Add((resourceKey, property));

        // If the element is already in the visual tree → apply immediately
        if (element.IsLoaded)
        {
            HookGlobalUiSettings(element);
            ApplyAll(element);
        }
    }

    /// <summary>
    /// Called when a element is loaded into the visual tree.
    /// Ensures the element is tracked and updated.
    /// </summary>
    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        HookGlobalUiSettings(element);
        ApplyAll(element);
    }

    /// <summary>
    /// Called when a element is unloaded from the visual tree.
    /// Removes it from our tracked list (no updates needed).
    /// </summary>
    private static void OnUnloaded(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        _trackedelements.Remove(element);
    }

    /// <summary>
    /// Hooks the global UISettings.ColorValuesChanged event (only once).
    /// This event tells us when system colors (like accent color) change.
    /// </summary>
    private static void HookGlobalUiSettings(FrameworkElement element)
    {
        _trackedelements.Add(element);

        if (_uiSettingsHooked) return;

        _uiSettings.ColorValuesChanged += (s, args) =>
        {
            // This event runs on a background thread → must dispatch to UI thread.
            foreach (var el in _trackedelements)
            {
                if (el.DispatcherQueue.HasThreadAccess)
                {
                    ApplyAll(el);
                }
                else
                {
                    el.DispatcherQueue.TryEnqueue(() => ApplyAll(el));
                }
            }
        };

        _uiSettingsHooked = true;
    }

    /// <summary>
    /// Applies all theme resource bindings for a given element.
    /// Chooses the correct dictionary (Light, Default/Dark, HighContrast),
    ///  ooks up the resource by key,
    ///  and ets the value on the DependencyProperty.
    /// </summary>
    private static void ApplyAll(FrameworkElement element)
    {
        var state = (BindingState)element.GetValue(StateProperty);
        if (state == null) return;

        // Detect if system is in High Contrast mode
        var isHighContrast = ThemeSettings
            .CreateForWindowId(element.XamlRoot.ContentIslandEnvironment.AppWindowId)
            .HighContrast;

        // Pick dictionary: HighContrast, Light, or Default (Dark)
        string dictKey = isHighContrast
            ? "HighContrast"
            : (element.ActualTheme == ElementTheme.Light ? "Light" : "Default");

        if (App.Current.Resources.MergedDictionaries[0].ThemeDictionaries.TryGetValue(dictKey, out var dictObj) && dictObj is ResourceDictionary dict)
        {
            foreach (var (key, prop) in state.Bindings)
            {
                if (dict.TryGetValue(key, out var resource))
                {
                    // Finally apply the resource to the element’s property
                    element.SetValue(prop, null);
                    element.SetValue(prop, resource);                }
            }
        }
    }
}
