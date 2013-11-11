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

            //USER TITLE
            List<Tuple<String,String>> users = adb.SelectAllUsersAndTitles();

            foreach (Tuple<String,String> user in users)
            {
                Response.Write("<a href='default.aspx?username="+user.Item1+"'>"+user.Item2+"</a><br/>");
            }
        }
    }
}