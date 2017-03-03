using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using System.Configuration;

namespace Retiremate_Integration_Services.Controllers
{
    public class ProductController
    {
        #region Properties
        //set the path for the service
        private string serviceUrl = ConfigurationManager.AppSettings["ProductServiceUrl"];

        #endregion

        [Route("product")]
        public object GetProductById(string productId)
        {
            try
            {
                //check to see if parameter has been passed
                if (string.IsNullOrEmpty(productId)) return string.Empty;

                //see if there is any data returned by your query and return null if nothing
                var productData = GetData("productId", productId);
                return productData.Count > 0 ? productData[0] : null;

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Date : {0}, GetProductById Failed: {1}, ERROR :  {2}: ", DateTime.Now, "GetProductById", ex));
            }
        }

        [Route("product/search")]
        public List<object> GetProductsByName(string productName)
        {
            try
            {
                //check to see if parameter has been passed
                if (string.IsNullOrEmpty(productName)) return new List<object>();

                return GetData("productName", productName);

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Date : {0}, GetProductsByName Failed: {1}, ERROR :  {2}: ", DateTime.Now, "GetProductsByName", ex));
            }

        }

        public List<object> GetData(string methodName, string paramValue)
        {
            string responseData = string.Empty;
            var resultData = new List<object>();
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    //construct request
                    responseData = client.UploadString(serviceUrl, "POST", "{ \"" + methodName + "\": \"" + paramValue + "\" }");
                }

                //get result
                var reponseObject = JsonConvert.DeserializeObject<List<ApiResponseProduct>>(responseData);

                //check that we got data from the service call and create data object 
                if (reponseObject != null)
                {
                    for (int i = 0; i < reponseObject.Count; i++)
                    {
                        var prices = new List<object>();
                        if (reponseObject[i].PriceRecords != null)
                        {
                            foreach (var responsePrice in reponseObject[i].PriceRecords)
                            {
                                if (responsePrice.CurrencyCode == "ZAR")
                                {
                                    prices.Add(new
                                    {
                                        Price = responsePrice.SellingPrice,
                                        Currency = responsePrice.CurrencyCode
                                    });
                                }
                            } 
                        }
                        resultData.Add(new
                        {
                            Id = reponseObject[i].BarCode,
                            Name = reponseObject[i].ItemName,
                            Prices = prices
                        });
                    } 
                }

                return resultData;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Date : {0}, GetData Failed: {1} : {2}, ERROR :  {3}: ", DateTime.Now, methodName, paramValue, ex));
            }
        }


    }



    class ApiResponseProduct
    {
        public string BarCode { get; set; }
        public string ItemName { get; set; }
        public List<ApiResponsePrice> PriceRecords { get; set; }
    }

    class ApiResponsePrice
    {
        public string SellingPrice { get; set; }
        public string CurrencyCode { get; set; }
    }
}