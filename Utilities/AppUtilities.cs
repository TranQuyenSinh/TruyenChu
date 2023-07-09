using Newtonsoft.Json;

namespace truyenchu.Utilities
{
    public class AppUtilities
    {
        public static string Base64EncodeObject(object obj)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static T Base64DecodeObject<T>(string base64String)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64String);
            return JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(base64EncodedBytes));
        }
    }
}