// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Mapping;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Elasticsearch.Net;

namespace Nest.Linq.Test.Request.Visitors.ElasticQueryTranslation
{
    public class ElasticQueryTranslationTestsBase
    {
        protected static readonly string IndexName = "integrationtest";
        protected static readonly IElasticMapping Mapping;
        protected static readonly IConnectionSettingsValues ConnectionSettingsValues;
        protected static readonly IElasticClient SharedClient;
        protected static readonly ElasticQueryProvider SharedProvider;

        static ElasticQueryTranslationTestsBase()
        {
            ConnectionSettingsValues = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://172.16.20.1:9200")));

            Mapping =new NestMapping(ConnectionSettingsValues);
            SharedClient = new ElasticClient(ConnectionSettingsValues);
            SharedProvider = new ElasticQueryProvider(SharedClient, Mapping, IndexName);
        }

        protected static IQueryable<Robot> Robots
        {
            get { return new ElasticQuery<Robot>(SharedProvider); }
        }

        protected static Expression MakeQueryableExpression<TSource>(string name, IQueryable<TSource> source, params Expression[] parameters)
        {
            parameters = parameters ?? new Expression[] { };

            var method = MakeQueryableMethod<TSource>(name, parameters.Length + 1);
            return Expression.Call(method, new[] { source.Expression }.Concat(parameters).ToArray());
        }

        protected static Expression MakeQueryableExpression<TSource, TResult>(IQueryable<TSource> source, Expression<Func<IQueryable<TSource>, TResult>> operation)
        {
            var methodCall = (MethodCallExpression)operation.Body;
            return Expression.Call(methodCall.Method, new[] { source.Expression }.Concat(methodCall.Arguments.Skip(1)).ToArray());
        }

        protected static MethodInfo MakeQueryableMethod<TSource>(string name, int parameterCount)
        {
            return typeof(Queryable).FindMembers
                (MemberTypes.Method,
                    BindingFlags.Static | BindingFlags.Public,
                    (info, criteria) => info.Name.Equals(criteria), name)
                .OfType<MethodInfo>()
                .Single(a => a.GetParameters().Length == parameterCount)
                .MakeGenericMethod(typeof(TSource));
        }
    }
}