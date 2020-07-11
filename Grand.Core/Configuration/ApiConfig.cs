namespace Grand.Core.Configuration
{
    public partial class ApiConfig
    {
        public bool Enabled { get; set; }
        public bool UseSwagger { get; set; }
        public string SecretKey { get; set; }
        public bool ValidateIssuer { get; set; }
        public string ValidIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public string ValidAudience { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public int ExpiryInMinutes { get; set; }
        public bool SystemModel { get; set; }
    }
}
