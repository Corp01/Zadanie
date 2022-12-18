using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadanie
{
    class Global
    {
        public static string strPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public string Site { get; set; }
        public string Name { get; set; }
        public int ID { get; set; }
    }
}
