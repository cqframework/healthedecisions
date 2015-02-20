/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeD.DSS.Models
{
    /// <summary>
    /// Provides an abstract base class for DSSExceptionWrapper classes.
    /// </summary>
    public class DSSExceptionWrapper : Exception
    {
        /// <summary>
        /// Constructs a new instance of the DSSExceptionWrapper.
        /// </summary>
        /// <param name="exception">The DSSException instance to be wrapped.</param>
        public DSSExceptionWrapper(DSSException exception) : this(exception, null) { }

        /// <summary>
        /// Constructs a new instance of the DSSExceptionWrapper.
        /// </summary>
        /// <param name="exception">The DSSException instance to be wrapped.</param>
        /// <param name="innerException">An optional inner exception.</param>
        public DSSExceptionWrapper(DSSException exception, Exception innerException) : base(exception.GetMessage(), innerException)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;
        }

        /// <summary>
        /// Gets the DSSException wrapped by this exception.
        /// </summary>
        public DSSException Exception { get; private set; }
    }
}
