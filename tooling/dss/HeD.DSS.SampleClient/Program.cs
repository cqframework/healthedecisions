/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HeD.CDSS.Model;
using HeD.DSS.Models;
using HeD.Model;
using vMR.Model;

namespace HeD.DSS.SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Prepare for a simple HttpPost request
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:61924/");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            // Get the body of the request
            var vMRRequest = GetSampleRequest();

            // Construct the DSS-level request, base-64 encoding the vMR request within it.
            var evaluate = 
                new evaluate 
                { 
                    interactionId =
                        new InteractionIdentifier 
                        { 
                            interactionId = Guid.NewGuid().ToString("N"), 
                            scopingEntityId = "SAMPLE-CLIENT", 
                            submissionTime = DateTime.Now.ToUniversalTime() 
                        },
                    evaluationRequest =
                        new EvaluationRequest
                        {
                            clientLanguage = "XXX",
                            clientTimeZoneOffset = "XXX",
                            dataRequirementItemData = new List<DataRequirementItemData> 
                            { 
                                new DataRequirementItemData
                                {
                                    driId = new ItemIdentifier { itemId = "RequiredDataId" },
                                    data = 
                                        new SemanticPayload 
                                        { 
                                            informationModelSSId = SemanticSignifiers.CDSInputId, 
                                            base64EncodedPayload = Packager.EncodeRequestPayload(vMRRequest) 
                                        }
                                }
                            },
                            kmEvaluationRequest = new List<KMEvaluationRequest> 
                            { 
                                new KMEvaluationRequest { kmId = new EntityIdentifier { scopingEntityId = "org.hl7.cds", businessId = "NQF-0068", version = "1.0" } }
                            }
                        }
                };

            // Post the request and retrieve the response
            var response = client.PostAsXmlAsync("api/evaluation", evaluate).Result;

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Evaluation succeeded.");
                var evaluateResponse = response.Content.ReadAsAsync<evaluateResponse>().Result;

                var evaluationResponse = evaluateResponse.evaluationResponse.finalKMEvaluationResponse.First();

                if (evaluationResponse.kmId.scopingEntityId != "SAMPLE-CLIENT")
                {
                    Console.WriteLine("Evaluation did not return the input scoping entity Id.");
                }

                var evaluationResult = evaluationResponse.kmEvaluationResultData.First();

                if (!SemanticSignifiers.AreEqual(evaluationResult.data.informationModelSSId, SemanticSignifiers.CDSActionGroupResponseId))
                {
                    Console.WriteLine("Evaluation did not return an action group response.");
                }

                var actionGroupResponse = Packager.DecodeActionGroupResponsePayload(evaluationResult.data.base64EncodedPayload);

                var createAction = actionGroupResponse.actionGroup.subElements.Items.First() as CreateAction;

                if (createAction == null)
                {
                    Console.WriteLine("Result does not include a CreateAction.");
                }
                else
                {
                    var proposalLiteral = createAction.actionSentence as ComplexLiteral;
                    if (proposalLiteral == null)
                    {
                        Console.WriteLine("Resulting CreateAction does not have a ComplexLiteral as the Action Sentence.");
                    }
                    else
                    {
                        var proposal = proposalLiteral.value as SubstanceAdministrationProposal;

                        if (proposal == null)
                        {
                            Console.WriteLine("Resulting proposal is not a substance administration proposal");
                        }
                        else
                        {
                            Console.WriteLine("Substance Administration Proposed: {0}.", proposal.substanceAdministrationGeneralPurpose.displayName);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);

                var content = response.Content.ReadAsStringAsync().Result;
                if (content.Length > 0)
                {
                    // NOTE: Deserialization here is assuming EvaluationException, need to peek into the Xml stream to determine the actual type.
                    var dssException = DSSExceptionExtensions.DeserializeFromString<EvaluationException>(content);
                    Console.WriteLine(dssException.GetType().Name);
                    Console.WriteLine(String.Join("\r\n", dssException.errorMessage));
                    if (dssException.value != null)
                    {
                        var cdsMessage = Packager.DecodeExecutionMessagePayload(dssException.value.base64EncodedPayload);
                        Console.WriteLine(cdsMessage.GetType().Name);
                        Console.WriteLine(cdsMessage.message.value);
                    }
                }
            }

            Console.ReadLine();
        }

        static CDSInput GetSampleRequest()
        {
            return
                new CDSInput
                {
                    vmrInput =
                        new VMR
                        {
                            patient =
                                new EvaluatedPerson
                                {
                                    id = new II { root = "TEST-PATIENT" },
                                    clinicalStatement =
                                        new List<ClinicalStatement>
                                        {
                                            new SubstanceAdministrationEvent
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
                                                administrationTimeInterval = new IVL_TS { low = new TS { value = "20130729" } }
                                            }
                                        }
                                }
                        }
                };
        }
    }
}
