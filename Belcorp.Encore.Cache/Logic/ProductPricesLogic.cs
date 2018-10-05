using Belcorp.Encore.Cache.DataAccess;
using Belcorp.Encore.Cache.Entities;
using Belcorp.Encore.Cache.Store;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Belcorp.Encore.Cache.Logic
{
    public class ProductPricesLogic
    {
        public bool IsEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["ManejaElasticacheNew"]);

        public int? GetPeriodID()
        {
            try
            {
                var redis = RedisStore.RedisCache;
                var hashKey = "PeriodID";
                var valor = redis.StringGet(hashKey);
                return int.Parse(valor);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public void SetPeriodID(int periodid)
        {
            var redis = RedisStore.RedisCache;
            var hashKey = "PeriodID";
            redis.StringSet(hashKey, periodid);
        }

        public int Cache_Log(int userID, DateTime date, string Message)
        {
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Core"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand ocom = new SqlCommand())
                {
                    ocom.Connection = connection;
                    ocom.CommandText = "upsInsertErrorLog";
                    ocom.Parameters.AddWithValue("@UserID", userID);
                    ocom.Parameters.AddWithValue("@LogDateUTC", date);
                    ocom.Parameters.AddWithValue("@Message", Message);
                    ocom.CommandType = CommandType.StoredProcedure;
                    return Convert.ToInt32(ocom.ExecuteScalar());
                }
            }
        }

        public IEnumerable<ProductPrice> GetPrices(int productID, int currencyID, int storeFrontId, bool DirecTDb=false)
        {

            List<ProductPrice> listaPrices = new List<ProductPrice>();

            var hashKey = "product_" + productID + "_" + storeFrontId;
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ManejaElasticacheNew"]) || DirecTDb)
            {
                var listaJash = new System.Collections.Generic.List<HashEntry>();
                         
                  var  redis = RedisStore.RedisCache;
                  var  allHash = redis.HashGetAll(hashKey);
              
                if (allHash == null || allHash.Count() == 0)
                {
                    var lista = ProductPricesDataAccess.GetPricesByProductId(productID, currencyID, storeFrontId);

                    foreach (var item in lista)
                    {
                        listaJash.Add(new HashEntry(item.ProductPriceTypeID, item.Price.ToString()));
                    }

                    try
                    {
                        redis.HashSet(hashKey, listaJash.ToArray());
                        DateTime timeNow = DateTime.Now;
                        DateTime timeMidnight = DateTime.Today.AddDays(1);
                        TimeSpan ts = timeMidnight.Subtract(timeNow);
                        redis.KeyExpire(hashKey, ts);
                    }
                    catch (Exception ax)
                    {
                        Cache_Log(0, DateTime.Now, ax.Message);
                    }
                    ///Guardar cache con expiracion
                    var ultimoPeriodo = lista.LastOrDefault().CatalogID;

                    listaPrices = lista.Where(x=>x.CatalogID==ultimoPeriodo).ToList();
                }
                else
                {

                    foreach (var item in allHash)
                    {
                        listaPrices.Add(new ProductPrice
                        {
                            ProductPriceTypeID = int.Parse(item.Name),
                            Price = decimal.Parse(item.Value.ToString()),
                            CurrencyID = currencyID,
                            ProductID = productID
                        });
                    }

                }

            }
            else
            {
                var lista = ProductPricesDataAccess.GetPricesByProductId(productID, currencyID, storeFrontId);
                var ultimoPeriodo = lista.LastOrDefault().CatalogID;
                return lista.Where(x => x.CatalogID == ultimoPeriodo).ToList();
            }




            return listaPrices;
        }

        public void ResetCache()
        {
            var endpoints = RedisStore.Connection.GetEndPoints();
            var server = RedisStore.Connection.GetServer(endpoints.First());
            server.FlushAllDatabasesAsync();
        }

    }

    public class ProductPriceLogicGMP
    {
        public bool EliminarPrices(int productID)
        {
            bool retorno = false;
            try
            {
                var redis = RedisStore.RedisCache;

                var hashKey = "product_" + productID + "_1";
                retorno = redis.KeyDelete(hashKey);
                hashKey = "product_" + productID + "_2";
                retorno = redis.KeyDelete(hashKey);
                hashKey = "product_" + productID + "_3";
                retorno = redis.KeyDelete(hashKey);
            }
            catch (Exception ex)
            {
                Cache_Log(0, DateTime.Now, ex.Message);
            }
            return retorno;
        }

        public int Cache_Log(int userID, DateTime date, string Message)
        {
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Core"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand ocom = new SqlCommand())
                {
                    ocom.Connection = connection;
                    ocom.CommandText = "upsInsertErrorLog";
                    ocom.Parameters.AddWithValue("@UserID", userID);
                    ocom.Parameters.AddWithValue("@LogDateUTC", date);
                    ocom.Parameters.AddWithValue("@Message", Message);
                    ocom.CommandType = CommandType.StoredProcedure;
                    return Convert.ToInt32(ocom.ExecuteScalar());
                }
            }
        }
    }

}
