using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Infra.Connection.ThirdParty.Dtos
{
    public class ResponseNFC
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
        public string Errors { get; set; }
        public Data Data { get; set; }
    }

    public class Data
    {
        public string MeetingId { get; set; }
        public string Id { get; set; }
        public string Uri { get; set; }
    }
}
