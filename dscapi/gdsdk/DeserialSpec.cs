
using System.Collections.Generic;
using System.Data;
using goudiw.sdk.client;
using dscapi.dsc.goods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace com.goudiw
{
    public class DeserialSpec
    {
        public DeserialSpec()
        {
        }

        /// <summary>
        /// 解析阿里巴巴数据中的规格信息，对比现有规格，有则增加
        /// </summary>
        /// <returns>The deserial.</returns>
        /// <param name="dr">阿里巴巴数据集</param>
        /// <param name="instance">实例化api</param>
        /// <param name="cat_id">商品类型</param>
        public Dictionary<string, gdsdk.Attribute>  Deserial(DataRow dr,SyncAPIClient instance,int cat_id)
        {
            string spec = dr["skumodelstr"].ToString();
            JObject spsm = (JObject)JsonConvert.DeserializeObject(spec);
            JToken skuList = spsm["skuList"];
            //解析出来的规格和值
            Dictionary<string, List<string>> specdic = new Dictionary<string, List<string>>();
            foreach (JToken j in skuList.Children())
            {
                JToken jc = j["attributeModelList"];
                foreach (JToken jcc in jc)
                {
                    string key = "";
                    string val = "";
                    if (jcc["sourceAttributeValue"] != null && jcc["sourceAttributeName"].ToString() != "")
                    {
                        key = jcc["sourceAttributeName"].ToString();
                        val = jcc["sourceAttributeValue"].ToString();
                    }
                    else if (jcc["targetAttributeValue"] != null && jcc["targetAttributeValue"].ToString() != "")
                    {
                        key = jcc["targetAttributeName"].ToString();
                        val = jcc["targetAttributeValue"].ToString();
                    }

                    if (specdic.ContainsKey(key))
                    {
                        if (!specdic[key].Contains(val))
                        {
                            specdic[key].Add(val);
                        }
                    }
                    else
                    {
                        List<string> lt = new List<string>();
                        lt.Add(val);
                        specdic.Add(key, lt);
                    }
                }
            }
           

            AttributeList al = new AttributeList();
            al.setcat_id(cat_id);
            string alobj = instance.send<string>(al);
            JObject alsm = (JObject)JsonConvert.DeserializeObject(alobj);
            //现有规格属性
            JToken list = alsm["info"]["list"];

            //相同规格属性值对比，现有值中没有则增加
            Dictionary<string,gdsdk.Attribute> reloadspec = new Dictionary<string, gdsdk.Attribute>();
            foreach(KeyValuePair<string,List<string>> att in specdic)
            {
                bool flag = true;
                foreach(JToken attr in list.Children())
                {
                    gdsdk.Attribute ae = new gdsdk.Attribute();
                    List<string> att_val = att.Value;
                    //属性存在
                    if (att.Key == attr["attr_name"].ToString())
                    {
                        //现有规格值中是以字符串形式保存规格值，将其转换成数组
                        List<string> attr_values = new List<string>(attr["attr_values"].ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None));

                        ae.attr_id = int.Parse(attr["attr_id"].ToString());
                        ae.cat_id = int.Parse(attr["cat_id"].ToString());
                        ae.attr_name = attr["attr_name"].ToString();
                        ae.attr_cat_type = int.Parse(attr["attr_cat_type"].ToString());
                        ae.attr_input_type = int.Parse(attr["attr_input_type"].ToString());
                        ae.attr_type = int.Parse(attr["attr_type"].ToString());
                        ae.attr_values = attr["attr_values"].ToString();
                        ae.attr_index = int.Parse(attr["attr_index"].ToString());
                        ae.sort_order = int.Parse(attr["sort_order"].ToString());
                        ae.is_linked = int.Parse(attr["is_linked"].ToString());
                        foreach (string v in att_val)
                        {
                            if (!attr_values.Contains(v))
                            {
                                if (ae.attr_values.Trim() == "")
                                {
                                    ae.attr_values = v;
                                }
                                else
                                {
                                    ae.attr_values += "\r\n" + v;
                                }
                            }
                        }

                        dscapi.dsc.goods.Attributeupdate attu = new Attributeupdate(ae.attr_id.ToString());
                        attu.setcat_id(ae.cat_id);
                        attu.setattr_name(ae.attr_name);
                        attu.setattr_cat_type(ae.attr_cat_type);
                        attu.setattr_input_type(ae.attr_input_type);
                        attu.setattr_type(ae.attr_type);
                        attu.setattr_values(ae.attr_values);
                        string result = instance.send<string>(attu);
                        JObject re = (JObject)JsonConvert.DeserializeObject(result);
                        reloadspec.Add(att.Key, ae);
                        flag = false;
                    }
                }
                //属性不存在

                if (flag)
                {
                    dscapi.dsc.goods.Attribute at = new dscapi.dsc.goods.Attribute();
                    at.setcat_id(cat_id);
                    at.setattr_name(att.Key);
                    at.setattr_cat_type(0);
                    at.setattr_input_type(1);
                    at.setattr_type(1);
                    string sval = "";
                    foreach (string v in att.Value)
                    {
                        if (sval.Trim() == "")
                        {
                            sval = v;
                        }
                        else
                        {
                            sval += "\r\n" + v;
                        }
                    }
                    at.setattr_values(sval);
                    string result = instance.send<string>(at);
                    JObject obj = (JObject)JsonConvert.DeserializeObject(result);
                    gdsdk.Attribute ae = new gdsdk.Attribute();
                    ae.attr_id = int.Parse(obj["id"].ToString());
                    ae.cat_id = cat_id;
                    ae.attr_name = att.Key;
                    ae.attr_cat_type = 0;
                    ae.attr_input_type = 1;
                    ae.attr_type = 1;
                    ae.attr_values = sval;
                    ae.attr_index = 0;
                    ae.sort_order = 0;
                    ae.is_linked = 0;
                    reloadspec.Add(att.Key, ae);
                }
            }
            return reloadspec;
        }
    }
}
