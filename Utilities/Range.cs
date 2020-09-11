using System;
using System.Linq.Expressions;

namespace ArchivalTibiaV71MapEditor.Utilities
{
    public class Range<T> where T : struct, IConvertible
    {
        static Range()
        {
            ParameterExpression paramA = Expression.Parameter(typeof(T), "a"),
                paramB = Expression.Parameter(typeof(T), "b");
            var lessThanBody = Expression.LessThan(paramA, paramB);
            LessThan = Expression.Lambda<Func<T, T, bool>>(lessThanBody, paramA, paramB).Compile();
            var greaterThanBody = Expression.GreaterThan(paramA, paramB);
            GreaterThan = Expression.Lambda<Func<T, T, bool>>(greaterThanBody, paramA, paramB).Compile();
        }

        private static Func<T, T, bool> GreaterThan { get; }

        private static Func<T, T, bool> LessThan { get; }

        public Range(T min, T max)
        {
            Min = min;
            Max = max;
        }

        public T Min { get; }
        public T Max { get; }

        public bool InRange(T value)
        {
            return !LessThan(value, Min) && !GreaterThan(value, Max);
        }
        
    }
    
}