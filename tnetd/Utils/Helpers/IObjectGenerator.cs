﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Helpers
{
    public interface IObjectGenerator<T>
    {
        /// <summary>
        /// Returns the number of items in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Generate the item that is located on the specified index.
        /// </summary>
        /// <remarks>
        /// This method is only be called once per index.
        /// </remarks>
        /// <param name="index">Index of the items that must be generated.</param>
        /// <returns>Fresh new instance.</returns>
        T CreateObject(int index);
    }
}
