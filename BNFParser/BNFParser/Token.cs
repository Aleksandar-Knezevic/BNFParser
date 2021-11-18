using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace BNFParser
{
    class Token
    {
        public string Name { get; private set; }
        public Regex Description { get; private set; }
        //public List<Token> subTokens {get; set;}
        public List<List<Token>> subTokenCollection { get; set; }
        public Token(string Name, Regex desc, List<List<Token>> subColl = null)
        {
            this.Name = Name;
            Description = desc;
            subTokenCollection = subColl;
            //subTokens = subs;
        }
        
    }
}
