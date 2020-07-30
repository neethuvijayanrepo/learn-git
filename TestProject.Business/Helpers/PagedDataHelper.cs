using TestProject.Business.BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Business.Helpers
{
    public class PagedDataHelper<T>
    {
        /// <summary>
        /// Tpo fetch paged data from collection
        /// </summary>
        /// <param name="data">Data to be paged</param>
        /// <param name="pageSize">Size of page</param>
        /// <param name="page">Current page index</param>
        /// <returns>PagedTestProjectResponse of input type collection type</returns>
        public static PagedTestProjectResponse<List<T>> GetPagedData(List<T> data, int pageSize, int page)
        {
            if (data == null || !data.Any())
            {
                throw new ArgumentException("Invalid data for paging");
            }

            PagedTestProjectResponse<List<T>> output = new PagedTestProjectResponse<List<T>>();
            output.TotalRows = data.Count();

            if (output.TotalRows <= (page - 1) * pageSize)
            {
                int lastPage = output.TotalRows / pageSize;
                if (output.TotalRows % pageSize != 0)
                {
                    lastPage += 1;
                }

                page = lastPage;
            }

            output.Output = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            output.PageSize = pageSize;
            output.CurrentPage = page;
            output.Status = Utilities.Common.ExecutionStatus.Success;

            return output;
        }
    }
}
