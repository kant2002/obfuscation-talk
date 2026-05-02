using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace hello_desktop;

public partial class UserControl1 : UserControl
{
    public UserControl1()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        nameBox.Content = "Hello, Desktop!";
    }
}