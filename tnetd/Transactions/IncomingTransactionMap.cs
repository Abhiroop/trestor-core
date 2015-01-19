
//@Author: Arpan Jati
//@Date: 16th January 2015

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Nodes;

namespace TNetD.Transactions
{

    class IncomingTransactionMap
    {
        NodeState nodeState;

        //[Transaction ID] -> [[Transaction Content] -> [vector of sender address]]
        ConcurrentDictionary<Hash, TransactionContentData> TransactionMap = new ConcurrentDictionary<Hash, TransactionContentData>();

        //[Transaction ID] -> Transaction Data
        ConcurrentDictionary<Hash, TransactionContent> IncomingTransactions = new ConcurrentDictionary<Hash, TransactionContent>();

        ConcurrentDictionary<Hash, TransactionContent> IncomingPropagations = new ConcurrentDictionary<Hash, TransactionContent>();

        public IncomingTransactionMap(NodeState nodeState)
        {
            this.nodeState = nodeState;
        }

        /// <summary>
        /// Given a transactionID returns current associated TransactionContent, else return false.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        Tuple<TransactionContent, bool> GetTransactionContent(Hash transactionID)
        {
            if (IncomingTransactions.ContainsKey(transactionID))
            {
                return new Tuple<TransactionContent, bool>(IncomingTransactions[transactionID], true);
            }

            return new Tuple<TransactionContent, bool>(null, false);
        }

        /// <summary>
        /// Given a transactionID returns current associated TransactionContentData, else return false.
        /// </summary>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        Tuple<TransactionContentData, bool> GetTransactionContentData(Hash transactionID)
        {
            if (TransactionMap.ContainsKey(transactionID))
            {
                return new Tuple<TransactionContentData, bool>(TransactionMap[transactionID], true);
            }

            return new Tuple<TransactionContentData, bool>(null, false);
        }

        /// <summary>
        /// Insert a new transaction to the incoming transaction processing queue.
        /// This queue is processed from time to time.
        /// </summary>
        /// <param name="transactionContent"></param>
        /// <param name="senderPublicKey"></param>
        public void InsertNewTransaction(TransactionContent transactionContent, Hash senderPublicKey)
        {
            TransactionProcessingResult rslt = transactionContent.VerifySignature();

            if (rslt == TransactionProcessingResult.Accepted)
            {
                // Insert if the transaction does not already exist.
                if (!IncomingTransactions.ContainsKey(transactionContent.TransactionID))
                {
                    IncomingTransactions.TryAdd(transactionContent.TransactionID, transactionContent);
                }
            }
            else
            {
                // Add the user to the blacklist if signature is invalid or integrity checks fails.
                nodeState.GlobalBlacklistedUsers.Add(senderPublicKey);
            }
        }

        public TransactionProcessingResult HandlePropagationRequest(TransactionContent transactionContent)
        {
            TransactionProcessingResult rslt = transactionContent.VerifySignature();

            if (rslt == TransactionProcessingResult.Accepted)
            {
                // Insert if the transaction does not already exist.
                if (!IncomingPropagations.ContainsKey(transactionContent.TransactionID))
                {
                    IncomingPropagations.TryAdd(transactionContent.TransactionID, transactionContent);
                }
            }

            return rslt;
        }

        /*
        void IncomingTransactionMap::GetEligibleTransactionForConsensus(vector<Hash> connectedValidators, vector<Hash>& transactionIDtoMigrate)
        {
            for (HM::iterator it = TransactionMap.begin(); it != TransactionMap.begin(); ++it)
            {
                Hash TransactionID = it->first;
                TransactionContentData TCD = it->second;
                hash_set<Hash> ForwardersPKs = TCD.ForwardersPK;

                //run through connectedValidators
                int counter = 0;
                for (int i = 0; i < (int)connectedValidators.size(); i++)
                {
                    Hash validatorPK = connectedValidators[i];
                    hash_set<Hash>::iterator itr = ForwardersPKs.find(validatorPK);

                    if (itr != ForwardersPKs.end())
                    {
                        ++counter;
                    }
                }
                float perc = (float)counter / connectedValidators.size();
                if (perc * 100 >= Constants::CONS_TRUSTED_VALIDATOR_THRESHOLD_PERC)
                    transactionIDtoMigrate.push_back(TransactionID);
            }
        }*/

        void RemoveTransactionsFromTransactionMap(List<Hash> transactionIDs)
        {
            foreach (Hash transactionID in transactionIDs)
            {
                if (TransactionMap.ContainsKey(transactionID))
                {
                    TransactionContentData tcd;
                    TransactionMap.TryRemove(transactionID, out tcd);
                }
            }
        }

        Hash[] FetchAllTransactionIDs()
        {
            return (Hash[])TransactionMap.Keys;
        }

        /*
        //given a set of transaction IDs get the associated transaction contents

        vector<TransactionContent> IncomingTransactionMap::FetchTransactionContent(vector<Hash> differenceTransactionIDs)
        {
            vector<TransactionContent> toSent;

            HM::accessor acc;

            for (int i = 0; i < (int)differenceTransactionIDs.size(); i++)
            {
                Hash transactionID = differenceTransactionIDs[i];

                if (TransactionMap.find(acc, transactionID))
                {
                    TransactionContentData TCD = acc->second;
                    TransactionContent TC = TCD.TC;
                    toSent.push_back(TC);
                }

            }

            return toSent;
        }*/

        bool HaveTransactionInfo(Hash transactionID)
        {
            return (TransactionMap.ContainsKey(transactionID));
        }

        /*
        //Given a transaction ID from a validator, upate it in the current transactionMap

        void IncomingTransactionMap::UpdateTransactionID(Hash transactionID, Hash forwarderPublicKey)
        {
            HM::accessor acc;

            if (TransactionMap.find(acc, transactionID))
            {
                TransactionContentData tcd = acc->second;

                hash_set<Hash> fpk = tcd.ForwardersPK;

                bool exists = false;
                for (hash_set<Hash>::iterator it = fpk.begin(); it != fpk.end(); ++it)
                {
                    Hash tmp = *it;
                    if (tmp == forwarderPublicKey)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    fpk.insert(forwarderPublicKey);
                }
            }
        }

        //Given a transaction ID from a validator, update it in the current transactionMap

        void IncomingTransactionMap::InsertTransactionContent(TransactionContent tc, Hash forwarderPublicKey)
        {
            //verify signature
            if (!tc.VerifySignature())
            {
                state.GlobalBlacklistedValidators.push_back(forwarderPublicKey);
                return;
            }

            //search in the Transaction map to see
            //if the particular transaction ID is in
            //the map. If exists then update. Otherwise
            //ask for the transaction content
            Hash transactionID = tc.GetHash();

            HM::accessor acc;
            if (!TransactionMap.find(acc, transactionID))
            {
                TransactionContentData tcd;

                tcd.ForwardersPK.insert(forwarderPublicKey);
                tcd.TC = tc;
                TransactionMap.insert(make_pair(transactionID, tcd));
            }

            else
            {
                TransactionContentData tcd = acc->second;

                hash_set<Hash> ForwardersPK = tcd.ForwardersPK;

                bool exists = false;
                for (hash_set<Hash>::iterator it = ForwardersPK.begin(); it != ForwardersPK.end(); ++it)
                {
                    Hash tmp = *it;
                    if (tmp == forwarderPublicKey)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    ForwardersPK.insert(forwarderPublicKey);
                }
            }
        }

        // Process pending/queued operations.
        void IncomingTransactionMap::DoEvents()
        {


        }

        */

    }
}

