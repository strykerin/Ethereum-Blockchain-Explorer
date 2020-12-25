using System.Collections.Generic;
using Nethereum.RPC.Eth.DTOs;
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

        public async Task<BlockWithTransactions> GetBlockInfo(HexBigInteger blockNumber)
        {
            return await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(blockNumber);
        }

        public async Task<List<BlockWithTransactions>> GetLatest5BlocksInfo(HexBigInteger latestBlockNumber)
        {
            List<BlockWithTransactions> blocksWithTransactions = new List<BlockWithTransactions>();
            for (int i = 0; i < 5; i++)
            {
                BlockWithTransactions block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(latestBlockNumber);
                blocksWithTransactions.Add(block);
            }
            return blocksWithTransactions;
        }
    }
}
