using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.SharedModels
{
    public class FetchArticlesRequest
    {
        public string Continent { get; set; } = null!;
        public int? MaxArticles { get; set; } = null;
        public DateOnly? StartDate { get; set; } = null;
        public DateOnly? EndDate { get; set; } = null;
    }
}
