
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;

namespace RestFulRequest
{
    public class DefineListService
    {
        private readonly Form1 form;
        
        public class Mapping : Attribute
        {
            public string Method;
            public string FunctionName;
            public Mapping(string _Method,string _FunctionName)
            {
                this.Method = _Method;
                this.FunctionName = _FunctionName;
            }
        }
        public DefineListService(Form1 form)
        {
            this.form = form;
        }
        //用FunctionName和methodName決定要Invoke的method
        //*進入點*
        public string InvokeRestful(string FunctionName, string methodName, JObject data)
        {
            Type t = Type.GetType("RestFulRequest.DefineListService");
            //取得所有方法後，用mapping的屬性過濾
            var method = Type.GetType("RestFulRequest.DefineListService").GetMethods()
                       .Where(mi => mi.GetCustomAttributes(true)
                       .Any(attr => attr is Mapping
                       && ((Mapping)attr).FunctionName == FunctionName && ((Mapping)attr).Method == methodName)).First();

            return (string)method.Invoke(this, new object[] { FunctionName, data });
        }
        //GET
        //http://127.0.0.1:90/Test
        [Mapping("GET","Test")]
        public string TryReturnJSONString(string FunctionName, JObject data)
        {
            //FunctionName is not necessary
            FunctionName = MethodBase.GetCurrentMethod().Name;

            //Sample return File
            var result = JsonConvert.SerializeObject(new { success = true, data = "The Data You want to return" });

            return result;
        }
        [Mapping("GET", "TestwithData")]
        public string TryReturnJSONStringWithData(string FunctionName, JObject data)
        {
            //FunctionName is not necessary
            FunctionName = MethodBase.GetCurrentMethod().Name;

            //JSON format
            //{
            //    "param":"value"
            //}
            //Becareful that ifyou loss some JSON param,compiler will not pass 
            string Trydata =(string)data["param"];

            //Sample return File
            var result = JsonConvert.SerializeObject(new { success = true, data = Trydata });

            return result;
        }




    }
}
