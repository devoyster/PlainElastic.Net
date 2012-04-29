﻿using PlainElastic.Net.Utils;

namespace PlainElastic.Net.IndexSettings
{
    public abstract class AnalysisComponentBase<TPart> : AnalysisBase<TPart> where TPart : AnalysisComponentBase<TPart>
    {
        private string name;

        
        public TPart Name(string name)
        {
            this.name = name;
            return (TPart)this;
        }

        /// <summary>
        /// Controls which Lucene version behavior this component should use.
        /// The highest version number is the default option.
        /// </summary>
        public TPart Version(string version)
        {
            RegisterJsonPart("'version': {0}", version.Quotate());
            return (TPart)this;
        }


        protected override string ApplyJsonTemplate(string body)
        {
            return "{0}: {{ {1} }}".F(name.Quotate(), body);
        }
    }
}