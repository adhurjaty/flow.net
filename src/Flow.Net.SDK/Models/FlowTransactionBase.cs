﻿using Flow.Net.Sdk.Cadence;
using Google.Protobuf;
using System.Collections.Generic;

namespace Flow.Net.Sdk.Models
{
    public abstract class FlowTransactionBase : FlowInteractionBase
    {
        protected FlowTransactionBase() : base()
        {
            Authorizers = new List<FlowAddress>();
            GasLimit = 9999;
        }

        public ByteString ReferenceBlockId { get; set; }
        public ulong GasLimit { get; set; }
        public FlowAddress Payer { get; set; }
        public FlowProposalKey ProposalKey { get; set; }
        public IList<FlowAddress> Authorizers { get; set; }
    }
}
