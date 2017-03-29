using System;
using System.Collections.Generic;
using System.Linq;
using ElasticLinq.IntegrationTest.Models;
using Xunit;

namespace ElasticLinq.IntegrationTest
{
    public class GroupByTests
    {
        [Fact]
        public void GroupByInt()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.GroupBy(w => w.Id).Select(g => KeyValuePair.Create(g.Key, g.Count())), true);
        }

        [Fact]
        public void GroupByTermless()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.GroupBy(w => 3).Select(g => KeyValuePair.Create(g.Key, g.Count())), true);
        }

        [Fact]
        public void GroupByDateTime()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.GroupBy(w => w.Joined).Select(g => KeyValuePair.Create(g.Key, g.Count())), true);
        }

        [Fact]
        public void GroupByIntWithMax()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.GroupBy(w => w.Id).Select(g => KeyValuePair.Create(g.Key, g.Max(w => w.Id))), true);
        }

        [Fact]
        public void GroupByDateTimeWithMax()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.GroupBy(w => w.Joined).Select(g => KeyValuePair.Create(g.Key, g.Max(w => w.Joined))), true);
        }

        [Fact]
        public void GroupByDatetimeWithMultipleMax()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.GroupBy(w => w.Joined).Select(g => KeyValuePair.Create(g.Max(w => w.Id), g.Max(w => w.Joined))), true);
        }

        [Fact]
        public void GroupByMultiple()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.GroupBy(w => new { w.Id, w.Joined }).Select(g => KeyValuePair.Create(g.Key.Id, g.Max(a => a.Joined))), true);
        }

        [Fact]
        public void GroupByMultipleWithMax()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.GroupBy(w => new { w.Id, w.Joined }).Select(g => KeyValuePair.Create(g.Max(w => w.Id), g.Max(w => w.Joined))), true);
        }

        [Fact]
        public void GroupByMultipleWithWhere()
        {
            DataAssert.Same((IQueryable<WebUser> q) => q.Where(g => g.Id > 10).GroupBy(w => new { w.Id, w.Joined }).Select(g => KeyValuePair.Create(g.Max(w => w.Id), g.Max(w => w.Joined))), true);
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