// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Request.Visitors;
using Nest.Linq.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace Nest.Linq
{
    /// <summary>
    /// Represents a LINQ query object to be used with Elasticsearch.
    /// </summary>
    /// <typeparam name="T">Element type being queried.</typeparam>
    public class ElasticQuery<T> : IElasticQuery<T>
    {
        readonly ElasticQueryProvider provider;
        readonly Expression expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticQuery{T}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="ElasticQueryProvider"/> used to execute the queries.</param>
        public ElasticQuery(ElasticQueryProvider provider)
        {
            Argument.EnsureNotNull("provider", provider);

            this.provider = provider;
            expression = Expression.Constant(this);
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="provider">The <see cref="ElasticQueryProvider"/> used to execute the queries.</param>
        /// <param name="expression">The <see cref="Expression"/> that represents the LINQ query so far.</param>
        public ElasticQuery(ElasticQueryProvider provider, Expression expression)
        {
            Argument.EnsureNotNull("provider", provider);
            Argument.EnsureNotNull("expression", expression);

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException("expression");

            this.provider = provider;
            this.expression = expression;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)provider.Execute(expression)).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ISearchRequest ToSearchRequest()
        {
            var translation = ElasticQueryTranslator.Translate(provider.Mapping, expression);
            
            return translation.SearchRequest.GetNestRequest(provider.IndexName);
        }

        /// <inheritdoc/>
        public Type ElementType
        {
            get { return typeof(T); }
        }

        /// <inheritdoc/>
        public Expression Expression
        {
            get { return expression; }
        }

        /// <inheritdoc/>
        public IQueryProvider Provider
        {
            get { return provider; }
        }
    }
}