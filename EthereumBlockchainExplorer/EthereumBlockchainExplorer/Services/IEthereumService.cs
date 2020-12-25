using Nethereum.Hex.HexTypes;
using System.Threading.Tasks;

namespace EthereumBlockchainExplorer.Services
{
    public interface IEthereumService
    {
        Task<HexBigInteger> GetLatestBlockNumber();
    }
}
