// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Request.Criteria;
using Nest.Linq.Request.Facets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Nest.Linq.Utility;
using Newtonsoft.Json.Linq;

namespace Nest.Linq.Request
{
    /// <summary>
    /// Represents a search request to be sent to Elasticsearch.
    /// </summary>
    public class SearchRequest
    {
        /// <summary>
        /// Create a new SearchRequest.
        /// </summary>
        public SearchRequest()
        {
            Fields = new List<string>();
            SortOptions = new List<SortOption>();
            Aggregations = new List<IFacet>();
        }

        /// <summary>
        /// Index to start taking the Elasticsearch documents from.
        /// </summary>
        /// <remarks>Maps to the Skip statement of LINQ.</remarks>
        public int? From { get; set; }

        /// <summary>
        /// Number of documents to return from Elasticsearch.
        /// </summary>
        /// <remarks>Maps to the Take statement of LINQ.</remarks>
        public int? Size { get; set; }

        /// <summary>
        /// Type of documents to return from Elasticsearch.
        /// </summary>
        /// <remarks>Derived from the T specified in Query&lt;T&gt;.</remarks>
        public string DocumentType { get; set; }

        /// <summary>
        /// List of fields to return for each document instead of the
        /// </summary>
        public List<string> Fields { get; set; }

        /// <summary>
        /// Sort sequence for the documents. This affects From and Size.
        /// </summary>
        /// <remarks>Determined by the OrderBy/ThenBy LINQ statements.</remarks>
        public List<SortOption> SortOptions { get; set; }

        /// <summary>
        /// Filter criteria for the documents.
        /// </summary>
        /// <remarks>Determined by the Where LINQ statements.</remarks>
        public ICriteria Filter { get; set; }

        /// <summary>
        /// Query criteria for the documents.
        /// </summary>
        /// <remarks>Determined by the Query extension methods.</remarks>
        public ICriteria Query { get; set; }

        /// <summary>
        /// Facet aggregations and statistical inform that should be included.
        /// </summary>
        /// <remarks>Determined by the GroupBy/Count/Sum/Average statements of LINQ.</remarks>
        public List<IFacet> Aggregations { get; set; }

        /// <summary>
        /// Type of search Elasticsearch should perform.
        /// </summary>
        /// <remarks>Is usually blank but can be set to Count when facets are required instead of hits.</remarks>
        public string SearchType { get; set; }

        /// <summary>
        /// Minimum score of results to be returned.
        /// </summary>
        public double? MinScore { get; set; }

        /// <summary>
        /// Specify the highlighting to be applied to the results.
        /// </summary>
        public Highlight Highlight { get; set; }

        public ISearchRequest GetNestRequest(string indexNmae)
        {
            return new SearchRequestFormatter().Create(indexNmae, this);
        }

        /// <summary>
        /// Formats a SearchRequest into a JSON POST to be sent to Elasticsearch.
        /// </summary>
        class SearchRequestFormatter
        {
            //static readonly CultureInfo transportCulture = CultureInfo.InvariantCulture;

            //readonly Lazy<string> body;
            //readonly IElasticConnection connection;
            //readonly IElasticMapping mapping;
            //readonly SearchRequest searchRequest;

            ///// <summary>
            ///// Create a new SearchRequestFormatter for the given connection, mapping and search request.
            ///// </summary>
            ///// <param name="connection">The ElasticConnection to prepare the SearchRequest for.</param>
            ///// <param name="mapping">The IElasticMapping used to format the SearchRequest.</param>
            ///// <param name="searchRequest">The SearchRequest to be formatted.</param>
            //public SearchRequestFormatter(IElasticConnection connection, IElasticMapping mapping, SearchRequest searchRequest)
            //{
            //    this.connection = connection;
            //    this.mapping = mapping;
            //    this.searchRequest = searchRequest;

            //    body = new Lazy<string>(() => CreateBody().ToString(connection.Options.Pretty ? Formatting.Indented : Formatting.None));
            //}

            /// <summary>
            /// Create the Json HTTP request body for this request given the search query and connection.
            /// </summary>
            /// <returns>Json to be used to execute this query by Elasticsearch.</returns>
            public ISearchRequest Create(string indexName, SearchRequest source)
            {
                var target = new Nest.SearchRequest(indexName, source.DocumentType);

                if (source.Fields.Any())
                    target.Fields = source.Fields.ToArray();

                if (source.MinScore.HasValue)
                    target.MinScore = source.MinScore;

                // Filters cause a query to be created
                if (source.Filter != null)
                    target.Query = new QueryContainer(new BoolQuery
                    {
                        Filter = new[] { Build(source.Filter) },
                        Must = source.Query == null ? null : new[] { Build(source.Query) }
                    });
                else if (source.Query != null)
                    target.Query = Build(source.Query);

                if (source.SortOptions.Any())
                    target.Sort = source.SortOptions.Select(s => new SortField
                    {
                        Field = s.Name,
                        IgnoreUnmappedFields = s.IgnoreUnmapped,
                        Order = s.Ascending ? SortOrder.Ascending : SortOrder.Descending
                    }).ToArray();

                target.From = source.From;

                //if (source.Highlight != null)
                //    root.Add("highlight", Build(source.Highlight));

                if (source.Aggregations.Any())
                {
                    target.Size = 0;
                    target.Aggregations = source.Aggregations.ToDictionary(facet => facet.Name, facet => Build(facet, source.Size ?? 1000));
                }
                else
                    target.Size = source.Size ?? 1000;

                //if (connection.Timeout != TimeSpan.Zero)
                //    root.Add("timeout", Format(connection.Timeout));

                return target;
            }

