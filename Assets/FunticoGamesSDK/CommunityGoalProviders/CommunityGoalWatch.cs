using System;
using System.Threading;

namespace FunticoGamesSDK.CommunityGoalProviders
{
    /// <summary>
    /// Handle of a running WatchCommunityGoal. Call <see cref="Stop"/> (or Dispose) when the goal
    /// leaves the screen; the watch also stops by itself once the goal is settled.
    /// </summary>
    public sealed class CommunityGoalWatch : IDisposable
    {
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        /// <summary>False once the watch has been stopped or the goal has settled.</summary>
        public bool IsRunning { get; private set; } = true;

        internal CancellationToken Token => _cancellation.Token;

        /// <summary>Stops polling. Safe to call more than once.</summary>
        public void Stop()
        {
            IsRunning = false;
            if (!_cancellation.IsCancellationRequested)
                _cancellation.Cancel();
        }

        public void Dispose() => Stop();

        internal void MarkStopped() => IsRunning = false;
    }
}
