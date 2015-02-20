/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using HeD.CDSS.Model;
using HeD.Model;
using vMR.Model;

namespace HeD.DSS.Models
{
    public class EngineResponse
    {
        public CDSOutput Response { get; set; }

        public List<CDSExecutionMessage> Messages { get; set; }
    }

    /// <summary>
    /// Provides an example Evaluator implementation.
    /// </summary>
    public class Evaluator : IEvaluator
    {
        #region IEvaluator Members

        /// <summary>
        /// Evaluates the request and returns a response.
        /// </summary>
        /// <param name="input">The input request.</param>
        /// <returns>The result of evaluating the given input.</returns>
        public evaluateResponse Evaluate(evaluate input)
        {
            var inputScopingEntityId = input.interactionId.scopingEntityId;
            if (input.interactionId.scopingEntityId != "SAMPLE-CLIENT")
            {
                throw
                    new DSSExceptionWrapper
                    (
                        new UnrecognizedScopingEntityException
                        {
                            errorMessage = new List<string> { "Unknown client" },
                            scopingEntityId = input.interactionId.scopingEntityId
                        }
                    );
            }

            if (input.evaluationRequest.dataRequirementItemData.Count > 1)
            {
                throw new ArgumentOutOfRangeException("Example service implementation can only process requests with a single data requirement item container.");
            }

            var dri = input.evaluationRequest.dataRequirementItemData[0];

            if (!SemanticSignifiers.AreEqual(dri.data.informationModelSSId, SemanticSignifiers.CDSInputId))
            {
                throw 
                    new DSSExceptionWrapper
                    (
                        new UnrecognizedScopedEntityException 
                        { 
                            errorMessage = new List<string> { "Unknown semantic signifier. Semantic signifier must be 'org.hl7.cds', 'cdss:r1', '1.0'" }, 
                            entityType = EntityType.SEMANTIC_SIGNIFIER, 
                            entityId = dri.data.informationModelSSId 
                        }
                    );
            }

            var payload = Packager.DecodeRequestPayload(dri.data.base64EncodedPayload);

            EngineResponse engineResponse = null;

            try
            {
                // Pass payload to rule engine for evaluation here...
                // NOTE: This could be done a per KM basis as well, depends on whether the engine
                // is aggregating results, or keeping results separate.
                engineResponse = GetSampleEngineResponse(payload);
            }
            catch (CDSExecutionMessageWrapper m)
            {
                // Catch any execution exceptions and convert them to DSS exceptions
                throw new DSSExceptionWrapper(m.ExecutionMessage.ToDSSException());
            }

            // Package the engine's response in a DSS evaluate response.
            return
                new evaluateResponse
                {
                    requestId = input.interactionId,
                    responseId = new InteractionIdentifier { scopingEntityId = "org.hl7.cds", interactionId = Guid.NewGuid().ToString("N"), submissionTime = DateTime.Now },
                    evaluationResponse =
                        new EvaluationResponse
                        {
                            finalKMEvaluationResponse = 
                                new List<FinalKMEvaluationResponse>
                                {
                                    new FinalKMEvaluationResponse
                                    {
                                        kmEvaluationResultData = 
                                            new List<KMEvaluationResultData>
                                            {
                                                new KMEvaluationResultData
                                                {
                                                    evaluationResultId = new ItemIdentifier { itemId = Guid.NewGuid().ToString("N") },
                                                    data = 
                                                        new SemanticPayload 
                                                        { 
                                                            informationModelSSId = SemanticSignifiers.CDSActionGroupResponseId, 
                                                            base64EncodedPayload = Packager.EncodeActionGroupResponsePayload((CDSActionGroupResponse)engineResponse.Response)
                                                        }
                                                }
                                            },
                                        kmId = new EntityIdentifier { businessId = Guid.NewGuid().ToString("N"), scopingEntityId = inputScopingEntityId, version = "1" },
                                        warning = 
                                            (
                                                from m in engineResponse.Messages
                                                select 
                                                    new Warning 
                                                    { 
                                                        value = 
                                                            new SemanticPayload 
                                                            { 
                                                                informationModelSSId = SemanticSignifiers.CDSExecutionMessageId, 
                                                                base64EncodedPayload = Packager.EncodeExecutionMessagePayload(m) 
                                                            } 
                                                    }
                                            ).ToList()
                                    }
                                }
                        }
                };
        }

        #endregion

        private EngineResponse GetSampleEngineResponse(CDSInput payload)
        {
            var responsePayload = GetSampleResponsePayload(payload);

            var messages = new List<CDSExecutionMessage>();

            // Uncomment this statement to demonstrate exception handling
			//throw 
			//	new CDSExecutionMessageWrapper
			//	(
			//		new CDSExecutionMessage 
			//		{ 
			//			message = new ST { value = "Error" }, 
			//			level = new CDSExecutionMessageLevel1 { valueSpecified = true, value = CDSExecutionMessageLevel.Error }, 
			//			reason = new CDSExecutionMessageReason1 { valueSpecified = true, value = CDSExecutionMessageReason.DataIsMissing }, 
			//			sourceComponentType = new CDSExecutionMessageSourceComponentType { valueSpecified = true, value = DSSComponentType.System } 
			//		}
			//	);

            return new EngineResponse { Response = responsePayload, Messages = messages };
        }

        private CDSActionGroupResponse GetSampleResponsePayload(CDSInput payload)
        {
            return
                new CDSActionGroupResponse
                {
                    patientId = payload.vmrInput.patient.id,
                    actionGroup = 
                        new ActionGroup
                        {
                            subElements =
                                new ActionGroupSubElements
                                {
                                    Items = 
                                        new List<ActionBase>
                                        {
                                            new CreateAction
                                            {
                                                actionId = new II { extension = Guid.NewGuid().ToString("N") },
                                                actionSentence =
                                                    new ComplexLiteral
                                                    {
                                                        value = 
                                                            new SubstanceAdministrationProposal
                                                            {
                                                                id = new II { extension = Guid.NewGuid().ToString("N") },
                                                                substanceAdministrationGeneralPurpose = 
                                                                    new CD 
                                                                    { 
                                                                        code = "ASPIRIN", 
                                                                        codeSystem = "org.hl7.example", 
                                                                        codeSystemName = "org.hl7.example", 
                                                                        displayName = new ST { value = "Aspirin" }
                                                                    },
                                                                proposedAdministrationTimeInterval = new IVL_TS { low = new TS { value = "20130729" } }
                                                            }
                                                    }
                                            }
                                        }
                                }
                        }
                    
                };
        }
    }
}