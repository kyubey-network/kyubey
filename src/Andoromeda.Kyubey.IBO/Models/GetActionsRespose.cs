using System.Collections.Generic;

namespace Andoromeda.Kyubey.IBO.Models
{
    public class GetActionsRespose
    {
        public IEnumerable<Action> actions { get; set; }
    }

    public class Action
    {
        public ActionTrace action_trace { get; set; }
    }

    public class ActionTrace
    {
        public Act act { get; set; }
    }

    public class Act
    {
        public string account { get; set; }

        public string name { get; set; }

        public ActData data { get; set; }
    }

    public class ActData
    {
        public string from { get; set; }

        public string to { get; set; }

        public string quantity { get; set; }
    }
}
