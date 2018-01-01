using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using NLog;

/// <summary>
/// ServiceHelper 的摘要说明
/// </summary>
public class ServiceHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    public ServiceHelper()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    #region CompleteReturn
    public string CompleteReturn(string data, string jetDefinedOrderId)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/returns/{jetDefinedOrderId}/complete";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region AcknowledgeOrder
    public string AcknowledgeOrder(string data, string jetDefinedOrderId)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/orders/{jetDefinedOrderId}/acknowledge";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region ShipOrder
    public string ShipOrder(string data, string jetDefinedOrderId)
    {
        try
        {
            var ob = JsonConvert.DeserializeObject<Ship>(data);
            var ship = ob.shipments[0];
            if (!string.IsNullOrEmpty(ship["response_shipment_date"].ToString()))
            {
                ship["response_shipment_date"] = Convert.ToDateTime(ship["response_shipment_date"]).ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss.fffffff-hh:mm");
            }
            else
            {
                ship.Remove("response_shipment_date");
            }
            if (!string.IsNullOrEmpty(ship["expected_delivery_date"].ToString()))
            {
                ship["expected_delivery_date"] = Convert.ToDateTime(ship["expected_delivery_date"]).ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss.fffffff-hh:mm");
            }
            else
            {
                ship.Remove("expected_delivery_date");
            }
            if (string.IsNullOrEmpty(ship["shipment_tracking_number"].ToString()))
            {
                ship.Remove("shipment_tracking_number");
            }
            if (string.IsNullOrEmpty(ship["response_shipment_method"].ToString()))
            {
                ship.Remove("response_shipment_method");
            }
            if (string.IsNullOrEmpty(ship["ship_from_zip_code"].ToString()))
            {
                ship.Remove("ship_from_zip_code");
            }
            if (string.IsNullOrEmpty(ship["carrier"].ToString()))
            {
                ship.Remove("carrier");
            }
            if (string.IsNullOrEmpty(ship["alt_shipment_id"].ToString()))
            {
                ship.Remove("alt_shipment_id");
            }
            var items = ship["shipment_items"];
            var itemObj = JsonConvert.DeserializeObject<List<Dictionary<string, Object>>>(items.ToString());
            if (Convert.ToInt32(itemObj[0]["response_shipment_sku_quantity"].ToString()) == 0)
            {
                itemObj[0].Remove("response_shipment_sku_quantity");
                ship["shipment_items"] = itemObj;
            }
            //ship["carrier_pick_up_date"] =Convert.ToDateTime( ship["carrier_pick_up_date"]).ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss.fffffff-hh:mm");
            data = JsonConvert.SerializeObject(ob);
            var url = $"https://merchant-api.jet.com/api/orders/{jetDefinedOrderId}/shipped";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region QueryOrder
    public string QueryOrder(string status, string isCancel, string nodeId)
    {
        try
        {
            var cancel = isCancel == "1";
            var url =
                $"https://merchant-api.jet.com/api/orders/{status}?isCancelled={cancel}&fulfillment_node={nodeId}";
            var result = GetData(url);
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            var orderUrls = response["order_urls"].ToString();
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\",\"orderList\":" + orderUrls + "}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region QueryOrderDetail
    public string QueryOrderDetail(string jetDefinedOrderId)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/orders/withoutShipmentDetail/{jetDefinedOrderId}";
            var result = GetData(url);
            return result;
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region SearchInvenotry
    public string SearchInvenotry(string skuId)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/merchant-skus/{skuId}/inventory";
            var result = GetData(url);
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            var nodes = response["fulfillment_nodes"].ToString();

            //var fulfillmentNodes = JsonConvert.SerializeObject(nodes);
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\",\"fulfillmentNodes\":" + nodes + "}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region UploadSKU
    public string UploadSKU(string id, string data)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/merchant-skus/{id}";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region PriceUpload
    public string PriceUpload(string skuId, decimal price)
    {
        try
        {
            var url = string.Format("https://merchant-api.jet.com/api/merchant-skus/{0}/price", skuId);
            string data = "{\"price\":" + price + "}";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }

    }
    #endregion

    #region SearchSKU
    public string SearchSKU(string skuId)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/merchant-skus/{skuId}";
            var response = JsonConvert.DeserializeObject<Dictionary<string, Object>>(GetData(url));
            response.Add("returnCode", "0000");
            response.Add("returnMessage", "request success");
            var result = JsonConvert.SerializeObject(response);
            return result;
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region InventoryUpload
    public string InventoryUpload(string skuId, int qty, string nodeId)
    {
        try
        {
            var url = string.Format("https://merchant-api.jet.com/api/merchant-skus/{0}/Inventory", skuId);
            string data = "{\"fulfillment_nodes\":[{\"fulfillment_node_id\":\"" + nodeId + "\",\"quantity\":" + qty + "}]}";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }

    }
    #endregion

    #region UserLogin
    public string UserLogin(string userName, string pwd)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(pwd))
        {
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"Please enter the username and password\"}";
        }
        GetConfig();
        if (userName == UserName && pwd == Password)
        {
            HttpContext.Current.Session["login"] = UserName;
        }
        return userName == UserName && pwd == Password
            ? "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\"}"
            : "{\"returnCode\":\"0096\",\"returnMessage\":\"User name or password error\"}";
    }
    #endregion

    #region GetToken
    public void GetToken(bool needRefresh = false, bool last = false)
    {

        try
        {
            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Pass))
            {
                GetConfig();
            }
            if (needRefresh || string.IsNullOrEmpty(Token))
            {
                var url = "https://merchant-api.jet.com/api/token";
                var data = "{ \"user\":\"" + User + "\",\"pass\":\"" + Pass + "\" }";
                var response = PostData(url, data, "POST", false, last);
                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("system error");
                    //return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
                }
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                string token = "";
                jsonData.TryGetValue("id_token", out token);
                Token = token;
                WriteConfig();
            }

        }
        catch (Exception e)
        {
            Logger.Error(e);
            throw;
        }
    }
    #endregion

    #region GetData
    public string GetData(string url, bool islast = false)
    {
        try
        {
            if (string.IsNullOrEmpty(Token))
            {
                GetToken();
            }
            ServicePointManager.ServerCertificateValidationCallback += delegate
            {
                return true;
            };
            Logger.Info($"请求地址:{url}");
            // 设置提交的相关参数 

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Headers.Add("Authorization", "bearer " + Token);           
            request.Method = "GET";
            request.ContentType = "application/json";
            // 提交请求数据 
            HttpWebResponse response;
            Stream responseStream;
            StreamReader reader;
            response = request.GetResponse() as HttpWebResponse;
            responseStream = response.GetResponseStream();
            reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
            string result = reader.ReadToEnd();
            reader.Close();
            Logger.Info("请求服务获得返回:" + result);
            return result;
        }
        catch (WebException e)
        {
            Logger.Error(e);
            HttpWebResponse resp = (HttpWebResponse)e.Response;
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!islast)
                {
                    GetToken(needRefresh: true, last: true);
                    return GetData(url, true);
                }
                else
                {
                    throw new TransactionException("0091", "request token error");
                }
            }
            else
            {
                switch (resp.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new TransactionException("0400", "Incorrect request data or no information was found");
                    case HttpStatusCode.NoContent:
                        throw new TransactionException("0200", "No information was found");
                    default:
                        throw;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion

    #region PostData
    public string PostData(string url, string data, string method, bool needToken = true, bool islast = false)
    {
        try
        {
            if (string.IsNullOrEmpty(Token) && needToken)
            {
                GetToken();
            }
            ServicePointManager.ServerCertificateValidationCallback += delegate
            {
                return true;
            };
            Logger.Info($"请求地址:{url},发送报文:{data}");
            byte[] postData = Encoding.UTF8.GetBytes(data);
            // 设置提交的相关参数 
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (needToken)
            {
                request.Headers.Add("Authorization", "bearer " + Token);
            }         
            request.Method = method;
            request.ContentType = "application/json";
            // 提交请求数据 
            Stream outputStream = request.GetRequestStream();
            outputStream.Write(postData, 0, postData.Length);
            outputStream.Close();
            HttpWebResponse response;
            Stream responseStream;
            StreamReader reader;
            response = request.GetResponse() as HttpWebResponse;
            if (method == "PUT")
            {
                return "";
            }
            else
            {
                responseStream = response.GetResponseStream();
                reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                string result = reader.ReadToEnd();
                reader.Close();
                Logger.Info("请求服务获得返回:" + result);
                return result;
            }

        }
        catch (WebException e)
        {
            Logger.Error(e);
            HttpWebResponse resp = (HttpWebResponse)e.Response;
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!islast)
                {
                    GetToken(needRefresh: true, last: true);
                    return PostData(url, data, method, needToken, true);
                }
                else
                {
                    throw new TransactionException("0091", "request token error");
                }

            }
            else
            {
                var msg= ProcessError(resp);
                switch (resp.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new TransactionException("0400", msg);
                    case HttpStatusCode.NoContent:
                        throw new TransactionException("0200", msg);
                    default:
                        throw;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion

    #region GetConfig
    public void GetConfig()
    {
        var path = HttpContext.Current.Server.MapPath("../config.json");
        var config = File.ReadAllText(path, Encoding.UTF8);
        var configData = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);
        User = configData["user"];
        Pass = configData["pass"];
        Token = configData["token"];
        Proxy = configData["proxy"];
        UserName = configData["userName"];
        Password = configData["password"];
    }
    #endregion

    #region WriteConfig
    public void WriteConfig()
    {
        var path = HttpContext.Current.Server.MapPath("../config.json");
        var dic = new Dictionary<string, string>()
        {
            {"user",User },
            {"pass",Pass },
            {"token",Token },
            {"proxy",Proxy },
            {"userName",UserName },
            {"password",Password }
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(dic), Encoding.UTF8);
    }
    #endregion

    #region QueryReturnOrder
    public string QueryReturnOrder(string status)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/returns/{status}";
            var result = GetData(url);
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            var orderUrls = response["return_urls"].ToString();
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"request success\",\"orderList\":" + orderUrls + "}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region QueryReturnOrderDetail
    public string QueryReturnOrderDetail(string jetDefinedOrderId)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/returns/state/{jetDefinedOrderId}";
            var result = GetData(url);
            return result;
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\"" + ex.ErrorCode + "\",\"returnMessage\":\"" + ex.ErrorMessage + "\"}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"system error\"}";
        }
    }
    #endregion

    #region Field

    private string Proxy { get; set; }


    private string User { get; set; }

    private string Pass { get; set; }


    private string Token { get; set; }

    private string UserName { get; set; }

    private string Password { get; set; }
    #endregion

    #region Model
    class Ship
    {
        public string alt_order_id { get; set; }

        public List<Dictionary<string, object>> shipments { get; set; }
    }
    #endregion

    private string ProcessError(HttpWebResponse response)
    {
        if (response == null)
        {
            return null;
        }
        var responseStream = response.GetResponseStream();
        var reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
        string data = reader.ReadToEnd();
        Logger.Info("error msg :"+data);
        var result = JsonConvert.DeserializeObject<Error>(data);
        var errorData = result.errors;
        var errorMsg = "";
        foreach (var error in errorData)
        {
            errorMsg += error;
        }
        return errorMsg;
    }

    class Error
    {
        public string[] errors { get; set; }
    }
}