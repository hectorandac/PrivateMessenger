using System;
using System.Collections.Generic;
using System.Text;

namespace pmessenger
{
    //Request structure
    class Request
    {
        public string request;
        public string userId = "";
        public string publicKey = "";
        public string message = "";
        public string recipientId = "";

        public Request(string request, string userId = "", string publicKey = "", string message = "", string recipientId = "")
        {
            this.request = request;
            this.userId = userId;
            this.publicKey = publicKey;
            this.message = message;
            this.recipientId = recipientId;
        }
    }
}
