using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class TokenCommentViewModel
    {
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public int PraiseCount { get; set; }
        public string CreateTime { get; set; }
        public string Content { get; set; }
        public long? ReplyUserId { get; set; }
        public string ReplyUserName { get; set; }
        public List<TokenCommentViewModel> ChildComments { get; set; }
    }
}
