using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Auction
{
    public partial class settitle : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuctionDb adb = new AuctionDb();

            String newTitle = Convert.ToString(Request.Form["title"]);

            adb.UpdateUserTitle(Convert.ToInt32(Session["UID"]), newTitle);

            Session["TITLE"] = newTitle;
            Response.Write(newTitle);
        }
    }
}