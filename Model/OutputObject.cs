using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureMLInterface.Model
{
    public class OutputObject
    {
        string accountName;
        string accountKey;
        string container;

        public Dictionary<string, string> NodeOutputName = new Dictionary<string, string>();

        public bool isAddTime = false;

        public string Container
        {
            get { return container; }
            set { container = value; }
        }

        public string AccountKey
        {
            get { return accountKey; }
            set { accountKey = value; }
        }

        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }
    }
}