using System;
using System.Collections.Generic;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class EosActionWrap
    {
        public IEnumerable<EosAction> actions { get; set; }
    }

    public class EosAction
    {
        public long global_action_seq { get; set; }

        public long account_action_seq { get; set; }

        public string block_time { get; set; }

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

        public IEnumerable<ActionTraceActAuthorization> authorization { get; set; }

        public ActionDataWrap data { get; set; }
    }

    public class ActionTraceActAuthorization
    {
        public string actor { get; set; }

        public string permission { get; set; }
    }

    public class ActionDataWrap
    {
        public long id { get; set; }

        public ActionData t { get; set; }

        public ActionData o { get; set; }

        public ActionData data { get => t ?? o; }

        public string symbol { get; set; }

        public string account { get; set; }
    }

    public class ActionData
    {
        public long id { get; set; }

        public string account { get; set; }

        public string asker { get; set; }

        public string bidder { get; set; }

        public string ask { get; set; }

        public string bid { get; set; }

        public long unit_price { get; set; }

        public long timestamp { get; set; }
    }
}
