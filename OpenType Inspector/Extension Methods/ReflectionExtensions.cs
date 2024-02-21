namespace OpenTypeInspector
{
    using System;
    using System.Reflection;

    static internal class ReflectionExtensions
    {
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        public static object Instantiate(this Type @this)
        {
            return InstantiateImpl(@this, null, null, PublicInstance);
        }
        public static object Instantiate<T>(this Type @this, T arg)
        {
            return InstantiateImpl(@this, ToArray(typeof(T)), ToArray<object>(arg), PublicInstance);
        }
        public static object Instantiate<T1, T2>(this Type @this, T1 arg1, T2 arg2)
        {
            return InstantiateImpl(@this, ToArray(typeof(T1), typeof(T2)), ToArray<object>(arg1, arg2), PublicInstance);
        }
        public static object Instantiate<T1, T2, T3>(this Type @this, T1 arg1, T2 arg2, T3 arg3)
        {
            return InstantiateImpl(@this, ToArray(typeof(T1), typeof(T2), typeof(T3)), ToArray<object>(arg1, arg2, arg3), PublicInstance);
        }
        public static object Instantiate(this Type @this, params object[] args)
        {
            Type[] types = InferArgumentTypes(args);

            return Instantiate(@this, types, args);
        }
        public static object Instantiate(this Type @this, Type[] types, object[] args)
        {
            return InstantiateImpl(@this, types, args, PublicInstance);
        }

        public static object InstantiateNonPublic(this Type @this)
        {
            return InstantiateImpl(@this, null, null, NonPublicInstance);
        }
        public static object InstantiateNonPublic<T>(this Type @this, T arg)
        {
            return InstantiateImpl(@this, ToArray(typeof(T)), ToArray<object>(arg), NonPublicInstance);
        }
        public static object InstantiateNonPublic<T1, T2>(this Type @this, T1 arg1, T2 arg2)
        {
            return InstantiateImpl(@this, ToArray(typeof(T1), typeof(T2)), ToArray<object>(arg1, arg2), NonPublicInstance);
        }
        public static object InstantiateNonPublic<T1, T2, T3>(this Type @this, T1 arg1, T2 arg2, T3 arg3)
        {
            return InstantiateImpl(@this, ToArray(typeof(T1), typeof(T2), typeof(T3)), ToArray<object>(arg1, arg2, arg3), NonPublicInstance);
        }
        public static object InstantiateNonPublic(this Type @this, params object[] args)
        {
            Type[] types = InferArgumentTypes(args);

            return InstantiateNonPublic(@this, types, args);
        }
        public static object InstantiateNonPublic(this Type @this, Type[] types, object[] args)
        {
            return InstantiateImpl(@this, types, args, NonPublicInstance);
        }

        private static object InstantiateImpl(Type @this, Type[] argTypes, object[] args, BindingFlags bindingFlags)
        {
            if (@this == null)
                throw new ArgumentNullException("this");

            ConstructorInfo ctor = @this.GetConstructor(bindingFlags, null /* binder */, argTypes ?? Type.EmptyTypes, null /* modifiers */);
            if (ctor == null)
                throw new MissingMethodException(@this.FullName, ".ctor");

            return ctor.Invoke(args);
        }

        private static T[] ToArray<T>(params T[] elements)
        {
            return elements;
        }

        internal static Type[] InferArgumentTypes(object[] args)
        {
            Type[] types = null;

            if (args != null)
            {
                types = new Type[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == null)
                        throw new ArgumentException("Cannot infer argument type from null value", "args[" + i + "]");

                    types[i] = args[i].GetType();
                }
            }
            return types;
        }
        internal static bool VerifyArgumentTypes(Type[] types, object[] args)
        {
            if (types == null)
                return args == null;

            if (args == null)
                return false;

            if (types.Length != args.Length)
                return false;

            for (int i = 0; i < types.Length; i++)
                if (args[i] == null)
                {
                    if (types[i].IsValueType)
                        return false;
                }
                else if (!types[i].IsInstanceOfType(args[i]))
                {
                    return false;
                }

            return true;
        }
        
        public static dynamic MakeFriend(this object @this)
        {
            if (@this == null)
                throw new ArgumentNullException("this");

            return new ReflectionObject(@this, true);
        }

        public static object GetInstanceValue(this MemberInfo memberInfo, object instance)
        {
            FieldInfo fi = memberInfo as FieldInfo;
            if (fi != null)
                return fi.GetValue(instance);

            PropertyInfo pi = memberInfo as PropertyInfo;
            if (pi != null && pi.GetIndexParameters().Length == 0)
                return pi.GetValue(instance, null);

            MethodInfo mi = memberInfo as MethodInfo;
            if (mi != null && mi.GetParameters().Length == 0)
                return mi.Invoke(instance, null);

            throw new InvalidOperationException();
        }
        public static void SetInstanceValue(this MemberInfo memberInfo, object instance, object value)
        {
            FieldInfo fi = memberInfo as FieldInfo;
            if (fi != null)
            {
                fi.SetValue(instance, value);
                return;
            }

            PropertyInfo pi = memberInfo as PropertyInfo;
            if (pi != null && pi.GetIndexParameters().Length == 0)
            {
                pi.SetValue(instance, value, null);
                return;
            }

            throw new InvalidOperationException();
        }
    }
}
