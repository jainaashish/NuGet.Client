// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Shared
{
    public static class Extensions
    {
        /// <summary>
        /// Compares two enumberables for equality, ordered according to the specified key and optional comparer. Handles null values gracefully.
        /// </summary>
        /// <typeparam name="TSource">The type of the list</typeparam>
        /// <typeparam name="TKey">The type of the sorting key</typeparam>
        /// <param name="self">This list</param>
        /// <param name="other">The other list</param>
        /// <param name="keySelector">The function to extract the key from each item in the list</param>
        /// <param name="comparer">An optional comparer for comparing keys</param>
        /// <returns></returns>
        internal static bool OrderedEquals<TSource, TKey>(this IEnumerable<TSource> self, IEnumerable<TSource> other, Func<TSource, TKey> keySelector, IComparer<TKey> orderComparer = null, IEqualityComparer<TSource> sequenceComparer = null)
        {
            if (self == null && other == null)
            {
                return true;
            }

            if (self == null || other == null)
            {
                return false;
            }

            return self.OrderBy(keySelector, orderComparer).SequenceEqual(other.OrderBy(keySelector, orderComparer), sequenceComparer);
        }
    }
}
