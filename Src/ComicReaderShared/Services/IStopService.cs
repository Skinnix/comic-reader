using System.Threading.Tasks;

namespace Skinnix.ComicReader.Services
{
    public interface IStopService : IStateService
    {
        Task Stop();
    }
}
