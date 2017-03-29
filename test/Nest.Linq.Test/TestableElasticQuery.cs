// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Nest;
using Nest.Linq;
using Nest.Linq.Request.Visitors;

namespace ElasticLinq.Test
{
    /// <summary>
    /// Provides an <see cref="IElasticQuery{T}"/> that can be used by unit tests.
    /// </summary>
    /// <typeparam name="T">Element type this query is for.</typeparam>
    public class TestableElasticQuery<T> : IElasticQuery<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestableElasticQuery{T}" /> class.
        /// </summary>
        /// <param name="context">The <see cref="TestableElasticContext"/> this query belongs to.</param>
        /// <param name="expression">The <see cref="Expression"/> that represents the LINQ query.</param>
        public TestableElasticQuery(TestableElasticContext context, Expression expression = null)
        {
            Context = context;
            ElementType = typeof(T);
            Expression = expression ?? Expression.Constant(context.Data<T>().AsQueryable());
        }

        /// <summary>
        /// The <see cref="TestableElasticContext"/> this query belongs to.
        /// </summary>
        public TestableElasticContext Context { get; private set; }

        /// <inheritdoc/>
        public Type ElementType { get; private set; }

        /// <inheritdoc/>
        public Expression Expression { get; private set; }

        /// <inheritdoc/>
        public IQueryProvider Provider { get { return Context.Provider; } }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Provider.Execute(Expression)).GetEnumerator();
        }

        public ISearchRequest ToSearchRequest()
        {
            var translation = ElasticQueryTranslator.Translate(Context.Mapping, Expression);

            return translation.SearchRequest.GetNestRequest(Context.Provider.IndexName);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
