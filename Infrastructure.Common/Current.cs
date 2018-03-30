using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Common
{
    /// <summary>
    /// Class containing static fields for interacting with environment variables provided by cloud foundry.
    /// </summary>
    public class Current
    {
        private static bool isRunningOnCF = false;
        static Current()
        {   
            Services = new List<Service>();
            if (!string.IsNullOrWhiteSpace(GetEnvVariable(Constants.APPLICATION_ENV_VARIABLE_NAME)))
                isRunningOnCF = true;
            ParseApplication();
            ParseServices();
        }

        /// <summary>
        /// Application Name provided by developer during push
        /// </summary>
        public static string AppName { get; set; }

        /// <summary>
        /// Application ID - guid assigned by cloud foundry
        /// </summary>
        public static string ID { get; set; }

        /// <summary>
        /// The Instance ID of the app
        /// </summary>
        public static int Index { get; set; }

        /// <summary>
        /// The IP address of the cell running the application
        /// </summary>
        public static string IP { get; set; }

        /// <summary>
        /// The  port number on the cell the application is listening on
        /// </summary>
        public static int Port { get; set; }

        /// <summary>
        /// The routes being driven to the app
        /// </summary>
        public static List<string> Uris { get; set; }

        /// <summary>
        /// The Cloud Foundry assigned App Version GUID, unique to this push of the app
        /// </summary>
        public static string AppVersion { get; set; }

        /// <summary>
        /// The memory limit assigned to the instance
        /// </summary>
        public static int MemoryLimit { get; set; }

        /// <summary>
        /// Disk limit assigned to the instance
        /// </summary>
        public static int DiskLimit { get; set; }

        /// <summary>
        /// Name of the space
        /// </summary>
        public static string SpaceName { get; set; }

        /// <summary>
        /// GIUD from the space
        /// </summary>
        public static string SpaceID { get; set; }

        /// <summary>
        /// Services bound to this application
        /// </summary>
        public static List<Service> Services { get; set; }

        public static Service GetService(string serviceName)
        {
            return Services.FirstOrDefault(s => s.ServiceName == serviceName);
        }

        private static void ParseApplication()
        {
            // Services
            if (isRunningOnCloudFoundry())
            {
                dynamic jsonApplication = JObject.Parse(GetEnvVariable(Constants.APPLICATION_ENV_VARIABLE_NAME));
                AppName = jsonApplication.name;
                ID = jsonApplication.application_id;
                AppVersion = jsonApplication.application_version;
                Port = int.Parse(GetEnvVariable(Constants.PORT_ENV_VARIABLE_NAME));
                Index = int.Parse(GetEnvVariable(Constants.INSTANCE_INDEX_ENV_VARIABLE_NAME));
                IP = GetEnvVariable(Constants.IP_ENV_VARIABLE_NAME);

                // cf limits and space detail
                DiskLimit = jsonApplication.limits.disk;
                MemoryLimit = jsonApplication.limits.mem;
                SpaceName = jsonApplication.space_name;
                SpaceID = jsonApplication.space_id;

                // uris 
                Uris = new List<string>();
                foreach (string thisUri in jsonApplication.application_uris)
                {
                    Uris.Add(thisUri);
                }
            }

        }

        private static void ParseServices()
        {
            string servicesVariable = GetEnvVariable(Constants.BOUND_SERVICES_ENV_VARIABLE_NAME);
            // Services
            if (!String.IsNullOrWhiteSpace(servicesVariable))
            {
                JObject jsonServices = JObject.Parse(servicesVariable);
                // service level
                foreach (KeyValuePair<string, JToken> j in jsonServices)
                {
                    // parse the service
                    Service thisService = new Service
                    {
                        ServiceName = j.Key
                    };

                    dynamic thisJsonService = j.Value.First();
                    thisService.BindingName = thisJsonService.name;
                    thisService.Label = thisJsonService.label;
                    thisService.Plan = thisJsonService.plan;

                    string[] credentialParams = new string[] { "vhost", "hostname", "username", "password" };
                    // credentials
                    JObject credentials = thisJsonService.credentials;
                    if (credentials != null)
                    {
                        thisService.Credentials = new Dictionary<string, string>();
                        foreach (KeyValuePair<string, JToken> thisProperty in credentials)
                        {
                            if (credentialParams.Contains(thisProperty.Key))
                            {
                                Console.WriteLine($"{thisProperty.Key} : {thisProperty.Value.ToString()}");
                                // thisService.Credentials.Add(thisProperty.Key, thisProperty.Value.Value<string>());
                                thisService.Credentials.Add(thisProperty.Key, thisProperty.Value.ToString());

                            }
                            //if (!(thisProperty.Value is JArray))
                            //    thisService.Credentials.Add(thisProperty.Key, thisProperty.Value.Value<string>());
                        }
                    }

                    // tags
                    if (thisJsonService.tags != null)
                    {
                        thisService.Tags = new List<string>();
                        foreach (string s in thisJsonService.tags)
                        {
                            thisService.Tags.Add(s);
                        }
                    }
                    Services.Add(thisService);
                }
            }
        }

        public static bool isRunningOnCloudFoundry()
        {
            return isRunningOnCF;

        }

        /// <summary>
        /// Gives you a way to pull environment variables that aren't provided by this class in case future versions of the CLR use a different API to intereact with environment variables.
        /// </summary>
        /// <param name="key">The name of the environment variable you need.</param>
        /// <returns>The value of the variable if it exists, otherwise returns an empty string.</returns>
        public static string GetEnvVariable(string key)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            else
                return value;
        }

        /// <summary>
        /// Casuses the app to crash
        /// </summary>
        public static void Kill()
        {
            Console.WriteLine("Forced kill of the app.");
            Environment.Exit(-1);
        }

        public static string GetConnectionString()
        {
            string connectionStr = "host=localhost:5672;username=guest;password=guest";

            var service = Current.GetService("p-rabbitmq");
            if (service != null)
            {
                var credentials = service.Credentials;

                string hostname = credentials["hostname"];
                string vhost = credentials["vhost"];
                string username = credentials["username"];
                string password = credentials["password"];

                connectionStr = $"virtualhost={vhost};host={hostname};username={username};password={password}";
            }

            Console.WriteLine($"Connection String{System.Environment.NewLine}{connectionStr}");
            return connectionStr;
        }
    }
}
