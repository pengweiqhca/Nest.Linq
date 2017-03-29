// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq;
using Nest.Linq.Async;
using Nest.Linq.Utility;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using Nest.Linq.Mapping;

namespace ElasticLinq.Test
{
    /// <summary>
    /// Provides an <see cref="IQueryProvider"/> that can be used for unit tests.
    /// </summary>
    public class TestableElasticQueryProvider : IQueryProvider, IAsyncQueryExecutor
    {
        /// <summary>
        /// Create a new ElasticQueryProvider for a given connection, mapping, log, retry policy and field prefix.
        /// </summary>
        /// <param name="context">Connection to use to connect to Elasticsearch.</param>
        /// <param name="mapping">A log to receive any information or debugging messages.</param>
        /// <param name="indexName">A log to receive any information or debugging messages.</param>
        public TestableElasticQueryProvider(TestableElasticContext context, IElasticMapping mapping, string indexName)
        {
            Argument.EnsureNotNull("connection", context);
            Argument.EnsureNotNull("mapping", mapping);
            Argument.EnsureNotNull("indexName", indexName);

            Context = context;
            Mapping = mapping;
            IndexName = indexName;
        }

        internal TestableElasticContext Context { get; private set; }

        internal IElasticMapping Mapping { get; private set; }
        public string IndexName { get; }

        /// <inheritdoc/>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestableElasticQuery<TElement>(Context, expression);
        }

        /// <inheritdoc/>
        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<object>(expression);
        }

        /// <inheritdoc/>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        /// <inheritdoc/>
        public object Execute(Expression expression)
        {
            expression = TestableElasticQueryRewriter.Rewrite(expression);
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        /// <inheritdoc/>
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new CancellationToken())
        {
            return (TResult)await ExecuteAsync(expression, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken = new CancellationToken())
        {
            return await Task.Run(() => Execute(expression), cancellationToken);
        }

        class TestableElasticQueryRewriter : ExpressionVisitor
        {
            static readonly TestableElasticQueryRewriter instance = new TestableElasticQueryRewriter();

            public static Expression Rewrite(Expression expression)
            {
                Argument.EnsureNotNull("expression", expression);
                return instance.Visit(expression);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                return node.Member.DeclaringType == typeof(ElasticFields)
                    ? Expression.Convert(Expression.Constant(TypeHelper.CreateDefault(node.Type)), node.Type)
                    : base.VisitMember(node);
            }
        }
    }
}