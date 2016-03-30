using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AzureMLInterface.Model;
using AzureMLInterface.Controlers;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Net;
using System.Net.Security;
using Newtonsoft.Json.Linq;
using ParameterIO;


using Microsoft.Azure.MachineLearning;
using Microsoft.Azure.MachineLearning.Contracts;
using Microsoft.Azure.MachineLearning.Exceptions;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Collections;
using CryptoLibrary;

namespace AzureML_BES_Web_Template
{
    public partial class Default : System.Web.UI.Page
    {
        AMLParameterObject paramObj = new AMLParameterObject();
        string webServicePostUrl;
        bool isHasInput;
        bool isHasOutput;
        int maxJobId = 25;
        protected void Page_Load(object sender, EventArgs e)
        {

           //Response.AddHeader("Keep-Alive", "21600");

            System.Web.HttpBrowserCapabilities browser = Request.Browser;
            if (paramObj.ImportInputParameter(Server.MapPath("~\\Resources\\AMLParameter.xml")))
            {
                Page.Title = paramObj.Title;
                lblTitle.Text = paramObj.Title;                
                webServicePostUrl = paramObj.Url;

                isHasInput = paramObj.listBatchInputs != null && paramObj.listBatchInputs.Count > 0;
                isHasOutput = paramObj.listBatchOutputs != null && paramObj.listBatchOutputs.Count > 0;
            }
            else RequireInfor();

            if(paramObj.listGlobalParameter != null && paramObj.listGlobalParameter.Count >0)
                // Show Global parameter
                GenerateControl.ShowInput(GlobalPlaceHoder, null, paramObj.listGlobalParameter, browser);

            if (!isHasInput && !isHasOutput)
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "Hide", "document.getElementById(\"both\").style.display = \"none\";", true);
            else if (!isHasInput)                
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "Hide", "document.getElementById(\"Input_FIle_Info_fieldset\").style.display = \"none\";", true);
            else if (!isHasOutput)                
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "Hide", "document.getElementById(\"azure_Storage_Info_fieldset\").style.display = \"none\";", true);


            
        }

        private void RequireInfor()
        {
            Response.Redirect("Setting.aspx");
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {

                //Dictionary<string, string> featureList = getColumnsAndValues();

                //if (featureList == null || featureList.Count == 0) return;

                // Blob contain input file. Get from Input Information
                string Account_Name = txtAccountName_blob.Text;
                string Account_Key = txtAccountKey_blob.Text;
                string container = txtContainer_blob.Text;
                string inputBlobName = txtBlobName.Text;

                // Blob store output. Get from Azure Blob information
                OutputObject output = new OutputObject
                {
                    AccountName = txtAccountName.Text,
                    AccountKey = txtAccountKey.Text,
                    Container = txtContainer.Text,
                    isAddTime = false,
                    NodeOutputName = new Dictionary<string, string>()
                };
                foreach (var batchOutput in paramObj.listBatchOutputs)
                {
                    output.NodeOutputName.Add(batchOutput.Name, string.IsNullOrEmpty(batchOutput.Alias) ? batchOutput.Name : batchOutput.Alias);
                }
                string filePath = _fileUpload.FileName;
                if (!input_radio_File.Checked) filePath = null;
                InvokeBatchExecutionService(Account_Name, Account_Key, container, inputBlobName, filePath, output).Wait();
            }
            catch (Exception ex)
            {
                //txtresultModal.Text = ex.Message;
                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "failModal", "$('#failModal').modal();", true);
                ShowError(ex.Message);
            }
        }


        //Check and adjust URL version if necessary

        async Task<bool> uploadBigFile(string accountNameInput, string accountKeyInput, string containerInput, string blobname)
        {

            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountNameInput, accountKeyInput));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerInput);


            CloudBlockBlob blob = container.GetBlockBlobReference(Path.GetFileName(blobname));

            int blockSize = 256 * 1024; //256 kb

            using (Stream fileStream = _fileUpload.FileContent)
            {
                long fileSize = fileStream.Length;

                //block count is the number of blocks + 1 for the last one
                int blockCount = (int)((float)fileSize / (float)blockSize) + 1;

                //List of block ids; the blocks will be committed in the order of this list 
                List<string> blockIDs = new List<string>();

                //starting block number - 1
                int blockNumber = 0;

                try
                {
                    int bytesRead = 0; //number of bytes read so far
                    long bytesLeft = fileSize; //number of bytes left to read and upload

                    //do until all of the bytes are uploaded
                    while (bytesLeft > 0)
                    {
                        blockNumber++;
                        int bytesToRead;
                        if (bytesLeft >= blockSize)
                        {
                            //more than one block left, so put up another whole block
                            bytesToRead = blockSize;
                        }
                        else
                        {
                            //less than one block left, read the rest of it
                            bytesToRead = (int)bytesLeft;
                        }

                        //create a blockID from the block number, add it to the block ID list
                        //the block ID is a base64 string
                        string blockId =
                          Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("BlockId{0}",
                            blockNumber.ToString("0000000"))));
                        blockIDs.Add(blockId);
                        //set up new buffer with the right size, and read that many bytes into it 
                        byte[] bytes = new byte[bytesToRead];
                        fileStream.Read(bytes, 0, bytesToRead);

                        //calculate the MD5 hash of the byte array
                        string blockHash = GetMD5HashFromStream(bytes);

                        //upload the block, provide the hash so Azure can verify it
                        blob.PutBlock(blockId, new MemoryStream(bytes), blockHash);

                        //increment/decrement counters
                        bytesRead += bytesToRead;
                        bytesLeft -= bytesToRead;
                    }

                    //commit the blocks
                    blob.PutBlockList(blockIDs);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public static string GetMD5HashFromStream(byte[] stream)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }

        public void AddJobIdCookie(string jobId, string startLocalTime)
        {
            try
            {
                string strListJobId = "";
                try
                {
                    strListJobId = Request.Cookies["ListJobIds"].Value;
                }
                catch (Exception) { };

                List<string> listJobId;
                if (!string.IsNullOrEmpty(strListJobId))
                    listJobId = strListJobId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                else listJobId = new List<string>();
                listJobId.Add(jobId + "||" + startLocalTime + "||" + paramObj.APIKey);
                if (listJobId.Count >= maxJobId) listJobId.RemoveAt(0);
                string strListJobId1 = listJobId.Aggregate((a, b) => a = a + "," + b);

                Response.Cookies["ListJobIds"].Value = strListJobId1;
                Response.Cookies["ListJobIds"].Expires = DateTime.Now.AddDays(60);
            }
            catch (Exception) { };
        }

        public void ShowError(string errorMessage)
        {
            txtresultModal.Text = errorMessage;
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "failModal", "$('#failModal').modal();", true);
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "enable", "$(\"#btnclear\").prop(\"disabled\",false);", true);
        }

        async Task InvokeBatchExecutionService(string accountNameInput, string accountKeyInput, string containerInput, string inputBlobName, string inputFileName, OutputObject outputObj)
        {
            // Upload file to Blob if pathFile given

            if (isHasInput && input_radio_File.Checked)
            {
                accountNameInput = outputObj.AccountName;
                if (string.IsNullOrEmpty(accountNameInput))
                {
                    //Console.WriteLine("Upload Error");
                    ShowError("Please enter Account Name");
                    return;
                }
                accountKeyInput = outputObj.AccountKey;
                if (string.IsNullOrEmpty(accountKeyInput))
                {
                    //Console.WriteLine("Upload Error");
                    ShowError("Please enter Account Key");
                    return;
                }
                containerInput = outputObj.Container;
                if (string.IsNullOrEmpty(containerInput))
                {
                    //Console.WriteLine("Upload Error");
                    ShowError("Please enter Container Name");
                    return;
                }
                

                if (string.IsNullOrEmpty(_fileUpload.FileName))
                {
                    //Console.WriteLine("Upload Error");
                    ShowError("Please choose input file");
                    return;
                }

                inputBlobName = inputFileName;

                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "disable", "$(\"#btnclear\").prop(\"disabled\",true);", true);

                bool uploadresult = await uploadBigFile(accountNameInput, accountKeyInput, containerInput, inputFileName);
                //$("#btnclear").prop("disabled",true)
                
                if (!uploadresult)
                {
                    //Console.WriteLine("Upload Error");
                    ShowError("Upload Error");
                    return;
                }
                
            }

            // First collect and fill in the URI and access key for your web service endpoint.
            // These are available on your service's API help page.
            var endpointUri = paramObj.Url;
            string accessKey = Crypto.DecryptStringAES(paramObj.APIKey);

            // Create an Azure Machine Learning runtime client for this endpoint
            var runtimeClient = new RuntimeClient(endpointUri, accessKey);

            // Define the request information for your batch job. This information can contain:
            // -- A reference to the AzureBlob containing the input for your job run
            // -- A set of values for global parameters defined as part of your experiment and service
            // -- A set of output blob locations that allow you to redirect the job's results

            // NOTE: This sample is applicable, as is, for a service with explicit input port and
            // potential global parameters. Also, we choose to also demo how you could override the
            // location of one of the output blobs that could be generated by your service. You might 
            // need to tweak these features to adjust the sample to your service.
            //
            // All of these properties of a BatchJobRequest shown below can be optional, depending on
            // your service, so it is not required to specify all with any request.  If you do not want to
            // use any of the parameters, a null value should be passed in its place.

            // Define the reference to the blob containing your input data. You can refer to this blob by its
            // connection string / container / blob name values; alternatively, we also support references 
            // based on a blob SAS URI

            string ext = ".csv";//inputBlobName.Substring(inputBlobName.LastIndexOf("."));

            BlobReference inputBlob;
            if (isHasInput)
            {
                inputBlob = BlobReference.CreateFromConnectionStringData(
                connectionString: string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountNameInput, accountKeyInput),
                containerName: containerInput,
                blobName: inputBlobName);
                ext = inputBlobName.Substring(inputBlobName.LastIndexOf("."));
            }
            else inputBlob = null;


            
            // If desired, one can override the location where the job outputs are to be stored, by passing in
            // the storage account details and name of the blob where we want the output to be redirected to.

            var outputLocations = new Dictionary<string, BlobReference>();
           
                foreach (var keyvalue in outputObj.NodeOutputName)
                {
                    outputLocations.Add(

                        keyvalue.Key,
                        BlobReference.CreateFromConnectionStringData(
                            connectionString: string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", outputObj.AccountName, outputObj.AccountKey),
                            containerName: outputObj.Container,
                            blobName: !outputObj.isAddTime ? keyvalue.Value + "_" + DateTime.Now.ToString("MMddyy_hhmmss") + ext : keyvalue.Value + ext
                    ));

                };
          

            // If applicable, you can also set the global parameters for your service
            var globalParameters = new Dictionary<string, string>();
            foreach(var global in paramObj.listGlobalParameter)
            {
                
                string columnValue = "";
                var control = FindControl(global.Name);
                if (control is TextBox)
                {
                    TextBox txt = control as TextBox;
                    if (txt.Text != "")
                        columnValue = txt.Text;
                }
                else if (control is DropDownList)
                {
                    DropDownList lb = control as DropDownList;
                    if (lb.SelectedIndex != -1)
                        columnValue = lb.SelectedValue;
                }
                if (control is RadioButtonList)
                {
                    RadioButtonList ct = control as RadioButtonList;
                    if (ct.SelectedIndex != -1)
                        columnValue = ct.SelectedValue;
                }
                globalParameters.Add(global.Name, columnValue);
            }
            

            var jobRequest = new BatchJobRequest
            {
                Input = inputBlob,
                GlobalParameters = globalParameters,
                Outputs = outputLocations
            };

            try
            {
                // Register the batch job with the system, which will grant you access to a job object
                BatchJob job = await runtimeClient.RegisterBatchJobAsync(jobRequest);

                AddJobIdCookie(job.Id, job.CreatedAt.ToLocalTime().ToString());

                // Start the job to allow it to be scheduled in the running queue
                await job.StartAsync();

                //ScriptManager.RegisterStartupScript(Page, typeof(Page), "OpenWindow", "window.open('Status.aspx?jobid=" + job.Id + "');", true);
                

                Response.Redirect("Status.aspx?jobid=" + job.Id);

                // Wait for the job's completion and handle the output

                //BatchJobStatus jobStatus = await job.WaitForCompletionAsync();
                //while (job.CheckStatus().JobState != JobState.Finished && job.CheckStatus().JobState != JobState.Failed)
                //{
                //    Console.WriteLine(job.Id + ":" + job.CreatedAt.ToLocalTime() + job.CheckStatus().JobState);
                //}

                //BatchJobStatus jobStatus = job.CheckStatus();
                ////job.CreatedAt
                //if (jobStatus.JobState == JobState.Finished)
                //{
                //    // Process job outputs
                //    //Console.WriteLine(@"Job {0} has completed successfully and returned {1} outputs", job.Id, jobStatus.Results.Count);
                //    lblJobIdSuccess.Text = job.Id;
                //    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "CompleteSuccess", "$('#CompleteSuccess').modal();", true);

                //    foreach (var output in jobStatus.Results)
                //    {
                //        //Console.WriteLine(@"\t{0}: {1}", output.Key, output.Value.AbsoluteUri);
                //        Response.Redirect(output.Value.AbsoluteUri);

                //    }
                //}
                //else if (jobStatus.JobState == JobState.Failed)
                //{
                //    // Handle job failure
                //    //Console.WriteLine(@"Job {0} has failed with this error: {1}", job.Id, jobStatus.Details);
                //    txtresultModal.Text = jobStatus.Details;
                //    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "failModal", "$('#failModal').modal();", true);
                //}
            }
            catch (ArgumentException ex)
            {
                //Console.WriteLine("Argument {0} is invalid: {1}", aex.ParamName, aex.Message);
                //txtresultModal.Text = ex.Message;
                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "failModal", "$('#failModal').modal();", true);
                ShowError(ex.Message);
            }
            catch (RuntimeException runtimeError)
            {
                //Console.WriteLine("Runtime error occurred: {0} - {1}", runtimeError.ErrorCode, runtimeError.Message);
                //Console.WriteLine("Error details:");
                string error = "";
                foreach (var errorDetails in runtimeError.Details)
                {
                    error += string.Format("\t{0} - {1}", errorDetails.Code, errorDetails.Message);
                }
                //txtresultModal.Text = error;
                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "failModal", "$('#failModal').modal();", true);
                ShowError(error);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Unexpected error occurred: {0} - {1}", ex.GetType().Name, ex.Message);
                //txtresultModal.Text = ex.Message;
                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "failModal", "$('#failModal').modal();", true);
                ShowError(ex.Message);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "clearModal", "$('#clearModal').modal();", true);
        }

        protected void btnClearOK_Click(object sender, EventArgs e)
        {
            ClearCache();
        }

        private void ClearCache()
        {
            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "clearCookie", "deleteAllCookies();", true);     
            Response.Cookies["ListJobIds"].Value = "";
        }

        protected void btnStatus_Click(object sender, EventArgs e)
        {
            Response.Redirect("Status.aspx");
        }
    }
}