using System.Threading.Tasks;
using Orleans;

namespace SampleApp.Abstractions
{
    public interface IChatGrain : IGrainWithStringKey
    {
        Task SendServerMessage();
    }
}