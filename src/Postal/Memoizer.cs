using System;
using System.Collections.Concurrent;

namespace Postal
{
    /// <summary>
    /// Function memoization utilities
    /// </summary>
    static class Memoizer
    {
        /// <summary>
        /// Memoizes a unary function
        /// </summary>
        /// <typeparam name="T">The type of the function argument</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function</typeparam>
        /// <param name="func">The function to memoize</param>
        /// <returns>The memoized representation of the function</returns>
        public static Func<T, TResult> Memoize<T, TResult>(Func<T, TResult> func)
        {
            var dic = new ConcurrentDictionary<T, TResult>();
            return key => dic.GetOrAdd(key, func(key));
        } 
    }
}
