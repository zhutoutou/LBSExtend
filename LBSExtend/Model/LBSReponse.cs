using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZIT.LBSExtend.Model
{
    public class LBSResponse
    {
        private string _errorCode = "0";
        public string ErrorCode
        {
            get
            {
                return _errorCode;
            }
            set
            {
                _errorCode = value;
            }
        }

        private string _errorMsg = "";
        public string ErrorMsg
        {
            get
            {
                return _errorMsg;
            }
            set
            {
                _errorMsg = value;
            }
        }
        private string _lsh = "";
        public string LSH
        {
            get
            {
                return _lsh;
            }
            set
            {
                _lsh = value;
            }
        }
        private string _phone = "";
        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                _phone = value;
            }
        }
        private string _jd = "";
        public string JD
        {
            get
            {
                return _jd;
            }
            set
            {
                _jd = value;
            }
        }
        private string _wd = "";
        public string WD
        {
            get
            {
                return _wd;
            }
            set
            {
                _wd = value;
            }
        }
        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
    }
}
