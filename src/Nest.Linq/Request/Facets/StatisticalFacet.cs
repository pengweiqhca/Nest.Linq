// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using Nest.Linq.Request.Criteria;
using Nest.Linq.Utility;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Nest.Linq.Request.Facets
{
    /// <summary>
    /// Represents a stastical facet.
    /// Statistical facets return all statistical information such
    /// as counts, sums, mean etc. for a given number of fields
    /// within the documents specified by the filter criteria.
    /// </summary>
    /// <remarks>Mapped to .GroupBy(a => 1).Select(a => a.Count(b => b.SomeField))</remarks>
    [DebuggerDisplay("StatisticalFacet {Field} {Filter}")]
    class StatisticalFacet : IFacet
    {
        readonly string name;
        readonly ICriteria criteria;
        readonly string field;

        public StatisticalFacet(string name, string field)
            : this(name, null, field)
        {
        }

        public StatisticalFacet(string name, ICriteria criteria, string field)
        {
            Argument.EnsureNotBlank("name", name);
            Argument.EnsureNotBlank("field", field);

            this.name = name;
            this.criteria = criteria;
            this.field = field;
        }

        public string Name { get { return name; } }

        public ICriteria Filter { get { return criteria; } }

        public string Field { get { return field; } }
    }
}