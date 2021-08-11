using System;
using Tesseract;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OCRDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var filePath = "./test.png";
            var str = string.Empty;

            // Tesseract库
            Console.WriteLine("-----------------------Tesseract-----------------------");
            // 中文语言包
            TesseractEngine ocr = new TesseractEngine("./tessdata", "chi_sim", EngineMode.Default);

            // 本地图片
            Console.WriteLine("-----------------------本地图片加载-----------------------");
            using (var img = Pix.LoadFromFile(filePath))
            {
                using (Page page = ocr.Process(img))
                {
                    str = page.GetText();
                    var model = GetModelByOCRStr(str);
                    Console.WriteLine($"时间：{model.Date.ToString("yyyy-MM-dd HH:mm:ss")}");
                    Console.WriteLine($"单号：{model.OrderNo}");
                }
            }

            // 网络图片
            Console.WriteLine("-----------------------网络图片加载-----------------------");
            var client = new HttpClient();
            var result = await client.GetByteArrayAsync("https://static.ganbajing.com/20210811155358250488test.png"); 
            using (var img = Pix.LoadFromMemory(result))
            {
                using (Page page = ocr.Process(img))
                {
                    str = page.GetText();
                    var model = GetModelByOCRStr(str);
                    Console.WriteLine($"时间：{model.Date.ToString("yyyy-MM-dd HH:mm:ss")}");
                    Console.WriteLine($"单号：{model.OrderNo}");
                }
            }
            Console.WriteLine("-----------------------Tesseract-----------------------");
        }

        public static ORCModel GetModelByOCRStr(string str)
        {
            var res = new ORCModel();
            str = str.Replace(" ","");
            var list = str.Split('\n').ToList();
            res.OrderNo = list.FirstOrDefault(l => l.Contains("业务单号"))?.Replace("业务单号", "").Trim();
            var dateStr = list.FirstOrDefault(l => l.Contains("时间"))?
                .Replace("时间", "").Replace("~","").Replace("-","").Replace(":","").Trim();
            res.Date = DateTime.ParseExact(dateStr, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
            return res;
        }
    }

    public class ORCModel
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 单号
        /// </summary>
        public string OrderNo { get; set; }
    }
}
