﻿using Flow.Net.Sdk.Cadence;
using Flow.Net.Sdk.Exceptions;
using Flow.Net.Sdk.Models;
using Flow.Net.Sdk.Protos.access;
using Google.Protobuf;
using Grpc.Core;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Flow.Net.Sdk.Protos.access.AccessAPI;
using static Flow.Net.Sdk.Constants.Defaults;

namespace Flow.Net.Sdk.Client
{
    public class FlowClientAsync
    {
        private readonly AccessAPIClient _client;
        private readonly CadenceConverter _cadenceConverter;
        private readonly Dictionary<string, string> _addressMap = EmulatorAddresses;

        /// <summary>
        /// A gRPC client for the Flow Access API.
        /// </summary>
        /// <param name="flowNetworkUrl"></param>
        /// <param name="channelCredentialsSecureSsl"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowClientAsync"/>.</returns>
        public FlowClientAsync(string flowNetworkUrl, 
            bool channelCredentialsSecureSsl = false, 
            IEnumerable<ChannelOption> options = null,
            Dictionary<string, string> addressMap = null)
        {
            try
            {
                _client = new AccessAPIClient(
                    new Channel(
                        flowNetworkUrl,
                        channelCredentialsSecureSsl ? ChannelCredentials.SecureSsl : ChannelCredentials.Insecure,
                        options
                ));

                _cadenceConverter = new CadenceConverter();
                _addressMap = addressMap ?? _addressMap;
            }
            catch (Exception exception)
            {
                throw new FlowException($"Failed to connect to {flowNetworkUrl}", exception);
            }
        }

        /// <summary>
        /// Check if the access node is alive and healthy.
        /// </summary>
        /// <param name="options"></param>
        public async Task PingAsync(CallOptions options = new CallOptions())
        {
            try
            {
                await _client.PingAsync(new PingRequest(), options);
            }
            catch (Exception exception)
            {
                throw new FlowException("Ping request failed.", exception);
            }
        }

        /// <summary>
        /// Gets the full payload of the latest sealed or unsealed block.
        /// </summary>
        /// <param name="isSealed"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowBlock"/>.</returns>
        public async Task<FlowBlock> GetLatestBlockAsync(bool isSealed = true, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetLatestBlockAsync(
                    new GetLatestBlockRequest
                    {
                        IsSealed = isSealed
                    }, options);

                return response.ToFlowBlock();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetLatestBlock request failed.", exception);
            }
        }

