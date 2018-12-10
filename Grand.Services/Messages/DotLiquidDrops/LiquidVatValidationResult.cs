using DotLiquid;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidVatValidationResult : Drop
    {
        private string name;
        private string address;

        public void SetProperties(string name, string address)
        {
            this.name = name;
            this.address = address;
        }

        public string Name
        {
            get { return name; }
        }

        public string Address
        {
            get { return address; }
        }
    }
}