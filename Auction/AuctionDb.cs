using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading;

using System.Data.SqlClient;

namespace Auction
{
    enum DbLocation
    {
        local,
        godaddy
    }

    public class AuctionDb
    {
        //location where app is running
        private DbLocation sqlLocation = DbLocation.godaddy;

        //connection string for location where app is running
        private SqlConnectionStringBuilder sqlString = new SqlConnectionStringBuilder();

        public AuctionDb()
        {
            //build connection string depending on location
            switch (sqlLocation)
            {
                case DbLocation.local:
                    sqlString.DataSource = "JAEMZWARE-PC";
                    sqlString.InitialCatalog = "Auction";
                    sqlString.IntegratedSecurity = true;
                    sqlString.PersistSecurityInfo = true;
                    break;
                case DbLocation.godaddy:
                    sqlString.DataSource = "ackermanauction.db.7736389.hostedresource.com";
                    sqlString.InitialCatalog = "ackermanauction";
                    sqlString.UserID = "ackermanauction"; //discountasp.net
                    sqlString.Password = "Shop@Ebay8"; //discountasp.net
                    break;
                default:
                    break;
            }

        }

        //opens a connection to the database
        private SqlConnection ConnectToDatabase()
        {
            SqlConnection connection = new SqlConnection(sqlString.ConnectionString);
            connection.Open();

            return connection;
        }

        //returns the connection string for current location
        public String ConnectionString()
        {
            return sqlString.ConnectionString;
        }

        /*old gridview select command
        //returns command string for getting items
        public String GetAllItemsCommandString()
        {
            String sql = "SELECT ID,RTRIM(Item) AS Item,RTRIM(Title) AS Title,RTRIM(Filename) AS Filename FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] ";
            sql += "ORDER BY ID DESC";
            return sql;
        }*/

        //used by admin.aspx.cs for gridview1's select command
        //returns command string for getting items
        public String GetCertainItemsCommandString(String item, int UID)
        {
            String sql;
            sql = "SELECT ID,RTRIM(Item) AS Item,RTRIM(Title) AS Title,RTRIM(Filename) AS Filename, UID FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] ";
            
            sql += "WHERE UID="+UID+" ";

            if (String.IsNullOrEmpty(item))
            {
                sql += "AND Item IS NULL ";
            }
            else if (ItemExists(item,UID))
            {
                sql += "AND Item='" + item + "' ";             
            }

            sql += "ORDER BY ID DESC";
            return sql;
        }

        //used by admin.aspx.cs for usergridview select command
        public String GetUsersCommandString()
        {
            return "SELECT ID,USERNAME,PASSWORD,TITLE FROM [" + sqlString.InitialCatalog + "].[dbo].[USERS]";
        }

        //YES UID REQD
        //used by admin.aspx.cs and GetCertainItemsCommandString for gridview1
        public bool ItemExists(String item, int UID)
        {
            bool itemexists;

            SqlConnection cxn = ConnectToDatabase();
            SqlCommand cmd = new SqlCommand("SELECT [Item] FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] WHERE [Item] = '" + item + "' AND [UID]="+UID, cxn);
            SqlDataReader rdr = cmd.ExecuteReader();
            itemexists = rdr.HasRows;
            cxn.Close();

            return itemexists;
        }

        //NO UID REQD
        //used by admin.aspx.cs for gridview1's update command
        public String UpdateItemsCommandString()
        {
            return "UPDATE [" + sqlString.InitialCatalog + "].[dbo].[Items] SET Item=@Item, Title=@Title, Filename=@Filename WHERE ID=@ID";
          
        }

        //used by admin.aspx.cs for user gridview's update command
        public String UpdateUsersCommandString()
        {
            return "UPDATE [" + sqlString.InitialCatalog + "].[dbo].[USERS] SET USERNAME=@USERNAME, PASSWORD=@PASSWORD, TITLE=@TITLE WHERE ID=@ID";
        }

        //used by admin.aspx.cs for user gridview's insert command
        public String InsertUserCommandString()
        {
            return "INSERT INTO [" + sqlString.InitialCatalog + "].[dbo].[USERS] ([USERNAME],[PASSWORD],[TITLE]) VALUES (@USERNAME,@PASSWORD,@TITLE)";
        }

        //NO UID REQD
        //used by admin.aspx.cs for gridview1's delete command
        public String DeleteItemCommandString()
        {
            return "DELETE FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] WHERE ID=@ID";
        }

