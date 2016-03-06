using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Grapevine.Server
{
    /// <summary>
    /// A serializable configuration object for configuring a RESTServer.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Protocol to listen on; defaults to http
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Host name to listen on; defaults to localhost
        /// </summary>
        public string Host { get; set; }
        
        /// <summary>
        /// Port number (as a string) to listen on; defaults to 1234
        /// </summary>
        public string Port { get; set; }
        
        /// <summary>
        /// The root directory to serve files from; no default value
        /// </summary>
        public string WebRoot { get; set; }
        
        /// <summary>
        /// Default filename to look for if a directory is specified, but not a fielname; defaults to index.html
        /// </summary>
        public string DirIndex { get; set; }
        
        /// <summary>
        /// Number of threads to use to respond to incoming requests; defaults to 5
        /// </summary>
        public int MaxThreads { get; set; }

        /// <summary>
        /// Used to configure the EventLogger Exception logging
        /// </summary>
        public bool LogExceptions { get; set; }

        /// <summary>
        /// Path to serialized config file
        /// </summary>
        [JsonIgnore]
        public string Filename { get; set; }

        public Config()
        {
            this.Protocol = "http";
            this.Host = "localhost";
            this.Port = "1234";
            this.DirIndex = "index.html";
            this.MaxThreads = 5;
            this.LogExceptions = false;
        }

        /// <summary>
        /// Save a serialized version of the config object to the location specified or the default location
        /// </summary>
        public void Save(string filename = null)
        {
            try
            {
                if (object.ReferenceEquals(filename, null))
                {
                    filename = (object.ReferenceEquals(this.Filename, null)) ? GetDefaultFile() : this.Filename;
                }
                this.Filename = filename;

                File.WriteAllText(filename, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch { }
        }

        public static Config Load(string filename = null)
        {
            try
            {
                filename = (object.ReferenceEquals(filename, null)) ? GetDefaultFile() : filename;

                var json = LoadJson(filename);
                if (!object.ReferenceEquals(json, null))
                {
                    Config config = JsonConvert.DeserializeObject<Config>(json);
                    config.Filename = filename;
                    return config;
                }
            }
            catch (Exception e)
            {
                EventLogger.Log(e);
            }

            return new Config();
        }

        protected static string LoadJson(string filename)
        {
            string data = null;

            try
            {
                StreamReader reader = new StreamReader(filename);
                data = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception e)
            {
                EventLogger.Log(e);
            }

            return data;
        }

        protected static string GetDefaultFile()
        {
            return Path.Combine(new[] { Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "config.json" });
        }
    }
}
