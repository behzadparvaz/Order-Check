using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.Domain.Contracts
{
    public interface INFCConnection
    {
        Task<string> JoinConnection(string meetingId, string vendorName);
    }
}
