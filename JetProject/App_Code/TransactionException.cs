using System;

/// <summary>
/// TransactionException 的摘要说明
/// </summary>
public class TransactionException:Exception
{
    public TransactionException(string errorCode, string errorMessage)
    {
        this.ErrorCode = errorCode;
        this.ErrorMessage = errorMessage;
    }

    public string ErrorCode { get; set; }

    public string ErrorMessage { get; set; }
}