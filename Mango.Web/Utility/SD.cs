namespace Mango.Web.Utility
{
    //SD = STATIC DETAILS
    public class SD
    {
        public static string CouponApiBase { get; set; }
        public enum ApiType
        {
            GET, 
            POST, 
            PUT, 
            DELETE
        }
    }
}
