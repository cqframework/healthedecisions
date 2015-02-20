/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;

using HeD.DSS.Models;

namespace HeD.DSS.Controllers
{
    /// <summary>
    /// Defines the MVC controller responsible for handling the Evaluate call.
    /// </summary>
    public class EvaluationController : ApiController
    {
        [HttpPost]
        public evaluateResponse Evaluate(evaluate input)
        {
            try
            {
                var className = ConfigurationManager.AppSettings["EvaluatorClassName"];
                var evaluator = Activator.CreateInstance(Type.GetType(className, true)) as IEvaluator;

                if (evaluator == null)
                {
                    throw new InvalidOperationException("Evaluator class name must specify a class that implements the IEvaluator interface.");
                }

                return evaluator.Evaluate(input);
            }
            catch (DSSExceptionWrapper de)
            {
                throw new HttpResponseException(de.Exception.ToHttpResponseMessage());
            }
            catch (SecurityException se)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            catch (UnauthorizedAccessException uae)
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
    }
}
