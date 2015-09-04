<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestRepeaterX.aspx.cs" Inherits="Test._Default" %>

<%@ Register Assembly="BobSystem.Controls" Namespace="BobSystem.Controls" TagPrefix="mng" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <mng:RepeaterX ID="rpt" runat="server" AllowPaging="true" PageSize="5"
            onpageindexchanging="rpt_PageIndexChanging" FooterStyle-Colspan="6">
        <HeaderTemplate>
          <table style="width:80%;background-color:gray;" cellspacing="1px">
          <tr style="background-color:white;text-align:center">
            <td width="10px"></td>
            <td>aa</td>
            <td>bb</td>
            <td>cc</td>
            <td>dd</td>
            <td>ee</td>
          </tr>
        </HeaderTemplate>
        <ItemTemplate>
          <tr style="background-color:white">
            <td style="text-align:center"><%# Container.ItemIndex + 1 %></td>
            <td><%# Eval("aa") %></td>
            <td><%# Eval("bb") %></td>
            <td><%# Eval("cc") %></td>
            <td><%# Eval("dd") %></td>
            <td><%# Eval("ee") %></td>
          </tr>
        </ItemTemplate>
        <FooterTemplate>
          </table>
        </FooterTemplate>
      </mng:RepeaterX>
      <br />
      <!-- repeaterX 2 -->
      <mng:RepeaterX ID="rpt2" runat="server" AllowPaging="true" PageSize="5"
            onpageindexchanging="rpt2_PageIndexChanging">
        <HeaderTemplate>
          <table style="width:80%;background-color:gray;" cellspacing="1px">
          <tr style="background-color:white;text-align:center">
            <td width="10px"></td>
            <td>aa</td>
            <td>bb</td>
            <td>cc</td>
            <td>dd</td>
            <td>ee</td>
          </tr>
        </HeaderTemplate>
        <ItemTemplate>
          <tr style="background-color:white">
            <td style="text-align:center"><%# Container.ItemIndex + 1 %></td>
            <td><%# Eval("aa") %></td>
            <td><%# Eval("bb") %></td>
            <td><%# Eval("cc") %></td>
            <td><%# Eval("dd") %></td>
            <td><%# Eval("ee") %></td>
          </tr>
        </ItemTemplate>
        <FooterTemplate>
          </table>
        </FooterTemplate>
      </mng:RepeaterX>
      <br />
      <!-- repeaterX 3 -->
      <mng:RepeaterX ID="rpt3" runat="server" AllowPaging="true" PageSize="5"
            onpageindexchanging="rpt3_PageIndexChanging">
        <HeaderTemplate>
          <table style="width:80%; background-color:gray;" cellspacing="1px">
          <tr style="background-color:white;text-align:center">
            <td width="10px"></td>
            <td>aa</td>
            <td>bb</td>
            <td>cc</td>
            <td>dd</td>
            <td>ee</td>
          </tr>
        </HeaderTemplate>
        <ItemTemplate>
          <tr style="background-color:white">
            <td style="text-align:center"><%# Container.ItemIndex + 1 %></td>
            <td><%# Eval("aa") %></td>
            <td><%# Eval("bb") %></td>
            <td><%# Eval("cc") %></td>
            <td><%# Eval("dd") %></td>
            <td><%# Eval("ee") %></td>
          </tr>
        </ItemTemplate>
        <FooterTemplate>
          </table>
        </FooterTemplate>
      </mng:RepeaterX>
    </div>
    </form>
</body>
</html>
