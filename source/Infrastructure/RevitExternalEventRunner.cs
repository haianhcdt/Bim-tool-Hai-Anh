using System;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace DSCons.Revit.Starter.Infrastructure;

/// <summary>
/// Queues one Revit API action from a modeless WPF window and completes when Revit is idle.
/// Create one runner for the lifetime of a tool window; do not call Revit API from the WPF command itself.
/// </summary>
public sealed class RevitExternalEventRunner : IExternalEventHandler, IDisposable
{
    private readonly ExternalEvent _externalEvent;
    private Action<UIApplication>? _pendingAction;
    private TaskCompletionSource<bool>? _completion;
    private bool _disposed;

    public RevitExternalEventRunner()
    {
        _externalEvent = ExternalEvent.Create(this);
    }

    public Task RunAsync(Action<UIApplication> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        if (_disposed) throw new ObjectDisposedException(nameof(RevitExternalEventRunner));
        if (_pendingAction is not null) throw new InvalidOperationException("Một yêu cầu Revit khác đang chờ thực hiện.");

        _completion = new TaskCompletionSource<bool>();
        _pendingAction = action;
        _externalEvent.Raise();
        return _completion.Task;
    }

    public void Execute(UIApplication application)
    {
        var action = _pendingAction;
        var completion = _completion;
        _pendingAction = null;
        _completion = null;

        try
        {
            action?.Invoke(application);
            completion?.TrySetResult(true);
        }
        catch (Exception exception)
        {
            completion?.TrySetException(exception);
        }
    }

    public string GetName() => "DSCons Revit External Event Runner";

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _externalEvent.Dispose();
    }
}
