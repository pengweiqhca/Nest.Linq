// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Utility;
using Newtonsoft.Json.Linq;

namespace Nest.Linq.Response.Materializers
{
    abstract class ChainMaterializer : IElasticMaterializer
    {
        protected ChainMaterializer(IElasticMaterializer next)
        {
            Next = next;
        }

        public IElasticMaterializer Next
        {
            get; set;
        }


        /// <summary>
        /// Process response, then translate it to next materializer.
        /// </summary>
        /// <param name="response">ElasticResponse to obtain the existence of a result.</param>
        /// <returns>Return result of previous materializer, previously processed by self</returns>
        public virtual object Materialize(ISearchResponse<JObject> response)
        {
            Argument.EnsureNotNull("Next materializer must be setted.",Next);

            return Next.Materialize(response);
        }
    }
}