using System;
using System.Linq;

namespace Grand.Framework.Menu
{
    public static class Extensions
    {
        /// <summary>
        /// Checks whether this node or child ones has a specified system name
        /// </summary>
        /// <param name="node"></param>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public static bool ContainsSystemName(this SiteMapNode node, string systemName)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (String.IsNullOrWhiteSpace(systemName))
                return false;

            if (systemName.Equals(node.SystemName, StringComparison.OrdinalIgnoreCase))
                return true;

            return node.ChildNodes.Any(cn => ContainsSystemName(cn, systemName));
        }
    }
}
