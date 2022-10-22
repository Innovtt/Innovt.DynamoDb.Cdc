// Innovt Company
// Author: Michel Borges
// Project: Innovt.DynamoDb.Cdc.Core

using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Innovt.Core.Collections;

namespace Innovt.DynamoDb.Cdc.Core.Infrastructure.Repositories;

public static class AttributeHelper
{
    internal static object FormatAttributeValue(AttributeValue value)
    {
        if (value is null)
            return default;

        if (value.IsBOOLSet)
        {
            return new {BOOL = value.BOOL};
        }

        if (value.N is { })
        {
            return new {N = value.N};
        }

        if (value.NS.IsNotNullOrEmpty())
        {
            return new {NS = value.NS};
        }

        if (value.SS.IsNotNullOrEmpty())
        {
            return new {SS = value.SS};
        }

        if (value.IsLSet)
        {
            return new {L = value.L.Select(FormatAttributeValue)};
        }

        //////Nested Type
        if (value.IsMSet)
        {
            dynamic complexObj = new ExpandoObject();

            foreach (var item in value.M)
            {
                ((IDictionary<string, object>) complexObj).Add(item.Key, FormatAttributeValue(item.Value));
            }

            return complexObj;
        }

        return new {S = value.S};
    }
}