using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDS_AdapterTHSSStub.Models
{
    public class TestSubmitApiResultModel
    {
        public List<TestSubmitApiResultFileModel> Files = new List<TestSubmitApiResultFileModel>();
    }

    public class TestSubmitApiResultFileModel
    {
        public string FileName { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}