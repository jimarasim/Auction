using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text; //stringbuilder

using System.Threading; //Thread for handling uploading async files

namespace Auction
{

    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                //clear error messages
                errormessage.Text = "";
                usererrormessage.Text = "";

                //populate file upload controls            
                FileUpload fu;
                FileUploadPlaceHolder.Controls.Add(new LiteralControl("<table>"));
                for (int i = 0; i < Convert.ToInt32(NumberOfFilesDropDown.SelectedValue); i++)
                {
                    fu = new FileUpload();
                    fu.ID = "fu" + Convert.ToString(i);
                    FileUploadPlaceHolder.Controls.Add(new LiteralControl("<tr><td>"));
                    FileUploadPlaceHolder.Controls.Add(fu);
                    FileUploadPlaceHolder.Controls.Add(new LiteralControl("</td></tr>"));
                }
                FileUploadPlaceHolder.Controls.Add(new LiteralControl("</table>"));
                
                //wire item drop downs at the top of the grid view and in the item template
                AuctionDb adb = new AuctionDb();

                //wire item drop downs
                itemdropdownDataSource.SelectCommand = adb.GetDistinctItemsCommandString(Convert.ToInt32(Session["UID"]));
                itemdropdownDataSource.ConnectionString = adb.ConnectionString();

                //wire gridview into database
                GridView1DataSource.ConnectionString = adb.ConnectionString();
                GridView1DataSource.SelectCommand = adb.GetCertainItemsCommandString(Convert.ToString(itemdropdownabovegrid.SelectedValue.Replace("'", "''").Replace('"', '\"')),Convert.ToInt32(Session["UID"]));
                GridView1DataSource.UpdateCommand = adb.UpdateItemsCommandString();
                GridView1DataSource.DeleteCommand = adb.DeleteItemCommandString();

