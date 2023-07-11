using System.Text.RegularExpressions;
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

        public static string GenerateSlug(string str, bool hierarchical = true)
        {
            var slug = str.Trim().ToLower();

            string[] decomposed = new string[] { "à","á","ạ","ả","ã","â","ầ","ấ","ậ","ẩ","ẫ","ă",
                                                    "ằ","ắ","ặ","ẳ","ẵ","è","é","ẹ","ẻ","ẽ","ê","ề" ,
                                                    "ế","ệ","ể","ễ", "ì","í","ị","ỉ","ĩ", "ò","ó","ọ",
                                                    "ỏ","õ","ô","ồ","ố","ộ","ổ","ỗ","ơ" ,"ò","ớ","ợ","ở",
                                                    "õ", "ù","ú","ụ","ủ","ũ","ư","ừ","ứ","ự","ử","ữ",
                                                    "ỳ","ý","ỵ","ỷ","ỹ", "đ",
                                                    "À","À","Ạ","Ả","Ã","Â","Ầ","Ấ","Ậ","Ẩ","Ẫ","Ă" ,
                                                    "Ằ","Ắ","Ặ","Ẳ","Ẵ", "È","É","Ẹ","Ẻ","Ẽ","Ê","Ề",
                                                    "Ế","Ệ","Ể","Ễ", "Ì","Í","Ị","Ỉ","Ĩ", "Ò","Ó","Ọ","Ỏ",
                                                    "Õ","Ô","Ồ","Ố","Ộ","Ổ","Ỗ","Ơ" ,"Ờ","Ớ","Ợ","Ở","Ỡ",
                                                    "Ù","Ú","Ụ","Ủ","Ũ","Ư","Ừ","Ứ","Ự","Ử","Ữ", "Ỳ","Ý","Ỵ",
                                                    "Ỷ","Ỹ", "Đ"};
            string[] precomposed =  {  "à","á","ạ","ả","ã","â","ầ","ấ","ậ","ẩ","ẫ","ă",
                                        "ằ","ắ","ặ","ẳ","ẵ","è","é","ẹ","ẻ","ẽ","ê","ề" ,
                                        "ế","ệ","ể","ễ", "ì","í","ị","ỉ","ĩ", "ò","ó","ọ","ỏ",
                                        "õ","ô","ồ","ố","ộ","ổ","ỗ","ơ" ,"ờ","ớ","ợ","ở","ỡ", "ù",
                                        "ú","ụ","ủ","ũ","ư","ừ","ứ","ự","ử","ữ", "ỳ","ý","ỵ","ỷ","ỹ",
                                        "đ", "À","Á","Ạ","Ả","Ã","Â","Ầ","Ấ","Ậ","Ẩ","Ẫ","Ă" ,"Ằ","Ắ",
                                        "Ặ","Ẳ","Ẵ", "È","É","Ẹ","Ẻ","Ẽ","Ê","Ề","Ế","Ệ","Ể","Ễ", "Ì",
                                        "Í","Ị","Ỉ","Ĩ", "Ò","Ó","Ọ","Ỏ","Õ","Ô","Ồ","Ố","Ộ","Ổ","Ỗ",
                                        "Ơ" ,"Ờ","Ớ","Ợ","Ở","Ỡ", "Ù","Ú","Ụ","Ủ","Ũ","Ư","Ừ","Ứ","Ự",
                                        "Ử","Ữ", "Ỳ","Ý","Ỵ","Ỷ","Ỹ", "Đ"};
            string[] latin =  { "a","a","a","a","a","a","a","a","a","a","a" ,
                                "a","a","a","a","a","a", "e","e","e","e","e",
                                "e","e","e","e","e","e", "i","i","i","i","i", "o",
                                "o","o","o","o","o","o","o","o","o","o","o" ,"o","o","o","o","o",
                                "u","u","u","u","u","u","u","u","u","u","u", "y","y","y","y","y", "d",
                                "a","a","a","a","a","a","a","a","a","a","a","a" ,"a","a","a","a","a",
                                "e","e","e","e","e","e","e","e","e","e","e", "i","i","i","i","i", "o",
                                "o","o","o","o","o","o","o","o","o","o","o" ,"o","o","o","o","o", "u",
                                "u","u","u","u","u","u","u","u","u","u", "y","y","y","y","y", "d"};

            // Convert culture specific characters
            for (int i = 0; i < decomposed.Length; i++)
            {
                slug = slug.Replace(decomposed[i], latin[i]);
                slug = slug.Replace(precomposed[i], latin[i]);
            }

            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z0-9-/ ]", "").Replace("--", "-");

            // Remove whitespaces
            slug = Regex.Replace(slug.Replace("-", " "), @"\s+", " ").Replace(" ", "-");

            // Remove slash if non-hierarchical
            if (!hierarchical)
                slug = slug.Replace("/", "-");

            // Remove multiple dashes
            slug = Regex.Replace(slug, @"[-]+", "-");

            // Remove leading & trailing dashes
            if (slug.EndsWith("-"))
                slug = slug.Substring(0, slug.LastIndexOf("-"));
            if (slug.StartsWith("-"))
                slug = slug.Substring(Math.Min(slug.IndexOf("-") + 1, slug.Length));
            return slug;
        }
        public static string TimeSince(DateTime input)
        {
            TimeSpan span = DateTime.Now.Subtract(input);

            if (span.TotalDays > 365)
            {
                int years = (int)(span.TotalDays / 365);
                return String.Format("{0} năm trước", years);
            }

            if (span.TotalDays > 30)
            {
                int months = (int)(span.TotalDays / 30);
                return String.Format("{0} tháng trước", months);
            }

            if (span.TotalDays > 0)
            {
                return String.Format("{0} ngày trước", (int)span.TotalDays);
            }

            if (span.TotalHours > 0)
            {
                return String.Format("{0} giờ trước", (int)span.TotalHours);
            }

            if (span.TotalMinutes > 0)
            {
                return String.Format("{0} phút trước", (int)span.TotalMinutes);
            }

            return "Vừa xong";
        }
    }
}