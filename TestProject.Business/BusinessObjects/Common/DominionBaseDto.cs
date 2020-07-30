using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Business.BusinessObjects.Common
{
    public class TestProjectBaseDto
    {
        public string SourceURL { get; set; }
        public string Comments { get; set; }

        public System.DateTime DC { get; set; }
        public short UIDC { get; set; }
        public System.DateTime LU { get; set; }
        public short UILU { get; set; }
    }
}
