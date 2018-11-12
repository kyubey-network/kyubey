using Andoromeda.Kyubey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class TokenIncubationViewModel
    {
        public IncubationInfo IncubationInfo { get; set; }
        public List<string> IncubatorBannerUrls { get; set; }
        public int PraiseCount { get; set; }
        public List<TokenCommentViewModel> Comments { get; set; }
        public List<RecentUpdateViewModel> RecentUpdate { get; set; }
    }
    public class IncubationInfo
    {
        public string Introduction { get; set; }
        public decimal CurrentRaised { get; set; }
        public decimal TargetCredits { get; set; }
        public int CurrentRaisedCount { get; set; }
        public int RemainingDay { get; set; }
        public string Detail { get; set; }
        public DateTimeOffset? BeginTime { get; set; }
    }
    public class RecentUpdateViewModel
    {
        public DateTimeOffset Time { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
