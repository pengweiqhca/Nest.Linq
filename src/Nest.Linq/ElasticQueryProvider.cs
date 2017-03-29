// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Async;
using Nest.Linq.Mapping;
using Nest.Linq.Request.Visitors;
using Nest.Linq.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nest.Linq
{
    /// <summary>
    /// Query provider implementation for Elasticsearch.
    /// </summary>
    public sealed class ElasticQueryProvider : IQueryProvider, IAsyncQueryExecutor
    {
        /// <summary>
        /// Create a new ElasticQueryProvider for a given connection, mapping, log, retry policy and field prefix.
        /// </summary>
        /// <param name="elasticClient">Connection to use to connect to Elasticsearch.</param>
        /// <param name="mapping">A log to receive any information or debugging messages.</param>
        /// <param name="indexName">A log to receive any information or debugging messages.</param>
        public ElasticQueryProvider(IElasticClient elasticClient, IElasticMapping mapping, string indexName)
        {
            Argument.EnsureNotNull("connection", elasticClient);
            Argument.EnsureNotNull("mapping", mapping);
            Argument.EnsureNotNull("indexName", indexName);

            ElasticClient = elasticClient;
            Mapping = mapping;
            IndexName = indexName;
        }

        internal IElasticClient ElasticClient { get; private set; }

        internal IElasticMapping Mapping { get; private set; }
        public string IndexName { get; }

        /// <inheritdoc/>
        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            Argument.EnsureNotNull("expression", expression);

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException(nameof(expression));

            return new ElasticQuery<T>(this, expression);
        }

        /// <inheritdoc/>
        public IQueryable CreateQuery(Expression expression)
        {
            Argument.EnsureNotNull("expression", expression);

            var elementType = TypeHelper.GetSequenceElementType(expression.Type);
            var queryType = typeof(ElasticQuery<>).MakeGenericType(elementType);
            try
            {
                return (IQueryable)Activator.CreateInstance(queryType, this, expression);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;  // Never called, as the above code re-throws
            }
        }

        /// <inheritdoc/>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        /// <inheritdoc/>
        public object Execute(Expression expression)
        {
            Argument.EnsureNotNull("expression", expression);

            var translation = ElasticQueryTranslator.Translate(Mapping, expression);

            var response = ElasticClient.Search<JObject>(translation.SearchRequest.GetNestRequest(IndexName));

            return translation.Materializer.Materialize(response);
        }

        /// <inheritdoc/>
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            return (TResult)await ExecuteAsync(expression, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            Argument.EnsureNotNull("expression", expression);

            var translation = ElasticQueryTranslator.Translate(Mapping, expression);

            var response= await ElasticClient.SearchAsync<JObject>(translation.SearchRequest.GetNestRequest(IndexName));

            return translation.Materializer.Materialize(response);
        }
    }
}
