/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Jan 5 2015
 *  @Description: SingleTransactionFactory to create new transactions having single inputs and outputs.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNetD.Transactions
{
    /// <summary>
    /// Class to create point to point transactions from individual parts.
    /// Method To use: 
    /// 1) Create the initial parts using constructor.
    /// 2) Call 'GetSignData' to get the data to be signed.
    /// 2) Call 'Create' and pass in the signature to create the TransactionContent.
    /// </summary>
    public class SingleTransactionFactory
    {
        Hash source, destination;
        long amount;

        TransactionContent TC;

        byte[] tranxData;

        public SingleTransactionFactory(Hash source, Hash destination, long amount)
        {
            this.source = source;
            this.destination = destination;
            this.amount = amount;

            TransactionEntity teSrc = new TransactionEntity(this.source.Hex, amount);
            TransactionEntity teDst = new TransactionEntity(this.destination.Hex, amount);

            long Time = DateTime.UtcNow.ToFileTimeUtc();

            TC = new TransactionContent(new TransactionEntity[] { teSrc }, Time,
                new TransactionEntity[] { teDst });

            tranxData = TC.GetTransactionData();
        }

        /// <summary>
        /// Returns the transaction data which needs to be signed by
        /// all the individual sources.
        /// </summary>
        /// <returns></returns>
        public byte [] GetTransactionData()
        {
            return tranxData;
        }

        /// <summary>
        /// Use the signature data, with the transaction data, to create the TransactionContent
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="transactionContent"></param>
        /// <returns></returns>
        public bool Create(byte[] signature, out TransactionContent transactionContent)
        {
            TC.SetSignatures(new List<byte[]> { signature });

            transactionContent = TC;
                        
            return TC.VerifySignature();

        }

    }
}
