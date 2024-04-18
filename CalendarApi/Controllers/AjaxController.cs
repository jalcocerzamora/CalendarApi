using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CalendarApi.Controllers
{
    [ApiController]
    [Route("cdn")]
    public class AjaxController : ControllerBase
    {
        string pathFile = Path.Combine("seo", "SEO.xml");

        [EnableCors]
        [HttpGet("seoxml")]
        public string SeoXml()
        {
            
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(pathFile);

            string json = JsonConvert.SerializeXmlNode(xmlDocument);
            return json;
        }

        [EnableCors]
        [HttpPost("guardarjson")]
        public string GuardarJson([FromBody] object jsonData)
        {
            try
            {
                XNode xmlData = JsonConvert.DeserializeXNode(jsonData.ToString());

                using (var writer = new StreamWriter(pathFile, false, Encoding.UTF8))
                {
                    writer.Write(xmlData.ToString());
                }

                return xmlData.ToString();
            }
            catch (Exception ex)
            {
                return $"No save: {ex.Message}";
            }
        }
    }
}
