// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Creates a transaction with the given parameters.
    ///</summary>
#if SERVER_CODEC
    internal static class TransactionCreateServerCodec
#else
    internal static class TransactionCreateCodec
#endif
    {
        public const int RequestMessageType = 1376768; // 0x150200
        public const int ResponseMessageType = 1376769; // 0x150201
        private const int RequestTimeoutFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestDurabilityFieldOffset = RequestTimeoutFieldOffset + BytesExtensions.SizeOfLong;
        private const int RequestTransactionTypeFieldOffset = RequestDurabilityFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestThreadIdFieldOffset = RequestTransactionTypeFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfGuid;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// The maximum allowed duration for the transaction operations.
            ///</summary>
            public long Timeout { get; set; }

            /// <summary>
            /// The durability of the transaction
            ///</summary>
            public int Durability { get; set; }

            /// <summary>
            /// Identifies the type of the transaction. Possible values are:
            /// 1 (Two phase):  The two phase commit is more than the classic two phase commit (if you want a regular
            /// two phase commit, use local). Before it commits, it copies the commit-log to other members, so in
            /// case of member failure, another member can complete the commit.
            /// 2 (Local): Unlike the name suggests, local is a two phase commit. So first all cohorts are asked
            /// to prepare if everyone agrees then all cohorts are asked to commit. The problem happens when during
            /// the commit phase one or more members crash, that the system could be left in an inconsistent state.
            ///</summary>
            public int TransactionType { get; set; }

            /// <summary>
            /// The thread id for the transaction.
            ///</summary>
            public long ThreadId { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(long timeout, int durability, int transactionType, long threadId)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Transaction.Create"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteLongL(RequestTimeoutFieldOffset, timeout);
            initialFrame.Bytes.WriteIntL(RequestDurabilityFieldOffset, durability);
            initialFrame.Bytes.WriteIntL(RequestTransactionTypeFieldOffset, transactionType);
            initialFrame.Bytes.WriteLongL(RequestThreadIdFieldOffset, threadId);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.Timeout = initialFrame.Bytes.ReadLongL(RequestTimeoutFieldOffset);
            request.Durability = initialFrame.Bytes.ReadIntL(RequestDurabilityFieldOffset);
            request.TransactionType = initialFrame.Bytes.ReadIntL(RequestTransactionTypeFieldOffset);
            request.ThreadId = initialFrame.Bytes.ReadLongL(RequestThreadIdFieldOffset);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// The transaction id for the created transaction.
            ///</summary>
            public Guid Response { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteGuidL(ResponseResponseFieldOffset, response);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadGuidL(ResponseResponseFieldOffset);
            return response;
        }

    }
}