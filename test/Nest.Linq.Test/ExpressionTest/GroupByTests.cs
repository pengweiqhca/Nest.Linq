using System;
using System.Collections.Generic;
using System.Linq;
using ElasticLinq.IntegrationTest;
using ElasticLinq.IntegrationTest.Models;
using Xunit;

namespace ElasticLinq.ExpressionTest
{
    public class GroupByTests
    {
        [Fact]
        public void GroupByInt()
        {
            DataAssert.Same<WebUser, KeyValuePair<int, int>>(s => s
                    .Aggregations(sa => sa.
                        Terms("id", d => d
                            .Field(m => m.Id)
                            .Size(1000))),
                q => q
                    .GroupBy(w => w.Id)
                    .Select(g => KeyValuePair.Create(g.Key, g.Count())));
        }

        [Fact]
        public void GroupByIntWithMax()
        {
            DataAssert.Same<WebUser, KeyValuePair<int, int>>(s => s
                    .Aggregations(sa => sa.
                        Terms("id", d => d
                            .Field(m => m.Id)
                            .Size(1000)
                            .Aggregations(da => da
                                .Stats("id", dad => dad
                                    .Field(m => m.Id))))),
                q => q
                    .GroupBy(w => w.Id)
                    .Select(g => KeyValuePair.Create(g.Key, g.Max(_ => _.Id))));
        }

        [Fact]
        public void GroupByDateTime()
        {
            DataAssert.Same<WebUser, KeyValuePair<DateTime, int>>(s => s
                    .Aggregations(sa => sa.
                        Terms("joined", d => d
                            .Field(m => m.Joined)
                            .Size(1000))),
                q => q
                    .GroupBy(w => w.Joined)
                    .Select(g => KeyValuePair.Create(g.Key, g.Count())));
        }

        [Fact]
        public void GroupByDateTimeWithMax()
        {
            DataAssert.Same<WebUser, KeyValuePair<DateTime, DateTime>>(s => s
                    .Aggregations(sa => sa.
                        Terms("joined", d => d
                            .Field(m => m.Joined)
                            .Size(1000)
                            .Aggregations(da => da
                                .Stats("joined", dad => dad
                                    .Field(m => m.Joined))))),
                q => q
                    .GroupBy(w => w.Joined)
                    .Select(g => KeyValuePair.Create(g.Key, g.Max(_ => _.Joined))));
        }

        [Fact]
        public void GroupByDatetimeWithMultipleMax()
        {
            DataAssert.Same<WebUser, KeyValuePair<int, DateTime>>(s => s
                    .Aggregations(sa => sa.
                        Terms("joined", d => d
                            .Field(m => m.Joined)
                            .Size(1000)
                            .Aggregations(da => da
                                .Stats("id", dad => dad
                                    .Field(m => m.Id))
                                .Stats("joined", dad => dad
                                    .Field(m => m.Joined))))),
                q => q
                    .GroupBy(w => w.Joined)
                    .Select(g => KeyValuePair.Create(g.Max(w => w.Id), g.Max(w => w.Joined))));
        }

        [Fact]
        public void GroupByMultiple()
        {
            DataAssert.Same<WebUser, KeyValuePair<int, DateTime>>(s => s
                    .Aggregations(a => a.
                        Terms("id", ad => ad
                            .Field(m => m.Id)
                            .Aggregations(ada => ada.
                                Terms("joined", d => d
                                    .Field(m => m.Joined)
                                    .Size(1000)
                                    .Aggregations(da => da
                                        .Stats("id", dad => dad
                                            .Field(m => m.Id))
                                        .Stats("joined", dad => dad
                                            .Field(m => m.Joined))))))),
                q => q
                    .GroupBy(w => new {w.Id, w.Joined})
                    .Select(g => KeyValuePair.Create(g.Max(w => w.Id), g.Max(w => w.Joined))));
        }

        [Fact]
        public void GroupByMultipleWithWhere()
        {
            DataAssert.Same<WebUser, KeyValuePair<int, DateTime>>(s => s
                    .Query(q => q
                        .Bool(qb => qb
                            .Filter(qbf => qbf
                                .Range(qbfr => qbfr
                                    .Field(m => m.Id)
                                    .GreaterThan(10)))))
                    .Aggregations(a => a.
                        Terms("id", ad => ad
                            .Field(m => m.Id)
                            .Aggregations(ada => ada.
                                Terms("joined", d => d
                                    .Field(m => m.Joined)
                                    .Size(1000)
                                    .Aggregations(da => da
                                        .Stats("id", dad => dad
                                            .Field(m => m.Id))
                                        .Stats("joined", dad => dad
                                            .Field(m => m.Joined))))))),
                q => q
                    .Where(g => g.Id > 10)
                    .GroupBy(w => new {w.Id, w.Joined})
                    .Select(g => KeyValuePair.Create(g.Max(w => w.Id), g.Max(w => w.Joined))));
        }
    }

    static class KeyValuePair
    {
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) where TKey : IComparable<TKey>
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}