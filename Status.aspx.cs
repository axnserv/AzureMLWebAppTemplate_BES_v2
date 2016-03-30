using AzureML_BES_Web_Template.Model;
using CryptoLibrary;
using Newtonsoft.Json.Linq;
using ParameterIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AzureML_BES_Web_Template
{
    public partial class Status : System.Web.UI.Page
    {
        AMLParameterObject paramObj = new AMLParameterObject();
        string jobid;
        protected void Page_Load(object sender, EventArgs e)
        {
            string BaseUrl = "";
            jobid = Request.QueryString["jobid"];//"https://ussouthcentral.services.azureml.net/workspaces/cdabeb6c4eb24cbda26e0a4da9c91bc3/services/7ba38e2fa40549538517794343c259da/jobs";

            lblJobId.Text = jobid;
            
            if (paramObj.ImportInputParameter(Server.MapPath("~\\Resources\\AMLParameter.xml")))
            {
                Page.Title = paramObj.Title;
                lblTitle.Text = paramObj.Title;


                if (string.IsNullOrEmpty(jobid))
                {
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "Hide", "document.getElementById(\"statusinfo\").style.display = \"none\";", true);
                    btnCancelJob.Visible = false;
                    ShowListJobID(paramObj.APIKey);
                }
                else
                {
                    PlaceHolderMenu.Controls.Add(new LiteralControl("<a href=\"Status.aspx\" class=\"btn btn-primary btnMenu\">Recent Jobs</a>"));
                    //PlaceHolderMenu.Controls.Add(new LinkButton(
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "Hide", "document.getElementById(\"menuDelete\").style.display = \"none\";", true);
                    BaseUrl = paramObj.Url.Replace("jobs", "jobs/" + jobid);
                    InvokeBatchExecutionService(BaseUrl, Crypto.DecryptStringAES(paramObj.APIKey)).Wait();
                }
            }
            else RequireInfor();
        }

        private void ShowListJobID(string api)
        {
            try
            {
                string strListJobId = "";
                try
                {
                    strListJobId = Request.Cookies["ListJobIds"].Value;
                    //strListJobId = Request.Cookies[paramObj.APIKey].Value;
                }
                catch (Exception) { };
                List<string> listJobId;
                if (!string.IsNullOrEmpty(strListJobId))
                    listJobId = strListJobId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                else listJobId = new List<string>();

                if (listJobId.Count == 0)
                {
                    PlaceHolderOutput.Controls.Add(new LiteralControl("<h3>No recent Job</h3>"));
                    PlaceHolderOutput.Controls.Add(new LiteralControl("<p style=\"font-size:12px\">Please make sure your browser allows cookies</p>"));
                    return;
                }

                listJobId.Reverse();

                PlaceHolderOutput.Controls.Add(new LiteralControl("<h3>List of recent Jobs</h3>"));
                PlaceHolderOutput.Controls.Add(new LiteralControl("<table class=\"table  table-bordered table-hover\">"));
                PlaceHolderOutput.Controls.Add(new LiteralControl("<tr><th style=\"width: 5%;\"><input id=\"chkall\" name=\"chkall\" type=\"checkbox\"></th><th style=\"width: 5%;\">#</th><th>Job ID</th><th>Start Date</th></tr>"));
                int count = 0;                
                foreach (string jobId in listJobId)
                {                    
                    string[] path = jobId.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (api != path[2])
                        continue;
                    PlaceHolderOutput.Controls.Add(new LiteralControl("<tr>"));
                    CheckBox chkbox = new CheckBox();
                    chkbox.ID = path[0];
                    PlaceHolderOutput.Controls.Add(new LiteralControl("<td>"));
                    PlaceHolderOutput.Controls.Add(chkbox);
                    PlaceHolderOutput.Controls.Add(new LiteralControl("</td>"));
                    PlaceHolderOutput.Controls.Add(new LiteralControl("<td>" + (count + 1).ToString() + "</td>"));
                    PlaceHolderOutput.Controls.Add(new LiteralControl("<td><a href=\"Status.aspx?jobid=" + path[0] + "\">" + path[0] + "</a></td>"));
                    PlaceHolderOutput.Controls.Add(new LiteralControl("<td>" + path[1] + "</td>"));
                    PlaceHolderOutput.Controls.Add(new LiteralControl("</tr>"));
                    count++;
                }

                PlaceHolderOutput.Controls.Add(new LiteralControl("</table>"));
            }
            catch (Exception) { };
        }

        async Task InvokeBatchExecutionService(string url, string api, string method = "get")
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", api);

                    Task<string> response = null;
                    if (method.ToLower() == "delete")
                    {
                        await client.DeleteAsync(url);
                        //Response.Redirect(Request.RawUrl);
                        lblStatus.Text = "Canceling .....";     
                    }

                    else response = client.GetStringAsync(url);                    

                    //lblStatus.Text = response.Status.ToString();

                    if (response.Result == null)
                    {
                        Response.AppendHeader("Refresh", "10");
                        return;
                        
                    }

                    var re = ReadResponse(response.Result);
                    lblStatus.Text = re.Status;

                    if (re.Status == "NotStarted")//|| re.Status == "Running")
                    {
                        lblStatus.CssClass = "btn-default disabled";
                        Response.AppendHeader("Refresh", "10");
                    }

                    else if (re.Status == "Running")
                    {
                        lblStatus.CssClass = "btn-primary disabled";
                        Response.AppendHeader("Refresh", "10");
                    }

                    else if (re.Status == "Failed")
                    {
                        lblStatus.CssClass = "btn-danger disabled";
                        btnCancelJob.Visible = false;
                    }

                    else if (re.Status == "Cancelled")
                    {
                        lblStatus.CssClass = " btn-warning disabled";
                        btnCancelJob.Visible = false;
                    }
                    else if (re.Status == "Finished")
                    {
                        lblStatus.CssClass = "btn-success disabled";
                        btnCancelJob.Visible = false;
                        CreateOutput(re);
                    }
                    else
                    {
                        Response.AppendHeader("Refresh", "10");
                    }

                    if (!string.IsNullOrEmpty(re.Details))
                        PlaceHolderOutput.Controls.Add((new LiteralControl(re.Details)));

                }
            }
            catch (Exception ex)
            {
                //lblStatus.Text = ex.Message;
                //Response.AppendHeader("Refresh", "20");
            }
        }

        private void RequireInfor()
        {
            Response.Redirect("Setting.aspx");
        }

        private void CreateOutput(Response r)
        {
            if (r.lOutput == null || r.lOutput.Count == 0)
                return;
            PlaceHolderOutput.Controls.Add(new LiteralControl("<h3>List of Outputs</h3>"));
            PlaceHolderOutput.Controls.Add(new LiteralControl("<table class=\"table  table-bordered table-hover\">"));
            PlaceHolderOutput.Controls.Add(new LiteralControl("<tr><th>Name</th><th >Relative Location</th><th style=\"width: 10%;\"></th></tr>"));
            foreach (res_output output in r.lOutput)
            {
                PlaceHolderOutput.Controls.Add(new LiteralControl("<tr>"));

                PlaceHolderOutput.Controls.Add(new LiteralControl("<td>" + output.Name + "</td>"));
                PlaceHolderOutput.Controls.Add(new LiteralControl("<td>" + output.RelativeLocation + "</td>"));
                PlaceHolderOutput.Controls.Add(new LiteralControl("<td><a href=\"" + output.BaseLocation + output.RelativeLocation + "\" class=\"btn btn-primary btnSubmit\">Download</a></td>"));
                PlaceHolderOutput.Controls.Add(new LiteralControl("</tr>"));
            }

            PlaceHolderOutput.Controls.Add(new LiteralControl("</table>"));
        }

        static Response ReadResponse(string json)
        {
            var objects = JObject.Parse(json);

            Response r = new Response();
            r.Status = objects.SelectToken("StatusCode").ToString();

            if (!string.IsNullOrEmpty(objects["Details"].ToString()))
                r.Details = objects["Details"].ToString();

            if (string.IsNullOrEmpty(objects["Results"].ToString()))
                return r;

            var outputList = JObject.Parse(objects["Results"].ToString());

            foreach (var _output in outputList)
            {
                res_output rop = new res_output();
                rop.Name = _output.Key;
                rop.ConnectionString = _output.Value["ConnectionString"] != null ? _output.Value["ConnectionString"].ToString() : null;
                rop.RelativeLocation = _output.Value["RelativeLocation"] != null ? _output.Value["RelativeLocation"].ToString() : null;
                rop.BaseLocation = _output.Value["BaseLocation"] != null ? _output.Value["BaseLocation"].ToString() : null;
                rop.SasBlobToken = _output.Value["SasBlobToken"] != null ? _output.Value["SasBlobToken"].ToString() : null;

                r.lOutput.Add(rop);
            }
            return r;
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string strListJobId = "";
            try
            {
                strListJobId = Request.Cookies["ListJobIds"].Value;
                //strListJobId = Request.Cookies[paramObj.APIKey].Value;
            }
            catch (Exception) { };
            List<string> listJobId;
            if (!string.IsNullOrEmpty(strListJobId))
                listJobId = strListJobId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            else listJobId = new List<string>();

            // Get list checked Id
            List<string> idWillRemoved = new List<string>();
            foreach (string jobId in listJobId)
            {
                string[] path = jobId.Split(new string[] { "||" }, StringSplitOptions.None);
                var control = FindControl(path[0]);
                if(control != null)
                {
                    CheckBox cb = control as CheckBox;
                    if (cb.Checked)
                        idWillRemoved.Add(jobId);
                }
            }

            // Delete list checked Id
            listJobId.RemoveAll(x => idWillRemoved.Contains(x));
            string strListJobId1 = "";
            if(listJobId.Count > 0)
                strListJobId1 = listJobId.Aggregate((a, b) => a = a + "," + b);

            Response.Cookies["ListJobIds"].Value = strListJobId1;
            Response.Cookies["ListJobIds"].Expires = DateTime.Now.AddDays(60);

            Response.Redirect("Status.aspx");
        }

        protected void btnCancelJob_Click(object sender, EventArgs e)
        {
            string BaseUrl = paramObj.Url.Replace("jobs", "jobs/" + jobid);
            InvokeBatchExecutionService(BaseUrl, Crypto.DecryptStringAES(paramObj.APIKey), "delete").Wait();
        }

    }
}