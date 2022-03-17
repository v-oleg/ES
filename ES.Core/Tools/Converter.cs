using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ES.Core.Events;
using Newtonsoft.Json.Linq;
using ES.Core.Extensions;
// ReSharper disable CheckNamespace

namespace ES.Core;

public class Converter
{
    public Guid StringToGuid(string value)
    {
        return Convert(MessageIdNamespace, value);
    }
    
    public Event ResourceToEvent(Assembly assembly, string path)
    {
        var resourceString = assembly.ResourceAsString(path);
        if (!string.IsNullOrWhiteSpace(resourceString)) 
            return new Event(JObject.Parse(resourceString));
        
        var pathParts = path.Split(".");
        throw new FileNotFoundException($"{pathParts[^2]}.{pathParts[^1]} not found.", pathParts[^1]);
    }
    
    public string ToEventNameStream(string serviceName, string aggregateType) =>
        $"$en-{serviceName}.{aggregateType}";
    
    public string ToAggregateNameStream(string serviceName, string aggregateType) =>
        $"$ce-{serviceName}.{aggregateType}";
    
    public string ToAggregateIdStream(string serviceName, string aggregateType, Guid aggregateId) =>
        $"{serviceName}.{aggregateType}-{aggregateId}";

    #region Helpers

    //https://github.com/Faithlife/FaithlifeUtility/blob/master/src/Faithlife.Utility/GuidUtility.cs
    private static readonly Guid MessageIdNamespace = new Guid("C8B474F4-9FA3-4B5A-B259-443FE79DC3BF");
    
    private static Guid Convert(Guid namespaceId, string strValue, int version = 5)
    {
        if (strValue == null)
            throw new ArgumentNullException(nameof(strValue));

        // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
        // ASSUME: UTF-8 encoding is always appropriate

        var nameBytes = Encoding.UTF8.GetBytes(strValue);

        // convert the namespace UUID to network order (step 3)
        var namespaceBytes = namespaceId.ToByteArray();

        SwapByteOrder(namespaceBytes);

        // compute the hash of the name space ID concatenated with the name (step 4)
        byte[] hash;
        using (var algorithm = version == 3 ? (HashAlgorithm) MD5.Create() : SHA1.Create())

        {
            algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
            algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
            hash = algorithm.Hash!;
        }

        // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
        var newGuid = new byte[16];

        Array.Copy(hash, 0, newGuid, 0, 16);

        // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
        newGuid[6] = (byte) ((newGuid[6] & 0x0F) | (version << 4));

        // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
        newGuid[8] = (byte) ((newGuid[8] & 0x3F) | 0x80);

        // convert the resulting UUID to local byte order (step 13)
        SwapByteOrder(newGuid);

        return new Guid(newGuid);
    }

    // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
    private static void SwapByteOrder(byte[] guid)
    {
        SwapBytes(guid, 0, 3);
        SwapBytes(guid, 1, 2);
        SwapBytes(guid, 4, 5);
        SwapBytes(guid, 6, 7);
    }

    private static void SwapBytes(byte[] guid, int left, int right)
    {
        (guid[left], guid[right]) = (guid[right], guid[left]);
    }

    #endregion
}