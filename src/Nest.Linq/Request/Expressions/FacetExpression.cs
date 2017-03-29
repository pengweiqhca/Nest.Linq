﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Request.Facets;
using System;
using System.Linq.Expressions;

namespace Nest.Linq.Request.Expressions
{
    /// <summary>
    /// An expression tree node that represents an Elasticsearch facet.
    /// </summary>
    class FacetExpression : Expression
    {
        readonly IFacet facet;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacetExpression"/> class.
        /// </summary>
        /// <param name="facet"><see cref="IFacet" /> to represent with this expression.</param>
        public FacetExpression(IFacet facet)
        {
            this.facet = facet;
        }

        /// <summary>
        /// <see cref="IFacet" /> that is represented by this expression.
        /// </summary>
        public IFacet Facet { get { return facet; } }

        /// <inheritdoc/>
        public override ExpressionType NodeType
        {
            get { return ElasticExpressionType.Facet; }
        }

        /// <inheritdoc/>
        public override Type Type
        {
            get { return typeof(bool); }
        }

        /// <inheritdoc/>
        public override bool CanReduce
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return facet.ToString();
        }
    }
}