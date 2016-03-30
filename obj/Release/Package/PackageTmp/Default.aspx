<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AzureML_BES_Web_Template.Default" %>


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

        function cloneBlobInfo() {
            $("#txtAccountName_blob").val($("#txtAccountName").val());
            $("#txtAccountKey_blob").val($("#txtAccountKey").val());
            $("#txtContainer_blob").val($("#txtContainer").val());
        };

        $(document).ready(function () {
            updateSliderIE();
            updatesliderHTML();
            updateResize();

            $("#btnSubmit").click(function () {
                $("#btnStatus").prop("disabled",true);
            });

            if ($("#input_radio_File").is(':checked')) {
                $("#div_fileupload").show();
                $("#div_fileBlob").hide();
            }
            else {
                $("#div_fileupload").hide();
                $("#div_fileBlob").show();
            }

            $("#input_radio_File").change(function () {
                $("#div_fileupload").toggle("slow");
                $("#div_fileBlob").toggle("slow");
            });

            $("#input_radio_Blob").change(function () {
                $("#div_fileupload").toggle("slow");
                $("#div_fileBlob").toggle("slow");
            });
        });
    </script>


</head>
<body onresize="updateResize()">
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
            <form id="form1" runat="server" style="margin: 0 auto; position: relative" cssclass="form-inline" autocomplete="off">
                <div id="divMain">

                   
                        <p id="col-1">
                            <asp:PlaceHolder ID="GlobalPlaceHoder" runat="server"></asp:PlaceHolder>
                        </p>

                    

                    <div id ="both">

                    <fieldset style="margin-top: 30px" id="azure_Storage_Info_fieldset">
                        <legend>Azure Storage Info <!-- <span class="glyphicon glyphicon-triangle-bottom" id="icon_expand_info" style="float: right"></span> --></legend>
                        <div style="text-align: left;" id="azureInfo">
                                
                            <div class="fieldname">Account Name</div>
                            <div>
                                <asp:TextBox ID="txtAccountName" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="fieldname">Account Key</div>
                            <div>
                                <asp:TextBox ID="txtAccountKey" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            </div>
                            <div class="fieldname">Container Name</div>
                            <div>
                                <asp:TextBox ID="txtContainer" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>

                            <!--
                            <div class="form-group has-success has-feedback">
                                <div class="col-sm-9" style="width: 100%; float: none">
                                    <div class="fieldname">Store to Blob Name (e.g. inputblob1.csv - File extention must match output)</div>
                                    <div>
                                        <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" ></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                            -->
                        </div>
                    </fieldset>

                    <fieldset style="margin-top: 30px" id="Input_FIle_Info_fieldset">
                        <legend >Input File Info <!-- <span class="glyphicon glyphicon-triangle-bottom" id="icon_expand_Input" style="float: right"></span> --></legend>
                            <fieldset class="subfieldset">
                                <legend class="sublegend">
                                    <asp:RadioButton ID="input_radio_File" runat="server" Checked="True" GroupName="optradio" />Load from local machine </legend>
                                <div id="div_fileupload">
                                    <p style="margin-left: 20px;">
                                        Select batch file to upload for scoring
                                    <asp:FileUpload ID="_fileUpload" runat="server" CssClass="filestyle" data-buttonName="btn-primary" Width="100%" />
                                    </p>
                                </div>
                            </fieldset>

                        <fieldset class="subfieldset">
                            <legend class="sublegend">
                                <asp:RadioButton ID="input_radio_Blob" runat="server" GroupName="optradio" />Load from Azure Storage Blob<span>
                                    <button type="button" id="btnClone" onclick="cloneBlobInfo()" class="btn btn-warning" style="margin-left: 10px;padding: 0px 5px;">Same as above</button></span></legend>
                                <div style="text-align: left; margin-left:20px " id="div_fileBlob">
                                    <div class="fieldname">Account Name</div>
                                        <div>
                                            <asp:TextBox ID="txtAccountName_blob" runat="server" CssClass="form-control"></asp:TextBox>
                                        </div>
                                    <div class="fieldname">Account Key</div>
                                            <div>
                                                <asp:TextBox ID="txtAccountKey_blob" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                            </div>
                                    <div class="fieldname">Container Name</div>
                                            <div>
                                                <asp:TextBox ID="txtContainer_blob" runat="server" CssClass="form-control"></asp:TextBox>
                                            </div>
                                    <div class="fieldname">Input Blob Name (e.g. inputblob1.csv)</div>
                                            <div>
                                                <asp:TextBox ID="txtBlobName" runat="server" CssClass="form-control"></asp:TextBox>
                                            </div>
                                </div>
                        </fieldset>
                    </fieldset>
                        </div>

                </div>




                <div id="submitDiv">


                    <asp:Button ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-primary btnSubmit" OnClick="btnSubmit_Click" />
                    <asp:Button ID="btnStatus" runat="server" Text="Job Status" CssClass="btn btn-primary btnSubmit" OnClick="btnStatus_Click"/>
                    <!-- <a href="Status.aspx"><button type="button" id="btnStatus" class="btn btn-primary btnSubmit">Job Status</button></a> -->
                    <!-- <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-primary btnSubmit" OnClick="btnClear_Click" />  -->
                </div>



                <div id="divResult" runat="server">
                    <asp:PlaceHolder ID="OutputPlaceHolder" runat="server"></asp:PlaceHolder>
                    <div id="topLoader"></div>
                </div>

                <asp:PlaceHolder ID="bottomsciprtPlaceHolde" runat="server"></asp:PlaceHolder>

                <div id="footer">
                    <div class="footerText">
                        <p>Powered by <strong>Azure Machine Learning</strong></p>
                    </div>

                </div>



