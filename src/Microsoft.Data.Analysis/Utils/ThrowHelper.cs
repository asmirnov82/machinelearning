// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;


namespace Microsoft.Data.Analysis
{
    internal static class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(string name, object value, string message)
        {
            throw new ArgumentOutOfRangeException(name, value, message);
        }
    }
}
