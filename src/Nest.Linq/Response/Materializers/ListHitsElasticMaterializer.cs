// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Nest.Linq.Response.Materializers
{
    /// <summary>
    /// Materializes multiple hits into a list of CLR objects.
    /// </summary>
    class ListHitsElasticMaterializer : IElasticMaterializer
    {
        static readonly MethodInfo manyMethodInfo = typeof(ListHitsElasticMaterializer).GetMethodInfo(f => f.Name == "Many" && f.IsStatic);

        readonly Func<IHit<JObject>, object> projector;
        readonly Type elementType;

        /// <summary>
        /// Create an instance of the ListHitsElasticMaterializer with the given parameters.
        /// </summary>
        /// <param name="projector">A function to turn a hit into a desired CLR object.</param>
        /// <param name="elementType">The type of CLR object being materialized.</param>
        public ListHitsElasticMaterializer(Func<IHit<JObject>, object> projector, Type elementType)
        {
            this.projector = projector;
            this.elementType = elementType;
        }

        /// <summary>
        /// Materialize the hits from the response into desired CLR objects.
        /// </summary>
        /// <param name="response">The <see cref="ElasticResponse"/> containing the hits to materialize.</param>
        /// <returns>List of <see cref="elementType"/> objects as constructed by the <see cref="projector"/>.</returns>
        public object Materialize(ISearchResponse<JObject> response)
        {
            Argument.EnsureNotNull("response", response);

            var hits = response.Hits;
            if (hits == null || !hits.Any())
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

            return manyMethodInfo
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] { hits, projector });
        }

        internal static IReadOnlyList<T> Many<T>(IEnumerable<IHit<JObject>> hits, Func<IHit<JObject>, object> projector)
        {
            return hits.Select(projector).Cast<T>().ToReadOnlyBatchedList();
        }
    }
}