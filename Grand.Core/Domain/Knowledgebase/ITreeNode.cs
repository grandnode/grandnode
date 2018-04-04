using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Core.Domain.Knowledgebase
{
    public interface ITreeNode
    {
        string Id { get; set; }

        string Name { get; set; }

        string ParentCategoryId { get; set; }
    }
}
