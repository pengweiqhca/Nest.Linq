// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System.Linq;

namespace Nest.Linq
{
    /// <summary>
    /// Represents a LINQ query that will be sent to Elasticsearch.
    /// </summary>
    /// <typeparam name="T">Type of element to be queried.</typeparam>
    public interface IElasticQuery<out T> : IOrderedQueryable<T>
    {
        /// <summary>
        /// Returns the query information including the JSON payload and Uri.
        /// </summary>
        ISearchRequest ToSearchRequest();
    }
}