        /// <summary>
        /// Gets a full block by height.
        /// </summary>
        /// <param name="blockHeight"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowBlock"/>.</returns>
        public async Task<FlowBlock> GetBlockByHeightAsync(ulong blockHeight, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetBlockByHeightAsync(
                    new GetBlockByHeightRequest
                    {
                        Height = blockHeight
                    }, options);

                return response.ToFlowBlock();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetBlockByHeight request failed.", exception);
            }
        }

        /// <summary>
        /// Gets a full block by Id.
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowBlock"/>.</returns>
        public async Task<FlowBlock> GetBlockByIdAsync(ByteString blockId, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetBlockByIDAsync(
                    new GetBlockByIDRequest
                    {
                        Id = blockId
                    }, options);

                return response.ToFlowBlock();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetBlockById request failed.", exception);
            }
        }

        /// <summary>
        /// Gets the latest sealed or unsealed block header.
        /// </summary>
        /// <param name="isSealed"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowBlockHeader"/>.</returns>
        public async Task<FlowBlockHeader> GetLatestBlockHeaderAsync(bool isSealed = true, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetLatestBlockHeaderAsync(
               new GetLatestBlockHeaderRequest
               {
                   IsSealed = isSealed
               }, options);

                return response.ToFlowBlockHeader();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetLatestBlockHeader request failed.", exception);
            }
        }

        /// <summary>
        /// Gets a block header by height.
        /// </summary>
        /// <param name="blockHeaderHeight"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowBlockHeader"/>.</returns>
        public async Task<FlowBlockHeader> GetBlockHeaderByHeightAsync(ulong blockHeaderHeight, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetBlockHeaderByHeightAsync(
                new GetBlockHeaderByHeightRequest
                {
                    Height = blockHeaderHeight
                }, options);

                return response.ToFlowBlockHeader();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetBlockHeaderByHeight request failed.", exception);
            }
        }

        /// <summary>
        /// Gets a block header by Id.
        /// </summary>
        /// <param name="blockHeaderId"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowBlockHeader"/>.</returns>
        public async Task<FlowBlockHeader> GetBlockHeaderByIdAsync(ByteString blockHeaderId, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetBlockHeaderByIDAsync(
                new GetBlockHeaderByIDRequest
                {
                    Id = blockHeaderId
                }, options);

                return response.ToFlowBlockHeader();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetBlockHeaderById request failed.", exception);
            }
        }

        /// <summary>
        /// Executes a read-only Cadence script against the latest sealed execution state.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="arguments"></param>
        /// <param name="options"></param>
        /// <returns><see cref="ICadence"/>.</returns>
        public async Task<ICadence> ExecuteScriptAtLatestBlockAsync(FlowScript script, CallOptions options = new CallOptions())
        {
            try
            {
                var request = script.FromFlowScript(_addressMap);
                var response = await _client.ExecuteScriptAtLatestBlockAsync(request, options);
                return response.Value.FromByteStringToString().Decode(_cadenceConverter);
            }
            catch (Exception exception)
            {
                throw new FlowException("ExecuteScriptAtLatestBlock request failed.", exception);
            }
        }

        /// <summary>
        /// Executes a ready-only Cadence script against the execution state at the given block height.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="blockHeight"></param>
        /// <param name="arguments"></param>
        /// <param name="options"></param>
        /// <returns><see cref="ICadence"/>.</returns>
        public async Task<ICadence> ExecuteScriptAtBlockHeightAsync(FlowScript script, ulong blockHeight, CallOptions options = new CallOptions())
        {
            try
            {
                var request = script.FromFlowScript(blockHeight, _addressMap);
                var response = await _client.ExecuteScriptAtBlockHeightAsync(request, options);
                return response.Value.FromByteStringToString().Decode(_cadenceConverter);
            }
            catch (Exception exception)
            {
                throw new FlowException("ExecuteScriptAtBlockHeight request failed.", exception);
            }
        }

        /// <summary>
        /// Executes a ready-only Cadence script against the execution state at the block with the given Id.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="blockId"></param>
        /// <param name="arguments"></param>
        /// <param name="options"></param>
        /// <returns><see cref="ICadence"/>.</returns>
        public async Task<ICadence> ExecuteScriptAtBlockIdAsync(FlowScript script, ByteString blockId, CallOptions options = new CallOptions())
        {
            try
            {
                var request = script.FromFlowScript(blockId, _addressMap);
                var response = await _client.ExecuteScriptAtBlockIDAsync(request, options);
                return response.Value.FromByteStringToString().Decode(_cadenceConverter);
            }
            catch (Exception exception)
            {
                throw new FlowException("ExecuteScriptAtBlockId request failed.", exception);
            }
        }

        /// <summary>
        /// Retrieves events for all sealed blocks between the start and end block heights (inclusive) with the given type.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="startHeight"></param>
        /// <param name="endHeight"></param>
        /// <param name="options"></param>
        /// <returns>A <see cref="IList{T}" /> of type <see cref="FlowBlockEvent"/>.</returns>
        public async Task<IEnumerable<FlowBlockEvent>> GetEventsForHeightRangeAsync(string eventType, ulong startHeight, ulong endHeight, CallOptions options = new CallOptions())
        {
            try
            {
                startHeight = startHeight > 0 ? startHeight : 0;

                var response = await _client.GetEventsForHeightRangeAsync(
                    new GetEventsForHeightRangeRequest
                    {
                        Type = eventType,
                        StartHeight = startHeight,
                        EndHeight = endHeight
                    }, options);

                return response.ToFlowBlockEvents();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetEventsForHeightRange request failed.", exception);
            }
        }

        /// <summary>
        /// Retrieves events with the given type from the specified block Ids.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="blockIds"></param>
        /// <param name="options"></param>
        /// <returns>A <see cref="IList{T}" /> of type <see cref="FlowBlockEvent"/>.</returns>
        public async Task<IEnumerable<FlowBlockEvent>> GetEventsForBlockIdsAsync(string eventType, IEnumerable<ByteString> blockIds, CallOptions options = new CallOptions())
        {
            try
            {
                var request = new GetEventsForBlockIDsRequest
                {
                    Type = eventType
                };

                if (blockIds != null)
                {
                    foreach (var block in blockIds)
                        request.BlockIds.Add(block);
                }

                var response = await _client.GetEventsForBlockIDsAsync(request, options);
                return response.ToFlowBlockEvents();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetEventsForBlockIds request failed.", exception);
            }
        }

        /// <summary>
        /// Submits a transaction to the network.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowSendTransactionResponse"/>.</returns>
        public async Task<FlowSendTransactionResponse> SendTransactionAsync(FlowTransaction transaction, CallOptions options = new CallOptions())
        {
            try
            {
                var tx = transaction.FromFlowTransaction(_addressMap);

                var response = await _client.SendTransactionAsync(
                    new SendTransactionRequest
                    {
                        Transaction = tx
                    }, options);

                return response.ToFlowSendTransactionResponse();
            }
            catch (Exception exception)
            {
                throw new FlowException("SendTransaction request failed.", exception);
            }
        }

        /// <summary>
        /// Gets a transaction by Id.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowTransactionResponse"/>.</returns>
        public async Task<FlowTransactionResponse> GetTransactionAsync(ByteString transactionId, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetTransactionAsync(
                    new GetTransactionRequest
                    {
                        Id = transactionId
                    }, options);

                return response.ToFlowTransactionResponse();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetTransaction request failed.", exception);
            }
        }

        /// <summary>
        /// Gets the result of a transaction.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowTransactionResult"/>.</returns>
        public async Task<FlowTransactionResult> GetTransactionResultAsync(ByteString transactionId, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetTransactionResultAsync(
                new GetTransactionRequest
                {
                    Id = transactionId
                }, options);

                return response.ToFlowTransactionResult();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetTransactionResult request failed.", exception);
            }
        }

        /// <summary>
        /// Gets a collection by Id.
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowCollectionResponse"/>.</returns>
        public async Task<FlowCollectionResponse> GetCollectionByIdAsync(ByteString collectionId, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetCollectionByIDAsync(
                new GetCollectionByIDRequest
                {
                    Id = collectionId
                }, options);

                return response.ToFlowCollectionResponse();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetCollectionById request failed.", exception);
            }
        }

        /// <summary>
        /// Retrieves execution result for the specified block Id.
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowExecutionResultForBlockIdResponse"/></returns>
        public async Task<FlowExecutionResultForBlockIdResponse> GetExecutionResultForBlockIdAsync(ByteString blockId, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetExecutionResultForBlockIDAsync(
                    new GetExecutionResultForBlockIDRequest
                    {
                        BlockId = blockId
                    }, options);

                return response.ToFlowExecutionResultForBlockIdResponse();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetExecutionResultForBlockId request failed.", exception);
            }
        }

        /// <summary>
        /// Retrieves the latest snapshot of the protocol state in serialized form.
        /// This is used to generate a root snapshot file used by Flow nodes to bootstrap their local protocol state database.
        /// </summary>
        /// <param name="options"></param>
        /// <returns><see cref="FlowProtocolStateSnapshotResponse"/>.</returns>
        public async Task<FlowProtocolStateSnapshotResponse> GetLatestProtocolStateSnapshotAsync(CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetLatestProtocolStateSnapshotAsync(new GetLatestProtocolStateSnapshotRequest(), options);

                return response.ToFlowProtocolStateSnapshotResponse();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetLatestProtocolStateSnapshot request failed.", exception);
            }
        }

        /// <summary>
        /// Retrieves network parameters
        /// </summary>
        /// <param name="options"></param>
        /// <returns><see cref="FlowGetNetworkParametersResponse"/>.</returns>
        public async Task<FlowGetNetworkParametersResponse> GetNetworkParametersAsync(CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetNetworkParametersAsync(new GetNetworkParametersRequest(), options);

                return response.ToFlowGetNetworkParametersResponse();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetLatestProtocolStateSnapshot request failed.", exception);
            }
        }

        /// <summary>
        /// Gets an account by address at the latest sealed block.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowAccount"/>.</returns>
        public async Task<FlowAccount> GetAccountAtLatestBlockAsync(FlowAddress address, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetAccountAtLatestBlockAsync(
                new GetAccountAtLatestBlockRequest
                {
                    Address = address.Value
                }, options);

                return response.ToFlowAccount();
            }
            catch (Exception exception)
            {
                throw new FlowException("GetAccountAtLatestBlock request failed.", exception);
            }            
        }

        /// <summary>
        /// Gets an account by address at the given block height.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="blockHeight"></param>
        /// <param name="options"></param>
        /// <returns><see cref="FlowAccount"/>.</returns>
        public async Task<FlowAccount> GetAccountAtBlockHeightAsync(FlowAddress address, ulong blockHeight, CallOptions options = new CallOptions())
        {
            try
            {
                var response = await _client.GetAccountAtBlockHeightAsync(
                new GetAccountAtBlockHeightRequest
                {
                    Address = address.Value,
                    BlockHeight = blockHeight
                }, options);

                return response.ToFlowAccount();
            }
            catch(Exception exception)
            {
                throw new FlowException("GetAccountAtBlockHeight request failed.", exception);
            }            
        }

        /// <summary>
        /// Waits for transaction result status to be sealed.
        /// </summary>
        /// <param name="transactionResponse"></param>
        /// <param name="delayMs"></param>
        /// <param name="timeoutMs"></param>
        /// <returns><see cref="FlowTransactionResult"/></returns>
        public async Task<FlowTransactionResult> WaitForSealAsync(FlowSendTransactionResponse transactionResponse, int delayMs = 1000, int timeoutMs = 30000)
        {
            var startTime = DateTime.UtcNow;
            while (true)
            {
                var result = await GetTransactionResultAsync(transactionResponse.Id);

                if (result != null && result.Status == Protos.entities.TransactionStatus.Sealed)
                    return result;

                if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeoutMs)
                    throw new FlowException("Timed out waiting for seal.");

                await Task.Delay(delayMs);
            }
        }

        /// <summary>
        /// Retrieves the specified account from the flow.json file and generates signers where private keys exist
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="configFileName"></param>
        /// <param name="configPath"></param>
        /// <returns><see cref="FlowAccount"/></returns>
        public async Task<FlowAccount> ReadAccountFromConfigAsync(string accountName, string configFileName = null, string configPath = null)
        {
            var config = Utilities.ReadConfig(configFileName, configPath);
            config.Accounts.TryGetValue(accountName, out var configAccount);

            if (configAccount == null)
                throw new FlowException($"Failed to find account \"{accountName}\"");

            var flowAccount = await GetAccountAtLatestBlockAsync(new FlowAddress(configAccount.Address));

            if (string.IsNullOrEmpty(configAccount.Key)) 
                return flowAccount;
            
            foreach (var key in flowAccount.Keys)
            {
                // getting the public key so we can match it to our account public keys
                var keyPair = Crypto.Ecdsa.Utilities.AsymmetricCipherKeyPairFromPrivateKey(configAccount.Key, key.SignatureAlgorithm);
                var publicKey = Crypto.Ecdsa.Utilities.DecodePublicKeyToHex(keyPair);

                // select the key with a matching public key
                var flowAccountKey = flowAccount.Keys.FirstOrDefault(w => w.PublicKey == publicKey);

                if (flowAccountKey == null)
                    continue;
                
                flowAccountKey.PrivateKey = configAccount.Key;

                var privateKey = keyPair.Private as ECPrivateKeyParameters;
                flowAccountKey.Signer = new Crypto.Ecdsa.Signer(privateKey, flowAccountKey.HashAlgorithm, flowAccountKey.SignatureAlgorithm);
            }

            return flowAccount;
        }

        private static void AddArgumentsToRequest(IEnumerable<ICadence> arguments, ICollection<ByteString> requestArguments)
        {
            if (arguments == null) return;
            
            foreach (var argument in arguments)
                requestArguments.Add(argument.Encode().FromStringToByteString());
        }
    }
}
