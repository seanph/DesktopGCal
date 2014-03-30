#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// STools.cs
//   Defines some useful extra functions that are used in more than one section 
//   of the code
#endregion

using System;

namespace Seanph.DataTools
{
    public static class STime
    {
        /// <summary>
        ///     Converts a DateTime object to a UNIX timestamp
        /// </summary>
        public static uint DT2Unix(DateTime dt)
        {
            return (uint)(dt.ToUniversalTime()
                                    .Subtract(new DateTime(1970, 1, 1)))
                                    .TotalSeconds;
        }

        /// <summary>
        ///     Converts a UNIX timestamp to a DateTime object
        /// </summary>
        public static DateTime Unix2DT(uint unix)
        {
            return new DateTime(1970, 1, 1).AddSeconds(unix);
        }
    }
}