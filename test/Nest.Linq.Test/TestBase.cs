using Nest.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Nest.Linq.Test
{
    public class TestBase
    {
        protected static readonly string IndexName = "integrationtest";
        protected static readonly IElasticMapping Mapping;
        protected static readonly IConnectionSettingsValues ConnectionSettingsValues;
        protected static readonly IElasticClient SharedClient;
        protected static readonly ElasticQueryProvider SharedProvider;

        static TestBase()
        {
            ConnectionSettingsValues = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://172.16.20.1:9200")));

            Mapping = new NestMapping(ConnectionSettingsValues);
            SharedClient = new ElasticClient(ConnectionSettingsValues);
            SharedProvider = new ElasticQueryProvider(SharedClient, Mapping, IndexName);
        }

        protected static string Serialize(object data) => SharedClient.Serializer.SerializeToString(data, SerializationFormatting.None);
    }
}
