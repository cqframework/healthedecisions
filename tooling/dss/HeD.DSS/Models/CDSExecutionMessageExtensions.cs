/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeD.CDSS.Model;

namespace HeD.DSS.Models
{
    /// <summary>
    /// Provides utility methods for CDS Execution Messages.
    /// </summary>
    public static class CDSExecutionMessageExtensions
    {
        /// <summary>
        /// Converts a CDSExecutionMessage into an equivalent DSSException, with the CDSExecutionMessage as payload for the exception.
        /// </summary>
        /// <param name="message">The message to be converted.</param>
        /// <returns>A DSSException descendent appropriate to the given message and containing the message as payload.</returns>
        public static DSSException ToDSSException(this CDSExecutionMessage message)
        {
            return
                message.level.value == CDSExecutionMessageLevel.Error
                    ?
                        (DSSException)
                        new EvaluationException
                        {
                            errorMessage = new List<string> { message.message.value },
                            value =
                                new SemanticPayload
                                {
                                    informationModelSSId = SemanticSignifiers.CDSExecutionMessageId,
                                    base64EncodedPayload = Packager.EncodeExecutionMessagePayload(message)
                                }
                        }
                    :
                        new DSSRuntimeException
                        {
                            errorMessage = new List<string> { message.message.value },
                            value =
                                new SemanticPayload
                                {
                                    informationModelSSId = SemanticSignifiers.CDSExecutionMessageId,
                                    base64EncodedPayload = Packager.EncodeExecutionMessagePayload(message)
                                }
                        };
        }
    }
}