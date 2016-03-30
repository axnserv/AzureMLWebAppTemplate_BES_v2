using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureML_BES_Web_Template.Model
{
    public class Response
    {
        public string Status;
        public List<res_output> lOutput = new List<res_output>();
        public string Details;
    }
}