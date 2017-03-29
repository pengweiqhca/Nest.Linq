using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nest.Linq.Mapping
{
    class NestMapping : IElasticMapping
    {
        private readonly IConnectionSettingsValues _connectionSettings;

        public NestMapping(IConnectionSettingsValues connectionSettings)
        {
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }
        public virtual string GetDocumentType(Type type)
        {
            return new TypeNameResolver(_connectionSettings).Resolve(type);
        }

        public virtual string GetFieldName(Type type, MemberExpression memberExpression)
        {
            return new FieldResolver(_connectionSettings).Resolve(new Field { Expression = memberExpression });
        }

        public virtual object Materialize(JToken sourceDocument, Type sourceType)
        {
            return sourceDocument.ToObject(sourceType);
        }
    }
}
