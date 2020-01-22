using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestFulRequest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DS= RestFulRequest.DefineListService;

namespace RestFulRequest
{
    public class ServerCenter
    {
        private readonly Form1 form;
        public ServerCenter(Form1 form)
        {
            this.form = form;
        }
        public void Run()
        {
            HttpListener server = new HttpListener();
            try
            {
                //finish
                server = new HttpListener
                {
                    //監聽90port
                    Prefixes = { "http://127.0.0.1:90/" },
                };
                server.Start();
            }
            catch (Exception exx)
            {
                MessageBox.Show(exx.Message);
            }
            while (true)
            {
                var context = server.GetContext();
                HttpListenerRequest request;
                request = context.Request;
                System.IO.Stream body = request.InputStream;
                System.Text.Encoding encoding = request.ContentEncoding;
                System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                HttpListenerResponse response = context.Response;
                string s = reader.ReadToEnd();
                try
                {
                    //有帶data
                    JObject  jsonObj = new JObject();

                    //GET or POST orPUSH....
                    string mdethod = context.Request.HttpMethod;

                    //http://127.0.0.1:90/methodName
                    string methodName = context.Request.Url.Segments[1].Replace("/", "");

                    //搜尋mapping Design裡面是否存在同樣method跟名稱
                    var method = Type.GetType("RestFulRequest.DefineListService").GetMethods()
                        .Where(mi => mi.GetCustomAttributes(true)
                        .Any(attr => attr is DS.Mapping 
                        &&((DS.Mapping)attr).FunctionName== methodName && ((DS.Mapping)attr).Method == mdethod)).First();
                    if (s!="")
                    {
                        
                            jsonObj = JObject.Parse(s);
                    }
                    else
                    {
                        //restful參數 方法後的參數 Ex:OpenCom/0
                        string strParams = context.Request.Url
                                           .Segments
                                           .Skip(2)
                                           .Select(k => k.Replace("/", "")).FirstOrDefault();

                        object PackageNameAndData = new { FunctionName = methodName,data= strParams };
                        jsonObj = JObject.FromObject(PackageNameAndData);
                    }
                    string result = "";
                    
                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        //帶入restful的參數

                        DS ds = new DS(form);
                        result = ds.InvokeRestful(methodName, mdethod, jsonObj);
                        string retstr = JsonConvert.SerializeObject(result);
                        Console.WriteLine("result : " + (string)result);
                        StringBuilder builder = new StringBuilder((string)result);
                        string something = builder.ToString();
                        byte[] buffer = Encoding.UTF8.GetBytes(something);
                        response.ContentLength64 = buffer.Length;
                        response.ContentType = "text/html";
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");
                        response.Headers.Add("Access-Control-Allow-Headers", "Origin, Content-Type, X-Auth-Token");
                        response.StatusDescription = "OK";
                        Stream st = response.OutputStream;
                        st.Write(buffer, 0, buffer.Length);
                        context.Response.Close();
                    });
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                    MessageBox.Show(e.Message);
                }
            }
        }
    }
}
