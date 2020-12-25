using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EthereumBlockchainExplorer.Services
{
    public interface IEthereumService
    {
        Task<HexBigInteger> GetLatestBlockNumber();
        Task<List<BlockWithTransactions>> GetLatest5BlocksInfo(HexBigInteger latestBlockNumber);
        Task<BlockWithTransactions> GetBlockInfo(HexBigInteger blockNumber);
    }
}
