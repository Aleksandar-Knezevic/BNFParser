using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace BNFParser
{
    class CityCrawler
    {
        public static string getCityRegex()
        {
            string info = "";
            string regex = "(";
            //string url = "http://worldpopulationreview.com/continents/cities-in-europe/";
            Regex reduced = new Regex(@"<tbody>(.*)<\/tbody>", RegexOptions.Compiled);
            Regex cities = new Regex(@"<tr><td>(.*?)<\/td><td>");
          /* using (WebClient data = new WebClient())
              {
                 data.Encoding = Encoding.UTF8;
                 data.Headers.Add("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36");
                 info = data.DownloadString(url);
              } */
            using (StreamReader file = new StreamReader("cities.txt"))
            {
                info = file.ReadToEnd();
            }

            Match content = reduced.Match(info);
            MatchCollection matched = cities.Matches(content.Value);
            Console.OutputEncoding = Encoding.Unicode;
            for(int i=0;i<200;i++)
                regex = regex.Insert(regex.Length, "(" + matched[i].Groups[1].Value + ")|");
            regex = regex.Remove(regex.LastIndexOf('|'), 1);
            regex = regex.Insert(regex.Length, ")");
            return regex;
        }
    }
}
