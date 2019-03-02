using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBulkActivatorCLI
{
    class ResultObject
    {
        class KeyResponse
        {
            public string Key { get; set; }

            public string Response { get; set; }

            public bool Added { get; set; }
        }

        public bool Completed;

        private List<string> _cdKeyList;
        private List<KeyResponse> _cdKeyResponses = new List<KeyResponse>();

        private int _registerDelay;
        private int _registerDelayFull;

        public ResultObject(List<string> keys, int registerDelay)
        {
            _registerDelayFull = registerDelay;
            _registerDelay = registerDelay;
            _cdKeyList = keys;
        }

        public void AddResult(string result)
        {
            _registerDelay = _registerDelayFull;

            if (_cdKeyList.Count() < _cdKeyResponses.Count() + 1)
                return;

            _cdKeyResponses.Add(new KeyResponse()
            {
                Response = result,
                Added = false
            });
        }

        public string GetResults()
        {
            /*If we reached too many activation attempts then it will stop
             trying to register keys. We'll find those keys without a response
             in the original cdKeyList and add them to the save list with a custom
             message to make it easier for users to see which keys did not get activated.*/
            foreach (var key in _cdKeyList)
            {
                if (_cdKeyResponses.Any(o => o.Key == key))
                    continue;

                _cdKeyResponses.Add(new KeyResponse()
                {
                    Key = key,
                    Response = "Not attempted"
                });
            }

            /*We'll add all keys to a dictionary for easier formatting
             We start with the responses as keys, and keys as the value list*/
            var keyDic = new Dictionary<string, List<string>>();
            foreach (var response in _cdKeyResponses.GroupBy(o => o.Response).Select(o => o.First()))
                keyDic.Add(response.Response, new List<string>());

            /*Add all the keys to the right reponse type*/
            foreach (var key in _cdKeyResponses)
                keyDic[key.Response].Add(key.Key);

            /*Format the final string to write to file*/
            string finalStr = string.Empty;
            foreach (var resp in keyDic)
            {
                finalStr += resp.Key;
                foreach (var key in resp.Value)
                    finalStr += $"\n{key}";

                finalStr += "\n\n";
            }

            return finalStr;
        }
    }
}
