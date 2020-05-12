using System;
using System.Threading;
using System.Threading.Tasks;

namespace P42.Utils
{
    public delegate bool TimerCallback(object state);

    sealed public class Timer : CancellationTokenSource
    {
        #region Private Fields
        static int Count;
        readonly Func<Task, object, Task> _function;
        readonly TimerCallback _callback;
        readonly object _state;
        TimeSpan _dueTime;
        TimeSpan _period;
        readonly TimeSpan _infinite = new TimeSpan(0, 0, Timeout.Infinite);
        readonly int _id;
        #endregion

        #region Constructors
        //public Timer(TimerCallback callback, object state=null, Int32 dueTime=Timeout.Infinite, Int32 period=Timeout.Infinite) : this  (callback, state, new TimeSpan (0, 0, dueTime), new TimeSpan (0, 0, period))
        //{
        //}

        public Timer(TimerCallback callback, object state, uint dueTime, uint period) : this(callback, state, new TimeSpan(0, 0, (int)dueTime), new TimeSpan(0, 0, (int)period))
        {
            P42.Utils.Debug.AddToCensus(this);
        }

        public Timer(TimerCallback callback, object state, long dueTime = Timeout.Infinite, long period = Timeout.Infinite) : this(callback, state, new TimeSpan(0, 0, (int)dueTime), new TimeSpan(0, 0, (int)period))
        {
            P42.Utils.Debug.AddToCensus(this);
        }

        public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            P42.Utils.Debug.AddToCensus(this);
            _id = Count++;
            _callback = callback;
            _state = state;

            _function = async (t, s) =>
            {
                var tuple = (Tuple<TimerCallback, object>)s;
                while (!IsCancellationRequested)
                {
                    if (_period == _infinite)
                        return;
                    if (!tuple.Item1(tuple.Item2))
                        _period = _infinite;
                    if (_period != _infinite)
                        await Task.Delay(_period);
                    else
                        return;
                }
            };
            Change(dueTime, period);
        }
        #endregion;

        #region Disposal
        bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                base.Cancel();
                P42.Utils.Debug.RemoveFromCensus(this);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Utility
        public override string ToString()
        {
            return string.Format("[Timer " + _id + "]");
        }
        #endregion

        #region Changers
        public new void Cancel()
        {
            Change(_infinite, _infinite);
        }

        //public void Change(Int32 dueTime=Timeout.Infinite, Int32 period=Timeout.Infinite) {
        //	Change(new TimeSpan (0, 0, 0, 0, dueTime), new TimeSpan (0, 0, 0, 0, period));
        //}

        public void Change(uint dueTime, uint period)
        {
            Change(new TimeSpan(0, 0, 0, 0, (int)dueTime), new TimeSpan(0, 0, 0, 0, (int)period));
        }

        public void Change(long dueTime = Timeout.Infinite, long period = Timeout.Infinite)
        {
            Change(new TimeSpan(0, 0, 0, 0, (int)dueTime), new TimeSpan(0, 0, 0, 0, (int)period));
        }

        public void Change(TimeSpan dueTime, TimeSpan period)
        {
            //if (_task != null)
            //	throw new NotImplementedException ("");
            _dueTime = dueTime;
            _period = period;
            if (_dueTime != _infinite)
                //_task = 
                Task.Delay(_dueTime, Token).ContinueWith(
                    _function,
                    Tuple.Create(_callback, _state),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                    TaskScheduler.Default
                );
        }
        #endregion
    }
}

