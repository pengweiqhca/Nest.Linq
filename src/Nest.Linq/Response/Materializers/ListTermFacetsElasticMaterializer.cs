// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nest.Linq.Response.Materializers
{
    /// <summary>
    /// Materializes facets with their terms from the response.
    /// </summary>
    class ListTermFacetsElasticMaterializer : IElasticMaterializer
    {
        static readonly MethodInfo manyMethodInfo = typeof(ListTermFacetsElasticMaterializer).GetMethodInfo(f => f.Name == "Many" && !f.IsStatic);
        static readonly string[] termsFacetTypes = { "terms_stats", "terms" };

        readonly Func<AggregateRow, object> projector;
        readonly Type elementType;
        readonly Type groupKeyType;

        /// <summary>
        /// Create an instance of the <see cref="ListTermFacetsElasticMaterializer"/> with the given parameters.
        /// </summary>
        /// <param name="projector">A function to turn a hit into a desired object.</param>
        /// <param name="elementType">The type of object being materialized.</param>
        /// <param name="groupKeyType">The type of the term/group key field.</param>
        public ListTermFacetsElasticMaterializer(Func<AggregateRow, object> projector, Type elementType, Type groupKeyType)
        {
            Argument.EnsureNotNull("projector", projector);
            Argument.EnsureNotNull("elementType", elementType);
            Argument.EnsureNotNull("groupKeyType", groupKeyType);

            this.projector = projector;
            this.elementType = elementType;
            this.groupKeyType = groupKeyType;
        }

        /// <summary>
        /// Materialize the facets from an response into a list of objects.
        /// </summary>
        /// <param name="response">The <see cref="ElasticResponse"/> containing the facets to materialize.</param>
        /// <returns>List of <see cref="elementType"/> objects with these facets projected onto them.</returns>
        public object Materialize(ISearchResponse<JObject> response)
        {
            Argument.EnsureNotNull("response", response);

            var facets = response.Aggregations;
            if (facets == null || facets.Count == 0)
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

            return manyMethodInfo
                .MakeGenericMethod(elementType)
                .Invoke(this, new object[] { response.Aggregations });
        }

        /// <summary>
        /// Given a JObject of facets in an Elasticsearch structure materialize them as
        /// objects of type <typeparamref name="T"/> as created by the <see cref="projector"/>.
        /// </summary>
        /// <typeparam name="T">Type of objects to be materialized.</typeparam>
        /// <param name="aggs">Elasticsearch formatted list of facets.</param>
        /// <returns>List of materialized <typeparamref name="T"/> objects.</returns>
        internal List<T> Many<T>(IDictionary<string, IAggregate> aggs)
        {
            return aggs.Count > 0
                ? FlattenTermsToAggregateRows(aggs).Select(projector).Cast<T>().ToList()
                : new List<T>();
        }

        /// <summary>
        /// terms_stats and terms facet responses have each field in an independent object with all
        /// possible operations for that field. Multiple fields means multiple objects
        /// each of which might not have all possible terms. Convert that structure into
        /// an SQL-style row with one term per row containing each aggregate field and operation combination.
        /// </summary>
        /// <param name="termsStats">Facets of type terms or terms_stats.</param>
        /// <returns>An <see cref="IEnumerable{AggregateRow}"/> containing keys and fields representing
        /// the terms and statistics.</returns>
        internal IEnumerable<AggregateTermRow> FlattenTermsToAggregateRows(IDictionary<string, IAggregate> termsStats)
        {
            return termsStats
                .SelectMany(ts => FlattenTermsToAggregateRows(new Dictionary<string, object>(), ts))
                .Select(ts => new AggregateTermRow(ts.Key, ts.Value))
                .ToArray();
        }

        internal IDictionary<IDictionary<string, object>, IReadOnlyList<AggregateField>> FlattenTermsToAggregateRows(IDictionary<string, object> key, KeyValuePair<string, IAggregate> termsStats)
        {
            if (termsStats.Value is BucketAggregate)
            {
                var ba = termsStats.Value as BucketAggregate;
                if (ba.Items != null)
                    return ba.Items.OfType<KeyedBucket>()
                        .SelectMany(kb =>
                        {
                            var key2 = new Dictionary<string, object>(key) { [termsStats.Key] = string.IsNullOrWhiteSpace(kb.KeyAsString) ? kb.Key : kb.KeyAsString };

                            var docCount = new AggregateField(termsStats.Key, "doc_count", kb.DocCount);

                            if (kb.Aggregations != null)
                                return kb.Aggregations.SelectMany(a =>
                                {
                                    var result = FlattenTermsToAggregateRows(key2, a);

                                    if (a.Value is StatsAggregate)
                                        foreach (var _ in result.Keys.ToArray())
                                        {
                                            var list = result[_].ToList();
                                            list.Add(docCount);
                                            result[_] = list;
                                        }

                                    return result;
                                });

                            return new Dictionary<IDictionary<string, object>, IReadOnlyList<AggregateField>> { { key2, new[] { docCount } } };
                        })
                        .GroupBy(a => a.Key)
                        .ToDictionary(a => a.Key, a => a.SelectMany(_ => _.Value).ToArray() as IReadOnlyList<AggregateField>);

            }
            else if (termsStats.Value is StatsAggregate)
            {
                var sa = termsStats.Value as StatsAggregate;

                return new Dictionary<IDictionary<string, object>, IReadOnlyList<AggregateField>>
                {
                    {
                        key, new []
                        {
                            new AggregateField(termsStats.Key, "sum", sa.Sum),
                            new AggregateField(termsStats.Key, "avg", sa.Average ),
                            new AggregateField(termsStats.Key, "max", sa.Max),
                            new AggregateField(termsStats.Key, "min", sa.Min)
                        }
                    }
                };
            }

            return new Dictionary<IDictionary<string, object>, IReadOnlyList<AggregateField>>();
        }

        /// <summary>
        /// Type of element being materialized.
        /// </summary>
        internal Type ElementType { get { return elementType; } }
    }
}