        public String DeleteUserCommandString()
        {
            return "DELETE FROM [" + sqlString.InitialCatalog + "].[dbo].[USERS] WHERE ID=@ID";
        }
        
        //YES UID REQD
        //used by admin.aspx.cs to populate item dropdowns and default.aspx.cs to list items
        //returns command string for getting distinct items
        public String GetDistinctItemsCommandString(int UID)
        {
            return "SELECT DISTINCT [Item] FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] WHERE [UID]="+UID+"ORDER BY Item";
        }

        //YES UID REQD
        //used by admin.aspx.cs
        //inserts a new record into the items database with only a filename
        public void InsertNewFileIntoDatabase(String filename,int UID)
        {
            SqlConnection cxn = ConnectToDatabase();
            SqlCommand cmd = new SqlCommand("INSERT INTO [" + sqlString.InitialCatalog + "].[dbo].[Items] ([Filename],[UID]) VALUES ('"+filename+"',"+UID+")",cxn);
            cmd.ExecuteNonQuery();
            cxn.Close();
        }

        public List<String> SelectAllUsers()
        {
            SqlConnection cxn = ConnectToDatabase();
            SqlCommand cmd = new SqlCommand("SELECT USERNAME FROM [" + sqlString.InitialCatalog + "].[dbo].[USERS] ORDER BY USERNAME", cxn);
            SqlDataReader rdr = cmd.ExecuteReader();

            List<String> listOfUsers = new List<String>();

            while (rdr.Read())
            {
                listOfUsers.Add(Convert.ToString(rdr[0]));
            }

            cxn.Close();

            return listOfUsers;
        }

        //YES UID REQD
        //used by default.aspx.cs
        public List<String> SelectItems(int UID)
        {
            SqlConnection cxn = ConnectToDatabase();
            SqlCommand cmd = new SqlCommand(GetDistinctItemsCommandString(UID), cxn);
            SqlDataReader rdr = cmd.ExecuteReader();

            List<String> listOfItems = new List<String>();

            while (rdr.Read())
            {
                listOfItems.Add(Convert.ToString(rdr[0]));
            }

            cxn.Close();

            return listOfItems;
        }

        //YES UID REQUD
        //get first image found for item
        public String SelectImageForItem(String item, int UID)
        {
            String retval = "";
            SqlConnection cxn = ConnectToDatabase();
            SqlCommand cmd = new SqlCommand("SELECT TOP 1 [Filename] FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] WHERE [Item] = '" + item + "' AND [UID]=" + UID + " AND ([Filename] LIKE ('%.jpg') OR [Filename] LIKE ('%.gif') OR [Filename] LIKE ('%.png'))", cxn);
            SqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
            {
                rdr.Read();
                retval = Convert.ToString(rdr[0]);
            }

            cxn.Close();

            return retval;
        }

        //this comparer function is used to sort files by extention in SelectFilenameTitleForItem
        private static int CompareByFileExtensionThenTitle(Tuple<String, String> x, Tuple<String, String> y)
        {
            //get the extension of the filename, and add the title to the end of it
            String xExtension = x.Item1.Substring(x.Item1.LastIndexOf(".") + 1)+x.Item2;
            String yExtension = y.Item1.Substring(y.Item1.LastIndexOf(".") + 1)+y.Item2;

            return String.Compare(xExtension, yExtension);
        }


        //YES UID REQD
        //used by default.aspx.cs
        public List<Tuple<String, String>> SelectFilenameTitleForItem(String item, int UID)
        {
            SqlConnection cxn = ConnectToDatabase();
            SqlCommand cmd;
            
            //check if we're looking for unnamed files
            if (item.Equals("--Ungrouped--"))
            {
                cmd = new SqlCommand("SELECT [Filename],[Title] FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] WHERE [Item] IS NULL AND [UID]=" + UID, cxn);
            }
            else
            {
                cmd = new SqlCommand("SELECT [Filename],[Title] FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] WHERE [Item] = '" + item + "' AND [UID]=" + UID, cxn);
            }
            SqlDataReader rdr = cmd.ExecuteReader();
            
            //FILENAME TITLE
            List<Tuple<String, String>> fileswithtitles = new List<Tuple<String, String>>();

            while (rdr.Read())
            {
                fileswithtitles.Add(new Tuple<String,String>(Convert.ToString(rdr[0]), Convert.ToString(rdr[1])));
            }

            //SORT ACCORDING TO SORT ROUTINE CompareByFileExtensionThenTitle
            fileswithtitles.Sort(CompareByFileExtensionThenTitle);

            cxn.Close();

            return fileswithtitles;
        }

