using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class TaskPool : Singleton<TaskPool>
{
    private List<Task> tasks = new List<Task>();
    private List<CancellationTokenSource> tokenSources = new List<CancellationTokenSource>();

    public CancellationTokenSource AddTask(Action<CancellationToken> action, Action onExit = null)
    {
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;
        tokenSources.Add(tokenSource);

        var task = Task.Run(() => action(token), token);
        tasks.Add(task);

        if (onExit != null)
        {
            task.ContinueWith(t => onExit(), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        return tokenSource;
    }
    public void AddTasks(IEnumerable<Action<CancellationToken>> actions, out List<CancellationTokenSource> returnedTokenSources, Action onExit = null)
    {
        returnedTokenSources = new List<CancellationTokenSource>();
        Task previousTask = null;

        foreach (var action in actions)
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            tokenSources.Add(tokenSource);

            returnedTokenSources.Add(tokenSource);

            var task = previousTask == null ? Task.Run(() => action(token), token) : previousTask.ContinueWith(t => action(token), token);
            tasks.Add(task);
            previousTask = task;
        }

        if (onExit != null && previousTask != null)
        {
            previousTask.ContinueWith(t => onExit(), TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }

    public void CancelTask(CancellationTokenSource token)
    {
        int tokenToCancel = tokenSources.FindIndex(tt => token.Token.Equals(tt.Token));
        if (tokenToCancel != -1)
        {
            tasks.RemoveAt(tokenToCancel);
            tokenSources.RemoveAt(tokenToCancel);
        }
    }

    override protected void OnDestroy()
    {
        foreach (var tokenSource in tokenSources)
        {
            tokenSource.Cancel();
        }
    }

    private void Update()
    {
        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            if (tasks[i].IsCompleted)
            {
                tasks.RemoveAt(i);
                tokenSources.RemoveAt(i);
            }
        }
    }
}