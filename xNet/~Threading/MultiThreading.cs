using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Better_xNet
{
    public class MultiThreading : IDisposable
    {
        private struct ForParams
        {
            public int Begin;

            public int End;

            public Action<int> Action;
        }

        private struct ForEachParams<T>
        {
            public IEnumerator<T> Source;

            public Action<T> Action;
        }

        private struct ForEachListParams<T>
        {
            public int Begin;

            public int End;

            public IList<T> List;

            public Action<T> Action;
        }

        private bool _disposed;

        private ulong _repeatCount;

        private Barrier _barrierForReps;

        private int _threadCount;

        private int _currentThreadCount;

        private bool _endEnumerator;

        private bool _enableInfiniteRepeat;

        private bool _notImplementedReset;

        private bool _canceling;

        private readonly ReaderWriterLockSlim _lockForCanceling = new ReaderWriterLockSlim();

        private object _lockForEndThread = new object();

        private AsyncOperation _asyncOperation;

        private SendOrPostCallback _callbackEndWork;

        private EventHandler<EventArgs> _beginningWorkHandler;

        private EventHandler<EventArgs> _workCompletedAsyncEvent;

        private EventHandler<MultiThreadingRepeatEventArgs> _repeatCompletedHandler;

        private AsyncEvent<MultiThreadingProgressEventArgs> _progressChangedAsyncEvent;

        private AsyncEvent<EventArgs> _cancelingWorkAsyncEvent;

        public bool Working
        {
            get;
            private set;
        }

        public bool Canceling
        {
            get
            {
                _lockForCanceling.EnterReadLock();
                try
                {
                    return _canceling;
                }
                finally
                {
                    _lockForCanceling.ExitReadLock();
                }
            }
        }

        public bool EnableInfiniteRepeat
        {
            get
            {
                return _enableInfiniteRepeat;
            }
            set
            {
                if (Working)
                {
                    throw new InvalidOperationException(Resources.InvalidOperationException_NetProcesses_CannotSetValue);
                }
                _enableInfiniteRepeat = value;
            }
        }

        public int ThreadCount
        {
            get
            {
                return _threadCount;
            }
            set
            {
                if (Working)
                {
                    throw new InvalidOperationException(Resources.InvalidOperationException_NetProcesses_CannotSetValue);
                }
                if (value < 1)
                {
                    throw ExceptionHelper.CanNotBeLess("ThreadsCount", 1);
                }
                _threadCount = value;
            }
        }

        protected AsyncOperation AsyncOperation => _asyncOperation;

        public event EventHandler<EventArgs> BeginningWork
        {
            add
            {
                _beginningWorkHandler = (EventHandler<EventArgs>)Delegate.Combine(_beginningWorkHandler, value);
            }
            remove
            {
                _beginningWorkHandler = (EventHandler<EventArgs>)Delegate.Remove(_beginningWorkHandler, value);
            }
        }

        public event EventHandler<EventArgs> WorkCompleted
        {
            add
            {
                _workCompletedAsyncEvent = (EventHandler<EventArgs>)Delegate.Combine(_workCompletedAsyncEvent, value);
            }
            remove
            {
                _workCompletedAsyncEvent = (EventHandler<EventArgs>)Delegate.Remove(_workCompletedAsyncEvent, value);
            }
        }

        public event EventHandler<MultiThreadingRepeatEventArgs> RepeatCompleted
        {
            add
            {
                _repeatCompletedHandler = (EventHandler<MultiThreadingRepeatEventArgs>)Delegate.Combine(_repeatCompletedHandler, value);
            }
            remove
            {
                _repeatCompletedHandler = (EventHandler<MultiThreadingRepeatEventArgs>)Delegate.Remove(_repeatCompletedHandler, value);
            }
        }

        public event EventHandler<MultiThreadingProgressEventArgs> ProgressChanged
        {
            add
            {
                AsyncEvent<MultiThreadingProgressEventArgs> progressChangedAsyncEvent = _progressChangedAsyncEvent;
                progressChangedAsyncEvent.EventHandler = (EventHandler<MultiThreadingProgressEventArgs>)Delegate.Combine(progressChangedAsyncEvent.EventHandler, value);
            }
            remove
            {
                AsyncEvent<MultiThreadingProgressEventArgs> progressChangedAsyncEvent = _progressChangedAsyncEvent;
                progressChangedAsyncEvent.EventHandler = (EventHandler<MultiThreadingProgressEventArgs>)Delegate.Remove(progressChangedAsyncEvent.EventHandler, value);
            }
        }

        public event EventHandler<EventArgs> CancelingWork
        {
            add
            {
                AsyncEvent<EventArgs> cancelingWorkAsyncEvent = _cancelingWorkAsyncEvent;
                cancelingWorkAsyncEvent.EventHandler = (EventHandler<EventArgs>)Delegate.Combine(cancelingWorkAsyncEvent.EventHandler, value);
            }
            remove
            {
                AsyncEvent<EventArgs> cancelingWorkAsyncEvent = _cancelingWorkAsyncEvent;
                cancelingWorkAsyncEvent.EventHandler = (EventHandler<EventArgs>)Delegate.Remove(cancelingWorkAsyncEvent.EventHandler, value);
            }
        }

        public MultiThreading(int threadCount = 1)
        {
            if (threadCount < 1)
            {
                throw ExceptionHelper.CanNotBeLess("threadCount", 1);
            }
            _threadCount = threadCount;
            _callbackEndWork = EndWorkCallback;
            _cancelingWorkAsyncEvent = new AsyncEvent<EventArgs>(OnCancelingWork);
            _progressChangedAsyncEvent = new AsyncEvent<MultiThreadingProgressEventArgs>(OnProgressChanged);
        }

        public virtual void Run(Action action)
        {
            ThrowIfDisposed();
            if (Working)
            {
                throw new InvalidOperationException(Resources.InvalidOperationException_MultiThreading_CannotStart);
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            InitBeforeRun(_threadCount);
            try
            {
                for (int i = 0; i < _threadCount; i++)
                {
                    StartThread(Thread, action);
                }
            }
            catch (Exception)
            {
                EndWork();
                throw;
            }
        }

        public virtual void RunFor(int fromInclusive, int toExclusive, Action<int> action)
        {
            ThrowIfDisposed();
            if (Working)
            {
                throw new InvalidOperationException(Resources.InvalidOperationException_MultiThreading_CannotStart);
            }
            if (fromInclusive < 0)
            {
                throw ExceptionHelper.CanNotBeLess("fromInclusive", 0);
            }
            if (fromInclusive > toExclusive)
            {
                throw new ArgumentOutOfRangeException("fromInclusive", Resources.ArgumentException_MultiThreading_BegIndexRangeMoreEndIndex);
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            int num = toExclusive - fromInclusive;
            if (num != 0)
            {
                int num2 = _threadCount;
                if (num2 > num)
                {
                    num2 = num;
                }
                InitBeforeRun(num2);
                int num3 = 0;
                int[] array = CalculateThreadsIterations(num, num2);
                try
                {
                    ForParams forParams = default(ForParams);
                    for (int i = 0; i < array.Length; i++)
                    {
                        forParams.Action = action;
                        forParams.Begin = num3 + fromInclusive;
                        forParams.End = num3 + array[i] + fromInclusive;
                        StartThread(ForInThread, forParams);
                        num3 += array[i];
                    }
                }
                catch (Exception)
                {
                    EndWork();
                    throw;
                }
            }
        }

        public virtual void RunForEach<T>(IEnumerable<T> source, Action<T> action)
        {
            ThrowIfDisposed();
            if (Working)
            {
                throw new InvalidOperationException(Resources.InvalidOperationException_MultiThreading_CannotStart);
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (source is IList<T>)
            {
                RunForEachList(source, action);
            }
            else
            {
                RunForEachOther(source, action);
            }
        }

        public void ReportProgress(object value = null)
        {
            ThrowIfDisposed();
            _progressChangedAsyncEvent.Post(_asyncOperation, this, new MultiThreadingProgressEventArgs(value));
        }

        public void ReportProgressSync(object value = null)
        {
            ThrowIfDisposed();
            OnProgressChanged(new MultiThreadingProgressEventArgs(value));
        }

        public virtual void Cancel()
        {
            ThrowIfDisposed();
            _lockForCanceling.EnterWriteLock();
            try
            {
                if (!_canceling)
                {
                    _canceling = true;
                    _cancelingWorkAsyncEvent.Post(_asyncOperation, this, EventArgs.Empty);
                }
            }
            finally
            {
                _lockForCanceling.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _lockForCanceling.Dispose();
            }
        }

        protected virtual void OnBeginningWork(EventArgs e)
        {
            _beginningWorkHandler?.Invoke(this, e);
        }

        protected virtual void OnWorkCompleted(EventArgs e)
        {
            _workCompletedAsyncEvent?.Invoke(this, e);
        }

        protected virtual void OnRepeatCompleted(MultiThreadingRepeatEventArgs e)
        {
            _repeatCompletedHandler?.Invoke(this, e);
        }

        protected virtual void OnProgressChanged(MultiThreadingProgressEventArgs e)
        {
            _progressChangedAsyncEvent.On(this, e);
        }

        protected virtual void OnCancelingWork(EventArgs e)
        {
            _cancelingWorkAsyncEvent.On(this, e);
        }

        private void InitBeforeRun(int threadCount, bool needCreateBarrierForReps = true)
        {
            _repeatCount = 0uL;
            _notImplementedReset = false;
            _currentThreadCount = threadCount;
            if (needCreateBarrierForReps)
            {
                _barrierForReps = new Barrier(threadCount, delegate
                {
                    if (!Canceling)
                    {
                        OnRepeatCompleted(new MultiThreadingRepeatEventArgs(++_repeatCount));
                    }
                });
            }
            _canceling = false;
            _asyncOperation = AsyncOperationManager.CreateOperation(new object());
            Working = true;
            OnBeginningWork(EventArgs.Empty);
        }

        private bool EndThread()
        {
            lock (_lockForEndThread)
            {
                _currentThreadCount--;
                if (_currentThreadCount == 0)
                {
                    _asyncOperation.PostOperationCompleted(_callbackEndWork, new EventArgs());
                    return true;
                }
            }
            return false;
        }

        private void EndWork()
        {
            Working = false;
            if (_barrierForReps != null)
            {
                _barrierForReps.Dispose();
                _barrierForReps = null;
            }
            _asyncOperation = null;
        }

        private void EndWorkCallback(object param)
        {
            EndWork();
            OnWorkCompleted(param as EventArgs);
        }

        private int[] CalculateThreadsIterations(int iterationCount, int threadsCount)
        {
            int[] array = new int[threadsCount];
            int num = iterationCount / threadsCount;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = num;
            }
            int num2 = 0;
            int num3 = iterationCount - threadsCount * num;
            for (int j = 0; j < num3; j++)
            {
                array[num2]++;
                if (++num2 == array.Length)
                {
                    num2 = 0;
                }
            }
            return array;
        }

        private void StartThread(Action<object> body, object param)
        {
            Thread thread = new Thread(body.Invoke);
            thread.IsBackground = true;
            thread.Start(param);
        }

        private void RunForEachList<T>(IEnumerable<T> source, Action<T> action)
        {
            IList<T> list = source as IList<T>;
            int count = list.Count;
            if (count != 0)
            {
                int num = _threadCount;
                if (num > count)
                {
                    num = count;
                }
                InitBeforeRun(num);
                int num2 = 0;
                int[] array = CalculateThreadsIterations(count, num);
                try
                {
                    ForEachListParams<T> forEachListParams = default(ForEachListParams<T>);
                    for (int i = 0; i < array.Length; i++)
                    {
                        forEachListParams.Action = action;
                        forEachListParams.List = list;
                        forEachListParams.Begin = num2;
                        forEachListParams.End = num2 + array[i];
                        StartThread(ForEachListInThread<T>, forEachListParams);
                        num2 += array[i];
                    }
                }
                catch (Exception)
                {
                    EndWork();
                    throw;
                }
            }
        }

        private void RunForEachOther<T>(IEnumerable<T> source, Action<T> action)
        {
            _endEnumerator = false;
            InitBeforeRun(_threadCount, needCreateBarrierForReps: false);
            ForEachParams<T> forEachParams = default(ForEachParams<T>);
            forEachParams.Action = action;
            forEachParams.Source = source.GetEnumerator();
            try
            {
                for (int i = 0; i < _threadCount; i++)
                {
                    StartThread(ForEachInThread<T>, forEachParams);
                }
            }
            catch (Exception)
            {
                EndWork();
                throw;
            }
        }

        private void Thread(object param)
        {
            Action action = param as Action;
            try
            {
                while (!Canceling)
                {
                    action();
                    if (!_enableInfiniteRepeat)
                    {
                        break;
                    }
                    _barrierForReps.SignalAndWait();
                }
            }
            catch (Exception)
            {
                Cancel();
                if (_enableInfiniteRepeat)
                {
                    _barrierForReps.RemoveParticipant();
                }
                throw;
            }
            finally
            {
                EndThread();
            }
        }

        private void ForInThread(object param)
        {
            ForParams forParams = (ForParams)param;
            try
            {
                do
                {
                    for (int i = forParams.Begin; i < forParams.End; i++)
                    {
                        if (Canceling)
                        {
                            break;
                        }
                        forParams.Action(i);
                    }
                    if (!_enableInfiniteRepeat)
                    {
                        break;
                    }
                    _barrierForReps.SignalAndWait();
                }
                while (!Canceling);
            }
            catch (Exception)
            {
                Cancel();
                if (_enableInfiniteRepeat)
                {
                    _barrierForReps.RemoveParticipant();
                }
                throw;
            }
            finally
            {
                EndThread();
            }
        }

        private void ForEachListInThread<T>(object param)
        {
            ForEachListParams<T> forEachListParams = (ForEachListParams<T>)param;
            IList<T> list = forEachListParams.List;
            try
            {
                do
                {
                    for (int i = forEachListParams.Begin; i < forEachListParams.End; i++)
                    {
                        if (Canceling)
                        {
                            break;
                        }
                        forEachListParams.Action(list[i]);
                    }
                    if (!_enableInfiniteRepeat)
                    {
                        break;
                    }
                    _barrierForReps.SignalAndWait();
                }
                while (!Canceling);
            }
            catch (Exception)
            {
                Cancel();
                if (_enableInfiniteRepeat)
                {
                    _barrierForReps.RemoveParticipant();
                }
                throw;
            }
            finally
            {
                EndThread();
            }
        }

        private void ForEachInThread<T>(object param)
        {
            ForEachParams<T> forEachParams = (ForEachParams<T>)param;
            try
            {
                while (!Canceling)
                {
                    T current;
                    lock (forEachParams.Source)
                    {
                        if (Canceling)
                        {
                            return;
                        }
                        if (forEachParams.Source.MoveNext())
                        {
                            current = forEachParams.Source.Current;
                            goto IL_009a;
                        }
                        if (!_enableInfiniteRepeat || _notImplementedReset)
                        {
                            return;
                        }
                        try
                        {
                            forEachParams.Source.Reset();
                        }
                        catch (NotImplementedException)
                        {
                            _notImplementedReset = true;
                            return;
                        }
                        OnRepeatCompleted(new MultiThreadingRepeatEventArgs(++_repeatCount));
                    }
                    continue;
                    IL_009a:
                    forEachParams.Action(current);
                }
            }
            catch (Exception)
            {
                Cancel();
            }
            finally
            {
                if (EndThread())
                {
                    forEachParams.Source.Dispose();
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("MultiThreading<TProgress>");
            }
        }
    }
}
