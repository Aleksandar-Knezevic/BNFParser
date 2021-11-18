using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BNFParser
{
    static class BNFReader
    {
        public static Regex leafToken = new Regex(@"(<\w+>)\s::=\s("".*?""\s?\|?\s?)+$", RegexOptions.Compiled); // provjera da li je sve validno, pa onda pomocu separatora izdvojiti
        //public static Regex Token = new Regex(@"(<\w+>)\s::=\s(<\w+>\s?|"".*?""\s?|\|\s?)+$", RegexOptions.Compiled); // -||-
        public static Regex phone_number = new Regex(@"((\+\d{1,3})|(0)|(\+\d\-\d{3}))\s?\d{2}(\s|\/)?\d{3}(\s|\-)?\d{3,4}\s*", RegexOptions.Compiled);
        public static Regex email_address = new Regex(@"[\w!#$%&'*+\-\/=?\^`{|}~]+\.?[\w!#$%&'*+\-\/=?^`{|}~]*@([a-zA-Z]+\.)+\w{2,4}", RegexOptions.Compiled);
        public static Regex web_link = new Regex(@"(((https?|ftp):\/\/)(((www)|(en)|(sr))\.)[a-z0-9-]+\.[a-z]+(\/?\.?[a-zA-Z0-9#-]+\/?\.?)*)|(((https?|ftp):\/\/)(?!www\.)[a-z0-9-]+\.[a-z]+(\/[a-zA-Z0-9#-]+\/?\.?)*)", RegexOptions.Compiled);
        public static Regex number_constant = new Regex(@"[+-]?(\d|,{1})+(\.\d)?\d*e?\d*", RegexOptions.Compiled);
        public static Regex standard_expression = new Regex(@"(<\w+>)\s?::=\s?(broj_telefona)|(mejl_adresa)|(web_link)|(brojevna_konstanta)|(veliki_grad)", RegexOptions.Compiled);
        public static Regex big_city = new Regex(CityCrawler.getCityRegex(), RegexOptions.Compiled);
        public static Regex regex_expression = new Regex(@"<(.*?)>\s?::=\s?regex\((.*?)\)\s", RegexOptions.Compiled);
        public static Regex termSimb = new Regex(@"^\s?""(.*?)""\s?");
        public static Regex specialSimbols = new Regex(@"\/|\.|\*|\-");

        public static bool isLeaf(string row) // provjera da li je cvor "terminalni" (da nema vise tokena)
        {
            return leafToken.IsMatch(row) || standard_expression.IsMatch(row) || regex_expression.IsMatch(row);
        }

        public static string[] separate(string allData) // rastavljanje u redove
        {
            string[] codeRows = allData.Split('\n');
            return codeRows;
        }

        public static List<Token> createLeafTokens(List<string> leafTokens) // kreiranje terminalnog cvora od stringa
        {
            /*if (leafTokens == null)
            {
                throw new ArgumentNullException(nameof(leafTokens));
            } */

            List<Token> tokens = new List<Token>();
            for (int i = 0; i < leafTokens.Count; i++)
            {
                if(standard_expression.IsMatch(leafTokens[i])) // provjera da li je standardni izraz
                {
                    string regexString = "(";
                    string standardName = leafTokens[i].Split('=', ':')[3].Trim();
                    string standardMeaning = leafTokens[i].Split('<', '>')[1].Trim();
                    if (standardName.Equals("broj_telefona"))
                         regexString = regexString + phone_number.ToString() + ")";
                    else if (standardName.Equals("mejl_adresa"))
                         regexString = regexString + email_address.ToString() + ")";
                    else if (standardName.Equals("web_link"))
                         regexString = regexString + web_link.ToString() + ")";
                    else if (standardName.Equals("brojevna_konstanta"))
                         regexString = regexString + number_constant.ToString() + ")";
                    else if (standardName.Equals("veliki_grad"))
                         regexString = regexString + big_city.ToString() + ")";
                    tokens.Add(new Token(standardMeaning, new Regex(regexString)));
                    continue;
                }
               else if (regex_expression.IsMatch(leafTokens[i])) // provjera da li je oblik <a>::=regex( )
                {
                    tokens.Add(new Token(regex_expression.Match(leafTokens[i]).Groups[1].Value, new Regex("(" + regex_expression.Match(leafTokens[i]).Groups[2].Value + ")")));
                    continue;
                }
                string[] allParts = leafTokens[i].Split('"'); // kreiranje terminalnog tokena

                for (int j = 0; j < allParts.Length; j++)
                {
                    if (allParts[j].Equals(" | ")/* || allParts[j].Equals("|")*/)
                        allParts[j].Trim();
                }

                string regex = "(";
                for (int k = 1; k < allParts.Length - 1; k++)
                {
                    if (allParts[k].Length>1)
                        allParts[k] = "(" + allParts[k] + ")";
                    regex = regex.Insert(regex.Length, allParts[k]);
                }
                regex = regex.Insert(regex.Length, ")");
                string[] namePart = allParts[0].Split('<', '>');
                tokens.Add(new Token(namePart[1], new Regex(regex)));
            }
            return tokens;
        }
        public static Token createToken(string Name, string regex, List<List<Token>> subTokenCollection=null) // kreiranje tokena
        {
            return new Token(Name, new Regex(regex, RegexOptions.Compiled), subTokenCollection);
        }

        /*public static string getString(string part)
        {
            string tempstr;
            if (specialSimbols.IsMatch(termSimb.Match(part).Groups[1].Value))
                tempstr = "(\\" + termSimb.Match(part).Groups[1].Value + ")|"; // ovdje je bilo bez value
            else
                tempstr = "(" + termSimb.Match(part).Groups[1].Value + ")|";
            return tempstr;
        } */


        public static int count(string str) // broj '<' za sortiranje subTokenCollection
        {
            int n = 0;
            foreach(char chr in str)
                if (chr == '<')
                    n++;
            return n;
        }

        public static string[] sortByCharacter(string[] toBeSorted)
        {
            for (int p = 0; p < toBeSorted.Length; p++) // sortiram po broju '<' da prvo bude regex sa vise komponenti
            {
                for (int q = 0; q < toBeSorted.Length - 1 - p; q++)
                {
                    if (count(toBeSorted[q + 1]) > count(toBeSorted[q]))
                    {
                        string pom = toBeSorted[q];
                        toBeSorted[q] = toBeSorted[q + 1];
                        toBeSorted[q + 1] = pom;
                    }
                }
            }
            return toBeSorted;
        }
        public static List<Token> createTokens(List<string> producingTokens, List<Token> allTokens) // kreiranje cvorova
        {
            
            string name = null;
            for (int i = producingTokens.Count - 1; i >= 0; i--)
            {
                string[] parts = producingTokens[i].Split('=', '|');
                name = parts[0].Split('<', '>')[1];
                string regex = "";
                List<Token> subTokens = new List<Token>();
                string[] subParts = new string[parts.Length - 1];
                for (int m = 1; m < parts.Length; m++)
                    subParts[m - 1] = parts[m];
                

                subParts = sortByCharacter(subParts);
               
                for(int m=0;m<subParts.Length;m++)
                    parts[m + 1] = subParts[m];
                
                List<List<Token>> subTokensCollection = new List<List<Token>>();
                for (int j = 1; j < parts.Length; j++)
                {
                    if (termSimb.IsMatch(parts[j])) // provjera da li je terminalni simbol
                    {
                        string tempstr;
                        if (specialSimbols.IsMatch(termSimb.Match(parts[j]).Groups[1].Value)) // provjera da li je specijalni simbol koji treba escape-ovati
                            tempstr = "(\\" + termSimb.Match(parts[j]).Groups[1].Value + ")|";
                        else
                            tempstr = "(" + termSimb.Match(parts[j]).Groups[1].Value + ")|"; 
                        regex = regex.Insert(regex.Length, tempstr);
                        subTokens.Add(createToken("", tempstr));
                        subTokensCollection.Add(new List<Token>(subTokens)); // pravim SubTokenCollection-e zbog rastavljanja u XML-u
                        subTokens.Clear();
                    }
                    else
                    {

                        string[] tokens = parts[j].Split('<', '>');
                        int orCondition = 1; // zbog upisa | u rekurziji
                        for (int k = 0; k <tokens.Length; k++)
                        {
                            
                            for (int z = 0; z < allTokens.Count; z++)
                            {
                                if (tokens[k].Equals(allTokens[z].Name)) // provjera da li token vec postoji u listi tokena
                                {
                                    regex = regex.Insert(regex.Length, allTokens[z].Description.ToString());
                                    if (tokens[k+1].Equals(" ") && (k+1) < tokens.Length - 1) // && k+1 > 0 // provjera na razmak
                                    {
                                        regex = regex.Insert(regex.Length, "(\\s)");
                                        allTokens[z] = new Token(allTokens[z].Name, new Regex(allTokens[z].Description.ToString() + "(\\s?)", RegexOptions.Compiled), allTokens[z].subTokenCollection); // dodajem razmak zbog match-ovanja
                                    }
                                    subTokens.Add(allTokens[z]);
                                }
                                if (tokens[k].Equals(name))
                                {
                                    regex = regex.Insert(regex.Length, "*");
                                    orCondition = 0;
                                    z++;
                                }
                                
                            }
                            
                            if (termSimb.IsMatch(tokens[k])) // npr "/" u broju indeksa
                            {
                                string tempstr;
                                if (specialSimbols.IsMatch(termSimb.Match(tokens[k]).Groups[1].Value))
                                    tempstr = "(\\" + termSimb.Match(tokens[k]).Groups[1].Value + ")"; // ovdje je bilo bez value
                                else
                                    tempstr = "(" + termSimb.Match(tokens[k]).Groups[1].Value + ")"; 
                                regex = regex.Insert(regex.Length, tempstr);
                                subTokens.Add(createToken("constant", tempstr));
                            }
                        }
                        if (orCondition == 1)
                        {
                            regex = regex.Insert(regex.Length, "|");
                            orCondition = 0;
                        }
                        subTokensCollection.Add(new List<Token>(subTokens));
                        subTokens.Clear();
                    }
                }
                
                regex = regex.Remove(regex.LastIndexOf('|'), 1);
                allTokens.Add(new Token(name, new Regex("(" + regex + ")"), subTokensCollection));
            }
            return allTokens;
        }
    }
}
