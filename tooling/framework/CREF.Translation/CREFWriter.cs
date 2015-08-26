/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HeD.Engine.Writing;

namespace CREF.Translation
{
	public class CREFWriter : IArtifactWriter
	{
		#region IArtifactWriter Members

		public String GetExtension()
		{
			return ".xml";
		}

		public void Write(Stream outputStream, object artifact)
		{
            using (var stream = new MemoryStream())
            {
			    var serializer = new XmlSerializer(artifact.GetType(), "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
			    var ns = new XmlSerializerNamespaces();
			    ns.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
			    ns.Add("ds", "http://schemas.allscripts.com/cds/evaluation");
			    ns.Add("am", "http://schemas.allscripts.com/mom");
			    serializer.Serialize(stream, artifact, ns);

                // This seems like a bug with the serializer to me, but the outer-most instance of
                // Composite expression within the artifact is serialized with the prefix ds, instead
                // of am, even though the CompositeAssertion class has the XmlNamespaceAttribute with
                // allscripts.com/mom as given above. So we serialize the output to a temporary stream,
                // read it into a string, replace the offending instance with the correct value, and
                // then write the output stream with that string.
                stream.Position = 0;
                using (var sr = new StreamReader(stream))
                {
                    var result = sr.ReadToEnd();

                    using (var sw = new StreamWriter(outputStream))
                    {
                        sw.Write(result.Replace("ds:CompositeAssertion", "am:CompositeAssertion"));
                    }
                }
            }
		}

		#endregion
	}
}
