namespace PCLMock.Utility
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    internal struct ContinuationKey : IEquatable<ContinuationKey>
    {
        private readonly MemberInfo memberInfo;

        public ContinuationKey(MemberInfo memberInfo)
        {
            Debug.Assert(memberInfo is MethodInfo || memberInfo is PropertyInfo);
            this.memberInfo = memberInfo;
        }

        public ContinuationKeyType Type
        {
            get { return this.memberInfo is MethodInfo ? ContinuationKeyType.Method : ContinuationKeyType.Property; }
        }

        public MemberInfo MemberInfo
        {
            get { return this.memberInfo; }
        }

        public bool Equals(ContinuationKey other)
        {
            Debug.Assert(this.memberInfo != null);
            Debug.Assert(other.memberInfo != null);

            return other.memberInfo.Equals(this.memberInfo);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ContinuationKey))
            {
                return false;
            }

            return this.Equals((ContinuationKey)obj);
        }

        public override int GetHashCode()
        {
            Debug.Assert(this.memberInfo != null);
            return this.memberInfo.GetHashCode();
        }
    }
}