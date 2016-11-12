namespace Reusable
{
    public class CommonResponse
    {
        public bool ErrorThrown { get; set; }
        public string ResponseDescription { get; set; }
        public object Result { get; set; }

        public CommonResponse()
        {
            ErrorThrown = false;
            ResponseDescription = "";
            Result = null;
        }

        public CommonResponse Error(string sError, object result)
        {
            ErrorThrown = true;
            ResponseDescription = sError;
            Result = result;
            return this;
        }

        public CommonResponse Error(string sError)
        {
            ErrorThrown = true;
            ResponseDescription = sError;
            return this;
        }

        public CommonResponse Success(object oResult, string sMessage = "OK")
        {
            ErrorThrown = false;
            Result = oResult;
            ResponseDescription = sMessage;
            return this;
        }

        public CommonResponse Success(string sMessage = "OK")
        {
            ErrorThrown = false;
            ResponseDescription = sMessage;
            return this;
        }
    }

    class ValidationResult
    {
        public long EntityId { get; set; }
        public string EntityKind { get; set; }
        public string FriendlyIdentifier { get; set; }
        public string Description { get; set; }
    }
}
