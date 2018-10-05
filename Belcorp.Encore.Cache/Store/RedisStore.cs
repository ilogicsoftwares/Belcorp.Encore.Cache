using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Belcorp.Encore.Cache.Store
{
    public class RedisStore
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisStore()
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { ConfigurationManager.AppSettings["redis.connection"] },
                AllowAdmin = true,
                AbortOnConnectFail=false
              
                
            };

            try { 
            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
            }
            catch(Exception ex)
            {

            }



        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;
        public static bool IsEnabled = bool.Parse(ConfigurationManager.AppSettings["ManejaElasticacheNew"]);
        public static IDatabase RedisCache => Connection.GetDatabase();
    }
}
