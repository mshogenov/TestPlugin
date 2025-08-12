﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;

namespace SumParameters.Services;

public static class WindowController
{
    private static readonly List<Window> Windows = new();

    /// <summary>
    ///    Попытка установить фокус на этот элемент
    /// </summary>
    /// <typeparam name="T">Type of <see cref="T:System.Windows.Window" /></typeparam>
    /// <returns>True, если экземпляр окна уже был создан</returns>
    public static bool Focus<T>() where T : Window
    {
        var type = typeof(T);
        foreach (var window in Windows)
            if (window.GetType() == type)
            {
                if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
                if (window.Visibility != System.Windows.Visibility.Visible) window.Show();
                window.Focus();
                return true;
            }

        return false;
    }

    /// <summary>Opens a window and returns without waiting for the newly opened window to close</summary>
    public static void Show(Window window)
    {
        RegisterWindow(window);
        window.Show();
    }

    /// <summary>
    ///     Opens a window and returns without waiting for the newly opened window to close. Sets the owner of a child window
    /// </summary>
    public static void Show(Window window, IntPtr handle)
    {
        RegisterWindow(window);
        _ = new WindowInteropHelper(window)
        {
            Owner = handle
        };
        window.Show();
    }

    /// <summary>
    ///     Makes a window visible
    /// </summary>
    /// <typeparam name="T">Type of <see cref="T:System.Windows.Window" /></typeparam>
    public static void Show<T>() where T : Window
    {
        var type = typeof(T);
        foreach (var window in Windows)
            if (window.GetType() == type)
                window.Show();
    }

    /// <summary>
    ///     Makes a window invisible
    /// </summary>
    /// <typeparam name="T">Type of <see cref="T:System.Windows.Window" /></typeparam>
    public static void Hide<T>() where T : Window
    {
        var type = typeof(T);
        foreach (var window in Windows)
            if (window.GetType() == type)
                window.Hide();
    }

    /// <summary>
    ///     Manually closes a <see cref="T:System.Windows.Window" />
    /// </summary>
    /// <typeparam name="T">Type of window</typeparam>
    public static void Close<T>() where T : Window
    {
        var type = typeof(T);
        for (var i = Windows.Count - 1; i >= 0; i--)
        {
            var window = Windows[i];
            if (window.GetType() == type)
                window.Close();
        }
    }

    private static void RegisterWindow(Window window)
    {
        Windows.Add(window);
        window.Closed += (sender, _) =>
        {
            var modelessWindow = (Window)sender;
            Windows.Remove(modelessWindow);
        };
    }
}