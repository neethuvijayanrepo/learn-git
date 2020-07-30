using TestProject.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Business.BusinessObjects.Common
{
    /// <summary>
    /// Business paged response object.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class PagedTestProjectResponse<TOutput> : TestProjectResponse<TOutput>
    {
        private int _currentPage = Constants.DefaultListPage;
        private int _pageSize = Constants.DefaultListPageSize;

        public int CurrentPage {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
            }
        }
        public int TotalRows { get; set; }
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value;
            }
        }
    }
}
