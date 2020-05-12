using System;
using System.Collections.Generic;
using System.Reflection;

namespace P42.Utils
{
	public class WeakEventManager
	{
		static readonly object SyncObj = new object();
		static readonly Dictionary<WeakReference<object>, WeakEventManager> WeakEventManagers = new Dictionary<WeakReference<object>, WeakEventManager>();

		public static WeakEventManager GetWeakEventManager(object source)
		{
			lock (SyncObj)
			{
                var keys = new List<WeakReference<object>>(WeakEventManagers.Keys);
				foreach (var key in keys)
				{
                    if (key.TryGetTarget(out object target))
                    {
                        if (ReferenceEquals(target, source))
                            return WeakEventManagers[key];
                    }
                    else
                        WeakEventManagers.Remove(key);
                }

				var manager = new WeakEventManager();
				WeakEventManagers.Add(new WeakReference<object>(source), manager);

				return manager;
			}
		}

		readonly object _syncObj = new object();
		readonly Dictionary<string, List<Tuple<WeakReference, MethodInfo>>> _eventHandlers = new Dictionary<string, List<Tuple<WeakReference, MethodInfo>>>();

		private WeakEventManager()
		{
		}

		public void AddEventHandler<TEventArgs>(string eventName, EventHandler<TEventArgs> value)
			where TEventArgs : EventArgs
		{
			BuildEventHandler(eventName, value.Target, value.GetMethodInfo());
		}

		public void AddEventHandler(string eventName, EventHandler value)
		{
			BuildEventHandler(eventName, value.Target, value.GetMethodInfo());
		}

		void BuildEventHandler(string eventName, object handlerTarget, MethodInfo methodInfo)
		{
			lock (_syncObj)
			{
                if (!_eventHandlers.TryGetValue(eventName, out List<Tuple<WeakReference, MethodInfo>> target))
                {
                    target = new List<Tuple<WeakReference, MethodInfo>>();
                    _eventHandlers.Add(eventName, target);
                }
                var weakRef = new WeakReference(handlerTarget);
				var tuple = Tuple.Create(weakRef, methodInfo);
				target.Add(tuple);
			}
		}

		public bool EventExists(string eventName)
		{
#pragma warning disable IDE0059 // Value assigned to symbol is never used
            return _eventHandlers.TryGetValue(eventName, out List<Tuple<WeakReference, MethodInfo>> target);
#pragma warning restore IDE0059 // Value assigned to symbol is never used
        }

		public void RaiseEvent(object sender, object args, string eventName)
		{
			var toRaise = new List<Tuple<object, MethodInfo>>();

			lock (_syncObj)
			{
                if (_eventHandlers.TryGetValue(eventName, out List<Tuple<WeakReference, MethodInfo>> targets))
                {
                    foreach (var tuple in targets)
                    {
                        var o = tuple.Item1.Target;

                        if (o == null && !tuple.Item2.Name.Contains("<"))
                            targets.Remove(tuple);
                        else
                            toRaise.Add(Tuple.Create(o, tuple.Item2));
                    }
                }
                if (targets != null && targets.Count == 0)
				{
					// remove event
					_eventHandlers.Remove(eventName);
				}
			}

			foreach (var tuple in toRaise)
				tuple.Item2.Invoke(tuple.Item1, new[] { sender, args });
		}

		public void RemoveEventHandler<TEventArgs>(string eventName, EventHandler<TEventArgs> value)
			where TEventArgs : EventArgs
		{
			RemoveEventHandlerImpl(eventName, value.Target, value.GetMethodInfo());
		}

		public void RemoveEventHandler(string eventName, EventHandler value)
		{
			RemoveEventHandlerImpl(eventName, value.Target, value.GetMethodInfo());
		}

		void RemoveEventHandlerImpl(string eventName, object handlerTarget, MemberInfo methodInfo)
		{
			lock (_syncObj)
			{
                if (_eventHandlers.TryGetValue(eventName, out List<Tuple<WeakReference, MethodInfo>> targets))
                {
                    for (int i = 0; i < targets.Count;)
                    {
                        var target = targets[i];
                        if (target.Item1.Target == handlerTarget && target.Item2.Name == methodInfo.Name)
                            targets.RemoveAt(i);
                        else
                            i++;
                    }
                    /*
					foreach (var tuple in targets.Where(
						t => t.Item1.Target == handlerTarget &&
						t.Item2.Name == methodInfo.Name
					).ToList())
						targets.Remove(tuple);
                        */
                }
            }
		}
	}
}