            IAggregationContainer Build(IFacet facet, int? defaultSize)
            {
                Argument.EnsureNotNull("facet", facet);

                var container = new AggregationContainer();
                if (facet is TermsFacet terms)
                {
                    var temp = container;
                    foreach (var field in terms.Fields)
                    {
                        var agg = new TermsAggregation(facet.Name)
                        {
                            Field = field,
                            Size = 0
                        };
                        if (temp.Terms == null)
                            temp.Terms = agg;
                        else
                            temp.Aggregations = new AggregationDictionary
                            {
                                [field] = temp = new AggregationContainer
                                {
                                    Terms = agg
                                }
                            };
                    }

                    temp.Terms.Size = terms.Size ?? defaultSize;

                    if (facet is TermsStatsFacet termsStats)
                        temp.Aggregations = termsStats.Statisticals.ToDictionary(s => s.Name, s => new AggregationContainer
                        {
                            Stats = new StatsAggregation(s.Name, s.Field)
                        });
                }
                else if (facet is StatisticalFacet stats)
                {
                    container.Stats = new StatsAggregation(stats.Name, stats.Field);
                }

                if (facet.Filter != null)
                    return new AggregationContainer
                    {
                        Filter = new FilterAggregation(facet.Name)
                        {
                            Aggregations = new Dictionary<string, IAggregationContainer>
                            {
                                [facet.Name] = container
                            }
                        }
                    };

                return container;
            }

            QueryContainer Build(ICriteria criteria)
            {
                if (criteria == null)
                    return null;

                if (criteria is RangeCriteria)
                    return Build((RangeCriteria)criteria);

                if (criteria is RegexpCriteria)
                    return Build((RegexpCriteria)criteria);

                if (criteria is PrefixCriteria)
                    return Build((PrefixCriteria)criteria);

                if (criteria is TermCriteria)
                    return Build((TermCriteria)criteria);

                if (criteria is TermsCriteria)
                    return Build((TermsCriteria)criteria);

                if (criteria is NotCriteria)
                    return Build((NotCriteria)criteria);

                if (criteria is QueryStringCriteria)
                    return Build((QueryStringCriteria)criteria);

                if (criteria is MatchAllCriteria)
                    return Build((MatchAllCriteria)criteria);

                if (criteria is BoolCriteria)
                    return Build((BoolCriteria)criteria);

                // Base class formatters using name property

                if (criteria is CompoundCriteria)
                    return Build((CompoundCriteria)criteria);

                throw new InvalidOperationException(string.Format("Unknown criteria type '{0}'", criteria.GetType()));
            }

            //static JObject Build(Highlight highlight)
            //{
            //    var fields = new JObject();

            //    foreach (var field in highlight.Fields)
            //        fields.Add(new JProperty(field, new JObject()));

            //    var queryStringCriteria = new JObject(new JProperty("fields", fields));

            //    if (!string.IsNullOrWhiteSpace(highlight.PostTag))
            //        queryStringCriteria.Add(new JProperty("post_tags", new JArray(highlight.PostTag)));
            //    if (!string.IsNullOrWhiteSpace(highlight.PreTag))
            //        queryStringCriteria.Add(new JProperty("pre_tags", new JArray(highlight.PreTag)));

            //    return queryStringCriteria;
            //}

            static QueryContainer Build(QueryStringCriteria criteria)
            {
                return new QueryStringQuery
                {
                    Fields = criteria.Fields.ToArray(),
                    Query = criteria.Value
                };
            }

            #region RangeCriteria

            QueryContainer Build(RangeCriteria criteria)
            {
                FieldNameQueryBase rawQuery;
                var value = criteria.Specifications[0].Value;
                if (value is DateTime)
                {
                    var query = new DateRangeQuery();
                    Bind(criteria, query);
                    rawQuery = query;
                }
                else if (value is Distance)
                {
                    var query = new GeoDistanceRangeQuery();
                    Bind(criteria, query);
                    rawQuery = query;
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(value.GetType());

                    if (converter.CanConvertTo(typeof(double)))
                    {
                        var query = new NumericRangeQuery();
                        Bind(criteria, query, converter);
                        rawQuery = query;
                    }
                    else
                    {
                        var query = new TermRangeQuery();
                        Bind(criteria, query);
                        rawQuery = query;
                    }
                }

                rawQuery.Field = criteria.Field;

                return new QueryContainer(rawQuery);
            }

