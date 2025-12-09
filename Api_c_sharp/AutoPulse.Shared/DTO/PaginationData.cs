using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPulse.Shared.DTO
{
    public class PaginationData
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }
        public bool CanGoToPrevious { get; set; }
        public bool CanGoToNext { get; set; }
        public bool ShowFirstPage { get; set; }
        public bool ShowLastPage { get; set; }
        public bool ShowFirstDots { get; set; }
        public bool ShowLastDots { get; set; }
        public int[] VisiblePages { get; set; } = Array.Empty<int>();
    }
}
