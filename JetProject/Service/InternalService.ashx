<%@ WebHandler Language="C#" Class="InternalService" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Channels;
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
            case "getToken":
                result = GetToken();
                break;
            case "Login":
                result = UserLogin(request["userName"], request["pwd"]);
                break;
            case "PriceUpload":
                result = PriceUpload(request["skuId"], Convert.ToInt32(request["price"]));
                break;
            case "InventoryUpload":
                result = PriceUpload(request["skuId"], Convert.ToInt32(request["price"]));
                break;
            case "SearchSKU":
                result = SearchSKU(request["skuId"]);
                break;
            default:
                break;

        }
        context.Response.ContentType = "application/json";
        context.Response.Write(result);
    }

    private string UploadSKU(string id, string data)
    {
        var url = $"https://merchant-api.jet.com/api/merchant-skus/{id}";
        PostData(url, data, "PUT");
        return null;
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
            var response=JsonConvert.DeserializeObject<Dictionary<string,Object>>(GetData(url));
                response.Add("returnCode","0000");
            response.Add("returnMessage","请求成功");
            var result = JsonConvert.SerializeObject(response);
            return result;
        }
        catch (Exception e)
        {
            logger.Error(e);
            return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
        }
    }

    private string InventoryUpload(string skuId, decimal price)
    {
        try
        {
            var url = string.Format("https://merchant-api.jet.com/api/merchant-skus/{0}/Inventory", skuId);
            string data = "{\"price\":" + price + "}";
            PostData(url, data, "PUT");
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"请求成功\"}";
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

    private string GetToken()
    {

        try
        {
            var user = "9BD7AA25D356387250E97564D2B91B4A29E7B677";
            var pass = "G/93glgnE+S2tRhvyE9/RavsBZMR385jqRl+o+172T3T";
            var url = "https://merchant-api.jet.com/api/token";
            var data = "{ \"user\":\"" + user + "\",\"pass\":\"" + pass + "\" }";
            var response = PostData(url, data, "POST", false);
            if (string.IsNullOrEmpty(response))
            {
                return "{\"returnCode\":\"0096\",\"returnMessage\":\"jet 请求错误\"}";
            }
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            string token = "";
            jsonData.TryGetValue("id_token", out token);
            return "{\"returnCode\":\"0000\",\"returnMessage\":\"请求成功\",\"token\":\"" + token + "\"}";
        }
        catch (Exception e)
        {
            logger.Error(e);
            throw;
        }

    }

    private string GetData(string url)
    {
        try
        {
            ServicePointManager.ServerCertificateValidationCallback += delegate
            {
                return true;
            };
            logger.Info("asdsdsda");
            logger.Info($"请求地址:{url},\\r\\n");
            // 设置提交的相关参数 
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Headers.Add("Authorization", "bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IldscUZYb2E1WkxSQ0hldmxwa1BHOVdHS0JrMCJ9.eyJpc19zYW5kYm94X3VpZCI6InRydWUiLCJtZXJjaGFudF9pZCI6ImM0OTA4MjFhM2VlZDRjNzRiMTUzNTVhYTUwM2Q4ZGQ3IiwicGFydG5lcl90eXBlIjoiTWVyY2hhbnQiLCJzY29wZSI6Imlyb25tYW4tYXBpIiwiaXNzIjoiamV0LmNvbSIsImV4cCI6MTUxMzE2NjI0NCwibmJmIjoxNTEzMTMwMjQ0fQ.JlbRZaqEJzp9ixbGF0hkm5f-H_wAnHLBLgrGeJRM0BbIj3X5nWOpbnppqqiFgFGBkDG4A_K68-Tyd92LAyxRIzOIqmV_8bSkSq2AkZtSKU5FqZqIuYwGqS-3KLaIa08AxOv7afphlIZ0D9Rp7_SEWvispjO_0HRbslaYQ3yscJRHLkdOUrdIUyicAG5hQZRwywbCh_gWBAnBTnO0nE2Mo27_scArG8d_Dc4KXrl1iuGpoMh4k-D0w6BLXQoEVjIGZblB34ae8KDyjZPKnJsiQ11LVpzL6PloUiYjvhdLNdLOhQeY-k1IdSgR-vcscjYUUA44d3qxlik7kFiL_1O8pC250tKU1yOS_K24YAqfombKOmQNNzwmJ6lXD5nDH5-_OxmxUW_ZS7jgjtIjhlNFnflkg6GhpMghtXFzNVz6ilrMGRMDN5j4HJ-w1YIDrsX1jqlIQq8Dxp7pywwgV6ofpRuTJ34ER1GhR8tzfisFX5EZK7w_vc0SX52BUro4KvwHMrfAwGc5oPLgbzwYh669JNGO9nwIViyMqTymaOJx8qTA0yUn2AH5SclKibtvRfqeOiSCvis9R6qZ3i_qo1wL030MOc9GeCbcv9K8x2REDur1W2rq6zzISBrjVDquXZMXtNB0vhI1dzeMFYX3VCwAvsDQT8y1YZl34LiLdMhDeaY");
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private string PostData(string url, string data, string method, bool needToken = true)
    {
        try
        {
            ServicePointManager.ServerCertificateValidationCallback += delegate
            {
                return true;
            };
            logger.Info("asdsdsda");
            logger.Info($"请求地址:{url},\\r\\n 发送报文:{data}");
            byte[] postData = Encoding.UTF8.GetBytes(data);
            // 设置提交的相关参数 
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (needToken)
            {
                request.Headers.Add("Authorization", "bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IldscUZYb2E1WkxSQ0hldmxwa1BHOVdHS0JrMCJ9.eyJpc19zYW5kYm94X3VpZCI6InRydWUiLCJtZXJjaGFudF9pZCI6ImM0OTA4MjFhM2VlZDRjNzRiMTUzNTVhYTUwM2Q4ZGQ3IiwicGFydG5lcl90eXBlIjoiTWVyY2hhbnQiLCJzY29wZSI6Imlyb25tYW4tYXBpIiwiaXNzIjoiamV0LmNvbSIsImV4cCI6MTUxMzE2NjI0NCwibmJmIjoxNTEzMTMwMjQ0fQ.JlbRZaqEJzp9ixbGF0hkm5f-H_wAnHLBLgrGeJRM0BbIj3X5nWOpbnppqqiFgFGBkDG4A_K68-Tyd92LAyxRIzOIqmV_8bSkSq2AkZtSKU5FqZqIuYwGqS-3KLaIa08AxOv7afphlIZ0D9Rp7_SEWvispjO_0HRbslaYQ3yscJRHLkdOUrdIUyicAG5hQZRwywbCh_gWBAnBTnO0nE2Mo27_scArG8d_Dc4KXrl1iuGpoMh4k-D0w6BLXQoEVjIGZblB34ae8KDyjZPKnJsiQ11LVpzL6PloUiYjvhdLNdLOhQeY-k1IdSgR-vcscjYUUA44d3qxlik7kFiL_1O8pC250tKU1yOS_K24YAqfombKOmQNNzwmJ6lXD5nDH5-_OxmxUW_ZS7jgjtIjhlNFnflkg6GhpMghtXFzNVz6ilrMGRMDN5j4HJ-w1YIDrsX1jqlIQq8Dxp7pywwgV6ofpRuTJ34ER1GhR8tzfisFX5EZK7w_vc0SX52BUro4KvwHMrfAwGc5oPLgbzwYh669JNGO9nwIViyMqTymaOJx8qTA0yUn2AH5SclKibtvRfqeOiSCvis9R6qZ3i_qo1wL030MOc9GeCbcv9K8x2REDur1W2rq6zzISBrjVDquXZMXtNB0vhI1dzeMFYX3VCwAvsDQT8y1YZl34LiLdMhDeaY");
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}