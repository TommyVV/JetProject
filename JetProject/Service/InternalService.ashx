<%@ WebHandler Language="C#" Class="InternalService" %>

using System;
using System.Web;
using System.Web.SessionState;
using NLog;

public class InternalService : IHttpHandler, IRequiresSessionState
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    #region ProcessRequest
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            context.Response.ContentType = "text/plain";
            var result = "";
            var request = context.Request;
            var t = request["t"];
            var sessionId = context.Session["login"] as string;
            if (string.IsNullOrEmpty(sessionId) && t != "Login")
            {
                result = "{\"returnCode\":\"1000\",\"returnMessage\":\"please log in first  \"}";
            }
            else
            {
                logger.Info(request.RawUrl);
                var sericesHelper = new ServiceHelper();
                switch (t)
                {
                    case "Login":
                        result = sericesHelper.UserLogin(request["userName"], request["pwd"]);
                        break;
                    case "PriceUpload":
                        result = sericesHelper.PriceUpload(request["skuId"], Convert.ToInt32(request["price"]));
                        break;
                    case "InventoryUpload":
                        result = sericesHelper.InventoryUpload(request["skuId"], Convert.ToInt32(request["quantity"]), request["nodeId"]);
                        break;
                    case "UploadSKU":
                        result = sericesHelper.UploadSKU(request["skuId"], request["data"]);
                        break;
                    case "SearchSKU":
                        result = sericesHelper.SearchSKU(request["skuId"]);
                        break;
                    case "SearchInventory":
                        result = sericesHelper.SearchInvenotry(request["skuId"]);
                        break;
                    case "QueryOrder":
                        result = sericesHelper.QueryOrder(request["status"], request["isCancel"], request["nodeId"]);
                        break;
                    case "QueryOrderDetail":
                        result = sericesHelper.QueryOrderDetail(request["jetDefinedOrderId"]);
                        break;
                    case "AcknowledgeOrder":
                        result = sericesHelper.AcknowledgeOrder(request["data"], request["jetDefinedOrderId"]);
                        break;
                    case "ShipOrder":
                        result = sericesHelper.ShipOrder(request["data"], request["jetDefinedOrderId"]);
                        break;
                    case "QueryReturnOrder":
                        result = sericesHelper.QueryReturnOrder(request["status"]);
                        break;
                    case "QueryReturnOrderDetail":
                        result = sericesHelper.QueryReturnOrderDetail(request["jetDefinedOrderId"]);
                        break;
                    case "CompleteReturn":
                        result = sericesHelper.CompleteReturn(request["data"], request["jetDefinedOrderId"]);
                        break;
                    case "shippingException":
                        result = sericesHelper.ShippingException(request["method"], request["nodeId"], request["shippingType"]
                            ,request["overrideType"],request["skuId"],request["amount"]);
                        break;
                    default:
                        break;

                }
            }
            context.Response.ContentType = "application/json";
            context.Response.Write(result);
        }
        catch (Exception e)
        {
            logger.Error(e);
        }



    }
    #endregion



    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}