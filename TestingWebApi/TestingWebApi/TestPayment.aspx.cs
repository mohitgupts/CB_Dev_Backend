using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using TestingWebApi.DAL;

namespace TestingWebApi
{
    public partial class TestPayment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {            
            try
            {
                Request.InputStream.Position = 0;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(Context.Request.InputStream))
                {
                    var jsonResult = sr.ReadToEnd();
                    PaymentHandler test = new PaymentHandler();
                    test.RequestLog("TestPayment", jsonResult);
                }
            }
            catch (Exception ex) {
                PaymentHandler test = new PaymentHandler();
                test.RequestLog("TestPayment", ex.Message);
            }
            //---------------------------- 
            Context.Response.StatusCode = 200;
            Context.Response.Status = "200 (OK)";
            Context.Response.End();
        }
    }
}