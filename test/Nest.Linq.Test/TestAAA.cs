using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ElasticLinq.IntegrationTest.Models;
using Nest.Linq.Test.Request.Visitors.ElasticQueryTranslation;
using Xunit;

namespace Nest.Linq.Test
{
    public class TestAAA : ElasticQueryTranslationTestsBase
    {
        [Fact]
        public void SearchRequestTypeIsSetFromType()
        {
            var table = SharedClient.AsQueryable<WebUser>(IndexName);
            var aaa = from a in table
                      where a.Id > 10 && a.Id < 200
                      select new
                      {
                          a.Id,
                          a.Email
                      };

            var model = aaa.ToArray();

            Assert.NotEmpty(model);
        }

        [Fact]
        public void Count()
        {
            var table = SharedClient.AsQueryable<WebUser>(IndexName);
            var aaa = from a in table
                      where a.Id > 10 && a.Id < 200
                      select new
                      {
                          a.Id,
                          a.Email
                      };

            var model = aaa.Count();

            Assert.True(model > 0);
        }
    }
}
