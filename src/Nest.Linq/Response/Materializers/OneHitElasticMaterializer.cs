// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Utility;
using System;
using Newtonsoft.Json.Linq;

namespace Nest.Linq.Response.Materializers
{
    /// <summary>
    /// Materializes one hit into a CLR object throwing necessary exceptions as required to ensure First/Single semantics.
    /// </summary>
    class OneHitElasticMaterializer : IElasticMaterializer
    {
        readonly Func<IHit<JObject>, object> projector;
        readonly Type elementType;
        readonly bool throwIfMoreThanOne;
        readonly bool defaultIfNone;

        /// <summary>
        /// Create an instance of the OneHitElasticMaterializer with the given parameters.
        /// </summary>
        /// <param name="projector">A function to turn a hit into a desired CLR object.</param>
        /// <param name="elementType">The type of CLR object being materialized.</param>
        /// <param name="throwIfMoreThanOne">Whether to throw an InvalidOperationException if there are multiple hits.</param>
        /// <param name="defaultIfNone">Whether to throw an InvalidOperationException if there are no hits.</param>
        public OneHitElasticMaterializer(Func<IHit<JObject>, object> projector, Type elementType, bool throwIfMoreThanOne, bool defaultIfNone)
        {
            this.projector = projector;
            this.elementType = elementType;
            this.throwIfMoreThanOne = throwIfMoreThanOne;
            this.defaultIfNone = defaultIfNone;
        }

        public object Materialize(ISearchResponse<JObject> response)
        {
            Argument.EnsureNotNull("response", response);

            using (var enumerator = response.Hits.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    if (defaultIfNone)
                        return TypeHelper.CreateDefault(elementType);
                    else
                        throw new InvalidOperationException("Sequence contains no elements");

                var current = enumerator.Current;

                if (throwIfMoreThanOne && enumerator.MoveNext())
                    throw new InvalidOperationException("Sequence contains more than one element");

                return projector(current);
            }
        }
    }
}