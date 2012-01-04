﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PlainElastic.Net.Mapping
{
    internal class Map<T>
    {
        #region Elastic Search Mapping Templates
        private const string RootObjectMap = @"
{{   
   ""{0}"" : {{
      ""dynamic"" : true,
      ""_all"" : {{""enabled"" : true}},

      ""properties"" : {{
{1}
      }}
   }}
}}
";


        private const string ObjectMap = @"
    ""{0}"" : {{
      ""properties"" : {{
{1}
      }}
   }}";
        private const string IndexPropertyMap = @"         ""{0}"" : {{ ""type"" : ""{1}"", ""boost"": {2} }}";
        private const string IgnorePropertyMap = @"         ""{0}"" : {{ ""type"" : ""{1}"", ""index"": ""no"" }}";

        #endregion


        private Map()
        {
            PropertyMapping = new List<string>();
        }


        public List<string> PropertyMapping { get; private set; }


        public static string Root(string rootType, Func<Map<T>, Map<T>> propertyMapping)
        {
            string body = GetPropertyMapping(propertyMapping);
            return RootObjectMap.F(rootType, body);
        }

        public Map<T> Index(Expression<Func<T, object>> property, int boost = 1)
        {
            string fieldName = GetPropertyName(property);
            string type = GetPropertyType(property);

            PropertyMapping.Add(IndexPropertyMap.F(fieldName, type, boost));

            return this;
        }

        public Map<T> Ignore(Expression<Func<T, object>> property)
        {
            string fieldName = GetPropertyName(property);
            string type = GetPropertyType(property);

            PropertyMapping.Add(IgnorePropertyMap.F(fieldName, type));
            return this;
        }


        public Map<T> Objects<TProp>(Expression<Func<T, IEnumerable<TProp>>> property, Func<Map<TProp>, Map<TProp>> propertyMapping)
        {
            string propertyName = GetPropertyName(property);
            string body = GetPropertyMapping(propertyMapping);

            PropertyMapping.Add(ObjectMap.F(propertyName, body));

            return this;
        }

        public Map<T> Object<TProp>(Expression<Func<T, TProp>> property, Func<Map<TProp>, Map<TProp>> propertyMapping)
        {
            string propertyName = GetPropertyName(property);
            string body = GetPropertyMapping(propertyMapping);

            PropertyMapping.Add(ObjectMap.F(propertyName, body));

            return this;
        }


        public string GenerateJsonMap()
        {
            return PropertyMapping.JoinWithSeparator(",\r\n");
        }


        private static string GetPropertyMapping<TProp>(Func<Map<TProp>, Map<TProp>> propertyMapping)
        {
            return propertyMapping.Invoke(new Map<TProp>()).GenerateJsonMap();
        }

        private static string GetPropertyType<TProp>(Expression<Func<T, TProp>> property)
        {
            var propertyType = typeof (TProp);

            // Elastic Search interprets all arrays/enumerations as strings.            
            if( typeof(IEnumerable).IsAssignableFrom(propertyType) )
                return "string";
            
            // Elastic Search stores enums as long.
            if (propertyType.IsEnum)
                return "long";

            if (propertyType == typeof (DateTime))
                return "date";

            var propertyTypeName = Reflect<T>.LowerCasedPropertyType(property);
            return propertyTypeName;
        }

        private static string GetPropertyName<TProp>(Expression<Func<T, TProp>> property)
        {
            return Reflect<T>.ShortCamelCasedPropertyName(property);
        }

    }

}
