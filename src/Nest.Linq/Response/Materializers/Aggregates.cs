// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Nest.Linq.Response.Materializers
{
    [DebuggerDisplay("Field {Name,nq}.{Operation,nq} = {Token}")]
    class AggregateField
    {
        readonly string name;
        readonly string operation;
        readonly JToken token;

        public AggregateField(string name, string operation, JToken token)
        {
            this.name = name;
            this.operation = operation;
            this.token = token;
        }

        public string Name { get { return name; } }
        public string Operation { get { return operation; } }
        public JToken Token { get { return token; } }
    }

    abstract class AggregateRow
    {
        static readonly DateTime epocStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static readonly DateTimeOffset epocStartDateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        internal static object GetValue(AggregateRow row, Func<string[]> func, string operation, Type valueType)
        {
            return row.GetValue(func().FirstOrDefault(), operation, valueType);
        }

        internal abstract object GetValue(string name, string operation, Type valueType);

        internal static object GetKey(AggregateRow row, Type keyType)
        {
            IDictionary<string, object> key = null;
            if (row is AggregateTermRow)
                key = ((AggregateTermRow)row).Key;
            else if (row is AggregateStatisticalRow)
                key = ((AggregateStatisticalRow)row).Key;

            if (key == null)
                return null;

            if (key.Count == 1)
                return ParseValue(JToken.FromObject(key.Values.First()), keyType);

            return ParseValue(JToken.FromObject(key), keyType);
        }

        internal static object ParseValue(JToken token, Type valueType)
        {
            switch (token.ToString())
            {
                case "Infinity":
                case "∞":
                    {
                        if (valueType == typeof(double))
                            return double.PositiveInfinity;

                        if (valueType == typeof(float))
                            return float.PositiveInfinity;

                        if (valueType == typeof(decimal?))
                            return null;

                        break;
                    }

                case "-Infinity":
                case "-∞":
                    {
                        if (valueType == typeof(double))
                            return double.NegativeInfinity;

                        if (valueType == typeof(float))
                            return float.NegativeInfinity;

                        if (valueType == typeof(decimal?))
                            return null;

                        break;
                    }
            }

            // Elasticsearch returns dates as milliseconds since epoch when using facets
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                if (valueType == typeof(DateTime))
                    return epocStartDateTime.AddMilliseconds((double)token);
                if (valueType == typeof(DateTimeOffset))
                    return epocStartDateTimeOffset.AddMilliseconds((double)token);
            }

            return token.ToObject(valueType, new JsonSerializer
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });
        }
    }

    [DebuggerDisplay("Statistical Row")]
    class AggregateStatisticalRow : AggregateRow
    {
        readonly IDictionary<string, object> key;
        readonly IDictionary<string, IAggregate> facets;

        public AggregateStatisticalRow(IDictionary<string, object> key, IDictionary<string, IAggregate> facets)
        {
            this.key = key;
            this.facets = facets;
        }
        public AggregateStatisticalRow(object key, IDictionary<string, IAggregate> facets)
        {
            this.key = new Dictionary<string, object> { { "", key } };
            this.facets = facets;
        }

        internal override object GetValue(string name, string operation, Type valueType)
        {
            if (facets.TryGetValue(name, out IAggregate agg))
            {
                if (agg is StatsAggregate statsAgg)
                    switch (operation)
                    {
                        case "avg":
                            return ParseValue(statsAgg.Average, valueType);
                        case "count":
                            return ParseValue(statsAgg.Count, valueType);
                        case "max":
                            return ParseValue(statsAgg.Max, valueType);
                        case "min":
                            return ParseValue(statsAgg.Min, valueType);
                    }
                else if (agg is SingleBucketAggregate sba && operation == "doc_count")
                    return ParseValue(sba.DocCount, valueType);
            }

            return TypeHelper.CreateDefault(valueType);
        }

        public IDictionary<string, object> Key { get { return key; } }
        public IDictionary<string, IAggregate> Facets { get { return facets; } }
    }

    [DebuggerDisplay("Term Row {Key} Fields({Fields.Count})")]
    class AggregateTermRow : AggregateRow
    {
        readonly IDictionary<string, object> key;
        readonly IReadOnlyList<AggregateField> fields;

        public AggregateTermRow(IDictionary<string, object> key, IReadOnlyList<AggregateField> fields)
        {
            this.key = key;
            this.fields = fields;
        }

        public IDictionary<string, object> Key { get { return key; } }

        public IReadOnlyList<AggregateField> Fields
        {
            get { return fields; }
        }

        internal override object GetValue(string name, string operation, Type valueType)
        {
            return ParseValue(fields.FirstOrDefault(f => f.Name == name && f.Operation == operation)?.Token, valueType);
        }
    }
}