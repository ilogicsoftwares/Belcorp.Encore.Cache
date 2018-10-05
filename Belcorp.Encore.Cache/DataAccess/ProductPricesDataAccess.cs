using Belcorp.Encore.Cache.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Belcorp.Encore.Cache.DataAccess
{
    public class ProductPricesDataAccess
    {
        public static List<ProductPrice> GetPricesByProductId(int productID, int currencyID, int storeFrontID)
        {
            List<ProductPrice> lista = new List<ProductPrice>();
            
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["Core"].ConnectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand()
                {
                    CommandText = "GetProductPricesLast",
                    CommandType = CommandType.StoredProcedure,
                    Connection = con
                };

                var parameters = new List<SqlParameter>() {
                        new SqlParameter()
                        {
                            ParameterName ="@ID",
                            Value = productID,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter()
                        {
                            ParameterName ="@Currency",
                            Value = currencyID,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter()
                        {
                            ParameterName ="@FrontID",
                            Value = storeFrontID,
                            Direction = ParameterDirection.Input
                        }
                    };

                cmd.Parameters.AddRange(parameters.ToArray());

                using (var dtr = cmd.ExecuteReader(CommandBehavior.Default))
                {
                    while (dtr.Read())
                    {
                        lista.Add(new ProductPrice
                        {
                            Price = !dtr.IsDBNull(dtr.GetOrdinal("Price")) ? dtr.GetDecimal(dtr.GetOrdinal("Price")) : 0,
                            CurrencyID = !dtr.IsDBNull(dtr.GetOrdinal("CurrencyID")) ? dtr.GetInt32(dtr.GetOrdinal("CurrencyID")) : 0,
                            ProductID = !dtr.IsDBNull(dtr.GetOrdinal("ProductID")) ? dtr.GetInt32(dtr.GetOrdinal("ProductID")) : 0,
                            ProductPriceTypeID = !dtr.IsDBNull(dtr.GetOrdinal("ProductPriceTypeID")) ? dtr.GetInt32(dtr.GetOrdinal("ProductPriceTypeID")) : 0,
                            CatalogID = !dtr.IsDBNull(dtr.GetOrdinal("catalogid")) ? dtr.GetInt32(dtr.GetOrdinal("catalogid")) : 0,
                        });
                    }

                }
            }

            return lista;
        }
    }
}
