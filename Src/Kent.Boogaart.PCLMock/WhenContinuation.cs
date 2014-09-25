namespace Kent.Boogaart.PCLMock
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public abstract class WhenContinuation
    {
        private readonly IDictionary<int, object> outAndRefAssignments;

        protected WhenContinuation()
        {
            this.outAndRefAssignments = new Dictionary<int, object>();
        }

        internal abstract object Apply(object mockedObject, params object[] args);

        internal T GetOutParameterValue<T>(int parameterIndex)
        {
            object value;

            if (!this.outAndRefAssignments.TryGetValue(parameterIndex, out value))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No value for out parameter at index {0} has been specified.", parameterIndex));
            }

            if (value != null)
            {
                if (!(value is T))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Out parameter at index {0} has a value of type '{1}' but type '{2}' was expected.", parameterIndex, value.GetType().FullName, typeof(T).FullName));
                }
            }
            else if (typeof(T).GetTypeInfo().IsValueType)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Out parameter at index {0} has a null value specified but it is a value type ('{1}') so cannot be null.", parameterIndex, typeof(T).FullName));
            }

            return (T)value;
        }

        internal T GetRefParameterValue<T>(int parameterIndex, T defaultValue)
        {
            object value;

            if (!this.outAndRefAssignments.TryGetValue(parameterIndex, out value))
            {
                // ref parameters need not be specified in the expectations, in which case the caller can provide the default value to assign
                return defaultValue;
            }

            if (value != null)
            {
                if (!(value is T))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Ref parameter at index {0} has a value of type '{1}' but type '{2}' was expected.", parameterIndex, value.GetType().FullName, typeof(T).FullName));
                }
            }
            else if (typeof(T).GetTypeInfo().IsValueType)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Ref parameter at index {0} has a null value specified but it is a value type ('{1}') so cannot be null.", parameterIndex, typeof(T).FullName));
            }

            return (T)value;
        }

        protected void AssignOutOrRefParameter(int parameterIndex, object value)
        {
            this.outAndRefAssignments[parameterIndex] = value;
        }

        protected object[] ValidateActionArgs(string actionType, Type[] expected, object[] received)
        {
            if (expected.Length == 0)
            {
                // target action expects no arguments, so it effectively doesn't matter what arguments we received - we can still invoke the action
                return new object[0];
            }

            var @throw = false;
            var receivedTypes = received.Select(x => x == null ? null : x.GetType());

            if (received.Length != expected.Length)
            {
                @throw = true;
            }
            else
            {
                @throw = !expected.SequenceEqual(receivedTypes, ActionTypeComparer.Instance);
            }

            if (@throw)
            {
                var receivedText = this.ConvertTypesToString(receivedTypes);
                var expectedText = this.ConvertTypesToString(expected);
                var message = string.Format(CultureInfo.InvariantCulture, "Could not execute the {0} action associated with this mocked member due to a parameter mismatch. Expected: {1} Received: {2}", actionType, expectedText, receivedText);
                throw new InvalidOperationException(message);
            }

            return received;
        }

        private string ConvertTypesToString(IEnumerable<Type> types)
        {
            return types
                    .Aggregate(
                        new StringBuilder(),
                        (sb, type) =>
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }

                        sb.Append(type == null ? "<unknown>" : type.FullName);

                        return sb;
                    },
                        sb => sb.Insert(0, "(").Append(")"))
                    .ToString();
        }

        private sealed class ActionTypeComparer : IEqualityComparer<Type>
        {
            public static readonly ActionTypeComparer Instance = new ActionTypeComparer();

            private ActionTypeComparer()
            {
            }
                
            public bool Equals(Type expectedType, Type receivedType)
            {
                Debug.Assert(expectedType != null);

                if (receivedType == null)
                {
                    // one of the arguments provided to the action must have been null, so we can't know the type and just have to assume it's OK
                    return true;
                }

                return expectedType.GetTypeInfo().IsAssignableFrom(receivedType.GetTypeInfo());
            }

            public int GetHashCode(Type obj)
            {
                return obj == null ? 0 : obj.GetHashCode();
            }
        }
    }

    public class WhenContinuation<TMock> : WhenContinuation
    {
        private Exception exception;
        private Delegate doAction;

        public void Throw()
        {
            this.Throw(new InvalidOperationException());
        }

        public void Throw(Exception exception)
        {
            this.exception = exception;
        }

        public new WhenContinuation<TMock> AssignOutOrRefParameter(int parameterIndex, object value)
        {
            base.AssignOutOrRefParameter(parameterIndex, value);
            return this;
        }

        public WhenContinuation<TMock> Do(Action doAction)
        {
            this.SetDoAction(doAction);
            return this;
        }

        public WhenContinuation<TMock> Do<T1>(Action<T1> doAction)
        {
            this.SetDoAction(doAction);
            return this;
        }

        public WhenContinuation<TMock> Do<T1, T2>(Action<T1, T2> doAction)
        {
            this.SetDoAction(doAction);
            return this;
        }

        public WhenContinuation<TMock> Do<T1, T2, T3>(Action<T1, T2, T3> doAction)
        {
            this.SetDoAction(doAction);
            return this;
        }

        public WhenContinuation<TMock> Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> doAction)
        {
            this.SetDoAction(doAction);
            return this;
        }

        public WhenContinuation<TMock> Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> doAction)
        {
            this.SetDoAction(doAction);
            return this;
        }

        private void SetDoAction(Delegate doAction)
        {
            if (this.doAction != null)
            {
                throw new InvalidOperationException("Do can only be specified once per mocked invocation.");
            }

            this.doAction = doAction;
        }

        internal override object Apply(object mockedObject, params object[] args)
        {
            if (this.exception != null)
            {
                throw this.exception;
            }

            if (this.doAction != null)
            {
                args = this.ValidateActionArgs("Do", this.doAction.GetMethodInfo().GetParameters().Select(x => x.ParameterType).ToArray(), args);
                this.doAction.DynamicInvoke(args);
            }

            return null;
        }
    }

    public sealed class WhenContinuation<TMock, TMember> : WhenContinuation<TMock>
    {
        private TMember returnValue;
        private Delegate returnAction;

        public new WhenContinuation<TMock, TMember> AssignOutOrRefParameter(int parameterIndex, object value)
        {
            base.AssignOutOrRefParameter(parameterIndex, value);
            return this;
        }

        public new WhenContinuation<TMock, TMember> Do(Action doAction)
        {
            base.Do(doAction);
            return this;
        }

        public new WhenContinuation<TMock, TMember> Do<T1>(Action<T1> doAction)
        {
            base.Do(doAction);
            return this;
        }

        public new WhenContinuation<TMock, TMember> Do<T1, T2>(Action<T1, T2> doAction)
        {
            base.Do(doAction);
            return this;
        }

        public new WhenContinuation<TMock, TMember> Do<T1, T2, T3>(Action<T1, T2, T3> doAction)
        {
            base.Do(doAction);
            return this;
        }

        public new WhenContinuation<TMock, TMember> Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> doAction)
        {
            base.Do(doAction);
            return this;
        }

        public new WhenContinuation<TMock, TMember> Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> doAction)
        {
            base.Do(doAction);
            return this;
        }

        public void Return(TMember value)
        {
            this.returnValue = value;
        }

        public void Return(Func<TMember> returnAction)
        {
            this.returnAction = returnAction;
        }

        public void Return<T1>(Func<T1, TMember> returnAction)
        {
            this.returnAction = returnAction;
        }

        public void Return<T1, T2>(Func<T1, T2, TMember> returnAction)
        {
            this.returnAction = returnAction;
        }

        public void Return<T1, T2, T3>(Func<T1, T2, T3, TMember> returnAction)
        {
            this.returnAction = returnAction;
        }

        public void Return<T1, T2, T3, T4>(Func<T1, T2, T3, T4, TMember> returnAction)
        {
            this.returnAction = returnAction;
        }

        public void Return<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TMember> returnAction)
        {
            this.returnAction = returnAction;
        }

        internal override object Apply(object mockedObject, params object[] args)
        {
            base.Apply(mockedObject, args);

            if (this.returnAction != null)
            {
                args = this.ValidateActionArgs("Return", this.returnAction.GetMethodInfo().GetParameters().Select(x => x.ParameterType).ToArray(), args);
                return this.returnAction.DynamicInvoke(args);
            }

            return this.returnValue;
        }
    }
}