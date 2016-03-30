<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Status.aspx.cs" Inherits="AzureML_BES_Web_Template.Status" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
    <meta name="description" content="Azure Machine Learning Web Service">
    <meta name="author" content="">

    <title></title>

    <!-- Bootstrap core CSS -->
    <link href="CSS/bootstrap.css" rel="stylesheet">
    <link href="CSS/jumbotron-narrow.css" rel="stylesheet">
    <link href="CSS/master.css" rel="stylesheet">

    <!-- Java script / jQuery -->
    <script src="Scripts/jquery-2.1.4.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="http://d3js.org/d3.v3.min.js"></script>
    <script type="text/javascript" src="Scripts/bootstrap-filestyle.min.js"> </script>

    <link href="http://www.bootstrap-switch.org/dist/css/bootstrap3/bootstrap-switch.css" rel="stylesheet" />
    <script src="http://www.bootstrap-switch.org/dist/js/bootstrap-switch.js"></script>
    <!-- -->

    <link href="CSS/jquery-ui-1.10.0.custom.min.css" rel="stylesheet">
    <link href="CSS/sliderIE.css" rel="stylesheet">
    <link href="CSS/sliderHTML.css" rel="stylesheet">
    <script src="Scripts/sliderIE.js"></script>
    <script src="Scripts/sliderHTML.js"></script>
    <script src="Scripts/jquery-ui.js"></script>
    <script src="Scripts/master.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            $("#chkall").click(function () {
                $('input:checkbox').not(this).prop('checked', this.checked);
            });
        });
    </script>

</head>
<body>
    <div class="container azue-header">
        <div class="jumbotron azue-header" style="background-image: url('Resources/azure-ml.png'); height: 126px; position: relative; padding-top: 10px;">
            <a href="/">
                <div class="wheel">
                    <wheel></wheel>
                </div>
            </a>
            <div style="max-height: 110px; overflow-y: hidden">
                <h3 class="text-muted" style="padding-left: 110px;">
                    <asp:Label ID="lblTitle" runat="server" Text="Azure Machine Learning"></asp:Label></h3>
            </div>
        </div>
    </div>
    <div class="container" id="fullpage">
        <form id="form1" runat="server" class="form-horizontal">
            <div>
                <asp:LinkButton ID="btnHome" runat="server" CssClass="btn btn-primary btnMenu" PostBackUrl="~/Default.aspx" OnClick="btnCancelJob_Click" ><span class="glyphicon glyphicon-home"></span>Home</asp:LinkButton>
                <!-- <a href="Default.aspx"><button type="button" class="btn btn-primary btnMenu" title="Go to Home"><span class="glyphicon glyphicon-home" aria-hidden="true"></span>Home</button></a> -->
                <asp:PlaceHolder ID="PlaceHolderMenu" runat="server"></asp:PlaceHolder>
                <asp:Button ID="btnCancelJob" runat="server" Text="Cancel Job" CssClass="btn btn-primary btnMenu" OnClick="btnCancelJob_Click" />

                <!-- <button type="button" class="btn btn-primary btnMenu" >Clear</button> -->

                <br>
                
            </div>

            <div id="statusinfo">
                <div class="form-group" style="clear: both;">
                    <label class="col-sm-2 control-label">Job ID</label>
                    <div class="col-sm-10">
                        <p class="form-control-static">
                            <asp:Label ID="lblJobId" runat="server"></asp:Label>
                        </p>
                    </div>
                </div>
                <div class="form-group">
                    <label for="inputPassword" class="col-sm-2 control-label">Status</label>
                    <div class="col-sm-10">
                        <p class="form-control-static">
                            <asp:Label ID="lblStatus" runat="server"></asp:Label>
                        </p>
                        <p style="font-style: italic; font-size: 12px;">Updated automatically (every 10 seconds)</p>
                    </div>
                </div>
            </div>

            <div style="margin-left: 20px;clear: both;">

                <asp:PlaceHolder ID="PlaceHolderOutput" runat="server"></asp:PlaceHolder>

            </div>

            <div id="menuDelete">
                <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-primary btnSubmit" OnClick="btnDelete_Click" />
            </div>
        </form>

        <div id="footer">
            <div class="footerText">
                <p>Powered by <strong>Azure Machine Learning</strong></p>
            </div>

        </div>
    </div>


</body>
</html>

<script type="text/javascript" src="Scripts/wheels.js"></script>
