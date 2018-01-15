using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using NLog;
using RestSharp;

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
            HttpRquest(url, data, "PUT");
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
            HttpRquest(url, data, "PUT");
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
            HttpRquest(url, data, "PUT");
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
            var result = HttpRquest(url,null,"GET");
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
            var result = HttpRquest(url,null,"GET");
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
            var result = HttpRquest(url,null,"GET");
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
            HttpRquest(url, data, "PUT");
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
            HttpRquest(url, data, "PUT");
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
            var response = JsonConvert.DeserializeObject<Dictionary<string, Object>>(HttpRquest(url,null,"GET"));
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
            HttpRquest(url, data, "PUT");
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
                var response = HttpRquest(url, data, "POST", false, last);
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
            var result = HttpRquest(url,null,"GET");
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
            var result = HttpRquest(url,null,"GET");
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

    #region  shipping exception

    public string ShippingException(string method,string nodeId,string shippingType,string overrideType,string jetDefinedOrderId,string amount)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/merchant-skus/{jetDefinedOrderId}/shippingexception";
            string data;
            if (shippingType == "restricted")
            {
                data = "{\"fulfillment_nodes\":[{\"fulfillment_node_id\":\"" + nodeId +
                       "\",\"shipping_exceptions\":[{\"shipping_method\":\"" + method +
                       "\",\"shipping_exception_type\":\"" + shippingType + "\"}]}]}";
            }
            else
            {
                data = "{\"fulfillment_nodes\":[{\"fulfillment_node_id\":\"" + nodeId +
                       "\",\"shipping_exceptions\":[{\"shipping_method\":\"" + method +
                       "\",\"shipping_exception_type\":\"" + shippingType + "\",\"override_type\":\"" + overrideType +
                       "\",\"shipping_charge_amount\":" + amount + "}]}]}";
            }
            
            var result = HttpRquest(url,data,"PUT");
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

    private string HttpRquest(string url, string data, string method, bool needToken = true, bool islast = false)
    {
        var clinet=new RestClient(url);
        var request=new RestRequest(url);
        GetConfig();
        if (needToken)
        {
            request.AddHeader("Authorization", "bearer " + Token);
        }
       
        if (!string.IsNullOrEmpty(data))
        {
            
            request.AddParameter("application/json; charset=utf-8", data, ParameterType.RequestBody);
          
            request.RequestFormat = DataFormat.Json;
        }
        Logger.Info($"请求地址:{url},发送报文:{data}");
        IRestResponse response;
        switch (method)
        {
            case "PUT":
                response =clinet.Put(request);
                break;
            case "GET":
                response = clinet.Get(request);
                break;
            case "POST":
                response = clinet.Post(request);
                break;
                default:
                    response = clinet.Put(request);
                    break;
        }
        if (response.IsSuccessful)
        {
            return response.Content;
        }
        Logger.Info(response.ErrorMessage);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (!islast)
            {
                GetToken(true,true);
                return HttpRquest(url, data, method, needToken, true);
            }
        }

        throw new TransactionException("0040", ProcessError(response.Content));
    }

    private string ProcessError(string data)
    {
        try
        {
            Logger.Info("error msg :" + data);
            if (string.IsNullOrEmpty(data))
            {
                return "jet request error";
            }
            var result = JsonConvert.DeserializeObject<Error>(data);
            var errorData = result.errors;
            var errorMsg = "";
            foreach (var error in errorData)
            {
                errorMsg += error;
            }
            return errorMsg;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "jet request error";
        }
    }

    class Error
    {
        public string[] errors { get; set; }
    }
}