using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppExperimental26.Models.Storage
{

    /*
 * 
 * {
    "id": "1",
    "FacilityCode": "A123",
    "CardNumber": "34125",
    "ParityBits": "8",
    "LocationAddress": "1144 Taylor St",
    "LocationCity": "New York",
    "LocationState": "NY",
    "LocationZip": "11580",
    "LocationLat": "41.40338",
    "LocationLong": "2.17403",
    "UploadedUser": "azaso",
    "UserID": "AndrewZaso",

    "CompanyName": "IBM",
    "IdString":"asdfasdfasdf",
    "_partitionKey" : "85F05E2D1492C89F3EC1052A"

*/

    public record RedIdRecord
    {

        [JsonProperty("id")]
        public int Id { get; set; } 
        public string? FacilityCode { get; set; } = string.Empty;
        public string? CardNumber { get; set; } = string.Empty;
        public string? ParityBits { get; set; } = string.Empty;
        public string? LocationAddress { get; set; } = string.Empty;
        public string? LocationCity { get; set; } = string.Empty;
        public string? LocationState { get; set; } = string.Empty;
        public string? LocationZip { get; set; } = string.Empty;
        public string? LocationLat { get; set; } = string.Empty;
        public string? LocationLong { get; set; } = string.Empty;

        // Make this server provided
        public string? UploadedUser { get; set; } = string.Empty;
        public string? UserID { get; set;} = string.Empty;

        public string? IdString { get; set; } = string.Empty;
        public string? CompanyName { get; set;} = string.Empty;

        // Make this server-provided
        public string? _partitionKey { get; set; } = string.Empty;  // TRIAL DEV USE ONLY


    }
}
