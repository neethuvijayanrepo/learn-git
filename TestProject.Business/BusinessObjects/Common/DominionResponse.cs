using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Utilities.Common;

namespace TestProject.Business.BusinessObjects.Common
{
    /// <summary>
    /// Business Response object.
    /// </summary>
    public class TestProjectResponse<TOutput>
    {
        public string Message { get; set; }

        public ExecutionStatus Status { get; set; }

        public TOutput Output { get; set; }
    }
}