            void Bind(RangeCriteria criteria, DateRangeQuery query)
            {
                foreach (var item in criteria.Specifications)
                {
                    switch (item.Comparison)
                    {
                        case RangeComparison.GreaterThan:
                            query.GreaterThan = (DateTime)item.Value;
                            break;
                        case RangeComparison.GreaterThanOrEqual:
                            query.GreaterThanOrEqualTo = (DateTime)item.Value;
                            break;
                        case RangeComparison.LessThan:
                            query.LessThan = (DateTime)item.Value;
                            break;
                        case RangeComparison.LessThanOrEqual:
                            query.LessThanOrEqualTo = (DateTime)item.Value;
                            break;
                    }
                }
            }
            void Bind(RangeCriteria criteria, NumericRangeQuery query, TypeConverter converter)
            {
                foreach (var item in criteria.Specifications)
                {
                    switch (item.Comparison)
                    {
                        case RangeComparison.GreaterThan:
                            query.GreaterThan = (double)converter.ConvertTo(item.Value, typeof(double));
                            break;
                        case RangeComparison.GreaterThanOrEqual:
                            query.GreaterThanOrEqualTo = (double)converter.ConvertTo(item.Value, typeof(double));
                            break;
                        case RangeComparison.LessThan:
                            query.LessThan = (double)converter.ConvertTo(item.Value, typeof(double));
                            break;
                        case RangeComparison.LessThanOrEqual:
                            query.LessThanOrEqualTo = (double)converter.ConvertTo(item.Value, typeof(double));
                            break;
                    }
                }
            }
            void Bind(RangeCriteria criteria, TermRangeQuery query)
            {
                foreach (var item in criteria.Specifications)
                {
                    switch (item.Comparison)
                    {
                        case RangeComparison.GreaterThan:
                            query.GreaterThan = item.Value.ToString();
                            break;
                        case RangeComparison.GreaterThanOrEqual:
                            query.GreaterThanOrEqualTo = item.Value.ToString();
                            break;
                        case RangeComparison.LessThan:
                            query.LessThan = item.Value.ToString();
                            break;
                        case RangeComparison.LessThanOrEqual:
                            query.LessThanOrEqualTo = item.Value.ToString();
                            break;
                    }
                }
            }
            void Bind(RangeCriteria criteria, GeoDistanceRangeQuery query)
            {
                foreach (var item in criteria.Specifications)
                {
                    switch (item.Comparison)
                    {
                        case RangeComparison.GreaterThan:
                            query.GreaterThan = (Distance)item.Value;
                            break;
                        case RangeComparison.GreaterThanOrEqual:
                            query.GreaterThanOrEqualTo = (Distance)item.Value;
                            break;
                        case RangeComparison.LessThan:
                            query.LessThan = (Distance)item.Value;
                            break;
                        case RangeComparison.LessThanOrEqual:
                            query.LessThanOrEqualTo = (Distance)item.Value;
                            break;
                    }
                }
            }
            #endregion

            static QueryContainer Build(RegexpCriteria criteria)
            {
                return new RegexpQuery
                {
                    Field = criteria.Field,
                    Value = criteria.Regexp
                };
            }

            static QueryContainer Build(PrefixCriteria criteria)
            {
                return new PrefixQuery
                {
                    Field = criteria.Field,
                    Value = criteria.Prefix
                };
            }

            QueryContainer Build(TermCriteria criteria)
            {
                return new TermQuery
                {
                    Field = criteria.Field,
                    Value = criteria.Value
                };
            }

            QueryContainer Build(TermsCriteria criteria)
            {
                if (criteria.Values.Count == 1)
                    return new TermQuery
                    {
                        Field = criteria.Field,
                        Value = criteria.Values[0]
                    };

                return new TermsQuery
                {
                    Field = criteria.Field,
                    Terms = criteria.Values
                };
            }

            QueryContainer Build(NotCriteria criteria)
            {
                return new BoolQuery
                {
                    MustNot = new[] { Build(criteria.Criteria) }
                };
            }

            static QueryContainer Build(MatchAllCriteria criteria)
            {
                return new MatchAllQuery();
            }

            QueryContainer Build(CompoundCriteria criteria)
            {
                // A compound filter with one item can be collapsed
                return criteria.Criteria.Count == 1
                    ? Build(criteria.Criteria.First())
                    : Build(new BoolCriteria(criteria is AndCriteria ? criteria.Criteria : null, criteria is OrCriteria ? criteria.Criteria : null, null));
            }

            QueryContainer Build(BoolCriteria criteria)
            {
                return new BoolQuery
                {
                    Should = criteria.Should?.Select(Build),
                    Must = criteria.Must?.Select(Build),
                    MustNot = criteria.MustNot?.Select(Build),
                };
            }

            internal static string Format(TimeSpan timeSpan)
            {
                if (timeSpan.Milliseconds != 0)
                    return timeSpan.TotalMilliseconds.ToString();

                if (timeSpan.Seconds != 0)
                    return timeSpan.TotalSeconds + "s";

                return timeSpan.TotalMinutes + "m";
            }
        }
    }
}