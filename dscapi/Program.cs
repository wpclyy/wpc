using System;
using System.IO;
using System.Net;
using System.Text;
using goudiw.sdk.client.policy;
using com.mysql;
using dscapi.dsc.goods;
using GDHttp.dsc.goods.attr.list;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using HtmlAgilityPack;
using System.Collections.Generic;
using com.goudiw;

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
            DataSet ds = MySqlHelper.GetDataSet(str, "SELECT * FROM `productinfo` where id=29", null);

            int cat_id = 21;
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];


                //商品相册
                string[] imglist = dr["imagelist"].ToString().Split(',');
                //图片上传后保存图片地址和所对应的ID,以便下面做对比
                Dictionary<string, JObject> uploadimg = new Dictionary<string, JObject>();
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
                gd.setgoods_type(cat_id);

                //解析阿里巴巴商品详情信息，获取其中图片
                DeHtml dh = new DeHtml();
                HtmlDocument dc = dh.htmlformat(dr["desction"].ToString());
                HtmlNode rootnode = dc.DocumentNode;    
                HtmlNodeCollection aa = rootnode.SelectNodes("//img");
                string imagesrc = "";
                foreach (HtmlNode imgs in aa)
                {
                    string src = imgs.Attributes["src"].Value;
                    imagesrc += " <p><br/><img src=\"" + src + "\"/></p>";
                }



                gd.setgoods_desc(imagesrc);
                string obj = instance.send<string>(gd);
                JObject sm = (JObject)JsonConvert.DeserializeObject(obj);

              

                //商品规格
                string spec = dr["skumodelstr"].ToString();
                if (sm["result"].ToString() == "success")
                {
                    Console.WriteLine("插入商品基本信息");
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
                    if (im["error"].ToString() == "0")
                    {
                        Console.WriteLine("上传封面图片\r\n");
                        uploadimg.Add(imglist[0], im);
                    }
                    //更新产品封面图片
                    Goodupdate gu = new Goodupdate(goods_id.ToString());
                    gu.setgoods_thumb(im["data"]["goods_thumb"].ToString());
                    gu.setgoods_img(im["data"]["goods_img"].ToString());
                    gu.setoriginal_img(im["data"]["original_img"].ToString());
                    string gdobj = instance.send<string>(gu);
                    JObject upgd = (JObject)JsonConvert.DeserializeObject(gdobj);
                    Console.WriteLine("更新封面图片\r\n");
                    //string msg = dr["productTitle"].ToString() + "------" + upgd["msg"].ToString() + "\r\n";
                    for (int g = 1; g < imglist.Length;g++)
                    {
                        s = null;
                        using (HttpClientClass hc = new HttpClientClass())
                        {
                            s = hc.htmlimg(alistr + imglist[g]);
                        }
                        result = instance.sendimg<string>(new UploadParameterType { UploadStream = s, FileNameValue = "goudiw.jpg" }, img);
                        im = (JObject)JsonConvert.DeserializeObject(result);
                        if (im["error"].ToString() == "0")
                        {
                            uploadimg.Add(imglist[g], im);
                        }
                    }
                    Console.WriteLine("上传相册图片\r\n");
                    //解析规格
                    DeserialSpec dspec = new DeserialSpec(instance,alistr);
                    Dictionary<string, gdsdk.Attribute> speclist = dspec.Deserial(dr, instance, cat_id);
                    Console.WriteLine("解析规格数据\r\n");
                    JToken goods_attr = dspec.QuChong(dr,speclist,goods_id,uploadimg);

                    foreach(JToken j in goods_attr.Children())
                    {
                        string minRetailPrice = j["minRetailPrice"].ToString();
                        string price = j["price"].ToString();
                        string proxyPrice = j["proxyPrice"].ToString();
                        string retailPrice = j["retailPrice"].ToString();
                        List<string> attridlist = new List<string>();
                        foreach(JToken t in j["attributeModelList"].Children())
                        {
                            attridlist.Add(t["attr_id"].ToString());
                        }
                        string[] attids = attridlist.ToArray();
                        string attridstr = string.Join("|", attids);

                        Products pd = new Products();
                        pd.setgoods_id(goods_id);
                        pd.setgoods_attr(attridstr);
                        pd.setproduct_price(price);
                        pd.setproduct_market_price(retailPrice);
                        pd.setproduct_number(1000);
                        result = instance.send<string>(pd);
                        im = (JObject)JsonConvert.DeserializeObject(result);

                    }


                    Console.WriteLine("规格值去重并提交\r\n");

                    Console.WriteLine("----------------------" + dr["productTitle"].ToString() + "-----------------------");
                }

            }
        }
    }
}
