using System;

namespace Andoromeda.Kyubey.Manage.Models
{
    public class EosAction
    {
        public long global_action_seq { get; set; }

        public long account_action_seq { get; set; }

        public DateTime block_time { get; set; }

        public ActionTrace action_trace { get; set; }
    }

    public class ActionTrace
    {
        public ActionTraceAct act { get; set; }
    }

    public class ActionTraceAct
    {
        public string account { get; set; }

        public string name { get; set; }

        public ActionTraceActAuthorization authorization { get; set; }

        public ActionDataWrap data { get; set; }
    }

    public class ActionTraceActAuthorization
    {
        public string actor { get; set; }

        public string permission { get; set; }
    }

    public class ActionDataWrap
    {
        public ActionData t { get; set; }
    }

    public class ActionData
    {
        public long id { get; set; }

        public string asker { get; set; }

        public string bidder { get; set; }

        public string ask { get; set; }

        public string bid { get; set; }

        public long unit_price { get; set; }
    }
}
