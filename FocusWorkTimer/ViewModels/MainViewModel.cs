using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FocusWorkTimer.Model;

namespace FocusWorkTimer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Timer? _timer;
    private int _timeRemaining = 25 * 60;
    [ObservableProperty] private bool _isBreak = false;
    [ObservableProperty] private int _totalWorkTime = 0;
    [ObservableProperty] private bool _isRunning = false;
    [ObservableProperty] private int _pomodoroCount = 0;
    [ObservableProperty] private string _newTaskText = string.Empty;
    [ObservableProperty] private int _workDuration = 25;
    [ObservableProperty] private int _breakDuration = 5;
    [ObservableProperty] private ObservableCollection<TodoTask> _tasks = new();
    public string PlayPauseIcon => IsRunning ? "PauseCircleOutline" : "PlayCircleOutline";
    public string ResetIcon => "Replay";
    public string TimeDisplay => $"{_timeRemaining / 60:00}:{_timeRemaining % 60:00}";
    public string SessionType => IsBreak ? "Break Time" : "Work Time";
    public string PomodoroCountText => PomodoroCount.ToString();
    public string TotalTimeText => $"{TotalWorkTime / 60}";

    public MainViewModel()
    {
        Tasks.Add(new TodoTask("Complete project documentation"));
        Tasks.Add(new TodoTask("Review code changes", isCompleted: true));
        Tasks.Add(new TodoTask("Prepare presentation slides"));
    }
    public string TaskStatsText
    {
        get
        {
            var completed = Tasks.Count(t => t.IsCompleted);
            var total = Tasks.Count;
            return $"Completed: {completed}/{total} tasks";
        }
    }
    partial void OnWorkDurationChanged(int value)
    {
        if (!IsRunning && !IsBreak)
        {
            _timeRemaining = value * 60;
            OnPropertyChanged(nameof(TimeDisplay));
        }
    }
    partial void OnIsBreakChanged(bool value)
    {
        OnPropertyChanged(nameof(SessionType));
    }
    partial void OnIsRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(PlayPauseIcon));
    }
    partial void OnPomodoroCountChanged(int value)
    {
        OnPropertyChanged(nameof(PomodoroCountText));
    }
    
    [RelayCommand]
    private void StartTimer()
    {
        if (!IsRunning)
        {
            IsRunning = true;
            _timer = new Timer(TimerTick, null, 0, 1000);
        }
        else
        {
            PauseTimer();
        }
        OnPropertyChanged(nameof(PlayPauseIcon));
    }

    private void PauseTimer()
    {
        IsRunning = false;
        _timer?.Dispose();
        _timer = null;
    }

    [RelayCommand]
    private void ResetTimer()
    {
        PauseTimer();
        _timeRemaining = IsBreak ? BreakDuration * 60 : WorkDuration * 60;
        OnPropertyChanged(nameof(TimeDisplay));
    }

    [RelayCommand]
    private void AddTask()
    {
        if (!string.IsNullOrWhiteSpace(NewTaskText))
        {
            Tasks.Add(new TodoTask(NewTaskText));
            NewTaskText = string.Empty;
            OnPropertyChanged(nameof(TaskStatsText));
        }
    }

    [RelayCommand]
    private void RemoveTask(TodoTask task)
    {
        Tasks.Remove(task);
        OnPropertyChanged(nameof(TaskStatsText));
    }

    [RelayCommand]
    private void ToggleTask(TodoTask task)
    {
        task.IsCompleted = !task.IsCompleted;
        
        OnPropertyChanged(nameof(TaskStatsText));
    }

    private void TimerTick(object? state)
    {
        if (_timeRemaining > 0)
        {
            _timeRemaining--;
            if (!IsBreak)
            {
                _totalWorkTime++;
                Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(TotalTimeText)));
            }
            Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(TimeDisplay)));
        }
        else
        {
            // Time's up!
            Dispatcher.UIThread.Post(() =>
            {
                PauseTimer();
                
                if (!IsBreak)
                {
                    // Work session completed
                    PomodoroCount++;
                    IsBreak = true;
                    _timeRemaining = BreakDuration * 60;
                }
                else
                {
                    // Break completed
                    IsBreak = false;
                    _timeRemaining = WorkDuration * 60;
                }
                
                OnPropertyChanged(nameof(TimeDisplay));
            });
        }
    }
    

}