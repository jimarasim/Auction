<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="Auction.login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Site Login</title>
    <link rel="Stylesheet" href="auction.css" />
</head>
<body>
    <form id="form1" runat="server" defaultbutton="Login" defaultfocus="UserName">
    <div>
        <div class="formsection">
        <span class="formsectiontitle">Authenticate</span><br />
        <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
        <asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox>
        <asp:Button ID="Login" runat="server" Text="Login" onclick="Login_Click" />
        <asp:Label ID="ErrorLabel" runat="server"></asp:Label>
        </div>
    </div>
    </form>
</body>
</html>
