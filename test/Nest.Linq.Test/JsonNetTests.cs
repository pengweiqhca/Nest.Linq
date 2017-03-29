// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Test.TestSupport;
using Nest;
using Nest.Linq;
using Nest.Linq.Mapping;
using Nest.Linq.Test;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace ElasticLinq.Test
{
    public class JsonNetTests: TestBase
    {
        class MyCustomMapping : NestMapping
        {
            public MyCustomMapping(IConnectionSettingsValues connectionSettings) : base(connectionSettings)
            {
            }

            public override string GetFieldName(Type type, MemberExpression memberInfo)
            {
                return string.Format("docWrapper.{0}", type.Name.ToCamelCase(CultureInfo.CurrentCulture) + "." + base.GetFieldName(type, memberInfo));
            }
        }

        [Fact]
        public static void CustomTypes_Term()
        {
            var context = new TestableElasticContext(new MyCustomMapping(ConnectionSettingsValues));
            var helloIdentifier = new Identifier("Hello");

            var queryInfo = context.Query<ClassWithIdentifier>().Where(x => x.id == helloIdentifier).ToQueryInfo();

            // Also verifies that any value which gets JSON converted into a string gets lower-cased
            Assert.Equal(@"{""bool"":{""filter"":[{""term"":{""docWrapper.classWithIdentifier.id"":{""value"":""Hello!!""}}}]}}", Serialize(queryInfo.Query));
        }

        [Fact]
        public static void CustomTypes_Terms()
        {
            var context = new TestableElasticContext();
            var identifiers = new[] { new Identifier("vALue1"), new Identifier("ValuE2") };

            var queryInfo = context.Query<ClassWithIdentifier>().Where(x => identifiers.Contains(x.id)).ToQueryInfo();

            // Also verifies that any value which gets JSON converted into a string gets lower-cased
            Assert.Equal(@"{""bool"":{""filter"":[{""terms"":{""id"":[""vALue1!!"",""ValuE2!!""]}}]}}", Serialize(queryInfo.Query));
        }

        class ClassWithIdentifier
        {
            public Identifier id { get; set; }
        }
    }
}
