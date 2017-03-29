using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ElasticLinq.IntegrationTest.Models;
using Elasticsearch.Net;
using Nest;
using Nest.Linq;
using Nest.Linq.Mapping;

namespace ElasticLinq.IntegrationTest
{
    class Data
    {
        internal static readonly string IndexName = "integrationtest";
        internal static readonly IElasticMapping Mapping;
        internal static readonly IConnectionSettingsValues ConnectionSettingsValues;
        internal static readonly IElasticClient SharedClient;
        internal static readonly ElasticQueryProvider SharedProvider;
        readonly List<object> memory = new List<object>();

        static Data()
        {
            var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://172.16.20.1:9200")));

            settings.MaximumRetries(3);
            settings.DisableDirectStreaming();
            settings.OnRequestCompleted(details =>
            {
                var request = $"{details.HttpMethod} {details.Uri}";

                if (details.RequestBodyInBytes != null)
                    request += Environment.NewLine + Encoding.UTF8.GetString(details.RequestBodyInBytes);

                Trace.TraceWarning(request);
            });

            ConnectionSettingsValues = settings;

            Mapping = new NestMapping(ConnectionSettingsValues);
            SharedClient = new ElasticClient(ConnectionSettingsValues);
            SharedProvider = new ElasticQueryProvider(SharedClient, Mapping, IndexName);
        }

        public IQueryable<T> Elastic<T>() where T : class
        {
            return Elastic<T>(IndexName);
        }
        public IQueryable<T> Elastic<T>(string indexName) where T : class
        {
            return SharedClient.AsQueryable<T>(IndexName);
        }

        public IQueryable<T> Memory<T>()
        {
            return memory.AsQueryable().OfType<T>();
        }

        public void LoadMemoryFromElastic()
        {
            memory.Clear();
            memory.AddRange(SharedClient.AsQueryable<WebUser>(IndexName));
            memory.AddRange(SharedClient.AsQueryable<JobOpening>(IndexName));

            if (memory.Count != 200)
                throw new InvalidOperationException();
        }
    }
}