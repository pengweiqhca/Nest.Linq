// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System.Collections;
using Nest.Linq.Utility;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Nest.Linq.Request.Criteria
{
    /// <summary>
    /// Criteria that specifies one or more possible values that a
    /// field must match in order to select a document.
    /// </summary>
    public class TermsCriteria : SingleFieldCriteria, ITermsCriteria
    {
        readonly MemberInfo member;
        readonly ReadOnlyCollection<object> values;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermsCriteria"/> class.
        /// </summary>
        /// <param name="field">Field to be checked for this term.</param>
        /// <param name="member">Property or field being checked for this term.</param>
        /// <param name="values">Constant values being searched for.</param>
        TermsCriteria(string field, MemberInfo member, IEnumerable<object> values)
            : base(field)
        {
            this.member = member;

            this.values = new ReadOnlyCollection<object>(values.ToArray());
        }

        bool ITermsCriteria.IsOrCriteria
        {
            get { return true; }
        }

        /// <summary>
        /// Property or field being checked for this term.
        /// </summary>
        public MemberInfo Member
        {
            get { return member; }
        }

        /// <summary>
        /// Constant values being searched for.
        /// </summary>
        public ReadOnlyCollection<object> Values
        {
            get { return values; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = string.Format("terms {0} [{1}]", base.Field, string.Join(", ", Values));

            return result;
        }

        /// <summary>
        /// Builds a <see cref="TermCriteria"/> or <see cref="TermsCriteria"/>, depending on how many values are
        /// present in the <paramref name="values"/> collection.
        /// </summary>
        /// <param name="executionMode">The terms execution mode (optional). Only used when a <see cref="TermsCriteria"/> is returned.</param>
        /// <param name="field">The field that's being searched.</param>
        /// <param name="member">The member information for the field.</param>
        /// <param name="values">The values to be matched.</param>
        /// <returns>Either a <see cref="TermCriteria"/> object or a <see cref="TermsCriteria"/> object.</returns>
        internal static ITermsCriteria Build(string field, MemberInfo member, params object[] values)
        {
            Argument.EnsureNotNull("values", values);

            var hashValues = new HashSet<object>(values.SelectMany(v => v is IEnumerable ? ((IEnumerable) v).Cast<object>() : new[] {v}));
            if (hashValues.Count == 1)
                return new TermCriteria(field, member, hashValues.First());

            return new TermsCriteria(field, member, hashValues);
        }
    }
}
