using ChoETL;
using System;
using System.IO;

namespace ZookJson
{
    internal class Util
    {
        public static void JsonToCSV(string jsonContent, string fileOut, Action? onFinish = null)
        {
            StringReader stringReader= new StringReader(jsonContent);
            using (var r = new ChoJSONReader(stringReader))
            {
                using (var w = new ChoCSVWriter(fileOut).WithFirstLineHeader())
                {
                    w.Write(r);
                }
            }
            onFinish?.Invoke();
        }
        public static void FileJsonToCSV(string jsonFile, string fileOut, Action? onFinish = null)
        {
            using (var r = new ChoJSONReader(jsonFile))
            {
                using (var w = new ChoCSVWriter(fileOut).WithFirstLineHeader())
                {
                    w.Write(r);
                }
            }
            onFinish?.Invoke();
        }
    }
}
