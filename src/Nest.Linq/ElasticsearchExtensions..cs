using Nest;
using System;
using System.Linq;
using Nest.Linq.Mapping;

namespace Nest.Linq
{
    public static class ElasticsearchExtensions
    {
        public static IQueryable<T> AsQueryable<T>(this IElasticClient client, string indexName)
            where T : class
        {
            return new ElasticQuery<T>(new ElasticQueryProvider(client, new NestMapping(client.ConnectionSettings), indexName));
        }

        //public static SearchDescriptor<T> Query<T>(this SearchDescriptor<T> sd, Func<IQueryable<T>, bool> query) where T : class
        //{
        //    return sd;
        //}
    }
}
