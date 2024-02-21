namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;

    internal class ReflectionObject : DynamicObject
    {
        private const BindingFlags PublicAny = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        private const BindingFlags Any = BindingFlags.NonPublic | PublicAny;

        private object _instance;
        private Type _type;
        private BindingFlags _flags;

        Dictionary<string, MethodInfo> _cachedMethods;
        Dictionary<string, MemberInfo> _cachedMembers;

        public ReflectionObject(object o) : this(o, false) { }
        public ReflectionObject(object o, bool allowNonPublic) : this(o, allowNonPublic ? Any : PublicAny) { }
        public ReflectionObject(object o, BindingFlags bindingFlags)
        {
            if (o == null)
                throw new ArgumentNullException("o");

            _instance = o;
            _type = o.GetType();
            _flags = bindingFlags;

            _cachedMethods = new Dictionary<string, MethodInfo>();
            _cachedMembers = new Dictionary<string, MemberInfo>();
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return from member in _type.GetMembers(_flags)
                   select member.Name;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            MethodInfo method = GetCachedMethod(binder.Name, args);

            if (method == null)
                return base.TryInvokeMember(binder, args, out result);

            result = method.Invoke(_instance, args);
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            MemberInfo member = GetCachedMember(binder.Name);

            if (member == null)
                return base.TryGetMember(binder, out result);

            result = member.GetInstanceValue(_instance);
            return true;            
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            MemberInfo member = GetCachedMember(binder.Name);

            if (member == null)
                return base.TrySetMember(binder, value);

            member.SetInstanceValue(_instance, value);
            return true;
        }

        private MethodInfo GetCachedMethod(string name, object[] args)
        {
            MethodInfo method = null;

            if (!_cachedMethods.TryGetValue(name, out method))
            {
                try
                {
                    method = _type.GetMethod(name, _flags);
                }
                catch (AmbiguousMatchException)
                {
                    _cachedMethods[name] = null;
                    goto ambiguous;
                }

                if (method == null)
                    return null;

                _cachedMethods[name] = method;
            }

            ambiguous:
            if (method == null)
            {
                try
                {
                    Type[] types = ReflectionExtensions.InferArgumentTypes(args);

                    method = _type.GetMethod(name, _flags, null /* binder */, types, null /* modifiers */);
                }
                catch (ArgumentException)
                {
                    MethodInfo[] allMethods = _type.GetMethods(_flags);
                    int argsLength = args == null ? 0 : args.Length;

                    method = allMethods.FirstOrDefault(m => m.Name == name && m.GetParameters().Length == argsLength);
                }
            }

            return method;
        }
        private MemberInfo GetCachedMember(string name)
        {
            MemberInfo member = null;

            if (!_cachedMembers.TryGetValue(name, out member))
            {
                try { member = _type.GetProperty(name, _flags); }
                catch (AmbiguousMatchException) { member = _type.GetProperty(name, _flags | BindingFlags.DeclaredOnly); }

                if (member == null)
                    try { member = _type.GetField(name, _flags); }
                    catch (AmbiguousMatchException) { member = _type.GetField(name, _flags | BindingFlags.DeclaredOnly); }

                if (member == null)
                    return null;

                _cachedMembers[name] = member;
            }

            return member;
        }

    }
}