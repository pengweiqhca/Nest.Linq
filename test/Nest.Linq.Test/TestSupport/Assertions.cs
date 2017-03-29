// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Newtonsoft.Json.Linq;
using Xunit;

namespace ElasticLinq.Test.TestSupport
{
    public static class Assertions
    {
        public static JToken TraverseWithAssert(this JToken token, params string[] paths)
        {
            foreach (var path in paths)
            {
                Assert.NotNull(token);
                if (token is JArray)
                {
                    var array = token as JArray;
                    Assert.True(array.Count > 0);
                    token = array[0];
                }
                token = token[path];
            }

            Assert.NotNull(token);
            return token;
        }
    }
}