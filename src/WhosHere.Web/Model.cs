using System.Collections.Generic;

namespace WhosHere.Web
{
    public class Employee
    {
        public string Mail { get; set; }
        public bool IsSelected { get; set; }
        public string Class => IsSelected ? "avatar float" : "avatar start-position";
        public string ImageUrl {get;set;}
    }
}

namespace WhosHere.Web
{
    public class AnalyzeMessage
    {
        public AnalyzeResult Result { get; set; }

    }
}

namespace WhosHere.Web
{
     public class AnalyzeResult
    {
        public int NrFound { get; set; }
        public List<string> Users { get; set; }
    }
}