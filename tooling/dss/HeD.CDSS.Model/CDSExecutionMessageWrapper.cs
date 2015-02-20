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

namespace HeD.CDSS.Model
{
    /// <summary>
    /// Provides an exception wrapper for the CDSExecution message.
    /// </summary>
    public class CDSExecutionMessageWrapper : Exception
    {
        /// <summary>
        /// Constructs a new instance of the CDSExecutionMessageWrapper exception.
        /// </summary>
        /// <param name="message">The message instance to wrap.</param>
        public CDSExecutionMessageWrapper(CDSExecutionMessage message) : this(message, null) { }

        /// <summary>
        /// Constructs a new instance of the CDSExecutionMessageWrapper exception.
        /// </summary>
        /// <param name="message">The message instance to wrap.</param>
        /// <param name="innerException">An optional inner exception.</param>
        public CDSExecutionMessageWrapper(CDSExecutionMessage message, Exception innerException) : base(message.message.value, innerException) 
        { 
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            ExecutionMessage = message;
        }

        /// <summary>
        /// Gets the execution message wrapped by this exception.
        /// </summary>
        public CDSExecutionMessage ExecutionMessage { get; private set; }
    }
}
