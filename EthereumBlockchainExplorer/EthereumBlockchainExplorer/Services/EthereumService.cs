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
            int index = (int)latestBlockNumber.Value;
            for (int i = index; i > index-5; i--)
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
            int beginInt = 11530900;
            int endInt = 11530902;
            List<Transaction> transactionsForAccount = new List<Transaction>();
            for (int i = beginInt; i <= endInt; i++)
            {
                BlockWithTransactions block = await this.GetBlockInfo(new HexBigInteger(i));
                foreach (Transaction transaction in block.Transactions)
                {
                    if (transaction.From == addressHash || transaction.To == addressHash)
                    {
                        transactionsForAccount.Add(transaction);
                    }
                }
            }

            return transactionsForAccount;
        }

    }
}
