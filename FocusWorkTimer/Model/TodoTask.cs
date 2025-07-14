using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FocusWorkTimer.ViewModels;

namespace FocusWorkTimer.Model;

public partial class TodoTask : ViewModelBase
{
    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private DateTime _createdAt;

    public TodoTask(string text, bool isCompleted = false)
    {
        Text = text;
        CreatedAt = DateTime.Now;
    }
}