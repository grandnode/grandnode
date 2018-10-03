using MongoDB.Bson;

namespace Grand.Core
{
    public abstract class ParentEntity
    {
        protected ParentEntity()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }

        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _id = ObjectId.GenerateNewId().ToString();
                else
                    _id = value;
            }
        }

        private string _id;

    }
}
