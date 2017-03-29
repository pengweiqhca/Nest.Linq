using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using Nest.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ElasticLinq.IntegrationTest
{
    static class DataAssert
    {
        public static readonly Data Data = new Data();

        static DataAssert()
        {
            Data.LoadMemoryFromElastic();
        }

        public static void Same<TSource>(Func<IQueryable<TSource>, IQueryable<TSource>> query, bool ignoreOrder = false) where TSource : class
        {
            Same<TSource, TSource>(query, ignoreOrder);
        }

        public static void Same<TSource, TTarget>(Func<IQueryable<TSource>, IQueryable<TTarget>> query, bool ignoreOrder = false) where TSource : class
        {
            var expect = query(Data.Memory<TSource>()).ToList();
            var actual = query(Data.Elastic<TSource>()).ToList();

            if (ignoreOrder)
            {
                var difference = Difference(expect, actual);
                Assert.Empty(difference);
            }
            else
                SameSequence(expect, actual);
        }

        public static void SameSequence<TTarget>(List<TTarget> expect, List<TTarget> actual)
        {
            var upperBound = Math.Min(expect.Count, actual.Count);
            for (var i = 0; i < upperBound; i++)
                Assert.Equal(expect[i], actual[i]);

            Assert.Equal(expect.Count, actual.Count);
        }

        internal static IEnumerable<T> Difference<T>(IEnumerable<T> left, IEnumerable<T> right)
        {
            var rightCache = new HashSet<T>(right);
            rightCache.SymmetricExceptWith(left);
            return rightCache;
        }

        public static void Same(JToken left, JToken right)
        {
            if (left.GetType() == right.GetType())
            {
                if (left is JObject)
                {
                    var leftObject = (JObject)left;
                    var rightObject = (JObject)right;
                    foreach (var kp in leftObject)
                    {
                        Same(kp.Value, rightObject.GetValue(kp.Key));
                    }
                }
                else if (left is JArray)
                {
                    var leftArray = (JArray)left;
                    var rightArray = (JArray)right;

                    Assert.Equal(leftArray.Count, rightArray.Count);

                    for (var index = 0; index < leftArray.Count; index++)
                    {
                        Same(leftArray[index], rightArray[index]);
                    }
                }
                else
                    Assert.Equal(left, right);
            }
            else
                Assert.Equal(left, right);
        }

        public static void Same<TSource>(Func<SearchDescriptor<TSource>, ISearchRequest> left, Func<IQueryable<TSource>, IQueryable<TSource>> right) where TSource : class
        {
            Same<TSource, TSource>(left, right);
        }

        public static void Same<TSource, TTarget>(Func<SearchDescriptor<TSource>, ISearchRequest> left, Func<IQueryable<TSource>, IQueryable<TTarget>> right) where TSource : class
        {
            var requestLeft = left(new SearchDescriptor<TSource>());
            var requestRight = right(new Data().Elastic<TSource>()).ToQueryInfo();

            var stringLeft = Data.SharedClient.Serializer.SerializeToString(requestLeft, SerializationFormatting.None);
            var stringRight = Data.SharedClient.Serializer.SerializeToString(requestRight, SerializationFormatting.None);

            DataAssert.Same(JsonConvert.DeserializeObject<JToken>(stringLeft), JsonConvert.DeserializeObject<JToken>(stringRight));
        }
    }
}