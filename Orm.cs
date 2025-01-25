using HueTwitter;
using ModKit.ORM;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueTwitterOrm
{
    public class HueTwitterOrm : ModEntity<HueTwitterOrm>
    {
        [AutoIncrement][PrimaryKey] public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Jour { get; set; }
        public string Mois { get; set; }
        public int Année { get; set; }
        public bool IsDelete { get; set; }
    }
}
