// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Nest.Linq.Mapping
{
    /// <summary>
    /// Interface to describe how types and properties are mapped into Elasticsearch.
    /// </summary>
    public interface IElasticMapping
    {
        /// <summary>
        /// Gets the document type name for the given CLR type. Extending this allows you to change the
        /// mapping of types names in the CLR to document type names in Elasticsearch. For example,
        /// using the Couchbase/Elasticsearch adapter yields documents with the document type
        /// "couchbaseDocument", regardless of the CLR type.
        /// </summary>
        /// <param name="type">The type whose name is required.</param>
        /// <returns>Returns the Elasticsearch document type name that matches the type; may
        /// return <c>null</c> or empty string to not limit searches to a document type.</returns>
        string GetDocumentType(Type type);

        /// <summary>
        /// Gets the field name for the given member. Extending this allows you to change the
        /// mapping field names in the CLR to field names in Elasticsearch. Typically, these rules
        /// will need to match the serialization rules you use when storing your documents.
        /// </summary>
        /// <param name="type">The type used in the source query.</param>
        /// <param name="memberExpression">The member expression whose name is required.</param>
        /// <returns>Returns the Elasticsearch field name that matches the member.</returns>
        string GetFieldName(Type type, MemberExpression memberExpression);

        /// <summary>
        /// Materialize the JObject hit object from Elasticsearch to a CLR object.
        /// </summary>
        /// <param name="sourceDocument">JSON source document.</param>
        /// <param name="sourceType">Type of CLR object to materialize to.</param>
        /// <returns>Freshly materialized CLR object version of the source document.</returns>
        object Materialize(JToken sourceDocument, Type sourceType);
    }
}