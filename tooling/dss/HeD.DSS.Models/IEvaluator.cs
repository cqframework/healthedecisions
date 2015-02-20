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
    /// Defines the interface for an evaluation plug-in.
    /// </summary>
    public interface IEvaluator
    {
        /// <summary>
        /// Evaluates the given input and returns a response.
        /// </summary>
        /// <param name="input">The input request.</param>
        /// <returns>The result of evaluating the given request.</returns>
        evaluateResponse Evaluate(evaluate input);
    }
}
