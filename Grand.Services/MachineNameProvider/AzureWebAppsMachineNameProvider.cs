//Contribution: Orchard project (http://www.orchardproject.net/)
using System;

namespace Grand.Services.MachineNameProvider
{
    /// <summary>
    /// Azure websites machine name provider
    /// </summary>
    public class AzureWebAppsMachineNameProvider : IMachineNameProvider
    {
        /// <summary>
        /// Returns the name of the machine (instance) running the application.
        /// </summary>
        public string GetMachineName()
        {
            //use the code below if run on Windows Azure cloud services (web roles)
            //return Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.CurrentRoleInstance.Id;
            var name = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            if (string.IsNullOrEmpty(name))
                name = Environment.MachineName;

            //you can also use ARR affinity cookie in order to detect instance name

            return name;
        }
    }
}
