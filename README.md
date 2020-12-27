# Ethereum Blockchain Explorer

## Building the Blockchain Explorer

First, we need to create our Blazor WebAssembly application. In order to do that, run the following cli command:

```PowerShell
dotnet new blazorwasm -o Your-Project-Name
```

Now, we add the library that will allow us to get data from Ethereum, the [Nethereum](https://nethereum.com/). It is an open source .NET integration library for blockchain. To add the library to your project:

```PowerShell
dotnet add package Nethereum.Web3 --version 3.8.0
```

We will create a service class to encapsulate all the logic of interacting with Ethereum. This class will be called `EthereumService` and will receive as construction injection the `IWeb3` interface:

```csharp
public class EthereumService : IEthereumService
{
    private readonly IWeb3 _web3;
    public EthereumService(IWeb3 web3)
    {
        _web3 = web3;
    }
```

In order to obtain the implementation of the `IWeb3` interface in runtime, we will need to add it to the Dependency Injection container. We will also add the `EthereumService` class to the DI container so the pages we will create can get data from the Blockchain using this class.

In the `Program.cs` class:

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        // Add services to DI container
        builder.Services.AddSingleton<IWeb3>(ctx => new Web3("https://mainnet.infura.io/v3/Your-Infura-Key"));
        builder.Services.AddSingleton<IEthereumService, EthereumService>();

        await builder.Build().RunAsync();
    }
}
```

Note that when we instantiate the Web3 class, we need the URL from the Infura mainnet with your API Key. If you donÔÇÖt have one, you can go to [Infura](https://infura.io/) website and create an account.

## Creating our First page

Now we will create our first page, the `index.razor`. We will inject two services to this page, the `IEthereumService` in order to get data from Ethereum and `NavigationManager` in order to navigate to others pages.

```razor
@page "/"
@inject IEthereumService _ethereumService 
@inject NavigationManager _navigationManager
```
On this page we will want to display the following information:

* The latest block number

* The latest blocks

* The latest transactions

To get the latest block number when we the page renders, we will override the method `OnInitializedAsync()`:

```csharp
@code {
    private Nethereum.Hex.HexTypes.HexBigInteger _latestBlockNumber = null;
    private string _blockNumberString;
    private int _blockNumber;
    private bool _invalidParse = false;

    protected override async Task OnInitializedAsync()
    {
        _latestBlockNumber = await _ethereumService.GetLatestBlockNumber();
    }
```
When we finish to get the latest block number, we can now display it in the page:

```razor
<div class="container">
    <div class="row">
        <div class="col">
            <h2>Ethereum Blockchain Explorer</h2>
        </div>
        <div class="col">
            <p>
                Latest Block Number: @_latestBlockNumber
            </p>
        </div>
    </div>
```
The method in `EthereumService` class that gets the latest block number:

```csharp
public async Task<HexBigInteger> GetLatestBlockNumber()
{
    return await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
}
```

## Get Latest Blocks and Transactions

Blazor applications are built using components. Components include a self-contained piece of the user interface and the logic necessary to get the data or respond to UI events.

We will create a razor component (`LatestBlocksAndTransactions.razor`) to encapsulate the User Interface that is responsible of getting the latest 5 Ethereum blocks and the 5 latest approved transactions.

This component will receive as parameter the latest block number. The `Index.razor` will render this component and pass this parameter by declaring them using the HTML element syntax:

```razor
<div class="row">
    @if (_latestBlockNumber != null)
    {
        <LatestBlocksAndTransactions LatestBlockNumber="_latestBlockNumber"/>
    }
</div>
```

This created `LatestBlocksAndTransactions` component will now get the data from the last 5 blocks:

```razor
@code {
    [Parameter]
    public Nethereum.Hex.HexTypes.HexBigInteger LatestBlockNumber { get; set; }

    private bool hasLoaded = false;
    private List<BlockWithTransactions> _blockWithTransactions = new List<BlockWithTransactions>();
    private Transaction[] _latest5Transactions;

    protected override async Task OnInitializedAsync()
    {
        _blockWithTransactions = await _ethereumService.GetLatestBlocksInfo(LatestBlockNumber, 5);
        _latest5Transactions = _blockWithTransactions.First().Transactions.Take(5).ToArray();
        hasLoaded = true;
    }
}
```

and then render it on the page:

```razor
<div class="row">
    @if (hasLoaded)
    {
        @foreach (BlockWithTransactions block in _blockWithTransactions)
        {
            <div class="card">
                <a href="/Block/@block.Number">
                    <p>
                        <b>Block Number:</b> @block.Number
                    </p>
                    <p>
                        <b>Miner:</b> @block.Miner
                    </p>
                    <p>
                        <b>Transactions:</b> @block.TransactionCount()
                    </p>
                </a>
            </div>
       }
    }
```

## Block Page

This page will contain information about a block. We will receive as a parameter for this page the `BlockNumber`. With this information, we can search for the desired block and its transactions by overriding the `OnInitializedAsync()` method and calling the `GetBlockInfomethod` from `IEthereumService`:

```razor
@code {
    [Parameter]
    public int BlockNumber { get; set; }
    private HexBigInteger _blockNumberBigInteger;
    private bool _hasLoaded = false;

    private BlockWithTransactions _block;

    protected override async Task OnInitializedAsync()
    {
        _blockNumberBigInteger = new HexBigInteger(BlockNumber);
        _block = await _ethereumService.GetBlockInfo(_blockNumberBigInteger);
        _hasLoaded = true;
    }
}
```
The field `_hasLoaded` was created because when the page renders there is still no information about the block and the transactions that it contains. We will only display the block's information when we get the data from Ethereum. So after we successfully get the data, we set the `_hasLoaded` to true and we can display this information as seen below:

```razor
@page "/Block/{BlockNumber:int}"
@inject IEthereumService _ethereumService

<div class="container">
    <div class="row">
        <h2>
            Block #@BlockNumber
        </h2>
    </div>
    @if (_hasLoaded)
    {
        @if (!(_block is null))
        {
            <div class="card">
                <p>
                    <b>Block Height:</b> @BlockNumber
                </p>
                <p>
                    <b>TimeStamp:</b> @_block.Timestamp
                </p>
                <p>
                    <b>Mined By:</b> @_block.Miner
                </p>
```
## Address Page

On this page we will display two information for the Address:

* The Balance in Ether

* The transactions From/To this Address on the last 10 blocks

In order to get the balance for an address, we call the `GetBalancemethod` from the `Web3` class:

```csharp
public async Task<HexBigInteger> GetAddressBalance(string addressHash)
{
    return await _web3.Eth.GetBalance.SendRequestAsync(addressHash);
}
```

To get the transactions From/To the Address, we will iterate over each block and verify the transactions. To keep things simple, we will iterate over the last 10 blocks and verify among the transactions if the Address was the source or the destination:

```csharp
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
```

## Deploy to GitHub Pages with GitHub Actions

Since the publish output of a Blazor WebAssembly project are static files, we can deploy the application to static site hosts such as Azure Static Web Apps and GitHub Pages. We will automate the deployment of our Blockchain Explorer to GitHub Pages with GitHub Actions.

In order to create a GitHub Actions, go to your repository, navigate to your Actions tab then click on the link "set up a workflow yourself". GitHub will display a template of a YAML file with instructions on how to build and deploy the application.

For our Blazor WebAssembly application, the created workflow YAML file to deploy our Blockchain Explorer can be found below:

```yml
name: Deploy to GitHub Pages

# Controls when the action will run. 
on:
  # Triggers the workflow on push or pull request events but only for the main branch
  push:
    branches: [ main ]

# A workflow run is made up of one or more jobs that can run sequentially or in parallel
jobs:
  deploy-to-github-pages:
    # use ubuntu-latest image to run steps on
    runs-on: ubuntu-latest
    steps:
      # uses GitHub's checkout action to checkout code form the master branch
      - uses: actions/checkout@v2
        
      # sets up .NET Core SDK 5.0
      - name: Setup .NET Core SDK
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 5.0.*       
     
     # publishes Blazor project to the release-folder
      - name: Publish .NET Core Project
        run: dotnet publish EthereumBlockchainExplorer/EthereumBlockchainExplorer/EthereumBlockchainExplorer.csproj -c Release -o release --nologo
      
     # changes the base-tag in index.html from '/' to 'Ethereum-Blockchain-Explorer' to match GitHub Pages repository subdirectory
      - name: Change base-tag in index.html from / to Ethereum-Blockchain-Explorer
        run: sed -i 's/<base href="\/" \/>/<base href="\/Ethereum-Blockchain-Explorer\/" \/>/g' release/wwwroot/index.html
    
     # copy index.html to 404.html to serve the same file when a file is not found
      - name: copy index.html to 404.html
        run: cp release/wwwroot/index.html release/wwwroot/404.html
    
     # add .nojekyll file to tell GitHub pages to not treat this as a Jekyll project. (Allow files and folders starting with an underscore)
      - name: Add .nojekyll file
        run: touch release/wwwroot/.nojekyll
        
      - name: Commit wwwroot to GitHub Pages
        uses: JamesIves/github-pages-deploy-action@3.7.1
        with:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          BRANCH: gh-pages
          FOLDER: release/wwwroot
```

There is a great article by Niels Swimberghe about [how to deploy ASP.NET Blazor WebAssembly to GitHub Pages](https://swimburger.net/blog/dotnet/how-to-deploy-aspnet-blazor-webassembly-to-github-pages). It has a video and a step by step guide on how to deploy a Blazor WebAssembly application to GitHub Pages and how to solve the problems when doing so.