        //NO UID REQD...THIS GETS IT!
        //used by login.aspx.cs
        //returns ID TITLE and USERNAME if user is in the database; otherwise 0 and empty string
        //note: collation on USERNAME is case insensitive...must be something to do with varchar
        public Tuple<int,String, String> Authenticate(String username, String password)
        {
            //get the ID, TITLE, and USERNAME for this USERNAME and PASSWORD
            SqlConnection cxn = ConnectToDatabase();
            Tuple<int, String, String> userInfo;
            SqlCommand cmd = new SqlCommand("SELECT [ID],[TITLE],[USERNAME] FROM [" + sqlString.InitialCatalog + "].[dbo].[USERS] WHERE [USERNAME] = '" + username + "' AND [PASSWORD] ='"+password+"'", cxn);
            SqlDataReader rdr = cmd.ExecuteReader();

            //if there's a record (there will only be one because USERNAME is marked UNIQUE in USERS), then create a tuple with the ID and TITLE in it
            if (rdr.HasRows)
            {
                rdr.Read();
                userInfo = Tuple.Create<int, String, String>(Convert.ToInt32(rdr[0]), Convert.ToString(rdr[1]), Convert.ToString(rdr[2]));
            }
            //otherwise, signify failure with an ID of 0 and a blank TITLE
            else
            {
                userInfo = Tuple.Create<int, String, String>(0, "", "");
            }

            cxn.Close();

            return userInfo;
        }

        //NO UID REQUD...THIS GETS IT
        //used by default.aspx.cs to get requested user's id, title, and username
        public Tuple<int, String, String> UserExists(String username)
        {
            //get the ID, TITLE, and USERNAME for this USERNAME
            SqlConnection cxn = ConnectToDatabase();
            Tuple<int, String, String> userInfo;
            SqlCommand cmd = new SqlCommand("SELECT [ID],[TITLE],[USERNAME] FROM [" + sqlString.InitialCatalog + "].[dbo].[USERS] WHERE [USERNAME] = '" + username + "'", cxn);
            SqlDataReader rdr = cmd.ExecuteReader();
            
            //if there's a record (there will only be one because USERNAME is marked UNIQUE in USERS), then create a tuple with the ID and TITLE in it
            if (rdr.HasRows)
            {
                rdr.Read();
                userInfo = Tuple.Create<int, String, String>(Convert.ToInt32(rdr[0]), Convert.ToString(rdr[1]), Convert.ToString(rdr[2]));
            }
            //otherwise, signify failure with an ID of 0 and a blank TITLE
            else
            {
                userInfo = Tuple.Create<int, String, String>(0, "", "");
            }

            cxn.Close();

            return userInfo;
        }

        //use this to call updatetitle stored procedure
        public int UpdateUserTitle(int userId, String title)
        {
            SqlConnection cxn = ConnectToDatabase();
            SqlCommand cmd = new SqlCommand("UpdateTitle", cxn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@user_id", userId));
            cmd.Parameters.Add(new SqlParameter("@new_title", title));

            int retval = cmd.ExecuteNonQuery();

            cxn.Close();

            return retval;
        }

        //use this to remove records from the items table for a user, and return a list of files those records pointed to
        public List<String> RemoveItemsAndReturnFiles(int userId)
        {
            //get the list of files to remove
            SqlConnection cxn = ConnectToDatabase();
            SqlCommand cmd = new SqlCommand("SELECT [Filename] FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] WHERE [UID]=" + userId, cxn);
            SqlDataReader rdr = cmd.ExecuteReader();

            List<String> filestoremove = new List<String>();

            while (rdr.Read())
            {
                filestoremove.Add(Convert.ToString(rdr[0]));
            }

            rdr.Close();

            //remove the records for the user
            SqlCommand deletecmd = new SqlCommand("DELETE FROM [" + sqlString.InitialCatalog + "].[dbo].[Items] WHERE [UID]=" + userId, cxn);
            deletecmd.ExecuteNonQuery();

            cxn.Close();

            return filestoremove;
        }
    }
}