<!-- Modal Job success -->
  <div class="modal fade" id="CompleteSuccess" role="dialog">
    <div class="modal-dialog">    
      <div class="modal-content">    
        <div class="modal-body alert alert-success fade in" style="margin-bottom: 0px;">
            <a href="#" class="close" data-dismiss="modal" aria-label="Close">&times;</a>
          <p><strong>Success!</strong> Job <asp:Label ID="lblJobIdSuccess" runat="server" Text="Label" Font-Bold="True"></asp:Label> is completed.</p>
        </div>
       
      </div>
      
    </div>
  </div>
  
<!-- Modal Job Error-->
  <div class="modal fade" id="failModal" role="dialog">
    <div class="modal-dialog">    
      <div class="modal-content">    
        <div class="modal-body alert alert-danger fade in" style="margin-bottom: 0px;">
            <a href="#" class="close" data-dismiss="modal" aria-label="Close">&times;</a>
          <p><strong>Error!</strong> Check description below:</p>
        </div>  
        <div class="modal-footer">            
            <asp:TextBox ID="txtresultModal" runat="server" TextMode="MultiLine" Width="100%"></asp:TextBox>
        </div>         
      </div>
      
    </div>
  </div>


<!-- Modal Clear Confirm-->
  <div class="modal fade" id="clearModal" role="dialog">
    <div class="modal-dialog">    
      <div class="modal-content">    
        <div class="modal-body alert alert-warning fade in" style="margin-bottom: 0px;">            
          <h3><strong>Warning!</strong> This will CLEAR all Job Ids in this Browser. OK to continue?</h3>
        </div>            
        <div class="modal-footer">            
            <asp:Button ID="btnClearOK" runat="server" Text="OK" CssClass="btn btn-primary btn-lg" OnClick="btnClearOK_Click"/>
            <button type="button" class="btn btn-primary btn-lg" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">Cancel</span></button> 
        </div>        
      </div>      
    </div>
  </div>

            </form>
        </div>
        <!-- /container -->





    <!-- IE10 viewport hack for Surface/desktop Windows 8 bug -->
    <script src="Scripts/js/ie10-viewport-bug-workaround.js"></script>
    <%--</form>--%>
</body>
</html>

<script type="text/javascript" src="Scripts/wheels.js"></script>
