using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Model;

namespace Novaroma {

    public static class Extensions {

        public static void AddRange<T>(this ConcurrentBag<T> bag, IEnumerable<T> items) {
            foreach (var item in items)
                bag.Add(item);
        }

        public static IEnumerable<T> WithoutLast<T>(this IEnumerable<T> source) {
            using (var e = source.GetEnumerator()) {
                if (e.MoveNext()) {
                    for (var value = e.Current; e.MoveNext(); value = e.Current)
                        yield return value;
                }
            }
        }

        public static async Task<bool> AllAsync(this IEnumerable<Task<bool>> source) {
            var tasks = source.ToList();

            while (tasks.Count != 0) {
                var finishedTask = await Task.WhenAny(tasks);

                if (!finishedTask.Result)
                    return false;

                tasks.Remove(finishedTask);
            }

            return true;
        }

        public static async Task<bool> AnyAsync(this IEnumerable<Task<bool>> source) {
            var tasks = source.ToList();

            while (tasks.Count != 0) {
                var finishedTask = await Task.WhenAny(tasks);

                if (finishedTask.Result)
                    return true;

                tasks.Remove(finishedTask);
            }

            return false;
        }

        public static IEnumerable<Task> RunTasks<T>(this IEnumerable<T> items, Func<T, Task> taskGetter, IExceptionHandler exceptionHandler,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            // ReSharper disable ExplicitCallerInfoArgument
            return items.Select(i => Helper.RunTask(() => taskGetter(i), exceptionHandler, callerName, callerFilePath, callerLine));
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public static List<TAttribute> GetAttributes<TAttribute>(this MemberInfo member, bool checkMetadataType = false,
                                                                 bool inherit = false) where TAttribute : Attribute {
            var retVal = member.GetCustomAttributes(inherit).OfType<TAttribute>();

            if (checkMetadataType) {
                var metadataTypeAtt = member.MemberType == MemberTypes.TypeInfo
                                          ? member.GetCustomAttribute<MetadataTypeAttribute>(inherit)
                                          : member.DeclaringType.GetCustomAttribute<MetadataTypeAttribute>(inherit);
                if (metadataTypeAtt != null) {
                    var metadataMember = metadataTypeAtt.MetadataClassType.GetMember(member.Name).FirstOrDefault();
                    if (metadataMember != null)
                        retVal = retVal.Union(metadataMember.GetCustomAttributes(inherit).OfType<TAttribute>());
                }
            }

            return retVal.ToList();
        }

        public static TAttribute GetAttribute<TAttribute>(this MemberInfo member, bool checkMetadataType = false,
                                                          bool inherit = false) where TAttribute : Attribute {
            return GetAttributes<TAttribute>(member, checkMetadataType, inherit).FirstOrDefault();
        }

        public static bool IsNumericType(this Type type) {
            if (type == null) return false;

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    return false;
            }

            return false;
        }

        public static string NovaromaName(this object obj) {
            var service = obj as INovaromaService;
            return service != null ? service.ServiceName : obj.ToString();
        }

        public static string NameWithoutExtension(this FileInfo fileInfo) {
            if (string.IsNullOrEmpty(fileInfo.Extension)) return fileInfo.Name;

            var idx = fileInfo.Name.LastIndexOf(fileInfo.Extension, StringComparison.OrdinalIgnoreCase);
            return fileInfo.Name.Substring(0, idx);
        }

        public static IQueryable<TvShowEpisode> Episodes(this IQueryable<TvShow> tvShow) {
            return tvShow.SelectMany(t => t.Seasons.SelectMany(s => s.Episodes));
        }
    }
}
