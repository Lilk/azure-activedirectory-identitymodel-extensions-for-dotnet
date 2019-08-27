//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Xml;
using System;
using System.Xml;

#pragma warning disable 1591

namespace Microsoft.IdentityModel.Protocols.WsFed
{
    /// <summary>
    /// This class is used to represent a ClaimType found in the WsFed specification: http://docs.oasis-open.org/wsfed/federation/v1.2/os/ws-federation-1.2-spec-os.html .
    /// </summary>
    /// <remarks>Only 'Value' is read.</remarks>
    public class ClaimType
    {
        private string _uri;
        private string _value;

        /// <summary>
        /// Instantiates a <see cref="ClaimType"/> instance.
        /// </summary>
        public ClaimType() {}

        /// <summary>
        /// Gets ClaimType optional attribute.
        /// </summary>
        /// <remarks>This is an optional attribute.</remarks>
        public bool? IsOptional { get; set; }

        /// <summary>
        /// Gets ClaimType value element.
        /// </summary>
        /// <remarks>this is an optional value.</remarks>
        public string Value
        {
            get => _value;
            set => _value = (string.IsNullOrEmpty(value)) ? throw LogHelper.LogArgumentNullException(nameof(Value)) : value;
        }

        /// <summary>
        /// Gets ClaimType uri attribute.
        /// </summary>
        /// <remarks>this is a required value.</remarks>
        public string Uri
        {
            get => _uri;
            set => _uri = (string.IsNullOrEmpty(value)) ? throw LogHelper.LogArgumentNullException(nameof(Uri)) : value;
        }

        /// <summary>
        /// Creates and populates a <see cref="ClaimType"/> by reading xml.
        /// Expects the <see cref="XmlDictionaryReader"/> to be positioned on the StartElement: "ClaimType" in the namespace passed in.
        /// </summary>
        /// <param name="reader">a <see cref="XmlDictionaryReader"/> positioned at the StartElement: "ClaimType".</param>
        /// <param name="namespace">the namespace for the StartElement.</param>
        /// <returns>a populated <see cref="ClaimType"/>.</returns>
        /// <remarks>Checking for the correct StartElement is as follows.</remarks>
        /// <remarks>if @namespace is null, then <see cref="XmlDictionaryReader.IsLocalName(string)"/> will be called.</remarks>
        /// <remarks>if @namespace is not null or empty, then <see cref="XmlDictionaryReader.IsStartElement(XmlDictionaryString, XmlDictionaryString)"/> will be called.></remarks>
        /// <exception cref="ArgumentNullException">if reader is null.</exception>
        /// <exception cref="XmlReadException">if reader is not positioned on a StartElement.</exception>
        /// <exception cref="XmlReadException">if the StartElement does not match the expectations in remarks.</exception>
        public static ClaimType ReadFrom(XmlDictionaryReader reader, string @namespace)
        {
            // example:
            // <auth:ClaimType Uri="a14bf1a3-a189-4a81-9d9a-7d3dfeb7724a" xmlns:auth="http://docs.oasis-open.org/wsfed/authorization/200706">
            //   <auth:Value>77a6fa04-0454-4d08-8761-2a840e281399</auth:Value>
            // </auth:ClaimType>

            XmlUtil.CheckReaderOnEntry(reader, WsFedElements.ClaimType, @namespace);
            reader.MoveToContent();
            var uri = reader.GetAttribute(WsFedAttributes.Uri);
            if (string.IsNullOrEmpty(uri))
                throw LogHelper.LogExceptionMessage(new XmlReadException(LogHelper.FormatInvariant(Xml.LogMessages.IDX30013, WsFedElements.ContextItem, WsFedAttributes.Name)));

            var optionalAttribute = reader.GetAttribute(WsFedAttributes.Optional);
            bool? optional = null;
            if (!string.IsNullOrEmpty(optionalAttribute))
                optional = XmlConvert.ToBoolean(optionalAttribute);

            string value = null;
            reader.MoveToContent();

            // brentsch - TODO, need loop for multiple elements
            if (reader.IsStartElement(WsFedElements.Value, @namespace))
                value = XmlUtil.ReadStringElement(reader);

            // brentsch - TODO, TESTCASE
            if (optional.HasValue && !string.IsNullOrEmpty(value))
                return new ClaimType { Uri = uri, IsOptional = optional, Value = value };
            else if (optional.HasValue)
                return new ClaimType { Uri = uri, IsOptional = optional };
            else if (!string.IsNullOrEmpty(value))
                return new ClaimType { Uri = uri, Value = value };

            return new ClaimType { Uri = uri };
        }
    }
}
