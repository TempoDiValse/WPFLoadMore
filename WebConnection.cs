using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace WebConnect {
    public delegate void ResultDelegate(string text);

    public class WebConnection{
        private string url;
        private string method;
        private string queryString;
        private ResultDelegate callback;

	    public WebConnection(string _url){
            url = _url;
	    }
        public void setMethod(string _method) {
            method = _method;   
        }
        public void setURL(string _url) {
            url = _url;
        }

        public void setParameter(Dictionary<String, String> _param) {
            foreach(KeyValuePair<string, string> kv in _param) {
                queryString += String.Format("{0}={1}&",kv.Key, kv.Value);
            }

            queryString = queryString.Remove(queryString.Length-1);
        }
    
        public void load() {
            Uri uri;
            if(method == "POST") {
                uri = new Uri(url);
            } else {
                uri = new Uri(url + "?" + queryString);
            }

            var request = WebRequest.Create(uri);
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";

            var response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());

            var resText = reader.ReadToEnd();

            reader.Close();
            response.Close();

            callback(resText);
        }

        public void setCallback(ResultDelegate _callback){
            callback = _callback;
        }
    }
}
