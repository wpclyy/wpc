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
            DataSet ds = MySqlHelper.GetDataSet(str, "SELECT * FROM `productinfo` where id=7", null);

            int cat_id = 20;
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
                        uploadimg.Add(imglist[0], im);
                    }
                    //更新产品封面图片
                    Goodupdate gu = new Goodupdate(goods_id.ToString());
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
                        result = instance.sendimg<string>(new UploadParameterType { UploadStream = s, FileNameValue = "goudiw.jpg" }, img);
                        im = (JObject)JsonConvert.DeserializeObject(result);
                        if (im["error"].ToString() == "0")
                        {
                            uploadimg.Add(imglist[g], im);
                        }
                    }

                    //解析规格
                    DeserialSpec dspec = new DeserialSpec();
                    Dictionary<string, gdsdk.Attribute> speclist = dspec.Deserial(dr, instance, cat_id);

                    //解析规格对应图片
                    string specinfo = dr["skuInfos"].ToString();
                    object oo = JsonConvert.DeserializeObject(specinfo);
                    JArray spinfosm = (JArray)oo;
                    foreach (JToken ssm in spinfosm)
                    {
                        //合并相同属性值
                        Dictionary<string, string> qucongspec = new Dictionary<string, string>();
                        foreach (JToken d in ssm["attributes"].Children())
                        {
                            string attributeValue = d["attributeValue"].ToString();
                            string skuImageUrl = d["skuImageUrl"].ToString();
                            if (!qucongspec.ContainsKey(attributeValue))
                            {
                                qucongspec.Add(attributeValue, skuImageUrl);
                            }
                        }


                        //获取属性值所属ID
                        Dictionary<string, string[]> newval = new Dictionary<string, string[]>();
                        foreach (KeyValuePair<string, string> jj in qucongspec)
                        {
                            
                            foreach (KeyValuePair<string, gdsdk.Attribute> sl in speclist)
                            {
                                gdsdk.Attribute attr = sl.Value;
                                //和现有规格比较是否已存在
                                List<string> speval = new List<string>(attr.attr_values.Split(new string[] { "\r\n" }, StringSplitOptions.None));
                                //判断图片是否已在图片相册中，有则获取现有相册中图片地址，没有则上传图片获取返回图片地址
                                if (speval.Contains(jj.Key) && jj.Value.Trim() != "")
                                {
                                    
                                    GoodsAttr ga = new GoodsAttr();
                                    ga.setattr_id(attr.attr_id.ToString());
                                    ga.setgoods_id(goods_id);
                                    ga.setattr_value(jj.Key);

                                    string goods_thumb = "";
                                    string goods_img = "";
                                    if (uploadimg.ContainsKey(jj.Value))
                                    {
                                        JObject oop = uploadimg[jj.Value];
                                        goods_thumb = oop["data"]["goods_thumb"].ToString();
                                        goods_img = oop["data"]["goods_img"].ToString();
                                    }
                                    else
                                    {
                                        using (HttpClientClass hc = new HttpClientClass())
                                        {
                                            s = hc.htmlimg(alistr + jj.Value);
                                        }
                                        result = instance.sendimg<string>(new UploadParameterType { UploadStream = s, FileNameValue = "goudiw.jpg" }, img);
                                        JObject oop = (JObject)JsonConvert.DeserializeObject(result);
                                        goods_thumb = oop["data"]["goods_thumb"].ToString();
                                        goods_img = oop["data"]["goods_img"].ToString();
                                    }
                                    ga.setattr_img_flie(goods_thumb);
                                    ga.setattr_gallery_flie(goods_img);
                                    ga.setattr_checked("0");
                                    result = instance.send<string>(ga);
                                }
                            }
                        }
                    }






                    Console.WriteLine(msg);
                }

            }
        }
    }
}
