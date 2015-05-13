using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Query
{
    public class Miner
    {
        public string run(string content)
        {
            var request = JsonConvert.DeserializeObject<Query_Request>(content);

            var response = new Query_Response { objects = new List<object>() };
            var item = new Dictionary<string, object>();
            item["id"] = 10;
            item["name"] = "cat";
            response.objects.Add(item);
            return JsonConvert.SerializeObject(response, Formatting.Indented);
        }
    }
}
