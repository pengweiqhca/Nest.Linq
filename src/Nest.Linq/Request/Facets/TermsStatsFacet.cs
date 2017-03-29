// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nest.Linq.Request.Criteria;
using Nest.Linq.Utility;

namespace Nest.Linq.Request.Facets
{
    /// <summary>
    /// Represents a terms_stats facet.
    /// Terms_stats facets return all statistical information for
    /// a given field broken down by a term.
    /// </summary>
    /// <remarks>Mapped to .GroupBy(a => a.Term).Select(a => a.Sum(b => b.Field))</remarks>
    class TermsStatsFacet : TermsFacet
    {
        readonly StatisticalFacet[] statisticals;

        public TermsStatsFacet(string name, string[] fields, string[] keys, int? size)
            : this(name, null, fields, keys, size)
        {
        }

        public TermsStatsFacet(string name, ICriteria criteria, string[] fields, string[] keys)
            : this(name, criteria, fields, keys, null)
        {
        }

        public TermsStatsFacet(string name, ICriteria criteria, string[] fields, string[] keys, int? size)
            : base(name, criteria, size, fields)
        {
            Argument.EnsureNotEmpty("keys", keys);

            statisticals = keys.Select(value => new StatisticalFacet(value, value)).ToArray();
        }

        public IReadOnlyList<StatisticalFacet> Statisticals { get { return statisticals; } }
    }
}