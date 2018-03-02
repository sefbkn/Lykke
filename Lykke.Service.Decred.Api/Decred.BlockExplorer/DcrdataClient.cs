using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Decred.BlockExplorer
{
    /// <summary>
    /// HTTP client for dcrdata
    /// </summary>
    public class DcrdataClient : BlockExplorer
    {
        private readonly HttpClient _client;
        private readonly Uri _apiEndpoint;

        public DcrdataClient(HttpClient client, Uri apiEndpoint)
        {
            _client = client;
            _apiEndpoint = apiEndpoint ?? throw new ArgumentNullException(nameof(apiEndpoint));
        }

        public override async Task<AddressTxRaw[]> GetAddressTxRawAsync(string address, int? count = 0)
        {
            return await GetResponseAsync<AddressTxRaw[]>($"api/address/{address}/count/{count}/raw");
        }

        private async Task<T> GetResponseAsync<T>(string path)
        {
            var url = _apiEndpoint + path;
            var response = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(response);
        }
    }

    public class AddressTxRaw
    {
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("txid")]
        public string TxId { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("locktime")]
        public uint Locktime { get; set; }
        [JsonProperty("vin")]
        public VinPrevOut[] Vin { get; set; }
        [JsonProperty("vout")]
        public Vout[] Vout { get; set; }
        [JsonProperty("confirmations")]
        public long Confirmations { get; set; }
        [JsonProperty("blockhash")]
        public string BlockHash { get; set; }
        [JsonProperty("time")]
        public long? Time { get; set; }
        [JsonProperty("blocktime")]
        public long? Blocktime { get; set; }
    }

    public class VinPrevOut
    {
        [JsonProperty("coinbase")]
        public string Coinbase { get; set; }
        [JsonProperty("stakebase")]
        public string Stakebase { get; set; }
        [JsonProperty("txid")]
        public string TxId { get; set; }
        [JsonProperty("vout")]
        public int Vout { get; set; }
        [JsonProperty("tree")]
        public byte Tree { get; set; }
        [JsonProperty("amountin")]
        public decimal? AmountIn { get; set; }
        [JsonProperty("blockheight")]
        public uint? BlockHeight { get; set; }
        [JsonProperty("blockindex")]
        public uint? BlockIndex { get; set; }
        [JsonProperty("scriptsig")]
        public ScriptSignature ScriptSig { get; set; }
        [JsonProperty("prevout")]
        public PrevOut PrevOut { get; set; }
        [JsonProperty("sequence")]
        public uint Sequence { get; set; }
    }

    public class Vout
    {
        [JsonProperty("value")]
        public decimal Value { get; set; }
        [JsonProperty("n")]
        public uint N { get; set; }
        [JsonProperty("version")]
        public ushort Version { get; set; }
        [JsonProperty("scriptPubKey")]
        public ScriptPublicKey ScriptPubKeyDecoded { get; set; }
    }

    public class ScriptPublicKey
    {
        [JsonProperty("asm")]
        public string Asm { get; set; }
        [JsonProperty("reqSigs")]
        public int? RequiredSignatures { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("addresses")]
        public string[] Addresses { get; set; }
        [JsonProperty("commitamt")]
        public decimal? CommitAmount { get; set; }
    }

    public class ScriptSignature
    {
        [JsonProperty("asm")]
        public string Asm { get; set; }
        [JsonProperty("hex")]
        public string Hex { get; set; }
    }

    public class PrevOut
    {
        [JsonProperty("addresses")]
        public string[] Addresses { get; set; }
        [JsonProperty("value")]
        public decimal Value { get; set; }
    }
}

