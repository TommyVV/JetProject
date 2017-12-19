<%@ WebHandler Language="C#" Class="InternalService" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using NLog;

public class InternalService : IHttpHandler
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        var request = context.Request;
        var t = request["t"];
        logger.Info(request.RawUrl);
        var result = "";
        switch (t)
        {
            case "Login":
                result = UserLogin(request["userName"], request["pwd"]);
                break;
            case "PriceUpload":
                result = PriceUpload(request["skuId"], Convert.ToInt32(request["price"]));
                break;
            case "InventoryUpload":
                result = InventoryUpload(request["skuId"], Convert.ToInt32(request["quantity"]), request["nodeId"]);
                break;
            case "UploadSKU":
                result = UploadSKU(request["skuId"], request["data"]);
                break;
            case "SearchSKU":
                result = SearchSKU(request["skuId"]);
                break;
            case "SearchInventory":
                result = SearchInvenotry(request["skuId"]);
                break;
            case "QueryOrder":
                result = QueryOrder(request["status"],request["isCancel"],request["nodeId"]);
                break;
            default:
                break;

        }
        context.Response.ContentType = "application/json";
        context.Response.Write(result);
    }

    private string QueryOrder(string status, string isCancel, string nodeId)
    {
        try
        {
            var cancel = isCancel == "1";
            var url =
                $"https://merchant-api.jet.com/api/orders/{status}?isCancelled={cancel}&fulfillment_node={nodeId}";
            var result = GetData(url);
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            var orderUrls = response["order_urls"].ToString();
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"请求成功\",\"orderList\":" + orderUrls + "}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\""+ex.ErrorCode+"\",\"returnMessage\":\""+ex.ErrorMessage+"\"}";
        }
        catch (Exception e)
        {
            logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
        }
    }

    private string SearchInvenotry(string skuId)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/merchant-skus/{skuId}/inventory";
            var result=GetData(url);
            var response = JsonConvert.DeserializeObject<Dictionary<string,object>>(result);
            var nodes = response["fulfillment_nodes"].ToString();

            //var fulfillmentNodes = JsonConvert.SerializeObject(nodes);
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"请求成功\",\"fulfillmentNodes\":"+nodes+"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\""+ex.ErrorCode+"\",\"returnMessage\":\""+ex.ErrorMessage+"\"}";
        }
        catch (Exception e)
        {
            logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
        }
    }

    private string UploadSKU(string id, string data)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/merchant-skus/{id}";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"请求成功\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\""+ex.ErrorCode+"\",\"returnMessage\":\""+ex.ErrorMessage+"\"}";
        }
        catch (Exception e)
        {
            logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
        }
    }

    private string PriceUpload(string skuId, decimal price)
    {
        try
        {
            var url = string.Format("https://merchant-api.jet.com/api/merchant-skus/{0}/price", skuId);
            string data = "{\"price\":" + price + "}";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"请求成功\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\""+ex.ErrorCode+"\",\"returnMessage\":\""+ex.ErrorMessage+"\"}";
        }
        catch (Exception e)
        {
            logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
        }

    }

    private string SearchSKU(string skuId)
    {
        try
        {
            var url = $"https://merchant-api.jet.com/api/merchant-skus/{skuId}";
            var response = JsonConvert.DeserializeObject<Dictionary<string, Object>>(GetData(url));
            response.Add("returnCode", "0000");
            response.Add("returnMessage", "请求成功");
            var result = JsonConvert.SerializeObject(response);
            return result;
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\""+ex.ErrorCode+"\",\"returnMessage\":\""+ex.ErrorMessage+"\"}";
        }
        catch (Exception e)
        {
            logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
        }
    }

    private string InventoryUpload(string skuId, int qty, string nodeId)
    {
        try
        {
            var url = string.Format("https://merchant-api.jet.com/api/merchant-skus/{0}/Inventory", skuId);
            string data = "{\"fulfillment_nodes\":[{\"fulfillment_node_id\":\"" + nodeId + "\",\"quantity\":" + qty + "}]}";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"请求成功\"}";
        }
        catch (TransactionException ex)
        {
            return "{\"returnCode\":\""+ex.ErrorCode+"\",\"returnMessage\":\""+ex.ErrorMessage+"\"}";
        }
        catch (Exception e)
        {
            logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
        }

    }

    private string UserLogin(string userName, string pwd)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(pwd))
        {
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"请输入用户名密码\"}";
        }
        return userName == "tommy" || pwd == "123456"
            ? "{\"returnCode\":\"0096\",\"returnMessage\":\"请求成功\"}"
            : "{\"returnCode\":\"0096\",\"returnMessage\":\"用户名密码错误\"}";
    }

    private void GetToken(bool needRefresh=false,bool last=false)
    {

        try
        {
            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Pass))
            {
                GetConfig();
            }
            if (needRefresh||string.IsNullOrEmpty(Token))
            {
                var url = "https://merchant-api.jet.com/api/token";
                var data = "{ \"user\":\"" + User + "\",\"pass\":\"" + Pass + "\" }";
                var response = PostData(url, data, "POST", false,last);
                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("jet 请求错误");
                    //return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
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
            logger.Error(e);
            throw;
        }
    }

    private string GetData(string url,bool islast=false)
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
            logger.Info($"请求地址:{url}");
            // 设置提交的相关参数 

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Headers.Add("Authorization", "bearer " + Token);
            WebProxy proxy = new WebProxy("http://s1firewall:8080", true);
            proxy.Credentials = new NetworkCredential("tz67", "Newegg123456");
            request.Proxy = proxy;
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
            logger.Info("请求服务获得返回:" + result);
            return result;
        }
        catch (WebException e)
        {
            HttpWebResponse resp = (HttpWebResponse)e.Response;
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!islast)
                {
                    GetToken(last:true);
                }
                else
                {
                    throw new TransactionException("0091", "token获取异常");
                }
            }
            else
            {
                switch (resp.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new TransactionException("0400","请求数据不正确,或者没有找到相关信息");
                    case HttpStatusCode.NoContent:
                        throw new TransactionException("0200","未查询到数据");
                }
            }
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }



    private string PostData(string url, string data, string method, bool needToken = true,bool islast=false)
    {
        try
        {
            if (string.IsNullOrEmpty(Token)&&needToken)
            {
                GetToken();
            }
            ServicePointManager.ServerCertificateValidationCallback += delegate
            {
                return true;
            };
            logger.Info($"请求地址:{url},发送报文:{data}");
            byte[] postData = Encoding.UTF8.GetBytes(data);
            // 设置提交的相关参数 
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (needToken)
            {
                request.Headers.Add("Authorization", "bearer "+Token);
            }
            WebProxy proxy = new WebProxy("http://s1firewall:8080", true);
            proxy.Credentials = new NetworkCredential("tz67", "Newegg123456");
            request.Proxy = proxy;
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
                logger.Info("请求服务获得返回:" + result);
                return result;
            }

        }
        catch (WebException e)
        {
            HttpWebResponse resp = (HttpWebResponse)e.Response;
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!islast)
                {
                    GetToken();
                }
                else
                {
                    throw new TransactionException("0091", "token获取异常");
                }

            }
            else
            {
                switch (resp.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new TransactionException("0400","请求数据不正确,或者没有找到相关信息");
                    case HttpStatusCode.NoContent:
                        throw new TransactionException("0200","未查询到数据");
                }
            }
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void GetConfig()
    {
        var path = HttpContext.Current.Server.MapPath("../config.json");
        var config = File.ReadAllText(path, Encoding.UTF8);
        var configData = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);
        User = configData["user"];
        Pass = configData["pass"];
        Token = configData["token"];
    }

    private void WriteConfig()
    {
        var path = HttpContext.Current.Server.MapPath("../config.json");
        var dic = new Dictionary<string, string>()
        {
            {"user",User },
            {"pass",Pass },
            {"token",Token }
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(dic), Encoding.UTF8);
    }



    private string User { get; set; }

    private string Pass { get; set; }


    private string Token { get; set; }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}