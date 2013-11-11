using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data.SqlClient; //SqlDataReader

namespace Auction
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void GeneratePage()
        {
            try
            {

                //setup a database object either way
                AuctionDb adb = new AuctionDb();

                if (Request["username"] == null || Request["username"] == "")
                {
                    Response.Write("User Name not specified. Your link is broken.");
                    return;
                }

                //USERID TITLE USERNAME
                Tuple<int, String, String> userinfo = adb.UserExists(Request["username"]);
                if (userinfo.Item1 == 0)
                {
                    Response.Write("User Name does not exist.");
                    return;
                }

                Session["USERID"] = userinfo.Item1;
                Session["TITLE"] = userinfo.Item2;
                Session["USERNAME"] = userinfo.Item3;
                
                Response.Write("<div class='description'>");
                Response.Write(Convert.ToString(Session["TITLE"]));                
                Response.Write("</div>");              
                Response.Write("<hr />");
                
                //check if request is for a specific ?item=
                if (Request["item"] == null || Request["item"] == "")
                {
                    //if not, select distinct items and display them as links
                    List<String> listOfItems = adb.SelectItems(Convert.ToInt32(Session["USERID"]));
                    if (listOfItems.Count<=0)
                    {
                        Response.Write("There are no items to display");
                        return;
                    }
                    Response.Write("<center><table><tr>");
                    foreach(String item in listOfItems)
                    {
                        String itemImageFile;
                        
                        //don't do this for records that dont have their item field set yet
                        if (item != "")
                        {
                            //see if the item has an image, and use it if so (if not, use default image record.jpg
                            itemImageFile = adb.SelectImageForItem(item.Replace("'", "''").Replace('"', '\"'), Convert.ToInt32(Session["USERID"]));
                            itemImageFile = String.IsNullOrEmpty(itemImageFile) ? "record.jpg" : "images/" + itemImageFile;

                            //url encode the item in case there are special characters
                            //use \" in case there are single quotes in the item name
                            Response.Write("<td class='admin'>");
                            Response.Write("<a class=\"filelink\" href=\"default.aspx?username=" + Server.UrlEncode(Convert.ToString(Session["USERNAME"])) + "&item=" + Server.UrlEncode(item) + "\"><img class='itemimage' src='" + itemImageFile + "' alt='" + item + "' /></a>");
                            Response.Write("<br/>");
                            Response.Write("<a class=\"filelink\" href=\"default.aspx?username=" + Server.UrlEncode(Convert.ToString(Session["USERNAME"])) + "&item=" + Server.UrlEncode(item) + "\">" + item + "</a>");
                            Response.Write("</td>");

                        }
                        //create an item group for those that have none
                        else
                        {
                            itemImageFile = "record.jpg";

                            Response.Write("<td class='admin'>");
                            Response.Write("<a class=\"filelink\" href=\"default.aspx?username=" + Server.UrlEncode(Convert.ToString(Session["USERNAME"])) + "&item=--Ungrouped--\"><img class='itemimage' src='" + itemImageFile + "' alt='--Ungrouped--' /></a>");
                            Response.Write("<br/>");
                            Response.Write("<a class=\"filelink\" href=\"default.aspx?username=" + Server.UrlEncode(Convert.ToString(Session["USERNAME"])) + "&item=--Ungrouped--\">--Ungrouped--</a>");
                            Response.Write("</td>");
                        }

                        //write out in rows of 3
                        if (((listOfItems.IndexOf(item)+1) % 3) == 0)
                        {
                            Response.Write("</tr><tr>");
                        }
                    }
                    Response.Write("</tr></table></center");
                }
                else
                {
                    //TITE, FILENAME
                    //url decode the item name
                    String item = Server.UrlDecode((String)Request["item"]);
                    Response.Write("<a class=\"itemlink\" href=\"default.aspx?username=" + Server.UrlEncode(Convert.ToString(Session["USERNAME"])) + "\">Home</a>");
                    Response.Write("<center>");
                    //display item at top of page
                    Response.Write("<span class='itemlink'>" + item + "</span>");
                    //escape single quotes
                    item = item.Replace("'", "''").Replace('"', '\"');
                    //get title records for item and display FILENAME and TITLE
                    List<Tuple<String, String>> fileswithtitles = adb.SelectFilenameTitleForItem(item,Convert.ToInt32(Session["USERID"]));
                    if (fileswithtitles.Count<=0)
                    {
                        Response.Write("There are no records for " + Request["item"]);
                        return;
                    }

                    //display all files
                    Response.Write("<table>");
                    foreach (Tuple<String, String> filewithtitle in fileswithtitles)
                    {

                        //display mp3 as link, and image as an image
                        if (filewithtitle.Item1.ToLower().Contains(".mp3"))
                        {
                            Response.Write("<tr><td class='adminnowidth'>");
                            Response.Write("<a class='filelink' href='images/" + filewithtitle.Item1 + "'>" + (filewithtitle.Item2 == "" ? "--Untitled--" : filewithtitle.Item2) + "</a>");
                            Response.Write("</td><td class='play'>");
                            
                            /*
                            String strFlash;
                            strFlash = "<embed type=\"application/x-shockwave-flash\" ";
                            strFlash += "src=\"playSoundParameterWithProgress.swf?titleUrl=images/" + filewithtitle.Item1 + "\" ";
                            strFlash += "width=\"300\" ";
                            strFlash += "id=\"movie_player\" ";
                            strFlash += "height=\"20\" ";
                            strFlash += "allowscriptaccess=\"always\" ";
                            strFlash += "allowfullscreen=\"true\" ";
                            strFlash += "bgcolor=\"#000000\">";
                            strFlash += "</embed>";
                            Response.Write(strFlash);
                             */
                             
                            Response.Write("<audio controls preload='none'><source src='images/"+filewithtitle.Item1+"' type='audio/mpeg'></audio>");
                            Response.Write("</td></tr>");

                        }
                        else if (filewithtitle.Item1.ToLower().Contains(".flv") || filewithtitle.Item1.ToLower().Contains(".f4v") || filewithtitle.Item1.ToLower().Contains(".mp4"))
                        {
                            Response.Write("<tr><td colspan=2>");
                            Response.Write("<h2>" + (filewithtitle.Item2 == "" ? "--Untitled--" : filewithtitle.Item2) + "</h2>");
                            Response.Write("</td></tr><tr><td colspan=2>");
                            String strFlash;
                            strFlash = "<embed type=\"application/x-shockwave-flash\" ";
                            strFlash += "src=\"auctionVideo.swf?titleUrl=" + filewithtitle.Item1 + "\" ";
                            strFlash += "width=\"640\" ";
                            strFlash += "id=\"movie_player\" ";
                            strFlash += "height=\"390\" ";
                            strFlash += "allowscriptaccess=\"always\" ";
                            strFlash += "allowfullscreen=\"true\" ";
                            strFlash += "bgcolor=\"#000000\">";
                            strFlash += "</embed>";
                            Response.Write(strFlash);
                            Response.Write("</td></tr>");
                        }
                        else if (filewithtitle.Item1.ToLower().Contains(".mov"))
                        {
                            Response.Write("<tr><td colspan=2>");
                            Response.Write("<h2>" + (filewithtitle.Item2 == "" ? "--Untitled--" : filewithtitle.Item2) + "</h2>");
                            Response.Write("</td></tr><tr><td colspan=2>");
                            String strMov;
                            strMov = "<embed width='640' height='390' autoplay='false' src='images/" + filewithtitle.Item1 + "' type='video/quicktime' pluginspage='http://www.apple.com/quicktime/download/'></embed>";
                            Response.Write(strMov);
                            Response.Write("</td></tr>");
                        }
                        else
                        {
                            Response.Write("<tr><td colspan=2>");
                            Response.Write("<h2>" + (filewithtitle.Item2 == "" ? "--Untitled--" : filewithtitle.Item2) + "</h2>");
                            Response.Write("</td></tr><tr><td colspan=2>");
                            Response.Write("<img src='images/" + filewithtitle.Item1 + "' alt=" + (filewithtitle.Item2 == "" ? "'--Untitled--'" : "'" + filewithtitle.Item2 + "'") + " />");
                            Response.Write("</td></tr>");
                        }
                        
                    }
                    Response.Write("</table>");
                    Response.Write("</center>");
                }
            }
            catch (Exception ex)
            {
                Response.Write("Exception generating page:" + ex.Message);
            }
        }
    }
}
