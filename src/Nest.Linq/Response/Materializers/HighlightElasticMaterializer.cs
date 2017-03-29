// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System;
using Newtonsoft.Json.Linq;

namespace Nest.Linq.Response.Materializers
{
    class HighlightElasticMaterializer : ChainMaterializer
    {
        public HighlightElasticMaterializer(IElasticMaterializer previous)
            : base(previous)
        {
        }

        /// <summary>
        /// Add to response fields that needs to read highlighted info.
        /// </summary>
        /// <param name="response">ElasticResponse to obtain the existence of a result.</param>
        /// <returns>Return result of next materializer</returns>
        public override object Materialize(ISearchResponse<JObject> response)
        {
            foreach (var hit in response.Hits)
            {
                if (hit.Highlights == null) continue;
                throw new NotImplementedException();
                //foreach (var prop in hit.Highlights.Properties())
                //{
                //    hit._source.Add(string.Format("{0}_highlight", prop.Name), prop.Value);
                //}
            }

            return base.Materialize(response);
        }
    }
}