using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

namespace BNFParser
{
    class XMLFormatter
    {
        public static void createXML(List<string> matches, Token finalToken, string outputFile)
        {
            //XmlWriterSettings settings = new XmlWriterSettings();
            //settings.Indent = true;
            //XmlWriter writer = XmlWriter.Create(outputFile, settings);
            StreamWriter writer = new StreamWriter(outputFile);
            writer.WriteLine("<root>");
            foreach (string match in matches)
                xmlRecursion(match, finalToken, writer);
            //writer.WriteEndElement();
            writer.WriteLine("</root>");
            writer.Close();
        }
        public static void xmlRecursion(string match, Token token, /*XmlWriter writer*/StreamWriter writer)
        {
            match = match.Trim(); // sklanjam whitespace-ove
            if (!token.Name.Equals(""))
                writer.WriteLine("<" + token.Name + ">");
              //writer.WriteStartElement(token.Name);
            while(token.subTokenCollection!=null)
               {
                bool breakCondition = false; // ako match-uje subTokenCollection, nece provjeravati dalje
               
                for(int i=0;i<token.subTokenCollection.Count;i++)
                {
                    if (breakCondition)
                        break; 
                    List<Token> tempTokens = new List<Token>(token.subTokenCollection[i]);
                    string tempString = "";
                    foreach (var item in tempTokens)
                        tempString = tempString + item.Description.ToString();
                    if (tempString.EndsWith("|"))
                        tempString = tempString.Remove(tempString.Length - 1, 1);
                    Regex regex = new Regex(tempString, RegexOptions.Compiled); // regex trenutnog subTokenCollection
                    if (regex.IsMatch(match))
                    {
                        breakCondition = true;
                        for(int j=0;j<tempTokens.Count;j++)
                        {
                            string subMatch = tempTokens[j].Description.Match(match).ToString(); // izdvajam dio matcha koji odgovara trenutnom tokenu
                            match = match.Remove(0, subMatch.Length);
                            xmlRecursion(subMatch, tempTokens[j], writer);
                        }
                    }
                    if (match.Equals("")) //uslov izlaska iz rekurzije
                    {
                        writer.WriteLine("</" + token.Name + ">");
                        //writer.WriteEndElement();
                        return;
                    }
                }
            }
            writer.WriteLine(match);
           // writer.WriteString(match.Trim());
            if (token.subTokenCollection == null && !token.Name.Equals(""))
                //writer.WriteEndElement();
               writer.WriteLine("</" + token.Name + ">");
        }
    }
}