// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using Hazelcast.IO;

// Client Protocol version, Since:1.0 - Update:1.0

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientGetPartitionsCodec
    {
        public static readonly ClientMessageType RequestType = ClientMessageType.ClientGetPartitions;
        public const int ResponseType = 108;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;

            public static int CalculateDataSize()
            {
                var dataSize = ClientMessage.HeaderSize;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest()
        {
            var requiredDataSize = RequestParameters.CalculateDataSize();
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//
        public class ResponseParameters
        {
            public IList<KeyValuePair<Address, IList<int>>> partitions;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var partitions = new List<KeyValuePair<Address, IList<int>>>();
            var partitionsSize = clientMessage.GetInt();
            for (var partitionsIndex = 0; partitionsIndex < partitionsSize; partitionsIndex++)
            {
                var partitionsItemKey = AddressCodec.Decode(clientMessage);
                var partitionsItemVal = new List<int>();
                var partitionsItemValSize = clientMessage.GetInt();
                for (var partitionsItemValIndex = 0;
                    partitionsItemValIndex < partitionsItemValSize;
                    partitionsItemValIndex++)
                {
                    var partitionsItemValItem = clientMessage.GetInt();
                    partitionsItemVal.Add(partitionsItemValItem);
                }
                var partitionsItem = new KeyValuePair<Address, IList<int>>(partitionsItemKey, partitionsItemVal);
                partitions.Add(partitionsItem);
            }
            parameters.partitions = partitions;
            return parameters;
        }
    }
}