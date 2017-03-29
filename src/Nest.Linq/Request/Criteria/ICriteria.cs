// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

namespace Nest.Linq.Request.Criteria
{
    /// <summary>
    /// Interface that all criteria must implement to be part of
    /// the query filter tree.
    /// </summary>
    public interface ICriteria
    {
        /// <summary>
        /// Name of this criteria as specified in the Elasticsearch DSL.
        /// </summary>
        string Field { get; }
    }
}