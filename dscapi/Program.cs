using System;
using System.IO;
using System.Net;
using System.Text;
using com.alibaba.openapi.client;
using com.alibaba.openapi.client.policy;
using com.mysql;
using dscapi.dsc.goods;
using GDHttp.dsc.goods.attr.list;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using com.down;
using dscapi.sdk;
using HtmlAgilityPack;

namespace dscapi
{
    class MainClass
    {
        
        public static void Main(string[] args)
        {
            SyncAPIClient instance = null;

            ClientPolicy clientPolicy = new ClientPolicy();
            clientPolicy.AppKey = "4291F75C-A7B9-4DB3-AADC-234490550B14";
            instance = new SyncAPIClient(clientPolicy);

            string alistr = "https://cbu01.alicdn.com/";

            string str = "server=192.168.2.100;user id=Fany;password=wang198912;database=GCollection";
            DataSet ds = MySqlHelper.GetDataSet(str, "SELECT * FROM `productinfo` where id=9", null);


            for (int i = 0; i < ds.Tables[0].Rows.Count;i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];
                //插入商品
                Goods gd = new Goods();
                gd.setcat_id(161);
                gd.setuser_id(0);
                gd.setgoods_name(dr["productTitle"].ToString());
                gd.setbrand_id(0);
                gd.setgoods_sn(dr["productID"].ToString());
                gd.setgoods_number(326);
                gd.setmarket_price(1333.2F);
                gd.setshop_price(1111.0F);
                gd.setreview_status(3);
                gd.setis_alone_sale(1);
                gd.setis_on_sale(1);

                DeHtml dh = new DeHtml();
                HtmlDocument dc = dh.htmlformat(dr["desction"].ToString());
                HtmlNode rootnode = dc.DocumentNode;    
                HtmlNodeCollection aa = rootnode.SelectNodes("//img");
                string imagesrc = "";
                foreach (HtmlNode imgs in aa)
                {
                    string src = imgs.Attributes["src"].Value;
                    imagesrc += " <img src=\"" + src + "\"/>";
                }



                gd.setgoods_desc(imagesrc);
                string obj = instance.send<string>(gd);
                JObject sm = (JObject)JsonConvert.DeserializeObject(obj);

                string[] imglist = dr["imagelist"].ToString().Split(',');

                if (sm["result"].ToString() == "success")
                {
                    //插入图片
                    int goods_id = int.Parse(sm["id"].ToString());
                    Image img = new Image();
                    img.setid(goods_id);

                    Stream s = null;
                    using (HttpClientClass hc = new HttpClientClass())
                    {
                        s = hc.htmlimg(alistr+imglist[0]);
                    }
                    string result = instance.sendimg<string>(new UploadParameterType { UploadStream = s, FileNameValue = "goudiw.jpg" }, img);
                    JObject im = (JObject)JsonConvert.DeserializeObject(result);
                    //更新产品封面图片
                    Goodupdate gu = new Goodupdate();
                    gu.setgoods_id(goods_id);
                    gu.setgoods_thumb(im["data"]["goods_thumb"].ToString());
                    gu.setgoods_img(im["data"]["goods_img"].ToString());
                    gu.setoriginal_img(im["data"]["original_img"].ToString());
                    string gdobj = instance.send<string>(gu);
                    JObject upgd = (JObject)JsonConvert.DeserializeObject(gdobj);
                    string msg = dr["productTitle"].ToString() + "------" + upgd["msg"].ToString() + "\r\n";
                    for (int g = 1; g < imglist.Length;g++)
                    {
                        s = null;
                        using (HttpClientClass hc = new HttpClientClass())
                        {
                            s = hc.htmlimg(alistr + imglist[g]);
                        }
                        instance.sendimg<string>(new UploadParameterType { UploadStream = s, FileNameValue = "goudiw.jpg" }, img);
                    }
                    Console.WriteLine(msg);
                }

            }
        }
    }
}
