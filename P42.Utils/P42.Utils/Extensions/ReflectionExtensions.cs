using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace P42.Utils
{
    public static class ReflectionExtensions
    {
        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;
            PropertyInfo propInfo = null;
            do
            {
                var properties = type.GetRuntimeProperties();
                //propInfo = properties.FirstOrDefault(arg => arg.Name == propertyName);
                if (properties != null)
                    foreach (var property in properties)
                        if (property.Name == propertyName)
                            propInfo = property;
                type = type.GetTypeInfo().BaseType;
            } while (propInfo == null && type != null);
            return propInfo;
        }

        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return null;
            FieldInfo fieldInfo = null;
            do
            {
                var fields = type.GetRuntimeFields();
                //fieldInfo = fields.FirstOrDefault(arg => arg.Name == fieldName);
                if (fields != null)
                    foreach (var field in fields)
                        if (field.Name == fieldName)
                            fieldInfo = field;
                type = type.GetTypeInfo().BaseType;
            } while (fieldInfo == null && type != null);
            return fieldInfo;
        }

        public static MethodInfo GetMethodInfo(this Type type, string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
                return null;
            MethodInfo methodInfo = null;
            do
            {
                var methods = type.GetRuntimeMethods();
                //methodInfo = methods.FirstOrDefault(arg => arg.Name == methodName);
                if (methods != null)
                    foreach (var method in methods)
                        if (method.Name == methodName)
                            methodInfo = method;
                type = type.GetTypeInfo().BaseType;
            } while (methodInfo == null && type != null);
            return methodInfo;
        }

        /* Never tested
		public static string GetPropertyNameForValue(this object instance, object propertyValue)
		{
			var type = instance.GetType();
			//PropertyInfo propInfo;
			var properties = type.GetRuntimeProperties();
			foreach (var propInfo in properties)
			{
				var value = propInfo.GetValue(instance, null);
				if (value == propertyValue)
					return propInfo.Name;
			}
			return null;
		}
		*/

        public static IEnumerable<PropertyInfo> GetProperties(this object obj)
        {
            var objecType = obj.GetType();
            var properties = objecType.GetRuntimeProperties();
            return properties;
        }

        public static PropertyInfo GetProperty(this object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return null;
            var objType = obj.GetType();
            var propInfo = GetPropertyInfo(objType, propertyName);
            return propInfo;
        }

        public static List<string> PropertyNames(this object obj)
        {
            var objecType = obj.GetType();
            var properties = objecType.GetRuntimeProperties();
            var result = new List<string>();
            foreach (var property in properties)
                result.Add(property.Name);
            return result;
        }

        public static bool PropertyExists(this object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return false;
            var objType = obj.GetType();
            var propInfo = GetPropertyInfo(objType, propertyName);
            return propInfo != null;
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            return PropertyExists(obj, propertyName);
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(obj));
            var objType = obj.GetType();
            var propInfo = GetPropertyInfo(objType, propertyName);
            return propInfo?.GetValue(obj, null);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object val)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(obj));
            var objType = obj.GetType();
            var propInfo = GetPropertyInfo(objType, propertyName);
            if (propInfo == null)
                throw new ArgumentOutOfRangeException(nameof(propertyName), string.Format("Couldn't find property {0} in type {1}", propertyName, objType.FullName));
            propInfo.SetValue(obj, val, null);
        }

        public static object GetFieldValue(this object obj, string fieldName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException(nameof(obj));
            var objType = obj.GetType();
            var fieldInfo = GetFieldInfo(objType, fieldName);
            return fieldInfo?.GetValue(obj);
        }

        public static void SetFieldValue(this object obj, string fieldName, object val)
        {
            if (obj == null || string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException(nameof(obj));
            var objType = obj.GetType();
            var fieldInfo = GetFieldInfo(objType, fieldName);
            if (fieldInfo == null)
                throw new ArgumentOutOfRangeException(nameof(fieldName), string.Format("Couldn't find field {0} in type {1}", fieldName, objType.FullName));
            fieldInfo.SetValue(obj, val);
        }

        public static object GetFieldValue(this Type type, string fieldName)
        {
            var fieldInfo = GetFieldInfo(type, fieldName);
            return fieldInfo?.GetValue(null);
        }

        public static object GetPropertyValue(this Type type, string fieldName)
        {
            var propertyInfo = GetPropertyInfo(type, fieldName);
            return propertyInfo?.GetValue(null);
        }

        public static object CallMethod(this object obj, string methodName, object[] parameters)
        {
            var methodInfo = GetMethodInfo(obj.GetType(), methodName);
            if (methodInfo == null)
                throw new ArgumentOutOfRangeException(nameof(methodName), $"Couldn't find method {methodName} in type {obj.GetType().FullName}");
            return methodInfo.Invoke(obj, parameters);
        }

        public static string CallerMemberName([System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
        {
            return callerName;
        }

        public static string CallerFilePath([System.Runtime.CompilerServices.CallerFilePath] string callerPath = null)
        {
            return callerPath;
        }

        public static int CallerLineNumber([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            return lineNumber;
        }

        /*
        public static Assembly CallerAssembly()
        {
            var result = (Assembly)typeof(Assembly).GetTypeInfo().GetDeclaredMethod("GetCallingAssembly").Invoke(null, new object[0]);
            return result;
        }
        */

        public static string CallerString([System.Runtime.CompilerServices.CallerMemberName] string callerName = null, [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            return callerName + ":" + lineNumber;
        }

        public static Assembly GetApplicationAssembly()
        {
            return Assembly.GetEntryAssembly();
            /*
            var asmType = typeof(Assembly).GetTypeInfo();
            var getAppAsmMethod = asmType.GetDeclaredMethod("GetEntryAssembly");
            var result = (Assembly)getAppAsmMethod.Invoke(null, new object[0]);
            if (result == null)
                throw new Exception("P42.Utils: Could not get Executing Assembly");
            return result;
            */
        }

        public static List<Assembly> GetAssemblies()
        {
            /*
            var appDomain = typeof(string).GetTypeInfo().Assembly.GetType("System.AppDomain");
            var currentDomainProperty = appDomain.GetRuntimeProperty("CurrentDomain");
            var currentdomainMethod = currentDomainProperty.GetMethod;//.Invoke(null, new object[] { });
            var currentdomain = currentdomainMethod.Invoke(null, null);
            var getassemblies = currentdomain.GetType().GetRuntimeMethod(nameof(GetAssemblies), new Type[] { });
            var assemblies = getassemblies.Invoke(currentdomain, new object[] { }) as Assembly[];
            */
            var currentDomain = System.AppDomain.CurrentDomain;
            var assemblies = currentDomain.GetAssemblies();
            var result = assemblies?.ToList(); //new List<Assembly>(assemblies);
            return result;
        }

        public static Assembly GetAssemblyByName(string name)
        {
            var asms = GetAssemblies();
            foreach (var asm in asms)
            {
                if (asm.GetName().Name == name)
                    return asm;
            }
            return null;
        }

        public static IEnumerable<Type> GetChildClassesOf(Type parentType)
        {
            foreach (var asm in GetAssemblies())
                foreach (var type in asm.DefinedTypes)
                    if (type.IsSubclassOf(parentType))
                        yield return type.AsType();
        }

        public static Assembly GetAssembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static string SimpleQualifiedTypeName(this Type type)
        {
            /*
            System.Diagnostics.Debug.WriteLine("=================================================================");
            System.Diagnostics.Debug.WriteLine("type.ToString()="+type.ToString());
            System.Diagnostics.Debug.WriteLine("type.NameSpace="+type.Namespace);
            System.Diagnostics.Debug.WriteLine("type.Name="+type.Name);
            */
            /*
            System.Diagnostics.Debug.WriteLine("type.FullName="+type.FullName);
            System.Diagnostics.Debug.WriteLine("type.AssemblyQualifiedName"+type.AssemblyQualifiedName);
            System.Diagnostics.Debug.WriteLine("type.DeclaringType=" + type.DeclaringType);
            //System.Diagnostics.Debug.WriteLine("type.GenericParameterPosition=" + type.GenericParameterPosition);
            System.Diagnostics.Debug.WriteLine("type.GenericTypeArguments=" + type.GenericTypeArguments);
            //System.Diagnostics.Debug.WriteLine("type.GetGenericTypeDefinition()=" + type.GetGenericTypeDefinition());
            System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().AssemblyQualifiedName=" + type.GetTypeInfo().AssemblyQualifiedName);
            System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().ContainsGenericParameters=" + type.GetTypeInfo().ContainsGenericParameters);
            System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().FullName=" + type.GetTypeInfo().FullName);
            //System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().GenericTypeArguments=" + type.GetTypeInfo().GenericTypeArguments);
            //System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().GenericTypeParameters=" + type.GetTypeInfo().GenericTypeParameters);
            System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().GetElementType()=" + type.GetTypeInfo().GetElementType());
            System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().IsGenericType=" + type.GetTypeInfo().IsGenericType);
            System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().IsGenericTypeDefinition=" + type.GetTypeInfo().IsGenericTypeDefinition);
            System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().Namespace=" + type.GetTypeInfo().Namespace);
            System.Diagnostics.Debug.WriteLine("type.GetTypeInfo().ToString()=" + type.GetTypeInfo().ToString());
            */
            var result = new StringBuilder(type.Namespace + "." + type.Name);

            if (type.GetTypeInfo().IsGenericType)
            {
                var genericParameters = type.GenericTypeArguments;
                result.Append("[");
                for (int i = 0; i < genericParameters.Length; i++)
                {
                    var parameter = genericParameters[i];
                    if (i > 0)
                        result.Append(",");
                    result.Append("[" + SimpleQualifiedTypeName(parameter) + "]");
                }
                result.Append("]");
            }
            /*
            var asmFullName = type.GetAssembly().GetName().Name;
            var asmFullNameParts = asmFullName.Split(',');
            var asmName = asmFullNameParts[0];

            /*
            System.Diagnostics.Debug.WriteLine(" type.GetAssembly().ToString()=" + type.GetAssembly().ToString());
            System.Diagnostics.Debug.WriteLine(" type.GetAssembly().GetName().ToString()=" + type.GetAssembly().GetName().ToString());
            System.Diagnostics.Debug.WriteLine(" type.GetAssembly().GetName().Name=" + type.GetAssembly().GetName().Name);
            System.Diagnostics.Debug.WriteLine(" type.GetAssembly().GetName().FullName=" + type.GetAssembly().GetName().FullName);
            */

            result.Append("," + type.GetAssembly().GetName().Name);
            return result.ToString();
        }
    }
}

