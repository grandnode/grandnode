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

    public class TreeNode
    {
        public string text { get; set; }

        public string id { get; set; }

        public List<TreeNode> nodes { get; set; }

        public bool isCategory { get; set; }
    }
}
