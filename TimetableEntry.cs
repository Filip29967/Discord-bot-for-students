using System;
using System.Collections.Generic;
using System.Text;

namespace ProjektOI
{
    public class TimetableEntry
    {
        // pojedyncze zajecis
        public string Zajecia { get; set; };
        public DateTime PoczatekZajec { get; set; };
        public DateTime KoniecZajec { get; set; };
        public string Sala { get; set; };
        public string Budynek  { get; set; };


    }
}
