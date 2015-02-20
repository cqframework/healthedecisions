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
using HeD.DSS.Models;

namespace HeD.CDSS.Model
{
    /// <summary>
    /// Defines the semantic signifiers used by the example implementation.
    /// </summary>
    public static class SemanticSignifiers
    {
        /// <summary>
        /// The identifier for the CDSInput semantic signifier.
        /// </summary>
        public static EntityIdentifier CDSInputId = new EntityIdentifier { scopingEntityId = "org.hl7.cds", businessId = "cdsinput:r2:CDSInput", version = "1.0" };

        /// <summary>
        /// The CDSVMRRequest semantic signifier.
        /// </summary>
        public static SemanticSignifier CDSInput = 
            new SemanticSignifier 
            { 
                name = "CDSInput",
                description = "vMR Request Container",
                entityId = CDSInputId,
                xsdComputableDefinition = new XSDComputableDefinition { xsdURL = "urn:hl7-org:cdsinput:r2", xsdRootGlobalElementName = "cdsInput" }
            };

        /// <summary>
        /// The identifier for the CDSOutputAsVMR semantic signifier.
        /// </summary>
        public static EntityIdentifier CDSOutputAsVMRId = new EntityIdentifier { scopingEntityId = "org.hl7.cds", businessId = "cdsoutput:r2:CDSOutputAsVMR", version = "1.0" };

        /// <summary>
        /// The CDSOutputAsVMR semantic signifier.
        /// </summary>
        public static SemanticSignifier CDSOutputAsVMR =
            new SemanticSignifier
            {
                name = "CDSOutputAsVMR",
                description = "vMR Response Container",
                entityId = CDSOutputAsVMRId,
                xsdComputableDefinition = new XSDComputableDefinition { xsdURL = "urn:hl7-org:cdsoutput:r2", xsdRootGlobalElementName = "cdsOutputAsVMR" }
            };

        /// <summary>
        /// The identifier for the CDSActionGroupResponse semantic signifier.
        /// </summary>
        public static EntityIdentifier CDSActionGroupResponseId = new EntityIdentifier { scopingEntityId = "org.hl7.cds", businessId = "kaoutput:r1:CDSActionGroupResponse", version = "1.0" };

        /// <summary>
        /// The CDSActionGroupResponse semantic signifier.
        /// </summary>
        public static SemanticSignifier CDSActionGroupResponse =
            new SemanticSignifier
            {
                name = "CDSActionGroupResponse",
                description = "Action Group Response Container",
                entityId = CDSActionGroupResponseId,
                xsdComputableDefinition = new XSDComputableDefinition { xsdURL = "urn:hl7-org:kaoutput:r1", xsdRootGlobalElementName = "CDSActionGroupResponse" }
            };

        /// <summary>
        /// The identifier for the CDSExecutionMessage semantic signifier.
        /// </summary>
        public static EntityIdentifier CDSExecutionMessageId = new EntityIdentifier { scopingEntityId = "org.hl7.cds", businessId = "kaoutput:r1:CDSExecutionMessage", version = "1.0" };

        /// <summary>
        /// The CDSExecutionMessage semantic signifier.
        /// </summary>
        public static SemanticSignifier CDSExecutionMessage =
            new SemanticSignifier
            {
                name = "CDSExecutionMessage",
                description = "ExecutionMessage Container",
                entityId = CDSExecutionMessageId,
                xsdComputableDefinition = new XSDComputableDefinition { xsdURL = "urn:hl7-org:kaoutput:r1", xsdRootGlobalElementName = "CDSExecutionMessage" }
            };

        /// <summary>
        /// Compares two EntityIdentifier instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True if the values of each property of both instances are equal, false otherwise.</returns>
        public static bool AreEqual(EntityIdentifier a, EntityIdentifier b)
        {
            return 
                a.scopingEntityId == b.scopingEntityId
                    && a.businessId == b.businessId
                    && a.version == b.version;
        }

        /// <summary>
        /// Compares two SemanticSignifier instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True if the identifiers of each instance are equal, false otherwise.</returns>
        public static bool AreEqual(SemanticSignifier a, SemanticSignifier b)
        {
            return AreEqual(a.entityId, b.entityId);
        }
    }
}
