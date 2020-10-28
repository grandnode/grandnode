using System.Collections.Generic;

namespace Grand.Domain.Customers
{
    /// <summary>
    /// Represents a Customer ActionType
    /// </summary>
    public partial class CustomerActionType : BaseEntity
    {
        private ICollection<int> _conditionType;

        public string SystemKeyword { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public virtual ICollection<int> ConditionType
        {
            get { return _conditionType ??= new List<int>(); }
            protected set { _conditionType = value; }
        }


    }


}
