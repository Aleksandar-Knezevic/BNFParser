using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace BNFParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string BNFgrammar;
            string inData;
            using (StreamReader Bnf = new StreamReader("config.bnf"))
            {
                BNFgrammar = Bnf.ReadToEnd();
            }
           using (StreamReader data = new StreamReader(args[0]))
            {
                inData = data.ReadToEnd();
            }
            string[] codeRows = BNFReader.separate(BNFgrammar);
            string[] dataRows = BNFReader.separate(inData);
            List<string> leafStrings = new List<string>();
            List<string> producingStrings = new List<string>();
            

            foreach (string row in codeRows)
            {
                if (BNFReader.isLeaf(row))
                    leafStrings.Add(row);
                else
                    producingStrings.Add(row);
            }
            List<Token> termTokens = BNFReader.createLeafTokens(leafStrings);
            List<Token> allTokens = BNFReader.createTokens(producingStrings, termTokens);
            Regex parser = new Regex("^" + allTokens[allTokens.Count - 1].Description.ToString() + "\\s$" , RegexOptions.Compiled);
            for(int i=0;i<dataRows.Length-1;i++)
            {
                if (!parser.IsMatch(dataRows[i]))
                {
                    Console.WriteLine("Greska u " + (i + 1) + " liniji ulazne datoteke");
                    return;
                }
            }
            //var matched = parser.Matches(inData);
            List<string> matches = new List<string>();
            for(int i=0;i<dataRows.Length-1;i++)
                matches.Add(dataRows[i].ToString());
            XMLFormatter.createXML(matches, allTokens[allTokens.Count-1], args[1]);
        }
    }
}
