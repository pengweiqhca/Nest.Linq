﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System.Linq.Expressions;

namespace Nest.Linq.Request.Expressions
{
    /// <summary>
    /// List of expression type constant numeric values used by Nest.Linq.
    /// </summary>
    static class ElasticExpressionType
    {
        public const ExpressionType Criteria = (ExpressionType)10000;
        public const ExpressionType Facet = (ExpressionType)10001;
    }
}