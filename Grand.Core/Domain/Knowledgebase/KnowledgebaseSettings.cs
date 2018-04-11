using Grand.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Core.Domain.Knowledgebase
{
    public class KnowledgebaseSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether knowledgebase is enabled
        /// </summary>
        public bool Enabled { get; set; }
    }
}
