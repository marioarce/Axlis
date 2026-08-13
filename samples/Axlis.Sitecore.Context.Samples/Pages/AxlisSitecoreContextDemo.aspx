<%@ Page Title="Axlis.Sitecore.Context Demo" Language="C#" AutoEventWireup="true" Codebehind="AxlisSitecoreContextDemo.aspx.cs" Inherits="Axlis.Sitecore.Samples.Pages.AxlisSitecoreContextDemo" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Axlis.Sitecore.Context - Thread-Safety Demo</title>
</head>
<body style="font-family: sans-serif;">
    <form id="MainForm" runat="server">
        <h1>Axlis.Sitecore.Context - Thread-Safety Demo</h1>
        <p>
            This page reads <code>Sitecore.Context.Database</code> and <code>HttpContext.Current</code>
            (the non-thread-safe ambient statics) side-by-side with <code>Axlis.Sitecore.Context.Database</code>
            and <code>.Request</code> - first on the original request thread, then again from inside several
            simulated background threads (<code>Task.Run</code>). Expect the raw statics to go <code>NULL</code>
            on the simulated threads while the Axlis equivalents stay correct.
        </p>

        <h2>On the original request thread</h2>
        <asp:Literal ID="BaselineLiteral" runat="server" />

        <h2>From simulated background threads</h2>
        <asp:Literal ID="ThreadResultsLiteral" runat="server" />
    </form>
</body>
</html>
