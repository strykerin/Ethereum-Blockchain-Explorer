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

        public async Task<List<BlockWithTransactions>> GetLatestBlocksInfo(HexBigInteger latestBlockNumber, int n)
        {
            List<BlockWithTransactions> blocksWithTransactions = new List<BlockWithTransactions>();
            int index = (int)latestBlockNumber.Value;
            for (int i = index; i > index-n; i--)
            {
                HexBigInteger blockNumber = new HexBigInteger(i);
                BlockWithTransactions block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(blockNumber);
                blocksWithTransactions.Add(block);
            }
            return blocksWithTransactions;
        }

        public async Task<Transaction> GetTransaction(string txHash)
        {
            return await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);
        }

        public async Task<HexBigInteger> GetAddressBalance(string addressHash)
        {
            return await _web3.Eth.GetBalance.SendRequestAsync(addressHash);
        }

        public async Task<List<Transaction>> GetTransactionsByAccount(string addressHash)
        {
            List<Transaction> transactionsForAccount = new List<Transaction>();
            List<BlockWithTransactions> blocks = await this.GetLatestBlocksInfo(await this.GetLatestBlockNumber(), 10);
            foreach (BlockWithTransactions block in blocks)
            {
                foreach (Transaction transaction in block.Transactions)
                {
                    if (transaction.From.ToLower().Contains(addressHash.ToLower()) || 
                        (!(transaction.To is null) && transaction.To.ToLower().Contains(addressHash.ToLower())))
                    {
                        transactionsForAccount.Add(transaction);
                    }
                }
            }
            return transactionsForAccount;
        }
    }
}
