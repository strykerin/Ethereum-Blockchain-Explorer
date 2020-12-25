    using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System.Threading.Tasks;

namespace EthereumBlockchainExplorer.Services
{
    public class EthereumService : IEthereumService
    {
        private readonly IWeb3 _web3;
        public EthereumService(IWeb3 web3)
        {
            _web3 = web3;
        }

        public async Task<HexBigInteger> GetLatestBlockNumber()
        {
            return await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        }
    }
}
