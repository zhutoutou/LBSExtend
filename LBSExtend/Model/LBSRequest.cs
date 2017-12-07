using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZIT.LBSExtend.Model
{
    public class LBSRequest
    {
        private string _lsh;

        private string _phone;

        private string _name;

        public string LSH { get => _lsh; set => _lsh = value; }
        public string Phone { get => _phone; set => _phone = value; }
        public string Name { get => _name; set => _name = value; }
    }
}
