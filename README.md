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

We will create a service class to encapsulate all the logic of interacting with Ethereum. This class will be called EthereumService and will receive as construction injection the IWeb3 interface:

```csharp
public class EthereumService : IEthereumService
{
    private readonly IWeb3 _web3;
    public EthereumService(IWeb3 web3)
    {
        _web3 = web3;
    }
```

In order to obtain the implementation of the IWeb3 interface in runtime, we will need to add it to the Dependency Injection container. We will also add the EthereumService class to the DI container so the pages we will create can get data from the Blockchain using this class.

In the Program.cs class:

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

Now we will create our first page, the index.razor. We will inject two services to this page, the IEthereumService in order to get data from Ethereum and NavigationManager in order to navigate to others pages.

```razor
@page "/"
@inject IEthereumService _ethereumService 
@inject NavigationManager _navigationManager
```
On this page we will want to display the following information:

* The latest block number

* The latest blocks

* The latest transactions

To get the latest block number when we the page renders, we will override the method OnInitializedAsync():

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
The method in EthereumService class that gets the latest block number:

```csharp
public async Task<HexBigInteger> GetLatestBlockNumber()
{
    return await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
}
```

## Get Latest Blocks and Transactions

Blazor applications are built using components. Components include a self-contained piece of the user interface and the logic necessary to get the data or respond to UI events.

We will create a razor component (LatestBlocksAndTransactions.razor) to encapsulate the User Interface that is responsible of getting the latest 5 Ethereum blocks and the 5 latest approved transactions.

This component will receive as parameter the latest block number. The Index.razor will render this component and pass this parameter by declaring them using the HTML element syntax:

```razor
<div class="row">
    @if (_latestBlockNumber != null)
    {
        <LatestBlocksAndTransactions LatestBlockNumber="_latestBlockNumber"/>
    }
</div>
```

This created LatestBlocksAndTransactions component will now get the data from the last 5 blocks:

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

This page will contain information about a block. We will receive as a parameter for this page the BlockNumber. With this information, we can search for the desired block and its transactions by overriding the OnInitializedAsync() method and calling the GetBlockInfomethod from IEthereumService:

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
The field `_hasLoaded` was created because when the page renders there is still no information about the block and the transactions that it contains. We will only display the blockÔÇÖs information when we get the data from Ethereum. So after we successfully get the data, we set the `_hasLoaded` to true and we can display this information as seen below:

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

In order to get the balance for an address, we call the GetBalancemethod from the Web3 class:

<iframe src="https://medium.com/media/bbfb0b88071bb5ad1608d419c4fe8f6c" frameborder=0></iframe>

To get the transactions From/To the Address, we will iterate over each block and verify the transactions. To keep things simple, we will iterate over the last 10 blocks and verify among the transactions if the Address was the source or the destination:

<iframe src="https://medium.com/media/6425a0d9fd3881a65a5d90846331f197" frameborder=0></iframe>

## Deploy to GitHub Pages with GitHub Actions

Since the publish output of a Blazor WebAssembly project are static files, we can deploy the application to static site hosts such as Azure Static Web Apps and GitHub Pages. We will automate the deployment of our Blockchain Explorer to GitHub Pages with GitHub Actions.

In order to create a GitHub Actions, go to your repository, navigate to your Actions tab then click on the link ÔÇ£set up a workflow yourselfÔÇØ. GitHub will display a template of a YAML file with instructions on how to build and deploy the application.

For our Blazor WebAssembly application, the created workflow YAML file to deploy our Blockchain Explorer can be found below:

<iframe src="https://medium.com/media/5211eb6310cf12c0b17f5dab64f1f7c0" frameborder=0></iframe>

There is a great article by Niels Swimberghe about [how to deploy ASP.NET Blazor WebAssembly to GitHub Pages](https://swimburger.net/blog/dotnet/how-to-deploy-aspnet-blazor-webassembly-to-github-pages). It has a video and a step by step guide on how to deploy a Blazor WebAssembly application to GitHub Pages and how to solve the problems when doing so. The link can be found on the references section below.

## Conclusion

In this article we have created an Ethereum Blockchain Explorer using the new Blazor WebAssembly framework. We also have published this application to GitHub Pages using GitHub Actions.

The source code for this application and the URL for the deployed app can be found below:

[strykerin/Ethereum-Blockchain-Explorer (github.com)](https://github.com/strykerin/Ethereum-Blockchain-Explorer)

[EthereumBlockchainExplorer (strykerin.github.io)](https://strykerin.github.io/Ethereum-Blockchain-Explorer/)

## References

[strykerin/Ethereum-Blockchain-Explorer (github.com)](https://github.com/strykerin/Ethereum-Blockchain-Explorer)

[EthereumBlockchainExplorer (strykerin.github.io)](https://strykerin.github.io/Ethereum-Blockchain-Explorer/)

[Blockchain Explorer Tutorial ÔÇö What Is A Blockchain Explorer? (softwaretestinghelp.com)](https://www.softwaretestinghelp.com/blockchain-explorer-tutorial/)

[What is Blazor WebAssembly? ÔÇö Learn | Microsoft Docs](https://docs.microsoft.com/en-us/learn/modules/build-blazor-webassembly-visual-studio-code/2-understand-blazor-webassembly)

[What Is Infura? | The BeginnerÔÇÖs Guide ÔÇö Decrypt](https://decrypt.co/resources/what-is-infura)

[Ethereum API | IPFS API Gateway | ETH Nodes as a Service | Infura](https://infura.io/)

[Create and use ASP.NET Core Razor components | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-5.0)

[How to deploy ASP.NET Blazor WebAssembly to GitHub Pages (swimburger.net)](https://swimburger.net/blog/dotnet/how-to-deploy-aspnet-blazor-webassembly-to-github-pages)
