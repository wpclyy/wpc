using com.mysql;
using System.Data;
using dscapi.gdsdk;
using com.goudiw;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System;

namespace dscapi
{
    class MainClass
    {

        public static void Main(string[] args)
        {
            const string alistr = "https://cbu01.alicdn.com/";

            string str = "server=192.168.2.100;user id=Fany;password=wang198912;database=GCollection";
            DataSet ds = MySqlHelper.GetDataSet(str, "SELECT * FROM `productinfo` where id>8045 and id<8050", null);

            int cat_id = 5;
            int category_id = 583;
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];
                GoodRequest gr = new GoodRequest(alistr);
                gr.Submit(dr, cat_id, category_id);
            }
        }
    }
}
