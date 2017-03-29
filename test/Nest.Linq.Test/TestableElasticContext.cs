// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Nest;

namespace ElasticLinq.Test
{
    /// <summary>
    /// Provides an IElasticContext that can be used by unit tests.
    /// </summary>
    public class TestableElasticContext
    {
        readonly Dictionary<Type, object> data = new Dictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestableElasticContext"/> class.
        /// </summary>
        /// <param name="mapping">The <see cref="IElasticMapping"/> used to define mapping between the document and object.</param>
        /// <param name="log">The  <see cref="ILog"/> instance to receive logging information.</param>
        /// <param name="maxAttempts">Maximum number of attempts to try before failing.</param>
        /// <param name="timeout">How long to wait before failing a request.</param>
        public TestableElasticContext(IElasticMapping mapping = null,
                                      int maxAttempts = 1,
                                      TimeSpan timeout = default(TimeSpan))
        {
            Mapping = mapping ?? new NestMapping(new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://172.16.20.1:9200"))));
            Provider = new TestableElasticQueryProvider(this, Mapping, "integrationtest");
        }

        /// <summary>
        /// The mapping to describe how objects and their properties are mapped to Elasticsearch.
        /// </summary>
        public IElasticMapping Mapping { get; private set; }

        /// <summary>
        /// Access the underlying <see cref="TestableElasticQueryProvider" />.
        /// </summary>
        public TestableElasticQueryProvider Provider { get; private set; }

        /// <summary>
        /// The in-memory data to be used for results when performing queries.
        /// </summary>
        /// <typeparam name="T">Type of in-memory data to retrieve.</typeparam>
        /// <returns>The in-memory data of the given type that will be used to test queries against.</returns>
        public IEnumerable<T> Data<T>()
        {
            object result;
            if (!data.TryGetValue(typeof(T), out result))
                result = Enumerable.Empty<T>();

            return (IEnumerable<T>)result;
        }

        /// <summary>
        /// Set the in-memory data for the type given.
        /// </summary>
        /// <typeparam name="T">Type of in-memory data to store.</typeparam>
        /// <param name="values">The objects to use when testing queries against this type.</param>
        public void SetData<T>(IEnumerable<T> values)
        {
            data[typeof(T)] = values.ToList();
        }

        /// <summary>
        /// Set the in-memory data for the type given.
        /// </summary>
        /// <typeparam name="T">Type of in-memory data to store.</typeparam>
        /// <param name="values">The objects to use when testing queries against this type.</param>
        public void SetData<T>(params T[] values)
        {
            SetData((IEnumerable<T>)values);
        }

        /// <inheritdoc/>
        public IQueryable<T> Query<T>()
        {
            return new TestableElasticQuery<T>(this);
        }
    }
}