using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Auction
{
    public partial class all : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void GeneratePage()
        {
            AuctionDb adb = new AuctionDb();

            List<String> users = adb.SelectAllUsers();

            foreach (String user in users)
            {
                Response.Write("<a href='default.aspx?username="+user+"'>"+user+"</a><br/>");
            }
        }
    }
}