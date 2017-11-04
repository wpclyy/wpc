using System;
using System.IO;
using System.Net;
using System.Text;
using com.alibaba.openapi.client;
using com.alibaba.openapi.client.policy;
using dscapi.dsc.goods;
using GDHttp.dsc.goods.attr.list;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dscapi
{
    class MainClass
    {
        
        public static void Main(string[] args)
        {
            SyncAPIClient instance = null;
            Goods gd = new Goods();
            //gd.setgoods_id(1000);
            gd.setcat_id(161);
            gd.setuser_id(0);
            gd.setgoods_name("华殿卫浴 台上盆椭圆形陶瓷卫生间面盆陶瓷盆洗脸盆台盆艺术洗手盆洗面台 410*330*150mm 送货上门加安装=省心");
            gd.setbrand_id(0);
            gd.setgoods_sn("ECS000594");
            gd.setgoods_number(326);
            gd.setmarket_price(1333.2F);
            gd.setshop_price (1111.0F);
            gd.setreview_status(3);

            ClientPolicy clientPolicy = new ClientPolicy();
            clientPolicy.AppKey = "4291F75C-A7B9-4DB3-AADC-234490550B14";
            instance = new SyncAPIClient(clientPolicy);

            string obj = instance.send<string>(gd);
            JObject sm = (JObject)JsonConvert.DeserializeObject(obj);
            Console.WriteLine(sm["msg"]);
        }

        private string HttpPost(string Url, string postDataStr,CookieContainer cookie)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
            request.CookieContainer = cookie;
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
            myStreamWriter.Write(postDataStr);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            response.Cookies = cookie.GetCookies(response.ResponseUri);
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
    }
}
