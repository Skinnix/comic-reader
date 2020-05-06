using System.Threading.Tasks;

namespace Skinnix.ComicReader.Services
{
    public interface IStartService : IStateService
    {
        Task Start();
    }
}
