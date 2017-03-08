using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;

namespace OpenIdConnectServer.Services
{
    public static class DeviceCodeFlow
    {
        public static class ResponseTypes
        {
            public const string DeviceCode = "device_code";
        }

        public static class GrantTypes
        {
            public const string DeviceCode = "device_code";
        }
    }

    public static class OpenIdConnectRequestDeviceCodeFlowExtensions
    {
        public static bool IsDeviceCodeGrantType(this OpenIdConnectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.GrantType))
            {
                return false;
            }

            var segment = TrimUtils.Trim(new StringSegment(request.GrantType), OpenIdConnectConstants.Separators.Space);
            if (segment.Length == 0)
            {
                return false;
            }

            return segment.Equals(DeviceCodeFlow.GrantTypes.DeviceCode, StringComparison.Ordinal);
        }

        public static bool IsDeviceCodeFlow(this OpenIdConnectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.ResponseType))
            {
                return false;
            }

            var segment = TrimUtils.Trim(new StringSegment(request.ResponseType), OpenIdConnectConstants.Separators.Space);
            if (segment.Length == 0)
            {
                return false;
            }

            return segment.Equals(DeviceCodeFlow.ResponseTypes.DeviceCode, StringComparison.Ordinal);
        }
    }
    
    public static class OpenIddictDeviceCodeFlowExtensions
    {
        public static OpenIddictBuilder AllowDeviceCodeFlow(this OpenIddictBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Configure(options =>
                options.GrantTypes.Add(DeviceCodeFlow.GrantTypes.DeviceCode)
            );
        }
    }

    public static class TrimUtils
    {
        public static StringSegment TrimStart(StringSegment segment, char[] separators)
        {
            Debug.Assert(separators?.Length != 0, "The separators collection shouldn't be null or empty.");

            var index = segment.Offset;

            while (index < segment.Offset + segment.Length)
            {
                if (!IsSeparator(segment.Buffer[index], separators))
                {
                    break;
                }

                index++;
            }

            return new StringSegment(segment.Buffer, index, segment.Offset + segment.Length - index);
        }

        private static StringSegment TrimEnd(StringSegment segment, char[] separators)
        {
            Debug.Assert(separators?.Length != 0, "The separators collection shouldn't be null or empty.");

            var index = segment.Offset + segment.Length - 1;

            while (index >= segment.Offset)
            {
                if (!IsSeparator(segment.Buffer[index], separators))
                {
                    break;
                }

                index--;
            }

            return new StringSegment(segment.Buffer, segment.Offset, index - segment.Offset + 1);
        }

        public static StringSegment Trim(StringSegment segment, char[] separators)
        {
            Debug.Assert(separators?.Length != 0, "The separators collection shouldn't be null or empty.");

            return TrimEnd(TrimStart(segment, separators), separators);
        }

        public static bool IsSeparator(char character, char[] separators)
        {
            Debug.Assert(separators?.Length != 0, "The separators collection shouldn't be null or empty.");

            for (var index = 0; index < separators.Length; index++)
            {
                if (character == separators[index])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
