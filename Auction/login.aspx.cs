using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Auction
{
    public partial class login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Login_Click(object sender, EventArgs e)
        {            
            try
            {
                AuctionDb adb = new AuctionDb();
                
                //get UID TITLE for user
                Tuple<int, String, String> uidandtitle = adb.Authenticate(UserName.Text, Password.Text);

                //UID returned will be 0 (and TITLE will be "") if credentials are wrong
                if (uidandtitle.Item1 == 0)
                {
                    throw new Exception("Authentication Failure.");
                }

                Session["UID"] = uidandtitle.Item1;
                Session["TITLE"] = uidandtitle.Item2;
                Session["USERNAME"] = uidandtitle.Item3;
                Response.Redirect("admin.aspx");                

            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
            }
        }
    }
}