                //if this is user 1, then show the users table
                if (Convert.ToInt32(Session["UID"]) == 1)
                {
                    //wire the users gridview datasource into the database
                    UsersDataSource.ConnectionString = adb.ConnectionString();
                    UsersDataSource.SelectCommand = adb.GetUsersCommandString();
                    UsersDataSource.UpdateCommand = adb.UpdateUsersCommandString();
                    UsersDataSource.InsertCommand = adb.InsertUserCommandString();
                    UsersDataSource.DeleteCommand = adb.DeleteUserCommandString();

                    UsersUpdatePanel.Visible = true;

                }

            }
            catch (Exception ex)
            {
                errormessage.Text = "Page Load Exception:" + ex.Message;
            }
        }

        //add a user from the users grid to the USERS table
        protected void adduserbutton_click(object sender, EventArgs e)
        {
            try
            {
                TextBox addusernametext = (TextBox)UsersGridView.FooterRow.FindControl("addusernametext");
                TextBox addpasswordtext = (TextBox)UsersGridView.FooterRow.FindControl("addpasswordtext");
                TextBox addusertitletext = (TextBox)UsersGridView.FooterRow.FindControl("addusertitletext");

                UsersDataSource.InsertParameters["USERNAME"].DefaultValue = addusernametext.Text;
                UsersDataSource.InsertParameters["PASSWORD"].DefaultValue = addpasswordtext.Text;
                UsersDataSource.InsertParameters["TITLE"].DefaultValue = addusertitletext.Text;

                UsersDataSource.Insert();
            }
            catch (Exception ex)
            {
                usererrormessage.Text = "EXCEPTION ADDING USER:" + ex.Message;
            }
        }


        protected void UploadButton_Click(object sender, EventArgs e)
        {
            //upload all files selected
            try
            {
                AuctionDb adb = new AuctionDb();
                FileUpload fu;
                StringBuilder invalidfiles = new StringBuilder();
                InvalidFilesLabel.Text = "";

                for (int i = 0; i < Convert.ToInt32(NumberOfFilesDropDown.SelectedValue); i++)
                {
                    fu = (FileUpload)FileUploadPlaceHolder.FindControl("fu" + Convert.ToString(i));
                    if (fu.HasFile)
                    {
                        string name = Path.GetFileNameWithoutExtension(fu.FileName);
                        string extension = Path.GetExtension(fu.FileName);
                        string filename = name + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fffffff") + extension;

                        if (extension.ToLower().Contains(".mov") ||
                            extension.ToLower().Contains(".flv") ||
                            extension.ToLower().Contains(".f4v") || 
                            extension.ToLower().Contains(".mp4") || 
                           extension.ToLower().Contains(".mp3") ||
                           extension.ToLower().Contains(".png") ||
                           extension.ToLower().Contains(".gif") ||
                           extension.ToLower().Contains(".jpg"))
                        {
                            fu.SaveAs(Server.MapPath("images/") + filename);
                            //insert record in database
                            adb.InsertNewFileIntoDatabase(filename.Replace("'", "''").Replace('"', '\"'), Convert.ToInt32(Session["UID"]));
                        }
                        else
                        {
                            invalidfiles.AppendLine(name + extension);
                        }

                    }
                }

                if (invalidfiles.Length > 0)
                {
                    InvalidFilesLabel.Text = "Only .mov, .flv, .f4v, .mp4, .mp3, .png, .gif, and .jpg allowed, not:" + invalidfiles.ToString();
                    return;
                }

                //new item added, update dropdown in case it's the first of it's kind
                itemdropdownabovegrid.DataBind();

                //select the blank one after a file upload
                itemdropdownabovegrid.SelectedIndex = 0;
                gridview1_update();
            }
            catch (Exception ex)
            {
                InvalidFilesLabel.Text = "EXCEPTION UPLOADING: " + ex.Message;
                return;
            }
        }

        protected void GridView1_RowDeleting(Object sender, GridViewDeleteEventArgs e)
        {
            //delete file from images folder when the...before record is deleted, where we can still get the file name
            try
            {
                Label filenamelabel = (Label)GridView1.Rows[e.RowIndex].FindControl("filenamelabel");
                File.Delete(Server.MapPath("images/" + filenamelabel.Text));
            }
            catch (Exception ex)
            {
                errormessage.Text = "EXCEPTION GridView1_RowDeleting:" + ex.Message;
            }
        }

        protected void UsersGridView_RowDeleting(Object sender, GridViewDeleteEventArgs e)
        {
            //make sure admin row is not being deleted, and
            //remove files from folder and records from items table when a user is deleted
            try
            {
                AuctionDb adb = new AuctionDb();

                //get the user id of the row being deleted
                Label useridlabel = (Label)UsersGridView.Rows[e.RowIndex].FindControl("useridlabel");

                //don't delete the admin row
                if (Convert.ToInt32(useridlabel.Text) == 1)
                {
                    usererrormessage.Text = "CANNOT DELETE ADMIN USERID 1";
                    e.Cancel = true;
                    return;
                }

                //remove the items records and get a list of files to delete
                List<String> filesToDelete = adb.RemoveItemsAndReturnFiles(Convert.ToInt32(useridlabel.Text));

                //delete the files
                foreach (String file in filesToDelete)
                {
                    File.Delete(Server.MapPath("images/" + file));
                }
            }
            catch (Exception ex)
            {
                usererrormessage.Text = "EXCEPTION DELETING USER ITEMS:" + ex.Message;
            }

        }

        protected void GridView1_RowDeleted(Object sender, GridViewDeletedEventArgs e)
        {
            //item deleted, update dropdown in case it's the last of it's kind
            itemdropdownabovegrid.DataBind();

        }

        protected void GridView1_RowUpdated(object sender, EventArgs e)
        {
            //item updated, update dropdown in case it's the first of it's kind
            itemdropdownabovegrid.DataBind();
        }

        protected void itemdropdownabovegrid_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView1.DataBind();
        }

        private String oldselectedvalue;
        protected void itemdropdownabovegrid_DataBinding(object sender, EventArgs e)
        {
            //we're about to bind the dropdown, so save the selection
            oldselectedvalue = itemdropdownabovegrid.SelectedValue;
        }

        protected void itemdropdownabovegrid_DataBound(object sender, EventArgs e)
        {
            //data was bound, but we want our last selected value to be selected before updating the grid (unless it's gone)
            AuctionDb adb = new AuctionDb();
            if (adb.ItemExists(oldselectedvalue.Replace("'", "''").Replace('"', '\"'), Convert.ToInt32(Session["UID"])))
            {
                itemdropdownabovegrid.SelectedValue = oldselectedvalue;
            }

            gridview1_update();

        }

        private void gridview1_update()
        {
            AuctionDb adb = new AuctionDb();

            //re-establish select command, or we'll see no results 
            GridView1DataSource.SelectCommand = adb.GetCertainItemsCommandString(Convert.ToString(itemdropdownabovegrid.SelectedValue.Replace("'", "''").Replace('"', '\"')), Convert.ToInt32(Session["UID"]));

            GridView1.DataBind();
        }

        protected void itemdropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //when a dropdown item is selected in edititem template, populate the bound item field with that text
                ((TextBox)GridView1.Rows[GridView1.EditIndex].FindControl("itemtext")).Text =
                    ((DropDownList)GridView1.Rows[GridView1.EditIndex].FindControl("itemdropdown")).Text;
            }
            catch (Exception ex)
            {
                errormessage.Text = "EXCEPTION itemdropdown_SelectedIndexChanged:" + ex.Message;
            }
        }

        protected String DisplayFileTag(String filename)
        {
            try
            {
                //used by gridview itemtemplate to format the file as an audio link or image
                if(filename.ToLower().Contains(".mp3"))
                {
                    return "<h2><a href='images/" + filename + "'>" + filename + "</a></h2>";
                }
                else if (filename.ToLower().Contains(".flv") || filename.ToLower().Contains(".f4v") || filename.ToLower().Contains(".mp4"))
                {

                    String strFlash;
                    strFlash = "<embed type=\"application/x-shockwave-flash\" ";
                    strFlash += "src=\"auctionVideo.swf?titleUrl=" + filename + "\" ";
                    strFlash += "width=\"640\" ";
                    strFlash += "id=\"movie_player\" ";
                    strFlash += "height=\"390\" ";
                    strFlash += "allowscriptaccess=\"always\" ";
                    strFlash += "allowfullscreen=\"true\" ";
                    strFlash += "bgcolor=\"#000000\">";
                    strFlash += "</embed>";

                    return strFlash;
                }
                else if (filename.ToLower().Contains(".mov"))
                {
                    String strMov;
                    strMov = "<embed width='640' height='390' autoplay='false' src='images/" + filename + "' type='video/quicktime' pluginspage='http://www.apple.com/quicktime/download/'></embed>";

                    return strMov;
                }
                else
                {
                    return "<img src='images/" + filename + "' alt='" + filename + "' />";
                }
            }
            catch (Exception ex)
            {
                return "EXCEPTION DisplayFileTag:" + ex.Message;
            }
        }

    }
}
