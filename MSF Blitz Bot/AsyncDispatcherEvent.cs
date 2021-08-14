using System;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace MSFBlitzBot
{
    public sealed class AsyncDispatcherEvent<TEvent, TArgs> where TEvent : class where TArgs : EventArgs
    {
        private delegate void InvokeMethod(TEvent @event, object sender, TArgs args);

        private sealed class DelegateWrapper
        {
            public readonly TEvent handler;

            public readonly Dispatcher dispatcher;

            public DelegateWrapper(Dispatcher dispatcher, TEvent handler)
            {
                this.dispatcher = dispatcher;
                this.handler = handler;
            }

            public void invoke(object sender, TArgs args)
            {
                if (dispatcher == null || dispatcher == getDispatcherOrNull())
                {
                    _invoke(handler, sender, args);
                    return;
                }
                dispatcher.BeginInvoke(handler as Delegate, DispatcherPriority.DataBind, sender, args);
            }
        }

        private static readonly InvokeMethod _invoke;

        private readonly object _removeLock = new();

        public bool IsEmpty => _event == null;

        private event EventHandler<TArgs> _event;

        static AsyncDispatcherEvent()
        {
            Type typeFromHandle = typeof(TEvent);
            Type typeFromHandle2 = typeof(TArgs);
            if (!typeFromHandle.IsSubclassOf(typeof(MulticastDelegate)))
            {
                throw new InvalidOperationException("TEvent " + typeFromHandle.Name + " is not a subclass of MulticastDelegate");
            }
            MethodInfo method = typeFromHandle.GetMethod("Invoke", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find method Invoke() on TEvent " + typeFromHandle.Name);
            }
            if (method.ReturnType != typeof(void))
            {
                throw new InvalidOperationException("TEvent " + typeFromHandle.Name + " must have return type of void");
            }
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 2)
            {
                throw new InvalidOperationException("TEvent " + typeFromHandle.Name + " must have 2 parameters");
            }
            if (parameters[0].ParameterType != typeof(object))
            {
                throw new InvalidOperationException("TEvent " + typeFromHandle.Name + " must have first parameter of type object, instead was " + parameters[0].ParameterType.Name);
            }
            if (parameters[1].ParameterType != typeFromHandle2)
            {
                throw new InvalidOperationException("TEvent " + typeFromHandle.Name + " must have second paramater of type TArgs " + typeFromHandle2.Name + ", instead was " + parameters[1].ParameterType.Name);
            }
            _invoke = (InvokeMethod)method.CreateDelegate(typeof(InvokeMethod));
            if (_invoke == null)
            {
                throw new InvalidOperationException("CreateDelegate() returned null");
            }
        }

        public void add(TEvent value)
        {
            if (value != null)
            {
                _event += new EventHandler<TArgs>(new DelegateWrapper(getDispatcherOrNull(), value).invoke);
            }
        }

        public void remove(TEvent value)
        {
            if (value == null)
            {
                return;
            }
            Dispatcher dispatcherOrNull = getDispatcherOrNull();
            lock (_removeLock)
            {
                EventHandler<TArgs> @event = _event;
                if (@event == null)
                {
                    return;
                }
                Delegate[] invocationList = @event.GetInvocationList();
                for (var num = invocationList.Length - 1; num >= 0; num--)
                {
                    var delegateWrapper = (DelegateWrapper)invocationList[num].Target;
                    if (delegateWrapper.handler.Equals(value) && delegateWrapper.dispatcher == dispatcherOrNull)
                    {
                        _event -= new EventHandler<TArgs>(delegateWrapper.invoke);
                        break;
                    }
                }
            }
        }

        public void raise(object sender, TArgs args)
        {
            _event?.Invoke(sender, args);
        }

        private static Dispatcher getDispatcherOrNull()
        {
            return Dispatcher.FromThread(Thread.CurrentThread);
        }
    }
}